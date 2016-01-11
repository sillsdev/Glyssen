using System.Collections.Generic;
using Glyssen.Character;

namespace Glyssen.Rules
{
	public class AuthorStats
	{
		public BiblicalAuthors.Author Author { get; private set; }
		public int KeyStrokeCount { get; set; }
		private readonly Dictionary<string, int> m_keyStrokesByCharacterId;

		public AuthorStats(BiblicalAuthors.Author author, string bookId, Dictionary<string, int> keyStrokesByCharacterId)
		{
			Author = author;
			m_keyStrokesByCharacterId = keyStrokesByCharacterId;
			AddBook(bookId);
		}

		public void AddBook(string bookId)
		{
			KeyStrokeCount += m_keyStrokesByCharacterId[CharacterVerseData.GetStandardCharacterId(bookId, CharacterVerseData.StandardCharacter.Narrator)];
		}
	}

	internal class AuthorStatsComparer : IComparer<AuthorStats>
	{
		public int Compare(AuthorStats x, AuthorStats y)
		{
			return x.KeyStrokeCount.CompareTo(y.KeyStrokeCount);
		}
	}
}
