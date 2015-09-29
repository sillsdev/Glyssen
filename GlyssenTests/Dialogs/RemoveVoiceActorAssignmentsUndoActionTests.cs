using System.Collections.Generic;
using System.Linq;
using Glyssen;
using Glyssen.Character;
using Glyssen.Dialogs;
using NUnit.Framework;

namespace GlyssenTests.Dialogs
{
	[TestFixture]
	class RemoveVoiceActorAssignmentsUndoActionTests
	{
		private Project m_testProject;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.MRK);
			var actor1 = new Glyssen.VoiceActor.VoiceActor { Id = 1, Name = "Oneyda Figueroa" };
			var actor2 = new Glyssen.VoiceActor.VoiceActor { Id = 2, Name = "Paul Twomey" };
			var actor3 = new Glyssen.VoiceActor.VoiceActor { Id = 3, Name = "Threesa Hawkins" };
			m_testProject.VoiceActorList.Actors = new List<Glyssen.VoiceActor.VoiceActor> { actor1, actor2, actor3 };

			AddCharacterGroup("Jesus");
			AddCharacterGroup("Mary", "Rhoda", "Rahab");
			AddCharacterGroup("Moses", "Caiaphas", "Centurian");
		}

		[SetUp]
		public void Setup()
		{
			foreach (var group in m_testProject.CharacterGroupList.CharacterGroups)
				group.RemoveVoiceActor();

			m_testProject.CharacterGroupList.GroupContainingCharacterId("Moses").Name = "Crusty Old Dudes";
		}

		private CharacterGroup AddCharacterGroup(params string[] characterIds)
		{
			var group = new CharacterGroup();
			foreach (var character in characterIds)
				group.CharacterIds.Add(character);
			m_testProject.CharacterGroupList.CharacterGroups.Add(group);
			return group;
		}

		[Test]
		public void Description_UnassignmentOfSingleGroupWithSingleCharacter_DescriptionRefersToCharacterNotGroup()
		{
			var groupWithJesus = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
			groupWithJesus.AssignVoiceActor(2);
			groupWithJesus.RemoveVoiceActor();

			var action = new RemoveVoiceActorAssignmentsUndoAction(m_testProject, groupWithJesus);
			Assert.AreEqual("Remove voice actor assignment for Jesus", action.Description);
		}

		[Test]
		public void Description_UnassignmentOfSingleGroupWithMultipleCharacters_DescriptionRefersToGroup()
		{
			var groupWithMoses = m_testProject.CharacterGroupList.GroupContainingCharacterId("Moses");
			groupWithMoses.AssignVoiceActor(2);
			groupWithMoses.RemoveVoiceActor();

			var action = new RemoveVoiceActorAssignmentsUndoAction(m_testProject, groupWithMoses);
			Assert.AreEqual("Remove voice actor assignment for Crusty Old Dudes group", action.Description);
		}

		[Test]
		public void Description_UnassignmentOfMultipleGroups_DescriptionRefersToGroupsGenerically()
		{
			m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus").AssignVoiceActor(3);
			m_testProject.CharacterGroupList.GetGroupByName("Crusty Old Dudes").AssignVoiceActor(2);
			m_testProject.CharacterGroupList.GroupContainingCharacterId("Mary").AssignVoiceActor(1);
			var action = new RemoveVoiceActorAssignmentsUndoAction(m_testProject, m_testProject.CharacterGroupList.CharacterGroups);
			foreach (var characterGroup in m_testProject.CharacterGroupList.CharacterGroups)
				characterGroup.RemoveVoiceActor();

			Assert.AreEqual("Remove voice actor assignment for multiple groups", action.Description);
		}

		[Test]
		public void Undo_SingleGroupNotFound_NoChangeAndReturnsFalse()
		{
			var groupWithMoses = m_testProject.CharacterGroupList.GroupContainingCharacterId("Moses");
			groupWithMoses.AssignVoiceActor(2);
			groupWithMoses.RemoveVoiceActor();

			var action = new RemoveVoiceActorAssignmentsUndoAction(m_testProject, groupWithMoses);
			groupWithMoses.Name = "Elderly men";
			Assert.IsFalse(action.Undo());
			Assert.IsFalse(groupWithMoses.IsVoiceActorAssigned);
		}


		[Test]
		public void Undo_MultipleGroupsWithOneNotFound_NoChangeAndReturnsFalse()
		{
			m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus").AssignVoiceActor(3);
			var groupWithMoses = m_testProject.CharacterGroupList.GetGroupByName("Crusty Old Dudes");
			groupWithMoses.AssignVoiceActor(2);
			m_testProject.CharacterGroupList.GroupContainingCharacterId("Mary").AssignVoiceActor(1);
			var action = new RemoveVoiceActorAssignmentsUndoAction(m_testProject, m_testProject.CharacterGroupList.CharacterGroups);
			foreach (var characterGroup in m_testProject.CharacterGroupList.CharacterGroups)
				characterGroup.RemoveVoiceActor();

			groupWithMoses.Name = null;
			Assert.IsFalse(action.Undo());
			Assert.IsFalse(m_testProject.CharacterGroupList.CharacterGroups.Any(g => g.IsVoiceActorAssigned));
		}

		//[Test]
		//public void Undo_NoPreviousAssignment_NoChange()
		//{
		//	var groupWithJesus = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
		//	groupWithJesus.RemoveVoiceActor();
		//	var origActor = groupWithJesus.VoiceActorId;
		//	groupWithJesus.AssignVoiceActor(2);
		//	var action = new RemoveVoiceActorAssignmentsUndoAction(m_testProject, 2, origActor);
		//	Assert.IsTrue(action.Undo());
		//	Assert.AreEqual(origActor, groupWithJesus.VoiceActorId);
		//}

		//[Test]
		//public void Undo_PreviousAssignment_AssignmentCleared()
		//{
		//	var groupWithJesus = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
		//	groupWithJesus.AssignVoiceActor(3);
		//	groupWithJesus.AssignVoiceActor(2);
		//	var action = new RemoveVoiceActorAssignmentsUndoAction(m_testProject, 2, 3);
		//	Assert.IsTrue(action.Undo());
		//	Assert.AreEqual(3, groupWithJesus.VoiceActorId);
		//}

		//[Test]
		//public void Redo_GroupWithAssignmentNotFound_ReturnsFalse()
		//{
		//	var action = new RemoveVoiceActorAssignmentsUndoAction(m_testProject, 2, 1);
		//	Assert.IsFalse(action.Redo());
		//}

		//[Test]
		//public void Redo_NoPreviousAssignment_AssignmentRestored()
		//{
		//	var groupWithJesus = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
		//	groupWithJesus.RemoveVoiceActor();
		//	var origActor = groupWithJesus.VoiceActorId;
		//	groupWithJesus.AssignVoiceActor(2);
		//	var action = new RemoveVoiceActorAssignmentsUndoAction(m_testProject, 2, origActor);
		//	action.Undo();
		//	Assert.IsTrue(action.Redo());
		//	Assert.AreEqual(2, groupWithJesus.VoiceActorId);
		//}

		//[Test]
		//public void Redo_PreviousAssignment_AssignmentCleared()
		//{
		//	var groupWithJesus = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
		//	groupWithJesus.AssignVoiceActor(3);
		//	groupWithJesus.AssignVoiceActor(2);
		//	var action = new RemoveVoiceActorAssignmentsUndoAction(m_testProject, 2, 3);
		//	action.Undo();
		//	Assert.IsTrue(action.Redo());
		//	Assert.AreEqual(2, groupWithJesus.VoiceActorId);
		//}
	}
}
