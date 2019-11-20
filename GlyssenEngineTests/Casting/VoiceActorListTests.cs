using System.Collections.Generic;
using GlyssenEngine.Casting;
using NUnit.Framework;
using SIL.IO;
// using SIL.TestUtilities;                This will be used again when libpalaso moves to NUnit 3.12
using GlyssenSharedTests.TestUtilities; // This will no longer be used when libpalaso moves to NUnit 3.12

namespace GlyssenEngineTests.Casting
{
	[TestFixture]
	class VoiceActorListTests
	{
		[Test]
		public void Roundtrip_Actors()
		{
			using (TempFile tempFile = new TempFile())
			{
				VoiceActorList list = new VoiceActorList();
				list.AllActors = new List<VoiceActor>
				{
					new VoiceActor{Id = 0, Name = "A", Gender = ActorGender.Female, Age = ActorAge.Elder},
					new VoiceActor{Id = 1, Name = "B"}
				};

				// Generates file correctly
				list.SaveToFile(tempFile.Path);
				AssertThatXmlIn.File(tempFile.Path)
					.HasSpecifiedNumberOfMatchesForXpath("/VoiceActors/VoiceActor", 2);
				AssertThatXmlIn.File(tempFile.Path)
					.HasSpecifiedNumberOfMatchesForXpath("/VoiceActors/VoiceActor[@Id='0' and @Gender='Female' and @Age='Elder' and text()='A']", 1);
				AssertThatXmlIn.File(tempFile.Path)
					.HasSpecifiedNumberOfMatchesForXpath("/VoiceActors/VoiceActor[@Id='1' and @Gender='Male' and @Age='Adult' and text()='B']", 1);

				// Reads from file correctly
				VoiceActorList listFromFile = VoiceActorList.LoadVoiceActorListFromFile(tempFile.Path);
				Assert.AreEqual(list.ActiveActors, listFromFile.ActiveActors);
			}
		}

		[Test]
		public void MigrateDeprecatedGenderAndAgeStrings()
		{
			var actor = new VoiceActor { Id = 0, Name = "A", GenderDeprecatedString = "M - Male", AgeDeprecatedString = "O - Old" };
			Assert.AreEqual(ActorAge.Elder, actor.Age);
			Assert.AreEqual(ActorGender.Male, actor.Gender);

			actor = new VoiceActor { Id = 0, Name = "B", GenderDeprecatedString = "F - Female", AgeDeprecatedString = "E - Elder" };
			Assert.AreEqual(ActorAge.Elder, actor.Age);
			Assert.AreEqual(ActorGender.Female, actor.Gender);

			actor = new VoiceActor { Id = 0, Name = "C", AgeDeprecatedString = "C - Child" };
			Assert.AreEqual(ActorAge.Child, actor.Age);

			actor = new VoiceActor { Id = 0, Name = "D", AgeDeprecatedString = "M - Middle Adult" };
			Assert.AreEqual(ActorAge.Adult, actor.Age);

			actor = new VoiceActor { Id = 0, Name = "E", AgeDeprecatedString = "Y - Young" };
			Assert.AreEqual(ActorAge.YoungAdult, actor.Age);

			actor = new VoiceActor { Id = 0, Name = "E", AgeDeprecatedString = "Y - Young Adult" };
			Assert.AreEqual(ActorAge.YoungAdult, actor.Age);
		}
	}
}
