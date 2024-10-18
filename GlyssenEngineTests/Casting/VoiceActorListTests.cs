using System.Collections.Generic;
using System.IO;
using System.Text;
using GlyssenEngine.Casting;
using NUnit.Framework;
using SIL.TestUtilities;

namespace GlyssenEngineTests.Casting
{
	[TestFixture]
	class VoiceActorListTests
	{
		[Test]
		public void Roundtrip_Actors()
		{
			var list = new VoiceActorList
				{AllActors = new List<VoiceActor>
				{
					new VoiceActor {Id = 0, Name = "A", Gender = ActorGender.Female, Age = ActorAge.Elder},
					new VoiceActor {Id = 1, Name = "B"}
				}
			};
				
			var sb = new StringBuilder();
			using (var writer = new StringWriter(sb))
			{
				// Generates XML correctly
				list.Save(writer);
			}

			var results = sb.ToString();
			AssertThatXmlIn.String(results)
				.HasSpecifiedNumberOfMatchesForXpath("/VoiceActors/VoiceActor", 2);
			AssertThatXmlIn.String(results)
				.HasSpecifiedNumberOfMatchesForXpath("/VoiceActors/VoiceActor[@Id='0' and @Gender='Female' and @Age='Elder' and text()='A']", 1);
			AssertThatXmlIn.String(results)
				.HasSpecifiedNumberOfMatchesForXpath("/VoiceActors/VoiceActor[@Id='1' and @Gender='Male' and @Age='Adult' and text()='B']", 1);

			// Reads XML correctly
			var listFromDeserialization = VoiceActorList.LoadVoiceActorList(new StringReader(results));
			Assert.That(list.ActiveActors, Is.EqualTo(listFromDeserialization.ActiveActors));
		}

		[Test]
		public void MigrateDeprecatedGenderAndAgeStrings()
		{
			var actor = new VoiceActor { Id = 0, Name = "A", GenderDeprecatedString = "M - Male", AgeDeprecatedString = "O - Old" };
			Assert.That(ActorAge.Elder, Is.EqualTo(actor.Age));
			Assert.That(ActorGender.Male, Is.EqualTo(actor.Gender));

			actor = new VoiceActor { Id = 0, Name = "B", GenderDeprecatedString = "F - Female", AgeDeprecatedString = "E - Elder" };
			Assert.That(ActorAge.Elder, Is.EqualTo(actor.Age));
			Assert.That(ActorGender.Female, Is.EqualTo(actor.Gender));

			actor = new VoiceActor { Id = 0, Name = "C", AgeDeprecatedString = "C - Child" };
			Assert.That(ActorAge.Child, Is.EqualTo(actor.Age));

			actor = new VoiceActor { Id = 0, Name = "D", AgeDeprecatedString = "M - Middle Adult" };
			Assert.That(ActorAge.Adult, Is.EqualTo(actor.Age));

			actor = new VoiceActor { Id = 0, Name = "E", AgeDeprecatedString = "Y - Young" };
			Assert.That(ActorAge.YoungAdult, Is.EqualTo(actor.Age));

			actor = new VoiceActor { Id = 0, Name = "E", AgeDeprecatedString = "Y - Young Adult" };
			Assert.That(ActorAge.YoungAdult, Is.EqualTo(actor.Age));
		}
	}
}
