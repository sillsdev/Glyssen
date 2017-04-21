using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Glyssen;
using Glyssen.Bundle;
using Glyssen.Character;
using Glyssen.Dialogs;
using Glyssen.Rules;
using Glyssen.Shared.Bundle;
using Glyssen.VoiceActor;
using GlyssenTests.Properties;
using NUnit.Framework;
using SIL.Extensions;

namespace GlyssenTests.Rules
{
	[TestFixture]
	class CharacterGroupGeneratorTestsWithTwoBooksWithNoChildrenInScript : CharacterGroupGeneratorAndAdjusterTestBase
	{
		[TestFixtureSetUp]
		public void TextFixtureSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerse;
			CharacterDetailData.TabDelimitedCharacterDetailData = Resources.TestCharacterDetail;
			RelatedCharactersData.Source = Resources.TestRelatedCharacters;
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.MRK, TestProject.TestBook.JUD);
			TestProject.SimulateDisambiguationForAllBooks(m_testProject);

			m_testProject.DramatizationPreferences.BookTitleAndChapterDramatization = ExtraBiblicalMaterialSpeakerOption.MaleActor;
			m_testProject.DramatizationPreferences.SectionHeadDramatization = ExtraBiblicalMaterialSpeakerOption.MaleActor;
		}

		[SetUp]
		public void SetUp()
		{
			m_testProject.VoiceActorList.AllActors.Clear();
			m_testProject.CharacterGroupList.CharacterGroups.Clear();
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.CastSizeOption = CastSizeOption.NotSet;
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

			SetVoiceActors(8);
			var gen = new CharacterGroupGenerator(m_testProject);
			var groups = gen.GenerateCharacterGroups();
			var jesusGroup = groups.Single(g => g.CharacterIds.Contains("Jesus"));
			Assert.AreEqual(1, jesusGroup.CharacterIds.Count);
		}

		[Test]
		public void GenerateCharacterGroups_SingleNarratorRequested_SingleNarratorGroupGenerated()
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 1;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			SetVoiceActors(4);

			var gen = new CharacterGroupGenerator(m_testProject);
			var groups = gen.GenerateCharacterGroups();
			var narratorGroup = GetNarratorGroupForBook(groups, "MRK");
			Assert.AreEqual(2, narratorGroup.CharacterIds.Count);
			Assert.IsTrue(narratorGroup.CharacterIds.Contains("narrator-JUD"));
		}

		[Test]
		public void GenerateCharacterGroups_TwoDifferentAuthors_TenActors_TwoNarratorGroupsGenerated()
		{
			SetVoiceActors(10);

			var gen = new CharacterGroupGenerator(m_testProject);
			var groups = gen.GenerateCharacterGroups();
			AssertThatThereAreTwoDistinctNarratorGroups(groups);
		}

		[TestCase(4)]
		[TestCase(10)]
		public void GenerateCharacterGroups_SingleExtraBiblicalRequested_SingleExtraBiblicalGroupGenerated(int numberOfMaleActors)
		{
			SetVoiceActors(numberOfMaleActors);

			//TODO request single extra-Biblical group (this is currently the default)
			var gen = new CharacterGroupGenerator(m_testProject);
			var groups = gen.GenerateCharacterGroups();
			var extraBiblicalGroup = groups.Single(g => g.CharacterIds.Contains("extra-MRK"));
			Assert.IsTrue(extraBiblicalGroup.CharacterIds.Contains("extra-JUD"));
			Assert.IsTrue(extraBiblicalGroup.CharacterIds.Contains("BC-MRK"));
			Assert.IsTrue(extraBiblicalGroup.CharacterIds.Contains("BC-JUD"));
		}

		[TestCase(1, 0)]
		[TestCase(2, 0)]
		[TestCase(0, 1)]
		[TestCase(1, 1)]
		[TestCase(0, 2)]
		public void GenerateCharacterGroups_VariousNumbersOfActors_CreatesEqualNumberOfGroupsUpToMax(int numberOfMaleNarrators, int numberOfFemaleNarrators)
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = numberOfMaleNarrators;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = numberOfFemaleNarrators;
			m_testProject.CharacterGroupGenerationPreferences.CastSizeOption = CastSizeOption.MatchVoiceActorList;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			//0
			Debug.WriteLine("=======================================");
			Debug.WriteLine("************** 0 Actors ***************");
			Debug.WriteLine("=======================================");
			SetVoiceActors(0);
			Assert.AreEqual(0, new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups().Count);

			if (numberOfMaleNarrators + numberOfFemaleNarrators == 1)
			{
				//1
				Debug.WriteLine("=======================================");
				Debug.WriteLine("************** 1 Actor ***************");
				Debug.WriteLine("=======================================");
				SetVoiceActors(1 - numberOfFemaleNarrators, numberOfFemaleNarrators);
				Assert.AreEqual(1, new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups().Count);
			}

			// TODO: Need Analyst to decide what we want if there are only two actors.
			//2
			Debug.WriteLine("=======================================");
			Debug.WriteLine("************** 2 Actors ***************");
			Debug.WriteLine("=======================================");
			SetVoiceActors(2 - numberOfFemaleNarrators, numberOfFemaleNarrators);
			Assert.AreEqual(2, new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups().Count);

			List<CharacterGroup> groups;

			// TODO: Need Analyst to decide what we want if there are only 2 actors & 1 actress.
			//3 (2 Males & 1 Female)
			if (numberOfFemaleNarrators == 0)
			{
				Debug.WriteLine("=======================================");
				Debug.WriteLine("******* 2 Male/1 Female Actors ********");
				Debug.WriteLine("=======================================");
				SetVoiceActors(2, 1);
				groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();
				Assert.AreEqual(3, groups.Count);
				if (numberOfMaleNarrators == 2)
					AssertThatThereAreTwoDistinctNarratorGroups(groups);
			}

			//3
			Debug.WriteLine("=======================================");
			Debug.WriteLine("******* 3 Actors ********");
			Debug.WriteLine("=======================================");
			SetVoiceActors(3 - numberOfFemaleNarrators, numberOfFemaleNarrators);
			groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();
			Assert.AreEqual(3, groups.Count);
			if (numberOfMaleNarrators + numberOfFemaleNarrators == 2)
				AssertThatThereAreTwoDistinctNarratorGroups(groups);

			//4 (3 Males & 1 Female)
			if (numberOfFemaleNarrators <= 1)
			{
				Debug.WriteLine("=======================================");
				Debug.WriteLine("******* 3,1 Actors ********");
				Debug.WriteLine("=======================================");
				SetVoiceActors(3, 1);
				groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();
				Assert.AreEqual(4, groups.Count);
				if (numberOfMaleNarrators + numberOfFemaleNarrators == 2)
					AssertThatThereAreTwoDistinctNarratorGroups(groups);
			}

			//4 (2 Males & 2 Females)
			Debug.WriteLine("=======================================");
			Debug.WriteLine("******* 2,2 Actors ********");
			Debug.WriteLine("=======================================");
			SetVoiceActors(2, 2);
			groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();
			Assert.AreEqual(4, groups.Count);
			if (numberOfMaleNarrators + numberOfFemaleNarrators == 2)
				AssertThatThereAreTwoDistinctNarratorGroups(groups);

			//10
			Debug.WriteLine("=======================================");
			Debug.WriteLine("******* 10 Actors ********");
			Debug.WriteLine("=======================================");
			SetVoiceActors(10 - numberOfFemaleNarrators, numberOfFemaleNarrators);
			groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();
			Assert.AreEqual(10, groups.Count);
			if (numberOfMaleNarrators + numberOfFemaleNarrators == 2)
				AssertThatThereAreTwoDistinctNarratorGroups(groups);

			//20
			Debug.WriteLine("=======================================");
			Debug.WriteLine("******* 20 Actors ********");
			Debug.WriteLine("=======================================");
			SetVoiceActors(20 - numberOfFemaleNarrators, numberOfFemaleNarrators);
			groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();
			Assert.AreEqual(20, groups.Count);
			if (numberOfMaleNarrators + numberOfFemaleNarrators == 2)
				AssertThatThereAreTwoDistinctNarratorGroups(groups);

			//25
			Debug.WriteLine("=======================================");
			Debug.WriteLine("******* 18,7 Actors ********");
			Debug.WriteLine("=======================================");
			SetVoiceActors(18, 7);
			groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();
			Assert.AreEqual(25, groups.Count);
			if (numberOfMaleNarrators + numberOfFemaleNarrators == 2)
				AssertThatThereAreTwoDistinctNarratorGroups(groups);

			SetVoiceActors(25 - numberOfFemaleNarrators, numberOfFemaleNarrators);
			Debug.WriteLine("=======================================");
			Debug.WriteLine("******* 25 Actors ********");
			Debug.WriteLine("=======================================");
			groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();
			Assert.AreEqual(25, groups.Count);
			if (numberOfMaleNarrators + numberOfFemaleNarrators == 2)
				AssertThatThereAreTwoDistinctNarratorGroups(groups);

			//50
			Debug.WriteLine("=======================================");
			Debug.WriteLine("******* 50 Actors ********");
			Debug.WriteLine("=======================================");
			SetVoiceActors(50 - numberOfFemaleNarrators, numberOfFemaleNarrators);
			groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();
			Assert.AreEqual(50, groups.Count);
			if (numberOfMaleNarrators + numberOfFemaleNarrators == 2)
				AssertThatThereAreTwoDistinctNarratorGroups(groups);

			//Max
			Debug.WriteLine("=======================================");
			Debug.WriteLine("******* Max Actors ********");
			Debug.WriteLine("=======================================");
			int numberOfCharactersInProject = m_testProject.TotalCharacterCount;
			int numberOfCharactersRemovedByCoalescingCharactersWhichAreTheSameWithDifferentAges =
				RelatedCharactersData.Singleton.GetCharacterIdsForType(CharacterRelationshipType.SameCharacterWithMultipleAges).Intersect(m_testProject.AllCharacterIds).Count()
				- RelatedCharactersData.Singleton.GetAll().Count(rc => rc.RelationshipType == CharacterRelationshipType.SameCharacterWithMultipleAges && rc.CharacterIds.Intersect(m_testProject.AllCharacterIds).Any());
			int numberOfFemaleActors = m_testProject.AllCharacterIds.Count(c =>
			{
				var gender = m_testProject.AllCharacterDetailDictionary[c].Gender;
				return gender == CharacterGender.Female || gender == CharacterGender.PreferFemale;
			});
			const int kNumberOfExtraBiblicalCharactersRemovedByCoalescing = 3;
			int numberOfMaleActors = numberOfCharactersInProject - m_testProject.IncludedBooks.Count - numberOfFemaleActors + numberOfMaleNarrators -
				numberOfCharactersRemovedByCoalescingCharactersWhichAreTheSameWithDifferentAges - 
				kNumberOfExtraBiblicalCharactersRemovedByCoalescing;
			numberOfFemaleActors += numberOfFemaleNarrators;

			SetVoiceActors(numberOfMaleActors, numberOfFemaleActors);
			groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();
			foreach (var characterGroup in groups.Where(g => g.CharacterIds.Count != 1 || g.GroupIdLabel == CharacterGroup.Label.Female))
			{
				Debug.WriteLine("Group " + characterGroup.GroupIdForUiDisplay + " characters: " + characterGroup.CharacterIds);
			}
			foreach (var characterId in m_testProject.AllCharacterIds)
			{
				if (!groups.Any(g => g.CharacterIds.Contains(characterId)))
					Debug.WriteLine("Character not assigned to any group : " + characterId);
			}
			Assert.AreEqual(numberOfMaleActors + numberOfFemaleActors, groups.Count);
			if (numberOfMaleNarrators + numberOfFemaleNarrators == 2)
				AssertThatThereAreTwoDistinctNarratorGroups(groups);

			//Max + 1 Male
			Debug.WriteLine("=======================================");
			Debug.WriteLine("******* Max + 1 Male Actors ********");
			Debug.WriteLine("=======================================");
			SetVoiceActors(numberOfMaleActors + 1, numberOfFemaleActors);
			groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();
			Assert.AreEqual(numberOfMaleActors + numberOfFemaleActors, groups.Count);
			if (numberOfMaleNarrators + numberOfFemaleNarrators == 2)
				AssertThatThereAreTwoDistinctNarratorGroups(groups);

			//Max + 1 Female
			Debug.WriteLine("=======================================");
			Debug.WriteLine("******* Max + 1 Female Actors ********");
			Debug.WriteLine("=======================================");
			SetVoiceActors(numberOfMaleActors, numberOfFemaleActors + 1);
			groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();
			Assert.AreEqual(numberOfMaleActors + numberOfFemaleActors, groups.Count);
			if (numberOfMaleNarrators + numberOfFemaleNarrators == 2)
				AssertThatThereAreTwoDistinctNarratorGroups(groups);
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

			SetVoiceActors(numberOfMaleActors, numberOfFemaleActors);
			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();
			
			Assert.AreEqual(numberOfMaleActors + numberOfFemaleActors, groups.Count);
			VerifyProximityAndGenderConstraintsForAllGroups(groups);
			if (numberOfMaleNarrators + numberOfFemaleNarrators == 2)
				AssertThatThereAreTwoDistinctNarratorGroups(groups);
		}

		[TestCase(0, 1)]
		[TestCase(1, 0)]
		public void GenerateCharacterGroups_SingleNarrator_DifferentGendersOfActors_AppropriateGroupsCreatedForActors(int numberOfMaleNarrators, int numberOfFemaleNarrators)
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = numberOfMaleNarrators;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = numberOfFemaleNarrators;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			SetVoiceActors(9 - numberOfFemaleNarrators, 2 + numberOfFemaleNarrators);
			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();

			Assert.AreEqual(11, groups.Count);
			var maleGroups = groups.Where(g => g.ContainsCharacterWithGender(CharacterGender.Male)).ToList();
			var femaleGroups = groups.Where(g => g.ContainsCharacterWithGender(CharacterGender.Female)).ToList();
			Assert.IsFalse(maleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.Female)));
			Assert.IsFalse(maleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.PreferFemale)));
			Assert.IsFalse(femaleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.Male)));
			Assert.IsFalse(femaleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.PreferMale)));
			Assert.AreEqual(1, groups.Single(g => g.CharacterIds.Contains("Jesus")).CharacterIds.Count);
			Assert.IsTrue(GetNarratorGroupForBook(groups, "MRK").CharacterIds.All(
				i => CharacterVerseData.IsCharacterOfType(i, CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(groups.Single(g => g.CharacterIds.Contains("BC-MRK")).CharacterIds.All(
				i => CharacterVerseData.IsCharacterExtraBiblical(i)));
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

		// This test used to require only 8 male actors and 2 female actors to achieve the desired results.
		// However, after parser changes for PG-485, that was no longer sufficient, and it was changed to 10 and 2 in order to pass.
		// See PG-543 for an idea of how to improve the algorithm to keep the required number of actors lower.
		[TestCase(1, 1)]
		[TestCase(2, 0)]
		[TestCase(0, 2)]
		public void GenerateCharacterGroups_MultiNarrator_DifferentGendersOfActors_AppropriateGroupsCreatedForActors(int numberOfMaleNarrators, int numberOfFemaleNarrators)
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = numberOfMaleNarrators;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = numberOfFemaleNarrators;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			SetVoiceActors(10 - numberOfFemaleNarrators, 2 + numberOfFemaleNarrators);
			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();

			Assert.AreEqual(12, groups.Count);
			var maleGroups = groups.Where(g => g.ContainsCharacterWithGender(CharacterGender.Male)).ToList();
			var femaleGroups = groups.Where(g => g.ContainsCharacterWithGender(CharacterGender.Female)).ToList();
			Assert.IsFalse(maleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.Female)));
			Assert.IsFalse(maleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.PreferFemale)));
			Assert.IsFalse(femaleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.Male)));
			Assert.IsFalse(femaleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.PreferMale)));
			Assert.AreEqual(1, groups.Single(g => g.CharacterIds.Contains("Jesus")).CharacterIds.Count);
			AssertThatThereAreTwoDistinctNarratorGroups(groups);

			var a = groups.Single(g => g.CharacterIds.Contains("BC-MRK"));
			var b = a.CharacterIds.All(CharacterVerseData.IsCharacterExtraBiblical);

			Assert.IsTrue(groups.Single(g => g.CharacterIds.Contains("BC-MRK")).CharacterIds.All(CharacterVerseData.IsCharacterExtraBiblical));

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
			SetVoiceActors(7, 1, 1, 1);
			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();

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
			SetVoiceActors(18, 1, 1);
			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();

			Assert.AreEqual(19, groups.Count);
			Assert.IsFalse(groups.Any(g => g.ContainsCharacterWithAge(CharacterAge.Child)), "No kids speak in Mark or Jude");
			var maleAdultGroups = groups.Where(g => g.ContainsCharacterWithGender(CharacterGender.Male)).ToList();
			var femaleAdultGroup = groups.Single(g => g.ContainsCharacterWithGender(CharacterGender.Female));
			Assert.IsFalse(maleAdultGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.Female)));
			Assert.IsFalse(maleAdultGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.PreferFemale)));
			Assert.IsFalse(femaleAdultGroup.ContainsCharacterWithGender(CharacterGender.Male));
			Assert.IsFalse(femaleAdultGroup.ContainsCharacterWithGender(CharacterGender.PreferMale));
			Assert.AreEqual(m_testProject.VoiceActorList.AllActors[18].Id, femaleAdultGroup.VoiceActorId);
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
			SetVoiceActors(10);
			m_testProject.VoiceActorList.AllActors[0].IsCameo = true;

			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();
			Assert.AreEqual(10, groups.Count);
			Assert.AreEqual(1, groups.Count(g => g.IsVoiceActorAssigned));
			var groupWithActorAssigned = groups.Single(g => g.IsVoiceActorAssigned);
			Assert.AreEqual(m_testProject.VoiceActorList.AllActors[0].Id, groupWithActorAssigned.VoiceActorId);
			Assert.AreEqual(0, groupWithActorAssigned.CharacterIds.Count);
		}

		[Test]
		public void GenerateCharacterGroups_IncludesTwoCameoActors_GeneratesEmptyGroupAssignedToEachCameoActor()
		{
			SetVoiceActors(10);
			m_testProject.VoiceActorList.AllActors[0].IsCameo = true;
			m_testProject.VoiceActorList.AllActors[1].IsCameo = true;

			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();
			Assert.AreEqual(10, groups.Count);
			Assert.AreEqual(2, groups.Count(g => g.IsVoiceActorAssigned));
			var groupsWithActorAssigned = groups.Where(g => g.IsVoiceActorAssigned).ToList();
			Assert.True(groupsWithActorAssigned.Select(g => g.VoiceActorId).Contains(m_testProject.VoiceActorList.AllActors[0].Id));
			Assert.True(groupsWithActorAssigned.Select(g => g.VoiceActorId).Contains(m_testProject.VoiceActorList.AllActors[1].Id));
			Assert.True(groupsWithActorAssigned.All(g => g.CharacterIds.Count == 0));
		}

		[Test]
		public void GenerateCharacterGroups_CameoActorAlreadyAssignedToCharacter_GroupMaintained()
		{
			SetVoiceActors(10);
			m_testProject.VoiceActorList.AllActors[0].IsCameo = true;

			var assignedGroup = new CharacterGroup(m_testProject);
			assignedGroup.AssignVoiceActor(m_testProject.VoiceActorList.AllActors[0].Id);
			assignedGroup.CharacterIds.Add("centurion at crucifixion");
			m_testProject.CharacterGroupList.CharacterGroups.Add(assignedGroup);

			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();
			var groupWithActorAssigned = groups.First(g => g.IsVoiceActorAssigned);
			Assert.AreEqual(1, groupWithActorAssigned.CharacterIds.Count);
			Assert.True(groupWithActorAssigned.CharacterIds.Contains("centurion at crucifixion"));
			Assert.False(groups.Where(g => g != groupWithActorAssigned).SelectMany(g => g.CharacterIds).Contains("centurion at crucifixion"));
		}

		[Test]
		public void GenerateCharacterGroups_CameoActorAlreadyAssignedToCharactersWithCloseProximity_ProximityOfCameoRolesNotConsidered()
		{
			SetVoiceActors(20, 3);
			m_testProject.VoiceActorList.AllActors[0].IsCameo = true;

			var assignedGroup = new CharacterGroup(m_testProject);
			assignedGroup.AssignVoiceActor(m_testProject.VoiceActorList.AllActors[0].Id);
			assignedGroup.CharacterIds.Add("centurion at crucifixion");
			assignedGroup.CharacterIds.Add("man with sponge");
			m_testProject.CharacterGroupList.CharacterGroups.Add(assignedGroup);

			var generator = new CharacterGroupGenerator(m_testProject);
			var groups = generator.GenerateCharacterGroups();
			var groupWithActorAssigned = groups.First(g => g.IsVoiceActorAssigned);
			Assert.AreEqual(2, groupWithActorAssigned.CharacterIds.Count);
			Assert.True(groupWithActorAssigned.CharacterIds.Contains("centurion at crucifixion"));
			Assert.True(generator.MinimumProximity > Proximity.kDefaultMinimumProximity);
		}

		[Test]
		public void GenerateCharacterGroups_CameoActorAssignedToCharacterNoLongerInUse_UnusedCharacterIsRemovedFromGroup()
		{
			SetVoiceActors(10);
			m_testProject.VoiceActorList.AllActors[0].IsCameo = true;

			var assignedGroup = new CharacterGroup(m_testProject);
			assignedGroup.AssignVoiceActor(m_testProject.VoiceActorList.AllActors[0].Id);
			assignedGroup.CharacterIds.Add("Bob the Builder");
			m_testProject.CharacterGroupList.CharacterGroups.Add(assignedGroup);

			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();
			var groupWithActorAssigned = groups.Single(g => g.IsVoiceActorAssigned);
			Assert.False(groupWithActorAssigned.CharacterIds.Any());
			Assert.False(groups.Where(g => g != groupWithActorAssigned).SelectMany(g => g.CharacterIds).Contains("Bob the Builder"));
		}

		[Test]
		public void GenerateCharacterGroups_CameoActorAlreadyAssignedToJesus_GroupMaintainedAndJesusNotDuplicated()
		{
			SetVoiceActors(10);
			m_testProject.VoiceActorList.AllActors[0].IsCameo = true;

			var assignedGroup = new CharacterGroup(m_testProject);
			assignedGroup.AssignVoiceActor(m_testProject.VoiceActorList.AllActors[0].Id);
			assignedGroup.CharacterIds.Add("Jesus");
			m_testProject.CharacterGroupList.CharacterGroups.Add(assignedGroup);

			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();
			var groupWithActorAssigned = groups.First(g => g.IsVoiceActorAssigned);
			Assert.AreEqual(1, groupWithActorAssigned.CharacterIds.Count);
			Assert.True(groupWithActorAssigned.CharacterIds.Contains("Jesus"));
			Assert.False(groups.Where(g => g != groupWithActorAssigned).SelectMany(g => g.CharacterIds).Contains("Jesus"));
		}

		[Test]
		public void GenerateCharacterGroups_CameoActorAlreadyAssignedToBc_GroupMaintainedAndBcNotDuplicated()
		{
			SetVoiceActors(10);
			m_testProject.VoiceActorList.AllActors[0].IsCameo = true;

			var assignedGroup = new CharacterGroup(m_testProject);
			assignedGroup.AssignVoiceActor(m_testProject.VoiceActorList.AllActors[0].Id);
			assignedGroup.CharacterIds.Add("BC-MRK");
			m_testProject.CharacterGroupList.CharacterGroups.Add(assignedGroup);

			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();
			var groupWithActorAssigned = groups.First(g => g.IsVoiceActorAssigned);
			Assert.AreEqual(1, groupWithActorAssigned.CharacterIds.Count);
			Assert.True(groupWithActorAssigned.CharacterIds.Contains("BC-MRK"));
			Assert.False(groups.Where(g => g != groupWithActorAssigned).SelectMany(g => g.CharacterIds).Contains("BC-MRK"));
		}

		[Test]
		public void GenerateCharacterGroups_CameoActorAlreadyAssignedToExtraBiblical_GroupMaintainedAndExtraBiblicalNotDuplicated()
		{
			SetVoiceActors(10);
			m_testProject.VoiceActorList.AllActors[0].IsCameo = true;

			var assignedGroup = new CharacterGroup(m_testProject);
			assignedGroup.AssignVoiceActor(m_testProject.VoiceActorList.AllActors[0].Id);
			assignedGroup.CharacterIds.Add("extra-MRK");
			m_testProject.CharacterGroupList.CharacterGroups.Add(assignedGroup);

			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();
			var groupWithActorAssigned = groups.First(g => g.IsVoiceActorAssigned);
			Assert.AreEqual(1, groupWithActorAssigned.CharacterIds.Count);
			Assert.True(groupWithActorAssigned.CharacterIds.Contains("extra-MRK"));
			Assert.False(groups.Where(g => g != groupWithActorAssigned).SelectMany(g => g.CharacterIds).Contains("extra-MRK"));
		}

		[Test]
		public void GenerateCharacterGroups_MaintainAssignments_OneAssignment_OneCharacter_AssignmentMaintained()
		{
			SetVoiceActors(5);
			var generator = new CharacterGroupGenerator(m_testProject);
			generator.GenerateCharacterGroups();
			generator.ApplyGeneratedGroupsToProject(false);

			m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus").AssignVoiceActor(m_testProject.VoiceActorList.AllActors[0].Id);
			new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();
			Assert.AreEqual(5, m_testProject.CharacterGroupList.CharacterGroups.Count);
			Assert.AreEqual(m_testProject.VoiceActorList.AllActors[0].Id, m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus").VoiceActorId);
		}

		[Test]
		public void GenerateCharacterGroups_MaintainAssignments_OneAssignment_TwoCharacters_AssignmentMaintainedForMostProminentCharacter()
		{
			SetVoiceActors(5);
			var generator = new CharacterGroupGenerator(m_testProject);
			generator.GenerateCharacterGroups();
			generator.ApplyGeneratedGroupsToProject(false);

			var jesusGroup = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
			var originalJohnGroup = m_testProject.CharacterGroupList.GroupContainingCharacterId("John");
			originalJohnGroup.CharacterIds.ExceptWith(new [] {"John"});
			jesusGroup.CharacterIds.Add("John");
			jesusGroup.AssignVoiceActor(m_testProject.VoiceActorList.AllActors[0].Id);

			generator = new CharacterGroupGenerator(m_testProject);
			generator.GenerateCharacterGroups();
			generator.ApplyGeneratedGroupsToProject();
			Assert.AreEqual(5, m_testProject.CharacterGroupList.CharacterGroups.Count);
			Assert.AreEqual(m_testProject.VoiceActorList.AllActors[0].Id, m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus").VoiceActorId);
			Assert.IsFalse(m_testProject.CharacterGroupList.GroupContainingCharacterId("John").IsVoiceActorAssigned);
		}

		[Test]
		public void GenerateCharacterGroups_MaintainAssignments_TwoAssignments_GroupsAreCombined_AssignmentMaintainedForMostProminentCharacter()
		{
			// allow BC and Extra to be in separate or the same group
			m_testProject.DramatizationPreferences.BookTitleAndChapterDramatization = ExtraBiblicalMaterialSpeakerOption.MaleActor;
			m_testProject.DramatizationPreferences.SectionHeadDramatization = ExtraBiblicalMaterialSpeakerOption.MaleActor;

			SetVoiceActors(5);
			var generator = new CharacterGroupGenerator(m_testProject);
			generator.GenerateCharacterGroups();
			generator.ApplyGeneratedGroupsToProject(false);
			Assert.IsTrue(m_testProject.KeyStrokesByCharacterId["BC-MRK"] < m_testProject.KeyStrokesByCharacterId["extra-MRK"],
				"For this test to make sense as written, there have to be more key strokes associated with \"extra\" than with BC.");

			// expect BC-MRK and extra-MRK to be placed in same group
			var bcGroup = m_testProject.CharacterGroupList.GroupContainingCharacterId("BC-MRK");
			Assert.IsTrue(bcGroup.CharacterIds.Contains("extra-MRK"));

			// put extra-MRK in a new group and assign voice actors
			var newGroup = new CharacterGroup(m_testProject);
			m_testProject.CharacterGroupList.CharacterGroups.Add(newGroup);
			bcGroup.CharacterIds.ExceptWith(new[] { "extra-MRK" });
			newGroup.CharacterIds.Add("extra-MRK");
			bcGroup.AssignVoiceActor(m_testProject.VoiceActorList.AllActors[0].Id);
			newGroup.AssignVoiceActor(m_testProject.VoiceActorList.AllActors[1].Id);

			// re-generate the groups
			generator = new CharacterGroupGenerator(m_testProject);
			generator.GenerateCharacterGroups();
			generator.ApplyGeneratedGroupsToProject();

			// expect one group per voice actor
			Assert.AreEqual(5, m_testProject.CharacterGroupList.CharacterGroups.Count);

			// expect BC-MRK and extra-MRK to be placed back in same group
			var extraBiblicalGroup = m_testProject.CharacterGroupList.GroupContainingCharacterId("extra-MRK");
			bcGroup = m_testProject.CharacterGroupList.GroupContainingCharacterId("BC-MRK");
			Assert.AreEqual(extraBiblicalGroup, bcGroup);

			// expect the voice actor for extra-MRK has not changed
			Assert.AreEqual(m_testProject.VoiceActorList.AllActors[1].Id, extraBiblicalGroup.VoiceActorId);
			Assert.False(m_testProject.CharacterGroupList.HasVoiceActorAssigned(m_testProject.VoiceActorList.AllActors[0].Id));
		}

		[Test]
		public void GenerateCharacterGroups_HasCameoAssignedButAttemptToMaintainAssignmentsIsFalse_MaintainsCameoGroup()
		{
			SetVoiceActors(3);
			var generator = new CharacterGroupGenerator(m_testProject);
			generator.GenerateCharacterGroups();
			generator.ApplyGeneratedGroupsToProject(false);

			var actor4 = new Glyssen.VoiceActor.VoiceActor { Id = 4, IsCameo = true };
			m_testProject.VoiceActorList.AllActors.Add(actor4);
			var cameoGroup = new CharacterGroup(m_testProject);
			m_testProject.CharacterGroupList.CharacterGroups.Add(cameoGroup);
			cameoGroup.CharacterIds.Add("John");
			m_testProject.CharacterGroupList.GroupContainingCharacterId("John").CharacterIds.ExceptWith(new[] { "John" });
			cameoGroup.AssignVoiceActor(actor4.Id);

			generator = new CharacterGroupGenerator(m_testProject);
			generator.GenerateCharacterGroups();
			generator.ApplyGeneratedGroupsToProject(false);
			var groups = m_testProject.CharacterGroupList.CharacterGroups;
			Assert.AreEqual(4, groups.Count);

			cameoGroup = groups.First(g => g.VoiceActorId == actor4.Id);
			Assert.AreEqual(actor4.Id, cameoGroup.VoiceActorId);
			Assert.AreEqual(1, cameoGroup.CharacterIds.Count);
			Assert.True(cameoGroup.CharacterIds.Contains("John"));
			Assert.False(groups.Where(g => g != cameoGroup).SelectMany(g => g.CharacterIds).Contains("John"));
		}

		[Test]
		public void GenerateCharacterGroups_NotEnoughActressesToKeepNarratorAndCharacterRolesDistinct_NarratorGroupForJudeIncludesFemaleCharactersInMark()
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 2;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			SetVoiceActors(8, 2);

			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();
			AssertThatThereAreTwoDistinctNarratorGroups(groups);
			Assert.IsTrue(GetNarratorGroupForBook(groups, "JUD").ContainsCharacterWithGender(CharacterGender.Female));
		}

		[Test]
		public void GenerateCharacterGroups_NotEnoughActressesForMinimumProximityAndNarratorPreferences_NarratorGroupForJudeIncludesSomeFemaleCharactersInMark()
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 2;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			SetVoiceActors(11, 3);

			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();
			Assert.AreEqual(14, groups.Count);
			AssertThatThereAreTwoDistinctNarratorGroups(groups);
			Assert.IsTrue(GetNarratorGroupForBook(groups, "JUD").ContainsCharacterWithGender(CharacterGender.Female));
			VerifyProximityAndGenderConstraintsForAllGroups(groups, false, true);
		}

		[Test]
		public void GenerateCharacterGroups_NotEnoughActorsForMinimumProximity_NarratorsDoCharacterRolesInOtherBook()
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 2;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			SetVoiceActors(3, 2);

			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();
			Assert.AreEqual(5, groups.Count);
			AssertThatThereAreTwoDistinctNarratorGroups(groups);
			Assert.IsTrue(GetNarratorGroupForBook(groups, "JUD").CharacterIds.Count > 10); // All the male characters in MRK, except for the Trinity
			Assert.IsTrue(GetNarratorGroupForBook(groups, "MRK").CharacterIds.Intersect(new[]
			{
				"angel, arch-, Michael",
				"Enoch",
				"apostles"
			}).Any());
		}

		[Test]
		public void GenerateCharacterGroups_NotEnoughActorsForMinimumProximityAndNarratorPreferences_FallbackToOneNarrator()
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 2;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			SetVoiceActors(4, 2);

			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();
			Assert.AreEqual(6, groups.Count);
			AssertThatThereAreTwoDistinctNarratorGroups(groups);
			Assert.IsTrue(GetNarratorGroupForBook(groups, "JUD").CharacterIds.Count > 1); // Some of the male characters in MRK
			Assert.IsTrue(GetNarratorGroupForBook(groups, "MRK").CharacterIds.Count > 1);
			Assert.IsTrue(GetNarratorGroupForBook(groups, "MRK").CharacterIds.Intersect(new List<string>(new[]
			{
				"angel, arch-, Michael",
				"Enoch",
				"apostles"
			})).Any());
		}

		private void AssertThatThereAreTwoDistinctNarratorGroups(List<CharacterGroup> groups)
		{
			var narratorMrkGroup = GetNarratorGroupForBook(groups, "MRK");
			var narratorJudGroup = GetNarratorGroupForBook(groups, "JUD");
			Assert.AreNotEqual(narratorMrkGroup, narratorJudGroup);
		}
	}

	[TestFixture]
	public class CharacterGroupGeneratorTestsWithBookHavingSameCharacterWithTwoAges : CharacterGroupGeneratorAndAdjusterTestBase
	{
		[TestFixtureSetUp]
		public void TextFixtureSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerseOct2015;
			CharacterDetailData.TabDelimitedCharacterDetailData = Resources.TestCharacterDetailOct2015;
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.MRK, TestProject.TestBook.JOS);
		}

		[SetUp]
		public void SetUp()
		{
			m_testProject.VoiceActorList.AllActors.Clear();
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
			SetVoiceActors(10, 5);

			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();
			var joshuaGroup = groups.Single(g => g.CharacterIds.Contains("Joshua"));
			var joshuaOldGroup = groups.Single(g => g.CharacterIds.Contains("Joshua (old)"));

			Assert.AreEqual(joshuaGroup, joshuaOldGroup);
		}
	}

	[TestFixture]
	public class CharacterGroupGeneratorTestsWithChildrenInScript : CharacterGroupGeneratorAndAdjusterTestBase
	{
		[TestFixtureSetUp]
		public void TextFixtureSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerse;
			CharacterDetailData.TabDelimitedCharacterDetailData = Resources.TestCharacterDetail;
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.LUK);

			m_testProject.DramatizationPreferences.BookTitleAndChapterDramatization = ExtraBiblicalMaterialSpeakerOption.ActorOfEitherGender;
			m_testProject.DramatizationPreferences.SectionHeadDramatization = ExtraBiblicalMaterialSpeakerOption.ActorOfEitherGender;
			m_testProject.DramatizationPreferences.BookIntroductionsDramatization = ExtraBiblicalMaterialSpeakerOption.ActorOfEitherGender;
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			TestProject.DeleteTestProjectFolder();
		}

		[Test]
		public void GenerateCharacterGroups_20ActorsOfDifferentGendersAndAges_AppropriateGroupsCreatedForActors()
		{
			SetVoiceActors(17, 2, 1);
			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();

			Assert.AreEqual(20, groups.Count);
			var maleGroups = groups.Where(g => g.ContainsCharacterWithGender(CharacterGender.Male)).ToList();
			var femaleGroups = groups.Where(g => g.ContainsCharacterWithGender(CharacterGender.Female)).ToList();

			Assert.IsFalse(maleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.Female)));
			Assert.IsFalse(maleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.PreferFemale)));
			Assert.IsFalse(femaleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.Male)));
			Assert.IsFalse(femaleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.PreferMale)));
			Assert.AreEqual(1, groups.Single(g => g.CharacterIds.Contains("Jesus")).CharacterIds.Count);

			var narratorGroup = GetNarratorGroupForBook(groups, "LUK");
			Assert.IsTrue(narratorGroup.CharacterIds.All(
				i => CharacterVerseData.IsCharacterOfType(i, CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(groups.Single(g => g.CharacterIds.Contains("BC-LUK")).CharacterIds.All(
				CharacterVerseData.IsCharacterExtraBiblical));
			Assert.GreaterOrEqual(groups.Count(g => g.CharacterIds.All(c =>
			{
				if (CharacterVerseData.IsCharacterStandard(c))
					return true;
				var gender = CharacterDetailData.Singleton.GetDictionary()[c].Gender;
				return gender != CharacterGender.Male && gender != CharacterGender.PreferMale;
			})), 2);

			// One male child actor and one male child character -- make assignment automatically
			var maleChildGroup = groups.Single(g => g.ContainsCharacterWithGender(CharacterGender.Male) && g.ContainsCharacterWithAge(CharacterAge.Child));
			Assert.AreEqual(m_testProject.VoiceActorList.AllActors[19].Id, maleChildGroup.VoiceActorId);
			Assert.AreEqual(1, maleChildGroup.CharacterIds.Count);
		}

		[Test]
		public void GenerateCharacterGroups_UnneededActors_AppropriateGroupsCreatedForActors()
		{
			SetVoiceActors(13, 2, 3, 3);
			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();

			Assert.AreEqual(16, groups.Count);
			var maleGroups = groups.Where(g => g.ContainsCharacterWithGender(CharacterGender.Male)).ToList();
			var femaleGroups = groups.Where(g => g.ContainsCharacterWithGender(CharacterGender.Female)).ToList();

			Assert.IsFalse(maleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.Female)));
			Assert.IsFalse(maleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.PreferFemale)));
			Assert.IsFalse(femaleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.Male)));
			Assert.IsFalse(femaleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.PreferMale)));
			Assert.AreEqual(1, groups.Single(g => g.CharacterIds.Contains("Jesus")).CharacterIds.Count);
			Assert.IsTrue(GetNarratorGroupForBook(groups, "LUK").CharacterIds.All(
				i => CharacterVerseData.IsCharacterOfType(i, CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(groups.Single(g => g.CharacterIds.Contains("BC-LUK")).CharacterIds.All(CharacterVerseData.IsCharacterExtraBiblical));
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
	class CharacterGroupGeneratorTestsWithHolySpiritInScript : CharacterGroupGeneratorAndAdjusterTestBase
	{
		[TestFixtureSetUp]
		public void TextFixtureSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerse;
			CharacterDetailData.TabDelimitedCharacterDetailData = Resources.TestCharacterDetail;
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.MRK, TestProject.TestBook.ACT);

			m_testProject.DramatizationPreferences.BookTitleAndChapterDramatization = ExtraBiblicalMaterialSpeakerOption.ActorOfEitherGender;
			m_testProject.DramatizationPreferences.SectionHeadDramatization = ExtraBiblicalMaterialSpeakerOption.Omitted;
			m_testProject.DramatizationPreferences.BookIntroductionsDramatization = ExtraBiblicalMaterialSpeakerOption.Omitted;
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			TestProject.DeleteTestProjectFolder();
		}

		[Test]
		public void GenerateCharacterGroups_21Actors_GodAndJesusAndHolySpiritAndScriptureEachInOwnGroup()
		{
			SetVoiceActors(21);
			var gen = new CharacterGroupGenerator(m_testProject);
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
			SetVoiceActors(12);
			var gen = new CharacterGroupGenerator(m_testProject);
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
			SetVoiceActors(8);
			var gen = new CharacterGroupGenerator(m_testProject);
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
			SetVoiceActors(6);
			var gen = new CharacterGroupGenerator(m_testProject);
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
			SetVoiceActors(3);
			var gen = new CharacterGroupGenerator(m_testProject);
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
	internal class CharacterGroupGeneratorTestsWithJesusAsBitPartInScript : CharacterGroupGeneratorAndAdjusterTestBase
	{
		[TestFixtureSetUp]
		public void TextFixtureSetUp()
		{
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.ACT);
			TestProject.SimulateDisambiguationForAllBooks(m_testProject);
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			TestProject.DeleteTestProjectFolder();
		}

		[SetUp]
		public void SetUp()
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.CastSizeOption = CastSizeOption.NotSet;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = false;
		}

		[Test]
		public void GenerateCharacterGroups_FewActors_JesusInGroupByHimself()
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 1;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			SetVoiceActors(7);
			var gen = new CharacterGroupGenerator(m_testProject);
			var groups = gen.GenerateCharacterGroups();
			var jesusGroup = groups.Single(g => g.CharacterIds.Contains("Jesus"));
			Assert.AreEqual(1, jesusGroup.CharacterIds.Count);
		}

		[Test]
		public void GenerateCharacterGroups_BigEnoughCastToAvoidProximityProblems_FemaleRolesAssignedToFemaleGroups()
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 1;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			SetVoiceActors(14, 2);
			var gen = new CharacterGroupGenerator(m_testProject);
			var groups = gen.GenerateCharacterGroups();

			var extraGroup = groups.Single(g => g.CharacterIds.Contains("BC-ACT"));
			Assert.AreEqual(CharacterGroup.Label.Male, extraGroup.GroupIdLabel);

			VerifyProximityAndGenderConstraintsForAllGroups(groups);
		}
	}

	[TestFixture]
	public class CharacterGroupGeneratorTestsWithSingleVoiceBook : CharacterGroupGeneratorAndAdjusterTestBase
	{
		[TestFixtureSetUp]
		public void TextFixtureSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerse;
			CharacterDetailData.TabDelimitedCharacterDetailData = Resources.TestCharacterDetail;
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.LUK);
			m_testProject.IncludedBooks[0].SingleVoice = true;
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			TestProject.DeleteTestProjectFolder();
		}

		[Test]
		public void GenerateCharacterGroups_OneBookSingleVoice_AllLinesAreNarrator()
		{
			SetVoiceActors(7);
			var gen = new CharacterGroupGenerator(m_testProject);
			var groups = gen.GenerateCharacterGroups();
			var singleGroup = GetNarratorGroupForBook(groups, "LUK");
			Assert.AreEqual(1, singleGroup.CharacterIds.Count);
			Assert.AreEqual(1, groups.Count);
		}
	}

	[TestFixture]
	public class CharacterGroupGeneratorTestsWithBookThatHasNoBiblicalCharactersExceptScripture : CharacterGroupGeneratorAndAdjusterTestBase
	{
		[TestFixtureSetUp]
		public void TextFixtureSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerse;
			CharacterDetailData.TabDelimitedCharacterDetailData = Resources.TestCharacterDetail;
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.EPH);
			m_testProject.DramatizationPreferences.BookIntroductionsDramatization = ExtraBiblicalMaterialSpeakerOption.ActorOfEitherGender;
			m_testProject.DramatizationPreferences.SectionHeadDramatization = ExtraBiblicalMaterialSpeakerOption.ActorOfEitherGender;
			m_testProject.DramatizationPreferences.BookTitleAndChapterDramatization = ExtraBiblicalMaterialSpeakerOption.ActorOfEitherGender;
			TestProject.SimulateDisambiguationForAllBooks(m_testProject);
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			TestProject.DeleteTestProjectFolder();
		}

		[Test]
		public void GenerateCharacterGroups_CastWithMultipleMenWomenAndChild_DistinctScriptureExtraBiblicalAndNarratorGroups()
		{
			SetVoiceActors(7, 2, 1);
			var gen = new CharacterGroupGenerator(m_testProject);
			var groups = gen.GenerateCharacterGroups();
			Assert.AreEqual(3, groups.Count);
			var narGroup = GetNarratorGroupForBook(groups, "EPH");
			Assert.AreEqual(CharacterGroup.Label.Narrator, narGroup.GroupIdLabel);
			Assert.IsTrue(groups.Except(new [] { narGroup }).All(g => g.GroupIdLabel == CharacterGroup.Label.Male));
			var extraGroup = groups.Single(g => g.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("EPH",
				CharacterVerseData.StandardCharacter.BookOrChapter)));
			Assert.IsTrue(extraGroup.CharacterIds.SetEquals(new [] {
				CharacterVerseData.GetStandardCharacterId("EPH", CharacterVerseData.StandardCharacter.BookOrChapter),
				CharacterVerseData.GetStandardCharacterId("EPH", CharacterVerseData.StandardCharacter.ExtraBiblical)}
				));
			Assert.IsTrue(groups.Except(new[] { extraGroup }).All(g => g.CharacterIds.Count == 1));
		}
	}

	[TestFixture]
	public class CharacterGroupGeneratorTestsWithSingleVoiceBookAndNonSingleVoiceBook : CharacterGroupGeneratorAndAdjusterTestBase
	{
		[TestFixtureSetUp]
		public void TextFixtureSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerse;
			CharacterDetailData.TabDelimitedCharacterDetailData = Resources.TestCharacterDetail;
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.LUK, TestProject.TestBook.ACT);
			m_testProject.IncludedBooks[0].SingleVoice = true;

			m_testProject.DramatizationPreferences.BookTitleAndChapterDramatization = ExtraBiblicalMaterialSpeakerOption.ActorOfEitherGender;
			m_testProject.DramatizationPreferences.SectionHeadDramatization = ExtraBiblicalMaterialSpeakerOption.Omitted;
			m_testProject.DramatizationPreferences.BookIntroductionsDramatization = ExtraBiblicalMaterialSpeakerOption.Omitted;
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			TestProject.DeleteTestProjectFolder();
		}

		[SetUp]
		public void SetUp()
		{
			m_testProject.CharacterGroupGenerationPreferences.CastSizeOption = CastSizeOption.NotSet;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = false;
		}

		[TestCase(2, 0)]
		[TestCase(1, 1)]
		public void GenerateCharacterGroups_ExplicitlyRequestTwoNarrators_AllLinesForLukeAreNarratorAndLukeNarratorHandlesSomeMaleRolesInActs(int numMale, int numFemale)
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = numMale;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = numFemale;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;
			m_testProject.DramatizationPreferences.BookTitleAndChapterDramatization = ExtraBiblicalMaterialSpeakerOption.MaleActor;
			m_testProject.DramatizationPreferences.SectionHeadDramatization = ExtraBiblicalMaterialSpeakerOption.Omitted;

			SetVoiceActors(6, 2);
			var gen = new CharacterGroupGenerator(m_testProject);
			var groups = gen.GenerateCharacterGroups();
			var narratorLukeGroup = GetNarratorGroupForBook(groups, "LUK");
			var narratorActsGroup = GetNarratorGroupForBook(groups, "ACT");
			Assert.IsTrue(narratorLukeGroup.CharacterIds.Count > 1);
			Assert.AreEqual(1, narratorActsGroup.CharacterIds.Count);
			AssertThatThereAreTwoDistinctNarratorGroups(groups);
			Assert.False(groups.Any(g => g.CharacterIds.Contains("BC-LUK")));
			Assert.AreEqual(8, groups.Count);
		}

		[TestCase(ExtraBiblicalMaterialSpeakerOption.FemaleActor, ExtraBiblicalMaterialSpeakerOption.FemaleActor)]
		[TestCase(ExtraBiblicalMaterialSpeakerOption.ActorOfEitherGender, ExtraBiblicalMaterialSpeakerOption.ActorOfEitherGender)]
		public void GenerateCharacterGroups_ExplicitlyRequestTwoFemaleNarratorsWithTooSmallCast_LukeNarratorHandlesExtraBiblicalAndCharacterRolesInActs(
			ExtraBiblicalMaterialSpeakerOption bookTitleAndChapterOption,
			ExtraBiblicalMaterialSpeakerOption extraBiblicalOption)
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 2;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;
			m_testProject.DramatizationPreferences.BookTitleAndChapterDramatization = bookTitleAndChapterOption;
			m_testProject.DramatizationPreferences.SectionHeadDramatization = extraBiblicalOption;
			m_testProject.ClearCharacterStatistics();

			SetVoiceActors(6, 2);
			var gen = new CharacterGroupGenerator(m_testProject);
			var groups = gen.GenerateCharacterGroups();
			var narratorLukeGroup = GetNarratorGroupForBook(groups, "LUK");
			var narratorActsGroup = GetNarratorGroupForBook(groups, "ACT");
			Assert.IsTrue(narratorLukeGroup.CharacterIds.Count > 3);
			Assert.IsTrue(narratorLukeGroup.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("ACT", CharacterVerseData.StandardCharacter.BookOrChapter)));
			Assert.IsTrue(narratorLukeGroup.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("ACT", CharacterVerseData.StandardCharacter.ExtraBiblical)));
			Assert.AreEqual(1, narratorActsGroup.CharacterIds.Count);
			AssertThatThereAreTwoDistinctNarratorGroups(groups);
			Assert.False(groups.Any(g => g.CharacterIds.Contains("BC-LUK")));
			Assert.AreEqual(8, groups.Count);
		}

		[Test]
		public void GenerateCharacterGroups_ExplicitlyRequestTwoFemaleNarratorsAndMaleExtraWithTooSmallCast_ExtraBiblicalRoleHandledByMaleActorDespiteBadProximity()
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 2;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;
			m_testProject.DramatizationPreferences.BookTitleAndChapterDramatization = ExtraBiblicalMaterialSpeakerOption.MaleActor;
			m_testProject.DramatizationPreferences.SectionHeadDramatization = ExtraBiblicalMaterialSpeakerOption.MaleActor;

			SetVoiceActors(6, 2);
			var gen = new CharacterGroupGenerator(m_testProject);
			var groups = gen.GenerateCharacterGroups();
			var narratorLukeGroup = GetNarratorGroupForBook(groups, "LUK");
			var narratorActsGroup = GetNarratorGroupForBook(groups, "ACT");
			Assert.IsFalse(narratorLukeGroup.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("ACT", CharacterVerseData.StandardCharacter.BookOrChapter)));
			Assert.IsFalse(narratorLukeGroup.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("ACT", CharacterVerseData.StandardCharacter.ExtraBiblical)));
			Assert.AreEqual(1, narratorActsGroup.CharacterIds.Count);
			AssertThatThereAreTwoDistinctNarratorGroups(groups);
			Assert.False(groups.Any(g => g.CharacterIds.Contains("BC-LUK")));
			Assert.AreEqual(8, groups.Count);
		}

		[Test]
		public void GenerateCharacterGroups_ExplicitlyRequestTwoFemaleNarratorsWithMinimalCast_AllLinesForLukeAreNarratorAndLukeNarratorHandlesOneFemaleRoleInActs()
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 2;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			SetVoiceActors(8, 3);
			var gen = new CharacterGroupGenerator(m_testProject);
			var groups = gen.GenerateCharacterGroups();
			var narratorLukeGroup = GetNarratorGroupForBook(groups, "LUK");
			var narratorActsGroup = GetNarratorGroupForBook(groups, "ACT");
			Assert.IsTrue(narratorLukeGroup.CharacterIds.Count > 1);
			Assert.AreEqual(1, narratorLukeGroup.CharacterIds.Intersect(new[] { "Lydia", "fortune telling slave girl" }).Count(),
				"Lydia and the fortune-telling slave girl speak very close to each other, so the narrator of Luke should take on one of those character roles");
			Assert.AreEqual(1, narratorActsGroup.CharacterIds.Count);
			AssertThatThereAreTwoDistinctNarratorGroups(groups);
			Assert.False(groups.Any(g => g.CharacterIds.Contains("BC-LUK")));
			Assert.AreEqual(11, groups.Count);
		}

		[Test]
		public void GenerateCharacterGroups_ExplicitlyRequestTwoFemaleNarratorsWithJustBigEnoughCast_AchievesMinimumProximityByHavingLukeNarratorHandleOneFemaleCharacterRoleInActs()
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 2;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			SetVoiceActors(11, 3);
			var gen = new CharacterGroupGenerator(m_testProject);
			var groups = gen.GenerateCharacterGroups();
			var narratorLukeGroup = GetNarratorGroupForBook(groups, "LUK");
			var narratorActsGroup = GetNarratorGroupForBook(groups, "ACT");
			Assert.AreEqual(2, narratorLukeGroup.CharacterIds.Count);
			Assert.AreEqual(1, narratorLukeGroup.CharacterIds.Intersect(new[] { "Lydia", "fortune telling slave girl" }).Count(),
				"Lydia and the fortune-telling slave girl speak very close to each other, so the narrator of Luke should take on one of those character roles");
			Assert.AreEqual(1, narratorActsGroup.CharacterIds.Count);
			AssertThatThereAreTwoDistinctNarratorGroups(groups);
			VerifyProximityAndGenderConstraintsForAllGroups(groups, false, true);
			Assert.False(groups.Any(g => g.CharacterIds.Contains("BC-LUK")));
			Assert.AreEqual(14, groups.Count);
		}

		[TestCase(1, 0)]
		[TestCase(0, 1)]
		public void GenerateCharacterGroups_ExplicitlyRequestSingleNarrator_AllLinesForLukeAreNarratorAndOnlyOneNarratorGroup(int numMale, int numFemale)
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = numMale;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = numFemale;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			SetVoiceActors(5, 2);
			var gen = new CharacterGroupGenerator(m_testProject);
			var groups = gen.GenerateCharacterGroups();
			var narratorLukeGroup = GetNarratorGroupForBook(groups, "LUK");
			Assert.AreEqual(2, narratorLukeGroup.CharacterIds.Count);
			Assert.True(narratorLukeGroup.CharacterIds.Contains("narrator-ACT"));
			Assert.False(groups.Any(g => g.CharacterIds.Contains("BC-LUK")));
			Assert.AreEqual(7, groups.Count);
		}

		[Test]
		public void GenerateCharacterGroups_DefaultNarratorPreferences_AllLinesForSingleVoiceBookAreNarrator()
		{
			SetVoiceActors(5, 2);
			var gen = new CharacterGroupGenerator(m_testProject);
			var groups = gen.GenerateCharacterGroups();
			Assert.AreEqual(GetNarratorGroupForBook(groups, "LUK"), GetNarratorGroupForBook(groups, "ACT"));
			Assert.False(groups.Any(g => g.CharacterIds.Contains("BC-LUK")));
			Assert.AreEqual(7, groups.Count);
		}

		private void AssertThatThereAreTwoDistinctNarratorGroups(List<CharacterGroup> groups)
		{
			var narrator1Group = GetNarratorGroupForBook(groups, "LUK");
			var narrator2Group = GetNarratorGroupForBook(groups, "ACT");
			Assert.AreNotEqual(narrator1Group, narrator2Group);
		}
	}

	[TestFixture]
	public class CharacterGroupGeneratorTestsWithScriptModifiedDuringTest : CharacterGroupGeneratorAndAdjusterTestBase
	{
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
		}

		[TearDown]
		public void TearDown()
		{
			TestProject.DeleteTestProjectFolder();
		}

		[Test]
		public void GenerateCharacterGroups_CameoAssignedToCharacterWhichIsRemovedFromScript_RegenerationDropsCharacter()
		{
			SetVoiceActors(7);
			var cameoActor = m_testProject.VoiceActorList.AllActors[0];
			cameoActor.IsCameo = true;
			var gen = new CharacterGroupGenerator(m_testProject);
			var groups = gen.GenerateCharacterGroups();
			m_testProject.CharacterGroupList.CharacterGroups.AddRange(groups);

			var cameoGroup = m_testProject.CharacterGroupList.GetGroupsAssignedToActor(0).Single();

			var zachGroup = groups.Single(g => g.CharacterIds.Contains("Zaccheaus"));
			zachGroup.CharacterIds.Remove("Zaccheaus");
			cameoGroup.CharacterIds = new CharacterIdHashSet { "Zaccheaus" };

			zachGroup = groups.Single(g => g.CharacterIds.Contains("Zaccheaus"));
			Assert.AreEqual(1, zachGroup.CharacterIds.Count);

			m_testProject.AvailableBooks[0].IncludeInScript = false;
			m_testProject.ClearCharacterStatistics(); // This simulates behavior in UI when the project is saved after displaying ScriptureRangeSelectionDlg			
			gen = new CharacterGroupGenerator(m_testProject);

			groups = gen.GenerateCharacterGroups();

			cameoGroup = groups.Single(g => g.VoiceActorId == cameoActor.Id);
			Assert.AreEqual(7, groups.Count);
			Assert.AreEqual(0, groups.Count(g => g.CharacterIds.Contains("Zaccheaus")));
			Assert.AreEqual(0, cameoGroup.CharacterIds.Count);
		}

		[Test]
		public void GenerateCharacterGroups_ProjectCharacterDetailExistsAndInScript_ProjectCharacterDetailIncludedInGeneratedGroups()
		{
			SetVoiceActors(8, 2);
			m_testProject.AddProjectCharacterDetail(new CharacterDetail { CharacterId = "Bobette", Gender = CharacterGender.Female });

			m_testProject.IncludedBooks[0].Blocks[3].CharacterId = "Bobette";

			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();
			Assert.True(groups.Any(g => g.CharacterIds.Contains("Bobette")));
		}

		[Test]
		public void GenerateCharacterGroups_ProjectCharacterDetailExistsButNotInScript_ProjectCharacterDetailNotIncludedInGeneratedGroups()
		{
			SetVoiceActors(8, 2);
			m_testProject.AddProjectCharacterDetail(new CharacterDetail { CharacterId = "Bobette", Gender = CharacterGender.Female });

			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();
			Assert.False(groups.Any(g => g.CharacterIds.Contains("Bobette")));
		}
	}

	[TestFixture]
	public class CharacterGroupGeneratorTestsWithLotsOfBooks : CharacterGroupGeneratorAndAdjusterTestBase
	{
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

			m_testProject.DramatizationPreferences.BookTitleAndChapterDramatization = ExtraBiblicalMaterialSpeakerOption.ActorOfEitherGender;
			m_testProject.DramatizationPreferences.SectionHeadDramatization = ExtraBiblicalMaterialSpeakerOption.Omitted;
			m_testProject.DramatizationPreferences.BookIntroductionsDramatization = ExtraBiblicalMaterialSpeakerOption.Omitted;
		}

		[SetUp]
		public void SetUp()
		{
			m_testProject.VoiceActorList.AllActors.Clear();
			m_testProject.CharacterGroupList.CharacterGroups.Clear();
			m_testProject.CharacterGroupGenerationPreferences.NarratorsOption = NarratorsOption.NotSet;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.CastSizeOption = CastSizeOption.NotSet;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = false;
		}

		[Test]
		public void GenerateCharacterGroups_IsCancelable()
		{
			SetVoiceActors(10);
			m_testProject.CharacterGroupList.CharacterGroups.Clear();
			var group = new CharacterGroup(m_testProject);
			group.AssignVoiceActor(m_testProject.VoiceActorList.AllActors[0].Id);
			m_testProject.CharacterGroupList.CharacterGroups.Add(group);

			BackgroundWorker worker = new BackgroundWorker();
			worker.WorkerSupportsCancellation = true;
			CharacterGroupGenerator generator = new CharacterGroupGenerator(m_testProject, null, worker);
			worker.DoWork += (sender, args) =>
			{
				generator.GenerateCharacterGroups();

				Assert.Null(generator.GeneratedGroups);
				Assert.AreEqual(1, m_testProject.CharacterGroupList.CharacterGroups.Count);
				Assert.AreEqual(group, m_testProject.CharacterGroupList.CharacterGroups[0]);
			};
			worker.RunWorkerAsync();
			worker.CancelAsync();

			Assert.Null(generator.GeneratedGroups);
			Assert.AreEqual(1, m_testProject.CharacterGroupList.CharacterGroups.Count);
			Assert.AreEqual(group, m_testProject.CharacterGroupList.CharacterGroups[0]);
		}

		[Test]
		public void GenerateCharacterGroups_LargeCast_NarrationByAuthor_NarratorsGroupedByAuthorAndHaveDistinctNarrationRoles()
		{
			m_testProject.CharacterGroupGenerationPreferences.NarratorsOption = NarratorsOption.NarrationByAuthor;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 6;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;
			m_testProject.CharacterGroupGenerationPreferences.CastSizeOption = CastSizeOption.MatchVoiceActorList;

			SetVoiceActors(34, 4);
			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();

			Assert.AreEqual(38, groups.Count);
			var narMark = GetNarratorGroupForBook(groups, "MRK");
			var narLuke = GetNarratorGroupForBook(groups, "LUK");
			var narPaul = GetNarratorGroupForBook(groups, "EPH");
			var narHebrews = GetNarratorGroupForBook(groups, "HEB");
			var narJohn = GetNarratorGroupForBook(groups, "1JN");
			var narJude = GetNarratorGroupForBook(groups, "JUD");

			Assert.AreEqual(1, narMark.CharacterIds.Count);
			Assert.AreEqual(2, narLuke.CharacterIds.Count);
			Assert.IsTrue(narLuke.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("ACT", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.AreEqual(4, narPaul.CharacterIds.Count);
			Assert.IsTrue(narPaul.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("GAL", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(narPaul.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("PHM", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(narPaul.CharacterIds.Contains("Paul"));
			Assert.AreEqual(1, narHebrews.CharacterIds.Count);
			Assert.AreEqual(4, narJohn.CharacterIds.Count);
			Assert.IsTrue(narJohn.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("2JN", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(narJohn.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("3JN", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(narJohn.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("REV", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.AreEqual(1, narJude.CharacterIds.Count);
		}

		[Test]
		public void GenerateCharacterGroups_LargeCast_NumberOfNarratorsMatchAuthors_NarratorsGroupedByAuthorAndHaveDistinctNarrationRoles()
		{
			m_testProject.CharacterGroupGenerationPreferences.NarratorsOption = NarratorsOption.Custom;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 6;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			SetVoiceActors(34, 4);
			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();

			Assert.AreEqual(38, groups.Count);
			var narMark = GetNarratorGroupForBook(groups, "MRK");
			var narLuke = GetNarratorGroupForBook(groups, "LUK");
			var narPaul = GetNarratorGroupForBook(groups, "EPH");
			var narHebrews = GetNarratorGroupForBook(groups, "HEB");
			var narJohn = GetNarratorGroupForBook(groups, "1JN");
			var narJude = GetNarratorGroupForBook(groups, "JUD");

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

		[Test]
		public void GenerateCharacterGroups_SmallCast_NarrationByAuthor_NarratorsGroupedByAuthorAndHaveCharacterRolesInOtherBooks()
		{
			m_testProject.CharacterGroupGenerationPreferences.NarratorsOption = NarratorsOption.NarrationByAuthor;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 6;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			SetVoiceActors(10, 2);
			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();

			Assert.AreEqual(12, groups.Count);
			var narMark = GetNarratorGroupForBook(groups, "MRK");
			var narLuke = GetNarratorGroupForBook(groups, "LUK");
			var narPaul = GetNarratorGroupForBook(groups, "EPH");
			var narHebrews = GetNarratorGroupForBook(groups, "HEB");
			var narJohn = GetNarratorGroupForBook(groups, "1JN");
			var narJude = GetNarratorGroupForBook(groups, "JUD");

			Assert.IsTrue(narMark.CharacterIds.Count > 1);
			Assert.IsTrue(narLuke.CharacterIds.Count > 2);
			Assert.IsTrue(narLuke.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("ACT", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(narPaul.CharacterIds.Count >= 4);
			Assert.IsTrue(narPaul.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("GAL", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(narPaul.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("PHM", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(narPaul.CharacterIds.Contains("Paul"));
			Assert.IsTrue(narHebrews.CharacterIds.Count > 1);
			Assert.IsTrue(narJohn.CharacterIds.Count > 4);
			Assert.IsTrue(narJohn.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("2JN", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(narJohn.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("3JN", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(narJohn.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("REV", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(narJude.CharacterIds.Count > 1);
		}

		[Test]
		public void GenerateCharacterGroups_SmallCast_NumberOfNarratorsMatchAuthors_NarratorsGroupedByAuthorAndHaveCharacterRolesInOtherBooks()
		{
			m_testProject.CharacterGroupGenerationPreferences.NarratorsOption = NarratorsOption.Custom;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 6;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			SetVoiceActors(10, 2);
			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();

			Assert.AreEqual(12, groups.Count);
			var narMark = GetNarratorGroupForBook(groups, "MRK");
			var narLuke = GetNarratorGroupForBook(groups, "LUK");
			var narPaul = GetNarratorGroupForBook(groups, "EPH");
			var narHebrews = GetNarratorGroupForBook(groups, "HEB");
			var narJohn = GetNarratorGroupForBook(groups, "1JN");
			var narJude = GetNarratorGroupForBook(groups, "JUD");

			Assert.IsTrue(narMark.CharacterIds.Count > 1);
			Assert.IsTrue(narLuke.CharacterIds.Count > 2);
			Assert.IsTrue(narLuke.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("ACT", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(narPaul.CharacterIds.Count > 3);
			Assert.IsTrue(narPaul.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("GAL", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(narPaul.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("PHM", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(narHebrews.CharacterIds.Count > 1);
			Assert.IsTrue(narJohn.CharacterIds.Count > 4);
			Assert.IsTrue(narJohn.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("2JN", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(narJohn.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("3JN", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(narJohn.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("REV", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(narJude.CharacterIds.Count > 1);
		}

		[Test]
		public void GenerateCharacterGroups_NumberOfNarratorsOneFewerThanAuthors_HebrewsAndJudeShareNarrators()
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 5;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			SetVoiceActors(34, 4);
			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();

			Assert.AreEqual(38, groups.Count);
			var narMark = GetNarratorGroupForBook(groups, "MRK");
			var narLuke = GetNarratorGroupForBook(groups, "LUK");
			var narPaul = GetNarratorGroupForBook(groups, "EPH");
			var narHebrewsAndJude = GetNarratorGroupForBook(groups, "HEB");
			var narJohn = GetNarratorGroupForBook(groups, "1JN");

			Assert.AreEqual(1, narMark.CharacterIds.Count);
			Assert.AreEqual(2, narLuke.CharacterIds.Count);
			Assert.IsTrue(narLuke.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("ACT", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.AreEqual(3, narPaul.CharacterIds.Count);
			Assert.IsTrue(narPaul.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("GAL", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(narPaul.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("PHM", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.AreEqual(2, narHebrewsAndJude.CharacterIds.Count);
			Assert.IsTrue(narHebrewsAndJude.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.AreEqual(4, narJohn.CharacterIds.Count);
			Assert.IsTrue(narJohn.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("2JN", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(narJohn.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("3JN", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(narJohn.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("REV", CharacterVerseData.StandardCharacter.Narrator)));
		}

		[Test]
		public void GenerateCharacterGroups_NumberOfNarratorsTwoFewerThanAuthors_PaulCombinesWithJudeAndHebrewsCombinesWithMark()
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 4;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			SetVoiceActors(34, 4);
			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();

			Assert.AreEqual(38, groups.Count);
			var narMarkAndHebrews = GetNarratorGroupForBook(groups, "MRK");
			var narLuke = GetNarratorGroupForBook(groups, "LUK");
			var narPaulAndJude = GetNarratorGroupForBook(groups, "EPH");
			var narJohn = GetNarratorGroupForBook(groups, "1JN");

			Assert.AreEqual(2, narMarkAndHebrews.CharacterIds.Count);
			Assert.IsTrue(narMarkAndHebrews.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("HEB", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.AreEqual(2, narLuke.CharacterIds.Count);
			Assert.IsTrue(narLuke.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("ACT", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.AreEqual(4, narPaulAndJude.CharacterIds.Count);
			Assert.IsTrue(narPaulAndJude.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("GAL", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(narPaulAndJude.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("PHM", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(narPaulAndJude.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.AreEqual(4, narJohn.CharacterIds.Count);
			Assert.IsTrue(narJohn.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("2JN", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(narJohn.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("3JN", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(narJohn.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("REV", CharacterVerseData.StandardCharacter.Narrator)));
		}

		[Test]
		public void GenerateCharacterGroups_NumberOfNarratorsThreeFewerThanAuthors_JohnCombinesWithJudeAndHebrewsAndPaulCombinesWithMark()
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 3;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			SetVoiceActors(34, 4);
			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();

			Assert.AreEqual(38, groups.Count);
			var narLuke = GetNarratorGroupForBook(groups, "LUK");
			var narPaulAndMark = GetNarratorGroupForBook(groups, "EPH");
			var narJohnJudeHebrews = GetNarratorGroupForBook(groups, "1JN");

			Assert.AreEqual(2, narLuke.CharacterIds.Count);
			Assert.IsTrue(narLuke.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("ACT", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.AreEqual(4, narPaulAndMark.CharacterIds.Count);
			Assert.IsTrue(narPaulAndMark.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(narPaulAndMark.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("GAL", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(narPaulAndMark.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("PHM", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.AreEqual(6, narJohnJudeHebrews.CharacterIds.Count);
			Assert.IsTrue(narJohnJudeHebrews.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("HEB", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(narJohnJudeHebrews.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("2JN", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(narJohnJudeHebrews.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("3JN", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(narJohnJudeHebrews.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(narJohnJudeHebrews.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("REV", CharacterVerseData.StandardCharacter.Narrator)));
		}

		[Test]
		public void GenerateCharacterGroups_NumberOfNarratorsFourFewerThanAuthors_LukeCombinesWithJudeAndHebrewsAndJohnCombinesWithPaulAndMark()
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 2;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			SetVoiceActors(34, 4);
			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();

			Assert.AreEqual(38, groups.Count);
			var narLukeJudeAndHebrews = GetNarratorGroupForBook(groups, "LUK");
			var narJohnPaulAndMark = GetNarratorGroupForBook(groups, "1JN");

			Assert.AreEqual(4, narLukeJudeAndHebrews.CharacterIds.Count);
			Assert.IsTrue(narLukeJudeAndHebrews.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("ACT", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(narLukeJudeAndHebrews.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("HEB", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(narLukeJudeAndHebrews.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.AreEqual(8, narJohnPaulAndMark.CharacterIds.Count);
			Assert.IsTrue(narJohnPaulAndMark.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(narJohnPaulAndMark.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("GAL", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(narJohnPaulAndMark.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("EPH", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(narJohnPaulAndMark.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("PHM", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(narJohnPaulAndMark.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("2JN", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(narJohnPaulAndMark.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("3JN", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(narJohnPaulAndMark.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("REV", CharacterVerseData.StandardCharacter.Narrator)));
		}

		[Test]
		public void GenerateCharacterGroups_NumberOfNarratorsTwoFewerThanBooks_JohanineEpistlesGetCombined()
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 8;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 2;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			SetVoiceActors(36, 6);
			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();

			Assert.AreEqual(42, groups.Count);
			var narMrk = GetNarratorGroupForBook(groups, "MRK");
			var narLuk = GetNarratorGroupForBook(groups, "LUK");
			var narAct = GetNarratorGroupForBook(groups, "ACT");
			var narGal = GetNarratorGroupForBook(groups, "GAL");
			var narEph = GetNarratorGroupForBook(groups, "EPH");
			var narPhm = GetNarratorGroupForBook(groups, "PHM");
			var narHeb = GetNarratorGroupForBook(groups, "HEB");
			var narJohanineEpistles = GetNarratorGroupForBook(groups, "1JN");
			var narRev = GetNarratorGroupForBook(groups, "REV");
			var narJud = GetNarratorGroupForBook(groups, "JUD");

			Assert.AreEqual(1, narMrk.CharacterIds.Count);
			Assert.AreEqual(1, narLuk.CharacterIds.Count);
			Assert.AreEqual(1, narAct.CharacterIds.Count);
			Assert.AreEqual(1, narGal.CharacterIds.Count);
			Assert.AreEqual(1, narEph.CharacterIds.Count);
			Assert.AreEqual(1, narPhm.CharacterIds.Count);
			Assert.AreEqual(1, narHeb.CharacterIds.Count);
			Assert.AreEqual(3, narJohanineEpistles.CharacterIds.Count);
			Assert.IsTrue(narJohanineEpistles.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("2JN", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(narJohanineEpistles.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("3JN", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.AreEqual(1, narJud.CharacterIds.Count);
			Assert.AreEqual(1, narRev.CharacterIds.Count);

			var narrators = new List<CharacterGroup>
			{
				narMrk,
				narLuk,
				narAct,
				narGal,
				narEph,
				narPhm,
				narHeb,
				narJohanineEpistles,
				narJud,
				narRev
			};
			Assert.AreEqual(4, groups.Except(narrators).Count(n => n.ContainsCharacterWithGender(CharacterGender.Female)));
		}

		[Test]
		public void GenerateCharacterGroups_CameoActorsAssignedToNarratorRole_NumberOfGeneratedNarratorGroupsReduced()
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 8;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 2;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			SetVoiceActors(36, 6);
			var cameoMaleNarrator = m_testProject.VoiceActorList.AllActors.First(a => a.Gender == ActorGender.Male);
			var cameoFemaleNarrator = m_testProject.VoiceActorList.AllActors.First(a => a.Gender == ActorGender.Female);
			cameoMaleNarrator.IsCameo = true;
			cameoFemaleNarrator.IsCameo = true;
			m_testProject.CharacterGroupList.CharacterGroups.AddRange(new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups());
			m_testProject.CharacterGroupList.CharacterGroups.Single(g => g.VoiceActorId == cameoMaleNarrator.Id).CharacterIds.Add(CharacterVerseData.GetStandardCharacterId("2JN", CharacterVerseData.StandardCharacter.Narrator));
			m_testProject.CharacterGroupList.CharacterGroups.Single(g => g.VoiceActorId == cameoFemaleNarrator.Id).CharacterIds.Add(CharacterVerseData.GetStandardCharacterId("HEB", CharacterVerseData.StandardCharacter.Narrator));
			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();

			Assert.AreEqual(42, groups.Count);
			var narMrk = GetNarratorGroupForBook(groups, "MRK");
			var narLuk = GetNarratorGroupForBook(groups, "LUK");
			var narAct = GetNarratorGroupForBook(groups, "ACT");
			var narGal = GetNarratorGroupForBook(groups, "GAL");
			var narEphAndPhm = GetNarratorGroupForBook(groups, "EPH");
			var narHeb = GetNarratorGroupForBook(groups, "HEB");
			var nar1JnAnd3Jn = GetNarratorGroupForBook(groups, "1JN");
			var nar2Jn = GetNarratorGroupForBook(groups, "2JN");
			var narRev = GetNarratorGroupForBook(groups, "REV");
			var narJud = GetNarratorGroupForBook(groups, "JUD");

			Assert.AreEqual(1, narMrk.CharacterIds.Count);
			Assert.AreEqual(1, narLuk.CharacterIds.Count);
			Assert.AreEqual(1, narAct.CharacterIds.Count);
			Assert.AreEqual(1, narGal.CharacterIds.Count);
			Assert.AreEqual(2, narEphAndPhm.CharacterIds.Count);
			Assert.IsTrue(narEphAndPhm.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("PHM", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.AreEqual(1, narHeb.CharacterIds.Count);
			Assert.IsTrue(narHeb.AssignedToCameoActor);
			Assert.AreEqual(2, nar1JnAnd3Jn.CharacterIds.Count);
			Assert.IsTrue(nar1JnAnd3Jn.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("3JN", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.AreEqual(1, nar2Jn.CharacterIds.Count);
			Assert.IsTrue(nar2Jn.AssignedToCameoActor);
			Assert.AreEqual(1, narJud.CharacterIds.Count);
			Assert.AreEqual(1, narRev.CharacterIds.Count);

			var narrators = new List<CharacterGroup>
			{
				narMrk,
				narLuk,
				narAct,
				narGal,
				narEphAndPhm,
				narHeb,
				nar1JnAnd3Jn,
				nar2Jn,
				narJud,
				narRev
			};
			Assert.AreEqual(4, groups.Except(narrators).Count(n => n.ContainsCharacterWithGender(CharacterGender.Female)));
		}

		[Test]
		public void GenerateCharacterGroups_AllNarratorRolesAssignedToCameoActors_NoAdditionalNarratorGroupsReserved()
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 4;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			SetVoiceActors(36, 6);
			var cameoMaleNarrator1 = m_testProject.VoiceActorList.AllActors.First(a => a.Gender == ActorGender.Male);
			cameoMaleNarrator1.IsCameo = true;
			var cameoMaleNarrator2 = m_testProject.VoiceActorList.AllActors.First(a => a.Gender == ActorGender.Male && !a.IsCameo);
			cameoMaleNarrator2.IsCameo = true;
			m_testProject.CharacterGroupList.CharacterGroups.AddRange(new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups());
			m_testProject.CharacterGroupList.CharacterGroups.Single(g => g.VoiceActorId == cameoMaleNarrator1.Id).CharacterIds.AddRange(new []
			{
				CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.Narrator),
				CharacterVerseData.GetStandardCharacterId("LUK", CharacterVerseData.StandardCharacter.Narrator),
				CharacterVerseData.GetStandardCharacterId("ACT", CharacterVerseData.StandardCharacter.Narrator),
				CharacterVerseData.GetStandardCharacterId("GAL", CharacterVerseData.StandardCharacter.Narrator),
				CharacterVerseData.GetStandardCharacterId("EPH", CharacterVerseData.StandardCharacter.Narrator),
				CharacterVerseData.GetStandardCharacterId("PHM", CharacterVerseData.StandardCharacter.Narrator)
			});
			m_testProject.CharacterGroupList.CharacterGroups.Single(g => g.VoiceActorId == cameoMaleNarrator2.Id).CharacterIds.AddRange(new[]
			{
				CharacterVerseData.GetStandardCharacterId("HEB", CharacterVerseData.StandardCharacter.Narrator),
				CharacterVerseData.GetStandardCharacterId("1JN", CharacterVerseData.StandardCharacter.Narrator),
				CharacterVerseData.GetStandardCharacterId("2JN", CharacterVerseData.StandardCharacter.Narrator),
				CharacterVerseData.GetStandardCharacterId("3JN", CharacterVerseData.StandardCharacter.Narrator),
				CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.Narrator),
				CharacterVerseData.GetStandardCharacterId("REV", CharacterVerseData.StandardCharacter.Narrator)
			});

			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();

			Assert.AreEqual(42, groups.Count);
			var narMrk = GetNarratorGroupForBook(groups, "MRK");
			var narHeb = GetNarratorGroupForBook(groups, "HEB");
			
			Assert.AreEqual(6, narMrk.CharacterIds.Count);
			Assert.IsTrue(narMrk.AssignedToCameoActor);
			Assert.AreEqual(6, narHeb.CharacterIds.Count);
			Assert.IsTrue(narHeb.AssignedToCameoActor);

			var narrators = new List<CharacterGroup> { narMrk, narHeb };
			foreach (var grp in groups.Except(narrators))
				Assert.IsFalse(grp.CharacterIds.Any(c => CharacterVerseData.IsCharacterOfType(c, CharacterVerseData.StandardCharacter.Narrator)));
		}

		[Test]
		public void GenerateCharacterGroups_CameoActorsWithNarratorRolesEqualsNumberOfRequestedNarratorsButNotAllRolesAreAssigned_OneAdditionalNarratorGroupReserved()
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 3;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			SetVoiceActors(36, 5);
			var cameoMaleNarrator1 = m_testProject.VoiceActorList.AllActors.First(a => a.Gender == ActorGender.Male);
			cameoMaleNarrator1.IsCameo = true;
			var cameoMaleNarrator2 = m_testProject.VoiceActorList.AllActors.First(a => a.Gender == ActorGender.Male && !a.IsCameo);
			cameoMaleNarrator2.IsCameo = true;
			var cameoMaleNarrator3 = m_testProject.VoiceActorList.AllActors.First(a => a.Gender == ActorGender.Male && !a.IsCameo);
			cameoMaleNarrator3.IsCameo = true;
			m_testProject.CharacterGroupList.CharacterGroups.AddRange(new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups());
			m_testProject.CharacterGroupList.CharacterGroups.Single(g => g.VoiceActorId == cameoMaleNarrator1.Id).CharacterIds.AddRange(new[]
			{
				CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.Narrator),
			});
			m_testProject.CharacterGroupList.CharacterGroups.Single(g => g.VoiceActorId == cameoMaleNarrator2.Id).CharacterIds.AddRange(new[]
			{
				CharacterVerseData.GetStandardCharacterId("GAL", CharacterVerseData.StandardCharacter.Narrator),
				CharacterVerseData.GetStandardCharacterId("EPH", CharacterVerseData.StandardCharacter.Narrator),
				CharacterVerseData.GetStandardCharacterId("PHM", CharacterVerseData.StandardCharacter.Narrator)
			});
			m_testProject.CharacterGroupList.CharacterGroups.Single(g => g.VoiceActorId == cameoMaleNarrator3.Id).CharacterIds.AddRange(new[]
			{
				CharacterVerseData.GetStandardCharacterId("HEB", CharacterVerseData.StandardCharacter.Narrator),
			});

			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();

			Assert.AreEqual(41, groups.Count);
			var narMrk = GetNarratorGroupForBook(groups, "MRK");
			var narGal = GetNarratorGroupForBook(groups, "GAL");
			var narHeb = GetNarratorGroupForBook(groups, "HEB");
			var narNonCameo = GetNarratorGroupForBook(groups, "LUK");

			Assert.AreEqual(1, narMrk.CharacterIds.Count);
			Assert.IsTrue(narMrk.AssignedToCameoActor);
			Assert.AreEqual(3, narGal.CharacterIds.Count);
			Assert.IsTrue(narGal.AssignedToCameoActor);
			Assert.IsTrue(narGal.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("EPH", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(narGal.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("PHM", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.AreEqual(1, narHeb.CharacterIds.Count);
			Assert.IsTrue(narHeb.AssignedToCameoActor);
			Assert.IsFalse(narNonCameo.AssignedToCameoActor);
			Assert.AreEqual(7, narNonCameo.CharacterIds.Count(c => CharacterVerseData.IsCharacterOfType(c, CharacterVerseData.StandardCharacter.Narrator)));

			var narrators = new List<CharacterGroup> { narMrk, narGal, narHeb, narNonCameo };
			foreach (var grp in groups.Except(narrators))
				Assert.IsFalse(grp.CharacterIds.Any(c => CharacterVerseData.IsCharacterOfType(c, CharacterVerseData.StandardCharacter.Narrator)));
		}

		[TestCase(CastSizeOption.Small)]
		[TestCase(CastSizeOption.Recommended)]
		[TestCase(CastSizeOption.Large)]
		public void GenerateCharacterGroups_GhostCastUsed_CorrectNumberOfEachTypeOfActorCreated(CastSizeOption castSize)
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 1;
			m_testProject.CharacterGroupGenerationPreferences.CastSizeOption = castSize;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			var castSizePlanningViewModel = new CastSizePlanningViewModel(m_testProject);
			var castSizeValues = castSizePlanningViewModel.GetCastSizeRowValues(castSize);
			var gen = new CharacterGroupGenerator(m_testProject, castSizeValues);
			var groups = gen.GenerateCharacterGroups();
			Assert.AreEqual(1, groups.Count(g => g.GroupIdLabel == CharacterGroup.Label.Narrator));
			Assert.AreEqual(0, groups.Count(g => g.GroupIdLabel == CharacterGroup.Label.None));
			Assert.AreEqual(0, groups.Count(g => g.GroupIdLabel == CharacterGroup.Label.Other));
			Assert.AreEqual(castSizeValues.Child, groups.Count(g => g.GroupIdLabel == CharacterGroup.Label.Child));
			Assert.AreEqual(castSizeValues.Female - m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators, groups.Count(g => g.GroupIdLabel == CharacterGroup.Label.Female));
			Assert.AreEqual(castSizeValues.Male - m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators, groups.Count(g => g.GroupIdLabel == CharacterGroup.Label.Male));
		}

		[TestCase(20, 4, 2)]
		[TestCase(25, 2, 0)]
		[TestCase(30, 0, 0)]
		public void GenerateCharacterGroups_CustomGhostCastUsed_CorrectNumberOfEachTypeOfActorCreated(int maleActors, int femaleActors, int childActors)
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 1;
			m_testProject.CharacterGroupGenerationPreferences.CastSizeOption = CastSizeOption.Custom;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			var castSizeValues = new CastSizeRowValues(maleActors, femaleActors, childActors);
			var gen = new CharacterGroupGenerator(m_testProject, castSizeValues);
			var groups = gen.GenerateCharacterGroups();
			var charactersInScript = TestProject.GetIncludedCharacterDetails(m_testProject);
			var numChildCharactersInScript = charactersInScript.Count(c => c.Age == CharacterAge.Child);
			Assert.AreEqual(1, groups.Count(g => g.GroupIdLabel == CharacterGroup.Label.Narrator));
			Assert.AreEqual(0, groups.Count(g => g.GroupIdLabel == CharacterGroup.Label.None));
			Assert.AreEqual(0, groups.Count(g => g.GroupIdLabel == CharacterGroup.Label.Other));
			Assert.AreEqual(Math.Min(castSizeValues.Child, numChildCharactersInScript), groups.Count(g => g.GroupIdLabel == CharacterGroup.Label.Child));
			Assert.AreEqual(castSizeValues.Female - m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators, groups.Count(g => g.GroupIdLabel == CharacterGroup.Label.Female));
			Assert.AreEqual(castSizeValues.Male - m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators, groups.Count(g => g.GroupIdLabel == CharacterGroup.Label.Male));
		}

		[Test]
		public void GenerateCharacterGroups_CastSoSmallThatOnlyOneMaleActorIsNotANarratorOrExtra_DietyNotGroupedWithOtherCharacters()
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 6;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			SetVoiceActors(m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators + 2, 1);
			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();

			Assert.AreEqual(9, groups.Count);
			var dietyGroup = groups.Single(g => g.CharacterIds.Contains("Jesus"));
			Assert.IsTrue(dietyGroup.CharacterIds.SetEquals(new []
				{
					"Jesus", "God", "Holy Spirit, the", "scripture"
				}));
		}
	}

	[TestFixture]
	class CharacterGroupGeneratorTestsWith1JnAnd2JnAndEph : CharacterGroupGeneratorAndAdjusterTestBase
	{
		[TestFixtureSetUp]
		public void TextFixtureSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerseOct2015;
			CharacterDetailData.TabDelimitedCharacterDetailData = Resources.TestCharacterDetailOct2015;
			RelatedCharactersData.Source = Resources.TestRelatedCharacters;
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.IJN, TestProject.TestBook.IIJN, TestProject.TestBook.EPH);
			TestProject.SimulateDisambiguationForAllBooks(m_testProject);

			m_testProject.DramatizationPreferences.BookIntroductionsDramatization = ExtraBiblicalMaterialSpeakerOption.Narrator;
			m_testProject.DramatizationPreferences.SectionHeadDramatization = ExtraBiblicalMaterialSpeakerOption.Narrator;
			m_testProject.DramatizationPreferences.BookTitleAndChapterDramatization = ExtraBiblicalMaterialSpeakerOption.Narrator;
		}

		[SetUp]
		public void SetUp()
		{
			m_testProject.VoiceActorList.AllActors.Clear();
			m_testProject.CharacterGroupList.CharacterGroups.Clear();
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 2;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			TestProject.DeleteTestProjectFolder();
		}

		[Test]
		public void GenerateGroups_AllActorsAreNarrators_NarratorsDoNotCauseProximityClash()
		{
			SetVoiceActors(2);
			var generator = new CharacterGroupGenerator(m_testProject);
			var groups = generator.GenerateCharacterGroups();

			Assert.AreEqual(2, groups.Count);
			Assert.True(!CharacterVerseData.IsCharacterOfType(generator.MinimumProximity.FirstCharacterId, CharacterVerseData.StandardCharacter.Narrator) ||
				!CharacterVerseData.IsCharacterOfType(generator.MinimumProximity.SecondCharacterId, CharacterVerseData.StandardCharacter.Narrator));
			Assert.True(generator.MinimumProximity.NumberOfBlocks > Proximity.kDefaultMinimumProximity);
		}
	}

	[TestFixture]
	public class CharacterGroupGeneratorTestsWithExtrabiblicalCharacterOptions : CharacterGroupGeneratorAndAdjusterTestBase
	{
		[TestFixtureSetUp]
		public void TextFixtureSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerseOct2015;
			CharacterDetailData.TabDelimitedCharacterDetailData = Resources.TestCharacterDetailOct2015;
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.RUT);

			m_testProject.DramatizationPreferences.BookIntroductionsDramatization = ExtraBiblicalMaterialSpeakerOption.ActorOfEitherGender;
			m_testProject.DramatizationPreferences.SectionHeadDramatization = ExtraBiblicalMaterialSpeakerOption.ActorOfEitherGender;
			m_testProject.DramatizationPreferences.BookTitleAndChapterDramatization = ExtraBiblicalMaterialSpeakerOption.ActorOfEitherGender;
		}

		[SetUp]
		public void SetUp()
		{
			m_testProject.VoiceActorList.AllActors.Clear();
			m_testProject.CharacterGroupList.CharacterGroups.Clear();
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.CastSizeOption = CastSizeOption.NotSet;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = false;
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			TestProject.DeleteTestProjectFolder();
		}

		[Test]
		public void GenerateGroups_NarratorSpeaksAllExtra_AllExtraAssignedToNarrator()
		{
			SetVoiceActors(8, 2, 2);

			m_testProject.DramatizationPreferences.BookIntroductionsDramatization = ExtraBiblicalMaterialSpeakerOption.Narrator;
			m_testProject.DramatizationPreferences.SectionHeadDramatization = ExtraBiblicalMaterialSpeakerOption.Narrator;
			m_testProject.DramatizationPreferences.BookTitleAndChapterDramatization = ExtraBiblicalMaterialSpeakerOption.Narrator;

			var generator = new CharacterGroupGenerator(m_testProject);
			var groups = generator.GenerateCharacterGroups();

			var characterIds = groups[0].CharacterIds;
			Assert.True(characterIds.Contains("narrator-RUT"));
			Assert.True(characterIds.Contains("intro-RUT"));
			Assert.True(characterIds.Contains("extra-RUT"));
			Assert.True(characterIds.Contains("BC-RUT"));
		}
	}

	public abstract class CharacterGroupGeneratorAndAdjusterTestBase
	{
		protected Project m_testProject;
		protected void SetVoiceActors(int numberOfAdultMales, int numberOfAdultFemales = 0, int numberOfMaleChildren = 0, int numberOfFemaleChildren = 0)
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
			m_testProject.VoiceActorList.AllActors = actorList;
		}

		protected CharacterGroup GetNarratorGroupForBook(List<CharacterGroup> groups, string bookId)
		{
			return groups.Single(g => g.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId(bookId,
				CharacterVerseData.StandardCharacter.Narrator)));
		}

		protected void VerifyProximityAndGenderConstraintsForAllGroups(List<CharacterGroup> groups,
			bool allowMaleNarratorsToDoBiblicalCharacterRoles = false, bool allowFemaleNarratorsToDoBiblicalCharacterRoles = false)
		{
			var p = new Proximity(m_testProject.IncludedBooks, m_testProject.DramatizationPreferences);
			foreach (var group in groups.Where(g => !CharacterGroupGenerator.ContainsDeityCharacter(g)))
			{
				var minProximity = p.CalculateMinimumProximity(group.CharacterIds);
				Debug.WriteLine(group.GroupIdForUiDisplay + ": " + minProximity);
				Assert.True(minProximity.NumberOfBlocks >= Proximity.kDefaultMinimumProximity);
			}

			foreach (var group in groups.Where(g => g.ContainsCharacterWithGender(CharacterGender.Female)))
				Assert.IsTrue(CharacterGroup.Label.Female == group.GroupIdLabel ||
					(allowFemaleNarratorsToDoBiblicalCharacterRoles && CharacterGroup.Label.Narrator == group.GroupIdLabel));

			foreach (var group in groups.Where(g => g.ContainsCharacterWithGender(CharacterGender.Male)))
				Assert.IsTrue(CharacterGroup.Label.Male == group.GroupIdLabel ||
					(allowMaleNarratorsToDoBiblicalCharacterRoles && CharacterGroup.Label.Narrator == group.GroupIdLabel));
		}
	}
}
