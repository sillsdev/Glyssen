using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Glyssen;
using Glyssen.Character;
using Glyssen.Dialogs;
using Glyssen.Properties;
using Glyssen.Rules;
using NUnit.Framework;
using SIL.IO;

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
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Properties.Resources.TestCharacterVerse;
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
			//TestProject.DeleteTestProjectFolder();
		}

		[Test]
		public void CalculateMinimumProximity_OneCharacterGroupJesus_ReturnsNegative()
		{
			HashSet<string> characterIds = new HashSet<string>();

			characterIds.Add("Jesus");

			int minProximity = m_proximity.CalculateMinimumProximity(characterIds);

			Assert.AreEqual(minProximity, -1);
		}

		[Test]
		public void CalculateMinimumProximity_JesusAndNarrator_ReturnsZero()
		{
			HashSet<string> characterIds = new HashSet<string>();

			characterIds.Add("Jesus");
			characterIds.Add("narrator-MRK");

			int minProximity = m_proximity.CalculateMinimumProximity(characterIds);

			Assert.AreEqual(minProximity, 0);
		}

		[Test]
		public void CalculateMinimumProximity_MultipleLesserSpeakers_ReturnsPositive()
		{
			HashSet<string> characterIds = new HashSet<string>();

			//Mark 5
			characterIds.Add("man with evil spirit");
			characterIds.Add("woman, bleeding for 12 years");
			characterIds.Add("people, sick");

			int minProximity = m_proximity.CalculateMinimumProximity(characterIds);

			Assert.Greater(minProximity, 0);
		}

		[Test]
		public void CalculateMinimumProximity_JesusAndTeachersOfTheLaw_FirstTwoBlocksThenOneThenMore_ReturnsOne()
		{
			HashSet<string> characterIds = new HashSet<string>();

			//Mark 2:14-17
			characterIds.Add("Jesus");
			characterIds.Add("teachers of the law");

			int minProximity = m_proximity.CalculateMinimumProximity(characterIds);

			Assert.AreEqual(minProximity, 1);
		}

		[Test]
		public void CalculateMinimumProximity_NeighborsAndJames_CharacterIdsWithinSlashes()
		{
			HashSet<string> characterIds = new HashSet<string>();

			//11 blocks between lines 3104 and 3152 in MRK.xml
			characterIds.Add("teachers of the law");
			characterIds.Add("Herodians");
			characterIds.Add("centurion=centurion");

			int minProximity = m_proximity.CalculateMinimumProximity(characterIds);

			Assert.AreEqual(minProximity, 11);
		}

		[Test]
		public void CalculateMinimumProximity_CharacterIdsGroupedTogether()
		{
			HashSet<string> characterIds = new HashSet<string>();

			characterIds.Add("teachers of the law");
			characterIds.Add("Pharisees=Pharisees");

			int minProximity = m_proximity.CalculateMinimumProximity(characterIds);

			Assert.Greater(minProximity, 0);			
		}
	}
}
