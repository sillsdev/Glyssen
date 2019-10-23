using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Glyssen.Properties;
using L10NSharp.TMXUtils;
using L10NSharp.UI;
using SIL.Extensions;
using SIL.Scripture;
using static System.String;

namespace Glyssen.Character
{
	public class ControlCharacterVerseData : CharacterVerseData
	{
		private static ControlCharacterVerseData s_singleton;
		private static string s_tabDelimitedCharacterVerseData;
		private Dictionary<int, Dictionary<int, HashSet<int>>> m_expectedQuotes;

		internal static string TabDelimitedCharacterVerseData
		{
			get { return s_tabDelimitedCharacterVerseData; }
			set
			{
				s_tabDelimitedCharacterVerseData = value;
				s_singleton?.Dispose();
			}
		}

		private ControlCharacterVerseData()
		{
			// Tests can set this before accessing the Singleton.
			if (TabDelimitedCharacterVerseData == null)
				TabDelimitedCharacterVerseData = Resources.CharacterVerseData;
			LoadAll();
			LocalizeItemDlg<TMXDocument>.StringsLocalized += HandleStringsLocalized; // Don't need to unsubscribe since this object will be around as long as the program is running.
		}

		private void Dispose()
		{
			LocalizeItemDlg<TMXDocument>.StringsLocalized -= HandleStringsLocalized;
			s_singleton = null;
		}

		public static ControlCharacterVerseData Singleton
		{
			get { return s_singleton ?? (s_singleton = new ControlCharacterVerseData()); }
		}

		public Dictionary<int, Dictionary<int, HashSet<int>>> ExpectedQuotes
		{
			get
			{
				if (m_expectedQuotes == null)
					InitializeExpectedQuotes();
				return m_expectedQuotes;
			}
		}

		public int ControlFileVersion { get; private set; }

		private void LoadAll()
		{
			if (Any())
				return;

			LoadData(TabDelimitedCharacterVerseData);

			if (!Any())
				throw new ApplicationException("No character verse data available!");
		}

		protected override void AddCharacterVerse(CharacterVerse cv)
		{
			throw new ApplicationException("The control file cannot be modified programmatically.");
		}

		protected override void RemoveAll(IEnumerable<CharacterVerse> cvsToRemove, IEqualityComparer<CharacterVerse> comparer)
		{
			throw new ApplicationException("The control file cannot be modified programmatically.");
		}

		protected override HashSet<ICharacterDeliveryInfo> GetUniqueCharacterDeliveryAliasSet()
		{
			var set = base.GetUniqueCharacterDeliveryAliasSet();
			set.AddRange(NarratorOverrides.Singleton.Books.SelectMany(b => b.Overrides).Select(o => o.Character)
				.Distinct().Select(c => new NarratorOverrideCharacter(c)));
			return set;
		}

		public override ISet<ICharacterDeliveryInfo> GetUniqueCharacterDeliveryInfo(string bookCode)
		{
			var set = base.GetUniqueCharacterDeliveryInfo(bookCode);
			set.AddRange(NarratorOverrides.GetNarratorOverridesForBook(bookCode).Select(o => o.Character)
				.Distinct().Select(c => new NarratorOverrideCharacter(c)));
			return set;
		}

		protected override IList<CharacterVerse> ProcessLine(string[] items, int lineNumber)
		{
			if (lineNumber == 1) // Using 1-based index because this is reported in error messages
			{
				ProcessFirstLine(items);
				return null;
			}
			return base.ProcessLine(items, lineNumber);
		}

		private void ProcessFirstLine(string[] items)
		{
			int cfv;
			if (Int32.TryParse(items[1], out cfv) && items[0].StartsWith("Control File"))
				ControlFileVersion = cfv;
			else
				throw new ApplicationException("Bad format in CharacterVerseDataBase metadata: " + items);
		}

		protected override CharacterVerse CreateCharacterVerse(BCVRef bcvRef, string[] items)
		{
			QuoteType quoteType = QuoteType.Normal;
			if (items.Length > kiQuoteType)
			{
				var value = items[kiQuoteType];
				if (value != "FALSE" && value != Empty && !Enum.TryParse(value, out quoteType))
				{
					throw new InvalidDataException(
						$"{items[0]} {items[1]}:{items[2]}, {items[3]} - items[{kiQuoteType}] has a value of {value}, which is not a valid {typeof(QuoteType).Name}.");
				}
			}

			var characterId = items[3];
			var delivery = items[4];
			var alias = items[5];
			var defaultCharacter = (items.Length > kiDefaultCharacter) ? items[kiDefaultCharacter] : null;
			var parallelPassageInfo = (items.Length > kiParallelPassageInfo) ? items[kiParallelPassageInfo] : null;

			return new CharacterVerse(bcvRef, characterId, delivery, alias, false, quoteType, defaultCharacter, parallelPassageInfo);
		}

		private void InitializeExpectedQuotes()
		{
			m_expectedQuotes = new Dictionary<int, Dictionary<int, HashSet<int>>>(66);

			foreach (var expectedQuote in Singleton.GetAllQuoteInfo().Where(c => c.IsExpected))
			{
				Dictionary<int, HashSet<int>> expectedQuotesInBook;
				if (!m_expectedQuotes.TryGetValue(expectedQuote.Book, out expectedQuotesInBook))
					m_expectedQuotes.Add(expectedQuote.Book, expectedQuotesInBook = new Dictionary<int, HashSet<int>>());

				HashSet<int> versesWithExpectedQuotesInChapter;
				if (!expectedQuotesInBook.TryGetValue(expectedQuote.Chapter, out versesWithExpectedQuotesInChapter))
					expectedQuotesInBook.Add(expectedQuote.Chapter, versesWithExpectedQuotesInChapter = new HashSet<int>());

				versesWithExpectedQuotesInChapter.Add(expectedQuote.Verse);
			}
		}

		protected override void AdjustData(ILookup<int, CharacterVerse> bookLookup)
		{
			for (var b = 1; b <= 66; b++)
			{
				var book = bookLookup[b];
				var narrator = GetStandardCharacterId(BCVRef.NumberToBookCode(b), StandardCharacter.Narrator);
				for (int i = 0; i < book.Count(); i++)
				{
					var cv = book.ElementAt(i);
					if (cv.QuoteType == QuoteType.Quotation && cv.DefaultCharacter == narrator)
					{
						// PG-1248: cv is a Quotation entry which, if used, would have the quote spoken by the original
						// speaker. (Even though the default character is the narrator, we ignore that.) if there is a
						// narrator Quotation entry for this same verse, we want it to be considered as the primary
						// option. We'll mark the cv we've found as an Alternate, so the quote parser will not
						// consider it as an option, but the user will still be able to use it to override the default.

						// Following line should be SingleOrDefault, but this is more efficient
						var quotationByNarrator = book.FirstOrDefault(a => a.Book == cv.Book &&
							a.Chapter == cv.Chapter &&
							a.Verse == cv.Verse &&
							a.QuoteType == QuoteType.Quotation &&
							a.Character == narrator);
						if (quotationByNarrator != null)
							cv.ChangeToAlternate();
					}
				}
			}
		}
	}
}
