using System;
using System.Collections.Generic;
using System.Linq;
using Glyssen;
using GlyssenTests.Properties;
using NUnit.Framework;
using Waxuquerque;
using Waxuquerque.Bundle;
using Waxuquerque.Character;
using Waxuquerque.Rules;

namespace GlyssenTests.Rules
{
	[TestFixture]
	class ProximityTests
	{
		private Project m_testProject;
		private Proximity m_proximity;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerse;
			RelatedCharactersData.Source = Resources.TestRelatedCharacters;
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.MRK);
			m_testProject.UseDefaultForUnresolvedMultipleChoiceCharacters();

			m_testProject.DramatizationPreferences.BookTitleAndChapterDramatization = ExtraBiblicalMaterialSpeakerOption.ActorOfEitherGender;
			m_testProject.DramatizationPreferences.SectionHeadDramatization = ExtraBiblicalMaterialSpeakerOption.ActorOfEitherGender;
			m_testProject.DramatizationPreferences.BookIntroductionsDramatization = ExtraBiblicalMaterialSpeakerOption.ActorOfEitherGender;
		}

		[SetUp]
		public void SetUp()
		{
			m_proximity = new Proximity(m_testProject);
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			TestProject.DeleteTestProjectFolder();
		}

		[Test]
		public void CalculateMinimumProximity_OneCharacterGroupJesus_ReturnsMaxInt()
		{
			HashSet<string> characterIds = new HashSet<string>();

			characterIds.Add("Jesus");

			MinimumProximity minProximity = m_proximity.CalculateMinimumProximity(characterIds);

			Assert.AreEqual(Int32.MaxValue, minProximity.NumberOfBlocks);
			Assert.AreEqual("Jesus", minProximity.FirstCharacterId);
			Assert.AreEqual("Jesus", minProximity.SecondCharacterId);
			Assert.AreEqual("MRK 1:17", minProximity.FirstReference);
			Assert.AreEqual("MRK 1:17", minProximity.SecondReference);

		}

		[Test]
		public void CalculateMinimumProximity_JesusAndNarrator_ReturnsZero()
		{
			HashSet<string> characterIds = new HashSet<string>();

			characterIds.Add("Jesus");
			characterIds.Add("narrator-MRK");

			MinimumProximity minProximity = m_proximity.CalculateMinimumProximity(characterIds);

			Assert.AreEqual(0, minProximity.NumberOfBlocks);
			Assert.AreEqual("narrator-MRK", minProximity.FirstCharacterId);
			Assert.AreEqual("Jesus", minProximity.SecondCharacterId);
			Assert.AreEqual("MRK 1:16", minProximity.FirstReference);
			Assert.AreEqual("MRK 1:17", minProximity.SecondReference);
		}

		[Test]
		public void CalculateMinimumProximity_MultipleLesserSpeakers_ReturnsPositive()
		{
			HashSet<string> characterIds = new HashSet<string>();

			//Mark 5
			characterIds.Add("demons (Legion)");
			characterIds.Add("woman, bleeding for twelve years");
			characterIds.Add("people, sick");

			MinimumProximity minProximity = m_proximity.CalculateMinimumProximity(characterIds);

			Assert.Greater(minProximity.NumberOfBlocks, 0);
			Assert.AreEqual("demons (Legion)", minProximity.FirstCharacterId);
			Assert.AreEqual("woman, bleeding for twelve years", minProximity.SecondCharacterId);
			Assert.AreEqual("MRK 5:12", minProximity.FirstReference);
			Assert.AreEqual("MRK 5:28", minProximity.SecondReference);
		}

		[Test]
		public void CalculateMinimumProximity_JesusAndTeachersOfReligiousLaw_ReturnsOne()
		{
			HashSet<string> characterIds = new HashSet<string>();

			//Mark 2:16-17
			characterIds.Add("Jesus");
			characterIds.Add("teachers of religious law");

			MinimumProximity minProximity = m_proximity.CalculateMinimumProximity(characterIds);

			Assert.AreEqual(1, minProximity.NumberOfBlocks);
			//Assert.AreEqual("Ka lupwony-cik mogo, ma onoŋo gibedo kunnu, gubedo ka pyem kekengi wa i cwinygi ni, ".Length, minProximity.NumberOfKeystrokes);
			Assert.AreEqual("Jesus", minProximity.FirstCharacterId);
			Assert.AreEqual("teachers of religious law", minProximity.SecondCharacterId);
			Assert.AreEqual("MRK 2:5", minProximity.FirstReference);
			Assert.AreEqual("MRK 2:7", minProximity.SecondReference);
		}

		[Test]
		public void CalculateMinimumProximity_SpecificCharacters_ReturnsExactNumbers()
		{
			HashSet<string> characterIds = new HashSet<string>();

			//Mark 9:24-26
			characterIds.Add("father of demon-possessed boy");
			characterIds.Add("many in crowd");

			MinimumProximity minProximity = m_proximity.CalculateMinimumProximity(characterIds);

			Assert.AreEqual(3, minProximity.NumberOfBlocks);
			//Assert.AreEqual("Ka Yecu oneno lwak giriŋo gibino bote, ogerre i wi cen marac, kun wacce ni, “In cen ma pe loko-ni, ma iti odiŋ-ŋi, aciki ni, a woko ki i kom awobi-nu, pe dok idwog i kome matwal.” En okok, ci ocako ryene aryeya ki tek, ka oa woko i kome. Awobi-nu koŋ obedo macalo dano muto, omiyo jo mapol doggi ocer ni, "
			//	.Length, minProximity.NumberOfKeystrokes);
			Assert.AreEqual("father of demon-possessed boy", minProximity.FirstCharacterId);
			Assert.AreEqual("many in crowd", minProximity.SecondCharacterId);
			Assert.AreEqual("MRK 9:24", minProximity.FirstReference);
			Assert.AreEqual("MRK 9:26", minProximity.SecondReference);
		}

		[Test]
		public void CalculateMinimumProximity_TwoVersionsOfSameCharacter_ReturnsMaxInt()
		{
			HashSet<string> characterIds = new HashSet<string>();

			//Mark 14:60-63
			characterIds.Add("Caiaphas, the high priest");
			characterIds.Add("Caiaphas, the high priest (old)");

			MinimumProximity minProximity = m_proximity.CalculateMinimumProximity(characterIds);

			Assert.AreEqual(Int32.MaxValue, minProximity.NumberOfBlocks);
			Assert.AreEqual("Caiaphas, the high priest", minProximity.FirstCharacterId);
			Assert.AreEqual("Caiaphas, the high priest", minProximity.SecondCharacterId);
			Assert.AreEqual("MRK 14:60", minProximity.FirstReference);
			Assert.AreEqual("MRK 14:60", minProximity.SecondReference);
		}

		[Test]
		public void CalculateMinimumProximity_OneOfTheCharactersHasAnAgeVariationCounterpart_CalculatesAllAgeVariationsAsSameCharacter()
		{
			HashSet<string> characterIds = new HashSet<string>();

			//Mark 14:60-65
			characterIds.Add("Caiaphas, the high priest");
			characterIds.Add("chief priests");

			MinimumProximity minProximity = m_proximity.CalculateMinimumProximity(characterIds);

			Assert.AreEqual(2, minProximity.NumberOfBlocks);
			//Assert.AreEqual("Gin ducu guŋolo lokke ni, balle romme to. Jo mukene gucako ŋulo laa i kome, kun giumo waŋe woko, ka gidoŋe kwede, ma kun giwacci, "
			//	.Length, minProximity.NumberOfKeystrokes);
			Assert.AreEqual("Caiaphas, the high priest (old)", minProximity.FirstCharacterId);
			Assert.AreEqual("chief priests", minProximity.SecondCharacterId);
			Assert.AreEqual("MRK 14:63", minProximity.FirstReference);
			Assert.AreEqual("MRK 14:65", minProximity.SecondReference);
		}

		[Test]
		public void CalculateMinimumProximity_TreatStandardNonScriptureCharactersAsDistinct_ExtraBiblicalResultsInZeroProximityWithChapterNumber()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.MRK);
			project.UseDefaultForUnresolvedMultipleChoiceCharacters();

			project.DramatizationPreferences.BookTitleAndChapterDramatization = ExtraBiblicalMaterialSpeakerOption.MaleActor;
			project.DramatizationPreferences.SectionHeadDramatization = ExtraBiblicalMaterialSpeakerOption.ActorOfEitherGender;
			project.DramatizationPreferences.BookIntroductionsDramatization = ExtraBiblicalMaterialSpeakerOption.FemaleActor;

			var proximity = new Proximity(project);

			var characterIds = new HashSet<string>
			{
				CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.BookOrChapter),
				CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.ExtraBiblical),
				CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.Intro)
			};


			MinimumProximity minProximity = proximity.CalculateMinimumProximity(characterIds);

			Assert.AreEqual(0, minProximity.NumberOfBlocks);
			Assert.AreEqual(CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.BookOrChapter), minProximity.FirstCharacterId);
			Assert.AreEqual(CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.ExtraBiblical), minProximity.SecondCharacterId);
			Assert.AreEqual("MRK 1:0", minProximity.FirstReference);
			Assert.AreEqual("MRK 1:0", minProximity.SecondReference);
		}

		[Test]
		public void CalculateMinimumProximity_TreatStandardNonScriptureCharactersAsOne_ExtraBiblicalResultsInZeroProximityWithChapterNumber()
		{
			HashSet<string> characterIds = new HashSet<string>();

			characterIds.Add(CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.BookOrChapter));
			characterIds.Add(CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.ExtraBiblical));
			characterIds.Add(CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.Intro));

			MinimumProximity minProximity = m_proximity.CalculateMinimumProximity(characterIds);

			Assert.AreEqual(Int32.MaxValue, minProximity.NumberOfBlocks);
			Assert.AreEqual(CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.BookOrChapter), minProximity.FirstCharacterId);
			Assert.AreEqual(minProximity.FirstCharacterId, minProximity.SecondCharacterId);
			Assert.AreEqual("MRK 0:0", minProximity.FirstReference);
			Assert.AreEqual(minProximity.FirstReference, minProximity.SecondReference);
		}

		[Test]
		public void CalculateMinimumProximity_NarrationByAuthor_CharacterSpeakingInBookHeNarratesResultsInMaxProximity()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.GAL);
			project.UseDefaultForUnresolvedMultipleChoiceCharacters();
			var idPaul = BiblicalAuthors.GetAuthorOfBook("GAL").Name;
			foreach (var block in project.IncludedBooks[0].GetBlocksForVerse(2, 15))
			{
				block.CharacterId = idPaul;
			}

			project.CharacterGroupGenerationPreferences.NarratorsOption = NarratorsOption.NarrationByAuthor;
			project.DramatizationPreferences.BookTitleAndChapterDramatization = ExtraBiblicalMaterialSpeakerOption.Narrator;
			project.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 1;

			var proximity = new Proximity(project);

			var characterIds = new HashSet<string>
			{
				CharacterVerseData.GetStandardCharacterId("GAL", CharacterVerseData.StandardCharacter.Narrator),
				CharacterVerseData.GetStandardCharacterId("GAL", CharacterVerseData.StandardCharacter.BookOrChapter),
				idPaul
			};

			MinimumProximity minProximity = proximity.CalculateMinimumProximity(characterIds);

			Assert.AreEqual(Int32.MaxValue, minProximity.NumberOfBlocks);
		}

		[Test]
		public void CalculateMinimumProximity_NonStrictAdherenceToNarratorPrefs_AllStandardCharactersAndBookAuthorResultsInMaxProximity()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.JOS); // Using Joshua because the test data for Joshua has into material 
			project.UseDefaultForUnresolvedMultipleChoiceCharacters();

			project.CharacterGroupGenerationPreferences.NarratorsOption = NarratorsOption.SingleNarrator;
			// By making theses all different, we force the CharacterGroupGenerator (which we aren't calling here) to put each
			// type of standard character in a different group, but with "not strict" proximity, we still consider it legit to
			// manually put them in the same group.
			project.DramatizationPreferences.BookTitleAndChapterDramatization = ExtraBiblicalMaterialSpeakerOption.ActorOfEitherGender;
			project.DramatizationPreferences.BookIntroductionsDramatization = ExtraBiblicalMaterialSpeakerOption.FemaleActor;
			project.DramatizationPreferences.SectionHeadDramatization = ExtraBiblicalMaterialSpeakerOption.MaleActor;

			var proximity = new Proximity(project, false);

			var characterIds = new HashSet<string>
			{
				CharacterVerseData.GetStandardCharacterId("JOS", CharacterVerseData.StandardCharacter.Narrator),
				CharacterVerseData.GetStandardCharacterId("JOS", CharacterVerseData.StandardCharacter.ExtraBiblical),
				CharacterVerseData.GetStandardCharacterId("JOS", CharacterVerseData.StandardCharacter.Intro),
				CharacterVerseData.GetStandardCharacterId("JOS", CharacterVerseData.StandardCharacter.BookOrChapter),
			};

			MinimumProximity minProximity = proximity.CalculateMinimumProximity(characterIds);

			Assert.AreEqual(Int32.MaxValue, minProximity.NumberOfBlocks);
		}

		[Test]
		public void CalculateMinimumProximity_NarrationByAuthor_NonStrictAdherenceToNarratorPrefs_AllStandardCharactersAndBookAuthorResultsInMaxProximity()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.GAL);
			project.UseDefaultForUnresolvedMultipleChoiceCharacters();
			var idPaul = BiblicalAuthors.GetAuthorOfBook("GAL").Name;
			foreach (var block in project.IncludedBooks[0].GetBlocksForVerse(2, 15))
			{
				block.CharacterId = idPaul;
			}

			project.CharacterGroupGenerationPreferences.NarratorsOption = NarratorsOption.NarrationByAuthor;
			// The following can be anything but omitted, but by making them all different, we prove that
			// non-strict adherence to the the narrator prefs is really happening.
			project.DramatizationPreferences.BookTitleAndChapterDramatization = ExtraBiblicalMaterialSpeakerOption.ActorOfEitherGender;
			project.DramatizationPreferences.BookIntroductionsDramatization = ExtraBiblicalMaterialSpeakerOption.FemaleActor;
			project.DramatizationPreferences.SectionHeadDramatization = ExtraBiblicalMaterialSpeakerOption.MaleActor;

			var proximity = new Proximity(project, false);

			var characterIds = new HashSet<string>
			{
				CharacterVerseData.GetStandardCharacterId("GAL", CharacterVerseData.StandardCharacter.Narrator),
				CharacterVerseData.GetStandardCharacterId("GAL", CharacterVerseData.StandardCharacter.ExtraBiblical),
				CharacterVerseData.GetStandardCharacterId("GAL", CharacterVerseData.StandardCharacter.Intro), // Not actually used in GAL test data
				CharacterVerseData.GetStandardCharacterId("GAL", CharacterVerseData.StandardCharacter.BookOrChapter),
				idPaul
			};

			MinimumProximity minProximity = proximity.CalculateMinimumProximity(characterIds);

			Assert.AreEqual(Int32.MaxValue, minProximity.NumberOfBlocks);
		}
	}
}
