using Glyssen.Shared;
using NUnit.Framework;
using SIL.TestUtilities;
using SIL.Xml;
using Waxuquerque;

namespace WaxuquerqueTests
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
