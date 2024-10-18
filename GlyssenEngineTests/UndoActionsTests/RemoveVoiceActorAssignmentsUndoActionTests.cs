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
	internal class RemoveVoiceActorAssignmentsUndoActionTests
	{
		private Project m_testProject;
			
		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.MRK);

			var actor1 = new VoiceActor {Id = 1, Name = "Oneyda Figueroa"};
			var actor2 = new VoiceActor {Id = 2, Name = "Paul Twomey"};
			var actor3 = new VoiceActor {Id = 3, Name = "Threesa Hawkins"};
			m_testProject.VoiceActorList.AllActors = new List<VoiceActor> {actor1, actor2, actor3};

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
			Assert.That(groupWithJesus.IsVoiceActorAssigned, Is.False);
			Assert.That(groupWithJesus, Is.EqualTo(action.GroupsAffectedByLastOperation.Single()));
		}

		[Test]
		public void Constructor_SingleGroupWithNoCharacterIds_RemovesTheGroup()
		{
			AddCharacterGroup();
			var emptyGroup = m_testProject.CharacterGroupList.CharacterGroups.Last();
			emptyGroup.AssignVoiceActor(2);
			var action = new RemoveVoiceActorAssignmentsUndoAction(m_testProject, emptyGroup);
			Assert.That(m_testProject.CharacterGroupList.CharacterGroups.Contains(emptyGroup), Is.False);
			Assert.That(action.GroupsAffectedByLastOperation.Any(), Is.False);
		}

		[Test]
		public void Constructor_MultipleGroups_RemovesTheVoiceActorForAllGroups()
		{
			m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus").AssignVoiceActor(3);
			m_testProject.CharacterGroupList.GetGroupById("Crusty Old Dudes").AssignVoiceActor(2);
			m_testProject.CharacterGroupList.GroupContainingCharacterId("Mary, Jesus' mother").AssignVoiceActor(1);
			var action = new RemoveVoiceActorAssignmentsUndoAction(m_testProject, m_testProject.CharacterGroupList.CharacterGroups);
			Assert.That(m_testProject.CharacterGroupList.CharacterGroups.Any(g => g.IsVoiceActorAssigned), Is.False);
			Assert.That(action.GroupsAffectedByLastOperation.Count(), Is.EqualTo(3));
			Assert.That(action.GroupsAffectedByLastOperation.Intersect(m_testProject.CharacterGroupList.CharacterGroups).Count(), Is.EqualTo(3));
		}

		[Test]
		public void Description_UnassignmentOfSingleGroupWithSingleCharacter_DescriptionRefersToCharacterNotGroup()
		{
			var groupWithJesus = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
			groupWithJesus.AssignVoiceActor(2);
			var action = new RemoveVoiceActorAssignmentsUndoAction(m_testProject, groupWithJesus);
			Assert.That(action.Description, Is.EqualTo("Remove voice actor assignment for Jesus"));
		}

		[Test]
		public void Description_UnassignmentOfSingleGroupWithMultipleCharacters_DescriptionRefersToGroup()
		{
			var groupWithMoses = m_testProject.CharacterGroupList.GroupContainingCharacterId("Moses");
			groupWithMoses.AssignVoiceActor(2);
			var action = new RemoveVoiceActorAssignmentsUndoAction(m_testProject, groupWithMoses);
			Assert.That(action.Description, Is.EqualTo("Remove voice actor assignment for Crusty Old Dudes group"));
		}

		[Test]
		public void Description_UnassignmentOfSingleGroupWithNoCharacters_DescriptionDoesNotReferToGroup()
		{
			AddCharacterGroup();
			var emptygroup = m_testProject.CharacterGroupList.CharacterGroups.Last();
			emptygroup.AssignVoiceActor(2);
			var action = new RemoveVoiceActorAssignmentsUndoAction(m_testProject, emptygroup);
			Assert.That(action.Description, Is.EqualTo("Remove voice actor assignment"));
		}

		[Test]
		public void Description_UnassignmentOfMultipleGroups_DescriptionRefersToGroupsGenerically()
		{
			m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus").AssignVoiceActor(3);
			m_testProject.CharacterGroupList.GetGroupById("Crusty Old Dudes").AssignVoiceActor(2);
			m_testProject.CharacterGroupList.GroupContainingCharacterId("Mary, Jesus' mother").AssignVoiceActor(1);
			var action = new RemoveVoiceActorAssignmentsUndoAction(m_testProject, m_testProject.CharacterGroupList.CharacterGroups);
			Assert.That(action.Description, Is.EqualTo("Remove voice actor assignment for multiple groups"));
		}

		[Test]
		public void Undo_SingleGroupNotFound_NoChangeAndReturnsFalse()
		{
			var groupWithMoses = m_testProject.CharacterGroupList.GroupContainingCharacterId("Moses");
			groupWithMoses.AssignVoiceActor(2);
			var action = new RemoveVoiceActorAssignmentsUndoAction(m_testProject, groupWithMoses);
			Assert.That(groupWithMoses.IsVoiceActorAssigned, Is.False);
			groupWithMoses.GroupIdOtherText = "Elderly men";
			Assert.That(action.Undo(), Is.False);
			Assert.That(groupWithMoses.IsVoiceActorAssigned, Is.False);
			Assert.That(action.GroupsAffectedByLastOperation.Count(), Is.EqualTo(0));
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
			Assert.That(action.Undo(), Is.False);
			Assert.That(m_testProject.CharacterGroupList.CharacterGroups.Any(g => g.IsVoiceActorAssigned), Is.False);
			Assert.That(action.GroupsAffectedByLastOperation.Count(), Is.EqualTo(0));
		}

		[Test]
		public void Undo_NoPreviousAssignment_NoChangeAndReturnsTrue()
		{
			var groupWithJesus = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
			groupWithJesus.RemoveVoiceActor();
			var action = new RemoveVoiceActorAssignmentsUndoAction(m_testProject, groupWithJesus);
			Assert.That(action.Undo(), Is.True);
			Assert.That(groupWithJesus.IsVoiceActorAssigned, Is.False);
			Assert.That(action.GroupsAffectedByLastOperation.Count(), Is.EqualTo(0));
		}

		[Test]
		public void Undo_SingleGroupPreviousAssignment_AssignmentRestored()
		{
			var groupWithJesus = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
			groupWithJesus.AssignVoiceActor(3);
			var action = new RemoveVoiceActorAssignmentsUndoAction(m_testProject, groupWithJesus);
			Assert.That(action.Undo(), Is.True);
			Assert.That(groupWithJesus.VoiceActorId, Is.EqualTo(3));
			Assert.That(groupWithJesus, Is.EqualTo(action.GroupsAffectedByLastOperation.Single()));
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
			Assert.That(action.Undo(), Is.True);
			Assert.That(groupWithJesus.VoiceActorId, Is.EqualTo(3));
			Assert.That(groupWithMoses.VoiceActorId, Is.EqualTo(2));
			Assert.That(groupWithMary.VoiceActorId, Is.EqualTo(1));
			Assert.That(action.GroupsAffectedByLastOperation.Count(), Is.EqualTo(3));
			Assert.That(action.GroupsAffectedByLastOperation.Intersect(m_testProject.CharacterGroupList.CharacterGroups).Count(), Is.EqualTo(3));
		}

		[Test]
		public void Undo_UnassignmentOfSingleGroupWithNoCharacters_EmptyGroupReAddedAndAssignedToOriginalActor()
		{
			AddCharacterGroup();
			var emptygroup = m_testProject.CharacterGroupList.CharacterGroups.Last();
			emptygroup.AssignVoiceActor(2);
			var wasCameo = emptygroup.AssignedToCameoActor;
			var action = new RemoveVoiceActorAssignmentsUndoAction(m_testProject, emptygroup);
			Assert.That(action.Undo(), Is.True);
			var restoredGroup = m_testProject.CharacterGroupList.GetGroupsAssignedToActor(2).Single();
			Assert.That(restoredGroup.CharacterIds.Any(), Is.False);
			Assert.That(restoredGroup.EstimatedHours, Is.EqualTo(0));
			Assert.That(restoredGroup.AttributesDisplay, Is.Not.Null);
			Assert.That(wasCameo, Is.EqualTo(restoredGroup.AssignedToCameoActor));
		}

		[Test]
		public void Redo_SingleGroupNotFound_NoChangeAndReturnsFalse()
		{
			var groupWithMoses = m_testProject.CharacterGroupList.GroupContainingCharacterId("Moses");
			groupWithMoses.AssignVoiceActor(2);
			var action = new RemoveVoiceActorAssignmentsUndoAction(m_testProject, groupWithMoses);
			action.Undo();
			Assert.That(groupWithMoses.VoiceActorId, Is.EqualTo(2));
			groupWithMoses.GroupIdOtherText = "Elderly men";
			Assert.That(action.Redo(), Is.False);
			Assert.That(groupWithMoses.VoiceActorId, Is.EqualTo(2));
			Assert.That(action.GroupsAffectedByLastOperation.Count(), Is.EqualTo(0));
		}

		[Test]
		public void Redo_SingleGroupNameChangedToBeExplicit_RedoFails()
		{
			var groupWithMary = m_testProject.CharacterGroupList.GroupContainingCharacterId("Mary, Jesus' mother");
			groupWithMary.AssignVoiceActor(1);
			var action = new RemoveVoiceActorAssignmentsUndoAction(m_testProject, groupWithMary);
			action.Undo();
			Assert.That(groupWithMary.VoiceActorId, Is.EqualTo(1));
			groupWithMary.GroupIdLabel = CharacterGroup.Label.Other;
			groupWithMary.GroupIdOtherText = "Ladies";
			Assert.That(action.Redo(), Is.False);
			Assert.That(groupWithMary.IsVoiceActorAssigned, Is.True);
			Assert.That(action.GroupsAffectedByLastOperation.Any(), Is.False);
		}

		[Test]
		public void Redo_NoPreviousAssignment_NoChangeAndReturnsTrue()
		{
			var groupWithJesus = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
			groupWithJesus.RemoveVoiceActor();
			var action = new RemoveVoiceActorAssignmentsUndoAction(m_testProject, groupWithJesus);
			action.Undo();
			Assert.That(action.Redo(), Is.True);
			Assert.That(groupWithJesus.IsVoiceActorAssigned, Is.False);
			Assert.That(action.GroupsAffectedByLastOperation.Count(), Is.EqualTo(0));
		}

		[Test]
		public void Redo_SingleGroupPreviousAssignment_AssignmentCleared()
		{
			var groupWithJesus = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
			groupWithJesus.AssignVoiceActor(3);
			var action = new RemoveVoiceActorAssignmentsUndoAction(m_testProject, groupWithJesus);
			action.Undo();
			Assert.That(action.Redo(), Is.True);
			Assert.That(groupWithJesus.IsVoiceActorAssigned, Is.False);
			Assert.That(groupWithJesus, Is.EqualTo(action.GroupsAffectedByLastOperation.Single()));
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
			Assert.That(action.Redo(), Is.True);
			Assert.That(groupWithJesus.IsVoiceActorAssigned, Is.False);
			Assert.That(groupWithMoses.IsVoiceActorAssigned, Is.False);
			Assert.That(groupWithMary.IsVoiceActorAssigned, Is.True);
			Assert.That(action.GroupsAffectedByLastOperation.Count(), Is.EqualTo(2));
			Assert.That(action.GroupsAffectedByLastOperation.Contains(groupWithJesus), Is.True);
			Assert.That(action.GroupsAffectedByLastOperation.Contains(groupWithMoses), Is.True);
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
			Assert.That(action.Redo(), Is.True);
			Assert.That(m_testProject.CharacterGroupList.GetGroupsAssignedToActor(2).Any(), Is.False);
			Assert.That(countAfterUndo - 1, Is.EqualTo(m_testProject.CharacterGroupList.CharacterGroups.Count));
		}
	}
}
