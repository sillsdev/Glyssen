using System.Linq;
using GlyssenCharacters;
using GlyssenEngine.Bundle;
using GlyssenEngine.ViewModels;
using NUnit.Framework;

namespace GlyssenEngineTests.ViewModelTests
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
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = GlyssenCharactersTests.Properties.Resources.TestCharacterVerseOct2015;
			CharacterDetailData.TabDelimitedCharacterDetailData = GlyssenCharactersTests.Properties.Resources.TestCharacterDetailOct2015;
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			TestProject.DeleteTestProjects();
		}

		private CastSizePlanningViewModel CreateModelWithInitialCustomValues(
			GlyssenEngine.Project project, int maleNarrators, int femaleNarrators)
		{
			var model = new CastSizePlanningViewModel(project)
			{
				NarratorOption = NarratorsOption.Custom,
				MaleNarrators = maleNarrators,
				FemaleNarrators = femaleNarrators
			};
			return model;
		}

		private void VerifyThatModelAndDlgAreInSync(CastSizePlanningViewModel model, MockCastSizePlanningDialog dlg)
		{
			Assert.That(model.MaleNarrators, Is.EqualTo(dlg.MaleNarratorsValue));
			Assert.That(model.FemaleNarrators, Is.EqualTo(dlg.FemaleNarratorsValue));
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
			Assert.That(dlg.MaleNarratorsValue, Is.EqualTo(1));
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
			Assert.That(dlg.MaleNarratorsValue, Is.EqualTo(1));
			Assert.That(dlg.FemaleNarratorsValue, Is.EqualTo(0));
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
			Assert.That(dlg.MaleNarratorsValue, Is.EqualTo(2));
			Assert.That(dlg.FemaleNarratorsValue, Is.EqualTo(0));
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
			Assert.That(dlg.MaleNarratorsValue, Is.EqualTo(2));
			Assert.That(dlg.FemaleNarratorsValue, Is.EqualTo(0));
			VerifyThatModelAndDlgAreInSync(model, dlg);

			// Test - Part 2
			model.NarratorOption = NarratorsOption.SingleNarrator;
			// Verify results
			Assert.That(dlg.MaleNarratorsValue, Is.EqualTo(1));
			Assert.That(dlg.FemaleNarratorsValue, Is.EqualTo(0));
			VerifyThatModelAndDlgAreInSync(model, dlg);

			// Test - Part 3
			model.NarratorOption = NarratorsOption.Custom;
			// Verify results
			Assert.That(dlg.MaleNarratorsValue, Is.EqualTo(2));
			Assert.That(dlg.FemaleNarratorsValue, Is.EqualTo(1));
			VerifyThatModelAndDlgAreInSync(model, dlg);
		}

		[Test]
		public void GetCastSizeRowValues_ProjectHasSingleBookWithNoSpeakingParts_AllCastSizesHaveTwoMalesForNarratorAndExtra()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.PHM);
			CastSizePlanningViewModel model = new CastSizePlanningViewModel(testProject);

			var smallCast = model.GetCastSizeRowValues(CastSizeOption.Small);
			Assert.That(smallCast.Male, Is.EqualTo(2));
			Assert.That(smallCast.Female, Is.EqualTo(0));
			Assert.That(smallCast.Child, Is.EqualTo(0));
			Assert.That(smallCast.Total, Is.EqualTo(2));

			var recommendedCast = model.GetCastSizeRowValues(CastSizeOption.Recommended);
			Assert.That(recommendedCast.Male, Is.EqualTo(2));
			Assert.That(recommendedCast.Female, Is.EqualTo(0));
			Assert.That(recommendedCast.Child, Is.EqualTo(0));
			Assert.That(recommendedCast.Total, Is.EqualTo(2));

			var largeCast = model.GetCastSizeRowValues(CastSizeOption.Large);
			Assert.That(largeCast.Male, Is.EqualTo(2));
			Assert.That(largeCast.Female, Is.EqualTo(0));
			Assert.That(largeCast.Child, Is.EqualTo(0));
			Assert.That(largeCast.Total, Is.EqualTo(2));
		}

		[Test]
		public void GetCastSizeRowValues_ProjectHasTwoBooksByDifferentAuthorsWithNoSpeakingParts_AllCastSizesHaveThreeMalesForNarratorAndExtra()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.PHM, TestProject.TestBook.IIIJN);
			CastSizePlanningViewModel model = new CastSizePlanningViewModel(testProject)
			{
				NarratorOption = NarratorsOption.NarrationByAuthor
			};
			testProject.DramatizationPreferences.SectionHeadDramatization = ExtraBiblicalMaterialSpeakerOption.MaleActor;
			testProject.DramatizationPreferences.BookTitleAndChapterDramatization = ExtraBiblicalMaterialSpeakerOption.MaleActor;

			var smallCast = model.GetCastSizeRowValues(CastSizeOption.Small);
			Assert.That(smallCast.Male, Is.EqualTo(3));
			Assert.That(smallCast.Female, Is.EqualTo(0));
			Assert.That(smallCast.Child, Is.EqualTo(0));
			Assert.That(smallCast.Total, Is.EqualTo(3));

			var recommendedCast = model.GetCastSizeRowValues(CastSizeOption.Recommended);
			Assert.That(recommendedCast.Male, Is.EqualTo(3));
			Assert.That(recommendedCast.Female, Is.EqualTo(0));
			Assert.That(recommendedCast.Child, Is.EqualTo(0));
			Assert.That(recommendedCast.Total, Is.EqualTo(3));

			var largeCast = model.GetCastSizeRowValues(CastSizeOption.Large);
			Assert.That(largeCast.Male, Is.EqualTo(3));
			Assert.That(largeCast.Female, Is.EqualTo(0));
			Assert.That(largeCast.Child, Is.EqualTo(0));
			Assert.That(largeCast.Total, Is.EqualTo(3));
		}

		[Test]
		public void GetCastSizeRowValues_ProjectHasTwoBooksBySameAuthorsWithNoSpeakingParts_AllCastSizesHaveTwoMalesForNarratorAndExtra()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.IIJN, TestProject.TestBook.IIIJN);
			CastSizePlanningViewModel model = new CastSizePlanningViewModel(testProject)
			{
				NarratorOption = NarratorsOption.NarrationByAuthor
			};

			var smallCast = model.GetCastSizeRowValues(CastSizeOption.Small);
			Assert.That(smallCast.Male, Is.EqualTo(2));
			Assert.That(smallCast.Female, Is.EqualTo(0));
			Assert.That(smallCast.Child, Is.EqualTo(0));
			Assert.That(smallCast.Total, Is.EqualTo(2));

			var recommendedCast = model.GetCastSizeRowValues(CastSizeOption.Recommended);
			Assert.That(recommendedCast.Male, Is.EqualTo(2));
			Assert.That(recommendedCast.Female, Is.EqualTo(0));
			Assert.That(recommendedCast.Child, Is.EqualTo(0));
			Assert.That(recommendedCast.Total, Is.EqualTo(2));

			var largeCast = model.GetCastSizeRowValues(CastSizeOption.Large);
			Assert.That(largeCast.Male, Is.EqualTo(2));
			Assert.That(largeCast.Female, Is.EqualTo(0));
			Assert.That(largeCast.Child, Is.EqualTo(0));
			Assert.That(largeCast.Total, Is.EqualTo(2));
		}

		[Test]
		public void GetCastSizeRowValues_ProjectHasTwoBooksWithNoSpeakingPartsCustomNarratorValues_AllCastSizesHaveBasedOnCustomNarratorsPlusExtraMale()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.IIJN, TestProject.TestBook.IIIJN);
			CastSizePlanningViewModel model = new CastSizePlanningViewModel(testProject)
			{
				NarratorOption = NarratorsOption.Custom,
				MaleNarrators = 1,
				FemaleNarrators = 1
			};

			var smallCast = model.GetCastSizeRowValues(CastSizeOption.Small);
			Assert.That(smallCast.Male, Is.EqualTo(2));
			Assert.That(smallCast.Female, Is.EqualTo(1));
			Assert.That(smallCast.Child, Is.EqualTo(0));
			Assert.That(smallCast.Total, Is.EqualTo(3));

			var recommendedCast = model.GetCastSizeRowValues(CastSizeOption.Recommended);
			Assert.That(recommendedCast.Male, Is.EqualTo(2));
			Assert.That(recommendedCast.Female, Is.EqualTo(1));
			Assert.That(recommendedCast.Child, Is.EqualTo(0));
			Assert.That(recommendedCast.Total, Is.EqualTo(3));

			var largeCast = model.GetCastSizeRowValues(CastSizeOption.Large);
			Assert.That(largeCast.Male, Is.EqualTo(2));
			Assert.That(largeCast.Female, Is.EqualTo(1));
			Assert.That(largeCast.Child, Is.EqualTo(0));
			Assert.That(largeCast.Total, Is.EqualTo(3));
		}

		[Test]
		public void GetCastSizeRowValues_ProjectHasSingleBookWithThreeMaleSpeakingPartsCloseTogether_AllCastSizesHaveFiveMales
			()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.JUD);
			CastSizePlanningViewModel model = new CastSizePlanningViewModel(testProject);

			var smallCast = model.GetCastSizeRowValues(CastSizeOption.Small);
			Assert.That(smallCast.Male, Is.EqualTo(4));
			Assert.That(smallCast.Female, Is.EqualTo(0));
			Assert.That(smallCast.Child, Is.EqualTo(0));
			Assert.That(smallCast.Total, Is.EqualTo(4));

			var recommendedCast = model.GetCastSizeRowValues(CastSizeOption.Recommended);
			Assert.That(recommendedCast.Male, Is.EqualTo(5));
			Assert.That(recommendedCast.Female, Is.EqualTo(0));
			Assert.That(recommendedCast.Child, Is.EqualTo(0));
			Assert.That(recommendedCast.Total, Is.EqualTo(5));

			var largeCast = model.GetCastSizeRowValues(CastSizeOption.Large);
			Assert.That(largeCast.Male, Is.EqualTo(5));
			Assert.That(largeCast.Female, Is.EqualTo(0));
			Assert.That(largeCast.Child, Is.EqualTo(0));
			Assert.That(largeCast.Total, Is.EqualTo(5));
		}

		[Test]
		public void GetCastSizeRowValues_ProjectHasSingleBookWithManySpeakingPartsNoChildren_NoChildrenInCastSizes()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.ACT);
			TestProject.SimulateDisambiguationForAllBooks(testProject);
			CastSizePlanningViewModel model = new CastSizePlanningViewModel(testProject);

			var smallCast = model.GetCastSizeRowValues(CastSizeOption.Small);
			Assert.That(smallCast.Male, Is.EqualTo(9));
			Assert.That(smallCast.Female, Is.EqualTo(2));
			Assert.That(smallCast.Child, Is.EqualTo(0));
			Assert.That(smallCast.Total, Is.EqualTo(11));

			var recommendedCast = model.GetCastSizeRowValues(CastSizeOption.Recommended);
			Assert.That(recommendedCast.Male, Is.EqualTo(13));
			Assert.That(recommendedCast.Female, Is.EqualTo(3));
			Assert.That(recommendedCast.Child, Is.EqualTo(0));
			Assert.That(recommendedCast.Total, Is.EqualTo(16));

			var largeCast = model.GetCastSizeRowValues(CastSizeOption.Large);
			Assert.That(largeCast.Male, Is.EqualTo(17)); // TODO: Talk to Duane about logic for this
			Assert.That(largeCast.Female, Is.EqualTo(4));
			Assert.That(largeCast.Child, Is.EqualTo(0));
			Assert.That(largeCast.Total, Is.EqualTo(21));
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
			Assert.That(expectedBaseNNumberOfMaleActors + 2, Is.EqualTo(model.GetCastSizeRowValues(CastSizeOption.Small).Male));
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
			CastSizePlanningViewModel model = new CastSizePlanningViewModel(testProject)
			{
				NarratorOption = NarratorsOption.Custom,
				MaleNarrators = 0,
				FemaleNarrators = 1
			};

			var smallCast = model.GetCastSizeRowValues(CastSizeOption.Small);
			Assert.That(smallCast.Male, Is.EqualTo(3));
			Assert.That(smallCast.Female, Is.EqualTo(5));
			Assert.That(smallCast.Child, Is.EqualTo(0));
			Assert.That(smallCast.Total, Is.EqualTo(8));

			var recommendedCast = model.GetCastSizeRowValues(CastSizeOption.Recommended);
			Assert.That(recommendedCast.Male, Is.EqualTo(5));
			Assert.That(recommendedCast.Female, Is.EqualTo(5));
			Assert.That(recommendedCast.Child, Is.EqualTo(0));
			Assert.That(recommendedCast.Total, Is.EqualTo(10));

			var largeCast = model.GetCastSizeRowValues(CastSizeOption.Large);
			Assert.That(largeCast.Male, Is.EqualTo(6));
			Assert.That(largeCast.Female, Is.EqualTo(5));
			Assert.That(largeCast.Child, Is.EqualTo(0));
			Assert.That(largeCast.Total, Is.EqualTo(11));
		}

		[Test]
		public void GetCastSizeRowValues_ProjectHasManyBooksWithManySpeakingParts_SmallCastStaysSmall()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.ACT, TestProject.TestBook.RUT,
				TestProject.TestBook.JUD, TestProject.TestBook.LUK, TestProject.TestBook.JOS);
			TestProject.SimulateDisambiguationForAllBooks(testProject);
			CastSizePlanningViewModel model = new CastSizePlanningViewModel(testProject)
			{
				NarratorOption = NarratorsOption.Custom,
				MaleNarrators = 1,
				FemaleNarrators = 1
			};

			var smallCast = model.GetCastSizeRowValues(CastSizeOption.Small);
			Assert.That(smallCast.Male, Is.EqualTo(9));
			Assert.That(smallCast.Female, Is.EqualTo(5));
			Assert.That(smallCast.Child, Is.EqualTo(1));
			Assert.That(smallCast.Total, Is.EqualTo(15));

			var recommendedCast = model.GetCastSizeRowValues(CastSizeOption.Recommended);
			Assert.That(recommendedCast.Male, Is.EqualTo(13));
			Assert.That(recommendedCast.Female, Is.EqualTo(7));
			Assert.That(recommendedCast.Child, Is.EqualTo(1));
			Assert.That(recommendedCast.Total, Is.EqualTo(21));

			var largeCast = model.GetCastSizeRowValues(CastSizeOption.Large);
			Assert.That(largeCast.Male, Is.EqualTo(17)); // TODO: Talk to Duane about logic for this
			Assert.That(largeCast.Female, Is.EqualTo(9));
			Assert.That(largeCast.Child, Is.EqualTo(1));
			Assert.That(largeCast.Total, Is.EqualTo(27));
		}
	}
}
