using System;
using System.Collections.Generic;
using System.Linq;
using Glyssen.Shared;
using GlyssenEngine.Utilities;
using SIL.Scripture;
using static System.String;

namespace GlyssenEngine.Character
{
	internal class CharacterUsageStore : ICharacterUsageStore
	{
		private readonly ScrVers m_versification;
		private readonly ICharacterVerseInfo m_cvInfo;
		private Func<string, IEnumerable<string>> m_getVariants;

		internal CharacterUsageStore(ScrVers versification, ICharacterVerseInfo cvInfo,
			Func<string, IEnumerable<string>> getLocalizedVariants)
		{
			m_versification = versification;
			m_cvInfo = cvInfo;
			m_getVariants = getLocalizedVariants;
		}

		public string GetStandardCharacterName(string character, int bookNum, int chapter,
			IReadOnlyCollection<IVerse> verses, out string singleKnownDelivery, out string defaultCharacter)
		{
			defaultCharacter = null;

			var charactersInPassage = m_cvInfo.GetCharacters(bookNum, chapter, verses, m_versification, true, true).ToList();
			var exactMatches = charactersInPassage.Where(cv => cv.Character == character).ToList();
			if (exactMatches.Any())
			{
				var singleMatch = exactMatches.OnlyOrDefault();
				if (singleMatch == null)
				{
					singleKnownDelivery = null;
				}
				else
				{
					singleKnownDelivery = singleMatch.Delivery;
					if (singleKnownDelivery == Empty)
						singleKnownDelivery = null;
					defaultCharacter = singleMatch.ResolvedDefaultCharacter;
					if (character == defaultCharacter)
						defaultCharacter = null;
				}
				
				return character;
			}

			exactMatches = charactersInPassage.Where(cv => cv.Character.SplitCharacterId().Any(c => c == character)).ToList();
			if (exactMatches.Any())
			{
				singleKnownDelivery = exactMatches.OnlyOrDefault()?.Delivery;
				defaultCharacter = character;
				return exactMatches.First().Character;
			}

			singleKnownDelivery = null;

			if (m_getVariants != null)
			{
				foreach (var characterId in charactersInPassage.Select(cv => cv.Character))
				{
					if (m_getVariants.Invoke(characterId).Any(c => c == character))
					{
						return characterId;
					}
				}
			}

			return null;
		}
	}
}
