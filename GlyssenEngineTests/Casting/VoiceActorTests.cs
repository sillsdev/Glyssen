using GlyssenCharacters;
using GlyssenEngine.Casting;
using NUnit.Framework;

namespace GlyssenEngineTests.Casting
{
	class VoiceActorTests
	{
		[Test]
		public void IsValid_HasName_ReturnsTrue()
		{
			var actor = new VoiceActor { Name = "A" };
			Assert.That(actor.IsValid(), Is.True);
		}

		[Test]
		public void IsValid_HasNoName_ReturnsFalse()
		{
			var actor = new VoiceActor { Gender = ActorGender.Female };
			Assert.That(actor.IsValid(), Is.False);
		}

		[Test]
		public void Matches_CharacterMatchesExactlyOnGenderAndAge_ReturnsTrue()
		{
			var actor = new VoiceActor { Gender = ActorGender.Female, Age = ActorAge.Elder };
			Assert.That(actor.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.Elder }));
		}

		[Test]
		public void Matches_GenderDifferent_ReturnsFalse()
		{
			var actor = new VoiceActor { Gender = ActorGender.Male };
			Assert.That(actor.Matches(new CharacterDetail { Gender = CharacterGender.Female }), Is.False);

			actor = new VoiceActor { Gender = ActorGender.Female };
			Assert.That(actor.Matches(new CharacterDetail { Gender = CharacterGender.Male }), Is.False);

			actor = new VoiceActor { Gender = ActorGender.Female };
			Assert.That(actor.Matches(new CharacterDetail { Gender = CharacterGender.PreferMale }), Is.False);

			actor = new VoiceActor { Gender = ActorGender.Male };
			Assert.That(actor.Matches(new CharacterDetail { Gender = CharacterGender.PreferFemale }), Is.False);
		}

		[Test]
		public void Matches_MaleActorCharacterGenderEither_ReturnsTrue()
		{
			var actor = new VoiceActor { Gender = ActorGender.Male };
			Assert.That(actor.Matches(new CharacterDetail { Gender = CharacterGender.Either }));
		}

		[Test]
		public void Matches_CharacterGenderNeuter_ReturnsTrue()
		{
			var actor = new VoiceActor { Gender = ActorGender.Male };
			Assert.That(actor.Matches(new CharacterDetail { Gender = CharacterGender.Neuter }));

			actor = new VoiceActor { Gender = ActorGender.Female };
			Assert.That(actor.Matches(new CharacterDetail { Gender = CharacterGender.Neuter }));
		}

		[Test]
		public void Matches_FemaleActorCharacterGenderEither_ReturnsFalse()
		{
			var actor = new VoiceActor { Gender = ActorGender.Female };
			Assert.That(actor.Matches(new CharacterDetail { Gender = CharacterGender.Either }), Is.False);
		}

		[Test]
		public void Matches_CharacterGenderPrefMale_ActorGenderMale_ReturnsTrue()
		{
			var actor = new VoiceActor { Gender = ActorGender.Male };
			Assert.That(actor.Matches(new CharacterDetail { Gender = CharacterGender.PreferMale }));
		}

		[Test]
		public void Matches_CharacterGenderPrefFemale_ActorGenderFemale_ReturnsTrue()
		{
			var actor = new VoiceActor { Gender = ActorGender.Female };
			Assert.That(actor.Matches(new CharacterDetail { Gender = CharacterGender.PreferFemale }));
		}

		[Test]
		public void Matches_StrictAgeMatchingFalse_CharacterHasDifferentAge_ReturnsTrue()
		{
			var actor = new VoiceActor { Gender = ActorGender.Female, Age = ActorAge.YoungAdult };
			Assert.That(actor.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.Elder }, false));
		}

		[Test]
		public void Matches_StrictAgeMatchingFalse_CharacterIsChild_ActorIsChild_ReturnsTrue()
		{
			var actor = new VoiceActor { Gender = ActorGender.Female, Age = ActorAge.Child };
			Assert.That(actor.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.Child }, false));
		}

		[TestCase(ActorAge.Child)]
		[TestCase(ActorAge.YoungAdult)]
		[TestCase(ActorAge.Adult)]
		public void Matches_StrictAgeMatchingFalse_CharacterIsChild_ActorIsNonElderlyFemale_ReturnsTrue(ActorAge age)
		{
			var actor = new VoiceActor { Gender = ActorGender.Female, Age = age };
			Assert.That(actor.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.Child }, false));
		}

		[Test]
		public void Matches_StrictAgeMatchingFalse_CharacterIsChild_ActorIsElderlyFemale_ReturnsFalse()
		{
			var actor = new VoiceActor { Gender = ActorGender.Female, Age = ActorAge.Elder };
			Assert.That(actor.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.Child }, false), Is.False);
		}

		[Test]
		public void Matches_StrictAgeMatchingFalse_CharacterIsNotChild_ActorIsAChild_ReturnsFalse()
		{
			var actor = new VoiceActor { Gender = ActorGender.Female, Age = ActorAge.Child };
			Assert.That(actor.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.YoungAdult }, false), Is.False);
		}

		[Test]
		public void Matches_StrictAgeMatching_CharacterHasDifferentAge_ReturnsFalse()
		{
			var actor = new VoiceActor { Gender = ActorGender.Female, Age = ActorAge.YoungAdult };
			Assert.That(actor.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.Elder }, true), Is.False);

			actor = new VoiceActor { Gender = ActorGender.Female, Age = ActorAge.Elder };
			Assert.That(actor.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.YoungAdult }, true), Is.False);

			actor = new VoiceActor { Gender = ActorGender.Female, Age = ActorAge.Child };
			Assert.That(actor.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.Adult }, true), Is.False);

			actor = new VoiceActor { Gender = ActorGender.Female, Age = ActorAge.Adult };
			Assert.That(actor.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.Child }, true), Is.False);
		}

		[TestCase(ActorAge.Adult, CharacterAge.Adult)]
		[TestCase(ActorAge.Elder, CharacterAge.Elder)]
		[TestCase(ActorAge.YoungAdult, CharacterAge.YoungAdult)]
		[TestCase(ActorAge.Child, CharacterAge.Child)]
		public void GetAgeMatchQuality_Perfect(ActorAge actorAge, CharacterAge characterAge)
		{
			var actor = new VoiceActor { Age = actorAge };
			var matchQuality = actor.GetAgeMatchQuality(new CharacterDetail {  Age = characterAge });
			Assert.That(MatchLevel.Perfect, Is.EqualTo(matchQuality));
		}

		[TestCase(ActorAge.Adult, CharacterAge.YoungAdult)]
		[TestCase(ActorAge.Adult, CharacterAge.Elder)]
		[TestCase(ActorAge.YoungAdult, CharacterAge.Adult)]
		[TestCase(ActorAge.Elder, CharacterAge.Adult)]
		public void GetAgeMatchQuality_CloseAdult(ActorAge actorAge, CharacterAge characterAge)
		{
			var actor = new VoiceActor { Age = actorAge };
			var matchQuality = actor.GetAgeMatchQuality(new CharacterDetail { Age = characterAge });
			Assert.That(MatchLevel.Acceptable, Is.EqualTo(matchQuality));
		}

		[TestCase(ActorAge.YoungAdult, CharacterAge.Elder)]
		[TestCase(ActorAge.Elder, CharacterAge.YoungAdult)]
		public void GetAgeMatchQuality_Poor(ActorAge actorAge, CharacterAge characterAge)
		{
			var actor = new VoiceActor { Age = actorAge };
			var matchQuality = actor.GetAgeMatchQuality(new CharacterDetail { Age = characterAge });
			Assert.That(MatchLevel.Poor, Is.EqualTo(matchQuality));
		}

		[TestCase(ActorAge.Adult, CharacterAge.Child)]
		[TestCase(ActorAge.YoungAdult, CharacterAge.Child)]
		[TestCase(ActorAge.Elder, CharacterAge.Child)]
		[TestCase(ActorAge.Child, CharacterAge.Adult)]
		[TestCase(ActorAge.Child, CharacterAge.YoungAdult)]
		[TestCase(ActorAge.Child, CharacterAge.Elder)]
		public void GetAgeMatchQuality_Mismatch(ActorAge actorAge, CharacterAge characterAge)
		{
			var actor = new VoiceActor { Age = actorAge };
			var matchQuality = actor.GetAgeMatchQuality(new CharacterDetail { Age = characterAge });
			Assert.That(MatchLevel.Mismatch, Is.EqualTo(matchQuality));
		}

		[TestCase(ActorGender.Female, CharacterGender.Female)]
		[TestCase(ActorGender.Female, CharacterGender.PreferFemale)]
		[TestCase(ActorGender.Female, CharacterGender.Neuter)]
		[TestCase(ActorGender.Male, CharacterGender.Male)]
		[TestCase(ActorGender.Male, CharacterGender.PreferMale)]
		[TestCase(ActorGender.Male, CharacterGender.Either)]
		[TestCase(ActorGender.Male, CharacterGender.Neuter)]
		public void GetGenderMatchQuality_Perfect(ActorGender actorGender, CharacterGender characterGender)
		{
			var actor = new VoiceActor { Gender = actorGender };
			var matchQuality = actor.GetGenderMatchQuality(new CharacterDetail { Gender = characterGender });
			Assert.That(MatchLevel.Perfect, Is.EqualTo(matchQuality));
		}

		[TestCase(ActorGender.Male, CharacterGender.PreferFemale)]
		public void GetGenderMatchQuality_Acceptable(ActorGender actorGender, CharacterGender characterGender)
		{
			// This might seem odd at first glance, but the only "prefer female" characters in the data
			// could easily be performed by a male actor.
			var actor = new VoiceActor { Gender = actorGender };
			var matchQuality = actor.GetGenderMatchQuality(new CharacterDetail { Gender = characterGender });
			Assert.That(MatchLevel.Acceptable, Is.EqualTo(matchQuality));
		}

		[TestCase(ActorGender.Female, CharacterGender.Male)]
		[TestCase(ActorGender.Female, CharacterGender.PreferMale)]
		[TestCase(ActorGender.Male, CharacterGender.Female)]
		[TestCase(ActorGender.Female, CharacterGender.Either)]
		public void GetGenderMatchQuality_Mismatch(ActorGender actorGender, CharacterGender characterGender)
		{
			var actor = new VoiceActor { Gender = actorGender };
			var matchQuality = actor.GetGenderMatchQuality(new CharacterDetail { Gender = characterGender });
			Assert.That(MatchLevel.Mismatch, Is.EqualTo(matchQuality));
		}
	}
}
