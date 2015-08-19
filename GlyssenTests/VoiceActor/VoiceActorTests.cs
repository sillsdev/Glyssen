using Glyssen.Character;
using Glyssen.VoiceActor;
using NUnit.Framework;

namespace GlyssenTests.VoiceActor
{
	class VoiceActorTests
	{
		[Test]
		public void IsValid_HasName_ReturnsTrue()
		{
			var actor = new Glyssen.VoiceActor.VoiceActor { Name = "A" };
			Assert.IsTrue(actor.IsValid());
		}

		[Test]
		public void IsValid_HasNoName_ReturnsFalse()
		{
			var actor = new Glyssen.VoiceActor.VoiceActor { Gender = ActorGender.Female };
			Assert.IsFalse(actor.IsValid());
		}

		[Test]
		public void Matches_CharacterMatchesExactlyOnGenderAndAge_ReturnsTrue()
		{
			var actor = new Glyssen.VoiceActor.VoiceActor { Gender = ActorGender.Female, Age = ActorAge.Elder };
			Assert.IsTrue(actor.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.Elder }));
		}

		[Test]
		public void Matches_GenderDifferent_ReturnsFalse()
		{
			var actor = new Glyssen.VoiceActor.VoiceActor { Gender = ActorGender.Male };
			Assert.IsFalse(actor.Matches(new CharacterDetail { Gender = CharacterGender.Female }));

			actor = new Glyssen.VoiceActor.VoiceActor { Gender = ActorGender.Female };
			Assert.IsFalse(actor.Matches(new CharacterDetail { Gender = CharacterGender.Male }));

			actor = new Glyssen.VoiceActor.VoiceActor { Gender = ActorGender.Female };
			Assert.IsFalse(actor.Matches(new CharacterDetail { Gender = CharacterGender.PreferMale }));

			actor = new Glyssen.VoiceActor.VoiceActor { Gender = ActorGender.Male };
			Assert.IsFalse(actor.Matches(new CharacterDetail { Gender = CharacterGender.PreferFemale }));
		}

		[Test]
		public void Matches_CharacterGenderEitherOrNeuter_ReturnsTrue()
		{
			var actor = new Glyssen.VoiceActor.VoiceActor { Gender = ActorGender.Male };
			Assert.IsTrue(actor.Matches(new CharacterDetail { Gender = CharacterGender.Either }));

			actor = new Glyssen.VoiceActor.VoiceActor { Gender = ActorGender.Female };
			Assert.IsTrue(actor.Matches(new CharacterDetail { Gender = CharacterGender.Either }));

			actor = new Glyssen.VoiceActor.VoiceActor { Gender = ActorGender.Male };
			Assert.IsTrue(actor.Matches(new CharacterDetail { Gender = CharacterGender.Neuter }));

			actor = new Glyssen.VoiceActor.VoiceActor { Gender = ActorGender.Female };
			Assert.IsTrue(actor.Matches(new CharacterDetail { Gender = CharacterGender.Neuter }));
		}

		[Test]
		public void Matches_CharacterGenderPrefMale_ActorGenderMale_ReturnsTrue()
		{
			var actor = new Glyssen.VoiceActor.VoiceActor { Gender = ActorGender.Male };
			Assert.IsTrue(actor.Matches(new CharacterDetail { Gender = CharacterGender.PreferMale }));
		}

		[Test]
		public void Matches_CharacterGenderPrefFemale_ActorGenderFemale_ReturnsTrue()
		{
			var actor = new Glyssen.VoiceActor.VoiceActor { Gender = ActorGender.Female };
			Assert.IsTrue(actor.Matches(new CharacterDetail { Gender = CharacterGender.PreferFemale }));
		}

		[Test]
		public void Matches_StrictAgeMatchingFalse_CharacterHasDifferentAge_ReturnsTrue()
		{
			var actor = new Glyssen.VoiceActor.VoiceActor { Gender = ActorGender.Female, Age = ActorAge.YoungAdult };
			Assert.IsTrue(actor.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.Elder }, false));
		}

		[Test]
		public void Matches_StrictAgeMatchingFalse_CharacterIsChild_ActorIsChild_ReturnsTrue()
		{
			var actor = new Glyssen.VoiceActor.VoiceActor { Gender = ActorGender.Female, Age = ActorAge.Child };
			Assert.IsTrue(actor.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.Child }, false));
		}

		[Test]
		public void Matches_StrictAgeMatchingFalse_CharacterIsChild_ActorIsNotChild_ReturnsFalse()
		{
			var actor = new Glyssen.VoiceActor.VoiceActor { Gender = ActorGender.Female, Age = ActorAge.YoungAdult };
			Assert.IsFalse(actor.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.Child }, false));
		}

		[Test]
		public void Matches_StrictAgeMatchingFalse_CharacterIsNotChild_ActorIsAChild_ReturnsFalse()
		{
			var actor = new Glyssen.VoiceActor.VoiceActor { Gender = ActorGender.Female, Age = ActorAge.Child };
			Assert.IsFalse(actor.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.YoungAdult }, false));
		}

		[Test]
		public void Matches_StrictAgeMatching_CharacterHasDifferentAge_ReturnsFalse()
		{
			var actor = new Glyssen.VoiceActor.VoiceActor { Gender = ActorGender.Female, Age = ActorAge.YoungAdult };
			Assert.IsFalse(actor.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.Elder }, true));

			actor = new Glyssen.VoiceActor.VoiceActor { Gender = ActorGender.Female, Age = ActorAge.Elder };
			Assert.IsFalse(actor.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.YoungAdult }, true));

			actor = new Glyssen.VoiceActor.VoiceActor { Gender = ActorGender.Female, Age = ActorAge.Child };
			Assert.IsFalse(actor.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.Adult }, true));

			actor = new Glyssen.VoiceActor.VoiceActor { Gender = ActorGender.Female, Age = ActorAge.Adult };
			Assert.IsFalse(actor.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.Child }, true));
		}
	}
}
