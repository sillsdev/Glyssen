using System;
using System.Collections.Generic;
using System.Linq;
using Glyssen.Character;

namespace Glyssen.Dialogs
{
	public class AddCharactersToGroupViewModel
	{
		private readonly IReadOnlyDictionary<string, CharacterDetail> m_characterDetailDictionary;
		private readonly Dictionary<string, int> m_keyStrokesByCharacterId;
		private readonly List<CharacterDetail> m_availableCharacters;
		private List<CharacterDetail> m_filteredCharacters;
		private readonly VoiceActor.VoiceActor m_cameoActor;

		public AddCharactersToGroupViewModel(IReadOnlyDictionary<string, CharacterDetail> characterDetailDictionary,
			Dictionary<string, int> keyStrokesByCharacterId,
			ISet<string> existingCharactersInGroup,
			VoiceActor.VoiceActor cameoActor = null)
		{
			m_characterDetailDictionary = characterDetailDictionary;
			m_keyStrokesByCharacterId = keyStrokesByCharacterId;
			var charactersNotInGroup = m_keyStrokesByCharacterId.Where(kvp => !existingCharactersInGroup.Contains(kvp.Key));
			if (cameoActor != null)
			{
				m_availableCharacters = new List<CharacterDetail>();
				m_cameoActor = cameoActor;
				foreach (var kvp in charactersNotInGroup)
				{
					if ((kvp.Value / GlyssenInfo.kKeyStrokesPerHour) < GlyssenInfo.kCameoCharacterEstimatedHoursLimit)
					{
						var detail = m_characterDetailDictionary[kvp.Key];
						if (cameoActor.Matches(detail))
							m_availableCharacters.Add(detail);
					}
				}
			}
			else
			{
				m_availableCharacters = new List<CharacterDetail>(charactersNotInGroup.Select(k => m_characterDetailDictionary[k.Key]));
			}

			// TODO: Sort list
		}

		public bool AddingToCameoGroup
		{
			get { return m_cameoActor != null; }
		}

		public string CameoActorName
		{
			get { return AddingToCameoGroup ? m_cameoActor.Name : null; }
		}

		private List<CharacterDetail> ActiveList
		{
			get { return (m_filteredCharacters ?? m_availableCharacters); }
		}

		public int FilteredCharactersCount
		{
			get { return ActiveList.Count; }
		}

		public void FilterCharacterIds(string match)
		{
			m_filteredCharacters = new List<CharacterDetail>();
			foreach (var character in m_availableCharacters)
			{
				var charName = CharacterVerseData.GetCharacterNameForUi(character.CharacterId);
				if (charName.IndexOf(match, StringComparison.OrdinalIgnoreCase) >= 0)
				{
					m_filteredCharacters.Add(character);
				}
			}
		}

		public string GetCharacterId(int filteredCharacterIndex)
		{
			return ActiveList[filteredCharacterIndex].CharacterId;
		}

		public string GetLocalizedCharacterId(int filteredCharacterIndex)
		{
			return CharacterVerseData.GetCharacterNameForUi(GetCharacterId(filteredCharacterIndex));
		}

		public string GetUiStringForCharacterGender(int filteredCharacterIndex)
		{
			return CharacterGroupAttribute<CharacterGender>.GetUiStringForValue(ActiveList[filteredCharacterIndex].Gender);
		}

		public string GetUiStringForCharacterAge(int filteredCharacterIndex)
		{
			return CharacterGroupAttribute<CharacterAge>.GetUiStringForValue(ActiveList[filteredCharacterIndex].Age);
		}

		public double GetEstimatedHoursForCharacter(int filteredCharacterIndex)
		{
			return m_keyStrokesByCharacterId[ActiveList[filteredCharacterIndex].CharacterId] /
				GlyssenInfo.kKeyStrokesPerHour;
		}
	}
}
