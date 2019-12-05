using System;
using System.Collections.Generic;
using System.Linq;
using Glyssen;
using Glyssen.Character;
using Glyssen.Dialogs;
using GlyssenEngine.VoiceActor;
using NUnit.Framework;
using SIL.Extensions;

namespace GlyssenTests.Dialogs
{
	[TestFixture]
	class VoiceActorEditingUndoActionTests
	{
		private Project m_testProject;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.MRK);
		}

		[SetUp]
		public void Setup()
		{
			m_testProject.CharacterGroupList.CharacterGroups.Clear();
			m_testProject.VoiceActorList.AllActors.Clear();
		}

		private CharacterGroup AddCharacterGroup(params string[] characterIds)
		{
			var group = new CharacterGroup(m_testProject);
			foreach (var character in characterIds)
				group.CharacterIds.Add(character);
			m_testProject.CharacterGroupList.CharacterGroups.Add(group);
			return group;
		}

		[Test]
		public void Contructor_SingleEditingActionForUnassignedActor_AffectedGroupsIsEmpty()
		{
			m_testProject.VoiceActorList.AllActors.Add(new GlyssenEngine.VoiceActor.VoiceActor { Id = 1, Name = "B", Age = ActorAge.Adult });
			var replacedActor = new GlyssenEngine.VoiceActor.VoiceActor { Id = 1, Name = "B", Age = ActorAge.YoungAdult };
			var action = new VoiceActorEditingUndoAction(m_testProject, new[] { new VoiceActorEditUndoAction(m_testProject, replacedActor) });
			Assert.AreEqual(0, action.GroupsAffectedByLastOperation.Count());
		}

		[Test]
		public void Contructor_SingleEditingActionForAssignedActor_AffectedGroupsConsistsOfGroupAssignedToAffectedActor()
		{
			var assignedGroup = AddCharacterGroup("Moses", "John");
			var anotherGroup = AddCharacterGroup("Mary", "Ruth");
			m_testProject.CharacterGroupList.CharacterGroups.Add(anotherGroup);
			var affectedActor = new GlyssenEngine.VoiceActor.VoiceActor { Id = 1, Name = "B", Age = ActorAge.Adult };
			var replacedActor = new GlyssenEngine.VoiceActor.VoiceActor { Id = 1, Name = "B", Age = ActorAge.YoungAdult };
			m_testProject.VoiceActorList.AllActors = new List<GlyssenEngine.VoiceActor.VoiceActor> { affectedActor };
			assignedGroup.AssignVoiceActor(1);
			var action = new VoiceActorEditingUndoAction(m_testProject, new[] { new VoiceActorEditUndoAction(m_testProject, replacedActor) });
			Assert.AreEqual(assignedGroup, action.GroupsAffectedByLastOperation.Single());
		}

		[Test]
		public void Contructor_MultipleEditingActions_AffectedGroupsConsistsOfGroupsAssignedToAffectedActors()
		{
			var maleGroup = AddCharacterGroup("Moses", "John");
			m_testProject.CharacterGroupList.CharacterGroups.Add(maleGroup);
			var femaleGroup = AddCharacterGroup("Mary", "Ruth");
			m_testProject.CharacterGroupList.CharacterGroups.Add(femaleGroup);
			var childGroup = AddCharacterGroup("children");
			m_testProject.CharacterGroupList.CharacterGroups.Add(childGroup);
			var anotherGroup = AddCharacterGroup("Pharisees", "ear");
			m_testProject.CharacterGroupList.CharacterGroups.Add(anotherGroup);

			var affectedActorA = new GlyssenEngine.VoiceActor.VoiceActor { Id = 1, Name = "A", Age = ActorAge.YoungAdult };
			var replacedActorA = new GlyssenEngine.VoiceActor.VoiceActor { Id = 1, Name = "A" };
			var affectedActorB = new GlyssenEngine.VoiceActor.VoiceActor { Id = 2, Name = "B", Gender = ActorGender.Female };
			var replacedActorB = new GlyssenEngine.VoiceActor.VoiceActor { Id = 2, Name = "B"};
			var addedActor = new GlyssenEngine.VoiceActor.VoiceActor { Id = 3, Name = "C", Age = ActorAge.YoungAdult };
			var removedActor = new GlyssenEngine.VoiceActor.VoiceActor { Id = 4, Name = "D", Age = ActorAge.YoungAdult };
			var unchangedActor = new GlyssenEngine.VoiceActor.VoiceActor { Id = 5, Name = "E", Age = ActorAge.Child };
			m_testProject.VoiceActorList.AllActors = new List<GlyssenEngine.VoiceActor.VoiceActor> { affectedActorA, affectedActorB, addedActor, unchangedActor };

			maleGroup.AssignVoiceActor(1);
			femaleGroup.AssignVoiceActor(2);
			childGroup.AssignVoiceActor(5);

			var action = new VoiceActorEditingUndoAction(m_testProject, new IVoiceActorUndoAction[]
			{
				new VoiceActorEditUndoAction(m_testProject, replacedActorA),
				new VoiceActorEditUndoAction(m_testProject, replacedActorB),
				new VoiceActorAddedUndoAction(m_testProject, addedActor.Id),
				new VoiceActorDeletedUndoAction(m_testProject, removedActor)
			});
			Assert.IsTrue(action.GroupsAffectedByLastOperation.SetEquals(new [] {maleGroup, femaleGroup} ));
		}

		[Test]
		public void Description_SingleActorDeleted_DeletedActorReferencedByName()
		{
			var removedActor = new GlyssenEngine.VoiceActor.VoiceActor { Id = 4, Name = "Dominic", Age = ActorAge.YoungAdult };

			var action = new VoiceActorEditingUndoAction(m_testProject, new[]
			{
				new VoiceActorDeletedUndoAction(m_testProject, removedActor)
			});
			Assert.AreEqual("Delete voice actor Dominic", action.Description);
		}

		[Test]
		public void Description_SingleActorAdded_AddedActorReferencedByName()
		{
			m_testProject.VoiceActorList.AllActors.Add(new GlyssenEngine.VoiceActor.VoiceActor { Id = 3, Name = "Chuck", Age = ActorAge.YoungAdult });

			var action = new VoiceActorEditingUndoAction(m_testProject, new[]
			{
				new VoiceActorAddedUndoAction(m_testProject, 3)
			});
			Assert.AreEqual("Add voice actor Chuck", action.Description);
		}

		[Test]
		public void Description_ChangedNameOfSingleActor_NameChangeIndicated()
		{
			m_testProject.VoiceActorList.AllActors.Add(new GlyssenEngine.VoiceActor.VoiceActor { Id = 1, Name = "Icon" });
			var oldName = new GlyssenEngine.VoiceActor.VoiceActor { Id = 1, Name = "Prince" };

			var action = new VoiceActorEditingUndoAction(m_testProject, new[]
			{
				new VoiceActorEditUndoAction(m_testProject, oldName)
			});
			Assert.AreEqual("Change name of voice actor from Prince to Icon", action.Description);
		}

		[Test]
		public void Description_ChangedNameAndDetailsOfSingleActor_NameChangeIndicated()
		{
			m_testProject.VoiceActorList.AllActors.Add(
				new GlyssenEngine.VoiceActor.VoiceActor { Id = 1, Name = "Aimee", Gender = ActorGender.Female, Age = ActorAge.YoungAdult });
			var replacedActor = new GlyssenEngine.VoiceActor.VoiceActor { Id = 1, Name = "Amy", Gender = ActorGender.Female, Age = ActorAge.Child};

			var action = new VoiceActorEditingUndoAction(m_testProject, new[]
			{
				new VoiceActorEditUndoAction(m_testProject, replacedActor)
			});
			Assert.AreEqual("Edit voice actor Aimee", action.Description);
		}
	}
}
