using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Glyssen.Bundle;
using Glyssen.Character;
using Glyssen.Dialogs;
using Glyssen.VoiceActor;
using SIL.Extensions;
using SIL.Progress;

namespace Glyssen.Rules
{
	public class CharacterGroupGenerator
	{
		private readonly Project m_project;
		private readonly CastSizeRowValues m_ghostCastSize;
		private readonly Proximity m_proximity;
		private readonly BackgroundWorker m_worker;

		private static readonly SortedDictionary<int, IList<HashSet<string>>> DeityCharacters;
		private CharacterGroupGenerationPreferences m_groupGenerationPreferencesOverride;

		private IList<CharacterGroup> ProjectCharacterGroups
		{
			get { return m_project.CharacterGroupList.CharacterGroups; }
		}

		public static bool ContainsDeityCharacter(CharacterGroup group)
		{
			return group.CharacterIds.Any(c => DeityCharacters.First().Value[0].Contains(c));
		}

		static CharacterGroupGenerator()
		{
			// REVIEW: Should these numbers be hard-coded like this (which probably assumes a NT recording project), or should they
			// really be based on the percentage that these characters speak compared to the total recording time for the project?
			DeityCharacters = new SortedDictionary<int, IList<HashSet<string>>>();
			DeityCharacters.Add(1, new List<HashSet<string>> { new HashSet<string> { "Jesus", "God", "Holy Spirit, the", "scripture" } });
			var jesusSet = new HashSet<string> { "Jesus" };
			DeityCharacters.Add(4, new List<HashSet<string>> { jesusSet, new HashSet<string> { "God", "Holy Spirit, the", "scripture" } });
			var holySpiritSet = new HashSet<string> { "Holy Spirit, the" };
			DeityCharacters.Add(7, new List<HashSet<string>> { jesusSet, new HashSet<string> { "God", "scripture" }, holySpiritSet });
			DeityCharacters.Add(17, new List<HashSet<string>> { jesusSet, new HashSet<string> { "God" }, holySpiritSet, new HashSet<string> { "scripture" } });
		}

		public CharacterGroupGenerator(Project project, CastSizeRowValues ghostCastSize = null, BackgroundWorker worker = null)
		{
			m_project = project;
			m_ghostCastSize = ghostCastSize;
			m_proximity = new Proximity(project.IncludedBooks);
			m_worker = worker ?? new BackgroundWorker();
		}

		public List<CharacterGroup> GeneratedGroups { get; private set; }
		internal MinimumProximity MinimumProximity { get; private set; }
		internal int CountOfCharactersInGroupWithConflictingGender { get; private set; }

		private CharacterGroupGenerationPreferences GroupGenerationPreferences
		{
			get { return m_groupGenerationPreferencesOverride ?? m_project.CharacterGroupGenerationPreferences; }
		}
 
		public void ApplyGeneratedGroupsToProject(bool attemptToPreserveActorAssignments = true)
		{
			// Create a copy. Cameos are handled in the generation code (because we always maintain those assignments).
			// REVIEW: Would it still work (and potentially be more efficient) if we only included groups with a voice
 			// actor assigned, or would there be some weird edge cases where this might result in the wrong assignment?
			var previousGroups = attemptToPreserveActorAssignments ?
				ProjectCharacterGroups.Where(g => !g.AssignedToCameoActor).ToList() : new List<CharacterGroup>();

			ProjectCharacterGroups.Clear();
			ProjectCharacterGroups.AddRange(GeneratedGroups);

			if (previousGroups.Count == 0) return;

			// prevent an actor from being automatically assigned twice, especially when the gender is changed
			var alreadyAssigned = GeneratedGroups.Where(grp => grp.IsVoiceActorAssigned && previousGroups.Any(prevGrp => prevGrp.VoiceActorId == grp.VoiceActorId))
				.Select(grp => grp.VoiceActorId).ToList();

			// We assume the parts with the most keystrokes are most important to maintain
				var characterIdsSortedByKeystrokes = from entry in m_project.KeyStrokesByCharacterId orderby entry.Value descending select entry;
				foreach (var entry in characterIdsSortedByKeystrokes)
			{
				var characterId = entry.Key;
				var previousGroupWithCharacter = previousGroups.FirstOrDefault(g => g.CharacterIds.Contains(characterId) && !alreadyAssigned.Contains(g.VoiceActorId));
				if (previousGroupWithCharacter != null)
				{
					var newlyGeneratedGroupWithCharacter = ProjectCharacterGroups.FirstOrDefault(g => !g.IsVoiceActorAssigned && g.CharacterIds.Contains(characterId));
					if (newlyGeneratedGroupWithCharacter == null)
						continue;
					newlyGeneratedGroupWithCharacter.AssignVoiceActor(previousGroupWithCharacter.VoiceActorId);
					previousGroups.Remove(previousGroupWithCharacter);
					if (previousGroups.Count == 0)
						break;
				}
			}
		}

		public static void GenerateGroupsWithProgress(Project project, bool attemptToPreserveActorAssignments, bool firstGroupGenerationRun, bool forceMatchToActors, CastSizeRowValues ghostCastSize = null, bool cancelLink = false)
		{
			var castSizeOption = project.CharacterGroupGenerationPreferences.CastSizeOption;
			if (forceMatchToActors)
				project.CharacterGroupGenerationPreferences.CastSizeOption = CastSizeOption.MatchVoiceActorList;
			bool saveGroups = false;
			using (var progressDialog = new GenerateGroupsProgressDialog(project, OnGenerateGroupsWorkerDoWork, firstGroupGenerationRun, cancelLink))
			{
				var generator = new CharacterGroupGenerator(project, ghostCastSize, progressDialog.BackgroundWorker);
				progressDialog.ProgressState.Arguments = generator;

				if (progressDialog.ShowDialog() == DialogResult.OK && generator.GeneratedGroups != null)
				{
					generator.ApplyGeneratedGroupsToProject(attemptToPreserveActorAssignments);
					saveGroups = true;
				}
				else if (forceMatchToActors)
					project.CharacterGroupGenerationPreferences.CastSizeOption = castSizeOption;
			}
			project.Save(saveGroups);
		}

		private static void OnGenerateGroupsWorkerDoWork(object s, DoWorkEventArgs e)
		{
			var generator = (CharacterGroupGenerator)((ProgressState)e.Argument).Arguments;
			generator.GenerateCharacterGroups();
		}

		public List<CharacterGroup> GenerateCharacterGroups(bool enforceProximityAndGenderConstraints = false)
		{
			if (m_groupGenerationPreferencesOverride == null)
				m_project.SetDefaultCharacterGroupGenerationPreferences();

			List<VoiceActor.VoiceActor> actorsForGeneration;
			List<VoiceActor.VoiceActor> realActorsToReset = null;
			if (GroupGenerationPreferences.CastSizeOption == CastSizeOption.MatchVoiceActorList)
				actorsForGeneration = m_project.VoiceActorList.ActiveActors.ToList();
			else
			{
				realActorsToReset = m_project.VoiceActorList.AllActors.ToList();
				actorsForGeneration = CreateGhostCastActors().ToList();
				m_project.VoiceActorList.AllActors = actorsForGeneration;
			}
			List<CharacterGroup> characterGroups = CreateGroupsForActors(actorsForGeneration);

			if (m_worker.CancellationPending)
			{
				EnsureActorListIsSetToRealActors(realActorsToReset);
				return GeneratedGroups = null;
			}

			if (characterGroups.Count == 0)
			{
				EnsureActorListIsSetToRealActors(realActorsToReset);
				return GeneratedGroups = characterGroups; // REVIEW: Maybe we should throw an exception instead.
			}

			List<VoiceActor.VoiceActor> nonCameoActors = actorsForGeneration.Where(a => !a.IsCameo).ToList();

			if (m_worker.CancellationPending)
			{
				EnsureActorListIsSetToRealActors(realActorsToReset);
				return GeneratedGroups = null;
			}

			if (nonCameoActors.Count == 0)
			{
				EnsureActorListIsSetToRealActors(realActorsToReset);
				return GeneratedGroups = characterGroups; // All cameo actors! This should probably never happen, but user could maybe mark them all as cameo after the fact (?)
			}

			IOrderedEnumerable<KeyValuePair<string, int>> characterIdsOrderedToMinimizeProximityConflicts =
				from entry in m_project.SpeechDistributionScoreByCharacterId orderby entry.Value descending select entry;

			IReadOnlyDictionary<string, CharacterDetail> characterDetails = m_project.AllCharacterDetailDictionary;
			var includedCharacterDetails = characterDetails.Values.Where(c => characterIdsOrderedToMinimizeProximityConflicts.Select(e => e.Key).Contains(c.CharacterId)).ToList();

			if (m_worker.CancellationPending)
			{
				EnsureActorListIsSetToRealActors(realActorsToReset);
				return GeneratedGroups = null;
			}

			// In the first loop, we're looking for actors that could only possibly play one character role.
			// Since we're not doing strict age matching, this is most likely only to find any candidates in
			// the case of children (and then only if the project includes a limited selection of books)
			var characterDetailsUniquelyMatchedToActors = new Dictionary<CharacterDetail, List<VoiceActor.VoiceActor>>();
			foreach (var actor in nonCameoActors)
			{
				// After we find the second match, we can quit looking because we're only interested in unique matches.
				var matches = includedCharacterDetails.Where(c => !CharacterVerseData.IsCharacterStandard(c.CharacterId) && actor.Matches(c)).Take(2).ToList();
				if (matches.Any())
				{
					if (matches.Count == 1)
					{
						var characterDetail = matches.First();
						if (characterDetailsUniquelyMatchedToActors.ContainsKey(characterDetail))
							characterDetailsUniquelyMatchedToActors[characterDetail].Add(actor);
						else
							characterDetailsUniquelyMatchedToActors[characterDetail] = new List<VoiceActor.VoiceActor> { actor };
					}
				}
			}

			// This loop uses the results of the previous one to add the characters to a group, and to close that
			// group to further additions. If there's more than one candidate actor, we pick one arbitrarily,
			// since afterwards we'll be clearing actor names anyway.
			// Since all groups now have an actor assigned up-front, at the
			// end of the generation process, we'll need to clear all actor assignments except for the ones that
			// are pre-determined here.
			List<int> actorsWithRealAssignments = new List<int>();
			foreach (var characterDetailToActors in characterDetailsUniquelyMatchedToActors)
			{
				var matchingActors = characterDetailToActors.Value;

				var matchingGroups = characterGroups.Where(g => matchingActors.Any(a => a.Id == g.VoiceActorId)).ToList();

				if (matchingGroups.Count == 1)
				{
					matchingGroups.First().CharacterIds.Add(characterDetailToActors.Key.CharacterId);
					actorsWithRealAssignments.Add(matchingGroups[0].VoiceActorId);
				}
			}

			if (m_worker.CancellationPending)
			{
				EnsureActorListIsSetToRealActors(realActorsToReset);
				return GeneratedGroups = null;
			}

			foreach (var character in includedCharacterDetails)
			{
				var matchingActors = characterGroups.Where(g => !g.Closed).Select(g => g.VoiceActor).Where(a => a.Matches(character)).ToList();
				if (matchingActors.Count == 1)
				{
					var matchingActor = matchingActors.First();
					CharacterGroup groupForActor = characterGroups.Single(g => g.VoiceActorId == matchingActor.Id);
					groupForActor.CharacterIds.Add(character.CharacterId);
					actorsWithRealAssignments.Add(matchingActor.Id);
				}
			}

			if (m_groupGenerationPreferencesOverride == null)
				m_project.SetCharacterGroupGenerationPreferencesToValidValues();

			int maleNarrators = GroupGenerationPreferences.NumberOfMaleNarrators;
			int femaleNarrators = GroupGenerationPreferences.NumberOfFemaleNarrators;

			TrialGroupConfiguration bestConfiguration = null;
			bool fallbackPass = false;
			do
			{
				if (!fallbackPass)
				{
					Debug.WriteLine("===========================================================");
					Debug.WriteLine("First pass (no biblical character roles in narrator groups)");
					Debug.WriteLine("===========================================================");
				}

				if (m_worker.CancellationPending)
				{
					EnsureActorListIsSetToRealActors(realActorsToReset);
					return GeneratedGroups = null;
				}

				// TODO: The parameter indicating whether to allow the extra-biblical role(s) to be done by a female actor should be set
				// based on a user-contolled option.
				var trialConfigurationsForNarratorsAndExtras = TrialGroupConfiguration.GeneratePossibilities(fallbackPass, characterGroups,
					maleNarrators, femaleNarrators, !enforceProximityAndGenderConstraints, includedCharacterDetails, m_project.KeyStrokesByCharacterId,
					m_project, characterDetails);

				if (fallbackPass)
				{
					Debug.WriteLine("=================================================================");
					Debug.WriteLine("Fallback pass (allow biblical character roles in narrator groups)");
					Debug.WriteLine("=================================================================");
				}

				if (trialConfigurationsForNarratorsAndExtras.Any())
				{
					foreach (var configuration in trialConfigurationsForNarratorsAndExtras)
					{
						if (m_worker.CancellationPending)
						{
							EnsureActorListIsSetToRealActors(realActorsToReset);
							return GeneratedGroups = null;
						}

						List<CharacterDetail> charactersToProcessLast = new List<CharacterDetail>();

						foreach (var entry in characterIdsOrderedToMinimizeProximityConflicts)
						{
							string characterId = entry.Key;

							if (configuration.Groups.Any(g => g.CharacterIds.Contains(characterId)))
								continue;

							CharacterDetail characterDetail;
							if (!characterDetails.TryGetValue(characterId, out characterDetail))
							{
								throw new KeyNotFoundException("No character details for unexpected character ID: " + characterId);
							}
							if (characterDetail.Gender == CharacterGender.Neuter)
								charactersToProcessLast.Add(characterDetail);
							else
								AddCharacterToBestGroup(characterDetail, configuration);
						}
						foreach (var characterDetail in charactersToProcessLast)
							AddCharacterToBestGroup(characterDetail, configuration);							
					}
					bestConfiguration = TrialGroupConfiguration.Best(trialConfigurationsForNarratorsAndExtras, bestConfiguration);
					
					Debug.WriteLine("Trial configurations for " + maleNarrators + " male narrators and " + femaleNarrators + " female narrators:");
					for (int index = 0; index < trialConfigurationsForNarratorsAndExtras.Count; index++)
					{
						var config = trialConfigurationsForNarratorsAndExtras[index];
						Debug.WriteLine("   Configuration " + index + ((config == bestConfiguration) ? " (best)" : ""));
						Debug.WriteLine("   Minimum Proximity: " + config.MinimumProximity);
						Debug.WriteLine("   Conflicting Gender count: " + config.CharactersInGroupWithConflictingGender);
						Debug.WriteLine("   Narrator Groups:");
						foreach (var narrGroup in config.NarratorGroups)
							Debug.WriteLine("      " + narrGroup.VoiceActor.Gender + " group: " + narrGroup.CharacterIds.ToString().Replace("CharacterName.", ""));
						if (config.ExtraBiblicalGroup != null)
						{
							if (config.NarratorGroups.Contains(config.ExtraBiblicalGroup))
								Debug.WriteLine("   Extra-biblical group is a narrator group!");
							else
								Debug.WriteLine("   Extra-biblical group: " + config.ExtraBiblicalGroup.VoiceActor.Gender + " - " +
												config.ExtraBiblicalGroup.CharacterIds.ToString().Replace("CharacterName.", ""));
						}
						Debug.WriteLine("   Other Groups:");
						foreach (
							var group in config.Groups.Where(g => !config.NarratorGroups.Contains(g) && g != config.ExtraBiblicalGroup))
						{
							Debug.WriteLine("      " + @group.VoiceActor.Gender + " group: " + @group.CharacterIds.ToString().Replace("CharacterName.", ""));
						}
					}

					if (bestConfiguration.MinimumProximity >= Proximity.kDefaultMinimumProximity &&
						bestConfiguration.CharactersInGroupWithConflictingGender == 0 &&
						bestConfiguration.TotalCountOfNarratorGroupsIncludingCameos == maleNarrators + femaleNarrators)
					{
						return GetFinalizedGroups(bestConfiguration, actorsWithRealAssignments, realActorsToReset);
					}
				}

				if (fallbackPass)
					break;
				fallbackPass = true;
			} while (true);

			Debug.Assert(bestConfiguration != null);

			if (enforceProximityAndGenderConstraints &&
				(bestConfiguration.MinimumProximity < Proximity.kDefaultMinimumProximity ||
				bestConfiguration.CharactersInGroupWithConflictingGender > 0))
				return null;

			return GetFinalizedGroups(bestConfiguration, actorsWithRealAssignments, realActorsToReset);
		}

		private void EnsureActorListIsSetToRealActors(List<VoiceActor.VoiceActor> realActorsToReset)
		{
			if (realActorsToReset != null)
				m_project.VoiceActorList.AllActors = realActorsToReset;
		}

		private List<CharacterGroup> GetFinalizedGroups(TrialGroupConfiguration configuration, List<int> actorsWithRealAssignments, List<VoiceActor.VoiceActor> realActorsToReset)
		{
			List<CharacterGroup> groups = configuration.Groups;

			if (m_worker.CancellationPending)
			{
				EnsureActorListIsSetToRealActors(realActorsToReset);
				return GeneratedGroups = null;
			}
			if (realActorsToReset != null)
				actorsWithRealAssignments.Clear();

			CharacterGroupList.AssignGroupIds(groups);
			foreach (var group in groups)
			{
				if (!group.AssignedToCameoActor && !actorsWithRealAssignments.Contains(group.VoiceActorId))
					group.RemoveVoiceActor();

				//TODO - we need to figure out how to actually handle locked groups from a UI perspective.
				// But for now, we need to make sure we don't throw an exception if the user manually changes it.
				group.Closed = false;
			}

			GeneratedGroups = groups.Where(g => g.AssignedToCameoActor || g.CharacterIds.Any()).ToList();
			MinimumProximity = configuration.MinimumProximity;
			CountOfCharactersInGroupWithConflictingGender = configuration.CharactersInGroupWithConflictingGender;
			EnsureActorListIsSetToRealActors(realActorsToReset);
			return GeneratedGroups;
		}

		private List<CharacterGroup> CreateGroupsForActors(IEnumerable<VoiceActor.VoiceActor> actors)
		{
			List<CharacterGroup> groups = new List<CharacterGroup>();
			bool? projectHasChildRole = null;
			foreach (var voiceActor in actors)
			{
				if (voiceActor.Age == ActorAge.Child)
				{
					if (projectHasChildRole == null)
					{
						projectHasChildRole =
							m_project.AllCharacterIds.Any(c => m_project.AllCharacterDetailDictionary[c].Age == CharacterAge.Child);
					}

					if (!(bool)projectHasChildRole)
						continue;
				}

				CharacterGroup group = null;
				if (voiceActor.IsCameo)
					group = ProjectCharacterGroups.FirstOrDefault(g => g.VoiceActorId == voiceActor.Id);

				if (group == null)
				{
					group = new CharacterGroup(m_project);
					group.AssignVoiceActor(voiceActor.Id);
					if (voiceActor.IsCameo)
						group.Closed = true;
				}
				else
				{
					group = group.Copy();
					group.CharacterIds.IntersectWith(m_project.AllCharacterIds);
					group.Closed = true;
				}
				groups.Add(group);
			}
			return groups;
		}

		private IEnumerable<VoiceActor.VoiceActor> CreateGhostCastActors()
		{
			List<VoiceActor.VoiceActor> ghostCastActors = new List<VoiceActor.VoiceActor>();
			ghostCastActors.AddRange(CreateGhostCastActors(ActorGender.Male, ActorAge.Adult, m_ghostCastSize.Male, ghostCastActors.Count));
			ghostCastActors.AddRange(CreateGhostCastActors(ActorGender.Female, ActorAge.Adult, m_ghostCastSize.Female, ghostCastActors.Count));
			ghostCastActors.AddRange(CreateGhostCastActors(ActorGender.Male, ActorAge.Child, m_ghostCastSize.Child, ghostCastActors.Count));
			return ghostCastActors;
		}

		private IEnumerable<VoiceActor.VoiceActor> CreateGhostCastActors(ActorGender gender, ActorAge age, int number, int startingIdNumber)
		{
			for (int i = 0; i < number; i++)
				yield return new VoiceActor.VoiceActor
				{
					Id = startingIdNumber++,
					Gender = gender,
					Age = age,
				};
		}

		private void AddCharacterToBestGroup(CharacterDetail characterDetail, TrialGroupConfiguration configuration)
		{
			List<CharacterGroup> groups;

			IEnumerable<CharacterGroup> availableGroups = configuration.Groups.Where(g => !g.Closed &&
				configuration.IsGroupAvailableForBiblicalCharacterRoles(g));

			// ReSharper disable PossibleMultipleEnumeration
			if (!availableGroups.Any())
				availableGroups = configuration.Groups.Where(g => !g.Closed);

			var groupMatchQualityDictionary = new Dictionary<MatchQuality, List<CharacterGroup>>(configuration.Groups.Count);
			foreach (var characterGroup in availableGroups)
			{
				var voiceActor = characterGroup.VoiceActor;
				var quality = new MatchQuality(voiceActor.GetGenderMatchQuality(characterDetail), voiceActor.GetAgeMatchQuality(characterDetail));
				if (!groupMatchQualityDictionary.TryGetValue(quality, out groups))
					groupMatchQualityDictionary[quality] = groups = new List<CharacterGroup>();
				groups.Add(characterGroup);
			}
			// ReSharper restore PossibleMultipleEnumeration

			var groupToProximityDict = new Dictionary<CharacterGroup, WeightedMinimumProximity>();

			if (groupMatchQualityDictionary.TryGetValue(new MatchQuality(GenderMatchQuality.Perfect, AgeMatchQuality.Perfect), out groups))
			{
				CalculateProximityForGroups(characterDetail, groups, groupToProximityDict);
			}
			if (!groupToProximityDict.Any(i => i.Value.WeightedNumberOfBlocks >= Proximity.kDefaultMinimumProximity) &&
				groupMatchQualityDictionary.TryGetValue(new MatchQuality(GenderMatchQuality.Perfect, AgeMatchQuality.CloseAdult), out groups))
			{
				CalculateProximityForGroups(characterDetail, groups, groupToProximityDict);
			}
			if (!groupToProximityDict.Any(i => i.Value.WeightedNumberOfBlocks >= Proximity.kDefaultMinimumProximity || i.Value >= configuration.MinimumProximity) &&
				groupMatchQualityDictionary.TryGetValue(new MatchQuality(GenderMatchQuality.Perfect, AgeMatchQuality.AdultVsChild), out groups))
			{
				CalculateProximityForGroups(characterDetail, groups, groupToProximityDict);
			}
			if (!groupToProximityDict.Any(i => i.Value.WeightedNumberOfBlocks >= Proximity.kDefaultMinimumProximity || i.Value >= configuration.MinimumProximity) &&
				groupMatchQualityDictionary.TryGetValue(new MatchQuality(GenderMatchQuality.Acceptable, AgeMatchQuality.Perfect), out groups))
			{
				CalculateProximityForGroups(characterDetail, groups, groupToProximityDict, 1.1);
			}
			if (!groupToProximityDict.Any(i => i.Value.WeightedNumberOfBlocks >= Proximity.kDefaultMinimumProximity || i.Value >= configuration.MinimumProximity) &&
				groupMatchQualityDictionary.TryGetValue(new MatchQuality(GenderMatchQuality.Acceptable, AgeMatchQuality.CloseAdult), out groups))
			{
				CalculateProximityForGroups(characterDetail, groups, groupToProximityDict, 1.1);
			}
			if (!groupToProximityDict.Any(i => i.Value.WeightedNumberOfBlocks >= Proximity.kDefaultMinimumProximity || i.Value >= configuration.MinimumProximity) &&
				groupMatchQualityDictionary.TryGetValue(new MatchQuality(GenderMatchQuality.Acceptable, AgeMatchQuality.AdultVsChild), out groups))
			{
				CalculateProximityForGroups(characterDetail, groups, groupToProximityDict, 1.1);
			}
			if (!groupToProximityDict.Any(i => i.Value.WeightedNumberOfBlocks >= Proximity.kDefaultMinimumProximity || i.Value >= configuration.MinimumProximity) &&
				groupMatchQualityDictionary.TryGetValue(new MatchQuality(GenderMatchQuality.Mismatch, AgeMatchQuality.Perfect), out groups))
			{
				CalculateProximityForGroups(characterDetail, groups, groupToProximityDict, 2.3);
			}
			if (!groupToProximityDict.Any(i => i.Value.WeightedNumberOfBlocks >= Proximity.kDefaultMinimumProximity || i.Value >= configuration.MinimumProximity) &&
				groupMatchQualityDictionary.TryGetValue(new MatchQuality(GenderMatchQuality.Mismatch, AgeMatchQuality.CloseAdult), out groups))
			{
				CalculateProximityForGroups(characterDetail, groups, groupToProximityDict, 2.4);
			}
			if (!groupToProximityDict.Any(i => i.Value.WeightedNumberOfBlocks >= Proximity.kDefaultMinimumProximity || i.Value >= configuration.MinimumProximity) &&
				groupMatchQualityDictionary.TryGetValue(new MatchQuality(GenderMatchQuality.Mismatch, AgeMatchQuality.AdultVsChild), out groups))
			{
				CalculateProximityForGroups(characterDetail, groups, groupToProximityDict, 2.5);
			}
			if (!groupToProximityDict.Any(i => i.Value.WeightedNumberOfBlocks >= Proximity.kDefaultMinimumProximity || i.Value >= configuration.MinimumProximity) &&
				groupMatchQualityDictionary.TryGetValue(new MatchQuality(GenderMatchQuality.Perfect, AgeMatchQuality.Mismatch), out groups))
			{
				CalculateProximityForGroups(characterDetail, groups, groupToProximityDict, 2.7);
			}
			if (!groupToProximityDict.Any(i => i.Value.WeightedNumberOfBlocks >= Proximity.kDefaultMinimumProximity || i.Value >= configuration.MinimumProximity) &&
				groupMatchQualityDictionary.TryGetValue(new MatchQuality(GenderMatchQuality.Acceptable, AgeMatchQuality.Mismatch), out groups))
			{
				CalculateProximityForGroups(characterDetail, groups, groupToProximityDict, 2.9);
			}
			if (!groupToProximityDict.Any(i => i.Value.WeightedNumberOfBlocks >= Proximity.kDefaultMinimumProximity || i.Value >= configuration.MinimumProximity) &&
				groupMatchQualityDictionary.TryGetValue(new MatchQuality(GenderMatchQuality.Mismatch, AgeMatchQuality.Mismatch), out groups))
			{
				CalculateProximityForGroups(characterDetail, groups, groupToProximityDict, 3.2);
			}
			var bestGroupEntry = groupToProximityDict.Aggregate((l, r) => l.Value.WeightedNumberOfBlocks > r.Value.WeightedNumberOfBlocks ? l : r);
			var bestGroup = bestGroupEntry.Key;
			if (configuration.MinimumProximity > bestGroupEntry.Value)
			{
				// We're adding the character to the best group we could find, but it is now the *worst* group in the configuration.
				configuration.NoteGroupWithWorstProximity(bestGroupEntry.Key, bestGroupEntry.Value);
			}
			bestGroup.CharacterIds.Add(characterDetail.CharacterId);
			if (RelatedCharactersData.Singleton.GetCharacterIdsForType(CharacterRelationshipType.SameCharacterWithMultipleAges).Contains(characterDetail.CharacterId))
				foreach (var relatedCharacters in RelatedCharactersData.Singleton.GetCharacterIdToRelatedCharactersDictionary()[characterDetail.CharacterId])
					bestGroup.CharacterIds.AddRange(relatedCharacters.CharacterIds.Where(c => m_project.AllCharacterIds.Contains(c)));
		}

		private void CalculateProximityForGroups(CharacterDetail characterDetail, IEnumerable<CharacterGroup> characterGroups, Dictionary<CharacterGroup, WeightedMinimumProximity> groupToProximityDict, double weightingFactor = 1.0)
		{
			if (!weightingFactor.Equals(1d))
			{
				foreach (var weightedMinimumProximity in groupToProximityDict)
					weightedMinimumProximity.Value.WeightingPower = weightingFactor;
			}

			foreach (var group in characterGroups)
			{
				HashSet<string> testSet = new HashSet<string>(group.CharacterIds);
				testSet.Add(characterDetail.CharacterId);
				groupToProximityDict.Add(group, new WeightedMinimumProximity(m_proximity.CalculateMinimumProximity(testSet)));
			}
		}

		public class TrialGroupConfiguration
		{
			private readonly List<CharacterGroup> m_groups;
			private readonly bool m_allowGroupsForNonBiblicalCharactersToDoBiblicalCharacterRoles;
			private readonly IReadOnlyDictionary<string, CharacterDetail> m_characterDetails;
			internal int CharactersInGroupWithConflictingGender { get; private set; }
			internal CharacterGroup GroupWithWorstProximity { get; private set; }
			internal MinimumProximity MinimumProximity { get; private set; }
			internal List<CharacterGroup> NarratorGroups { get; set; }
			internal int TotalCountOfNarratorGroupsIncludingCameos
			{
				get
				{
					return NarratorGroups.Count +
						m_groups.Count(g => g.AssignedToCameoActor &&
						g.CharacterIds.Any(c => CharacterVerseData.IsCharacterOfType(c, CharacterVerseData.StandardCharacter.Narrator)));
				}
			}
			// TODO: Change to List
			internal CharacterGroup ExtraBiblicalGroup { get; set; }
			internal List<CharacterGroup> Groups
			{
				get { return m_groups; }
			}

			// Note that the list of includedBooks omits any whose narrator role has already been assinged to a group.
			private TrialGroupConfiguration(bool allowGroupsForNonBiblicalCharactersToDoBiblicalCharacterRoles,
				IEnumerable<CharacterGroup> characterGroups,
				IReadOnlyDictionary<string, CharacterDetail> characterDetails,
				int numberOfMaleNarratorGroups, int numberOfFemaleNarratorGroups,
				int numberOfMaleExtraBiblicalGroups, int numberOfFemaleExtraBiblicalGroups,
				Dictionary<string, int> keyStrokesByCharacterId,
				List<CharacterDetail> includedCharacterDetails, List<string> includedBooks)
			{
				m_groups = characterGroups.Select(g => g.Copy()).ToList();
				m_allowGroupsForNonBiblicalCharactersToDoBiblicalCharacterRoles = allowGroupsForNonBiblicalCharactersToDoBiblicalCharacterRoles;
				m_characterDetails = characterDetails;

				if (Groups.Count == 1)
				{
					NarratorGroups = includedBooks.Any() ? Groups.ToList() : new List<CharacterGroup>(0);
					ExtraBiblicalGroup = Groups[0];
				}
				else
				{
					var availableAdultGroups = GetGroupsAvailableForNarratorOrExtraBiblical(Groups, m_allowGroupsForNonBiblicalCharactersToDoBiblicalCharacterRoles);
					if (availableAdultGroups.Count == 1)
					{
						NarratorGroups = availableAdultGroups.ToList();
						ExtraBiblicalGroup = NarratorGroups[0];
					}
					else
					{
						Debug.WriteLine("Male narrators desired = " + numberOfMaleNarratorGroups);
						Debug.WriteLine("Female narrators desired = " + numberOfFemaleNarratorGroups);
						NarratorGroups = new List<CharacterGroup>(numberOfMaleNarratorGroups + numberOfFemaleNarratorGroups);
						var idealAge = new List<ActorAge> {ActorAge.Adult};
						int attempt = 0;
						while (attempt++ < 2 && (numberOfMaleNarratorGroups > 0 || numberOfFemaleNarratorGroups > 0 ||
												numberOfMaleExtraBiblicalGroups > 0 || numberOfFemaleExtraBiblicalGroups > 0))
						{
							foreach (var characterGroup in availableAdultGroups)
							{
								var actor = characterGroup.VoiceActor;
								if (!idealAge.Contains(actor.Age))
									continue;

								if (actor.Gender == ActorGender.Male)
								{
									if (numberOfMaleNarratorGroups > 0)
									{
										NarratorGroups.Add(characterGroup);
										numberOfMaleNarratorGroups--;
									}
									else if (numberOfMaleExtraBiblicalGroups > 0)
									{
										// TODO: Deal with numberOfMaleExtraBiblicalGroups > 1 (see above)
										ExtraBiblicalGroup = characterGroup;
										numberOfMaleExtraBiblicalGroups--;
									}
								}
								else
								{
									if (numberOfFemaleNarratorGroups > 0)
									{
										NarratorGroups.Add(characterGroup);
										numberOfFemaleNarratorGroups--;
									}
									else if (numberOfFemaleExtraBiblicalGroups > 0)
									{
										// TODO: Deal with numberOfFemaleExtraBiblicalGroups > 1 (see way above)
										ExtraBiblicalGroup = characterGroup;
										numberOfFemaleExtraBiblicalGroups--;
									}
								}
								if (numberOfMaleNarratorGroups == 0 && numberOfFemaleNarratorGroups == 0 &&
									numberOfMaleExtraBiblicalGroups == 0 && numberOfFemaleExtraBiblicalGroups == 0)
									break;
							}
							idealAge = new List<ActorAge> {ActorAge.Elder, ActorAge.YoungAdult};
						}
					}
					// TODO: Handle multiple extra-biblical groups
					if (numberOfMaleExtraBiblicalGroups == 1)
						ExtraBiblicalGroup = NarratorGroups.First(g => g.VoiceActor.Gender == ActorGender.Male);
					else if (numberOfFemaleExtraBiblicalGroups == 1)
						ExtraBiblicalGroup = NarratorGroups.First(g => g.VoiceActor.Gender == ActorGender.Female);
				}

				AssignNarratorCharactersToNarratorGroups(keyStrokesByCharacterId, includedBooks);
				// TODO if extra-biblical characters are being assigned to specific group(s) based on type
				AssignExtraBiblicalCharactersToExtraBiblicalGroups(includedCharacterDetails);
				AssignDeityCharacters(includedCharacterDetails);
			}

			// Possible (and impossible) scenarios:
			// 0) number of narrators > number of books -> This should never happen!
			// 1) single narrator -> EASY: only one group!
			// 2) number of narrators == number of books -> Each narrator does one book
			// 3) number of narrators == number of authors -> add narrator to group with other books by same other, if any; otherwise first empty group
			// 4) number of narrators < number of authors -> shorter books share narrators
			// 5) number of narrators > number of authors -> break up most prolific authors into multiple narrator groups
			private void AssignNarratorCharactersToNarratorGroups(Dictionary<string, int> keyStrokesByCharacterId, List<string> bookIds)
			{
				if (!NarratorGroups.Any())
				{
					if (!bookIds.Any())
						return;
					foreach (var characterGroup in Groups)
					{
						Debug.WriteLine("Cameo = " + characterGroup.AssignedToCameoActor);
						Debug.WriteLine("CharacterIds = " + characterGroup.CharacterIds);
					}
					throw new Exception("None of the " + Groups.Count + " groups were suitable for narrator role.");
				}

				Debug.Assert(NarratorGroups.Count <= bookIds.Count);

				if (NarratorGroups.Count == 1)
				{
					foreach (var bookId in bookIds)
						AddNarratorToGroup(NarratorGroups[0], bookId);
					return;
				}
				if (NarratorGroups.Count == bookIds.Count)
				{
					int n = 0;
					foreach (var bookId in bookIds)
						AddNarratorToGroup(NarratorGroups[n++], bookId);
					return;
				}

				var authors = new Dictionary<BiblicalAuthors.Author, List<string>>();
				foreach (var bookId in bookIds)
				{
					var author = BiblicalAuthors.GetAuthorOfBook(bookId);
					List<string> booksForAuthor;
					if (authors.TryGetValue(author, out booksForAuthor))
						booksForAuthor.Add(bookId);
					else
						authors[author] = new List<string> { bookId };
				}

				if (NarratorGroups.Count == authors.Count)
				{
					int i = 0;
					foreach (var booksForAuthor in authors.Values)
					{
						foreach (var bookId in booksForAuthor)
							AddNarratorToGroup(NarratorGroups[i], bookId);
						i++;
					}
					return;
				}
				if (NarratorGroups.Count < authors.Count)
				{
					var authorStats = new List<AuthorStats>(authors.Count);
					authorStats.AddRange(authors.Select(authorsAndBooks =>
						new AuthorStats(authorsAndBooks.Key, authorsAndBooks.Value, keyStrokesByCharacterId)));

					DistributeBooksAmongNarratorGroups(authorStats, NarratorGroups);
					return;
				}
				if (NarratorGroups.Count < bookIds.Count)
					DistributeBooksAmongNarratorGroups(NarratorGroups, authors.Count, bookIds, keyStrokesByCharacterId);
			}

			private static void AddNarratorToGroup(CharacterGroup narratorGroup, string bookId)
			{
				narratorGroup.CharacterIds.Add(CharacterVerseData.GetStandardCharacterId(bookId, CharacterVerseData.StandardCharacter.Narrator));
			}

			/// <summary>
			/// This is broken out as a separate static method to facilitate faster and more complete unit testing
			/// </summary>
			/// <param name="authorStats">keystroke stats for each biblical author who wrote one or more books included in the project</param>
			/// <param name="narratorGroups">list of available narrator groups (no greater than the total number of authors)</param>
			public static void DistributeBooksAmongNarratorGroups(List<AuthorStats> authorStats, List<CharacterGroup> narratorGroups)
			{
				authorStats.Sort(new AuthorStatsComparer());
				int min = 0;
				int n = 0;
				bool addingAdditionalAuthorsToCurrentNarratorGroup = false;
				bool currentMustCombine = false;
				for (int i = authorStats.Count - 1; i >= min; i--)
				{
					foreach (var bookId in authorStats[i].BookIds)
						AddNarratorToGroup(narratorGroups[n], bookId);

					int numberOfRemainingNarratorGroupsToAssign = narratorGroups.Count - n - 1;
					if (numberOfRemainingNarratorGroupsToAssign > 0)
					{
						if (numberOfRemainingNarratorGroupsToAssign == 1)
							currentMustCombine = true;
						if (i < (authorStats.Count - narratorGroups.Count) * 2)
						{
							// We now need to work our way down through the list of authors to see which one(s), if any
							// should be combined with the current author. For this purpose, we definitely need to skip
							// enough authors to allow the "middle" ones to combine into groups of at least 2 authors.
							// We don't want to combine this author with too many or too few of the smaller ones, so we're
							// looking for the "breaking point" where combining it would result in groups that are as
							// balanced as possible.
							bool hitTippingPoint = false;
							for (int lastAuthorToConsiderCombiningWithCurrent = i - (numberOfRemainingNarratorGroupsToAssign) * 2 - 1;
								lastAuthorToConsiderCombiningWithCurrent >= min;
								lastAuthorToConsiderCombiningWithCurrent--)
							{
								int keystrokesForCurrentPlusAllAuthorsFromMinUpToLastExclusive =
									authorStats[i].KeyStrokeCount +
									authorStats.Skip(min).Take(lastAuthorToConsiderCombiningWithCurrent - min).Sum(s => s.KeyStrokeCount);
								int keystrokesForCurrentPlusAllAuthorsFromMinUpToLastInclusive =
									keystrokesForCurrentPlusAllAuthorsFromMinUpToLastExclusive +
									authorStats[lastAuthorToConsiderCombiningWithCurrent].KeyStrokeCount;
								int keystrokesForRemainingGroupsIfLastIsExcluded =
									authorStats.Skip(lastAuthorToConsiderCombiningWithCurrent + 1)
										.Take(i - lastAuthorToConsiderCombiningWithCurrent - 1).Sum(s => s.KeyStrokeCount);
								int averageKeystrokesForRemainingGroupsIfLastIsIncluded =
									(keystrokesForRemainingGroupsIfLastIsExcluded + authorStats[lastAuthorToConsiderCombiningWithCurrent].KeyStrokeCount) /
									numberOfRemainingNarratorGroupsToAssign;
								int averageKeystrokesForRemainingGroupsIfLastIsExcluded = keystrokesForRemainingGroupsIfLastIsExcluded / numberOfRemainingNarratorGroupsToAssign;

								if (keystrokesForCurrentPlusAllAuthorsFromMinUpToLastInclusive >= averageKeystrokesForRemainingGroupsIfLastIsExcluded &&
									keystrokesForCurrentPlusAllAuthorsFromMinUpToLastExclusive < averageKeystrokesForRemainingGroupsIfLastIsIncluded)
									hitTippingPoint = true;

								if ((hitTippingPoint &&
									keystrokesForCurrentPlusAllAuthorsFromMinUpToLastInclusive - averageKeystrokesForRemainingGroupsIfLastIsExcluded <=
									averageKeystrokesForRemainingGroupsIfLastIsIncluded - keystrokesForCurrentPlusAllAuthorsFromMinUpToLastExclusive) ||
									currentMustCombine && min == lastAuthorToConsiderCombiningWithCurrent)
								{
									addingAdditionalAuthorsToCurrentNarratorGroup = true;
									while (min <= lastAuthorToConsiderCombiningWithCurrent)
									{
										foreach (var bookId in authorStats[min++].BookIds)
											AddNarratorToGroup(narratorGroups[n], bookId);
									}
									currentMustCombine = true; // Once we find an author that combines, all subequent authors must also combine.
									break;
								}
							}
						}
						if (n < narratorGroups.Count - 1 &&
							(i > numberOfRemainingNarratorGroupsToAssign || addingAdditionalAuthorsToCurrentNarratorGroup))
						{
							n++;
							addingAdditionalAuthorsToCurrentNarratorGroup = false;
						}
						else
							addingAdditionalAuthorsToCurrentNarratorGroup = true;
					}
				}
				Debug.Assert(n == narratorGroups.Count - 1);
			}

			/// <summary>
			/// This is broken out as a separate static method to facilitate faster and more complete unit testing
			/// </summary>
			/// <param name="narratorGroups">list of available narrator groups (greater than the number of authors but
			/// less than the total number of books)</param>
			/// <param name="countOfAuthors">Number of authors of books included in the project</param>
			/// <param name="bookIDs">IDs of books included in the project</param>
			/// <param name="keyStrokesByCharacterId">Dictionary of total key strokes for each character ID</param>
			public static void DistributeBooksAmongNarratorGroups(List<CharacterGroup> narratorGroups, int countOfAuthors,
				IEnumerable<string> bookIDs, Dictionary<string, int> keyStrokesByCharacterId)
			{
				// Go through the books in descending size order. Add each book to a group. If there are already one or more groups for that author, then:
				// 1) if there are still any available "extra" narrator groups, use one of those;
 				// 2) otherwise, add this book to the smallest existing group for that author.
	
				var extraNarrators = narratorGroups.Count - countOfAuthors;
				Debug.Assert(extraNarrators > 0);

				var booksByNarratorKeystrokes = new List<Tuple<string, int>>(bookIDs
					.Select(bookId => new Tuple<string, int>(bookId,
						keyStrokesByCharacterId[CharacterVerseData.GetStandardCharacterId(bookId, CharacterVerseData.StandardCharacter.Narrator)])));
				Debug.Assert(booksByNarratorKeystrokes.Count > narratorGroups.Count);

				booksByNarratorKeystrokes.Sort((a, b) => b.Item2.CompareTo(a.Item2));
				var narratorGroupsByAuthor = new Dictionary<BiblicalAuthors.Author, List<CharacterGroup>>(countOfAuthors);
				int n = 0;
				foreach (var bookId in booksByNarratorKeystrokes.Select(t => t.Item1))
				{
					var author = BiblicalAuthors.GetAuthorOfBook(bookId);
					List<CharacterGroup> groupsForAuthor;
					CharacterGroup narratorGroupForBook;
					if (narratorGroupsByAuthor.TryGetValue(author, out groupsForAuthor))
					{
						if (extraNarrators > 0)
						{
							narratorGroupForBook = narratorGroups[n++];
							extraNarrators--;
						}
						else
						{
							narratorGroupForBook = groupsForAuthor.First();
							var lowestKeyStrokeCount = narratorGroupForBook.CharacterIds.Sum(c => keyStrokesByCharacterId[c]);
							foreach (var group in groupsForAuthor.Skip(1))
							{
								var keyStrokeCount = group.CharacterIds.Sum(c => keyStrokesByCharacterId[c]);
								if (keyStrokeCount < lowestKeyStrokeCount)
								{
									narratorGroupForBook = group;
									lowestKeyStrokeCount = keyStrokeCount;
								}
							}
						}
						groupsForAuthor.Add(narratorGroupForBook);
					}
					else
					{
						narratorGroupForBook = narratorGroups[n++];
						narratorGroupsByAuthor[author] = new List<CharacterGroup> { narratorGroupForBook };
					}
					AddNarratorToGroup(narratorGroupForBook, bookId);
				}
			}

			private void AssignExtraBiblicalCharactersToExtraBiblicalGroups(List<CharacterDetail> includedCharacterDetails)
			{
				if (ExtraBiblicalGroup == null)
					return;
				ExtraBiblicalGroup.CharacterIds.AddRange(includedCharacterDetails.
					Select(cd => cd.CharacterId).Where(c => CharacterVerseData.IsCharacterStandard(c, false) &&
					!Groups.Any(g => g.CharacterIds.Contains(c))));
			}

			private void AssignDeityCharacters(List<CharacterDetail> includedCharacterDetails)
			{
				var groupsAvailableForDiety = Groups.Where(g => !g.Closed && !g.CharacterIds.Any() &&
					IsGroupAvailableForDeity(g) &&
					g.VoiceActor.Gender == ActorGender.Male &&
					g.VoiceActor.Age != ActorAge.Child).ToList();

				var setsOfCharactersToGroup = DeityCharacters.LastOrDefault(kvp => kvp.Key <= groupsAvailableForDiety.Count).Value;
				if (setsOfCharactersToGroup == null)
					return;

				foreach (var characterSet in setsOfCharactersToGroup)
				{
					var bestGroup = groupsAvailableForDiety.FirstOrDefault(g => g.VoiceActor.Age == ActorAge.Adult)
						?? groupsAvailableForDiety.First();

					var charactersToPutInGroup = characterSet.Where(c => includedCharacterDetails.Any(d => d.CharacterId == c)).Except(Groups.SelectMany(g => g.CharacterIds)).ToList();
					if (charactersToPutInGroup.Any())
					{
						bestGroup.CharacterIds.AddRange(charactersToPutInGroup);
						bestGroup.Closed = true;
						groupsAvailableForDiety.Remove(bestGroup);
					}
				}
			}

			private bool IsBetterThan(TrialGroupConfiguration other)
			{
				if (CharactersInGroupWithConflictingGender < other.CharactersInGroupWithConflictingGender)
					return true;
				if (CharactersInGroupWithConflictingGender > other.CharactersInGroupWithConflictingGender || 
					MinimumProximity < other.MinimumProximity)
					return false;
				if (MinimumProximity > other.MinimumProximity)
					return true;
				return NarratorGroups.Count > other.NarratorGroups.Count;
			}

			private static List<CharacterGroup> GetGroupsAvailableForNarratorOrExtraBiblical(IEnumerable<CharacterGroup> groups,
				bool allowGroupsForNonBiblicalCharactersToDoBiblicalCharacterRoles)
			{
				return groups.Where(g => !g.Closed &&
					(!g.CharacterIds.Any() || allowGroupsForNonBiblicalCharactersToDoBiblicalCharacterRoles) &&
					g.VoiceActor.Age != ActorAge.Child).ToList();
			}

			internal bool IsGroupAvailableForBiblicalCharacterRoles(CharacterGroup group)
			{
				return m_allowGroupsForNonBiblicalCharactersToDoBiblicalCharacterRoles ||
					(!NarratorGroups.Contains(group) && ExtraBiblicalGroup != group);
			}

			private bool IsGroupAvailableForDeity(CharacterGroup group)
			{
				return !(NarratorGroups.Contains(group) || ExtraBiblicalGroup == group);
			}

			internal static List<TrialGroupConfiguration> GeneratePossibilities(bool allowGroupsForNonBiblicalCharactersToDoBiblicalCharacterRoles,
				List<CharacterGroup> characterGroups,
				int numberOfMaleNarratorGroups, int numberOfFemaleNarratorGroups, bool allowFemaleExtraBiblical, List<CharacterDetail> includedCharacterDetails,
				Dictionary<string, int> keyStrokesByCharacterId, Project project, IReadOnlyDictionary<string, CharacterDetail> characterDetails)
			{
				var includedBooks = project.IncludedBooks.Select(b => b.BookId).ToList();
				int maleGroupsWithExistingNarratorRoles = 0;
				int femaleGroupsWithExistingNarratorRoles = 0;
				CharacterGroup groupWithWorstPrioximity = null;
				MinimumProximity worstProximity = null;
				var proximity = new Proximity(project.IncludedBooks);
				foreach (var characterGroup in characterGroups)
				{
					var narratorRoles = characterGroup.CharacterIds.Where(c =>
						CharacterVerseData.IsCharacterOfType(c, CharacterVerseData.StandardCharacter.Narrator)).ToList();

					if (narratorRoles.Any())
					{
						foreach (var bookId in narratorRoles.Select(CharacterVerseData.GetBookCodeFromStandardCharacterId))
							includedBooks.Remove(bookId);
						if (characterGroup.VoiceActor.Gender == ActorGender.Male)
							maleGroupsWithExistingNarratorRoles++;
						else
							femaleGroupsWithExistingNarratorRoles++;
					}

					if (characterGroup.AssignedToCameoActor)
						continue;

					var minimumProximity = proximity.CalculateMinimumProximity(characterGroup.CharacterIds);
					if ((worstProximity != null && minimumProximity < worstProximity) || minimumProximity < Int32.MaxValue)
					{
						groupWithWorstPrioximity = characterGroup;
						worstProximity = minimumProximity;
					}
				}

				var list = new List<TrialGroupConfiguration>(2);

				var availableAdultGroups = GetGroupsAvailableForNarratorOrExtraBiblical(characterGroups, allowGroupsForNonBiblicalCharactersToDoBiblicalCharacterRoles);
				if (!availableAdultGroups.Any())
					return list;
				var availableMaleGroups = availableAdultGroups.Where(g => g.VoiceActor.Gender == ActorGender.Male).ToList();
				var availableFemaleGroups = availableAdultGroups.Where(g => g.VoiceActor.Gender == ActorGender.Female).ToList();
				var numberOfExtraBiblicalGroups = 1;

				int numberOfUnassignedMaleNarrators, numberOfUnassignedFemaleNarrators;
				if (!includedBooks.Any())
				{
					numberOfMaleNarratorGroups = 0;
					numberOfFemaleNarratorGroups = 0;
					numberOfUnassignedMaleNarrators = 0;
					numberOfUnassignedFemaleNarrators = 0;
				}
				else
				{
					numberOfMaleNarratorGroups = Math.Min(numberOfMaleNarratorGroups, availableMaleGroups.Count + maleGroupsWithExistingNarratorRoles);
					numberOfFemaleNarratorGroups = Math.Min(numberOfFemaleNarratorGroups,availableFemaleGroups.Count + femaleGroupsWithExistingNarratorRoles);
					if (numberOfMaleNarratorGroups + numberOfFemaleNarratorGroups == 0)
						numberOfMaleNarratorGroups = 1;
					numberOfUnassignedMaleNarrators = numberOfMaleNarratorGroups - maleGroupsWithExistingNarratorRoles;
					numberOfUnassignedFemaleNarrators = numberOfFemaleNarratorGroups - femaleGroupsWithExistingNarratorRoles;
					if (numberOfUnassignedMaleNarrators + numberOfUnassignedFemaleNarrators == 0)
						numberOfUnassignedMaleNarrators = 1;

					if (!includedCharacterDetails.Any(c => CharacterVerseData.IsCharacterStandard(c.CharacterId, false)))
						numberOfExtraBiblicalGroups = 0;
				}

				if (availableMaleGroups.Count >= Math.Max(Math.Min(1, numberOfExtraBiblicalGroups), numberOfUnassignedMaleNarrators))
				{
					list.Add(new TrialGroupConfiguration(allowGroupsForNonBiblicalCharactersToDoBiblicalCharacterRoles, characterGroups,
						characterDetails, numberOfUnassignedMaleNarrators, numberOfUnassignedFemaleNarrators,
						Math.Min(1, numberOfExtraBiblicalGroups), 0, keyStrokesByCharacterId, includedCharacterDetails, includedBooks));
				}
				if (allowFemaleExtraBiblical && numberOfExtraBiblicalGroups > 0 && availableFemaleGroups.Count >= Math.Max(1, numberOfUnassignedFemaleNarrators))
				{
					list.Add(new TrialGroupConfiguration(allowGroupsForNonBiblicalCharactersToDoBiblicalCharacterRoles, characterGroups,
						characterDetails, numberOfUnassignedMaleNarrators, numberOfUnassignedFemaleNarrators,
						0, numberOfExtraBiblicalGroups, keyStrokesByCharacterId, includedCharacterDetails, includedBooks));
				}
				if (!list.Any() && allowGroupsForNonBiblicalCharactersToDoBiblicalCharacterRoles)
				{
					list.Add(new TrialGroupConfiguration(allowGroupsForNonBiblicalCharactersToDoBiblicalCharacterRoles, characterGroups,
					characterDetails, numberOfUnassignedMaleNarrators, numberOfUnassignedFemaleNarrators,
					numberOfExtraBiblicalGroups, 0, keyStrokesByCharacterId, includedCharacterDetails, includedBooks));
					Debug.Assert(list[0].NarratorGroups.Count + maleGroupsWithExistingNarratorRoles + femaleGroupsWithExistingNarratorRoles == 1);
				}

				if (groupWithWorstPrioximity != null)
				{
					foreach (var configuration in list)
						configuration.NoteGroupWithWorstProximity(groupWithWorstPrioximity, worstProximity);
				}
				return list;
			}

			internal static TrialGroupConfiguration Best(List<TrialGroupConfiguration> configurations, TrialGroupConfiguration previousBest)
			{
				foreach (var configuration in configurations)
					configuration.CalculateConflicts();

				var best = previousBest ?? configurations.First();
				// ReSharper disable once AccessToModifiedClosure
				foreach (var config in configurations.Skip(previousBest == null ? 1 : 0).Where(config => config.IsBetterThan(best)))
					best = config;
				return best;
			}

			private void CalculateConflicts()
			{
				foreach (var group in Groups.Where(g => g.VoiceActor.Age != ActorAge.Child))
				{
					if (group.VoiceActor.Gender == ActorGender.Female)
					{
						CharactersInGroupWithConflictingGender += RelatedCharactersData.Singleton.UniqueIndividuals(group.CharacterIds).Count(c =>
						{
							var detail = m_characterDetails[c];
							return detail.Gender == CharacterGender.Male || detail.Gender == CharacterGender.PreferMale;
						});
					}
					else
					{
						CharactersInGroupWithConflictingGender += RelatedCharactersData.Singleton.UniqueIndividuals(group.CharacterIds).Count(c =>
						{
							var detail = m_characterDetails[c];
							return detail.Gender == CharacterGender.Female || detail.Gender == CharacterGender.PreferFemale;
						});
					}
				}
			}

			internal void NoteGroupWithWorstProximity(CharacterGroup worstGroup, MinimumProximity minimumProximity)
			{
				GroupWithWorstProximity = worstGroup;
				MinimumProximity = minimumProximity;
			}
		}
	}
}
