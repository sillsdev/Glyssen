using System;
using System.Collections.Generic;
using System.Linq;
using Glyssen;
using Glyssen.Character;
using Glyssen.Rules;
using Glyssen.VoiceActor;
using GlyssenTests.Properties;
using NUnit.Framework;
using SIL.Extensions;

namespace GlyssenTests.Rules
{
	[TestFixture]
	class CharacterGroupGeneratorTestsWithTwoBooksWithNoChildrenInScript
	{
		private Project m_testProject;
		private Dictionary<string, int> m_keyStrokesPerCharacterId;
		private CharacterByKeyStrokeComparer m_priorityComparer;

		[TestFixtureSetUp]
		public void TextFixtureSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerse;
			CharacterDetailData.TabDelimitedCharacterDetailData = Resources.TestCharacterDetail;
			RelatedCharactersData.Source = Resources.TestRelatedCharacters;
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.MRK, TestProject.TestBook.JUD);
			m_keyStrokesPerCharacterId = m_testProject.GetKeyStrokesByCharacterId();
			m_priorityComparer = new CharacterByKeyStrokeComparer(m_keyStrokesPerCharacterId);
		}

		[SetUp]
		public void SetUp()
		{
			m_testProject.VoiceActorList.Actors.Clear();
			m_testProject.CharacterGroupList.CharacterGroups.Clear();
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = false;
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			TestProject.DeleteTestProjectFolder();
		}

		[TestCase(0, 0)]
		[TestCase(1, 0)]
		[TestCase(1, 1)]
		[TestCase(2, 0)]
		[TestCase(0, 2)]
		public void GenerateCharacterGroups_MoreThanSevenActors_JesusInGroupByHimself(int numberOfMaleNarrators, int numberOfFemaleNarrators)
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = numberOfMaleNarrators;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = numberOfFemaleNarrators;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(8);
			var gen = new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId);
			var groups = gen.GenerateCharacterGroups();
			var jesusGroup = groups.Single(g => g.CharacterIds.Contains("Jesus"));
			Assert.AreEqual(1, jesusGroup.CharacterIds.Count);
		}

		[Test]
		public void GenerateCharacterGroups_SingleNarratorRequested_SingleNarratorGroupGenerated()
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 1;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(4);

			var gen = new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId);
			var groups = gen.GenerateCharacterGroups();
			var narratorGroup = groups.Single(g => g.CharacterIds.Contains("narrator-MRK"));
			Assert.AreEqual(2, narratorGroup.CharacterIds.Count);
			Assert.IsTrue(narratorGroup.CharacterIds.Contains("narrator-JUD"));
		}

		[Test]
		public void GenerateCharacterGroups_TwoBooks_EnoughActors_TwoNarratorGroupsGenerated()
		{
			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(10);

			var gen = new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId);
			var groups = gen.GenerateCharacterGroups();
			var narratorMrkGroup = groups.Single(g => g.CharacterIds.Contains("narrator-MRK"));
			var narratorJudGroup = groups.Single(g => g.CharacterIds.Contains("narrator-JUD"));
			Assert.AreEqual(1, narratorMrkGroup.CharacterIds.Count);
			Assert.AreEqual(1, narratorJudGroup.CharacterIds.Count);
			Assert.AreNotEqual(narratorMrkGroup, narratorJudGroup);
		}

		[Test, Ignore("Need Analyst to decide what should happen in this scenario")]
		public void GenerateCharacterGroups_SingleExtraBiblicalRequested_4Actors_SingleExtraBiblicalGroupGenerated()
		{
			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(4);

			//TODO request single extra-Biblical group
			var gen = new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId);
			var groups = gen.GenerateCharacterGroups();
			var extraBiblicalGroup = groups.Single(g => g.CharacterIds.Contains("extra-MRK"));
			Assert.AreEqual(4, extraBiblicalGroup.CharacterIds.Count);
			Assert.IsTrue(extraBiblicalGroup.CharacterIds.Contains("extra-JUD"));
			Assert.IsTrue(extraBiblicalGroup.CharacterIds.Contains("BC-MRK"));
			Assert.IsTrue(extraBiblicalGroup.CharacterIds.Contains("BC-JUD"));
		}

		[Test]
		public void GenerateCharacterGroups_SingleExtraBiblicalRequested_10Actors_SingleExtraBiblicalGroupGenerated()
		{
			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(10);

			//TODO request single extra-Biblical group
			var gen = new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId);
			var groups = gen.GenerateCharacterGroups();
			var extraBiblicalGroup = groups.Single(g => g.CharacterIds.Contains("extra-MRK"));
			Assert.AreEqual(4, extraBiblicalGroup.CharacterIds.Count);
			Assert.IsTrue(extraBiblicalGroup.CharacterIds.Contains("extra-JUD"));
			Assert.IsTrue(extraBiblicalGroup.CharacterIds.Contains("BC-MRK"));
			Assert.IsTrue(extraBiblicalGroup.CharacterIds.Contains("BC-JUD"));
		}

		[TestCase(0, 0)]
		[TestCase(1, 0)]
		[TestCase(2, 0)]
		[TestCase(0, 1)]
		[TestCase(1, 1)]
		[TestCase(0, 2)]
		public void GenerateCharacterGroups_VariousNumbersOfActors_CreatesEqualNumberOfGroupsUpToMax(int numberOfMaleNarrators, int numberOfFemaleNarrators)
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = numberOfMaleNarrators;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			//0
			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(0);
			Assert.AreEqual(0, new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId).GenerateCharacterGroups().Count);

			//1
			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(1);
			Assert.AreEqual(1, new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId).GenerateCharacterGroups().Count);

			// TODO: Need Analyst to decide what we want if there are only two actors.
			//2
			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(2);
			Assert.AreEqual(2, new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId).GenerateCharacterGroups().Count);

			// TODO: Need Analyst to decide what we want if there are only 2 actors & 1 actress.
			//3 (2 Males & 1 Female)
			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(2, 1);
			Assert.AreEqual(3, new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId).GenerateCharacterGroups().Count);

			//3 (3 Males)
			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(3);
			Assert.AreEqual(3, new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId).GenerateCharacterGroups().Count);

			//4 (3 Males & 1 Female)
			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(3, 1);
			Assert.AreEqual(4, new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId).GenerateCharacterGroups().Count);

			//4 (2 Males & 2 Females)
			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(2, 2);
			Assert.AreEqual(4, new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId).GenerateCharacterGroups().Count);

			//10
			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(10);
			Assert.AreEqual(10, new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId).GenerateCharacterGroups().Count);

			//20
			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(20);
			Assert.AreEqual(20, new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId).GenerateCharacterGroups().Count);

			//25
			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(18, 7);
			Assert.AreEqual(25, new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId).GenerateCharacterGroups().Count);
			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(25);
			Assert.AreEqual(25, new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId).GenerateCharacterGroups().Count);

			//50
			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(50);
			Assert.AreEqual(50, new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId).GenerateCharacterGroups().Count);

			//Max
			HashSet<string> includedCharacterIds = new HashSet<string>();
			foreach (var book in m_testProject.IncludedBooks)
				foreach (var block in book.GetScriptBlocks(true))
					if (!block.CharacterIsUnclear())
						includedCharacterIds.Add(block.CharacterIdInScript);
			int numberOfCharactersInProject = includedCharacterIds.Count(i => CharacterDetailData.Singleton.GetDictionary().ContainsKey(i));
			int numberOfCharactersRemovedByCoalescingCharactersWhichAreTheSameWithDifferentAges =
				RelatedCharactersData.Singleton.GetCharacterIdsForType(CharacterRelationshipType.SameCharacterWithMultipleAges).Intersect(includedCharacterIds).Count()
				- RelatedCharactersData.Singleton.GetAll().Count(rc => rc.RelationshipType == CharacterRelationshipType.SameCharacterWithMultipleAges && rc.CharacterIds.Intersect(includedCharacterIds).Any());
			int numberOfBooks = m_testProject.IncludedBooks.Count;
			int numberOfMaleActors = m_testProject.VoiceActorList.Actors.Count(a => a.Gender == ActorGender.Male);
			int numberOfFemaleActors = m_testProject.VoiceActorList.Actors.Count(a => a.Gender == ActorGender.Female);
			int numberOfNarratorAndExtraBiblicalCharactersRemovedByCoalescing = 5 - (Math.Min(numberOfBooks, Math.Max(1, Math.Min(numberOfMaleNarrators, numberOfMaleActors) + Math.Min(numberOfFemaleNarrators, numberOfFemaleActors))));
			int maxGroups = numberOfCharactersInProject - numberOfCharactersRemovedByCoalescingCharactersWhichAreTheSameWithDifferentAges
				- numberOfNarratorAndExtraBiblicalCharactersRemovedByCoalescing;
			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(maxGroups);
			Assert.AreEqual(maxGroups, new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId).GenerateCharacterGroups().Count);

			//Max + 1
			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(maxGroups + 1);
			Assert.AreEqual(maxGroups, new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId).GenerateCharacterGroups().Count);
		}

		[TestCase(20, 5, 1, 0)]
		[TestCase(25, 5, 2, 0)]
		[TestCase(25, 6, 1, 1)]
		[TestCase(30, 4, 2, 0)]
		public void GenerateCharacterGroups_SufficientCast_GroupsDoNotContainMixedGenders(
			int numberOfMaleActors, int numberOfFemaleActors, int numberOfMaleNarrators, int numberOfFemaleNarrators)
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = numberOfMaleNarrators;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = numberOfFemaleNarrators;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(numberOfMaleActors, numberOfFemaleActors);
			var groups = new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId).GenerateCharacterGroups();
			
			Assert.AreEqual(numberOfMaleActors + numberOfFemaleActors, groups.Count);
			Assert.IsFalse(groups.Any(g => g.CharacterIds.Any(c => CharacterDetailData.CharacterIsMale(c)) &&
											g.CharacterIds.Any(CharacterDetailData.CharacterIsFemale)));
		}

		[TestCase(0, 1)]
		[TestCase(1, 0)]
		[TestCase(0, 2)] // Fall back to 1 female narrator
		public void GenerateCharacterGroups_SingleNarrator_DifferentGendersOfActors_AppropriateGroupsCreatedForActors(int numberOfMaleNarrators, int numberOfFemaleNarrators)
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = numberOfMaleNarrators;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = numberOfFemaleNarrators;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(8, 2);
			var groups = new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId).GenerateCharacterGroups();

			Assert.AreEqual(10, groups.Count);
			var maleGroups = groups.Where(g => g.ContainsCharacterWithGender(CharacterGender.Male)).ToList();
			var femaleGroups = groups.Where(g => g.ContainsCharacterWithGender(CharacterGender.Female)).ToList();
			Assert.IsFalse(maleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.Female)));
			Assert.IsFalse(maleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.PreferFemale)));
			Assert.IsFalse(femaleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.Male)));
			Assert.IsFalse(femaleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.PreferMale)));
			Assert.AreEqual(1, groups.Single(g => g.CharacterIds.Contains("Jesus")).CharacterIds.Count);
			Assert.IsTrue(groups.Single(g => g.CharacterIds.Contains("narrator-MRK")).CharacterIds.All(
				i => CharacterVerseData.IsCharacterOfType(i, CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(groups.Single(g => g.CharacterIds.Contains("BC-MRK")).CharacterIds.All(
				i => CharacterVerseData.IsCharacterStandard(i, false)));
			Assert.GreaterOrEqual(groups.Count(g => g.CharacterIds.All(c =>
			{
				if (CharacterVerseData.IsCharacterStandard(c))
					return true;
				var gender = CharacterDetailData.Singleton.GetDictionary()[c].Gender;
				return gender != CharacterGender.Male && gender != CharacterGender.PreferMale;
			})), 2);

			Assert.True(maleGroups.Count <= groups.Count - numberOfFemaleNarrators);
			Assert.True(femaleGroups.Count <= groups.Count - numberOfMaleNarrators);
		}

		[TestCase(1, 1)]
		[TestCase(2, 0)]
		public void GenerateCharacterGroups_MultiNarrator_DifferentGendersOfActors_AppropriateGroupsCreatedForActors(int numberOfMaleNarrators, int numberOfFemaleNarrators)
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = numberOfMaleNarrators;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = numberOfFemaleNarrators;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(8, 2);
			var groups = new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId).GenerateCharacterGroups();

			Assert.AreEqual(10, groups.Count);
			var maleGroups = groups.Where(g => g.ContainsCharacterWithGender(CharacterGender.Male)).ToList();
			var femaleGroups = groups.Where(g => g.ContainsCharacterWithGender(CharacterGender.Female)).ToList();
			Assert.IsFalse(maleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.Female)));
			Assert.IsFalse(maleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.PreferFemale)));
			Assert.IsFalse(femaleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.Male)));
			Assert.IsFalse(femaleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.PreferMale)));
			Assert.AreEqual(1, groups.Single(g => g.CharacterIds.Contains("Jesus")).CharacterIds.Count);
			Assert.AreNotEqual(groups.Single(g => g.CharacterIds.Contains("narrator-MRK")), groups.Single(g => g.CharacterIds.Contains("narrator-JUD")));
			Assert.IsTrue(groups.Single(g => g.CharacterIds.Contains("BC-MRK")).CharacterIds.All(
				i => CharacterVerseData.IsCharacterStandard(i, false)));
			Assert.GreaterOrEqual(groups.Count(g => g.CharacterIds.All(c =>
			{
				if (CharacterVerseData.IsCharacterStandard(c))
					return true;
				var gender = CharacterDetailData.Singleton.GetDictionary()[c].Gender;
				return gender != CharacterGender.Male && gender != CharacterGender.PreferMale;
			})), 2);

			Assert.True(maleGroups.Count <= groups.Count - numberOfFemaleNarrators);
			Assert.True(femaleGroups.Count <= groups.Count - numberOfMaleNarrators);
		}

		[Test]
		public void GenerateCharacterGroups_DifferentGendersAndAgesOfTenActors_AppropriateGroupsCreatedForActorsWhichHaveCorrespondingCharacters()
		{
			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(7, 1, 1, 1);
			var groups = new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId).GenerateCharacterGroups();

			Assert.IsFalse(groups.Any(g => g.ContainsCharacterWithAge(CharacterAge.Child)), "No kids speak in Mark or Jude");
			var maleAdultGroups = groups.Where(g => g.ContainsCharacterWithGender(CharacterGender.Male)).ToList();
			var femaleAdultGroup = groups.Single(g => g.ContainsCharacterWithGender(CharacterGender.Female));
			Assert.IsFalse(maleAdultGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.Female)));
			Assert.IsFalse(maleAdultGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.PreferFemale)));
			Assert.IsFalse(femaleAdultGroup.ContainsCharacterWithGender(CharacterGender.Male));
			Assert.IsFalse(femaleAdultGroup.ContainsCharacterWithGender(CharacterGender.PreferMale));
			Assert.AreEqual(8, groups.Count);
			Assert.GreaterOrEqual(groups.Count(g => g.CharacterIds.All(c =>
			{
				if (CharacterVerseData.IsCharacterStandard(c))
					return true;
				var gender = CharacterDetailData.Singleton.GetDictionary()[c].Gender;
				return gender != CharacterGender.Female && gender != CharacterGender.PreferFemale;
			})), 7);
			Assert.GreaterOrEqual(groups.Count(g => g.CharacterIds.All(c =>
			{
				if (CharacterVerseData.IsCharacterStandard(c))
					return true;
				var gender = CharacterDetailData.Singleton.GetDictionary()[c].Gender;
				return gender != CharacterGender.Male && gender != CharacterGender.PreferMale;
			})), 1);
		}

		[Test]
		public void GenerateCharacterGroups_DifferentGendersAndAgesOfTwentyActors_AppropriateGroupsCreatedForActorsWhichHaveCorrespondingCharacters()
		{
			var actors = m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(18, 1, 1);
			var groups = new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId).GenerateCharacterGroups();

			Assert.AreEqual(19, groups.Count);
			Assert.IsFalse(groups.Any(g => g.ContainsCharacterWithAge(CharacterAge.Child)), "No kids speak in Mark or Jude");
			var maleAdultGroups = groups.Where(g => g.ContainsCharacterWithGender(CharacterGender.Male)).ToList();
			var femaleAdultGroup = groups.Single(g => g.ContainsCharacterWithGender(CharacterGender.Female));
			Assert.IsFalse(maleAdultGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.Female)));
			Assert.IsFalse(maleAdultGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.PreferFemale)));
			Assert.IsFalse(femaleAdultGroup.ContainsCharacterWithGender(CharacterGender.Male));
			Assert.IsFalse(femaleAdultGroup.ContainsCharacterWithGender(CharacterGender.PreferMale));
			Assert.AreEqual(actors[18].Id, femaleAdultGroup.VoiceActorId);
			Assert.GreaterOrEqual(groups.Count(g => g.CharacterIds.All(c =>
			{
				if (CharacterVerseData.IsCharacterStandard(c))
					return true;
				var gender = CharacterDetailData.Singleton.GetDictionary()[c].Gender;
				return gender != CharacterGender.Female && gender != CharacterGender.PreferFemale;
			})), 18);
			Assert.GreaterOrEqual(groups.Count(g => g.CharacterIds.All(c =>
			{
				if (CharacterVerseData.IsCharacterStandard(c))
					return true;
				var gender = CharacterDetailData.Singleton.GetDictionary()[c].Gender;
				return gender != CharacterGender.Male && gender != CharacterGender.PreferMale;
			})), 1);
		}

		[Test]
		public void GenerateCharacterGroups_IncludesOneCameoActor_GeneratesEmptyGroupAssignedToCameoActor()
		{
			var actors = m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(10);
			actors[0].IsCameo = true;

			var groups = new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId).GenerateCharacterGroups();
			Assert.AreEqual(10, groups.Count);
			Assert.AreEqual(1, groups.Count(g => g.IsVoiceActorAssigned));
			var groupWithActorAssigned = groups.Single(g => g.IsVoiceActorAssigned);
			Assert.AreEqual(actors[0].Id, groupWithActorAssigned.VoiceActorId);
			Assert.AreEqual(0, groupWithActorAssigned.CharacterIds.Count);
		}

		[Test]
		public void GenerateCharacterGroups_IncludesTwoCameoActors_GeneratesEmptyGroupAssignedToEachCameoActor()
		{
			var actors = m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(10);
			actors[0].IsCameo = true;
			actors[1].IsCameo = true;

			var groups = new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId).GenerateCharacterGroups();
			Assert.AreEqual(10, groups.Count);
			Assert.AreEqual(2, groups.Count(g => g.IsVoiceActorAssigned));
			var groupsWithActorAssigned = groups.Where(g => g.IsVoiceActorAssigned);
			Assert.True(groupsWithActorAssigned.Select(g => g.VoiceActorId).Contains(actors[0].Id));
			Assert.True(groupsWithActorAssigned.Select(g => g.VoiceActorId).Contains(actors[1].Id));
			Assert.True(groupsWithActorAssigned.All(g => g.CharacterIds.Count == 0));
		}

		[Test]
		public void GenerateCharacterGroups_CameoActorAlreadyAssignedToCharacter_GroupMaintained()
		{
			var actors = m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(10);
			actors[0].IsCameo = true;

			var assignedGroup = new CharacterGroup(m_testProject, m_priorityComparer);
			assignedGroup.AssignVoiceActor(actors[0].Id);
			assignedGroup.CharacterIds.Add("centurion at crucifixion");
			m_testProject.CharacterGroupList.CharacterGroups.Add(assignedGroup);

			var groups = new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId).GenerateCharacterGroups();
			var groupWithActorAssigned = groups.First(g => g.IsVoiceActorAssigned);
			Assert.AreEqual(1, groupWithActorAssigned.CharacterIds.Count);
			Assert.True(groupWithActorAssigned.CharacterIds.Contains("centurion at crucifixion"));
			Assert.False(groups.Where(g => g != groupWithActorAssigned).SelectMany(g => g.CharacterIds).Contains("centurion at crucifixion"));
		}

		[Test]
		public void GenerateCharacterGroups_CameoActorAssignedToCharacterNoLongerInUse_UnusedCharacterIsRemovedFromGroup()
		{
			var actors = m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(10);
			actors[0].IsCameo = true;

			var assignedGroup = new CharacterGroup(m_testProject, m_priorityComparer);
			assignedGroup.AssignVoiceActor(actors[0].Id);
			assignedGroup.CharacterIds.Add("Bob the Builder");
			m_testProject.CharacterGroupList.CharacterGroups.Add(assignedGroup);

			var groups = new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId).GenerateCharacterGroups();
			var groupWithActorAssigned = groups.Single(g => g.IsVoiceActorAssigned);
			Assert.False(groupWithActorAssigned.CharacterIds.Any());
			Assert.False(groups.Where(g => g != groupWithActorAssigned).SelectMany(g => g.CharacterIds).Contains("Bob the Builder"));
		}

		[Test]
		public void GenerateCharacterGroups_CameoActorAlreadyAssignedToJesus_GroupMaintainedAndJesusNotDuplicated()
		{
			var actors = m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(10);
			actors[0].IsCameo = true;

			var assignedGroup = new CharacterGroup(m_testProject, m_priorityComparer);
			assignedGroup.AssignVoiceActor(actors[0].Id);
			assignedGroup.CharacterIds.Add("Jesus");
			m_testProject.CharacterGroupList.CharacterGroups.Add(assignedGroup);

			var groups = new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId).GenerateCharacterGroups();
			var groupWithActorAssigned = groups.First(g => g.IsVoiceActorAssigned);
			Assert.AreEqual(1, groupWithActorAssigned.CharacterIds.Count);
			Assert.True(groupWithActorAssigned.CharacterIds.Contains("Jesus"));
			Assert.False(groups.Where(g => g != groupWithActorAssigned).SelectMany(g => g.CharacterIds).Contains("Jesus"));
		}

		[Test]
		public void GenerateCharacterGroups_CameoActorAlreadyAssignedToBc_GroupMaintainedAndBcNotDuplicated()
		{
			var actors = m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(10);
			actors[0].IsCameo = true;

			var assignedGroup = new CharacterGroup(m_testProject, m_priorityComparer);
			assignedGroup.AssignVoiceActor(actors[0].Id);
			assignedGroup.CharacterIds.Add("BC-MRK");
			m_testProject.CharacterGroupList.CharacterGroups.Add(assignedGroup);

			var groups = new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId).GenerateCharacterGroups();
			var groupWithActorAssigned = groups.First(g => g.IsVoiceActorAssigned);
			Assert.AreEqual(1, groupWithActorAssigned.CharacterIds.Count);
			Assert.True(groupWithActorAssigned.CharacterIds.Contains("BC-MRK"));
			Assert.False(groups.Where(g => g != groupWithActorAssigned).SelectMany(g => g.CharacterIds).Contains("BC-MRK"));
		}

		[Test]
		public void GenerateCharacterGroups_CameoActorAlreadyAssignedToExtraBiblical_GroupMaintainedAndExtraBiblicalNotDuplicated()
		{
			var actors = m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(10);
			actors[0].IsCameo = true;

			var assignedGroup = new CharacterGroup(m_testProject, m_priorityComparer);
			assignedGroup.AssignVoiceActor(actors[0].Id);
			assignedGroup.CharacterIds.Add("extra-MRK");
			m_testProject.CharacterGroupList.CharacterGroups.Add(assignedGroup);

			var groups = new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId).GenerateCharacterGroups();
			var groupWithActorAssigned = groups.First(g => g.IsVoiceActorAssigned);
			Assert.AreEqual(1, groupWithActorAssigned.CharacterIds.Count);
			Assert.True(groupWithActorAssigned.CharacterIds.Contains("extra-MRK"));
			Assert.False(groups.Where(g => g != groupWithActorAssigned).SelectMany(g => g.CharacterIds).Contains("extra-MRK"));
		}

		[Test]
		public void GenerateCharacterGroups_MaintainAssignments_OneAssignment_OneCharacter_AssignmentMaintained()
		{
			var actors = m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(5);
			new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId, false).UpdateProjectCharacterGroups();

			m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus").AssignVoiceActor(actors[0].Id);
			new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId, true).GenerateCharacterGroups();
			Assert.AreEqual(5, m_testProject.CharacterGroupList.CharacterGroups.Count);
			Assert.AreEqual(actors[0].Id, m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus").VoiceActorId);
		}

		[Test]
		public void GenerateCharacterGroups_MaintainAssignments_OneAssignment_TwoCharacters_AssignmentMaintainedForMostProminentCharacter()
		{
			var actors = m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(5);
			new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId, false).UpdateProjectCharacterGroups();

			var jesusGroup = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
			var originalJohnGroup = m_testProject.CharacterGroupList.GroupContainingCharacterId("John");
			originalJohnGroup.CharacterIds.ExceptWith(new [] {"John"});
			jesusGroup.CharacterIds.Add("John");
			jesusGroup.AssignVoiceActor(actors[0].Id);

			new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId, true).UpdateProjectCharacterGroups();
			Assert.AreEqual(5, m_testProject.CharacterGroupList.CharacterGroups.Count);
			Assert.AreEqual(actors[0].Id, m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus").VoiceActorId);
			Assert.IsFalse(m_testProject.CharacterGroupList.GroupContainingCharacterId("John").IsVoiceActorAssigned);
		}

		[Test]
		public void GenerateCharacterGroups_MaintainAssignments_TwoAssignments_GroupsAreCombined_AssignmentMaintainedForMostProminentCharacter()
		{
			var actors = m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(5);
			new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId, false).UpdateProjectCharacterGroups();
			Assert.IsTrue(m_keyStrokesPerCharacterId["BC-MRK"] < m_keyStrokesPerCharacterId["extra-MRK"],
				"For this tes to make sense as written, there have to be more characters associated with \"extra\" than with BC.");

			var newGroup = new CharacterGroup(m_testProject);
			m_testProject.CharacterGroupList.CharacterGroups.Add(newGroup);
			var bcGroup = m_testProject.CharacterGroupList.GroupContainingCharacterId("BC-MRK");
			Assert.IsTrue(bcGroup.CharacterIds.Contains("extra-MRK"));
			bcGroup.CharacterIds.ExceptWith(new[] { "extra-MRK" });
			newGroup.CharacterIds.Add("extra-MRK");

			bcGroup.AssignVoiceActor(actors[0].Id);
			newGroup.AssignVoiceActor(actors[1].Id);

			new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId, true).UpdateProjectCharacterGroups();
			Assert.AreEqual(5, m_testProject.CharacterGroupList.CharacterGroups.Count);
			var extraBiblicalGroup = m_testProject.CharacterGroupList.GroupContainingCharacterId("extra-MRK");
			bcGroup = m_testProject.CharacterGroupList.GroupContainingCharacterId("BC-MRK");
			Assert.AreEqual(extraBiblicalGroup, bcGroup);
			Assert.AreEqual(actors[1].Id, extraBiblicalGroup.VoiceActorId);
			Assert.False(m_testProject.CharacterGroupList.HasVoiceActorAssigned(actors[0].Id));
		}

		[Test]
		public void GenerateCharacterGroups_HasCameoAssignedButAttemptToMaintainAssignmentsIsFalse_MaintainsCameoGroup()
		{
			var actors = m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(3);
			new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId, false).UpdateProjectCharacterGroups();

			var actor4 = new Glyssen.VoiceActor.VoiceActor { Id = 4, IsCameo = true };
			m_testProject.VoiceActorList.Actors.Add(actor4);
			var cameoGroup = new CharacterGroup(m_testProject);
			m_testProject.CharacterGroupList.CharacterGroups.Add(cameoGroup);
			cameoGroup.CharacterIds.Add("John");
			m_testProject.CharacterGroupList.GroupContainingCharacterId("John").CharacterIds.ExceptWith(new[] { "John" });
			cameoGroup.AssignVoiceActor(actor4.Id);

			new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId, false).UpdateProjectCharacterGroups();
			var groups = m_testProject.CharacterGroupList.CharacterGroups;
			Assert.AreEqual(4, groups.Count);

			cameoGroup = groups.First(g => g.VoiceActorId == actor4.Id);
			Assert.AreEqual(actor4.Id, cameoGroup.VoiceActorId);
			Assert.AreEqual(1, cameoGroup.CharacterIds.Count);
			Assert.True(cameoGroup.CharacterIds.Contains("John"));
			Assert.False(groups.Where(g => g != cameoGroup).SelectMany(g => g.CharacterIds).Contains("John"));
		}

		[Test]
		public void GenerateCharacterGroups_NotEnoughActressesForDramatizationAndNarratorPreferences_FallbackToOneNarrator()
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 2;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(8, 2);

			var groups = new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId).GenerateCharacterGroups();
			Assert.AreEqual(groups.Single(g => g.CharacterIds.Contains("narrator-MRK")), groups.Single(g => g.CharacterIds.Contains("narrator-JUD")));
			Assert.AreEqual(1, groups.Count(g => g.ContainsCharacterWithGender(CharacterGender.Female)));
		}

		[Test]
		public void GenerateCharacterGroups_NotEnoughActressesForMinimumProximityAndNarratorPreferences_FallbackToOneNarrator()
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 2;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(8, 3);

			var groups = new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId).GenerateCharacterGroups();
			Assert.AreEqual(11, groups.Count);
			Assert.AreEqual(groups.Single(g => g.CharacterIds.Contains("narrator-MRK")), groups.Single(g => g.CharacterIds.Contains("narrator-JUD")));
			// Ideally, we'd like to assert an exact value, but there isn't enough info in this test to predict whether the
			// "extra-biblical" group will be given to a male or female actor.
			Assert.IsTrue(groups.Count(g => g.ContainsCharacterWithGender(CharacterGender.Female) || g.ContainsCharacterWithGender(CharacterGender.PreferFemale)) <= 2);
		}

		// Currently, this test and the following one are basically the same (proximity is 1 no matter what). We wrote this test
		// thinking that maybe we should ensure at least one non-narrator group is set aside up front, but maybe that isn't really
		// desirable.
		[Test, Ignore("Need an analyst to determine what should happen in this case.")]
		public void GenerateCharacterGroups_NotEnoughActorsForDramatizationAndNarratorPreferences_FallbackToOneNarrator()
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 2;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(2, 2);

			var groups = new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId).GenerateCharacterGroups();
			Assert.AreEqual(groups.Single(g => g.CharacterIds.Contains("narrator-MRK")), groups.Single(g => g.CharacterIds.Contains("narrator-JUD")));
			Assert.AreEqual(1, groups.Count(g => g.ContainsCharacterWithGender(CharacterGender.Male)));
		}

		[Test]
		public void GenerateCharacterGroups_NotEnoughActorsForMinimumProximityAndFallbackIsNoBetter_DoNotFallbackToOneNarrator()
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 2;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			// With only 3 male actors, even if only one is the designated narrator, one will be reserved for deity.
			// That means that anywhere else where two other male characters are talking to each other, we'll still end up
			// with a proximity of 1. Since that's no better than what we get with two narrators, we go ahead and give
			// the user what they asked for (basically, each book will have a narrator and a non-narrator, plus a woman).
			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(3, 2);

			var groups = new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId).GenerateCharacterGroups();
			Assert.AreEqual(5, groups.Count);
			Assert.AreNotEqual(groups.Single(g => g.CharacterIds.Contains("narrator-MRK")), groups.Single(g => g.CharacterIds.Contains("narrator-JUD")));
		}

		[Test]
		public void GenerateCharacterGroups_NotEnoughActorsForMinimumProximityAndNarratorPreferences_FallbackToOneNarrator()
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 2;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(4, 2);

			var groups = new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId).GenerateCharacterGroups();
			Assert.AreEqual(6, groups.Count);
			Assert.AreEqual(groups.Single(g => g.CharacterIds.Contains("narrator-MRK")), groups.Single(g => g.CharacterIds.Contains("narrator-JUD")));
		}
	}

	[TestFixture]
	internal class CharacterGroupGeneratorTestsWithBookHavingSameCharacterWithTwoAges
	{
		private Project m_testProject;
		private Dictionary<string, int> m_keyStrokesPerCharacterId;

		[TestFixtureSetUp]
		public void TextFixtureSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerse;
			CharacterDetailData.TabDelimitedCharacterDetailData = Resources.TestCharacterDetail;
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.MRK, TestProject.TestBook.JOS);
			m_keyStrokesPerCharacterId = m_testProject.GetKeyStrokesByCharacterId();
		}

		[SetUp]
		public void SetUp()
		{
			m_testProject.VoiceActorList.Actors.Clear();
			m_testProject.CharacterGroupList.CharacterGroups.Clear();
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			TestProject.DeleteTestProjectFolder();
		}


		[Test]
		public void GenerateCharacterGroups_SameCharacterWithTwoAges_CharactersAreGeneratedInTheSameGroup()
		{
			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(10, 5);

			var groups = new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId).GenerateCharacterGroups();
			var sadduceesGroup = groups.Single(g => g.CharacterIds.Contains("Joshua"));
			var sadduceesOldGroup = groups.Single(g => g.CharacterIds.Contains("Joshua (old)"));

			Assert.AreEqual(sadduceesGroup, sadduceesOldGroup);
		}
	}

	[TestFixture]
	internal class CharacterGroupGeneratorTestsWithChildrenInScript
	{
		private Project m_testProject;
		private Dictionary<string, int> m_keyStrokesPerCharacterId;

		[TestFixtureSetUp]
		public void TextFixtureSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerse;
			CharacterDetailData.TabDelimitedCharacterDetailData = Resources.TestCharacterDetail;
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.LUK);
			m_keyStrokesPerCharacterId = m_testProject.GetKeyStrokesByCharacterId();
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			TestProject.DeleteTestProjectFolder();
		}

		[Test]
		public void GenerateCharacterGroups_20ActorsOfDifferentGendersAndAges_AppropriateGroupsCreatedForActors()
		{
			var actors = m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(17, 2, 1);
			var groups = new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId).GenerateCharacterGroups();

			Assert.AreEqual(20, groups.Count);
			var maleGroups = groups.Where(g => g.ContainsCharacterWithGender(CharacterGender.Male)).ToList();
			var femaleGroups = groups.Where(g => g.ContainsCharacterWithGender(CharacterGender.Female)).ToList();

			Assert.IsFalse(maleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.Female)));
			Assert.IsFalse(maleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.PreferFemale)));
			Assert.IsFalse(femaleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.Male)));
			Assert.IsFalse(femaleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.PreferMale)));
			Assert.AreEqual(1, groups.Single(g => g.CharacterIds.Contains("Jesus")).CharacterIds.Count);
			Assert.IsTrue(groups.Single(g => g.CharacterIds.Contains("narrator-LUK")).CharacterIds.All(
				i => CharacterVerseData.IsCharacterOfType(i, CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(groups.Single(g => g.CharacterIds.Contains("BC-LUK")).CharacterIds.All(
				i => CharacterVerseData.IsCharacterStandard(i, false)));
			Assert.GreaterOrEqual(groups.Count(g => g.CharacterIds.All(c =>
			{
				if (CharacterVerseData.IsCharacterStandard(c))
					return true;
				var gender = CharacterDetailData.Singleton.GetDictionary()[c].Gender;
				return gender != CharacterGender.Male && gender != CharacterGender.PreferMale;
			})), 2);

			// One male child actor and one male child character -- make assignment automatically
			var maleChildGroup = groups.Single(g => g.ContainsCharacterWithGender(CharacterGender.Male) && g.ContainsCharacterWithAge(CharacterAge.Child));
			Assert.AreEqual(actors[19].Id, maleChildGroup.VoiceActorId);
			Assert.AreEqual(1, maleChildGroup.CharacterIds.Count);
		}

		[Test]
		public void GenerateCharacterGroups_UnneededActors_AppropriateGroupsCreatedForActors()
		{
			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(13, 2, 3, 3);
			var groups = new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId).GenerateCharacterGroups();

			Assert.AreEqual(16, groups.Count);
			var maleGroups = groups.Where(g => g.ContainsCharacterWithGender(CharacterGender.Male)).ToList();
			var femaleGroups = groups.Where(g => g.ContainsCharacterWithGender(CharacterGender.Female)).ToList();

			Assert.IsFalse(maleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.Female)));
			Assert.IsFalse(maleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.PreferFemale)));
			Assert.IsFalse(femaleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.Male)));
			Assert.IsFalse(femaleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.PreferMale)));
			Assert.AreEqual(1, groups.Single(g => g.CharacterIds.Contains("Jesus")).CharacterIds.Count);
			Assert.IsTrue(groups.Single(g => g.CharacterIds.Contains("narrator-LUK")).CharacterIds.All(
				i => CharacterVerseData.IsCharacterOfType(i, CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(groups.Single(g => g.CharacterIds.Contains("BC-LUK")).CharacterIds.All(
				i => CharacterVerseData.IsCharacterStandard(i, false)));
			Assert.GreaterOrEqual(groups.Count(g => g.CharacterIds.All(c =>
			{
				if (CharacterVerseData.IsCharacterStandard(c))
					return true;
				var gender = CharacterDetailData.Singleton.GetDictionary()[c].Gender;
				return gender != CharacterGender.Male && gender != CharacterGender.PreferMale;
			})), 2);

			// Three male child actors and one male child character -- do not make assignment automatically
			var maleChildGroup = groups.Single(g => g.ContainsCharacterWithGender(CharacterGender.Male) && g.ContainsCharacterWithAge(CharacterAge.Child));
			Assert.IsFalse(maleChildGroup.IsVoiceActorAssigned);
			Assert.AreEqual(1, maleChildGroup.CharacterIds.Count);
		}
	}

	[TestFixture]
	class CharacterGroupGeneratorTestsWithHolySpiritInScript
	{
		private Project m_testProject;
		private Dictionary<string, int> m_keyStrokesPerCharacterId;

		[TestFixtureSetUp]
		public void TextFixtureSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerse;
			CharacterDetailData.TabDelimitedCharacterDetailData = Resources.TestCharacterDetail;
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.MRK, TestProject.TestBook.ACT);
			m_keyStrokesPerCharacterId = m_testProject.GetKeyStrokesByCharacterId();
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			TestProject.DeleteTestProjectFolder();
		}

		[Test]
		public void GenerateCharacterGroups_21Actors_GodAndJesusAndHolySpiritAndScriptureEachInOwnGroup()
		{
			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(21);
			var gen = new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId);
			var groups = gen.GenerateCharacterGroups();
			var jesusGroup = groups.Single(g => g.CharacterIds.Contains("Jesus"));
			var scriptureGroup = groups.Single(g => g.CharacterIds.Contains("scripture"));
			var godGroup = groups.Single(g => g.CharacterIds.Contains("God"));
			var hsGroup = groups.Single(g => g.CharacterIds.Contains("Holy Spirit, the"));
			Assert.AreEqual(1, jesusGroup.CharacterIds.Count);
			Assert.AreEqual(1, scriptureGroup.CharacterIds.Count);
			Assert.AreEqual(1, godGroup.CharacterIds.Count);
			Assert.AreEqual(1, hsGroup.CharacterIds.Count);
		}

		[Test]
		public void GenerateCharacterGroups_AtLeast10Actors_JesusAndHolySpiritEachInOwnGroupAndGodAndScriptureGroupedTogether()
		{
			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(12);
			var gen = new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId);
			var groups = gen.GenerateCharacterGroups();
			var jesusGroup = groups.Single(g => g.CharacterIds.Contains("Jesus"));
			var holySpiritGroup = groups.Single(g => g.CharacterIds.Contains("Holy Spirit, the"));
			var scriptureAndGodGroup = groups.Single(g => g.CharacterIds.Contains("God"));
			Assert.That(scriptureAndGodGroup.CharacterIds.Contains("scripture"));
			Assert.AreEqual(1, jesusGroup.CharacterIds.Count);
			Assert.AreEqual(1, holySpiritGroup.CharacterIds.Count);
			Assert.AreEqual(2, scriptureAndGodGroup.CharacterIds.Count);
		}

		[Test]
		public void GenerateCharacterGroups_AtLeast7Actors_GodAndHolySpiritAndScriptureInGroupByThemselves()
		{
			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(8);
			var gen = new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId);
			var groups = gen.GenerateCharacterGroups();
			var jesusGroup = groups.Single(g => g.CharacterIds.Contains("Jesus"));
			var dietyGroup = groups.Single(g => g.CharacterIds.Contains("God"));
			Assert.That(dietyGroup.CharacterIds.Contains("Holy Spirit, the"));
			Assert.That(dietyGroup.CharacterIds.Contains("scripture"));
			Assert.AreEqual(1, jesusGroup.CharacterIds.Count);
			Assert.AreEqual(3, dietyGroup.CharacterIds.Count);
		}

		[Test]
		public void GenerateCharacterGroups_AtLeast4Actors_GodAndHolySpiritAndScriptureGroupedWithJesus()
		{
			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(6);
			var gen = new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId);
			var groups = gen.GenerateCharacterGroups();
			var dietyGroup = groups.Single(g => g.CharacterIds.Contains("Jesus"));
			Assert.That(dietyGroup.CharacterIds.Contains("God"));
			Assert.That(dietyGroup.CharacterIds.Contains("Holy Spirit, the"));
			Assert.That(dietyGroup.CharacterIds.Contains("scripture"));
			Assert.AreEqual(4, dietyGroup.CharacterIds.Count);
		}

		[Test]
		public void GenerateCharacterGroups_FewerThan4Actors_GodAndHolySpiritAndScriptureAndJesusNotInIsolatedGroups()
		{
			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(3);
			var gen = new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId);
			var groups = gen.GenerateCharacterGroups();
			var jesusGroup = groups.Single(g => g.CharacterIds.Contains("Jesus"));
			Assert.True(jesusGroup.CharacterIds.Count > 1);
			var godGroup = groups.Single(g => g.CharacterIds.Contains("God"));
			Assert.True(godGroup.CharacterIds.Count > 1);
			var hsGroup = groups.Single(g => g.CharacterIds.Contains("Holy Spirit, the"));
			Assert.True(hsGroup.CharacterIds.Count > 1);
			var scriptureGroup = groups.Single(g => g.CharacterIds.Contains("scripture"));
			Assert.True(scriptureGroup.CharacterIds.Count > 1);
		}
	}

	[TestFixture]
	internal class CharacterGroupGeneratorTestsWithJesusAsBitPartInScript
	{
		private Project m_testProject;
		private Dictionary<string, int> m_keyStrokesPerCharacterId;

		[TestFixtureSetUp]
		public void TextFixtureSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerse;
			CharacterDetailData.TabDelimitedCharacterDetailData = Resources.TestCharacterDetail;
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.ACT);
			m_keyStrokesPerCharacterId = m_testProject.GetKeyStrokesByCharacterId();
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			TestProject.DeleteTestProjectFolder();
		}

		[SetUp]
		public void Setup()
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = false;
		}

		[Test]
		public void GenerateCharacterGroups_FewActors_JesusInGroupByHimself()
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 1;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(7);
			var gen = new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId);
			var groups = gen.GenerateCharacterGroups();
			var jesusGroup = groups.Single(g => g.CharacterIds.Contains("Jesus"));
			Assert.AreEqual(1, jesusGroup.CharacterIds.Count);
		}
	}

	[TestFixture]
	internal class CharacterGroupGeneratorTestsWithSingleVoiceBook
	{
		private Project m_testProject;
		private Dictionary<string, int> m_keyStrokesPerCharacterId;

		[TestFixtureSetUp]
		public void TextFixtureSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerse;
			CharacterDetailData.TabDelimitedCharacterDetailData = Resources.TestCharacterDetail;
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.LUK);
			m_testProject.IncludedBooks[0].SingleVoice = true;
			m_keyStrokesPerCharacterId = m_testProject.GetKeyStrokesByCharacterId();
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			TestProject.DeleteTestProjectFolder();
		}

		[Test]
		public void GenerateCharacterGroups_OneBookSingleVoice_AllLinesAreNarrator()
		{
			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(7);
			var gen = new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId);
			var groups = gen.GenerateCharacterGroups();
			var singleGroup = groups.Single(g => g.CharacterIds.Contains("narrator-LUK"));
			Assert.AreEqual(1, singleGroup.CharacterIds.Count);
			Assert.AreEqual(1, groups.Count);
		}
	}

	[TestFixture]
	internal class CharacterGroupGeneratorTestsWithSingleVoiceBookAndNonSingleVoiceBook
	{
		private Project m_testProject;
		private Dictionary<string, int> m_keyStrokesPerCharacterId;

		[TestFixtureSetUp]
		public void TextFixtureSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerse;
			CharacterDetailData.TabDelimitedCharacterDetailData = Resources.TestCharacterDetail;
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.LUK, TestProject.TestBook.ACT);
			m_testProject.IncludedBooks[0].SingleVoice = true;
			m_keyStrokesPerCharacterId = m_testProject.GetKeyStrokesByCharacterId();
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			TestProject.DeleteTestProjectFolder();
		}

		[SetUp]
		public void SetUp()
		{
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = false;
		}

		[TestCase(2, 0)]
		[TestCase(1, 1)]
		public void GenerateCharacterGroups_MultiNarrator_OneBookIsSingleVoice_ExplicitNarratorPreferences_AllLinesAreNarrator(int numMale, int numFemale)
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = numMale;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = numFemale;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(6, 2);
			var gen = new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId);
			var groups = gen.GenerateCharacterGroups();
			var narratorLukeGroup = groups.Single(g => g.CharacterIds.Contains("narrator-LUK"));
			var narratorActsGroup = groups.Single(g => g.CharacterIds.Contains("narrator-ACT"));
			Assert.AreEqual(1, narratorLukeGroup.CharacterIds.Count);
			Assert.AreEqual(1, narratorActsGroup.CharacterIds.Count);
			Assert.False(groups.Any(g => g.CharacterIds.Contains("BC-LUK")));
			Assert.AreEqual(8, groups.Count);
		}

		[TestCase(1, 0)]
		[TestCase(0, 1)]
		[TestCase(0, 2)] // Not enough female actors, so will end up being a single female narrator
		public void GenerateCharacterGroups_SingleNarrator_OneBookIsSingleVoice_ExplicitNarratorPreferences_AllLinesAreNarrator(int numMale, int numFemale)
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = numMale;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = numFemale;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(5, 2);
			var gen = new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId);
			var groups = gen.GenerateCharacterGroups();
			var narratorLukeGroup = groups.Single(g => g.CharacterIds.Contains("narrator-LUK"));
			Assert.AreEqual(2, narratorLukeGroup.CharacterIds.Count);
			Assert.True(narratorLukeGroup.CharacterIds.Contains("narrator-ACT"));
			Assert.False(groups.Any(g => g.CharacterIds.Contains("BC-LUK")));
			Assert.AreEqual(7, groups.Count);
		}

		[Test]
		public void GenerateCharacterGroups_OneBookIsSingleVoice_DefaultNarratorPreferences_AllLinesForSingleVoiceBookAreNarrator()
		{
			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(5, 2);
			var gen = new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId);
			var groups = gen.GenerateCharacterGroups();
			Assert.AreEqual(groups.Single(g => g.CharacterIds.Contains("narrator-LUK")), groups.Single(g => g.CharacterIds.Contains("narrator-ACT")));
			Assert.False(groups.Any(g => g.CharacterIds.Contains("BC-LUK")));
			Assert.AreEqual(7, groups.Count);
		}
	}

	[TestFixture]
	internal class CharacterGroupGeneratorTestsWithScriptModifiedDuringTest
	{
		private Project m_testProject;
		private Dictionary<string, int> m_keyStrokesPerCharacterId;

		[TestFixtureSetUp]
		public void TextFixtureSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerse;
			CharacterDetailData.TabDelimitedCharacterDetailData = Resources.TestCharacterDetail;
		}

		[SetUp]
		public void SetUp()
		{
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.LUK, TestProject.TestBook.ACT);
			m_keyStrokesPerCharacterId = m_testProject.GetKeyStrokesByCharacterId();
		}

		[TearDown]
		public void TearDown()
		{
			TestProject.DeleteTestProjectFolder();
		}

		[Test]
		public void GenerateCharacterGroups_CameoAssignedToCharacterWhichIsRemovedFromScript_RegenerationDropsCharacter()
		{
			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(7);
			var cameoActor = m_testProject.VoiceActorList.Actors[0];
			cameoActor.IsCameo = true;
			var gen = new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId);
			var groups = gen.GenerateCharacterGroups();
			m_testProject.CharacterGroupList.CharacterGroups.AddRange(groups);

			var cameoGroup = m_testProject.CharacterGroupList.GetGroupsAssignedToActor(0).Single();

			var zachGroup = groups.Single(g => g.CharacterIds.Contains("Zaccheaus"));
			zachGroup.CharacterIds.Remove("Zaccheaus");
			cameoGroup.CharacterIds = new CharacterIdHashSet { "Zaccheaus" };

			zachGroup = groups.Single(g => g.CharacterIds.Contains("Zaccheaus"));
			Assert.AreEqual(1, zachGroup.CharacterIds.Count);

			m_testProject.AvailableBooks[0].IncludeInScript = false;
			m_keyStrokesPerCharacterId = m_testProject.GetKeyStrokesByCharacterId();
			gen = new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId);

			groups = gen.GenerateCharacterGroups();

			cameoGroup = groups.Single(g => g.VoiceActorId == cameoActor.Id);
			Assert.AreEqual(7, groups.Count);
			Assert.AreEqual(0, groups.Count(g => g.CharacterIds.Contains("Zaccheaus")));
			Assert.AreEqual(0, cameoGroup.CharacterIds.Count);
		}

		[Test]
		public void GenerateCharacterGroups_ProjectCharacterDetailExistsAndInScript_ProjectCharacterDetailIncludedInGeneratedGroups()
		{
			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(8, 2);
			m_testProject.AddProjectCharacterDetail(new CharacterDetail { CharacterId = "Bobette", Gender = CharacterGender.Female });

			m_testProject.IncludedBooks[0].Blocks[3].CharacterId = "Bobette";
			m_keyStrokesPerCharacterId = m_testProject.GetKeyStrokesByCharacterId();

			var groups = new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId).GenerateCharacterGroups();
			Assert.True(groups.Any(g => g.CharacterIds.Contains("Bobette")));
		}

		[Test]
		public void GenerateCharacterGroups_ProjectCharacterDetailExistsButNotInScript_ProjectCharacterDetailNotIncludedInGeneratedGroups()
		{
			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(8, 2);
			m_testProject.AddProjectCharacterDetail(new CharacterDetail { CharacterId = "Bobette", Gender = CharacterGender.Female });

			var groups = new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId).GenerateCharacterGroups();
			Assert.False(groups.Any(g => g.CharacterIds.Contains("Bobette")));
		}
	}

	[TestFixture]
	internal class CharacterGroupGeneratorTestsWithLotsOfBooks
	{
		private Project m_testProject;
		private Dictionary<string, int> m_keyStrokesPerCharacterId;

		[TestFixtureSetUp]
		public void TextFixtureSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerseOct2015;
			CharacterDetailData.TabDelimitedCharacterDetailData = Resources.TestCharacterDetailOct2015;
			m_testProject = TestProject.CreateTestProject(
				TestProject.TestBook.MRK,
				TestProject.TestBook.LUK,
				TestProject.TestBook.ACT,
				TestProject.TestBook.GAL,
				TestProject.TestBook.EPH,
				TestProject.TestBook.PHM,
				TestProject.TestBook.HEB,
				TestProject.TestBook.IJN,
				TestProject.TestBook.IIJN,
				TestProject.TestBook.IIIJN,
				TestProject.TestBook.JUD,
				TestProject.TestBook.REV);
			m_keyStrokesPerCharacterId = m_testProject.GetKeyStrokesByCharacterId();
		}

		[SetUp]
		public void SetUp()
		{
			m_testProject.VoiceActorList.Actors.Clear();
			m_testProject.CharacterGroupList.CharacterGroups.Clear();
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = false;
		}

		[Test]
		public void GenerateCharacterGroups_NumberOfNarratorsMatchAuthors_NarratorsGroupedByAuthor()
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 6;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(34, 4);
			var groups = new CharacterGroupGenerator(m_testProject, m_keyStrokesPerCharacterId).GenerateCharacterGroups();

			Assert.AreEqual(38, groups.Count);
			var narMark = groups.Single(g => g.CharacterIds.Contains("narrator-MRK"));
			var narLuke = groups.Single(g => g.CharacterIds.Contains("narrator-LUK"));
			var narPaul = groups.Single(g => g.CharacterIds.Contains("narrator-EPH"));
			var narHebrews = groups.Single(g => g.CharacterIds.Contains("narrator-HEB"));
			var narJohn = groups.Single(g => g.CharacterIds.Contains("narrator-1JN"));
			var narJude = groups.Single(g => g.CharacterIds.Contains("narrator-JUD"));

			Assert.AreEqual(1, narMark.CharacterIds.Count);
			Assert.AreEqual(2, narLuke.CharacterIds.Count);
			Assert.IsTrue(narLuke.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("ACT", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.AreEqual(3, narPaul.CharacterIds.Count);
			Assert.IsTrue(narPaul.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("GAL", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(narPaul.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("PHM", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.AreEqual(1, narHebrews.CharacterIds.Count);
			Assert.AreEqual(4, narJohn.CharacterIds.Count);
			Assert.IsTrue(narJohn.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("2JN", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(narJohn.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("3JN", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(narJohn.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("REV", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.AreEqual(1, narJude.CharacterIds.Count);
		}
	}

	class CharacterGroupGeneratorTests
	{
		public static List<Glyssen.VoiceActor.VoiceActor> GetVoiceActors(int numberOfAdultMales, int numberOfAdultFemales = 0, int numberOfMaleChildren = 0, int numberOfFemaleChildren = 0)
		{
			var actorList = new List<Glyssen.VoiceActor.VoiceActor>();
			int actorNumber = 0;
			for (int i = 0; i < numberOfAdultMales; i++)
				actorList.Add(new Glyssen.VoiceActor.VoiceActor { Id = actorNumber++ } );
			for (int i = 0; i < numberOfAdultFemales; i++)
				actorList.Add(new Glyssen.VoiceActor.VoiceActor {  Id = actorNumber++, Gender = ActorGender.Female });
			for (int i = 0; i < numberOfMaleChildren; i++)
				actorList.Add(new Glyssen.VoiceActor.VoiceActor {  Id = actorNumber++, Age = ActorAge.Child });
			for (int i = 0; i < numberOfFemaleChildren; i++)
				actorList.Add(new Glyssen.VoiceActor.VoiceActor {  Id = actorNumber++, Gender = ActorGender.Female, Age = ActorAge.Child });
			return actorList;
		}
	}
}
