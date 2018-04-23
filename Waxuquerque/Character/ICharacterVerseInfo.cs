using System.Collections.Generic;
using SIL.Scripture;

namespace Waxuquerque.Character
{
	public interface ICharacterVerseInfo
	{
		/// <summary>
		/// This method is preferred over the string bookId counterpart for performance reasons (so we don't have to look up the book number)
		/// </summary>
		IEnumerable<CharacterVerse> GetCharacters(int bookId, int chapter, int initialStartVerse, int initialEndVerse = 0, int finalVerse = 0, ScrVers versification = null);

		/// <summary>
		/// Prefer the int bookId counterpart method for performance reasons (this method has to perform a book Id lookup)
		/// </summary>
		IEnumerable<CharacterVerse> GetCharacters(string bookId, int chapter, int initialStartVerse, int initialEndVerse = 0, int finalVerse = 0, ScrVers versification = null);

		IEnumerable<CharacterVerse> GetAllQuoteInfo();

		IEnumerable<CharacterVerse> GetAllQuoteInfo(string bookId);

		IEnumerable<CharacterVerse> GetUniqueCharacterAndDeliveries();

		IEnumerable<CharacterVerse> GetUniqueCharacterAndDeliveries(string bookCode);

		IEnumerable<string> GetUniqueDeliveries();
	}
}
