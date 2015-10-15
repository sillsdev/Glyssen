using System.Collections.Generic;
using System.Linq;
using Glyssen;
using Glyssen.Character;
using Glyssen.Controls;
using GlyssenTests.Properties;
using NUnit.Framework;

namespace GlyssenTests.Dialogs
{
	[TestFixture]
	class VoiceActorInformationViewModelTests
	{
		private Project m_testProject;
		private VoiceActorInformationViewModel m_model;
		private int m_countOfAssignedActorsToDelete;
		private int m_countOfActorsToDelete;
		private bool m_cancel;

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
			m_countOfAssignedActorsToDelete = -1;
			m_countOfActorsToDelete = -1;
			m_cancel = false;

			m_model = new VoiceActorInformationViewModel(m_testProject);
			m_model.DeletingActors += delegate(VoiceActorInformationViewModel sender, DeletingActorsEventArgs args)
			{
				m_countOfAssignedActorsToDelete = args.CountOfAssignedActorsToDelete;
				m_countOfActorsToDelete = args.CountOfActorsToDelete;
				args.Cancel = m_cancel;
			};
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			TestProject.DeleteTestProjectFolder();
		}

		[Test]
		public void DeleteVoiceActors_ActorsDeleted()
		{
			m_testProject.VoiceActorList.Actors.AddRange(new List<Glyssen.VoiceActor.VoiceActor>
			{
				new Glyssen.VoiceActor.VoiceActor{Id = 1},
				new Glyssen.VoiceActor.VoiceActor{Id = 2},
				new Glyssen.VoiceActor.VoiceActor{Id = 3},
				new Glyssen.VoiceActor.VoiceActor{Id = 4},
			});
			var actorsToDelete = new HashSet<Glyssen.VoiceActor.VoiceActor>(m_testProject.VoiceActorList.Actors.Where(a => a.Id < 3));
			Assert.AreEqual(4, m_testProject.VoiceActorList.Actors.Count);
			Assert.True(m_model.DeleteVoiceActors(actorsToDelete));
			Assert.AreEqual(0, m_countOfAssignedActorsToDelete);
			Assert.AreEqual(2, m_countOfActorsToDelete);
			Assert.AreEqual(2, m_testProject.VoiceActorList.Actors.Count);
		}

		[Test]
		public void DeleteVoiceActors_Cancelled_ActorsNotDeleted()
		{
			m_cancel = true;
			m_testProject.VoiceActorList.Actors.AddRange(new List<Glyssen.VoiceActor.VoiceActor>
			{
				new Glyssen.VoiceActor.VoiceActor{Id = 1},
				new Glyssen.VoiceActor.VoiceActor{Id = 2},
				new Glyssen.VoiceActor.VoiceActor{Id = 3},
				new Glyssen.VoiceActor.VoiceActor{Id = 4},
			});
			var actorsToDelete = new HashSet<Glyssen.VoiceActor.VoiceActor>(m_testProject.VoiceActorList.Actors.Where(a => a.Id < 3));
			Assert.AreEqual(4, m_testProject.VoiceActorList.Actors.Count);
			Assert.IsFalse(m_model.DeleteVoiceActors(actorsToDelete));
			Assert.AreEqual(4, m_testProject.VoiceActorList.Actors.Count);
		}

		[Test]
		public void DeleteVoiceActors_SomeActorsAssigned_CountsAreAccurateAndAssignmentsAreRemoved()
		{
			m_testProject.VoiceActorList.Actors.AddRange(new List<Glyssen.VoiceActor.VoiceActor>
			{
				new Glyssen.VoiceActor.VoiceActor{Id = 1},
				new Glyssen.VoiceActor.VoiceActor{Id = 2},
				new Glyssen.VoiceActor.VoiceActor{Id = 3},
				new Glyssen.VoiceActor.VoiceActor{Id = 4},
			});
			var actorsToDelete = new HashSet<Glyssen.VoiceActor.VoiceActor>(m_testProject.VoiceActorList.Actors.Where(a => a.Id < 3));
			var characterGroup1 = new CharacterGroup(1);
			var characterGroup2 = new CharacterGroup(2);
			m_testProject.CharacterGroupList.CharacterGroups.Add(characterGroup1);
			m_testProject.CharacterGroupList.CharacterGroups.Add(characterGroup2);
			characterGroup1.AssignVoiceActor(2);
			characterGroup2.AssignVoiceActor(4);
			Assert.AreEqual(4, m_testProject.VoiceActorList.Actors.Count);
			Assert.True(m_model.DeleteVoiceActors(actorsToDelete));
			Assert.AreEqual(1, m_countOfAssignedActorsToDelete);
			Assert.AreEqual(2, m_countOfActorsToDelete);
			Assert.AreEqual(2, m_testProject.VoiceActorList.Actors.Count);
			Assert.IsFalse(characterGroup1.IsVoiceActorAssigned);
			Assert.IsTrue(characterGroup2.IsVoiceActorAssigned);
		}

		[Test]
		public void DeleteVoiceActors_NoActorsProvided_ReturnsFalse()
		{
			Assert.False(m_model.DeleteVoiceActors(new HashSet<Glyssen.VoiceActor.VoiceActor>()));
			Assert.AreEqual(-1, m_countOfAssignedActorsToDelete);
			Assert.AreEqual(-1, m_countOfActorsToDelete);
		}
	}
}
