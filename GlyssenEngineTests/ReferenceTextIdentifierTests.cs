using System.Collections.Generic;
using System.IO;
using System.Linq;
using Glyssen.Shared;
using GlyssenEngine;
using GlyssenSharedTests;
using NUnit.Framework;

namespace GlyssenEngineTests
{
	[TestFixture]
	class ReferenceTextIdentifierTests
	{
		[TearDown]
		public void Teardown()
		{
			TestReferenceText.DeleteTempCustomReferenceProjectFolder();
		}

		[TestCase(true)]
		[TestCase(false)]
		public void AllAvailable_NoCustomReferenceTexts_IncludesOnlyBuiltInPublicDomainTexts(bool proprietaryReferenceTextLocationExists)
		{
			if (proprietaryReferenceTextLocationExists)
				TestReferenceText.OverrideProprietaryReferenceTextProjectFileLocationToTempLocation();
			else
			{
				var tempFolder = Path.GetFileNameWithoutExtension(Path.GetTempFileName());
				Assert.IsFalse(Directory.Exists(tempFolder));
				ReferenceTextProxy.Reader = GlyssenFileBasedPersistenceTests.TestFilePersistenceImplementation.OverrideProprietaryReferenceTextProjectFileLocationToTempLocation();
			}

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

			ReferenceTextProxy.ClearCache();
			var idAzeri = ReferenceTextProxy.GetOrCreate(ReferenceTextType.Custom, "Azeri");

			var referenceTexts = ReferenceTextProxy.AllAvailable.ToList();

			VerifyBuiltInReferenceTexts(referenceTexts);

			Assert.AreEqual(4, referenceTexts.Count);
			Assert.IsNotNull(referenceTexts.Single(r => r.Type == ReferenceTextType.Custom && r.CustomIdentifier == "English"));
		}

		[Test]
		public void AllAvailable_AfterCallingGetOrCreateForUnavailableCustomRefText_IncludesMissingCustomText()
		{
			TestReferenceText.OverrideProprietaryReferenceTextProjectFileLocationToTempLocation();

			var idEpl = ReferenceTextProxy.GetOrCreate(ReferenceTextType.Custom, "EsperantoPigLatin");

			Assert.IsTrue(idEpl.Missing);

			var referenceTexts = ReferenceTextProxy.AllAvailable.ToList();

			VerifyBuiltInReferenceTexts(referenceTexts);

			Assert.AreEqual(3, referenceTexts.Count);
			Assert.AreEqual(idEpl, referenceTexts.Single(r => r.Type == ReferenceTextType.Custom && r.CustomIdentifier == "EsperantoPigLatin"));
		}

		#region REVIEW: Should we delete these tests? They test a method only used here.
		[Test]
		public void IsCustomReferenceAvailable_Yes_ReturnsTrue()
		{
			TestReferenceText.CreateCustomReferenceText(TestReferenceTextResource.AzeriJUD);
			TestReferenceText.CreateCustomReferenceText(TestReferenceTextResource.EnglishJUD);

			ReferenceTextProxy.ClearCache();
			Assert.IsTrue(ReferenceTextProxy.IsCustomReferenceAvailable("English"));
			Assert.IsTrue(ReferenceTextProxy.IsCustomReferenceAvailable("Azeri"));
		}

		[Test]
		public void IsCustomReferenceAvailable_NoCustomReferenceTexts_ReturnsFalse()
		{
			TestReferenceText.OverrideProprietaryReferenceTextProjectFileLocationToTempLocation();
			Assert.IsFalse(ReferenceTextProxy.IsCustomReferenceAvailable("English"));
			Assert.IsFalse(ReferenceTextProxy.IsCustomReferenceAvailable("Azeri"));
		}

		[Test]
		public void IsCustomReferenceAvailable_No_ReturnsFalse()
		{
			TestReferenceText.CreateCustomReferenceText(TestReferenceTextResource.AzeriJUD);

			Assert.IsFalse(ReferenceTextProxy.IsCustomReferenceAvailable("Spanish"));
			Assert.IsFalse(ReferenceTextProxy.IsCustomReferenceAvailable("English"));
		}
		#endregion

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
