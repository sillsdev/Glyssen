using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Xml.Serialization;
using Glyssen.VoiceActor;
using SIL.ObjectModel;
using SIL.Xml;

namespace Glyssen.Character
{
	[XmlRoot("CharacterGroupList")]
	public class CharacterGroupList
	{
		private ObservableList<CharacterGroup> m_characterGroups;
		private int m_iMale;
		private int m_iFemale;
		private int m_iChild;

		public CharacterGroupList()
		{
			CharacterGroups = new ObservableList<CharacterGroup>();
		}

		[XmlElement("CharacterGroup")]
		public ObservableList<CharacterGroup> CharacterGroups
		{
			get { return m_characterGroups; }
			set 
			{
				m_characterGroups = value;
				m_characterGroups.CollectionChanged += CharacterGroups_CollectionChanged;
			}
		}

		void CharacterGroups_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.NewItems == null)
				return;
			var nextNumberToTry = 1;
			foreach (CharacterGroup characterGroup in e.NewItems)
			{
				if (characterGroup.GroupNumber == default(int))
				{
					while (CharacterGroups.Any(g => g.GroupNumber == nextNumberToTry))
						nextNumberToTry++;
					characterGroup.GroupNumber = nextNumberToTry;
				}
				if (characterGroup.GroupIdNumber != 0)
				{
					switch (characterGroup.GroupIdLabel)
					{
						case CharacterGroup.Label.Male:
							if (characterGroup.GroupIdNumber > m_iMale)
								m_iMale = characterGroup.GroupIdNumber;
							break;
						case CharacterGroup.Label.Female:
							if (characterGroup.GroupIdNumber > m_iFemale)
								m_iFemale = characterGroup.GroupIdNumber;
							break;
						case CharacterGroup.Label.Child:
							if (characterGroup.GroupIdNumber > m_iChild)
								m_iChild = characterGroup.GroupIdNumber;
							break;
					}
				}
				UpdateGroupIdNumbers(e.NewItems.Cast<CharacterGroup>());
			}
		}

		public void SaveToFile(string filename)
		{
			XmlSerializationHelper.SerializeToFile(filename, this);
		}
		public static CharacterGroupList LoadCharacterGroupListFromFile(string filename, Project project)
		{
			var comparer = new CharacterByKeyStrokeComparer(project.GetKeyStrokesByCharacterId());
			var list = XmlSerializationHelper.DeserializeFromFile<CharacterGroupList>(filename);
			foreach (var characterGroup in list.CharacterGroups)
				characterGroup.Initialize(project, comparer);
			return list;
		}

		/// <summary>
		/// <returns>true if any CharacterGroup has any voice actor assigned, false otherwise</returns>
		/// </summary>
		public bool AnyVoiceActorAssigned()
		{
			return CharacterGroups.Any(cg => cg.VoiceActorId != -1);
		}

		/// <summary>
		/// <returns>true if any CharacterGroup has the specified voice actor assigned, false otherwise</returns>
		/// </summary>
		public bool HasVoiceActorAssigned(int voiceActorId)
		{
			return CharacterGroups.Any(cg => cg.VoiceActorId == voiceActorId);
		}

		public int CountVoiceActorsAssigned()
		{
			return CharacterGroups.Count(cg => cg.VoiceActorId != -1);
		}

		public CharacterGroup GroupContainingCharacterId(string characterId)
		{
			return CharacterGroups.FirstOrDefault(g => g.CharacterIds.Contains(characterId));
		}

		public CharacterGroup GetGroupByName(string name)
		{
			return CharacterGroups.FirstOrDefault(g => g.Name == name);
		}

		public IEnumerable<CharacterGroup> AssignedGroups
		{
			get { return CharacterGroups.Where(g => g.IsVoiceActorAssigned); }
		}

		/// <summary>
		/// Note: There should only ever be one such group, but early on, Glyssen would allow the same
		/// actor to be assigned to multiple groups, so for compatibility reasons, we allow for this.
		/// </summary>
		public IEnumerable<CharacterGroup> GetGroupsAssignedToActor(int actorId)
		{
			if (actorId <= CharacterGroup.kNoActorAssigned)
				throw new ArgumentException("GetGroupsAssignedToActor should not be used to get groups assigned to no actor (no good reason; it just shouldn't).");
			return CharacterGroups.Where(g => g.VoiceActorId == actorId);
		}

		public void RemoveVoiceActor(int voiceActorId)
		{
			foreach (var group in GetGroupsAssignedToActor(voiceActorId))
				group.RemoveVoiceActor();
		}

		public void PopulateEstimatedHours(Dictionary<string, int> keyStrokesByCharacterId)
		{
			PopulateEstimatedHours(CharacterGroups, keyStrokesByCharacterId);
		}

		public static void PopulateEstimatedHours(IList<CharacterGroup> groups, Dictionary<string, int> keyStrokesByCharacterId)
		{
			foreach (var group in groups)
			{
				int keyStrokes = 0;
				foreach (var characterId in group.CharacterIds)
				{
					int keystrokesForCharacter;
					if (keyStrokesByCharacterId.TryGetValue(characterId, out keystrokesForCharacter))
						keyStrokes += keystrokesForCharacter;
				}
				group.EstimatedHours = keyStrokes / Program.kKeyStrokesPerHour;
			}
		}

		public static void AssignGroupIds(IList<CharacterGroup> groups, Dictionary<string, int> keyStrokesByCharacterId)
		{
			int iMale = 0;
			int iFemale = 0;
			int iChild = 0;
			PopulateEstimatedHours(groups, keyStrokesByCharacterId);
			foreach (var group in groups.OrderByDescending(g => g.EstimatedHours))
			{
				if (group.AssignedToCameoActor)
				{
					group.GroupIdLabel = CharacterGroup.Label.Other;
					group.GroupIdOtherText = group.VoiceActor.Name;
				}
				else if (group.IsVoiceActorAssigned)
				{
					VoiceActor.VoiceActor actor = group.VoiceActor;
					if (actor.Age == ActorAge.Child)
					{
						group.GroupIdLabel = CharacterGroup.Label.Child;
						group.GroupIdNumber = ++iChild;
					}
					else if (actor.Gender == ActorGender.Male)
					{
						group.GroupIdLabel = CharacterGroup.Label.Male;
						group.GroupIdNumber = ++iMale;
					}
					else
					{
						group.GroupIdLabel = CharacterGroup.Label.Female;
						group.GroupIdNumber = ++iFemale;
					}
				}
				else
				{
					// Currently, this is only used for migrating legacy projects
					if (group.ContainsCharacterWithAge(CharacterAge.Child))
					{
						group.GroupIdLabel = CharacterGroup.Label.Child;
						group.GroupIdNumber = ++iChild;
					}
					if (group.ContainsCharacterWithGender(CharacterGender.Female) || group.ContainsCharacterWithGender(CharacterGender.PreferFemale))
					{
						group.GroupIdLabel = CharacterGroup.Label.Female;
						group.GroupIdNumber = ++iFemale;
					}
					else
					{
						group.GroupIdLabel = CharacterGroup.Label.Male;
						group.GroupIdNumber = ++iMale;
					}
				}
			}
		}

		public void UpdateGroupIdNumbers(IEnumerable<CharacterGroup> subset = null)
		{
			IEnumerable<CharacterGroup> groups = subset ?? CharacterGroups;
			foreach (var group in groups)
			{
				if (group.GroupIdNumber == 0)
				{
					switch (group.GroupIdLabel)
					{
						case CharacterGroup.Label.Male:
							group.GroupIdNumber = ++m_iMale;
							break;
						case CharacterGroup.Label.Female:
							group.GroupIdNumber = ++m_iFemale;
							break;
						case CharacterGroup.Label.Child:
							group.GroupIdNumber = ++m_iChild;
							break;
					}
				}
			}
		}
	}
}
