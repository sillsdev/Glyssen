using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SIL.ScriptureUtils;
using Palaso.Extensions;

namespace ProtoScript.Character
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

		protected const int kiIsDialogue = 6;
		protected const int kiDefaultCharacter = 7;
		protected const int kiParallelPassageInfo = 8;
		protected const int kMaxItems = kiParallelPassageInfo + 1;

		public enum StandardCharacter
		{
			NonStandard,
			Narrator,
			BookOrChapter,
			ExtraBiblical,
			Intro,
		}

		public static bool IsCharacterStandard(string characterId, bool includeNarrator = true)
		{
			switch (GetStandardCharacterType(characterId))
			{
				case StandardCharacter.Narrator:
					return includeNarrator;
				case StandardCharacter.Intro:
				case StandardCharacter.ExtraBiblical:
				case StandardCharacter.BookOrChapter:
					return true;
				default: return false;
			}
		}

		public static StandardCharacter GetStandardCharacterType(string characterId)
		{
			if (String.IsNullOrEmpty(characterId))
				return StandardCharacter.NonStandard;

			int i = characterId.IndexOf("-", StringComparison.Ordinal);
			if (i < 0)
				return StandardCharacter.NonStandard;
			switch (characterId.Substring(0, i + 1))
			{
				case kNarratorPrefix: return StandardCharacter.Narrator;
				case kIntroPrefix: return StandardCharacter.Intro;
				case kExtraBiblicalPrefix: return StandardCharacter.ExtraBiblical;
				case kBookOrChapterPrefix: return StandardCharacter.BookOrChapter;
			}
			return StandardCharacter.NonStandard;
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
				if (line.Length == 0 || line[0] == '#')
					continue;
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

			if (items.Length < kiIsDialogue)
				throw new ApplicationException("Bad format in CharacterVerseDataBase! Line #: " + lineNumber + "; Line contents: " + items.Concat("\t"));
			Debug.Assert(items.Length <= kMaxItems);

			int chapter = Int32.Parse(items[1]);
			for (int verse = ScrReference.VerseToIntStart(items[2]); verse <= ScrReference.VerseToIntEnd(items[2]); verse++)
				list.Add(CreateCharacterVerse(new BCVRef(BCVRef.BookToNumber(items[0]), chapter, verse), items));

			return list;
		}

		protected abstract CharacterVerse CreateCharacterVerse(BCVRef bcvRef, string[] items);
	}
}
