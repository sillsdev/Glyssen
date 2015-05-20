using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Glyssen.Character;
using Glyssen.Properties;
using SIL.ScriptureUtils;

namespace ControlDataIntegrityTests
{
	[TestFixture]
	public class CharacterVerseDataTests
	{
		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			// Fixes issue where other test project was interfering with the running of this one (by setting the data to test data).
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = null;
		}

		[Test]
		public void DataIntegrity_ValidControlVersionPresent()
		{
			Assert.IsTrue(Regex.IsMatch(Resources.CharacterVerseData, @"\AControl File Version\t\d+\r?$", RegexOptions.Multiline));
		}

		[Test]
		public void DataIntegrity_RequiredFieldsHaveValidFormatAndThereAreNoDuplicateLines()
		{
			Regex regex = new Regex("^(?<bookId>...)\t(?<chapter>\\d+)\t(?<verse>\\d+)(-(?<endVerse>\\d+))?\t(?<character>[^\t]+)\t(?<delivery>[^\t]*)\t(?<alias>[^\t]*)\t(?<type>(Normal)|(Dialogue)|(Implicit)|(Indirect)|(Potential)|(Quotation)|(Hypothetical)|(FALSE))(\t(?<defaultCharacter>[^\t]*))?", RegexOptions.Compiled);
			string[] allLines = Resources.CharacterVerseData.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

			var set = new HashSet<string>();
			foreach (var line in allLines.Skip(1))
			{
				if (line.StartsWith("#"))
					continue;

				var match = regex.Match(line);
				Assert.IsTrue(match.Success, "Failed to match line: " + line);

				var bookId = match.Result("${bookId}");
				var bookNum = BCVRef.BookToNumber(bookId);
				Assert.IsTrue(bookNum > 0, "Line: " + line);
				Assert.IsTrue(bookNum <= 66, "Line: " + line);

				var chapter = Int32.Parse(match.Result("${chapter}"));
				Assert.IsTrue(chapter > 0, "Line: " + line);
				Assert.IsTrue(chapter <= 150, "Line: " + line);

				var verse = Int32.Parse(match.Result("${verse}"));
				Assert.IsTrue(verse > 0 || verse == 0 && bookId == "PSA", "Line: " + line);
				Assert.IsTrue(verse <= 152, "Line: " + line);

				var sEndVerse = match.Result("${endVerse}");
				if (!string.IsNullOrEmpty(sEndVerse))
				{
					var endVerse = Int32.Parse(sEndVerse);
					Assert.IsTrue(endVerse > verse, "Line: " + line);
					Assert.IsTrue(endVerse <= 152, "Line: " + line);
				}

				var character = match.Result("${character}");

				var alias = match.Result("${alias}");
				if (!string.IsNullOrEmpty(alias))
					Assert.AreNotEqual(character, alias, "Line: " + line);

				var defaultCharacter = match.Result("${defaultCharacter}");
				if (!string.IsNullOrEmpty(defaultCharacter))
					Assert.AreNotEqual(character, defaultCharacter, "Line: " + line);

				if (CharacterVerseData.IsCharacterOfType(character, CharacterVerseData.StandardCharacter.Narrator))
					Assert.AreNotEqual("Dialogue", match.Result("${type}"), "Line: " + line);

				var matchResult = match.Result("$&");
				Assert.IsTrue(set.Add(matchResult), "Duplicate line: " + matchResult);
			}
		}

		[Test]
		public void DataIntegrity_NoDuplicateData()
		{
			ISet<CharacterVerse> uniqueCharacterVerses = new HashSet<CharacterVerse>();
			IList<CharacterVerse> duplicateCharacterVerses = new List<CharacterVerse>();
			foreach (CharacterVerse cv in ControlCharacterVerseData.Singleton.GetAllQuoteInfo())
				if (!uniqueCharacterVerses.Add(cv))
					duplicateCharacterVerses.Add(cv);

			Assert.False(duplicateCharacterVerses.Any(),
				"Duplicate Character-Verse data:" + 
				Environment.NewLine + 
				duplicateCharacterVerses.Select(cv => cv.BcvRef + ", " + cv.Character).OnePerLineWithIndent());
		}

		[Test]
		public void DataIntegrity_NoDuplicateWhereOnlyDifferenceIsNormalVsNonnormalDelivery()
		{
			// PG-152: Currently, the program does not handle duplicates where the
			// only difference is between normal (blank) delivery and a specified delivery
			ISet<CharacterVerse> uniqueCharacterVerses = new HashSet<CharacterVerse>(new BcvCharacterEqualityComparer());
			IList<CharacterVerse> duplicateCharacterVerses = new List<CharacterVerse>();
			foreach (CharacterVerse cv in ControlCharacterVerseData.Singleton.GetAllQuoteInfo()
				.OrderBy(cv => cv.BcvRef).ThenBy(cv => string.IsNullOrEmpty(cv.Delivery)))
			{
				if (!uniqueCharacterVerses.Add(cv) && string.IsNullOrEmpty(cv.Delivery))
					duplicateCharacterVerses.Add(cv);
			}

			Assert.False(duplicateCharacterVerses.Any(),
				"Duplicate Character-Verse data:" +
				Environment.NewLine +
				duplicateCharacterVerses.Select(cv => cv.BcvRef + ", " + cv.Character).OnePerLineWithIndent());
		}

		[Test]
		public void DataIntegrity_AllCharacterIdsAndDefaultCharactersHaveCharacterDetail()
		{
			IEnumerable<string> charactersHavingDetail = CharacterDetailData.Singleton.GetAll().Select(d => d.Character);
			ISet<string> missingCharacters = new HashSet<string>();
			ISet<string> missingDefaultCharacters = new HashSet<string>();
			foreach (CharacterVerse cv in ControlCharacterVerseData.Singleton.GetAllQuoteInfo())
			{
				if (!charactersHavingDetail.Contains(cv.Character))
					missingCharacters.Add(cv.Character);
				if (!(string.IsNullOrEmpty(cv.DefaultCharacter) || charactersHavingDetail.Contains(cv.DefaultCharacter)))
					missingDefaultCharacters.Add(cv.DefaultCharacter);
			}
			Assert.False(missingCharacters.Any() || missingDefaultCharacters.Any(),
				"Characters in Character-Verse data but not in Character-Detail:" +
				Environment.NewLine +
				missingCharacters.OnePerLineWithIndent() +
				Environment.NewLine +
				"Default characters in Character-Verse data but not in Character-Detail:" +
				Environment.NewLine +
				missingDefaultCharacters.OnePerLineWithIndent());
		}
	}
}
