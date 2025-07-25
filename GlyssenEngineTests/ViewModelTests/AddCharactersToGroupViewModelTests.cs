﻿using System.Collections.Generic;
using System.Linq;
using GlyssenCharacters;
using GlyssenEngine;
using GlyssenEngine.ViewModels;
using NUnit.Framework;
using CharacterIdHashSet = GlyssenEngine.Character.CharacterIdHashSet;
using Resources = GlyssenCharactersTests.Properties.Resources;

namespace GlyssenEngineTests.ViewModelTests
{
	[TestFixture]
	class AddCharactersToGroupViewModelTests
	{
		private Project m_testProject;
		private AddCharactersToGroupViewModel m_model;
		private Dictionary<string, CharacterDetail> m_characterDetails;
		private Dictionary<string, int> m_keystrokesByCharacterId;
		private bool m_fullProjectRefreshRequired;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerseOct2015;
			CharacterDetailData.TabDelimitedCharacterDetailData = Resources.TestCharacterDetailOct2015;
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
				OneTimeTearDown();
				OneTimeSetUp();
				m_fullProjectRefreshRequired = false;
			}
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			m_testProject = null;
			TestProject.DeleteTestProjects();
		}

		[Test]
		public void Constructor_ValidObject()
		{
			Assert.That(m_characterDetails.Count, Is.EqualTo(m_model.FilteredCharactersCount));
		}

		[Test]
		public void FilterCharacterIds_FindJohn_ActiveCountCorrect()
		{
			m_model.FilterCharacterIds("john");
			Assert.That(m_model.FilteredCharactersCount, Is.EqualTo(2));
			Assert.That(m_model.GetCharacterId(0), Is.EqualTo("John the Baptist"));
			Assert.That(m_model.GetCharacterId(1), Is.EqualTo("John"));
		}
	}
}
