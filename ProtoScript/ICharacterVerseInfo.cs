using System.Collections.Generic;

namespace ProtoScript
{
	public interface ICharacterVerseInfo
	{
		IEnumerable<CharacterVerse> GetCharacters(string bookId, int chapter, int verse);

		IEnumerable<CharacterVerse> GetAllQuoteInfo(string bookId);
	}
}
