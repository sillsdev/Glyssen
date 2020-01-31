﻿using GlyssenEngine.Character;
using GlyssenEngine.VoiceActor;
using NUnit.Framework;

namespace GlyssenEngineTests.VoiceActor
{
	class VoiceActorTests
	{
		[Test]
		public void IsValid_HasName_ReturnsTrue()
		{
			var actor = new GlyssenEngine.VoiceActor.VoiceActor { Name = "A" };
			Assert.IsTrue(actor.IsValid());
		}

		[Test]
		public void IsValid_HasNoName_ReturnsFalse()
		{
			var actor = new GlyssenEngine.VoiceActor.VoiceActor { Gender = ActorGender.Female };
			Assert.IsFalse(actor.IsValid());
		}

		[Test]
		public void Matches_CharacterMatchesExactlyOnGenderAndAge_ReturnsTrue()
		{
			var actor = new GlyssenEngine.VoiceActor.VoiceActor { Gender = ActorGender.Female, Age = ActorAge.Elder };
			Assert.IsTrue(actor.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.Elder }));
		}

		[Test]
		public void Matches_GenderDifferent_ReturnsFalse()
		{
			var actor = new GlyssenEngine.VoiceActor.VoiceActor { Gender = ActorGender.Male };
			Assert.IsFalse(actor.Matches(new CharacterDetail { Gender = CharacterGender.Female }));

			actor = new GlyssenEngine.VoiceActor.VoiceActor { Gender = ActorGender.Female };
			Assert.IsFalse(actor.Matches(new CharacterDetail { Gender = CharacterGender.Male }));

			actor = new GlyssenEngine.VoiceActor.VoiceActor { Gender = ActorGender.Female };
			Assert.IsFalse(actor.Matches(new CharacterDetail { Gender = CharacterGender.PreferMale }));

			actor = new GlyssenEngine.VoiceActor.VoiceActor { Gender = ActorGender.Male };
			Assert.IsFalse(actor.Matches(new CharacterDetail { Gender = CharacterGender.PreferFemale }));
		}

		[Test]
		public void Matches_MaleActorCharacterGenderEither_ReturnsTrue()
		{
			var actor = new GlyssenEngine.VoiceActor.VoiceActor { Gender = ActorGender.Male };
			Assert.IsTrue(actor.Matches(new CharacterDetail { Gender = CharacterGender.Either }));
		}

		[Test]
		public void Matches_CharacterGenderNeuter_ReturnsTrue()
		{
			var actor = new GlyssenEngine.VoiceActor.VoiceActor { Gender = ActorGender.Male };
			Assert.IsTrue(actor.Matches(new CharacterDetail { Gender = CharacterGender.Neuter }));

			actor = new GlyssenEngine.VoiceActor.VoiceActor { Gender = ActorGender.Female };
			Assert.IsTrue(actor.Matches(new CharacterDetail { Gender = CharacterGender.Neuter }));
		}

		[Test]
		public void Matches_FemaleActorCharacterGenderEither_ReturnsFalse()
		{
			var actor = new GlyssenEngine.VoiceActor.VoiceActor { Gender = ActorGender.Female };
			Assert.IsFalse(actor.Matches(new CharacterDetail { Gender = CharacterGender.Either }));
		}

		[Test]
		public void Matches_CharacterGenderPrefMale_ActorGenderMale_ReturnsTrue()
		{
			var actor = new GlyssenEngine.VoiceActor.VoiceActor { Gender = ActorGender.Male };
			Assert.IsTrue(actor.Matches(new CharacterDetail { Gender = CharacterGender.PreferMale }));
		}

		[Test]
		public void Matches_CharacterGenderPrefFemale_ActorGenderFemale_ReturnsTrue()
		{
			var actor = new GlyssenEngine.VoiceActor.VoiceActor { Gender = ActorGender.Female };
			Assert.IsTrue(actor.Matches(new CharacterDetail { Gender = CharacterGender.PreferFemale }));
		}

		[Test]
		public void Matches_StrictAgeMatchingFalse_CharacterHasDifferentAge_ReturnsTrue()
		{
			var actor = new GlyssenEngine.VoiceActor.VoiceActor { Gender = ActorGender.Female, Age = ActorAge.YoungAdult };
			Assert.IsTrue(actor.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.Elder }, false));
		}

		[Test]
		public void Matches_StrictAgeMatchingFalse_CharacterIsChild_ActorIsChild_ReturnsTrue()
		{
			var actor = new GlyssenEngine.VoiceActor.VoiceActor { Gender = ActorGender.Female, Age = ActorAge.Child };
			Assert.IsTrue(actor.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.Child }, false));
		}

		[TestCase(ActorAge.Child)]
		[TestCase(ActorAge.YoungAdult)]
		[TestCase(ActorAge.Adult)]
		public void Matches_StrictAgeMatchingFalse_CharacterIsChild_ActorIsNonElderlyFemale_ReturnsTrue(ActorAge age)
		{
			var actor = new GlyssenEngine.VoiceActor.VoiceActor { Gender = ActorGender.Female, Age = age };
			Assert.IsTrue(actor.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.Child }, false));
		}

		[Test]
		public void Matches_StrictAgeMatchingFalse_CharacterIsChild_ActorIsElderlyFemale_ReturnsFalse()
		{
			var actor = new GlyssenEngine.VoiceActor.VoiceActor { Gender = ActorGender.Female, Age = ActorAge.Elder };
			Assert.IsFalse(actor.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.Child }, false));
		}

		[Test]
		public void Matches_StrictAgeMatchingFalse_CharacterIsNotChild_ActorIsAChild_ReturnsFalse()
		{
			var actor = new GlyssenEngine.VoiceActor.VoiceActor { Gender = ActorGender.Female, Age = ActorAge.Child };
			Assert.IsFalse(actor.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.YoungAdult }, false));
		}

		[Test]
		public void Matches_StrictAgeMatching_CharacterHasDifferentAge_ReturnsFalse()
		{
			var actor = new GlyssenEngine.VoiceActor.VoiceActor { Gender = ActorGender.Female, Age = ActorAge.YoungAdult };
			Assert.IsFalse(actor.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.Elder }, true));

			actor = new GlyssenEngine.VoiceActor.VoiceActor { Gender = ActorGender.Female, Age = ActorAge.Elder };
			Assert.IsFalse(actor.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.YoungAdult }, true));

			actor = new GlyssenEngine.VoiceActor.VoiceActor { Gender = ActorGender.Female, Age = ActorAge.Child };
			Assert.IsFalse(actor.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.Adult }, true));

			actor = new GlyssenEngine.VoiceActor.VoiceActor { Gender = ActorGender.Female, Age = ActorAge.Adult };
			Assert.IsFalse(actor.Matches(new CharacterDetail { Gender = CharacterGender.Female, Age = CharacterAge.Child }, true));
		}

		[TestCase(ActorAge.Adult, CharacterAge.Adult)]
		[TestCase(ActorAge.Elder, CharacterAge.Elder)]
		[TestCase(ActorAge.YoungAdult, CharacterAge.YoungAdult)]
		[TestCase(ActorAge.Child, CharacterAge.Child)]
		public void GetAgeMatchQuality_Perfect(ActorAge actorAge, CharacterAge characterAge)
		{
			var actor = new GlyssenEngine.VoiceActor.VoiceActor { Age = actorAge };
			var matchQuality = actor.GetAgeMatchQuality(new CharacterDetail {  Age = characterAge });
			Assert.AreEqual(MatchLevel.Perfect, matchQuality);
		}

		[TestCase(ActorAge.Adult, CharacterAge.YoungAdult)]
		[TestCase(ActorAge.Adult, CharacterAge.Elder)]
		[TestCase(ActorAge.YoungAdult, CharacterAge.Adult)]
		[TestCase(ActorAge.Elder, CharacterAge.Adult)]
		public void GetAgeMatchQuality_CloseAdult(ActorAge actorAge, CharacterAge characterAge)
		{
			var actor = new GlyssenEngine.VoiceActor.VoiceActor { Age = actorAge };
			var matchQuality = actor.GetAgeMatchQuality(new CharacterDetail { Age = characterAge });
			Assert.AreEqual(MatchLevel.Acceptable, matchQuality);
		}

		[TestCase(ActorAge.YoungAdult, CharacterAge.Elder)]
		[TestCase(ActorAge.Elder, CharacterAge.YoungAdult)]
		public void GetAgeMatchQuality_Poor(ActorAge actorAge, CharacterAge characterAge)
		{
			var actor = new GlyssenEngine.VoiceActor.VoiceActor { Age = actorAge };
			var matchQuality = actor.GetAgeMatchQuality(new CharacterDetail { Age = characterAge });
			Assert.AreEqual(MatchLevel.Poor, matchQuality);
		}

		[TestCase(ActorAge.Adult, CharacterAge.Child)]
		[TestCase(ActorAge.YoungAdult, CharacterAge.Child)]
		[TestCase(ActorAge.Elder, CharacterAge.Child)]
		[TestCase(ActorAge.Child, CharacterAge.Adult)]
		[TestCase(ActorAge.Child, CharacterAge.YoungAdult)]
		[TestCase(ActorAge.Child, CharacterAge.Elder)]
		public void GetAgeMatchQuality_Mismatch(ActorAge actorAge, CharacterAge characterAge)
		{
			var actor = new GlyssenEngine.VoiceActor.VoiceActor { Age = actorAge };
			var matchQuality = actor.GetAgeMatchQuality(new CharacterDetail { Age = characterAge });
			Assert.AreEqual(MatchLevel.Mismatch, matchQuality);
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
			var actor = new GlyssenEngine.VoiceActor.VoiceActor { Gender = actorGender };
			var matchQuality = actor.GetGenderMatchQuality(new CharacterDetail { Gender = characterGender });
			Assert.AreEqual(MatchLevel.Perfect, matchQuality);
		}

		[TestCase(ActorGender.Male, CharacterGender.PreferFemale)]
		public void GetGenderMatchQuality_Acceptable(ActorGender actorGender, CharacterGender characterGender)
		{
			// This might seem odd at first glance, but the only "prefer female" characters in the data
			// could easily be performed by a male actor.
			var actor = new GlyssenEngine.VoiceActor.VoiceActor { Gender = actorGender };
			var matchQuality = actor.GetGenderMatchQuality(new CharacterDetail { Gender = characterGender });
			Assert.AreEqual(MatchLevel.Acceptable, matchQuality);
		}

		[TestCase(ActorGender.Female, CharacterGender.Male)]
		[TestCase(ActorGender.Female, CharacterGender.PreferMale)]
		[TestCase(ActorGender.Male, CharacterGender.Female)]
		[TestCase(ActorGender.Female, CharacterGender.Either)]
		public void GetGenderMatchQuality_Mismatch(ActorGender actorGender, CharacterGender characterGender)
		{
			var actor = new GlyssenEngine.VoiceActor.VoiceActor { Gender = actorGender };
			var matchQuality = actor.GetGenderMatchQuality(new CharacterDetail { Gender = characterGender });
			Assert.AreEqual(MatchLevel.Mismatch, matchQuality);
		}
	}
}
