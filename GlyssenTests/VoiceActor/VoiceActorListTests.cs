using System.Collections.Generic;
using Glyssen.VoiceActor;
using NUnit.Framework;
using SIL.IO;
using SIL.TestUtilities;

namespace GlyssenTests.VoiceActor
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
				list.Actors = new List<Glyssen.VoiceActor.VoiceActor>
				{
					new Glyssen.VoiceActor.VoiceActor{Id = 0, Name = "A", Gender = "M - Male", Age = "O - Old"},
					new Glyssen.VoiceActor.VoiceActor{Id = 1, Name = "B"}
				};

				// Generates file correctly
				list.SaveToFile(tempFile.Path);
				AssertThatXmlIn.File(tempFile.Path)
					.HasSpecifiedNumberOfMatchesForXpath("/VoiceActors/VoiceActor", 2);
				AssertThatXmlIn.File(tempFile.Path)
					.HasSpecifiedNumberOfMatchesForXpath("/VoiceActors/VoiceActor[@Id='0' and @Gender='M - Male' and @Age='E - Elder' and text()='A']", 1);
				AssertThatXmlIn.File(tempFile.Path)
					.HasSpecifiedNumberOfMatchesForXpath("/VoiceActors/VoiceActor[@Id='1' and not(@Gender) and not(@Age) and text()='B']", 1);

				// Reads from file correctly
				VoiceActorList listFromFile = VoiceActorList.LoadVoiceActorListFromFile(tempFile.Path);
				Assert.AreEqual(list.Actors, listFromFile.Actors);
			}
		}
	}
}
