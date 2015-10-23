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

		private static readonly SortedDictionary<int, IList<HashSet<string>>> CharactersInClosedGroups;

		private IList<CharacterGroup> CharacterGroups { get { return m_project.CharacterGroupList.CharacterGroups; } }

		static CharacterGroupGenerator()
		{
			CharactersInClosedGroups = new SortedDictionary<int, IList<HashSet<string>>>();
			CharactersInClosedGroups.Add(4, new List<HashSet<string>> { new HashSet<string> { "Jesus", "God", "Holy Spirit, the", "scripture" } });
			var jesusSet = new HashSet<string> { "Jesus" };
			CharactersInClosedGroups.Add(7, new List<HashSet<string>> { jesusSet, new HashSet<string> { "God", "Holy Spirit, the", "scripture" } });
			var holySpiritSet = new HashSet<string> { "Holy Spirit, the" };
			CharactersInClosedGroups.Add(10, new List<HashSet<string>> { jesusSet, new HashSet<string> { "God", "scripture" }, holySpiritSet });
			CharactersInClosedGroups.Add(20, new List<HashSet<string>> { jesusSet, new HashSet<string> { "God" }, holySpiritSet, new HashSet<string> { "scripture" } });
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
			List<CharacterGroup> characterGroups = new List<CharacterGroup>();

			List<VoiceActor.VoiceActor> actors = m_project.VoiceActorList.Actors;

			if (actors.Count == 0)
				return characterGroups; // REVIEW: Maybe we should throw an exception instead.

			actors = actors.Where(a => !a.IsCameo).ToList();

			if (actors.Count == 0)
				return CreateGroupsForCameoActors().ToList();

			IEnumerable<string> excludedCharacterIds = GetCharacterIdsAssignedToCameoActor();

			var sortedDict = from entry in m_keyStrokesByCharacterId where !excludedCharacterIds.Contains(entry.Key) orderby entry.Value descending select entry;

			List<VoiceActor.VoiceActor> actorsNeedingGroups = new List<VoiceActor.VoiceActor>();

			IReadOnlyDictionary<string, CharacterDetail> characterDetails = m_project.AllCharacterDetailDictionary;
			var includedCharacterDetails = characterDetails.Values.Where(c => sortedDict.Select(e => e.Key).Contains(c.CharacterId)).ToList();

			var characterDetailsUniquelyMatchedToActors = new Dictionary<CharacterDetail, List<VoiceActor.VoiceActor>>();
			foreach (var actor in actors)
			{
				// After we find the second match, we can quit looking because we're only interested in unique matches.
				var matches = includedCharacterDetails.Where(c => actor.Matches(c)).Take(2).ToList();
				if (matches.Any())
				{
					if (matches.Count == 1)
					{
						var characterDetail = matches.First();
						if (characterDetailsUniquelyMatchedToActors.ContainsKey(characterDetail))
							characterDetailsUniquelyMatchedToActors[characterDetail].Add(actor);
						else
							characterDetailsUniquelyMatchedToActors[characterDetail] = new List<VoiceActor.VoiceActor>{ actor };
					}
					else
						actorsNeedingGroups.Add(actor);
				}
			}

			foreach (var characterDetailToActors in characterDetailsUniquelyMatchedToActors)
			{
				var character = characterDetailToActors.Key;
				var matchingActors = characterDetailToActors.Value;

				var group = new CharacterGroup(m_project, m_characterIdComparer);
				group.CharacterIds.Add(character.CharacterId);
				characterGroups.Add(group);
				group.Closed = true;

				if (matchingActors.Count == 1)
					group.AssignVoiceActor(matchingActors.Single().Id);
			}

			var predeterminedActorGroups = new Dictionary<VoiceActor.VoiceActor, CharacterGroup>();
			foreach (var character in includedCharacterDetails)
			{
				var matchingActors = actorsNeedingGroups.Where(a => a.Matches(character));
				if (matchingActors.Count() == 1)
				{
					var matchingActor = matchingActors.First();
					CharacterGroup groupForActor;
					if (!predeterminedActorGroups.TryGetValue(matchingActor, out groupForActor))
					{
						groupForActor = new CharacterGroup(m_project, m_characterIdComparer);
						characterGroups.Add(groupForActor);
						predeterminedActorGroups[matchingActor] = groupForActor;
						groupForActor.AssignVoiceActor(matchingActor.Id);
						actorsNeedingGroups.Remove(matchingActor);
					}
					groupForActor.CharacterIds.Add(character.CharacterId);
				}
			}

			var nbrMaleAdultActors = actors.Count(a => a.Gender == ActorGender.Male && a.Age != ActorAge.Child);
			var trialConfigurationsForNarratorsAndExtras = TrialGroupConfiguration.GeneratePossibilities(characterGroups, actorsNeedingGroups.Count,
				nbrMaleAdultActors,
				actors.Count(a => a.Gender == ActorGender.Female && a.Age != ActorAge.Child),
				Math.Min(1, includedCharacterDetails.Count(c => CharacterVerseData.IsCharacterOfType(c.CharacterId, CharacterVerseData.StandardCharacter.Narrator))),
				Math.Min(1, includedCharacterDetails.Count(c => CharacterVerseData.IsCharacterStandard(c.CharacterId, false))),
				includedCharacterDetails.Where(c => CharacterVerseData.IsCharacterOfType(c.CharacterId, CharacterVerseData.StandardCharacter.Narrator)).Select(c => CharacterVerseData.GetBookNameFromStandardCharacterId(c.CharacterId)).FirstOrDefault(),
				includedCharacterDetails.Where(c => CharacterVerseData.IsCharacterOfType(c.CharacterId, CharacterVerseData.StandardCharacter.BookOrChapter)).Select(c => CharacterVerseData.GetBookNameFromStandardCharacterId(c.CharacterId)).FirstOrDefault(),
				m_project,
				m_characterIdComparer);

			foreach (var configuration in trialConfigurationsForNarratorsAndExtras)
			{
				characterGroups = configuration.m_groups;

				characterGroups.AddRange(CreateGroupsForReservedCharacters(includedCharacterDetails, nbrMaleAdultActors, configuration));

				foreach (var entry in sortedDict)
				{
					string characterId = entry.Key;

					if (characterGroups.Any(g => g.CharacterIds.Contains(characterId)))
						continue;

					CharacterDetail characterDetail;
					if (!characterDetails.TryGetValue(characterId, out characterDetail))
					{
						// This should never happen in production code!
						continue;
					}

					if (CharacterVerseData.IsCharacterOfType(characterId, CharacterVerseData.StandardCharacter.Narrator))
					{
						if (configuration.NarratorGroup != null)
						{
							configuration.NarratorGroup.CharacterIds.Add(characterId);
							continue;
						}
					}
					else if (CharacterVerseData.IsCharacterStandard(characterId, false))
					{
						if (configuration.ExtraBiblicalGroup != null)
						{
							configuration.ExtraBiblicalGroup.CharacterIds.Add(characterId);
							continue;
						}
					}

					int numMatchingCharacterGroups = characterGroups.Count(g => g.Matches(characterDetail, CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.LooseExceptChild));
					numMatchingCharacterGroups += configuration.m_narratorsOrExtraActors.Count(a => a.Matches(characterDetail));
					int numMatchingActors = actorsNeedingGroups.Count(a => a.Matches(characterDetail));
					if (configuration.RemainingUsableActors > 0 &&
						(numMatchingActors == 0 || numMatchingCharacterGroups < numMatchingActors))
					{
						var group = new CharacterGroup(m_project, m_characterIdComparer);
						group.CharacterIds.Add(characterId);
						characterGroups.Add(group);
						configuration.RemainingUsableActors--;
					}
					else
						AddCharacterToBestGroup(characterGroups, characterDetail, configuration);
				}
			}
			var groups = TrialGroupConfiguration.Best(trialConfigurationsForNarratorsAndExtras).m_groups;

			//TODO - we need to figure out how to actually handle locked groups from a UI perspective.
			// But for now, we need to make sure we don't throw an exception if the user manually changes it.
			foreach (var group in groups)
				group.Closed = false;

			groups.AddRange(CreateGroupsForCameoActors());

			return groups;
		}

		private IEnumerable<string> GetCharacterIdsAssignedToCameoActor()
		{
			var characterIds = new List<string>();
			foreach (var cameoGroup in CharacterGroups.Where(g => g.AssignedToCameoActor))
				characterIds.AddRange(cameoGroup.CharacterIds);
			return characterIds;
		}

		private IEnumerable<CharacterGroup> CreateGroupsForCameoActors()
		{
			foreach (var cameoActor in m_project.VoiceActorList.Actors.Where(a => a.IsCameo))
			{
				if (m_project.CharacterGroupList.HasVoiceActorAssigned(cameoActor.Id))
				{
					var groupForCameoActor = CharacterGroups.First(g => g.VoiceActorId == cameoActor.Id);
					groupForCameoActor.CharacterIds.IntersectWith(m_keyStrokesByCharacterId.Keys);
					yield return groupForCameoActor;
				}
				else
				{
					var newGroup = new CharacterGroup(m_project, m_characterIdComparer);
					newGroup.AssignVoiceActor(cameoActor.Id);
					yield return newGroup;
				}
			}
		}

		private IEnumerable<CharacterGroup> CreateGroupsForReservedCharacters(List<CharacterDetail> includedCharacterDetails, int nbrMaleAdultActors,
			TrialGroupConfiguration configuration)
		{
			if (CharactersInClosedGroups.Any(kvp => kvp.Key <= nbrMaleAdultActors))
			{
				var setsOfCharactersToGroup = CharactersInClosedGroups.LastOrDefault(kvp => kvp.Key <= nbrMaleAdultActors).Value;
				if (setsOfCharactersToGroup == null)
					setsOfCharactersToGroup = CharactersInClosedGroups.Last().Value;

				foreach (var characterSet in setsOfCharactersToGroup)
				{
					if (configuration.RemainingUsableActors == 0)
						break;

					var charactersToPutInGroup = characterSet.Where(c => includedCharacterDetails.Any(d => d.CharacterId == c)).ToList();

					if (charactersToPutInGroup.Any())
					{
						configuration.RemainingUsableActors--;
						var group = new CharacterGroup(m_project, m_characterIdComparer);
						group.CharacterIds.AddRange(charactersToPutInGroup);
						group.Closed = true;
						yield return group;
					}
				}
			}
		}

		private void AddCharacterToBestGroup(List<CharacterGroup> characterGroups, CharacterDetail characterDetail, TrialGroupConfiguration configuration)
		{
			var groupToProximityDict = new Dictionary<CharacterGroup, MinimumProximity>();

			CalculateProximityForMatchingGroups(characterDetail, characterGroups, g => g.Matches(characterDetail, CharacterGenderMatchingOptions.Moderate, CharacterAgeMatchingOptions.Strict), groupToProximityDict);

			if (!groupToProximityDict.Any(i => i.Value.NumberOfBlocks >= Proximity.kDefaultMinimumProximity))
			{
				CalculateProximityForMatchingGroups(characterDetail, characterGroups, g => g.Matches(characterDetail, CharacterGenderMatchingOptions.Moderate, CharacterAgeMatchingOptions.LooseExceptChild), groupToProximityDict);
			}
			if (!groupToProximityDict.Any(i => i.Value.NumberOfBlocks >= Proximity.kDefaultMinimumProximity))
			{
				CalculateProximityForMatchingGroups(characterDetail, characterGroups, g => g.Matches(characterDetail, CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.LooseExceptChild), groupToProximityDict);
			}
			if (!groupToProximityDict.Any())
			{
				CalculateProximityForMatchingGroups(characterDetail, characterGroups, g => true, groupToProximityDict);
			}
			var bestGroupEntry = groupToProximityDict.Aggregate((l, r) => l.Value.NumberOfBlocks > r.Value.NumberOfBlocks ? l : r);
			var bestGroup = bestGroupEntry.Key;
			if (configuration.m_minimumProximity > bestGroupEntry.Value.NumberOfBlocks)
				configuration.m_minimumProximity = bestGroupEntry.Value.NumberOfBlocks;
			bestGroup.CharacterIds.Add(characterDetail.CharacterId);
		}

		private void CalculateProximityForMatchingGroups(CharacterDetail characterDetail, IEnumerable<CharacterGroup> characterGroups, Func<CharacterGroup, bool> matchCriteria, Dictionary<CharacterGroup, MinimumProximity> groupToProximityDict)
		{
			var matchingGroups = characterGroups.Where(g => !g.Closed && matchCriteria(g));

			foreach (var group in matchingGroups)
			{
				if (groupToProximityDict.ContainsKey(group))
					continue;

				HashSet<string> testSet = new HashSet<string>(group.CharacterIds);
				testSet.Add(characterDetail.CharacterId);

				MinimumProximity minProx = m_proximity.CalculateMinimumProximity(testSet);
				groupToProximityDict.Add(group, minProx);
			}
		}

		private class TrialGroupConfiguration
		{
			internal readonly List<VoiceActor.VoiceActor> m_narratorsOrExtraActors = new List<VoiceActor.VoiceActor>();
			internal readonly List<CharacterGroup> m_groups;
			private int m_groupsWithConflictingGenders;
			internal int m_minimumProximity = Int32.MaxValue;
			internal int RemainingUsableActors { get; set; }
			internal CharacterGroup NarratorGroup { get; private set; }
			internal CharacterGroup ExtraBiblicalGroup { get; private set; }

			private TrialGroupConfiguration(IEnumerable<CharacterGroup> predeterminedGroups, int nbrUnassignedActors,
				int n, int e,
				string anyNarratorBookId, string anyExtraBookId,
				Project project,
				IComparer<string> characterComparer)
			{
				m_groups = predeterminedGroups.ToList();
				RemainingUsableActors = nbrUnassignedActors;

				if (RemainingUsableActors == 0 && m_groups.Count == 1)
				{
					NarratorGroup = ExtraBiblicalGroup = m_groups[0];
				}
				else
				{
					// TODO: Create the number of narrators and extra-biblicals requested

					if (RemainingUsableActors > 0 && n > 0)
					{
						NarratorGroup = new CharacterGroup(project, characterComparer) { Status = true };
						m_groups.Add(NarratorGroup);
						RemainingUsableActors--;
					}

					if (RemainingUsableActors > 0 && e > 0)
					{
						ExtraBiblicalGroup = new CharacterGroup(project, characterComparer);
						m_groups.Add(ExtraBiblicalGroup);
						RemainingUsableActors--;
					}
					else if (e == 0)
						ExtraBiblicalGroup = NarratorGroup;
				}
				if (NarratorGroup != null && !NarratorGroup.Closed && anyNarratorBookId != null)
					NarratorGroup.CharacterIds.Add(CharacterVerseData.GetStandardCharacterId(anyNarratorBookId, CharacterVerseData.StandardCharacter.Narrator));
				if (ExtraBiblicalGroup != null && !ExtraBiblicalGroup.Closed && anyExtraBookId != null)
					ExtraBiblicalGroup.CharacterIds.Add(CharacterVerseData.GetStandardCharacterId(anyExtraBookId, CharacterVerseData.StandardCharacter.BookOrChapter));
			}

			private bool IsBetterThan(TrialGroupConfiguration other)
			{
				if (m_groupsWithConflictingGenders < other.m_groupsWithConflictingGenders)
					return true;
				if (m_groupsWithConflictingGenders > other.m_groupsWithConflictingGenders)
					return false;
				return m_minimumProximity > other.m_minimumProximity;
			}

			private void AddActor(ActorGender gender)
			{
				m_narratorsOrExtraActors.Add(new VoiceActor.VoiceActor { Gender = gender });
			}

			internal static List<TrialGroupConfiguration> GeneratePossibilities(List<CharacterGroup> predeterminedGroups, int nbrUnassignedActors,
				int nbrAdultMaleActors, int nbrAdultFemaleActors, int numberOfNarratorGroups, int numberOfExtraGroups,
				string anyNarratorBookId, string anyExtraBookId,
				Project project,
				IComparer<string> characterComparer)
			{
				var nbrStandardGroups = numberOfNarratorGroups + numberOfExtraGroups;
				var nbrConfigs = nbrStandardGroups + 1;
				nbrStandardGroups = Math.Min(nbrAdultMaleActors + nbrAdultFemaleActors, nbrStandardGroups);
				var list = new List<TrialGroupConfiguration>(nbrConfigs);
				for (int n = 0; n < nbrConfigs; n++)
				{
					var config = new TrialGroupConfiguration(predeterminedGroups, nbrUnassignedActors,
						numberOfNarratorGroups, numberOfExtraGroups,
						anyNarratorBookId, anyExtraBookId,
						project,
						characterComparer);

					int nbrMalesInConfig = Math.Min(n, nbrAdultMaleActors);
					for (int i = 0; i < nbrMalesInConfig; i++)
						config.AddActor(ActorGender.Male);

					int nbrFemalesInConfig = Math.Min(nbrStandardGroups - nbrMalesInConfig, nbrAdultFemaleActors);
					for (int i = 0; i < nbrFemalesInConfig; i++)
						config.AddActor(ActorGender.Female);

					if (config.m_narratorsOrExtraActors.Count == nbrStandardGroups)
						list.Add(config);
				}

				return list;
			}

			internal static TrialGroupConfiguration Best(List<TrialGroupConfiguration> configurations)
			{
				foreach (var configuration in configurations)
					configuration.CalculateConflicts();

				var best = configurations.First();
				foreach (var config in configurations.Skip(1).Where(config => config.IsBetterThan(best)))
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
		}
	}
}
