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
			group.AssignVoiceActor(new VoiceActor { Id = 0, Name = "A" });
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
			group.AssignVoiceActor(new VoiceActor { Id = 0, Name = "A" });
			list.CharacterGroups.Add(group);

			Assert.IsFalse(list.HasVoiceActorAssigned(1));
		}

		[Test]
		public void HasVoiceActorAssigned_RequestedActorAssigned_ReturnsTrue()
		{
			var list = new CharacterGroupList();
			var group = new CharacterGroup(1);
			group.AssignVoiceActor(new VoiceActor { Id = 1, Name = "A" });
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
			var voiceActor1 = new VoiceActor { Id = 1, Name = "A" };
			var voiceActor2 = new VoiceActor { Id = 2, Name = "B" };
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
			var voiceActor1 = new VoiceActor { Id = 1, Name = "A" };
			var voiceActor2 = new VoiceActor { Id = 2, Name = "B" };
			group1.AssignVoiceActor(voiceActor1);
			group1.AssignVoiceActor(voiceActor2);
			list.CharacterGroups.Add(group1);

			var group2 = new CharacterGroup(2)
			{
				CharacterIds = { "C", "D" }
			};
			var voiceActor3 = new VoiceActor { Id = 3, Name = "C" };
			var voiceActor4 = new VoiceActor { Id = 4, Name = "D" };
			group2.AssignVoiceActor(voiceActor3);
			group2.AssignVoiceActor(voiceActor4);
			list.CharacterGroups.Add(group2);

			BookScript book = new BookScript();
			book.Blocks = new List<Block>
			{
				new Block
				{
					BlockElements = new List<BlockElement> { new ScriptText(GetRandomStringOfLength(3500)) },
					CharacterId = "A"
				},
				new Block
				{
					BlockElements = new List<BlockElement> { new ScriptText(GetRandomStringOfLength(4000)) },
					CharacterId = "B"
				},
				new Block
				{
					BlockElements = new List<BlockElement> { new ScriptText(GetRandomStringOfLength(198)) },
					CharacterId = "C"
				}
			};
			list.PopulateEstimatedHours(new List<BookScript> { book });

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
