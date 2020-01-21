using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Glyssen.Dialogs;
using Glyssen.Rules;
using GlyssenEngine;
using GlyssenEngine.Character;
using GlyssenEngine.VoiceActor;
using GlyssenEngineTests;
using NUnit.Framework;
using SIL.Extensions;
using Resources = GlyssenTests.Properties.Resources;

namespace GlyssenTests.Dialogs
{
	[TestFixture]
	class VoiceActorAssignmentViewModelTests
	{
		private Project m_testProject;
		private VoiceActorAssignmentViewModel m_model;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerse;
			CharacterDetailData.TabDelimitedCharacterDetailData = Resources.TestCharacterDetail;
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.MRK);
		}

		[SetUp]
		public void SetUp()
		{
			m_testProject.VoiceActorList.AllActors.Clear();
			m_testProject.CharacterGroupList.CharacterGroups.Clear();
			// Adding one group here prevents the constructor from generating groups
			AddNewGroup("John"); // Need a character in the group, otherwise it is treated as a Cameo group and it's not legal to assign/unassign actor.
			m_model = new VoiceActorAssignmentViewModel(m_testProject);
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			TestProject.DeleteTestProjectFolder();
		}

		[Test]
		public void AssignActorToGroup_CanAssignTrue_ActorAssigned()
		{
			var actor1 = new GlyssenEngine.VoiceActor.VoiceActor { Id = 1, Name = "Puni Upalari" };
			m_testProject.VoiceActorList.AllActors = new List<GlyssenEngine.VoiceActor.VoiceActor> { actor1 };
			var group = m_model.CharacterGroups[0];
			m_model.AssignActorToGroup(actor1.Id, group);
			Assert.AreEqual(actor1.Id, group.VoiceActorId);
			var undoDescriptions = m_model.UndoActions;
			Assert.AreEqual(1, undoDescriptions.Count);
			Assert.AreEqual("Assign voice actor Puni Upalari", undoDescriptions[0]);
		}

		[Test]
		public void AssignActorToGroup_AssignedToSameActor_NoChangeAndNoUndoAction()
		{
			var actor1 = new GlyssenEngine.VoiceActor.VoiceActor { Id = 1, Name = "Puni Upalari" };
			m_testProject.VoiceActorList.AllActors = new List<GlyssenEngine.VoiceActor.VoiceActor> { actor1 };
			var group = m_model.CharacterGroups[0];
			group.VoiceActorId = actor1.Id;
			m_model.AssignActorToGroup(actor1.Id, group);
			Assert.AreEqual(actor1.Id, group.VoiceActorId);
			Assert.AreEqual(0, m_model.UndoActions.Count);
		}

		[Test]
		public void AssignActorToGroup_ExistingGroupAssignedToActor_ActorUnassignedFromPreviousGroupAndAssignedToRequestedGroup()
		{
			var actor1 = new GlyssenEngine.VoiceActor.VoiceActor { Id = 1, Name = "Eduardo Lopez" };
			m_testProject.VoiceActorList.AllActors.Add(actor1);
			var existingGroup = m_model.CharacterGroups[0];
			existingGroup.VoiceActorId = 1;
			var newGroup = AddNewGroup();
//			newGroup.Name = "New group";
			m_model.AssignActorToGroup(1, newGroup);
			Assert.AreEqual(1, newGroup.VoiceActorId);
			Assert.AreEqual(1, m_model.UndoActions.Count);
			Assert.AreEqual("Assign voice actor Eduardo Lopez", m_model.UndoActions[0]);
			Assert.False(existingGroup.IsVoiceActorAssigned);
		}

		[Test]
		public void Undo_AssignActorToGroup_ExistingGroupAssignedToActor_ActorUnassignedFromAssignedGroupAndReassignedToPreviousGroup()
		{
			var actor1 = new GlyssenEngine.VoiceActor.VoiceActor { Id = 1, Name = "Eduardo Lopez" };
			m_testProject.VoiceActorList.AllActors.Add(actor1);
			var existingGroup = m_model.CharacterGroups[0];
			existingGroup.VoiceActorId = 1;
			var newGroup = AddNewGroup();
//			newGroup.Name = "New group";
			m_model.AssignActorToGroup(1, newGroup);
			List<CharacterGroup> affectedGroups = null;
			m_model.Saved += (sender, affected, f) => { affectedGroups = affected.ToList(); Assert.IsFalse(f);};
			Assert.IsTrue(m_model.Undo());
			Assert.AreEqual(0, m_model.UndoActions.Count);
			Assert.AreEqual(1, m_model.RedoActions.Count);
			Assert.AreEqual("Assign voice actor Eduardo Lopez", m_model.RedoActions[0]);
			Assert.False(newGroup.IsVoiceActorAssigned);
			Assert.AreEqual(1, existingGroup.VoiceActorId);
			Assert.IsTrue(affectedGroups.SequenceEqual(new [] {existingGroup, newGroup }));
		}

		[Test]
		public void UnAssignActorFromGroups_ByGroup()
		{
			var actor1 = new GlyssenEngine.VoiceActor.VoiceActor { Id = 1, Name = "Marco Polo" };
			m_testProject.VoiceActorList.AllActors.Add(actor1);
			var actor2 = new GlyssenEngine.VoiceActor.VoiceActor { Id = 2, Name = "Wilbur Wright" };
			m_testProject.VoiceActorList.AllActors.Add(actor2);
			var group1 = m_model.CharacterGroups[0];
			group1.SetGroupIdLabel();
			m_model.AssignActorToGroup(actor2.Id, group1);
			Assert.True(group1.IsVoiceActorAssigned);
			var group2 = AddNewGroup("Nicodemus");
			m_model.AssignActorToGroup(actor1.Id, group2);
			Assert.True(group2.IsVoiceActorAssigned);
			Assert.AreEqual(2, m_model.UndoActions.Count);

			m_model.UnAssignActorFromGroups(new List<CharacterGroup> { group1, group2 });
			Assert.False(group1.IsVoiceActorAssigned);
			Assert.False(group2.IsVoiceActorAssigned);
			Assert.AreEqual(3, m_model.UndoActions.Count);
		}

		[Test]
		public void UnAssignActorFromGroups_GroupsContainsCameoGroup_CameoGroupNotUnassigned()
		{
			var actor1 = new GlyssenEngine.VoiceActor.VoiceActor { Id = 1, Name = "Marco Polo", IsCameo = true };
			m_testProject.VoiceActorList.AllActors.Add(actor1);
			var actor2 = new GlyssenEngine.VoiceActor.VoiceActor { Id = 2, Name = "Wilbur Wright" };
			m_testProject.VoiceActorList.AllActors.Add(actor2);
			var group1 = m_model.CharacterGroups[0];
			m_model.AssignActorToGroup(actor2.Id, group1);
			Assert.True(group1.IsVoiceActorAssigned);
			var cameoGroup = AddNewGroup(); // No characters => cameo
			m_model.AssignActorToGroup(actor1.Id, cameoGroup);
			Assert.True(cameoGroup.IsVoiceActorAssigned);
			Assert.AreEqual(2, m_model.UndoActions.Count);

			m_model.UnAssignActorFromGroups(new List<CharacterGroup> { group1, cameoGroup });
			Assert.False(group1.IsVoiceActorAssigned);
			Assert.True(cameoGroup.IsVoiceActorAssigned);
			Assert.AreEqual(3, m_model.UndoActions.Count);
		}

		[Test]
		public void CanMoveCharactersToGroup_UnexpectedCharacterId_ReturnsFalse()
		{
			Assert.IsFalse(m_model.CanMoveCharactersToGroup(new[] { "Hairy Man of Hinkley" }, null));
		}

		[Test]
		public void CanMoveCharactersToGroup_NoDestGroupSpecifiedNotAllCharactersInSourceAreIncluded_ReturnsTrue()
		{
			var sourceGroup = m_model.CharacterGroups[0];
			sourceGroup.CharacterIds = new CharacterIdHashSet(new[] { "John", "Andrew", "Moses" });

			Assert.IsTrue(m_model.CanMoveCharactersToGroup(new List<string> { "John", "Moses" }, null));
		}

		[Test]
		public void CanMoveCharactersToGroup_NoDestGroupSpecifiedAllCharactersInUnassignedSourceAreIncluded_ReturnsFalse()
		{
			var sourceGroup = m_model.CharacterGroups[0];
			sourceGroup.CharacterIds = new CharacterIdHashSet(new[] { "John", "Andrew", "Moses" });

			Assert.IsFalse(m_model.CanMoveCharactersToGroup(new List<string> { "Andrew", "John", "Moses" }, null));
		}

		[Test]
		public void CanMoveCharactersToGroup_NoDestGroupSpecifiedAllCharactersInAssignedSourceAreIncluded_ReturnsTrue()
		{
			var sourceGroup = m_model.CharacterGroups[0];
			sourceGroup.CharacterIds = new CharacterIdHashSet(new[] { "John", "Andrew", "Moses" });
			sourceGroup.VoiceActorId = 56;

			Assert.IsTrue(m_model.CanMoveCharactersToGroup(new List<string> { "Andrew", "John", "Moses" }, null));
		}

		[Test]
		public void CanMoveCharactersToGroup_NoDestGroupSpecifiedAllCharactersInCameoSourceAreIncluded_ReturnsTrue()
		{
			var sourceGroup = m_model.CharacterGroups[0];
			sourceGroup.CharacterIds = new CharacterIdHashSet(new[] { "John", "Andrew", "Moses" });

			Assert.IsFalse(m_model.CanMoveCharactersToGroup(new List<string> { "Andrew", "John", "Moses" }, null));
		}

		[Test]
		public void MoveCharactersToGroup_NoCharacterProvided_ThrowsArgumentException()
		{
			var destGroup = m_model.CharacterGroups[0];
			Assert.Throws<ArgumentException>(() => m_model.MoveCharactersToGroup(new List<string>(), destGroup));
		}

		[Test]
		public void MoveCharactersToGroup_UnexpectedCharacterId_ReturnsFalse()
		{
			Assert.IsFalse(m_model.MoveCharactersToGroup(new[] { "Hairy Man of Hinkley" }, null));
			Assert.AreEqual(0, m_model.UndoActions.Count);
		}

		[Test]
		public void MoveCharactersToGroup_NoDestGroupSpecified_CharactersMovedFromOriginalGroupToNewGroup()
		{
			var sourceGroup = m_model.CharacterGroups[0];
			sourceGroup.CharacterIds = new CharacterIdHashSet(new[] { "John", "Andrew", "Moses" });

			List<CharacterGroup> affectedGroups = null;
			m_model.Saved += (sender, affected, f) => { affectedGroups = affected.ToList(); Assert.IsFalse(f);};
			Assert.IsTrue(m_model.MoveCharactersToGroup(new List<string> { "John", "Moses" }, null));
			var newGroup = m_testProject.CharacterGroupList.GroupContainingCharacterId("Moses");
			Assert.IsNotNull(newGroup);
			Assert.IsTrue(affectedGroups.SequenceEqual(new[] { newGroup, sourceGroup }));

			Assert.AreEqual(2, newGroup.CharacterIds.Count);
			Assert.True(newGroup.CharacterIds.Contains("John"));
			Assert.AreEqual(1, sourceGroup.CharacterIds.Count);
			Assert.True(sourceGroup.CharacterIds.Contains("Andrew"));
			Assert.AreEqual("Create new group", m_model.UndoActions.Single());
		}

		[Test]
		public void MoveCharactersToGroup_NoDestGroupSpecifiedAllCharactersInAssignedSourceGroup_CharactersMovedToNewGroupButSourceGroupNotRemoved()
		{
			var allThreeCharacters = new List<string>(new[] {"John", "Andrew", "Moses"});
			var sourceGroup = m_model.CharacterGroups[0];
			sourceGroup.CharacterIds = new CharacterIdHashSet(allThreeCharacters);
			sourceGroup.VoiceActorId = 5;

			List<CharacterGroup> affectedGroups = null;
			m_model.Saved += (sender, affected, f) => { affectedGroups = affected.ToList(); Assert.IsFalse(f);};
			Assert.IsTrue(m_model.MoveCharactersToGroup(allThreeCharacters, null));

			var newGroup = m_testProject.CharacterGroupList.GroupContainingCharacterId("Moses");
			Assert.IsNotNull(newGroup);
			Assert.AreNotEqual(sourceGroup, newGroup);
			Assert.IsTrue(affectedGroups.SequenceEqual(new[] { newGroup, sourceGroup }));
			Assert.IsTrue(newGroup.CharacterIds.SetEquals(allThreeCharacters));
			Assert.AreEqual(0, sourceGroup.CharacterIds.Count);
			Assert.AreEqual("Create new group", m_model.UndoActions.Single());
		}

		[Test]
		public void MoveCharactersToGroup_DestinationContainsCharacters()
		{
			var characterIds = new List<string> { "John", "Andrew" };
			var sourceGroup = m_model.CharacterGroups[0];
			sourceGroup.CharacterIds = new CharacterIdHashSet(characterIds);
			sourceGroup.CharacterIds.Add("Lot");
			sourceGroup.CharacterIds.Add("Cain");
			sourceGroup.SetGroupIdLabel();
			var destGroup = AddNewGroup("foot", "ear");

			List<CharacterGroup> affectedGroups = null;
			m_model.Saved += (sender, affected, f) => { affectedGroups = affected.ToList(); Assert.IsFalse(f);};
			Assert.True(m_model.MoveCharactersToGroup(characterIds, destGroup));

			Assert.IsTrue(affectedGroups.SequenceEqual(new[] { destGroup, sourceGroup }));
			var destGroupCharacterIds = destGroup.CharacterIds;
			Assert.AreEqual(4, destGroup.CharacterIds.Count);
			Assert.True(destGroupCharacterIds.Contains("John"));
			Assert.True(destGroupCharacterIds.Contains("Andrew"));
			Assert.AreEqual("Move characters to Man 2 group", m_model.UndoActions.Single());
		}

		[Test]
		public void MoveCharactersToGroup_SourceGroupSameAsDestinationGroup_NoActionTaken()
		{
			var characterIds = new List<string> { "John", "Andrew" };
			var sourceGroup = m_model.CharacterGroups[0];
			sourceGroup.CharacterIds = new CharacterIdHashSet(characterIds);

			List<CharacterGroup> affectedGroups = null;
			m_model.Saved += (sender, affected, f) => { affectedGroups = affected.ToList(); Assert.IsFalse(f);};
			Assert.False(m_model.MoveCharactersToGroup(characterIds, sourceGroup));
			Assert.IsNull(affectedGroups);
			Assert.AreEqual(0, m_model.UndoActions.Count);
		}

		[Test]
		public void MoveCharactersToGroup_AllCharactersMovedOutOfSource_SourceGroupRemoved()
		{
			var characterIds = new List<string> { "John", "Andrew" };
			var sourceGroup = m_model.CharacterGroups[0];
			sourceGroup.CharacterIds = new CharacterIdHashSet(characterIds);
			sourceGroup.SetGroupIdLabel();
			var destGroup = AddNewGroup("ear", "foot");
			Assert.AreEqual(2, m_testProject.CharacterGroupList.CharacterGroups.Count);

			List<CharacterGroup> affectedGroups = null;
			m_model.Saved += (sender, affected, f) => { affectedGroups = affected.ToList(); Assert.IsFalse(f);};
			Assert.True(m_model.MoveCharactersToGroup(characterIds, destGroup));
			Assert.AreEqual(destGroup, affectedGroups.Single());
			Assert.AreEqual(1, m_testProject.CharacterGroupList.CharacterGroups.Count);
			Assert.IsTrue(destGroup.CharacterIds.SetEquals(new[] { "John", "Andrew", "ear", "foot" }));
			Assert.AreEqual("Move characters to Man 2 group", m_model.UndoActions.Single());
		}

		[Test]
		public void RegenerateGroups()
		{
			m_testProject.CharacterGroupList.CharacterGroups.Clear();
			var actor1 = new GlyssenEngine.VoiceActor.VoiceActor { Id = 1 };
			var actor2 = new GlyssenEngine.VoiceActor.VoiceActor { Id = 2, Gender = ActorGender.Female};
			var actor3 = new GlyssenEngine.VoiceActor.VoiceActor { Id = 3 };
			m_testProject.VoiceActorList.AllActors = new List<GlyssenEngine.VoiceActor.VoiceActor> { actor1, actor2, actor3 };
			List<CharacterGroup> affectedGroups = null;
			m_model.Saved += (sender, affected, f) => { if (affected != null) affectedGroups = affected.ToList(); Assert.IsFalse(f);};
			m_model.RegenerateGroups(() =>
			{
				// This test is just testing the behavior of "RegenerateGroups" which just runs the action given to it.
				// So rather than actually instantiating a CharacterGroupGenerator and doing all that work, we jst
				// test the behavior using a simple approach that will create a group and stick something on the undo stack.
				var group1 = new CharacterGroup(m_testProject) { CharacterIds = new CharacterIdHashSet(new[] { "Martha" }) };
				var group2 = new CharacterGroup(m_testProject) { CharacterIds = new CharacterIdHashSet(new[] { "Jesus" }) };
				var group3 = new CharacterGroup(m_testProject) { CharacterIds = new CharacterIdHashSet(new[] { "NAR-MAT" }) };
				m_testProject.CharacterGroupList.CharacterGroups.AddRange(new[] { group1, group2, group3 });
				m_model.AssignActorToGroup(2, group1);
			});
			Assert.AreEqual(3, m_testProject.CharacterGroupList.CharacterGroups.Count);
			// TODO (PG-437): Check that correct groups are included in affectedGroups.
			// For now, just check based on the undo item that "AssignActorToGroup" should put there.
			Assert.IsTrue(affectedGroups.SetEquals(m_testProject.CharacterGroupList.GetGroupsAssignedToActor(2)));
		}

		[Test]
		public void SplitGroup_MoveOne()
		{
			var characterIds = new List<string> { "John", "Andrew" };
			var existingGroup = m_model.CharacterGroups[0];
			existingGroup.CharacterIds = new CharacterIdHashSet(characterIds);

			Assert.AreEqual(1, m_testProject.CharacterGroupList.CharacterGroups.Count);
			var newGroup = m_model.SplitGroup(new List<string> {"John"});
			Assert.IsNotNull(newGroup);

			Assert.AreEqual(2, m_testProject.CharacterGroupList.CharacterGroups.Count);

			Assert.False(existingGroup == newGroup);
			Assert.True(existingGroup.CharacterIds.Contains("Andrew"));
			Assert.False(existingGroup.CharacterIds.Contains("John"));
			Assert.True(newGroup.CharacterIds.Contains("John"));
			Assert.AreEqual("Split group", m_model.UndoActions.Single());
		}

		[Test]
		public void SplitGroup_MoveMultiple()
		{
			var characterIds = new List<string> { "John", "Andrew", "Peter" };
			var existingGroup = m_model.CharacterGroups[0];
			existingGroup.CharacterIds = new CharacterIdHashSet(characterIds);

			Assert.AreEqual(1, m_testProject.CharacterGroupList.CharacterGroups.Count);

			var newGroup = m_model.SplitGroup(new List<string> {"John", "Peter"});
			Assert.IsNotNull(newGroup);

			Assert.AreEqual(2, m_testProject.CharacterGroupList.CharacterGroups.Count);

			Assert.False(existingGroup == newGroup);
			Assert.True(existingGroup.CharacterIds.Contains("Andrew"));
			Assert.False(existingGroup.CharacterIds.Contains("John"));
			Assert.False(existingGroup.CharacterIds.Contains("Peter"));
			Assert.True(newGroup.CharacterIds.Contains("John"));
			Assert.True(newGroup.CharacterIds.Contains("Peter"));
			Assert.AreEqual("Split group", m_model.UndoActions.Single());
		}

		[Test]
		public void GetMultiColumnActorDataTable_NoActorsAssigned_GetsAllActorsInAlphbeticalOrder()
		{
			var actorB = new GlyssenEngine.VoiceActor.VoiceActor { Id = 1, Name = "B" };
			var actorC = new GlyssenEngine.VoiceActor.VoiceActor { Id = 2, Name = "C" };
			var actorA = new GlyssenEngine.VoiceActor.VoiceActor { Id = 3, Name = "A" };
			m_testProject.VoiceActorList.AllActors = new List<GlyssenEngine.VoiceActor.VoiceActor> { actorB, actorC, actorA };
			var generator = new CharacterGroupGenerator(m_testProject);
			generator.GenerateCharacterGroups();
			generator.ApplyGeneratedGroupsToProject(false);

			var dataTable = m_model.GetMultiColumnActorDataTable(m_model.CharacterGroups[0]);
			var actorList = GetActorListFromDataTable(dataTable);
			Assert.AreEqual(actorA, actorList[0]);
			Assert.AreEqual(actorB, actorList[1]);
			Assert.AreEqual(actorC, actorList[2]);
			Assert.AreEqual(null, actorList[3]); // The "Unassigned" option
		}

		[Test]
		public void GetMultiColumnActorDataTable_ActorAssigned_AssignedActorSortsLast()
		{
			var actorB = new GlyssenEngine.VoiceActor.VoiceActor { Id = 1, Name = "B" };
			var actorC = new GlyssenEngine.VoiceActor.VoiceActor { Id = 2, Name = "C" };
			var actorA = new GlyssenEngine.VoiceActor.VoiceActor { Id = 3, Name = "A" };
			m_testProject.VoiceActorList.AllActors = new List<GlyssenEngine.VoiceActor.VoiceActor> { actorB, actorC, actorA };
			var generator = new CharacterGroupGenerator(m_testProject);
			generator.GenerateCharacterGroups();
			generator.ApplyGeneratedGroupsToProject(false);
			var group = m_model.CharacterGroups[0];
			m_model.AssignActorToGroup(actorA.Id, group);

			var dataTable = m_model.GetMultiColumnActorDataTable(m_model.CharacterGroups[0]);
			var actorList = GetActorListFromDataTable(dataTable);
			Assert.AreEqual(actorB, actorList[0]);
			Assert.AreEqual(actorC, actorList[1]);
			Assert.AreEqual(null, actorList[2]); // The "Unassigned" option
			Assert.AreEqual(actorA, actorList[3]);
		}

		[Test]
		public void NoteActorChanges_NoChanges_DoNothing()
		{
			Assert.AreEqual(0, m_model.UndoActions.Count);
			List<CharacterGroup> affectedGroups = null;
			m_model.Saved += (sender, affected, f) => { affectedGroups = affected.ToList(); Assert.IsFalse(f);};
			m_model.NoteActorChanges(new IVoiceActorUndoAction[0]);
			Assert.AreEqual(0, m_model.UndoActions.Count);
			Assert.IsNull(affectedGroups);
		}

		[Test]
		public void NoteActorChanges_SingleChangeToUnassignedActor_NewUndoActionAddedWithDescriptionBasedOnSingleChange_NoGroupsAffected()
		{
			var affectedActor = new GlyssenEngine.VoiceActor.VoiceActor { Id = 1, Name = "B", Age = ActorAge.Adult };
			var replacedActor = new GlyssenEngine.VoiceActor.VoiceActor { Id = 1, Name = "B", Age = ActorAge.YoungAdult };
			m_testProject.VoiceActorList.AllActors = new List<GlyssenEngine.VoiceActor.VoiceActor> { affectedActor };
			Assert.AreEqual(0, m_model.UndoActions.Count);
			List<CharacterGroup> affectedGroups = null;
			m_model.Saved += (sender, affected, f) => { affectedGroups = affected.ToList(); Assert.IsTrue(f); };
			m_model.NoteActorChanges(new IVoiceActorUndoAction[] { new VoiceActorEditUndoAction(m_testProject, replacedActor) });
			Assert.AreEqual("Edit voice actor B", m_model.UndoActions.Single());
			Assert.AreEqual(0, affectedGroups.Count);
		}

		[Test]
		public void NoteActorChanges_MultipleChanges_NewUndoActionAddedWithGeneralDescription_GroupsAffected()
		{
			var replacedActor = new GlyssenEngine.VoiceActor.VoiceActor { Id = 1, Name = "B", Age = ActorAge.YoungAdult };
			m_testProject.VoiceActorList.AllActors.Add(replacedActor);
			var characterGroup = new CharacterGroup(m_testProject);
			m_testProject.CharacterGroupList.CharacterGroups.Add(characterGroup);
			characterGroup.AssignVoiceActor(1);
			var affectedActor = new GlyssenEngine.VoiceActor.VoiceActor { Id = 1, Name = "B", Age = ActorAge.Adult };
			var deletedActor = new GlyssenEngine.VoiceActor.VoiceActor { Id = 2, Name = "C" };
			m_testProject.VoiceActorList.AllActors = new List<GlyssenEngine.VoiceActor.VoiceActor> { affectedActor };
			Assert.AreEqual(0, m_model.UndoActions.Count);
			List<CharacterGroup> affectedGroups = null;
			m_model.Saved += (sender, affected, f) => { affectedGroups = affected.ToList(); Assert.IsTrue(f);};
			m_model.NoteActorChanges(new IVoiceActorUndoAction[]
			{
				new VoiceActorEditUndoAction(m_testProject, replacedActor),
				new VoiceActorDeletedUndoAction(m_testProject, deletedActor)
			});
			Assert.AreEqual("Edit voice actors", m_model.UndoActions.Single());
			Assert.AreEqual(characterGroup, affectedGroups.Single());
		}

		// Started writing this test because I thought group assignments were not being cleared at all. When I realized that
		// that was not the problem, I abandoned this. It might still be a nice enhancement someday.
		[Test]
		[Ignore("Low priority feature")]
		public void NoteActorChanges_AssignedActorsDeleted_SubUndoActionsAddedToEnableReassignments()
		{
		//	var deletedActor1 = new GlyssenEngine.VoiceActor.VoiceActor { Id = 1, Name = "Adam", Age = ActorAge.YoungAdult };
		//	m_testProject.VoiceActorList.AllActors.Add(deletedActor1);
		//	var characterGroupA1 = new CharacterGroup(m_testProject);
		//	m_testProject.CharacterGroupList.CharacterGroups.Add(characterGroupA1);
		//	characterGroupA1.AssignVoiceActor(deletedActor1.Id);

			//	var unchangedActor = new GlyssenEngine.VoiceActor.VoiceActor { Id = 2, Name = "Baker", Age = ActorAge.Adult };
			//	m_testProject.VoiceActorList.AllActors.Add(unchangedActor);
			//	var characterGroupA2 = new CharacterGroup(m_testProject);
			//	m_testProject.CharacterGroupList.CharacterGroups.Add(characterGroupA2);
			//	characterGroupA2.AssignVoiceActor(unchangedActor.Id);

			//	var deletedActor2 = new GlyssenEngine.VoiceActor.VoiceActor { Id = 3, Name = "Charlie", Age = ActorAge.Elder };
			//	m_testProject.VoiceActorList.AllActors.Add(deletedActor2);
			//	var characterGroupA3 = new CharacterGroup(m_testProject);
			//	m_testProject.CharacterGroupList.CharacterGroups.Add(characterGroupA3);
			//	characterGroupA3.AssignVoiceActor(deletedActor2.Id);

			//	var modifiedActor = new GlyssenEngine.VoiceActor.VoiceActor { Id = 1, Name = "B", Age = ActorAge.YoungAdult };
			//	m_testProject.VoiceActorList.AllActors.Add(deletedActor1);
			//	var characterGroup = new CharacterGroup(m_testProject);
			//	m_testProject.CharacterGroupList.CharacterGroups.Add(characterGroup);
			//	characterGroup.AssignVoiceActor(1);


			//	var unchangedActor = new GlyssenEngine.VoiceActor.VoiceActor { Id = 1, Name = "B", Age = ActorAge.Adult };
			//	var deletedActor = new GlyssenEngine.VoiceActor.VoiceActor { Id = 2, Name = "C" };
			//	m_testProject.VoiceActorList.AllActors = new List<Glyssen.VoiceActor.VoiceActor> { unchangedActor, modifiedActor };
			//	Assert.AreEqual(0, m_model.UndoActions.Count);
			//	List<CharacterGroup> affectedGroups = null;
			//	m_model.Saved += (sender, args) => { affectedGroups = args.ToList(); };
			//	m_model.NoteActorChanges(new IVoiceActorUndoAction[]
			//	{
			//		new VoiceActorEditUndoAction(m_testProject, replacedActor),
			//		new VoiceActorDeletedUndoAction(m_testProject, deletedActor)
			//	});
			//	Assert.AreEqual("Edit voice actors", m_model.UndoActions.Single());
			//	Assert.AreEqual(characterGroup, affectedGroups.Single());
		}

		[Test]
		public void FindNextMatchingCharacter_InvalidTextToFindArgument_ThrowsAppropriateArgumentException()
		{
			Assert.Throws<ArgumentNullException>(() => m_model.FindNextMatchingCharacter(null, 0, 0));
			Assert.Throws<ArgumentException>(() => m_model.FindNextMatchingCharacter("    ", 0, 0));
			Assert.Throws<ArgumentException>(() => m_model.FindNextMatchingCharacter("A", 0, 0));
		}

		[Test]
		public void FindNextMatchingCharacter_NoMatches_ReturnsNegativeOneNegativeOne()
		{
			var result = m_model.FindNextMatchingCharacter("boogey-man", 0, 0);
			Assert.AreEqual(-1, result.Item1);
			Assert.AreEqual(-1, result.Item2);
		}

		[Test]
		public void FindNextMatchingCharacter_ExactMatchWithOnlyCharacterInFirstGroup_ReturnsZeroZero()
		{
			var result = m_model.FindNextMatchingCharacter("John", 0, 0);
			Assert.AreEqual(0, result.Item1);
			Assert.AreEqual(0, result.Item2);
		}

		[Test]
		public void FindNextMatchingCharacter_NegativeIndex_SearchStartsAtBeginning()
		{
			var result = m_model.FindNextMatchingCharacter("John", -1, -1);
			Assert.AreEqual(0, result.Item1);
			Assert.AreEqual(0, result.Item2);
		}

		[Test]
		public void FindNextMatchingCharacter_IndexOutOfRange_SearchStartsAtBeginning()
		{
			var result = m_model.FindNextMatchingCharacter("John", 40, 40);
			Assert.AreEqual(0, result.Item1);
			Assert.AreEqual(0, result.Item2);
		}

		[Test]
		public void FindNextMatchingCharacter_PartialMatchWithCharacterInSubsequentGroup_ReturnsMatchIndices()
		{
			AddNewGroup("chief priests", "demon-possessed man", "Judas");
			AddNewGroup("Aaron", "John the Baptist", "Joshua");
			AddNewGroup("Martha", "Rhoda");
			var result = m_model.FindNextMatchingCharacter("John", 0, 0);
			Assert.AreEqual(2, result.Item1);
			Assert.AreEqual(1, result.Item2);
		}

		[Test]
		public void FindNextMatchingCharacter_PartialMatchWrapAround_ReturnsMatchIndices()
		{
			AddNewGroup("Amnon, son of David", "man delivered from Legion of demons", "Judas");
			AddNewGroup("Aaron", "John the Baptist", "Joshua");
			AddNewGroup("Martha", "Miriam", "Rhoda");
			var result = m_model.FindNextMatchingCharacter("am", 3, 1);
			Assert.AreEqual(1, result.Item1);
			Assert.AreEqual(0, result.Item2);
			result = m_model.FindNextMatchingCharacter("am", result.Item1, result.Item2);
			Assert.AreEqual(3, result.Item1);
			Assert.AreEqual(1, result.Item2);
		}

		[Test]
		public void FindNextMatchingCharacter_PartialMatchWithOnlyCharacterInFirstGroup_ReturnsZeroZero()
		{
			var result = m_model.FindNextMatchingCharacter("Joh", 0, 0);
			Assert.AreEqual(0, result.Item1);
			Assert.AreEqual(0, result.Item2);
		}

		[Test]
		public void AddNewActorToGroup_OneNewActor_UniqueActorIdAssignedToGroup()
		{
			var group = AddNewGroup("Amnon, son of David", "man delivered from Legion of demons", "Judas");
			var newActor = m_model.AddNewActorToGroup("Friedrich", group);
			Assert.AreEqual("Friedrich", newActor.Name);
			Assert.AreEqual(0, newActor.Id);
			Assert.That(m_testProject.VoiceActorList.ActiveActors.Contains(newActor));
			Assert.AreEqual(newActor, group.VoiceActor);
			Assert.AreEqual(ActorGender.Male, newActor.Gender);
			Assert.AreEqual(ActorAge.Adult, newActor.Age);
		}

		[Test]
		public void AddNewActorToGroup_ThreeNewActors_UniqueActorIdsAssignedToCorrectGroups()
		{
			var group1 = AddNewGroup("Amnon, son of David", "man delivered from Legion of demons", "Judas");
			var group2 = AddNewGroup("Rhoda");
			var group3 = AddNewGroup("children");
			var newActor1 = m_model.AddNewActorToGroup("Friedrich", group1);
			var newActor2 = m_model.AddNewActorToGroup("Wallace", group2);
			var newActor3 = m_model.AddNewActorToGroup("Grommit", group3);
			Assert.AreEqual("Friedrich", newActor1.Name);
			Assert.AreEqual(0, newActor1.Id);
			Assert.That(m_testProject.VoiceActorList.ActiveActors.Contains(newActor1));
			Assert.AreEqual(newActor1, group1.VoiceActor);
			Assert.AreEqual(ActorGender.Male, newActor1.Gender);
			Assert.AreEqual(ActorAge.Adult, newActor1.Age);
			Assert.AreEqual("Wallace", newActor2.Name);
			Assert.AreEqual(1, newActor2.Id);
			Assert.That(m_testProject.VoiceActorList.ActiveActors.Contains(newActor2));
			Assert.AreEqual(newActor2, group2.VoiceActor);
			Assert.AreEqual(ActorGender.Female, newActor2.Gender);
			Assert.AreEqual(ActorAge.Adult, newActor2.Age);
			Assert.AreEqual("Grommit", newActor3.Name);
			Assert.AreEqual(2, newActor3.Id);
			Assert.That(m_testProject.VoiceActorList.ActiveActors.Contains(newActor3));
			Assert.AreEqual(newActor3, group3.VoiceActor);
			Assert.AreEqual(ActorGender.Male, newActor3.Gender);
			Assert.AreEqual(ActorAge.Child, newActor3.Age);
		}

		private List<GlyssenEngine.VoiceActor.VoiceActor> GetActorListFromDataTable(DataTable dataTable)
		{
			return (from DataRow row in dataTable.Rows select m_testProject.VoiceActorList.GetVoiceActorById((int)row.ItemArray[0])).ToList();
		}

		private CharacterGroup AddNewGroup(params string[] characterIds)
		{
			CharacterGroup newGroup = new CharacterGroup(m_testProject);
			newGroup.CharacterIds = new CharacterIdHashSet(characterIds);
			newGroup.SetGroupIdLabel();
			m_testProject.CharacterGroupList.CharacterGroups.Add(newGroup);

			return newGroup;
		}
	}
}
