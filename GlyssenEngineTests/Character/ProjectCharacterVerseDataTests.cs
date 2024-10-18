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

				Assert.That(quoteInfo.Character, Is.EqualTo("Peter/Andrew"));
				Assert.That(quoteInfo.Delivery, Is.EqualTo("Confused"));
				Assert.That(quoteInfo.Alias, Is.EqualTo("disciples"));
				Assert.That(quoteInfo.IsDialogue, Is.False);
				Assert.That(quoteInfo.ProjectSpecific, Is.True);
				Assert.That(quoteInfo.DefaultCharacter, Is.Null);
				Assert.That(quoteInfo.ParallelPassageReferences, Is.Null);
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

				Assert.That(quoteInfo.Character, Is.EqualTo("Jesus"));
				Assert.That(quoteInfo.Delivery, Is.EqualTo("Mysteriously"));
				Assert.That(quoteInfo.IsDialogue, Is.False);
				Assert.That(quoteInfo.ProjectSpecific, Is.True);
				Assert.That(quoteInfo.DefaultCharacter, Is.Null);
				Assert.That(quoteInfo.ParallelPassageReferences, Is.Null);
			}
		}
	}
}
