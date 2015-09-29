using System.Collections.Generic;
using Glyssen;
using Glyssen.Character;
using Glyssen.Dialogs;
using NUnit.Framework;

namespace GlyssenTests.Dialogs
{
	[TestFixture]
	internal class VoiceActorAssignmentUndoActionTests
	{
		[SetUp]
		public void Setup()
		{
			foreach (var group in m_testProject.CharacterGroupList.CharacterGroups)
				group.RemoveVoiceActor();
		}

		private Project m_testProject;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.MRK);
			var actor1 = new Glyssen.VoiceActor.VoiceActor {Id = 1, Name = "Oneyda Figueroa"};
			var actor2 = new Glyssen.VoiceActor.VoiceActor {Id = 2, Name = "Paul Twomey"};
			var actor3 = new Glyssen.VoiceActor.VoiceActor {Id = 3, Name = "Threesa Hawkins"};
			m_testProject.VoiceActorList.Actors = new List<Glyssen.VoiceActor.VoiceActor> {actor1, actor2, actor3};

			AddCharacterGroup("Jesus");
		}

		private void AddCharacterGroup(params string[] characterIds)
		{
			var group = new CharacterGroup();
			foreach (var character in characterIds)
				group.CharacterIds.Add(character);
			m_testProject.CharacterGroupList.CharacterGroups.Add(group);
		}

		[Test]
		public void Constructor_MakesAssignment()
		{
			var groupWithJesus = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
			groupWithJesus.AssignVoiceActor(3);
			Assert.IsNotNull(new VoiceActorAssignmentUndoAction(m_testProject, groupWithJesus, 1));
			Assert.AreEqual(1, groupWithJesus.VoiceActorId);
		}

		[Test]
		public void Description()
		{
			var groupWithJesus = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
			groupWithJesus.AssignVoiceActor(3);
			var action = new VoiceActorAssignmentUndoAction(m_testProject, groupWithJesus, 1);
			Assert.AreEqual("Assign voice actor Oneyda Figueroa", action.Description);
		}

		[Test]
		public void Redo_GroupWithAssignmentNotFound_ReturnsFalse()
		{
			var groupWithJesus = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
			groupWithJesus.AssignVoiceActor(2);
			var action = new VoiceActorAssignmentUndoAction(m_testProject, groupWithJesus, 3);// This will assign it to 3.
			action.Undo(); // This will reassign it back to 2.
			groupWithJesus.AssignVoiceActor(3);
			Assert.IsFalse(action.Redo());
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
		}

		[Test]
		public void Redo_PreviousAssignment_AssignmentCleared()
		{
			var groupWithJesus = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
			groupWithJesus.AssignVoiceActor(3);
			var action = new VoiceActorAssignmentUndoAction(m_testProject, groupWithJesus, 2);
			action.Undo();
			Assert.IsTrue(action.Redo());
			Assert.AreEqual(2, groupWithJesus.VoiceActorId);
		}

		[Test]
		public void Undo_GroupWithAssignmentNotFound_ReturnsFalse()
		{
			var groupWithJesus = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
			groupWithJesus.AssignVoiceActor(2);
			var action = new VoiceActorAssignmentUndoAction(m_testProject, groupWithJesus, 3);// This will assign it to 3.
			groupWithJesus.AssignVoiceActor(2);
			Assert.IsFalse(action.Undo());
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
		}

		[Test]
		public void Undo_PreviousAssignment_AssignmentCleared()
		{
			var groupWithJesus = m_testProject.CharacterGroupList.GroupContainingCharacterId("Jesus");
			groupWithJesus.AssignVoiceActor(3);
			var action = new VoiceActorAssignmentUndoAction(m_testProject, groupWithJesus, 2);
			Assert.IsTrue(action.Undo());
			Assert.AreEqual(3, groupWithJesus.VoiceActorId);
		}
	}
}