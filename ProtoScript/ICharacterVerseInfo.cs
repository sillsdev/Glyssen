using System.Collections.Generic;

namespace ProtoScript
{
	public interface ICharacterVerseInfo
	{
		IEnumerable<CharacterVerse> GetCharacters(string bookId, int chapter, int verseStart, int verseEnd = 0);

		IEnumerable<CharacterVerse> GetAllQuoteInfo(string bookId);
	}
}
