using System.IO;
using System.Linq;
using Glyssen;
using Glyssen.Character;
using Glyssen.Shared;
using NUnit.Framework;
using SIL.IO;
using SIL.Scripture;

namespace GlyssenTests.Character
{
	[TestFixture]
	class ProjectCharacterVerseDataTests
	{
		private static readonly int kMATbookNum = BCVRef.BookToNumber("MAT");

		[Test]
		public void Constructor_PostVersion33Format_LoadsDataFromFile()
		{
			const string postVersion34CvDataLine = "MAT\t24\t1\tPeter/Andrew\tConfused\tdisciples\tFalse";
			using (var file = TempFile.WithFilenameInTempFolder(Project.kProjectCharacterVerseFileName))
			{
				File.WriteAllText(file.Path, postVersion34CvDataLine);

				var data = new ProjectCharacterVerseData(file.Path, ScrVers.English);

				var quoteInfo = data.GetCharacters(kMATbookNum, 24, new SingleVerse(1)).Single();

				Assert.AreEqual("Peter/Andrew", quoteInfo.Character);
				Assert.AreEqual("Confused", quoteInfo.Delivery);
				Assert.AreEqual("disciples", quoteInfo.Alias);
				Assert.IsFalse(quoteInfo.IsDialogue);
				Assert.IsTrue(quoteInfo.ProjectSpecific);
				Assert.IsNull(quoteInfo.DefaultCharacter);
				Assert.IsNull(quoteInfo.ParallelPassageReferences);
			}
		}

		[Test]
		public void Constructor_PreVersion34Format_AddsAdditionalEmptyFieldsWhenLoadingDataFromFile()
		{
			const string preVersion34CvDataLine = "MAT\t24\t1\tJesus\tMysteriously\t\tFalse\tTrue";
			using (var file = TempFile.WithFilenameInTempFolder(Project.kProjectCharacterVerseFileName))
			{
				File.WriteAllText(file.Path, preVersion34CvDataLine);

				var data = new ProjectCharacterVerseData(file.Path, ScrVers.English);

				var quoteInfo = data.GetCharacters(kMATbookNum, 24, new SingleVerse(1)).Single();

				Assert.AreEqual("Jesus", quoteInfo.Character);
				Assert.AreEqual("Mysteriously", quoteInfo.Delivery);
				Assert.IsFalse(quoteInfo.IsDialogue);
				Assert.IsTrue(quoteInfo.ProjectSpecific);
				Assert.IsNull(quoteInfo.DefaultCharacter);
				Assert.IsNull(quoteInfo.ParallelPassageReferences);
			}
		}
	}
}
