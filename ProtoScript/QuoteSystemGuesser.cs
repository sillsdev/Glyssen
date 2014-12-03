using System.Collections.Generic;
using System.Linq;
using SIL.ScriptureUtils;

namespace ProtoScript
{
	public class QuoteSystemGuesser
	{
		private class QuoteStatistics
		{
			public int StartQuoteHits = 0;
			public int EndQuoteHits = 0;
		}

		public static QuoteSystem Guess(IEnumerable<IScrBook> bookList)
		{
			var bookCount = bookList.Count();
			if (bookCount == 0)
				return QuoteSystem.Default;
			var stats = QuoteSystem.AllSystems.ToDictionary(s => s, s => new QuoteStatistics());
			int totalVersesAnalyzed = 0;
			int versesAnalyzedForCurrentBook;
			const int minSample = 15;
			int maxSamplePerBook = BCVRef.LastBook * minSample / bookCount;
			const double minStartQuotePercent = .75;
			const double minEndQuotePercent = .22;
			const int maxFollowingVersesToSearchForEndQuote = 7;
			int prevQuoteChapter;
			int prevQuoteVerse;

			foreach (var book in bookList)
			{
				versesAnalyzedForCurrentBook = 0;
				prevQuoteChapter = -1;
				prevQuoteVerse = -1;

				foreach (var quote in CharacterVerse.GetAllQuoteInfo(book.BookId).Where(q => q.Character != CharacterVerse.kNotAQuote))
				{
					if (quote.Chapter == prevQuoteChapter && (quote.Verse == prevQuoteVerse || quote.Verse == prevQuoteVerse + 1))
					{
						prevQuoteVerse = quote.Verse;
						continue;
					}
					var text = book.GetVerseText(quote.Chapter, quote.Verse);
					foreach (var quoteSystem in QuoteSystem.AllSystems)
					{
						if (text.Contains(quoteSystem.StartQuoteMarker))
							stats[quoteSystem].StartQuoteHits++;
						if (text.Contains(quoteSystem.EndQuoteMarker))
							stats[quoteSystem].EndQuoteHits++;
						else
						{
							for (int i = 1; i < maxFollowingVersesToSearchForEndQuote; i++)
							{
								if (CharacterVerse.GetCharacter(book.BookId, quote.Chapter, quote.Verse + i) != Block.UnknownCharacter)
									break;
								text = book.GetVerseText(quote.Chapter, quote.Verse);
								if (text.Contains(quoteSystem.EndQuoteMarker))
								{
									stats[quoteSystem].EndQuoteHits++;
									break;
								}
							}
						}
					}
					totalVersesAnalyzed++;
					versesAnalyzedForCurrentBook++;

					if (totalVersesAnalyzed > minSample)
					{
						foreach (var kvp in stats)
						{
							if (kvp.Value.StartQuoteHits > totalVersesAnalyzed * minStartQuotePercent &&
								kvp.Value.EndQuoteHits > totalVersesAnalyzed * minEndQuotePercent)
								return kvp.Key;
						}
					}

					if (versesAnalyzedForCurrentBook >= maxSamplePerBook)
						break;

					prevQuoteChapter = quote.Chapter;
					prevQuoteVerse = quote.Verse;
				}
			}
			return QuoteSystem.Default;
		}
	}
}
