using System.Collections.Generic;
using Glyssen;
using Glyssen.Character;
using NUnit.Framework;

namespace GlyssenTests.Character
{
	[TestFixture]
	class CharacterGroupListTests
	{
		private Project m_project;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			m_project = TestProject.CreateBasicTestProject();
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			m_project = null;
			TestProject.DeleteTestProjectFolder();
		}

		[Test]
		public void AnyVoiceActorAssigned_NoneAssigned_ReturnsFalse()
		{
			var list = new CharacterGroupList();
			var group = new CharacterGroup(m_project, 1);
			list.CharacterGroups.Add(group);

			Assert.IsFalse(list.AnyVoiceActorAssigned());
		}

		[Test]
		public void AnyVoiceActorAssigned_OneAssigned_ReturnsTrue()
		{
			var list = new CharacterGroupList();
			var group = new CharacterGroup(m_project, 1);
			group.AssignVoiceActor(0);
			list.CharacterGroups.Add(group);

			Assert.IsTrue(list.AnyVoiceActorAssigned());
		}
		[Test]
		public void HasVoiceActorAssigned_NoneAssigned_ReturnsFalse()
		{
			var list = new CharacterGroupList();
			var group = new CharacterGroup(m_project, 1);
			list.CharacterGroups.Add(group);

			Assert.IsFalse(list.HasVoiceActorAssigned(1));
		}

		[Test]
		public void HasVoiceActorAssigned_RequestedActorNotAssigned_ReturnsFalse()
		{
			var list = new CharacterGroupList();
			var group = new CharacterGroup(m_project, 1);
			group.AssignVoiceActor(0);
			list.CharacterGroups.Add(group);

			Assert.IsFalse(list.HasVoiceActorAssigned(1));
		}

		[Test]
		public void HasVoiceActorAssigned_RequestedActorAssigned_ReturnsTrue()
		{
			var list = new CharacterGroupList();
			var group = new CharacterGroup(m_project, 1);
			group.AssignVoiceActor(1);
			list.CharacterGroups.Add(group);

			Assert.IsTrue(list.HasVoiceActorAssigned(1));
		}

		[Test]
		public void RemoveVoiceActor_ActorAssignedToMoreThanOneGroup_RemovesAllAssignmentsForSpecifiedActorOnly()
		{
			// Note: this tests something that is actually illegal: there should only ever be one group assigned
			// to a particular actor.
			var list = new CharacterGroupList();
			var group1 = new CharacterGroup(m_project, 1);
			var group2 = new CharacterGroup(m_project, 2);
			var group3 = new CharacterGroup(m_project, 3);

			group1.AssignVoiceActor(1);
			list.CharacterGroups.Add(group1);
			group2.AssignVoiceActor(1);
			list.CharacterGroups.Add(group2);
			group3.AssignVoiceActor(2);
			list.CharacterGroups.Add(group3);

			list.RemoveVoiceActor(1);

			Assert.IsFalse(list.HasVoiceActorAssigned(1));
			Assert.IsTrue(list.HasVoiceActorAssigned(2));
		}

		[Test]
		public void PopulateEstimatedHours()
		{
			var list = new CharacterGroupList();
			var group1 = new CharacterGroup(m_project, 1)
			{
				CharacterIds = { "A", "B" }
			};
			list.CharacterGroups.Add(group1);

			var group2 = new CharacterGroup(m_project, 2)
			{
				CharacterIds = { "C", "D" }
			};
			list.CharacterGroups.Add(group2);

			var keyStrokesPerCharacter = new Dictionary<string, int>(4);
			keyStrokesPerCharacter["A"] = 3500;
			keyStrokesPerCharacter["B"] = 4000;
			keyStrokesPerCharacter["C"] = 198;
			list.PopulateEstimatedHours(keyStrokesPerCharacter);

			Assert.AreEqual((3500 + 4000d) / Program.kKeyStrokesPerHour, group1.EstimatedHours);
			Assert.AreEqual(198d / Program.kKeyStrokesPerHour, group2.EstimatedHours);
		}
	}
}
