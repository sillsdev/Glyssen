using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Glyssen.Shared;
using SIL.Extensions;
using SIL.Scripture;
using static System.String;

namespace GlyssenEngine.Character
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
		}

		private void Dispose()
		{
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

			LoadData(TabDelimitedCharacterVerseData.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None));

			if (!Any())
				throw new ApplicationException("No character verse data available!");
		}

		// In a perfect world, this would be private, but there are a lot of unit tests that make use of it because it
		// is easier and cheaper than the public one. It assumes the chapter and verse passed in are in the English
		// versification.
		internal IEnumerable<ICharacterDeliveryInfo> GetCharacters(int bookId, int chapter, int verse)
		{
			return GetEntriesForRef(new BCVRef(bookId, chapter, verse));
		}

		/// <summary>
		/// Gets all character/delivery pairs for the given verse or bridge.
		/// </summary>
		public override HashSet<CharacterSpeakingMode> GetCharacters(int bookId, int chapter, IVerse verseOrBridge,
			ScrVers versification = null, bool includeAlternatesAndRareQuotes = false, bool includeNarratorOverrides = false)
		{
			return GetCharacters(bookId, chapter, new[] {verseOrBridge}, versification, includeAlternatesAndRareQuotes, includeNarratorOverrides);
		}

		/// <summary>
		/// Gets all characters completely covered by the given range of verses. If there are multiple verses, only
		/// characters known to speak in ALL the verses will be included in the returned set, with the exception of
		/// Interruptions, which will be included if they occur in any verse. Returned items will include the accompanying
		/// deliveries if the deliveries are consistent across all verses.
		/// </summary>
		public override HashSet<CharacterSpeakingMode> GetCharacters(int bookId, int chapter, IReadOnlyCollection<IVerse> verses,
			ScrVers versification = null, bool includeAlternatesAndRareQuotes = false, bool includeNarratorOverrides = false)
		{
			if (versification == null)
				versification = ScrVers.English;

			List<string> overrideCharacters = null;
			CharacterSpeakingMode interruption = null;
			HashSet<CharacterSpeakingMode> result = null;

			foreach (var verse in verses)
			{
				var entriesForCurrentVerseBridge = new HashSet<CharacterSpeakingMode>();
				foreach (var v in verse.AllVerseNumbers)
				{
					var verseRef = new VerseRef(bookId, chapter, v, versification);

					if (includeNarratorOverrides)
					{
						overrideCharacters = NarratorOverrides.GetCharacterOverrideDetailsForRefRange(verseRef,
							verses.Last().EndVerse)?.Select(o => o.Character).ToList();
						if (overrideCharacters != null && !overrideCharacters.Any())
							overrideCharacters = null;
						includeNarratorOverrides = false; // Don't need to get them again
					}

					verseRef.ChangeVersification(ScrVers.English);
					foreach (var cv in GetSpeakingModesForRef(verseRef))
					{
						if (cv.QuoteType == QuoteType.Interruption)
						{
							if (interruption == null)
								interruption = cv;
							continue;
						}
						var match = entriesForCurrentVerseBridge.FirstOrDefault(e => m_characterDeliveryEqualityComparer.Equals(e, cv));
						if (match == null)
						{
							entriesForCurrentVerseBridge.Add(cv);
						}
						else if (!cv.IsUnusual && match.IsUnusual && !includeAlternatesAndRareQuotes)
						{
							// We prefer a regular quote type because in the end we will eliminate Alternate and Rare quotes.
							// Since we have found a subsequent verse with the unusual character as a "regular" quote type,
							// we want to replace the unusual entry with the preferred one.
							entriesForCurrentVerseBridge.Remove(match);
							entriesForCurrentVerseBridge.Add(cv);
						}
					}
				}

				if (result == null)
					result = entriesForCurrentVerseBridge;
				else
					PerformPreferentialIntersection(ref result, entriesForCurrentVerseBridge);
			}

			if (result == null)
				throw new ArgumentException("Empty enumeration passed to GetCharacters.", nameof(verses));

			if (interruption != null)
				result.Add(interruption);

			if (!includeAlternatesAndRareQuotes)
				result.RemoveWhere(cv => cv.QuoteType == QuoteType.Alternate || cv.QuoteType == QuoteType.Rare);

			if (overrideCharacters != null)
			{
				foreach (var character in overrideCharacters.Where(c => !result.Any(r => r.Character == c && r.Delivery == Empty)))
					result.Add(new CharacterSpeakingMode(character, Empty, null, false, QuoteType.Potential));
			}

			return result;
		}

		/// <summary>
		/// Even though two CharacterSpeakingModes are equal if their character and delivery values are equal, some are "more
		/// equal than others". This custom intersection logic prefers regular entries over unusual (rare/alternate ones) and
		/// will also coalesce entries with conflicting deliveries into a single entry with no delivery specified.
		/// </summary>
		/// <param name="result"></param>
		/// <param name="entriesForCurrentVerseBridge"></param>
		internal void PerformPreferentialIntersection(ref HashSet<CharacterSpeakingMode> result, ISet<CharacterSpeakingMode> entriesForCurrentVerseBridge)
		{
			if (result.Any())
			{
				var exactMatches = new HashSet<CharacterSpeakingMode>(m_characterDeliveryEqualityComparer);
				var characterMatches = new HashSet<CharacterSpeakingMode>(m_characterDeliveryEqualityComparer);
				foreach (var entry in result)
				{
					var match = entriesForCurrentVerseBridge.FirstOrDefault(n => m_characterDeliveryEqualityComparer.Equals(n, entry));
					if (match != null)
					{
						// We prefer a regular quote type because in the end we might eliminate Alternate and Rare quotes,
						// but if any of the included verses have the unusual character as a "regular" quote type, we want
						// to keep the character in the list.
						exactMatches.Add(!entry.IsUnusual ? entry : match);
					}
					if (exactMatches.Any())
						continue;
					match = entriesForCurrentVerseBridge.FirstOrDefault(r => r.Character == entry.Character);
					if (match != null)
					{
						if (entry.Delivery == null)
							characterMatches.Add(entry);
						else if (match.Delivery == null)
							characterMatches.Add(match);
						else
							characterMatches.Add(new CharacterSpeakingMode(entry.Character, "", entry.Alias, false, QuoteType.Potential));
					}
				}

				result = exactMatches.Any() ? exactMatches : characterMatches;
			}
		}

		protected override bool AddCharacterVerse(CharacterVerse cv)
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
				if (expectedQuote.IsScriptureQuotation)
				{
					if (Singleton.GetEntriesForRef(expectedQuote.BcvRef)
						.Any(cv => cv.QuoteType == QuoteType.Rare && cv.Character == kNeedsReview))
						continue;
				}
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
