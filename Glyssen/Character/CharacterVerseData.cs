using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Glyssen.Shared;
using L10NSharp;
using SIL.Extensions;
using SIL.Scripture;
using static System.String;

namespace Glyssen.Character
{
	public abstract class CharacterVerseData : ICharacterVerseInfo
	{
		/// <summary>Represents a quote whose character has not been set because not quote was expected at this location</summary>
		public const string kUnexpectedCharacter = "Unknown";
		/// <summary>Represents a quote where the user needs to disambiguate between multiple potential characters</summary>
		public const string kAmbiguousCharacter = "Ambiguous";
		/// <summary>Special character ID that the user can use for a quote that needs further review (by a vernacular speaker, advisor, etc.)</summary>
		public const string kNeedsReview = "Needs Review";

		public const int kiMinRequiredFields = 5;
		protected const int kiQuoteType = kiMinRequiredFields + 1;
		protected const int kiDefaultCharacter = kiQuoteType + 1;
		protected const int kiParallelPassageInfo = kiDefaultCharacter + 1;
		protected const int kMaxItems = kiParallelPassageInfo + 1;

		private static Dictionary<string, string> s_singletonLocalizedCharacterIdToCharacterIdDictionary;

		public enum StandardCharacter
		{
			NonStandard,
			Narrator,
			BookOrChapter,
			ExtraBiblical,
			Intro,
		}

		public static StandardCharacter GetStandardCharacterType(string characterId)
		{
			if (IsNullOrEmpty(characterId))
				return StandardCharacter.NonStandard;

			var i = characterId.IndexOf("-", StringComparison.Ordinal);
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

		public static bool IsCharacterUnclear(string characterId)
		{
			return characterId == kAmbiguousCharacter || characterId == kUnexpectedCharacter;
		}

		public static string GetStandardCharacterId(string bookId, StandardCharacter standardCharacterType)
		{
			return GetCharacterPrefix(standardCharacterType) + bookId;
		}

		public static bool IsCharacterOfType(string characterId, StandardCharacter standardCharacterType)
		{
			return characterId.StartsWith(GetCharacterPrefix(standardCharacterType), StringComparison.Ordinal);
		}

		public static bool TryGetBookIdFromNarratorCharacterId(string characterId, out string bookId)
		{
			var match = s_narratorRegex.Match(characterId);
			if (!match.Success)
			{
				bookId = null;
				return false;
			}
			bookId = match.Result("${bookId}");
			return true;
		}

		public static bool IsCharacterStandard(string characterId)
		{
			if (characterId == null)
				return false;

			return IsCharacterOfType(characterId, StandardCharacter.Narrator) ||
				IsCharacterOfType(characterId, StandardCharacter.BookOrChapter) ||
				IsCharacterOfType(characterId, StandardCharacter.ExtraBiblical) ||
				IsCharacterOfType(characterId, StandardCharacter.Intro);
			// We could call IsCharacterExtraBiblical instead of the last three lines of this if,
			// but this is speed-critical code and the overhead of the extra method call is
			// expensive.
		}

		public static bool IsCharacterExtraBiblical(string characterId)
		{
			if (characterId == null)
				return false;

			return IsCharacterOfType(characterId, StandardCharacter.BookOrChapter) ||
				IsCharacterOfType(characterId, StandardCharacter.ExtraBiblical) ||
				IsCharacterOfType(characterId, StandardCharacter.Intro);
		}

		public static string StandardCharacterNameFormatNarrator = LocalizationManager.GetString("CharacterName.Standard.Fmt.Narrator", "narrator ({0})");
		public static string StandardCharacterNameFormatIntroduction = LocalizationManager.GetString("CharacterName.Standard.Fmt.Introduction", "introduction ({0})");
		public static string StandardCharacterNameFormatSectionHead = LocalizationManager.GetString("CharacterName.Standard.Fmt.SectionHead", "section head ({0})");
		public static string StandardCharacterNameFormatBookOrChapter = LocalizationManager.GetString("CharacterName.Standard.Fmt.BookOrChapter", "book title or chapter ({0})");

		public static string GetCharacterNameForUi(string characterId)
		{
			var standardCharacterType = GetStandardCharacterType(characterId);
			string localizedCharacterId = standardCharacterType == StandardCharacter.NonStandard ?
				LocalizationManager.GetDynamicString(GlyssenInfo.kApplicationId, "CharacterName." + characterId, characterId) :
				GetStandardCharacterNameForUi(standardCharacterType, GetBookCodeFromStandardCharacterId(characterId));
			SingletonLocalizedCharacterIdToCharacterIdDictionary[localizedCharacterId] = characterId;

			return localizedCharacterId;
		}

		private static string GetStandardCharacterNameFormatForUi(StandardCharacter standardCharacter)
		{
			switch (standardCharacter)
			{
				case StandardCharacter.Narrator: return StandardCharacterNameFormatNarrator;
				case StandardCharacter.Intro: return StandardCharacterNameFormatIntroduction;
				case StandardCharacter.ExtraBiblical: return StandardCharacterNameFormatSectionHead;
				case StandardCharacter.BookOrChapter: return StandardCharacterNameFormatBookOrChapter;
				default:
					throw new InvalidEnumArgumentException($"{nameof(standardCharacter)} must be a standard character type!");
			}
		}

		public static string GetStandardCharacterNameForUi(StandardCharacter standardCharacter, string bookId) =>
			Format(GetStandardCharacterNameFormatForUi(standardCharacter), bookId);

		public static Dictionary<string, string> SingletonLocalizedCharacterIdToCharacterIdDictionary
		{
			get
			{
				return
					s_singletonLocalizedCharacterIdToCharacterIdDictionary ?? (s_singletonLocalizedCharacterIdToCharacterIdDictionary =
						new Dictionary<string, string>());
			}
		}

		public static string GetBookCodeFromStandardCharacterId(string characterId)
		{
			return characterId.Substring(characterId.Length - 3);
		}

		internal static string GetCharacterPrefix(StandardCharacter standardCharacterType)
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

		/// <summary>Character ID prefix for material to be read by narrator (not for UI)</summary>
		protected const string kNarratorPrefix = "narrator-";
		/// <summary>Character ID prefix for book titles or chapter breaks (not for UI)</summary>
		protected const string kBookOrChapterPrefix = "BC-";
		/// <summary>Character ID prefix for extra-biblical material (i.e., section heads) (not for UI)</summary>
		protected const string kExtraBiblicalPrefix = "extra-";
		/// <summary>Character ID prefix for intro material (not for UI)</summary>
		protected const string kIntroPrefix = "intro-";

		private static readonly Regex s_narratorRegex = new Regex($"{kNarratorPrefix}(?<bookId>...)");

		private readonly CharacterDeliveryEqualityComparer m_characterDeliveryEqualityComparer = new CharacterDeliveryEqualityComparer();
		private ISet<CharacterVerse> m_data = new HashSet<CharacterVerse>();
		private ILookup<int, CharacterVerse> m_lookup;
		private IEnumerable<CharacterVerse> m_uniqueCharacterAndDeliveries;
		private IEnumerable<string> m_uniqueDeliveries;

		public IEnumerable<CharacterVerse> GetCharacters(int bookId, int chapter, int initialStartVerse, int initialEndVerse = 0, int finalVerse = 0, ScrVers versification = null, bool includeAlternates = false)
		{
			if (versification == null)
				versification = ScrVers.English;

			IEnumerable<CharacterVerse> result;

			var verseRef = new VerseRef(bookId, chapter, initialStartVerse, versification);
			verseRef.ChangeVersification(ScrVers.English);

			if (initialEndVerse == 0 || initialStartVerse == initialEndVerse)
			{
				result = m_lookup[verseRef.BBBCCCVVV];
			}
			else
			{
				var initialEndRef = new VerseRef(bookId, chapter, initialEndVerse, versification);
				initialEndRef.ChangeVersification(ScrVers.English);
				result = Enumerable.Empty<CharacterVerse>();
				do
				{
					result = result.Union(m_lookup[verseRef.BBBCCCVVV]);
					verseRef.NextVerse();
					// ReSharper disable once LoopVariableIsNeverChangedInsideLoop - NextVerse changes verseRef
				} while (verseRef <= initialEndRef);
			}
			if (!includeAlternates)
				result = result.Where(cv => cv.QuoteType != QuoteType.Alternate);
			if (finalVerse == 0) // Because of the possibility of interruptions, we can't quit early when we're down to 1 character/delivery // || result.Count() == 1)
				return result;

			// This is a list (because that makes it easy to do a Union), but it should only ever have exactly one item in it.
			var interruption = result.Where(c => c.QuoteType == QuoteType.Interruption).ToList();

			var finalVerseRef = new VerseRef(bookId, chapter, finalVerse, versification);
			finalVerseRef.ChangeVersification(ScrVers.English);
			verseRef.NextVerse();
			while (verseRef <= finalVerseRef)
			{
				IEnumerable<CharacterVerse> nextResult = m_lookup[verseRef.BBBCCCVVV];
				if (nextResult.Any())
				{
					if (!interruption.Any())
						interruption = nextResult.Where(c => c.QuoteType == QuoteType.Interruption).ToList();

					if (!result.Any())
					{
						result = nextResult;
					}
					else
					{
						var intersection = nextResult.Intersect(result, m_characterDeliveryEqualityComparer);
						if (intersection.Count() == 1)
						{
							result = intersection;
							break;
						}
					}
				}
				verseRef.NextVerse();
			}
			return result.Union(interruption);
		}

		public virtual CharacterVerse GetImplicitCharacter(int bookId, int chapter, int startVerse, int endVerse = 0, ScrVers versification = null)
		{
			if (versification == null)
				versification = ScrVers.English;

			var startRef = new VerseRef(bookId, chapter, startVerse, versification);
			startRef.ChangeVersification(ScrVers.English);
			var start = startRef.BBBCCCVVV;
			var implicitCv = m_lookup[start].SingleOrDefault(cv => cv.QuoteType == QuoteType.Implicit);

			if (endVerse == 0 || startVerse == endVerse)
				return implicitCv;

			var endRef = new VerseRef(bookId, chapter, endVerse, versification);
			endRef.ChangeVersification(ScrVers.English);
			int end = endRef.BBBCCCVVV;
			for (int i = start; i <= end; i++)
			{
				var cvImplicit = m_lookup[i].SingleOrDefault(cv => cv.QuoteType == QuoteType.Implicit);
				if (cvImplicit != null)
					return cvImplicit;
			}
			return null;
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
			ResetCaches();
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
			return m_uniqueDeliveries ?? (m_uniqueDeliveries = new SortedSet<string>(m_data.Select(cv => cv.Delivery).Where(d => !IsNullOrEmpty(d))));
		}

		protected virtual void RemoveAll(IEnumerable<CharacterVerse> cvsToRemove, IEqualityComparer<CharacterVerse> comparer)
		{
			var intersection = m_data.Intersect(cvsToRemove, comparer).ToList();
			foreach (CharacterVerse cv in intersection)
				m_data.Remove(cv);

			ResetCaches();
		}

		private void ResetCaches()
		{
			m_lookup = m_data.ToLookup(c => c.BcvRef.BBCCCVVV);
			m_uniqueCharacterAndDeliveries = null;
			m_uniqueDeliveries = null;
		}

		public void LoadData(string tabDelimitedCharacterVerseData)
		{
			var data = new HashSet<CharacterVerse>();
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
			ResetCaches();
		}

		protected virtual IList<CharacterVerse> ProcessLine(string[] items, int lineNumber)
		{
			var list = new List<CharacterVerse>();

			if (items.Length < kiQuoteType)
				throw new ApplicationException("Bad format in CharacterVerse control file! Line #: " + lineNumber + "; Line contents: " + Join("\t", items));
			if (items.Length > kMaxItems)
				throw new ApplicationException("Incorrect number of fields in CharacterVerse control file! Line #: " + lineNumber + "; Line contents: " + Join("\t", items));

			int chapter;
			if (!Int32.TryParse(items[1], out chapter))
				Debug.Assert(false, Format("Invalid chapter number ({0}) on line {1}: {2}", items[1], lineNumber, items[0]));
			for (int verse = BCVRef.VerseToIntStart(items[2]); verse <= BCVRef.VerseToIntEnd(items[2]); verse++)
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
