using System.Collections.Generic;
using System.Linq;
using GlyssenEngine;
using GlyssenEngine.Character;
using GlyssenEngine.UndoActions;
using GlyssenEngine.Casting;
using NUnit.Framework;
using SIL.Extensions;

namespace GlyssenEngineTests.UndoActionsTests
{
	[TestFixture]
	internal class MoveCharactersToGroupUndoActionTests
	{
		private Project m_testProject;

		[OneTimeSetUp]
		public void OneTimeSetUp()
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

			Assert.That(action.GroupsAffectedByLastOperation.SequenceEqual(new[] { destGroup, sourceGroup }), Is.True);
			Assert.That(sourceGroup.CharacterIds.SetEquals(new[] { "Peter (Simon)", "John the Baptist" }), Is.True);
			Assert.That(destGroup.CharacterIds.SetEquals(new[] { "centurion at crucifixion", "man possessed by evil spirit", "John", "Pharisees", "rich young ruler" }), Is.True);
			Assert.That(estHoursOfSourceBeforeMove > sourceGroup.EstimatedHours, Is.True);
			Assert.That(estHoursOfDestBeforeMove < destGroup.EstimatedHours, Is.True);
		}

		[Test]
		public void Constructor_AllCharactersMovedFromSourceWithNoAssignedActor_CharactersGetMovedAndSourceIsDeleted()
		{
			var charactersToMove = new[] { "Peter (Simon)", "John the Baptist", "rich young ruler" };
			var sourceGroup = AddCharacterGroup(charactersToMove);
			var destGroup = AddCharacterGroup("centurion at crucifixion", "man possessed by evil spirit", "John", "Pharisees");
			var estHoursOfDestBeforeMove = destGroup.EstimatedHours;

			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, destGroup, charactersToMove);

			Assert.That(destGroup, Is.EqualTo(action.GroupsAffectedByLastOperation.Single()));
			m_testProject.CharacterGroupList.CharacterGroups.SetEquals(new[] {destGroup});
			Assert.That(destGroup.CharacterIds, Is.EquivalentTo(new[]
			{
				"centurion at crucifixion",
				"man possessed by evil spirit",
				"John",
				"Pharisees",
				"Peter (Simon)",
				"John the Baptist",
				"rich young ruler"
			}));
			Assert.That(estHoursOfDestBeforeMove < destGroup.EstimatedHours, Is.True);
		}

		[Test]
		public void Constructor_NoDestSupplied_CharactersGetMovedToNewGroup()
		{
			var sourceGroup = AddCharacterGroup("Peter (Simon)", "John the Baptist",
				"rich young ruler", "centurion at crucifixion", "man possessed by evil spirit");
			var estHoursOfSourceBeforeMove = sourceGroup.EstimatedHours;
			var anotherGroup = AddCharacterGroup("John, third", "Pharisees");
			var charactersToMove = new List<string> { "rich young ruler", "man possessed by evil spirit" };
			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, null, charactersToMove);

			var newGroup = m_testProject.CharacterGroupList.GroupContainingCharacterId("rich young ruler");
			Assert.That(action.GroupsAffectedByLastOperation.SequenceEqual(new[] { newGroup, sourceGroup }), Is.True);
			Assert.That(sourceGroup, Is.Not.EqualTo(newGroup));
			Assert.That(newGroup.CharacterIds, Is.EquivalentTo(charactersToMove));
			Assert.That(sourceGroup.CharacterIds, Is.EquivalentTo(new[] { "Peter (Simon)", "John the Baptist", "centurion at crucifixion" }));
			Assert.That(anotherGroup.CharacterIds, Is.EquivalentTo(new[] { "John, third", "Pharisees" }));
			Assert.That(estHoursOfSourceBeforeMove > sourceGroup.EstimatedHours, Is.True);
		}

		[Test]
		public void Constructor_NoDestSuppliedMoveAllCharactersFromSourceWithAssignedActor_CharactersGetMovedToNewGroupAndSourceLeftEmpty()
		{
			var charactersToMove = new [] { "Peter (Simon)", "John the Baptist", "rich young ruler", "centurion", "man possessed by evil spirit" };
			var sourceGroup = AddCharacterGroup(charactersToMove);
			sourceGroup.AssignVoiceActor(13);
			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, null, new List<string>(sourceGroup.CharacterIds));

			var newGroup = m_testProject.CharacterGroupList.GroupContainingCharacterId("rich young ruler");
			Assert.That(action.GroupsAffectedByLastOperation.SequenceEqual(new[] { newGroup, sourceGroup }), Is.True);
			Assert.That(sourceGroup, Is.Not.EqualTo(newGroup));
			Assert.That(newGroup.CharacterIds, Is.EquivalentTo(charactersToMove));
			Assert.That(sourceGroup.CharacterIds.Count, Is.EqualTo(0));
			Assert.That(sourceGroup.VoiceActorId, Is.EqualTo(13));
		}

		[Test]
		public void Description_MoveToGroupWithImplicitName_GroupReferencedByGeneratedLabelIdForUiDisplay()
		{
			var sourceGroup = AddCharacterGroup("Peter (Simon)", "John the Baptist", "rich young ruler");
			var destGroup = AddCharacterGroup("centurion at crucifixion", "man possessed by evil spirit", "John, third", "Pharisees");

			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, destGroup, new List<string> { "rich young ruler" });
			Assert.That(action.Description, Is.EqualTo("Move characters to Man 2 group"));
		}

		[Test]
		public void Description_MoveToGroupWithExplicitId_GroupReferencedById()
		{
			var sourceGroup = AddCharacterGroup("Peter (Simon)", "John the Baptist", "rich young ruler");
			var destGroup = AddCharacterGroup("centurion at crucifixion", "man possessed by evil spirit", "John", "Pharisees");
			destGroup.GroupIdLabel = CharacterGroup.Label.Other;
			destGroup.GroupIdOtherText = "Forty-three";

			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, destGroup, new List<string> { "rich young ruler" });
			Assert.That(action.Description, Is.EqualTo("Move characters to Forty-three group"));
		}

		[Test]
		public void Description_NoDestinationGroupSpecified_CreateNewGroup()
		{
			var sourceGroup = AddCharacterGroup("Peter (Simon)", "John the Baptist", "rich young ruler");

			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, null, new List<string> { "rich young ruler" });
			Assert.That(action.Description, Is.EqualTo("Create new group"));
		}

		[Test]
		public void Description_IsSplit_SplitGroup()
		{
			var sourceGroup = AddCharacterGroup("Peter (Simon)", "John the Baptist", "rich young ruler");

			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, null, new List<string> { "rich young ruler" });
			action.IsSplit = true;
			Assert.That(action.Description, Is.EqualTo("Split group"));
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

			Assert.That(action.Undo(), Is.True);
			Assert.That(sourceGroup.CharacterIds, Is.EquivalentTo(originalCharactersInSource));
			Assert.That(action.GroupsAffectedByLastOperation.SequenceEqual(new [] { sourceGroup, destGroup }), Is.True);
			Assert.That(m_testProject.CharacterGroupList.CharacterGroups.Count, Is.EqualTo(2));
			Assert.That(sourceGroup.CharacterIds, Is.EquivalentTo(originalCharactersInSource));
			Assert.That(destGroup.CharacterIds, Is.EquivalentTo(originalCharactersInDest));
			Assert.That(estHoursOfDestBeforeMove, Is.EqualTo(destGroup.EstimatedHours));
			Assert.That(estHoursOfSourceBeforeMove, Is.EqualTo(sourceGroup.EstimatedHours));
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
			Assert.That(m_testProject.CharacterGroupList.CharacterGroups.Count, Is.EqualTo(1));

			Assert.That(action.Undo(), Is.True);
			var recreatedGroup = m_testProject.CharacterGroupList.GroupContainingCharacterId("rich young ruler");
			Assert.That(recreatedGroup.CharacterIds, Is.EquivalentTo(charactersToMove));
			Assert.That(action.GroupsAffectedByLastOperation.SequenceEqual(new[] { recreatedGroup, destGroup }), Is.True);
			Assert.That(m_testProject.CharacterGroupList.CharacterGroups.Count, Is.EqualTo(2));
			Assert.That(recreatedGroup.CharacterIds, Is.EquivalentTo(charactersToMove));
			Assert.That(destGroup.CharacterIds, Is.EquivalentTo(originalCharactersInDest));
			Assert.That(estHoursOfDestBeforeMove, Is.EqualTo(destGroup.EstimatedHours));
			Assert.That(estHoursOfSourceBeforeMove, Is.EqualTo(recreatedGroup.EstimatedHours));
		}

		[Test]
		public void Undo_NoDestSuppliedMoveAllCharactersFromSourceWithAssignedActor_CharactersGetMovedBackToOriginalGroupAndNewGroupIsRemoved()
		{
			var actor = new VoiceActor { Id = 13 };
			m_testProject.VoiceActorList.AllActors.Add(actor);
			var originalCharactersInSource = new[] { "Peter (Simon)", "John the Baptist", "rich young ruler", "centurion at crucifixion", "man possessed by evil spirit", "John", "Pharisees" };
			var sourceGroup = AddCharacterGroup(originalCharactersInSource);
			sourceGroup.AssignVoiceActor(13);
			var estHoursOfSourceBeforeMove = sourceGroup.EstimatedHours;
			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, null, new List<string>(sourceGroup.CharacterIds));

			Assert.That(action.Undo(), Is.True);
			Assert.That(sourceGroup.CharacterIds, Is.EquivalentTo(originalCharactersInSource));
			Assert.That(sourceGroup, Is.EqualTo(action.GroupsAffectedByLastOperation.Single()));
			Assert.That(m_testProject.CharacterGroupList.CharacterGroups.Count, Is.EqualTo(1));
			Assert.That(sourceGroup, Is.EqualTo(m_testProject.CharacterGroupList.GroupContainingCharacterId("rich young ruler")));
			Assert.That(estHoursOfSourceBeforeMove, Is.EqualTo(sourceGroup.EstimatedHours));
		}

		[Test]
		public void Undo_Split_CharactersGetMovedBackToOriginalGroupAndNewGroupIsRemoved()
		{
			var originalCharactersInSource = new[] { "Peter (Simon)", "John the Baptist", "rich young ruler", "centurion", "man possessed by evil spirit", "John", "Pharisees" };
			var sourceGroup = AddCharacterGroup(originalCharactersInSource);
			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, null, new List<string> { "rich young ruler", "man possessed by evil spirit", "John" });
			action.IsSplit = true;

			Assert.That(action.Undo(), Is.True);
			Assert.That(sourceGroup.CharacterIds, Is.EquivalentTo(originalCharactersInSource));
			Assert.That(sourceGroup, Is.EqualTo(action.GroupsAffectedByLastOperation.Single()));
			Assert.That(m_testProject.CharacterGroupList.CharacterGroups.Count, Is.EqualTo(1));
			Assert.That(sourceGroup, Is.EqualTo(m_testProject.CharacterGroupList.GroupContainingCharacterId("rich young ruler")));
		}

		[Test]
		public void Redo_SourceAndDestHaveCharactersBeforeAndAfterMove_CharactersGetMovedToDestNoGroupsDeleted()
		{
			var originalCharactersInSource = new[] { "Peter (Simon)", "John the Baptist", "rich young ruler", "Pilate", "Jesus' family", "man possessed by evil spirit", "John", "Pharisees", "passers by" };
			var originalCharactersInDest = new[] { "crowd", "woman, bleeding for twelve years" };
			var sourceGroup = AddCharacterGroup(originalCharactersInSource);
			var destGroup = AddCharacterGroup(originalCharactersInDest);
			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, destGroup, new List<string> { "passers by", "Pilate" });

			Assert.That(action.Undo(), Is.True);
			var estHoursOfSourceBeforeRedo = sourceGroup.EstimatedHours;
			var estHoursOfDestBeforeRedo = destGroup.EstimatedHours;
			Assert.That(action.Redo(), Is.True);

			Assert.That(action.GroupsAffectedByLastOperation.SequenceEqual(new[] { destGroup, sourceGroup }), Is.True);
			Assert.That(sourceGroup.CharacterIds, Is.EquivalentTo(new[]
			{
				"Peter (Simon)",
				"John the Baptist",
				"rich young ruler",
				"Jesus' family",
				"man possessed by evil spirit",
				"John",
				"Pharisees"
			}));
			Assert.That(destGroup.CharacterIds, Is.EquivalentTo(
				new[] { "crowd", "woman, bleeding for twelve years", "Pilate", "passers by" }));
			Assert.That(m_testProject.CharacterGroupList.CharacterGroups.Count, Is.EqualTo(2));
			Assert.That(estHoursOfSourceBeforeRedo > sourceGroup.EstimatedHours, Is.True);
			Assert.That(estHoursOfDestBeforeRedo < destGroup.EstimatedHours, Is.True);
		}

		[Test]
		public void Redo_AllCharactersMovedFromSourceWithNoAssignedActor_GroupRecreatedAndCharactersGetMovedBack()
		{
			var charactersToMove = new[] { "Peter (Simon)", "John the Baptist", "rich young ruler" };
			var originalCharactersInDest = new[] { "centurion at crucifixion", "man possessed by evil spirit", "John", "Pharisees" };
			var sourceGroup = AddCharacterGroup(charactersToMove);
			var destGroup = AddCharacterGroup(originalCharactersInDest);
			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, destGroup, charactersToMove);
			Assert.That(m_testProject.CharacterGroupList.CharacterGroups.Count, Is.EqualTo(1));

			Assert.That(action.Undo(), Is.True);
			var estHoursOfDestBeforeRedo = destGroup.EstimatedHours;
			Assert.That(action.Redo(), Is.True);

			Assert.That(destGroup, Is.EqualTo(action.GroupsAffectedByLastOperation.Single()));
			m_testProject.CharacterGroupList.CharacterGroups.SetEquals(new[] { destGroup });
			Assert.That(destGroup.CharacterIds, Is.EquivalentTo(new[]
			{
				"centurion at crucifixion",
				"man possessed by evil spirit",
				"John",
				"Pharisees",
				"Peter (Simon)",
				"John the Baptist",
				"rich young ruler"
			}));
			Assert.That(estHoursOfDestBeforeRedo < destGroup.EstimatedHours, Is.True);
		}

		[Test]
		public void Redo_NoDestSuppliedMoveAllCharactersFromSourceWithAssignedActor_CharactersGetMovedBackToOriginalGroupAndNewGroupIsRemoved()
		{
			var actor = new VoiceActor { Id = 13 };
			m_testProject.VoiceActorList.AllActors.Add(actor);
			var charactersToMove = new[] { "Peter (Simon)", "John the Baptist", "rich young ruler", "centurion at crucifixion", "man possessed by evil spirit", "John", "Pharisees" };
			var sourceGroup = AddCharacterGroup(charactersToMove);
			sourceGroup.AssignVoiceActor(13);
			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, null, new List<string>(sourceGroup.CharacterIds));

			Assert.That(action.Undo(), Is.True);
			var estHoursOfSourceBeforeRedo = sourceGroup.EstimatedHours;
			Assert.That(action.Redo(), Is.True);

			var newGroup = m_testProject.CharacterGroupList.GroupContainingCharacterId("rich young ruler");
			Assert.That(action.GroupsAffectedByLastOperation.SequenceEqual(new[] { newGroup, sourceGroup }), Is.True);
			Assert.That(sourceGroup, Is.Not.EqualTo(newGroup));
			Assert.That(newGroup.CharacterIds, Is.EquivalentTo(charactersToMove));
			Assert.That(sourceGroup.CharacterIds.Count, Is.EqualTo(0));
			Assert.That(sourceGroup.VoiceActorId, Is.EqualTo(13));
			Assert.That(estHoursOfSourceBeforeRedo > sourceGroup.EstimatedHours, Is.True);
		}

		[Test]
		public void Redo_Split_CharactersGetMovedBackToOriginalGroupAndNewGroupIsRemoved()
		{
			var originalCharactersInSource = new[] { "Peter (Simon)", "John the Baptist", "rich young ruler", "Jesus' family", "man possessed by evil spirit", "John", "Pharisees" };
			var charactersToMove = new List<string> { "rich young ruler", "man possessed by evil spirit" };
			var sourceGroup = AddCharacterGroup(originalCharactersInSource);
			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, null, new List<string>(charactersToMove));
			action.IsSplit = true;

			Assert.That(action.Undo(), Is.True);
			var estHoursOfSourceBeforeRedo = sourceGroup.EstimatedHours;
			Assert.That(action.Redo(), Is.True);

			var newGroup = m_testProject.CharacterGroupList.GroupContainingCharacterId("rich young ruler");
			Assert.That(action.GroupsAffectedByLastOperation.SequenceEqual(new[] { newGroup, sourceGroup }), Is.True);
			Assert.That(sourceGroup, Is.Not.EqualTo(newGroup));
			Assert.That(newGroup.CharacterIds, Is.EquivalentTo(charactersToMove));
			Assert.That(sourceGroup.CharacterIds, Is.EquivalentTo(new[]
				{ "Peter (Simon)", "John the Baptist", "Jesus' family", "John", "Pharisees" }));
			Assert.That(estHoursOfSourceBeforeRedo > sourceGroup.EstimatedHours, Is.True);
		}

		[Test]
		public void Undo_AssignCameoRole_CameoGroupNotRemoved()
		{
			// list of characters
			var originalCharactersInSource = new[] { "Peter (Simon)", "woman, bleeding for twelve years" };
			var sourceGroup = AddCharacterGroup(originalCharactersInSource);

			// set up cameo actor and group
			var cameoActor = new VoiceActor { Id = 1, Name = "Missy Cameo", IsCameo = true };
			m_testProject.VoiceActorList.AllActors.Add(cameoActor);
			var cameoGroup = AddCharacterGroup();
			cameoGroup.AssignVoiceActor(1);

			// should be 2 groups now
			Assert.That(m_testProject.CharacterGroupList.CharacterGroups.Count, Is.EqualTo(2));

			var estHoursOfSourceBeforeMove = sourceGroup.EstimatedHours;
			Assert.That(cameoGroup.EstimatedHours, Is.EqualTo(0));

			// assign the character role "woman, bleeding for twelve years" to the cameo actor
			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, cameoGroup, new List<string> { "woman, bleeding for twelve years" });

			// should still be 2 groups
			Assert.That(m_testProject.CharacterGroupList.CharacterGroups.Count, Is.EqualTo(2));

			// "woman, bleeding for twelve years" should be in the cameo group now
			Assert.That(m_testProject.CharacterGroupList.GroupContainingCharacterId("woman, bleeding for twelve years").AssignedToCameoActor, Is.True);

			// undo
			Assert.That(action.Undo(), Is.True);

			// should still be 2 groups
			Assert.That(m_testProject.CharacterGroupList.CharacterGroups.Count, Is.EqualTo(2));

			// "woman, bleeding for twelve years" should be back in the original group
			Assert.That(m_testProject.CharacterGroupList.GroupContainingCharacterId("woman, bleeding for twelve years").AssignedToCameoActor, Is.False);
			Assert.That(estHoursOfSourceBeforeMove, Is.EqualTo(sourceGroup.EstimatedHours));
			Assert.That(cameoGroup.EstimatedHours, Is.EqualTo(0));
		}
	}
}
