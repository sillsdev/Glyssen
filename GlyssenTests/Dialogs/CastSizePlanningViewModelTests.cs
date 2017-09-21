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
		public void SetNarratorOption_NarrationByAuthorForTwoBooksWrittenBySameAuthor_DefaultsToOneNarrator()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.REV, TestProject.TestBook.IIIJN);
			CastSizePlanningViewModel model = new CastSizePlanningViewModel(testProject);
			model.MaleNarratorsValueChanged += model_MaleNarratorsValueChanged;
			model.FemaleNarratorsValueChanged += model_FemaleNarratorsValueChanged;
			model.NarratorOption = NarratorsOption.Custom;
			model.MaleNarrators = 2;
			model.FemaleNarrators = 0;
			m_femaleNarratorsValue = 0;
			model.NarratorOption = NarratorsOption.NarrationByAuthor;
			Assert.AreEqual(1, m_maleNarratorsValue);
			Assert.AreEqual(0, m_femaleNarratorsValue);
		}

		[Test]
		public void SetNarratorOption_NarrationByAuthorForThreeBooksWrittenByNonSpeakingAuthors_DefaultsToOneNarrator()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.RUT, TestProject.TestBook.ACT, TestProject.TestBook.HEB);
			CastSizePlanningViewModel model = new CastSizePlanningViewModel(testProject);
			model.MaleNarratorsValueChanged += model_MaleNarratorsValueChanged;
			model.FemaleNarratorsValueChanged += model_FemaleNarratorsValueChanged;
			model.NarratorOption = NarratorsOption.Custom;
			model.MaleNarrators = 2;
			model.FemaleNarrators = 1;
			m_femaleNarratorsValue = 0;
			model.NarratorOption = NarratorsOption.NarrationByAuthor;
			Assert.AreEqual(1, m_maleNarratorsValue);
			Assert.AreEqual(0, m_femaleNarratorsValue);
		}

		[Test]
		public void SetNarratorOption_NarrationByAuthorForTwoBooksWrittenBySameSpeakingAuthorAndTwoBooksWrittenByDifferentNonSpeakingAuthors_DefaultsToTwoNarrators()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.RUT, TestProject.TestBook.ACT, TestProject.TestBook.GAL, TestProject.TestBook.EPH);
			CastSizePlanningViewModel model = new CastSizePlanningViewModel(testProject);
			model.MaleNarratorsValueChanged += model_MaleNarratorsValueChanged;
			model.FemaleNarratorsValueChanged += model_FemaleNarratorsValueChanged;
			model.NarratorOption = NarratorsOption.Custom;
			model.MaleNarrators = 3;
			model.FemaleNarrators = 1;
			m_femaleNarratorsValue = 0;
			model.NarratorOption = NarratorsOption.NarrationByAuthor;
			Assert.AreEqual(2, m_maleNarratorsValue);
			Assert.AreEqual(0, m_femaleNarratorsValue);
		}

		[Test]
		public void SetNarratorOption_CallbackPublishesValues()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.GAL, TestProject.TestBook.IIIJN);
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
				b.InitialStartVerseNumber == 4 && !b.ContainsVerseNumber);
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
}
