using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using L10NSharp;
using Paratext;
using SIL.Scripture;
using ScrVers = Paratext.ScrVers;

namespace Glyssen.Character
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

		public const int kiMinRequiredFields = 5;
		protected const int kiQuoteType = kiMinRequiredFields + 1;
		protected const int kiDefaultCharacter = kiQuoteType + 1;
		protected const int kiParallelPassageInfo = kiDefaultCharacter + 1;
		protected const int kMaxItems = kiParallelPassageInfo + 1;

		private static Dictionary<string, string> m_singletonLocalizedCharacterIdToCharacterIdDictionary; 

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

		public static string GetCharacterNameForUi(string characterId)
		{
			string localizedCharacterId;

			switch (GetStandardCharacterType(characterId))
			{
				case StandardCharacter.Narrator: 
					localizedCharacterId = String.Format(LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.Narrator", kNarratorAsEnglishCharacterName), GetBookNameFromStandardCharacterId(characterId));
					break;
				case StandardCharacter.Intro:
					localizedCharacterId = String.Format(LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.IntroCharacter", kIntroductionAsEnglishCharacterName), GetBookNameFromStandardCharacterId(characterId));
					break;
				case StandardCharacter.ExtraBiblical:
					localizedCharacterId = String.Format(LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.ExtraCharacter", kSectionHeadAsEnglishCharacterName), GetBookNameFromStandardCharacterId(characterId));
					break;
				case StandardCharacter.BookOrChapter:
					localizedCharacterId = String.Format(LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.BookChapterCharacter", kBookChapterAsEnglishCharacterName), GetBookNameFromStandardCharacterId(characterId));
					break;
				default:
					localizedCharacterId = LocalizationManager.GetDynamicString(Program.kApplicationId, "CharacterName." + characterId, characterId);
					break;
			}
			if (!SingletonLocalizedCharacterIdToCharacterIdDictionary.ContainsKey(localizedCharacterId))
				SingletonLocalizedCharacterIdToCharacterIdDictionary.Add(localizedCharacterId, characterId);

			return localizedCharacterId;
		}

		public static string GetStandardCharacterIdAsEnglish(string standardCharacterId)
		{
			switch (GetStandardCharacterType(standardCharacterId))
			{
				case StandardCharacter.Narrator:
					return String.Format(kNarratorAsEnglishCharacterName, GetBookNameFromStandardCharacterId(standardCharacterId));
				case StandardCharacter.Intro:
					return String.Format(kIntroductionAsEnglishCharacterName, GetBookNameFromStandardCharacterId(standardCharacterId));
				case StandardCharacter.ExtraBiblical:
					return String.Format(kSectionHeadAsEnglishCharacterName, GetBookNameFromStandardCharacterId(standardCharacterId));
				case StandardCharacter.BookOrChapter:
					return String.Format(kBookChapterAsEnglishCharacterName, GetBookNameFromStandardCharacterId(standardCharacterId));
				default:
					throw new ArgumentException("The provided character ID is not a standard character.", "standardCharacterId");
			}
		}

		public static Dictionary<string, string> SingletonLocalizedCharacterIdToCharacterIdDictionary
		{
			get
			{
				return
					m_singletonLocalizedCharacterIdToCharacterIdDictionary ?? (m_singletonLocalizedCharacterIdToCharacterIdDictionary =
						new Dictionary<string, string>());
			}
		}

		public static string GetBookNameFromStandardCharacterId(string characterId)
		{
			return characterId.Substring(characterId.Length - 3);
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

		private const string kNarratorAsEnglishCharacterName = "narrator ({0})";
		private const string kIntroductionAsEnglishCharacterName = "introduction ({0})";
		private const string kSectionHeadAsEnglishCharacterName = "section head ({0})";
		private const string kBookChapterAsEnglishCharacterName = "book title or chapter ({0})";

		private readonly CharacterDeliveryEqualityComparer m_characterDeliveryEqualityComparer = new CharacterDeliveryEqualityComparer();
		private IList<CharacterVerse> m_data = new List<CharacterVerse>();
		private ILookup<int, CharacterVerse> m_lookup;
		private IEnumerable<CharacterVerse> m_uniqueCharacterAndDeliveries;
		private IEnumerable<string> m_uniqueDeliveries;

		/// <summary>
		/// Prefer the int bookId counterpart method for performance reasons (this method has to perform a book Id lookup)
		/// </summary>
		public IEnumerable<CharacterVerse> GetCharacters(string bookId, int chapter, int initialStartVerse, int initialEndVerse = 0, int finalVerse = 0, ScrVers versification = null)
		{
			return GetCharacters(BCVRef.BookToNumber(bookId), chapter, initialStartVerse, initialEndVerse, finalVerse, versification);
		}

		/// <summary>
		/// This method is preferred over the string bookId counterpart for performance reasons (so we don't have to look up the book number)
		/// </summary>
		public IEnumerable<CharacterVerse> GetCharacters(int bookId, int chapter, int initialStartVerse, int initialEndVerse = 0, int finalVerse = 0, ScrVers versification = null)
		{
			if (versification == null)
				versification = ScrVers.English;

			IEnumerable<CharacterVerse> result;

			if (initialEndVerse == 0 || initialStartVerse == initialEndVerse)
			{
				var verseRef = new VerseRef(bookId, chapter, initialStartVerse, versification);
				verseRef.ChangeVersification(ScrVers.English);
				result = m_lookup[verseRef.BBBCCCVVV];
			}
			else
			{
				int start = new BCVRef(bookId, chapter, initialStartVerse).BBCCCVVV;
				int end = new BCVRef(bookId, chapter, initialEndVerse).BBCCCVVV;
				result = Enumerable.Empty<CharacterVerse>();
				for (int i = start; i <= end; i++)
					result = result.Union(m_lookup[i]);
			}
			if (finalVerse == 0 || result.Count() == 1)
				return result;

			var nextVerse = Math.Max(initialStartVerse, initialEndVerse) + 1;
			while (nextVerse <= finalVerse)
			{
				var verseRef = new VerseRef(bookId, chapter, nextVerse, versification);
				verseRef.ChangeVersification(ScrVers.English);
				IEnumerable<CharacterVerse> nextResult = m_lookup[verseRef.BBBCCCVVV];
				if (!nextResult.Any())
				{
					nextVerse++;
					continue;
				}
				if (!result.Any())
				{
					result = nextResult;
					nextVerse++;
					continue;	
				}
				var intersection = nextResult.Intersect(result, m_characterDeliveryEqualityComparer);
				if (intersection.Count() == 1)
				{
					result = intersection;
					break;
				}
				nextVerse++;
			}
			return result;
		}

		public IEnumerable<CharacterVerse> GetAllQuoteInfo()
		{
			return m_data;
		}

		public IEnumerable<CharacterVerse> GetAllQuoteInfo(string bookCode)
		{
			return m_data.Where(cv => cv.BookCode == bookCode);
		}

		protected virtual void AddCharacterVerse(CharacterVerse cv)
		{
			m_data.Add(cv);
			m_lookup = m_data.ToLookup(c => c.BcvRef.BBCCCVVV);
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

		protected virtual void RemoveAll(IEnumerable<CharacterVerse> cvsToRemove, IEqualityComparer<CharacterVerse> comparer)
		{
			var intersection = m_data.Intersect(cvsToRemove, comparer).ToList();
			foreach (CharacterVerse cv in intersection)
				m_data.Remove(cv);

			m_lookup = m_data.ToLookup(c => c.BcvRef.BBCCCVVV);
			m_uniqueCharacterAndDeliveries = null;
			m_uniqueDeliveries = null;
		}

		public void LoadData(string tabDelimitedCharacterVerseData)
		{
			var data = new List<CharacterVerse>();
			int lineNumber = 0;
			foreach (var line in tabDelimitedCharacterVerseData.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
			{
				if (line.Length == 0 || line[0] == '#')
					continue;
				string[] items = line.Split(new[] { "\t" }, StringSplitOptions.None);
				IList<CharacterVerse> cvs = ProcessLine(items, lineNumber++);
				if (cvs != null)
					data.AddRange(cvs);
			}
			m_data = data;
			m_lookup = data.ToLookup(c => c.BcvRef.BBCCCVVV);
			m_uniqueCharacterAndDeliveries = null;
			m_uniqueDeliveries = null;
		}

		protected virtual IList<CharacterVerse> ProcessLine(string[] items, int lineNumber)
		{
			var list = new List<CharacterVerse>();

			if (items.Length < kiQuoteType)
				throw new ApplicationException("Bad format in CharacterVerse control file! Line #: " + lineNumber + "; Line contents: " + string.Join("\t", items));
			if (items.Length > kMaxItems)
				throw new ApplicationException("Incorrect number of fields in CharacterVerse control file! Line #: " + lineNumber + "; Line contents: " + string.Join("\t", items));

			int chapter;
			if (!Int32.TryParse(items[1], out chapter))
				Debug.Assert(false, string.Format("Invalid chapter number ({0}) on line {1}: {2}", items[1], lineNumber, items[0]));
			for (int verse = ScrReference.VerseToIntStart(items[2]); verse <= ScrReference.VerseToIntEnd(items[2]); verse++)
				list.Add(CreateCharacterVerse(new BCVRef(BCVRef.BookToNumber(items[0]), chapter, verse), items));

			return list;
		}

		protected abstract CharacterVerse CreateCharacterVerse(BCVRef bcvRef, string[] items);

		public void HandleStringsLocalized()
		{
			foreach (CharacterVerse cv in GetAllQuoteInfo())
				cv.ResetLocalization();
		}
	}
}
