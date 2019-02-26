using System;
using System.Linq;
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
		public void RemoveDangerousCharacters_Valid_NoChange(string filename)
		{
			Assert.AreEqual(filename, FileSystemUtils.RemoveDangerousCharacters(filename));
		}

		[TestCase("    ", ".")]
		[TestCase(" ", "")]
		[TestCase("....", "   ")]
		[TestCase(" .", "..")]
		[TestCase(".", ".")] // Although technically a leading period is allowed by the OS, we don't allow it in Glyssen
		[TestCase("", ". ")]
		public void RemoveDangerousCharacters_LeadingOrTrailingSpacesOrPeriods_Trimmed(string startJunk, string endJunk)
		{
			const string goodPart = "!abcdefg^";
			Assert.AreEqual(goodPart, FileSystemUtils.RemoveDangerousCharacters(startJunk + goodPart + endJunk));
		}

		[TestCase("This\u00a0is\u00a0a\u00a0string.", ExpectedResult = "This is a string")]
		[TestCase("This:way to\0\vthe\nbeach!   ", ExpectedResult = "This way to the beach!")]
		[TestCase("File: nice", ExpectedResult = "File nice")]
		[TestCase(" This is bad-\u0001", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-\u0002", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-\u0003", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-\u0004", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-\u0005", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-\u0006", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-\a", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-\b", ExpectedResult = "This is bad-")]
		[TestCase("\t This is bad-\t", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-\v", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-\f", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-\r", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-\u000e", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-\u000f", ExpectedResult = "This is bad-")]
		[TestCase(" This is bad-\u0010", ExpectedResult = "This is bad-")]
		[TestCase(" . This is bad-\u0011", ExpectedResult = "This is bad-")]
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
		[TestCase(" This is bad-/...", ExpectedResult = "This is bad-")]
		public string RemoveDangerousCharacters_BogusCharacters_ReplacedWithSpaces(string filename)
		{
			return FileSystemUtils.RemoveDangerousCharacters(filename);
		}

		[TestCase("CON")]
		[TestCase("PRN")]
		[TestCase("AUX")]
		[TestCase("NUL")]
		[TestCase("COM1")]
		[TestCase("COM2")]
		[TestCase("COM3")]
		[TestCase("COM4")]
		[TestCase("COM5")]
		[TestCase("COM6")]
		[TestCase("COM7")]
		[TestCase("COM8")]
		[TestCase("COM9")]
		[TestCase("LPT1")]
		[TestCase("LPT2")]
		[TestCase("LPT3")]
		[TestCase("LPT4")]
		[TestCase("LPT5")]
		[TestCase("LPT6")]
		[TestCase("LPT7")]
		[TestCase("LPT8")]
		[TestCase("LPT9")]
		[TestCase("LPT9 \t")]
		[TestCase("NUL.txt")]
		[TestCase("COM6.jpeg")]
		[TestCase("PRN.html")]
		[TestCase("LPT9.bloggyboofunkmomma")]
		public void RemoveDangerousCharacters_ReservedFilenames_ReturnsEmptyString(string filename)
		{
			Assert.AreEqual("", FileSystemUtils.RemoveDangerousCharacters(filename));
		}

		[TestCase("")]
		[TestCase(".")]
		[TestCase("a")]
		[TestCase(" .\t")]
		public void RemoveDangerousCharacters_NameTooLong_ReturnsTruncatedString(string stringToAppend)
		{
			var goodPart = "glkejhg#fekfnrwefnrwqenfjunruiugkfjnslkjn wefojweofjiw ;wer f.fuw";
			Assert.AreEqual(goodPart, FileSystemUtils.RemoveDangerousCharacters(goodPart + stringToAppend, goodPart.Length));
		}

		[TestCase("This\u00a0is\u00a0a\u00a0string.", new[] { "NO-BREAK SPACE (U+00A0)" })]
		[TestCase("This:\vway to\0\vthe\nbeach!   ", new [] { ":", "LINE TABULATION (U+000B)", "NULL (U+0000)", "LINE FEED (LF) (U+000A)" })]
		[TestCase("File: nice", new[] { ":" })]
		[TestCase(" This is bad-\u0001", new[] { "START OF HEADING (U+0001)" })]
		[TestCase(" This is bad-\u0002", new[] { "START OF TEXT (U+0002)" })]
		[TestCase(" This is bad-\u0003", new[] { "END OF TEXT (U+0003)" })]
		[TestCase(" This is bad-\u0004", new[] { "END OF TRANSMISSION (U+0004)" })]
		[TestCase(" This is bad-\u0005", new[] { "ENQUIRY (U+0005)" })]
		[TestCase(" This is bad-\u0006", new[] { "ACKNOWLEDGE (U+0006)" })]
		[TestCase(" This is bad-\a", new[] { "NO-BREAK SPACE (U+00A0)" })]
		[TestCase(" This is bad-\b", new[] { "BACKSPACE (U+0008)" })]
		[TestCase(" This is bad-\t", new[] { "CHARACTER TABULATION (U+0009)" })]
		[TestCase(" This is bad-\v", new[] { "LINE TABULATION (U+000B)" })]
		[TestCase(" This is bad-\f", new[] { "FORM FEED (FF) (U+000C)" })]
		[TestCase(" This is bad-\r", new[] { "CARRIAGE RETURN (CR) (U+000D)" })]
		[TestCase(" This is bad-\u000e", new[] { "SHIFT OUT (U+000E)" })]
		[TestCase(" This is bad-\u000f", new[] { "SHIFT IN (U+000F)" })]
		[TestCase(" This is bad-\u0010", new[] { "DATA LINK ESCAPE (U+0010)" })]
		[TestCase(" This is bad-\u0011", new[] { "DEVICE CONTROL ONE (U+0011)" })]
		[TestCase(" This is bad-\u0012", new[] { "DEVICE CONTROL TWO (U+0012)" })]
		[TestCase(" This is bad-\u0013", new[] { "DEVICE CONTROL THREE (U+0013)" })]
		[TestCase(" This is bad-\u0014", new[] { "DEVICE CONTROL FOUR (U+0014)" })]
		[TestCase(" This is bad-\u0015", new[] { "NEGATIVE ACKNOWLEDGE (U+0015)" })]
		[TestCase(" This is bad-\u0016", new[] { "SYNCHRONOUS IDLE (U+0016)" })]
		[TestCase(" This is bad-\u0017", new[] { "END OF TRANSMISSION BLOCK (U+0017)" })]
		[TestCase(" This is bad-\u0018", new[] { "CANCEL (U+0018)" })]
		[TestCase(" This is bad-\u0019", new[] { "END OF MEDIUM (U+0019)" })]
		[TestCase(" This is bad-\u001a", new[] { "SUBSTITUTE (U+001A)" })]
		[TestCase(" This is bad-\u001b", new[] { "ESCAPE (U+001B)" })]
		[TestCase(" This is bad-\u001c", new[] { "INFORMATION SEPARATOR FOUR (U+001C)" })]
		[TestCase(" This is bad-\u001d", new[] { "INFORMATION SEPARATOR THREE (U+001D)" })]
		[TestCase(" This is bad-\u001e", new[] { "INFORMATION SEPARATOR TWO (U+001E)" })]
		[TestCase(" This is bad-\u001f", new[] { "INFORMATION SEPARATOR ONE (U+001F)" })]
		[TestCase(" This is bad-\"", new[] { "\"" })]
		[TestCase(" This is bad-<>", new[] { "<", ">" })]
		[TestCase(" This is bad-|", new[] { "|" })]
		[TestCase(" This is bad-*", new[] { "*" })]
		[TestCase(" This is bad-?", new[] { "?" })]
		[TestCase(" This is bad-\\", new[] { "\\" })]
		[TestCase(" This is bad-/", new[] { "/" })]
		public void GetIllegalFilenameCharacters_BogusCharacters_GetsUniqueList(string filename, string[] badCharacters)
		{
			Assert.IsTrue(badCharacters.SequenceEqual(FileSystemUtils.GetIllegalFilenameCharacters(filename)));
		}

		[TestCase("")]
		[TestCase("abcdefg")]
		[TestCase("Tom's a cool guy!")]
		[TestCase("#deal&time~")]
		public void StartsOrEndsWithDisallowedCharacters_Valid_ReturnsFalse(string filename)
		{
			Assert.AreEqual(filename, FileSystemUtils.RemoveDangerousCharacters(filename));
		}

		[TestCase("    ", ".")]
		[TestCase(" ", "")]
		[TestCase("....", "   ")]
		[TestCase(" .", "..")]
		[TestCase(".", ".")] // Although technically a leading period is allowed by the OS, we don't allow it in Glyssen
		[TestCase("", ". ")]
		public void StartsOrEndsWithDisallowedCharacters_LeadingOrTrailingSpacesOrPeriods_ReturnsTrue(string startJunk, string endJunk)
		{
			const string goodPart = "!abcdefg^";
			Assert.IsTrue(FileSystemUtils.StartsOrEndsWithDisallowedCharacters(startJunk + goodPart + endJunk));
		}

		[Test]
		public void StartsOrEndsWithDisallowedCharacters_EmptyString_ReturnsFalse()
		{
			Assert.IsFalse(FileSystemUtils.StartsOrEndsWithDisallowedCharacters(String.Empty));
		}

		[TestCase("")]
		[TestCase(null)]
		[TestCase("....")]
		[TestCase("abcdefg")]
		[TestCase("Tom's a cool guy!")]
		[TestCase("#deal&time~")]
		[TestCase("This:\vway to\0\vthe\nbeach!   ")]
		[TestCase("LPT9 \t")]
		public void IsReservedFilename_NotReservedFilenames_ReturnsFalse(string filename)
		{
			Assert.IsFalse(FileSystemUtils.IsReservedFilename(filename));
		}

		[TestCase("CON")]
		[TestCase("PRN")]
		[TestCase("AUX")]
		[TestCase("NUL")]
		[TestCase("COM1")]
		[TestCase("COM2")]
		[TestCase("COM3")]
		[TestCase("COM4")]
		[TestCase("COM5")]
		[TestCase("COM6")]
		[TestCase("COM7")]
		[TestCase("COM8")]
		[TestCase("COM9")]
		[TestCase("LPT1")]
		[TestCase("LPT2")]
		[TestCase("LPT3")]
		[TestCase("LPT4")]
		[TestCase("LPT5")]
		[TestCase("LPT6")]
		[TestCase("LPT7")]
		[TestCase("LPT8")]
		[TestCase("LPT9")]
		[TestCase("NUL.txt")]
		[TestCase("COM6.jpeg")]
		[TestCase("PRN.html")]
		[TestCase("LPT9.bloggyboofunkmomma")]
		public void IsReservedFilename_ReservedFilenames_ReturnsTrue(string filename)
		{
			Assert.IsTrue(FileSystemUtils.IsReservedFilename(filename));
		}
	}
}
