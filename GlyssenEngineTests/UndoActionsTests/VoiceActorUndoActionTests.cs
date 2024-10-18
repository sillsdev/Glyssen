using System;
using System.Linq;
using GlyssenEngine;
using GlyssenEngine.Character;
using GlyssenEngine.UndoActions;
using GlyssenEngine.Casting;
using NUnit.Framework;

namespace GlyssenEngineTests.UndoActionsTests
{
	[TestFixture]
	class VoiceActorUndoActionTests
	{
		private Project m_testProject;

		[OneTimeSetUp]
		public void OneTimeSetUp()
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

		//-------------------------------------------------------------------------------------------------------------------
		#region VoiceActorEditUndoAction
		#region Constructor Tests
		[Test]
		public void VoiceActorEditUndoAction_Constructor_NullActor_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() =>
			{
				new VoiceActorEditUndoAction(m_testProject, null);
			});
		}

		[Test]
		public void VoiceActorEditUndoAction_Constructor_PreviousActorInformationIdDoesNotCorrespondToAnyCurrentActor_ThrowsArgumentException()
		{
			Assert.Throws<ArgumentException>(() =>
			{
				new VoiceActorEditUndoAction(m_testProject, new VoiceActor { Id = 1, Name = "B", Age = ActorAge.Adult });
			});
		}

		[Test]
		public void VoiceActorEditUndoAction_Constructor_EditedActor_AffectedActorEqualsReplacedActor_NotARename()
		{
			m_testProject.VoiceActorList.AllActors.Add(new VoiceActor { Id = 1, Name = "B", Age = ActorAge.Adult });
			var action = new VoiceActorEditUndoAction(m_testProject,
				new VoiceActor { Id = 1, Name = "B", Age = ActorAge.YoungAdult });
			Assert.That(action.PreviousNameOfActor, Is.EqualTo(action.ActorAffected));
			Assert.That(action.JustChangedName, Is.False);
		}
		#endregion

		#region Description Tests
		[Test]
		public void VoiceActorEditUndoAction_Description_ChangedNameOfActor_NameChangeIndicated()
		{
			m_testProject.VoiceActorList.AllActors.Add(
				new VoiceActor { Id = 1, Name = "Icon" });
			var action = new VoiceActorEditUndoAction(m_testProject,
				new VoiceActor { Id = 1, Name = "Prince" });
			Assert.That(action.Description, Is.EqualTo("Change name of voice actor from Prince to Icon"));
		}

		[Test]
		public void VoiceActorEditUndoAction_Description_ChangedNameAndDetailsOfActor_EditingOfActorRefersToNewName()
		{
			m_testProject.VoiceActorList.AllActors.Add(
				new VoiceActor { Id = 1, Name = "Aimee", Gender = ActorGender.Female, Age = ActorAge.YoungAdult });
			var action = new VoiceActorEditUndoAction(m_testProject,
				new VoiceActor { Id = 1, Name = "Amy", Gender = ActorGender.Female, Age = ActorAge.Child });
			Assert.That(action.Description, Is.EqualTo("Edit voice actor Aimee"));
		}

		[Test]
		public void VoiceActorEditUndoAction_Description_ChangedDetailsOfActor_EditingOfActorIndicated()
		{
			m_testProject.VoiceActorList.AllActors.Add(
				new VoiceActor { Id = 1, Name = "Arnold", Gender = ActorGender.Male, Age = ActorAge.YoungAdult });
			var action = new VoiceActorEditUndoAction(m_testProject,
				new VoiceActor { Id = 1, Name = "Arnold", Gender = ActorGender.Female, Age = ActorAge.Child });
			Assert.That(action.Description, Is.EqualTo("Edit voice actor Arnold"));
		}
		#endregion

		#region Undo Tests
		[Test]
		public void VoiceActorEditUndoAction_Undo_ChangedNameOfActor_NameRestored()
		{
			m_testProject.VoiceActorList.AllActors.Add(new VoiceActor { Id = 1, Name = "Icon" });
			var action = new VoiceActorEditUndoAction(m_testProject, new VoiceActor { Id = 1, Name = "Prince" });
			Assert.That(action.Undo(), Is.True);
			Assert.That(m_testProject.VoiceActorList.GetVoiceActorById(1).Name, Is.EqualTo("Prince"));
		}

		[Test]
		public void VoiceActorEditUndoAction_Undo_ChangedNameAndDetailsOfActor_NameAndDetailsRestored()
		{
			m_testProject.VoiceActorList.AllActors.Add(
				new VoiceActor { Id = 1, Name = "Aimee", Gender = ActorGender.Female, Age = ActorAge.YoungAdult });
			var replacedActor = new VoiceActor { Id = 1, Name = "Amy", Gender = ActorGender.Female, Age = ActorAge.Child };
			var action = new VoiceActorEditUndoAction(m_testProject, replacedActor);

			Assert.That(action.Undo(), Is.True);
			var restoredActor = m_testProject.VoiceActorList.GetVoiceActorById(1);
			Assert.That(restoredActor.Name, Is.EqualTo("Amy"));
			Assert.That(ActorAge.Child, Is.EqualTo(restoredActor.Age));
		}

		[Test]
		public void VoiceActorEditUndoAction_Undo_ChangedNameOfActorAndSubsequentlyAddedActorWithPreviousName_ReturnsFalse()
		{
			m_testProject.VoiceActorList.AllActors.Add(
				new VoiceActor { Id = 1, Name = "Aimee", Gender = ActorGender.Female, Age = ActorAge.YoungAdult });
			var replacedActor = new VoiceActor { Id = 1, Name = "Amy", Gender = ActorGender.Female, Age = ActorAge.Child };
			var action = new VoiceActorEditUndoAction(m_testProject, replacedActor);
			m_testProject.VoiceActorList.AllActors.Add(
				new VoiceActor { Id = 2, Name = "Amy", Gender = ActorGender.Female, Age = ActorAge.Elder, VoiceQuality = VoiceQuality.Dramatic });

			Assert.That(action.Undo(), Is.False);
			Assert.That(m_testProject.VoiceActorList.GetVoiceActorById(1).Name, Is.EqualTo("Aimee"));
			Assert.That(m_testProject.VoiceActorList.GetVoiceActorById(2).Name, Is.EqualTo("Amy"));
		}
		#endregion

		#region Redo Tests
		[Test]
		public void VoiceActorEditUndoAction_Redo_ChangedNameOfActor_NameSetBackToNewValue()
		{
			m_testProject.VoiceActorList.AllActors.Add(new VoiceActor { Id = 1, Name = "Icon" });
			var action = new VoiceActorEditUndoAction(m_testProject, new VoiceActor { Id = 1, Name = "Prince" });
			action.Undo();

			Assert.That(action.Redo(), Is.True);
			Assert.That(m_testProject.VoiceActorList.GetVoiceActorById(1).Name, Is.EqualTo("Icon"));
		}

		[Test]
		public void VoiceActorEditUndoAction_Redo_AddedActorWithNewNameAfterUndo_ReturnsFalse()
		{
			m_testProject.VoiceActorList.AllActors.Add(
				new VoiceActor { Id = 1, Name = "Aimee", Gender = ActorGender.Female, Age = ActorAge.YoungAdult });
			var replacedActor = new VoiceActor { Id = 1, Name = "Amy", Gender = ActorGender.Female, Age = ActorAge.Child };
			var action = new VoiceActorEditUndoAction(m_testProject, replacedActor);
			action.Undo();
			m_testProject.VoiceActorList.AllActors.Add(
				new VoiceActor { Id = 2, Name = "Aimee", Gender = ActorGender.Female, Age = ActorAge.Adult, VoiceQuality = VoiceQuality.Suspicious});

			Assert.That(action.Redo(), Is.False);
			Assert.That(m_testProject.VoiceActorList.GetVoiceActorById(1).Name, Is.EqualTo("Amy"));
			Assert.That(m_testProject.VoiceActorList.GetVoiceActorById(2).Name, Is.EqualTo("Aimee"));
		}

		[Test]
		public void VoiceActorEditUndoAction_Redo_ChangedNameAndDetailsOfActor_NameAndDetailsRestored()
		{
			m_testProject.VoiceActorList.AllActors.Add(
				new VoiceActor { Id = 1, Name = "Aimee", Gender = ActorGender.Female, Age = ActorAge.YoungAdult });
			var replacedActor = new VoiceActor { Id = 1, Name = "Amy", Gender = ActorGender.Female, Age = ActorAge.Child };
			var action = new VoiceActorEditUndoAction(m_testProject, replacedActor);
			action.Undo();

			Assert.That(action.Redo(), Is.True);
			var restoredActor = m_testProject.VoiceActorList.GetVoiceActorById(1);
			Assert.That(restoredActor.Name, Is.EqualTo("Aimee"));
			Assert.That(ActorAge.YoungAdult, Is.EqualTo(restoredActor.Age));
		}
		#endregion
		#endregion

		//-------------------------------------------------------------------------------------------------------------------
		#region VoiceActorAddedUndoAction
		#region Constructor Tests
		[Test]
		public void VoiceActorAddedUndoAction_Constructor_ActorNotInProject_ThrowsArgumentException()
		{
			Assert.Throws<ArgumentException>(() =>
			{
				new VoiceActorAddedUndoAction(m_testProject, 1);
			});
		}

		[Test]
		public void VoiceActorAddedUndoAction_Constructor_ActorInProject_NotARename()
		{
			m_testProject.VoiceActorList.AllActors.Add(new VoiceActor { Id = 3, Name = "Chuck", Age = ActorAge.YoungAdult });

			var action = new VoiceActorAddedUndoAction(m_testProject, 3);
			Assert.That(action.ActorAffected, Is.EqualTo("Chuck"));
			Assert.That(action.JustChangedName, Is.False);
		}
		#endregion

		#region Description Tests
		[Test]
		public void VoiceActorAddedUndoAction_Description_Normal_AddedActorReferencedByName()
		{
			m_testProject.VoiceActorList.AllActors.Add(new VoiceActor { Id = 3, Name = "Chuck", Age = ActorAge.YoungAdult });

			var action = new VoiceActorAddedUndoAction(m_testProject, 3);
			Assert.That(action.Description, Is.EqualTo("Add voice actor Chuck"));
		}

		[Test]
		public void VoiceActorAddedUndoAction_Description_ActorSubsequentlyDeleted_AddedActorReferencedByName()
		{
			var addedActor = new VoiceActor {Id = 3, Name = "Chuck", Age = ActorAge.YoungAdult};
			m_testProject.VoiceActorList.AllActors.Add(addedActor);
			var action = new VoiceActorAddedUndoAction(m_testProject, 3);
			m_testProject.VoiceActorList.AllActors.Remove(addedActor);
			Assert.That(action.Description, Is.EqualTo("Add voice actor Chuck"));
		}
		#endregion

		#region Undo Tests
		[Test]
		public void VoiceActorAddedUndoAction_Undo_Normal_ActorDeleted()
		{
			m_testProject.VoiceActorList.AllActors.Add(
				new VoiceActor { Id = 3, Name = "Chuck", Age = ActorAge.YoungAdult });
			var action = new VoiceActorAddedUndoAction(m_testProject, 3);

			Assert.That(action.Undo(), Is.True);
			Assert.That(m_testProject.VoiceActorList.AllActors.Any(a => a.Name == "Chuck" || a.Id == 3), Is.False);
		}

		[Test]
		public void VoiceActorAddedUndoAction_Undo_ActorSubsequentlyDeleted_ReturnsFalse()
		{
			m_testProject.VoiceActorList.AllActors.Add(
				new VoiceActor { Id = 3, Name = "Chuck", Age = ActorAge.YoungAdult });
			var action = new VoiceActorAddedUndoAction(m_testProject, 3);
			m_testProject.VoiceActorList.AllActors.Clear();
			Assert.That(m_testProject.VoiceActorList.AllActors.Count, Is.EqualTo(0));

			Assert.That(action.Undo(), Is.False);
			Assert.That(m_testProject.VoiceActorList.AllActors.Count, Is.EqualTo(0));
		}

		[Test]
		public void VoiceActorAddedUndoAction_Undo_ActorSubsequentlyModified_ActorDeleted()
		{
			var addedActor = new VoiceActor { Id = 3, Name = "Chuck", Age = ActorAge.YoungAdult };
			m_testProject.VoiceActorList.AllActors.Add(addedActor);
			var action = new VoiceActorAddedUndoAction(m_testProject, 3);
			addedActor.Name = "Charlie";
			addedActor.VoiceQuality = VoiceQuality.Authoritative;

			Assert.That(action.Undo(), Is.True);
			Assert.That(m_testProject.VoiceActorList.AllActors.Count, Is.EqualTo(0));
		}

		[Test]
		public void VoiceActorAddedUndoAction_Undo_ActorSubsequentlyAssignedToGroup_ReturnsFalse()
		{
			var group = new CharacterGroup(m_testProject);
			group.CharacterIds = new CharacterIdHashSet(new[] { "Moses" });
			m_testProject.CharacterGroupList.CharacterGroups.Add(group);

			var addedActor = new VoiceActor { Id = 3, Name = "Chuck", Age = ActorAge.YoungAdult };
			m_testProject.VoiceActorList.AllActors.Add(addedActor);
			var action = new VoiceActorAddedUndoAction(m_testProject, 3);
			group.AssignVoiceActor(addedActor.Id);

			Assert.That(action.Undo(), Is.False);
			Assert.That(m_testProject.VoiceActorList.AllActors.Single().Id, Is.EqualTo(3));
		}
		#endregion

		#region Redo Tests
		[Test]
		public void VoiceActorAddedUndoAction_Redo_Normal_ActorReinstated()
		{
			m_testProject.VoiceActorList.AllActors.Add(
				new VoiceActor { Id = 3, Name = "Chuck", Age = ActorAge.YoungAdult });
			var action = new VoiceActorAddedUndoAction(m_testProject, 3);
			action.Undo();

			Assert.That(action.Redo(), Is.True);
			var reAddedActor = m_testProject.VoiceActorList.GetVoiceActorById(3);
			Assert.That(reAddedActor.Name, Is.EqualTo("Chuck"));
			Assert.That(ActorAge.YoungAdult, Is.EqualTo(reAddedActor.Age));
		}

		[Test]
		public void VoiceActorAddedUndoAction_Redo_AddedDifferentActorWithSameIdAfterUndo_ActorReinstatedWithUniqueId()
		{
			m_testProject.VoiceActorList.AllActors.Add(
				new VoiceActor { Id = 3, Name = "Chuck", Age = ActorAge.YoungAdult });
			var action = new VoiceActorAddedUndoAction(m_testProject, 3);
			m_testProject.VoiceActorList.AllActors.Clear();
			Assert.That(m_testProject.VoiceActorList.AllActors.Count, Is.EqualTo(0));
			action.Undo();
			m_testProject.VoiceActorList.AllActors.Add(
				new VoiceActor { Id = 3, Name = "Maggie", Gender = ActorGender.Female});

			Assert.That(action.Redo(), Is.True);
			Assert.That(m_testProject.VoiceActorList.AllActors.Count, Is.EqualTo(2));
			var reAddedActor = m_testProject.VoiceActorList.AllActors.Single(a => a.Id != 3);
			Assert.That(reAddedActor.Name, Is.EqualTo("Chuck"));
			Assert.That(ActorAge.YoungAdult, Is.EqualTo(reAddedActor.Age));
		}

		[Test]
		public void VoiceActorAddedUndoAction_Redo_AddedActorWithSameNameAfterUndo_ReturnsFalse()
		{
			m_testProject.VoiceActorList.AllActors.Add(
				new VoiceActor { Id = 3, Name = "Chuck", Age = ActorAge.YoungAdult });
			var action = new VoiceActorAddedUndoAction(m_testProject, 3);
			m_testProject.VoiceActorList.AllActors.Clear();
			Assert.That(m_testProject.VoiceActorList.AllActors.Count, Is.EqualTo(0));
			action.Undo();
			m_testProject.VoiceActorList.AllActors.Add(
				new VoiceActor { Id = 40, Name = "Chuck", Age = ActorAge.Elder});

			Assert.That(action.Redo(), Is.False);
			Assert.That(ActorAge.Elder, Is.EqualTo(m_testProject.VoiceActorList.AllActors.Single().Age));
		}
		#endregion

		#endregion

		//-------------------------------------------------------------------------------------------------------------------
		#region VoiceActorDeletedUndoAction
		#region Constructor Tests
		[Test]
		public void VoiceActorDeletedUndoAction_Constructor_NullActor_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() =>
			{
				new VoiceActorDeletedUndoAction(m_testProject, null);
			});
		}

		[Test]
		public void VoiceActorDeletedUndoAction_Constructor_Normal_NotARename()
		{
			var action = new VoiceActorDeletedUndoAction(m_testProject,
				new VoiceActor { Id = 4, Name = "Dominic", Age = ActorAge.YoungAdult });
			Assert.That(action.JustChangedName, Is.False);
			Assert.That(action.ActorAffected, Is.Null);
			Assert.That(action.DeletedActorName, Is.EqualTo("Dominic"));
		}
		#endregion

		#region Description Tests
		[Test]
		public void VoiceActorDeletedUndoAction_Description_Normal_DeletedActorReferencedByName()
		{
			var action = new VoiceActorDeletedUndoAction(m_testProject,
				new VoiceActor { Id = 4, Name = "Dominic", Age = ActorAge.YoungAdult });
			Assert.That(action.Description, Is.EqualTo("Delete voice actor Dominic"));
		}
		#endregion

		#region Undo Tests
		[Test]
		public void VoiceActorDeletedUndoAction_Undo_Normal_ActorRestored()
		{
			var removedActor = new VoiceActor { Id = 4, Name = "Dominic", Age = ActorAge.YoungAdult };

			var action = new VoiceActorDeletedUndoAction(m_testProject, removedActor);
			Assert.That(action.Undo(), Is.True);
			var restoredActor = m_testProject.VoiceActorList.GetVoiceActorById(4);
			Assert.That(restoredActor, Is.Not.Null);
			// Equals is just defined as having the same ID, so we need to check name and details separately.
			Assert.That(restoredActor.Name, Is.EqualTo(removedActor.Name));
			Assert.That(restoredActor.IsInterchangeableWith(removedActor), Is.True);
		}

		[Test]
		public void VoiceActorDeletedUndoAction_Undo_ActorAssignedToGroup_ActorAndAssignmentRestored()
		{
			var assignedGroup = AddCharacterGroup("Barnabas", "Caleb", "Hosea");
			var removedActor = new VoiceActor { Id = 4, Name = "Dominic", Age = ActorAge.YoungAdult };

			var action = new VoiceActorDeletedUndoAction(m_testProject, removedActor, assignedGroup);
			Assert.That(action.Undo(), Is.True);
			var restoredActor = m_testProject.VoiceActorList.GetVoiceActorById(4);
			Assert.That(restoredActor, Is.Not.Null);
			// Equals is just defined as having the same ID, so we need to check name and details separately.
			Assert.That(restoredActor.Name, Is.EqualTo(removedActor.Name));
			Assert.That(restoredActor.IsInterchangeableWith(removedActor), Is.True);
			Assert.That(assignedGroup.VoiceActorId, Is.EqualTo(4));
		}

		[Test]
		public void VoiceActorDeletedUndoAction_Undo_ActorAssignedToGroupWhichIsSubsequentlyRegeneratedWithSameCharacters_ActorAndAssignmentRestored()
		{
			var assignedGroup = AddCharacterGroup("Barnabas", "Caleb", "Hosea");
			var removedActor = new VoiceActor { Id = 4, Name = "Dominic", Age = ActorAge.YoungAdult };

			var action = new VoiceActorDeletedUndoAction(m_testProject, removedActor, assignedGroup);
			m_testProject.CharacterGroupList.CharacterGroups.Remove(assignedGroup);
			var newGroup = AddCharacterGroup("Barnabas", "Caleb", "Hosea");

			Assert.That(action.Undo(), Is.True);
			var restoredActor = m_testProject.VoiceActorList.GetVoiceActorById(4);
			Assert.That(restoredActor, Is.Not.Null);
			// Equals is just defined as having the same ID, so we need to check name and details separately.
			Assert.That(restoredActor.Name, Is.EqualTo(removedActor.Name));
			Assert.That(restoredActor.IsInterchangeableWith(removedActor), Is.True);
			Assert.That(newGroup.VoiceActorId, Is.EqualTo(4));
		}

		[Test]
		public void VoiceActorDeletedUndoAction_Undo_ActorAssignedToGroupWhichIsSubsequentlyDeleted_ReturnsFalse()
		{
			var assignedGroup = AddCharacterGroup("Barnabas", "Caleb", "Hosea");
			var removedActor = new VoiceActor { Id = 4, Name = "Dominic", Age = ActorAge.YoungAdult };

			var action = new VoiceActorDeletedUndoAction(m_testProject, removedActor, assignedGroup);
			m_testProject.CharacterGroupList.CharacterGroups.Remove(assignedGroup);
			Assert.That(m_testProject.CharacterGroupList.CharacterGroups.Count, Is.EqualTo(0));
			AddCharacterGroup("Caleb", "Hosea");
			AddCharacterGroup("Barnabas", "Joshua");
			AddCharacterGroup("Martha");
			Assert.That(m_testProject.CharacterGroupList.CharacterGroups.Count, Is.EqualTo(3));

			Assert.That(action.Undo(), Is.False);
			Assert.That(m_testProject.VoiceActorList.GetVoiceActorById(4), Is.Null);
			Assert.That(m_testProject.VoiceActorList.AllActors.SingleOrDefault(a => a.Name == "Dominic"), Is.Null);
			Assert.That(m_testProject.CharacterGroupList.CountVoiceActorsAssigned(), Is.EqualTo(0));
		}

		[Test]
		public void VoiceActorDeletedUndoAction_Undo_AnotherActorAddedWithSameId_ActorRestoredWithUniqueId()
		{
			var removedActor = new VoiceActor { Id = 4, Name = "Dominic", Age = ActorAge.YoungAdult };

			var action = new VoiceActorDeletedUndoAction(m_testProject, removedActor);

			var reAddedActor = new VoiceActor { Id = 4, Name = "Marshall", Age = ActorAge.Elder };
			m_testProject.VoiceActorList.AllActors.Add(reAddedActor);

			Assert.That(action.Undo(), Is.True);
			var restoredActor = m_testProject.VoiceActorList.AllActors.Single(a => a.Name == "Dominic");
			Assert.That(restoredActor, Is.Not.Null);
			// Equals is just defined as having the same ID, so we need to check name and details separately.
			Assert.That(restoredActor.Name, Is.EqualTo(removedActor.Name));
			Assert.That(restoredActor.IsInterchangeableWith(removedActor), Is.True);
			Assert.That(m_testProject.VoiceActorList.AllActors.Count, Is.EqualTo(m_testProject.VoiceActorList.AllActors.Select(a => a.Id).Distinct().Count()));
		}

		[Test]
		public void VoiceActorDeletedUndoAction_Undo_AnotherActorAddedWithSameName_ReturnsFalse()
		{
			var removedActor = new VoiceActor { Id = 4, Name = "Dominic", Age = ActorAge.YoungAdult };

			var action = new VoiceActorDeletedUndoAction(m_testProject, removedActor);

			m_testProject.VoiceActorList.AllActors.Add(new VoiceActor { Id = 2, Name = "Dominic", Age = ActorAge.Child });

			Assert.That(action.Undo(), Is.False);
		}
		#endregion

		#region Redo Tests
		[Test]
		public void VoiceActorDeletedUndoAction_Redo_Normal_ActorReDeleted()
		{
			var removedActor = new VoiceActor { Id = 4, Name = "Dominic", Age = ActorAge.YoungAdult };

			var action = new VoiceActorDeletedUndoAction(m_testProject, removedActor);
			action.Undo();
			Assert.That(action.Redo(), Is.True);

			Assert.That(m_testProject.VoiceActorList.GetVoiceActorById(4), Is.Null);
			Assert.That(m_testProject.VoiceActorList.AllActors.SingleOrDefault(a => a.Name == "Dominic"), Is.Null);
		}

		[Test]
		public void VoiceActorDeletedUndoAction_Redo_ActorAssignedToGroup_ActorReDeletedAndAssignmentRemoved()
		{
			var assignedGroup = AddCharacterGroup("Barnabas", "Caleb", "Hosea");
			var removedActor = new VoiceActor { Id = 4, Name = "Dominic", Age = ActorAge.YoungAdult };

			var action = new VoiceActorDeletedUndoAction(m_testProject, removedActor, assignedGroup);
			action.Undo();
			Assert.That(action.Redo(), Is.True);

			Assert.That(m_testProject.VoiceActorList.GetVoiceActorById(4), Is.Null);
			Assert.That(m_testProject.VoiceActorList.AllActors.SingleOrDefault(a => a.Name == "Dominic"), Is.Null);
			Assert.That(m_testProject.CharacterGroupList.CountVoiceActorsAssigned(), Is.EqualTo(0));
		}

		[Test]
		public void VoiceActorDeletedUndoAction_Redo_ActorAssignedToGroupWhichIsSubsequentlyUnassigned_ActorReDeleted()
		{
			var assignedGroup = AddCharacterGroup("Barnabas", "Caleb", "Hosea");
			var removedActor = new VoiceActor { Id = 4, Name = "Dominic", Age = ActorAge.YoungAdult };

			var action = new VoiceActorDeletedUndoAction(m_testProject, removedActor, assignedGroup);

			action.Undo();
			Assert.That(m_testProject.CharacterGroupList.CountVoiceActorsAssigned(), Is.EqualTo(1));
			Assert.That(assignedGroup.VoiceActorId, Is.EqualTo(4));
			assignedGroup.RemoveVoiceActor();
			Assert.That(action.Redo(), Is.True);

			Assert.That(m_testProject.VoiceActorList.GetVoiceActorById(4), Is.Null);
			Assert.That(m_testProject.VoiceActorList.AllActors.SingleOrDefault(a => a.Name == "Dominic"), Is.Null);
			Assert.That(m_testProject.CharacterGroupList.CountVoiceActorsAssigned(), Is.EqualTo(0));
		}

		[Test]
		public void VoiceActorDeletedUndoAction_Redo_ActorAssignedToGroupButAssignedToDifferentGroupFollowingUndo_ReturnsFalse()
		{
			var assignedGroup = AddCharacterGroup("Barnabas", "Caleb", "Hosea");
			var differentGroup = AddCharacterGroup("Thomas", "Jonah");
			var removedActor = new VoiceActor { Id = 4, Name = "Dominic", Age = ActorAge.YoungAdult };

			var action = new VoiceActorDeletedUndoAction(m_testProject, removedActor, assignedGroup);
			action.Undo();
			Assert.That(m_testProject.CharacterGroupList.CountVoiceActorsAssigned(), Is.EqualTo(1));
			Assert.That(assignedGroup.VoiceActorId, Is.EqualTo(4));
			assignedGroup.RemoveVoiceActor();
			differentGroup.AssignVoiceActor(m_testProject.VoiceActorList.GetVoiceActorById(4).Id);

			Assert.That(action.Redo(), Is.False);

			var restoredActor = m_testProject.VoiceActorList.GetVoiceActorById(4);
			// Equals is just defined as having the same ID, so we need to check name and details separately.
			Assert.That(restoredActor.Name, Is.EqualTo(removedActor.Name));
			Assert.That(restoredActor.IsInterchangeableWith(removedActor), Is.True);
			Assert.That(differentGroup.VoiceActorId, Is.EqualTo(4));
		}

		[Test]
		public void VoiceActorDeletedUndoAction_Redo_ActorAssignedToGroupWhichIsSubsequentlyDeleted_ActorReDeleted()
		{
			var assignedGroup = AddCharacterGroup("Barnabas", "Caleb", "Hosea");
			AddCharacterGroup("Adam", "Lot");
			var removedActor = new VoiceActor { Id = 4, Name = "Dominic", Age = ActorAge.YoungAdult };

			var action = new VoiceActorDeletedUndoAction(m_testProject, removedActor, assignedGroup);

			action.Undo();
			m_testProject.CharacterGroupList.CharacterGroups.Remove(assignedGroup);

			Assert.That(action.Redo(), Is.True);

			Assert.That(m_testProject.VoiceActorList.GetVoiceActorById(4), Is.Null);
			Assert.That(m_testProject.VoiceActorList.AllActors.SingleOrDefault(a => a.Name == "Dominic"), Is.Null);
			Assert.That(m_testProject.CharacterGroupList.CountVoiceActorsAssigned(), Is.EqualTo(0));
		}

		[Test]
		public void VoiceActorDeletedUndoAction_Redo_ActorSubsequentlyDeleted_ReturnsTrueButNoChange()
		{
			var removedActor = new VoiceActor { Id = 4, Name = "Dominic", Age = ActorAge.YoungAdult };
			var action = new VoiceActorDeletedUndoAction(m_testProject, removedActor);
			action.Undo();

			var restoredActor = m_testProject.VoiceActorList.AllActors.Single(a => a.Name == "Dominic");
			Assert.That(restoredActor, Is.Not.Null);
			m_testProject.VoiceActorList.AllActors.Remove(restoredActor);

			Assert.That(action.Redo(), Is.True);

			Assert.That(m_testProject.VoiceActorList.GetVoiceActorById(4), Is.Null);
			Assert.That(m_testProject.VoiceActorList.AllActors.SingleOrDefault(a => a.Name == "Dominic"), Is.Null);
		}
		#endregion

		#endregion

	}
}
