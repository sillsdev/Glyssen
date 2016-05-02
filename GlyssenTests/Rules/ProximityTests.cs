using System;
using System.Collections.Generic;
using Glyssen;
using Glyssen.Character;
using Glyssen.Rules;
using GlyssenTests.Properties;
using NUnit.Framework;

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
		}

		[SetUp]
		public void SetUp()
		{
			m_proximity = new Proximity(m_testProject.IncludedBooks);
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
			Assert.AreEqual("Jesus", minProximity.FirstCharacterId);
			Assert.AreEqual("teachers of religious law", minProximity.SecondCharacterId);
			Assert.AreEqual("MRK 2:5", minProximity.FirstReference);
			Assert.AreEqual("MRK 2:7", minProximity.SecondReference);
		}

		[Test]
		public void CalculateMinimumProximity_JohnAndPeter_ReturnsSix()
		{
			HashSet<string> characterIds = new HashSet<string>();

			//Mark 9:24-26
			characterIds.Add("father of demon-possessed boy");
			characterIds.Add("many in crowd");

			MinimumProximity minProximity = m_proximity.CalculateMinimumProximity(characterIds);

			Assert.AreEqual(3, minProximity.NumberOfBlocks);
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

			Assert.AreEqual(1, minProximity.NumberOfBlocks);
			Assert.AreEqual("Caiaphas, the high priest (old)", minProximity.FirstCharacterId);
			Assert.AreEqual("chief priests", minProximity.SecondCharacterId);
			Assert.AreEqual("MRK 14:63", minProximity.FirstReference);
			Assert.AreEqual("MRK 14:65", minProximity.SecondReference);
		}

		[Test]
		public void CalculateMinimumProximity_TreatStandardNonScriptureCharactersAsDistinct_ExtraBiblicalResultsInZeroProximityWithChapterNumber()
		{
			HashSet<string> characterIds = new HashSet<string>();

			characterIds.Add(CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.BookOrChapter));
			characterIds.Add(CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.ExtraBiblical));
			characterIds.Add(CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.Intro));

			MinimumProximity minProximity = m_proximity.CalculateMinimumProximity(characterIds, true);

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

			MinimumProximity minProximity = m_proximity.CalculateMinimumProximity(characterIds, false);

			Assert.AreEqual(Int32.MaxValue, minProximity.NumberOfBlocks);
			Assert.AreEqual(CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.BookOrChapter), minProximity.FirstCharacterId);
			Assert.AreEqual(minProximity.FirstCharacterId, minProximity.SecondCharacterId);
			Assert.AreEqual("MRK 0:0", minProximity.FirstReference);
			Assert.AreEqual(minProximity.FirstReference, minProximity.SecondReference);
		}
	}
}
