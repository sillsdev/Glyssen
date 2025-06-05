using System;
using System.Collections.Generic;
using System.Linq;
using GlyssenCharacters;
using GlyssenEngine;
using GlyssenEngine.Casting;
using GlyssenEngine.Character;
using GlyssenEngine.UndoActions;
using GlyssenEngine.ViewModels;
using NUnit.Framework;
using Resources = GlyssenCharactersTests.Properties.Resources;

namespace GlyssenEngineTests.ViewModelTests
{
	[TestFixture]
	class VoiceActorInformationViewModelTests
	{
		private Project m_testProject;
		private VoiceActorInformationViewModel m_model;

		[OneTimeSetUp]
		public void OneTimeSetUp()
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

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			TestProject.DeleteTestProjects();
		}

		[Test]
		public void ValidateActor_NameIsBlank_ReturnNoName()
		{
			m_model.AddNewActor();
			Assert.That(VoiceActorInformationViewModel.ActorValidationState.NoName, Is.EqualTo(m_model.ValidateActor(m_model.Actors.Count - 1)));
		}

		[Test]
		public void ValidateActor_NameIsNull_ReturnNoName()
		{
			m_model.AddNewActor().Name = null;
			Assert.That(VoiceActorInformationViewModel.ActorValidationState.NoName, Is.EqualTo(m_model.ValidateActor(m_model.Actors.Count - 1)));
		}

		[Test]
		public void ValidateActor_NameIsSpaces_ReturnNoName()
		{
			m_model.AddNewActor().Name = "     ";
			Assert.That(VoiceActorInformationViewModel.ActorValidationState.NoName, Is.EqualTo(m_model.ValidateActor(m_model.Actors.Count - 1)));
		}

		[TestCase(0)]
		[TestCase(1)]
		[TestCase(2)]
		[TestCase(3)]
		public void ValidateActor_NameIsValid_ReturnValid(int index)
		{
			Assert.That(VoiceActorInformationViewModel.ActorValidationState.Valid, Is.EqualTo(m_model.ValidateActor(index)));
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
			Assert.That(m_testProject.VoiceActorList.AllActors.Count, Is.EqualTo(4));
			Assert.That(m_model.DeleteVoiceActors(actorsToDelete), Is.True);
			Assert.That(m_testProject.VoiceActorList.AllActors.Count, Is.EqualTo(2));
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
			Assert.That(m_testProject.VoiceActorList.AllActors.Count, Is.EqualTo(4));
			Assert.That(m_model.DeleteVoiceActors(actorsToDelete), Is.True);
			Assert.That(m_testProject.VoiceActorList.AllActors.Count, Is.EqualTo(2));
			Assert.That(characterGroup1.IsVoiceActorAssigned, Is.False);
			Assert.That(characterGroup2.IsVoiceActorAssigned, Is.True);
		}

		[Test]
		public void DeleteVoiceActors_NoActorsProvided_ReturnsFalse()
		{
			Assert.That(m_model.DeleteVoiceActors(new HashSet<VoiceActor>()), Is.False);
		}

		[Test]
		public void SetInactive_PreviouslyInactiveSetToInactive_NoChange()
		{
			var actor = new VoiceActor { Id = 5, Name = "Gubaru", IsInactive = true };
			m_testProject.VoiceActorList.AllActors.Add(actor);
			var characterGroup = new CharacterGroup(m_testProject);
			m_testProject.CharacterGroupList.CharacterGroups.Add(characterGroup);
			Assert.That(actor.IsInactive, Is.True);
			Assert.That(m_testProject.CharacterGroupList.HasVoiceActorAssigned(actor.Id), Is.False);

			m_model.SetInactive(actor, true);
			Assert.That(actor.IsInactive, Is.True);
			Assert.That(m_testProject.CharacterGroupList.HasVoiceActorAssigned(actor.Id), Is.False);
		}

		[Test]
		public void SetInactive_PreviouslyActiveAndAssignedSetToActive_NoChange()
		{
			var actor = m_testProject.VoiceActorList.AllActors.Single(a => a.Id == 2);
			var characterGroup = new CharacterGroup(m_testProject);
			m_testProject.CharacterGroupList.CharacterGroups.Add(characterGroup);
			characterGroup.AssignVoiceActor(actor.Id);
			Assert.That(actor.IsInactive, Is.False);
			Assert.That(m_testProject.CharacterGroupList.HasVoiceActorAssigned(actor.Id), Is.True);

			m_model.SetInactive(actor, false);
			Assert.That(actor.IsInactive, Is.False);
			Assert.That(m_testProject.CharacterGroupList.HasVoiceActorAssigned(actor.Id), Is.True);
		}

		[Test]
		public void SetInactive_PreviouslyActiveAndUnassignedSetToInactive_ModifyActiveStateButNoChangeToGroups()
		{
			var actor = m_testProject.VoiceActorList.AllActors.Single(a => a.Id == 2);
			var characterGroup = new CharacterGroup(m_testProject);
			m_testProject.CharacterGroupList.CharacterGroups.Add(characterGroup);
			Assert.That(actor.IsInactive, Is.False);
			Assert.That(m_testProject.CharacterGroupList.HasVoiceActorAssigned(actor.Id), Is.False);

			m_model.SetInactive(actor, true);
			Assert.That(actor.IsInactive, Is.True);
			Assert.That(m_testProject.CharacterGroupList.HasVoiceActorAssigned(actor.Id), Is.False);
		}

		[Test]
		public void SetInactive_PreviouslyActiveAndAssignedSetToInactive_ModifyActiveStateAndUnassign()
		{
			var actor = m_testProject.VoiceActorList.AllActors.Single(a => a.Id == 2);
			var characterGroup = new CharacterGroup(m_testProject);
			m_testProject.CharacterGroupList.CharacterGroups.Add(characterGroup);
			characterGroup.AssignVoiceActor(actor.Id);
			Assert.That(actor.IsInactive, Is.False);
			Assert.That(m_testProject.CharacterGroupList.HasVoiceActorAssigned(actor.Id), Is.True);

			m_model.SetInactive(actor, true);
			Assert.That(actor.IsInactive, Is.True);
			Assert.That(m_testProject.CharacterGroupList.HasVoiceActorAssigned(actor.Id), Is.False);
		}

		[Test]
		public void Changes_VoiceActorAdded_UndoActionCreated()
		{
			Assert.That(m_testProject.VoiceActorList.AllActors.Count, Is.EqualTo(4));
			m_model.AddNewActor().Name = "Phoenix";
			Assert.That(m_testProject.VoiceActorList.AllActors.Count, Is.EqualTo(5));
			Assert.That(m_model.Changes.Single() is VoiceActorAddedUndoAction, Is.True);
		}

		[Test]
		public void Changes_VoiceActorModified_UndoActionCreated()
		{
			Assert.That(m_testProject.VoiceActorList.AllActors.Count, Is.EqualTo(4));
			m_testProject.VoiceActorList.AllActors[0].Name = "Monkey Soup";
			Assert.That(m_model.Changes.Single() is VoiceActorEditUndoAction, Is.True);
		}

		[Test]
		public void Changes_VoiceActorDeleted_UndoActionCreated()
		{
			var actorsToDelete = new HashSet<VoiceActor>(m_testProject.VoiceActorList.AllActors.Where(a => a.Id == 3));
			Assert.That(m_model.DeleteVoiceActors(actorsToDelete), Is.True);
			Assert.That(m_model.Changes.Single() is VoiceActorDeletedUndoAction, Is.True);
		}

		[Test]
		public void Changes_DeleteNewlyAddedVoiceActor_NoUndoActionCreated()
		{
			var addedActor = m_model.AddNewActor();
			Assert.That(m_testProject.VoiceActorList.AllActors.Count, Is.EqualTo(5));
			var actorsToDelete = new HashSet<VoiceActor>();
			actorsToDelete.Add(addedActor);
			Assert.That(m_model.DeleteVoiceActors(actorsToDelete), Is.True);
			Assert.That(m_testProject.VoiceActorList.AllActors.Count, Is.EqualTo(4));
			Assert.That(m_model.Changes.Count(), Is.EqualTo(0));
		}
	}
}
