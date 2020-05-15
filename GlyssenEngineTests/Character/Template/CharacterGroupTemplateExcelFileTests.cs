﻿/// <summary>
///  This code is not compiled.
///  Experimental code. Not used at this time.
/// </summary>

using System;
using System.IO;
using GlyssenEngine.Character;
using GlyssenEngine.Character.Template;
using GlyssenEngineTests.Properties;
using NUnit.Framework;
using SIL.IO;

namespace GlyssenEngineTests.Character.Template
{
	[TestFixture]
	public class CharacterGroupTemplateExcelFileTests
	{
		private TempFile m_tempFile;
		private ICharacterGroupSource m_charGroupSource;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			m_tempFile = new TempFile();
			File.WriteAllBytes(m_tempFile.Path, Resources.TestCharacterGroups);
			m_charGroupSource = new CharacterGroupTemplateExcelFile(TestProject.CreateBasicTestProject(), m_tempFile.Path);
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			m_tempFile.Dispose();
		}

		[Test]
		public void NumberOfActors_TooFew_ThrowsArgumentException()
		{
			Assert.Throws<ArgumentException>(() => m_charGroupSource.GetTemplate(0));
		}

		[Test]
		public void GetTemplate_NumberOfActors_TooMany_GetsMaxAvailable()
		{
			CharacterGroupTemplate template = m_charGroupSource.GetTemplate(29);
			Assert.AreEqual(28, template.CharacterGroups.Count);
		}

		[Test]
		public void GetTemplate_1Actor_1Group()
		{
			CharacterGroupTemplate template = m_charGroupSource.GetTemplate(1);
			Assert.AreEqual(1, template.CharacterGroups.Count);
		}

		[Test]
		public void GetTemplate_28Actors_28Groups()
		{
			CharacterGroupTemplate template = m_charGroupSource.GetTemplate(28);
			Assert.AreEqual(28, template.CharacterGroups.Count);
		}

		[Test]
		public void GetTemplate_GroupContainsCharacter()
		{
			CharacterGroupTemplate template = m_charGroupSource.GetTemplate(21);
			CharacterGroup group;
			Assert.IsTrue(template.CharacterGroups.TryGetValue(3, out group));
			Assert.IsTrue(group.CharacterIds.Contains("John the Baptist"));

			template = m_charGroupSource.GetTemplate(22);
			Assert.IsTrue(template.CharacterGroups.TryGetValue(15, out group));
			Assert.IsTrue(group.CharacterIds.Contains("John the Baptist"));
		}

		[Test]
		public void GetTemplate_GetsFirstRow()
		{
			CharacterGroupTemplate template = m_charGroupSource.GetTemplate(1);
			CharacterGroup group;
			Assert.IsTrue(template.CharacterGroups.TryGetValue(1, out group));
			Assert.IsTrue(group.CharacterIds.Contains("Narr_001: Matthew"));
		}

		[Test]
		public void GetTemplate_GetsLastRow()
		{
			CharacterGroupTemplate template = m_charGroupSource.GetTemplate(28);
			CharacterGroup group;
			Assert.IsTrue(template.CharacterGroups.TryGetValue(8, out group));
			Assert.IsTrue(group.CharacterIds.Contains("Religious Leader_04"));
		}
	}
}
