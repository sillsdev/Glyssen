using System;
using System.Collections.Generic;
using System.Linq;
using Glyssen.Character;
using SIL.Scripture;

namespace Glyssen.Quote
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

		public IEnumerable<CharacterVerse> GetCharacters(int bookId, int chapter, int initialStartVerse, int initialEndVerse = 0, int finalVerse = 0, ScrVers versification = null, bool includeAlternates = false)
		{
			return m_cvInfo.GetCharacters(bookId, chapter, initialStartVerse, initialEndVerse, finalVerse, versification, includeAlternates)
				.Select(cv => cv.QuoteType != QuoteType.Hypothetical ||
					m_referenceText.GetBook(bookId)?.GetBlocksForVerse(chapter,
							initialStartVerse, finalVerse > 0 ? finalVerse : (initialEndVerse > 0 ? initialEndVerse : initialStartVerse))
						.Any(b => b.CharacterId == cv.Character) == true ? cv : new CharacterVerse(cv.BcvRef,
						CharacterVerseData.GetStandardCharacterId(cv.BookCode, CharacterVerseData.StandardCharacter.Narrator),
						String.Empty, String.Empty, false, QuoteType.Quotation)).Distinct();
		}

		public ICharacterDeliveryInfo GetImplicitCharacter(int bookId, int chapter, int startVerse, int endVerse = 0, ScrVers versification = null)
		{
			return m_cvInfo.GetImplicitCharacter(bookId, chapter, startVerse, endVerse, versification);
		}
	}
}
