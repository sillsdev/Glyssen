using Glyssen.Shared;
using NUnit.Framework;
using SIL.Xml;

namespace GlyssenSharedTests
{
	[TestFixture]
	public class BlockElementTests
	{
		[Test]
		public void SerializeDeserialize_ObjectIsSound_RoundtripDataRemainsTheSame()
		{
			var sound = new Sound();

			var before = sound.Clone();
			var xmlString = XmlSerializationHelper.SerializeToString(sound);

			xmlString.AssertHasXPathMatchCount("/Sound", 1);

			var after = XmlSerializationHelper.DeserializeFromString<Sound>(xmlString);
			Assert.That(after, Is.EqualTo(before));
		}
	}
}
