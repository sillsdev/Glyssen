using System.Collections.Generic;
using GlyssenCharacters;
using GlyssenEngine;
using GlyssenEngine.Bundle;
using GlyssenEngine.Rules;
using NUnit.Framework;
using static System.Int32;
using static GlyssenCharacters.CharacterVerseData.StandardCharacter;
using Resources = GlyssenCharactersTests.Properties.Resources;

namespace GlyssenEngineTests.Rules
{
	[TestFixture]
	class ProximityTests
	{
		private Project m_testProject;
		private Proximity m_proximity;

		[OneTimeSetUp]
		public void OneTimeSetUp()
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

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			TestProject.DeleteTestProjects();
		}

		[Test]
		public void CalculateMinimumProximity_OneCharacterGroupJesus_ReturnsMaxInt()
		{
			var characterIds = new HashSet<string> { "Jesus" };

			var minProximity = m_proximity.CalculateMinimumProximity(characterIds);

			Assert.That(minProximity.NumberOfBlocks, Is.EqualTo(MaxValue));
			Assert.That(minProximity.FirstCharacterId, Is.EqualTo("Jesus"));
			Assert.That(minProximity.SecondCharacterId, Is.EqualTo("Jesus"));
			Assert.That(minProximity.FirstReference, Is.EqualTo("MRK 1:17"));
			Assert.That(minProximity.SecondReference, Is.EqualTo("MRK 1:17"));

		}

		[Test]
		public void CalculateMinimumProximity_JesusAndNarrator_ReturnsZero()
		{
			var characterIds = new HashSet<string>
			{
				"Jesus",
				"narrator-MRK"
			};

			var minProximity = m_proximity.CalculateMinimumProximity(characterIds);

			Assert.That(minProximity.NumberOfBlocks, Is.EqualTo(0));
			Assert.That(minProximity.FirstCharacterId, Is.EqualTo("narrator-MRK"));
			Assert.That(minProximity.SecondCharacterId, Is.EqualTo("Jesus"));
			Assert.That(minProximity.FirstReference, Is.EqualTo("MRK 1:16"));
			Assert.That(minProximity.SecondReference, Is.EqualTo("MRK 1:17"));
		}

		[Test]
		public void CalculateMinimumProximity_MultipleLesserSpeakers_ReturnsPositive()
		{
			var characterIds = new HashSet<string>();

			//Mark 5
			characterIds.Add("demons (Legion)");
			characterIds.Add("woman, bleeding for twelve years");
			characterIds.Add("people, sick");

			var minProximity = m_proximity.CalculateMinimumProximity(characterIds);

			Assert.That(minProximity.NumberOfBlocks, Is.GreaterThan(0));
			Assert.That(minProximity.FirstCharacterId, Is.EqualTo("demons (Legion)"));
			Assert.That(minProximity.SecondCharacterId, Is.EqualTo("woman, bleeding for twelve years"));
			Assert.That(minProximity.FirstReference, Is.EqualTo("MRK 5:12"));
			Assert.That(minProximity.SecondReference, Is.EqualTo("MRK 5:28"));
		}

		[Test]
		public void CalculateMinimumProximity_JesusAndTeachersOfReligiousLaw_ReturnsOne()
		{
			var characterIds = new HashSet<string>();

			//Mark 2:16-17
			characterIds.Add("Jesus");
			characterIds.Add("teachers of religious law");

			var minProximity = m_proximity.CalculateMinimumProximity(characterIds);

			Assert.That(minProximity.NumberOfBlocks, Is.EqualTo(1));
			//Assert.That(minProximity.NumberOfKeystrokes, Is.EqualTo("Ka lupwony-cik mogo, ma onoŋo gibedo kunnu, gubedo ka pyem kekengi wa i cwinygi ni, ".Length));
			Assert.That(minProximity.FirstCharacterId, Is.EqualTo("Jesus"));
			Assert.That(minProximity.SecondCharacterId, Is.EqualTo("teachers of religious law"));
			Assert.That(minProximity.FirstReference, Is.EqualTo("MRK 2:5"));
			Assert.That(minProximity.SecondReference, Is.EqualTo("MRK 2:7"));
		}

		[Test]
		public void CalculateMinimumProximity_SpecificCharacters_ReturnsExactNumbers()
		{
			var characterIds = new HashSet<string>();

			//Mark 9:24-26
			characterIds.Add("father of demon-possessed boy");
			characterIds.Add("many in crowd");

			var minProximity = m_proximity.CalculateMinimumProximity(characterIds);

			Assert.That(minProximity.NumberOfBlocks, Is.EqualTo(3));
			//Assert.AreEqual("Ka Yecu oneno lwak giriŋo gibino bote, ogerre i wi cen marac, kun wacce ni, “In cen ma pe loko-ni, ma iti odiŋ-ŋi, aciki ni, a woko ki i kom awobi-nu, pe dok idwog i kome matwal.” En okok, ci ocako ryene aryeya ki tek, ka oa woko i kome. Awobi-nu koŋ obedo macalo dano muto, omiyo jo mapol doggi ocer ni, "
			//	.Length, minProximity.NumberOfKeystrokes);
			Assert.That(minProximity.FirstCharacterId, Is.EqualTo("father of demon-possessed boy"));
			Assert.That(minProximity.SecondCharacterId, Is.EqualTo("many in crowd"));
			Assert.That(minProximity.FirstReference, Is.EqualTo("MRK 9:24"));
			Assert.That(minProximity.SecondReference, Is.EqualTo("MRK 9:26"));
		}

		[Test]
		public void CalculateMinimumProximity_TwoVersionsOfSameCharacter_ReturnsMaxInt()
		{
			var characterIds = new HashSet<string>();

			//Mark 14:60-63
			characterIds.Add("Caiaphas, the high priest");
			characterIds.Add("Caiaphas, the high priest (old)");

			var minProximity = m_proximity.CalculateMinimumProximity(characterIds);

			Assert.That(minProximity.NumberOfBlocks, Is.EqualTo(MaxValue));
			Assert.That(minProximity.FirstCharacterId, Is.EqualTo("Caiaphas, the high priest"));
			Assert.That(minProximity.SecondCharacterId, Is.EqualTo("Caiaphas, the high priest"));
			Assert.That(minProximity.FirstReference, Is.EqualTo("MRK 14:60"));
			Assert.That(minProximity.SecondReference, Is.EqualTo("MRK 14:60"));
		}

		[Test]
		public void CalculateMinimumProximity_OneOfTheCharactersHasAnAgeVariationCounterpart_CalculatesAllAgeVariationsAsSameCharacter()
		{
			var characterIds = new HashSet<string>();

			//Mark 14:60-65
			characterIds.Add("Caiaphas, the high priest");
			characterIds.Add("chief priests");

			var minProximity = m_proximity.CalculateMinimumProximity(characterIds);

			Assert.That(minProximity.NumberOfBlocks, Is.EqualTo(2));
			//Assert.AreEqual("Gin ducu guŋolo lokke ni, balle romme to. Jo mukene gucako ŋulo laa i kome, kun giumo waŋe woko, ka gidoŋe kwede, ma kun giwacci, "
			//	.Length, minProximity.NumberOfKeystrokes);
			Assert.That(minProximity.FirstCharacterId, Is.EqualTo("Caiaphas, the high priest (old)"));
			Assert.That(minProximity.SecondCharacterId, Is.EqualTo("chief priests"));
			Assert.That(minProximity.FirstReference, Is.EqualTo("MRK 14:63"));
			Assert.That(minProximity.SecondReference, Is.EqualTo("MRK 14:65"));
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
				CharacterVerseData.GetStandardCharacterId("MRK", BookOrChapter),
				CharacterVerseData.GetStandardCharacterId("MRK", ExtraBiblical),
				CharacterVerseData.GetStandardCharacterId("MRK", Intro)
			};


			var minProximity = proximity.CalculateMinimumProximity(characterIds);

			Assert.That(minProximity.NumberOfBlocks, Is.EqualTo(0));
			Assert.That(CharacterVerseData.GetStandardCharacterId("MRK", BookOrChapter), Is.EqualTo(minProximity.FirstCharacterId));
			Assert.That(CharacterVerseData.GetStandardCharacterId("MRK", ExtraBiblical), Is.EqualTo(minProximity.SecondCharacterId));
			Assert.That(minProximity.FirstReference, Is.EqualTo("MRK 1:0"));
			Assert.That(minProximity.SecondReference, Is.EqualTo("MRK 1:0"));
		}

		[Test]
		public void CalculateMinimumProximity_TreatStandardNonScriptureCharactersAsOne_ExtraBiblicalResultsInZeroProximityWithChapterNumber()
		{
			var characterIds = new HashSet<string>
			{
				CharacterVerseData.GetStandardCharacterId("MRK", BookOrChapter),
				CharacterVerseData.GetStandardCharacterId("MRK", ExtraBiblical),
				CharacterVerseData.GetStandardCharacterId("MRK", Intro)
			};

			var minProximity = m_proximity.CalculateMinimumProximity(characterIds);

			Assert.That(minProximity.NumberOfBlocks, Is.EqualTo(MaxValue));
			Assert.That(CharacterVerseData.GetStandardCharacterId("MRK", BookOrChapter), Is.EqualTo(minProximity.FirstCharacterId));
			Assert.That(minProximity.FirstCharacterId, Is.EqualTo(minProximity.SecondCharacterId));
			Assert.That(minProximity.FirstReference, Is.EqualTo("MRK 0:0"));
			Assert.That(minProximity.FirstReference, Is.EqualTo(minProximity.SecondReference));
		}

		[Test]
		public void CalculateMinimumProximity_NarrationByAuthor_CharacterSpeakingInBookHeNarratesResultsInMaxProximity()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.GAL);
			project.UseDefaultForUnresolvedMultipleChoiceCharacters();
			var idPaul = BiblicalAuthors.GetAuthorOfBook("GAL").Name;
			foreach (var block in project.IncludedBooks[0].GetBlocksForVerse(2, 15))
				block.CharacterId = idPaul;

			project.CharacterGroupGenerationPreferences.NarratorsOption = NarratorsOption.NarrationByAuthor;
			project.DramatizationPreferences.BookTitleAndChapterDramatization = ExtraBiblicalMaterialSpeakerOption.Narrator;
			project.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 1;

			var proximity = new Proximity(project);

			var characterIds = new HashSet<string>
			{
				CharacterVerseData.GetStandardCharacterId("GAL", Narrator),
				CharacterVerseData.GetStandardCharacterId("GAL", BookOrChapter),
				idPaul
			};

			var minProximity = proximity.CalculateMinimumProximity(characterIds);

			Assert.That(minProximity.NumberOfBlocks, Is.EqualTo(MaxValue));
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
				CharacterVerseData.GetStandardCharacterId("JOS", Narrator),
				CharacterVerseData.GetStandardCharacterId("JOS", ExtraBiblical),
				CharacterVerseData.GetStandardCharacterId("JOS", Intro),
				CharacterVerseData.GetStandardCharacterId("JOS", BookOrChapter),
			};

			var minProximity = proximity.CalculateMinimumProximity(characterIds);

			Assert.That(minProximity.NumberOfBlocks, Is.EqualTo(MaxValue));
		}

		[Test]
		public void CalculateMinimumProximity_NarrationByAuthor_NonStrictAdherenceToNarratorPrefs_AllStandardCharactersAndBookAuthorResultsInMaxProximity()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.GAL);
			project.UseDefaultForUnresolvedMultipleChoiceCharacters();
			var idPaul = BiblicalAuthors.GetAuthorOfBook("GAL").Name;
			foreach (var block in project.IncludedBooks[0].GetBlocksForVerse(2, 15))
				block.CharacterId = idPaul;

			project.CharacterGroupGenerationPreferences.NarratorsOption = NarratorsOption.NarrationByAuthor;
			// The following can be anything but omitted, but by making them all different, we prove that
			// non-strict adherence to the narrator preferences is really happening.
			project.DramatizationPreferences.BookTitleAndChapterDramatization = ExtraBiblicalMaterialSpeakerOption.ActorOfEitherGender;
			project.DramatizationPreferences.BookIntroductionsDramatization = ExtraBiblicalMaterialSpeakerOption.FemaleActor;
			project.DramatizationPreferences.SectionHeadDramatization = ExtraBiblicalMaterialSpeakerOption.MaleActor;

			var proximity = new Proximity(project, false);

			var characterIds = new HashSet<string>
			{
				CharacterVerseData.GetStandardCharacterId("GAL", Narrator),
				CharacterVerseData.GetStandardCharacterId("GAL", ExtraBiblical),
				CharacterVerseData.GetStandardCharacterId("GAL", Intro), // Not actually used in GAL test data
				CharacterVerseData.GetStandardCharacterId("GAL", BookOrChapter),
				idPaul
			};

			var minProximity = proximity.CalculateMinimumProximity(characterIds);

			Assert.That(minProximity.NumberOfBlocks, Is.EqualTo(MaxValue));
		}
	}
}
