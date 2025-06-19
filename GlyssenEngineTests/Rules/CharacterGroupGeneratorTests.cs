using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using GlyssenCharacters;
using GlyssenEngine;
using GlyssenEngine.Bundle;
using GlyssenEngine.Casting;
using GlyssenEngine.Character;
using GlyssenEngine.Rules;
using GlyssenEngine.ViewModels;
using NUnit.Framework;
using SIL.Extensions;
using static System.String;
using static GlyssenCharacters.CharacterSpeakingMode;
using static GlyssenCharacters.CharacterVerseData;
using static GlyssenCharacters.CharacterVerseData.StandardCharacter;
using CharacterIdHashSet = GlyssenEngine.Character.CharacterIdHashSet;
using Resources = GlyssenCharactersTests.Properties.Resources;
using static GlyssenSharedTests.CustomConstraints;

namespace GlyssenEngineTests.Rules
{
	[TestFixture]
	class CharacterGroupGeneratorTestsWithTwoBooksWithNoChildrenInScript : CharacterGroupGeneratorAndAdjusterTestBase
	{
		[OneTimeSetUp]
		public void OneTimeSetUp()
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

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			TestProject.DeleteTestProjects();
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
			Assert.That(jesusGroup.CharacterIds.Count, Is.EqualTo(1));
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
			Assert.That(narratorGroup.CharacterIds.Count, Is.EqualTo(2));
			Assert.That(narratorGroup.CharacterIds, Does.Contain("narrator-JUD"));
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
			Assert.That(extraBiblicalGroup.CharacterIds, Does.Contain("extra-JUD"));
			Assert.That(extraBiblicalGroup.CharacterIds, Does.Contain("BC-MRK"));
			Assert.That(extraBiblicalGroup.CharacterIds, Does.Contain("BC-JUD"));
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
			Assert.That(new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups().Count, Is.EqualTo(0));

			if (numberOfMaleNarrators + numberOfFemaleNarrators == 1)
			{
				//1
				Debug.WriteLine("=======================================");
				Debug.WriteLine("************** 1 Actor ***************");
				Debug.WriteLine("=======================================");
				SetVoiceActors(1 - numberOfFemaleNarrators, numberOfFemaleNarrators);
				Assert.That(new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups().Count, Is.EqualTo(1));
			}

			// TODO: Need Analyst to decide what we want if there are only two actors.
			//2
			Debug.WriteLine("=======================================");
			Debug.WriteLine("************** 2 Actors ***************");
			Debug.WriteLine("=======================================");
			SetVoiceActors(2 - numberOfFemaleNarrators, numberOfFemaleNarrators);
			Assert.That(new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups().Count, Is.EqualTo(2));

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
				Assert.That(groups.Count, Is.EqualTo(3));
				if (numberOfMaleNarrators == 2)
					AssertThatThereAreTwoDistinctNarratorGroups(groups);
			}

			//3
			Debug.WriteLine("=======================================");
			Debug.WriteLine("******* 3 Actors ********");
			Debug.WriteLine("=======================================");
			SetVoiceActors(3 - numberOfFemaleNarrators, numberOfFemaleNarrators);
			groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();
			Assert.That(groups.Count, Is.EqualTo(3));
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
				Assert.That(groups.Count, Is.EqualTo(4));
				if (numberOfMaleNarrators + numberOfFemaleNarrators == 2)
					AssertThatThereAreTwoDistinctNarratorGroups(groups);
			}

			//4 (2 Males & 2 Females)
			Debug.WriteLine("=======================================");
			Debug.WriteLine("******* 2,2 Actors ********");
			Debug.WriteLine("=======================================");
			SetVoiceActors(2, 2);
			groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();
			Assert.That(groups.Count, Is.EqualTo(4));
			if (numberOfMaleNarrators + numberOfFemaleNarrators == 2)
				AssertThatThereAreTwoDistinctNarratorGroups(groups);

			//10
			Debug.WriteLine("=======================================");
			Debug.WriteLine("******* 10 Actors ********");
			Debug.WriteLine("=======================================");
			SetVoiceActors(10 - numberOfFemaleNarrators, numberOfFemaleNarrators);
			groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();
			Assert.That(groups.Count, Is.EqualTo(10));
			if (numberOfMaleNarrators + numberOfFemaleNarrators == 2)
				AssertThatThereAreTwoDistinctNarratorGroups(groups);

			//20
			Debug.WriteLine("=======================================");
			Debug.WriteLine("******* 20 Actors ********");
			Debug.WriteLine("=======================================");
			SetVoiceActors(20 - numberOfFemaleNarrators, numberOfFemaleNarrators);
			groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();
			Assert.That(groups.Count, Is.EqualTo(20));
			if (numberOfMaleNarrators + numberOfFemaleNarrators == 2)
				AssertThatThereAreTwoDistinctNarratorGroups(groups);

			//25
			Debug.WriteLine("=======================================");
			Debug.WriteLine("******* 18,7 Actors ********");
			Debug.WriteLine("=======================================");
			SetVoiceActors(18, 7);
			groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();
			Assert.That(groups.Count, Is.EqualTo(25));
			if (numberOfMaleNarrators + numberOfFemaleNarrators == 2)
				AssertThatThereAreTwoDistinctNarratorGroups(groups);

			SetVoiceActors(25 - numberOfFemaleNarrators, numberOfFemaleNarrators);
			Debug.WriteLine("=======================================");
			Debug.WriteLine("******* 25 Actors ********");
			Debug.WriteLine("=======================================");
			groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();
			Assert.That(groups.Count, Is.EqualTo(25));
			if (numberOfMaleNarrators + numberOfFemaleNarrators == 2)
				AssertThatThereAreTwoDistinctNarratorGroups(groups);

			//50
			Debug.WriteLine("=======================================");
			Debug.WriteLine("******* 50 Actors ********");
			Debug.WriteLine("=======================================");
			SetVoiceActors(50 - numberOfFemaleNarrators, numberOfFemaleNarrators);
			groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();
			Assert.That(groups.Count, Is.EqualTo(50));
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
			Assert.That(numberOfMaleActors + numberOfFemaleActors, Is.EqualTo(groups.Count));
			if (numberOfMaleNarrators + numberOfFemaleNarrators == 2)
				AssertThatThereAreTwoDistinctNarratorGroups(groups);

			//Max + 1 Male
			Debug.WriteLine("=======================================");
			Debug.WriteLine("******* Max + 1 Male Actors ********");
			Debug.WriteLine("=======================================");
			SetVoiceActors(numberOfMaleActors + 1, numberOfFemaleActors);
			groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();
			Assert.That(numberOfMaleActors + numberOfFemaleActors, Is.EqualTo(groups.Count));
			if (numberOfMaleNarrators + numberOfFemaleNarrators == 2)
				AssertThatThereAreTwoDistinctNarratorGroups(groups);

			//Max + 1 Female
			Debug.WriteLine("=======================================");
			Debug.WriteLine("******* Max + 1 Female Actors ********");
			Debug.WriteLine("=======================================");
			SetVoiceActors(numberOfMaleActors, numberOfFemaleActors + 1);
			groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();
			Assert.That(numberOfMaleActors + numberOfFemaleActors, Is.EqualTo(groups.Count));
			if (numberOfMaleNarrators + numberOfFemaleNarrators == 2)
				AssertThatThereAreTwoDistinctNarratorGroups(groups);
		}

		[TestCase(8, 2, 3, 0)]
		[TestCase(10, 5, 1, 0)]
		[TestCase(15, 5, 1, 0)]
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

			Assert.That(numberOfMaleActors + numberOfFemaleActors, Is.EqualTo(groups.Count));
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

			Assert.That(groups.Count, Is.EqualTo(11));
			var maleGroups = groups.Where(g => g.ContainsCharacterWithGender(CharacterGender.Male)).ToList();
			var femaleGroups = groups.Where(g => g.ContainsCharacterWithGender(CharacterGender.Female)).ToList();
			Assert.That(maleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.Female)), Is.False);
			Assert.That(maleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.PreferFemale)), Is.False);
			Assert.That(femaleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.Male)), Is.False);
			Assert.That(femaleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.PreferMale)), Is.False);
			Assert.That(groups.Single(g => g.CharacterIds.Contains("Jesus")).CharacterIds.Count, Is.EqualTo(1));
			Assert.That(GetNarratorGroupForBook(groups, "MRK").CharacterIds.All(
				i => IsCharacterOfType(i, Narrator)), Is.True);
			Assert.That(groups.Single(g => g.CharacterIds.Contains("BC-MRK")).CharacterIds.All(
				IsCharacterExtraBiblical), Is.True);
			Assert.That(groups.Count(g => g.CharacterIds.All(c =>
			{
				if (IsCharacterStandard(c))
					return true;
				var gender = CharacterDetailData.Singleton.GetDictionary()[c].Gender;
				return gender != CharacterGender.Male && gender != CharacterGender.PreferMale;
			})), Is.GreaterThanOrEqualTo(2));

			Assert.That(maleGroups.Count <= groups.Count - numberOfFemaleNarrators, Is.True);
			Assert.That(femaleGroups.Count <= groups.Count - numberOfMaleNarrators, Is.True);
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

			Assert.That(groups.Count, Is.EqualTo(12));
			var maleGroups = groups.Where(g => g.ContainsCharacterWithGender(CharacterGender.Male)).ToList();
			var femaleGroups = groups.Where(g => g.ContainsCharacterWithGender(CharacterGender.Female)).ToList();
			Assert.That(maleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.Female)), Is.False);
			Assert.That(maleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.PreferFemale)), Is.False);
			Assert.That(femaleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.Male)), Is.False);
			Assert.That(femaleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.PreferMale)), Is.False);
			Assert.That(groups.Single(g => g.CharacterIds.Contains("Jesus")).CharacterIds.Count, Is.EqualTo(1));
			AssertThatThereAreTwoDistinctNarratorGroups(groups);

			Assert.That(groups.Single(g => g.CharacterIds.Contains("BC-MRK")).CharacterIds.All(IsCharacterExtraBiblical), Is.True);

			Assert.That(groups.Count(g => g.CharacterIds.All(c =>
			{
				if (IsCharacterStandard(c))
					return true;
				var gender = CharacterDetailData.Singleton.GetDictionary()[c].Gender;
				return gender != CharacterGender.Male && gender != CharacterGender.PreferMale;
			})), Is.GreaterThanOrEqualTo(2));

			Assert.That(maleGroups.Count <= groups.Count - numberOfFemaleNarrators, Is.True);
			Assert.That(femaleGroups.Count <= groups.Count - numberOfMaleNarrators, Is.True);
		}

		[Test]
		public void GenerateCharacterGroups_DifferentGendersAndAgesOfTenActors_AppropriateGroupsCreatedForActorsWhichHaveCorrespondingCharacters()
		{
			SetVoiceActors(7, 1, 1, 1);
			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();

			Assert.That(groups.Any(g => g.ContainsCharacterWithAge(CharacterAge.Child)), Is.False, "No kids speak in Mark or Jude");
			var maleAdultGroups = groups.Where(g => g.ContainsCharacterWithGender(CharacterGender.Male)).ToList();
			var femaleAdultGroup = groups.Single(g => g.ContainsCharacterWithGender(CharacterGender.Female));
			Assert.That(maleAdultGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.Female)), Is.False);
			Assert.That(maleAdultGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.PreferFemale)), Is.False);
			Assert.That(femaleAdultGroup.ContainsCharacterWithGender(CharacterGender.Male), Is.False);
			Assert.That(femaleAdultGroup.ContainsCharacterWithGender(CharacterGender.PreferMale), Is.False);
			Assert.That(groups.Count, Is.EqualTo(8));
			Assert.That(groups.Count(g => g.CharacterIds.All(c =>
			{
				if (IsCharacterStandard(c))
					return true;
				var gender = CharacterDetailData.Singleton.GetDictionary()[c].Gender;
				return gender != CharacterGender.Female && gender != CharacterGender.PreferFemale;
			})), Is.GreaterThanOrEqualTo(7));
			Assert.That(groups.Count(g => g.CharacterIds.All(c =>
			{
				if (IsCharacterStandard(c))
					return true;
				var gender = CharacterDetailData.Singleton.GetDictionary()[c].Gender;
				return gender != CharacterGender.Male && gender != CharacterGender.PreferMale;
			})), Is.GreaterThanOrEqualTo(1));
		}

		[Test]
		public void GenerateCharacterGroups_DifferentGendersAndAgesOfTwentyActors_AppropriateGroupsCreatedForActorsWhichHaveCorrespondingCharacters()
		{
			SetVoiceActors(18, 1, 1);
			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();

			Assert.That(groups.Count, Is.EqualTo(19));
			Assert.That(groups.Any(g => g.ContainsCharacterWithAge(CharacterAge.Child)), Is.False, "No kids speak in Mark or Jude");
			var maleAdultGroups = groups.Where(g => g.ContainsCharacterWithGender(CharacterGender.Male)).ToList();
			var femaleAdultGroup = groups.Single(g => g.ContainsCharacterWithGender(CharacterGender.Female));
			Assert.That(maleAdultGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.Female)), Is.False);
			Assert.That(maleAdultGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.PreferFemale)), Is.False);
			Assert.That(femaleAdultGroup.ContainsCharacterWithGender(CharacterGender.Male), Is.False);
			Assert.That(femaleAdultGroup.ContainsCharacterWithGender(CharacterGender.PreferMale), Is.False);
			Assert.That(m_testProject.VoiceActorList.AllActors[18].Id, Is.EqualTo(femaleAdultGroup.VoiceActorId));
			Assert.That(groups.Count(g => g.CharacterIds.All(c =>
			{
				if (IsCharacterStandard(c))
					return true;
				var gender = CharacterDetailData.Singleton.GetDictionary()[c].Gender;
				return gender != CharacterGender.Female && gender != CharacterGender.PreferFemale;
			})), Is.GreaterThanOrEqualTo(18));
			Assert.That(groups.Count(g => g.CharacterIds.All(c =>
			{
				if (IsCharacterStandard(c))
					return true;
				var gender = CharacterDetailData.Singleton.GetDictionary()[c].Gender;
				return gender != CharacterGender.Male && gender != CharacterGender.PreferMale;
			})), Is.GreaterThanOrEqualTo(1));
		}

		[Test]
		public void GenerateCharacterGroups_IncludesOneCameoActor_GeneratesEmptyGroupAssignedToCameoActor()
		{
			SetVoiceActors(10);
			m_testProject.VoiceActorList.AllActors[0].IsCameo = true;

			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();
			Assert.That(groups.Count, Is.EqualTo(10));
			Assert.That(groups.Count(g => g.IsVoiceActorAssigned), Is.EqualTo(1));
			var groupWithActorAssigned = groups.Single(g => g.IsVoiceActorAssigned);
			Assert.That(m_testProject.VoiceActorList.AllActors[0].Id, Is.EqualTo(groupWithActorAssigned.VoiceActorId));
			Assert.That(groupWithActorAssigned.CharacterIds.Count, Is.EqualTo(0));
		}

		[Test]
		public void GenerateCharacterGroups_IncludesTwoCameoActors_GeneratesEmptyGroupAssignedToEachCameoActor()
		{
			SetVoiceActors(10);
			m_testProject.VoiceActorList.AllActors[0].IsCameo = true;
			m_testProject.VoiceActorList.AllActors[1].IsCameo = true;

			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();
			Assert.That(groups.Count, Is.EqualTo(10));
			Assert.That(groups.Count(g => g.IsVoiceActorAssigned), Is.EqualTo(2));
			var groupsWithActorAssigned = groups.Where(g => g.IsVoiceActorAssigned).ToList();
			Assert.That(groupsWithActorAssigned.Select(g => g.VoiceActorId),
				Does.Contain(m_testProject.VoiceActorList.AllActors[0].Id));
			Assert.That(groupsWithActorAssigned.Select(g => g.VoiceActorId),
				Does.Contain(m_testProject.VoiceActorList.AllActors[1].Id));
			Assert.That(groupsWithActorAssigned,
				ForEvery<CharacterGroup>(g => g.CharacterIds.Count, Is.EqualTo(0)));
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
			Assert.That(groupWithActorAssigned.CharacterIds.Count, Is.EqualTo(1));
			Assert.That(groupWithActorAssigned.CharacterIds, Does.Contain("centurion at crucifixion"));
			Assert.That(groups.Where(g => g != groupWithActorAssigned).SelectMany(g => g.CharacterIds), Does.Not.Contain("centurion at crucifixion"));
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
			Assert.That(groupWithActorAssigned.CharacterIds.Count, Is.EqualTo(2));
			Assert.That(groupWithActorAssigned.CharacterIds, Does.Contain("centurion at crucifixion"));
			Assert.That(generator.MinimumProximity.IsAcceptable(), Is.True);
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
			Assert.That(groupWithActorAssigned.CharacterIds, Is.Empty);
			Assert.That(groups.Where(g => g != groupWithActorAssigned).SelectMany(g => g.CharacterIds),
				Does.Not.Contain("Bob the Builder"));
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
			Assert.That(groupWithActorAssigned.CharacterIds.Count, Is.EqualTo(1));
			Assert.That(groupWithActorAssigned.CharacterIds, Does.Contain("Jesus"));
			Assert.That(groups.Where(g => g != groupWithActorAssigned).SelectMany(g => g.CharacterIds),
				Does.Not.Contain("Jesus"));
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
			Assert.That(groupWithActorAssigned.CharacterIds.Count, Is.EqualTo(1));
			Assert.That(groupWithActorAssigned.CharacterIds, Does.Contain("BC-MRK"));
			Assert.That(groups.Where(g => g != groupWithActorAssigned).SelectMany(g => g.CharacterIds),
				Does.Not.Contain("BC-MRK"));
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
			Assert.That(groupWithActorAssigned.CharacterIds.Count, Is.EqualTo(1));
			Assert.That(groupWithActorAssigned.CharacterIds, Does.Contain("extra-MRK"));
			Assert.That(groups.Where(g => g != groupWithActorAssigned).SelectMany(g => g.CharacterIds), Does.Not.Contain("extra-MRK"));
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
			Assert.That(m_testProject.CharacterGroupList.CharacterGroups.Count, Is.EqualTo(5));
			Assert.That(m_testProject.VoiceActorList.AllActors[0].Id, Is.EqualTo(m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus").VoiceActorId));
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
			Assert.That(m_testProject.CharacterGroupList.CharacterGroups.Count, Is.EqualTo(5));
			Assert.That(m_testProject.VoiceActorList.AllActors[0].Id, Is.EqualTo(m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus").VoiceActorId));
			Assert.That(m_testProject.CharacterGroupList.GroupContainingCharacterId("John").IsVoiceActorAssigned, Is.False);
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
			Assert.That(m_testProject.KeyStrokesByCharacterId["BC-MRK"] < m_testProject.KeyStrokesByCharacterId["extra-MRK"], Is.True,
				"For this test to make sense as written, there have to be more key strokes associated with \"extra\" than with BC.");

			// expect BC-MRK and extra-MRK to be placed in same group
			var bcGroup = m_testProject.CharacterGroupList.GroupContainingCharacterId("BC-MRK");
			Assert.That(bcGroup.CharacterIds, Does.Contain("extra-MRK"));

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
			Assert.That(m_testProject.CharacterGroupList.CharacterGroups.Count, Is.EqualTo(5));

			// expect BC-MRK and extra-MRK to be placed back in same group
			var extraBiblicalGroup = m_testProject.CharacterGroupList.GroupContainingCharacterId("extra-MRK");
			bcGroup = m_testProject.CharacterGroupList.GroupContainingCharacterId("BC-MRK");
			Assert.That(extraBiblicalGroup, Is.EqualTo(bcGroup));

			// expect the voice actor for extra-MRK has not changed
			Assert.That(m_testProject.VoiceActorList.AllActors[1].Id, Is.EqualTo(extraBiblicalGroup.VoiceActorId));
			Assert.That(m_testProject.CharacterGroupList.HasVoiceActorAssigned(m_testProject.VoiceActorList.AllActors[0].Id), Is.False);
		}

		[Test]
		public void GenerateCharacterGroups_HasCameoAssignedButAttemptToMaintainAssignmentsIsFalse_MaintainsCameoGroup()
		{
			SetVoiceActors(3);
			var generator = new CharacterGroupGenerator(m_testProject);
			generator.GenerateCharacterGroups();
			generator.ApplyGeneratedGroupsToProject(false);

			var actor4 = new VoiceActor { Id = 4, IsCameo = true };
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
			Assert.That(groups.Count, Is.EqualTo(4));

			cameoGroup = groups.First(g => g.VoiceActorId == actor4.Id);
			Assert.That(actor4.Id, Is.EqualTo(cameoGroup.VoiceActorId));
			Assert.That(cameoGroup.CharacterIds.Count, Is.EqualTo(1));
			Assert.That(cameoGroup.CharacterIds, Does.Contain("John"));
			Assert.That(groups.Where(g => g != cameoGroup).SelectMany(g => g.CharacterIds), Does.Not.Contain("John"));
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
			Assert.That(GetNarratorGroupForBook(groups, "JUD").ContainsCharacterWithGender(CharacterGender.Female), Is.True);
		}

		[TestCase(7)]
		[TestCase(11)]
		public void GenerateCharacterGroups_NotEnoughActressesForMinimumProximityAndNarratorPreferences_NarratorGroupForJudeIncludesSomeFemaleCharactersInMark(int maleActors)
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 2;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			SetVoiceActors(maleActors, 3);

			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();
			Assert.That(m_testProject.VoiceActorList.AllActors.Count, Is.EqualTo(groups.Count), "One group expected per actor.");
			AssertThatThereAreTwoDistinctNarratorGroups(groups);
			Assert.That(GetNarratorGroupForBook(groups, "JUD").ContainsCharacterWithGender(CharacterGender.Female), Is.True);
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
			Assert.That(groups.Count, Is.EqualTo(5));
			AssertThatThereAreTwoDistinctNarratorGroups(groups);
			Assert.That(GetNarratorGroupForBook(groups, "JUD").CharacterIds.Count, Is.GreaterThan(10)); // All the male characters in MRK, except for the Trinity
			Assert.That(GetNarratorGroupForBook(groups, "MRK").CharacterIds.Intersect(new[]
			{
				"angel, arch-, Michael",
				"Enoch",
				"apostles"
			}), Is.Not.Empty);
		}

		[Test]
		public void GenerateCharacterGroups_NotEnoughActorsForMinimumProximityAndNarratorPreferences_FallbackToOneNarrator()
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 2;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			SetVoiceActors(4, 2);

			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();
			Assert.That(groups.Count, Is.EqualTo(6));
			AssertThatThereAreTwoDistinctNarratorGroups(groups);
			Assert.That(GetNarratorGroupForBook(groups, "JUD").CharacterIds.Count, Is.GreaterThan(1)); // Some of the male characters in MRK
			Assert.That(GetNarratorGroupForBook(groups, "MRK").CharacterIds.Count, Is.GreaterThan(1));
			Assert.That(GetNarratorGroupForBook(groups, "MRK").CharacterIds.Intersect(new List<string>(new[]
			{
				"angel, arch-, Michael",
				"Enoch",
				"apostles"
			})).Any(), Is.True);
		}

		private void AssertThatThereAreTwoDistinctNarratorGroups(List<CharacterGroup> groups)
		{
			var narratorMrkGroup = GetNarratorGroupForBook(groups, "MRK");
			var narratorJudGroup = GetNarratorGroupForBook(groups, "JUD");
			Assert.That(narratorMrkGroup, Is.Not.EqualTo(narratorJudGroup));
		}
	}

	[TestFixture]
	public class CharacterGroupGeneratorTestsWithBookHavingSameCharacterWithTwoAges : CharacterGroupGeneratorAndAdjusterTestBase
	{
		[OneTimeSetUp]
		public void OneTimeSetUp()
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

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			TestProject.DeleteTestProjects();
		}


		[Test]
		public void GenerateCharacterGroups_SameCharacterWithTwoAges_CharactersAreGeneratedInTheSameGroup()
		{
			SetVoiceActors(10, 5);

			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();
			var joshuaGroup = groups.Single(g => g.CharacterIds.Contains("Joshua"));
			var joshuaOldGroup = groups.Single(g => g.CharacterIds.Contains("Joshua (old)"));

			Assert.That(joshuaGroup, Is.EqualTo(joshuaOldGroup));
		}
	}

	[TestFixture]
	public class CharacterGroupGeneratorTestsWithChildrenInScript : CharacterGroupGeneratorAndAdjusterTestBase
	{
		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerse;
			CharacterDetailData.TabDelimitedCharacterDetailData = Resources.TestCharacterDetail;
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.LUK);

			m_testProject.DramatizationPreferences.BookTitleAndChapterDramatization = ExtraBiblicalMaterialSpeakerOption.ActorOfEitherGender;
			m_testProject.DramatizationPreferences.SectionHeadDramatization = ExtraBiblicalMaterialSpeakerOption.ActorOfEitherGender;
			m_testProject.DramatizationPreferences.BookIntroductionsDramatization = ExtraBiblicalMaterialSpeakerOption.ActorOfEitherGender;
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			TestProject.DeleteTestProjects();
		}

		[Test]
		public void GenerateCharacterGroups_20ActorsOfDifferentGendersAndAges_AppropriateGroupsCreatedForActors()
		{
			SetVoiceActors(17, 2, 1);
			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();

			Assert.That(groups.Count, Is.EqualTo(20));
			var maleGroups = groups.Where(g => g.ContainsCharacterWithGender(CharacterGender.Male)).ToList();
			var femaleGroups = groups.Where(g => g.ContainsCharacterWithGender(CharacterGender.Female)).ToList();

			Assert.That(maleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.Female)), Is.False);
			Assert.That(maleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.PreferFemale)), Is.False);
			Assert.That(femaleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.Male)), Is.False);
			Assert.That(femaleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.PreferMale)), Is.False);
			Assert.That(groups.Single(g => g.CharacterIds.Contains("Jesus")).CharacterIds.Count, Is.EqualTo(1));

			var narratorGroup = GetNarratorGroupForBook(groups, "LUK");
			Assert.That(narratorGroup.CharacterIds.All(
				i => IsCharacterOfType(i, Narrator)), Is.True);
			Assert.That(groups.Single(g => g.CharacterIds.Contains("BC-LUK")).CharacterIds.All(
				IsCharacterExtraBiblical), Is.True);
			Assert.That(groups.Count(g => g.CharacterIds.All(c =>
			{
				if (IsCharacterStandard(c))
					return true;
				var gender = CharacterDetailData.Singleton.GetDictionary()[c].Gender;
				return gender != CharacterGender.Male && gender != CharacterGender.PreferMale;
			})), Is.GreaterThanOrEqualTo(2));

			// One male child actor and one male child character -- make assignment automatically
			var maleChildGroup = groups.Single(g => g.ContainsCharacterWithGender(CharacterGender.Male) && g.ContainsCharacterWithAge(CharacterAge.Child));
			Assert.That(m_testProject.VoiceActorList.AllActors[19].Id, Is.EqualTo(maleChildGroup.VoiceActorId));
			Assert.That(maleChildGroup.CharacterIds.Count, Is.EqualTo(1));
		}

		[Test]
		public void GenerateCharacterGroups_UnneededActors_AppropriateGroupsCreatedForActors()
		{
			SetVoiceActors(13, 2, 3, 3);
			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();

			Assert.That(groups.Count, Is.EqualTo(16));
			var maleGroups = groups.Where(g => g.ContainsCharacterWithGender(CharacterGender.Male)).ToList();
			var femaleGroups = groups.Where(g => g.ContainsCharacterWithGender(CharacterGender.Female)).ToList();

			Assert.That(maleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.Female)), Is.False);
			Assert.That(maleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.PreferFemale)), Is.False);
			Assert.That(femaleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.Male)), Is.False);
			Assert.That(femaleGroups.Any(g => g.ContainsCharacterWithGender(CharacterGender.PreferMale)), Is.False);
			Assert.That(groups.Single(g => g.CharacterIds.Contains("Jesus")).CharacterIds.Count, Is.EqualTo(1));
			Assert.That(GetNarratorGroupForBook(groups, "LUK").CharacterIds.All(
				i => IsCharacterOfType(i, Narrator)), Is.True);
			Assert.That(groups.Single(g => g.CharacterIds.Contains("BC-LUK")).CharacterIds.All(IsCharacterExtraBiblical), Is.True);
			Assert.That(groups.Count(g => g.CharacterIds.All(c =>
			{
				if (IsCharacterStandard(c))
					return true;
				var gender = CharacterDetailData.Singleton.GetDictionary()[c].Gender;
				return gender != CharacterGender.Male && gender != CharacterGender.PreferMale;
			})), Is.GreaterThanOrEqualTo(2));

			// Three male child actors and one male child character -- do not make assignment automatically
			var maleChildGroup = groups.Single(g => g.ContainsCharacterWithGender(CharacterGender.Male) && g.ContainsCharacterWithAge(CharacterAge.Child));
			Assert.That(maleChildGroup.IsVoiceActorAssigned, Is.False);
			Assert.That(maleChildGroup.CharacterIds.Count, Is.EqualTo(1));
		}
	}

	[TestFixture]
	class CharacterGroupGeneratorTestsWithHolySpiritInScript : CharacterGroupGeneratorAndAdjusterTestBase
	{
		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerse;
			CharacterDetailData.TabDelimitedCharacterDetailData = Resources.TestCharacterDetail;
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.MRK, TestProject.TestBook.ACT);

			m_testProject.DramatizationPreferences.BookTitleAndChapterDramatization = ExtraBiblicalMaterialSpeakerOption.ActorOfEitherGender;
			m_testProject.DramatizationPreferences.SectionHeadDramatization = ExtraBiblicalMaterialSpeakerOption.Omitted;
			m_testProject.DramatizationPreferences.BookIntroductionsDramatization = ExtraBiblicalMaterialSpeakerOption.Omitted;
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			TestProject.DeleteTestProjects();
		}

		[Test]
		public void GenerateCharacterGroups_21Actors_GodAndJesusAndHolySpiritAndScriptureEachInOwnGroup()
		{
			SetVoiceActors(21);
			var gen = new CharacterGroupGenerator(m_testProject);
			var groups = gen.GenerateCharacterGroups();
			var jesusGroup = groups.Single(g => g.CharacterIds.Contains("Jesus"));
			var scriptureGroup = groups.Single(g => g.CharacterIds.Contains(kScriptureCharacter));
			var godGroup = groups.Single(g => g.CharacterIds.Contains("God"));
			var hsGroup = groups.Single(g => g.CharacterIds.Contains("Holy Spirit, the"));
			Assert.That(jesusGroup.CharacterIds.Count, Is.EqualTo(1));
			Assert.That(scriptureGroup.CharacterIds.Count, Is.EqualTo(1));
			Assert.That(godGroup.CharacterIds.Count, Is.EqualTo(1));
			Assert.That(hsGroup.CharacterIds.Count, Is.EqualTo(1));
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
			Assert.That(scriptureAndGodGroup.CharacterIds, Does.Contain(kScriptureCharacter));
			Assert.That(jesusGroup.CharacterIds.Count, Is.EqualTo(1));
			Assert.That(holySpiritGroup.CharacterIds.Count, Is.EqualTo(1));
			Assert.That(scriptureAndGodGroup.CharacterIds.Count, Is.EqualTo(2));
		}

		[Test]
		public void GenerateCharacterGroups_AtLeast7Actors_GodAndHolySpiritAndScriptureInGroupByThemselves()
		{
			SetVoiceActors(8);
			var gen = new CharacterGroupGenerator(m_testProject);
			var groups = gen.GenerateCharacterGroups();
			var jesusGroup = groups.Single(g => g.CharacterIds.Contains("Jesus"));
			var deityGroup = groups.Single(g => g.CharacterIds.Contains("God"));
			Assert.That(deityGroup.CharacterIds, Does.Contain("Holy Spirit, the"));
			Assert.That(deityGroup.CharacterIds, Does.Contain(kScriptureCharacter));
			Assert.That(jesusGroup.CharacterIds.Count, Is.EqualTo(1));
			Assert.That(deityGroup.CharacterIds.Count, Is.EqualTo(3));
		}

		[Test]
		public void GenerateCharacterGroups_AtLeast4Actors_GodAndHolySpiritAndScriptureGroupedWithJesus()
		{
			SetVoiceActors(6);
			var gen = new CharacterGroupGenerator(m_testProject);
			var groups = gen.GenerateCharacterGroups();
			var deityGroup = groups.Single(g => g.CharacterIds.Contains("Jesus"));
			Assert.That(deityGroup.CharacterIds, Does.Contain("God"));
			Assert.That(deityGroup.CharacterIds, Does.Contain("Holy Spirit, the"));
			Assert.That(deityGroup.CharacterIds, Does.Contain(kScriptureCharacter));
			Assert.That(deityGroup.CharacterIds.Count, Is.EqualTo(4));
		}

		[Test]
		public void GenerateCharacterGroups_FewerThan4Actors_GodAndHolySpiritAndScriptureAndJesusNotInIsolatedGroups()
		{
			SetVoiceActors(3);
			var gen = new CharacterGroupGenerator(m_testProject);
			var groups = gen.GenerateCharacterGroups();
			var jesusGroup = groups.Single(g => g.CharacterIds.Contains("Jesus"));
			Assert.That(jesusGroup.CharacterIds.Count, Is.GreaterThan(1));
			var godGroup = groups.Single(g => g.CharacterIds.Contains("God"));
			Assert.That(godGroup.CharacterIds.Count, Is.GreaterThan(1));
			var hsGroup = groups.Single(g => g.CharacterIds.Contains("Holy Spirit, the"));
			Assert.That(hsGroup.CharacterIds.Count, Is.GreaterThan(1));
			var scriptureGroup = groups.Single(g => g.CharacterIds.Contains(kScriptureCharacter));
			Assert.That(scriptureGroup.CharacterIds.Count, Is.GreaterThan(1));
		}
	}

	[TestFixture]
	internal class CharacterGroupGeneratorTestsWithJesusAsBitPartInScript : CharacterGroupGeneratorAndAdjusterTestBase
	{
		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.ACT);
			TestProject.SimulateDisambiguationForAllBooks(m_testProject);
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			TestProject.DeleteTestProjects();
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
			Assert.That(jesusGroup.CharacterIds.Count, Is.EqualTo(1));
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
			Assert.That(extraGroup.GroupIdLabel, Is.EqualTo(CharacterGroup.Label.Male));

			VerifyProximityAndGenderConstraintsForAllGroups(groups);
		}
	}

	[TestFixture]
	public class CharacterGroupGeneratorTestsWithSingleVoiceBook : CharacterGroupGeneratorAndAdjusterTestBase
	{
		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerse;
			CharacterDetailData.TabDelimitedCharacterDetailData = Resources.TestCharacterDetail;
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.LUK);
			m_testProject.IncludedBooks[0].SingleVoice = true;
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			TestProject.DeleteTestProjects();
		}

		[Test]
		public void GenerateCharacterGroups_OneBookSingleVoice_AllLinesAreNarrator()
		{
			SetVoiceActors(7);
			var gen = new CharacterGroupGenerator(m_testProject);
			var groups = gen.GenerateCharacterGroups();
			var singleGroup = GetNarratorGroupForBook(groups, "LUK");
			Assert.That(singleGroup.CharacterIds.Count, Is.EqualTo(1));
			Assert.That(groups.Count, Is.EqualTo(1));
		}
	}

	[TestFixture]
	public class CharacterGroupGeneratorTestsWithBookThatHasNoBiblicalCharactersExceptScripture : CharacterGroupGeneratorAndAdjusterTestBase
	{
		[OneTimeSetUp]
		public void OneTimeSetUp()
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

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			TestProject.DeleteTestProjects();
		}

		[Test]
		public void GenerateCharacterGroups_CastWithMultipleMenWomenAndChild_DistinctScriptureExtraBiblicalAndNarratorGroups()
		{
			SetVoiceActors(7, 2, 1);
			var gen = new CharacterGroupGenerator(m_testProject);
			var groups = gen.GenerateCharacterGroups();
			Assert.That(groups.Count, Is.EqualTo(3));
			var narGroup = GetNarratorGroupForBook(groups, "EPH");
			Assert.That(narGroup.GroupIdLabel, Is.EqualTo(CharacterGroup.Label.Narrator));
			Assert.That(groups.Except(new [] { narGroup }),
				ForEvery<CharacterGroup>(g => g.GroupIdLabel, Is.EqualTo(CharacterGroup.Label.Male)));
			var extraGroup = groups.Single(g => g.CharacterIds.Contains(GetStandardCharacterId("EPH",
				BookOrChapter)));
			Assert.That(extraGroup.CharacterIds, Is.EquivalentTo(new [] {
				GetStandardCharacterId("EPH", BookOrChapter),
				GetStandardCharacterId("EPH", ExtraBiblical)}
				));
			Assert.That(groups.Except(new[] { extraGroup }),
				ForEvery<CharacterGroup>(g => g.CharacterIds.Count, Is.EqualTo(1)));
		}
	}

	[TestFixture]
	public class CharacterGroupGeneratorTestsWithSingleVoiceBookAndNonSingleVoiceBook : CharacterGroupGeneratorAndAdjusterTestBase
	{
		[OneTimeSetUp]
		public void OneTimeSetUp()
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

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			TestProject.DeleteTestProjects();
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
			Assert.That(narratorLukeGroup.CharacterIds.Count, Is.GreaterThan(1));
			Assert.That(narratorActsGroup.CharacterIds.Count, Is.EqualTo(1));
			AssertThatThereAreTwoDistinctNarratorGroups(groups);
			Assert.That(groups, No<CharacterGroup>(g => g.CharacterIds, Does.Contain("BC-LUK")));
			Assert.That(groups.Count, Is.EqualTo(8));
		}

		[Test]
		public void GenerateCharacterGroups_ExplicitlyRequestTwoFemaleNarratorsWithTooSmallCast_LukeNarratorHandlesExtraBiblicalAndCharacterRolesInActs()
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 2;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;
			m_testProject.DramatizationPreferences.BookTitleAndChapterDramatization = ExtraBiblicalMaterialSpeakerOption.FemaleActor;
			m_testProject.DramatizationPreferences.SectionHeadDramatization = ExtraBiblicalMaterialSpeakerOption.FemaleActor;
			m_testProject.ClearCharacterStatistics();

			SetVoiceActors(6, 2);
			var gen = new CharacterGroupGenerator(m_testProject);
			var groups = gen.GenerateCharacterGroups();
			var narratorLukeGroup = GetNarratorGroupForBook(groups, "LUK");
			var narratorActsGroup = GetNarratorGroupForBook(groups, "ACT");
			Assert.That(narratorLukeGroup.CharacterIds.Count, Is.GreaterThan(3));
			Assert.That(narratorLukeGroup.CharacterIds,
				Does.Contain(GetStandardCharacterId("ACT", BookOrChapter)));
			Assert.That(narratorLukeGroup.CharacterIds,
				Does.Contain(GetStandardCharacterId("ACT", ExtraBiblical)));
			Assert.That(narratorActsGroup.CharacterIds.Count, Is.EqualTo(1));
			AssertThatThereAreTwoDistinctNarratorGroups(groups);
			Assert.That(groups, No<CharacterGroup>(g => g.CharacterIds, Does.Contain("BC-LUK")));
			Assert.That(groups.Count, Is.EqualTo(8));
		}

		[Test]
		public void GenerateCharacterGroups_ExplicitlyRequestTwoFemaleNarratorsWithTooSmallCast_LukeNarratorHandlesFemaleCharacterRolesInActsAndMaleGroupHandlesExtraBiblical()
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 2;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;
			m_testProject.DramatizationPreferences.BookTitleAndChapterDramatization = ExtraBiblicalMaterialSpeakerOption.ActorOfEitherGender;
			m_testProject.DramatizationPreferences.SectionHeadDramatization = ExtraBiblicalMaterialSpeakerOption.ActorOfEitherGender;
			m_testProject.ClearCharacterStatistics();

			SetVoiceActors(6, 2);
			var gen = new CharacterGroupGenerator(m_testProject);
			var groups = gen.GenerateCharacterGroups();
			var narratorLukeGroup = GetNarratorGroupForBook(groups, "LUK");
			var narratorActsGroup = GetNarratorGroupForBook(groups, "ACT");
			Assert.That(narratorLukeGroup.CharacterIds.Count, Is.GreaterThan(1));
			Assert.That(narratorLukeGroup.CharacterIds,
				Does.Not.Contain(GetStandardCharacterId("ACT", BookOrChapter)));
			Assert.That(narratorLukeGroup.CharacterIds,
				Does.Not.Contain(GetStandardCharacterId("ACT", ExtraBiblical)));
			Assert.That(narratorActsGroup.CharacterIds.Count, Is.EqualTo(1));
			AssertThatThereAreTwoDistinctNarratorGroups(groups);
			Assert.That(groups, No<CharacterGroup>(g => g.CharacterIds, Does.Contain("BC-LUK")));
			Assert.That(groups.Count, Is.EqualTo(8));
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
			Assert.That(narratorLukeGroup.CharacterIds,
				Does.Not.Contain(GetStandardCharacterId("ACT", BookOrChapter)));
			Assert.That(narratorLukeGroup.CharacterIds,
				Does.Not.Contain(GetStandardCharacterId("ACT", ExtraBiblical)));
			Assert.That(narratorActsGroup.CharacterIds.Count, Is.EqualTo(1));
			AssertThatThereAreTwoDistinctNarratorGroups(groups);
			Assert.That(groups, No<CharacterGroup>(g => g.CharacterIds, Does.Contain("BC-LUK")));
			Assert.That(groups.Count, Is.EqualTo(8));
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
			Assert.That(narratorLukeGroup.CharacterIds.Count, Is.GreaterThan(1));
			Assert.That(narratorLukeGroup.CharacterIds.Intersect(new[] { "Lydia", "fortune telling slave girl" }).Count(), Is.EqualTo(1),
				"Lydia and the fortune-telling slave girl speak very close to each other, so the narrator of Luke should take on one of those character roles");
			Assert.That(narratorActsGroup.CharacterIds.Count, Is.EqualTo(1));
			AssertThatThereAreTwoDistinctNarratorGroups(groups);
			Assert.That(groups, No<CharacterGroup>(g => g.CharacterIds, Does.Contain("BC-LUK")));
			Assert.That(groups.Count, Is.EqualTo(11));
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
			Assert.That(narratorLukeGroup.CharacterIds.Count, Is.EqualTo(2));
			Assert.That(narratorLukeGroup.CharacterIds.Intersect(new[] { "Lydia", "fortune telling slave girl" }).Count(), Is.EqualTo(1),
				"Lydia and the fortune-telling slave girl speak very close to each other, so the narrator of Luke should take on one of those character roles");
			Assert.That(narratorActsGroup.CharacterIds.Count, Is.EqualTo(1));
			AssertThatThereAreTwoDistinctNarratorGroups(groups);
			VerifyProximityAndGenderConstraintsForAllGroups(groups, false, true);
			Assert.That(groups, No<CharacterGroup>(g => g.CharacterIds, Does.Contain("BC-LUK")));
			Assert.That(groups.Count, Is.EqualTo(14));
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
			Assert.That(narratorLukeGroup.CharacterIds.Count, Is.EqualTo(2));
			Assert.That(narratorLukeGroup.CharacterIds, Does.Contain("narrator-ACT"));
			Assert.That(groups, No<CharacterGroup>(g => g.CharacterIds, Does.Contain("BC-LUK")));
			Assert.That(groups.Count, Is.EqualTo(7));
		}

		[Test]
		public void GenerateCharacterGroups_DefaultNarratorPreferences_AllLinesForSingleVoiceBookAreNarrator()
		{
			SetVoiceActors(5, 2);
			var gen = new CharacterGroupGenerator(m_testProject);
			var groups = gen.GenerateCharacterGroups();
			Assert.That(GetNarratorGroupForBook(groups, "LUK"), Is.EqualTo(GetNarratorGroupForBook(groups, "ACT")));
			Assert.That(groups, No<CharacterGroup>(g => g.CharacterIds, Does.Contain("BC-LUK")));
			Assert.That(groups.Count, Is.EqualTo(7));
		}

		private void AssertThatThereAreTwoDistinctNarratorGroups(List<CharacterGroup> groups)
		{
			var narrator1Group = GetNarratorGroupForBook(groups, "LUK");
			var narrator2Group = GetNarratorGroupForBook(groups, "ACT");
			Assert.That(narrator1Group, Is.Not.EqualTo(narrator2Group));
		}
	}

	[TestFixture]
	public class CharacterGroupGeneratorTestsWithScriptModifiedDuringTest : CharacterGroupGeneratorAndAdjusterTestBase
	{
		[OneTimeSetUp]
		public void OneTimeSetUp()
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
			TestProject.DeleteTestProjects();
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
			Assert.That(zachGroup.CharacterIds.Count, Is.EqualTo(1));

			m_testProject.AvailableBooks[0].IncludeInScript = false;
			m_testProject.ClearCharacterStatistics(); // This simulates behavior in UI when the project is saved after displaying ScriptureRangeSelectionDlg
			gen = new CharacterGroupGenerator(m_testProject);

			groups = gen.GenerateCharacterGroups();

			cameoGroup = groups.Single(g => g.VoiceActorId == cameoActor.Id);
			Assert.That(groups.Count, Is.EqualTo(7));
			Assert.That(groups.Count(g => g.CharacterIds.Contains("Zaccheaus")), Is.EqualTo(0));
			Assert.That(cameoGroup.CharacterIds.Count, Is.EqualTo(0));
		}

		[Test]
		public void GenerateCharacterGroups_ProjectCharacterDetailExistsAndInScript_ProjectCharacterDetailIncludedInGeneratedGroups()
		{
			SetVoiceActors(8, 2);
			m_testProject.AddProjectCharacterDetail(new CharacterDetail { CharacterId = "Bobette", Gender = CharacterGender.Female });

			m_testProject.IncludedBooks[0].Blocks[3].CharacterId = "Bobette";

			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();
			Assert.That(groups.Count(g => g.CharacterIds.Contains("Bobette")), Is.EqualTo(1));
		}

		[Test]
		public void GenerateCharacterGroups_BlockNeedsReview_NeedsReviewCharacterNotIncludedInGeneratedGroups()
		{
			SetVoiceActors(8, 2);

			m_testProject.IncludedBooks[0].Blocks[3].CharacterId = kNeedsReview;

			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();
			Assert.That(groups, No<CharacterGroup>(g => g.CharacterIds, Does.Contain(kNeedsReview)));
		}

		[Test]
		public void GenerateCharacterGroups_ProjectCharacterDetailExistsButNotInScript_ProjectCharacterDetailNotIncludedInGeneratedGroups()
		{
			SetVoiceActors(8, 2);
			m_testProject.AddProjectCharacterDetail(new CharacterDetail { CharacterId = "Bobette", Gender = CharacterGender.Female });

			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();
			Assert.That(groups, No<CharacterGroup>(g => g.CharacterIds, Does.Contain("Bobette")));
		}
	}

	[TestFixture]
	public class CharacterGroupGeneratorTestsWithLotsOfBooks : CharacterGroupGeneratorAndAdjusterTestBase
	{
		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			// Note: there are currently only two tests in this fixture that require this:
			//    GenerateCharacterGroups_NumberOfNarratorsFourFewerThanAuthors_LukeCombinesWithJudeAndHebrewsAndJohnCombinesWithPaulAndMark
			//    GenerateCharacterGroups_NumberOfNarratorsTwoFewerThanAuthors_PaulCombinesWithJudeAndHebrewsCombinesWithMark
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

			BackgroundWorker worker = new BackgroundWorker {WorkerSupportsCancellation = true};
			CharacterGroupGenerator generator = new CharacterGroupGenerator(m_testProject, null, worker);
			worker.DoWork += (sender, args) =>
			{
				generator.GenerateCharacterGroups();
			};

			var start = DateTime.Now;
			worker.RunWorkerAsync();
			worker.CancelAsync();

			while (worker.IsBusy)
			{
				Assert.That(DateTime.Now.Subtract(start).Seconds, Is.LessThan(6), "Failed to cancel within timeout (6 seconds)");
				Thread.Sleep(100);
			}

			Assert.That(generator.GeneratedGroups, Is.Null);
			Assert.That(m_testProject.CharacterGroupList.CharacterGroups.Count, Is.EqualTo(1));
			Assert.That(group, Is.EqualTo(m_testProject.CharacterGroupList.CharacterGroups[0]));
		}

		// Comma-separated lists of books which are expected to be grouped together. Each group delimited by a |
		[TestCase("MRK,LUK,ACT,GAL,EPH,PHM,HEB,1JN,2JN,3JN,JUD,REV")]
		[TestCase("MRK,HEB,JUD,LUK,ACT,1JN,2JN,3JN,REV|GAL,EPH,PHM")]
		[TestCase("MRK,HEB,1JN,2JN,3JN,REV|LUK,ACT,JUD|GAL,EPH,PHM")]
		[TestCase("MRK,HEB|LUK,ACT|GAL,EPH,PHM|1JN,2JN,3JN,JUD,REV")]
		[TestCase("MRK|LUK,ACT|GAL,EPH,PHM|HEB,JUD|1JN,2JN,3JN,REV")]
		[TestCase("MRK|LUK,ACT|GAL,EPH,PHM|HEB|JUD|1JN,2JN,3JN,REV")]
		public void GenerateCharacterGroups_LargeCast_NarrationByAuthor_NarratorsGroupedByAuthorAndHaveDistinctNarrationRoles(string narratorGroups)
		{
			var expectedNarratorGroups = narratorGroups.Split('|');
			m_testProject.CharacterGroupGenerationPreferences.NarratorsOption = NarratorsOption.NarrationByAuthor;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = expectedNarratorGroups.Length;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;
			m_testProject.CharacterGroupGenerationPreferences.CastSizeOption = CastSizeOption.MatchVoiceActorList;

			SetVoiceActors(34, 4, 1);
			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();

			Assert.That(m_testProject.VoiceActorList.AllActors.Count, Is.EqualTo(groups.Count));
			foreach (var bookList in expectedNarratorGroups)
			{
				var books = bookList.Split(',');
				var narGroup = GetNarratorGroupForBook(groups, books[0]);
				for (int i = 1; i < books.Length; i++)
				{
					Assert.That(narGroup.CharacterIds.Contains(GetStandardCharacterId(books[i], Narrator)),
						$"Expected group containing the narrator of {books[0]} to also contain the narrator of {books[i]}. All characters in group containing narrator of {books[0]}:" +
						Environment.NewLine + Join(Environment.NewLine, narGroup.CharacterIds) +
						Environment.NewLine + $"All characters in group for {books[i]}:" +
						Environment.NewLine + Join(Environment.NewLine, GetNarratorGroupForBook(groups, books[i])));
				}
				if (books.Contains("EPH"))
				{
					Assert.That(narGroup.CharacterIds, Does.Contain("Paul"));
					Assert.That(books.Length + 1, Is.EqualTo(narGroup.CharacterIds.Count));
				}
				else
					Assert.That(books.Length, Is.EqualTo(narGroup.CharacterIds.Count));
			}
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

			Assert.That(m_testProject.VoiceActorList.AllActors.Count, Is.EqualTo(groups.Count));
			var narMark = GetNarratorGroupForBook(groups, "MRK");
			var narLuke = GetNarratorGroupForBook(groups, "LUK");
			var narPaul = GetNarratorGroupForBook(groups, "EPH");
			var narHebrews = GetNarratorGroupForBook(groups, "HEB");
			var narJohn = GetNarratorGroupForBook(groups, "1JN");
			var narJude = GetNarratorGroupForBook(groups, "JUD");

			Assert.That(narMark.CharacterIds.Count, Is.EqualTo(1));
			Assert.That(narLuke.CharacterIds.Count, Is.EqualTo(2));
			Assert.That(narLuke.CharacterIds, Does.Contain(GetStandardCharacterId("ACT", Narrator)));
			Assert.That(narPaul.CharacterIds.Count, Is.EqualTo(3));
			Assert.That(narPaul.CharacterIds, Does.Contain(GetStandardCharacterId("GAL", Narrator)));
			Assert.That(narPaul.CharacterIds, Does.Contain(GetStandardCharacterId("PHM", Narrator)));
			Assert.That(narHebrews.CharacterIds.Count, Is.EqualTo(1));
			Assert.That(narJohn.CharacterIds.Count, Is.EqualTo(4));
			Assert.That(narJohn.CharacterIds, Does.Contain(GetStandardCharacterId("2JN", Narrator)));
			Assert.That(narJohn.CharacterIds, Does.Contain(GetStandardCharacterId("3JN", Narrator)));
			Assert.That(narJohn.CharacterIds, Does.Contain(GetStandardCharacterId("REV", Narrator)));
			Assert.That(narJude.CharacterIds.Count, Is.EqualTo(1));
		}

		[TestCase(2)]
		[TestCase(4)]
		[TestCase(6)]
		public void GenerateCharacterGroups_SmallCast_NarrationByAuthor_PaulAndHisBooksSeparateFromOtherNarrators(int numberOfNarrators)
		{
			m_testProject.CharacterGroupGenerationPreferences.NarratorsOption = NarratorsOption.NarrationByAuthor;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = numberOfNarrators;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			SetVoiceActors(10, 2, 1);
			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();
			var allBookIds = m_testProject.IncludedBookIds.ToList();

			Assert.That(m_testProject.VoiceActorList.AllActors.Count, Is.EqualTo(groups.Count));
			var narratorGroups = groups.Where(g => g.CharacterIds.Any(
				c => IsCharacterOfType(c, Narrator))).ToList();
			Assert.That(numberOfNarrators, Is.EqualTo(narratorGroups.Count));
			foreach (var narGroup in narratorGroups)
			{
				var booksNarratedInThisGroup = narGroup.CharacterIds.Where(c => IsCharacterOfType(c, Narrator))
					.Select(GetBookCodeFromStandardCharacterId).ToList();
				Assert.That(booksNarratedInThisGroup.All(b => allBookIds.Contains(b)),
					"Every narrator character in the group should be for a book that is in the project and is not in any other group.");
				allBookIds.RemoveAll(b => booksNarratedInThisGroup.Contains(b));

				if (booksNarratedInThisGroup.Contains("EPH"))
				{
					Assert.That(booksNarratedInThisGroup, Is.EquivalentTo(new [] { "GAL", "EPH", "PHM" }));
					Assert.That(narGroup.CharacterIds.ToList(), Does.Contain("Paul"));
				}
				else
				{
					// REVIEW: This expectation seems to be true for now, but it is not a strict requirement of the test and
					// future code changes may render it false for some test cases.
					Assert.That(booksNarratedInThisGroup.Count,
						Is.LessThan(narGroup.CharacterIds.Count),
						$"Narrator group {Join(", ", booksNarratedInThisGroup)} expected to contain some other roles.");
				}
			}
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

			Assert.That(m_testProject.VoiceActorList.AllActors.Count, Is.EqualTo(groups.Count));
			var narMark = GetNarratorGroupForBook(groups, "MRK");
			var narLuke = GetNarratorGroupForBook(groups, "LUK");
			var narPaul = GetNarratorGroupForBook(groups, "EPH");
			var narHebrews = GetNarratorGroupForBook(groups, "HEB");
			var narJohn = GetNarratorGroupForBook(groups, "1JN");
			var narJude = GetNarratorGroupForBook(groups, "JUD");

			Assert.That(narMark.CharacterIds.Count, Is.GreaterThan(1));
			Assert.That(narLuke.CharacterIds.Count, Is.GreaterThan(2));
			Assert.That(narLuke.CharacterIds, Does.Contain(GetStandardCharacterId("ACT", Narrator)));
			Assert.That(narPaul.CharacterIds.Count, Is.GreaterThan(3));
			Assert.That(narPaul.CharacterIds, Does.Contain(GetStandardCharacterId("GAL", Narrator)));
			Assert.That(narPaul.CharacterIds, Does.Contain(GetStandardCharacterId("PHM", Narrator)));
			Assert.That(narHebrews.CharacterIds.Count, Is.GreaterThan(1));
			Assert.That(narJohn.CharacterIds.Count, Is.GreaterThan(4));
			Assert.That(narJohn.CharacterIds, Does.Contain(GetStandardCharacterId("2JN", Narrator)));
			Assert.That(narJohn.CharacterIds, Does.Contain(GetStandardCharacterId("3JN", Narrator)));
			Assert.That(narJohn.CharacterIds, Does.Contain(GetStandardCharacterId("REV", Narrator)));
			Assert.That(narJude.CharacterIds.Count, Is.GreaterThan(1));

			VerifyGenderConformityInGroups(groups, true);
		}

		[Test]
		public void GenerateCharacterGroups_TooSmallCast_FourNarrators_AllCharactersConformToGroupGender()
		{
			m_testProject.CharacterGroupGenerationPreferences.NarratorsOption = NarratorsOption.Custom;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 4;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			SetVoiceActors(9, 2);
			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();

			Assert.That(m_testProject.VoiceActorList.AllActors.Count, Is.EqualTo(groups.Count));

			VerifyGenderConformityInGroups(groups, true);
		}

		[Test]
		public void GenerateCharacterGroups_NumberOfNarratorsOneFewerThanAuthors_HebrewsAndJudeShareNarrators()
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 5;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			SetVoiceActors(34, 4);
			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();

			Assert.That(m_testProject.VoiceActorList.AllActors.Count, Is.EqualTo(groups.Count));
			var narMark = GetNarratorGroupForBook(groups, "MRK");
			var narLuke = GetNarratorGroupForBook(groups, "LUK");
			var narPaul = GetNarratorGroupForBook(groups, "EPH");
			var narHebrewsAndJude = GetNarratorGroupForBook(groups, "HEB");
			var narJohn = GetNarratorGroupForBook(groups, "1JN");

			Assert.That(narMark.CharacterIds.Count, Is.EqualTo(1));
			Assert.That(narLuke.CharacterIds.Count, Is.EqualTo(2));
			Assert.That(narLuke.CharacterIds, Does.Contain(GetStandardCharacterId("ACT", Narrator)));
			Assert.That(narPaul.CharacterIds.Count, Is.EqualTo(3));
			Assert.That(narPaul.CharacterIds, Does.Contain(GetStandardCharacterId("GAL", Narrator)));
			Assert.That(narPaul.CharacterIds, Does.Contain(GetStandardCharacterId("PHM", Narrator)));
			Assert.That(narHebrewsAndJude.CharacterIds.Count, Is.EqualTo(2));
			Assert.That(narHebrewsAndJude.CharacterIds, Does.Contain(GetStandardCharacterId("JUD", Narrator)));
			Assert.That(narJohn.CharacterIds.Count, Is.EqualTo(4));
			Assert.That(narJohn.CharacterIds, Does.Contain(GetStandardCharacterId("2JN", Narrator)));
			Assert.That(narJohn.CharacterIds, Does.Contain(GetStandardCharacterId("3JN", Narrator)));
			Assert.That(narJohn.CharacterIds, Does.Contain(GetStandardCharacterId("REV", Narrator)));
		}

		[Test]
		public void GenerateCharacterGroups_NumberOfNarratorsTwoFewerThanAuthors_PaulCombinesWithJudeAndHebrewsCombinesWithMark()
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 4;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			SetVoiceActors(34, 4);
			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();

			Assert.That(m_testProject.VoiceActorList.AllActors.Count, Is.EqualTo(groups.Count));
			var narMarkAndHebrews = GetNarratorGroupForBook(groups, "MRK");
			var narLuke = GetNarratorGroupForBook(groups, "LUK");
			var narPaulAndJude = GetNarratorGroupForBook(groups, "EPH");
			var narJohn = GetNarratorGroupForBook(groups, "1JN");

			Assert.That(narMarkAndHebrews.CharacterIds.Count, Is.EqualTo(2));
			Assert.That(narMarkAndHebrews.CharacterIds, Does.Contain(GetStandardCharacterId("HEB", Narrator)));
			Assert.That(narLuke.CharacterIds.Count, Is.EqualTo(2));
			Assert.That(narLuke.CharacterIds, Does.Contain(GetStandardCharacterId("ACT", Narrator)));
			Assert.That(narPaulAndJude.CharacterIds.Count, Is.EqualTo(4));
			Assert.That(narPaulAndJude.CharacterIds, Does.Contain(GetStandardCharacterId("GAL", Narrator)));
			Assert.That(narPaulAndJude.CharacterIds, Does.Contain(GetStandardCharacterId("PHM", Narrator)));
			Assert.That(narPaulAndJude.CharacterIds, Does.Contain(GetStandardCharacterId("JUD", Narrator)));
			Assert.That(narJohn.CharacterIds.Count, Is.EqualTo(4));
			Assert.That(narJohn.CharacterIds, Does.Contain(GetStandardCharacterId("2JN", Narrator)));
			Assert.That(narJohn.CharacterIds, Does.Contain(GetStandardCharacterId("3JN", Narrator)));
			Assert.That(narJohn.CharacterIds, Does.Contain(GetStandardCharacterId("REV", Narrator)));
		}

		[Test]
		public void GenerateCharacterGroups_NumberOfNarratorsThreeFewerThanAuthors_JohnCombinesWithJudeAndHebrewsAndPaulCombinesWithMark()
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 3;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			SetVoiceActors(34, 4);
			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();

			Assert.That(m_testProject.VoiceActorList.AllActors.Count, Is.EqualTo(groups.Count));
			var narLuke = GetNarratorGroupForBook(groups, "LUK");
			var narPaulAndMark = GetNarratorGroupForBook(groups, "EPH");
			var narJohnJudeHebrews = GetNarratorGroupForBook(groups, "1JN");

			Assert.That(narLuke.CharacterIds.Count, Is.EqualTo(2));
			Assert.That(narLuke.CharacterIds, Does.Contain(GetStandardCharacterId("ACT", Narrator)));
			Assert.That(narPaulAndMark.CharacterIds.Count, Is.EqualTo(4));
			Assert.That(narPaulAndMark.CharacterIds, Does.Contain(GetStandardCharacterId("MRK", Narrator)));
			Assert.That(narPaulAndMark.CharacterIds, Does.Contain(GetStandardCharacterId("GAL", Narrator)));
			Assert.That(narPaulAndMark.CharacterIds, Does.Contain(GetStandardCharacterId("PHM", Narrator)));
			Assert.That(narJohnJudeHebrews.CharacterIds.Count, Is.EqualTo(6));
			Assert.That(narJohnJudeHebrews.CharacterIds, Does.Contain(GetStandardCharacterId("HEB", Narrator)));
			Assert.That(narJohnJudeHebrews.CharacterIds, Does.Contain(GetStandardCharacterId("2JN", Narrator)));
			Assert.That(narJohnJudeHebrews.CharacterIds, Does.Contain(GetStandardCharacterId("3JN", Narrator)));
			Assert.That(narJohnJudeHebrews.CharacterIds, Does.Contain(GetStandardCharacterId("JUD", Narrator)));
			Assert.That(narJohnJudeHebrews.CharacterIds, Does.Contain(GetStandardCharacterId("REV", Narrator)));
		}

		[Test]
		public void GenerateCharacterGroups_NumberOfNarratorsFourFewerThanAuthors_LukeCombinesWithJudeAndHebrewsAndJohnCombinesWithPaulAndMark()
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 2;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			SetVoiceActors(34, 4);
			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();

			Assert.That(m_testProject.VoiceActorList.AllActors.Count, Is.EqualTo(groups.Count));
			var narLukeJudeAndHebrews = GetNarratorGroupForBook(groups, "LUK");
			var narJohnPaulAndMark = GetNarratorGroupForBook(groups, "1JN");

			Assert.That(narLukeJudeAndHebrews.CharacterIds.Count, Is.EqualTo(4));
			Assert.That(narLukeJudeAndHebrews.CharacterIds, Does.Contain(GetStandardCharacterId("ACT", Narrator)));
			Assert.That(narLukeJudeAndHebrews.CharacterIds, Does.Contain(GetStandardCharacterId("HEB", Narrator)));
			Assert.That(narLukeJudeAndHebrews.CharacterIds, Does.Contain(GetStandardCharacterId("JUD", Narrator)));
			Assert.That(narJohnPaulAndMark.CharacterIds.Count, Is.EqualTo(8));
			Assert.That(narJohnPaulAndMark.CharacterIds, Does.Contain(GetStandardCharacterId("MRK", Narrator)));
			Assert.That(narJohnPaulAndMark.CharacterIds, Does.Contain(GetStandardCharacterId("GAL", Narrator)));
			Assert.That(narJohnPaulAndMark.CharacterIds, Does.Contain(GetStandardCharacterId("EPH", Narrator)));
			Assert.That(narJohnPaulAndMark.CharacterIds, Does.Contain(GetStandardCharacterId("PHM", Narrator)));
			Assert.That(narJohnPaulAndMark.CharacterIds, Does.Contain(GetStandardCharacterId("2JN", Narrator)));
			Assert.That(narJohnPaulAndMark.CharacterIds, Does.Contain(GetStandardCharacterId("3JN", Narrator)));
			Assert.That(narJohnPaulAndMark.CharacterIds, Does.Contain(GetStandardCharacterId("REV", Narrator)));
		}

		[Test]
		public void GenerateCharacterGroups_NumberOfNarratorsTwoFewerThanBooks_JohanineEpistlesGetCombined()
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 8;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 2;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			SetVoiceActors(36, 6);
			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();

			Assert.That(m_testProject.VoiceActorList.AllActors.Count, Is.EqualTo(groups.Count));
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

			Assert.That(narMrk.CharacterIds.Count, Is.EqualTo(1));
			Assert.That(narLuk.CharacterIds.Count, Is.EqualTo(1));
			Assert.That(narAct.CharacterIds.Count, Is.EqualTo(1));
			Assert.That(narGal.CharacterIds.Count, Is.EqualTo(1));
			Assert.That(narEph.CharacterIds.Count, Is.EqualTo(1));
			Assert.That(narPhm.CharacterIds.Count, Is.EqualTo(1));
			Assert.That(narHeb.CharacterIds.Count, Is.EqualTo(1));
			Assert.That(narJohanineEpistles.CharacterIds.Count, Is.EqualTo(3));
			Assert.That(narJohanineEpistles.CharacterIds, Does.Contain(GetStandardCharacterId("2JN", Narrator)));
			Assert.That(narJohanineEpistles.CharacterIds, Does.Contain(GetStandardCharacterId("3JN", Narrator)));
			Assert.That(narJud.CharacterIds.Count, Is.EqualTo(1));
			Assert.That(narRev.CharacterIds.Count, Is.EqualTo(1));

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
			Assert.That(groups.Except(narrators).Count(n => n.ContainsCharacterWithGender(CharacterGender.Female)), Is.EqualTo(4));
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
			m_testProject.CharacterGroupList.CharacterGroups.Single(g => g.VoiceActorId == cameoMaleNarrator.Id).CharacterIds.Add(GetStandardCharacterId("2JN", Narrator));
			m_testProject.CharacterGroupList.CharacterGroups.Single(g => g.VoiceActorId == cameoFemaleNarrator.Id).CharacterIds.Add(GetStandardCharacterId("HEB", Narrator));
			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();

			Assert.That(m_testProject.VoiceActorList.AllActors.Count, Is.EqualTo(groups.Count));
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

			Assert.That(narMrk.CharacterIds.Count, Is.EqualTo(1));
			Assert.That(narLuk.CharacterIds.Count, Is.EqualTo(1));
			Assert.That(narAct.CharacterIds.Count, Is.EqualTo(1));
			Assert.That(narGal.CharacterIds.Count, Is.EqualTo(1));
			Assert.That(narEphAndPhm.CharacterIds.Count, Is.EqualTo(2));
			Assert.That(narEphAndPhm.CharacterIds, Does.Contain(GetStandardCharacterId("PHM", Narrator)));
			Assert.That(narHeb.CharacterIds.Count, Is.EqualTo(1));
			Assert.That(narHeb.AssignedToCameoActor, Is.True);
			Assert.That(nar1JnAnd3Jn.CharacterIds.Count, Is.EqualTo(2));
			Assert.That(nar1JnAnd3Jn.CharacterIds, Does.Contain(GetStandardCharacterId("3JN", Narrator)));
			Assert.That(nar2Jn.CharacterIds.Count, Is.EqualTo(1));
			Assert.That(nar2Jn.AssignedToCameoActor, Is.True);
			Assert.That(narJud.CharacterIds.Count, Is.EqualTo(1));
			Assert.That(narRev.CharacterIds.Count, Is.EqualTo(1));

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
			Assert.That(groups.Except(narrators).Count(n => n.ContainsCharacterWithGender(CharacterGender.Female)), Is.EqualTo(4));
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
				GetStandardCharacterId("MRK", Narrator),
				GetStandardCharacterId("LUK", Narrator),
				GetStandardCharacterId("ACT", Narrator),
				GetStandardCharacterId("GAL", Narrator),
				GetStandardCharacterId("EPH", Narrator),
				GetStandardCharacterId("PHM", Narrator)
			});
			m_testProject.CharacterGroupList.CharacterGroups.Single(g => g.VoiceActorId == cameoMaleNarrator2.Id).CharacterIds.AddRange(new[]
			{
				GetStandardCharacterId("HEB", Narrator),
				GetStandardCharacterId("1JN", Narrator),
				GetStandardCharacterId("2JN", Narrator),
				GetStandardCharacterId("3JN", Narrator),
				GetStandardCharacterId("JUD", Narrator),
				GetStandardCharacterId("REV", Narrator)
			});

			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();

			Assert.That(m_testProject.VoiceActorList.AllActors.Count, Is.EqualTo(groups.Count));
			var narMrk = GetNarratorGroupForBook(groups, "MRK");
			var narHeb = GetNarratorGroupForBook(groups, "HEB");

			Assert.That(narMrk.CharacterIds.Count, Is.EqualTo(6));
			Assert.That(narMrk.AssignedToCameoActor, Is.True);
			Assert.That(narHeb.CharacterIds.Count, Is.EqualTo(6));
			Assert.That(narHeb.AssignedToCameoActor, Is.True);

			var narrators = new List<CharacterGroup> { narMrk, narHeb };
			foreach (var grp in groups.Except(narrators))
				Assert.That(grp.CharacterIds.Any(c => IsCharacterOfType(c, Narrator)), Is.False);
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
				GetStandardCharacterId("MRK", Narrator),
			});
			m_testProject.CharacterGroupList.CharacterGroups.Single(g => g.VoiceActorId == cameoMaleNarrator2.Id).CharacterIds.AddRange(new[]
			{
				GetStandardCharacterId("GAL", Narrator),
				GetStandardCharacterId("EPH", Narrator),
				GetStandardCharacterId("PHM", Narrator)
			});
			m_testProject.CharacterGroupList.CharacterGroups.Single(g => g.VoiceActorId == cameoMaleNarrator3.Id).CharacterIds.AddRange(new[]
			{
				GetStandardCharacterId("HEB", Narrator),
			});

			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();

			Assert.That(m_testProject.VoiceActorList.AllActors.Count, Is.EqualTo(groups.Count));
			var narMrk = GetNarratorGroupForBook(groups, "MRK");
			var narGal = GetNarratorGroupForBook(groups, "GAL");
			var narHeb = GetNarratorGroupForBook(groups, "HEB");
			var narNonCameo = GetNarratorGroupForBook(groups, "LUK");

			Assert.That(narMrk.CharacterIds.Count, Is.EqualTo(1));
			Assert.That(narMrk.AssignedToCameoActor, Is.True);
			Assert.That(narGal.CharacterIds.Count, Is.EqualTo(3));
			Assert.That(narGal.AssignedToCameoActor, Is.True);
			Assert.That(narGal.CharacterIds, Does.Contain(GetStandardCharacterId("EPH", Narrator)));
			Assert.That(narGal.CharacterIds, Does.Contain(GetStandardCharacterId("PHM", Narrator)));
			Assert.That(narHeb.CharacterIds.Count, Is.EqualTo(1));
			Assert.That(narHeb.AssignedToCameoActor, Is.True);
			Assert.That(narNonCameo.AssignedToCameoActor, Is.False);
			Assert.That(narNonCameo.CharacterIds.Count(c => IsCharacterOfType(c, Narrator)), Is.EqualTo(7));

			var narrators = new List<CharacterGroup> { narMrk, narGal, narHeb, narNonCameo };
			foreach (var grp in groups.Except(narrators))
				Assert.That(grp.CharacterIds.Any(c => IsCharacterOfType(c, Narrator)), Is.False);
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
			Assert.That(groups.Count(g => g.GroupIdLabel == CharacterGroup.Label.Narrator), Is.EqualTo(1));
			Assert.That(groups.Count(g => g.GroupIdLabel == CharacterGroup.Label.None), Is.EqualTo(0));
			Assert.That(groups.Count(g => g.GroupIdLabel == CharacterGroup.Label.Other), Is.EqualTo(0));
			Assert.That(castSizeValues.Child, Is.EqualTo(groups.Count(g => g.GroupIdLabel == CharacterGroup.Label.Child)));
			Assert.That(castSizeValues.Female - m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators, Is.EqualTo(groups.Count(g => g.GroupIdLabel == CharacterGroup.Label.Female)));
			Assert.That(castSizeValues.Male - m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators, Is.EqualTo(groups.Count(g => g.GroupIdLabel == CharacterGroup.Label.Male)));
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
			Assert.That(groups.Count(g => g.GroupIdLabel == CharacterGroup.Label.Narrator),
				Is.EqualTo(1));
			Assert.That(groups.Count(g => g.GroupIdLabel == CharacterGroup.Label.None),
				Is.EqualTo(0));
			Assert.That(groups.Count(g => g.GroupIdLabel == CharacterGroup.Label.Other),
				Is.EqualTo(0));
			Assert.That(groups.Count(g => g.GroupIdLabel == CharacterGroup.Label.Child),
				Is.EqualTo(Math.Min(castSizeValues.Child, numChildCharactersInScript)));
			Assert.That(groups.Count(g => g.GroupIdLabel == CharacterGroup.Label.Female),
				Is.EqualTo(castSizeValues.Female - m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators));
			Assert.That(groups.Count(g => g.GroupIdLabel == CharacterGroup.Label.Male),
				Is.EqualTo(castSizeValues.Male - m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators));
		}

		[Test]
		public void GenerateCharacterGroups_CastSoSmallThatOnlyOneMaleActorIsNotANarratorOrExtra_DeityNotGroupedWithOtherCharacters()
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 6;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			SetVoiceActors(m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators + 2, 1);
			var groups = new CharacterGroupGenerator(m_testProject).GenerateCharacterGroups();

			Assert.That(m_testProject.VoiceActorList.AllActors.Count, Is.EqualTo(groups.Count));
			var deityGroup = groups.Single(g => g.CharacterIds.Contains("Jesus"));
			Assert.That(deityGroup.CharacterIds, Is.EquivalentTo(new []
				{
					"Jesus", "God", "Holy Spirit, the", kScriptureCharacter
				}));
		}
	}

	[TestFixture]
	class CharacterGroupGeneratorTestsWith1JnAnd2JnAndEph : CharacterGroupGeneratorAndAdjusterTestBase
	{
		[OneTimeSetUp]
		public void OneTimeSetUp()
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

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			TestProject.DeleteTestProjects();
		}

		[Test]
		public void GenerateGroups_AllActorsAreNarrators_NarratorsDoNotCauseProximityClash()
		{
			SetVoiceActors(2);
			var generator = new CharacterGroupGenerator(m_testProject);
			var groups = generator.GenerateCharacterGroups();

			Assert.That(groups.Count, Is.EqualTo(2));
			Assert.That(!IsCharacterOfType(generator.MinimumProximity.FirstCharacterId, Narrator) ||
				!IsCharacterOfType(generator.MinimumProximity.SecondCharacterId, Narrator), Is.True);
			Assert.That(generator.MinimumProximity.IsAcceptable(), Is.True);
		}
	}

	[TestFixture]
	public class CharacterGroupGeneratorTestsWithExtraBiblicalCharacterOptions : CharacterGroupGeneratorAndAdjusterTestBase
	{
		[OneTimeSetUp]
		public void OneTimeSetUp()
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

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			TestProject.DeleteTestProjects();
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
			Assert.That(characterIds, Does.Contain("narrator-RUT"));
			Assert.That(characterIds, Does.Contain("intro-RUT"));
			Assert.That(characterIds, Does.Contain("extra-RUT"));
			Assert.That(characterIds, Does.Contain("BC-RUT"));
		}
	}

	public abstract class CharacterGroupGeneratorAndAdjusterTestBase
	{
		protected Project m_testProject;
		protected void SetVoiceActors(int numberOfAdultMales, int numberOfAdultFemales = 0, int numberOfMaleChildren = 0, int numberOfFemaleChildren = 0)
		{
			var actorList = new List<VoiceActor>();
			int actorNumber = 0;
			for (int i = 0; i < numberOfAdultMales; i++)
				actorList.Add(new VoiceActor { Id = actorNumber++ } );
			for (int i = 0; i < numberOfAdultFemales; i++)
				actorList.Add(new VoiceActor {  Id = actorNumber++, Gender = ActorGender.Female });
			for (int i = 0; i < numberOfMaleChildren; i++)
				actorList.Add(new VoiceActor {  Id = actorNumber++, Age = ActorAge.Child });
			for (int i = 0; i < numberOfFemaleChildren; i++)
				actorList.Add(new VoiceActor {  Id = actorNumber++, Gender = ActorGender.Female, Age = ActorAge.Child });
			m_testProject.VoiceActorList.AllActors = actorList;
		}

		protected static string GetNarratorId(string bookId)
		{
			return GetStandardCharacterId(bookId, Narrator);
		}

		protected static CharacterGroup GetNarratorGroupForBook(List<CharacterGroup> groups, string bookId)
		{
			return groups.Single(g => g.CharacterIds.Contains(GetStandardCharacterId(bookId,
				Narrator)));
		}

		protected void VerifyProximityAndGenderConstraintsForAllGroups(List<CharacterGroup> groups,
			bool allowMaleNarratorsToDoBiblicalCharacterRoles = false, bool allowFemaleNarratorsToDoBiblicalCharacterRoles = false)
		{
			var p = new Proximity(m_testProject);
			foreach (var group in groups.Where(g => !CharacterGroupGenerator.ContainsDeityCharacter(g)))
			{
				var minProximity = p.CalculateMinimumProximity(group.CharacterIds);
				Debug.WriteLine(group.GroupIdForUiDisplay + ": " + minProximity);
				Assert.That(minProximity.IsAcceptable, Is.True, $"Group {group.GroupIdForUiDisplay} has proximity problem: " +
					$"{minProximity.NumberOfBlocks} between {minProximity.FirstCharacterId} ({minProximity.FirstReference}) and " +
					$"{minProximity.SecondCharacterId} ({minProximity.SecondReference}).");
			}

			VerifyGenderConformityInGroups(groups, allowMaleNarratorsToDoBiblicalCharacterRoles, allowFemaleNarratorsToDoBiblicalCharacterRoles);
		}

		protected void VerifyGenderConformityInGroups(List<CharacterGroup> groups,
			bool allowMaleNarratorsToDoBiblicalCharacterRoles = false, bool allowFemaleNarratorsToDoBiblicalCharacterRoles = false)
		{
			foreach (var group in groups.Where(g => g.ContainsCharacterWithGender(CharacterGender.Female)))
				Assert.That(CharacterGroup.Label.Female == group.GroupIdLabel ||
					(allowFemaleNarratorsToDoBiblicalCharacterRoles && CharacterGroup.Label.Narrator == group.GroupIdLabel),
					Is.True,
					$"Group {group.GroupIdForUiDisplay} contains female characters.");


			foreach (var group in groups.Where(g => g.CharactersWithGender(CharacterGender.Male, CharacterGender.PreferMale)
				.Any(d => d.Age != CharacterAge.Child)))
			{
				Assert.That(CharacterGroup.Label.Male == group.GroupIdLabel ||
					(allowMaleNarratorsToDoBiblicalCharacterRoles && CharacterGroup.Label.Narrator == group.GroupIdLabel),
					Is.True,
					$"Group {group.GroupIdForUiDisplay} contains male characters.");
			}
		}
	}
}
