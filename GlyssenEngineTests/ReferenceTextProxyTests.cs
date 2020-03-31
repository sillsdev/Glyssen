using System.Collections.Generic;
using System.Linq;
using Glyssen.Shared;
using GlyssenEngine;
using GlyssenSharedTests;
using NUnit.Framework;

namespace GlyssenEngineTests
{
	[TestFixture]
	class ReferenceTextProxyTests
	{
		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			TestReferenceText.ForgetCustomReferenceTexts();
		}

		[TearDown]
		public void Teardown()
		{
			TestReferenceText.ForgetCustomReferenceTexts();
			ReferenceTextProxy.ForgetMissingCustomReferenceTexts();
		}

		[Test]
		public void AllAvailable_NoCustomReferenceTexts_IncludesOnlyBuiltInPublicDomainTexts()
		{
			var publicDomainDistributedReferenceTexts = ReferenceTextProxy.AllAvailable.ToList();

			Assert.AreEqual(2, publicDomainDistributedReferenceTexts.Count);
			VerifyBuiltInReferenceTexts(publicDomainDistributedReferenceTexts);
		}

		[Test]
		public void AllAvailable_CustomReferenceTexts_IncludesBuiltInAndCustomTexts()
		{
			TestReferenceText.CreateCustomReferenceText(TestReferenceTextResource.AzeriJUD);
			TestReferenceText.CreateCustomReferenceText(TestReferenceTextResource.EnglishJUD);

			var referenceTexts = ReferenceTextProxy.AllAvailable.ToList();

			VerifyBuiltInReferenceTexts(referenceTexts);

			Assert.AreEqual(4, referenceTexts.Count);
			var customEnglish = referenceTexts.Single(r => r.Type == ReferenceTextType.Custom && r.CustomIdentifier == "English");
			Assert.AreEqual(ReferenceTextType.Custom, customEnglish.Type);
			Assert.AreEqual("English", customEnglish.CustomIdentifier);

			var azeri = referenceTexts.Single(r => r.Type == ReferenceTextType.Custom && r.CustomIdentifier == "Azeri");
			Assert.AreEqual(ReferenceTextType.Custom, azeri.Type);
			Assert.AreEqual("Azeri", azeri.CustomIdentifier);
		}

		[Test]
		public void AllAvailable_AfterCallingGetOrCreate_IncludesAllCustomTexts()
		{
			TestReferenceText.CreateCustomReferenceText(TestReferenceTextResource.AzeriJUD);
			TestReferenceText.CreateCustomReferenceText(TestReferenceTextResource.EnglishJUD);

			// The above two lines will already have put the custom ones into the list of available,
			// so we clear the cache to simulate an initial start-up condition.
			ReferenceTextProxy.ClearCache();
			var _ = ReferenceTextProxy.GetOrCreate(ReferenceTextType.Custom, "Azeri");

			var referenceTexts = ReferenceTextProxy.AllAvailable.ToList();

			VerifyBuiltInReferenceTexts(referenceTexts);

			Assert.AreEqual(4, referenceTexts.Count);
			Assert.IsNotNull(referenceTexts.Single(r => r.Type == ReferenceTextType.Custom && r.CustomIdentifier == "English"));
		}

		[Test]
		public void AllAvailable_AfterCallingGetOrCreateForUnavailableCustomRefText_IncludesMissingCustomText()
		{
			var idEpl = ReferenceTextProxy.GetOrCreate(ReferenceTextType.Custom, "EsperantoPigLatin");

			Assert.IsTrue(idEpl.Missing);

			var referenceTexts = ReferenceTextProxy.AllAvailable.ToList();

			VerifyBuiltInReferenceTexts(referenceTexts);

			Assert.AreEqual(3, referenceTexts.Count);
			Assert.AreEqual(idEpl, referenceTexts.Single(r => r.Type == ReferenceTextType.Custom && r.CustomIdentifier == "EsperantoPigLatin"));
		}

		private static void VerifyBuiltInReferenceTexts(IEnumerable<ReferenceTextProxy> referenceTexts)
		{
			var english = referenceTexts.Single(r => r.Type == ReferenceTextType.English);
			Assert.AreEqual(ReferenceTextType.English, english.Type);
			Assert.IsNull(english.CustomIdentifier);

			var russian = referenceTexts.Single(r => r.Type == ReferenceTextType.Russian);
			Assert.AreEqual(ReferenceTextType.Russian, russian.Type);
			Assert.IsNull(russian.CustomIdentifier);
		}
	}
}
