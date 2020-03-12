using System.IO;
using System.Linq;
using Glyssen.Shared;
using GlyssenSharedTests;
using GlyssenFileBasedPersistence;
using NUnit.Framework;
using SIL.IO;

namespace GlyssenFileBasedPersistenceTests
{
	[TestFixture]
	public class PersistenceImplementationTests
	{
		[Test]
		public void GetAllCustomReferenceTexts_CustomReferenceTextsExist_GetsThemAll()
		{
			PersistenceImplementation persistenceImpl = TestFilePersistenceImplementation.OverrideProprietaryReferenceTextProjectFileLocationToTempLocation();

			try
			{
				Assert.AreEqual("Azeri",
					ReferenceTextTestUtils.CreateCustomReferenceText(persistenceImpl, TestReferenceTextResource.AzeriJUD),
					"Setup problem: AzeriJUD should return language name as Azeri.");
				Assert.AreEqual("English",
					ReferenceTextTestUtils.CreateCustomReferenceText(persistenceImpl, TestReferenceTextResource.EnglishJUD),
					"Setup problem: AzeriJUD should return language name as Azeri.");

				var result = persistenceImpl.GetAllCustomReferenceTexts().ToList();

				try
				{
					Assert.AreEqual(2, result.Count);
					var metadata = ((TextReader)result.Single(r => r.Id == "English")).ReadToEnd();
					Assert.AreEqual(ReferenceTextTestUtils.GetReferenceTextMetadata("English"), metadata);
					metadata = ((TextReader)result.Single(r => r.Id == "Azeri")).ReadToEnd();
					Assert.AreEqual(ReferenceTextTestUtils.GetReferenceTextMetadata("Azeri"), metadata);
				}
				finally
				{
					foreach (var resourceReader in result)
					{
						resourceReader?.Dispose();
					}
				}
			}
			finally	
			{
				TestFilePersistenceImplementation.DeleteTempCustomReferenceProjectFolder();
			}
		}

		[Test]
		public void GetAllCustomReferenceTexts_NoCustomReferenceTextsExist_GetsEmptyCollection()
		{
			PersistenceImplementation persistenceImpl = TestFilePersistenceImplementation.OverrideProprietaryReferenceTextProjectFileLocationToTempLocation();

			try
			{
				Assert.IsFalse(persistenceImpl.GetAllCustomReferenceTexts().Any());
			}
			finally	
			{
				TestFilePersistenceImplementation.DeleteTempCustomReferenceProjectFolder();
			}
		}

		[TestCase(ReferenceTextType.English)]
		[TestCase(ReferenceTextType.Russian)]
		public void GetStandardReferenceText_NoCustomReferenceTextsExist_GetsEmptyCollection(ReferenceTextType type)
		{
			IProjectPersistenceReader persistenceImpl = new PersistenceImplementation();
			string projectFileName = type.ToString().ToLowerInvariant() + ProjectRepository.kProjectFileExtension;

			using (var reader = persistenceImpl.Load(new ReferenceTextId(type), ProjectResource.Metadata))
			{
				Assert.IsTrue(reader.ReadLine().StartsWith("<?xml version"));
				Assert.IsTrue(reader.ReadLine().StartsWith("DBLMetadata id="));
				Assert.AreEqual("<language>", reader.ReadLine().Trim());
				Assert.IsTrue(reader.ReadLine().Trim().Contains("<iso>"));
				Assert.AreEqual($"<name>{type.ToString()}</name>", reader.ReadLine().Trim());

			}
		}
	}
}
