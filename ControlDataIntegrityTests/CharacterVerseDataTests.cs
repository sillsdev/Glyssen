using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Glyssen.Character;
using Glyssen.Properties;
using NUnit.Framework;
using SIL.Scripture;

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
			CharacterDetailData.TabDelimitedCharacterDetailData = Resources.CharacterDetail; //resets cache

			var charactersHavingDetail = CharacterDetailData.Singleton.GetAll().Select(d => d.CharacterId).ToList();
			ISet<string> missingCharacters = new SortedSet<string>();
			ISet<string> missingDefaultCharacters = new SortedSet<string>();
			foreach (CharacterVerse cv in ControlCharacterVerseData.Singleton.GetAllQuoteInfo())
			{
				if (!charactersHavingDetail.Contains(cv.Character))
				{
					if (CharacterVerseData.IsCharacterStandard(cv.Character) || cv.Character == CharacterVerseData.kNeedsReview)
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
					if (CharacterVerseData.IsCharacterStandard(cv.DefaultCharacter) || cv.DefaultCharacter == CharacterVerseData.kNeedsReview)
						continue;

					missingDefaultCharacters.Add(cv.DefaultCharacter);
				}
			}

			Assert.False(missingCharacters.Any() || missingDefaultCharacters.Any(),
				(missingCharacters.Any() ? "Characters in Character-Verse data but not in Character-Detail:" +
				Environment.NewLine +
				missingCharacters.OnePerLineWithIndent() : "") +
				(missingDefaultCharacters.Any() ? Environment.NewLine +
				"Default characters in Character-Verse data but not in Character-Detail:" +
				Environment.NewLine +
				missingDefaultCharacters.OnePerLineWithIndent() : ""));
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

		/// <summary>
		/// The Implicit quote type indicates that we expect the (whole) verse to be spoken by a particular character.
		/// Since this will be automatically applied to any verse that does not have any explicit quotes (i.e., is 
		/// identified by the quote parser as 100% "narrator"), this test ensures that we don't have any verses marked
		/// as Implicit which also include some other quote type. The one exception to this is that we do allow "Needs Review"
		/// as an implicit character, alongside other potential characters. This is used when we know a verse should be
		/// dramatized, but it is not necessarily clear who is speaking.
		/// </summary>
		[Test]
		public void DataIntegrity_ImplicitCharacterIsExclusiveUnlessItIsNeedsReviewOrTheOtherEntryIsAQuotationInDeuteronomy()
		{
			foreach (var cv in ControlCharacterVerseData.Singleton.GetAllQuoteInfo()
				.Where(i => i.QuoteType == QuoteType.Implicit && i.Character != CharacterVerseData.kNeedsReview))
			{
				var otherEntries = ControlCharacterVerseData.Singleton.GetCharacters(BCVRef.BookToNumber(cv.BookCode),
					cv.Chapter, cv.Verse, versification: ScrVers.English).Where(c => c != cv).ToList();
				if (otherEntries.Any())
				{
					Assert.IsTrue(cv.BookCode == "DEU" && otherEntries.All(o => 
						o.QuoteType == QuoteType.Quotation || o.QuoteType == QuoteType.Hypothetical ||
						o.QuoteType == QuoteType.Indirect),
					$"Character-verse file contains an Implicit quote for {cv.Character} in {cv.BookCode} {cv.Chapter}:{cv.Verse} " +
						"that also has other incompatible quotes.");
				}
			}
		}

		/// <summary>
		/// The Alternate quote type implies that there should also be (at least one) regular (not Indirect, Hypotthetical or Interruption)
		/// entry for the verse.
		/// </summary>
		[Test]
		public void DataIntegrity_AlternateAccompaniedByAnotherCharacter()
		{
			foreach (var alternate in ControlCharacterVerseData.Singleton.GetAllQuoteInfo()
				.Where(i => i.QuoteType == QuoteType.Alternate))
			{
				var otherEntries = ControlCharacterVerseData.Singleton.GetCharacters(BCVRef.BookToNumber(alternate.BookCode),
					alternate.Chapter, alternate.Verse, versification: ScrVers.English)
					.Where(c => c.QuoteType == QuoteType.Normal ||
					c.QuoteType == QuoteType.Potential ||
					c.QuoteType == QuoteType.Dialogue ||
					c.QuoteType == QuoteType.Quotation);
				Assert.IsTrue(otherEntries.Any(),
					$"Character-verse file contains an Alternate quote for {alternate.Character} in {alternate.BookCode} {alternate.Chapter}:{alternate.Verse}" +
					", but there is no primary character.");
			}
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
