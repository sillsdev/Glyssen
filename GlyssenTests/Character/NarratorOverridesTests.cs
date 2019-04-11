using System;
using System.Linq;
using Glyssen.Character;
using NUnit.Framework;
using SIL.Scripture;

namespace GlyssenTests.Character
{
	// Note: NarratorOverrides does not currently allow the tests to jam in an alternate version of the control-file data in order
	// to ensure that tests will not be broken by future data changes. My expectation is that the control-file data for the
	// NarratorOverrides class will be pretty static, so these tests are not likely to be brittle. If tests are broken by future
	// changes and cannot be easily updated, we could use the same strategy used in the RelatedCharactersTests, where we set
	// RelatedCharactersData.Source to a test version of that file that doesn't change. Also note that some of these tests are
	// dependent on standard versifications that are not expected to change.
	class NarratorOverridesTests
	{
		[Test]
		public void GetCharacterOverrideDetailsForRefRange_BookWithNoOverrides_ReturnsEmpty()
		{
			var genesis1_1 = new VerseRef(001001001, ScrVers.English);
			Assert.IsFalse(NarratorOverrides.GetCharacterOverrideDetailsForRefRange(genesis1_1, 1).Any());
		}

		[Test]
		public void GetCharacterOverrideDetailsForRefRange_ChapterBeforeAnyOverrideInBook_ReturnsEmpty()
		{
			var bookNum = BCVRef.BookToNumber("PSA");
			var psalm1_1 = new VerseRef(new BCVRef(bookNum, 1, 1), ScrVers.English);
			Assert.IsFalse(NarratorOverrides.GetCharacterOverrideDetailsForRefRange(psalm1_1, 1).Any());
		}

		[Test]
		public void GetCharacterOverrideDetailsForRefRange_ChapterAfterAnyOverrideInBook_ReturnsEmpty()
		{
			var bookNum = BCVRef.BookToNumber("EZR");
			var ezra10_1 = new VerseRef(new BCVRef(bookNum, 10, 1), ScrVers.English);
			Assert.IsFalse(NarratorOverrides.GetCharacterOverrideDetailsForRefRange(ezra10_1, 1).Any());
		}

		[Test]
		public void GetCharacterOverrideDetailsForRefRange_VerseBeforeFirstVerseInOverriddenChapter_ReturnsEmpty()
		{
			var bookNum = BCVRef.BookToNumber("EZR");
			var ezra7_26 = new VerseRef(new BCVRef(bookNum, 7, 26), ScrVers.English);
			Assert.IsFalse(NarratorOverrides.GetCharacterOverrideDetailsForRefRange(ezra7_26, 26).Any());
		}

		[Test]
		public void GetCharacterOverrideDetailsForRefRange_VerseAfterLastVerseInOverriddenChapter_ReturnsEmpty()
		{
			var bookNum = BCVRef.BookToNumber("NEH");
			var nehemiah7_6 = new VerseRef(new BCVRef(bookNum, 7, 6), ScrVers.English);
			Assert.IsFalse(NarratorOverrides.GetCharacterOverrideDetailsForRefRange(nehemiah7_6, 6).Any());
		}

		[Test]
		public void GetCharacterOverrideDetailsForRefRange_LastVerseInOverride_ReturnsCorrectOverrideCharacter()
		{
			var bookNum = BCVRef.BookToNumber("NEH");
			var nehemiah7_5 = new VerseRef(new BCVRef(bookNum, 7, 5), ScrVers.English);
			Assert.AreEqual("Nehemiah",
				NarratorOverrides.GetCharacterOverrideDetailsForRefRange(nehemiah7_5, 5).Single().Character);
		}

		[Test]
		public void GetCharacterOverrideDetailsForRefRange_FirstVerseInOverride_ReturnsCorrectOverrideCharacter()
		{
			var bookNum = BCVRef.BookToNumber("EZR");
			var ezra7_27 = new VerseRef(new BCVRef(bookNum, 7, 27), ScrVers.English);
			Assert.AreEqual("Ezra, priest and teacher",
				NarratorOverrides.GetCharacterOverrideDetailsForRefRange(ezra7_27, 27).Single().Character);
		}

		[TestCase("PSA", 42, 4, ExpectedResult = "sons of Korah")]
		[TestCase("EZR", 9, 5, ExpectedResult = "Ezra, priest and teacher")]
		[TestCase("NEH", 12, 38, ExpectedResult = "Nehemiah")]
		public string GetCharacterOverrideDetailsForRefRange_MidVerseInSingleChapterOverride_ReturnsCorrectOverrideCharacter(string bookId, int chapter, int verse)
		{
			var verseRef = new VerseRef(new BCVRef(BCVRef.BookToNumber(bookId), chapter, verse), ScrVers.English);
			return NarratorOverrides.GetCharacterOverrideDetailsForRefRange(verseRef, verse).Single().Character;
		}

		[TestCase("PSA", 31, -1, ExpectedResult = "David")]
		[TestCase("EZR", 8, 1, ExpectedResult = "Ezra, priest and teacher")]
		[TestCase("EZR", 8, -1, ExpectedResult = "Ezra, priest and teacher")]
		public string GetCharacterOverrideDetailsForRefRange_VerseInMidChapterInMultiChapterOverride_ReturnsCorrectOverrideCharacter(string bookId, int chapter, int verse)
		{
			var bookNum = BCVRef.BookToNumber(bookId);
			if (verse == -1)
				verse = ScrVers.English.GetLastVerse(bookNum, chapter);
			var verseRef = new VerseRef(new BCVRef(bookNum, chapter, verse), ScrVers.English);
			return NarratorOverrides.GetCharacterOverrideDetailsForRefRange(verseRef, verse).Single().Character;
		}

		[TestCase("PSA", 31, 1, -1, ExpectedResult = "David")]
		[TestCase("EZR", 8, 1, -1, ExpectedResult = "Ezra, priest and teacher")]
		[TestCase("NEH", 12, 30, 42, ExpectedResult = "Nehemiah")]
		public string GetCharacterOverrideDetailsForRefRange_RangeWithinOverride_ReturnsCorrectOverrideCharacter(string bookId, int chapter, int startVerse, int endVerse)
		{
			var bookNum = BCVRef.BookToNumber(bookId);
			if (endVerse == -1)
				endVerse = ScrVers.English.GetLastVerse(bookNum, chapter);
			var startRef = new VerseRef(new BCVRef(bookNum, chapter, startVerse), ScrVers.English);
			return NarratorOverrides.GetCharacterOverrideDetailsForRefRange(startRef, endVerse).Single().Character;
		}

		[TestCase("EZR", 7, 26, 28)]
		[TestCase("NEH", 7, 5, 6)]
		public void GetCharacterOverrideDetailsForRefRange_RangeOnlyPartiallyWithinOverride_ReturnsEmpty(string bookId, int chapter, int startVerse, int endVerse)
		{
			var bookNum = BCVRef.BookToNumber(bookId);
			var verseRef = new VerseRef(new BCVRef(bookNum, chapter, startVerse), ScrVers.English);
			Assert.IsFalse(NarratorOverrides.GetCharacterOverrideDetailsForRefRange(verseRef, endVerse).Any());
		}

		[Test]
		public void GetCharacterOverrideDetailsForRefRange_RangeStartsInVerseZeroInMultiChapterOverride_ReturnsEmpty()
		{
			var bookNum = BCVRef.BookToNumber("PSA");
			var verseRef = new VerseRef(new BCVRef(bookNum, 45, 0), ScrVers.English);
			Assert.IsFalse(NarratorOverrides.GetCharacterOverrideDetailsForRefRange(verseRef, 2).Any());
		}

		[Test]
		public void GetCharacterOverrideDetailsForRefRange_EndVerseLessThanStartVerse_ThrowsArgumentOutOfRangeException()
		{
			var bookNum = BCVRef.BookToNumber("PSA");
			var verseRef = new VerseRef(new BCVRef(bookNum, 45, 2), ScrVers.English);
			Assert.Throws<ArgumentOutOfRangeException>(() => NarratorOverrides.GetCharacterOverrideDetailsForRefRange(verseRef, 0));
			Assert.Throws<ArgumentOutOfRangeException>(() => NarratorOverrides.GetCharacterOverrideDetailsForRefRange(verseRef, 1));
		}

		[TestCase("NEH", 4, 1, ExpectedResult = "Nehemiah")] // Should map to NEH 4:7
		[TestCase("NEH", 4, 23, ExpectedResult = "Nehemiah")] // Should map to NEH 4:17
		public string GetCharacterOverrideDetailsForRefRange_OriginalVersificationVerseOutsideOfOverrideRangeBeforeMappingToEnglish_MapsToReturnCorrectOverrideCharacter(string bookId, int chapter, int verse)
		{
			var bookNum = BCVRef.BookToNumber(bookId);
			var verseRef = new VerseRef(new BCVRef(bookNum, chapter, verse), ScrVers.Original);
			return NarratorOverrides.GetCharacterOverrideDetailsForRefRange(verseRef, verse).Single().Character;
		}

		[TestCase("PSA", 33, 23, ExpectedResult = "David")] // Should map to PSA 34:23 (Original) -> PSA 34:22 (English)
		public string GetCharacterOverrideDetailsForRefRange_VulgateVersificationChapterOutsideOfOverrideRangeBeforeMappingToEnglish_MapsToReturnCorrectOverrideCharacter(string bookId, int chapter, int verse)
		{
			var bookNum = BCVRef.BookToNumber(bookId);
			var verseRef = new VerseRef(new BCVRef(bookNum, chapter, verse), ScrVers.Vulgate);
			return NarratorOverrides.GetCharacterOverrideDetailsForRefRange(verseRef, verse).Single().Character;
		}

		[TestCase("PSA", 32, 1)] // Should map to PSA 33:1
		[TestCase("PSA", 32, -1)] // Should map to last verse in PSA 33
		public void GetCharacterOverrideDetailsForRefRange_VulgateVersificationChapterInsideOfOverrideRangeBeforeMappingToEnglish_MapsToReturnEmpty(string bookId, int chapter, int verse)
		{
			var bookNum = BCVRef.BookToNumber(bookId);
			if (verse == -1)
				verse = ScrVers.Vulgate.GetLastVerse(bookNum, chapter);
			var verseRef = new VerseRef(new BCVRef(bookNum, chapter, verse), ScrVers.Vulgate);
			Assert.IsFalse(NarratorOverrides.GetCharacterOverrideDetailsForRefRange(verseRef, verse).Any());
		}
	}
}