using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Glyssen.Shared;
using GlyssenCharacters;
using NUnit.Framework;
using SIL.Extensions;
using SIL.Scripture;
using static System.Int32;
using static System.String;
using NarratorOverrides = GlyssenCharacters.NarratorOverrides;
using Resources = GlyssenCharacters.Properties.Resources;

namespace ControlDataIntegrityTests
{
	[TestFixture]
	public class CharacterVerseDataTests
	{
		private const string kRegexBCV = "^(?<bookId>...)\t(?<chapter>\\d+)\t(?<verse>\\d+)(-(?<endVerse>\\d+))?\t";

		private IEnumerable<string> AllDataLines =>
			Resources.CharacterVerseData.Split(new[] {"\r", "\n"}, StringSplitOptions.RemoveEmptyEntries).Skip(1).Where(l => !l.StartsWith("#"));

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			// Fixes issue where other test project was interfering with the running of this one (by setting the data to test data).
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = null;
		}

		[Test]
		public void DataIntegrity_ValidControlVersionPresent()
		{
			Assert.That(Regex.IsMatch(Resources.CharacterVerseData,
				@"\AControl File Version\t\d+\r?$", RegexOptions.Multiline));
		}

		[Test]
		public void DataIntegrity_RequiredFieldsHaveValidFormatAndThereAreNoDuplicateLines()
		{
			Regex regex = new Regex(kRegexBCV + "(?<character>[^\t]+)\t(?<delivery>[^\t]*)\t(?<alias>[^\t]*)\t(?<type>" + typeof(QuoteType).GetRegexEnumValuesString() + ")\t(?<defaultCharacter>[^\t]*)\t(?<parallelPassageRef>[^\t]*)(\t(?<quotePosition>[^\t]*))?$", RegexOptions.Compiled);
			Regex extraSpacesRegex = new Regex("^ |\t | \t| $", RegexOptions.Compiled);

			var set = new HashSet<string>();
			ISet<CharacterVerse> uniqueCharacterVerses = new HashSet<CharacterVerse>();
			foreach (var line in AllDataLines)
			{
				var match = regex.Match(line);
				Assert.That(match.Success, Is.True, "Failed to match line: " + line);

				var bookId = match.Result("${bookId}");
				var bookNum = BCVRef.BookToNumber(bookId);
				Assert.That(bookNum, Is.GreaterThan(0), "Line: " + line);
				Assert.That(bookNum, Is.LessThanOrEqualTo(66), "Line: " + line);

				var chapterAsString = match.Result("${chapter}");
				var chapter = Parse(chapterAsString);
				Assert.That(chapter, Is.GreaterThan(0), "Line: " + line);
				Assert.That(chapter, Is.LessThanOrEqualTo(ScrVers.English.GetLastChapter(bookNum)),
					"Line: " + line);

				var verseAsString = match.Result("${verse}");
				var verse = Parse(verseAsString);
				Assert.That(verse > 0 || verse == 0 && bookId == "PSA", Is.True, "Line: " + line);
				Assert.That(verse, Is.LessThanOrEqualTo(ScrVers.English.GetLastVerse(bookNum, chapter)),
					"Line: " + line);

				var sEndVerse = match.Result("${endVerse}");
				if (!IsNullOrEmpty(sEndVerse))
				{
					var endVerse = Parse(sEndVerse);
					Assert.That(endVerse, Is.GreaterThan(verse), "Line: " + line);
					Assert.That(endVerse, Is.LessThanOrEqualTo(152), "Line: " + line);
				}

				var character = match.Result("${character}");

				var alias = match.Result("${alias}");
				bool aliasDefined = !IsNullOrEmpty(alias);
				if (aliasDefined)
					Assert.That(character, Is.Not.EqualTo(alias), "Line: " + line);

				var defaultCharacter = match.Result("${defaultCharacter}");
				if (!IsNullOrEmpty(defaultCharacter))
				{
					Assert.That(character, Is.Not.EqualTo(defaultCharacter), "Line: " + line);
					Assert.That(defaultCharacter, Does.Not.Contain("/"),
						$"Line: {line} has a default character which is a multi-character ID.");
				}

				string typeAsString = match.Result("${type}");
				if (CharacterVerseData.IsCharacterOfType(character, CharacterVerseData.StandardCharacter.Narrator))
				{
					Assert.That("Dialogue", Is.Not.EqualTo(typeAsString), "Line: " + line);
					Assert.That(aliasDefined, Is.False, "Line: " + line);
				}

				var matchResult = match.Result("$&");
				Assert.That(set.Add(matchResult), Is.True, "Duplicate line: " + matchResult);

				Assert.That(Enum.TryParse(typeAsString, out QuoteType type), Is.True);
				foreach (var bcvRef in CharacterVerseData.GetAllVerses(new [] {bookId, chapterAsString, verseAsString}, () => throw new Exception("This should never happen")))
				{
					var cv = new CharacterVerse(bcvRef, character, match.Result("${delivery}"), alias, false, type, defaultCharacter);
					Assert.That(uniqueCharacterVerses.Add(cv), Is.True,
						"Line is equivalent to another line even though they are not identical: " + matchResult);
				}

				var sPosition = match.Result("${quotePosition}");
				if (!IsNullOrEmpty(sPosition))
				{
					Assert.That(Enum.TryParse(sPosition, out QuotePosition position), Is.True, "Invalid QuotePosition: " + sPosition);
					if (type == QuoteType.Implicit || type == QuoteType.ImplicitWithPotentialSelfQuote)
						Assert.That(position, Is.EqualTo(QuotePosition.EntireVerse), "Line: " + line);

					Assert.That(type.IsOneOf(QuoteType.Potential, QuoteType.Rare, QuoteType.Alternate,
						QuoteType.Indirect, QuoteType.Interruption), Is.False, "Line: " + line);
				}
				else
				{
					// For consistency and simplicity, we require that the quote position be set to
					// EntireVerse for Implicit quotes (even though it could be unambiguously
					// inferred from the quote type).
					Assert.That(type, Is.Not.EqualTo(QuoteType.Implicit).And
						.Not.EqualTo(QuoteType.ImplicitWithPotentialSelfQuote), "Line: " + line);
				}

				var extraSpacesMatch = extraSpacesRegex.Match(line);
				Assert.That(extraSpacesMatch.Success, Is.False, "Line with extra space(s): " + line);
			}
		}

		[Test]
		public void DataIntegrity_NoDuplicateWhereOnlyDifferenceIsNormalVsNonNormalDelivery()
		{
			// PG-152/PG-1272: Glyssen now handles duplicates where the only difference is
			// between normal (blank) delivery and a specified delivery, but if the same
			// character speaks twice in the same verse with two different deliveries, it's
			// still generally best to make them both explicit. We allow for the exception
			// of an Alternate (where the main speaker is allowed to speak the quotation
			// rather than having it spoken by the character being quoted), because in this
			// case the delivery is as much an informational note to the scripter as it is
			// a delivery for the recording team to attend to.
			IEqualityComparer <CharacterVerse> comparer = new BcvCharacterEqualityComparer();
			ISet<CharacterVerse> uniqueCharacterVerses = new HashSet<CharacterVerse>(comparer);
			IList<CharacterVerse> duplicateCharacterVerses = new List<CharacterVerse>();
			foreach (CharacterVerse cv in ControlCharacterVerseData.Singleton.GetAllQuoteInfo()
				.OrderBy(cv => cv.BcvRef).ThenBy(cv => IsNullOrEmpty(cv.Delivery)))
			{
				if (!uniqueCharacterVerses.Add(cv) && IsNullOrEmpty(cv.Delivery))
				{
					if (uniqueCharacterVerses.Single(c => comparer.Equals(c, cv)).QuoteType != QuoteType.Alternate)
						duplicateCharacterVerses.Add(cv);
				}
			}

			Assert.That(duplicateCharacterVerses, Is.Empty,
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

			Assert.That(entriesWhereAliasEqualsCharacterId, Is.Empty,
				"Character-Verse data where Alias equals Character ID:" +
				Environment.NewLine +
				entriesWhereAliasEqualsCharacterId.Select(cv => cv.BcvRef + ", " + cv.Character + ", " + cv.Alias).OnePerLineWithIndent());
		}

		[Test]
		public void DataIntegrity_NoExtraBiblicalCharacterIds()
		{
			foreach (CharacterVerse cv in ControlCharacterVerseData.Singleton.GetAllQuoteInfo())
			{
				Assert.That(CharacterVerseData.IsCharacterExtraBiblical(cv.Character), Is.False,
					$"Character-Verse data cannot contain extra-biblical characters: {cv.BcvRef}: {cv.Character}");
			}
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
					// For interruptions, we actually never pay any attention to the character ID. They are always narrator.
					// But in some verses we have both a normal narrator Quotation and an interruption, so to avoid a problem
					// in other data integrity checks, we just use the dummy character id "interruption-XXX", where XXX is the
					// 3-letter book code.
					if (cv.QuoteType == QuoteType.Interruption || cv.Character.StartsWith("interruption-"))
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

				if (!(IsNullOrEmpty(cv.DefaultCharacter) || charactersHavingDetail.Contains(cv.DefaultCharacter)))
				{
					if (CharacterVerseData.IsCharacterStandard(cv.DefaultCharacter) || cv.DefaultCharacter == CharacterVerseData.kNeedsReview)
						continue;

					missingDefaultCharacters.Add(cv.DefaultCharacter);
				}
			}

			Assert.That(missingCharacters.Any() || missingDefaultCharacters.Any(), Is.False,
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
			var referenceDoesNotMatchLineFailures = new List<string>();
			var charactersNotEqualFailures = new List<string>();

			var allParallelPassageData = ControlCharacterVerseData.Singleton.GetAllQuoteInfo().Where(c => !IsNullOrEmpty(c.ParallelPassageReferences));
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
					referenceDoesNotMatchLineFailures.Add($"{cv.BcvRef}  =>  {cv.ParallelPassageReferences}");

				if (checkCharacters && !allParallelPassageData.Any(p => (p.BookCode != cv.BookCode || p.Chapter != cv.Chapter || p.Verse != cv.Verse) &&
					(p.Character == cv.Character || p.Character == cv.DefaultCharacter || p.DefaultCharacter == cv.Character) &&
					parallelPassageVersesForCurrentDatum.Contains(p.BcvRef)))
					charactersNotEqualFailures.Add($"{cv.BcvRef}  =>  {cv.Character}  =>  {cv.ParallelPassageReferences}");
			}

			Assert.That(referenceDoesNotMatchLineFailures, Is.Empty,
				"Parallel passage reference does not match the reference for this line:" + Environment.NewLine +
				referenceDoesNotMatchLineFailures.OnePerLineWithIndent());
			Assert.That(charactersNotEqualFailures, Is.Empty,
				"Characters do not match for one or more parallel passages:" + Environment.NewLine +
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
				.Where(i => i.IsImplicit && i.Character != CharacterVerseData.kNeedsReview))
			{
				var otherEntries = ControlCharacterVerseData
					.GetOtherEntriesIncompatibleWithImplicitCv(cv).ToList();
				if (otherEntries.Any())
				{
					Assert.That(cv.BookCode == "DEU" && otherEntries.All(o => 
						o.QuoteType == QuoteType.Quotation || o.QuoteType == QuoteType.Indirect), Is.True,
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
				.Where(q => q.QuoteType == QuoteType.Quotation && q.Character == CharacterSpeakingMode.kScriptureCharacter &&
					(IsNullOrEmpty(q.DefaultCharacter) || q.DefaultCharacter == "God")))
			{
				Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(BCVRef.BookToNumber(cv.BookCode),
					cv.Chapter, new SingleVerse(cv.Verse), ScrVers.English).Where(c => c.Character == "God"),
					Is.Empty,
					$"Character-verse file contains a Scripture quote that defaults to God in {cv.BookCode} {cv.Chapter}:{cv.Verse}, " +
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
				QuoteType.Implicit,
				QuoteType.ImplicitWithPotentialSelfQuote
			};
			var numberOfAcceptableQuoteTypesInOT = acceptablePrimaryQuoteTypes.Count;
			foreach (var alternate in ControlCharacterVerseData.Singleton.GetAllQuoteInfo()
				.Where(i => i.QuoteType == QuoteType.Alternate))
			{
				if (alternate.BookCode == "MAT" && acceptablePrimaryQuoteTypes.Count == numberOfAcceptableQuoteTypesInOT)
					acceptablePrimaryQuoteTypes.Remove(QuoteType.Hypothetical);
				var otherEntries = ControlCharacterVerseData.Singleton.GetCharacters(BCVRef.BookToNumber(alternate.BookCode),
						alternate.Chapter, new SingleVerse(alternate.Verse), ScrVers.English)
					.Where(c => acceptablePrimaryQuoteTypes.Contains(c.QuoteType)).ToList();
				Assert.That(otherEntries.Where(c => c.Character == alternate.Character && c.Delivery == alternate.Delivery), Is.Empty,
					$"Alternate used for a {alternate.Character} in {alternate.BookCode} {alternate.Chapter}:{alternate.Verse}, " +
					"but that character also has another quote type in that verse!");
				Assert.That(otherEntries.Any(c => c.QuoteType != QuoteType.Quotation || !c.Character.StartsWith("narrator-") ||
						// PG-1248: Because of the logic in AdjustData, this Alternate could be a Quotation that was turned into
						// an Alternate because there was a corresponding narrator Quotation that should be considered as primary.
						// If so, we don't want to flag this as a mistake.
						c.Character == alternate.DefaultCharacter), Is.True,
					$"Character-verse file contains an Alternate quote for {alternate.Character} in {alternate.BookCode} {alternate.Chapter}:{alternate.Verse}" +
					", but there is no primary character.");
			}
		}

		/// <summary>
		/// Some "quotations" are strictly narrator. (This is where the narrator is saying something that is a "quote" of
		/// speech that "no one" said, a translation, a foreign phrase, a title, or a literal name. Any non-narrator
		/// quotation should also be accompanied by the option to have the narrator (or the active first-level normal speaker)
		/// speak the quoted text because it is typically not desirable to have the original speaker re-speak the quote
		/// (though it sometimes can be for dramatic effect).
		/// This would not be strictly required in places where the Director's Guide does dramatize the quotation if we just
		/// wanted to let it be assigned with no review needed, but these are rare enough that it is probably better to
		/// make the scripter look at them.
		/// </summary>
		[Test]
		public void DataIntegrity_QuotationAccompaniedByNarratorOrActiveSpeaker()
		{
			var problems = new List<string>();
			foreach (var quote in ControlCharacterVerseData.Singleton.GetAllQuoteInfo()
				.Where(cv => cv.QuoteType == QuoteType.Quotation && cv.Character != "scripture" &&
				!CharacterVerseData.IsCharacterOfType(cv.Character, CharacterVerseData.StandardCharacter.Narrator)))
			{
				if (!ControlCharacterVerseData.Singleton.GetCharacters(BCVRef.BookToNumber(quote.BookCode),
						quote.Chapter, new SingleVerse(quote.Verse), ScrVers.English, true)
					.Any(c => CharacterVerseData.IsCharacterOfType(c.Character, CharacterVerseData.StandardCharacter.Narrator) ||
					c.QuoteType == QuoteType.Normal ||
					c.QuoteType == QuoteType.Dialogue ||
					c.QuoteType == QuoteType.Alternate ||
					c.QuoteType == QuoteType.Implicit ||
					c.QuoteType == QuoteType.ImplicitWithPotentialSelfQuote ||
					c.QuoteType == QuoteType.Potential))
				{
					// Just passing true in the above call to GetCharacters to grab the narrator overrides is insufficient
					// because there are places (e.g., PSA 95:10) where the narrator override character has self-quotes with
					// no specified delivery, in which case the narrator override is not included as a separate result.
					var verse = new VerseRef(BCVRef.BookToNumber(quote.BookCode), quote.Chapter, quote.Verse, ScrVers.English);
					if (!NarratorOverrides.GetCharacterOverrideDetailsForRefRange(verse, quote.Verse).Any())
						problems.Add($"{quote.BookCode}\t{quote.Chapter}\t{quote.Verse}\t{quote.Character}");
				}
			}

			Assert.That(problems, Is.Empty,
				"Character-verse file contains the following Quotation with no accompanying narrator option:" +
				Environment.NewLine + Join(Environment.NewLine, problems));
		}

		/// <summary>
		/// The point of the Hypothetical quote type is to indicate something someone or something (personified) might think or
		/// say but which they didn't. The Quote Parser treats these as Narrator Quotations unless the reference text actually
		/// has the hypothetical character speaking (which pretty much never happens in the NT, but it does in the OT). However,
		/// the hypothetical character is still available as a choice in Identify Speaking Parts. We used to allow the Narrator
		/// to be listed as the speaker of the hypothetical speech, but since the narrator typically does speak in verses where
		/// there is hypothetical speech, this totally defeated the purpose of this special logic in verses where the only
		/// expected speech was by the narrator. In those cases, we really do want the speech to be spoken by the narrator. We
		/// could accomplish this by having a "no one" character listed as the hypothetical speaker, but this is unnecessary
		/// and would result in "no one" appearing as a choice in ISP. So this test ensures that we do not allow this illogical
		/// combination.
		/// </summary>
		[Test]
		public void DataIntegrity_HypotheticalNarratorQuoteNotPermitted()
		{
			var hypotheticalNarrators = ControlCharacterVerseData.Singleton.GetAllQuoteInfo().Where(i => i.QuoteType == QuoteType.Hypothetical && i.Character.StartsWith("narrator-")).ToList();
			Assert.That(hypotheticalNarrators, Is.Empty,
				"Hypothetical narrator quotes are not permitted:" + Environment.NewLine +
				Join(Environment.NewLine, hypotheticalNarrators.Select(cv => $"{cv.BcvRef.AsString}")));
		}

		/// <summary>
		/// Although the narrator does need to read with good expression, particularly in poetic and prophetic books where
		/// the narrator is sometimes also the author. Glyssen does not display or save deliveries along with the narrator
		/// character. Therefore, it makes no sense to have these in the control file. Presumably, the narrator will have
		/// a pretty good idea of the context and required tone since he is recording pretty much the whole text. In any
		/// case, the deliveries are only of minimal value and no substitute for good coaching during the recording and
		/// good feedback during the review process.
		/// </summary>
		[Test]
		public void DataIntegrity_NarratorCannotHaveDelivery()
		{
			var narratorsWithDelivery = ControlCharacterVerseData.Singleton.GetAllQuoteInfo().Where(i => i.Character.StartsWith("narrator-") &&
				!IsNullOrEmpty(i.Delivery)).ToList();
			Assert.That(narratorsWithDelivery, Is.Empty,
				"Narrator quotes are not permitted to have delivery info:" + Environment.NewLine +
				Join(Environment.NewLine, narratorsWithDelivery.Select(cv => $"{cv.BcvRef.AsString}")));
		}
		 
		/// <summary>
		/// The Rare quote type must always be accompanied by a Needs Review entry, because this type of quote nearly always requires
		/// input from a native speaker to know whether to dramatize it as actual spoken words (or thinking out loud) or treat
		/// it as "hypothetical" speech that should be read by the narrator to convey an attitude, thought or belief of a character.
		/// In many cases review may also be needed to determine which character is thinking or speaking the quoted text. Furthermore,
		/// if all the possible quotes in the verse are Rare, then the Needs Review should be of type Potential, so that it will be
		/// assigned automatically to any quoted text found. However, if the verse has a mix of Rare and non-Rare quotes, then the
		/// Needs Review entry should itself be marked as Rare (at least until we find a plausible exception) if there is exactly
		/// one such entry, so that it would not cause a needless ambiguity in the most common case where the only quoted text in
		/// the verse is the expected (i.e., non-rare) text. If there is an extra bit of quoted text, it may well be automatically
		/// assigned to the expected speaker, but this should not normally be a problem because it will not align to the reference
		/// text, so the user will have to look at it anyway, and can at that point assign it correctly to the Rare character or mark
		/// it as needing review. If there is more than one non-rare entry in addition to the obligatory Needs Review entry, then
		/// most likely the Needs Review entry will be of type Potential, but either way there will be an ambiguity, so we can trust
		/// the data analyst to choose the most appropriate type.
		/// </summary>
		[Test]
		public void DataIntegrity_RareMustBeAccompaniedByNeedsReview()
		{
			foreach (var rare in ControlCharacterVerseData.Singleton.GetAllQuoteInfo()
				.Where(i => i.QuoteType == QuoteType.Rare))
			{
				var nonRare = ControlCharacterVerseData.Singleton.GetCharacters(BCVRef.BookToNumber(rare.BookCode),
					rare.Chapter, new SingleVerse(rare.Verse), ScrVers.English, true)
					.Where(c => c.QuoteType != QuoteType.Rare || c.Character == CharacterVerseData.kNeedsReview).ToList();
				try
				{
					var needsReviewQuoteType = nonRare.Single(c => c.Character == CharacterVerseData.kNeedsReview).QuoteType;
					var anticipatedCharacterEntries = nonRare.Count(c => c.Character != CharacterVerseData.kNeedsReview);
					switch (anticipatedCharacterEntries)
					{
						case 0:
							Assert.That(needsReviewQuoteType, Is.EqualTo(QuoteType.Potential),
								$"Character-verse file contains a Rare quote for {rare.Character} in " +
								$"{rare.BookCode} {rare.Chapter}:{rare.Verse}, and there are no non-rare " +
								$"entries, so the corresponding {CharacterVerseData.kNeedsReview} " +
								"entry for that verse should be of type Potential.");
							break;
						case 1:
							Assert.That(needsReviewQuoteType, Is.EqualTo(QuoteType.Rare),
								"Character-verse file contains a Rare quote " +
								$"for {rare.Character} in {rare.BookCode} {rare.Chapter}:{rare.Verse} along with 1 " +
								$"non-rare entry, so the corresponding {CharacterVerseData.kNeedsReview} entry for that " +
								"verse should be of type Rare to prevent an unnecessary ambiguity.");
							break;
						default:
							Assert.That(needsReviewQuoteType, Is.AnyOf(QuoteType.Rare, QuoteType.Potential),
								$"Character-verse file contains a Needs Review entry in {rare.BookCode} {rare.Chapter}:" +
								$"{rare.Verse} that is neither Rare nor Potential. If there is a valid reason, this check can " +
								"be removed from this test.");
							break;
					}
				}
				catch (InvalidOperationException)
				{
					Assert.Fail($"Character-verse file contains a Rare quote for {rare.Character} in {rare.BookCode} {rare.Chapter}:" +
						$"{rare.Verse}, but there is no corresponding {CharacterVerseData.kNeedsReview} character in that verse.");
				}
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
				Assert.That(match.Success, Is.True, "Failed to match line: " + line);

				var bookId = match.Result("${bookId}");
				var bookNum = BCVRef.BookToNumber(bookId);
				if (prevBookNum < bookNum)
				{
					prevChapterNum = 1;
					prevVerseNum = bookId == "PSA" ? 0 : 1;
				}
				else
					Assert.That(prevBookNum, Is.EqualTo(bookNum), "Book out of order" + Environment.NewLine + line);

				var chapter = Parse(match.Result("${chapter}"));
				if (prevChapterNum < chapter)
					prevVerseNum = bookId == "PSA" ? 0 : 1;
				else
					Assert.That(prevChapterNum, Is.EqualTo(chapter), "Chapter out of order" + Environment.NewLine + line);

				var verse = Parse(match.Result("${verse}"));
				Assert.That(verse, Is.GreaterThanOrEqualTo(prevVerseNum),
					"Verse out of order" + Environment.NewLine + line);
			}
		}

		private class BcvCharacterEqualityComparer : IEqualityComparer<CharacterVerse>
		{
			public bool Equals(CharacterVerse x, CharacterVerse y)
			{
				return x.BcvRef.Equals(y.BcvRef) && x.Character.Equals(y.Character);
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
