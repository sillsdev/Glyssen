//#define SHOWTESTINFO

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SIL.ScriptureUtils;

namespace ProtoScript
{
	public class QuoteSystemGuesser
	{
		private const int kMinSample = 15;
		private const int kMinQuotationDashSample = 15;
		private const double kMinPercent = .75;
		private const double kMinQuotationDashPercent = .25;
		private const double kQuotationDashFailPercent = .10;
		private const double kMaxCompetitorPercent = .6;
		private const int kMaxFollowingVersesToSearchForEndQuote = 7;
		private const int kMaxTimeLimit = 8000; // milliseconds - TODO: Lower to 4800

		private const int kStartQuoteValue = 2;
		private const int kEndQuoteValue = 2;
		private const int kQuotationDashValue = 3;

		public static QuoteSystem Guess<T>(ICharacterVerseInfo cvInfo, List<T> bookList, out bool certain) where T : IScrBook
		{
			certain = false;
			var bookCount = bookList.Count();
			if (bookCount == 0)
				return QuoteSystem.Default;
			var scores = QuoteSystem.UniquelyGuessableSystems.ToDictionary(s => s, s => 0);
			var quotationDashCounts = QuoteSystem.UniquelyGuessableSystems.Where(s => !String.IsNullOrEmpty(s.QuotationDashMarker))
				.ToDictionary(s => s, s => 0);
			var viableSystems = scores.Keys.ToList();
			int totalVersesAnalyzed = 0;
			int totalDialoqueQuoteVersesAnalyzed = 0;
			int maxNonDialogueSamplesPerBook = BCVRef.LastBook * kMinSample / bookCount;

			int bestScore = 0;
			bool foundEndQuote = false;

			int kVerseValue = Math.Min(kStartQuoteValue + kEndQuoteValue, kQuotationDashValue);

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
#if SHOWTESTINFO
					Debug.WriteLine("Evaluating {0} {1}:{2} - contents: {3}", book.BookId, quote.Chapter, quote.Verse, text);
#endif
					foreach (var quoteSystem in viableSystems)
					{
						int ichStartQuote = text.IndexOf(quoteSystem.StartQuoteMarker, StringComparison.Ordinal);

						if (quote.IsDialogue && !string.IsNullOrEmpty(quoteSystem.QuotationDashMarker))
						{
							int i = text.IndexOf(quoteSystem.QuotationDashMarker, StringComparison.Ordinal);
							if (i >= 0 && (ichStartQuote < 0 || i < ichStartQuote))
							{
								// Found a dialogue quote marker earlier in the text.
								scores[quoteSystem] += kQuotationDashValue;
								quotationDashCounts[quoteSystem]++;
								if (scores[quoteSystem] > bestScore)
									bestScore = scores[quoteSystem];
								continue;
							}
						}
						if (ichStartQuote >= 0 && ichStartQuote < text.Length - 2)
						{
							scores[quoteSystem] += kStartQuoteValue;
							if (scores[quoteSystem] > bestScore)
								bestScore = scores[quoteSystem];

							if (text.IndexOf(quoteSystem.EndQuoteMarker, ichStartQuote + 1, StringComparison.Ordinal) > ichStartQuote)
							{
								foundEndQuote = true;
								scores[quoteSystem] += kEndQuoteValue;
								if (scores[quoteSystem] > bestScore)
									bestScore = scores[quoteSystem];
							}
							else
							{
								for (int i = 1; i < kMaxFollowingVersesToSearchForEndQuote; i++)
								{
									if (!cvInfo.GetCharacters(book.BookId, quote.Chapter, quote.Verse + i).Any())
										break;
									text = book.GetVerseText(quote.Chapter, quote.Verse);
									if (text.IndexOf(quoteSystem.EndQuoteMarker, StringComparison.Ordinal) > 0)
									{
										foundEndQuote = true;
										scores[quoteSystem] += kEndQuoteValue;
										if (scores[quoteSystem] > bestScore)
											bestScore = scores[quoteSystem];
										break;
									}
								}
							}
						}
					}
					totalVersesAnalyzed++;
					if (quote.IsDialogue)
						totalDialoqueQuoteVersesAnalyzed++;
					versesAnalyzedForCurrentBook++;

					if (totalVersesAnalyzed >= kMinSample && foundEndQuote &&
						(totalDialoqueQuoteVersesAnalyzed >= kMinQuotationDashSample ||
						viableSystems.TrueForAll(s => String.IsNullOrEmpty(s.QuotationDashMarker))))
					{
						var competitors = new List<QuoteSystem>();

						foreach (var kvp in scores)
						{
							if (kvp.Value > totalVersesAnalyzed * kVerseValue * kMinPercent &&
								kvp.Value > bestScore * kMaxCompetitorPercent)
							{
								competitors.Add(kvp.Key);
							}
						}

						if (competitors.Any())
						{
							Debug.WriteLine("STATISTICS:");
							foreach (var system in competitors)
							{
								Debug.WriteLine(system.Name + "(" + system + ")\tScore: " + scores[system]);
								if (!String.IsNullOrEmpty(system.QuotationDashMarker))
								{
									Debug.WriteLine("\tPercentage matches of total Dialogue quotes analyzed: " + (100.0 * quotationDashCounts[system]) / totalDialoqueQuoteVersesAnalyzed);
								}
							}

							if (competitors.Count == 1)
							{
								certain = true;
								return competitors[0];
							}

							viableSystems = viableSystems.Where(competitors.Contains).ToList();
							if (competitors.TrueForAll(c => c.StartQuoteMarker == competitors[0].StartQuoteMarker &&
								c.EndQuoteMarker == competitors[0].EndQuoteMarker))
							{
								var contendersWithQDash = competitors.Where(c => !String.IsNullOrEmpty(c.QuotationDashMarker)).ToList();
								if (contendersWithQDash.TrueForAll(
									c => quotationDashCounts[c] < kQuotationDashFailPercent * totalDialoqueQuoteVersesAnalyzed))
								{
									certain = true;
									return competitors.Single(c => String.IsNullOrEmpty(c.QuotationDashMarker));
								}
								var winners = contendersWithQDash.Where(c => scores[c] == bestScore &&
									quotationDashCounts[c] > kMinQuotationDashPercent * totalDialoqueQuoteVersesAnalyzed).ToList();
								if (winners.Count == 1)
								{
									// Don't set certain to true. Can't ever be sure of details for dialogue quotes
									return winners[0];
								}
							}
							// Still have multiple systems in contention with the same first-level start & end markers,
							// but we haven't seen enough evidence to pick a clear winner.
						}
					}

					if (stopwatch.ElapsedMilliseconds > kMaxTimeLimit)
					{
#if SHOWTESTINFO
						Debug.WriteLine("Time-out guessing quote system.");
#endif
						return BestGuess(viableSystems, scores, bestScore, foundEndQuote);
					}

					prevQuoteChapter = quote.Chapter;
					prevQuoteVerse = quote.Verse;
				}
			}
			return BestGuess(viableSystems, scores, bestScore, foundEndQuote);
		}

		private static QuoteSystem BestGuess(IEnumerable<QuoteSystem> viableSystems, Dictionary<QuoteSystem, int> scores, int bestScore, bool foundEndQuote)
		{
			var bestSystem = viableSystems.FirstOrDefault(s => scores[s] == bestScore);
			if (bestSystem == null || !foundEndQuote)
			{
#if SHOWTESTINFO
				if (bestSystem == null)
					Debug.WriteLine("No best system found. Using default.");
				if (!foundEndQuote)
					Debug.WriteLine("No end-quote match found for any system. Using default.");
#endif
				return QuoteSystem.Default;
			}

			return bestSystem;
		}
	}
}
