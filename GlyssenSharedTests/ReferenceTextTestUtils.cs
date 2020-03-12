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
			string language;
			string bookId;
			string fileContents;
			switch (testResource)
			{
				case TestReferenceTextResource.EnglishJUD:
					language = "English";
					bookId = "JUD";
					fileContents = Resources.TestReferenceTextJUD;
					break;
				case TestReferenceTextResource.AzeriJUD:
					language = "Azeri";
					bookId = "JUD";
					fileContents = Resources.AzeriJUDRefText;
					break;
				case TestReferenceTextResource.AzeriREV:
					language = "Azeri";
					bookId = "REV";
					fileContents = Resources.AzeriREVRefText;
					break;
				case TestReferenceTextResource.FrenchMAT:
					language = "French";
					bookId = "MAT";
					fileContents = Resources.FrenchMATRefText;
					break;
				case TestReferenceTextResource.FrenchMRK:
					language = "French";
					bookId = "MRK";
					fileContents = Resources.FrenchMRKRefText;
					break;
				case TestReferenceTextResource.SpanishMAT:
					language = "Spanish";
					bookId = "MAT";
					fileContents = Resources.SpanishMATRefText;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(testResource), testResource, null);
			}
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
	}
}
