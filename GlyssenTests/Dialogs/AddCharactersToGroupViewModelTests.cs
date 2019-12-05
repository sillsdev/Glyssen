using System.Collections.Generic;
using System.Linq;
using Glyssen;
using Glyssen.Character;
using Glyssen.Dialogs;
using GlyssenEngine.Character;
using NUnit.Framework;

namespace GlyssenTests.Dialogs
{
	[TestFixture]
	class AddCharactersToGroupViewModelTests
	{
		private Project m_testProject;
		private AddCharactersToGroupViewModel m_model;
		private Dictionary<string, CharacterDetail> m_characterDetails;
		private Dictionary<string, int> m_keystrokesByCharacterId;
		private bool m_fullProjectRefreshRequired;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Properties.Resources.TestCharacterVerseOct2015;
			CharacterDetailData.TabDelimitedCharacterDetailData = Properties.Resources.TestCharacterDetailOct2015;
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.MRK);
		}

		[SetUp]
		public void SetUp()
		{
			m_testProject.ClearAssignCharacterStatus(); //Otherwise tests interfere with each other in an undesirable way
			
			// These 2 loops are to make sure m_characterDetails and m_keystrokesByCharacterId contain the same characters
			m_characterDetails = new Dictionary<string, CharacterDetail>();
			m_keystrokesByCharacterId = new Dictionary<string, int>();

			foreach (var kvp in m_testProject.AllCharacterDetailDictionary.Where(kvp => m_testProject.AllCharacterIds.Contains(kvp.Key)))
				m_characterDetails.Add(kvp.Key, kvp.Value);
			
			foreach (var kvp in m_testProject.KeyStrokesByCharacterId.Where(kvp => m_characterDetails.ContainsKey(kvp.Key)))
			{
				m_keystrokesByCharacterId.Add(kvp.Key, kvp.Value);
			}

			m_model = new AddCharactersToGroupViewModel(m_characterDetails, m_keystrokesByCharacterId, new CharacterIdHashSet());
		}

		[TearDown]
		public void Teardown()
		{
			if (m_fullProjectRefreshRequired)
			{
				TestFixtureTearDown();
				TestFixtureSetUp();
				m_fullProjectRefreshRequired = false;
			}
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			m_testProject = null;
			TestProject.DeleteTestProjectFolder();
		}

		[Test]
		public void Constructor_ValidObject()
		{
			Assert.AreEqual(m_characterDetails.Count, m_model.FilteredCharactersCount);
		}

		[Test]
		public void FilterCharacterIds_FindJohn_ActiveCountCorrect()
		{
			m_model.FilterCharacterIds("john");
			Assert.AreEqual(2, m_model.FilteredCharactersCount);
			Assert.AreEqual("John the Baptist", m_model.GetCharacterId(0));
			Assert.AreEqual("John", m_model.GetCharacterId(1));
		}
	}
}
