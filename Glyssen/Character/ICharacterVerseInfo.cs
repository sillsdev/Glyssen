using System.Collections.Generic;
using SIL.Scripture;

namespace Glyssen.Character
{
	public interface ICharacterVerseRepository
	{
		IEnumerable<CharacterVerse> GetCharacters(int bookId, int chapter, int initialStartVerse, int initialEndVerse = 0, int finalVerse = 0, ScrVers versification = null, bool includeAlternates = false);
	}

	public interface ICharacterVerseInfo : ICharacterVerseRepository
	{
		IEnumerable<CharacterVerse> GetAllQuoteInfo();

		IEnumerable<CharacterVerse> GetAllQuoteInfo(string bookId);

		IEnumerable<CharacterVerse> GetUniqueCharacterAndDeliveries();

		IEnumerable<CharacterVerse> GetUniqueCharacterAndDeliveries(string bookCode);

		IEnumerable<CharacterVerse> GetUniqueCharacterAndDeliveries(string bookCode, int chapter);

		IEnumerable<string> GetUniqueDeliveries();
	}
}
