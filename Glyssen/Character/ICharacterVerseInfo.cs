using System.Collections.Generic;
using SIL.Scripture;

namespace Glyssen.Character
{
	public interface ICharacterVerseRepository
	{
		/// <summary>
		/// Gets all characters completely covered by the given range of verses. If there are multiple verses, only
		/// characters known to speak in ALL the verses will be included in the returned set.
		/// </summary>
		IEnumerable<CharacterVerse> GetCharacters(int bookId, int chapter, int initialStartVerse, int initialEndVerse = 0,
			int finalVerse = 0, ScrVers versification = null, bool includeAlternates = false);

		CharacterVerse GetImplicitCharacter(int bookId, int chapter, int startVerse, int endVerse = 0, ScrVers versification = null);
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
