using System;
using System.Linq;
using Glyssen;
using Glyssen.Character;
using Glyssen.Dialogs;
using Glyssen.VoiceActor;
using NUnit.Framework;

namespace GlyssenTests.Dialogs
{
	[TestFixture]
	class VoiceActorUndoActionTests
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
			m_testProject.VoiceActorList.Actors.Clear();
		}

		private CharacterGroup AddCharacterGroup(int groupNumber, params string[] characterIds)
		{
			var group = new CharacterGroup(m_testProject, groupNumber);
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
				new VoiceActorEditUndoAction(m_testProject, new Glyssen.VoiceActor.VoiceActor { Id = 1, Name = "B", Age = ActorAge.Adult });
			});
		}

		[Test]
		public void VoiceActorEditUndoAction_Constructor_EditedActor_AffectedActorEqualsReplacedActor_NotARename()
		{
			m_testProject.VoiceActorList.Actors.Add(new Glyssen.VoiceActor.VoiceActor { Id = 1, Name = "B", Age = ActorAge.Adult });
			var action = new VoiceActorEditUndoAction(m_testProject,
				new Glyssen.VoiceActor.VoiceActor { Id = 1, Name = "B", Age = ActorAge.YoungAdult });
			Assert.AreEqual(action.PreviousNameOfActor, action.ActorAffected);
			Assert.IsFalse(action.JustChangedName);
		}
		#endregion

		#region Description Tests
		[Test]
		public void VoiceActorEditUndoAction_Description_ChangedNameOfActor_NameChangeIndicated()
		{
			m_testProject.VoiceActorList.Actors.Add(
				new Glyssen.VoiceActor.VoiceActor { Id = 1, Name = "Icon" });
			var action = new VoiceActorEditUndoAction(m_testProject,
				new Glyssen.VoiceActor.VoiceActor { Id = 1, Name = "Prince" });
			Assert.AreEqual("Change name of voice actor from Prince to Icon", action.Description);
		}

		[Test]
		public void VoiceActorEditUndoAction_Description_ChangedNameAndDetailsOfActor_EditingOfActorRefersToNewName()
		{
			m_testProject.VoiceActorList.Actors.Add(
				new Glyssen.VoiceActor.VoiceActor { Id = 1, Name = "Aimee", Gender = ActorGender.Female, Age = ActorAge.YoungAdult });
			var action = new VoiceActorEditUndoAction(m_testProject,
				new Glyssen.VoiceActor.VoiceActor { Id = 1, Name = "Amy", Gender = ActorGender.Female, Age = ActorAge.Child });
			Assert.AreEqual("Edit voice actor Aimee", action.Description);
		}

		[Test]
		public void VoiceActorEditUndoAction_Description_ChangedDetailsOfActor_EditingOfActorIndicated()
		{
			m_testProject.VoiceActorList.Actors.Add(
				new Glyssen.VoiceActor.VoiceActor { Id = 1, Name = "Arnold", Gender = ActorGender.Male, Age = ActorAge.YoungAdult });
			var action = new VoiceActorEditUndoAction(m_testProject,
				new Glyssen.VoiceActor.VoiceActor { Id = 1, Name = "Arnold", Gender = ActorGender.Female, Age = ActorAge.Child });
			Assert.AreEqual("Edit voice actor Arnold", action.Description);
		}
		#endregion

		#region Undo Tests
		[Test]
		public void VoiceActorEditUndoAction_Undo_ChangedNameOfActor_NameRestored()
		{
			m_testProject.VoiceActorList.Actors.Add(new Glyssen.VoiceActor.VoiceActor { Id = 1, Name = "Icon" });
			var action = new VoiceActorEditUndoAction(m_testProject, new Glyssen.VoiceActor.VoiceActor { Id = 1, Name = "Prince" });
			Assert.IsTrue(action.Undo());
			Assert.AreEqual("Prince", m_testProject.VoiceActorList.GetVoiceActorById(1).Name);
		}

		[Test]
		public void VoiceActorEditUndoAction_Undo_ChangedNameAndDetailsOfActor_NameAndDetailsRestored()
		{
			m_testProject.VoiceActorList.Actors.Add(
				new Glyssen.VoiceActor.VoiceActor { Id = 1, Name = "Aimee", Gender = ActorGender.Female, Age = ActorAge.YoungAdult });
			var replacedActor = new Glyssen.VoiceActor.VoiceActor { Id = 1, Name = "Amy", Gender = ActorGender.Female, Age = ActorAge.Child };
			var action = new VoiceActorEditUndoAction(m_testProject, replacedActor);

			Assert.IsTrue(action.Undo());
			var restoredActor = m_testProject.VoiceActorList.GetVoiceActorById(1);
			Assert.AreEqual("Amy", restoredActor.Name);
			Assert.AreEqual(ActorAge.Child, restoredActor.Age);
		}

		[Test]
		public void VoiceActorEditUndoAction_Undo_ChangedNameOfActorAndSubsequentlyAddedActorWithPreviousName_ReturnsFalse()
		{
			m_testProject.VoiceActorList.Actors.Add(
				new Glyssen.VoiceActor.VoiceActor { Id = 1, Name = "Aimee", Gender = ActorGender.Female, Age = ActorAge.YoungAdult });
			var replacedActor = new Glyssen.VoiceActor.VoiceActor { Id = 1, Name = "Amy", Gender = ActorGender.Female, Age = ActorAge.Child };
			var action = new VoiceActorEditUndoAction(m_testProject, replacedActor);
			m_testProject.VoiceActorList.Actors.Add(
				new Glyssen.VoiceActor.VoiceActor { Id = 2, Name = "Amy", Gender = ActorGender.Female, Age = ActorAge.Elder, VoiceQuality = VoiceQuality.Dramatic });

			Assert.IsFalse(action.Undo());
			Assert.AreEqual("Aimee", m_testProject.VoiceActorList.GetVoiceActorById(1).Name);
			Assert.AreEqual("Amy", m_testProject.VoiceActorList.GetVoiceActorById(2).Name);
		}
		#endregion

		#region Redo Tests
		[Test]
		public void VoiceActorEditUndoAction_Redo_ChangedNameOfActor_NameSetBackToNewValue()
		{
			m_testProject.VoiceActorList.Actors.Add(new Glyssen.VoiceActor.VoiceActor { Id = 1, Name = "Icon" });
			var action = new VoiceActorEditUndoAction(m_testProject, new Glyssen.VoiceActor.VoiceActor { Id = 1, Name = "Prince" });
			action.Undo();

			Assert.IsTrue(action.Redo());
			Assert.AreEqual("Icon", m_testProject.VoiceActorList.GetVoiceActorById(1).Name);
		}

		[Test]
		public void VoiceActorEditUndoAction_Redo_AddedActorWithNewNameAfterUndo_ReturnsFalse()
		{
			m_testProject.VoiceActorList.Actors.Add(
				new Glyssen.VoiceActor.VoiceActor { Id = 1, Name = "Aimee", Gender = ActorGender.Female, Age = ActorAge.YoungAdult });
			var replacedActor = new Glyssen.VoiceActor.VoiceActor { Id = 1, Name = "Amy", Gender = ActorGender.Female, Age = ActorAge.Child };
			var action = new VoiceActorEditUndoAction(m_testProject, replacedActor);
			action.Undo();
			m_testProject.VoiceActorList.Actors.Add(
				new Glyssen.VoiceActor.VoiceActor { Id = 2, Name = "Aimee", Gender = ActorGender.Female, Age = ActorAge.Adult, VoiceQuality = VoiceQuality.Suspicious});

			Assert.IsFalse(action.Redo());
			Assert.AreEqual("Amy", m_testProject.VoiceActorList.GetVoiceActorById(1).Name);
			Assert.AreEqual("Aimee", m_testProject.VoiceActorList.GetVoiceActorById(2).Name);
		}

		[Test]
		public void VoiceActorEditUndoAction_Redo_ChangedNameAndDetailsOfActor_NameAndDetailsRestored()
		{
			m_testProject.VoiceActorList.Actors.Add(
				new Glyssen.VoiceActor.VoiceActor { Id = 1, Name = "Aimee", Gender = ActorGender.Female, Age = ActorAge.YoungAdult });
			var replacedActor = new Glyssen.VoiceActor.VoiceActor { Id = 1, Name = "Amy", Gender = ActorGender.Female, Age = ActorAge.Child };
			var action = new VoiceActorEditUndoAction(m_testProject, replacedActor);
			action.Undo();

			Assert.IsTrue(action.Redo());
			var restoredActor = m_testProject.VoiceActorList.GetVoiceActorById(1);
			Assert.AreEqual("Aimee", restoredActor.Name);
			Assert.AreEqual(ActorAge.YoungAdult, restoredActor.Age);
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
			m_testProject.VoiceActorList.Actors.Add(new Glyssen.VoiceActor.VoiceActor { Id = 3, Name = "Chuck", Age = ActorAge.YoungAdult });

			var action = new VoiceActorAddedUndoAction(m_testProject, 3);
			Assert.AreEqual("Chuck", action.ActorAffected);
			Assert.IsFalse(action.JustChangedName);
		}
		#endregion

		#region Description Tests
		[Test]
		public void VoiceActorAddedUndoAction_Description_Normal_AddedActorReferencedByName()
		{
			m_testProject.VoiceActorList.Actors.Add(new Glyssen.VoiceActor.VoiceActor { Id = 3, Name = "Chuck", Age = ActorAge.YoungAdult });

			var action = new VoiceActorAddedUndoAction(m_testProject, 3);
			Assert.AreEqual("Add voice actor Chuck", action.Description);
		}

		[Test]
		public void VoiceActorAddedUndoAction_Description_ActorSubsequentlyDeleted_AddedActorReferencedByName()
		{
			var addedActor = new Glyssen.VoiceActor.VoiceActor {Id = 3, Name = "Chuck", Age = ActorAge.YoungAdult};
			m_testProject.VoiceActorList.Actors.Add(addedActor);
			var action = new VoiceActorAddedUndoAction(m_testProject, 3);
			m_testProject.VoiceActorList.Actors.Remove(addedActor);
			Assert.AreEqual("Add voice actor Chuck", action.Description);
		}
		#endregion

		#region Undo Tests
		[Test]
		public void VoiceActorAddedUndoAction_Undo_Normal_ActorDeleted()
		{
			m_testProject.VoiceActorList.Actors.Add(
				new Glyssen.VoiceActor.VoiceActor { Id = 3, Name = "Chuck", Age = ActorAge.YoungAdult });
			var action = new VoiceActorAddedUndoAction(m_testProject, 3);

			Assert.IsTrue(action.Undo());
			Assert.IsFalse(m_testProject.VoiceActorList.Actors.Any(a => a.Name == "Chuck" || a.Id == 3));
		}

		[Test]
		public void VoiceActorAddedUndoAction_Undo_ActorSubsequentlyDeleted_ReturnsFalse()
		{
			m_testProject.VoiceActorList.Actors.Add(
				new Glyssen.VoiceActor.VoiceActor { Id = 3, Name = "Chuck", Age = ActorAge.YoungAdult });
			var action = new VoiceActorAddedUndoAction(m_testProject, 3);
			m_testProject.VoiceActorList.Actors.Clear();
			Assert.AreEqual(0, m_testProject.VoiceActorList.Actors.Count);

			Assert.IsFalse(action.Undo());
			Assert.AreEqual(0, m_testProject.VoiceActorList.Actors.Count);
		}

		[Test]
		public void VoiceActorAddedUndoAction_Undo_ActorSubsequentlyModified_ActorDeleted()
		{
			var addedActor = new Glyssen.VoiceActor.VoiceActor { Id = 3, Name = "Chuck", Age = ActorAge.YoungAdult };
			m_testProject.VoiceActorList.Actors.Add(addedActor);
			var action = new VoiceActorAddedUndoAction(m_testProject, 3);
			addedActor.Name = "Charlie";
			addedActor.VoiceQuality = VoiceQuality.Authoritative;

			Assert.IsTrue(action.Undo());
			Assert.AreEqual(0, m_testProject.VoiceActorList.Actors.Count);
		}

		[Test]
		public void VoiceActorAddedUndoAction_Undo_ActorSubsequentlyAssignedToGroup_ReturnsFalse()
		{
			var group = new CharacterGroup(m_testProject, 22);
			group.CharacterIds = new CharacterIdHashSet(new[] { "Moses" });
			m_testProject.CharacterGroupList.CharacterGroups.Add(group);

			var addedActor = new Glyssen.VoiceActor.VoiceActor { Id = 3, Name = "Chuck", Age = ActorAge.YoungAdult };
			m_testProject.VoiceActorList.Actors.Add(addedActor);
			var action = new VoiceActorAddedUndoAction(m_testProject, 3);
			group.AssignVoiceActor(addedActor.Id);

			Assert.IsFalse(action.Undo());
			Assert.AreEqual(3, m_testProject.VoiceActorList.Actors.Single().Id);
		}
		#endregion

		#region Redo Tests
		[Test]
		public void VoiceActorAddedUndoAction_Redo_Normal_ActorReinstated()
		{
			m_testProject.VoiceActorList.Actors.Add(
				new Glyssen.VoiceActor.VoiceActor { Id = 3, Name = "Chuck", Age = ActorAge.YoungAdult });
			var action = new VoiceActorAddedUndoAction(m_testProject, 3);
			action.Undo();

			Assert.IsTrue(action.Redo());
			var reAddedActor = m_testProject.VoiceActorList.GetVoiceActorById(3);
			Assert.AreEqual("Chuck", reAddedActor.Name);
			Assert.AreEqual(ActorAge.YoungAdult, reAddedActor.Age);
		}

		[Test]
		public void VoiceActorAddedUndoAction_Redo_AddedDifferentActorWithSameIdAfterUndo_ActorReinstatedWithUniqueId()
		{
			m_testProject.VoiceActorList.Actors.Add(
				new Glyssen.VoiceActor.VoiceActor { Id = 3, Name = "Chuck", Age = ActorAge.YoungAdult });
			var action = new VoiceActorAddedUndoAction(m_testProject, 3);
			m_testProject.VoiceActorList.Actors.Clear();
			Assert.AreEqual(0, m_testProject.VoiceActorList.Actors.Count);
			action.Undo();
			m_testProject.VoiceActorList.Actors.Add(
				new Glyssen.VoiceActor.VoiceActor { Id = 3, Name = "Maggie", Gender = ActorGender.Female});

			Assert.IsTrue(action.Redo());
			Assert.AreEqual(2, m_testProject.VoiceActorList.Actors.Count);
			var reAddedActor = m_testProject.VoiceActorList.Actors.Single(a => a.Id != 3);
			Assert.AreEqual("Chuck", reAddedActor.Name);
			Assert.AreEqual(ActorAge.YoungAdult, reAddedActor.Age);
		}

		[Test]
		public void VoiceActorAddedUndoAction_Redo_AddedActorWithSameNameAfterUndo_ReturnsFalse()
		{
			m_testProject.VoiceActorList.Actors.Add(
				new Glyssen.VoiceActor.VoiceActor { Id = 3, Name = "Chuck", Age = ActorAge.YoungAdult });
			var action = new VoiceActorAddedUndoAction(m_testProject, 3);
			m_testProject.VoiceActorList.Actors.Clear();
			Assert.AreEqual(0, m_testProject.VoiceActorList.Actors.Count);
			action.Undo();
			m_testProject.VoiceActorList.Actors.Add(
				new Glyssen.VoiceActor.VoiceActor { Id = 40, Name = "Chuck", Age = ActorAge.Elder});

			Assert.IsFalse(action.Redo());
			Assert.AreEqual(ActorAge.Elder, m_testProject.VoiceActorList.Actors.Single().Age);
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
				new Glyssen.VoiceActor.VoiceActor { Id = 4, Name = "Dominic", Age = ActorAge.YoungAdult });
			Assert.IsFalse(action.JustChangedName);
			Assert.IsNull(action.ActorAffected);
			Assert.AreEqual("Dominic", action.DeletedActorName);
		}
		#endregion

		#region Description Tests
		[Test]
		public void VoiceActorDeletedUndoAction_Description_Normal_DeletedActorReferencedByName()
		{
			var action = new VoiceActorDeletedUndoAction(m_testProject,
				new Glyssen.VoiceActor.VoiceActor { Id = 4, Name = "Dominic", Age = ActorAge.YoungAdult });
			Assert.AreEqual("Delete voice actor Dominic", action.Description);
		}
		#endregion

		#region Undo Tests
		[Test]
		public void VoiceActorDeletedUndoAction_Undo_Normal_ActorRestored()
		{
			var removedActor = new Glyssen.VoiceActor.VoiceActor { Id = 4, Name = "Dominic", Age = ActorAge.YoungAdult };

			var action = new VoiceActorDeletedUndoAction(m_testProject, removedActor);
			Assert.IsTrue(action.Undo());
			var restoredActor = m_testProject.VoiceActorList.GetVoiceActorById(4);
			Assert.IsNotNull(restoredActor);
			// Equals is just defined as having the same ID, so we need to check name and details separately.
			Assert.AreEqual(restoredActor.Name, removedActor.Name);
			Assert.IsTrue(restoredActor.IsInterchangeableWith(removedActor));
		}

		[Test]
		public void VoiceActorDeletedUndoAction_Undo_ActorAssignedToGroup_ActorAndAssignmentRestored()
		{
			var assignedGroup = AddCharacterGroup(2, "Barnabas", "Caleb", "Hosea");
			var removedActor = new Glyssen.VoiceActor.VoiceActor { Id = 4, Name = "Dominic", Age = ActorAge.YoungAdult };

			var action = new VoiceActorDeletedUndoAction(m_testProject, removedActor, assignedGroup);
			Assert.IsTrue(action.Undo());
			var restoredActor = m_testProject.VoiceActorList.GetVoiceActorById(4);
			Assert.IsNotNull(restoredActor);
			// Equals is just defined as having the same ID, so we need to check name and details separately.
			Assert.AreEqual(restoredActor.Name, removedActor.Name);
			Assert.IsTrue(restoredActor.IsInterchangeableWith(removedActor));
			Assert.AreEqual(4, assignedGroup.VoiceActorId);
		}

		[Test]
		public void VoiceActorDeletedUndoAction_Undo_ActorAssignedToGroupWhichIsSubsequentlyRegeneratedWithSameCharacters_ActorAndAssignmentRestored()
		{
			var assignedGroup = AddCharacterGroup(2, "Barnabas", "Caleb", "Hosea");
			var removedActor = new Glyssen.VoiceActor.VoiceActor { Id = 4, Name = "Dominic", Age = ActorAge.YoungAdult };

			var action = new VoiceActorDeletedUndoAction(m_testProject, removedActor, assignedGroup);
			m_testProject.CharacterGroupList.CharacterGroups.Remove(assignedGroup);
			var newGroup = AddCharacterGroup(3, "Barnabas", "Caleb", "Hosea");

			Assert.IsTrue(action.Undo());
			var restoredActor = m_testProject.VoiceActorList.GetVoiceActorById(4);
			Assert.IsNotNull(restoredActor);
			// Equals is just defined as having the same ID, so we need to check name and details separately.
			Assert.AreEqual(restoredActor.Name, removedActor.Name);
			Assert.IsTrue(restoredActor.IsInterchangeableWith(removedActor));
			Assert.AreEqual(4, newGroup.VoiceActorId);
		}

		[Test]
		public void VoiceActorDeletedUndoAction_Undo_ActorAssignedToGroupWhichIsSubsequentlyDeleted_ReturnsFalse()
		{
			var assignedGroup = AddCharacterGroup(1, "Barnabas", "Caleb", "Hosea");
			var removedActor = new Glyssen.VoiceActor.VoiceActor { Id = 4, Name = "Dominic", Age = ActorAge.YoungAdult };

			var action = new VoiceActorDeletedUndoAction(m_testProject, removedActor, assignedGroup);
			m_testProject.CharacterGroupList.CharacterGroups.Remove(assignedGroup);
			Assert.AreEqual(0, m_testProject.CharacterGroupList.CharacterGroups.Count);
			AddCharacterGroup(1, "Caleb", "Hosea");
			AddCharacterGroup(2, "Barnabas", "Joshua");
			AddCharacterGroup(3, "Martha");
			Assert.AreEqual(3, m_testProject.CharacterGroupList.CharacterGroups.Count);

			Assert.IsFalse(action.Undo());
			Assert.IsNull(m_testProject.VoiceActorList.GetVoiceActorById(4));
			Assert.IsNull(m_testProject.VoiceActorList.Actors.SingleOrDefault(a => a.Name == "Dominic"));
			Assert.AreEqual(0, m_testProject.CharacterGroupList.CountVoiceActorsAssigned());
		}

		[Test]
		public void VoiceActorDeletedUndoAction_Undo_AnotherActorAddedWithSameId_ActorRestoredWithUniqueId()
		{
			var removedActor = new Glyssen.VoiceActor.VoiceActor { Id = 4, Name = "Dominic", Age = ActorAge.YoungAdult };

			var action = new VoiceActorDeletedUndoAction(m_testProject, removedActor);

			var reAddedActor = new Glyssen.VoiceActor.VoiceActor { Id = 4, Name = "Marshall", Age = ActorAge.Elder };
			m_testProject.VoiceActorList.Actors.Add(reAddedActor);

			Assert.IsTrue(action.Undo());
			var restoredActor = m_testProject.VoiceActorList.Actors.Single(a => a.Name == "Dominic");
			Assert.IsNotNull(restoredActor);
			// Equals is just defined as having the same ID, so we need to check name and details separately.
			Assert.AreEqual(restoredActor.Name, removedActor.Name);
			Assert.IsTrue(restoredActor.IsInterchangeableWith(removedActor));
			Assert.AreEqual(m_testProject.VoiceActorList.Actors.Count, m_testProject.VoiceActorList.Actors.Select(a => a.Id).Distinct().Count());
		}

		[Test]
		public void VoiceActorDeletedUndoAction_Undo_AnotherActorAddedWithSameName_ReturnsFalse()
		{
			var removedActor = new Glyssen.VoiceActor.VoiceActor { Id = 4, Name = "Dominic", Age = ActorAge.YoungAdult };

			var action = new VoiceActorDeletedUndoAction(m_testProject, removedActor);

			m_testProject.VoiceActorList.Actors.Add(new Glyssen.VoiceActor.VoiceActor { Id = 2, Name = "Dominic", Age = ActorAge.Child });

			Assert.IsFalse(action.Undo());
		}
		#endregion

		#region Redo Tests
		[Test]
		public void VoiceActorDeletedUndoAction_Redo_Normal_ActorReDeleted()
		{
			var removedActor = new Glyssen.VoiceActor.VoiceActor { Id = 4, Name = "Dominic", Age = ActorAge.YoungAdult };

			var action = new VoiceActorDeletedUndoAction(m_testProject, removedActor);
			action.Undo();
			Assert.IsTrue(action.Redo());

			Assert.IsNull(m_testProject.VoiceActorList.GetVoiceActorById(4));
			Assert.IsNull(m_testProject.VoiceActorList.Actors.SingleOrDefault(a => a.Name == "Dominic"));
		}

		[Test]
		public void VoiceActorDeletedUndoAction_Redo_ActorAssignedToGroup_ActorReDeletedAndAssignmentRemoved()
		{
			var assignedGroup = AddCharacterGroup(2, "Barnabas", "Caleb", "Hosea");
			var removedActor = new Glyssen.VoiceActor.VoiceActor { Id = 4, Name = "Dominic", Age = ActorAge.YoungAdult };

			var action = new VoiceActorDeletedUndoAction(m_testProject, removedActor, assignedGroup);
			action.Undo();
			Assert.IsTrue(action.Redo());

			Assert.IsNull(m_testProject.VoiceActorList.GetVoiceActorById(4));
			Assert.IsNull(m_testProject.VoiceActorList.Actors.SingleOrDefault(a => a.Name == "Dominic"));
			Assert.AreEqual(0, m_testProject.CharacterGroupList.CountVoiceActorsAssigned());
		}

		[Test]
		public void VoiceActorDeletedUndoAction_Redo_ActorAssignedToGroupWhichIsSubsequentlyUnassigned_ActorReDeleted()
		{
			var assignedGroup = AddCharacterGroup(2, "Barnabas", "Caleb", "Hosea");
			var removedActor = new Glyssen.VoiceActor.VoiceActor { Id = 4, Name = "Dominic", Age = ActorAge.YoungAdult };

			var action = new VoiceActorDeletedUndoAction(m_testProject, removedActor, assignedGroup);

			action.Undo();
			Assert.AreEqual(1, m_testProject.CharacterGroupList.CountVoiceActorsAssigned());
			Assert.AreEqual(4, assignedGroup.VoiceActorId);
			assignedGroup.RemoveVoiceActor();
			Assert.IsTrue(action.Redo());

			Assert.IsNull(m_testProject.VoiceActorList.GetVoiceActorById(4));
			Assert.IsNull(m_testProject.VoiceActorList.Actors.SingleOrDefault(a => a.Name == "Dominic"));
			Assert.AreEqual(0, m_testProject.CharacterGroupList.CountVoiceActorsAssigned());
		}

		[Test]
		public void VoiceActorDeletedUndoAction_Redo_ActorAssignedToGroupButAssignedToDifferentGroupFollowingUndo_ReturnsFalse()
		{
			var assignedGroup = AddCharacterGroup(2, "Barnabas", "Caleb", "Hosea");
			var differentGroup = AddCharacterGroup(4, "Thomas", "Jonah");
			var removedActor = new Glyssen.VoiceActor.VoiceActor { Id = 4, Name = "Dominic", Age = ActorAge.YoungAdult };

			var action = new VoiceActorDeletedUndoAction(m_testProject, removedActor, assignedGroup);
			action.Undo();
			Assert.AreEqual(1, m_testProject.CharacterGroupList.CountVoiceActorsAssigned());
			Assert.AreEqual(4, assignedGroup.VoiceActorId);
			assignedGroup.RemoveVoiceActor();
			differentGroup.AssignVoiceActor(m_testProject.VoiceActorList.GetVoiceActorById(4).Id);

			Assert.IsFalse(action.Redo());

			var restoredActor = m_testProject.VoiceActorList.GetVoiceActorById(4);
			// Equals is just defined as having the same ID, so we need to check name and details separately.
			Assert.AreEqual(restoredActor.Name, removedActor.Name);
			Assert.IsTrue(restoredActor.IsInterchangeableWith(removedActor));
			Assert.AreEqual(4, differentGroup.VoiceActorId);
		}

		[Test]
		public void VoiceActorDeletedUndoAction_Redo_ActorAssignedToGroupWhichIsSubsequentlyDeleted_ActorReDeleted()
		{
			var assignedGroup = AddCharacterGroup(1, "Barnabas", "Caleb", "Hosea");
			AddCharacterGroup(5, "Adam", "Lot");
			var removedActor = new Glyssen.VoiceActor.VoiceActor { Id = 4, Name = "Dominic", Age = ActorAge.YoungAdult };

			var action = new VoiceActorDeletedUndoAction(m_testProject, removedActor, assignedGroup);

			action.Undo();
			m_testProject.CharacterGroupList.CharacterGroups.Remove(assignedGroup);

			Assert.IsTrue(action.Redo());

			Assert.IsNull(m_testProject.VoiceActorList.GetVoiceActorById(4));
			Assert.IsNull(m_testProject.VoiceActorList.Actors.SingleOrDefault(a => a.Name == "Dominic"));
			Assert.AreEqual(0, m_testProject.CharacterGroupList.CountVoiceActorsAssigned());
		}

		[Test]
		public void VoiceActorDeletedUndoAction_Redo_ActorSubsequentlyDeleted_ReturnsTrueButNoChange()
		{
			var removedActor = new Glyssen.VoiceActor.VoiceActor { Id = 4, Name = "Dominic", Age = ActorAge.YoungAdult };
			var action = new VoiceActorDeletedUndoAction(m_testProject, removedActor);
			action.Undo();

			var restoredActor = m_testProject.VoiceActorList.Actors.Single(a => a.Name == "Dominic");
			Assert.IsNotNull(restoredActor);
			m_testProject.VoiceActorList.Actors.Remove(restoredActor);

			Assert.IsTrue(action.Redo());

			Assert.IsNull(m_testProject.VoiceActorList.GetVoiceActorById(4));
			Assert.IsNull(m_testProject.VoiceActorList.Actors.SingleOrDefault(a => a.Name == "Dominic"));
		}
		#endregion

		#endregion

	}
}
