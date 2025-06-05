using System.Diagnostics;
using System.IO;
using System.Linq;
using Glyssen.Shared;
using GlyssenSharedTests;
using GlyssenFileBasedPersistence;
using NUnit.Framework;
using static GlyssenFileBasedPersistenceTests.TestFilePersistenceImplementation;

namespace GlyssenFileBasedPersistenceTests
{
	[TestFixture]
	public class PersistenceImplementationTests
	{
		[Test]
		public void GetCustomReferenceTextsNotAlreadyLoaded_CustomReferenceTextsExist_GetsThemAll()
		{
			var persistenceImpl = OverrideProprietaryReferenceTextProjectFileLocationToTempLocation();

			try
			{
				Assert.That(ReferenceTextTestUtils.CreateCustomReferenceText(persistenceImpl, TestReferenceTextResource.AzeriJUD),
					Is.EqualTo("Azeri"),
					"Setup problem: AzeriJUD should return language name as Azeri.");
				Assert.That(ReferenceTextTestUtils.CreateCustomReferenceText(persistenceImpl, TestReferenceTextResource.EnglishJUD),
					Is.EqualTo("English"),
					"Setup problem: AzeriJUD should return language name as Azeri.");

				var result = persistenceImpl.GetCustomReferenceTextsNotAlreadyLoaded().ToList();

				try
				{
					Assert.That(result.Count, Is.EqualTo(2));
					var metadata = ((TextReader)result.Single(r => r.Id == "English")).ReadToEnd();
					Assert.That(metadata, Is.EqualTo(ReferenceTextTestUtils.GetReferenceTextMetadata("English")));
					metadata = ((TextReader)result.Single(r => r.Id == "Azeri")).ReadToEnd();
					Assert.That(metadata, Is.EqualTo(ReferenceTextTestUtils.GetReferenceTextMetadata("Azeri")));
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
				CleanupUpTempImplementationAndRestorePreviousImplementation();
			}
		}

		[TestCase(true)]
		[TestCase(false)]
		public void GetCustomReferenceTextsNotAlreadyLoaded_NoCustomReferenceTextsExist_GetsEmptyCollection(bool folderExists)
		{
			var persistenceImpl = OverrideProprietaryReferenceTextProjectFileLocationToTempLocation(folderExists);

			try
			{
				Assert.That(persistenceImpl.GetCustomReferenceTextsNotAlreadyLoaded(), Is.Empty);
			}
			finally	
			{
				CleanupUpTempImplementationAndRestorePreviousImplementation();
			}
		}

		[TestCase(ReferenceTextType.English)]
		[TestCase(ReferenceTextType.Russian)]
		public void GetStandardReferenceText_NoCustomReferenceTextsExist_GetsReaderForMetadataOfRequestedReferenceText(ReferenceTextType type)
		{
			IProjectPersistenceReader persistenceImpl = new PersistenceImplementation();

			using (var reader = persistenceImpl.Load(new ReferenceTextId(type), ProjectResource.Metadata))
			{
				Assert.That(reader.ReadLine(), Does.StartWith("<?xml version"));
				Assert.That(reader.ReadLine(), Does.StartWith("<DBLMetadata id="));
				Assert.That(reader.ReadLine().Trim(), Is.EqualTo("<language>"));
				Assert.That(reader.ReadLine().Trim(), Does.Contain("<iso>"));
				Assert.That($"<name>{type.ToString()}</name>", Is.EqualTo(reader.ReadLine().Trim()));

			}
		}
	}
}
