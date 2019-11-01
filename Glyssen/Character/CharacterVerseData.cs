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
using static System.Int32;
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

		protected readonly CharacterDeliveryEqualityComparer m_characterDeliveryEqualityComparer = new CharacterDeliveryEqualityComparer();
		private readonly IEqualityComparer<ICharacterDeliveryInfo> m_characterDeliveryAliasEqualityComparer = new CharacterDeliveryAliasEqualityComparer();
		private ISet<CharacterVerse> m_data = new HashSet<CharacterVerse>();
		private ILookup<int, CharacterVerse> m_lookupByRef;
		private ILookup<int, CharacterVerse> m_lookupByBookNum;
		private IReadOnlySet<ICharacterDeliveryInfo> m_uniqueCharacterDeliverAliasEntries;
		private ISet<string> m_uniqueDeliveries;

		public abstract HashSet<CharacterSpeakingMode> GetCharacters(int bookId, int chapter, IVerse verseOrBridge,
			ScrVers versification = null, bool includeAlternatesAndRareQuotes = false, bool includeNarratorOverrides = false);

		public abstract HashSet<CharacterSpeakingMode> GetCharacters(int bookId, int chapter, IReadOnlyCollection<IVerse> verses,
			ScrVers versification = null, bool includeAlternatesAndRareQuotes = false, bool includeNarratorOverrides = false);

		protected List<CharacterSpeakingMode> GetSpeakingModesForRef(VerseRef verseRef)
		{
			return GetEntriesForRef(verseRef.BBBCCCVVV).Cast<CharacterSpeakingMode>().ToList();
		}

		protected IEnumerable<CharacterVerse> GetEntriesForRef(int BbbCcVvv)
		{
			return m_lookupByRef[BbbCcVvv];
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
			var implicitCv = m_lookupByRef[verseRef.BBBCCCVVV].SingleOrDefault(cv => cv.IsImplicit);

			if (endVerse == 0 || startVerse == endVerse || implicitCv == null)
				return implicitCv;

			var initialEndRef = new VerseRef(bookId, chapter, endVerse, versification);
			initialEndRef.ChangeVersification(ScrVers.English);
			do
			{
				var cvNextVerse = m_lookupByRef[verseRef.BBBCCCVVV].SingleOrDefault(cv => cv.IsImplicit);
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

		public IEnumerable<CharacterVerse> GetAllQuoteInfo(int bookNum)
		{
			return m_lookupByBookNum[bookNum];
		}

		protected virtual bool AddCharacterVerse(CharacterVerse cv)
		{
			if (m_data.Add(cv))
			{
				ResetCaches();
				return true;
			}

			return false;
		}

		public bool Any()
		{
			return m_data.Any();
		}

		public IReadOnlySet<ICharacterDeliveryInfo> GetUniqueCharacterDeliveryAliasInfo()
		{
			return m_uniqueCharacterDeliverAliasEntries ??
				(m_uniqueCharacterDeliverAliasEntries = new ReadOnlySet<ICharacterDeliveryInfo>(GetUniqueCharacterDeliveryAliasSet()));
		}

		protected virtual HashSet<ICharacterDeliveryInfo> GetUniqueCharacterDeliveryAliasSet()
		{
			return new HashSet<ICharacterDeliveryInfo>(m_data, m_characterDeliveryAliasEqualityComparer);
		}

		public virtual ISet<ICharacterDeliveryInfo> GetUniqueCharacterDeliveryInfo(string bookCode)
		{
			return new HashSet<ICharacterDeliveryInfo>(m_data.Where(cv => cv.BookCode == bookCode), m_characterDeliveryEqualityComparer);
		}

		public ISet<string> GetUniqueDeliveries()
		{
			return m_uniqueDeliveries ?? (m_uniqueDeliveries = new SortedSet<string>(m_data.Select(cv => cv.Delivery).Where(d => !IsNullOrEmpty(d))));
		}

		protected bool Remove(int bookNum, int chapterNumber, int initialStartVerseNumber, string characterId, string delivery)
		{
			bool removed = false;
			var cvToDelete = m_lookupByRef[new BCVRef(bookNum, chapterNumber, initialStartVerseNumber).BBCCCVVV]
				.SingleOrDefault(cv => cv.Character == characterId && cv.Delivery == delivery);
			if (cvToDelete != null)
				removed = m_data.Remove(cvToDelete);

			if (removed)
				ResetCaches();
			return removed;
		}

		protected virtual void RemoveAll(IEnumerable<CharacterVerse> cvsToRemove, IEqualityComparer<CharacterVerse> comparer)
		{
			var intersection = m_data.Intersect(cvsToRemove, comparer).ToList();
			if (intersection.Any())
			{
				foreach (CharacterVerse cv in intersection)
					m_data.Remove(cv);

				ResetCaches();
			}
		}

		private void ResetCaches()
		{
			m_lookupByBookNum = m_data.ToLookup(c => c.Book);
			AdjustData(m_lookupByBookNum);
			m_lookupByRef = m_data.ToLookup(c => c.BcvRef.BBCCCVVV);
			m_uniqueCharacterDeliverAliasEntries = null;
			m_uniqueDeliveries = null;
		}

		protected virtual void AdjustData(ILookup<int, CharacterVerse> data)
		{
			// base implementation is a no-op;
		}

		protected void LoadData(string tabDelimitedCharacterVerseData)
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
			if (items.Length < kiQuoteType)
				throw new ApplicationException($"Bad format in CharacterVerse control file! Line #: {lineNumber}; Line contents: {string.Join("\t", items)}");
			if (items.Length > kMaxItems)
				throw new ApplicationException($"Incorrect number of fields in CharacterVerse control file! Line #: {lineNumber}; Line contents: {string.Join("\t", items)}");

			return GetAllVerses(items, () => throw new ApplicationException($"Invalid chapter number ({items[1]}) on line {lineNumber}: {items[0]}"))
				.Select(bcvRef => CreateCharacterVerse(bcvRef, items)).ToList();
		}

		internal static IEnumerable<BCVRef> GetAllVerses(string[] items, Action chapterNumberErrorHandler)
		{
			if (!TryParse(items[1], out var chapter))
				chapterNumberErrorHandler();

			for (int verse = BCVRef.VerseToIntStart(items[2]); verse <= BCVRef.VerseToIntEnd(items[2]); verse++)
				yield return new BCVRef(BCVRef.BookToNumber(items[0]), chapter, verse);
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
