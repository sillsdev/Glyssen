using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using SIL.Scripture;
using Waxuquerque.Character;
using Waxuquerque.Properties;

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

			ControlCharacterVerseData.ReadHypotheticalAsNarrator = false;
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			ControlCharacterVerseData.ReadHypotheticalAsNarrator = true;
		}

		[Test]
		public void DataIntegrity_ValidControlVersionPresent()
		{
			Assert.IsTrue(Regex.IsMatch(Resources.CharacterVerseData, @"\AControl File Version\t\d+\r?$", RegexOptions.Multiline));
		}

		[Test]
		public void DataIntegrity_RequiredFieldsHaveValidFormatAndThereAreNoDuplicateLines()
		{
			Regex regex = new Regex("^(?<bookId>...)\t(?<chapter>\\d+)\t(?<verse>\\d+)(-(?<endVerse>\\d+))?\t(?<character>[^\t]+)\t(?<delivery>[^\t]*)\t(?<alias>[^\t]*)\t(?<type>" + typeof(QuoteType).GetRegexEnumValuesString() + ")\t(?<defaultCharacter>[^\t]*)\t(?<parallelPassageRef>[^\t]*)$", RegexOptions.Compiled);
			Regex extraSpacesRegex = new Regex("^ |\t | \t| $", RegexOptions.Compiled);
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

				var extraSpacesMatch = extraSpacesRegex.Match(line);
				Assert.IsFalse(extraSpacesMatch.Success, "Line with extra space(s): " + line);
			}
		}

		[TestCase(true)]
		[TestCase(false)]
		public void DataIntegrity_NoDuplicateData(bool readHypotheticalAsNarrator)
		{
			ControlCharacterVerseData.ReadHypotheticalAsNarrator = readHypotheticalAsNarrator;

			try
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
			finally
			{
				ControlCharacterVerseData.ReadHypotheticalAsNarrator = false;
			}
		}

		[TestCase(true)]
		[TestCase(false)]
		public void DataIntegrity_NoDuplicateWhereOnlyDifferenceIsNormalVsNonnormalDelivery(bool readHypotheticalAsNarrator)
		{
			ControlCharacterVerseData.ReadHypotheticalAsNarrator = readHypotheticalAsNarrator;

			try
			{
				// PG-152: Currently, the program does not handle duplicates where the
				// only difference is between normal (blank) delivery and a specified delivery
				ISet<CharacterVerse> uniqueCharacterVerses = new HashSet<CharacterVerse>(new BcvCharacterAndTypeEqualityComparer());
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
			finally
			{
				ControlCharacterVerseData.ReadHypotheticalAsNarrator = false;
			}
		}

		[Test]
		public void DataIntegrity_NoAliasEqualToCharacterId()
		{
			List<CharacterVerse> entriesWhereAliasEqualsCharacterId = new List<CharacterVerse>();
			foreach (CharacterVerse cv in ControlCharacterVerseData.Singleton.GetAllQuoteInfo())
			{
				if (cv.Alias == cv.Character)
					entriesWhereAliasEqualsCharacterId.Add(cv);
			}

			Assert.False(entriesWhereAliasEqualsCharacterId.Any(),
				"Character-Verse data where Alias equals Character ID:" +
				Environment.NewLine +
				entriesWhereAliasEqualsCharacterId.Select(cv => cv.BcvRef + ", " + cv.Character + ", " + cv.Alias).OnePerLineWithIndent());
		}

		[Test]
		public void DataIntegrity_AllCharacterIdsAndDefaultCharactersHaveCharacterDetail()
		{
			var charactersHavingDetail = CharacterDetailData.Singleton.GetAll().Select(d => d.CharacterId).ToList();
			ISet<string> missingCharacters = new SortedSet<string>();
			ISet<string> missingDefaultCharacters = new SortedSet<string>();
			foreach (CharacterVerse cv in ControlCharacterVerseData.Singleton.GetAllQuoteInfo())
			{
				if (!charactersHavingDetail.Contains(cv.Character))
				{
					if (CharacterVerseData.IsCharacterStandard(cv.Character))
						continue;

					var characters = cv.Character.Split('/');
					if (characters.Length > 1)
					{
						foreach (var character in characters.Where(character => !charactersHavingDetail.Contains(character)))
							missingCharacters.Add(character);
					}
					else
						missingCharacters.Add(cv.Character);
				}
				if (!(string.IsNullOrEmpty(cv.DefaultCharacter) || charactersHavingDetail.Contains(cv.DefaultCharacter)))
				{
					if (CharacterVerseData.IsCharacterStandard(cv.DefaultCharacter))
						continue;

					missingDefaultCharacters.Add(cv.DefaultCharacter);
				}
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

		[Test]
		public void DataIntegrity_ParallelPassageReferences()
		{
			var referenceDoesntMatchLineFailures = new List<string>();
			var charactersNotEqualFailures = new List<string>();

			var allParallelPassageData = ControlCharacterVerseData.Singleton.GetAllQuoteInfo().Where(c => !string.IsNullOrEmpty(c.ParallelPassageReferences));
			foreach (CharacterVerse cv in allParallelPassageData)
			{
				bool checkCharacters = !cv.ParallelPassageReferences.StartsWith("*");
				ISet<BCVRef> parallelPassageVersesForCurrentDatum = new HashSet<BCVRef>();
				foreach (var reference in cv.ParallelPassageReferences.Split(';'))
				{
					if (reference.StartsWith("*"))
						continue;
					var verseRefs = new VerseRef(reference.Trim().Replace(".", ":"), ScrVers.English).AllVerses();
					foreach (var verseRef in verseRefs)
						parallelPassageVersesForCurrentDatum.Add(new BCVRef(verseRef.BBBCCCVVV));
				}

				if (!parallelPassageVersesForCurrentDatum.Contains(cv.BcvRef))
					referenceDoesntMatchLineFailures.Add(string.Format("{0}  =>  {1}", cv.BcvRef, cv.ParallelPassageReferences));

				if (checkCharacters && !allParallelPassageData.Any(p => p.BookCode != cv.BookCode &&
					(p.Character == cv.Character || p.Character == cv.DefaultCharacter || p.DefaultCharacter == cv.Character) &&
					parallelPassageVersesForCurrentDatum.Contains(p.BcvRef)))
					charactersNotEqualFailures.Add(string.Format("{0}  =>  {1}  =>  {2}", cv.BcvRef, cv.Character, cv.ParallelPassageReferences));
			}

			Assert.IsTrue(!referenceDoesntMatchLineFailures.Any(), "Parallel passage reference does not match the reference for this line:" + Environment.NewLine +
				referenceDoesntMatchLineFailures.OnePerLineWithIndent());
			Assert.IsTrue(!charactersNotEqualFailures.Any(), "Characters do not match for one or more parallel passages:" + Environment.NewLine +
				charactersNotEqualFailures.OnePerLineWithIndent());
		}

		private class BcvCharacterAndTypeEqualityComparer : IEqualityComparer<CharacterVerse>
		{
			public bool Equals(CharacterVerse x, CharacterVerse y)
			{
				return x.BcvRef.Equals(y.BcvRef) && x.Character.Equals(y.Character) &&
					x.QuoteType == y.QuoteType;
			}

			public int GetHashCode(CharacterVerse obj)
			{
				unchecked
				{
					int hashCode = obj.BcvRef != null ? obj.BcvRef.GetHashCode() : 0;
					hashCode = (hashCode * 397) ^ (obj.Character?.GetHashCode() ?? 0);
					return hashCode;
				}
			}
		}
	}
}
