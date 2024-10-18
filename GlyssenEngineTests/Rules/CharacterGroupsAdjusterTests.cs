using System.Linq;
using GlyssenCharacters;
using GlyssenEngine.Bundle;
using GlyssenEngine.Casting;
using GlyssenEngine.Rules;
using NUnit.Framework;
using SIL.Scripture;
using Resources = GlyssenCharactersTests.Properties.Resources;

namespace GlyssenEngineTests.Rules
{
	[TestFixture]
	class CharacterGroupsAdjusterTests : CharacterGroupGeneratorAndAdjusterTestBase
	{
		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerseOct2015;
			CharacterDetailData.TabDelimitedCharacterDetailData = Resources.TestCharacterDetailOct2015;
			RelatedCharactersData.Source = null;
		}

		[SetUp]
		public void SetUp()
		{
			CreateTestProject();

			m_testProject.DramatizationPreferences.BookTitleAndChapterDramatization = ExtraBiblicalMaterialSpeakerOption.Narrator;
			m_testProject.DramatizationPreferences.SectionHeadDramatization = ExtraBiblicalMaterialSpeakerOption.Narrator;

			m_testProject.AvailableBooks.Single(b => b.Code == "MRK").IncludeInScript = true;
			m_testProject.AvailableBooks.Single(b => b.Code == "LUK").IncludeInScript = true;
			m_testProject.AvailableBooks.Single(b => b.Code == "ACT").IncludeInScript = false;
			m_testProject.AvailableBooks.Single(b => b.Code == "JUD").IncludeInScript = false;
			m_testProject.ClearCharacterStatistics(); // This simulates behavior in UI when the project is saved after displaying ScriptureRangeSelectionDlg
			m_testProject.CharacterGroupList.CharacterGroups.Clear();
		}

		private void CreateTestProject()
		{
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.MRK, TestProject.TestBook.LUK, TestProject.TestBook.ACT, TestProject.TestBook.JUD);
			// Since we're testing something that should only happen once phase 1 is complete, we simulate that by assigning all ambiguous/unknown blocks
			foreach (var block in m_testProject.IncludedBooks.SelectMany(b => b.Blocks).Where(b => b.CharacterIsUnclear))
				block.CharacterId = "Adam";

			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 1;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;
		}

		private void GenerateGroups()
		{
			var generator = new CharacterGroupGenerator(m_testProject);
			generator.GenerateCharacterGroups();
			generator.ApplyGeneratedGroupsToProject();
		}

		private void RenameCharacter(int bookNum, int chapter, int verse, string existingCharacterId, string newCharacterId)
		{
			var block = m_testProject.Books.Single(book => book.BookId == BCVRef.NumberToBookCode(bookNum)).GetBlocksForVerse(chapter, verse)
				.Single(b => b.CharacterId == existingCharacterId);
			block.CharacterId = newCharacterId;

			m_testProject.ProjectCharacterVerseData.AddEntriesFor(bookNum, block);

			if (!m_testProject.AllCharacterDetailDictionary.ContainsKey(newCharacterId))
				m_testProject.AddProjectCharacterDetail(new CharacterDetail {CharacterId = newCharacterId});

			m_testProject.ClearCharacterStatistics(); // This simulates behavior in UI when user makes changes in AssignCharacterDlg
		}

		private void SetBlockCharacterToNarrator(string bookCode, int chapter, int verse, string existingCharacterId)
		{
			m_testProject.Books.Single(book => book.BookId == bookCode).GetBlocksForVerse(chapter, verse)
				.Single(b => b.CharacterId == existingCharacterId).CharacterId =
				CharacterVerseData.GetStandardCharacterId(bookCode, CharacterVerseData.StandardCharacter.Narrator);
		}

		[Test]
		public void Constructor_PerfectCoverage_NoAdditionsOrDeletions()
		{
			SetVoiceActors(9, 2, 1);
			GenerateGroups();
			var adjuster = new CharacterGroupsAdjuster(m_testProject);
			Assert.That(adjuster.CharactersNotCoveredByAnyGroup, Is.Empty);
			Assert.That(adjuster.CharactersNoLongerInUse, Is.Empty);
			Assert.That(adjuster.CharacterGroupsToRemove, Is.Empty);
			Assert.That(adjuster.NewBooksHaveBeenIncluded, Is.False);
			Assert.That(adjuster.BooksHaveBeenExcluded, Is.False);
			Assert.That(adjuster.FullRegenerateRecommended, Is.False);
			Assert.That(adjuster.GroupsAreNotInSynchWithData, Is.False);
		}

		/// <summary>
		/// PG-965
		/// </summary>
		[Test]
		public void CharactersNotCoveredByAnyGroup_ExtraBiblicalCharactersOmittedInSettings_ExtraBiblicalCharactersNotConsideredToBeMissing()
		{
			m_testProject.DramatizationPreferences.SectionHeadDramatization = ExtraBiblicalMaterialSpeakerOption.Omitted;
			SetVoiceActors(9, 2, 1);
			GenerateGroups();
			var adjuster = new CharacterGroupsAdjuster(m_testProject);
			Assert.That(adjuster.CharactersNotCoveredByAnyGroup, Is.Empty);
		}

		[Test]
		public void CharacterGroupsToRemove_EmptyGroup_ReturnsFalse()
		{
			SetVoiceActors(9, 2, 1);
			GenerateGroups();
			m_testProject.CharacterGroupList.CharacterGroups[0].AssignVoiceActor(m_testProject.VoiceActorList.AllActors[2].Id);
			foreach (var character in m_testProject.CharacterGroupList.CharacterGroups[0].CharacterIds)
				m_testProject.CharacterGroupList.CharacterGroups[1].CharacterIds.Add(character);
			m_testProject.CharacterGroupList.CharacterGroups[0].CharacterIds.Clear();
			var adjuster = new CharacterGroupsAdjuster(m_testProject);
			Assert.That(adjuster.CharactersNotCoveredByAnyGroup, Is.Empty);
			Assert.That(adjuster.CharactersNoLongerInUse, Is.Empty);
			Assert.That(adjuster.CharacterGroupsToRemove, Is.Empty);
			Assert.That(adjuster.NewBooksHaveBeenIncluded, Is.False);
			Assert.That(adjuster.BooksHaveBeenExcluded, Is.False);
			Assert.That(adjuster.FullRegenerateRecommended, Is.False);
			Assert.That(adjuster.GroupsAreNotInSynchWithData, Is.False);
		}

		[Test]
		public void Constructor_CharactersAddedToProject_AdditionsButNoDeletions()
		{
			SetVoiceActors(9, 2, 1);
			GenerateGroups();
			m_testProject.AvailableBooks.Single(b => b.Code == "ACT").IncludeInScript = true;
			m_testProject.ClearCharacterStatistics(); // This simulates behavior in UI when the project is saved after displaying ScriptureRangeSelectionDlg
			var adjuster = new CharacterGroupsAdjuster(m_testProject);
			Assert.That(adjuster.CharactersNotCoveredByAnyGroup,
				Does.Contain(CharacterVerseData.GetStandardCharacterId("ACT", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.That(adjuster.CharactersNotCoveredByAnyGroup,
				Does.Contain(CharacterVerseData.GetStandardCharacterId("ACT", CharacterVerseData.StandardCharacter.ExtraBiblical)));
			Assert.That(adjuster.CharactersNotCoveredByAnyGroup,
				Does.Contain(CharacterVerseData.GetStandardCharacterId("ACT", CharacterVerseData.StandardCharacter.BookOrChapter)));
			Assert.That(adjuster.CharactersNotCoveredByAnyGroup,
				Does.Contain("Gamaliel"));
			Assert.That(adjuster.CharactersNotCoveredByAnyGroup,
				Does.Not.Contain("Jesus"));
			Assert.That(adjuster.CharactersNotCoveredByAnyGroup,
				Does.Not.Contain("Peter (Simon)"));
			Assert.That(adjuster.CharactersNoLongerInUse.Count(), Is.EqualTo(0));
			Assert.That(adjuster.CharacterGroupsToRemove.Count(), Is.EqualTo(0));
			Assert.That(adjuster.NewBooksHaveBeenIncluded, Is.True);
			Assert.That(adjuster.BooksHaveBeenExcluded, Is.False);
			Assert.That(adjuster.FullRegenerateRecommended, Is.True);
			Assert.That(adjuster.GroupsAreNotInSynchWithData, Is.True);
		}

		[Test]
		public void Constructor_CharactersRemovedFromProjectButNotEnoughToResultInAnEmptyGroup_AdditionsButNoDeletions()
		{
			// By keeping the number of actors really low, we guarantee that groups will have lots of characters,
			// thus more-or-less ensuring that no groups will consist only of characters no longer in use after excluding Mark from the
			// project.
			SetVoiceActors(7, 1, 1);
			GenerateGroups();
			m_testProject.AvailableBooks.Single(b => b.Code == "MRK").IncludeInScript = false;
			m_testProject.ClearCharacterStatistics(); // This simulates behavior in UI when the project is saved after displaying ScriptureRangeSelectionDlg
			var adjuster = new CharacterGroupsAdjuster(m_testProject);
			Assert.That(adjuster.CharactersNotCoveredByAnyGroup.Count(), Is.EqualTo(0));
			Assert.That(adjuster.CharactersNoLongerInUse,
				Does.Contain(CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.That(adjuster.CharactersNoLongerInUse,
				Does.Contain(CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.ExtraBiblical)));
			Assert.That(adjuster.CharactersNoLongerInUse,
				Does.Contain(CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.BookOrChapter)));
			Assert.That(adjuster.CharacterGroupsToRemove.Count(), Is.EqualTo(0));
			Assert.That(adjuster.NewBooksHaveBeenIncluded, Is.False);
			Assert.That(adjuster.BooksHaveBeenExcluded, Is.True);
			Assert.That(adjuster.FullRegenerateRecommended, Is.True);
			Assert.That(adjuster.GroupsAreNotInSynchWithData, Is.True);
		}

		[Test]
		public void Constructor_TwoCharactersRenamed_FullRegenerateNotRecommended()
		{
			SetVoiceActors(9, 2, 1);
			m_testProject.AvailableBooks.Single(b => b.Code == "LUK").IncludeInScript = false;
			m_testProject.ClearCharacterStatistics(); // This simulates behavior in UI when the project is saved after displaying ScriptureRangeSelectionDlg
			GenerateGroups();

			try
			{
				RenameCharacter(41, 1, 27, "men in Capernaum synagogue", "dudes in the Capernaum teaching center");
				RenameCharacter(41, 2, 12, "everyone who saw healing of paralytic", "witnesses of paralytic's healing");

				var adjuster = new CharacterGroupsAdjuster(m_testProject);
				Assert.That(adjuster.CharactersNotCoveredByAnyGroup.Count(), Is.EqualTo(2));
				Assert.That(adjuster.CharactersNotCoveredByAnyGroup,
					Does.Contain("dudes in the Capernaum teaching center"));
				Assert.That(adjuster.CharactersNotCoveredByAnyGroup,
					Does.Contain("witnesses of paralytic's healing"));
				Assert.That(adjuster.CharactersNoLongerInUse.Count(), Is.EqualTo(2));
				Assert.That(adjuster.CharactersNoLongerInUse,
					Does.Contain("men in Capernaum synagogue"));
				Assert.That(adjuster.CharactersNoLongerInUse,
					Does.Contain("everyone who saw healing of paralytic"));
				Assert.That(adjuster.CharacterGroupsToRemove.Count(), Is.EqualTo(0));
				Assert.That(adjuster.NewBooksHaveBeenIncluded, Is.False);
				Assert.That(adjuster.BooksHaveBeenExcluded, Is.False);
				Assert.That(adjuster.FullRegenerateRecommended, Is.False);
				Assert.That(adjuster.GroupsAreNotInSynchWithData, Is.True);
			}
			finally
			{
				CreateTestProject();
			}
		}

		[Test]
		public void Constructor_FiveCharactersAddedRemovedOrRenamed_FullRegenerateRecommended()
		{
			SetVoiceActors(9, 2, 1);
			m_testProject.AvailableBooks.Single(b => b.Code == "LUK").IncludeInScript = false;
			m_testProject.ClearCharacterStatistics(); // This simulates behavior in UI when the project is saved after displaying ScriptureRangeSelectionDlg
			GenerateGroups();

			try
			{
				RenameCharacter(41, 1, 27, "men in Capernaum synagogue", "dudes in the Capernaum teaching center");
				RenameCharacter(41, 2, 12, "everyone who saw healing of paralytic", "witnesses of paralytic's healing");
				SetBlockCharacterToNarrator("MRK", 9, 26, "many in crowd");
				SetBlockCharacterToNarrator("MRK", 5, 35, "men from Jairus' house");
				RenameCharacter(41, 7, 1,
					CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.Narrator),
					"Mephibosheth");

				var adjuster = new CharacterGroupsAdjuster(m_testProject);
				Assert.That(adjuster.CharactersNotCoveredByAnyGroup.Count(), Is.EqualTo(3));
				Assert.That(adjuster.CharactersNotCoveredByAnyGroup,
					Does.Contain("dudes in the Capernaum teaching center"));
				Assert.That(adjuster.CharactersNotCoveredByAnyGroup,
					Does.Contain("witnesses of paralytic's healing"));
				Assert.That(adjuster.CharactersNotCoveredByAnyGroup,
					Does.Contain("Mephibosheth"));
				Assert.That(adjuster.CharactersNoLongerInUse.Count(), Is.EqualTo(4));
				Assert.That(adjuster.CharactersNoLongerInUse,
					Does.Contain("men in Capernaum synagogue"));
				Assert.That(adjuster.CharactersNoLongerInUse,
					Does.Contain("everyone who saw healing of paralytic"));
				Assert.That(adjuster.CharactersNoLongerInUse,
					Does.Contain("many in crowd"));
				Assert.That(adjuster.CharactersNoLongerInUse,
					Does.Contain("men from Jairus' house"));
				Assert.That(adjuster.CharacterGroupsToRemove.Count(), Is.EqualTo(0));
				Assert.That(adjuster.NewBooksHaveBeenIncluded, Is.False);
				Assert.That(adjuster.BooksHaveBeenExcluded, Is.False);
				Assert.That(adjuster.FullRegenerateRecommended, Is.True);
				Assert.That(adjuster.GroupsAreNotInSynchWithData, Is.True);
			}
			finally
			{
				CreateTestProject();
			}
		}

		[Test]
		public void Constructor_CharactersRemovedFromProjectLeavingGroupsThatWouldHaveNoCharacters_CharacterGroupsToRemoveNotEmpty()
		{
			// By jacking up the number of actors really high, we guarantee that most characters will end up in a group by themselves,
			// thus more-or-less ensuring that some groups will no longer contain any characters in use after excluding Mark from the
			// project.
			SetVoiceActors(99, 22, 7);
			GenerateGroups();
			m_testProject.AvailableBooks.Single(b => b.Code == "MRK").IncludeInScript = false;
			m_testProject.ClearCharacterStatistics(); // This simulates behavior in UI when the project is saved after displaying ScriptureRangeSelectionDlg
			var adjuster = new CharacterGroupsAdjuster(m_testProject);
			Assert.That(adjuster.CharactersNotCoveredByAnyGroup.Count(), Is.EqualTo(0));
			Assert.That(adjuster.CharactersNoLongerInUse,
				Does.Contain(CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.That(adjuster.CharactersNoLongerInUse,
				Does.Contain(CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.ExtraBiblical)));
			Assert.That(adjuster.CharactersNoLongerInUse,
				Does.Contain(CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.BookOrChapter)));
			Assert.That(adjuster.CharacterGroupsToRemove.Any(), Is.True);
			Assert.That(adjuster.GroupsAreNotInSynchWithData, Is.True);
		}

		[Test]
		public void MakeMinimalAdjustments_FewAdditions_NewGroupAddedWithNewCharacters()
		{
			SetVoiceActors(9, 2, 1);
			GenerateGroups();
			m_testProject.AvailableBooks.Single(b => b.Code == "JUD").IncludeInScript = true;
			m_testProject.ClearCharacterStatistics(); // This simulates behavior in UI when the project is saved after displaying ScriptureRangeSelectionDlg
			var adjuster = new CharacterGroupsAdjuster(m_testProject);
			Assert.That(adjuster.CharactersNotCoveredByAnyGroup,
				Does.Contain(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.That(adjuster.CharactersNotCoveredByAnyGroup,
				Does.Contain(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.ExtraBiblical)));
			Assert.That(adjuster.CharactersNotCoveredByAnyGroup,
				Does.Contain(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.BookOrChapter)));
			var countOfCharactersNotCovered = adjuster.CharactersNotCoveredByAnyGroup.Count();
			Assert.That(adjuster.CharactersNoLongerInUse.Count(), Is.EqualTo(0));
			var originalCountOfGroups = m_testProject.CharacterGroupList.CharacterGroups.Count;
			
			adjuster.MakeMinimalAdjustments();
			Assert.That(adjuster.GroupsAreNotInSynchWithData, Is.False);
			Assert.That(adjuster.CharactersNotCoveredByAnyGroup.Count(), Is.EqualTo(0));
			Assert.That(adjuster.CharactersNoLongerInUse.Count(), Is.EqualTo(0));
			Assert.That(originalCountOfGroups + 1, Is.EqualTo(m_testProject.CharacterGroupList.CharacterGroups.Count));
			var newGroup = m_testProject.CharacterGroupList.GroupContainingCharacterId(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.Narrator));
			Assert.That(m_testProject.CharacterGroupList.CharacterGroups.Any(g => g.GroupId == newGroup.GroupId && g != newGroup), Is.False);
			Assert.That(newGroup.CharacterIds,
				Does.Contain(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.ExtraBiblical)));
			Assert.That(newGroup.CharacterIds,
				Does.Contain(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.BookOrChapter)));
			Assert.That(countOfCharactersNotCovered, Is.EqualTo(newGroup.CharacterIds.Count));
		}

		[Test]
		public void MakeMinimalAdjustments_FewDeletionsAndFewGroups_CharactersRemovedFromExistingCharacterGroups()
		{
			m_testProject.AvailableBooks.Single(b => b.Code == "JUD").IncludeInScript = true;
			m_testProject.ClearCharacterStatistics(); // This simulates behavior in UI when the project is saved after displaying ScriptureRangeSelectionDlg
			SetVoiceActors(20, 2, 1);
			GenerateGroups();
			m_testProject.AvailableBooks.Single(b => b.Code == "JUD").IncludeInScript = false;
			m_testProject.ClearCharacterStatistics(); // This simulates behavior in UI when the project is saved after displaying ScriptureRangeSelectionDlg
			var adjuster = new CharacterGroupsAdjuster(m_testProject);
			var charactersNotInUse = adjuster.CharactersNoLongerInUse.ToList();
			Assert.That(charactersNotInUse.Count, Is.GreaterThan(0));
			Assert.That(charactersNotInUse, Does.Contain(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.That(charactersNotInUse, Does.Contain(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.ExtraBiblical)));
			Assert.That(charactersNotInUse, Does.Contain(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.BookOrChapter)));
			var originalCountOfGroups = m_testProject.CharacterGroupList.CharacterGroups.Count;

			adjuster.MakeMinimalAdjustments();
			Assert.That(adjuster.GroupsAreNotInSynchWithData, Is.False);
			Assert.That(adjuster.CharactersNoLongerInUse.Count(), Is.EqualTo(0));
			Assert.That(adjuster.CharactersNotCoveredByAnyGroup.Count(), Is.EqualTo(0));
			Assert.That(originalCountOfGroups, Is.EqualTo(m_testProject.CharacterGroupList.CharacterGroups.Count));
			Assert.That(m_testProject.CharacterGroupList.CharacterGroups.Any(g => g.CharacterIds.Any(c => charactersNotInUse.Contains(c))), Is.False);
		}

		[Test]
		public void MakeMinimalAdjustments_FewDeletionsAndManyGroups_CharactersRemovedFromExistingCharacterGroupsAndEmptyGroupsRemoved()
		{
			m_testProject.AvailableBooks.Single(b => b.Code == "JUD").IncludeInScript = true;
			m_testProject.ClearCharacterStatistics(); // This simulates behavior in UI when the project is saved after displaying ScriptureRangeSelectionDlg
			SetVoiceActors(100, 7, 2);
			GenerateGroups();
			m_testProject.AvailableBooks.Single(b => b.Code == "JUD").IncludeInScript = false;
			m_testProject.ClearCharacterStatistics(); // This simulates behavior in UI when the project is saved after displaying ScriptureRangeSelectionDlg
			var adjuster = new CharacterGroupsAdjuster(m_testProject);
			var charactersNotInUse = adjuster.CharactersNoLongerInUse.ToList();
			Assert.That(charactersNotInUse.Count, Is.GreaterThan(0));
			Assert.That(charactersNotInUse,
				Does.Contain(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.That(charactersNotInUse,
				Does.Contain(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.ExtraBiblical)));
			Assert.That(charactersNotInUse,
				Does.Contain(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.BookOrChapter)));
			var originalCountOfGroups = m_testProject.CharacterGroupList.CharacterGroups.Count;
			var groupsToRemove = adjuster.CharacterGroupsToRemove.ToList();
			Assert.That(groupsToRemove.Any(), Is.True);

			adjuster.MakeMinimalAdjustments();
			Assert.That(adjuster.GroupsAreNotInSynchWithData, Is.False);
			Assert.That(adjuster.CharactersNoLongerInUse, Is.Empty);
			Assert.That(adjuster.CharactersNotCoveredByAnyGroup, Is.Empty);
			Assert.That(adjuster.CharacterGroupsToRemove, Is.Empty);
			Assert.That(originalCountOfGroups - groupsToRemove.Count,
				Is.EqualTo(m_testProject.CharacterGroupList.CharacterGroups.Count));
			Assert.That(m_testProject.CharacterGroupList.CharacterGroups.Any(g => !g.CharacterIds.Any() ||
				g.CharacterIds.Any(c => charactersNotInUse.Contains(c))), Is.False);
			Assert.That(m_testProject.CharacterGroupList.CharacterGroups
				.Where(g => groupsToRemove.Contains(g)), Is.Empty);
		}

		[Test]
		public void MakeMinimalAdjustments_CameoGroupsWithCharactersNoLongerInUse_EmptyCameoGroupsNotRemoved()
		{
			m_testProject.AvailableBooks.Single(b => b.Code == "JUD").IncludeInScript = true;
			m_testProject.ClearCharacterStatistics(); // This simulates behavior in UI when the project is saved after displaying ScriptureRangeSelectionDlg
			SetVoiceActors(290, 17, 8);
			GenerateGroups();
			var frankie = m_testProject.VoiceActorList.AllActors.First(a => a.Gender == ActorGender.Male);
			frankie.Name = "Frankie";
			frankie.IsCameo = true;
			var michaelTheArchAngelGroup = m_testProject.CharacterGroupList.GroupContainingCharacterId("Michael, archangel");
			michaelTheArchAngelGroup.AssignVoiceActor(frankie.Id);
			m_testProject.AvailableBooks.Single(b => b.Code == "JUD").IncludeInScript = false;
			m_testProject.ClearCharacterStatistics(); // This simulates behavior in UI when the project is saved after displaying ScriptureRangeSelectionDlg
			var adjuster = new CharacterGroupsAdjuster(m_testProject);
			var charactersNotInUse = adjuster.CharactersNoLongerInUse.ToList();
			Assert.That(charactersNotInUse, Does.Contain("Michael, archangel"));
			var originalCountOfGroups = m_testProject.CharacterGroupList.CharacterGroups.Count;
			var groupsToRemove = adjuster.CharacterGroupsToRemove.ToList();
			Assert.That(groupsToRemove.Count, Is.EqualTo(1));
			Assert.That(groupsToRemove.Contains(michaelTheArchAngelGroup), Is.False);

			adjuster.MakeMinimalAdjustments();
			Assert.That(adjuster.GroupsAreNotInSynchWithData, Is.False);
			Assert.That(adjuster.CharactersNoLongerInUse, Is.Empty);
			Assert.That(adjuster.CharactersNotCoveredByAnyGroup, Is.Empty);
			Assert.That(adjuster.CharacterGroupsToRemove, Is.Empty);
			Assert.That(originalCountOfGroups - groupsToRemove.Count,
				Is.EqualTo(m_testProject.CharacterGroupList.CharacterGroups.Count));
			Assert.That(m_testProject.CharacterGroupList.CharacterGroups.Single(
					g => !g.CharacterIds.Any() || g.CharacterIds.Any(c => charactersNotInUse.Contains(c))),
				Is.EqualTo(michaelTheArchAngelGroup));
			Assert.That(m_testProject.CharacterGroupList.CharacterGroups
				.Where(g => groupsToRemove.Contains(g)), Is.Empty);
			Assert.That(michaelTheArchAngelGroup.CharacterIds, Is.Empty);
			Assert.That(frankie.Id, Is.EqualTo(michaelTheArchAngelGroup.VoiceActorId));
		}
	}
}
