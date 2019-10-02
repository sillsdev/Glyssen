using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Glyssen.Character;
using Glyssen.Properties;
using NUnit.Framework;
using SIL.Scripture;
using static System.String;

namespace ControlDataIntegrityTests
{
	[TestFixture]
	public class CharacterVerseDataTests
	{
		private const string kRegexBCV = "^(?<bookId>...)\t(?<chapter>\\d+)\t(?<verse>\\d+)(-(?<endVerse>\\d+))?\t";

		private IEnumerable<string> AllDataLines =>
			Resources.CharacterVerseData.Split(new[] {"\r", "\n"}, StringSplitOptions.RemoveEmptyEntries).Skip(1).Where(l => !l.StartsWith("#"));

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
			Regex regex = new Regex(kRegexBCV + "(?<character>[^\t]+)\t(?<delivery>[^\t]*)\t(?<alias>[^\t]*)\t(?<type>" + typeof(QuoteType).GetRegexEnumValuesString() + ")\t(?<defaultCharacter>[^\t]*)\t(?<parallelPassageRef>[^\t]*)$", RegexOptions.Compiled);
			Regex extraSpacesRegex = new Regex("^ |\t | \t| $", RegexOptions.Compiled);

			var set = new HashSet<string>();
			foreach (var line in AllDataLines)
			{
				var match = regex.Match(line);
				Assert.IsTrue(match.Success, "Failed to match line: " + line);

				var bookId = match.Result("${bookId}");
				var bookNum = BCVRef.BookToNumber(bookId);
				Assert.IsTrue(bookNum > 0, "Line: " + line);
				Assert.IsTrue(bookNum <= 66, "Line: " + line);

				var chapter = Int32.Parse(match.Result("${chapter}"));
				Assert.IsTrue(chapter > 0, "Line: " + line);
				Assert.IsTrue(chapter <= ScrVers.English.GetLastChapter(bookNum), "Line: " + line);

				var verse = Int32.Parse(match.Result("${verse}"));
				Assert.IsTrue(verse > 0 || verse == 0 && bookId == "PSA", "Line: " + line);
				Assert.IsTrue(verse <= ScrVers.English.GetLastVerse(bookNum, chapter), "Line: " + line);

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

				if (checkCharacters && !allParallelPassageData.Any(p => (p.BookCode != cv.BookCode || p.Chapter != cv.Chapter  || p.Verse != cv.Verse) &&
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
		/// as Implicit which also include some other quote type. The four exceptions to this are:
		/// * We do allow "Needs Review" as an implicit character, alongside other potential characters. This is used when we know a verse should be
		/// dramatized, but it is not necessarily clear who is speaking.
		/// * We do allow Hypothetical characters (which often occur in the speech and are sometimes dramatized) 
		/// * We allow self-quotations (as these help us determine when *not* to mark a block as Needs Review)
		/// * In Deuteronomy, we also allow Quotations and Indirect speech
		/// </summary>
		[Test]
		public void DataIntegrity_ImplicitCharacterIsExclusiveUnlessItIsNeedsReviewOrTheOtherEntryIsAQuotationInDeuteronomy()
		{
			foreach (var cv in ControlCharacterVerseData.Singleton.GetAllQuoteInfo()
				.Where(i => i.QuoteType == QuoteType.Implicit && i.Character != CharacterVerseData.kNeedsReview))
			{
				var otherEntries = ControlCharacterVerseData.Singleton.GetCharacters(BCVRef.BookToNumber(cv.BookCode),
					cv.Chapter, cv.Verse, versification: ScrVers.English)
					.Where(c => c != cv && c.QuoteType != QuoteType.Hypothetical &&
					(c.QuoteType != QuoteType.Quotation || c.Character != cv.Character)).ToList();
				if (otherEntries.Any())
				{
					Assert.IsTrue(cv.BookCode == "DEU" && otherEntries.All(o => 
						o.QuoteType == QuoteType.Quotation || o.QuoteType == QuoteType.Indirect),
					$"Character-verse file contains an Implicit quote for {cv.Character} in {cv.BookCode} {cv.Chapter}:{cv.Verse} " +
						"that also has other incompatible quotes.");
				}
			}
		}

		/// <summary>
		/// A Scripture quote that is either explicitly defaulted to God or has no default (which will typically
		/// appear in FCBH's Director's Guide as a line to be spoken by God) should not exist alongside (i.e., for
		/// the same verse) an explicit entry for God. The reason this is critical is that the ReferenceTextUtility
		/// expects to be able to find exactly one reliable match, but if the DG has God, and the control file has
		/// both God and scripture, that will result in two equally reliable matches.
		/// </summary>
		[Test]
		public void DataIntegrity_ScriptureQuoteDefaultedToGodOrWithoutDefaultDoesNotCoOccurWithEntryForGod()
		{
			foreach (var cv in ControlCharacterVerseData.Singleton.GetAllQuoteInfo()
				.Where(q => q.QuoteType == QuoteType.Quotation && q.Character == CharacterVerse.kScriptureCharacter &&
					(IsNullOrEmpty(q.DefaultCharacter) || q.DefaultCharacter == "God")))
			{
				Assert.IsFalse(ControlCharacterVerseData.Singleton.GetCharacters(BCVRef.BookToNumber(cv.BookCode),
						cv.Chapter, cv.Verse, versification: ScrVers.English).Any(c => c.Character == "God"),
						$"Character-verse file contains an scripture quote that defaults to God in {cv.BookCode} {cv.Chapter}:{cv.Verse}, " +
						"but that verse also has God speaking directly.");
			}
		}

		/// <summary>
		/// The Alternate quote type implies that there should also be (at least one) regular (not Indirect or Interruption)
		/// entry for the verse. At least for now (until we find some place where it's definitely warranted), we also won't allow Dialogue
		/// to be the primary character (since hopefully, that's more concrete). And it can't be a narrator quotation. Also in the NT, the
		/// reference text typically doesn't dramatize Hypotheticals, so for NT books, we won't allow those to be the primary either.
		/// While it might seem that Implicit should not be allowed to be primary, it can be because Alternate is typically used in
		/// places such as prophecy where we know exactly who's talking, but it really could be dramatized using either the primary or the
		/// secondary character. By making the primary character implicit, we ensure that the script gets created with a useful default
		/// but still allow the user to override it if desired.
		/// </summary>
		[Test]
		public void DataIntegrity_AlternateAccompaniedByAnotherCharacter()
		{
			var acceptablePrimaryQuoteTypes = new List<QuoteType>
			{
				QuoteType.Normal,
				QuoteType.Potential,
				QuoteType.Quotation,
				QuoteType.Hypothetical,
				QuoteType.Implicit
			};
			foreach (var alternate in ControlCharacterVerseData.Singleton.GetAllQuoteInfo()
				.Where(i => i.QuoteType == QuoteType.Alternate))
			{
				if (alternate.BookCode == "MAT" && acceptablePrimaryQuoteTypes.Count == 5)
					acceptablePrimaryQuoteTypes.Remove(QuoteType.Hypothetical);
				var otherEntries = ControlCharacterVerseData.Singleton.GetCharacters(BCVRef.BookToNumber(alternate.BookCode),
						alternate.Chapter, alternate.Verse, versification: ScrVers.English)
					.Where(c => acceptablePrimaryQuoteTypes.Contains(c.QuoteType));
				Assert.IsFalse(otherEntries.Any(c => c.Character == alternate.Character && c.Delivery == alternate.Delivery),
					$"Alternate used for a {alternate.Character} in {alternate.BookCode} {alternate.Chapter}:{alternate.Verse}, " +
					"but that character also has another quote type in that verse!");
				Assert.IsTrue(otherEntries.Any(c => c.QuoteType != QuoteType.Quotation || !c.Character.StartsWith("narrator-")),
					$"Character-verse file contains an Alternate quote for {alternate.Character} in {alternate.BookCode} {alternate.Chapter}:{alternate.Verse}" +
					", but there is no primary character.");
			}
		}

		/// <summary>
		/// Although the class that reads and uses the data in this file does not actually care what order the lines are in,
		/// for ease of maintenance, we keep it in canonical order. If any verse or chapter numbers appear out of order, it
		/// is most likely a typo.
		/// </summary>
		[Test]
		public void DataIntegrity_AscendingCanonicalOrder()
		{
			Regex regex = new Regex(kRegexBCV, RegexOptions.Compiled);
			int prevBookNum = 1;
			int prevChapterNum = 1;
			int prevVerseNum = 1;

			foreach (var line in AllDataLines)
			{
				var match = regex.Match(line);
				Assert.IsTrue(match.Success, "Failed to match line: " + line);

				var bookId = match.Result("${bookId}");
				var bookNum = BCVRef.BookToNumber(bookId);
				if (prevBookNum < bookNum)
				{
					prevChapterNum = 1;
					prevVerseNum = bookId == "PSA" ? 0 : 1;
				}
				else
					Assert.AreEqual(prevBookNum, bookNum, "Book out of order" + Environment.NewLine + line);

				var chapter = Int32.Parse(match.Result("${chapter}"));
				if (prevChapterNum < chapter)
					prevVerseNum = bookId == "PSA" ? 0 : 1;
				else
					Assert.AreEqual(prevChapterNum, chapter, "Chapter out of order" + Environment.NewLine + line);

				var verse = Int32.Parse(match.Result("${verse}"));
				Assert.IsTrue(verse >= prevVerseNum, "Verse out of order" + Environment.NewLine + line);
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
