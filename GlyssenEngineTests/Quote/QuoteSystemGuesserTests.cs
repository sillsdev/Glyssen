//#define USE_RANDOM_SEED

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Glyssen.Shared;
using GlyssenEngine;
using GlyssenEngine.Character;
using GlyssenEngine.Quote;
using GlyssenEngineTests.Script;
using NUnit.Framework;
using Rhino.Mocks;
using SIL.Scripture;

namespace GlyssenEngineTests.Quote
{
	[TestFixture]
	class QuoteSystemGuesserTests
	{
		[Test]
		public void Guess_NoBooks_ReturnsDefaultQuoteSystem()
		{
			bool certain;
			Assert.AreEqual(QuoteSystem.Default, QuoteSystemGuesser.Guess(ControlCharacterVerseData.Singleton, new List<IScrBook>(), ScrVers.English, out certain));
			Assert.IsFalse(certain);
		}

		[Test]
		public void Guess_NoTextForVersesWithQuotations_ReturnsDefaultQuoteSystem()
		{
			var mockedBooks = new List<IScrBook>();

			for (int i = 1; i < BCVRef.LastBook; i++)
			{
				var mockedBook = MockRepository.GenerateMock<IScrBook>();
				mockedBook.Stub(x => x.BookId).Return(BCVRef.NumberToBookCode(i));
				mockedBook.Stub(x => x.GetVerseText(Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(string.Empty);
				mockedBooks.Add(mockedBook);
			}

			bool certain;
			Assert.AreEqual(QuoteSystem.Default, QuoteSystemGuesser.Guess(ControlCharacterVerseData.Singleton, mockedBooks, ScrVers.English, out certain));
			Assert.IsFalse(certain);
		}

		/// <summary>
		/// This is more of an acceptance test since it depends on randomly generated test data (to attempt
		/// to simulate real data), and the interworking of the QuoteSystemGuesser and the CharacterVerseData class.
		/// </summary>
		[Test]
		public void Guess_AllFirstLevelQuoteSystemsWithMultipleSecondLevelPossibilitiesWithHighlyConsistentData_CorrectlyIdentifiesSystemWithUncertainty()
		{
			foreach (var quoteSystem in QuoteSystem.UniquelyGuessableSystems.Where(qs => String.IsNullOrEmpty(qs.QuotationDashMarker) &&
				qs.NormalLevels.Count == 1 && QuoteUtils.GetLevel2Possibilities(qs.FirstLevel).Length > 1))
			{
				RunTest(quoteSystem, true, false, false);
			}
		}

		/// <summary>
		/// This is more of an acceptance test since it depends on randomly generated test data (to attempt
		/// to simulate real data), and the interworking of the QuoteSystemGuesser and the CharacterVerseData class.
		///
		/// "מֵירְכָאוֹת (Curly) with levels 2 (’/‘) and 3." has to be excluded here because it is so similar to "Citation marks with levels 2 (’/’) and 3."
		/// We still test it in the test which doesn't require certainty.
		/// </summary>
		[Test]
		public void Guess_MostSinglePossibilityMultipleLevelQuoteSystemsWithHighlyConsistentData_CorrectlyIdentifiesSystemWithCertainty()
		{
			foreach (var quoteSystem in QuoteSystem.UniquelyGuessableSystems.Where(qs => String.IsNullOrEmpty(qs.QuotationDashMarker) &&
				qs.NormalLevels.Count == 3 && QuoteUtils.GetLevel2Possibilities(qs.FirstLevel).Length == 1 &&
				qs.Name != "מֵירְכָאוֹת (Curly) with levels 2 (’/‘) and 3."))
			{
				RunTest(quoteSystem, true, false, true);
			}
		}

		/// <summary>
		/// This is more of an acceptance test since it depends on randomly generated test data (to attempt
		/// to simulate real data), and the interworking of the QuoteSystemGuesser and the CharacterVerseData class.
		/// </summary>
		[Test]
		public void Guess_AllSinglePossibilityMultipleLevelQuoteSystemsWithHighlyConsistentData_CorrectlyIdentifiesSystemWithoutCertainty()
		{
			foreach (var quoteSystem in QuoteSystem.UniquelyGuessableSystems.Where(qs => String.IsNullOrEmpty(qs.QuotationDashMarker) &&
				qs.NormalLevels.Count == 3 && QuoteUtils.GetLevel2Possibilities(qs.FirstLevel).Length == 1))
			{
				RunTest(quoteSystem, true, false, false);
			}
		}

		/// <summary>
		/// This is more of an acceptance test since it depends on randomly generated test data (to attempt
		/// to simulate real data), and the interworking of the QuoteSystemGuesser and the CharacterVerseData class.
		/// </summary>
		[Test]
		public void Guess_AllFirstLevelQuoteSystemsWithMultipleSecondLevelPossibilitiesAndDialogueQuotesWithHighlyConsistentData_CorrectlyIdentifiesSystemWithUncertainty()
		{
			foreach (var quoteSystem in QuoteSystem.UniquelyGuessableSystems.Where(qs => !String.IsNullOrEmpty(qs.QuotationDashMarker) &&
				qs.NormalLevels.Count == 1 && QuoteUtils.GetLevel2Possibilities(qs.FirstLevel).Length > 1))
			{
				RunTest(quoteSystem, true, false, false);
			}
		}

		/// <summary>
		/// This is more of an acceptance test since it depends on randomly generated test data (to attempt
		/// to simulate real data), and the interworking of the QuoteSystemGuesser and the CharacterVerseData class.
		/// </summary>
		[Test]
		public void Guess_AllSinglePossibilityMultipleLevelQuoteSystemsWithDialogueQuotesWithHighlyConsistentDataNoSecondLevelQuotesInData_CorrectlyIdentifiesSystemWithUncertainty()
		{
			foreach (var quoteSystem in QuoteSystem.UniquelyGuessableSystems.Where(qs => !String.IsNullOrEmpty(qs.QuotationDashMarker) &&
				qs.NormalLevels.Count == 3 && QuoteUtils.GetLevel2Possibilities(qs.FirstLevel).Length == 1))
			{
				RunTest(quoteSystem, true, false, false);
			}
		}

		/// <summary>
		/// This is more of an acceptance test since it depends on randomly generated test data (to attempt
		/// to simulate real data), and the interworking of the QuoteSystemGuesser and the CharacterVerseData class.
		/// </summary>
		[Test]
		public void Guess_AllMultiplePossibilityMultipleLevelQuoteSystemsWithDialogueQuotesWithHighlyConsistentDataAndSecondLevelQuotesInData_CorrectlyIdentifiesSystemWithUncertainty()
		{
			foreach (var quoteSystem in QuoteSystem.UniquelyGuessableSystems.Where(qs => !String.IsNullOrEmpty(qs.QuotationDashMarker) &&
				qs.NormalLevels.Count == 3 && QuoteUtils.GetLevel2Possibilities(qs.FirstLevel).Length > 1))
			{
				RunTest(quoteSystem, true, true, false);
			}
		}

		[Test]
		public void Guess_DoubleCurlyQuotesWithLessConsistentData_CorrectlyIdentifiesSystemWithUncertainty()
		{
			var quoteSystem = QuoteSystem.UniquelyGuessableSystems.Single(qs => qs.Name == "Quotation marks, double with levels 2 (‘/’) and 3.");
			RunTest(quoteSystem, false, false, false);
		}

		[Test]
		public void Guess_StraightQuotesWithLessConsistentData_CorrectlyIdentifiesSystemWithCertainty()
		{
			var quoteSystem = QuoteSystem.UniquelyGuessableSystems.Single(qs => qs.FirstLevel.Open == "\"" && qs.NormalLevels.Count > 1);
			RunTest(quoteSystem, false, false, true);
		}

		private void RunTest(QuoteSystem quoteSystem, bool highlyConsistentData, bool includeSecondLevelQuotes, bool expectedCertain)
		{
			Console.WriteLine("Attempting to guess " + quoteSystem.Name + "(" + quoteSystem + ")");
			var sw = new Stopwatch();
			sw.Start();
			bool certain;
			var guessedQuoteSystem = QuoteSystemGuesser.Guess(ControlCharacterVerseData.Singleton, MockedBookForQuoteSystem.GetMockedBooks(quoteSystem, highlyConsistentData, includeSecondLevelQuotes), ScrVers.English, out certain);
			sw.Stop();
			Console.WriteLine("   took " + sw.ElapsedMilliseconds + " milliseconds.");
			if (expectedCertain)
			{
				Assert.AreEqual(quoteSystem, guessedQuoteSystem, "Expected " + quoteSystem.FirstLevel + ", but was " + guessedQuoteSystem.FirstLevel);
				Assert.IsTrue(certain, "Quote system was not guessed with sufficient certainty: " + quoteSystem.Name + "(" + quoteSystem + ")");
			}
			else
			{
				Assert.AreEqual(quoteSystem, guessedQuoteSystem, "Expected " + quoteSystem + ", but was " + guessedQuoteSystem);
			}
		}

		[Test]
		[Category("SkipOnTeamCity")]
		public void Guess_NoQuotes_GivesUpWithoutTakingForever()
		{
			var sw = new Stopwatch();
			sw.Start();
			bool certain;
			Assert.AreEqual(QuoteSystem.Default, QuoteSystemGuesser.Guess(ControlCharacterVerseData.Singleton, MockedBookForQuoteSystem.GetMockedBooks(null), ScrVers.English, out certain));
			sw.Stop();
			Assert.IsTrue(sw.ElapsedMilliseconds < 5200, "Actual time (ms): " + sw.ElapsedMilliseconds);
			Assert.IsFalse(certain);
		}
	}

	internal class MockedBookForQuoteSystem : IScrBook
	{
		private readonly QuoteSystem m_desiredQuoteSystem;
		private readonly bool m_highlyConsistentData;
		private readonly bool m_includeSecondLevelQuotes;
		private readonly Random m_random;
		private bool m_secondLevelQuoteCloserPending;
#if USE_RANDOM_SEED
		static int s_seedBase = (int)DateTime.Now.Ticks;
#endif

		public static List<IScrBook> GetMockedBooks(QuoteSystem desiredQuoteSystem, bool highlyConsistentData = false, bool includeSecondLevelQuotes = false)
		{
			var mockedBooks = new List<IScrBook>();
			for (int i = 1; i <= BCVRef.LastBook; i++)
				mockedBooks.Add(new MockedBookForQuoteSystem(i, desiredQuoteSystem, highlyConsistentData, includeSecondLevelQuotes));
			return mockedBooks;
		}

		public MockedBookForQuoteSystem(int bookNum, QuoteSystem desiredQuoteSystem, bool highlyConsistentData, bool includeSecondLevelQuotes)
		{
#if USE_RANDOM_SEED
			Debug.WriteLine("Seed base = " + s_seedBase);
			var seed = s_seedBase + bookNum;
#else
			var seed = 300 + bookNum;
#endif
			m_random = new Random(seed);
			m_desiredQuoteSystem = desiredQuoteSystem;
			m_highlyConsistentData = highlyConsistentData;
			m_includeSecondLevelQuotes = includeSecondLevelQuotes && m_desiredQuoteSystem.NormalLevels.Count > 1;
			BookId = BCVRef.NumberToBookCode(bookNum);
			BookNumber = bookNum;
		}

		public string BookId { get; }

		private int BookNumber { get; }

		private enum QuotePosition
		{
			None,
			StartOfVerse,
			MiddleOfVerse,
			EndOfVerse,
			OutOfOrder,
		}

		public string GetVerseText(int chapter, int verse)
		{
			if (m_desiredQuoteSystem == null)
				return BlockTestExtensions.RandomString();

			var verseText = new StringBuilder();

			var characters = ControlCharacterVerseData.Singleton.GetCharacters(BookNumber, chapter, new SingleVerse(verse)).ToList();
			// If previous verse had same character talking, it's probably a longer discourse, so minimize the number of start quotes.
			bool quoteStartExpected = (verse == 1 || characters.Count > 1 || !ControlCharacterVerseData.Singleton.GetCharacters(BookNumber, chapter, verse - 1)
				.SequenceEqual(characters) || verse % 5 == 0);

			// The following attempts to more-or-less simulate real data.
			// When this method is called for verses that are expected to begin a quote, not having
			// a start quote needs to be fairly rare.
			QuotePosition startQuote = quoteStartExpected ? QuotePosition.MiddleOfVerse : QuotePosition.None;
			QuotePosition endQuote = QuotePosition.None;

			if (m_secondLevelQuoteCloserPending && startQuote != QuotePosition.None)
				m_secondLevelQuoteCloserPending = false;

			if (m_highlyConsistentData)
				RandomizeHighlyConsistent(quoteStartExpected, ref startQuote, ref endQuote);
			else
				RandomizeLessConsistent(quoteStartExpected, ref startQuote, ref endQuote);


			string startQuoteMarker = m_desiredQuoteSystem.FirstLevel.Open;
			string endQuoteMarker = m_desiredQuoteSystem.FirstLevel.Close;

			if (!String.IsNullOrEmpty(m_desiredQuoteSystem.QuotationDashMarker))
			{
				if (quoteStartExpected && characters[0].IsDialogue)
				{
					if (m_random.Next(10) > 1)
					{
						// 90% of expected dialogue quotes will, in fact, use the dialogue quote marker.
						startQuoteMarker = m_desiredQuoteSystem.QuotationDashMarker;
						if (!String.IsNullOrEmpty(m_desiredQuoteSystem.QuotationDashEndMarker))
							endQuoteMarker = m_desiredQuoteSystem.QuotationDashEndMarker;
						else
							endQuote = QuotePosition.None;
					}
				}
				else
				{
					if (m_random.Next(50) > 1)
					{
						// 2% of quotes that were not expected to be dialogue will, nonetheless, use the dialogue quote marker.
						startQuoteMarker = m_desiredQuoteSystem.QuotationDashMarker;
						endQuoteMarker = m_desiredQuoteSystem.QuotationDashEndMarker;
					}
				}
			}

			int startQuotePos = -1;
			int endQuotePos = -1;

			if (startQuote == QuotePosition.StartOfVerse)
			{
				verseText.Append(startQuoteMarker);
				startQuotePos = verseText.Length;
			}
			if (endQuote == QuotePosition.StartOfVerse)
				verseText.Append(endQuoteMarker);

			verseText.Append(BlockTestExtensions.RandomString());

			if (m_highlyConsistentData && startQuote != QuotePosition.None && startQuote != QuotePosition.StartOfVerse)
			{
				verseText.Replace('\u2014', '-'); // Prevent spurious em-dashes before starting quote mark
			}

			if (startQuote == QuotePosition.MiddleOfVerse)
			{
				verseText.Append(startQuoteMarker);
				startQuotePos = verseText.Length;
				verseText.Append(BlockTestExtensions.RandomString());
			}

			if (endQuote == QuotePosition.MiddleOfVerse || endQuote == QuotePosition.OutOfOrder)
			{
				endQuotePos = verseText.Length;
				verseText.Append(endQuoteMarker);
				verseText.Append(BlockTestExtensions.RandomString());
			}

			if (startQuote == QuotePosition.OutOfOrder)
			{
				verseText.Append(startQuoteMarker);
				startQuotePos = verseText.Length;
				verseText.Append(BlockTestExtensions.RandomString());
			}

			if (startQuote == QuotePosition.EndOfVerse)
				verseText.Append(startQuoteMarker);
			if (endQuote == QuotePosition.EndOfVerse)
			{
				endQuotePos = verseText.Length;
				verseText.Append(endQuoteMarker);
			}

			if (m_includeSecondLevelQuotes && startQuoteMarker != m_desiredQuoteSystem.QuotationDashMarker)
			{
				float likelihoodFactor = m_highlyConsistentData ? 2F : 4F;

				if (startQuotePos > 0 && endQuotePos > startQuotePos)
				{
					var pos = m_random.Next(startQuotePos, (int)(verseText.Length * likelihoodFactor));
					if (pos + 3 < endQuotePos)
					{
						verseText.Insert(pos, m_desiredQuoteSystem.NormalLevels[1].Open);

						if (characters.Count == 1 && ControlCharacterVerseData.Singleton.GetCharacters(BookNumber, chapter, verse + 1)
							.SequenceEqual(characters) && (verse + 1) % 5 == 0)
							likelihoodFactor = 1;

						pos = m_random.Next(pos + 2, (int)(endQuotePos + (endQuotePos - pos) * likelihoodFactor));
						if (pos < endQuotePos)
							verseText.Insert(pos, m_desiredQuoteSystem.NormalLevels[1].Close);
						else
							m_secondLevelQuoteCloserPending = true;
					}
				}
				else if (m_secondLevelQuoteCloserPending)
				{
					int pos = m_random.Next(startQuotePos, verseText.Length);
					if (pos < endQuotePos)
						verseText.Insert(pos, m_desiredQuoteSystem.NormalLevels[1].Close);
				}
			}

			return verseText.ToString();
		}

		private void RandomizeHighlyConsistent(bool quoteStartExpected, ref QuotePosition startQuote, ref QuotePosition endQuote)
		{
			var randomizer = m_random.Next(50);
			if (!quoteStartExpected)
			{
				switch (randomizer)
				{
					case 1:
						startQuote = QuotePosition.MiddleOfVerse;
						endQuote = QuotePosition.MiddleOfVerse;
						break;
					case 2:
					case 3:
					case 4:
					case 5:
						endQuote = QuotePosition.MiddleOfVerse;
						break;
					case 6:
					case 7:
					case 8:
					case 9:
						endQuote = QuotePosition.EndOfVerse;
						break;
				}
			}
			else
			{
				switch (randomizer)
				{
					case 1:
					case 2:
						startQuote = QuotePosition.None;
						break;
					case 3:
					case 4:
					case 5:
					case 6:
					case 7:
					case 8:
						startQuote = QuotePosition.StartOfVerse;
						break;
					case 9:
					case 10:
					case 11:
						endQuote = QuotePosition.MiddleOfVerse;
						break;
					case 12:
					case 13:
					case 14:
					case 15:
					case 16:
					case 17:
					case 18:
					case 19:
					case 20:
					case 21:
					case 22:
					case 23:
					case 24:
					case 25:
					case 26:
						endQuote = QuotePosition.EndOfVerse;
						break;
				}
			}
		}

		private void RandomizeLessConsistent(bool quoteStartExpected, ref QuotePosition startQuote, ref QuotePosition endQuote)
		{
			var randomizer = m_random.Next(50);
			if (!quoteStartExpected)
			{
				switch (randomizer)
				{
					case 1:
						startQuote = QuotePosition.MiddleOfVerse;
						endQuote = QuotePosition.MiddleOfVerse;
						break;
					case 2:
						endQuote = QuotePosition.MiddleOfVerse;
						break;
					case 3:
					case 4:
					case 5:
					case 6:
						endQuote = QuotePosition.EndOfVerse;
						break;
				}
			}
			else
			{
				switch (randomizer)
				{
					case 1:
					case 2:
					case 3:
						startQuote = QuotePosition.None;
						endQuote = QuotePosition.EndOfVerse;
						break;
					case 4:
					case 5:
					case 6:
						startQuote = QuotePosition.StartOfVerse;
						break;
					case 7:
					case 8:
					case 9:
					case 10:
						startQuote = QuotePosition.OutOfOrder;
						endQuote = QuotePosition.OutOfOrder;
						break;
					case 11:
					case 12:
					case 13:
					case 14:
						endQuote = QuotePosition.MiddleOfVerse;
						break;
					case 15:
					case 16:
					case 17:
					case 18:
					case 19:
					case 20:
						endQuote = QuotePosition.EndOfVerse;
						break;
					case 21:
					case 22:
					case 23:
					case 24:
					case 25:
					case 26:
					case 27:
						startQuote = QuotePosition.None;
						break;
				}
			}
		}
	}
}
