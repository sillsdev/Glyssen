using Glyssen;
using NUnit.Framework;

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

		[TestCase("ROM")]
		[TestCase("1CO")]
		[TestCase("2CO")]
		[TestCase("GAL")]
		[TestCase("EPH")]
		[TestCase("PHP")]
		[TestCase("COL")]
		[TestCase("1TH")]
		[TestCase("2TH")]
		[TestCase("1TI")]
		[TestCase("2TI")]
		[TestCase("TIT")]
		[TestCase("PHM")]
		[TestCase("HEB")]
		[TestCase("JAS")]
		[TestCase("1PE")]
		[TestCase("2PE")]
		[TestCase("1JN")]
		[TestCase("2JN")]
		[TestCase("3JN")]
		[TestCase("JUD")]
		public void DefaultToSingleVoice_Epistles_ReturnsTrue(string bookCode)
		{
			SingleVoiceReason singleVoiceReason;
			Assert.True(BookMetadata.DefaultToSingleVoice(bookCode, out singleVoiceReason));
			Assert.AreEqual(SingleVoiceReason.MostUsersWillNotDramatize, singleVoiceReason);
		}
	}
}
