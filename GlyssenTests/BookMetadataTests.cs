using Glyssen;
using NUnit.Framework;
using Waxuquerque;

namespace GlyssenTests
{
	[TestFixture]
	class BookMetadataTests
	{
		[TestCase("NEH")]
		[TestCase("ISA")]
		[TestCase("JER")]
		[TestCase("HOS")]
		[TestCase("JOL")]
		[TestCase("AMO")]
		[TestCase("MIC")]
		[TestCase("NAM")]
		[TestCase("HAB")]
		[TestCase("ZEP")]
		[TestCase("HAG")]
		[TestCase("ZEC")]
		[TestCase("MAL")]
		public void DefaultToSingleVoice_TooComplexToAssignAccurately_ReturnsTrue(string bookCode)
		{
			SingleVoiceReason singleVoiceReason;
			Assert.True(BookMetadata.DefaultToSingleVoice(bookCode, out singleVoiceReason));
			Assert.AreEqual(SingleVoiceReason.TooComplexToAssignAccurately, singleVoiceReason);
		}
	}
}
