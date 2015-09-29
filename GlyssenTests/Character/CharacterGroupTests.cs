using System;
using Glyssen.Character;
using Glyssen.Rules;
using NUnit.Framework;

namespace GlyssenTests.Character
{
	class CharacterGroupTests
	{
		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			CharacterGroupAttribute<CharacterGender>.GetUiStringForValue = GetUiStringForCharacterGender;
			CharacterGroupAttribute<CharacterAge>.GetUiStringForValue = GetUiStringForCharacterAge;
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
			var group = new CharacterGroup();
			group.CharacterIds.Add("Peter (Simon)");
			group.CharacterIds.Add("Stephen");
			Assert.IsTrue(group.Matches(new CharacterDetail { Gender = CharacterGender.Male, Age = CharacterAge.Adult },
				CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.LooseExceptChild));
		}

		[Test]
		public void Matches_ModerateGenderMatching_ContainsCharacterThatMatchesExactlyOnGenderAndAge_ReturnsTrue()
		{
			var group = new CharacterGroup();
			group.CharacterIds.Add("Peter (Simon)");
			group.CharacterIds.Add("Stephen");
			Assert.IsTrue(group.Matches(new CharacterDetail { Gender = CharacterGender.Male, Age = CharacterAge.Adult },
				CharacterGenderMatchingOptions.Moderate, CharacterAgeMatchingOptions.LooseExceptChild));
		}

		[Test]
		public void Matches_ModerateGenderMatching_GroupContainsOnlyMale_CharacterPreferMale_ReturnsTrue()
		{
			var group = new CharacterGroup();
			group.CharacterIds.Add("Peter (Simon)");
			group.CharacterIds.Add("Stephen");
			Assert.IsTrue(group.Matches(new CharacterDetail { Gender = CharacterGender.PreferMale },
				CharacterGenderMatchingOptions.Moderate, CharacterAgeMatchingOptions.LooseExceptChild));
		}

		[Test]
		public void Matches_ModerateGenderMatching_GroupContainsOnlyPreferFemale_CharacterFemale_ReturnsTrue()
		{
			var group = new CharacterGroup();
			group.CharacterIds.Add("inscription on forehead of Babylon");
			Assert.IsTrue(group.Matches(new CharacterDetail { Gender = CharacterGender.Female },
				CharacterGenderMatchingOptions.Moderate, CharacterAgeMatchingOptions.LooseExceptChild));
		}

		[Test]
		public void Matches_LooseGenderMatching_ContainsCharacterThatMatchesExactlyOnAgeWithCompatibleGender_ReturnsTrue()
		{
			var group = new CharacterGroup();
			group.CharacterIds.Add("eagle crying with loud voice");
			Assert.IsTrue(group.Matches(new CharacterDetail { Gender = CharacterGender.Male, Age = CharacterAge.Adult },
				CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.LooseExceptChild));
			Assert.IsTrue(group.Matches(new CharacterDetail { Gender = CharacterGender.PreferMale, Age = CharacterAge.Adult },
				CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.LooseExceptChild));
		}

		[Test]
		public void Matches_LooseGenderMatching_ContainsNoCharacterWithCompatibleGender_ReturnsFalse()
		{
			var group = new CharacterGroup();
			group.CharacterIds.Add("eagle crying with loud voice");
			group.CharacterIds.Add("Stephen");
			Assert.IsFalse(group.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.Adult },
				CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.LooseExceptChild));
			Assert.IsFalse(group.Matches(new CharacterDetail { Gender = CharacterGender.PreferFemale, Age = CharacterAge.Adult },
				CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.LooseExceptChild));

			group = new CharacterGroup();
			group.CharacterIds.Add("inscription on forehead of Babylon");
			Assert.IsFalse(group.Matches(new CharacterDetail { Gender = CharacterGender.Male, Age = CharacterAge.Adult },
				CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.LooseExceptChild));
			Assert.IsFalse(group.Matches(new CharacterDetail { Gender = CharacterGender.PreferMale, Age = CharacterAge.Adult },
				CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.LooseExceptChild));
		}

		[Test]
		public void Matches_LooseGenderMatching_GroupContainsOnlyFemale_CharacterIsEither_ReturnsTrue()
		{
			var group = new CharacterGroup();
			group.CharacterIds.Add("Mary Magdalene");
			Assert.IsTrue(group.Matches(new CharacterDetail { Gender = CharacterGender.Either, Age = CharacterAge.Adult },
				CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.LooseExceptChild));
		}

		[Test]
		public void Matches_ModerateGenderMatching_GroupContainsOnlyFemale_CharacterIsEither_ReturnsFalse()
		{
			var group = new CharacterGroup();
			group.CharacterIds.Add("Mary Magdalene");
			Assert.IsFalse(group.Matches(new CharacterDetail { Gender = CharacterGender.Either, Age = CharacterAge.Adult },
				CharacterGenderMatchingOptions.Moderate, CharacterAgeMatchingOptions.LooseExceptChild));
		}

		[Test]
		public void Matches_LooseGenderMatching_GroupHasOnlyMaleAndFemale_CharacterIsNeuter_ReturnsTrue()
		{
			var group = new CharacterGroup();
			group.CharacterIds.Add("Mary Magdalene");
			group.CharacterIds.Add("Judas Iscariot");
			Assert.IsTrue(group.Matches(new CharacterDetail { Gender = CharacterGender.Neuter, Age = CharacterAge.Adult },
				CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.LooseExceptChild));
		}

		[Test]
		public void Matches_ModerateGenderMatching_GroupHasOnlyMaleAndFemale_CharacterIsNeuter_ReturnsFalse()
		{
			var group = new CharacterGroup();
			group.CharacterIds.Add("Mary Magdalene");
			group.CharacterIds.Add("Judas Iscariot");
			Assert.IsFalse(group.Matches(new CharacterDetail { Gender = CharacterGender.Neuter, Age = CharacterAge.Adult },
				CharacterGenderMatchingOptions.Moderate, CharacterAgeMatchingOptions.LooseExceptChild));
		}

		[Test]
		public void Matches_LooseGenderMatching_GroupHasOnlyEither_CharacterIsMale_ReturnsTrue()
		{
			var group = new CharacterGroup();
			group.CharacterIds.Add("dreamers");
			Assert.IsFalse(group.Matches(new CharacterDetail { Gender = CharacterGender.Male, Age = CharacterAge.Adult },
				CharacterGenderMatchingOptions.Moderate, CharacterAgeMatchingOptions.LooseExceptChild));
		}

		[Test]
		public void Matches_ModerateGenderMatching_GroupHasOnlyEither_CharacterIsMale_ReturnsFalse()
		{
			var group = new CharacterGroup();
			group.CharacterIds.Add("dreamers");
			Assert.IsFalse(group.Matches(new CharacterDetail { Gender = CharacterGender.Male, Age = CharacterAge.Adult },
				CharacterGenderMatchingOptions.Moderate, CharacterAgeMatchingOptions.LooseExceptChild));
		}

		[Test]
		public void Matches_LooseGenderMatching_NoCharacters_ThrowsException()
		{
			var group = new CharacterGroup();
			Assert.Throws<InvalidOperationException>(() =>
			{
				group.Matches(new CharacterDetail { Gender = CharacterGender.Neuter, Age = CharacterAge.Adult },
					CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.LooseExceptChild);
			});
		}

		[Test]
		public void Matches_ModerateGenderMatching_NoCharacters_ThrowsException()
		{
			var group = new CharacterGroup();
			Assert.Throws<InvalidOperationException>(() =>
			{
				group.Matches(new CharacterDetail { Gender = CharacterGender.Neuter, Age = CharacterAge.Adult },
					CharacterGenderMatchingOptions.Moderate, CharacterAgeMatchingOptions.LooseExceptChild);
			});
		}

		[Test]
		public void Matches_LooseGenderMatching_NarratorGroupNarratorDetail_ReturnsTrue()
		{
			var group = new CharacterGroup();
			group.CharacterIds.Add("narrator-MRK");
			group.CharacterIds.Add("narrator-JUD");
			Assert.IsTrue(group.Matches(new CharacterDetail { CharacterId = "narrator-MAT", Gender = CharacterGender.Either, Age = CharacterAge.Adult },
				CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.LooseExceptChild));
		}

		[Test]
		public void Matches_LooseGenderMatching_NarratorGroupWithNonNarratorDetail_ReturnsFalse()
		{
			var group = new CharacterGroup();
			group.CharacterIds.Add("narrator-MRK");
			group.CharacterIds.Add("narrator-JUD");
			Assert.IsFalse(group.Matches(new CharacterDetail { CharacterId = "Thomas" },
				CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.LooseExceptChild));
		}

		[Test]
		public void Matches_LooseGenderMatching_ExtraBiblicalGroupExtraBiblicalDetail_ReturnsTrue()
		{
			var group = new CharacterGroup();
			group.CharacterIds.Add("BC-MRK");
			group.CharacterIds.Add("BC-JUD");
			Assert.IsTrue(group.Matches(new CharacterDetail { CharacterId = "BC-MAT", Gender = CharacterGender.Either, Age = CharacterAge.Adult },
				CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.LooseExceptChild));
		}

		[Test]
		[Ignore("Need setting to tell Glyssen that Paul narrates his own books.")]
		public void Matches_LooseGenderMatching_PaulineEpistleNarratorGroupWithPaulAndSomeFutureSetting_ReturnsTrue()
		{
			var group = new CharacterGroup();
			group.CharacterIds.Add("narrator-ROM");
			group.CharacterIds.Add("narrator-EPH");
			Assert.IsTrue(group.Matches(new CharacterDetail { CharacterId = "Paul", Gender = CharacterGender.Male, Age = CharacterAge.Adult },
				CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.LooseExceptChild));
		}

		[Test]
		public void Matches_LooseGenderMatching_PaulineEpistleNarratorGroupWithPaulAndDefaultSetting_ReturnsTrue()
		{
			var group = new CharacterGroup();
			group.CharacterIds.Add("narrator-ROM");
			group.CharacterIds.Add("narrator-EPH");
			Assert.IsFalse(group.Matches(new CharacterDetail { CharacterId = "Paul", Gender = CharacterGender.Male, Age = CharacterAge.Adult },
				CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.LooseExceptChild));
		}

		[Test]
		public void Matches_LooseGenderMatching_LooseAgeMatching_CharacterHasDifferentAge_ReturnsTrue()
		{
			var group = new CharacterGroup();
			group.CharacterIds.Add("Rhoda"); // Female, YoungAdult
			Assert.IsTrue(group.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.Elder },
				CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.LooseExceptChild));
		}

		[Test]
		public void Matches_LooseGenderMatching_LooseAgeMatching_GroupHasChild_CharacterIsChild_ReturnsTrue()
		{
			var group = new CharacterGroup();
			group.CharacterIds.Add("children"); // Either, Child
			Assert.IsTrue(group.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.Child },
				CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.LooseExceptChild));
		}

		[Test]
		public void Matches_LooseGenderMatching_LooseAgeMatching_GroupHasChild_CharacterIsNotChild_ReturnsFalse()
		{
			var group = new CharacterGroup();
			group.CharacterIds.Add("children"); // Either, Child
			Assert.IsFalse(group.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.Adult },
				CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.LooseExceptChild));
		}

		[Test]
		public void Matches_LooseGenderMatching_LooseAgeMatching_GroupHasNoChild_CharacterIsChild_ReturnsFalse()
		{
			var group = new CharacterGroup();
			group.CharacterIds.Add("Rhoda"); // Female, YoungAdult
			Assert.IsFalse(group.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.Child },
				CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.LooseExceptChild));
		}

		[Test]
		public void Matches_LooseGenderMatching_StrictAgeMatching_CharacterHasDifferentAge_ReturnsFalse()
		{
			var group = new CharacterGroup();
			group.CharacterIds.Add("Rhoda"); // Female, YoungAdult
			Assert.IsFalse(group.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.Elder },
				CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.Strict));

			group.CharacterIds.Add("Sarah (Sarai) (old)"); // Female, Elder
			Assert.IsFalse(group.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.YoungAdult },
				CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.Strict));

			group.CharacterIds.Add("Miriam (young)"); // Female, Child
			Assert.IsFalse(group.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.Adult },
				CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.Strict));

			group.CharacterIds.Add("Mary Magdalene"); // Female, Adult
			Assert.IsFalse(group.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.Child },
				CharacterGenderMatchingOptions.Loose, CharacterAgeMatchingOptions.Strict));
		}

		[Test]
		public void AttributesDisplay_AllAdultEither_ReturnsEmptyString()
		{
			var group = new CharacterGroup();
			group.CharacterIds.Add("people");
			Assert.AreEqual(string.Empty, group.AttributesDisplay);
		}

		[Test]
		public void AttributesDisplay_TwoAdultMales_ReturnsMaleWithCountOfTwo()
		{
			var group = new CharacterGroup();
			group.CharacterIds.Add("Stephen");
			group.CharacterIds.Add("Jesus");
			Assert.AreEqual("Dude [2]", group.AttributesDisplay);
		}

		[Test]
		public void AttributesDisplay_TwoEitherChildrenAndOneEitherAdult_ReturnsChildWithCountOfTwo()
		{
			var group = new CharacterGroup();
			group.CharacterIds.Add("people");
			group.CharacterIds.Add("children");
			group.CharacterIds.Add("children of Zion (Jerusalem)");
			Assert.AreEqual("Kiddo [2]", group.AttributesDisplay);
		}

		[Test]
		public void AttributesDisplay_CoedWithTwoAges_ReturnsAllAttributes()
		{
			var group = new CharacterGroup();
			group.CharacterIds.Add("Rhoda");
			group.CharacterIds.Add("Jesus (child)");
			Assert.AreEqual("Gal [1]; Dude [1]; Kiddo [1]; Whippersnapper [1]", group.AttributesDisplay);
		}

		[Test]
		public void AttributesDisplay_ThreeNeuter_ReturnsNeuterWithCountOfThree()
		{
			var group = new CharacterGroup();
			group.CharacterIds.Add("ear");
			group.CharacterIds.Add("foot");
			group.CharacterIds.Add("head");
			Assert.AreEqual("Whatever [3]", group.AttributesDisplay);
		}
	}
}
