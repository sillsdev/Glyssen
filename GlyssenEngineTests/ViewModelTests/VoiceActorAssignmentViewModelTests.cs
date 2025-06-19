using System;
using System.Collections.Generic;
using System.Linq;
using GlyssenCharacters;
using GlyssenEngine;
using GlyssenEngine.Casting;
using GlyssenEngine.Character;
using GlyssenEngine.Rules;
using GlyssenEngine.UndoActions;
using GlyssenEngine.ViewModels;
using NUnit.Framework;
using SIL.Extensions;
using CharacterIdHashSet = GlyssenEngine.Character.CharacterIdHashSet;
using Resources = GlyssenCharactersTests.Properties.Resources;
using static GlyssenSharedTests.CustomConstraints;

namespace GlyssenEngineTests.ViewModelTests
{
	[TestFixture]
	class VoiceActorAssignmentViewModelTests
	{
		private Project m_testProject;
		private VoiceActorAssignmentViewModel m_model;

		[OneTimeSetUp]
		public void OneTimeSetUp()
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

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			TestProject.DeleteTestProjects();
		}

		[Test]
		public void AssignActorToGroup_CanAssignTrue_ActorAssigned()
		{
			var actor1 = new VoiceActor { Id = 1, Name = "Puni Upalari" };
			m_testProject.VoiceActorList.AllActors = new List<VoiceActor> { actor1 };
			var group = m_model.CharacterGroups[0];
			m_model.AssignActorToGroup(actor1.Id, group);
			Assert.That(actor1.Id, Is.EqualTo(group.VoiceActorId));
			var undoDescriptions = m_model.UndoActions;
			Assert.That(undoDescriptions.Count, Is.EqualTo(1));
			Assert.That(undoDescriptions[0], Is.EqualTo("Assign voice actor Puni Upalari"));
		}

		[Test]
		public void AssignActorToGroup_AssignedToSameActor_NoChangeAndNoUndoAction()
		{
			var actor1 = new VoiceActor { Id = 1, Name = "Puni Upalari" };
			m_testProject.VoiceActorList.AllActors = new List<VoiceActor> { actor1 };
			var group = m_model.CharacterGroups[0];
			group.VoiceActorId = actor1.Id;
			m_model.AssignActorToGroup(actor1.Id, group);
			Assert.That(actor1.Id, Is.EqualTo(group.VoiceActorId));
			Assert.That(m_model.UndoActions.Count, Is.EqualTo(0));
		}

		[Test]
		public void AssignActorToGroup_ExistingGroupAssignedToActor_ActorUnassignedFromPreviousGroupAndAssignedToRequestedGroup()
		{
			var actor1 = new VoiceActor { Id = 1, Name = "Eduardo Lopez" };
			m_testProject.VoiceActorList.AllActors.Add(actor1);
			var existingGroup = m_model.CharacterGroups[0];
			existingGroup.VoiceActorId = 1;
			var newGroup = AddNewGroup();
//			newGroup.Name = "New group";
			m_model.AssignActorToGroup(1, newGroup);
			Assert.That(newGroup.VoiceActorId, Is.EqualTo(1));
			Assert.That(m_model.UndoActions.Count, Is.EqualTo(1));
			Assert.That(m_model.UndoActions[0], Is.EqualTo("Assign voice actor Eduardo Lopez"));
			Assert.That(existingGroup.IsVoiceActorAssigned, Is.False);
		}

		[Test]
		public void Undo_AssignActorToGroup_ExistingGroupAssignedToActor_ActorUnassignedFromAssignedGroupAndReassignedToPreviousGroup()
		{
			var actor1 = new VoiceActor { Id = 1, Name = "Eduardo Lopez" };
			m_testProject.VoiceActorList.AllActors.Add(actor1);
			var existingGroup = m_model.CharacterGroups[0];
			existingGroup.VoiceActorId = 1;
			var newGroup = AddNewGroup();
//			newGroup.Name = "New group";
			m_model.AssignActorToGroup(1, newGroup);
			List<CharacterGroup> affectedGroups = null;
			m_model.Saved += (sender, affected, f) => { affectedGroups = affected.ToList(); Assert.That(f, Is.False);};
			Assert.That(m_model.Undo(), Is.True);
			Assert.That(m_model.UndoActions.Count, Is.EqualTo(0));
			Assert.That(m_model.RedoActions.Count, Is.EqualTo(1));
			Assert.That(m_model.RedoActions[0], Is.EqualTo("Assign voice actor Eduardo Lopez"));
			Assert.That(newGroup.IsVoiceActorAssigned, Is.False);
			Assert.That(existingGroup.VoiceActorId, Is.EqualTo(1));
			Assert.That(affectedGroups.SequenceEqual(new [] {existingGroup, newGroup }), Is.True);
		}

		[Test]
		public void UnAssignActorFromGroups_ByGroup()
		{
			var actor1 = new VoiceActor { Id = 1, Name = "Marco Polo" };
			m_testProject.VoiceActorList.AllActors.Add(actor1);
			var actor2 = new VoiceActor { Id = 2, Name = "Wilbur Wright" };
			m_testProject.VoiceActorList.AllActors.Add(actor2);
			var group1 = m_model.CharacterGroups[0];
			group1.SetGroupIdLabel();
			m_model.AssignActorToGroup(actor2.Id, group1);
			Assert.That(group1.IsVoiceActorAssigned, Is.True);
			var group2 = AddNewGroup("Nicodemus");
			m_model.AssignActorToGroup(actor1.Id, group2);
			Assert.That(group2.IsVoiceActorAssigned, Is.True);
			Assert.That(m_model.UndoActions.Count, Is.EqualTo(2));

			m_model.UnAssignActorFromGroups(new List<CharacterGroup> { group1, group2 });
			Assert.That(group1.IsVoiceActorAssigned, Is.False);
			Assert.That(group2.IsVoiceActorAssigned, Is.False);
			Assert.That(m_model.UndoActions.Count, Is.EqualTo(3));
		}

		[Test]
		public void UnAssignActorFromGroups_GroupsContainsCameoGroup_CameoGroupNotUnassigned()
		{
			var actor1 = new VoiceActor { Id = 1, Name = "Marco Polo", IsCameo = true };
			m_testProject.VoiceActorList.AllActors.Add(actor1);
			var actor2 = new VoiceActor { Id = 2, Name = "Wilbur Wright" };
			m_testProject.VoiceActorList.AllActors.Add(actor2);
			var group1 = m_model.CharacterGroups[0];
			m_model.AssignActorToGroup(actor2.Id, group1);
			Assert.That(group1.IsVoiceActorAssigned, Is.True);
			var cameoGroup = AddNewGroup(); // No characters => cameo
			m_model.AssignActorToGroup(actor1.Id, cameoGroup);
			Assert.That(cameoGroup.IsVoiceActorAssigned, Is.True);
			Assert.That(m_model.UndoActions.Count, Is.EqualTo(2));

			m_model.UnAssignActorFromGroups(new List<CharacterGroup> { group1, cameoGroup });
			Assert.That(group1.IsVoiceActorAssigned, Is.False);
			Assert.That(cameoGroup.IsVoiceActorAssigned, Is.True);
			Assert.That(m_model.UndoActions.Count, Is.EqualTo(3));
		}

		[Test]
		public void CanMoveCharactersToGroup_UnexpectedCharacterId_ReturnsFalse()
		{
			Assert.That(m_model.CanMoveCharactersToGroup(new[] { "Hairy Man of Hinkley" }, null), Is.False);
		}

		[Test]
		public void CanMoveCharactersToGroup_NoDestGroupSpecifiedNotAllCharactersInSourceAreIncluded_ReturnsTrue()
		{
			var sourceGroup = m_model.CharacterGroups[0];
			sourceGroup.CharacterIds = new CharacterIdHashSet(new[] { "John", "Andrew", "Moses" });

			Assert.That(m_model.CanMoveCharactersToGroup(new List<string> { "John", "Moses" }, null), Is.True);
		}

		[Test]
		public void CanMoveCharactersToGroup_NoDestGroupSpecifiedAllCharactersInUnassignedSourceAreIncluded_ReturnsFalse()
		{
			var sourceGroup = m_model.CharacterGroups[0];
			sourceGroup.CharacterIds = new CharacterIdHashSet(new[] { "John", "Andrew", "Moses" });

			Assert.That(m_model.CanMoveCharactersToGroup(new List<string> { "Andrew", "John", "Moses" }, null), Is.False);
		}

		[Test]
		public void CanMoveCharactersToGroup_NoDestGroupSpecifiedAllCharactersInAssignedSourceAreIncluded_ReturnsTrue()
		{
			var sourceGroup = m_model.CharacterGroups[0];
			sourceGroup.CharacterIds = new CharacterIdHashSet(new[] { "John", "Andrew", "Moses" });
			sourceGroup.VoiceActorId = 56;

			Assert.That(m_model.CanMoveCharactersToGroup(new List<string> { "Andrew", "John", "Moses" }, null), Is.True);
		}

		[Test]
		public void CanMoveCharactersToGroup_NoDestGroupSpecifiedAllCharactersInCameoSourceAreIncluded_ReturnsTrue()
		{
			var sourceGroup = m_model.CharacterGroups[0];
			sourceGroup.CharacterIds = new CharacterIdHashSet(new[] { "John", "Andrew", "Moses" });

			Assert.That(m_model.CanMoveCharactersToGroup(new List<string> { "Andrew", "John", "Moses" }, null), Is.False);
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
			Assert.That(m_model.MoveCharactersToGroup(new[] { "Hairy Man of Hinkley" }, null), Is.False);
			Assert.That(m_model.UndoActions.Count, Is.EqualTo(0));
		}

		[Test]
		public void MoveCharactersToGroup_NoDestGroupSpecified_CharactersMovedFromOriginalGroupToNewGroup()
		{
			var sourceGroup = m_model.CharacterGroups[0];
			sourceGroup.CharacterIds = new CharacterIdHashSet(new[] { "John", "Andrew", "Moses" });

			List<CharacterGroup> affectedGroups = null;
			m_model.Saved += (sender, affected, f) => { affectedGroups = affected.ToList(); Assert.That(f, Is.False);};
			Assert.That(m_model.MoveCharactersToGroup(new List<string> { "John", "Moses" }, null), Is.True);
			var newGroup = m_testProject.CharacterGroupList.GroupContainingCharacterId("Moses");
			Assert.That(newGroup, Is.Not.Null);
			Assert.That(affectedGroups.SequenceEqual(new[] { newGroup, sourceGroup }), Is.True);

			Assert.That(newGroup.CharacterIds.Count, Is.EqualTo(2));
			Assert.That(newGroup.CharacterIds, Does.Contain("John"));
			Assert.That(sourceGroup.CharacterIds.Count, Is.EqualTo(1));
			Assert.That(sourceGroup.CharacterIds, Does.Contain("Andrew"));
			Assert.That(m_model.UndoActions.Single(), Is.EqualTo("Create new group"));
		}

		[Test]
		public void MoveCharactersToGroup_NoDestGroupSpecifiedAllCharactersInAssignedSourceGroup_CharactersMovedToNewGroupButSourceGroupNotRemoved()
		{
			var allThreeCharacters = new List<string>(new[] {"John", "Andrew", "Moses"});
			var sourceGroup = m_model.CharacterGroups[0];
			sourceGroup.CharacterIds = new CharacterIdHashSet(allThreeCharacters);
			sourceGroup.VoiceActorId = 5;

			List<CharacterGroup> affectedGroups = null;
			m_model.Saved += (sender, affected, f) => { affectedGroups = affected.ToList(); Assert.That(f, Is.False);};
			Assert.That(m_model.MoveCharactersToGroup(allThreeCharacters, null), Is.True);

			var newGroup = m_testProject.CharacterGroupList.GroupContainingCharacterId("Moses");
			Assert.That(newGroup, Is.Not.Null);
			Assert.That(sourceGroup, Is.Not.EqualTo(newGroup));
			Assert.That(affectedGroups.SequenceEqual(new[] { newGroup, sourceGroup }), Is.True);
			Assert.That(newGroup.CharacterIds.SetEquals(allThreeCharacters), Is.True);
			Assert.That(sourceGroup.CharacterIds.Count, Is.EqualTo(0));
			Assert.That(m_model.UndoActions.Single(), Is.EqualTo("Create new group"));
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
			m_model.Saved += (sender, affected, f) => { affectedGroups = affected.ToList(); Assert.That(f, Is.False);};
			Assert.That(m_model.MoveCharactersToGroup(characterIds, destGroup), Is.True);

			Assert.That(affectedGroups.SequenceEqual(new[] { destGroup, sourceGroup }), Is.True);
			var destGroupCharacterIds = destGroup.CharacterIds;
			Assert.That(destGroup.CharacterIds.Count, Is.EqualTo(4));
			Assert.That(destGroupCharacterIds, Does.Contain("John"));
			Assert.That(destGroupCharacterIds, Does.Contain("Andrew"));
			Assert.That(m_model.UndoActions.Single(), Is.EqualTo("Move characters to Man 2 group"));
		}

		[Test]
		public void MoveCharactersToGroup_SourceGroupSameAsDestinationGroup_NoActionTaken()
		{
			var characterIds = new List<string> { "John", "Andrew" };
			var sourceGroup = m_model.CharacterGroups[0];
			sourceGroup.CharacterIds = new CharacterIdHashSet(characterIds);

			List<CharacterGroup> affectedGroups = null;
			m_model.Saved += (sender, affected, f) => { affectedGroups = affected.ToList(); Assert.That(f, Is.False);};
			Assert.That(m_model.MoveCharactersToGroup(characterIds, sourceGroup), Is.False);
			Assert.That(affectedGroups, Is.Null);
			Assert.That(m_model.UndoActions.Count, Is.EqualTo(0));
		}

		[Test]
		public void MoveCharactersToGroup_AllCharactersMovedOutOfSource_SourceGroupRemoved()
		{
			var characterIds = new List<string> { "John", "Andrew" };
			var sourceGroup = m_model.CharacterGroups[0];
			sourceGroup.CharacterIds = new CharacterIdHashSet(characterIds);
			sourceGroup.SetGroupIdLabel();
			var destGroup = AddNewGroup("ear", "foot");
			Assert.That(m_testProject.CharacterGroupList.CharacterGroups.Count, Is.EqualTo(2));

			List<CharacterGroup> affectedGroups = null;
			m_model.Saved += (sender, affected, f) => { affectedGroups = affected.ToList(); Assert.That(f, Is.False);};
			Assert.That(m_model.MoveCharactersToGroup(characterIds, destGroup), Is.True);
			Assert.That(destGroup, Is.EqualTo(affectedGroups.Single()));
			Assert.That(m_testProject.CharacterGroupList.CharacterGroups.Count, Is.EqualTo(1));
			Assert.That(destGroup.CharacterIds.SetEquals(new[] { "John", "Andrew", "ear", "foot" }), Is.True);
			Assert.That(m_model.UndoActions.Single(), Is.EqualTo("Move characters to Man 2 group"));
		}

		[Test]
		public void RegenerateGroups()
		{
			m_testProject.CharacterGroupList.CharacterGroups.Clear();
			var actor1 = new VoiceActor { Id = 1 };
			var actor2 = new VoiceActor { Id = 2, Gender = ActorGender.Female};
			var actor3 = new VoiceActor { Id = 3 };
			m_testProject.VoiceActorList.AllActors = new List<VoiceActor> { actor1, actor2, actor3 };
			List<CharacterGroup> affectedGroups = null;
			m_model.Saved += (sender, affected, f) => { if (affected != null) affectedGroups = affected.ToList(); Assert.That(f, Is.False);};
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
			Assert.That(m_testProject.CharacterGroupList.CharacterGroups.Count, Is.EqualTo(3));
			// TODO (PG-437): Check that correct groups are included in affectedGroups.
			// For now, just check based on the undo item that "AssignActorToGroup" should put there.
			Assert.That(affectedGroups.SetEquals(m_testProject.CharacterGroupList.GetGroupsAssignedToActor(2)), Is.True);
		}

		[Test]
		public void SplitGroup_MoveOne()
		{
			var characterIds = new List<string> { "John", "Andrew" };
			var existingGroup = m_model.CharacterGroups[0];
			existingGroup.CharacterIds = new CharacterIdHashSet(characterIds);

			Assert.That(m_testProject.CharacterGroupList.CharacterGroups.Count, Is.EqualTo(1));
			var newGroup = m_model.SplitGroup(new List<string> {"John"});
			Assert.That(newGroup, Is.Not.Null);

			Assert.That(m_testProject.CharacterGroupList.CharacterGroups.Count, Is.EqualTo(2));

			Assert.That(existingGroup == newGroup, Is.False);
			Assert.That(existingGroup.CharacterIds, Does.Contain("Andrew"));
			Assert.That(existingGroup.CharacterIds, Does.Not.Contain("John"));
			Assert.That(newGroup.CharacterIds, Does.Contain("John"));
			Assert.That(m_model.UndoActions.Single(), Is.EqualTo("Split group"));
		}

		[Test]
		public void SplitGroup_MoveMultiple()
		{
			var characterIds = new List<string> { "John", "Andrew", "Peter" };
			var existingGroup = m_model.CharacterGroups[0];
			existingGroup.CharacterIds = new CharacterIdHashSet(characterIds);

			Assert.That(m_testProject.CharacterGroupList.CharacterGroups.Count, Is.EqualTo(1));

			var newGroup = m_model.SplitGroup(new List<string> {"John", "Peter"});
			Assert.That(newGroup, Is.Not.Null);

			Assert.That(m_testProject.CharacterGroupList.CharacterGroups.Count, Is.EqualTo(2));

			Assert.That(existingGroup == newGroup, Is.False);
			Assert.That(existingGroup.CharacterIds, Does.Contain("Andrew"));
			Assert.That(existingGroup.CharacterIds, Does.Not.Contain("John"));
			Assert.That(existingGroup.CharacterIds, Does.Not.Contain("Peter"));
			Assert.That(newGroup.CharacterIds, Does.Contain("John"));
			Assert.That(newGroup.CharacterIds, Does.Contain("Peter"));
			Assert.That(m_model.UndoActions.Single(), Is.EqualTo("Split group"));
		}

		[Test]
		public void GetActorsSortedByAvailabilityAndName_NoActorsAssigned_GetsAllActorsInAlphbeticalOrder()
		{
			var actorB = new VoiceActor { Id = 1, Name = "B" };
			var actorC = new VoiceActor { Id = 2, Name = "C" };
			var actorA = new VoiceActor { Id = 3, Name = "A" };
			m_testProject.VoiceActorList.AllActors = new List<VoiceActor> { actorB, actorC, actorA };
			var generator = new CharacterGroupGenerator(m_testProject);
			generator.GenerateCharacterGroups();
			generator.ApplyGeneratedGroupsToProject(false);

			var actorList = m_model.GetActorsSortedByAvailabilityAndName(m_model.CharacterGroups[0]).ToList();
			Assert.That(actorA, Is.EqualTo(actorList[0].Item1));
			Assert.That(actorB, Is.EqualTo(actorList[1].Item1));
			Assert.That(actorC, Is.EqualTo(actorList[2].Item1));
			Assert.That(actorList, ForEvery<Tuple<VoiceActor, bool>>(a => a.Item2, Is.True, "available"));
		}

		[Test]
		public void GetActorsSortedByAvailabilityAndName_ActorAssigned_AssignedActorSortsLast()
		{
			var actorB = new VoiceActor { Id = 1, Name = "B" };
			var actorC = new VoiceActor { Id = 2, Name = "C" };
			var actorA = new VoiceActor { Id = 3, Name = "A" };
			m_testProject.VoiceActorList.AllActors = new List<VoiceActor> { actorB, actorC, actorA };
			var generator = new CharacterGroupGenerator(m_testProject);
			generator.GenerateCharacterGroups();
			generator.ApplyGeneratedGroupsToProject(false);
			var group = m_model.CharacterGroups[0];
			m_model.AssignActorToGroup(actorA.Id, group);

			var actorList = m_model.GetActorsSortedByAvailabilityAndName(m_model.CharacterGroups[0]).ToList();
			Assert.That(actorB, Is.EqualTo(actorList[0].Item1));
			Assert.That(actorList[0].Item2, Is.True);
			Assert.That(actorC, Is.EqualTo(actorList[1].Item1));
			Assert.That(actorList[1].Item2, Is.True);
			Assert.That(actorA, Is.EqualTo(actorList[2].Item1));
			Assert.That(actorList[2].Item2, Is.False);
		}

		[Test]
		public void NoteActorChanges_NoChanges_DoNothing()
		{
			Assert.That(m_model.UndoActions.Count, Is.EqualTo(0));
			List<CharacterGroup> affectedGroups = null;
			m_model.Saved += (sender, affected, f) => { affectedGroups = affected.ToList(); Assert.That(f, Is.False);};
			m_model.NoteActorChanges(new IVoiceActorUndoAction[0]);
			Assert.That(m_model.UndoActions.Count, Is.EqualTo(0));
			Assert.That(affectedGroups, Is.Null);
		}

		[Test]
		public void NoteActorChanges_SingleChangeToUnassignedActor_NewUndoActionAddedWithDescriptionBasedOnSingleChange_NoGroupsAffected()
		{
			var affectedActor = new VoiceActor { Id = 1, Name = "B", Age = ActorAge.Adult };
			var replacedActor = new VoiceActor { Id = 1, Name = "B", Age = ActorAge.YoungAdult };
			m_testProject.VoiceActorList.AllActors = new List<VoiceActor> { affectedActor };
			Assert.That(m_model.UndoActions.Count, Is.EqualTo(0));
			List<CharacterGroup> affectedGroups = null;
			m_model.Saved += (sender, affected, f) => { affectedGroups = affected.ToList(); Assert.That(f, Is.True); };
			m_model.NoteActorChanges(new IVoiceActorUndoAction[] { new VoiceActorEditUndoAction(m_testProject, replacedActor) });
			Assert.That(m_model.UndoActions.Single(), Is.EqualTo("Edit voice actor B"));
			Assert.That(affectedGroups.Count, Is.EqualTo(0));
		}

		[Test]
		public void NoteActorChanges_MultipleChanges_NewUndoActionAddedWithGeneralDescription_GroupsAffected()
		{
			var replacedActor = new VoiceActor { Id = 1, Name = "B", Age = ActorAge.YoungAdult };
			m_testProject.VoiceActorList.AllActors.Add(replacedActor);
			var characterGroup = new CharacterGroup(m_testProject);
			m_testProject.CharacterGroupList.CharacterGroups.Add(characterGroup);
			characterGroup.AssignVoiceActor(1);
			var affectedActor = new VoiceActor { Id = 1, Name = "B", Age = ActorAge.Adult };
			var deletedActor = new VoiceActor { Id = 2, Name = "C" };
			m_testProject.VoiceActorList.AllActors = new List<VoiceActor> { affectedActor };
			Assert.That(m_model.UndoActions.Count, Is.EqualTo(0));
			List<CharacterGroup> affectedGroups = null;
			m_model.Saved += (sender, affected, f) => { affectedGroups = affected.ToList(); Assert.That(f, Is.True);};
			m_model.NoteActorChanges(new IVoiceActorUndoAction[]
			{
				new VoiceActorEditUndoAction(m_testProject, replacedActor),
				new VoiceActorDeletedUndoAction(m_testProject, deletedActor)
			});
			Assert.That(m_model.UndoActions.Single(), Is.EqualTo("Edit voice actors"));
			Assert.That(characterGroup, Is.EqualTo(affectedGroups.Single()));
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
			//	Assert.That(m_model.UndoActions.Count, Is.EqualTo(0));
			//	List<CharacterGroup> affectedGroups = null;
			//	m_model.Saved += (sender, args) => { affectedGroups = args.ToList(); };
			//	m_model.NoteActorChanges(new IVoiceActorUndoAction[]
			//	{
			//		new VoiceActorEditUndoAction(m_testProject, replacedActor),
			//		new VoiceActorDeletedUndoAction(m_testProject, deletedActor)
			//	});
			//	Assert.That(m_model.UndoActions.Single(), Is.EqualTo("Edit voice actors"));
			//	Assert.That(characterGroup, Is.EqualTo(affectedGroups.Single()));
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
			Assert.That(-1, Is.EqualTo(result.Item1));
			Assert.That(-1, Is.EqualTo(result.Item2));
		}

		[Test]
		public void FindNextMatchingCharacter_ExactMatchWithOnlyCharacterInFirstGroup_ReturnsZeroZero()
		{
			var result = m_model.FindNextMatchingCharacter("John", 0, 0);
			Assert.That(result.Item1, Is.EqualTo(0));
			Assert.That(result.Item2, Is.EqualTo(0));
		}

		[Test]
		public void FindNextMatchingCharacter_NegativeIndex_SearchStartsAtBeginning()
		{
			var result = m_model.FindNextMatchingCharacter("John", -1, -1);
			Assert.That(result.Item1, Is.EqualTo(0));
			Assert.That(result.Item2, Is.EqualTo(0));
		}

		[Test]
		public void FindNextMatchingCharacter_IndexOutOfRange_SearchStartsAtBeginning()
		{
			var result = m_model.FindNextMatchingCharacter("John", 40, 40);
			Assert.That(result.Item1, Is.EqualTo(0));
			Assert.That(result.Item2, Is.EqualTo(0));
		}

		[Test]
		public void FindNextMatchingCharacter_PartialMatchWithCharacterInSubsequentGroup_ReturnsMatchIndices()
		{
			AddNewGroup("chief priests", "demon-possessed man", "Judas");
			AddNewGroup("Aaron", "John the Baptist", "Joshua");
			AddNewGroup("Martha", "Rhoda");
			var result = m_model.FindNextMatchingCharacter("John", 0, 0);
			Assert.That(result.Item1, Is.EqualTo(2));
			Assert.That(result.Item2, Is.EqualTo(1));
		}

		[Test]
		public void FindNextMatchingCharacter_PartialMatchWrapAround_ReturnsMatchIndices()
		{
			AddNewGroup("Amnon, son of David", "man delivered from Legion of demons", "Judas");
			AddNewGroup("Aaron", "John the Baptist", "Joshua");
			AddNewGroup("Martha", "Miriam", "Rhoda");
			var result = m_model.FindNextMatchingCharacter("am", 3, 1);
			Assert.That(result.Item1, Is.EqualTo(1));
			Assert.That(result.Item2, Is.EqualTo(0));
			result = m_model.FindNextMatchingCharacter("am", result.Item1, result.Item2);
			Assert.That(result.Item1, Is.EqualTo(3));
			Assert.That(result.Item2, Is.EqualTo(1));
		}

		[Test]
		public void FindNextMatchingCharacter_PartialMatchWithOnlyCharacterInFirstGroup_ReturnsZeroZero()
		{
			var result = m_model.FindNextMatchingCharacter("Joh", 0, 0);
			Assert.That(result.Item1, Is.EqualTo(0));
			Assert.That(result.Item2, Is.EqualTo(0));
		}

		[Test]
		public void AddNewActorToGroup_OneNewActor_UniqueActorIdAssignedToGroup()
		{
			var group = AddNewGroup("Amnon, son of David", "man delivered from Legion of demons", "Judas");
			var newActor = m_model.AddNewActorToGroup("Friedrich", group);
			Assert.That(newActor.Name, Is.EqualTo("Friedrich"));
			Assert.That(newActor.Id, Is.EqualTo(0));
			Assert.That(m_testProject.VoiceActorList.ActiveActors.Contains(newActor));
			Assert.That(newActor, Is.EqualTo(group.VoiceActor));
			Assert.That(newActor.Gender, Is.EqualTo(ActorGender.Male));
			Assert.That(newActor.Age, Is.EqualTo(ActorAge.Adult));
		}

		[Test]
		public void AddNewActorToGroup_ThreeNewActors_UniqueActorIdsAssignedToCorrectGroups()
		{
			var group1 = AddNewGroup("Amnon, son of David", "man delivered from Legion of demons", "Judas");
			var group2 = AddNewGroup("Rhoda");
			var group3 = AddNewGroup("children");
			var newActor1 = m_model.AddNewActorToGroup("Friedrich", group1);
			var newActor2 = m_model.AddNewActorToGroup("Wallace", group2);
			var newActor3 = m_model.AddNewActorToGroup("Gromit", group3);
			Assert.That(newActor1.Name, Is.EqualTo("Friedrich"));
			Assert.That(newActor1.Id, Is.EqualTo(0));
			Assert.That(m_testProject.VoiceActorList.ActiveActors.Contains(newActor1));
			Assert.That(newActor1, Is.EqualTo(group1.VoiceActor));
			Assert.That(newActor1.Gender, Is.EqualTo(ActorGender.Male));
			Assert.That(newActor1.Age, Is.EqualTo(ActorAge.Adult));
			Assert.That(newActor2.Name, Is.EqualTo("Wallace"));
			Assert.That(newActor2.Id, Is.EqualTo(1));
			Assert.That(m_testProject.VoiceActorList.ActiveActors.Contains(newActor2));
			Assert.That(newActor2, Is.EqualTo(group2.VoiceActor));
			Assert.That(newActor2.Gender, Is.EqualTo(ActorGender.Female));
			Assert.That(newActor2.Age, Is.EqualTo(ActorAge.Adult));
			Assert.That(newActor3.Name, Is.EqualTo("Gromit"));
			Assert.That(newActor3.Id, Is.EqualTo(2));
			Assert.That(m_testProject.VoiceActorList.ActiveActors.Contains(newActor3));
			Assert.That(newActor3, Is.EqualTo(group3.VoiceActor));
			Assert.That(newActor3.Gender, Is.EqualTo(ActorGender.Male));
			Assert.That(newActor3.Age, Is.EqualTo(ActorAge.Child));
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
