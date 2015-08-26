using System.Collections.Generic;
using System.Text;
using Glyssen;
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
			list.CharacterGroups.Add(group);

			Assert.IsFalse(list.AnyVoiceActorAssigned());
		}

		[Test]
		public void AnyVoiceActorAssigned_OneAssigned_ReturnsTrue()
		{
			var list = new CharacterGroupList();
			var group = new CharacterGroup(1);
			group.AssignVoiceActor(new Glyssen.VoiceActor.VoiceActor { Id = 0, Name = "A" });
			list.CharacterGroups.Add(group);

			Assert.IsTrue(list.AnyVoiceActorAssigned());
		}
		[Test]
		public void HasVoiceActorAssigned_NoneAssigned_ReturnsFalse()
		{
			var list = new CharacterGroupList();
			var group = new CharacterGroup(1);
			list.CharacterGroups.Add(group);

			Assert.IsFalse(list.HasVoiceActorAssigned(1));
		}

		[Test]
		public void HasVoiceActorAssigned_RequestedActorNotAssigned_ReturnsFalse()
		{
			var list = new CharacterGroupList();
			var group = new CharacterGroup(1);
			group.AssignVoiceActor(new Glyssen.VoiceActor.VoiceActor { Id = 0, Name = "A" });
			list.CharacterGroups.Add(group);

			Assert.IsFalse(list.HasVoiceActorAssigned(1));
		}

		[Test]
		public void HasVoiceActorAssigned_RequestedActorAssigned_ReturnsTrue()
		{
			var list = new CharacterGroupList();
			var group = new CharacterGroup(1);
			group.AssignVoiceActor(new Glyssen.VoiceActor.VoiceActor { Id = 1, Name = "A" });
			list.CharacterGroups.Add(group);

			Assert.IsTrue(list.HasVoiceActorAssigned(1));
		}

		[Test]
		public void RemoveVoiceActor_ActorAssignedToMoreThanOneGroup_RemovesAllAssignmentsForSpecifiedActorOnly()
		{
			var list = new CharacterGroupList();
			var group1 = new CharacterGroup(1);
			var group2 = new CharacterGroup(2);
			var group3 = new CharacterGroup(3);
			var voiceActor1 = new Glyssen.VoiceActor.VoiceActor { Id = 1, Name = "A" };
			var voiceActor2 = new Glyssen.VoiceActor.VoiceActor { Id = 2, Name = "B" };
			group1.AssignVoiceActor(voiceActor1);
			list.CharacterGroups.Add(group1);
			group2.AssignVoiceActor(voiceActor1);
			list.CharacterGroups.Add(group2);
			group3.AssignVoiceActor(voiceActor2);
			list.CharacterGroups.Add(group3);

			list.RemoveVoiceActor(1);

			Assert.IsFalse(list.HasVoiceActorAssigned(1));
			Assert.IsTrue(list.HasVoiceActorAssigned(2));
		}

		[Test]
		public void PopulateEstimatedHours()
		{
			var list = new CharacterGroupList();
			var group1 = new CharacterGroup(1)
			{
				CharacterIds = { "A", "B" }
			};
			var voiceActor1 = new Glyssen.VoiceActor.VoiceActor { Id = 1, Name = "A" };
			var voiceActor2 = new Glyssen.VoiceActor.VoiceActor { Id = 2, Name = "B" };
			group1.AssignVoiceActor(voiceActor1);
			group1.AssignVoiceActor(voiceActor2);
			list.CharacterGroups.Add(group1);

			var group2 = new CharacterGroup(2)
			{
				CharacterIds = { "C", "D" }
			};
			var voiceActor3 = new Glyssen.VoiceActor.VoiceActor { Id = 3, Name = "C" };
			var voiceActor4 = new Glyssen.VoiceActor.VoiceActor { Id = 4, Name = "D" };
			group2.AssignVoiceActor(voiceActor3);
			group2.AssignVoiceActor(voiceActor4);
			list.CharacterGroups.Add(group2);

			var keyStrokesPerCharacter = new Dictionary<string, int>(4);
			keyStrokesPerCharacter["A"] = 3500;
			keyStrokesPerCharacter["B"] = 4000;
			keyStrokesPerCharacter["C"] = 198;
			list.PopulateEstimatedHours(keyStrokesPerCharacter);

			Assert.AreEqual((3500 + 4000d) / Program.kKeyStrokesPerHour, group1.EstimatedHours);
			Assert.AreEqual(198d / Program.kKeyStrokesPerHour, group2.EstimatedHours);
		}

		private string GetRandomStringOfLength(int length)
		{
			var sb = new StringBuilder();
			for (int i = 0; i < length; i++)
				sb.Append("x");
			return sb.ToString();
		}
	}
}
