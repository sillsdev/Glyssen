using System.IO;
using System.Linq;
using Glyssen.Shared;
using GlyssenCharacters;
using GlyssenCharactersTests.Properties;
using GlyssenCharactersTests.Utilities;
using NUnit.Framework;
using SIL.IO;
using SIL.Scripture;

namespace GlyssenCharactersTests
{
	/// <summary>
	/// Note that these tests purport to test the GetCharacters method, but in fact that is just a simple LINQ statement;
	/// they're really testing the Load method.
	/// </summary>
	[TestFixture]
	class ControlCharacterVerseDataTests
	{
		private static readonly int kGENbookNum = BCVRef.BookToNumber("GEN");
		private static readonly int k1SAbookNum = BCVRef.BookToNumber("1SA");
		private static readonly int kMRKbookNum = BCVRef.BookToNumber("MRK");
		private static readonly int kLUKbookNum = BCVRef.BookToNumber("LUK");
		private static readonly int kACTbookNum = BCVRef.BookToNumber("ACT");

		private ScrVers m_testVersification;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerse;

			using (TempFile tempFile = new TempFile())
			{
				File.WriteAllText(tempFile.Path, Resources.TestVersification);
				m_testVersification = Versification.Table.Implementation.Load(tempFile.Path);
			}
		}

		[Test]
		public void GetCharacters_NoMatch_EmptyResults()
		{
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(kMRKbookNum, 1, new SingleVerse(1)).Any(), Is.False);
		}

		[Test]
		public void GetCharacters_OneInControlFile_Retrieved()
		{
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(kGENbookNum, 15, new SingleVerse(20)).Single().Character, Is.EqualTo("God"));
		}

		[Test]
		public void GetCharacters_VerseBridgeInControlFile_StartVerse()
		{
			var character = ControlCharacterVerseData.Singleton.GetCharacters(kLUKbookNum, 1, new SingleVerse(43)).Single();
			Assert.That(character.Character, Is.EqualTo("Elizabeth"));
		}

		[Test]
		public void GetCharacters_VerseBridgeInControlFile_MiddleVerse()
		{
			var character = ControlCharacterVerseData.Singleton.GetCharacters(kGENbookNum, 15, new SingleVerse(20)).Single();
			Assert.That(character.Character, Is.EqualTo("God"));
		}

		[Test]
		public void GetCharacters_VerseBridgeInControlFile_EndVerse()
		{
			var character = ControlCharacterVerseData.Singleton.GetCharacters(kLUKbookNum, 1, new SingleVerse(55)).Single();
			Assert.That(character.Character, Is.EqualTo("Mary (Jesus' mother)"));
		}

		[Test]
		public void GetCharacters_ControlHasNoDataForInitialStartVerseButDoesForSecondVerse_ReturnsNoCharacters()
		{
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(kACTbookNum, 11, new []{new SingleVerse(2), new SingleVerse(3) }).Any(), Is.False);
		}

		[Test]
		public void GetCharacters_ControlHasSameCharacterEntryForInitialStartAndInitialEndVerse_GetsOnlyOneEntryForCharacter()
		{
			var character = ControlCharacterVerseData.Singleton.GetCharacters(kGENbookNum, 1, new VerseBridge(14, 15)).Single();
			Assert.That(character.Character, Is.EqualTo("God"));
		}

		[Test]
		public void GetCharacters_OriginalVersification_StartVerseMovesToPreviousChapter_GettingNarratorOverridesDoesNotCrash()
		{
			// Note: There are no narrator overrides for GEN, so this test would also work for includeNarratorOverrides: false, 
			// but it is testing the case where we attempt to retrieve narrator overrides for a range which (after converting to
			// English versification), the verses are no longer in the same chapter.
			var bookNumGen = BCVRef.BookToNumber("GEN");
			// In English, GEN 31:55 => GEN 32:1 & GEN 32:1-32 => GEN 32:2-33
			var expected = ControlCharacterVerseData.Singleton.GetCharacters(bookNumGen, 31, new SingleVerse(55),
				ScrVers.English, includeNarratorOverrides: true);
			expected.UnionWith(ControlCharacterVerseData.Singleton.GetCharacters(bookNumGen, 32, new VerseBridge(1, 2),
				ScrVers.English, includeNarratorOverrides: true));
			var characters = ControlCharacterVerseData.Singleton.GetCharacters(bookNumGen, 32, new VerseBridge(1, 3),
				ScrVers.Original, includeNarratorOverrides: true);
			Assert.That(characters, Is.EquivalentTo(expected));
		}

		[TestCase(false)]
		[TestCase(true)]
		public void GetCharacters_RussianOrthodoxVersificationWithInitialVerseBridge_FindsCharacterAfterChangingVersification(bool includeNarratorOverrides)
		{
			// Psalm 37:16-17 in Russian Orthodox => Psalm 38:15-16 in English
			var expected = ControlCharacterVerseData.Singleton.GetCharacters(BCVRef.BookToNumber("PSA"), 38, new VerseBridge(15, 16),
				ScrVers.English, includeNarratorOverrides: includeNarratorOverrides);
			var characters = ControlCharacterVerseData.Singleton.GetCharacters(BCVRef.BookToNumber("PSA"), 37, new VerseBridge(16, 17),
				ScrVers.RussianOrthodox, includeNarratorOverrides: includeNarratorOverrides).ToList();
			Assert.That(characters, Is.EquivalentTo(expected));
		}

		[TestCase(2)]
		[TestCase(0)]
		[TestCase(3)]
		public void GetCharacters_RussianOrthodoxIncludeNarratorOverrides_FindsCharacterAfterChangingVersification(int endVerse)
		{
			var character = ControlCharacterVerseData.Singleton.GetCharacters(BCVRef.BookToNumber("PSA"), 41, endVerse <= 2 ? (IVerse)new SingleVerse(2) : new VerseBridge(2, endVerse),
				ScrVers.RussianOrthodox, includeNarratorOverrides: true).Single();

			// Psalm 41:2 in Russian Orthodox => Psalm 42:1 in English
			if (endVerse > 0)
				endVerse--;
			var expected = ControlCharacterVerseData.Singleton.GetCharacters(BCVRef.BookToNumber("PSA"), 42, endVerse <= 1 ? (IVerse)new SingleVerse(1) : new VerseBridge(1, endVerse),
				ScrVers.English, includeNarratorOverrides: true).Single();
			Assert.That(character, Is.EqualTo(expected));
		}

		[Test]
		public void GetCharacters_OriginalVersificationWithVerseBridge_FindsCharacterAfterChangingVersification()
		{
			// 1 Samuel 24:1-2 in Original => 1 Samuel 23:29-24:1 in English
			var expected = ControlCharacterVerseData.Singleton.GetCharacters(BCVRef.BookToNumber("1SA"), 24, new SingleVerse(1),
				ScrVers.English).Single();
			var character = ControlCharacterVerseData.Singleton.GetCharacters(BCVRef.BookToNumber("1SA"), 24, new VerseBridge(1, 2),
				ScrVers.Original).Single();
			Assert.That(character.Character, Is.EqualTo(expected.Character));
		}

		[Test]
		public void GetCharacters_ControlHasNoDataForInitialStartVerseButDoesForThirdVerse_ReturnsNoCharacters()
		{
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(kACTbookNum, 11, new []{ new SingleVerse(1), new SingleVerse(3) }).Any(), Is.False);
		}

		[Test]
		public void GetCharacters_MoreThanOneWithNoDuplicates_ReturnsAll()
		{
			var characters = ControlCharacterVerseData.Singleton.GetCharacters(kMRKbookNum, 6, new SingleVerse(24));
			Assert.That(characters.Count, Is.EqualTo(2));
			Assert.That(characters.Count(c => c.Character == "Herodias"), Is.EqualTo(1));
			Assert.That(characters.Count(c => c.Character == "Herodias' daughter"), Is.EqualTo(1));
		}

		[Test]
		public void GetCharacters_MultipleCharactersInOneButNotAllVerses_ReturnsSingleCharacter()
		{
			var character = ControlCharacterVerseData.Singleton.GetCharacters(k1SAbookNum, 6, new[] { new SingleVerse(4), new SingleVerse(6) }).Single();
			Assert.That(character.Character, Is.EqualTo("Philistine priests and diviners"));
		}

		[Test]
		public void GetCharacters_MultipleCharactersInMultipleVerses_ReturnsAmbiguous()
		{
			var characters = ControlCharacterVerseData.Singleton.GetCharacters(k1SAbookNum, 8, new[] { new SingleVerse(21), new SingleVerse(22) });
			Assert.That(characters.Count, Is.EqualTo(2));
			Assert.That(characters.Count(c => c.Character == "God"), Is.EqualTo(1));
			Assert.That(characters.Count(c => c.Character == "Samuel"), Is.EqualTo(1));
		}

		[Test]
		public void GetCharacters_MultipleCharactersInMultipleVerses_NoCharacterInInitialStartVerse_ReturnsNoCharacters()
		{
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(k1SAbookNum, 8, new[] { new SingleVerse(20), new SingleVerse(22) }).Any(), Is.False);
		}

		[Test]
		public void GetCharacters_SingleCharactersInMultipleVerses_NoCharacterInInitialStartVerse_ReturnsNoCharacters()
		{
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(k1SAbookNum, 9, new[] { new SingleVerse(4), new SingleVerse(6) }).Any(), Is.False);
		}

		[Test]
		public void GetCharacters_NonEnglishVersification()
		{
			// Prove the test is valid
			var character = ControlCharacterVerseData.Singleton.GetCharacters(1, 32, new SingleVerse(6)).Single();
			Assert.That(character.Character, Is.EqualTo("messengers of Jacob"));
			var verseRef = new VerseRef(1, 32, 6, ScrVers.English);
			verseRef.ChangeVersification(m_testVersification);
			Assert.That(verseRef.BookNum, Is.EqualTo(1));
			Assert.That(verseRef.ChapterNum, Is.EqualTo(32));
			Assert.That(verseRef.VerseNum, Is.EqualTo(7));

			// Run the test
			character = ControlCharacterVerseData.Singleton.GetCharacters(verseRef.BookNum, verseRef.ChapterNum, (SingleVerse)verseRef, m_testVersification).Single();
			Assert.That(character.Character, Is.EqualTo("messengers of Jacob"));
		}

		[Test]
		public void GetCharacters_NormalDeliveryPlusAlternate_GetsBothDeliveries()
		{
			var characters = ControlCharacterVerseData.Singleton.GetCharacters(BCVRef.BookToNumber("JER"), 31, new SingleVerse(18),
				includeAlternatesAndRareQuotes: true);
			Assert.That(characters.Count, Is.EqualTo(3));
			Assert.That(characters.Where(c => c.Character == "God" && c.Delivery == ""),
				Is.Not.Empty);
			Assert.That(characters.Where(c => c.Character == "God" && c.Delivery != "" &&
				c.QuoteType == QuoteType.Alternate), Is.Not.Empty);
			Assert.That(characters.Where(c => c.Character == "Ephraim" &&
				c.QuoteType == QuoteType.Quotation), Is.Not.Empty);
		}
	}

	[TestFixture]
	class ControlCharacterVerseDataTests_Oct2015
	{
		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerseOct2015;
		}

		[Test]
		public void ExpectedQuotes_ScriptureQuotation_IsExpected()
		{
			// ACT	8	32	scripture			Quotation	Isaiah	
			Assert.That(ControlCharacterVerseData.Singleton
				.ExpectedQuotes[BCVRef.BookToNumber("ACT")][8], Does.Contain(32));
		}

		[Test]
		public void ExpectedQuotes_ScriptureQuotationAccompaniedByRareNeedsReview_IsNotExpected()
		{
			// MAT	9	36	narrator-MAT			Quotation		
			// MAT	9	36	scripture			Quotation	Micaiah, prophet of the LORD	
			// MAT	9	36	Jesus			Rare		
			// MAT	9	36	Needs Review			Rare		
			Assert.That(ControlCharacterVerseData.Singleton
				.ExpectedQuotes[BCVRef.BookToNumber("MAT")][9], Does.Not.Contain(36));
		}

		[Test]
		public void ExpectedQuotes_QuotationDefaultedToNarrator_IsNotExpected()
		{
			// 1KI	12	12	Rehoboam, king			Quotation	narrator-1KI	
			Assert.That(ControlCharacterVerseData.Singleton
				.ExpectedQuotes[BCVRef.BookToNumber("1KI")][12], Does.Not.Contain(12));
		}

		[Test]
		public void ExpectedQuotes_Normal_IsExpected()
		{
			//GEN 1   11  God God(the LORD)  Normal
			Assert.That(ControlCharacterVerseData.Singleton
				.ExpectedQuotes[BCVRef.BookToNumber("GEN")][1], Does.Contain(11));
		}

		[Test]
		public void ExpectedQuotes_Dialogue_IsExpected()
		{
			// 2SA 9   2   David David, king Dialogue
			// 2SA 9   2   Ziba, servant of Saul's household			Dialogue		
			Assert.That(ControlCharacterVerseData.Singleton
				.ExpectedQuotes[BCVRef.BookToNumber("2SA")][9], Does.Contain(2));
		}

		[Test]
		public void ExpectedQuotes_Hypothetical_IsNotExpected()
		{
			// Every line in the control file for PSA 10 is Hypothetical
			Assert.That(ControlCharacterVerseData.Singleton
				.ExpectedQuotes[BCVRef.BookToNumber("PSA")].ContainsKey(10), Is.False);
		}

		[Test]
		public void ExpectedQuotes_PotentialAndIndirect_IsNotExpected()
		{
			// ACT 8   37  Ethiopian officer of Queen Candace Indirect
			// ACT 8   37  Philip the evangelist Philip  Potential
			Assert.That(ControlCharacterVerseData.Singleton
				.ExpectedQuotes[BCVRef.BookToNumber("ACT")][8], Does.Not.Contain(37));
		}

		// Note: We didn't used to consider Implicit as expected, but now that we're marking more things as implicit (so we can
		// take advantage of the improved logic in the quote parser for implicit quotes), it seems better to regard them as
		// expected. They really are expected, and most languages that regularly use quotation marks will have them for most
		// implicit passages.
		[Test]
		public void ExpectedQuotes_Implicit_IsExpected()
		{
			// Every line in the control file for LEV 1 is Implicit.
			Assert.That(ControlCharacterVerseData.Singleton
				.ExpectedQuotes[BCVRef.BookToNumber("LEV")].ContainsKey(1), Is.True);
		}

		[Test]
		public void GetUniqueCharacterDeliveryAliasInfo_DoesNotContainInterruption()
		{
			Assert.That(ControlCharacterVerseData.Singleton.GetUniqueCharacterDeliveryAliasInfo()
				.Any(c => c.Character.StartsWith("interruption-")), Is.False);
		}

		[Test]
		public void GetUniqueCharacterDeliveryInfo_DoesNotContainInterruption()
		{
			Assert.That(ControlCharacterVerseData.Singleton.GetUniqueCharacterDeliveryInfo("JHN")
				.Any(c => c.Character.StartsWith("interruption-")), Is.False);
		}
	}
}
