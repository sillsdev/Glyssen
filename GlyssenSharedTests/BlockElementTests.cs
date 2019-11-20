using Glyssen.Shared;
using NUnit.Framework;
// using SIL.TestUtilities;                This will be used again when libpalaso moves to NUnit 3.12
using GlyssenSharedTests.TestUtilities; // This will no longer be used when libpalaso moves to NUnit 3.12
using SIL.Xml;

namespace GlyssenTests
{
	class BlockElementTests
	{
		[Test]
		public void SerializeDeserialize_ObjectIsSound_RoundtripDataRemainsTheSame()
		{
			Sound sound = new Sound();

			var before = sound.Clone();
			var xmlString = XmlSerializationHelper.SerializeToString(sound);
			AssertThatXmlIn.String(xmlString).HasSpecifiedNumberOfMatchesForXpath("/Sound", 1);
			var after = XmlSerializationHelper.DeserializeFromString<Sound>(xmlString);
			Assert.AreEqual(before, after);
		}
	}
}
