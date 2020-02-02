using System;
using System.Collections.Generic;
using System.Linq;
using GlyssenEngine;
using GlyssenEngine.Casting;
using GlyssenEngine.Character;
using GlyssenEngine.UndoActions;
using GlyssenEngine.ViewModels;
using NUnit.Framework;
using Resources = GlyssenEngineTests.Properties.Resources;

namespace GlyssenEngineTests.ViewModelTests
{
	[TestFixture]
	class VoiceActorInformationViewModelTests
	{
		private Project m_testProject;
		private VoiceActorInformationViewModel m_model;

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
			m_testProject.VoiceActorList.AllActors.Clear();
			m_testProject.VoiceActorList.AllActors.AddRange(new List<VoiceActor>
			{
				new VoiceActor{Id = 1, Name = "Mergat"},
				new VoiceActor{Id = 2, Name = "Hendrick"},
				new VoiceActor{Id = 3, Name = "Polygo"},
				new VoiceActor{Id = 4, Name = "Imran"},
			});
			m_model = new VoiceActorInformationViewModel(m_testProject);
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			TestProject.DeleteTestProjectFolder();
		}

		[Test]
		public void ValidateActor_NameIsBlank_ReturnNoName()
		{
			m_model.AddNewActor();
			Assert.AreEqual(VoiceActorInformationViewModel.ActorValidationState.NoName, m_model.ValidateActor(m_model.Actors.Count - 1));
		}

		[Test]
		public void ValidateActor_NameIsNull_ReturnNoName()
		{
			m_model.AddNewActor().Name = null;
			Assert.AreEqual(VoiceActorInformationViewModel.ActorValidationState.NoName, m_model.ValidateActor(m_model.Actors.Count - 1));
		}

		[Test]
		public void ValidateActor_NameIsSpaces_ReturnNoName()
		{
			m_model.AddNewActor().Name = "     ";
			Assert.AreEqual(VoiceActorInformationViewModel.ActorValidationState.NoName, m_model.ValidateActor(m_model.Actors.Count - 1));
		}

		[TestCase(0)]
		[TestCase(1)]
		[TestCase(2)]
		[TestCase(3)]
		public void ValidateActor_NameIsValid_ReturnValid(int index)
		{
			Assert.AreEqual(VoiceActorInformationViewModel.ActorValidationState.Valid, m_model.ValidateActor(index));
		}

		[TestCase(-1)]
		[TestCase(4)]
		public void ValidateActor_IndexOutOfRange_ThrowsArgumentOutOfRangeException(int index)
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => m_model.ValidateActor(index));
		}

		[Test]
		public void DeleteVoiceActors_ActorsDeleted()
		{
			var actorsToDelete = new HashSet<VoiceActor>(m_testProject.VoiceActorList.AllActors.Where(a => a.Id < 3));
			Assert.AreEqual(4, m_testProject.VoiceActorList.AllActors.Count);
			Assert.True(m_model.DeleteVoiceActors(actorsToDelete));
			Assert.AreEqual(2, m_testProject.VoiceActorList.AllActors.Count);
		}

		[Test]
		public void DeleteVoiceActors_SomeActorsAssigned_CountsAreAccurateAndAssignmentsAreRemoved()
		{
			var actorsToDelete = new HashSet<VoiceActor>(m_testProject.VoiceActorList.AllActors.Where(a => a.Id < 3));
			var characterGroup1 = new CharacterGroup(m_testProject);
			var characterGroup2 = new CharacterGroup(m_testProject);
			m_testProject.CharacterGroupList.CharacterGroups.Add(characterGroup1);
			m_testProject.CharacterGroupList.CharacterGroups.Add(characterGroup2);
			characterGroup1.AssignVoiceActor(2);
			characterGroup2.AssignVoiceActor(4);
			Assert.AreEqual(4, m_testProject.VoiceActorList.AllActors.Count);
			Assert.True(m_model.DeleteVoiceActors(actorsToDelete));
			Assert.AreEqual(2, m_testProject.VoiceActorList.AllActors.Count);
			Assert.IsFalse(characterGroup1.IsVoiceActorAssigned);
			Assert.IsTrue(characterGroup2.IsVoiceActorAssigned);
		}

		[Test]
		public void DeleteVoiceActors_NoActorsProvided_ReturnsFalse()
		{
			Assert.False(m_model.DeleteVoiceActors(new HashSet<VoiceActor>()));
		}

		[Test]
		public void SetInactive_PreviouslyInactiveSetToInactive_NoChange()
		{
			var actor = new VoiceActor { Id = 5, Name = "Gubaru", IsInactive = true };
			m_testProject.VoiceActorList.AllActors.Add(actor);
			var characterGroup = new CharacterGroup(m_testProject);
			m_testProject.CharacterGroupList.CharacterGroups.Add(characterGroup);
			Assert.True(actor.IsInactive);
			Assert.False(m_testProject.CharacterGroupList.HasVoiceActorAssigned(actor.Id));

			m_model.SetInactive(actor, true);
			Assert.True(actor.IsInactive);
			Assert.False(m_testProject.CharacterGroupList.HasVoiceActorAssigned(actor.Id));
		}

		[Test]
		public void SetInactive_PreviouslyActiveAndAssignedSetToActive_NoChange()
		{
			var actor = m_testProject.VoiceActorList.AllActors.Single(a => a.Id == 2);
			var characterGroup = new CharacterGroup(m_testProject);
			m_testProject.CharacterGroupList.CharacterGroups.Add(characterGroup);
			characterGroup.AssignVoiceActor(actor.Id);
			Assert.False(actor.IsInactive);
			Assert.True(m_testProject.CharacterGroupList.HasVoiceActorAssigned(actor.Id));

			m_model.SetInactive(actor, false);
			Assert.False(actor.IsInactive);
			Assert.True(m_testProject.CharacterGroupList.HasVoiceActorAssigned(actor.Id));
		}

		[Test]
		public void SetInactive_PreviouslyActiveAndUnassignedSetToInactive_ModifyActiveStateButNoChangeToGroups()
		{
			var actor = m_testProject.VoiceActorList.AllActors.Single(a => a.Id == 2);
			var characterGroup = new CharacterGroup(m_testProject);
			m_testProject.CharacterGroupList.CharacterGroups.Add(characterGroup);
			Assert.False(actor.IsInactive);
			Assert.False(m_testProject.CharacterGroupList.HasVoiceActorAssigned(actor.Id));

			m_model.SetInactive(actor, true);
			Assert.True(actor.IsInactive);
			Assert.False(m_testProject.CharacterGroupList.HasVoiceActorAssigned(actor.Id));
		}

		[Test]
		public void SetInactive_PreviouslyActiveAndAssignedSetToInactive_ModifyActiveStateAndUnassign()
		{
			var actor = m_testProject.VoiceActorList.AllActors.Single(a => a.Id == 2);
			var characterGroup = new CharacterGroup(m_testProject);
			m_testProject.CharacterGroupList.CharacterGroups.Add(characterGroup);
			characterGroup.AssignVoiceActor(actor.Id);
			Assert.False(actor.IsInactive);
			Assert.True(m_testProject.CharacterGroupList.HasVoiceActorAssigned(actor.Id));

			m_model.SetInactive(actor, true);
			Assert.True(actor.IsInactive);
			Assert.False(m_testProject.CharacterGroupList.HasVoiceActorAssigned(actor.Id));
		}

		[Test]
		public void Changes_VoiceActorAdded_UndoActionCreated()
		{
			Assert.AreEqual(4, m_testProject.VoiceActorList.AllActors.Count);
			m_model.AddNewActor().Name = "Phoenix";
			Assert.AreEqual(5, m_testProject.VoiceActorList.AllActors.Count);
			Assert.IsTrue(m_model.Changes.Single() is VoiceActorAddedUndoAction);
		}

		[Test]
		public void Changes_VoiceActorModified_UndoActionCreated()
		{
			Assert.AreEqual(4, m_testProject.VoiceActorList.AllActors.Count);
			m_testProject.VoiceActorList.AllActors[0].Name = "Monkey Soup";
			Assert.IsTrue(m_model.Changes.Single() is VoiceActorEditUndoAction);
		}

		[Test]
		public void Changes_VoiceActorDeleted_UndoActionCreated()
		{
			var actorsToDelete = new HashSet<VoiceActor>(m_testProject.VoiceActorList.AllActors.Where(a => a.Id == 3));
			Assert.True(m_model.DeleteVoiceActors(actorsToDelete));
			Assert.IsTrue(m_model.Changes.Single() is VoiceActorDeletedUndoAction);
		}

		[Test]
		public void Changes_DeleteNewlyAddedVoiceActor_NoUndoActionCreated()
		{
			var addedActor = m_model.AddNewActor();
			Assert.AreEqual(5, m_testProject.VoiceActorList.AllActors.Count);
			var actorsToDelete = new HashSet<VoiceActor>();
			actorsToDelete.Add(addedActor);
			Assert.True(m_model.DeleteVoiceActors(actorsToDelete));
			Assert.AreEqual(4, m_testProject.VoiceActorList.AllActors.Count);
			Assert.AreEqual(0, m_model.Changes.Count());
		}
	}
}
