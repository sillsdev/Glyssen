using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SIL.ScriptureUtils;

namespace ProtoScript
{
	public abstract class CharacterVerseData : ICharacterVerseInfo
	{
		/// <summary>Blocks represents a quote whose character has not been set (usually represents an unexpected quote)</summary>
		public const string UnknownCharacter = "Unknown";
		/// <summary>
		/// Blocks represents a quote whose character has not been set.
		/// Used when the user needs to disambiguate between multiple potential characters.
		/// </summary>
		public const string AmbiguousCharacter = "Ambiguous";

		public enum StandardCharacter
		{
			Narrator,
			BookOrChapter,
			ExtraBiblical,
			Intro,
		}

		public static bool IsCharacterStandard(string characterId)
		{
			if (String.IsNullOrEmpty(characterId))
				return false;

			int i = characterId.IndexOf("-", StringComparison.Ordinal);
			if (i < 0)
				return false;
			switch (characterId.Substring(0, i + 1))
			{
				case kNarratorPrefix:
				case kIntroPrefix:
				case kExtraBiblicalPrefix:
				case kBookOrChapterPrefix:
					return true;
			}
			return false;
		}

		public static string GetStandardCharacterId(string bookId, StandardCharacter standardCharacterType)
		{
			return GetCharacterPrefix(standardCharacterType) + bookId;
		}

		public static bool IsCharacterOfType(string characterId, StandardCharacter standardCharacterType)
		{
			return characterId.StartsWith(GetCharacterPrefix(standardCharacterType));
		}

		private static string GetCharacterPrefix(StandardCharacter standardCharacterType)
		{
			switch (standardCharacterType)
			{
				case StandardCharacter.Narrator:
					return kNarratorPrefix;
				case StandardCharacter.BookOrChapter:
					return kBookOrChapterPrefix;
				case StandardCharacter.ExtraBiblical:
					return kExtraBiblicalPrefix;
				case StandardCharacter.Intro:
					return kIntroPrefix;
				default:
					throw new ArgumentException("Unexpected standard character type.");
			}
		}

		/// <summary>Character ID prefix for material to be read by narrator</summary>
		protected const string kNarratorPrefix = "narrator-";
		/// <summary>Character ID prefix for book titles or chapter breaks</summary>
		protected const string kBookOrChapterPrefix = "BC-";
		/// <summary>Character ID prefix for extra-biblical material (section heads, etc.)</summary>
		protected const string kExtraBiblicalPrefix = "extra-";
		/// <summary>Character ID prefix for intro material</summary>
		protected const string kIntroPrefix = "intro-";

		private ISet<CharacterVerse> m_data = new HashSet<CharacterVerse>();
		private IEnumerable<CharacterVerse> m_uniqueCharacterAndDeliveries;
		private IEnumerable<string> m_uniqueDeliveries;

		public IEnumerable<CharacterVerse> GetCharacters(string bookCode, int chapter, int startVerse, int endVerse = 0)
		{
			if (startVerse > 0 && endVerse == 0)
				return m_data.Where(cv => cv.BookCode == bookCode && cv.Chapter == chapter && cv.Verse == startVerse);
			return m_data.Where(cv => cv.BookCode == bookCode && cv.Chapter == chapter && cv.Verse >= startVerse && cv.Verse <= endVerse);
		}

		public IEnumerable<CharacterVerse> GetAllQuoteInfo()
		{
			return m_data;
		}

		public IEnumerable<CharacterVerse> GetAllQuoteInfo(string bookCode)
		{
			return m_data.Where(cv => cv.BookCode == bookCode);
		}

		public virtual void AddCharacterVerse(CharacterVerse cv)
		{
			m_data.Add(cv);
			m_uniqueCharacterAndDeliveries = null;
			m_uniqueDeliveries = null;
		}

		public bool Any()
		{
			return m_data.Any();
		}

		public IEnumerable<CharacterVerse> GetUniqueCharacterAndDeliveries()
		{
			return m_uniqueCharacterAndDeliveries ?? (m_uniqueCharacterAndDeliveries = new SortedSet<CharacterVerse>(m_data, new CharacterDeliveryComparer()));
		}

		public IEnumerable<CharacterVerse> GetUniqueCharacterAndDeliveries(string bookCode)
		{
			return new SortedSet<CharacterVerse>(m_data.Where(cv => cv.BookCode == bookCode), new CharacterDeliveryComparer());
		}

		public IEnumerable<CharacterVerse> GetUniqueCharacterAndDeliveries(string bookCode, int chapter)
		{
			return new SortedSet<CharacterVerse>(m_data.Where(cv => cv.BookCode == bookCode && cv.Chapter == chapter), new CharacterDeliveryComparer());
		}

		public IEnumerable<string> GetUniqueDeliveries()
		{
			return m_uniqueDeliveries ?? (m_uniqueDeliveries = new SortedSet<string>(m_data.Select(cv => cv.Delivery).Where(d => !string.IsNullOrEmpty(d))));
		}

		public void RemoveAll(IEnumerable<CharacterVerse> cvsToRemove, IEqualityComparer<CharacterVerse> comparer)
		{
			var intersection = m_data.Intersect(cvsToRemove, comparer);
			foreach (CharacterVerse cv in intersection)
				m_data.Remove(cv);

			m_uniqueCharacterAndDeliveries = null;
			m_uniqueDeliveries = null;
		}

		public void LoadData(string tabDelimitedCharacterVerseData)
		{
			var set = new HashSet<CharacterVerse>();
			int lineNumber = 0;
			foreach (var line in tabDelimitedCharacterVerseData.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
			{
				string[] items = line.Split(new[] { "\t" }, StringSplitOptions.None);
				ISet<CharacterVerse> cvs = ProcessLine(items, lineNumber++);
				if (cvs != null)
					set.UnionWith(cvs);
			}
			m_data = set;
			m_uniqueCharacterAndDeliveries = null;
			m_uniqueDeliveries = null;
		}

		protected virtual ISet<CharacterVerse> ProcessLine(string[] items, int lineNumber)
		{
			var list = new HashSet<CharacterVerse>();

			if (items.Length < 6 || items.Length > 8)
				throw new ApplicationException("Bad format in CharacterVerseDataBase! Line #: " + lineNumber + "; Line contents: " + items);

			int chapter = Int32.Parse(items[1]);
			for (int verse = ScrReference.VerseToIntStart(items[2]); verse <= ScrReference.VerseToIntEnd(items[2]); verse++)
				list.Add(new CharacterVerse
				{
					BcvRef = new BCVRef(BCVRef.BookToNumber(items[0]), chapter, verse),
					Character = items[3],
					Delivery = items[4],
					Alias = items[5],
					IsDialogue = (items.Length == 7 && items[6].Equals("True", StringComparison.OrdinalIgnoreCase)),
					UserCreated = (items.Length == 8 && items[7].Equals("True", StringComparison.OrdinalIgnoreCase)),
				});

			return list;
		}

		public string ToTabDelimited()
		{
			var sb = new StringBuilder();
			foreach (CharacterVerse cv in m_data)
				sb.Append(cv.ToTabDelimited()).Append(Environment.NewLine);
			return sb.ToString();
		}
	}
}
