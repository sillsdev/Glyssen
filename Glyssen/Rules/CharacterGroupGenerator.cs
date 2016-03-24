using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Glyssen.Character;
using Glyssen.Dialogs;
using Glyssen.VoiceActor;
using SIL.Extensions;

namespace Glyssen.Rules
{
	public class CharacterGroupGenerator
	{
		private readonly Project m_project;
		private readonly Dictionary<string, int> m_keyStrokesByCharacterId;
		private readonly IComparer<string> m_characterIdComparer;
		private readonly Proximity m_proximity;
		private readonly BackgroundWorker m_worker;

		private static readonly SortedDictionary<int, IList<HashSet<string>>> DeityCharacters;

		private IList<CharacterGroup> ProjectCharacterGroups
		{
			get { return m_project.CharacterGroupList.CharacterGroups; }
		}

		static CharacterGroupGenerator()
		{
			DeityCharacters = new SortedDictionary<int, IList<HashSet<string>>>();
			DeityCharacters.Add(2, new List<HashSet<string>> { new HashSet<string> { "Jesus", "God", "Holy Spirit, the", "scripture" } });
			var jesusSet = new HashSet<string> { "Jesus" };
			DeityCharacters.Add(5, new List<HashSet<string>> { jesusSet, new HashSet<string> { "God", "Holy Spirit, the", "scripture" } });
			var holySpiritSet = new HashSet<string> { "Holy Spirit, the" };
			DeityCharacters.Add(8, new List<HashSet<string>> { jesusSet, new HashSet<string> { "God", "scripture" }, holySpiritSet });
			DeityCharacters.Add(18, new List<HashSet<string>> { jesusSet, new HashSet<string> { "God" }, holySpiritSet, new HashSet<string> { "scripture" } });
		}

		public CharacterGroupGenerator(Project project, Dictionary<string, int> keyStrokesByCharacterId, BackgroundWorker worker = null)
		{
			m_project = project;
			m_keyStrokesByCharacterId = new Dictionary<string, int>(keyStrokesByCharacterId);
			m_proximity = new Proximity(project);
			m_characterIdComparer = new CharacterByKeyStrokeComparer(m_keyStrokesByCharacterId);
			m_worker = worker ?? new BackgroundWorker();
		}

		public List<CharacterGroup> GeneratedGroups { get; private set; }

		public void ApplyGeneratedGroupsToProject(bool attemptToPreserveActorAssignments = true)
		{
			// Create a copy. Cameos are handled in the generation code (because we always maintain those assignments).
			List<CharacterGroup> previousGroups = attemptToPreserveActorAssignments ?
				ProjectCharacterGroups.Where(g => !g.AssignedToCameoActor).ToList() : new List<CharacterGroup>();

			ProjectCharacterGroups.Clear();
			ProjectCharacterGroups.AddRange(GeneratedGroups);

			if (previousGroups.Count > 0)
			{
				// We assume the parts with the most keystrokes are most important to maintain
				var sortedDict = from entry in m_keyStrokesByCharacterId orderby entry.Value descending select entry;
				foreach (var entry in sortedDict)
				{
					string characterId = entry.Key;
					var previousGroupWithCharacter = previousGroups.FirstOrDefault(g => g.CharacterIds.Contains(characterId));
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
		}

		public List<CharacterGroup> GenerateCharacterGroups()
		{
			m_project.SetDefaultCharacterGroupGenerationPreferences();

			List<VoiceActor.VoiceActor> actorsForGeneration;
			List<VoiceActor.VoiceActor> realActorsToReset = null;
			if (m_project.CharacterGroupGenerationPreferences.CastSizeOption == CastSizeRow.MatchVoiceActorList)
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

			var sortedDict = from entry in m_keyStrokesByCharacterId orderby entry.Value descending select entry;

			IReadOnlyDictionary<string, CharacterDetail> characterDetails = m_project.AllCharacterDetailDictionary;
			var includedCharacterDetails = characterDetails.Values.Where(c => sortedDict.Select(e => e.Key).Contains(c.CharacterId)).ToList();

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

				matchingGroups.First().CharacterIds.Add(characterDetailToActors.Key.CharacterId);

				if (matchingGroups.Count == 1)
					actorsWithRealAssignments.Add(matchingGroups[0].VoiceActorId);

				foreach (var characterGroup in matchingGroups)
					characterGroup.Closed = true;
			}

			if (m_worker.CancellationPending)
			{
				EnsureActorListIsSetToRealActors(realActorsToReset);
				return GeneratedGroups = null;
			}

			// TODO: Make sure we didn't close all the groups (unless we assigned all the character IDs)

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

			m_project.SetCharacterGroupGenerationPreferencesToValidValues();
			int maxMaleNarrators = m_project.CharacterGroupGenerationPreferences.NumberOfMaleNarrators;
			int maxFemaleNarrators = m_project.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators;

			TrialGroupConfiguration bestConfiguration = null;
			do
			{
				if (m_worker.CancellationPending)
				{
					EnsureActorListIsSetToRealActors(realActorsToReset);
					return GeneratedGroups = null;
				}

				var trialConfigurationsForNarratorsAndExtras = TrialGroupConfiguration.GeneratePossibilities(characterGroups,
					ref maxMaleNarrators, ref maxFemaleNarrators, includedCharacterDetails, m_keyStrokesByCharacterId, m_project);

				if (trialConfigurationsForNarratorsAndExtras.Any())
				{
					foreach (var configuration in trialConfigurationsForNarratorsAndExtras)
					{
						if (m_worker.CancellationPending)
						{
							EnsureActorListIsSetToRealActors(realActorsToReset);
							return GeneratedGroups = null;
						}

						foreach (var entry in sortedDict)
						{
							string characterId = entry.Key;

							if (configuration.Groups.Any(g => g.CharacterIds.Contains(characterId)))
								continue;

							CharacterDetail characterDetail;
							if (!characterDetails.TryGetValue(characterId, out characterDetail))
							{
								if (characterId == CharacterVerseData.AmbiguousCharacter || characterId == CharacterVerseData.UnknownCharacter)
									continue; // This should never happen in production code!
								throw new KeyNotFoundException("No character details for unexpected character ID (see PG-471): " + characterId);
							}

							if (!configuration.AddToReservedGroupIfAppropriate(characterId))
								AddCharacterToBestGroup(characterDetail, configuration);
						}
					}
					bestConfiguration = TrialGroupConfiguration.Best(trialConfigurationsForNarratorsAndExtras, bestConfiguration);
					if (bestConfiguration.MinimumProximity >= Proximity.kDefaultMinimumProximity)
					{
						if (m_worker.CancellationPending)
						{
							EnsureActorListIsSetToRealActors(realActorsToReset);
							return GeneratedGroups = null;
						}
						if (realActorsToReset != null)
							actorsWithRealAssignments.Clear();
						GeneratedGroups = GetFinalizedGroups(bestConfiguration.Groups, actorsWithRealAssignments);
						EnsureActorListIsSetToRealActors(realActorsToReset);
						return GeneratedGroups;
					}
				}
				if (maxMaleNarrators == 0)
					maxFemaleNarrators--;
				else if (maxFemaleNarrators == 0)
					maxMaleNarrators--;
				else if (bestConfiguration != null && (bestConfiguration.GroupWithWorstProximity.ContainsCharacterWithGender(CharacterGender.Female) ||
						bestConfiguration.GroupWithWorstProximity.ContainsCharacterWithGender(CharacterGender.PreferFemale)))
					maxFemaleNarrators--;
				else if (bestConfiguration != null && (bestConfiguration.GroupWithWorstProximity.ContainsCharacterWithGender(CharacterGender.Male) ||
						bestConfiguration.GroupWithWorstProximity.ContainsCharacterWithGender(CharacterGender.PreferMale)))
					maxMaleNarrators--;
				else if (maxMaleNarrators > maxFemaleNarrators)
					maxMaleNarrators--;
				else
					maxFemaleNarrators--;
			} while (maxMaleNarrators + maxFemaleNarrators > 0);

			Debug.Assert(bestConfiguration != null);

			if (m_worker.CancellationPending)
			{
				EnsureActorListIsSetToRealActors(realActorsToReset);
				return GeneratedGroups = null;
			}

			if (realActorsToReset != null)
				actorsWithRealAssignments.Clear();
			GeneratedGroups = GetFinalizedGroups(bestConfiguration.Groups, actorsWithRealAssignments);
			EnsureActorListIsSetToRealActors(realActorsToReset);
			return GeneratedGroups;
		}

		private void EnsureActorListIsSetToRealActors(List<VoiceActor.VoiceActor> realActorsToReset)
		{
			if (realActorsToReset != null)
				m_project.VoiceActorList.AllActors = realActorsToReset;
		}

		private List<CharacterGroup> GetFinalizedGroups(List<CharacterGroup> groups, List<int> actorsWithRealAssignments)
		{
			CharacterGroupList.AssignGroupIds(groups, m_keyStrokesByCharacterId);
			foreach (var group in groups)
			{
				if (!group.AssignedToCameoActor && !actorsWithRealAssignments.Contains(group.VoiceActorId))
					group.RemoveVoiceActor();

				//TODO - we need to figure out how to actually handle locked groups from a UI perspective.
				// But for now, we need to make sure we don't throw an exception if the user manually changes it.
				group.Closed = false;
			}

			return groups.Where(g => g.AssignedToCameoActor || g.CharacterIds.Any()).ToList();
		}

		private List<CharacterGroup> CreateGroupsForActors(IEnumerable<VoiceActor.VoiceActor> actors)
		{
			List<CharacterGroup> groups = new List<CharacterGroup>();
			foreach (var voiceActor in actors)
			{
				CharacterGroup group = null;
				if (voiceActor.IsCameo)
					group = ProjectCharacterGroups.FirstOrDefault(g => g.VoiceActorId == voiceActor.Id);

				if (group == null)
				{
					group = new CharacterGroup(m_project, m_characterIdComparer);
					group.AssignVoiceActor(voiceActor.Id);
					if (voiceActor.IsCameo)
						group.Closed = true;
				}
				else
				{
					group = group.Copy();
					group.CharacterIds.IntersectWith(m_keyStrokesByCharacterId.Keys);
					group.Closed = true;
				}
				groups.Add(group);
			}
			return groups;
		}

		private IEnumerable<VoiceActor.VoiceActor> CreateGhostCastActors()
		{
			List<VoiceActor.VoiceActor> ghostCastActors = new List<VoiceActor.VoiceActor>();
			var pref = m_project.CharacterGroupGenerationPreferences;
			var castSizePlanningViewModel = new CastSizePlanningViewModel(m_project);
			var rowValues = castSizePlanningViewModel.GetCastSizeRowValues(pref.CastSizeOption);
			ghostCastActors.AddRange(CreateGhostCastActors(ActorGender.Male, ActorAge.Adult, rowValues.Male, ghostCastActors.Count));
			ghostCastActors.AddRange(CreateGhostCastActors(ActorGender.Female, ActorAge.Adult, rowValues.Female, ghostCastActors.Count));
			ghostCastActors.AddRange(CreateGhostCastActors(ActorGender.Male, ActorAge.Child, rowValues.Child, ghostCastActors.Count));
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
				!configuration.IsGroupReservedForNarratorOrExtraBiblical(g));
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
			if (!groupToProximityDict.Any(i => i.Value.WeightedNumberOfBlocks >= Proximity.kDefaultMinimumProximity || i.Value.NumberOfBlocks >= configuration.MinimumProximity) &&
				groupMatchQualityDictionary.TryGetValue(new MatchQuality(GenderMatchQuality.Perfect, AgeMatchQuality.AdultVsChild), out groups))
			{
				CalculateProximityForGroups(characterDetail, groups, groupToProximityDict);
			}
			if (!groupToProximityDict.Any(i => i.Value.WeightedNumberOfBlocks >= Proximity.kDefaultMinimumProximity || i.Value.NumberOfBlocks >= configuration.MinimumProximity) &&
				groupMatchQualityDictionary.TryGetValue(new MatchQuality(GenderMatchQuality.Acceptable, AgeMatchQuality.Perfect), out groups))
			{
				CalculateProximityForGroups(characterDetail, groups, groupToProximityDict, 1.1);
			}
			if (!groupToProximityDict.Any(i => i.Value.WeightedNumberOfBlocks >= Proximity.kDefaultMinimumProximity || i.Value.NumberOfBlocks >= configuration.MinimumProximity) &&
				groupMatchQualityDictionary.TryGetValue(new MatchQuality(GenderMatchQuality.Acceptable, AgeMatchQuality.CloseAdult), out groups))
			{
				CalculateProximityForGroups(characterDetail, groups, groupToProximityDict, 1.1);
			}
			if (!groupToProximityDict.Any(i => i.Value.WeightedNumberOfBlocks >= Proximity.kDefaultMinimumProximity || i.Value.NumberOfBlocks >= configuration.MinimumProximity) &&
				groupMatchQualityDictionary.TryGetValue(new MatchQuality(GenderMatchQuality.Acceptable, AgeMatchQuality.AdultVsChild), out groups))
			{
				CalculateProximityForGroups(characterDetail, groups, groupToProximityDict, 1.1);
			}
			if (!groupToProximityDict.Any(i => i.Value.WeightedNumberOfBlocks >= Proximity.kDefaultMinimumProximity || i.Value.NumberOfBlocks >= configuration.MinimumProximity) &&
				groupMatchQualityDictionary.TryGetValue(new MatchQuality(GenderMatchQuality.Mismatch, AgeMatchQuality.Perfect), out groups))
			{
				CalculateProximityForGroups(characterDetail, groups, groupToProximityDict, 2.3);
			}
			if (!groupToProximityDict.Any(i => i.Value.WeightedNumberOfBlocks >= Proximity.kDefaultMinimumProximity || i.Value.NumberOfBlocks >= configuration.MinimumProximity) &&
				groupMatchQualityDictionary.TryGetValue(new MatchQuality(GenderMatchQuality.Mismatch, AgeMatchQuality.CloseAdult), out groups))
			{
				CalculateProximityForGroups(characterDetail, groups, groupToProximityDict, 2.4);
			}
			if (!groupToProximityDict.Any(i => i.Value.WeightedNumberOfBlocks >= Proximity.kDefaultMinimumProximity || i.Value.NumberOfBlocks >= configuration.MinimumProximity) &&
				groupMatchQualityDictionary.TryGetValue(new MatchQuality(GenderMatchQuality.Mismatch, AgeMatchQuality.AdultVsChild), out groups))
			{
				CalculateProximityForGroups(characterDetail, groups, groupToProximityDict, 2.5);
			}
			if (!groupToProximityDict.Any(i => i.Value.WeightedNumberOfBlocks >= Proximity.kDefaultMinimumProximity || i.Value.NumberOfBlocks >= configuration.MinimumProximity) &&
				groupMatchQualityDictionary.TryGetValue(new MatchQuality(GenderMatchQuality.Perfect, AgeMatchQuality.Mismatch), out groups))
			{
				CalculateProximityForGroups(characterDetail, groups, groupToProximityDict, 2.7);
			}
			if (!groupToProximityDict.Any(i => i.Value.WeightedNumberOfBlocks >= Proximity.kDefaultMinimumProximity || i.Value.NumberOfBlocks >= configuration.MinimumProximity) &&
				groupMatchQualityDictionary.TryGetValue(new MatchQuality(GenderMatchQuality.Acceptable, AgeMatchQuality.Mismatch), out groups))
			{
				CalculateProximityForGroups(characterDetail, groups, groupToProximityDict, 2.9);
			}
			if (!groupToProximityDict.Any(i => i.Value.WeightedNumberOfBlocks >= Proximity.kDefaultMinimumProximity || i.Value.NumberOfBlocks >= configuration.MinimumProximity) &&
				groupMatchQualityDictionary.TryGetValue(new MatchQuality(GenderMatchQuality.Mismatch, AgeMatchQuality.Mismatch), out groups))
			{
				CalculateProximityForGroups(characterDetail, groups, groupToProximityDict, 3.2);
			}
			var bestGroupEntry = groupToProximityDict.Aggregate((l, r) => l.Value.WeightedNumberOfBlocks > r.Value.WeightedNumberOfBlocks ? l : r);
			var bestGroup = bestGroupEntry.Key;
			if (configuration.MinimumProximity > bestGroupEntry.Value.NumberOfBlocks)
			{
				// We're adding the character to the best group we could find, but it is now the *worst* group in the configuration.
				configuration.NoteGroupWithWorstProximity(bestGroupEntry.Key, bestGroupEntry.Value.NumberOfBlocks);
			}
			bestGroup.CharacterIds.Add(characterDetail.CharacterId);
			if (RelatedCharactersData.Singleton.GetCharacterIdsForType(CharacterRelationshipType.SameCharacterWithMultipleAges).Contains(characterDetail.CharacterId))
				foreach (var relatedCharacters in RelatedCharactersData.Singleton.GetCharacterIdToRelatedCharactersDictionary()[characterDetail.CharacterId])
					bestGroup.CharacterIds.AddRange(relatedCharacters.CharacterIds.Where(c => m_keyStrokesByCharacterId.ContainsKey(c)));
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
			private int m_groupsWithConflictingGenders;
			internal CharacterGroup GroupWithWorstProximity { get; private set; }
			internal int MinimumProximity { get; private set; }
			private List<CharacterGroup> NarratorGroups { get; set; }
			// TODO: Change to List
			private CharacterGroup ExtraBiblicalGroup { get; set; }
			internal List<CharacterGroup> Groups
			{
				get { return m_groups; }
			}

			// Note that the list of includedBooks omits any whose narrator role has already been assinged to a group.
			private TrialGroupConfiguration(IEnumerable<CharacterGroup> characterGroups,
				int numberOfMaleNarratorGroups, int numberOfFemaleNarratorGroups,
				int numberOfMaleExtraBiblicalGroups, int numberOfFemaleExtraBiblicalGroups,
				Dictionary<string, int> keyStrokesByCharacterId, Func<int, VoiceActor.VoiceActor> getVoiceActorById,
				List<CharacterDetail> includedCharacterDetails, List<string> includedBooks)
			{
				m_groups = characterGroups.Select(g => g.Copy()).ToList();
				MinimumProximity = Int32.MaxValue;

				if (Groups.Count == 1)
				{
					NarratorGroups = includedBooks.Any() ? Groups.ToList() : new List<CharacterGroup>(0);
					ExtraBiblicalGroup = Groups[0];
				}
				else
				{
					var availableAdultGroups = GetGroupsAvailableForNarratorOrExtraBiblical(Groups);
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
								var actor = getVoiceActorById(characterGroup.VoiceActorId);
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
						ExtraBiblicalGroup = NarratorGroups.First(g => getVoiceActorById(g.VoiceActorId).Gender == ActorGender.Male);
					else if (numberOfFemaleExtraBiblicalGroups == 1)
						ExtraBiblicalGroup = NarratorGroups.First(g => getVoiceActorById(g.VoiceActorId).Gender == ActorGender.Female);
				}

				AssignNarratorCharactersToNarratorGroups(keyStrokesByCharacterId, includedBooks);
				AssignDeityCharacters(includedCharacterDetails, getVoiceActorById);
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

			private void AssignDeityCharacters(List<CharacterDetail> includedCharacterDetails, Func<int, VoiceActor.VoiceActor> getVoiceActorById)
			{
				var numberOfAvailableAdultMaleActors = Groups.Count(g => !g.Closed && !g.CharacterIds.Any() &&
					g.VoiceActor.Age != ActorAge.Child && g.VoiceActor.Gender == ActorGender.Male &&
					!IsGroupReservedForNarratorOrExtraBiblical(g));

				var setsOfCharactersToGroup = DeityCharacters.LastOrDefault(kvp => kvp.Key <= numberOfAvailableAdultMaleActors).Value;
				if (setsOfCharactersToGroup == null)
					return;

				var possibleGroups = Groups.Where(g => !g.AssignedToCameoActor && !g.CharacterIds.Any() &&
					!IsGroupReservedForNarratorOrExtraBiblical(g) &&
					getVoiceActorById(g.VoiceActorId).Gender == ActorGender.Male &&
					getVoiceActorById(g.VoiceActorId).Age != ActorAge.Child).ToList();

				foreach (var characterSet in setsOfCharactersToGroup)
				{
					var bestGroup = possibleGroups.FirstOrDefault(g => getVoiceActorById(g.VoiceActorId).Age == ActorAge.Adult)
						?? possibleGroups.First();

					var charactersToPutInGroup = characterSet.Where(c => includedCharacterDetails.Any(d => d.CharacterId == c)).Except(Groups.SelectMany(g => g.CharacterIds)).ToList();
					if (charactersToPutInGroup.Any())
					{
						bestGroup.CharacterIds.AddRange(charactersToPutInGroup);
						bestGroup.Closed = true;
						possibleGroups.Remove(bestGroup);
					}
				}
			}

			private bool IsBetterThan(TrialGroupConfiguration other)
			{
				if (m_groupsWithConflictingGenders < other.m_groupsWithConflictingGenders)
					return true;
				if (m_groupsWithConflictingGenders > other.m_groupsWithConflictingGenders)
					return false;
				return MinimumProximity > other.MinimumProximity;
			}

			private static List<CharacterGroup> GetGroupsAvailableForNarratorOrExtraBiblical(IEnumerable<CharacterGroup> groups)
			{
				return groups.Where(g => !g.Closed && !g.CharacterIds.Any() && g.VoiceActor.Age != ActorAge.Child).ToList();
			}

			internal bool IsGroupReservedForNarratorOrExtraBiblical(CharacterGroup group)
			{
				return NarratorGroups.Contains(group) || ExtraBiblicalGroup == group;
			}

			internal static List<TrialGroupConfiguration> GeneratePossibilities(List<CharacterGroup> characterGroups,
				ref int numberOfMaleNarratorGroups, ref int numberOfFemaleNarratorGroups, List<CharacterDetail> includedCharacterDetails,
				Dictionary<string, int> keyStrokesByCharacterId, Project project)
			{
				var includedBooks = project.IncludedBooks.Select(b => b.BookId).ToList();
				int maleGroupsWithExistingNarratorRoles = 0;
				int femaleGroupsWithExistingNarratorRoles = 0;
				foreach (var characterGroup in characterGroups)
				{
					var narratorRoles = characterGroup.CharacterIds.Where(c =>
						CharacterVerseData.IsCharacterOfType(c, CharacterVerseData.StandardCharacter.Narrator)).ToList();

					if (narratorRoles.Any())
					{
						foreach (var narratorCharacter in narratorRoles)
							includedBooks.RemoveAt(includedBooks.IndexOf(CharacterVerseData.GetBookCodeFromStandardCharacterId(narratorCharacter)));
						if (characterGroup.VoiceActor.Gender == ActorGender.Male)
							maleGroupsWithExistingNarratorRoles++;
						else
							femaleGroupsWithExistingNarratorRoles++;
					}
				}

				var list = new List<TrialGroupConfiguration>(2);

				var availableAdultGroups = GetGroupsAvailableForNarratorOrExtraBiblical(characterGroups);
				var availableMaleGroups = availableAdultGroups.Where(g => g.VoiceActor.Gender == ActorGender.Male).ToList();
				var availableFemaleGroups = availableAdultGroups.Where(g => g.VoiceActor.Gender == ActorGender.Female).ToList();

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
					numberOfFemaleNarratorGroups = Math.Min(numberOfFemaleNarratorGroups, availableFemaleGroups.Count + femaleGroupsWithExistingNarratorRoles);
					if (numberOfMaleNarratorGroups + numberOfFemaleNarratorGroups == 0)
						numberOfMaleNarratorGroups = 1;
					numberOfUnassignedMaleNarrators = numberOfMaleNarratorGroups - maleGroupsWithExistingNarratorRoles;
					numberOfUnassignedFemaleNarrators = numberOfFemaleNarratorGroups - femaleGroupsWithExistingNarratorRoles;
					if (numberOfUnassignedMaleNarrators + numberOfUnassignedFemaleNarrators == 0)
						numberOfUnassignedMaleNarrators = 1;
				}

				var numberOfExtraBiblicalGroups = (numberOfMaleNarratorGroups + numberOfFemaleNarratorGroups) == 0 ? 0 : Math.Min(1, includedCharacterDetails.Count(c => CharacterVerseData.IsCharacterStandard(c.CharacterId, false)));

				Func<int, VoiceActor.VoiceActor> getVoiceActorById = id => project.VoiceActorList.GetVoiceActorById(id);

				if (availableMaleGroups.Count >= Math.Max(Math.Min(1, numberOfExtraBiblicalGroups), numberOfUnassignedMaleNarrators))
				{
					list.Add(new TrialGroupConfiguration(characterGroups, numberOfUnassignedMaleNarrators, numberOfUnassignedFemaleNarrators,
						Math.Min(1, numberOfExtraBiblicalGroups), 0, keyStrokesByCharacterId, getVoiceActorById, includedCharacterDetails, includedBooks));
				}

				if (numberOfExtraBiblicalGroups > 0 && availableFemaleGroups.Count >= Math.Max(1, numberOfUnassignedFemaleNarrators))
				{
					list.Add(new TrialGroupConfiguration(characterGroups, numberOfUnassignedMaleNarrators, numberOfUnassignedFemaleNarrators,
						0, numberOfExtraBiblicalGroups, keyStrokesByCharacterId, getVoiceActorById, includedCharacterDetails, includedBooks));
				}

				if (!list.Any())
				{
					list.Add(new TrialGroupConfiguration(characterGroups, numberOfUnassignedMaleNarrators, numberOfUnassignedFemaleNarrators,
						numberOfExtraBiblicalGroups, 0, keyStrokesByCharacterId, getVoiceActorById, includedCharacterDetails, includedBooks));
					numberOfMaleNarratorGroups = list[0].NarratorGroups.Count(g => g.VoiceActor.Gender == ActorGender.Male) + maleGroupsWithExistingNarratorRoles;
					numberOfFemaleNarratorGroups = list[0].NarratorGroups.Count(g => g.VoiceActor.Gender == ActorGender.Female) + femaleGroupsWithExistingNarratorRoles;
					Debug.Assert(numberOfMaleNarratorGroups + numberOfFemaleNarratorGroups == 1 || !includedBooks.Any());
				}
				return list;
			}

			internal static TrialGroupConfiguration Best(List<TrialGroupConfiguration> configurations, TrialGroupConfiguration previousBest)
			{
				foreach (var configuration in configurations)
					configuration.CalculateConflicts();

				var best = previousBest ?? configurations.First();
				foreach (var config in configurations.Skip(previousBest == null ? 1 : 0).Where(config => config.IsBetterThan(best)))
					best = config;
				return best;
			}

			private void CalculateConflicts()
			{
				foreach (var group in Groups)
				{
					if ((group.ContainsCharacterWithGender(CharacterGender.Female) || group.ContainsCharacterWithGender(CharacterGender.PreferFemale)) &&
						(group.ContainsCharacterWithGender(CharacterGender.Male) || group.ContainsCharacterWithGender(CharacterGender.PreferMale)))
					{
						m_groupsWithConflictingGenders++;
					}
				}
			}

			internal void NoteGroupWithWorstProximity(CharacterGroup worstGroup, int numberOfBlocks)
			{
				GroupWithWorstProximity = worstGroup;
				MinimumProximity = numberOfBlocks;
			}

			internal bool AddToReservedGroupIfAppropriate(string characterId)
			{
				if (CharacterVerseData.IsCharacterOfType(characterId, CharacterVerseData.StandardCharacter.Narrator))
				{
					throw new ArgumentException("Attempted to include narrator character for book that is not included in project");
				}
				if (CharacterVerseData.IsCharacterStandard(characterId, false))
				{
					if (ExtraBiblicalGroup != null)
					{
						ExtraBiblicalGroup.CharacterIds.Add(characterId);
						return true;
					}
				}
				return false;
			}
		}
	}
}
