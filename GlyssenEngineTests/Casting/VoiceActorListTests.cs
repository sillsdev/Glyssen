using System.Collections.Generic;
using System.IO;
using System.Text;
using GlyssenEngine.Casting;
using GlyssenSharedTests;
using NUnit.Framework;

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
			results.AssertHasXPathMatchCount("/VoiceActors/VoiceActor", 2);
			results.AssertHasXPathMatchCount("/VoiceActors/VoiceActor[@Id='0' and @Gender='Female' and @Age='Elder' and text()='A']", 1);
			results.AssertHasXPathMatchCount("/VoiceActors/VoiceActor[@Id='1' and @Gender='Male' and @Age='Adult' and text()='B']", 1);

			// Reads XML correctly
			var listFromDeserialization = VoiceActorList.LoadVoiceActorList(new StringReader(results));
			Assert.That(list.ActiveActors, Is.EqualTo(listFromDeserialization.ActiveActors));
		}

		[Test]
		public void MigrateDeprecatedGenderAndAgeStrings()
		{
			var actor = new VoiceActor { Id = 0, Name = "A", GenderDeprecatedString = "M - Male", AgeDeprecatedString = "O - Old" };
			Assert.That(actor.Age, Is.EqualTo(ActorAge.Elder));
			Assert.That(actor.Gender, Is.EqualTo(ActorGender.Male));

			actor = new VoiceActor { Id = 0, Name = "B", GenderDeprecatedString = "F - Female", AgeDeprecatedString = "E - Elder" };
			Assert.That(actor.Age, Is.EqualTo(ActorAge.Elder));
			Assert.That(actor.Gender, Is.EqualTo(ActorGender.Female));

			actor = new VoiceActor { Id = 0, Name = "C", AgeDeprecatedString = "C - Child" };
			Assert.That(actor.Age, Is.EqualTo(ActorAge.Child));

			actor = new VoiceActor { Id = 0, Name = "D", AgeDeprecatedString = "M - Middle Adult" };
			Assert.That(actor.Age, Is.EqualTo(ActorAge.Adult));

			actor = new VoiceActor { Id = 0, Name = "E", AgeDeprecatedString = "Y - Young" };
			Assert.That(actor.Age, Is.EqualTo(ActorAge.YoungAdult));

			actor = new VoiceActor { Id = 0, Name = "E", AgeDeprecatedString = "Y - Young Adult" };
			Assert.That(actor.Age, Is.EqualTo(ActorAge.YoungAdult));
		}
	}
}
