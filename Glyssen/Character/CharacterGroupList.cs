using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Xml.Serialization;
using SIL.ObjectModel;
using SIL.Xml;

namespace Glyssen.Character
{
	[XmlRoot("CharacterGroupList")]
	public class CharacterGroupList
	{
		private ObservableList<CharacterGroup> m_characterGroups;

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
				m_characterGroups.CollectionChanged += m_characterGroups_CollectionChanged;
			}
		}

		void m_characterGroups_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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
			foreach (var group in CharacterGroups)
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
	}
}
