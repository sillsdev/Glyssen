using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Glyssen.Shared;
using L10NSharp;
using SIL.Extensions;
using SIL.ObjectModel;
using SIL.Scripture;
using static System.String;

namespace Glyssen.Character
{
	public abstract class CharacterVerseData : ICharacterVerseInfo
	{
		/// <summary>Represents a quote whose character has not been set because no quote was expected at this location</summary>
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

		private readonly IEqualityComparer<ICharacterDeliveryInfo> m_characterDeliveryEqualityComparer = new CharacterDeliveryEqualityComparer();
		private ISet<CharacterVerse> m_data = new HashSet<CharacterVerse>();
		private ILookup<int, CharacterVerse> m_lookup;
		private IReadOnlySet<ICharacterDeliveryInfo> m_uniqueCharacterAndDeliveries;
		private ISet<string> m_uniqueDeliveries;

		public IEnumerable<CharacterVerse> GetCharacters(int bookId, int chapter, int initialStartVerse, int initialEndVerse = 0,
			int finalVerse = 0, ScrVers versification = null, bool includeAlternates = false, bool includeNarratorOverrides = false)
		{
			if (versification == null)
				versification = ScrVers.English;

			List<CharacterVerse> result;

			var verseRef = new VerseRef(bookId, chapter, initialStartVerse, versification);
			verseRef.ChangeVersification(ScrVers.English);

			if (initialEndVerse == 0)
				initialEndVerse = initialStartVerse;

			List<string> overrideCharacters = null;
			if (includeNarratorOverrides)
			{
				overrideCharacters = NarratorOverrides.GetCharacterOverrideDetailsForRefRange(verseRef,
					(finalVerse == 0 ? initialEndVerse : finalVerse))?.Select(o => o.Character).ToList();
				if (overrideCharacters != null && !overrideCharacters.Any())
					overrideCharacters = null;
			}

			if (initialStartVerse == initialEndVerse)
			{
				result = m_lookup[verseRef.BBBCCCVVV].ToList();
			}
			else
			{
				var initialEndRef = new VerseRef(bookId, chapter, initialEndVerse, versification);
				initialEndRef.ChangeVersification(ScrVers.English);
				result = new List<CharacterVerse>();
				do
				{
					result = result.Union(m_lookup[verseRef.BBBCCCVVV]).ToList();
					verseRef.NextVerse();
					// ReSharper disable once LoopVariableIsNeverChangedInsideLoop - NextVerse changes verseRef
				} while (verseRef <= initialEndRef);
			}
			if (!includeAlternates)
				result = result.Where(cv => cv.QuoteType != QuoteType.Alternate).ToList();
			if (overrideCharacters != null)
			{
				foreach (var character in overrideCharacters.Where(c => !result.Any(r => r.Character == c && r.Delivery == Empty)))
					result.Add(new CharacterVerse(new BCVRef(verseRef.BBBCCCVVV), character, Empty, null, false, QuoteType.Potential));
			}
			if (finalVerse == 0) // Because of the possibility of interruptions, we can't quit early when we're down to 1 character/delivery // || result.Count() == 1)
				return result;

			// This is a list (because that makes it easy to do a Union), but it should only ever have exactly one item in it.
			var interruption = result.Where(c => c.QuoteType == QuoteType.Interruption).ToList();

			var finalVerseRef = new VerseRef(bookId, chapter, finalVerse, versification);
			finalVerseRef.ChangeVersification(ScrVers.English);
			verseRef.NextVerse();
			// ReSharper disable once LoopVariableIsNeverChangedInsideLoop - NextVerse changes verseRef
			while (verseRef <= finalVerseRef)
			{
				var nextResult = m_lookup[verseRef.BBBCCCVVV].ToList();
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
						nextResult.RemoveAll(c => !result.Contains(c, m_characterDeliveryEqualityComparer));
						if (overrideCharacters != null && !nextResult.Any(cv => overrideCharacters.Contains(cv.Character) && cv.Delivery == Empty))
							nextResult.AddRange(result.Where(cv => overrideCharacters.Contains(cv.Character) && cv.Delivery == Empty));
						if (nextResult.Count == 1)
						{
							result = nextResult;
							break;
						}
					}
				}
				verseRef.NextVerse();
			}
			return result.Union(interruption);
		}

		/// <summary>
		/// Gets a single character/delivery object that represents the one known character expected to be the
		/// exclusive (implicit) speaker over the entire reference range represented by the given parameters.
		/// If there are conflicting implicit characters or an implicit character covers only part of the range,
		/// the returned object will be a "Needs Review" character.
		/// </summary>
		public virtual ICharacterDeliveryInfo GetImplicitCharacter(int bookId, int chapter, int startVerse, int endVerse = 0, ScrVers versification = null)
		{
			if (versification == null)
				versification = ScrVers.English;

			var verseRef = new VerseRef(bookId, chapter, startVerse, versification);
			verseRef.ChangeVersification(ScrVers.English);
			var implicitCv = m_lookup[verseRef.BBBCCCVVV].SingleOrDefault(cv => cv.QuoteType == QuoteType.Implicit);

			if (endVerse == 0 || startVerse == endVerse || implicitCv == null)
				return implicitCv;

			var initialEndRef = new VerseRef(bookId, chapter, endVerse, versification);
			initialEndRef.ChangeVersification(ScrVers.English);
			do
			{
				var cvNextVerse = m_lookup[verseRef.BBBCCCVVV].SingleOrDefault(cv => cv.QuoteType == QuoteType.Implicit);
				// Unless all verses in the range have the same implicit character, we cannot say that there is an
				// implicit character for this range. Note that there is the slight possibility that the delivery may vary
				// from one verse to the next, but it doesn't seem worth it to fail to find the implicit character just
				// because of that. Especially since the delivery info is only of minor usefulness.
				if (cvNextVerse?.Character != implicitCv.Character)
					return NeedsReviewCharacter.Singleton;
				verseRef.NextVerse();
				// ReSharper disable once LoopVariableIsNeverChangedInsideLoop - NextVerse changes verseRef
			} while (verseRef <= initialEndRef);
			return implicitCv;
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

		public IReadOnlySet<ICharacterDeliveryInfo> GetUniqueCharacterAndDeliveries()
		{
			if (m_uniqueCharacterAndDeliveries == null)
			{
				var set = new HashSet<ICharacterDeliveryInfo>(m_data, m_characterDeliveryEqualityComparer);
				set.AddRange(NarratorOverrides.Singleton.Books.SelectMany(b => b.Overrides).Select(o => o.Character)
					.Distinct().Select(c => new NarratorOverrideCharacter(c)));
				m_uniqueCharacterAndDeliveries = new ReadOnlySet<ICharacterDeliveryInfo>(set);
			}

			return m_uniqueCharacterAndDeliveries;
		}

		public ISet<ICharacterDeliveryInfo> GetUniqueCharacterAndDeliveries(string bookCode)
		{
			var set = new HashSet<ICharacterDeliveryInfo>(m_data.Where(cv => cv.BookCode == bookCode), m_characterDeliveryEqualityComparer);
			set.AddRange(NarratorOverrides.GetNarratorOverridesForBook(bookCode).Select(o => o.Character)
				.Distinct().Select(c => new NarratorOverrideCharacter(c)));
			return set;
		}

		public ISet<ICharacterDeliveryInfo> GetUniqueCharacterAndDeliveries(string bookCode, int chapter)
		{
			var set = new HashSet<ICharacterDeliveryInfo>(m_data.Where(cv => cv.BookCode == bookCode && cv.Chapter == chapter), m_characterDeliveryEqualityComparer);
			set.AddRange(NarratorOverrides.GetNarratorOverridesForBook(bookCode)
				.Where(o => o.StartChapter <= chapter && o.EndChapter >= chapter)
				.Select(o => o.Character).Distinct().Select(c => new NarratorOverrideCharacter(c)));
			return set;
		}

		public ISet<string> GetUniqueDeliveries()
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
			foreach (var line in tabDelimitedCharacterVerseData.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
				.Select((contents, number) => new { Contents = contents, Number = number + 1 }))
			{
				if (line.Contents.Length != 0 && line.Contents[0] != '#')
				{
					string[] items = line.Contents.Split(new[] {"\t"}, StringSplitOptions.None);
					IList<CharacterVerse> cvs = ProcessLine(items, line.Number);
					if (cvs != null)
						data.AddRange(cvs);
				}
			}
			m_data = data;
			ResetCaches();
		}

		/// <summary>
		/// Gets a list of <see cref="CharacterVerse"/> objects built from the details contained in
		/// <paramref name="items"/>. Typically the list will contain a single item, but if the
		/// verse reference represents a range of verses, then there will be one per verse.
		/// </summary>
		/// <param name="items">field values from the line (tab-delimited in file)</param>
		/// <param name="lineNumber">1-based line number (used only for error reporting)</param>
		/// <exception cref="ApplicationException">Bad data (incorrect number of fields, etc.)</exception>
		protected virtual IList<CharacterVerse> ProcessLine(string[] items, int lineNumber)
		{
			var list = new List<CharacterVerse>();

			if (items.Length < kiQuoteType)
				throw new ApplicationException($"Bad format in CharacterVerse control file! Line #: {lineNumber}; Line contents: {string.Join("\t", items)}");
			if (items.Length > kMaxItems)
				throw new ApplicationException($"Incorrect number of fields in CharacterVerse control file! Line #: {lineNumber}; Line contents: {string.Join("\t", items)}");

			int chapter;
			if (!Int32.TryParse(items[1], out chapter))
				throw new ApplicationException($"Invalid chapter number ({items[1]}) on line {lineNumber}: {items[0]}");
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

		public class NeedsReviewCharacter : ICharacterDeliveryInfo
		{
			public static NeedsReviewCharacter Singleton { get; }

			public string Character => kNeedsReview;
			public string LocalizedCharacter =>
				LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.CharacterNeedsReview", "Needs Review");
			public string Delivery => Empty;
			public string DefaultCharacter => null;
			public string Alias => null;
			public string LocalizedAlias => null;
			public bool ProjectSpecific => false;

			static NeedsReviewCharacter()
			{
				Singleton = new NeedsReviewCharacter();
			}

			private NeedsReviewCharacter()
			{
			}
		}
	}
}
