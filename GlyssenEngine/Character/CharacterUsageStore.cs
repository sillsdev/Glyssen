using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Glyssen.Shared;
using GlyssenEngine.Utilities;
using SIL.Extensions;
using SIL.Scripture;
using static System.String;
using FuzzySharp;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;

namespace GlyssenEngine.Character
{
	internal class CharacterUsageStore : ICharacterUsageStore
	{
		public ScrVers Versification { get; }

		private readonly ICharacterVerseInfo m_cvInfo;
		private readonly Func<string, IEnumerable<string>> m_getVariants;
		private static readonly Regex s_regexTextInParentheses = new Regex(@"\([^)]*\)", RegexOptions.Compiled);

		internal CharacterUsageStore(ScrVers versification, ICharacterVerseInfo cvInfo,
			Func<string, IEnumerable<string>> getLocalizedVariants)
		{
			Versification = versification;
			m_cvInfo = cvInfo;
			m_getVariants = getLocalizedVariants;
		}

		private class FuzzyMatchCandidate
		{
			private readonly CharacterSpeakingMode m_csm;
			private readonly string m_candidate;
			private readonly string m_defaultCharacter;

			internal FuzzyMatchCandidate(CharacterSpeakingMode csm, string candidate = null, string defaultCharacter = null)
			{
				m_csm = csm;
				m_candidate = candidate;
				m_defaultCharacter = defaultCharacter;
			}

			public string Character => m_candidate ?? m_csm.Character;

			public string Delivery  => GetDelivery(m_csm);

			public string DefaultCharacter => m_defaultCharacter ?? GetDefaultCharacter(m_csm);

			public string UnderlyingCharacter => m_csm.Character;
		}

		public string GetStandardCharacterName(string character, int bookNum, int chapter,
			IReadOnlyCollection<IVerse> verses, out string singleKnownDelivery, out string defaultCharacter)
		{
			character = character.Trim();
			defaultCharacter = null;
			singleKnownDelivery = null;

			var charactersInPassage = m_cvInfo.GetCharacters(bookNum, chapter, verses, Versification, true, true).ToList();
			if (!charactersInPassage.Any())
			{
				return null;
			}

			var matches = charactersInPassage.Where(cv => cv.Character == character || cv.Alias == character).ToList();
			if (matches.Any())
			{
				var singleMatch = matches.OnlyOrDefault();
				if (singleMatch != null)
				{
					singleKnownDelivery = GetDelivery(singleMatch);
					defaultCharacter = GetDefaultCharacter(singleMatch);
				}
				
				return matches.First().Character;
			}

			var considerIndividualMatches = !character.Contains(CharacterSpeakingMode.kMultiCharacterIdSeparator);
			if (considerIndividualMatches)
			{
				matches = charactersInPassage.Where(cv => cv.Character.SplitCharacterId().Any(c => c == character)).ToList();
				if (matches.Any())
				{
					singleKnownDelivery = matches.OnlyOrDefault()?.Delivery;
					defaultCharacter = character;
					return matches.First().Character;
				}
			}

			// Finally, try for a fuzzy match. The following "magic numbers" are based on some
			var candidates = new List<FuzzyMatchCandidate>(charactersInPassage.Count);

			foreach (var candidateChar in charactersInPassage)
			{
				string[] individuals = null;
				if (considerIndividualMatches)
				{
					individuals = candidateChar.Character.SplitCharacterId();
					if (individuals.Length > 1)
					{
						candidates.AddRange(individuals.Select(individual =>
							new FuzzyMatchCandidate(candidateChar, individual, individual)));
					}
				}

				// Note that the individual candidates (above) have to be added first because the ExtractSorted
				// method below is a stable sort and the weighting (somewhat inexplicably) assigns the same
				// score to an exact match and a partial match.
				candidates.Add(new FuzzyMatchCandidate(candidateChar));

				if (!IsNullOrEmpty(candidateChar.Alias))
					candidates.Add(new FuzzyMatchCandidate(candidateChar, candidateChar.Alias));

				if (m_getVariants != null)
				{
					if (individuals?.Length > 1)
					{
						foreach (var individual in individuals)
						{
							candidates.AddRange(m_getVariants(individual).Select(loc =>
								new FuzzyMatchCandidate(candidateChar, loc, individual)));
						}
					}

					candidates.AddRange(m_getVariants(candidateChar.Character).Select(loc =>
						new FuzzyMatchCandidate(candidateChar, loc)));

					if (!IsNullOrEmpty(candidateChar.Alias))
					{
						candidates.AddRange(m_getVariants(candidateChar.Alias).Select(loc =>
							new FuzzyMatchCandidate(candidateChar, loc)));
					}
				}
			}

			// The following "magic numbers" are based on some trial and error with existing unit
			// test cases (as of 4/13/2022).
			const int minMarginOverSecondPlace = 30; // tests pass if this is [30 - 47]
			const int minScoreToCountAsMatchWhenThereIsOnlyOneKnownCharacter = 74; // [74 - 80]

			var testCharacter = character;
			var options = candidates.Select(c => c.Character).ToList();
			if (!options.Any(o => s_regexTextInParentheses.IsMatch(o)))
				testCharacter = s_regexTextInParentheses.Replace(testCharacter, "");
			var results = Process.ExtractSorted(testCharacter, options, s => s.ToLowerInvariant()).ToList();

			if (results.Any())
			{
				var iSecondPlace = results.IndexOf(r => candidates[r.Index].UnderlyingCharacter !=
					candidates[results[0].Index].UnderlyingCharacter);
				int minScore;
				if (iSecondPlace >= 0)
				{
					minScore = results[iSecondPlace].Score + minMarginOverSecondPlace;
				}
				else
				{
					minScore = minScoreToCountAsMatchWhenThereIsOnlyOneKnownCharacter;
				}

				if (results[0].Score >= minScore)
				{
					var winner = candidates[results[0].Index];
					singleKnownDelivery = winner.Delivery;
					if (candidates.Any(c => c.UnderlyingCharacter == winner.UnderlyingCharacter &&
						    c.Delivery != winner.Delivery))
						singleKnownDelivery = null;
					defaultCharacter = winner.DefaultCharacter;
					return winner.UnderlyingCharacter;
				}
			}

			return null;
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
