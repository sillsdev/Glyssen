using Glyssen.Utilities;
using NUnit.Framework;

namespace GlyssenTests.Utilities
{
	[TestFixture]
	public class FileSystemUtilsTests
	{
		[TestCase("")]
		[TestCase("abcdefg")]
		[TestCase("Tom's a cool guy")]
		[TestCase("#deal&time~")]
		public void RemoveDangerousCharacters_NoChange(string filename)
		{
			Assert.AreEqual(filename, FileSystemUtils.RemoveDangerousCharacters(filename));
		}

		[TestCase("  \t  ", ".")]
		[TestCase("\t", "")]
		[TestCase("....", "   ")]
		[TestCase(" \t.", "..\t")]
		[TestCase("", ".\t ")]
		public void RemoveDangerousCharacters_Trimming(string startJunk, string endJunk)
		{
			const string goodPart = "!abcdefg^";
			Assert.AreEqual(goodPart, FileSystemUtils.RemoveDangerousCharacters(startJunk + goodPart + endJunk));
		}

		[TestCase("This\u00a0is\u00a0a\u00a0string.", ExpectedResult = "This is a string")]
		[TestCase("This:way to\0the\nbeach!   ", ExpectedResult = "This way to the beach!")]
		[TestCase(" This is bad-", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-\u0001", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-\u0002", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-\u0003", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-\u0004", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-\u0005", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-\u0006", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-\a", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-\b", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-\t", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-\v", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-\f", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-\r", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-\u000e", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-\u000f", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-\u0010", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-\u0011", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-\u0012", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-\u0013", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-\u0014", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-\u0015", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-\u0016", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-\u0017", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-\u0018", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-\u0019", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-\u001a", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-\u001b", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-\u001c", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-\u001d", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-\u001e", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-\u001f", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-\"", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-<>", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-|", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-*", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-?", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-\\", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-/", ExpectedResult = "This is bad-")]
		public string RemoveDangerousCharacters_BogusCharactersReplacedWithSpaces(string filename)
		{
			return FileSystemUtils.RemoveDangerousCharacters(filename);
		}
	}
}
