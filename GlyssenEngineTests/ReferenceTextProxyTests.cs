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

			Assert.That(publicDomainDistributedReferenceTexts.Count, Is.EqualTo(2));
			VerifyBuiltInReferenceTexts(publicDomainDistributedReferenceTexts);
		}

		[Test]
		public void AllAvailable_CustomReferenceTexts_IncludesBuiltInAndCustomTexts()
		{
			TestReferenceText.CreateCustomReferenceText(TestReferenceTextResource.AzeriJUD);
			TestReferenceText.CreateCustomReferenceText(TestReferenceTextResource.EnglishJUD);

			var referenceTexts = ReferenceTextProxy.AllAvailable.ToList();

			VerifyBuiltInReferenceTexts(referenceTexts);

			Assert.That(referenceTexts.Count, Is.EqualTo(4));
			var customEnglish = referenceTexts.Single(r => r.Type == ReferenceTextType.Custom && r.CustomIdentifier == "English");
			Assert.That(ReferenceTextType.Custom, Is.EqualTo(customEnglish.Type));
			Assert.That(customEnglish.CustomIdentifier, Is.EqualTo("English"));

			var azeri = referenceTexts.Single(r => r.Type == ReferenceTextType.Custom && r.CustomIdentifier == "Azeri");
			Assert.That(ReferenceTextType.Custom, Is.EqualTo(azeri.Type));
			Assert.That(azeri.CustomIdentifier, Is.EqualTo("Azeri"));
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

			Assert.That(referenceTexts.Count, Is.EqualTo(4));
			Assert.That(referenceTexts.Single(r => r.Type == ReferenceTextType.Custom && r.CustomIdentifier == "English"),
				Is.Not.Null);
		}

		[Test]
		public void AllAvailable_AfterCallingGetOrCreateForUnavailableCustomRefText_IncludesMissingCustomText()
		{
			var idEpl = ReferenceTextProxy.GetOrCreate(ReferenceTextType.Custom, "EsperantoPigLatin");

			Assert.That(idEpl.Missing, Is.True);

			var referenceTexts = ReferenceTextProxy.AllAvailable.ToList();

			VerifyBuiltInReferenceTexts(referenceTexts);

			Assert.That(referenceTexts.Count, Is.EqualTo(3));
			Assert.That(idEpl, Is.EqualTo(referenceTexts.Single(r => r.Type == ReferenceTextType.Custom && r.CustomIdentifier == "EsperantoPigLatin")));
		}

		private static void VerifyBuiltInReferenceTexts(IEnumerable<ReferenceTextProxy> referenceTexts)
		{
			var english = referenceTexts.Single(r => r.Type == ReferenceTextType.English);
			Assert.That(ReferenceTextType.English, Is.EqualTo(english.Type));
			Assert.That(english.CustomIdentifier, Is.Null);

			var russian = referenceTexts.Single(r => r.Type == ReferenceTextType.Russian);
			Assert.That(ReferenceTextType.Russian, Is.EqualTo(russian.Type));
			Assert.That(russian.CustomIdentifier, Is.Null);
		}
	}
}
