using System;
using Glyssen.Shared;
using GlyssenSharedTests.Properties;

namespace GlyssenSharedTests
{
	public enum TestReferenceTextResource
	{
		EnglishJUD,
		AzeriJUD,
		AzeriREV,
		FrenchMAT,
		FrenchMRK,
		SpanishMAT,
	}

	public static class ReferenceTextTestUtils
	{
		public static string CreateCustomReferenceText(IProjectPersistenceWriter persistenceImpl, params TestReferenceTextResource[] booksToInclude)
		{
			string customLanguageId = null;

			foreach (var testBook in booksToInclude)
				AddBook(testBook, persistenceImpl, ref customLanguageId);

			return customLanguageId;
		}

		public static void AddBook(TestReferenceTextResource testResource, IProjectPersistenceWriter persistenceImpl, ref string customLanguageId)
		{
			string fileContents = GetBookContents(testResource);
			var resourceName = testResource.ToString();
			var splitPos = resourceName.Length - 3;
			string language = resourceName.Substring(0, splitPos);
			string bookId = resourceName.Substring(splitPos);
			var customReferenceTextId = new ReferenceTextId(ReferenceTextType.Custom, language);
			if (customLanguageId == null)
			{
				customLanguageId = language;
				persistenceImpl.SetUpProjectPersistence(customReferenceTextId);
				using (var metadataWriter = persistenceImpl.GetTextWriter(customReferenceTextId, ProjectResource.Metadata))
					metadataWriter.Write(GetReferenceTextMetadata(language));
				using (var versificationWriter = persistenceImpl.GetTextWriter(customReferenceTextId, ProjectResource.Versification))
					versificationWriter.Write(Resources.EnglishVersification);
			}
			else if (customLanguageId != language)
			{
				throw new ArgumentException("Attempt to combine resources for different languages into a single reference text.",
					nameof(testResource));
			}
			
			using (var bookWriter = persistenceImpl.GetTextWriter(customReferenceTextId, new BookStub { BookId = bookId }))
				bookWriter.Write(fileContents);
		}

		public static string GetReferenceTextMetadata(string language) =>
			Resources.ResourceManager.GetString(language.ToLowerInvariant() + "_metadata");

		public static string GetBookContents(TestReferenceTextResource testResource)
		{
			switch (testResource)
			{
				case TestReferenceTextResource.EnglishJUD: return Resources.TestReferenceTextJUD;
				case TestReferenceTextResource.AzeriJUD: return Resources.AzeriJUDRefText;
				case TestReferenceTextResource.AzeriREV: return Resources.AzeriREVRefText;
				case TestReferenceTextResource.FrenchMAT: return Resources.FrenchMATRefText;
				case TestReferenceTextResource.FrenchMRK: return Resources.FrenchMRKRefText;
				case TestReferenceTextResource.SpanishMAT: return Resources.SpanishMATRefText;
				default:
					throw new ArgumentOutOfRangeException(nameof(testResource), testResource, null);
			}
		}
	}
}
