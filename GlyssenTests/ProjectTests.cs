using Glyssen.Character;
using NUnit.Framework;

namespace GlyssenTests
{
	internal class ProjectTests
	{
		[Test]
		public void IsVoiceActorAssignmentsComplete_Complete_ReturnsTrue()
		{
			var project = TestProject.CreateBasicTestProject();

			var actor = new Glyssen.VoiceActor.VoiceActor();
			project.VoiceActorList.Actors.Add(actor);
			var group = new CharacterGroup(project);
			group.AssignVoiceActor(actor.Id);
			project.CharacterGroupList.CharacterGroups.Add(group);

			Assert.True(project.IsVoiceActorAssignmentsComplete);
		}

		[Test]
		public void IsVoiceActorAssignmentsComplete_GroupWithNoActor_ReturnsFalse()
		{
			var project = TestProject.CreateBasicTestProject();

			var actor = new Glyssen.VoiceActor.VoiceActor();
			project.VoiceActorList.Actors.Add(actor);
			var group = new CharacterGroup(project);
			project.CharacterGroupList.CharacterGroups.Add(group);

			Assert.False(project.IsVoiceActorAssignmentsComplete);
		}

		[Test]
		public void EveryAssignedGroupHasACharacter_EveryAssignedGroupHasACharacter_ReturnsTrue()
		{
			var project = TestProject.CreateBasicTestProject();

			var actor = new Glyssen.VoiceActor.VoiceActor();
			project.VoiceActorList.Actors.Add(actor);
			var group = new CharacterGroup(project);
			group.CharacterIds.Add("Bob");
			group.AssignVoiceActor(actor.Id);
			project.CharacterGroupList.CharacterGroups.Add(group);

			Assert.True(project.CharacterGroupList.AnyVoiceActorAssigned());
			Assert.True(project.EveryAssignedGroupHasACharacter);
		}

		[Test]
		public void EveryAssignedGroupHasACharacter_AssignedGroupWithNoCharacter_ReturnsFalse()
		{
			var project = TestProject.CreateBasicTestProject();

			var actor = new Glyssen.VoiceActor.VoiceActor();
			project.VoiceActorList.Actors.Add(actor);
			var group = new CharacterGroup(project);
			group.AssignVoiceActor(actor.Id);
			project.CharacterGroupList.CharacterGroups.Add(group);

			Assert.True(project.CharacterGroupList.AnyVoiceActorAssigned());
			Assert.False(project.EveryAssignedGroupHasACharacter);
		}

		[Test]
		public void HasUnusedActor_NoUnusedActor_ReturnsFalse()
		{
			var project = TestProject.CreateBasicTestProject();

			var actor1 = new Glyssen.VoiceActor.VoiceActor();
			project.VoiceActorList.Actors.Add(actor1);
			var group = new CharacterGroup(project);
			group.AssignVoiceActor(actor1.Id);
			project.CharacterGroupList.CharacterGroups.Add(group);

			Assert.True(project.CharacterGroupList.AnyVoiceActorAssigned());
			Assert.False(project.HasUnusedActor);
		}

		[Test]
		public void HasUnusedActor_UnusedActor_ReturnsTrue()
		{
			var project = TestProject.CreateBasicTestProject();

			var actor1 = new Glyssen.VoiceActor.VoiceActor { Id = 0 };
			project.VoiceActorList.Actors.Add(actor1);
			var actor2 = new Glyssen.VoiceActor.VoiceActor { Id = 1 };
			project.VoiceActorList.Actors.Add(actor2);
			var group = new CharacterGroup(project);
			group.AssignVoiceActor(actor1.Id);
			project.CharacterGroupList.CharacterGroups.Add(group);

			Assert.True(project.CharacterGroupList.AnyVoiceActorAssigned());
			Assert.True(project.HasUnusedActor);
		}
	}
}
