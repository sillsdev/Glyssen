using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using NUnit.Framework;
using ProtoScript;
using Rhino.Mocks;
using SIL.ScriptureUtils;

namespace ProtoScriptTests
{
	[TestFixture]
	class QuoteSystemGuesserTests
	{
		[Test]
		public void Guess_NoBooks_ReturnsDefaultQuoteSystem()
		{
			bool certain;
			Assert.AreEqual(QuoteSystem.Default, QuoteSystemGuesser.Guess(new List<IScrBook>(), out certain));
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
			Assert.AreEqual(QuoteSystem.Default, QuoteSystemGuesser.Guess(mockedBooks, out certain));
			Assert.IsFalse(certain);
		}

		[Test]
		public void Guess_AllBuiltInQuoteSystemsWithHighlyConsistentData_CorrectlyIdentifiesSystemWithCertainty()
		{
			foreach (var quoteSystem in QuoteSystem.AllSystems)
			{
				RunTest(quoteSystem, true, true);
			}
		}

		[Test]
		public void Guess_DoubleCurlyQuotesWithLessConsistentData_CorrectlyIdentifiesSystemWithCertainty()
		{
			var quoteSystem = QuoteSystem.AllSystems.Single(qs => qs.Name == "Quotation marks, double");
			RunTest(quoteSystem, false, true);
		}

		private void RunTest(QuoteSystem quoteSystem, bool highlyConsistentData, bool expectedCertain)
		{
			Console.WriteLine("Attempting to guess " + quoteSystem.Name + "(" + quoteSystem + ")");
			var sw = new Stopwatch();
			sw.Start();
			bool certain;
			var guessedQuoteSystem = QuoteSystemGuesser.Guess(MockedBookForQuoteSystem.GetMockedBooks(quoteSystem, highlyConsistentData), out certain);
			sw.Stop();
			Console.WriteLine("   took " + sw.ElapsedMilliseconds + " milliseconds.");
			Assert.AreEqual(quoteSystem, guessedQuoteSystem);
			if (expectedCertain)
			Assert.IsTrue(certain);
		}

		[Test]
		public void Guess_NoQuotes_GivesUpWithoutTakingForever()
		{
			var sw = new Stopwatch();
			sw.Start();
			bool certain;
			Assert.AreEqual(QuoteSystem.Default, QuoteSystemGuesser.Guess(MockedBookForQuoteSystem.GetMockedBooks(null), out certain));
			sw.Stop();
			Assert.IsTrue(sw.ElapsedMilliseconds < 5200, "Actual time (ms): " + sw.ElapsedMilliseconds);
			Assert.IsFalse(certain);
		}
	}

	internal class MockedBookForQuoteSystem : IScrBook
	{
		private QuoteSystem m_desiredQuoteSystem;
		private readonly bool m_highlyConsistentData;
		private Random m_random = new Random();

		public static List<IScrBook> GetMockedBooks(QuoteSystem desiredQuoteSystem, bool highlyConsistentData = false)
		{
			var mockedBooks = new List<IScrBook>();
			for (int i = 1; i < BCVRef.LastBook; i++)
				mockedBooks.Add(new MockedBookForQuoteSystem(i, desiredQuoteSystem, highlyConsistentData));
			return mockedBooks;
		}

		public MockedBookForQuoteSystem(int bookNum, QuoteSystem desiredQuoteSystem, bool highlyConsistentData)
		{
			m_desiredQuoteSystem = desiredQuoteSystem;
			m_highlyConsistentData = highlyConsistentData;
			BookId = BCVRef.NumberToBookCode(bookNum);
		}

		public string BookId { get; private set; }

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

			string character = CharacterVerse.GetCharacter(BookId, chapter, verse);
			bool quoteStartExpected = character != Block.UnknownCharacter && character != "scripture" && character != CharacterVerse.kNotAQuote;
			// If previous verse had same character talking, it's probably a longer discourse, so minimize the number of start quotes.
			if (verse > 1 && CharacterVerse.GetCharacter(BookId, chapter, verse - 1) == character && verse % 5 != 0)
				quoteStartExpected = false;

			// The following attempts to more-or-less simulate real data.
			// When this method is called for verses that are expected to begin a quote, not having
			// a start quote needs to be fairly rare.
			QuotePosition startQuote = quoteStartExpected ? QuotePosition.MiddleOfVerse : QuotePosition.None;
			QuotePosition endQuote = QuotePosition.None;

			if (m_highlyConsistentData)
				RandomizeHighlyConsistent(quoteStartExpected, ref startQuote, ref endQuote);
			else
				RandomizeLessConsistent(quoteStartExpected, ref startQuote, ref endQuote);

			if (startQuote == QuotePosition.StartOfVerse)
				verseText.Append(m_desiredQuoteSystem.StartQuoteMarker);
			if (endQuote == QuotePosition.StartOfVerse)
				verseText.Append(m_desiredQuoteSystem.EndQuoteMarker);

			verseText.Append(BlockTestExtensions.RandomString());

			if (startQuote == QuotePosition.MiddleOfVerse)
			{
				verseText.Append(m_desiredQuoteSystem.StartQuoteMarker);
				verseText.Append(BlockTestExtensions.RandomString());
			}

			if (endQuote == QuotePosition.MiddleOfVerse || endQuote == QuotePosition.OutOfOrder)
			{
				verseText.Append(m_desiredQuoteSystem.EndQuoteMarker);
				verseText.Append(BlockTestExtensions.RandomString());
			}

			if (startQuote == QuotePosition.OutOfOrder)
			{
				verseText.Append(m_desiredQuoteSystem.StartQuoteMarker);
				verseText.Append(BlockTestExtensions.RandomString());
			}

			if (startQuote == QuotePosition.EndOfVerse)
				verseText.Append(m_desiredQuoteSystem.StartQuoteMarker);
			if (endQuote == QuotePosition.EndOfVerse)
				verseText.Append(m_desiredQuoteSystem.EndQuoteMarker);

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
