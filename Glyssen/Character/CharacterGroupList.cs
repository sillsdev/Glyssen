using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
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
		private int m_iNarrator;

		public CharacterGroupList()
		{
			CharacterGroups = new ObservableList<CharacterGroup>();
		}

		[XmlElement("CharacterGroup")]
		public ObservableList<CharacterGroup> CharacterGroups
		{
			get { return m_characterGroups; }
			private set 
			{
				m_characterGroups = value;
				m_characterGroups.CollectionChanged += CharacterGroups_CollectionChanged;
			}
		}

		void CharacterGroups_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (!CharacterGroups.Any())
			{
				m_iMale = 0;
				m_iFemale = 0;
				m_iChild = 0;
				m_iNarrator = 0;
				return;
			}
			if (e.NewItems == null)
				return;
			foreach (CharacterGroup characterGroup in e.NewItems)
			{
				if (characterGroup.GroupIdNumber != 0)
				{
					switch (characterGroup.GroupIdLabel)
					{
						case CharacterGroup.Label.Narrator:
							if (characterGroup.GroupIdNumber > m_iNarrator)
								m_iNarrator = characterGroup.GroupIdNumber;
							break;
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
			var list = XmlSerializationHelper.DeserializeFromFile<CharacterGroupList>(filename);
			foreach (var characterGroup in list.CharacterGroups)
				characterGroup.Initialize(project);
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

		public CharacterGroup GetGroupById(string id)
		{
			return CharacterGroups.FirstOrDefault(g => g.GroupId == id);
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

		public static void AssignGroupIds(IList<CharacterGroup> groups)
		{
			int iMale = 0;
			int iFemale = 0;
			int iChild = 0;
			int iNarrator = 0;
			foreach (var group in groups.Where(g => g.GroupIdLabel != CharacterGroup.Label.Other).OrderByDescending(g => g.EstimatedHours))
			{
				Debug.Assert(group.GroupIdLabel == CharacterGroup.Label.None);

				group.SetGroupIdLabel();

				switch (group.GroupIdLabel)
				{
					case CharacterGroup.Label.Narrator:
						group.GroupIdNumber = ++iNarrator;
						break;
					case CharacterGroup.Label.Male:
						group.GroupIdNumber = ++iMale;
						break;
					case CharacterGroup.Label.Female:
						group.GroupIdNumber = ++iFemale;
						break;
					case CharacterGroup.Label.Child:
						group.GroupIdNumber = ++iChild;
						break;
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
						case CharacterGroup.Label.Narrator:
							group.GroupIdNumber = ++m_iNarrator;
							break;
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
