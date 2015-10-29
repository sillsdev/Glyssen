using System;
using System.Collections.Generic;
using Glyssen;
using Glyssen.Character;
using Glyssen.Rules;
using GlyssenTests.Properties;
using NUnit.Framework;

namespace GlyssenTests.Rules
{
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
			Assert.AreEqual("Jesus", minProximity.FirstBlock.CharacterIdInScript);
			Assert.AreEqual("Jesus", minProximity.SecondBlock.CharacterIdInScript);
			Assert.AreEqual("MRK", minProximity.FirstBook.BookId);
			Assert.AreEqual("MRK", minProximity.SecondBook.BookId);
			Assert.AreEqual(1, minProximity.FirstBlock.ChapterNumber);
			Assert.AreEqual(1, minProximity.SecondBlock.ChapterNumber);
			Assert.AreEqual(17, minProximity.FirstBlock.InitialStartVerseNumber);
			Assert.AreEqual(17, minProximity.SecondBlock.InitialStartVerseNumber);
		}

		[Test]
		public void CalculateMinimumProximity_JesusAndNarrator_ReturnsZero()
		{
			HashSet<string> characterIds = new HashSet<string>();

			characterIds.Add("Jesus");
			characterIds.Add("narrator-MRK");

			MinimumProximity minProximity = m_proximity.CalculateMinimumProximity(characterIds);

			Assert.AreEqual(0, minProximity.NumberOfBlocks);
			Assert.AreEqual("narrator-MRK", minProximity.FirstBlock.CharacterIdInScript);
			Assert.AreEqual("Jesus", minProximity.SecondBlock.CharacterIdInScript);
			Assert.AreEqual("MRK", minProximity.FirstBook.BookId);
			Assert.AreEqual("MRK", minProximity.SecondBook.BookId);
			Assert.AreEqual(1, minProximity.FirstBlock.ChapterNumber);
			Assert.AreEqual(1, minProximity.SecondBlock.ChapterNumber);
			Assert.AreEqual(16, minProximity.FirstBlock.InitialStartVerseNumber);
			Assert.AreEqual(17, minProximity.SecondBlock.InitialStartVerseNumber);
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
			Assert.AreEqual("demons (Legion)", minProximity.FirstBlock.CharacterIdInScript);
			Assert.AreEqual("woman, bleeding for twelve years", minProximity.SecondBlock.CharacterId);
			Assert.AreEqual("MRK", minProximity.FirstBook.BookId);
			Assert.AreEqual(5, minProximity.FirstBlock.ChapterNumber);
			Assert.AreEqual(12, minProximity.FirstBlock.InitialStartVerseNumber);
			Assert.AreEqual("MRK", minProximity.SecondBook.BookId);
			Assert.AreEqual(5, minProximity.SecondBlock.ChapterNumber);
			Assert.AreEqual(28, minProximity.SecondBlock.InitialStartVerseNumber);
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
			Assert.AreEqual("Jesus", minProximity.FirstBlock.CharacterIdInScript);
			Assert.AreEqual("teachers of religious law", minProximity.SecondBlock.CharacterIdInScript);
			Assert.AreEqual("MRK", minProximity.FirstBook.BookId);
			Assert.AreEqual("MRK", minProximity.SecondBook.BookId);
			Assert.AreEqual(2, minProximity.FirstBlock.ChapterNumber);
			Assert.AreEqual(2, minProximity.SecondBlock.ChapterNumber);
			Assert.AreEqual(5, minProximity.FirstBlock.InitialStartVerseNumber);
			Assert.AreEqual(7, minProximity.SecondBlock.InitialStartVerseNumber);
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
			Assert.AreEqual("father of demon-possessed boy", minProximity.FirstBlock.CharacterId);
			Assert.AreEqual("many in crowd", minProximity.SecondBlock.CharacterId);
			Assert.AreEqual("MRK", minProximity.FirstBook.BookId);
			Assert.AreEqual("MRK", minProximity.SecondBook.BookId);
			Assert.AreEqual(9, minProximity.FirstBlock.ChapterNumber);
			Assert.AreEqual(9, minProximity.SecondBlock.ChapterNumber);
			Assert.AreEqual(24, minProximity.FirstBlock.InitialStartVerseNumber);
			Assert.AreEqual(26, minProximity.SecondBlock.InitialStartVerseNumber);
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
			Assert.AreEqual("Caiaphas, the high priest", minProximity.FirstBlock.CharacterId);
			Assert.AreEqual("Caiaphas, the high priest", minProximity.SecondBlock.CharacterId);
			Assert.AreEqual("MRK", minProximity.FirstBook.BookId);
			Assert.AreEqual("MRK", minProximity.SecondBook.BookId);
			Assert.AreEqual(14, minProximity.FirstBlock.ChapterNumber);
			Assert.AreEqual(14, minProximity.SecondBlock.ChapterNumber);
			Assert.AreEqual(60, minProximity.FirstBlock.InitialStartVerseNumber);
			Assert.AreEqual(60, minProximity.SecondBlock.InitialStartVerseNumber);
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
			Assert.AreEqual("Caiaphas, the high priest (old)", minProximity.FirstBlock.CharacterId);
			Assert.AreEqual("chief priests", minProximity.SecondBlock.CharacterId);
			Assert.AreEqual("MRK", minProximity.FirstBook.BookId);
			Assert.AreEqual("MRK", minProximity.SecondBook.BookId);
			Assert.AreEqual(14, minProximity.FirstBlock.ChapterNumber);
			Assert.AreEqual(14, minProximity.SecondBlock.ChapterNumber);
			Assert.AreEqual(63, minProximity.FirstBlock.InitialStartVerseNumber);
			Assert.AreEqual(65, minProximity.SecondBlock.InitialStartVerseNumber);
		}
	}
}
