using System.Collections.Generic;
using System.Linq;
using Glyssen;
using Glyssen.Character;
using Glyssen.Controls;
using Glyssen.Dialogs;
using GlyssenTests.Properties;
using NUnit.Framework;

namespace GlyssenTests.Dialogs
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
			m_testProject.VoiceActorList.Actors.Clear();
			m_testProject.VoiceActorList.Actors.AddRange(new List<Glyssen.VoiceActor.VoiceActor>
			{
				new Glyssen.VoiceActor.VoiceActor{Id = 1, Name = "Mergat"},
				new Glyssen.VoiceActor.VoiceActor{Id = 2, Name = "Hendrick"},
				new Glyssen.VoiceActor.VoiceActor{Id = 3, Name = "Polygo"},
				new Glyssen.VoiceActor.VoiceActor{Id = 4, Name = "Imran"},
			});
			m_model = new VoiceActorInformationViewModel(m_testProject);
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			TestProject.DeleteTestProjectFolder();
		}

		[Test]
		public void DeleteVoiceActors_ActorsDeleted()
		{
			var actorsToDelete = new HashSet<Glyssen.VoiceActor.VoiceActor>(m_testProject.VoiceActorList.Actors.Where(a => a.Id < 3));
			Assert.AreEqual(4, m_testProject.VoiceActorList.Actors.Count);
			Assert.True(m_model.DeleteVoiceActors(actorsToDelete));
			Assert.AreEqual(2, m_testProject.VoiceActorList.Actors.Count);
		}

		[Test]
		public void DeleteVoiceActors_SomeActorsAssigned_CountsAreAccurateAndAssignmentsAreRemoved()
		{
			var actorsToDelete = new HashSet<Glyssen.VoiceActor.VoiceActor>(m_testProject.VoiceActorList.Actors.Where(a => a.Id < 3));
			var priorityComparer = new CharacterByKeyStrokeComparer(m_testProject.GetKeyStrokesByCharacterId());
			var characterGroup1 = new CharacterGroup(m_testProject, priorityComparer);
			var characterGroup2 = new CharacterGroup(m_testProject, priorityComparer);
			m_testProject.CharacterGroupList.CharacterGroups.Add(characterGroup1);
			m_testProject.CharacterGroupList.CharacterGroups.Add(characterGroup2);
			characterGroup1.AssignVoiceActor(2);
			characterGroup2.AssignVoiceActor(4);
			Assert.AreEqual(4, m_testProject.VoiceActorList.Actors.Count);
			Assert.True(m_model.DeleteVoiceActors(actorsToDelete));
			Assert.AreEqual(2, m_testProject.VoiceActorList.Actors.Count);
			Assert.IsFalse(characterGroup1.IsVoiceActorAssigned);
			Assert.IsTrue(characterGroup2.IsVoiceActorAssigned);
		}

		[Test]
		public void DeleteVoiceActors_NoActorsProvided_ReturnsFalse()
		{
			Assert.False(m_model.DeleteVoiceActors(new HashSet<Glyssen.VoiceActor.VoiceActor>()));
		}

		[Test]
		public void AssessChanges_VoiceActorAdded_UndoActionCreated()
		{
			Assert.AreEqual(4, m_testProject.VoiceActorList.Actors.Count);
			m_model.AddNewActor().Name = "Phoenix";
			Assert.AreEqual(5, m_testProject.VoiceActorList.Actors.Count);
			m_model.AssessChanges();
			Assert.IsTrue(m_model.Changes.Single() is VoiceActorAddedUndoAction);
		}

		[Test]
		public void AssessChanges_VoiceActorModified_UndoActionCreated()
		{
			Assert.AreEqual(4, m_testProject.VoiceActorList.Actors.Count);
			m_testProject.VoiceActorList.Actors[0].Name = "Monkey Soup";
			m_model.AssessChanges();
			Assert.IsTrue(m_model.Changes.Single() is VoiceActorEditUndoAction);
		}

		[Test]
		public void AssessChanges_VoiceActorDeleted_UndoActionCreated()
		{
			var actorsToDelete = new HashSet<Glyssen.VoiceActor.VoiceActor>(m_testProject.VoiceActorList.Actors.Where(a => a.Id == 3));
			Assert.True(m_model.DeleteVoiceActors(actorsToDelete));
			m_model.AssessChanges();
			Assert.IsTrue(m_model.Changes.Single() is VoiceActorDeletedUndoAction);
		}

		[Test]
		public void AssessChanges_DeleteNewlyAddedVoiceActor_NoUndoActionCreated()
		{
			var addedActor = m_model.AddNewActor();
			Assert.AreEqual(5, m_testProject.VoiceActorList.Actors.Count);
			var actorsToDelete = new HashSet<Glyssen.VoiceActor.VoiceActor>();
			actorsToDelete.Add(addedActor);
			Assert.True(m_model.DeleteVoiceActors(actorsToDelete));
			Assert.AreEqual(4, m_testProject.VoiceActorList.Actors.Count);
			m_model.AssessChanges();
			Assert.AreEqual(0, m_model.Changes.Count());
		}
	}
}
