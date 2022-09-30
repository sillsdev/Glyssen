using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Glyssen.Shared;
using GlyssenCharacters;
using SIL.Extensions;
using SIL.Scripture;

namespace GlyssenEngine.Quote
{
	public class ParserCharacterRepository : ICharacterVerseRepository
	{
		private readonly ReferenceText m_referenceText;
		private readonly ICharacterVerseRepository m_cvInfo;

		public ParserCharacterRepository(ICharacterVerseRepository cvInfo, ReferenceText referenceText)
		{
			m_cvInfo = cvInfo;
			m_referenceText = referenceText;
		}

		/// <summary>
		/// Gets all known (potential and/or expected) characters for the given verse (or bridge). If any of
		/// them are Hypothetical, those characters will only be included if the reference text contains a
		/// block actually assigned to that hypothetical character. Otherwise, the hypothetical character
		/// will be treated as a narrator "quotation."
		/// </summary>
		public HashSet<CharacterSpeakingMode> GetCharacters(int bookId, int chapter, IVerse verseOrBridge,
			ScrVers versification = null, bool includeAlternatesAndRareQuotes = false, bool includeNarratorOverrides = false)
		{
			return GetCharacters(bookId, chapter, new[] {verseOrBridge}, versification, includeAlternatesAndRareQuotes);
		}

		/// <summary>
		/// Gets all known (potential and/or expected) characters for the given reference range. If any of
		/// them are Hypothetical, those characters will only be included if the reference text contains a
		/// block actually assigned to that hypothetical character. Otherwise, the hypothetical character
		/// will be treated as a narrator "quotation."
		/// </summary>
		public HashSet<CharacterSpeakingMode> GetCharacters(int bookId, int chapter, IReadOnlyCollection<IVerse> verses,
		ScrVers versification = null, bool includeAlternatesAndRareQuotes = false, bool includeNarratorOverrides = false)
		{
			Debug.Assert(!includeNarratorOverrides, "Can't think of any valid reason the quote parser should ever want to consider" +
				"narrator overrides. These should be applied only after user has done disambiguation.");
			var result = m_cvInfo.GetCharacters(bookId, chapter, verses, versification,
				includeAlternatesAndRareQuotes, includeNarratorOverrides);
			// m_referenceText will never be null for Glyssen users, so the test for null will not effect Glyssen.
			// At this time, Vessel users never have a reference text, so this test prevents an exception being thrown.
			// This logic may be changed in the future so that hypothetical speakers can be better handled. 
			if (m_referenceText != null)
			{
				var hypotheticalsToReplace = result.Where(cv =>
					{
						if (cv.QuoteType == QuoteType.Hypothetical)
						{
							var refTextBook = m_referenceText.GetBook(bookId);
							if (refTextBook != null)
								return !refTextBook.GetBlocksForVerse(chapter,
									verses.First().StartVerse, verses.Last().EndVerse).Any(b => b.CharacterId == cv.Character);
							// REVIEW: Should we replace hypotheticals if there is no reference text for this book?
						}
						return false;
					}).Select(c => c.Character).ToList();
				if (hypotheticalsToReplace.Any())
				{
					result.Add(new CharacterSpeakingMode(CharacterVerseData.GetStandardCharacterId(BCVRef.NumberToBookCode(bookId), CharacterVerseData.StandardCharacter.Narrator),
						String.Empty, String.Empty, false, QuoteType.Quotation));
					result.RemoveAll(e => e.QuoteType == QuoteType.Hypothetical && hypotheticalsToReplace.Contains(e.Character));
				}
			}
			return result;
		}

		public ICharacterDeliveryInfo GetImplicitCharacter(int bookId, int chapter, int startVerse, int endVerse = 0, ScrVers versification = null)
		{
			return m_cvInfo.GetImplicitCharacter(bookId, chapter, startVerse, endVerse, versification);
		}
	}
}
