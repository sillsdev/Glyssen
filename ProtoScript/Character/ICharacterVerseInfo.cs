using System.Collections.Generic;

namespace ProtoScript.Character
{
	public interface ICharacterVerseInfo
	{
		IEnumerable<CharacterVerse> GetCharacters(string bookCode, int chapter, int initialStartVerse, int initialEndVerse = 0, int finalVerse = 0);

		IEnumerable<CharacterVerse> GetAllQuoteInfo();

		IEnumerable<CharacterVerse> GetAllQuoteInfo(string bookId);

		IEnumerable<CharacterVerse> GetUniqueCharacterAndDeliveries();

		IEnumerable<CharacterVerse> GetUniqueCharacterAndDeliveries(string bookCode);

		IEnumerable<string> GetUniqueDeliveries();
	}
}
