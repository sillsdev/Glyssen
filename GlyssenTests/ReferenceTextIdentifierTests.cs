using System.IO;
using Glyssen;
using NUnit.Framework;

namespace GlyssenTests
{
	[TestFixture]
	class ReferenceTextIdentifierTests
	{
		[Test]
		public void AllAvailable_NoCustomReferenceTexts_IncludesOnlyBuiltInPublicDomainTexts()
		{
			ReferenceTextIdentifier.ProprietaryReferenceTextProjectFileLocation = Path.GetFileNameWithoutExtension(Path.GetTempFileName());

			Assert.IsFalse(Directory.Exists(ReferenceTextIdentifier.ProprietaryReferenceTextProjectFileLocation));

			var publicDomainDistributedReferenceTexts = ReferenceTextIdentifier.AllAvailable;

			Assert.AreEqual(2, publicDomainDistributedReferenceTexts.Count);
			Assert.AreEqual(ReferenceTextType.English, publicDomainDistributedReferenceTexts["English"]);
			Assert.AreEqual(ReferenceTextType.Russian, publicDomainDistributedReferenceTexts["Russian"]);
		}
	}
}
