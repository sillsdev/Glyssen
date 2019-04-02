using System;
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
		public void GetCharacterOverrideForRefRange_BookWithNoOverrides_ReturnsNull()
		{
			var genesis1_1 = new VerseRef(001001001, ScrVers.English);
			Assert.IsNull(NarratorOverrides.GetCharacterOverrideForRefRange(genesis1_1, 1));
		}

		[Test]
		public void GetCharacterOverrideForRefRange_ChapterBeforeAnyOverrideInBook_ReturnsNull()
		{
			var bookNum = BCVRef.BookToNumber("PSA");
			var psalm1_1 = new VerseRef(new BCVRef(bookNum, 1, 1), ScrVers.English);
			Assert.IsNull(NarratorOverrides.GetCharacterOverrideForRefRange(psalm1_1, 1));
		}

		[Test]
		public void GetCharacterOverrideForRefRange_ChapterAfterAnyOverrideInBook_ReturnsNull()
		{
			var bookNum = BCVRef.BookToNumber("EZR");
			var ezra10_1 = new VerseRef(new BCVRef(bookNum, 10, 1), ScrVers.English);
			Assert.IsNull(NarratorOverrides.GetCharacterOverrideForRefRange(ezra10_1, 1));
		}

		[Test]
		public void GetCharacterOverrideForRefRange_VerseBeforeFirstVerseInOverriddenChapter_ReturnsNull()
		{
			var bookNum = BCVRef.BookToNumber("EZR");
			var ezra7_26 = new VerseRef(new BCVRef(bookNum, 7, 26), ScrVers.English);
			Assert.IsNull(NarratorOverrides.GetCharacterOverrideForRefRange(ezra7_26, 26));
		}

		[Test]
		public void GetCharacterOverrideForRefRange_VerseAfterLastVerseInOverriddenChapter_ReturnsNull()
		{
			var bookNum = BCVRef.BookToNumber("NEH");
			var nehemiah7_6 = new VerseRef(new BCVRef(bookNum, 7, 6), ScrVers.English);
			Assert.IsNull(NarratorOverrides.GetCharacterOverrideForRefRange(nehemiah7_6, 6));
		}

		[Test]
		public void GetCharacterOverrideForRefRange_LastVerseInOverride_ReturnsCorrectOverrideCharacter()
		{
			var bookNum = BCVRef.BookToNumber("NEH");
			var nehemiah7_5 = new VerseRef(new BCVRef(bookNum, 7, 5), ScrVers.English);
			Assert.AreEqual("Nehemiah", NarratorOverrides.GetCharacterOverrideForRefRange(nehemiah7_5, 5));
		}

		[Test]
		public void GetCharacterOverrideForRefRange_FirstVerseInOverride_ReturnsCorrectOverrideCharacter()
		{
			var bookNum = BCVRef.BookToNumber("EZR");
			var ezra7_27 = new VerseRef(new BCVRef(bookNum, 7, 27), ScrVers.English);
			Assert.AreEqual("Ezra, priest and teacher", NarratorOverrides.GetCharacterOverrideForRefRange(ezra7_27, 27));
		}

		[TestCase("PSA", 42, 4, ExpectedResult = "sons of Korah")]
		[TestCase("EZR", 9, 5, ExpectedResult = "Ezra, priest and teacher")]
		[TestCase("NEH", 12, 38, ExpectedResult = "Nehemiah")]
		public string GetCharacterOverrideForRefRange_MidVerseInSingleChapterOverride_ReturnsCorrectOverrideCharacter(string bookId, int chapter, int verse)
		{
			var verseRef = new VerseRef(new BCVRef(BCVRef.BookToNumber(bookId), chapter, verse), ScrVers.English);
			return NarratorOverrides.GetCharacterOverrideForRefRange(verseRef, verse);
		}

		[TestCase("PSA", 31, -1, ExpectedResult = "David")]
		[TestCase("EZR", 8, 1, ExpectedResult = "Ezra, priest and teacher")]
		[TestCase("EZR", 8, -1, ExpectedResult = "Ezra, priest and teacher")]
		public string GetCharacterOverrideForRefRange_VerseInMidChapterInMultiChapterOverride_ReturnsCorrectOverrideCharacter(string bookId, int chapter, int verse)
		{
			var bookNum = BCVRef.BookToNumber(bookId);
			if (verse == -1)
				verse = ScrVers.English.GetLastVerse(bookNum, chapter);
			var verseRef = new VerseRef(new BCVRef(bookNum, chapter, verse), ScrVers.English);
			return NarratorOverrides.GetCharacterOverrideForRefRange(verseRef, verse);
		}

		[TestCase("PSA", 31, 1, -1, ExpectedResult = "David")]
		[TestCase("EZR", 8, 1, -1, ExpectedResult = "Ezra, priest and teacher")]
		[TestCase("NEH", 12, 30, 42, ExpectedResult = "Nehemiah")]
		public string GetCharacterOverrideForRefRange_RangeWithinOverride_ReturnsCorrectOverrideCharacter(string bookId, int chapter, int startVerse, int endVerse)
		{
			var bookNum = BCVRef.BookToNumber(bookId);
			if (endVerse == -1)
				endVerse = ScrVers.English.GetLastVerse(bookNum, chapter);
			var startRef = new VerseRef(new BCVRef(bookNum, chapter, startVerse), ScrVers.English);
			return NarratorOverrides.GetCharacterOverrideForRefRange(startRef, endVerse);
		}

		[TestCase("EZR", 7, 26, 28)]
		[TestCase("NEH", 7, 5, 6)]
		public void GetCharacterOverrideForRefRange_RangeOnlyPartiallyWithinOverride_ReturnsNull(string bookId, int chapter, int startVerse, int endVerse)
		{
			var bookNum = BCVRef.BookToNumber(bookId);
			var verseRef = new VerseRef(new BCVRef(bookNum, chapter, startVerse), ScrVers.English);
			Assert.IsNull(NarratorOverrides.GetCharacterOverrideForRefRange(verseRef, endVerse));
		}

		[Test]
		public void GetCharacterOverrideForRefRange_RangeStartsInVerseZeroInMultiChapterOverride_ReturnsNull()
		{
			var bookNum = BCVRef.BookToNumber("PSA");
			var verseRef = new VerseRef(new BCVRef(bookNum, 45, 0), ScrVers.English);
			Assert.IsNull(NarratorOverrides.GetCharacterOverrideForRefRange(verseRef, 2));
		}

		[Test]
		public void GetCharacterOverrideForRefRange_EndVerseLessThanStartVerse_ThrowsArgumentOutOfRangeException()
		{
			var bookNum = BCVRef.BookToNumber("PSA");
			var verseRef = new VerseRef(new BCVRef(bookNum, 45, 2), ScrVers.English);
			Assert.Throws<ArgumentOutOfRangeException>(() => NarratorOverrides.GetCharacterOverrideForRefRange(verseRef, 0));
			Assert.Throws<ArgumentOutOfRangeException>(() => NarratorOverrides.GetCharacterOverrideForRefRange(verseRef, 1));
		}

		[TestCase("NEH", 4, 1, ExpectedResult = "Nehemiah")] // Should map to NEH 4:7
		[TestCase("NEH", 4, 23, ExpectedResult = "Nehemiah")] // Should map to NEH 4:17
		public string GetCharacterOverrideForRefRange_OriginalVersificationVerseOutsideOfOverrideRangeBeforeMappingToEnglish_MapsToReturnCorrectOverrideCharacter(string bookId, int chapter, int verse)
		{
			var bookNum = BCVRef.BookToNumber(bookId);
			var verseRef = new VerseRef(new BCVRef(bookNum, chapter, verse), ScrVers.Original);
			return NarratorOverrides.GetCharacterOverrideForRefRange(verseRef, verse);
		}

		[TestCase("PSA", 33, 23, ExpectedResult = "David")] // Should map to PSA 34:23 (Original) -> PSA 34:22 (English)
		public string GetCharacterOverrideForRefRange_VulgateVersificationChapterOutsideOfOverrideRangeBeforeMappingToEnglish_MapsToReturnCorrectOverrideCharacter(string bookId, int chapter, int verse)
		{
			var bookNum = BCVRef.BookToNumber(bookId);
			var verseRef = new VerseRef(new BCVRef(bookNum, chapter, verse), ScrVers.Vulgate);
			return NarratorOverrides.GetCharacterOverrideForRefRange(verseRef, verse);
		}

		[TestCase("PSA", 32, 1)] // Should map to PSA 33:1
		[TestCase("PSA", 32, -1)] // Should map to last verse in PSA 33
		public void GetCharacterOverrideForRefRange_VulgateVersificationChapterInsideOfOverrideRangeBeforeMappingToEnglish_MapsToReturnNull(string bookId, int chapter, int verse)
		{
			var bookNum = BCVRef.BookToNumber(bookId);
			if (verse == -1)
				verse = ScrVers.Vulgate.GetLastVerse(bookNum, chapter);
			var verseRef = new VerseRef(new BCVRef(bookNum, chapter, verse), ScrVers.Vulgate);
			Assert.IsNull(NarratorOverrides.GetCharacterOverrideForRefRange(verseRef, verse));
		}
	}
}