using System.Collections.Generic;
using System.Linq;
using Waxuquerque.Character;

namespace Waxuquerque.CharacterGroupGeneration
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

		public AuthorStats(BiblicalAuthors.Author author, IEnumerable<string> bookIds, Dictionary<string, int> keyStrokesByCharacterId, bool includeAuthorCharacter = false)
		{
			Author = author;
			m_bookIds = bookIds.Intersect(author.Books).ToList();
			m_keyStrokesByCharacterId = keyStrokesByCharacterId;
			foreach (var bookId in m_bookIds)
				KeyStrokeCount += m_keyStrokesByCharacterId[CharacterVerseData.GetStandardCharacterId(bookId, CharacterVerseData.StandardCharacter.Narrator)];

			if (includeAuthorCharacter && author.CombineAuthorAndNarrator)
				KeyStrokeCount += m_keyStrokesByCharacterId[author.Name];
		}
	}

	internal class AuthorStatsComparer : IComparer<AuthorStats>
	{
		private readonly bool m_prioritizeAuthorsWithSpeakingParts;

		public AuthorStatsComparer(bool prioritizeAuthorsWithSpeakingParts)
		{
			m_prioritizeAuthorsWithSpeakingParts = prioritizeAuthorsWithSpeakingParts;
		}

		public int Compare(AuthorStats x, AuthorStats y)
		{
			if (m_prioritizeAuthorsWithSpeakingParts)
			{
				if (x.Author.CombineAuthorAndNarrator)
				{
					if (!y.Author.CombineAuthorAndNarrator)
						return 1;
				}
				else if (y.Author.CombineAuthorAndNarrator)
					return -1;
			}
			return x.KeyStrokeCount.CompareTo(y.KeyStrokeCount);
		}
	}
}
