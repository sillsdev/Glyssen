using System;
using System.Collections.Generic;
using System.Linq;
using Glyssen;
using Glyssen.Character;
using Glyssen.Dialogs;
using NUnit.Framework;

namespace GlyssenTests.Dialogs
{
	[TestFixture]
	internal class VoiceActorAssignmentUndoActionTests
	{
		private Project m_testProject;

		[SetUp]
		public void Setup()
		{
			foreach (var group in m_testProject.CharacterGroupList.CharacterGroups)
				group.RemoveVoiceActor();
		}

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.MRK);
			var actor1 = new GlyssenEngine.VoiceActor.VoiceActor {Id = 1, Name = "Oneyda Figueroa"};
			var actor2 = new GlyssenEngine.VoiceActor.VoiceActor {Id = 2, Name = "Paul Twomey"};
			var actor3 = new GlyssenEngine.VoiceActor.VoiceActor {Id = 3, Name = "Threesa Hawkins"};
			m_testProject.VoiceActorList.AllActors = new List<GlyssenEngine.VoiceActor.VoiceActor> {actor1, actor2, actor3};

			AddCharacterGroup("Jesus");
		}

		private CharacterGroup AddCharacterGroup(params string[] characterIds)
		{
			var group = new CharacterGroup(m_testProject);
			foreach (var character in characterIds)
				group.CharacterIds.Add(character);
			m_testProject.CharacterGroupList.CharacterGroups.Add(group);
			return group;
		}

		[Test]
		public void Constructor_MakesAssignment()
		{
			var groupWithJesus = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
			groupWithJesus.AssignVoiceActor(3);
			var action = new VoiceActorAssignmentUndoAction(m_testProject, groupWithJesus, 1);
			Assert.AreEqual(1, groupWithJesus.VoiceActorId);
			Assert.AreEqual(groupWithJesus, action.GroupsAffectedByLastOperation.Single());
		}

		[Test]
		public void Description_Normal()
		{
			var groupWithJesus = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
			groupWithJesus.AssignVoiceActor(3);
			var action = new VoiceActorAssignmentUndoAction(m_testProject, groupWithJesus, 1);
			Assert.AreEqual("Assign voice actor Oneyda Figueroa", action.Description);
		}

		[Test]
		public void Description_AfterDeletingActor_RemembersDeletedName()
		{
			var groupWithJesus = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
			VoiceActorAssignmentUndoAction action;
			string descriptionBeforeDelete;

			try
			{
				m_testProject.VoiceActorList.AllActors.Add(new GlyssenEngine.VoiceActor.VoiceActor() {Id = 400, Name = "Bruce Bliss"});
				action = new VoiceActorAssignmentUndoAction(m_testProject, groupWithJesus, 400);
				descriptionBeforeDelete = action.Description;
				Assert.AreEqual("Assign voice actor Bruce Bliss", descriptionBeforeDelete);
			}
			finally
			{
				m_testProject.VoiceActorList.AllActors.RemoveAt(3);
			}

			Assert.AreEqual(descriptionBeforeDelete, action.Description);
		}

		[Test]
		public void Redo_GroupWithAssignmentSubsequentlySetToDifferentActor_AssignmentRestored()
		{
			var groupWithJesus = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
			groupWithJesus.AssignVoiceActor(2);
			var action = new VoiceActorAssignmentUndoAction(m_testProject, groupWithJesus, 3);// This will assign it to 3.
			action.Undo(); // This will reassign it back to 2.
			groupWithJesus.AssignVoiceActor(4);
			Assert.IsTrue(action.Redo(), "This should still work because we can find the group by name");
			Assert.AreEqual(3, groupWithJesus.VoiceActorId);
			Assert.AreEqual(groupWithJesus, action.GroupsAffectedByLastOperation.Single());
		}

		[Test]
		public void Redo_GroupWithAssignmentSubsequentlyRenamed_AssignmentRestored()
		{
			var groupWithJesus = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
			groupWithJesus.AssignVoiceActor(2);
			var action = new VoiceActorAssignmentUndoAction(m_testProject, groupWithJesus, 3);// This will assign it to 3.
			action.Undo(); // This will reassign it back to 2.
			groupWithJesus.GroupIdLabel = CharacterGroup.Label.Other;
			groupWithJesus.GroupIdOtherText = "Son of God";
			Assert.IsTrue(action.Redo(), "This should still work because we can find the group by actor");
			Assert.AreEqual(3, groupWithJesus.VoiceActorId);
			Assert.AreEqual(groupWithJesus, action.GroupsAffectedByLastOperation.Single());
		}

		[Test]
		public void Redo_GroupWithAssignmentNotFound_ReturnsFalse()
		{
			var groupWithJesus = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
			groupWithJesus.AssignVoiceActor(2);
			var action = new VoiceActorAssignmentUndoAction(m_testProject, groupWithJesus, 3);// This will assign it to 3.
			action.Undo(); // This will reassign it back to 2.
			groupWithJesus.AssignVoiceActor(4);
			groupWithJesus.GroupIdLabel = CharacterGroup.Label.Other;
			groupWithJesus.GroupIdOtherText = "Son of God";
			Assert.IsFalse(action.Redo());
			Assert.AreEqual(4, groupWithJesus.VoiceActorId);
			Assert.AreEqual(0, action.GroupsAffectedByLastOperation.Count());
		}

		[Test]
		public void Redo_NoPreviousAssignment_AssignmentRestored()
		{
			var groupWithJesus = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
			groupWithJesus.RemoveVoiceActor();
			var action = new VoiceActorAssignmentUndoAction(m_testProject, groupWithJesus, 2);
			action.Undo();
			Assert.IsTrue(action.Redo());
			Assert.AreEqual(2, groupWithJesus.VoiceActorId);
			Assert.AreEqual(groupWithJesus, action.GroupsAffectedByLastOperation.Single());
		}

		[Test]
		public void Redo_PreviousAssignment_AssignmentRestored()
		{
			var groupWithJesus = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
			groupWithJesus.AssignVoiceActor(3);
			var action = new VoiceActorAssignmentUndoAction(m_testProject, groupWithJesus, 2);
			action.Undo();
			Assert.IsTrue(action.Redo());
			Assert.AreEqual(2, groupWithJesus.VoiceActorId);
			Assert.AreEqual(groupWithJesus, action.GroupsAffectedByLastOperation.Single());
		}

		[Test]
		public void Redo_TwoGroupsWithSamePreviousAssignment_AssignmentRestoredOnlyToOriginalGroup()
		{
			var otherGroup = AddCharacterGroup("Jacob", "Barnabas");
			try
			{
				var groupWithJesus = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
				groupWithJesus.AssignVoiceActor(3);
				otherGroup.AssignVoiceActor(3); // No longer possible via UI, but it used to be.
				var action = new VoiceActorAssignmentUndoAction(m_testProject, groupWithJesus, 2);
				action.Undo();
				Assert.IsTrue(action.Redo());
				Assert.AreEqual(2, groupWithJesus.VoiceActorId);
				Assert.AreEqual(3, otherGroup.VoiceActorId);
				Assert.AreEqual(groupWithJesus, action.GroupsAffectedByLastOperation.Single());
			}
			finally
			{
				m_testProject.CharacterGroupList.CharacterGroups.Remove(otherGroup);
			}
		}

		[Test]
		public void Redo_TwoGroupsWithSamePreviousAssignmentOriginalGroupRenamed_ReturnsFalse()
		{
			var otherGroup = AddCharacterGroup("Jacob", "Barnabas");
			try
			{
				var groupWithJesus = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
				groupWithJesus.AssignVoiceActor(3);
				otherGroup.AssignVoiceActor(3); // No longer possible via UI, but it used to be.
				var action = new VoiceActorAssignmentUndoAction(m_testProject, groupWithJesus, 2);
				action.Undo();
				groupWithJesus.GroupIdLabel = CharacterGroup.Label.Other;
				groupWithJesus.GroupIdOtherText = "Divine Son of God";
				Assert.IsFalse(action.Redo());
				Assert.AreEqual(3, groupWithJesus.VoiceActorId);
				Assert.AreEqual(3, otherGroup.VoiceActorId);
				Assert.AreEqual(0, action.GroupsAffectedByLastOperation.Count());
			}
			finally
			{
				m_testProject.CharacterGroupList.CharacterGroups.Remove(otherGroup);
			}
		}

		[Test]
		public void Undo_GroupWithAssignmentNotFound_ReturnsFalse()
		{
			var groupWithJesus = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
			groupWithJesus.AssignVoiceActor(2);
			var action = new VoiceActorAssignmentUndoAction(m_testProject, groupWithJesus, 3);// This will assign it to 3.
			groupWithJesus.AssignVoiceActor(2);
			groupWithJesus.GroupIdLabel = CharacterGroup.Label.Other;
			groupWithJesus.GroupIdOtherText = "Friend of Sinners";
			Assert.IsFalse(action.Undo());
			Assert.AreEqual(0, action.GroupsAffectedByLastOperation.Count());
		}

		[Test]
		public void Undo_NoPreviousAssignment_AssignmentCleared()
		{
			var groupWithJesus = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
			groupWithJesus.RemoveVoiceActor();
			var origActor = groupWithJesus.VoiceActorId;
			var action = new VoiceActorAssignmentUndoAction(m_testProject, groupWithJesus, 2);
			Assert.IsTrue(action.Undo());
			Assert.AreEqual(origActor, groupWithJesus.VoiceActorId);
			Assert.AreEqual(groupWithJesus, action.GroupsAffectedByLastOperation.Single());
		}

		[Test]
		public void Undo_PreviousAssignment_AssignmentCleared()
		{
			var groupWithJesus = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
			groupWithJesus.AssignVoiceActor(3);
			var action = new VoiceActorAssignmentUndoAction(m_testProject, groupWithJesus, 2);
			Assert.IsTrue(action.Undo());
			Assert.AreEqual(3, groupWithJesus.VoiceActorId);
			Assert.AreEqual(groupWithJesus, action.GroupsAffectedByLastOperation.Single());
		}

		[Test]
		public void Undo_AssignActorAlreadyAssignedToAnotherGroup_AssignmentRestoredOnOriginalGroup()
		{
			var otherGroup = AddCharacterGroup("Jacob", "Barnabas");
			try
			{
				var groupWithJesus = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
				groupWithJesus.AssignVoiceActor(3);
				otherGroup.AssignVoiceActor(2);
				var action = new VoiceActorAssignmentUndoAction(m_testProject, groupWithJesus, 2); // UI shouldn't allow this.
				Assert.IsTrue(action.Undo());
				Assert.AreEqual(3, groupWithJesus.VoiceActorId);
				Assert.AreEqual(2, otherGroup.VoiceActorId);
				Assert.AreEqual(groupWithJesus, action.GroupsAffectedByLastOperation.Single());
			}
			finally
			{
				m_testProject.CharacterGroupList.CharacterGroups.Remove(otherGroup);
			}
		}
	}
}