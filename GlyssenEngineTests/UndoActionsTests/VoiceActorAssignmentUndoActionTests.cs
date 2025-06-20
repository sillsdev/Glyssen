using System.Collections.Generic;
using System.Linq;
using GlyssenEngine;
using GlyssenEngine.Character;
using GlyssenEngine.UndoActions;
using GlyssenEngine.Casting;
using NUnit.Framework;

namespace GlyssenEngineTests.UndoActionsTests
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

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.MRK);
			var actor1 = new VoiceActor {Id = 1, Name = "Oneyda Figueroa"};
			var actor2 = new VoiceActor {Id = 2, Name = "Paul Twomey"};
			var actor3 = new VoiceActor {Id = 3, Name = "Threesa Hawkins"};
			m_testProject.VoiceActorList.AllActors = new List<VoiceActor> {actor1, actor2, actor3};

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
			Assert.That(groupWithJesus.VoiceActorId, Is.EqualTo(1));
			Assert.That(groupWithJesus, Is.EqualTo(action.GroupsAffectedByLastOperation.Single()));
		}

		[Test]
		public void Description_Normal()
		{
			var groupWithJesus = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
			groupWithJesus.AssignVoiceActor(3);
			var action = new VoiceActorAssignmentUndoAction(m_testProject, groupWithJesus, 1);
			Assert.That(action.Description, Is.EqualTo("Assign voice actor Oneyda Figueroa"));
		}

		[Test]
		public void Description_AfterDeletingActor_RemembersDeletedName()
		{
			var groupWithJesus = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
			VoiceActorAssignmentUndoAction action;
			string descriptionBeforeDelete;

			try
			{
				m_testProject.VoiceActorList.AllActors.Add(new VoiceActor() {Id = 400, Name = "Bruce Bliss"});
				action = new VoiceActorAssignmentUndoAction(m_testProject, groupWithJesus, 400);
				descriptionBeforeDelete = action.Description;
				Assert.That(descriptionBeforeDelete, Is.EqualTo("Assign voice actor Bruce Bliss"));
			}
			finally
			{
				m_testProject.VoiceActorList.AllActors.RemoveAt(3);
			}

			Assert.That(descriptionBeforeDelete, Is.EqualTo(action.Description));
		}

		[Test]
		public void Redo_GroupWithAssignmentSubsequentlySetToDifferentActor_AssignmentRestored()
		{
			var groupWithJesus = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
			groupWithJesus.AssignVoiceActor(2);
			var action = new VoiceActorAssignmentUndoAction(m_testProject, groupWithJesus, 3);// This will assign it to 3.
			action.Undo(); // This will reassign it back to 2.
			groupWithJesus.AssignVoiceActor(4);
			Assert.That(action.Redo(), Is.True, "This should still work because we can find the group by name");
			Assert.That(groupWithJesus.VoiceActorId, Is.EqualTo(3));
			Assert.That(groupWithJesus, Is.EqualTo(action.GroupsAffectedByLastOperation.Single()));
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
			Assert.That(action.Redo(), Is.True, "This should still work because we can find the group by actor");
			Assert.That(groupWithJesus.VoiceActorId, Is.EqualTo(3));
			Assert.That(groupWithJesus, Is.EqualTo(action.GroupsAffectedByLastOperation.Single()));
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
			Assert.That(action.Redo(), Is.False);
			Assert.That(groupWithJesus.VoiceActorId, Is.EqualTo(4));
			Assert.That(action.GroupsAffectedByLastOperation.Count(), Is.EqualTo(0));
		}

		[Test]
		public void Redo_NoPreviousAssignment_AssignmentRestored()
		{
			var groupWithJesus = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
			groupWithJesus.RemoveVoiceActor();
			var action = new VoiceActorAssignmentUndoAction(m_testProject, groupWithJesus, 2);
			action.Undo();
			Assert.That(action.Redo(), Is.True);
			Assert.That(groupWithJesus.VoiceActorId, Is.EqualTo(2));
			Assert.That(groupWithJesus, Is.EqualTo(action.GroupsAffectedByLastOperation.Single()));
		}

		[Test]
		public void Redo_PreviousAssignment_AssignmentRestored()
		{
			var groupWithJesus = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
			groupWithJesus.AssignVoiceActor(3);
			var action = new VoiceActorAssignmentUndoAction(m_testProject, groupWithJesus, 2);
			action.Undo();
			Assert.That(action.Redo(), Is.True);
			Assert.That(groupWithJesus.VoiceActorId, Is.EqualTo(2));
			Assert.That(groupWithJesus, Is.EqualTo(action.GroupsAffectedByLastOperation.Single()));
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
				Assert.That(action.Redo(), Is.True);
				Assert.That(groupWithJesus.VoiceActorId, Is.EqualTo(2));
				Assert.That(otherGroup.VoiceActorId, Is.EqualTo(3));
				Assert.That(groupWithJesus, Is.EqualTo(action.GroupsAffectedByLastOperation.Single()));
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
				Assert.That(action.Redo(), Is.False);
				Assert.That(groupWithJesus.VoiceActorId, Is.EqualTo(3));
				Assert.That(otherGroup.VoiceActorId, Is.EqualTo(3));
				Assert.That(action.GroupsAffectedByLastOperation.Count(), Is.EqualTo(0));
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
			Assert.That(action.Undo(), Is.False);
			Assert.That(action.GroupsAffectedByLastOperation.Count(), Is.EqualTo(0));
		}

		[Test]
		public void Undo_NoPreviousAssignment_AssignmentCleared()
		{
			var groupWithJesus = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
			groupWithJesus.RemoveVoiceActor();
			var origActor = groupWithJesus.VoiceActorId;
			var action = new VoiceActorAssignmentUndoAction(m_testProject, groupWithJesus, 2);
			Assert.That(action.Undo(), Is.True);
			Assert.That(origActor, Is.EqualTo(groupWithJesus.VoiceActorId));
			Assert.That(groupWithJesus, Is.EqualTo(action.GroupsAffectedByLastOperation.Single()));
		}

		[Test]
		public void Undo_PreviousAssignment_AssignmentCleared()
		{
			var groupWithJesus = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
			groupWithJesus.AssignVoiceActor(3);
			var action = new VoiceActorAssignmentUndoAction(m_testProject, groupWithJesus, 2);
			Assert.That(action.Undo(), Is.True);
			Assert.That(groupWithJesus.VoiceActorId, Is.EqualTo(3));
			Assert.That(groupWithJesus, Is.EqualTo(action.GroupsAffectedByLastOperation.Single()));
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
				Assert.That(action.Undo(), Is.True);
				Assert.That(groupWithJesus.VoiceActorId, Is.EqualTo(3));
				Assert.That(otherGroup.VoiceActorId, Is.EqualTo(2));
				Assert.That(groupWithJesus, Is.EqualTo(action.GroupsAffectedByLastOperation.Single()));
			}
			finally
			{
				m_testProject.CharacterGroupList.CharacterGroups.Remove(otherGroup);
			}
		}
	}
}
