using System.IO;
using System.Linq;
using Glyssen.Shared;
using GlyssenEngine.Character;
using NUnit.Framework;
using SIL.Scripture;

namespace GlyssenEngineTests.Character
{
	[TestFixture]
	class ProjectCharacterVerseDataTests
	{
		private static readonly int kMATbookNum = BCVRef.BookToNumber("MAT");

		[Test]
		public void Constructor_PostVersion33Format_LoadsDataFromFile()
		{
			const string postVersion34CvDataLine = "MAT\t24\t1\tPeter/Andrew\tConfused\tdisciples\tFalse";
			using (var postVersion34CvDataLineReader = new StringReader(postVersion34CvDataLine))
			{
				var data = new ProjectCharacterVerseData(postVersion34CvDataLineReader, ScrVers.English);

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
			using (var preVersion34CvDataLineReader = new StringReader(preVersion34CvDataLine))
			{
				var data = new ProjectCharacterVerseData(preVersion34CvDataLineReader, ScrVers.English);

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
