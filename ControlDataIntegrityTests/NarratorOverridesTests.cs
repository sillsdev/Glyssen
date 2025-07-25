﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GlyssenCharacters;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using SIL.Scripture;
using static GlyssenCharacters.NarratorOverrides;
using static GlyssenSharedTests.CustomConstraints;

namespace ControlDataIntegrityTests
{
	class NarratorOverridesTests
	{
		[Test]
		public void DataIntegrity_LoadsCorrectly()
		{
			Assert.That(GetNarratorOverridesForBook("PSA"), Is.Not.Empty);
		}

		[Test]
		public void DataIntegrity_AllCharactersExistInCharacterDetail()
		{
			foreach (var overrideDetail in NarratorOverridesByBookId.Values.SelectMany(o => o))
			{
				Assert.That(CharacterDetailData.Singleton.GetDictionary().Keys, Does.Contain(overrideDetail.Character),
					$"Character {overrideDetail.Character} In NarratorOverrides.xml was not found in CharacterDetail");
			}
		}

		[Test]
		public void DataIntegrity_StartsPrecedeEnds()
		{
			foreach (var bookId in NarratorOverridesByBookId.Keys)
			{
				foreach (var overrideDetail in GetNarratorOverridesForBook(bookId))
				{
					Assert.That(overrideDetail.StartChapter, Is.LessThanOrEqualTo(overrideDetail.EndChapter),
						$"In NarratorOverrides.xml, book {bookId}: end chapter {overrideDetail.EndChapter} precedes start chapter {overrideDetail.StartChapter}.");
					if (overrideDetail.StartChapter == overrideDetail.EndChapter)
					{
						Assert.That(overrideDetail.StartVerse, Is.LessThanOrEqualTo(overrideDetail.EndVerse),
							$"In NarratorOverrides.xml, book {bookId}, chapter {overrideDetail.StartChapter}: end verse {overrideDetail.EndVerse} precedes start verse {overrideDetail.StartVerse}.");

						if (overrideDetail.StartVerse == overrideDetail.EndVerse)
						{
							Assert.That(overrideDetail.StartBlock, Is.LessThanOrEqualTo(overrideDetail.EndBlock),
								$"In NarratorOverrides.xml, an entry for {bookId} {overrideDetail.StartChapter}:{overrideDetail.EndVerse} " +
								$"has an end block {overrideDetail.EndBlock} that precedes its start block {overrideDetail.StartBlock}.");
						}
					}
				}
			}
		}

		[Test]
		public void DataIntegrity_StartChapterSet()
		{
			foreach (var bookId in NarratorOverridesByBookId.Keys)
			{
				foreach (var overrideDetail in GetNarratorOverridesForBook(bookId))
				{
					Assert.That(overrideDetail.StartChapter, Is.GreaterThanOrEqualTo(1),
						$"In NarratorOverrides.xml, book {bookId}, StartChapter not set for override {overrideDetail}.");
				}
			}
		}

		/// <summary>
		/// Currently, 2 is the biggest we need. Can't imagine ever needing anything bigger than 3. (Following test wouldn't work if the start block was > 9.)
		/// </summary>
		[Test]
		public void DataIntegrity_StartBlockLessThanSix()
		{
			foreach (var bookId in NarratorOverridesByBookId.Keys)
			{
				foreach (var overrideDetail in GetNarratorOverridesForBook(bookId))
				{
					Assert.That(overrideDetail.StartBlock, Is.LessThan(6),
						$"In NarratorOverrides.xml, book {bookId}, StartBlock set to invalid value for override {overrideDetail}.");
				}
			}
		}

		[Test]
		public void DataIntegrity_ValidBookId()
		{
			foreach (var bookId in NarratorOverridesByBookId.Keys)
			{
				var bookNum = BCVRef.BookToNumber(bookId);
				Assert.That(bookNum, Is.GreaterThanOrEqualTo(1), $"Invalid book ID: {bookId}");
				Assert.That(bookNum, Is.LessThanOrEqualTo(66), $"Non-canonical book ID: {bookId}");
			}
		}

		[Test]
		public void DataIntegrity_NoOverlappingRanges()
		{
			var overridesByBook = new Dictionary<string, SortedDictionary<int, NarratorOverrideDetail>>();
			foreach (var bookId in NarratorOverridesByBookId.Keys)
			{
				var bookNum = BCVRef.BookToNumber(bookId);
				var dictionary = new SortedDictionary<int, NarratorOverrideDetail>();
				overridesByBook.Add(bookId, dictionary);
				foreach (var overrideDetail in GetNarratorOverridesForBook(bookId))
				{
					var newKey = new BCVRef(bookNum, overrideDetail.StartChapter, overrideDetail.StartVerse).BBCCCVVV * 10 + overrideDetail.StartBlock;
					if (dictionary.ContainsKey(newKey))
						Assert.Fail($"In NarratorOverrides.xml, book {bookId}: two overrides start at the same place:\n{dictionary[newKey]}\nAND\n{overrideDetail}");

					dictionary.Add(newKey, overrideDetail);
				}
			}

			foreach (var kvp in overridesByBook)
			{
				var dictionary = kvp.Value;
				NarratorOverrideDetail prev = null;
				foreach (var overrideDetail in dictionary.Values)
				{
					if (prev != null)
					{
						Assert.That(overrideDetail.EndChapter > prev.EndChapter || (overrideDetail.EndChapter == prev.EndChapter &&
								overrideDetail.EndVerse > prev.EndVerse) || (overrideDetail.EndChapter == prev.EndChapter &&
								overrideDetail.EndVerse == prev.EndVerse && overrideDetail.EndBlock > prev.EndBlock),
							$"In NarratorOverrides.xml, book {kvp.Key}: override {overrideDetail} overlaps {prev}.");
					}
					prev = overrideDetail;
				}
			}
		}

		[Test]
		public void DataIntegrity_EndChaptersAndVersesWithinEnglishVersificationLimits()
		{
			foreach (var bookOverrides in NarratorOverridesByBookId)
			{
				var bookNum = BCVRef.BookToNumber(bookOverrides.Key);
				foreach (NarratorOverrideDetail overrideDetail in bookOverrides.Value)
				{
					Assert.That(overrideDetail.EndChapter, Is.LessThanOrEqualTo(ScrVers.English.GetLastChapter(bookNum)),
						$"Invalid end chapter: {overrideDetail}");
					Assert.That(overrideDetail.EndVerse, Is.LessThanOrEqualTo(ScrVers.English.GetLastVerse(bookNum, overrideDetail.EndChapter)),
						$"Invalid end verse: {overrideDetail}");
				}
			}
		}

		/// <summary>
		/// The Implicit quote type indicates that we expect the (whole) verse to be spoken by a particular (real-life)
		/// character (or in the case of Wisdom and Folly in Proverbs, personfications of real abstract-concept characters).
		/// 
		/// Therefore, explicit quotes should be present. But since some languages don't mark quotes, or do so inconsistently
		/// (and often omit them for longer discourses), in the absence of any quoted text, we can safely apply the known
		/// character to the entire verse. (If quotes are marked up, there's no need to do this, and it would be wrong to do
		/// so since this could presumably end up assigning the character to speak a "he said".)
		/// 
		/// By contrast, a narrator override is used in places where the text is unlikely to contain quotes. The text (based
		/// on pronoun usage, etc.) tells us who the "narrator" is in real life. In this case, the decision to assign the text
		/// to the real-life character who is the author/narrator is based on desired dramatic effect or to prevent confusion
		/// where there are self-quotes. Strictly speaking, however, the text could be read entirely by a narrator.
		/// When text is to be overridden to be spoken by a character but could have explicit quotes, the Character-Verse control
		/// file should use Potential or Quotation rather than Implicit as the quote type, because in the absence of explicit
		/// quotes, we want to respect that the text really is narration and can be treated as such for scripting purposes. The
		/// ultimate decision to override the narrator can be left to the last minute.
		/// 
		/// This test ensures that we don't confuse these two (somewhat similar) ideas by ensuring that we don't have any verses
		/// marked as Implicit and also included in the narrator override control file for the same character.
		/// </summary>
		[Test]
		public void DataIntegrity_NoOverridesCoverVersesWithImplicitCharacter()
		{
			foreach (var cv in ControlCharacterVerseData.Singleton.GetAllQuoteInfo().Where(i => i.IsImplicit))
			{
				var verse = new VerseRef(BCVRef.BookToNumber(cv.BookCode), cv.Chapter, cv.Verse, ScrVers.English);
				var overrideInfo = GetCharacterOverrideDetailsForRefRange(verse, cv.Verse);
				Assert.That(overrideInfo, No<NarratorOverrideDetail>(oi => oi.Character,
						Is.EqualTo(cv.Character)),
					$"Character-verse file contains an Implicit quote for {cv.Character} in verse {verse} that is also covered " +
					$"by narrator override {overrideInfo}.");
			}
		}

		[Test]
		public void DataIntegrity_PartialVerseStartsAreValid()
		{
			foreach (var book in Singleton.Books)
			{
				foreach (var partialStart in book.Overrides.Where(o => o.StartBlock > 0))
				{
					if (partialStart.StartBlock == 1)
					{
						// This is an unusual situation, but it if happens that a character starts speaking in the first block
						// of a verse, they have to stop in that same block; otherwise, they might as well have spoken the whole verse.
						Assert.That(partialStart.EndBlock, Is.EqualTo(1),
							$"Character {partialStart.Character} starts speaking in block 1 of {book.Id} " +
							$"{partialStart.StartChapter}:{partialStart.StartVerse} but then keeps on talking!");
					}
					else
					{
						// Turns out that the following isn't necessarily true. In Poetic material, there can be multiple blocks of poetry
						// attributed to one character, followed by multiple blocks attributed to another character. Unfortunately, in practice
						// it can be tricky to know how many blocks to count because contiguous blocks of poetry may get joined if there is no
						// sentence-ending punctuation, and that may not be consistent across al translations. (Indeed, some translations may not
						// even retain the poetry-style markup.) If this proves unwieldy, we may just need to avoid using partial verse overrides
						// in poetry and instead explicitly set the reference text and require the scripter to look at each place to get it right.
						// This is an unusual situation, but it if happens that a character starts speaking in the first block
						// of a verse, they have to stop in that same block; otherwise, they might as well have spoken the whole verse.
						//Assert.That(partialStart.StartBlock == partialStart.EndBlock || partialStart.EndChapter > partialStart.StartChapter ||
						//	partialStart.EndVerse > partialStart.StartVerse, Is.True,
						//	$"Character {partialStart.Character} is assigned as the override for more than one contiguous block in {book.Id} " +
						//	$"{partialStart.StartChapter}:{partialStart.StartVerse}!");

						var endsForSameChapterAndVerse = book.Overrides.Where(o => o.EndChapter == partialStart.StartChapter &&
							o.EndVerse == partialStart.StartVerse).ToList();
						Assert.That(endsForSameChapterAndVerse,
							ForEvery<NarratorOverrideDetail>(e => e.EndBlock, Is.GreaterThan(0)),
							$"An override for {book.Id} " +
							$"{partialStart.StartChapter}:{partialStart.StartVerse} has a start " +
							$"block ({partialStart.StartBlock}) that is already covered by the " +
							"end verse of another entry!");

						if (partialStart.StartBlock >= 3)
						{
							if (!endsForSameChapterAndVerse.Any(
								e => e.EndBlock == partialStart.StartBlock - 1 || e.EndBlock == partialStart.StartBlock - 2))
							{
								var msg = $"There is a \"hole\" (more than one missing block in the block chain) because an override for {book.Id} " +
									$"{partialStart.StartChapter}:{partialStart.StartVerse} has a start block of {partialStart.StartBlock} " +
									"but no preceding entries";
								if (book.Id == "SNG" && partialStart.StartChapter == 4 && partialStart.StartVerse == 16 ||
									book.Id == "HAG" && (partialStart.StartChapter == 1 && partialStart.StartVerse == 13 ||
									partialStart.StartChapter == 2 && partialStart.StartVerse == 14))
								{
									Debug.WriteLine($"Known exception: {msg}");
								}
								else
								{
									// If this exception is thrown, check the specified entry carefully. If it is intentional, add another exception above.
									throw new InconclusiveException($"Possible error: {msg}");
								}
							}
						}
					}
				}
			}
		}
	}
}
