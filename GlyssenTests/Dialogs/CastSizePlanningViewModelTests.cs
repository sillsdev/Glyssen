using System.Linq;
using Glyssen.Bundle;
using Glyssen.Character;
using Glyssen.Dialogs;
using GlyssenTests.Properties;
using NUnit.Framework;

namespace GlyssenTests.Dialogs
{
	[TestFixture]
	class CastSizePlanningViewModelTests
	{
		private class MockCastSizePlanningDialog
		{
			// The model should never send a value of -1 (as asserted in the event handlers below), so when the test
			// if the value of either of these is still -1, it means that the corresponding event never fired.
			private int m_maleNarratorsValue = -1;
			private int m_femaleNarratorsValue = -1;

			public int MaleNarratorsValue => m_maleNarratorsValue;
			public int FemaleNarratorsValue => m_femaleNarratorsValue;
			public bool MaleNarratorsValueChangedEventNeverFired => m_maleNarratorsValue == -1;
			public bool FemaleNarratorsValueChangedEventNeverFired => m_femaleNarratorsValue == -1;

			public MockCastSizePlanningDialog(CastSizePlanningViewModel model)
			{
				model.MaleNarratorsValueChanged += model_MaleNarratorsValueChanged;
				model.FemaleNarratorsValueChanged += model_FemaleNarratorsValueChanged;
			}

			private void model_MaleNarratorsValueChanged(object sender, int newValue)
			{
				Assert.That(newValue >= 0);
				m_maleNarratorsValue = newValue;
			}

			private void model_FemaleNarratorsValueChanged(object sender, int newValue)
			{
				Assert.That(newValue >= 0);
				m_femaleNarratorsValue = newValue;
			}
		}

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerseOct2015;
			CharacterDetailData.TabDelimitedCharacterDetailData = Resources.TestCharacterDetailOct2015;
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			TestProject.DeleteTestProjectFolder();
		}

		private CastSizePlanningViewModel CreateModelWithInitialCustomValues(Glyssen.Project project, int maleNarrators, int femaleNarrators)
		{
			var model = new CastSizePlanningViewModel(project);
			model.NarratorOption = NarratorsOption.Custom;
			model.MaleNarrators = maleNarrators;
			model.FemaleNarrators = femaleNarrators;
			return model;
		}

		private void VerifyThatModelAndDlgAreInSync(CastSizePlanningViewModel model, MockCastSizePlanningDialog dlg)
		{
			Assert.AreEqual(model.MaleNarrators, dlg.MaleNarratorsValue);
			Assert.AreEqual(model.FemaleNarrators, dlg.FemaleNarratorsValue);
		}

		[Test]
		public void SetNarratorOption_NarrationByAuthorForTwoBooksWrittenBySameAuthor_DefaultsToOneNarrator()
		{
			// Setup
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.REV, TestProject.TestBook.IIIJN);
			var model = CreateModelWithInitialCustomValues(testProject, 2, 0);
			var dlg = new MockCastSizePlanningDialog(model);
			// Test
			model.NarratorOption = NarratorsOption.NarrationByAuthor;
			// Verify results
			Assert.AreEqual(1, dlg.MaleNarratorsValue);
			Assert.That(dlg.FemaleNarratorsValueChangedEventNeverFired);
		}

		[Test]
		public void SetNarratorOption_NarrationByAuthorForThreeBooksWrittenByNonSpeakingAuthors_DefaultsToOneNarrator()
		{
			// Setup
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.RUT, TestProject.TestBook.ACT, TestProject.TestBook.HEB);
			var model = CreateModelWithInitialCustomValues(testProject, 2, 1);
			var dlg = new MockCastSizePlanningDialog(model);
			// Test
			model.NarratorOption = NarratorsOption.NarrationByAuthor;
			// Verify results
			Assert.AreEqual(1, dlg.MaleNarratorsValue);
			Assert.AreEqual(0, dlg.FemaleNarratorsValue);
			VerifyThatModelAndDlgAreInSync(model, dlg);
		}

		[Test]
		public void SetNarratorOption_NarrationByAuthorForTwoBooksWrittenBySameSpeakingAuthorAndTwoBooksWrittenByDifferentNonSpeakingAuthors_DefaultsToTwoNarrators()
		{
			// Setup
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.RUT, TestProject.TestBook.ACT, TestProject.TestBook.GAL, TestProject.TestBook.EPH);
			var model = CreateModelWithInitialCustomValues(testProject, 3, 1);
			var dlg = new MockCastSizePlanningDialog(model);
			// Test
			model.NarratorOption = NarratorsOption.NarrationByAuthor;
			// Verify results
			Assert.AreEqual(2, dlg.MaleNarratorsValue);
			Assert.AreEqual(0, dlg.FemaleNarratorsValue);
			VerifyThatModelAndDlgAreInSync(model, dlg);
		}

		[Test]
		public void SetNarratorOption_CallbackPublishesValues()
		{
			// Setup
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.GAL, TestProject.TestBook.IIIJN);
			var model = CreateModelWithInitialCustomValues(testProject, 1, 1);
			var dlg = new MockCastSizePlanningDialog(model);

			// Test - Part 1
			model.NarratorOption = NarratorsOption.NarrationByAuthor;
			// Verify results
			Assert.AreEqual(2, dlg.MaleNarratorsValue);
			Assert.AreEqual(0, dlg.FemaleNarratorsValue);
			VerifyThatModelAndDlgAreInSync(model, dlg);

			// Test - Part 2
			model.NarratorOption = NarratorsOption.SingleNarrator;
			// Verify results
			Assert.AreEqual(1, dlg.MaleNarratorsValue);
			Assert.AreEqual(0, dlg.FemaleNarratorsValue);
			VerifyThatModelAndDlgAreInSync(model, dlg);

			// Test - Part 3
			model.NarratorOption = NarratorsOption.Custom;
			// Verify results
			Assert.AreEqual(2, dlg.MaleNarratorsValue);
			Assert.AreEqual(1, dlg.FemaleNarratorsValue);
			VerifyThatModelAndDlgAreInSync(model, dlg);
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
		public void GetCastSizeRowValues_ProjectHasTwoBooksBySameAuthorsWithNoSpeakingParts_AllCastSizesHaveTwoMalesForNarratorAndExtra()
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
		public void GetCastSizeRowValues_ProjectHasTwoBooksWithNoSpeakingPartsCustomNarratorValues_AllCastSizesHaveBasedOnCustomNarratorsPlusExtraMale()
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
			Assert.AreEqual(4, smallCast.Male);
			Assert.AreEqual(0, smallCast.Female);
			Assert.AreEqual(0, smallCast.Child);
			Assert.AreEqual(4, smallCast.Total);

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
			Assert.AreEqual(9, smallCast.Male);
			Assert.AreEqual(2, smallCast.Female);
			Assert.AreEqual(0, smallCast.Child);
			Assert.AreEqual(11, smallCast.Total);

			var recommendedCast = model.GetCastSizeRowValues(CastSizeOption.Recommended);
			Assert.AreEqual(13, recommendedCast.Male);
			Assert.AreEqual(3, recommendedCast.Female);
			Assert.AreEqual(0, recommendedCast.Child);
			Assert.AreEqual(16, recommendedCast.Total);

			var largeCast = model.GetCastSizeRowValues(CastSizeOption.Large);
			Assert.AreEqual(17, largeCast.Male); // TODO: Talk to Duane about logic for this
			Assert.AreEqual(4, largeCast.Female);
			Assert.AreEqual(0, largeCast.Child);
			Assert.AreEqual(21, largeCast.Total);
		}

		[TestCase(DramatizationOption.DedicatedCharacter, 2)]
		[TestCase(DramatizationOption.DefaultCharacter, 4, IgnoreReason = "Not fully implemented")]
		[TestCase(DramatizationOption.Narrator, 1, IgnoreReason = "Not fully implemented")]
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
				b.InitialStartVerseNumber == 4 && !b.ContainsVerseNumber);
			harvestersBlock.CharacterId = "harvesters";
			CastSizePlanningViewModel model = new CastSizePlanningViewModel(testProject);
			model.NarratorOption = NarratorsOption.Custom;
			model.MaleNarrators = 0;
			model.FemaleNarrators = 1;

			var smallCast = model.GetCastSizeRowValues(CastSizeOption.Small);
			Assert.AreEqual(3, smallCast.Male);
			Assert.AreEqual(5, smallCast.Female);
			Assert.AreEqual(0, smallCast.Child);
			Assert.AreEqual(8, smallCast.Total);

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
			Assert.AreEqual(9, smallCast.Male);
			Assert.AreEqual(5, smallCast.Female);
			Assert.AreEqual(1, smallCast.Child);
			Assert.AreEqual(15, smallCast.Total);

			var recommendedCast = model.GetCastSizeRowValues(CastSizeOption.Recommended);
			Assert.AreEqual(13, recommendedCast.Male);
			Assert.AreEqual(7, recommendedCast.Female);
			Assert.AreEqual(1, recommendedCast.Child);
			Assert.AreEqual(21, recommendedCast.Total);

			var largeCast = model.GetCastSizeRowValues(CastSizeOption.Large);
			Assert.AreEqual(17, largeCast.Male); // TODO: Talk to Duane about logic for this
			Assert.AreEqual(9, largeCast.Female);
			Assert.AreEqual(1, largeCast.Child);
			Assert.AreEqual(27, largeCast.Total);
		}
	}
}
