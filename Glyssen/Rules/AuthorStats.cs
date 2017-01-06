using System.Collections.Generic;
using System.Linq;
using Glyssen.Character;

namespace Glyssen.Rules
{
	public class AuthorStats
	{
		public BiblicalAuthors.Author Author { get; private set; }
		public int KeyStrokeCount { get; set; }
		public IEnumerable<string> BookIds
		{
			get { return m_bookIds; }
		}
		private readonly List<string> m_bookIds;
		private readonly Dictionary<string, int> m_keyStrokesByCharacterId;

		/// <summary>
		/// For easier testing
		/// </summary>
		internal AuthorStats(BiblicalAuthors.Author author, Dictionary<string, int> keyStrokesByCharacterId, params string[] bookIds)
			: this(author, bookIds, keyStrokesByCharacterId)
		{
		}

		public AuthorStats(BiblicalAuthors.Author author, IEnumerable<string> bookIds, Dictionary<string, int> keyStrokesByCharacterId)
		{
			Author = author;
			m_bookIds = bookIds.Intersect(author.Books).ToList();
			m_keyStrokesByCharacterId = keyStrokesByCharacterId;
			foreach (var bookId in m_bookIds)
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
