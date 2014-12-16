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
			int maxNonDialogueSamplesPerBook = BCVRef.LastBook * minSample / bookCount;
			const double minStartQuotePercent = .75;
			const double minEndQuotePercent = .25;
			const double maxCompetitorPercent = .6;
			const int maxFollowingVersesToSearchForEndQuote = 7;
			const int maxTimeLimit = 4800; // milliseconds
			QuoteStatistics bestStatistics = new QuoteStatistics();
			int followingVersesToSearchForEndQuote;

			var stopwatch = new Stopwatch();
			stopwatch.Start();

			foreach (var book in bookList)
			{
				int versesAnalyzedForCurrentBook = 0;
				int prevQuoteChapter = -1;
				int prevQuoteVerse = -1;

				foreach (var quote in cvInfo.GetAllQuoteInfo(book.BookId)
					.Where(q => !CharacterVerseData.IsCharacterOfType(q.Character, CharacterVerseData.StandardCharacter.Narrator)))
				{
					if (versesAnalyzedForCurrentBook > maxNonDialogueSamplesPerBook && !quote.IsDialogue)
						continue;

					if (quote.Chapter == prevQuoteChapter && (quote.Verse == prevQuoteVerse || quote.Verse == prevQuoteVerse + 1))
					{
						prevQuoteVerse = quote.Verse;
						continue;
					}
					var text = book.GetVerseText(quote.Chapter, quote.Verse);
					Debug.WriteLine("Evaluating {0} {1}:{2} - contents: {3}", book.BookId, quote.Chapter, quote.Verse, text);
					foreach (var quoteSystem in stats.Keys)
					{
						string endQuoteMarker = quoteSystem.EndQuoteMarker;
						followingVersesToSearchForEndQuote = maxFollowingVersesToSearchForEndQuote;

						int ichStartQuote = text.IndexOf(quoteSystem.StartQuoteMarker, StringComparison.Ordinal);

						if (quote.IsDialogue && !string.IsNullOrEmpty(quoteSystem.QuotationDashMarker))
						{
							int i = text.IndexOf(quoteSystem.QuotationDashMarker, StringComparison.Ordinal);
							if (i >= 0 && (ichStartQuote < 0 || i < ichStartQuote))
							{
								// Found a dialogue quote marker earlier in the text.
								if (!String.IsNullOrEmpty(quoteSystem.QuotationDashEndMarker) || ichStartQuote < 0)
									endQuoteMarker = quoteSystem.QuotationDashEndMarker;
								ichStartQuote = i;
								followingVersesToSearchForEndQuote = 1; // Dialogue quotes are typically short and rarely cross more than one verse.
							}
						}
						if (ichStartQuote >= 0 && ichStartQuote < text.Length - 2)
						{
							if (++stats[quoteSystem].StartQuoteHits > bestStatistics.StartQuoteHits)
								bestStatistics.StartQuoteHits++;

							if (endQuoteMarker == null || text.IndexOf(endQuoteMarker, ichStartQuote + 1, StringComparison.Ordinal) > ichStartQuote)
							{
								if (++stats[quoteSystem].EndQuoteHits > bestStatistics.EndQuoteHits)
									bestStatistics.EndQuoteHits++;
							}
							else
							{
								for (int i = 1; i < followingVersesToSearchForEndQuote; i++)
								{
									if (!cvInfo.GetCharacters(book.BookId, quote.Chapter, quote.Verse + i).Any())
										break;
									text = book.GetVerseText(quote.Chapter, quote.Verse);
									if (text.IndexOf(endQuoteMarker, StringComparison.Ordinal) > 0)
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
							var closeCompetitors = competitors.Where(c => c.StartQuoteMarker != match.StartQuoteMarker ||
								c.EndQuoteMarker != match.EndQuoteMarker ||
								stats[c].StartQuoteHits == bestStatistics.StartQuoteHits).ToList();
							if (closeCompetitors.Any())
							{
								foreach (var competitor in closeCompetitors)
									Debug.WriteLine("Competitor " + competitor.Name + " (" + competitor + ") too close to leader \"" + match + "\".");
							}
							else
							{
								Debug.WriteLine("STATISTICS:");
								foreach (var kvp in stats.Where(kvp => kvp.Value.StartQuoteHits > 0 || kvp.Value.EndQuoteHits > 0))
								{
									Debug.WriteLine(kvp.Key.Name + "(" + kvp.Key + ")\tStart Hits: " + kvp.Value.StartQuoteHits + "\tEnd Hits: " +
													kvp.Value.EndQuoteHits);
								}
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

			Debug.WriteLine("STATISTICS:");

			int maxEndQuoteHits = 0;
			QuoteSystem bestSystem = QuoteSystem.Default;
			foreach (var kvp in stats)
			{
				if (kvp.Value.StartQuoteHits > 0 || kvp.Value.EndQuoteHits > 0)
				{
					Debug.WriteLine(kvp.Key.Name + "(" + kvp.Key + ")\tStart Hits: " + kvp.Value.StartQuoteHits + "\tEnd Hits: " +
									kvp.Value.EndQuoteHits);
				}

				if ((kvp.Value.StartQuoteHits == bestStatistics.StartQuoteHits))
				{
					if (kvp.Value.EndQuoteHits > maxEndQuoteHits)
					{
						maxEndQuoteHits = kvp.Value.EndQuoteHits;
						bestSystem = kvp.Key;
					}
				}
			}
			return bestSystem;
		}
	}
}
