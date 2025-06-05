using GlyssenCharacters;
using GlyssenEngine;
using GlyssenEngine.Casting;
using GlyssenEngine.Character;
using GlyssenEngine.Rules;
using NUnit.Framework;

namespace GlyssenEngineTests.Character
{
	class CharacterGroupTests
	{
		private Project m_project;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			CharacterGroupAttribute<CharacterGender>.GetUiStringForValue = GetUiStringForCharacterGender;
			CharacterGroupAttribute<CharacterAge>.GetUiStringForValue = GetUiStringForCharacterAge;
			m_project = TestProject.CreateBasicTestProject();
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			m_project = null;
			TestProject.DeleteTestProjects();
		}

		private static string GetUiStringForCharacterGender(CharacterGender characterGender)
		{
			switch (characterGender)
			{
				case CharacterGender.Male: return "Dude";
				case CharacterGender.Female: return "Gal";
				case CharacterGender.PreferMale: return "Prefer Male";
				case CharacterGender.PreferFemale: return "Prefer Female";
				case CharacterGender.Neuter: return "Whatever";
				default: return string.Empty;
			}
		}

		private static string GetUiStringForCharacterAge(CharacterAge characterAge)
		{
			switch (characterAge)
			{
				case CharacterAge.Child: return "Kiddo";
				case CharacterAge.Elder: return "Geezer";
				case CharacterAge.YoungAdult: return "Whippersnapper";
				default: return string.Empty;
			}
		}

		[Test]
		public void Matches_LooseGenderMatching_ContainsCharacterThatMatchesExactlyOnGenderAndAge_ReturnsTrue()
		{
			var group = new CharacterGroup(m_project);
			group.CharacterIds.Add("Peter (Simon)");
			group.CharacterIds.Add("Stephen");
			Assert.That(group.Matches(new CharacterDetail { Gender = CharacterGender.Male, Age = CharacterAge.Adult },
				CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.LooseExceptChild), Is.True);
		}

		[Test]
		public void Matches_ModerateGenderMatching_ContainsCharacterThatMatchesExactlyOnGenderAndAge_ReturnsTrue()
		{
			var group = new CharacterGroup(m_project);
			group.CharacterIds.Add("Peter (Simon)");
			group.CharacterIds.Add("Stephen");
			Assert.That(group.Matches(new CharacterDetail { Gender = CharacterGender.Male, Age = CharacterAge.Adult },
				CharacterGenderMatchingOptions.Moderate, CharacterAgeMatchingOptions.LooseExceptChild), Is.True);
		}

		[Test]
		public void Matches_ModerateGenderMatching_GroupContainsOnlyMale_CharacterPreferMale_ReturnsTrue()
		{
			var group = new CharacterGroup(m_project);
			group.CharacterIds.Add("Peter (Simon)");
			group.CharacterIds.Add("Stephen");
			Assert.That(group.Matches(new CharacterDetail { Gender = CharacterGender.PreferMale },
				CharacterGenderMatchingOptions.Moderate, CharacterAgeMatchingOptions.LooseExceptChild), Is.True);
		}

		[Test]
		public void Matches_ModerateGenderMatching_GroupContainsOnlyPreferFemale_CharacterFemale_ReturnsTrue()
		{
			var group = new CharacterGroup(m_project);
			group.CharacterIds.Add("inscription on forehead of Babylon");
			Assert.That(group.Matches(new CharacterDetail { Gender = CharacterGender.Female },
				CharacterGenderMatchingOptions.Moderate, CharacterAgeMatchingOptions.LooseExceptChild), Is.True);
		}

		[Test]
		public void Matches_LooseGenderMatching_ContainsCharacterThatMatchesExactlyOnAgeWithCompatibleGender_ReturnsTrue()
		{
			var group = new CharacterGroup(m_project);
			group.CharacterIds.Add("eagle crying with loud voice");
			Assert.That(group.Matches(new CharacterDetail { Gender = CharacterGender.Male, Age = CharacterAge.Adult },
				CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.LooseExceptChild), Is.True);
			Assert.That(group.Matches(new CharacterDetail { Gender = CharacterGender.PreferMale, Age = CharacterAge.Adult },
				CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.LooseExceptChild), Is.True);
		}

		[Test]
		public void Matches_LooseGenderMatching_ContainsNoCharacterWithCompatibleGender_ReturnsFalse()
		{
			var group = new CharacterGroup(m_project);
			group.CharacterIds.Add("eagle crying with loud voice");
			group.CharacterIds.Add("Stephen");
			Assert.That(group.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.Adult },
				CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.LooseExceptChild), Is.False);
			Assert.That(group.Matches(new CharacterDetail { Gender = CharacterGender.PreferFemale, Age = CharacterAge.Adult },
				CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.LooseExceptChild), Is.False);

			group = new CharacterGroup(m_project);
			group.CharacterIds.Add("inscription on forehead of Babylon");
			Assert.That(group.Matches(new CharacterDetail { Gender = CharacterGender.Male, Age = CharacterAge.Adult },
				CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.LooseExceptChild), Is.False);
			Assert.That(group.Matches(new CharacterDetail { Gender = CharacterGender.PreferMale, Age = CharacterAge.Adult },
				CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.LooseExceptChild), Is.False);
		}

		[Test]
		public void Matches_LooseGenderMatching_GroupContainsOnlyFemale_CharacterIsEither_ReturnsTrue()
		{
			var group = new CharacterGroup(m_project);
			group.CharacterIds.Add("Mary Magdalene");
			Assert.That(group.Matches(new CharacterDetail { Gender = CharacterGender.Either, Age = CharacterAge.Adult },
				CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.LooseExceptChild), Is.True);
		}

		[Test]
		public void Matches_ModerateGenderMatching_GroupContainsOnlyFemale_CharacterIsEither_ReturnsFalse()
		{
			var group = new CharacterGroup(m_project);
			group.CharacterIds.Add("Mary Magdalene");
			Assert.That(group.Matches(new CharacterDetail { Gender = CharacterGender.Either, Age = CharacterAge.Adult },
				CharacterGenderMatchingOptions.Moderate, CharacterAgeMatchingOptions.LooseExceptChild), Is.False);
		}

		[Test]
		public void Matches_LooseGenderMatching_GroupHasOnlyMaleAndFemale_CharacterIsNeuter_ReturnsTrue()
		{
			var group = new CharacterGroup(m_project);
			group.CharacterIds.Add("Mary Magdalene");
			group.CharacterIds.Add("Judas Iscariot");
			Assert.That(group.Matches(new CharacterDetail { Gender = CharacterGender.Neuter, Age = CharacterAge.Adult },
				CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.LooseExceptChild), Is.True);
		}

		[Test]
		public void Matches_ModerateGenderMatching_GroupHasOnlyMaleAndFemale_CharacterIsNeuter_ReturnsFalse()
		{
			var group = new CharacterGroup(m_project);
			group.CharacterIds.Add("Mary Magdalene");
			group.CharacterIds.Add("Judas Iscariot");
			Assert.That(group.Matches(new CharacterDetail { Gender = CharacterGender.Neuter, Age = CharacterAge.Adult },
				CharacterGenderMatchingOptions.Moderate, CharacterAgeMatchingOptions.LooseExceptChild), Is.False);
		}

		[Test]
		public void Matches_LooseGenderMatching_GroupHasOnlyEither_CharacterIsMale_ReturnsTrue()
		{
			var group = new CharacterGroup(m_project);
			group.CharacterIds.Add("dreamers");
			Assert.That(group.Matches(new CharacterDetail { Gender = CharacterGender.Male, Age = CharacterAge.Adult },
				CharacterGenderMatchingOptions.Moderate, CharacterAgeMatchingOptions.LooseExceptChild), Is.False);
		}

		[Test]
		public void Matches_ModerateGenderMatching_GroupHasOnlyEither_CharacterIsMale_ReturnsFalse()
		{
			var group = new CharacterGroup(m_project);
			group.CharacterIds.Add("dreamers");
			Assert.That(group.Matches(new CharacterDetail { Gender = CharacterGender.Male, Age = CharacterAge.Adult },
				CharacterGenderMatchingOptions.Moderate, CharacterAgeMatchingOptions.LooseExceptChild), Is.False);
		}

		[Test]
		public void Matches_LooseGenderMatching_NoCharacters_ReturnsFalse()
		{
			var group = new CharacterGroup(m_project);
			Assert.That(group.Matches(new CharacterDetail { Gender = CharacterGender.Neuter, Age = CharacterAge.Adult },
				CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.LooseExceptChild), Is.False);
		}

		[Test]
		public void Matches_ModerateGenderMatching_NoCharacters_ReturnsFalse()
		{
			var group = new CharacterGroup(m_project);
			Assert.That(group.Matches(new CharacterDetail { Gender = CharacterGender.Neuter, Age = CharacterAge.Adult },
				CharacterGenderMatchingOptions.Moderate, CharacterAgeMatchingOptions.LooseExceptChild), Is.False);
		}

		[Test]
		public void Matches_LooseGenderMatching_NarratorGroupNarratorDetail_ReturnsTrue()
		{
			var group = new CharacterGroup(m_project);
			group.CharacterIds.Add("narrator-MRK");
			group.CharacterIds.Add("narrator-JUD");
			Assert.That(group.Matches(new CharacterDetail { CharacterId = "narrator-MAT", Gender = CharacterGender.Either, Age = CharacterAge.Adult },
				CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.LooseExceptChild), Is.True);
		}

		[Test]
		public void Matches_LooseGenderMatching_NarratorGroupWithNonNarratorDetail_ReturnsFalse()
		{
			var group = new CharacterGroup(m_project);
			group.CharacterIds.Add("narrator-MRK");
			group.CharacterIds.Add("narrator-JUD");
			Assert.That(group.Matches(new CharacterDetail { CharacterId = "Thomas" },
				CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.LooseExceptChild), Is.False);
		}

		[Test]
		public void Matches_LooseGenderMatching_ExtraBiblicalGroupExtraBiblicalDetail_ReturnsTrue()
		{
			var group = new CharacterGroup(m_project);
			group.CharacterIds.Add("BC-MRK");
			group.CharacterIds.Add("BC-JUD");
			Assert.That(group.Matches(new CharacterDetail { CharacterId = "BC-MAT", Gender = CharacterGender.Either, Age = CharacterAge.Adult },
				CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.LooseExceptChild), Is.True);
		}

		[Test]
		[Ignore("Need setting to tell Glyssen that Paul narrates his own books.")]
		public void Matches_LooseGenderMatching_PaulineEpistleNarratorGroupWithPaulAndSomeFutureSetting_ReturnsTrue()
		{
			var group = new CharacterGroup(m_project);
			group.CharacterIds.Add("narrator-ROM");
			group.CharacterIds.Add("narrator-EPH");
			Assert.That(group.Matches(new CharacterDetail { CharacterId = "Paul", Gender = CharacterGender.Male, Age = CharacterAge.Adult },
				CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.LooseExceptChild), Is.True);
		}

		[Test]
		public void Matches_LooseGenderMatching_PaulineEpistleNarratorGroupWithPaulAndDefaultSetting_ReturnsTrue()
		{
			var group = new CharacterGroup(m_project);
			group.CharacterIds.Add("narrator-ROM");
			group.CharacterIds.Add("narrator-EPH");
			Assert.That(group.Matches(new CharacterDetail { CharacterId = "Paul", Gender = CharacterGender.Male, Age = CharacterAge.Adult },
				CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.LooseExceptChild), Is.False);
		}

		[Test]
		public void Matches_LooseGenderMatching_LooseAgeMatching_CharacterHasDifferentAge_ReturnsTrue()
		{
			var group = new CharacterGroup(m_project);
			group.CharacterIds.Add("Rhoda"); // Female, YoungAdult
			Assert.That(group.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.Elder },
				CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.LooseExceptChild), Is.True);
		}

		[Test]
		public void Matches_LooseGenderMatching_LooseAgeMatching_GroupHasChild_CharacterIsChild_ReturnsTrue()
		{
			var group = new CharacterGroup(m_project);
			group.CharacterIds.Add("children"); // Either, Child
			Assert.That(group.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.Child },
				CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.LooseExceptChild), Is.True);
		}

		[Test]
		public void Matches_LooseGenderMatching_LooseAgeMatching_GroupHasChild_CharacterIsNotChild_ReturnsFalse()
		{
			var group = new CharacterGroup(m_project);
			group.CharacterIds.Add("children"); // Either, Child
			Assert.That(group.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.Adult },
				CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.LooseExceptChild), Is.False);
		}

		[Test]
		public void Matches_LooseGenderMatching_LooseAgeMatching_GroupHasNoChild_CharacterIsChild_ReturnsFalse()
		{
			var group = new CharacterGroup(m_project);
			group.CharacterIds.Add("Rhoda"); // Female, YoungAdult
			Assert.That(group.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.Child },
				CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.LooseExceptChild), Is.False);
		}

		[Test]
		public void Matches_LooseGenderMatching_StrictAgeMatching_CharacterHasDifferentAge_ReturnsFalse()
		{
			var group = new CharacterGroup(m_project);
			group.CharacterIds.Add("Rhoda"); // Female, YoungAdult
			Assert.That(group.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.Elder },
				CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.Strict), Is.False);

			group.CharacterIds.Add("Sarah (Sarai) (old)"); // Female, Elder
			Assert.That(group.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.YoungAdult },
				CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.Strict), Is.False);

			group.CharacterIds.Add("Miriam (young)"); // Female, Child
			Assert.That(group.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.Adult },
				CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.Strict), Is.False);

			group.CharacterIds.Add("Mary Magdalene"); // Female, Adult
			Assert.That(group.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.Child },
				CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.Strict), Is.False);
		}

		[Test]
		public void Matches_ProjectSpecificCharacter_ReturnTrue()
		{
			m_project.AddProjectCharacterDetail(new CharacterDetail { CharacterId = "Bobette", Gender = CharacterGender.Female, Age = CharacterAge.YoungAdult });
			var group = new CharacterGroup(m_project);
			group.CharacterIds.Add("Bobette");
			Assert.That(group.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.YoungAdult },
				CharacterGenderMatchingOptions.Moderate, CharacterAgeMatchingOptions.Strict), Is.True);
		}

		[Test]
		public void AttributesDisplay_AllAdultEither_ReturnsEmptyString()
		{
			var group = new CharacterGroup(m_project);
			group.CharacterIds.Add("people");
			Assert.That(group.AttributesDisplay, Is.Empty);
		}

		[Test]
		public void AttributesDisplay_TwoAdultMales_ReturnsMaleWithCountOfTwo()
		{
			var group = new CharacterGroup(m_project);
			group.CharacterIds.Add("Stephen");
			group.CharacterIds.Add("Jesus");
			Assert.That(group.AttributesDisplay, Is.EqualTo("Dude [2]"));
		}

		[Test]
		public void AttributesDisplay_TwoEitherChildrenAndOneEitherAdult_ReturnsChildWithCountOfTwo()
		{
			var group = new CharacterGroup(m_project);
			group.CharacterIds.Add("people");
			group.CharacterIds.Add("children");
			group.CharacterIds.Add("children of Zion (Jerusalem)");
			Assert.That(group.AttributesDisplay, Is.EqualTo("Kiddo [2]"));
		}

		[Test]
		public void AttributesDisplay_CoedWithTwoAges_ReturnsAllAttributes()
		{
			var group = new CharacterGroup(m_project);
			group.CharacterIds.Add("Rhoda");
			group.CharacterIds.Add("Jesus (child)");
			Assert.That(group.AttributesDisplay, Is.EqualTo("Gal [1]; Dude [1]; Kiddo [1]; Whippersnapper [1]"));
		}

		[Test]
		public void AttributesDisplay_ThreeNeuter_ReturnsNeuterWithCountOfThree()
		{
			var group = new CharacterGroup(m_project);
			group.CharacterIds.Add("ear");
			group.CharacterIds.Add("foot");
			group.CharacterIds.Add("head");
			Assert.That(group.AttributesDisplay, Is.EqualTo("Whatever [3]"));
		}

		[Test]
		public void SetGroupIdLabelBasedOnCharacterIds_LabelIsNotNone_NoChange()
		{
			var group = new CharacterGroup(m_project);
			group.CharacterIds.Add("Andrew");
			group.GroupIdLabel = CharacterGroup.Label.Child;
			group.SetGroupIdLabel();
			Assert.That(group.GroupIdLabel, Is.EqualTo(CharacterGroup.Label.Child));
		}

		[Test]
		public void SetGroupIdLabelBasedOnCharacterIds_CameoActorAssigned_GroupIdLabelIsOtherAndTextSet()
		{
			var actor = new VoiceActor { Id = 1, Name = "Cameo Name", IsCameo = true };
			m_project.VoiceActorList.AllActors.Add(actor);
			var group = new CharacterGroup(m_project);
			group.CharacterIds.Add("Andrew");
			group.AssignVoiceActor(actor.Id);
			group.SetGroupIdLabel();
			Assert.That(group.GroupIdLabel, Is.EqualTo(CharacterGroup.Label.Other));
			Assert.That(group.GroupIdNumber, Is.EqualTo(0));
			Assert.That(group.GroupId, Is.EqualTo("Cameo Name"));
			Assert.That(group.GroupIdForUiDisplay, Is.EqualTo("Cameo Name"));
		}

		[Test]
		public void SetGroupIdLabelBasedOnCharacterIds_CharactersAllChildren_GroupIdLabelIsChild()
		{
			var group = new CharacterGroup(m_project);
			group.CharacterIds.Add("Jesus (child)");
			group.SetGroupIdLabel();
			Assert.That(group.GroupIdLabel, Is.EqualTo(CharacterGroup.Label.Child));
		}

		[Test]
		public void SetGroupIdLabelBasedOnCharacterIds_CharactersNotAllChildren_GroupIdLabelIsNotChild()
		{
			var group = new CharacterGroup(m_project);
			group.CharacterIds.Add("Andrew");
			group.CharacterIds.Add("Jesus (child)");
			group.SetGroupIdLabel();
			Assert.That(group.GroupIdLabel, Is.EqualTo(CharacterGroup.Label.Male));
		}

		[Test]
		public void SetGroupIdLabelBasedOnCharacterIds_AnyCharacterMale_GroupIdLabelIsMale()
		{
			var group = new CharacterGroup(m_project);
			group.CharacterIds.Add("Andrew");
			group.CharacterIds.Add("Rhoda");
			group.CharacterIds.Add("crowd");
			group.SetGroupIdLabel();
			Assert.That(group.GroupIdLabel, Is.EqualTo(CharacterGroup.Label.Male));
		}

		[Test]
		public void SetGroupIdLabelBasedOnCharacterIds_NoMaleButFemale_GroupIdLabelIsFemale()
		{
			var group = new CharacterGroup(m_project);
			group.CharacterIds.Add("Rhoda");
			group.CharacterIds.Add("crowd");
			Assert.That(group.ContainsCharacterWithGender(CharacterGender.Female), Is.True);
			group.SetGroupIdLabel();
			Assert.That(group.GroupIdLabel, Is.EqualTo(CharacterGroup.Label.Female));
		}

		[Test]
		public void SetGroupIdLabelBasedOnCharacterIds_NoMaleOrFemaleButPreferMale_GroupIdLabelIsMale()
		{
			var group = new CharacterGroup(m_project);
			group.CharacterIds.Add("altar");
			group.CharacterIds.Add("crowd");
			Assert.That(group.ContainsCharacterWithGender(CharacterGender.PreferMale), Is.True);
			group.SetGroupIdLabel();
			Assert.That(group.GroupIdLabel, Is.EqualTo(CharacterGroup.Label.Male));
		}

		[Test]
		public void SetGroupIdLabelBasedOnCharacterIds_NoMaleOrFemaleOrPreferMaleButPreferFemale_GroupIdLabelIsFemale()
		{
			var group = new CharacterGroup(m_project);
			group.CharacterIds.Add("inscription on forehead of Babylon");
			group.CharacterIds.Add("crowd");
			Assert.That(group.ContainsCharacterWithGender(CharacterGender.PreferFemale), Is.True);
			group.SetGroupIdLabel();
			Assert.That(group.GroupIdLabel, Is.EqualTo(CharacterGroup.Label.Female));
		}

		[Test]
		public void SetGroupIdLabelBasedOnCharacterIds_NoMaleOrFemaleOrPreferMaleOrPreferFemale_GroupIdLabelIsMale()
		{
			var group = new CharacterGroup(m_project);
			group.CharacterIds.Add("crowd");
			Assert.That(group.ContainsCharacterWithGender(CharacterGender.Either), Is.True);
			group.SetGroupIdLabel();
			Assert.That(group.GroupIdLabel, Is.EqualTo(CharacterGroup.Label.Male));
		}
	}
}
