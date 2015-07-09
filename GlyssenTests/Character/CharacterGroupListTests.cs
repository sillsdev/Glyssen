using Glyssen.Character;
using Glyssen.VoiceActor;
using NUnit.Framework;

namespace GlyssenTests.Character
{
	[TestFixture]
	class CharacterGroupListTests
	{
		[Test]
		public void AnyVoiceActorAssigned_NoneAssigned_ReturnsFalse()
		{
			var list = new CharacterGroupList();
			var group = new CharacterGroup(1);
			group.CharacterIds.Add("A");
			list.CharacterGroups.Add(group);

			Assert.IsFalse(list.AnyVoiceActorAssigned());
		}

		[Test]
		public void AnyVoiceActorAssigned_OneAssigned_ReturnsTrue()
		{
			var list = new CharacterGroupList();
			var group = new CharacterGroup(1);
			group.CharacterIds.Add("A");
			group.AssignVoiceActor(new VoiceActor { Id = 0, Name = "A" });
			list.CharacterGroups.Add(group);

			Assert.IsTrue(list.AnyVoiceActorAssigned());
		}
		[Test]
		public void HasVoiceActorAssigned_NoneAssigned_ReturnsFalse()
		{
			var list = new CharacterGroupList();
			var group = new CharacterGroup(1);
			group.CharacterIds.Add("A");
			list.CharacterGroups.Add(group);

			Assert.IsFalse(list.HasVoiceActorAssigned(1));
		}

		[Test]
		public void HasVoiceActorAssigned_RequestedActorNotAssigned_ReturnsFalse()
		{
			var list = new CharacterGroupList();
			var group = new CharacterGroup(1);
			group.CharacterIds.Add("A");
			group.AssignVoiceActor(new VoiceActor { Id = 0, Name = "A" });
			list.CharacterGroups.Add(group);

			Assert.IsFalse(list.HasVoiceActorAssigned(1));
		}

		[Test]
		public void HasVoiceActorAssigned_RequestedActorAssigned_ReturnsTrue()
		{
			var list = new CharacterGroupList();
			var group = new CharacterGroup(1);
			group.CharacterIds.Add("A");
			group.AssignVoiceActor(new VoiceActor { Id = 1, Name = "A" });
			list.CharacterGroups.Add(group);

			Assert.IsTrue(list.HasVoiceActorAssigned(1));
		}
	}
}
