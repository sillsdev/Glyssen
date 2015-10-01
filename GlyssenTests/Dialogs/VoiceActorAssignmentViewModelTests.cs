using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Glyssen;
using Glyssen.Character;
using Glyssen.Dialogs;
using GlyssenTests.Properties;
using NUnit.Framework;

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
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.MRK);
		}

		[SetUp]
		public void SetUp()
		{
			m_testProject.VoiceActorList.Actors.Clear();
			m_testProject.CharacterGroupList.CharacterGroups.Clear();
			// Adding one group here prevents the constructor from generating groups
			var group = new CharacterGroup(1);
			group.CharacterIds.Add("John"); // Need a character in the group, otherwise it is treated as a Cameo group and it's not legal to assign/unassign actor.
			m_testProject.CharacterGroupList.CharacterGroups.Add(group);
			m_model = new VoiceActorAssignmentViewModel(m_testProject);
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			TestProject.DeleteTestProjectFolder();
		}

		[Test]
		public void AddNewGroup_NewGroupIsAddedToProject()
		{
			Assert.AreEqual(1, m_testProject.CharacterGroupList.CharacterGroups.Count);
			bool charactersMoved;
			var newGroup = m_model.AddNewGroup(new [] {"ear"}, out charactersMoved);
			Assert.IsFalse(charactersMoved, "In real life, we'd expect true, but in this test, \"ear\" is never added to a group.");
			Assert.AreEqual(2, m_testProject.CharacterGroupList.CharacterGroups.Count);
			Assert.AreEqual(newGroup, m_testProject.CharacterGroupList.CharacterGroups[1]);
		}

		[Test]
		public void AddNewGroup_CharactersMovedFromOriginalGroup()
		{
			var characterIds = new List<string> { "John", "Andrew" };
			var sourceGroup = m_model.CharacterGroups[0];
			sourceGroup.CharacterIds = new CharacterIdHashSet(characterIds);
			bool charactersMoved;
			var destGroup = m_model.AddNewGroup(characterIds, out charactersMoved);

			Assert.True(charactersMoved);

			var destGroupCharacterIds = destGroup.CharacterIds;
			Assert.AreEqual(2, destGroup.CharacterIds.Count);
			Assert.True(destGroupCharacterIds.Contains("John"));
			Assert.True(destGroupCharacterIds.Contains("Andrew"));
		}

		[Test]
		public void MoveActorFromGroupToGroup_SwapTrue()
		{
			var actor1 = new Glyssen.VoiceActor.VoiceActor { Id = 1 };
			var actor2 = new Glyssen.VoiceActor.VoiceActor { Id = 2 };
			var sourceGroup = m_model.CharacterGroups[0];
			sourceGroup.AssignVoiceActor(actor1.Id);
			var destGroup = new CharacterGroup(567);
			m_testProject.CharacterGroupList.CharacterGroups.Add(destGroup);
			destGroup.AssignVoiceActor(actor2.Id);

			Assert.AreEqual(actor1.Id, sourceGroup.VoiceActorId);
			Assert.AreEqual(actor2.Id, destGroup.VoiceActorId);
			m_model.MoveActorFromGroupToGroup(sourceGroup, destGroup, true);
			Assert.AreEqual(actor2.Id, sourceGroup.VoiceActorId);
			Assert.AreEqual(actor1.Id, destGroup.VoiceActorId);
		}

		[Test]
		public void MoveActorFromGroupToGroup_SwapFalse()
		{
			var actor1 = new Glyssen.VoiceActor.VoiceActor { Id = 1 };
			var actor2 = new Glyssen.VoiceActor.VoiceActor { Id = 2 };
			var sourceGroup = m_model.CharacterGroups[0];
			sourceGroup.AssignVoiceActor(actor1.Id);
			var destGroup = new CharacterGroup(567);
			m_testProject.CharacterGroupList.CharacterGroups.Add(destGroup);
			destGroup.AssignVoiceActor(actor2.Id);

			Assert.AreEqual(actor1.Id, sourceGroup.VoiceActorId);
			Assert.AreEqual(actor2.Id, destGroup.VoiceActorId);
			m_model.MoveActorFromGroupToGroup(sourceGroup, destGroup);
			Assert.False(sourceGroup.IsVoiceActorAssigned);
			Assert.AreEqual(actor1.Id, destGroup.VoiceActorId);
		}

		[Test]
		public void AssignActorToGroup_CanAssignTrue_ActorAssigned()
		{
			var actor1 = new Glyssen.VoiceActor.VoiceActor { Id = 1, Name = "Puni Upalari" };
			m_testProject.VoiceActorList.Actors = new List<Glyssen.VoiceActor.VoiceActor> { actor1 };
			var group = m_model.CharacterGroups[0];
			m_model.AssignActorToGroup(actor1.Id, group);
			Assert.AreEqual(actor1.Id, group.VoiceActorId);
			var undoDescriptions = m_model.UndoActions;
			Assert.AreEqual(1, undoDescriptions.Count);
			Assert.AreEqual("Assign voice actor Puni Upalari", undoDescriptions[0]);
		}

		[Test]
		public void AssignActorToGroup_ExistingGroupAssignedToActor_ActorUnassignedFromPreviousGroupAndAssignedToRequestedGroup()
		{
			var actor1 = new Glyssen.VoiceActor.VoiceActor { Id = 1, Name = "Eduardo Lopez" };
			m_testProject.VoiceActorList.Actors.Add(actor1);
			var existingGroup = m_model.CharacterGroups[0];
			existingGroup.VoiceActorId = 1;
			var newGroup = new CharacterGroup(42) { Name = "New group" };
			m_testProject.CharacterGroupList.CharacterGroups.Add(newGroup);
			m_model.AssignActorToGroup(1, newGroup);
			Assert.AreEqual(1, newGroup.VoiceActorId);
			Assert.AreEqual(1, m_model.UndoActions.Count);
			Assert.AreEqual("Assign voice actor Eduardo Lopez", m_model.UndoActions[0]);
			Assert.False(existingGroup.IsVoiceActorAssigned);
		}

		[Test]
		public void Undo_AssignActorToGroup_ExistingGroupAssignedToActor_ActorUnassignedFromAssignedGroupAndReassignedToPreviousGroup()
		{
			var actor1 = new Glyssen.VoiceActor.VoiceActor { Id = 1, Name = "Eduardo Lopez" };
			m_testProject.VoiceActorList.Actors.Add(actor1);
			var existingGroup = m_model.CharacterGroups[0];
			existingGroup.VoiceActorId = 1;
			var newGroup = new CharacterGroup(42) { Name = "New group" };
			m_testProject.CharacterGroupList.CharacterGroups.Add(newGroup);
			m_model.AssignActorToGroup(1, newGroup);
			bool savedCalled = false;
			m_model.Saved += (sender, args) => { savedCalled = true; };
			Assert.IsTrue(m_model.Undo());
			Assert.AreEqual(0, m_model.UndoActions.Count);
			Assert.AreEqual(1, m_model.RedoActions.Count);
			Assert.AreEqual("Assign voice actor Eduardo Lopez", m_model.RedoActions[0]);
			Assert.False(newGroup.IsVoiceActorAssigned);
			Assert.AreEqual(1, existingGroup.VoiceActorId);
			Assert.IsTrue(savedCalled);
		}

		[Test]
		public void AssignActorToGroup_CanAssignFalse_ActorNotAssigned()
		{
			m_model.CanAssign = false;
			var actor1 = new Glyssen.VoiceActor.VoiceActor { Id = 1, Name = "Charlotte Pohlig" };
			m_testProject.VoiceActorList.Actors.Add(actor1);
			var group = m_model.CharacterGroups[0];
			m_model.AssignActorToGroup(actor1.Id, group);
			Assert.False(group.IsVoiceActorAssigned);
			m_model.CanAssign = true;
		}

		[Test]
		public void UnAssignActorFromGroups_ByGroup()
		{
			var actor1 = new Glyssen.VoiceActor.VoiceActor { Id = 1, Name = "Marco Polo" };
			m_testProject.VoiceActorList.Actors.Add(actor1);
			var actor2 = new Glyssen.VoiceActor.VoiceActor { Id = 2, Name = "Wilbur Wright" };
			m_testProject.VoiceActorList.Actors.Add(actor2);
			var group1 = m_model.CharacterGroups[0];
			m_model.AssignActorToGroup(actor2.Id, group1);
			Assert.True(group1.IsVoiceActorAssigned);
			var group2 = new CharacterGroup(567);
			group2.CharacterIds.Add("Nocodemus");
			m_testProject.CharacterGroupList.CharacterGroups.Add(group2);
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
			var actor1 = new Glyssen.VoiceActor.VoiceActor { Id = 1, Name = "Marco Polo" };
			m_testProject.VoiceActorList.Actors.Add(actor1);
			var actor2 = new Glyssen.VoiceActor.VoiceActor { Id = 2, Name = "Wilbur Wright" };
			m_testProject.VoiceActorList.Actors.Add(actor2);
			var group1 = m_model.CharacterGroups[0];
			m_model.AssignActorToGroup(actor2.Id, group1);
			Assert.True(group1.IsVoiceActorAssigned);
			var cameoGroup = new CharacterGroup(567);
			m_testProject.CharacterGroupList.CharacterGroups.Add(cameoGroup);
			m_model.AssignActorToGroup(actor1.Id, cameoGroup);
			Assert.True(cameoGroup.IsVoiceActorAssigned);
			Assert.AreEqual(2, m_model.UndoActions.Count);

			m_model.UnAssignActorFromGroups(new List<CharacterGroup> { group1, cameoGroup });
			Assert.False(group1.IsVoiceActorAssigned);
			Assert.True(cameoGroup.IsVoiceActorAssigned);
			Assert.AreEqual(3, m_model.UndoActions.Count);
		}

		[Test]
		public void MoveCharactersToGroup_DestinationContainsCharacters()
		{
			var characterIds = new List<string> { "John", "Andrew" };
			var sourceGroup = m_model.CharacterGroups[0];
			sourceGroup.CharacterIds = new CharacterIdHashSet(characterIds);
			bool charactersMoved;
			var destGroup = m_model.AddNewGroup(new [] { "ear", "mouth" }, out charactersMoved);

			Assert.True(m_model.MoveCharactersToGroup(characterIds, destGroup));

			var destGroupCharacterIds = destGroup.CharacterIds;
			Assert.AreEqual(4, destGroup.CharacterIds.Count);
			Assert.True(destGroupCharacterIds.Contains("John"));
			Assert.True(destGroupCharacterIds.Contains("Andrew"));
		}

		[Test]
		public void MoveCharactersToGroup_SourceGroupSameAsDestinationGroup_NoActionTaken()
		{
			var characterIds = new List<string> { "John", "Andrew" };
			var sourceGroup = m_model.CharacterGroups[0];
			sourceGroup.CharacterIds = new CharacterIdHashSet(characterIds);

			Assert.False(m_model.MoveCharactersToGroup(characterIds, sourceGroup));
		}

		[Test]
		public void MoveCharactersToGroup_AllCharactersMovedOutOfSource_SourceGroupRemoved()
		{
			var characterIds = new List<string> { "John", "Andrew" };
			var sourceGroup = m_model.CharacterGroups[0];
			sourceGroup.CharacterIds = new CharacterIdHashSet(characterIds);
			bool charactersMoved;
			var destGroup = m_model.AddNewGroup(new[] { "ear", "mouth" }, out charactersMoved);

			Assert.AreEqual(2, m_testProject.CharacterGroupList.CharacterGroups.Count);
			Assert.True(m_model.MoveCharactersToGroup(characterIds, destGroup));
			Assert.AreEqual(1, m_testProject.CharacterGroupList.CharacterGroups.Count);
		}

		[Test]
		public void MoveCharactersToGroup_NoCharacterProvided_ThrowsArgumentException()
		{
			var destGroup = m_model.CharacterGroups[0];
			Assert.Throws<ArgumentException>(() => m_model.MoveCharactersToGroup(new List<string>(), destGroup));
		}

		[Test]
		public void RemoveUnusedGroups_RemovesGroupWithNoCharactersAndNoAssignedActor()
		{
			Assert.AreEqual(1, m_testProject.CharacterGroupList.CharacterGroups.Count);
			m_model.RemoveUnusedGroups();
			Assert.AreEqual(0, m_testProject.CharacterGroupList.CharacterGroups.Count);
		}

		[Test]
		public void RemoveUnusedGroups_DoesNotRemoveGroupWithAssignedActor()
		{
			var groupWithActor = m_testProject.CharacterGroupList.CharacterGroups[0];
			groupWithActor.AssignVoiceActor(45);
			m_testProject.CharacterGroupList.CharacterGroups.Add(new CharacterGroup(42));
			Assert.AreEqual(2, m_testProject.CharacterGroupList.CharacterGroups.Count);
			m_model.RemoveUnusedGroups();
			Assert.AreEqual(1, m_testProject.CharacterGroupList.CharacterGroups.Count);
			Assert.AreEqual(groupWithActor, m_testProject.CharacterGroupList.CharacterGroups[0]);
			Assert.True(m_testProject.CharacterGroupList.CharacterGroups[0].IsVoiceActorAssigned);
		}

		[Test]
		public void RegenerateGroups()
		{
			var actor1 = new Glyssen.VoiceActor.VoiceActor { Id = 1 };
			var actor2 = new Glyssen.VoiceActor.VoiceActor { Id = 2 };
			var actor3 = new Glyssen.VoiceActor.VoiceActor { Id = 3 };
			m_testProject.VoiceActorList.Actors = new List<Glyssen.VoiceActor.VoiceActor> { actor1, actor2, actor3 };
			m_model.RegenerateGroups(false);
			Assert.AreEqual(3, m_testProject.CharacterGroupList.CharacterGroups.Count);
		}

		[Test]
		public void RegenerateGroups_MaintainAssignments_OneAssignment_OneCharacter_AssignmentMaintained()
		{
			var actor1 = new Glyssen.VoiceActor.VoiceActor { Id = 1 };
			var actor2 = new Glyssen.VoiceActor.VoiceActor { Id = 2 };
			var actor3 = new Glyssen.VoiceActor.VoiceActor { Id = 3 };
			var actor4 = new Glyssen.VoiceActor.VoiceActor { Id = 4 };
			var actor5 = new Glyssen.VoiceActor.VoiceActor { Id = 5 };
			m_testProject.VoiceActorList.Actors = new List<Glyssen.VoiceActor.VoiceActor> { actor1, actor2, actor3, actor4, actor5 };
			m_model.RegenerateGroups(false);

			m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus").AssignVoiceActor(actor1.Id);
			m_model.RegenerateGroups(true);
			Assert.AreEqual(5, m_testProject.CharacterGroupList.CharacterGroups.Count);
			Assert.AreEqual(actor1.Id, m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus").VoiceActorId);
		}

		[Test]
		public void RegenerateGroups_MaintainAssignments_OneAssignment_TwoCharacters_AssignmentMaintainedForMostProminentCharacter()
		{
			var actor1 = new Glyssen.VoiceActor.VoiceActor { Id = 1 };
			var actor2 = new Glyssen.VoiceActor.VoiceActor { Id = 2 };
			var actor3 = new Glyssen.VoiceActor.VoiceActor { Id = 3 };
			var actor4 = new Glyssen.VoiceActor.VoiceActor { Id = 4 };
			var actor5 = new Glyssen.VoiceActor.VoiceActor { Id = 5 };
			m_testProject.VoiceActorList.Actors = new List<Glyssen.VoiceActor.VoiceActor> { actor1, actor2, actor3, actor4, actor5 };
			m_model.RegenerateGroups(false);

			bool charactersMoved;
			var newGroup = m_model.AddNewGroup(new[] { "Jesus", "John" }, out charactersMoved);
			Assert.IsTrue(charactersMoved);

			newGroup.AssignVoiceActor(actor1.Id);
			m_model.RegenerateGroups(true);
			Assert.AreEqual(5, m_testProject.CharacterGroupList.CharacterGroups.Count);
			Assert.AreEqual(actor1.Id, m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus").VoiceActorId);
			Assert.IsFalse(m_testProject.CharacterGroupList.GroupContainingCharacterId("John").IsVoiceActorAssigned);
		}

		[Test]
		public void RegenerateGroups_MaintainAssignments_TwoAssignments_GroupsAreCombined_AssignmentMaintainedForMostProminentCharacter()
		{
			var actor1 = new Glyssen.VoiceActor.VoiceActor { Id = 1 };
			var actor2 = new Glyssen.VoiceActor.VoiceActor { Id = 2 };
			var actor3 = new Glyssen.VoiceActor.VoiceActor { Id = 3 };
			var actor4 = new Glyssen.VoiceActor.VoiceActor { Id = 4 };
			var actor5 = new Glyssen.VoiceActor.VoiceActor { Id = 5 };
			m_testProject.VoiceActorList.Actors = new List<Glyssen.VoiceActor.VoiceActor> { actor1, actor2, actor3, actor4, actor5 };
			m_model.RegenerateGroups(false);

			m_model.SplitGroup(new List<string> { "extra-MRK" });
			var extraBiblicalGroup = m_testProject.CharacterGroupList.GroupContainingCharacterId("extra-MRK");
			var bcGroup = m_testProject.CharacterGroupList.GroupContainingCharacterId("BC-MRK");

			// Validate the test is set up correctly
			Assert.AreNotEqual(extraBiblicalGroup, bcGroup);

			extraBiblicalGroup.AssignVoiceActor(actor1.Id);
			bcGroup.AssignVoiceActor(actor2.Id);

			m_model.RegenerateGroups(true);
			Assert.AreEqual(5, m_testProject.CharacterGroupList.CharacterGroups.Count);
			extraBiblicalGroup = m_testProject.CharacterGroupList.GroupContainingCharacterId("extra-MRK");
			bcGroup = m_testProject.CharacterGroupList.GroupContainingCharacterId("BC-MRK");
			Assert.AreEqual(actor1.Id, extraBiblicalGroup.VoiceActorId);
			Assert.AreEqual(actor1.Id, bcGroup.VoiceActorId);
			Assert.AreEqual(extraBiblicalGroup, bcGroup);
			Assert.False(m_testProject.CharacterGroupList.HasVoiceActorAssigned(actor2.Id));
		}

		[Test]
		public void RegenerateGroups_HasCameoAssigned_MaintainsCameoGroup()
		{
			var actor1 = new Glyssen.VoiceActor.VoiceActor { Id = 1 };
			var actor2 = new Glyssen.VoiceActor.VoiceActor { Id = 2 };
			var actor3 = new Glyssen.VoiceActor.VoiceActor { Id = 3 };
			m_testProject.VoiceActorList.Actors = new List<Glyssen.VoiceActor.VoiceActor> { actor1, actor2, actor3 };
			m_model.RegenerateGroups(false);

			var actor4 = new Glyssen.VoiceActor.VoiceActor { Id = 4, IsCameo = true };
			m_testProject.VoiceActorList.Actors.Add(actor4);
			bool charactersMoved;
			var cameoGroup = m_model.AddNewGroup(new[] { "John" }, out charactersMoved);
			Assert.IsTrue(charactersMoved);
			cameoGroup.AssignVoiceActor(actor4.Id);

			m_model.RegenerateGroups(false);
			var groups = m_testProject.CharacterGroupList.CharacterGroups;
			Assert.AreEqual(4, groups.Count);

			cameoGroup = groups.First(g => g.VoiceActorId == actor4.Id);
			Assert.AreEqual(actor4.Id, cameoGroup.VoiceActorId);
			Assert.AreEqual(1, cameoGroup.CharacterIds.Count);
			Assert.True(cameoGroup.CharacterIds.Contains("John"));
			Assert.False(groups.Where(g => g != cameoGroup).SelectMany(g => g.CharacterIds).Contains("John"));
		}

		[Test]
		public void SplitGroup_MoveOne()
		{
			var characterIds = new List<string> { "John", "Andrew" };
			var existingGroup = m_model.CharacterGroups[0];
			existingGroup.CharacterIds = new CharacterIdHashSet(characterIds);

			Assert.AreEqual(1, m_testProject.CharacterGroupList.CharacterGroups.Count);

			Assert.True(m_model.SplitGroup(new List<string> { "John" }));

			Assert.AreEqual(2, m_testProject.CharacterGroupList.CharacterGroups.Count);

			var newGroup = m_testProject.CharacterGroupList.CharacterGroups[1];

			Assert.False(existingGroup == newGroup);
			Assert.True(existingGroup.CharacterIds.Contains("Andrew"));
			Assert.False(existingGroup.CharacterIds.Contains("John"));
			Assert.True(newGroup.CharacterIds.Contains("John"));
		}

		[Test]
		public void SplitGroup_MoveMultiple()
		{
			var characterIds = new List<string> { "John", "Andrew", "Peter" };
			var existingGroup = m_model.CharacterGroups[0];
			existingGroup.CharacterIds = new CharacterIdHashSet(characterIds);

			Assert.AreEqual(1, m_testProject.CharacterGroupList.CharacterGroups.Count);

			Assert.True(m_model.SplitGroup(new List<string> { "John", "Peter" }));

			Assert.AreEqual(2, m_testProject.CharacterGroupList.CharacterGroups.Count);

			var newGroup = m_testProject.CharacterGroupList.CharacterGroups[1];

			Assert.False(existingGroup == newGroup);
			Assert.True(existingGroup.CharacterIds.Contains("Andrew"));
			Assert.False(existingGroup.CharacterIds.Contains("John"));
			Assert.False(existingGroup.CharacterIds.Contains("Peter"));
			Assert.True(newGroup.CharacterIds.Contains("John"));
			Assert.True(newGroup.CharacterIds.Contains("Peter"));
		}

		[Test]
		public void GetMultiColumnActorDataTable_NoActorsAssigned_GetsAllActorsInAlphbeticalOrder()
		{
			var actorB = new Glyssen.VoiceActor.VoiceActor { Id = 1, Name = "B" };
			var actorC = new Glyssen.VoiceActor.VoiceActor { Id = 2, Name = "C" };
			var actorA = new Glyssen.VoiceActor.VoiceActor { Id = 3, Name = "A" };
			m_testProject.VoiceActorList.Actors = new List<Glyssen.VoiceActor.VoiceActor> { actorB, actorC, actorA };
			m_model.RegenerateGroups(false);

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
			var actorB = new Glyssen.VoiceActor.VoiceActor { Id = 1, Name = "B" };
			var actorC = new Glyssen.VoiceActor.VoiceActor { Id = 2, Name = "C" };
			var actorA = new Glyssen.VoiceActor.VoiceActor { Id = 3, Name = "A" };
			m_testProject.VoiceActorList.Actors = new List<Glyssen.VoiceActor.VoiceActor> { actorB, actorC, actorA };
			m_model.RegenerateGroups(false);
			var group = m_model.CharacterGroups[0];
			m_model.AssignActorToGroup(actorA.Id, group);

			var dataTable = m_model.GetMultiColumnActorDataTable(m_model.CharacterGroups[0]);
			var actorList = GetActorListFromDataTable(dataTable);
			Assert.AreEqual(actorB, actorList[0]);
			Assert.AreEqual(actorC, actorList[1]);
			Assert.AreEqual(null, actorList[2]); // The "Unassigned" option
			Assert.AreEqual(actorA, actorList[3]);
		}

		private List<Glyssen.VoiceActor.VoiceActor> GetActorListFromDataTable(DataTable dataTable)
		{
			return (from DataRow row in dataTable.Rows select m_testProject.VoiceActorList.GetVoiceActorById((int)row.ItemArray[0])).ToList();
		}
	}
}
