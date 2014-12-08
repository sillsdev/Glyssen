using System;
using System.Collections.Generic;
using System.Diagnostics;
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

		public static QuoteSystem Guess(ICharacterVerseInfo cvInfo, IEnumerable<IScrBook> bookList, out bool certain)
		{
			certain = false;
			var bookCount = bookList.Count();
			if (bookCount == 0)
				return QuoteSystem.Default;
			var stats = QuoteSystem.AllSystems.ToDictionary(s => s, s => new QuoteStatistics());
			int totalVersesAnalyzed = 0;
			const int minSample = 15;
			int maxSamplePerBook = BCVRef.LastBook * minSample / bookCount;
			const double minStartQuotePercent = .75;
			const double minEndQuotePercent = .25;
			const double maxCompetitorPercent = .6;
			const int maxFollowingVersesToSearchForEndQuote = 7;
			const int maxTimeLimit = 4800; // milliseconds
			QuoteStatistics bestStatistics = new QuoteStatistics();

			var stopwatch = new Stopwatch();
			stopwatch.Start();

			foreach (var book in bookList)
			{
				int versesAnalyzedForCurrentBook = 0;
				int prevQuoteChapter = -1;
				int prevQuoteVerse = -1;

				foreach (var quote in cvInfo.GetAllQuoteInfo(book.BookId).Where(q => q.Character != CharacterVerseData.kNotAQuote))
				{
					if (quote.Chapter == prevQuoteChapter && (quote.Verse == prevQuoteVerse || quote.Verse == prevQuoteVerse + 1))
					{
						prevQuoteVerse = quote.Verse;
						continue;
					}
					var text = book.GetVerseText(quote.Chapter, quote.Verse);
					foreach (var quoteSystem in QuoteSystem.AllSystems)
					{
						int ichStartQuote = text.IndexOf(quoteSystem.StartQuoteMarker, StringComparison.Ordinal);
						if (ichStartQuote >= 0 && ichStartQuote < text.Length - 2)
						{
							if (++stats[quoteSystem].StartQuoteHits > bestStatistics.StartQuoteHits)
								bestStatistics.StartQuoteHits++;

							if (text.IndexOf(quoteSystem.EndQuoteMarker, ichStartQuote + 1, StringComparison.Ordinal) > ichStartQuote)
							{
								if (++stats[quoteSystem].EndQuoteHits > bestStatistics.EndQuoteHits)
									bestStatistics.EndQuoteHits++;
							}
							else
							{
								for (int i = 1; i < maxFollowingVersesToSearchForEndQuote; i++)
								{
									if (!cvInfo.GetCharacters(book.BookId, quote.Chapter, quote.Verse + i).Any())
										break;
									text = book.GetVerseText(quote.Chapter, quote.Verse);
									if (text.IndexOf(quoteSystem.EndQuoteMarker, StringComparison.Ordinal) > 0)
									{
										if (++stats[quoteSystem].EndQuoteHits > bestStatistics.EndQuoteHits)
											bestStatistics.EndQuoteHits++;
										break;
									}
								}
							}
						}
					}
					totalVersesAnalyzed++;
					versesAnalyzedForCurrentBook++;

					if (totalVersesAnalyzed >= minSample && bestStatistics.EndQuoteHits > 0)
					{
						QuoteSystem match = null;
						var competitors = new List<QuoteSystem>();

						foreach (var kvp in stats)
						{
							bool possibleMatch = (kvp.Value.StartQuoteHits == bestStatistics.StartQuoteHits ||
												kvp.Value.EndQuoteHits == bestStatistics.EndQuoteHits);
							if (match == null && possibleMatch &&
								kvp.Value.StartQuoteHits > totalVersesAnalyzed * minStartQuotePercent &&
								kvp.Value.EndQuoteHits > totalVersesAnalyzed * minEndQuotePercent)
							{
								match = kvp.Key;
							}
							else if (kvp.Value.StartQuoteHits > bestStatistics.StartQuoteHits * maxCompetitorPercent &&
								kvp.Value.EndQuoteHits > bestStatistics.EndQuoteHits * maxCompetitorPercent)
							{
								// We just found a competitor that is too close to the current leader to safely declare
								// the winner a clear and convincing winner. (So nobody wins.)
								competitors.Add(kvp.Key);
							}
							//else if (possibleMatch)
							//{
							//	Debug.WriteLine("Possible match: " + kvp.Key);
							//	Debug.WriteLine(" -- StartQuoteHits: " + kvp.Value.StartQuoteHits + " of a possible " + totalVersesAnalyzed);
							//	Debug.WriteLine(" -- EndQuoteHits: " + kvp.Value.EndQuoteHits + " of a possible " + totalVersesAnalyzed);
							//}
						}
						if (match != null)
						{
							if (competitors.Any())
							{
								foreach (var competitor in competitors)
									Debug.WriteLine("Competitor \"" + competitor + "\" too close to leader \"" + match + "\".");
							}
							else
							{
								certain = true;
								return match;
							}
						}
					}

					if (stopwatch.ElapsedMilliseconds > maxTimeLimit)
					{
						Debug.WriteLine("Giving up guessing quote system.");
						return BestGuess(stats, bestStatistics);
					}

					if (versesAnalyzedForCurrentBook >= maxSamplePerBook)
						break;

					prevQuoteChapter = quote.Chapter;
					prevQuoteVerse = quote.Verse;
				}
			}
			return BestGuess(stats, bestStatistics);
		}

		private static QuoteSystem BestGuess(Dictionary<QuoteSystem, QuoteStatistics> stats, QuoteStatistics bestStatistics)
		{
			if (bestStatistics.StartQuoteHits == 0 && bestStatistics.EndQuoteHits == 0)
				return QuoteSystem.Default;

			int maxEndQuoteHits = 0;
			QuoteSystem bestSystem = QuoteSystem.Default;
			foreach (var system in stats.Where(kvp => (kvp.Value.StartQuoteHits == bestStatistics.StartQuoteHits)))
			{
				if (system.Value.EndQuoteHits > maxEndQuoteHits)
				{
					maxEndQuoteHits = system.Value.EndQuoteHits;
					bestSystem = system.Key;
				}
			}
			return bestSystem;
		}
	}
}
