using System.Collections.Generic;
using System.Linq;
using GlyssenEngine;
using GlyssenEngine.Character;
using GlyssenEngine.UndoActions;
using NUnit.Framework;
using SIL.Extensions;

namespace GlyssenEngineTests.UndoActionsTests
{
	[TestFixture]
	internal class MoveCharactersToGroupUndoActionTests
	{
		private Project m_testProject;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.MRK);
		}

		[SetUp]
		public void SetUp()
		{
			m_testProject.CharacterGroupList.CharacterGroups.Clear();
		}

		private CharacterGroup AddCharacterGroup(params string[] characterIds)
		{
			var group = new CharacterGroup(m_testProject);
			foreach (var character in characterIds)
				group.CharacterIds.Add(character);
			group.SetGroupIdLabel();
			m_testProject.CharacterGroupList.CharacterGroups.Add(group);
			return group;
		}

		[Test]
		public void Constructor_SourceAndDestHaveCharactersBeforeAndAfterMove_CharactersGetMovedNoGroupsDeleted()
		{
			var sourceGroup = AddCharacterGroup("Peter (Simon)", "John the Baptist", "rich young ruler");
			var destGroup = AddCharacterGroup("centurion at crucifixion", "man possessed by evil spirit", "John", "Pharisees");
			var estHoursOfSourceBeforeMove = sourceGroup.EstimatedHours;
			var estHoursOfDestBeforeMove = destGroup.EstimatedHours;

			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, destGroup, new List<string> { "rich young ruler" });

			Assert.IsTrue(action.GroupsAffectedByLastOperation.SequenceEqual(new[] { destGroup, sourceGroup }));
			Assert.IsTrue(sourceGroup.CharacterIds.SetEquals(new[] { "Peter (Simon)", "John the Baptist" }));
			Assert.IsTrue(destGroup.CharacterIds.SetEquals(new[] { "centurion at crucifixion", "man possessed by evil spirit", "John", "Pharisees", "rich young ruler" }));
			Assert.IsTrue(estHoursOfSourceBeforeMove > sourceGroup.EstimatedHours);
			Assert.IsTrue(estHoursOfDestBeforeMove < destGroup.EstimatedHours);
		}

		[Test]
		public void Constructor_AllCharactersMovedFromSourceWithNoAssignedActor_CharactersGetMovedAndSourceIsDeleted()
		{
			var charactersToMove = new[] { "Peter (Simon)", "John the Baptist", "rich young ruler" };
			var sourceGroup = AddCharacterGroup(charactersToMove);
			var destGroup = AddCharacterGroup("centurion at crucifixion", "man possessed by evil spirit", "John", "Pharisees");
			var estHoursOfDestBeforeMove = destGroup.EstimatedHours;

			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, destGroup, charactersToMove);

			Assert.AreEqual(destGroup, action.GroupsAffectedByLastOperation.Single());
			m_testProject.CharacterGroupList.CharacterGroups.SetEquals(new[] {destGroup});
			Assert.IsTrue(destGroup.CharacterIds.SetEquals(new[] { "centurion at crucifixion", "man possessed by evil spirit", "John", "Pharisees", "Peter (Simon)", "John the Baptist", "rich young ruler" }));
			Assert.IsTrue(estHoursOfDestBeforeMove < destGroup.EstimatedHours);
		}

		[Test]
		public void Constructor_NoDestSupplied_CharactersGetMovedToNewGroup()
		{
			var sourceGroup = AddCharacterGroup("Peter (Simon)", "John the Baptist", "rich young ruler", "centurion at crucifixion", "man possessed by evil spirit");
			var estHoursOfSourceBeforeMove = sourceGroup.EstimatedHours;
			var anotherGroup = AddCharacterGroup("John, third", "Pharisees");
			var charactersToMove = new List<string> { "rich young ruler", "man possessed by evil spirit" };
			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, null, charactersToMove);

			var newGroup = m_testProject.CharacterGroupList.GroupContainingCharacterId("rich young ruler");
			Assert.IsTrue(action.GroupsAffectedByLastOperation.SequenceEqual(new[] { newGroup, sourceGroup }));
			Assert.AreNotEqual(sourceGroup, newGroup);
			Assert.IsTrue(newGroup.CharacterIds.SetEquals(charactersToMove));
			Assert.IsTrue(sourceGroup.CharacterIds.SetEquals(new[] { "Peter (Simon)", "John the Baptist", "centurion at crucifixion" }));
			Assert.IsTrue(anotherGroup.CharacterIds.SetEquals(new[] { "John, third", "Pharisees" }));
			Assert.IsTrue(estHoursOfSourceBeforeMove > sourceGroup.EstimatedHours);
		}

		[Test]
		public void Constructor_NoDestSuppliedMoveAllCharactersFromSourceWithAssignedActor_CharactersGetMovedToNewGroupAndSourceLeftEmpty()
		{
			var charactersToMove = new [] { "Peter (Simon)", "John the Baptist", "rich young ruler", "centurion", "man possessed by evil spirit" };
			var sourceGroup = AddCharacterGroup(charactersToMove);
			sourceGroup.AssignVoiceActor(13);
			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, null, new List<string>(sourceGroup.CharacterIds));

			var newGroup = m_testProject.CharacterGroupList.GroupContainingCharacterId("rich young ruler");
			Assert.IsTrue(action.GroupsAffectedByLastOperation.SequenceEqual(new[] { newGroup, sourceGroup }));
			Assert.AreNotEqual(sourceGroup, newGroup);
			Assert.IsTrue(newGroup.CharacterIds.SetEquals(charactersToMove));
			Assert.AreEqual(0, sourceGroup.CharacterIds.Count);
			Assert.AreEqual(13, sourceGroup.VoiceActorId);
		}

		[Test]
		public void Description_MoveToGroupWithImplicitName_GroupReferencedByGeneratedLabelIdForUiDisplay()
		{
			var sourceGroup = AddCharacterGroup("Peter (Simon)", "John the Baptist", "rich young ruler");
			var destGroup = AddCharacterGroup("centurion at crucifixion", "man possessed by evil spirit", "John, third", "Pharisees");

			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, destGroup, new List<string> { "rich young ruler" });
			Assert.AreEqual("Move characters to Man 2 group", action.Description);
		}

		[Test]
		public void Description_MoveToGroupWithExplicitId_GroupReferencedById()
		{
			var sourceGroup = AddCharacterGroup("Peter (Simon)", "John the Baptist", "rich young ruler");
			var destGroup = AddCharacterGroup("centurion at crucifixion", "man possessed by evil spirit", "John", "Pharisees");
			destGroup.GroupIdLabel = CharacterGroup.Label.Other;
			destGroup.GroupIdOtherText = "Forty-three";

			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, destGroup, new List<string> { "rich young ruler" });
			Assert.AreEqual("Move characters to Forty-three group", action.Description);
		}

		[Test]
		public void Description_NoDestinationGroupSpecified_CreateNewGroup()
		{
			var sourceGroup = AddCharacterGroup("Peter (Simon)", "John the Baptist", "rich young ruler");

			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, null, new List<string> { "rich young ruler" });
			Assert.AreEqual("Create new group", action.Description);
		}

		[Test]
		public void Description_IsSplit_SplitGroup()
		{
			var sourceGroup = AddCharacterGroup("Peter (Simon)", "John the Baptist", "rich young ruler");

			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, null, new List<string> { "rich young ruler" });
			action.IsSplit = true;
			Assert.AreEqual("Split group", action.Description);
		}

		[Test]
		public void Undo_SourceAndDestHaveCharactersBeforeAndAfterMove_CharactersGetMovedBackToOriginalGroup()
		{
			var originalCharactersInSource = new[] { "Peter (Simon)", "John the Baptist", "rich young ruler", "people, some", "Jesus' family", "man possessed by evil spirit", "John", "Pharisees", "woman, bleeding for twelve years" };
			var originalCharactersInDest = new[] { "crowd", "Herodias' daughter" };
			var sourceGroup = AddCharacterGroup(originalCharactersInSource);
			var destGroup = AddCharacterGroup(originalCharactersInDest);
			var estHoursOfSourceBeforeMove = sourceGroup.EstimatedHours;
			var estHoursOfDestBeforeMove = destGroup.EstimatedHours;

			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, destGroup, new List<string> { "woman, bleeding for twelve years", "people, some" });

			Assert.IsTrue(action.Undo());
			Assert.IsTrue(sourceGroup.CharacterIds.SetEquals(originalCharactersInSource));
			Assert.IsTrue(action.GroupsAffectedByLastOperation.SequenceEqual(new [] { sourceGroup, destGroup }));
			Assert.AreEqual(2, m_testProject.CharacterGroupList.CharacterGroups.Count);
			Assert.IsTrue(sourceGroup.CharacterIds.SetEquals(originalCharactersInSource));
			Assert.IsTrue(destGroup.CharacterIds.SetEquals(originalCharactersInDest));
			Assert.AreEqual(estHoursOfDestBeforeMove, destGroup.EstimatedHours);
			Assert.AreEqual(estHoursOfSourceBeforeMove, sourceGroup.EstimatedHours);
		}

		[Test]
		public void Undo_AllCharactersMovedFromSourceWithNoAssignedActor_GroupRecreatedAndCharactersGetMovedBack()
		{
			var charactersToMove = new[] { "Peter (Simon)", "John the Baptist", "rich young ruler" };
			var originalCharactersInDest = new[] { "centurion at crucifixion", "man possessed by evil spirit", "John", "Pharisees" };
			var sourceGroup = AddCharacterGroup(charactersToMove);
			var destGroup = AddCharacterGroup(originalCharactersInDest);
			var estHoursOfSourceBeforeMove = sourceGroup.EstimatedHours;
			var estHoursOfDestBeforeMove = destGroup.EstimatedHours;
			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, destGroup, charactersToMove);
			Assert.AreEqual(1, m_testProject.CharacterGroupList.CharacterGroups.Count);

			Assert.IsTrue(action.Undo());
			var recreatedGroup = m_testProject.CharacterGroupList.GroupContainingCharacterId("rich young ruler");
			Assert.IsTrue(recreatedGroup.CharacterIds.SetEquals(charactersToMove));
			Assert.IsTrue(action.GroupsAffectedByLastOperation.SequenceEqual(new[] { recreatedGroup, destGroup }));
			Assert.AreEqual(2, m_testProject.CharacterGroupList.CharacterGroups.Count);
			Assert.IsTrue(recreatedGroup.CharacterIds.SetEquals(charactersToMove));
			Assert.IsTrue(destGroup.CharacterIds.SetEquals(originalCharactersInDest));
			Assert.AreEqual(estHoursOfDestBeforeMove, destGroup.EstimatedHours);
			Assert.AreEqual(estHoursOfSourceBeforeMove, recreatedGroup.EstimatedHours);
		}

		[Test]
		public void Undo_NoDestSuppliedMoveAllCharactersFromSourceWithAssignedActor_CharactersGetMovedBackToOriginalGroupAndNewGroupIsRemoved()
		{
			var actor = new GlyssenEngine.VoiceActor.VoiceActor { Id = 13 };
			m_testProject.VoiceActorList.AllActors.Add(actor);
			var originalCharactersInSource = new[] { "Peter (Simon)", "John the Baptist", "rich young ruler", "centurion at crucifixion", "man possessed by evil spirit", "John", "Pharisees" };
			var sourceGroup = AddCharacterGroup(originalCharactersInSource);
			sourceGroup.AssignVoiceActor(13);
			var estHoursOfSourceBeforeMove = sourceGroup.EstimatedHours;
			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, null, new List<string>(sourceGroup.CharacterIds));

			Assert.IsTrue(action.Undo());
			Assert.IsTrue(sourceGroup.CharacterIds.SetEquals(originalCharactersInSource));
			Assert.AreEqual(sourceGroup, action.GroupsAffectedByLastOperation.Single());
			Assert.AreEqual(1, m_testProject.CharacterGroupList.CharacterGroups.Count);
			Assert.AreEqual(sourceGroup, m_testProject.CharacterGroupList.GroupContainingCharacterId("rich young ruler"));
			Assert.AreEqual(estHoursOfSourceBeforeMove, sourceGroup.EstimatedHours);
		}

		[Test]
		public void Undo_Split_CharactersGetMovedBackToOriginalGroupAndNewGroupIsRemoved()
		{
			var originalCharactersInSource = new[] { "Peter (Simon)", "John the Baptist", "rich young ruler", "centurion", "man possessed by evil spirit", "John", "Pharisees" };
			var sourceGroup = AddCharacterGroup(originalCharactersInSource);
			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, null, new List<string> { "rich young ruler", "man possessed by evil spirit", "John" });
			action.IsSplit = true;

			Assert.IsTrue(action.Undo());
			Assert.IsTrue(sourceGroup.CharacterIds.SetEquals(originalCharactersInSource));
			Assert.AreEqual(sourceGroup, action.GroupsAffectedByLastOperation.Single());
			Assert.AreEqual(1, m_testProject.CharacterGroupList.CharacterGroups.Count);
			Assert.AreEqual(sourceGroup, m_testProject.CharacterGroupList.GroupContainingCharacterId("rich young ruler"));
		}

		[Test]
		public void Redo_SourceAndDestHaveCharactersBeforeAndAfterMove_CharactersGetMovedToDestNoGroupsDeleted()
		{
			var originalCharactersInSource = new[] { "Peter (Simon)", "John the Baptist", "rich young ruler", "Pilate", "Jesus' family", "man possessed by evil spirit", "John", "Pharisees", "passers by" };
			var originalCharactersInDest = new[] { "crowd", "woman, bleeding for twelve years" };
			var sourceGroup = AddCharacterGroup(originalCharactersInSource);
			var destGroup = AddCharacterGroup(originalCharactersInDest);
			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, destGroup, new List<string> { "passers by", "Pilate" });

			Assert.IsTrue(action.Undo());
			var estHoursOfSourceBeforeRedo = sourceGroup.EstimatedHours;
			var estHoursOfDestBeforeRedo = destGroup.EstimatedHours;
			Assert.IsTrue(action.Redo());

			Assert.IsTrue(action.GroupsAffectedByLastOperation.SequenceEqual(new[] { destGroup, sourceGroup }));
			Assert.IsTrue(sourceGroup.CharacterIds.SetEquals(new[] { "Peter (Simon)", "John the Baptist", "rich young ruler", "Jesus' family", "man possessed by evil spirit", "John", "Pharisees" }));
			Assert.IsTrue(destGroup.CharacterIds.SetEquals(new[] { "crowd", "woman, bleeding for twelve years", "Pilate", "passers by" }));
			Assert.AreEqual(2, m_testProject.CharacterGroupList.CharacterGroups.Count);
			Assert.IsTrue(estHoursOfSourceBeforeRedo > sourceGroup.EstimatedHours);
			Assert.IsTrue(estHoursOfDestBeforeRedo < destGroup.EstimatedHours);
		}

		[Test]
		public void Redo_AllCharactersMovedFromSourceWithNoAssignedActor_GroupRecreatedAndCharactersGetMovedBack()
		{
			var charactersToMove = new[] { "Peter (Simon)", "John the Baptist", "rich young ruler" };
			var originalCharactersInDest = new[] { "centurion at crucifixion", "man possessed by evil spirit", "John", "Pharisees" };
			var sourceGroup = AddCharacterGroup(charactersToMove);
			var destGroup = AddCharacterGroup(originalCharactersInDest);
			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, destGroup, charactersToMove);
			Assert.AreEqual(1, m_testProject.CharacterGroupList.CharacterGroups.Count);

			Assert.IsTrue(action.Undo());
			var estHoursOfDestBeforeRedo = destGroup.EstimatedHours;
			Assert.IsTrue(action.Redo());

			Assert.AreEqual(destGroup, action.GroupsAffectedByLastOperation.Single());
			m_testProject.CharacterGroupList.CharacterGroups.SetEquals(new[] { destGroup });
			Assert.IsTrue(destGroup.CharacterIds.SetEquals(new[] { "centurion at crucifixion", "man possessed by evil spirit", "John", "Pharisees", "Peter (Simon)", "John the Baptist", "rich young ruler" }));
			Assert.IsTrue(estHoursOfDestBeforeRedo < destGroup.EstimatedHours);
		}

		[Test]
		public void Redo_NoDestSuppliedMoveAllCharactersFromSourceWithAssignedActor_CharactersGetMovedBackToOriginalGroupAndNewGroupIsRemoved()
		{
			var actor = new GlyssenEngine.VoiceActor.VoiceActor { Id = 13 };
			m_testProject.VoiceActorList.AllActors.Add(actor);
			var charactersToMove = new[] { "Peter (Simon)", "John the Baptist", "rich young ruler", "centurion at crucifixion", "man possessed by evil spirit", "John", "Pharisees" };
			var sourceGroup = AddCharacterGroup(charactersToMove);
			sourceGroup.AssignVoiceActor(13);
			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, null, new List<string>(sourceGroup.CharacterIds));

			Assert.IsTrue(action.Undo());
			var estHoursOfSourceBeforeRedo = sourceGroup.EstimatedHours;
			Assert.IsTrue(action.Redo());

			var newGroup = m_testProject.CharacterGroupList.GroupContainingCharacterId("rich young ruler");
			Assert.IsTrue(action.GroupsAffectedByLastOperation.SequenceEqual(new[] { newGroup, sourceGroup }));
			Assert.AreNotEqual(sourceGroup, newGroup);
			Assert.IsTrue(newGroup.CharacterIds.SetEquals(charactersToMove));
			Assert.AreEqual(0, sourceGroup.CharacterIds.Count);
			Assert.AreEqual(13, sourceGroup.VoiceActorId);
			Assert.IsTrue(estHoursOfSourceBeforeRedo > sourceGroup.EstimatedHours);
		}

		[Test]
		public void Redo_Split_CharactersGetMovedBackToOriginalGroupAndNewGroupIsRemoved()
		{
			var originalCharactersInSource = new[] { "Peter (Simon)", "John the Baptist", "rich young ruler", "Jesus' family", "man possessed by evil spirit", "John", "Pharisees" };
			var charactersToMove = new List<string> { "rich young ruler", "man possessed by evil spirit" };
			var sourceGroup = AddCharacterGroup(originalCharactersInSource);
			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, null, new List<string>(charactersToMove));
			action.IsSplit = true;

			Assert.IsTrue(action.Undo());
			var estHoursOfSourceBeforeRedo = sourceGroup.EstimatedHours;
			Assert.IsTrue(action.Redo());

			var newGroup = m_testProject.CharacterGroupList.GroupContainingCharacterId("rich young ruler");
			Assert.IsTrue(action.GroupsAffectedByLastOperation.SequenceEqual(new[] { newGroup, sourceGroup }));
			Assert.AreNotEqual(sourceGroup, newGroup);
			Assert.IsTrue(newGroup.CharacterIds.SetEquals(charactersToMove));
			Assert.IsTrue(sourceGroup.CharacterIds.SetEquals(new[] { "Peter (Simon)", "John the Baptist", "Jesus' family", "John", "Pharisees" }));
			Assert.IsTrue(estHoursOfSourceBeforeRedo > sourceGroup.EstimatedHours);
		}

		[Test]
		public void Undo_AssignCameoRole_CameoGroupNotRemoved()
		{
			// list of characters
			var originalCharactersInSource = new[] { "Peter (Simon)", "woman, bleeding for twelve years" };
			var sourceGroup = AddCharacterGroup(originalCharactersInSource);

			// set up cameo actor and group
			var cameoActor = new GlyssenEngine.VoiceActor.VoiceActor { Id = 1, Name = "Missy Cameo", IsCameo = true };
			m_testProject.VoiceActorList.AllActors.Add(cameoActor);
			var cameoGroup = AddCharacterGroup();
			cameoGroup.AssignVoiceActor(1);

			// should be 2 groups now
			Assert.AreEqual(2, m_testProject.CharacterGroupList.CharacterGroups.Count);

			var estHoursOfSourceBeforeMove = sourceGroup.EstimatedHours;
			Assert.AreEqual(0, cameoGroup.EstimatedHours);

			// assign the character role "woman, bleeding for twelve years" to the cameo actor
			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, cameoGroup, new List<string> { "woman, bleeding for twelve years" });

			// should still be 2 groups
			Assert.AreEqual(2, m_testProject.CharacterGroupList.CharacterGroups.Count);

			// "woman, bleeding for twelve years" should be in the cameo group now
			Assert.IsTrue(m_testProject.CharacterGroupList.GroupContainingCharacterId("woman, bleeding for twelve years").AssignedToCameoActor);

			// undo
			Assert.IsTrue(action.Undo());

			// should still be 2 groups
			Assert.AreEqual(2, m_testProject.CharacterGroupList.CharacterGroups.Count);

			// "woman, bleeding for twelve years" should be back in the original group
			Assert.IsFalse(m_testProject.CharacterGroupList.GroupContainingCharacterId("woman, bleeding for twelve years").AssignedToCameoActor);
			Assert.AreEqual(estHoursOfSourceBeforeMove, sourceGroup.EstimatedHours);
			Assert.AreEqual(0, cameoGroup.EstimatedHours);
		}
	}
}