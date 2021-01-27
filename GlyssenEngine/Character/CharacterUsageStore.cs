using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Glyssen.Shared;
using GlyssenEngine.Utilities;
using SIL.Extensions;
using SIL.Scripture;
using static System.String;

namespace GlyssenEngine.Character
{
	internal class CharacterUsageStore : ICharacterUsageStore
	{
		private readonly ScrVers m_versification;
		private readonly ICharacterVerseInfo m_cvInfo;
		private readonly Func<string, IEnumerable<string>> m_getVariants;
		private static readonly IEqualityComparer<CharacterSpeakingMode> s_characterComparer = new CharacterEqualityComparer();
		private static readonly Regex s_regexTextInParentheses = new Regex(@"\([^)]*\)", RegexOptions.Compiled);

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
			character = character.Trim();
			defaultCharacter = null;

			var charactersInPassage = m_cvInfo.GetCharacters(bookNum, chapter, verses, m_versification, true, true).ToList();
			var matches = charactersInPassage.Where(cv => cv.Character == character).ToList();
			if (matches.Any())
			{
				var singleMatch = matches.OnlyOrDefault();
				if (singleMatch == null)
				{
					singleKnownDelivery = null;
				}
				else
				{
					singleKnownDelivery = GetDelivery(singleMatch);
					defaultCharacter = GetDefaultCharacter(singleMatch);
				}
				
				return character;
			}

			matches = charactersInPassage.Where(cv => cv.Character.SplitCharacterId().Any(c => c == character)).ToList();
			if (matches.Any())
			{
				singleKnownDelivery = matches.OnlyOrDefault()?.Delivery;
				defaultCharacter = character;
				return matches.First().Character;
			}

			singleKnownDelivery = null;

			if (m_getVariants != null)
			{
				matches.AddRange(charactersInPassage.Where(cv => m_getVariants.Invoke(cv.Character).Any(c => c == character)));

				if (matches.Count == 1)
				{
					singleKnownDelivery = GetDelivery(matches[0]);
					defaultCharacter = GetDefaultCharacter(matches[0]);
					return matches[0].Character;
				}
				if (matches.Count > 0 && matches.Distinct(s_characterComparer).Count() == 1)
				{
					defaultCharacter = GetDefaultCharacter(matches[0]);
					return matches[0].Character;
				}
			}

			character = character.ToLowerInvariant().Replace(" ", "");

			var singleCloseMatch = charactersInPassage.OnlyOrDefault(cv => IsCloseMatch(cv.Character,
				character));

			if (singleCloseMatch != null)
			{
				singleKnownDelivery = GetDelivery(singleCloseMatch);
				defaultCharacter = GetDefaultCharacter(singleCloseMatch);
				return singleCloseMatch.Character;
			}

			var matchesWithSpecifiedDefault = new List<Tuple<CharacterSpeakingMode, string>>();
			foreach (var cv in charactersInPassage)
			{
				var individuals = cv.Character.SplitCharacterId();
				if (individuals.Length == 1)
					continue;

				var closeIndividualMatch = individuals.FirstOrDefault(individual => IsCloseMatch(individual, character));

				if (closeIndividualMatch != null)
					matchesWithSpecifiedDefault.Add(new Tuple<CharacterSpeakingMode, string>(cv, closeIndividualMatch));
			}
			
			if (matchesWithSpecifiedDefault.Count == 1)
			{
				singleKnownDelivery = GetDelivery(matchesWithSpecifiedDefault[0].Item1);
				defaultCharacter = matchesWithSpecifiedDefault[0].Item2;
				return matchesWithSpecifiedDefault[0].Item1.Character;
			}

			return null;
		}

		private bool IsCloseMatch(string testCharacter, string character)
		{
			testCharacter = testCharacter.ToLowerInvariant().Replace(" ", "");
			var result = character == testCharacter ||
				character == testCharacter.Replace("/", "and") ||
				character == testCharacter.Replace("/", "") ||
				s_regexTextInParentheses.Replace(character, "") == s_regexTextInParentheses.Replace(testCharacter, "") ||
				character.Where(char.IsLetterOrDigit).SequenceEqual(testCharacter.Where(char.IsLetterOrDigit));
			if (result)
				return true;

			var individuals = testCharacter.SplitCharacterId();
			return individuals.Length > 1 && individuals.SetEquals(character.SplitCharacterId());
		}

		private static string GetDelivery(CharacterSpeakingMode singleMatch) =>
			singleMatch.Delivery == Empty ? null : singleMatch.Delivery;

		private static string GetDefaultCharacter(CharacterSpeakingMode singleMatch)
		{
			var defaultCharacter = singleMatch.ResolvedDefaultCharacter;
			return singleMatch.Character == defaultCharacter ? null : defaultCharacter;
		}
	}
}
