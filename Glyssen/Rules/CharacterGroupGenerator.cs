using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Glyssen.Character;
using Glyssen.VoiceActor;
using SIL.Extensions;

namespace Glyssen.Rules
{
	public class CharacterGroupGenerator
	{
		private readonly Project m_project;
		private readonly bool m_attemptToPreserveActorAssignments;
		private readonly Dictionary<string, int> m_keyStrokesByCharacterId;
		private readonly IComparer<string> m_characterIdComparer;
		private readonly Proximity m_proximity;

		private static readonly SortedDictionary<int, IList<HashSet<string>>> DeityCharacters;

		private IList<CharacterGroup> CharacterGroups
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

		public CharacterGroupGenerator(Project project, Dictionary<string, int> keyStrokesByCharacterId, bool attemptToPreserveActorAssignments = true)
		{
			m_project = project;
			m_attemptToPreserveActorAssignments = attemptToPreserveActorAssignments;
			m_keyStrokesByCharacterId = new Dictionary<string, int>(keyStrokesByCharacterId);
			m_proximity = new Proximity(project);
			m_characterIdComparer = new CharacterByKeyStrokeComparer(m_keyStrokesByCharacterId);
		}

		public void UpdateProjectCharacterGroups()
		{
			// Create a copy. Cameos are handled in the generation code (because we always maintain those assignments).
			List<CharacterGroup> previousGroups = m_attemptToPreserveActorAssignments ?
				CharacterGroups.Where(g => !g.AssignedToCameoActor).ToList() : new List<CharacterGroup>();

			var newGroups = GenerateCharacterGroups();
			CharacterGroups.Clear();
			CharacterGroups.AddRange(newGroups);

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
						var newlyGeneratedGroupWithCharacter = CharacterGroups.FirstOrDefault(g => !g.IsVoiceActorAssigned && g.CharacterIds.Contains(characterId));
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

		internal List<CharacterGroup> GenerateCharacterGroups()
		{
			List<CharacterGroup> characterGroups = CreateGroupsForActors(m_project.VoiceActorList.Actors).ToList();

			if (characterGroups.Count == 0)
				return characterGroups; // REVIEW: Maybe we should throw an exception instead.

			//if (m_project.CharacterGroupGenerationPreferences.IsSetByUser)
			m_project.SetDefaultCharacterGroupGenerationPreferences();

			List<VoiceActor.VoiceActor> nonCameoActors = m_project.VoiceActorList.Actors.Where(a => !a.IsCameo).ToList();

			if (nonCameoActors.Count == 0)
				return characterGroups; // All cameo actors! This should never happen.

			// This is OK, but unnecessarily complicates the code. Existing logic in main loop handles it.
			//List<string> excludedCharacterIds = CharacterGroups.Where(g => g.AssignedToCameoActor).SelectMany(g => g.CharacterIds).ToList();
			//var sortedDict = from entry in m_keyStrokesByCharacterId where !excludedCharacterIds.Contains(entry.Key) orderby entry.Value descending select entry;

			var sortedDict = from entry in m_keyStrokesByCharacterId orderby entry.Value descending select entry;

//			List<VoiceActor.VoiceActor> actorsNeedingGroups = new List<VoiceActor.VoiceActor>();

			IReadOnlyDictionary<string, CharacterDetail> characterDetails = m_project.AllCharacterDetailDictionary;
			var includedCharacterDetails = characterDetails.Values.Where(c => sortedDict.Select(e => e.Key).Contains(c.CharacterId)).ToList();

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
					//else
					//	actorsNeedingGroups.Add(actor);
				}
			}

			// These loop uses the results of the previous one to add the characters to a group, and to close that
			// group to further additions. If there's more than one candidate actor, we pick one arbitrarily,
			// since afterwards we'll be clearing actor names anyway.
			// TODO: later on, we'll need to prevent clearing the actor for the group, if this was the only
			// possible actor for the character. Since all groups now have an actor assigned up-front, at the
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

			// TODO: Make sure we didn't close all the groups

			//var predeterminedActorGroups = new Dictionary<VoiceActor.VoiceActor, CharacterGroup>();
			//// Because it is almost always the case (with perhaps the exception of Ruth and Song of Solomon) that male speaking
			//// parts will dominate the script, we want to ensure that at least one group is reserved for female characters if
			//// we have any female voice talent. Otherwise, characters with a gender of "Either" could end up being assigned to
			//// all the group(s) that would otherwise be available for female characters. Once a group has an "Either" character
			//// in it, male characters can be added to it, after which a female character would no longer be considered a match.
			//bool femaleGroupReserved = false;
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
				//else if (!femaleGroupReserved && nbrFemaleAdultActors > 0 && character.Gender == CharacterGender.Female)
				//{
				//	var femalegroup = new CharacterGroup(m_project, m_characterIdComparer);
				//	characterGroups.Add(femalegroup);
				//	femalegroup.CharacterIds.Add(character.CharacterId);
				//	femaleGroupReserved = true;
				//}
			}

			int maxMaleNarrators = m_project.CharacterGroupGenerationPreferences.NumberOfMaleNarrators;
			int maxFemaleNarrators = m_project.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators;

			TrialGroupConfiguration bestConfiguration = null;
			//int numberOfActorsForWhichNoGroupHasBeenCreated = actorsNeedingGroups.Count;
			//if (femaleGroupReserved)
			//	numberOfActorsForWhichNoGroupHasBeenCreated--;
			do
			{
				var trialConfigurationsForNarratorsAndExtras = TrialGroupConfiguration.GeneratePossibilities(characterGroups,
					ref maxMaleNarrators, ref maxFemaleNarrators,
					Math.Min(1, includedCharacterDetails.Count(c => CharacterVerseData.IsCharacterStandard(c.CharacterId, false))),
					m_project);

				if (trialConfigurationsForNarratorsAndExtras.Any())
				{
					foreach (var configuration in trialConfigurationsForNarratorsAndExtras)
					{
						AssignDeityCharacters(includedCharacterDetails, configuration);

						foreach (var entry in sortedDict)
						{
							string characterId = entry.Key;

							if (configuration.m_groups.Any(g => g.CharacterIds.Contains(characterId)))
								continue;

							CharacterDetail characterDetail;
							if (!characterDetails.TryGetValue(characterId, out characterDetail))
							{
								if (characterId == CharacterVerseData.AmbiguousCharacter || characterId == CharacterVerseData.UnknownCharacter)
									continue; // This should never happen in production code!
								//Debug.WriteLine("No character details for unexpected character ID (see PG-): " + characterId);
								//continue;
								throw new KeyNotFoundException("No character details for unexpected character ID (see PG-471): " + characterId);
							}

							if (CharacterVerseData.IsCharacterOfType(characterId, CharacterVerseData.StandardCharacter.Narrator))
							{
								// Need tests (and code here) to handle the following scenarios:
								// 1) number of narrators >= number of books -> Add narrator to first empty narrator group
								// 2) number of narrators > number of authors -> break up most prolific authors into multiple narrator groups
								// 3) number of narrators == number of authors -> add narrator to group with other books by same other, if any; otherwise first empty group
								// 4) number of narrators < number of authors -> shorter books share narrators
								// 5) single narrator -> EASY: only one group!

								CharacterGroup bestNarratorGroup = configuration.NarratorGroups.FirstOrDefault(g => !g.CharacterIds.Any());
								if (bestNarratorGroup == null)
									bestNarratorGroup = configuration.NarratorGroups.First();
								bestNarratorGroup.CharacterIds.Add(characterId);
								continue;
							}
							else if (CharacterVerseData.IsCharacterStandard(characterId, false))
							{
								if (configuration.ExtraBiblicalGroup != null)
								{
									configuration.ExtraBiblicalGroup.CharacterIds.Add(characterId);
									continue;
								}
							}

							AddCharacterToBestGroup(configuration.m_groups, characterDetail, configuration);
						}
					}
					bestConfiguration = TrialGroupConfiguration.Best(trialConfigurationsForNarratorsAndExtras, bestConfiguration);
					if (bestConfiguration.MinimumProximity >= Proximity.kDefaultMinimumProximity)
					   // && bestConfiguration.m_groups.All(g => g.CharacterIds.Any()))
					{
						return GetFinalizedGroups(bestConfiguration.m_groups, actorsWithRealAssignments);
					}
				}
				if (maxMaleNarrators == 0)
					maxFemaleNarrators--;
				else if (maxFemaleNarrators == 0)
					maxMaleNarrators--;
				else if (bestConfiguration.m_groupWithWorstProximity.ContainsCharacterWithGender(CharacterGender.Female) ||
						bestConfiguration.m_groupWithWorstProximity.ContainsCharacterWithGender(CharacterGender.PreferFemale))
					maxFemaleNarrators--;
				else if (bestConfiguration.m_groupWithWorstProximity.ContainsCharacterWithGender(CharacterGender.Male) ||
						bestConfiguration.m_groupWithWorstProximity.ContainsCharacterWithGender(CharacterGender.PreferMale))
					maxMaleNarrators--;
				else if (maxMaleNarrators > maxFemaleNarrators)
					maxMaleNarrators--;
				else
					maxFemaleNarrators--;
			} while (maxMaleNarrators + maxFemaleNarrators > 0);

			return GetFinalizedGroups(bestConfiguration.m_groups, actorsWithRealAssignments);
		}

		private List<CharacterGroup> GetFinalizedGroups(List<CharacterGroup> groups, List<int> actorsWithRealAssignments)
		{

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

		private IEnumerable<CharacterGroup> CreateGroupsForActors(IEnumerable<VoiceActor.VoiceActor> actors)
		{
			foreach (var voiceActor in actors)
			{
				CharacterGroup group = null;
				if (voiceActor.IsCameo)
					group = CharacterGroups.FirstOrDefault(g => g.VoiceActorId == voiceActor.Id);

				if (group == null)
				{
					group = new CharacterGroup(m_project, m_characterIdComparer);
					group.AssignVoiceActor(voiceActor.Id);
					if (voiceActor.IsCameo)
						group.Closed = true;
				}
				else
				{
					group.CharacterIds.IntersectWith(m_keyStrokesByCharacterId.Keys);
					group.Closed = true;
				}
				yield return group;
			}
		}

		//private IEnumerable<CharacterGroup> CreateGroupsForCameoActors()
		//{
		//	foreach (var cameoActor in m_project.VoiceActorList.Actors.Where(a => a.IsCameo))
		//	{
		//		if (m_project.CharacterGroupList.HasVoiceActorAssigned(cameoActor.Id))
		//		{
		//			var groupForCameoActor = CharacterGroups.First(g => g.VoiceActorId == cameoActor.Id);
		//			groupForCameoActor.CharacterIds.IntersectWith(m_keyStrokesByCharacterId.Keys);
		//			yield return groupForCameoActor;
		//		}
		//		else
		//		{
		//			var newGroup = new CharacterGroup(m_project, m_characterIdComparer);
		//			newGroup.AssignVoiceActor(cameoActor.Id);
		//			yield return newGroup;
		//		}
		//	}
		//}

		private void AssignDeityCharacters(List<CharacterDetail> includedCharacterDetails, TrialGroupConfiguration configuration)
		{
			var numberOfAvailableAdultMaleActors = configuration.m_groups.Count(g => !g.Closed && !g.CharacterIds.Any() &&
				g.VoiceActor.Age != ActorAge.Child && g.VoiceActor.Gender == ActorGender.Male &&
				!configuration.NarratorGroups.Contains(g) && !configuration.ExtraBiblicalGroups.Contains(g));

			var setsOfCharactersToGroup = DeityCharacters.LastOrDefault(kvp => kvp.Key <= numberOfAvailableAdultMaleActors).Value;
			if (setsOfCharactersToGroup == null)
				return;

			var possibleGroups = configuration.m_groups.Where(g => !g.AssignedToCameoActor && !g.CharacterIds.Any() &&
				!configuration.NarratorGroups.Contains(g) && !configuration.ExtraBiblicalGroups.Contains(g) &&
				m_project.VoiceActorList.GetVoiceActorById(g.VoiceActorId).Gender == ActorGender.Male &&
				m_project.VoiceActorList.GetVoiceActorById(g.VoiceActorId).Age != ActorAge.Child).ToList();

			foreach (var characterSet in setsOfCharactersToGroup)
			{
				var bestGroup = possibleGroups.FirstOrDefault(g => m_project.VoiceActorList.GetVoiceActorById(g.VoiceActorId).Age == ActorAge.Adult)
					?? possibleGroups.First();

				var charactersToPutInGroup = characterSet.Where(c => includedCharacterDetails.Any(d => d.CharacterId == c)).Except(configuration.m_groups.SelectMany(g => g.CharacterIds)).ToList();
				if (charactersToPutInGroup.Any())
				{
					bestGroup.CharacterIds.AddRange(charactersToPutInGroup);
					bestGroup.Closed = true;
					possibleGroups.Remove(bestGroup);
				}
			}
		}

		private void AddCharacterToBestGroup(List<CharacterGroup> characterGroups, CharacterDetail characterDetail, TrialGroupConfiguration configuration)
		{
			List<CharacterGroup> groups;

			IEnumerable<CharacterGroup> availableGroups = characterGroups.Where(g => !g.Closed &&
				!configuration.NarratorGroups.Contains(g) && !configuration.ExtraBiblicalGroups.Contains(g));
			if (!availableGroups.Any())
				availableGroups = characterGroups.Where(g => !g.Closed);

			var groupMatchQualityDictionary = new Dictionary<MatchQuality, List<CharacterGroup>>(characterGroups.Count);
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
					bestGroup.CharacterIds.AddRange(relatedCharacters.CharacterIds);
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

		private class TrialGroupConfiguration
		{
			internal readonly List<CharacterGroup> m_groups;
			private int m_groupsWithConflictingGenders;
			internal CharacterGroup m_groupWithWorstProximity;
			internal int MinimumProximity { get; private set; }
			internal List<CharacterGroup> NarratorGroups { get; private set; }
			// TODO: Change to List
			internal CharacterGroup ExtraBiblicalGroup { get; private set; }
			internal List<CharacterGroup> ExtraBiblicalGroups
			{
				get { return new List<CharacterGroup> { ExtraBiblicalGroup }; }
			}

			private TrialGroupConfiguration(IEnumerable<CharacterGroup> characterGroups,
				int numberOfMaleNarratorGroups, int numberOfFemaleNarratorGroups,
				int numberOfMaleExtraBiblicalGroups, int numberOfFemaleExtraBiblicalGroups,
				Project project)
			{
				m_groups = characterGroups.Select(g => g.Copy()).ToList();
				MinimumProximity = Int32.MaxValue;

				if (m_groups.Count == 1)
				{
					NarratorGroups = m_groups.ToList();
					ExtraBiblicalGroup = m_groups[0];
				}
				else
				{
					var availableAdultGroups = GetAvailableGroups(m_groups);
					if (availableAdultGroups.Count == 1)
					{
						NarratorGroups = availableAdultGroups.ToList();
						ExtraBiblicalGroup = NarratorGroups[0];
					}
					Debug.WriteLine("Male narrators desired = " + numberOfMaleNarratorGroups);
					Debug.WriteLine("Female narrators desired = " + numberOfFemaleNarratorGroups);
					NarratorGroups = new List<CharacterGroup>(numberOfMaleNarratorGroups + numberOfFemaleNarratorGroups);
					var idealAge = new List<ActorAge> { ActorAge.Adult };
					int attempt = 0;
					while (attempt++ < 2 && (numberOfMaleNarratorGroups > 0 || numberOfFemaleNarratorGroups > 0 ||
						numberOfMaleExtraBiblicalGroups > 0 || numberOfFemaleExtraBiblicalGroups > 0))
					{
						// ENHANCE: Simplify this by removing Union and handling as two separate passes: one for male and one for female.
						foreach (var characterGroup in availableAdultGroups)
						{
							var actor = project.VoiceActorList.GetVoiceActorById(characterGroup.VoiceActorId);
							if (!idealAge.Contains(actor.Age))
								continue;

							Debug.WriteLine("Actor gender = " + actor.Gender);

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
						idealAge = new List<ActorAge> { ActorAge.Elder, ActorAge.YoungAdult };
					}
					if (numberOfMaleExtraBiblicalGroups == 1)
					{
						// TODO: Handle multiple extra-biblical groups
						ExtraBiblicalGroup = NarratorGroups.First(g => project.VoiceActorList.GetVoiceActorById(g.VoiceActorId).Gender == ActorGender.Male);
					}
					else if (numberOfFemaleExtraBiblicalGroups == 1)
						ExtraBiblicalGroup = NarratorGroups.First(g => project.VoiceActorList.GetVoiceActorById(g.VoiceActorId).Gender == ActorGender.Female);

					if (!NarratorGroups.Any())
					{
						foreach (var characterGroup in m_groups)
						{
							Debug.WriteLine("Cameo = " + characterGroup.AssignedToCameoActor);
							Debug.WriteLine("CharacterIds = " + characterGroup.CharacterIds.ToString());
						}
						throw new Exception("None of the " + m_groups.Count + " groups were suitable for narrator role.");
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

			private static List<CharacterGroup> GetAvailableGroups(IEnumerable<CharacterGroup> groups)
			{
				return groups.Where(g => !g.Closed && !g.CharacterIds.Any() && g.VoiceActor.Age != ActorAge.Child).ToList();
			}

			internal static List<TrialGroupConfiguration> GeneratePossibilities(List<CharacterGroup> characterGroups,
				ref int numberOfMaleNarratorGroups, ref int numberOfFemaleNarratorGroups, int numberOfExtraBiblicalGroups, Project project)
			{
				var list = new List<TrialGroupConfiguration>(2);

				var availableAdultGroups = GetAvailableGroups(characterGroups);
				var availableMaleGroups = availableAdultGroups.Where(g => g.VoiceActor.Gender == ActorGender.Male).ToList();
				var availableFemaleGroups = availableAdultGroups.Where(g => g.VoiceActor.Gender == ActorGender.Female).ToList();

				numberOfMaleNarratorGroups = Math.Min(numberOfMaleNarratorGroups, availableMaleGroups.Count);
				numberOfFemaleNarratorGroups = Math.Min(numberOfFemaleNarratorGroups, availableFemaleGroups.Count);
				if (numberOfMaleNarratorGroups + numberOfFemaleNarratorGroups == 0)
					numberOfMaleNarratorGroups = 1;

				if (availableMaleGroups.Count >= Math.Max(Math.Min(1, numberOfExtraBiblicalGroups), numberOfMaleNarratorGroups))
				{
					list.Add(new TrialGroupConfiguration(characterGroups, numberOfMaleNarratorGroups, numberOfFemaleNarratorGroups,
						Math.Min(1, numberOfExtraBiblicalGroups), 0, project));
				}

				if (numberOfExtraBiblicalGroups > 0 && availableFemaleGroups.Count >= Math.Max(1, numberOfFemaleNarratorGroups))
				{
					list.Add(new TrialGroupConfiguration(characterGroups, numberOfMaleNarratorGroups, numberOfFemaleNarratorGroups,
						0, numberOfExtraBiblicalGroups, project));
				}

				if (!list.Any())
				{
					list.Add(new TrialGroupConfiguration(characterGroups, numberOfMaleNarratorGroups, numberOfFemaleNarratorGroups,
						numberOfExtraBiblicalGroups, 0, project));
					numberOfMaleNarratorGroups = list[0].NarratorGroups.Count(g => g.VoiceActor.Gender == ActorGender.Male);
					numberOfFemaleNarratorGroups = list[0].NarratorGroups.Count(g => g.VoiceActor.Gender == ActorGender.Female);
					Debug.Assert(numberOfMaleNarratorGroups + numberOfFemaleNarratorGroups == 1);
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
				foreach (var group in m_groups)
				{
					if ((group.ContainsCharacterWithGender(CharacterGender.Female) || group.ContainsCharacterWithGender(CharacterGender.PreferFemale)) &&
						(group.ContainsCharacterWithGender(CharacterGender.Male) || group.ContainsCharacterWithGender(CharacterGender.PreferMale)))
					{
						m_groupsWithConflictingGenders++;
					}
				}
			}

			public void NoteGroupWithWorstProximity(CharacterGroup worstGroup, int numberOfBlocks)
			{
				m_groupWithWorstProximity = worstGroup;
				MinimumProximity = numberOfBlocks;
			}
		}
	}
}
