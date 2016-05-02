using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Glyssen;
using Glyssen.Bundle;
using Glyssen.Character;
using Glyssen.Dialogs;
using Glyssen.Rules;
using GlyssenTests.Properties;
using NUnit.Framework;
using SIL.Scripture;
using SIL.WritingSystems;

namespace GlyssenTests.Dialogs
{
	[TestFixture]
	class CastSizePlanningViewModelTests
	{
		private int m_maleNarratorsValue = -1;
		private int m_femaleNarratorsValue = -1;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerseOct2015;
			CharacterDetailData.TabDelimitedCharacterDetailData = Resources.TestCharacterDetailOct2015;
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			TestProject.DeleteTestProjectFolder();
		}

		[Test]
		public void SetNarratorOption_CallbackPublishesValues()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.PHM, TestProject.TestBook.IIIJN);
			CastSizePlanningViewModel model = new CastSizePlanningViewModel(testProject);
			model.MaleNarratorsValueChanged += model_MaleNarratorsValueChanged;
			model.FemaleNarratorsValueChanged += model_FemaleNarratorsValueChanged;
			model.NarratorOption = NarratorsOption.Custom;
			model.MaleNarrators = 1;
			model.FemaleNarrators = 1;
			model.NarratorOption = NarratorsOption.NarrationByAuthor;
			Assert.AreEqual(2, m_maleNarratorsValue);
			Assert.AreEqual(0, m_femaleNarratorsValue);
			model.NarratorOption = NarratorsOption.SingleNarrator;
			Assert.AreEqual(1, m_maleNarratorsValue);
			Assert.AreEqual(0, m_femaleNarratorsValue);
			model.NarratorOption = NarratorsOption.Custom;
			Assert.AreEqual(1, m_maleNarratorsValue);
			Assert.AreEqual(1, m_femaleNarratorsValue);
		}

		private void model_MaleNarratorsValueChanged(object sender, int newValue)
		{
			m_maleNarratorsValue = newValue;
		}

		private void model_FemaleNarratorsValueChanged(object sender, int newValue)
		{
			m_femaleNarratorsValue = newValue;
		}

		[Test]
		public void GetCastSizeRowValues_ProjectHasSingleBookWithNoSpeakingParts_AllCastSizesHaveTwoMalesForNarratorAndExtra()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.PHM);
			CastSizePlanningViewModel model = new CastSizePlanningViewModel(testProject);

			var smallCast = model.GetCastSizeRowValues(CastSizeOption.Small);
			Assert.AreEqual(2, smallCast.Male);
			Assert.AreEqual(0, smallCast.Female);
			Assert.AreEqual(0, smallCast.Child);
			Assert.AreEqual(2, smallCast.Total);

			var recommendedCast = model.GetCastSizeRowValues(CastSizeOption.Recommended);
			Assert.AreEqual(2, recommendedCast.Male);
			Assert.AreEqual(0, recommendedCast.Female);
			Assert.AreEqual(0, recommendedCast.Child);
			Assert.AreEqual(2, recommendedCast.Total);

			var largeCast = model.GetCastSizeRowValues(CastSizeOption.Large);
			Assert.AreEqual(2, largeCast.Male);
			Assert.AreEqual(0, largeCast.Female);
			Assert.AreEqual(0, largeCast.Child);
			Assert.AreEqual(2, largeCast.Total);
		}

		[Test]
		public void GetCastSizeRowValues_ProjectHasTwoBooksByDifferentAuthorsWithNoSpeakingParts_AllCastSizesHaveThreeMalesForNarratorAndExtra()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.PHM, TestProject.TestBook.IIIJN);
			CastSizePlanningViewModel model = new CastSizePlanningViewModel(testProject);
			model.NarratorOption = NarratorsOption.NarrationByAuthor;
			testProject.DramatizationPreferences.SectionHeadDramatization = ExtraBiblicalMaterialSpeakerOption.MaleActor;
			testProject.DramatizationPreferences.BookTitleAndChapterDramatization = ExtraBiblicalMaterialSpeakerOption.MaleActor;

			var smallCast = model.GetCastSizeRowValues(CastSizeOption.Small);
			Assert.AreEqual(3, smallCast.Male);
			Assert.AreEqual(0, smallCast.Female);
			Assert.AreEqual(0, smallCast.Child);
			Assert.AreEqual(3, smallCast.Total);

			var recommendedCast = model.GetCastSizeRowValues(CastSizeOption.Recommended);
			Assert.AreEqual(3, recommendedCast.Male);
			Assert.AreEqual(0, recommendedCast.Female);
			Assert.AreEqual(0, recommendedCast.Child);
			Assert.AreEqual(3, recommendedCast.Total);

			var largeCast = model.GetCastSizeRowValues(CastSizeOption.Large);
			Assert.AreEqual(3, largeCast.Male);
			Assert.AreEqual(0, largeCast.Female);
			Assert.AreEqual(0, largeCast.Child);
			Assert.AreEqual(3, largeCast.Total);
		}

		[Test]
		public void
			GetCastSizeRowValues_ProjectHasTwoBooksBySameAuthorsWithNoSpeakingParts_AllCastSizesHaveTwoMalesForNarratorAndExtra()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.IIJN, TestProject.TestBook.IIIJN);
			CastSizePlanningViewModel model = new CastSizePlanningViewModel(testProject);
			model.NarratorOption = NarratorsOption.NarrationByAuthor;

			var smallCast = model.GetCastSizeRowValues(CastSizeOption.Small);
			Assert.AreEqual(2, smallCast.Male);
			Assert.AreEqual(0, smallCast.Female);
			Assert.AreEqual(0, smallCast.Child);
			Assert.AreEqual(2, smallCast.Total);

			var recommendedCast = model.GetCastSizeRowValues(CastSizeOption.Recommended);
			Assert.AreEqual(2, recommendedCast.Male);
			Assert.AreEqual(0, recommendedCast.Female);
			Assert.AreEqual(0, recommendedCast.Child);
			Assert.AreEqual(2, recommendedCast.Total);

			var largeCast = model.GetCastSizeRowValues(CastSizeOption.Large);
			Assert.AreEqual(2, largeCast.Male);
			Assert.AreEqual(0, largeCast.Female);
			Assert.AreEqual(0, largeCast.Child);
			Assert.AreEqual(2, largeCast.Total);
		}

		[Test]
		public void
			GetCastSizeRowValues_ProjectHasTwoBooksWithNoSpeakingPartsCustomNarratorValues_AllCastSizesHaveBasedOnCustomNarratorsPlusExtraMale
			()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.IIJN, TestProject.TestBook.IIIJN);
			CastSizePlanningViewModel model = new CastSizePlanningViewModel(testProject);
			model.NarratorOption = NarratorsOption.Custom;
			model.MaleNarrators = 1;
			model.FemaleNarrators = 1;

			var smallCast = model.GetCastSizeRowValues(CastSizeOption.Small);
			Assert.AreEqual(2, smallCast.Male);
			Assert.AreEqual(1, smallCast.Female);
			Assert.AreEqual(0, smallCast.Child);
			Assert.AreEqual(3, smallCast.Total);

			var recommendedCast = model.GetCastSizeRowValues(CastSizeOption.Recommended);
			Assert.AreEqual(2, recommendedCast.Male);
			Assert.AreEqual(1, recommendedCast.Female);
			Assert.AreEqual(0, recommendedCast.Child);
			Assert.AreEqual(3, recommendedCast.Total);

			var largeCast = model.GetCastSizeRowValues(CastSizeOption.Large);
			Assert.AreEqual(2, largeCast.Male);
			Assert.AreEqual(1, largeCast.Female);
			Assert.AreEqual(0, largeCast.Child);
			Assert.AreEqual(3, largeCast.Total);
		}

		[Test]
		public void GetCastSizeRowValues_ProjectHasSingleBookWithThreeMaleSpeakingPartsCloseTogether_AllCastSizesHaveFiveMales
			()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.JUD);
			CastSizePlanningViewModel model = new CastSizePlanningViewModel(testProject);

			var smallCast = model.GetCastSizeRowValues(CastSizeOption.Small);
			Assert.AreEqual(5, smallCast.Male);
			Assert.AreEqual(0, smallCast.Female);
			Assert.AreEqual(0, smallCast.Child);
			Assert.AreEqual(5, smallCast.Total);

			var recommendedCast = model.GetCastSizeRowValues(CastSizeOption.Recommended);
			Assert.AreEqual(5, recommendedCast.Male);
			Assert.AreEqual(0, recommendedCast.Female);
			Assert.AreEqual(0, recommendedCast.Child);
			Assert.AreEqual(5, recommendedCast.Total);

			var largeCast = model.GetCastSizeRowValues(CastSizeOption.Large);
			Assert.AreEqual(5, largeCast.Male);
			Assert.AreEqual(0, largeCast.Female);
			Assert.AreEqual(0, largeCast.Child);
			Assert.AreEqual(5, largeCast.Total);
		}

		[Test]
		public void GetCastSizeRowValues_ProjectHasSingleBookWithManySpeakingPartsNoChildren_NoChildrenInCastSizes()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.ACT);
			TestProject.SimulateDisambiguationForAllBooks(testProject);
			CastSizePlanningViewModel model = new CastSizePlanningViewModel(testProject);

			var smallCast = model.GetCastSizeRowValues(CastSizeOption.Small);
			Assert.AreEqual(15, smallCast.Male);
			Assert.AreEqual(2, smallCast.Female);
			Assert.AreEqual(0, smallCast.Child);
			Assert.AreEqual(17, smallCast.Total);

			var recommendedCast = model.GetCastSizeRowValues(CastSizeOption.Recommended);
			Assert.AreEqual(22, recommendedCast.Male);
			Assert.AreEqual(3, recommendedCast.Female);
			Assert.AreEqual(0, recommendedCast.Child);
			Assert.AreEqual(25, recommendedCast.Total);

			var largeCast = model.GetCastSizeRowValues(CastSizeOption.Large);
			Assert.AreEqual(29, largeCast.Male); // TODO: Talk to Duane about logic for this
			Assert.AreEqual(4, largeCast.Female);
			Assert.AreEqual(0, largeCast.Child);
			Assert.AreEqual(33, largeCast.Total);
		}

		[TestCase(DramatizationOption.DedicatedCharacter, 2)]
		[TestCase(DramatizationOption.DefaultCharacter, 4, Ignore = true, IgnoreReason = "Not fully implemented")]
		[TestCase(DramatizationOption.Narrator, 1, Ignore = true, IgnoreReason = "Not fully implemented")]
		public void GetCastSizeRowValues_HebrewsWithLotsOfScriptureQuotes_CastSizeDependsOnHowScriptureQuotationsAreDramatized(DramatizationOption scriptureQuotationsSpokenBy, int expectedBaseNNumberOfMaleActors)
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.HEB);
			//testProject.IncludedBooks.Single().GetBlocksForVerse()
			testProject.DramatizationPreferences.ScriptureQuotationsShouldBeSpokenBy = scriptureQuotationsSpokenBy;
			CastSizePlanningViewModel model = new CastSizePlanningViewModel(testProject);
			Assert.AreEqual(expectedBaseNNumberOfMaleActors + 2, model.GetCastSizeRowValues(CastSizeOption.Small).Male);
		}

		// ENHANCE: For now, the Orpah/Ruth character automatically resolves to Ruth. Later, when we give the user a way to determine this --
		// or possibly have a way to change it automatically to allow for a smaller cast -- we can revisit this test.
		[Test]
		public void GetCastSizeRowValues_ProjectHasSingleBookNarratedByAFemaleWithFourWomenInCloseProximity_MinimumCastHasFiveFemales()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.RUT);
			TestProject.SimulateDisambiguationForAllBooks(testProject);
			var harvestersBlock = testProject.IncludedBooks[0].GetScriptBlocks().First(b => b.ChapterNumber == 2 &&
				b.InitialStartVerseNumber == 4 && !b.BlockElements.OfType<Verse>().Any());
			harvestersBlock.CharacterId = "harvesters";
			CastSizePlanningViewModel model = new CastSizePlanningViewModel(testProject);
			model.NarratorOption = NarratorsOption.Custom;
			model.MaleNarrators = 0;
			model.FemaleNarrators = 1;

			var smallCast = model.GetCastSizeRowValues(CastSizeOption.Small);
			Assert.AreEqual(4, smallCast.Male);
			Assert.AreEqual(5, smallCast.Female);
			Assert.AreEqual(0, smallCast.Child);
			Assert.AreEqual(9, smallCast.Total);

			var recommendedCast = model.GetCastSizeRowValues(CastSizeOption.Recommended);
			Assert.AreEqual(5, recommendedCast.Male);
			Assert.AreEqual(5, recommendedCast.Female);
			Assert.AreEqual(0, recommendedCast.Child);
			Assert.AreEqual(10, recommendedCast.Total);

			var largeCast = model.GetCastSizeRowValues(CastSizeOption.Large);
			Assert.AreEqual(6, largeCast.Male);
			Assert.AreEqual(5, largeCast.Female);
			Assert.AreEqual(0, largeCast.Child);
			Assert.AreEqual(11, largeCast.Total);
		}

		[Test]
		public void GetCastSizeRowValues_ProjectHasManyBooksWithManySpeakingParts_SmallCastStaysSmall()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.ACT, TestProject.TestBook.RUT,
				TestProject.TestBook.JUD, TestProject.TestBook.LUK, TestProject.TestBook.JOS);
			TestProject.SimulateDisambiguationForAllBooks(testProject);
			CastSizePlanningViewModel model = new CastSizePlanningViewModel(testProject);
			model.NarratorOption = NarratorsOption.Custom;
			model.MaleNarrators = 1;
			model.FemaleNarrators = 1;

			var smallCast = model.GetCastSizeRowValues(CastSizeOption.Small);
			Assert.AreEqual(15, smallCast.Male);
				// 15 is actually big enough if the extra-biblical roles are assigned to a female actor, but our cast size estimates don't currently allow for this
			Assert.AreEqual(5, smallCast.Female);
			Assert.AreEqual(1, smallCast.Child);
			Assert.AreEqual(21, smallCast.Total);

			var recommendedCast = model.GetCastSizeRowValues(CastSizeOption.Recommended);
			Assert.AreEqual(22, recommendedCast.Male);
			Assert.AreEqual(7, recommendedCast.Female);
			Assert.AreEqual(1, recommendedCast.Child);
			Assert.AreEqual(30, recommendedCast.Total);

			var largeCast = model.GetCastSizeRowValues(CastSizeOption.Large);
			Assert.AreEqual(29, largeCast.Male); // TODO: Talk to Duane about logic for this
			Assert.AreEqual(9, largeCast.Female);
			Assert.AreEqual(1, largeCast.Child);
			Assert.AreEqual(39, largeCast.Total);
		}
	}

	[TestFixture]
	class CalculateMinimumCastSizesForNewTestamentBasedOnAcholi
	{
		private Project m_project;
		private readonly ConcurrentDictionary<string, CastSizeRowValues> m_results = new ConcurrentDictionary<string, CastSizeRowValues>();

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			// Use the real version of the file because we want the results to be based on the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = null;
			CharacterDetailData.TabDelimitedCharacterDetailData = null;

			Sldr.Initialize();
			m_project = Project.Load(@"C:\ProgramData\FCBH-SIL\Glyssen\ach\3b9fdc679b9319c3\Acholi New Test 1985 Audio\ach.glyssen");
			TestProject.SimulateDisambiguationForAllBooks(m_project);
			m_project.CharacterGroupGenerationPreferences.NarratorsOption = NarratorsOption.SingleNarrator;
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			Sldr.Cleanup();
			var ntBooks = SilBooks.Codes_3Letter.Skip(39).ToArray();

			if (m_results.Count == 27 || m_results.Count == 1)
			{
				foreach (var bookCode in ntBooks)
				{
					CastSizeRowValues validCast;
					if (m_results.TryGetValue(bookCode, out validCast))
						Debug.WriteLine("[TestCase(\"" + bookCode + "\", " + (validCast.Male - 1) + ")]");
				}
				Debug.WriteLine("****************");
			}
			else
			{
				Debug.WriteLine("WARNING: not all NT books are included in these results!!!!!!!!!!!");
			}

			Debug.WriteLine("Copy and paste the following into the CastSizePlanningViewModel constructor:");
			Debug.WriteLine("");

			foreach (var bookCode in ntBooks)
			{
				CastSizeRowValues validCast;
				if (m_results.TryGetValue(bookCode, out validCast))
				{
					Debug.WriteLine("case \"" + bookCode + "\":");
					if (bookCode == "HEB")
					{
						Debug.WriteLine("switch (m_project.DramatizationPreferences.ScriptureQuotationsShouldBeSpokenBy)");
						Debug.WriteLine("{");
						Debug.WriteLine("\tcase DramatizationOption.DedicatedCharacter:");
						Debug.WriteLine("\t\tsmallCast.Male = Math.Max(smallCast.Male, 1);");
						Debug.WriteLine("\t\tbreak;");
						Debug.WriteLine("\tcase DramatizationOption.DefaultCharacter:");
						Debug.WriteLine("\t\tsmallCast.Male = Math.Max(smallCast.Male, 4);");
						Debug.WriteLine("\t\tbreak;");
						Debug.WriteLine("\tcase DramatizationOption.Narrator:");
						Debug.WriteLine("\t\tsmallCast.Male = Math.Max(smallCast.Male, 0);");
						Debug.WriteLine("\t\tbreak;");
						Debug.WriteLine("}");
					}
					else
						Debug.WriteLine("smallCast.Male = Math.Max(smallCast.Male, " + (validCast.Male - 2) + ");");
					if (validCast.Female != 2)
						Debug.WriteLine("smallCast.Female = " + validCast.Female + ";");
					Debug.WriteLine("break;");
				}
			}
		}

		[Category("ByHand")]
		[TestCase("MAT", 11)]
		[TestCase("MRK", 13)]
		[TestCase("LUK", 14)]
		[TestCase("JHN", 11)]
		[TestCase("ACT", 14)]
		[TestCase("ROM", 3)]
		[TestCase("1CO", 6)]
		[TestCase("2CO", 4)]
		[TestCase("GAL", 4)]
		[TestCase("EPH", 1)]
		[TestCase("PHP", 1)]
		[TestCase("COL", 2)]
		[TestCase("1TH", 1)]
		[TestCase("2TH", 1)]
		[TestCase("1TI", 1)]
		[TestCase("2TI", 1)]
		[TestCase("TIT", 2)]
		[TestCase("PHM", 1)]
		[TestCase("HEB", 2)]
		[TestCase("JAS", 3)]
		[TestCase("1PE", 1)]
		[TestCase("2PE", 3)]
		[TestCase("1JN", 1)]
		[TestCase("2JN", 1)]
		[TestCase("3JN", 1)]
		[TestCase("JUD", 4)]
		[TestCase("REV", 16)]
		public void UtilityToCalculateMinimumCastSizesForAcholi_ThisIsNotARealUnitTest(string bookCode, int initialGuess)
		{
			foreach (var book in m_project.AvailableBooks)
			{
				book.IncludeInScript = book.Code == bookCode;
			}
			m_project.ClearCharacterStatistics();

			CastSizeRowValues validCast = new CastSizeRowValues(initialGuess + 2, 2, 1);
			var currentCast = new CastSizeRowValues(initialGuess + 1, 2, 1);
			List<CharacterGroup> groups;
			do
			{
				var gen = new CharacterGroupGenerator(m_project, currentCast);

				groups = gen.GenerateCharacterGroups(true);
				if (groups != null)
					validCast.Male = currentCast.Male;
				currentCast.Male--;

			} while (groups != null && currentCast.Male >= 2);
			Assert.IsTrue(validCast.Male <= initialGuess + 1);
			m_results[bookCode] = validCast;
		}
	}

	[TestFixture]
	class CalculateMinimumCastSizesForOldTestamentBasedOnKunaSanBlas
	{
		private Project m_project;
		private readonly ConcurrentDictionary<string, CastSizeRowValues> m_results = new ConcurrentDictionary<string, CastSizeRowValues>();

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			// Use the real version of the file because we want the results to be based on the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = null;
			CharacterDetailData.TabDelimitedCharacterDetailData = null;

			Sldr.Initialize();
			//Change this to Kuna and finish tests for OT books
			m_project = Project.Load(@"C:\ProgramData\FCBH-SIL\Glyssen\cuk\5a6b88fafe1c8f2b\The Bible in Kuna, San Blas Audio (1)\cuk.glyssen");
			TestProject.SimulateDisambiguationForAllBooks(m_project);
			m_project.CharacterGroupGenerationPreferences.NarratorsOption = NarratorsOption.SingleNarrator;
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			Sldr.Cleanup();

			var otBooks = SilBooks.Codes_3Letter.Take(39).ToArray();

			if (m_results.Count == 39 || m_results.Count == 1)
			{
				foreach (var bookCode in otBooks)
				{
					CastSizeRowValues validCast;
					if (m_results.TryGetValue(bookCode, out validCast))
						Debug.WriteLine("[TestCase(\"" + bookCode + "\", " + (validCast.Male - 1) + ")]");
				}
				Debug.WriteLine("****************");
			}
			else
			{
				Debug.WriteLine("WARNING: not all OT books are included in these results!!!!!!!!!!!");
			}

			Debug.WriteLine("Copy and paste the following into the CastSizePlanningViewModel constructor:");
			Debug.WriteLine("");

			foreach (var bookCode in otBooks)
			{
				CastSizeRowValues validCast;
				if (m_results.TryGetValue(bookCode, out validCast))
				{
					Debug.WriteLine("case \"" + bookCode + "\":");
					Debug.WriteLine("smallCast.Male = Math.Max(smallCast.Male, " + (validCast.Male - 2) + ");");
					if (validCast.Female != 2)
						Debug.WriteLine("smallCast.Female = " + validCast.Female + ";");
					Debug.WriteLine("break;");
				}
			}
		}

		[Category("ByHand")]
		[TestCase("GEN", 10)]
		[TestCase("EXO", 7)]
		[TestCase("LEV", 4)]
		[TestCase("NUM", 9)]
		[TestCase("DEU", 3)]
		[TestCase("JOS", 8)]
		[TestCase("JDG", 9)]
		[TestCase("RUT", 4)]
		[TestCase("1SA", 12)]
		[TestCase("2SA", 13)]
		[TestCase("1KI", 11)]
		[TestCase("2KI", 10)]
		[TestCase("1CH", 6)]
		[TestCase("2CH", 10)]
		[TestCase("EZR", 6)]
		[TestCase("NEH", 8)]
		[TestCase("EST", 7)]
		[TestCase("JOB", 8)]
		[TestCase("PSA", 4)]
		[TestCase("PRO", 1)]
		[TestCase("ECC", 2)]
		[TestCase("SNG", 3)]
		[TestCase("ISA", 7)]
		[TestCase("JER", 10)]
		[TestCase("LAM", 2)]
		[TestCase("EZK", 5)]
		[TestCase("DAN", 6)]
		[TestCase("HOS", 3)]
		[TestCase("JOL", 3)]
		[TestCase("AMO", 5)]
		[TestCase("OBA", 2)]
		[TestCase("JON", 5)]
		[TestCase("MIC", 4)]
		[TestCase("NAM", 2)]
		[TestCase("HAB", 2)]
		[TestCase("ZEP", 2)]
		[TestCase("HAG", 3)]
		[TestCase("ZEC", 6)]
		[TestCase("MAL", 2)]
		public void UtilityToCalculateMinimumCastSizesForKuna_ThisIsNotARealUnitTest(string bookCode, int initialGuess)
		{
			if (bookCode == "SNG")
			{
				// Song of Solomon is special because all the speech is "Implicit" (which we don't handle properly yet)
				m_results[bookCode] = new CastSizeRowValues(4, 2, 0);
				return;
			}

			foreach (var book in m_project.AvailableBooks)
				book.IncludeInScript = book.Code == bookCode;
			m_project.IncludedBooks.Single().SingleVoice = false;

			m_project.ClearCharacterStatistics();

			var women = (bookCode == "RUT") ? 4 : 2;
			if (bookCode == "GEN" || bookCode == "EXO")
				women = 3;
			CastSizeRowValues validCast = new CastSizeRowValues(initialGuess + 2, women, 1);
			var currentCast = new CastSizeRowValues(initialGuess + 1, women, 1);
			List<CharacterGroup> groups = null;
			do
			{
				var gen = new CharacterGroupGenerator(m_project, currentCast);

				groups = gen.GenerateCharacterGroups(true);
				if (groups != null)
					validCast.Male = currentCast.Male;
				currentCast.Male--;

			} while (groups != null && currentCast.Male >= 2);
			Assert.IsTrue(validCast.Male <= initialGuess + 1);
			m_results[bookCode] = validCast;
		}
	}
}
