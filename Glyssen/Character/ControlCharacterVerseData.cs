using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Glyssen.Properties;
using L10NSharp.UI;
using SIL.Scripture;

namespace Glyssen.Character
{
	public class ControlCharacterVerseData : CharacterVerseData
	{
		//// This is only here (for the moment) for ControlDataIntegrityTests to be able to validate the actual
		//// control file independent of how we want to read out the data.
		//public static bool ReadHypotheticalAsNarrator
		//{
		//	get => s_readHypotheticalAsNarrator;
			//set
			//{
			//	if (value == s_readHypotheticalAsNarrator)
			//		return;

			//	if (Program.IsRunning)
			//		throw new InvalidOperationException();
			//	s_readHypotheticalAsNarrator = value;
			//	s_singleton = null;
			//}
		//}

		//private static bool s_readHypotheticalAsNarrator = true;

		private static ControlCharacterVerseData s_singleton;
		private static string s_tabDelimitedCharacterVerseData;
		private Dictionary<int, Dictionary<int, HashSet<int>>> m_expectedQuotes;

		internal static string TabDelimitedCharacterVerseData
		{
			get { return s_tabDelimitedCharacterVerseData; }
			set
			{
				s_tabDelimitedCharacterVerseData = value;
				s_singleton = null;
			}
		}

		private ControlCharacterVerseData()
		{
			// Tests can set this before accessing the Singleton.
			if (TabDelimitedCharacterVerseData == null)
				TabDelimitedCharacterVerseData = Resources.CharacterVerseData;
			LoadAll();
			LocalizeItemDlg.StringsLocalized += HandleStringsLocalized; // Don't need to unsubscribe since this object will be around as long as the program is running.
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

		protected override IList<CharacterVerse> ProcessLine(string[] items, int lineNumber)
		{
			if (lineNumber == 0)
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
				if (value != "FALSE" && value != String.Empty && !Enum.TryParse(value, out quoteType))
				{
					throw new InvalidDataException(
						$"items[{kiQuoteType}] has a value of {value}, which is not a valid {typeof(QuoteType).Name}");
				}
			}

			var characterId = items[3];
			var delivery = items[4];
			var alias = items[5];
			var defaultCharacter = (items.Length > kiDefaultCharacter) ? items[kiDefaultCharacter] : null;
			var parallelPassageInfo = (items.Length > kiParallelPassageInfo) ? items[kiParallelPassageInfo] : null;

			//if (ReadHypotheticalAsNarrator && quoteType == QuoteType.Hypothetical)
			//{
			//	characterId = GetStandardCharacterId(BCVRef.NumberToBookCode(bcvRef.Book), StandardCharacter.Narrator);
			//	delivery = string.Empty;
			//	alias = string.Empty;
			//	quoteType = QuoteType.Quotation;
			//	defaultCharacter = string.Empty;
			//	parallelPassageInfo = string.Empty;
			//}
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
	}
}
