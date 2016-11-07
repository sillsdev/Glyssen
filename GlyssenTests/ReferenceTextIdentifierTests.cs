using System.Collections.Generic;
using System.IO;
using System.Linq;
using Glyssen;
using NUnit.Framework;

namespace GlyssenTests
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
				ReferenceTextIdentifier.ProprietaryReferenceTextProjectFileLocation = Path.GetFileNameWithoutExtension(Path.GetTempFileName());
				Assert.IsFalse(Directory.Exists(ReferenceTextIdentifier.ProprietaryReferenceTextProjectFileLocation));
			}

			var publicDomainDistributedReferenceTexts = ReferenceTextIdentifier.AllAvailable.ToList();

			Assert.AreEqual(2, publicDomainDistributedReferenceTexts.Count);
			VerifyBuiltInReferenceTexts(publicDomainDistributedReferenceTexts);
		}

		[Test]
		public void AllAvailable_CustomReferenceTexts_IncludesBuiltInAndCustomTexts()
		{
			TestReferenceText.CreateCustomReferenceText(TestReferenceText.TestReferenceTextResource.AzeriJUD);
			TestReferenceText.CreateCustomReferenceText(TestReferenceText.TestReferenceTextResource.EnglishJUD);

			var referenceTexts = ReferenceTextIdentifier.AllAvailable.ToList();

			VerifyBuiltInReferenceTexts(referenceTexts);

			Assert.AreEqual(4, referenceTexts.Count);
			var customEnglish = referenceTexts.Single(r => r.Type == ReferenceTextType.Custom && r.CustomIdentifier == "English");
			Assert.AreEqual(ReferenceTextType.Custom, customEnglish.Type);
			Assert.AreEqual("English", customEnglish.CustomIdentifier);
			var folder = customEnglish.ProjectFolder;
			Assert.IsTrue(folder.EndsWith(Path.DirectorySeparatorChar + "English"));
			folder = Path.GetDirectoryName(folder);

			var azeri = referenceTexts.Single(r => r.Type == ReferenceTextType.Custom && r.CustomIdentifier == "Azeri");
			Assert.AreEqual(ReferenceTextType.Custom, azeri.Type);
			Assert.AreEqual("Azeri", azeri.CustomIdentifier);
			Assert.AreEqual(folder, Path.GetDirectoryName(azeri.ProjectFolder));
		}

		[Test]
		public void AllAvailable_AfterCallingGetOrCreate_IncludesAllCustomTexts()
		{
			TestReferenceText.CreateCustomReferenceText(TestReferenceText.TestReferenceTextResource.AzeriJUD);
			TestReferenceText.CreateCustomReferenceText(TestReferenceText.TestReferenceTextResource.EnglishJUD);

			ReferenceTextIdentifier.ClearCache();
			var idAzeri = ReferenceTextIdentifier.GetOrCreate(ReferenceTextType.Custom, "Azeri");

			var referenceTexts = ReferenceTextIdentifier.AllAvailable.ToList();

			VerifyBuiltInReferenceTexts(referenceTexts);

			Assert.AreEqual(4, referenceTexts.Count);
			Assert.IsNotNull(referenceTexts.Single(r => r.Type == ReferenceTextType.Custom && r.CustomIdentifier == "English"));
		}

		[Test]
		public void AllAvailable_AfterCallingGetOrCreateForUnavailableCustomRefText_IncludesMissingCustomText()
		{
			TestReferenceText.OverrideProprietaryReferenceTextProjectFileLocationToTempLocation();

			var idEpl = ReferenceTextIdentifier.GetOrCreate(ReferenceTextType.Custom, "EsperantoPigLatin");

			Assert.IsTrue(idEpl.Missing);

			var referenceTexts = ReferenceTextIdentifier.AllAvailable.ToList();

			VerifyBuiltInReferenceTexts(referenceTexts);

			Assert.AreEqual(3, referenceTexts.Count);
			Assert.AreEqual(idEpl, referenceTexts.Single(r => r.Type == ReferenceTextType.Custom && r.CustomIdentifier == "EsperantoPigLatin"));
		}

		[Test]
		public void IsCustomReferenceAvailable_Yes_ReturnsTrue()
		{
			TestReferenceText.CreateCustomReferenceText(TestReferenceText.TestReferenceTextResource.AzeriJUD);
			TestReferenceText.CreateCustomReferenceText(TestReferenceText.TestReferenceTextResource.EnglishJUD);

			ReferenceTextIdentifier.ClearCache();
			Assert.IsTrue(ReferenceTextIdentifier.IsCustomReferenceAvailable("English"));
			Assert.IsTrue(ReferenceTextIdentifier.IsCustomReferenceAvailable("Azeri"));
		}

		[Test]
		public void IsCustomReferenceAvailable_NoCustomReferenceTexts_ReturnsFalse()
		{
			TestReferenceText.OverrideProprietaryReferenceTextProjectFileLocationToTempLocation();
			Assert.IsFalse(ReferenceTextIdentifier.IsCustomReferenceAvailable("English"));
			Assert.IsFalse(ReferenceTextIdentifier.IsCustomReferenceAvailable("Azeri"));
		}

		[Test]
		public void IsCustomReferenceAvailable_No_ReturnsFalse()
		{
			TestReferenceText.CreateCustomReferenceText(TestReferenceText.TestReferenceTextResource.AzeriJUD);

			Assert.IsFalse(ReferenceTextIdentifier.IsCustomReferenceAvailable("Spanish"));
			Assert.IsFalse(ReferenceTextIdentifier.IsCustomReferenceAvailable("English"));
		}

		private static void VerifyBuiltInReferenceTexts(IEnumerable<ReferenceTextIdentifier> referenceTexts)
		{
			var english = referenceTexts.Single(r => r.Type == ReferenceTextType.English);
			Assert.AreEqual(ReferenceTextType.English, english.Type);
			Assert.IsNull(english.CustomIdentifier);
			var folder = english.ProjectFolder;
			Assert.IsTrue(folder.EndsWith(Path.DirectorySeparatorChar + "English"));
			folder = Path.GetDirectoryName(folder);

			var russian = referenceTexts.Single(r => r.Type == ReferenceTextType.Russian);
			Assert.AreEqual(ReferenceTextType.Russian, russian.Type);
			Assert.IsNull(russian.CustomIdentifier);
			Assert.AreEqual(folder, Path.GetDirectoryName(russian.ProjectFolder));
		}
	}
}
