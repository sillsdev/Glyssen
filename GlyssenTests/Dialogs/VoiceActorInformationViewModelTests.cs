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
			m_model = new VoiceActorInformationViewModel();
			m_model.Initialize(m_testProject);
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
			Assert.True(m_model.DeleteVoiceActors(actorsToDelete, false));
			Assert.AreEqual(2, m_testProject.VoiceActorList.Actors.Count);
		}

		[Test]
		public void DeleteVoiceActors_NoActorsProvided_ReturnsFalse()
		{
			Assert.False(m_model.DeleteVoiceActors(new HashSet<Glyssen.VoiceActor.VoiceActor>(), false));
		}
	}
}
