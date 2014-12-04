using System;
using System.Collections.Generic;
using System.Diagnostics;
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
			Assert.AreEqual(QuoteSystem.Default, QuoteSystemGuesser.Guess(new List<IScrBook>()));
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
		
			Assert.AreEqual(QuoteSystem.Default, QuoteSystemGuesser.Guess(mockedBooks));
		}

		[Test]
		public void Guess_AllBuiltInQuoteSystems_CorrectlyIdentifiesSystem()
		{
			foreach (var quoteSystem in QuoteSystem.AllSystems)
			{
				var sw = new Stopwatch();
				sw.Start();
				var guessedQuoteSystem = QuoteSystemGuesser.Guess(MockedBookForQuoteSystem.GetMockedBooks(quoteSystem));
				sw.Stop();
				Console.WriteLine("Took " + sw.ElapsedMilliseconds + " milliseconds to attempt to guess " + quoteSystem.Name + "(" + quoteSystem + ")");
				Assert.AreEqual(quoteSystem, guessedQuoteSystem);
			}
		}

		[Test]
		public void Guess_NoQuotes_GivesUpWithoutTakingForever()
		{
			var sw = new Stopwatch();
			sw.Start();
			Assert.AreEqual(QuoteSystem.Default, QuoteSystemGuesser.Guess(MockedBookForQuoteSystem.GetMockedBooks(null)));
			sw.Stop();
			Assert.IsTrue(sw.ElapsedMilliseconds < 5200, "Actual time (ms): " + sw.ElapsedMilliseconds);
		}

		// TODO: Is there any kind of test (and/or better program logic) that will do a good job of dealing with the quote systems where the start and end tags are the same.
	}

	internal class MockedBookForQuoteSystem : IScrBook
	{
		private QuoteSystem m_desiredQuoteSystem;

		public static List<IScrBook> GetMockedBooks(QuoteSystem desiredQuoteSystem)
		{
			var mockedBooks = new List<IScrBook>();
			for (int i = 1; i < BCVRef.LastBook; i++)
				mockedBooks.Add(new MockedBookForQuoteSystem(i, desiredQuoteSystem));
			return mockedBooks;
		}

		public MockedBookForQuoteSystem(int bookNum, QuoteSystem desiredQuoteSystem)
		{
			m_desiredQuoteSystem = desiredQuoteSystem;
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
			var random = new Random();

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
			var randomizer = random.Next(50);
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
						startQuote = QuotePosition.OutOfOrder;
						endQuote = QuotePosition.OutOfOrder;
						break;
					case 8:
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
						endQuote = QuotePosition.EndOfVerse;
						break;
					case 18:
					case 19:
					case 20:
					case 21:
					case 22:
					case 23:
						startQuote = QuotePosition.None;
						break;
				}
			}

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
	}
}
