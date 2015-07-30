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
			m_testProject = TestProject.CreateTestProject();
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
		public void CalculateMinimumProximity_OneCharacterGroupJesus_ReturnsNegative()
		{
			HashSet<string> characterIds = new HashSet<string>();

			characterIds.Add("Jesus");

			MinimumProximity minProximity = m_proximity.CalculateMinimumProximity(characterIds);

			Assert.AreEqual(-1, minProximity.NumberOfBlocks);
			Assert.AreEqual("Jesus", minProximity.FirstBlock.CharacterId);
			Assert.AreEqual("Jesus", minProximity.SecondBlock.CharacterId);
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
			Assert.AreEqual("narrator-MRK", minProximity.FirstBlock.CharacterId);
			Assert.AreEqual("Jesus", minProximity.SecondBlock.CharacterId);
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
			characterIds.Add("man with evil spirit");
			characterIds.Add("woman, bleeding for 12 years");
			characterIds.Add("people, sick");

			MinimumProximity minProximity = m_proximity.CalculateMinimumProximity(characterIds);

			Assert.Greater(minProximity.NumberOfBlocks, 0);
			Assert.AreEqual("man with evil spirit", minProximity.FirstBlock.CharacterId);
			Assert.AreEqual("woman, bleeding for 12 years", minProximity.SecondBlock.CharacterId);
			Assert.AreEqual("MRK", minProximity.FirstBook.BookId);
			Assert.AreEqual("MRK", minProximity.SecondBook.BookId);
			Assert.AreEqual(5, minProximity.FirstBlock.ChapterNumber);
			Assert.AreEqual(5, minProximity.SecondBlock.ChapterNumber);
			Assert.AreEqual(7, minProximity.FirstBlock.InitialStartVerseNumber);
			Assert.AreEqual(28, minProximity.SecondBlock.InitialStartVerseNumber);
		}

		[Test]
		public void CalculateMinimumProximity_JesusAndTeachersOfTheLaw_ReturnsOne()
		{
			HashSet<string> characterIds = new HashSet<string>();

			//Mark 2:16-17
			characterIds.Add("Jesus");
			characterIds.Add("teachers of the law");

			MinimumProximity minProximity = m_proximity.CalculateMinimumProximity(characterIds);

			Assert.AreEqual(1, minProximity.NumberOfBlocks);
			Assert.AreEqual("teachers of the law", minProximity.FirstBlock.CharacterId);
			Assert.AreEqual("Jesus", minProximity.SecondBlock.CharacterId);
			Assert.AreEqual("MRK", minProximity.FirstBook.BookId);
			Assert.AreEqual("MRK", minProximity.SecondBook.BookId);
			Assert.AreEqual(2, minProximity.FirstBlock.ChapterNumber);
			Assert.AreEqual(2, minProximity.SecondBlock.ChapterNumber);
			Assert.AreEqual(16, minProximity.FirstBlock.InitialStartVerseNumber);
			Assert.AreEqual(17, minProximity.SecondBlock.InitialStartVerseNumber);
		}

		[Test]
		public void CalculateMinimumProximity_JohnAndPeter_ReturnsSix()
		{
			HashSet<string> characterIds = new HashSet<string>();

			//Mark 9:5-11
			characterIds.Add("John");
			characterIds.Add("Peter (Simon)");

			MinimumProximity minProximity = m_proximity.CalculateMinimumProximity(characterIds);

			Assert.AreEqual(6, minProximity.NumberOfBlocks);
			Assert.AreEqual("Peter (Simon)", minProximity.FirstBlock.CharacterId);
			Assert.AreEqual("John", minProximity.SecondBlock.CharacterId);
			Assert.AreEqual("MRK", minProximity.FirstBook.BookId);
			Assert.AreEqual("MRK", minProximity.SecondBook.BookId);
			Assert.AreEqual(9, minProximity.FirstBlock.ChapterNumber);
			Assert.AreEqual(9, minProximity.SecondBlock.ChapterNumber);
			Assert.AreEqual(5, minProximity.FirstBlock.InitialStartVerseNumber);
			Assert.AreEqual(11, minProximity.SecondBlock.InitialStartVerseNumber);
		}
	}
}
