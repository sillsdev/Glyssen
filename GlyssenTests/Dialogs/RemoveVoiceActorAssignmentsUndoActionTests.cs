using System.Collections.Generic;
using System.Linq;
using Glyssen.Dialogs;
using GlyssenEngine;
using GlyssenEngine.Character;
using NUnit.Framework;

namespace GlyssenTests.Dialogs
{
	[TestFixture]
	internal class RemoveVoiceActorAssignmentsUndoActionTests
	{
		private Project m_testProject;
			
		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.MRK);

			var actor1 = new GlyssenEngine.VoiceActor.VoiceActor {Id = 1, Name = "Oneyda Figueroa"};
			var actor2 = new GlyssenEngine.VoiceActor.VoiceActor {Id = 2, Name = "Paul Twomey"};
			var actor3 = new GlyssenEngine.VoiceActor.VoiceActor {Id = 3, Name = "Threesa Hawkins"};
			m_testProject.VoiceActorList.AllActors = new List<GlyssenEngine.VoiceActor.VoiceActor> {actor1, actor2, actor3};

			AddCharacterGroup("Jesus");
			AddCharacterGroup("Mary, Jesus' mother", "Rhoda", "Rahab");
			AddCharacterGroup("Moses", "Caiaphas", "centurion at crucifixion");
		}

		[SetUp]
		public void Setup()
		{
			foreach (var group in m_testProject.CharacterGroupList.CharacterGroups)
				group.RemoveVoiceActor();

			var mosesGroup = m_testProject.CharacterGroupList.GroupContainingCharacterId("Moses");
			mosesGroup.GroupIdLabel = CharacterGroup.Label.Other;
			mosesGroup.GroupIdOtherText = "Crusty Old Dudes";
		}

		private void AddCharacterGroup(params string[] characterIds)
		{
			var group = new CharacterGroup(m_testProject);
			foreach (var character in characterIds)
				group.CharacterIds.Add(character);
			group.SetGroupIdLabel();
			m_testProject.CharacterGroupList.CharacterGroups.Add(group);
		}

		[Test]
		public void Constructor_SingleGroup_RemovesTheVoiceActor()
		{
			var groupWithJesus = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
			groupWithJesus.AssignVoiceActor(2);
			var action = new RemoveVoiceActorAssignmentsUndoAction(m_testProject, groupWithJesus);
			Assert.False(groupWithJesus.IsVoiceActorAssigned);
			Assert.AreEqual(groupWithJesus, action.GroupsAffectedByLastOperation.Single());
		}

		[Test]
		public void Constructor_SingleGroupWithNoCharacterIds_RemovesTheGroup()
		{
			AddCharacterGroup();
			var emptyGroup = m_testProject.CharacterGroupList.CharacterGroups.Last();
			emptyGroup.AssignVoiceActor(2);
			var action = new RemoveVoiceActorAssignmentsUndoAction(m_testProject, emptyGroup);
			Assert.IsFalse(m_testProject.CharacterGroupList.CharacterGroups.Contains(emptyGroup));
			Assert.IsFalse(action.GroupsAffectedByLastOperation.Any());
		}

		[Test]
		public void Constructor_MultipleGroups_RemovesTheVoiceActorForAllGroups()
		{
			m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus").AssignVoiceActor(3);
			m_testProject.CharacterGroupList.GetGroupById("Crusty Old Dudes").AssignVoiceActor(2);
			m_testProject.CharacterGroupList.GroupContainingCharacterId("Mary, Jesus' mother").AssignVoiceActor(1);
			var action = new RemoveVoiceActorAssignmentsUndoAction(m_testProject, m_testProject.CharacterGroupList.CharacterGroups);
			Assert.IsFalse(m_testProject.CharacterGroupList.CharacterGroups.Any(g => g.IsVoiceActorAssigned));
			Assert.AreEqual(3, action.GroupsAffectedByLastOperation.Count());
			Assert.AreEqual(3, action.GroupsAffectedByLastOperation.Intersect(m_testProject.CharacterGroupList.CharacterGroups).Count());
		}

		[Test]
		public void Description_UnassignmentOfSingleGroupWithSingleCharacter_DescriptionRefersToCharacterNotGroup()
		{
			var groupWithJesus = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
			groupWithJesus.AssignVoiceActor(2);
			var action = new RemoveVoiceActorAssignmentsUndoAction(m_testProject, groupWithJesus);
			Assert.AreEqual("Remove voice actor assignment for Jesus", action.Description);
		}

		[Test]
		public void Description_UnassignmentOfSingleGroupWithMultipleCharacters_DescriptionRefersToGroup()
		{
			var groupWithMoses = m_testProject.CharacterGroupList.GroupContainingCharacterId("Moses");
			groupWithMoses.AssignVoiceActor(2);
			var action = new RemoveVoiceActorAssignmentsUndoAction(m_testProject, groupWithMoses);
			Assert.AreEqual("Remove voice actor assignment for Crusty Old Dudes group", action.Description);
		}

		[Test]
		public void Description_UnassignmentOfSingleGroupWithNoCharacters_DescriptionDoesNotReferToGroup()
		{
			AddCharacterGroup();
			var emptygroup = m_testProject.CharacterGroupList.CharacterGroups.Last();
			emptygroup.AssignVoiceActor(2);
			var action = new RemoveVoiceActorAssignmentsUndoAction(m_testProject, emptygroup);
			Assert.AreEqual("Remove voice actor assignment", action.Description);
		}

		[Test]
		public void Description_UnassignmentOfMultipleGroups_DescriptionRefersToGroupsGenerically()
		{
			m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus").AssignVoiceActor(3);
			m_testProject.CharacterGroupList.GetGroupById("Crusty Old Dudes").AssignVoiceActor(2);
			m_testProject.CharacterGroupList.GroupContainingCharacterId("Mary, Jesus' mother").AssignVoiceActor(1);
			var action = new RemoveVoiceActorAssignmentsUndoAction(m_testProject, m_testProject.CharacterGroupList.CharacterGroups);
			Assert.AreEqual("Remove voice actor assignment for multiple groups", action.Description);
		}

		[Test]
		public void Undo_SingleGroupNotFound_NoChangeAndReturnsFalse()
		{
			var groupWithMoses = m_testProject.CharacterGroupList.GroupContainingCharacterId("Moses");
			groupWithMoses.AssignVoiceActor(2);
			var action = new RemoveVoiceActorAssignmentsUndoAction(m_testProject, groupWithMoses);
			Assert.IsFalse(groupWithMoses.IsVoiceActorAssigned);
			groupWithMoses.GroupIdOtherText = "Elderly men";
			Assert.IsFalse(action.Undo());
			Assert.IsFalse(groupWithMoses.IsVoiceActorAssigned);
			Assert.AreEqual(0, action.GroupsAffectedByLastOperation.Count());
		}

		[Test]
		public void Undo_MultipleGroupsWithOneNotFound_NoChangeAndReturnsFalse()
		{
			m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus").AssignVoiceActor(3);
			var groupWithMoses = m_testProject.CharacterGroupList.GetGroupById("Crusty Old Dudes");
			groupWithMoses.AssignVoiceActor(2);
			m_testProject.CharacterGroupList.GroupContainingCharacterId("Mary, Jesus' mother").AssignVoiceActor(1);
			var action = new RemoveVoiceActorAssignmentsUndoAction(m_testProject, m_testProject.CharacterGroupList.CharacterGroups);

			groupWithMoses.GroupIdOtherText = null;
			Assert.IsFalse(action.Undo());
			Assert.IsFalse(m_testProject.CharacterGroupList.CharacterGroups.Any(g => g.IsVoiceActorAssigned));
			Assert.AreEqual(0, action.GroupsAffectedByLastOperation.Count());
		}

		[Test]
		public void Undo_NoPreviousAssignment_NoChangeAndReturnsTrue()
		{
			var groupWithJesus = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
			groupWithJesus.RemoveVoiceActor();
			var action = new RemoveVoiceActorAssignmentsUndoAction(m_testProject, groupWithJesus);
			Assert.IsTrue(action.Undo());
			Assert.IsFalse(groupWithJesus.IsVoiceActorAssigned);
			Assert.AreEqual(0, action.GroupsAffectedByLastOperation.Count());
		}

		[Test]
		public void Undo_SingleGroupPreviousAssignment_AssignmentRestored()
		{
			var groupWithJesus = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
			groupWithJesus.AssignVoiceActor(3);
			var action = new RemoveVoiceActorAssignmentsUndoAction(m_testProject, groupWithJesus);
			Assert.IsTrue(action.Undo());
			Assert.AreEqual(3, groupWithJesus.VoiceActorId);
			Assert.AreEqual(groupWithJesus, action.GroupsAffectedByLastOperation.Single());
		}

		[Test]
		public void Undo_MultipleGroupsPreviousAssignments_AssignmentsRestored()
		{
			var groupWithJesus = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
			var groupWithMoses = m_testProject.CharacterGroupList.GetGroupById("Crusty Old Dudes");
			var groupWithMary = m_testProject.CharacterGroupList.GroupContainingCharacterId("Mary, Jesus' mother");
			groupWithJesus.AssignVoiceActor(3);
			groupWithMoses.AssignVoiceActor(2);
			groupWithMary.AssignVoiceActor(1);
			var action = new RemoveVoiceActorAssignmentsUndoAction(m_testProject, m_testProject.CharacterGroupList.CharacterGroups);
			Assert.IsTrue(action.Undo());
			Assert.AreEqual(3, groupWithJesus.VoiceActorId);
			Assert.AreEqual(2, groupWithMoses.VoiceActorId);
			Assert.AreEqual(1, groupWithMary.VoiceActorId);
			Assert.AreEqual(3, action.GroupsAffectedByLastOperation.Count());
			Assert.AreEqual(3, action.GroupsAffectedByLastOperation.Intersect(m_testProject.CharacterGroupList.CharacterGroups).Count());
		}

		[Test]
		public void Undo_UnassignmentOfSingleGroupWithNoCharacters_EmptyGroupReAddedAndAssignedToOriginalActor()
		{
			AddCharacterGroup();
			var emptygroup = m_testProject.CharacterGroupList.CharacterGroups.Last();
			emptygroup.AssignVoiceActor(2);
			var wasCameo = emptygroup.AssignedToCameoActor;
			var action = new RemoveVoiceActorAssignmentsUndoAction(m_testProject, emptygroup);
			Assert.IsTrue(action.Undo());
			var restoredGroup = m_testProject.CharacterGroupList.GetGroupsAssignedToActor(2).Single();
			Assert.IsFalse(restoredGroup.CharacterIds.Any());
			Assert.AreEqual(0, restoredGroup.EstimatedHours);
			Assert.IsNotNull(restoredGroup.AttributesDisplay);
			Assert.AreEqual(wasCameo, restoredGroup.AssignedToCameoActor);
		}

		[Test]
		public void Redo_SingleGroupNotFound_NoChangeAndReturnsFalse()
		{
			var groupWithMoses = m_testProject.CharacterGroupList.GroupContainingCharacterId("Moses");
			groupWithMoses.AssignVoiceActor(2);
			var action = new RemoveVoiceActorAssignmentsUndoAction(m_testProject, groupWithMoses);
			action.Undo();
			Assert.AreEqual(2, groupWithMoses.VoiceActorId);
			groupWithMoses.GroupIdOtherText = "Elderly men";
			Assert.IsFalse(action.Redo());
			Assert.AreEqual(2, groupWithMoses.VoiceActorId);
			Assert.AreEqual(0, action.GroupsAffectedByLastOperation.Count());
		}

		[Test]
		public void Redo_SingleGroupNameChangedToBeExplicit_RedoFails()
		{
			var groupWithMary = m_testProject.CharacterGroupList.GroupContainingCharacterId("Mary, Jesus' mother");
			groupWithMary.AssignVoiceActor(1);
			var action = new RemoveVoiceActorAssignmentsUndoAction(m_testProject, groupWithMary);
			action.Undo();
			Assert.AreEqual(1, groupWithMary.VoiceActorId);
			groupWithMary.GroupIdLabel = CharacterGroup.Label.Other;
			groupWithMary.GroupIdOtherText = "Ladies";
			Assert.IsFalse(action.Redo());
			Assert.IsTrue(groupWithMary.IsVoiceActorAssigned);
			Assert.IsFalse(action.GroupsAffectedByLastOperation.Any());
		}

		[Test]
		public void Redo_NoPreviousAssignment_NoChangeAndReturnsTrue()
		{
			var groupWithJesus = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
			groupWithJesus.RemoveVoiceActor();
			var action = new RemoveVoiceActorAssignmentsUndoAction(m_testProject, groupWithJesus);
			action.Undo();
			Assert.IsTrue(action.Redo());
			Assert.IsFalse(groupWithJesus.IsVoiceActorAssigned);
			Assert.AreEqual(0, action.GroupsAffectedByLastOperation.Count());
		}

		[Test]
		public void Redo_SingleGroupPreviousAssignment_AssignmentCleared()
		{
			var groupWithJesus = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
			groupWithJesus.AssignVoiceActor(3);
			var action = new RemoveVoiceActorAssignmentsUndoAction(m_testProject, groupWithJesus);
			action.Undo();
			Assert.IsTrue(action.Redo());
			Assert.IsFalse(groupWithJesus.IsVoiceActorAssigned);
			Assert.AreEqual(groupWithJesus, action.GroupsAffectedByLastOperation.Single());
		}

		[Test]
		public void Redo_MultipleGroupsPreviousAssignments_AssignmentsCleared()
		{
			var groupWithJesus = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
			groupWithJesus.AssignVoiceActor(3);
			var groupWithMoses = m_testProject.CharacterGroupList.GetGroupById("Crusty Old Dudes");
			groupWithMoses.AssignVoiceActor(2);
			var groupWithMary = m_testProject.CharacterGroupList.GroupContainingCharacterId("Mary, Jesus' mother");
			groupWithMary.AssignVoiceActor(1);
			var action = new RemoveVoiceActorAssignmentsUndoAction(m_testProject, new [] { groupWithJesus, groupWithMoses });
			action.Undo();
			Assert.IsTrue(action.Redo());
			Assert.IsFalse(groupWithJesus.IsVoiceActorAssigned);
			Assert.IsFalse(groupWithMoses.IsVoiceActorAssigned);
			Assert.IsTrue(groupWithMary.IsVoiceActorAssigned);
			Assert.AreEqual(2, action.GroupsAffectedByLastOperation.Count());
			Assert.IsTrue(action.GroupsAffectedByLastOperation.Contains(groupWithJesus));
			Assert.IsTrue(action.GroupsAffectedByLastOperation.Contains(groupWithMoses));
		}

		[Test]
		public void Redo_UnassignmentOfSingleGroupWithNoCharacters_EmptyGroupRemoved()
		{
			AddCharacterGroup();
			var emptygroup = m_testProject.CharacterGroupList.CharacterGroups.Last();
			emptygroup.AssignVoiceActor(2);
			var action = new RemoveVoiceActorAssignmentsUndoAction(m_testProject, emptygroup);
			action.Undo();
			int countAfterUndo = m_testProject.CharacterGroupList.CharacterGroups.Count;
			Assert.IsTrue(action.Redo());
			Assert.IsFalse(m_testProject.CharacterGroupList.GetGroupsAssignedToActor(2).Any());
			Assert.AreEqual(countAfterUndo - 1, m_testProject.CharacterGroupList.CharacterGroups.Count);
		}
	}
}
