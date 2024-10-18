using System;
using System.Collections.Generic;
using System.Linq;
using Glyssen.Shared;
using GlyssenCharacters;
using GlyssenEngine;
using GlyssenEngine.Script;
using GlyssenEngineTests.Script;
using NUnit.Framework;
using SIL.Reflection;
using SIL.Scripture;
using static System.String;
using static GlyssenEngineTests.ReferenceTextTests;
using Resources = GlyssenCharactersTests.Properties.Resources;

namespace GlyssenEngineTests
{
	class ProjectDataMigratorTests
	{
		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = null;
			CharacterDetailData.TabDelimitedCharacterDetailData = null;
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			TestProject.DeleteTestProjects();
		}

		[TestCase(MultiBlockQuote.Continuation)]
		[TestCase(MultiBlockQuote.ChangeOfDelivery)]
		public void MigrateInvalidMultiBlockQuoteDataToVersion88_NoSplits_DataUnchanged(MultiBlockQuote lastBlockMultiBlockQuote)
		{
			var block1 = new Block
			{
				BlockElements = new List<BlockElement>
				{
					new Verse("1"),
					new ScriptText("Text of verse 1. "),
				},
				MultiBlockQuote = MultiBlockQuote.Start,
			};
			var block2 = new Block
			{
				BlockElements = new List<BlockElement>
				{
					new Verse("2"),
					new ScriptText("Text of verse 2. "),
				},
				MultiBlockQuote = lastBlockMultiBlockQuote,
			};

			var block1Original = block1.Clone();
			var block2Original = block2.Clone();

			var book = new BookScript("MAT", new List<Block> { block1, block2 }, ScrVers.English);
			var books = new List<BookScript> { book };
			ProjectDataMigrator.MigrateInvalidMultiBlockQuoteData(books);

			Assert.That(book.Blocks.Count, Is.EqualTo(2));
			Assert.That(block1Original.GetText(true), Is.EqualTo(block1.GetText(true)));
			Assert.That(block2Original.GetText(true), Is.EqualTo(block2.GetText(true)));
			Assert.That(MultiBlockQuote.Start, Is.EqualTo(block1.MultiBlockQuote));
			Assert.That(lastBlockMultiBlockQuote, Is.EqualTo(block2.MultiBlockQuote));
		}

		[Test]
		public void MigrateInvalidMultiBlockQuoteDataToVersion88_SplitDoesNotStartWithMultiBlockStart_DataUnchanged()
		{
			var block1 = new Block
			{
				BlockElements = new List<BlockElement>
				{
					new Verse("1"),
					new ScriptText("Text of verse 1. "),
				},
				MultiBlockQuote = MultiBlockQuote.None,
				SplitId = 0,
			};
			var block2 = new Block
			{
				BlockElements = new List<BlockElement>
				{
					new Verse("2"),
					new ScriptText("Text of verse 2. "),
				},
				MultiBlockQuote = MultiBlockQuote.Start,
				SplitId = 0,
			};
			var block3 = new Block
			{
				BlockElements = new List<BlockElement>
				{
					new Verse("3"),
					new ScriptText("Text of verse 3. "),
				},
				MultiBlockQuote = MultiBlockQuote.Continuation,
				SplitId = 0,
			};

			var block1Original = block1.Clone();
			var block2Original = block2.Clone();
			var block3Original = block3.Clone();

			var book = new BookScript("MAT", new List<Block> { block1, block2, block3 }, ScrVers.English);
			var books = new List<BookScript> { book };
			ProjectDataMigrator.MigrateInvalidMultiBlockQuoteData(books);

			Assert.That(book.Blocks.Count, Is.EqualTo(3));
			Assert.That(block1Original.GetText(true), Is.EqualTo(block1.GetText(true)));
			Assert.That(block2Original.GetText(true), Is.EqualTo(block2.GetText(true)));
			Assert.That(block3Original.GetText(true), Is.EqualTo(block3.GetText(true)));
			Assert.That(MultiBlockQuote.None, Is.EqualTo(block1.MultiBlockQuote));
			Assert.That(MultiBlockQuote.Start, Is.EqualTo(block2.MultiBlockQuote));
			Assert.That(MultiBlockQuote.Continuation, Is.EqualTo(block3.MultiBlockQuote));
		}

		[Test]
		public void MigrateInvalidMultiBlockQuoteDataToVersion88_SplitIsLastBlock_SplitWithMultiBlockStartAndNone_ChangedToNoneAndStart()
		{
			var block1 = new Block
			{
				BlockElements = new List<BlockElement>
				{
					new Verse("1"),
					new ScriptText("Text of verse 1. "),
				},
				MultiBlockQuote = MultiBlockQuote.Start,
				SplitId = 0,
			};
			var block2 = new Block
			{
				BlockElements = new List<BlockElement>
				{
					new Verse("2"),
					new ScriptText("Text of verse 2. "),
				},
				MultiBlockQuote = MultiBlockQuote.None,
				SplitId = 0,
			};

			var block1Original = block1.Clone();
			var block2Original = block2.Clone();

			var book = new BookScript("MAT", new List<Block> { block1, block2 }, ScrVers.English);
			var books = new List<BookScript> { book };
			ProjectDataMigrator.MigrateInvalidMultiBlockQuoteData(books);

			Assert.That(book.Blocks.Count, Is.EqualTo(2));
			Assert.That(block1Original.GetText(true), Is.EqualTo(block1.GetText(true)));
			Assert.That(block2Original.GetText(true), Is.EqualTo(block2.GetText(true)));
			Assert.That(MultiBlockQuote.None, Is.EqualTo(block1.MultiBlockQuote));
			Assert.That(MultiBlockQuote.Start, Is.EqualTo(block2.MultiBlockQuote));
		}

		[TestCase(MultiBlockQuote.Continuation)]
		[TestCase(MultiBlockQuote.ChangeOfDelivery)]
		public void MigrateInvalidMultiBlockQuoteDataToVersion88_SplitWithMultiBlockStartAndNone_ChangedToNoneAndStart(MultiBlockQuote lastBlockMultiBlockQuote)
		{
			var block1 = new Block
			{
				BlockElements = new List<BlockElement>
				{
					new Verse("1"),
					new ScriptText("Text of verse 1. "),
				},
				MultiBlockQuote = MultiBlockQuote.Start,
				SplitId = 0,
			};
			var block2 = new Block
			{
				BlockElements = new List<BlockElement>
				{
					new Verse("2"),
					new ScriptText("Text of verse 2. "),
				},
				MultiBlockQuote = MultiBlockQuote.None,
				SplitId = 0,
			};
			var block3 = new Block
			{
				BlockElements = new List<BlockElement>
				{
					new Verse("3"),
					new ScriptText("Text of verse 3. "),
				},
				MultiBlockQuote = lastBlockMultiBlockQuote
			};

			var block1Original = block1.Clone();
			var block2Original = block2.Clone();
			var block3Original = block3.Clone();

			var book = new BookScript("MAT", new List<Block> {block1, block2, block3}, ScrVers.English);
			var books = new List<BookScript> { book };
			ProjectDataMigrator.MigrateInvalidMultiBlockQuoteData(books);

			Assert.That(book.Blocks.Count, Is.EqualTo(3));
			Assert.That(block1Original.GetText(true), Is.EqualTo(block1.GetText(true)));
			Assert.That(block2Original.GetText(true), Is.EqualTo(block2.GetText(true)));
			Assert.That(block3Original.GetText(true), Is.EqualTo(block3.GetText(true)));
			Assert.That(MultiBlockQuote.None, Is.EqualTo(block1.MultiBlockQuote));
			Assert.That(MultiBlockQuote.Start, Is.EqualTo(block2.MultiBlockQuote));
			Assert.That(lastBlockMultiBlockQuote, Is.EqualTo(block3.MultiBlockQuote));
		}

		[TestCase(MultiBlockQuote.Continuation)]
		[TestCase(MultiBlockQuote.ChangeOfDelivery)]
		public void MigrateInvalidMultiBlockQuoteDataToVersion88_MultiSplitWithMultiBlockStartNoneNone_ChangedToNoneNoneStart(MultiBlockQuote lastBlockMultiBlockQuote)
		{
			var block1 = new Block
			{
				BlockElements = new List<BlockElement>
				{
					new Verse("1"),
					new ScriptText("Text of verse 1. "),
				},
				MultiBlockQuote = MultiBlockQuote.Start,
				SplitId = 0,
			};
			var block2 = new Block
			{
				BlockElements = new List<BlockElement>
				{
					new Verse("2"),
					new ScriptText("Text of verse 2. "),
				},
				MultiBlockQuote = MultiBlockQuote.None,
				SplitId = 0,
			};
			var block3 = new Block
			{
				BlockElements = new List<BlockElement>
				{
					new Verse("3"),
					new ScriptText("Text of verse 3. "),
				},
				MultiBlockQuote = MultiBlockQuote.None,
				SplitId = 0,
			};
			var block4 = new Block
			{
				BlockElements = new List<BlockElement>
				{
					new Verse("4"),
					new ScriptText("Text of verse 4. "),
				},
				MultiBlockQuote = lastBlockMultiBlockQuote
			};

			var block1Original = block1.Clone();
			var block2Original = block2.Clone();
			var block3Original = block3.Clone();
			var block4Original = block4.Clone();

			var book = new BookScript("MAT", new List<Block> {block1, block2, block3, block4}, ScrVers.English);

		var books = new List<BookScript> { book };
			ProjectDataMigrator.MigrateInvalidMultiBlockQuoteData(books);

			Assert.That(book.Blocks.Count, Is.EqualTo(4));
			Assert.That(block1Original.GetText(true), Is.EqualTo(block1.GetText(true)));
			Assert.That(block2Original.GetText(true), Is.EqualTo(block2.GetText(true)));
			Assert.That(block3Original.GetText(true), Is.EqualTo(block3.GetText(true)));
			Assert.That(block4Original.GetText(true), Is.EqualTo(block4.GetText(true)));
			Assert.That(MultiBlockQuote.None, Is.EqualTo(block1.MultiBlockQuote));
			Assert.That(MultiBlockQuote.None, Is.EqualTo(block2.MultiBlockQuote));
			Assert.That(MultiBlockQuote.Start, Is.EqualTo(block3.MultiBlockQuote));
			Assert.That(lastBlockMultiBlockQuote, Is.EqualTo(block4.MultiBlockQuote));
		}

		[Test]
		public void MigrateInvalidMultiBlockQuoteDataToVersion88_SplitWithMultiBlockStartChangeNoneChange_ChangedToStartChangeStartChange()
		{
			var block1 = new Block
			{
				BlockElements = new List<BlockElement>
				{
					new Verse("1"),
					new ScriptText("Text of verse 1. "),
				},
				MultiBlockQuote = MultiBlockQuote.Start,
			};
			var block2 = new Block
			{
				BlockElements = new List<BlockElement>
				{
					new Verse("2"),
					new ScriptText("Text of verse 2. "),
				},
				MultiBlockQuote = MultiBlockQuote.ChangeOfDelivery,
				SplitId = 0,
			};
			var block3 = new Block
			{
				BlockElements = new List<BlockElement>
				{
					new Verse("3"),
					new ScriptText("Text of verse 3. "),
				},
				MultiBlockQuote = MultiBlockQuote.None,
				SplitId = 0,
			};
			var block4 = new Block
			{
				BlockElements = new List<BlockElement>
				{
					new Verse("4"),
					new ScriptText("Text of verse 4. "),
				},
				MultiBlockQuote = MultiBlockQuote.ChangeOfDelivery
			};
			var block5 = new Block
			{
				BlockElements = new List<BlockElement>
				{
					new Verse("5"),
					new ScriptText("Text of verse 5. "),
				},
				MultiBlockQuote = MultiBlockQuote.Continuation
			};

			var block1Original = block1.Clone();
			var block2Original = block2.Clone();
			var block3Original = block3.Clone();
			var block4Original = block4.Clone();
			var block5Original = block5.Clone();

			var book = new BookScript("MAT", new List<Block> { block1, block2, block3, block4, block5 }, ScrVers.English);
			var books = new List<BookScript> { book };
			ProjectDataMigrator.MigrateInvalidMultiBlockQuoteData(books);

			Assert.That(book.Blocks.Count, Is.EqualTo(5));
			Assert.That(block1Original.GetText(true), Is.EqualTo(block1.GetText(true)));
			Assert.That(block2Original.GetText(true), Is.EqualTo(block2.GetText(true)));
			Assert.That(block3Original.GetText(true), Is.EqualTo(block3.GetText(true)));
			Assert.That(block4Original.GetText(true), Is.EqualTo(block4.GetText(true)));
			Assert.That(block5Original.GetText(true), Is.EqualTo(block5.GetText(true)));
			Assert.That(MultiBlockQuote.Start, Is.EqualTo(block1.MultiBlockQuote));
			Assert.That(MultiBlockQuote.ChangeOfDelivery, Is.EqualTo(block2.MultiBlockQuote));
			Assert.That(MultiBlockQuote.Start, Is.EqualTo(block3.MultiBlockQuote));
			Assert.That(MultiBlockQuote.ChangeOfDelivery, Is.EqualTo(block4.MultiBlockQuote));
			Assert.That(MultiBlockQuote.Continuation, Is.EqualTo(block5.MultiBlockQuote));
		}

		[TestCase(MultiBlockQuote.Continuation)]
		[TestCase(MultiBlockQuote.ChangeOfDelivery)]
		public void CleanUpOrphanedMultiBlockQuoteStati_CleanData_NoChange(MultiBlockQuote continuingStatus)
		{
			var block1 = CreateTestBlock(1, MultiBlockQuote.None);
			var block2 = CreateTestBlock(2, MultiBlockQuote.Start);
			var block3 = CreateTestBlock(3, continuingStatus);
			var block4 = CreateTestBlock(4, MultiBlockQuote.None);

			var block1Original = block1.Clone();
			var block2Original = block2.Clone();
			var block3Original = block3.Clone();
			var block4Original = block4.Clone();

			var book = new BookScript("MAT", new List<Block> { block1, block2, block3, block4 }, ScrVers.English);
			var books = new List<BookScript> { book };
			ProjectDataMigrator.CleanUpOrphanedMultiBlockQuoteStati(books);

			Assert.That(book.Blocks.Count, Is.EqualTo(4));
			Assert.That(block1Original.GetText(true), Is.EqualTo(block1.GetText(true)));
			Assert.That(block2Original.GetText(true), Is.EqualTo(block2.GetText(true)));
			Assert.That(block3Original.GetText(true), Is.EqualTo(block3.GetText(true)));
			Assert.That(block4Original.GetText(true), Is.EqualTo(block4.GetText(true)));
			Assert.That(MultiBlockQuote.None, Is.EqualTo(block1.MultiBlockQuote));
			Assert.That(MultiBlockQuote.Start, Is.EqualTo(block2.MultiBlockQuote));
			Assert.That(continuingStatus, Is.EqualTo(block3.MultiBlockQuote));
			Assert.That(MultiBlockQuote.None, Is.EqualTo(block4.MultiBlockQuote));
		}

		[TestCase(MultiBlockQuote.Continuation)]
		[TestCase(MultiBlockQuote.ChangeOfDelivery)]
		public void CleanUpOrphanedMultiBlockQuoteStati_FirstBlockIsNotNoneOrStart_ChangedToNone(MultiBlockQuote continuingStatus)
		{
			var block1 = CreateTestBlock(1, continuingStatus);
			var block2 = CreateTestBlock(2, MultiBlockQuote.None);

			var block1Original = block1.Clone();
			var block2Original = block2.Clone();

			var book = new BookScript("MAT", new List<Block> { block1, block2 }, ScrVers.English);
			var books = new List<BookScript> { book };
			ProjectDataMigrator.CleanUpOrphanedMultiBlockQuoteStati(books);

			Assert.That(book.Blocks.Count, Is.EqualTo(2));
			Assert.That(block1Original.GetText(true), Is.EqualTo(block1.GetText(true)));
			Assert.That(block2Original.GetText(true), Is.EqualTo(block2.GetText(true)));
			Assert.That(MultiBlockQuote.None, Is.EqualTo(block1.MultiBlockQuote));
			Assert.That(MultiBlockQuote.None, Is.EqualTo(block2.MultiBlockQuote));
		}

		[Test]
		public void CleanUpOrphanedMultiBlockQuoteStati_StartFollowedByNone_StartChangedToNone()
		{
			var block1 = CreateTestBlock(1, MultiBlockQuote.None);
			var block2 = CreateTestBlock(2, MultiBlockQuote.Start);
			var block3 = CreateTestBlock(3, MultiBlockQuote.None);

			var block1Original = block1.Clone();
			var block2Original = block2.Clone();
			var block3Original = block3.Clone();

			var book = new BookScript("MAT", new List<Block> { block1, block2, block3 }, ScrVers.English);
			var books = new List<BookScript> { book };
			ProjectDataMigrator.CleanUpOrphanedMultiBlockQuoteStati(books);

			Assert.That(book.Blocks.Count, Is.EqualTo(3));
			Assert.That(block1Original.GetText(true), Is.EqualTo(block1.GetText(true)));
			Assert.That(block2Original.GetText(true), Is.EqualTo(block2.GetText(true)));
			Assert.That(block3Original.GetText(true), Is.EqualTo(block3.GetText(true)));
			Assert.That(MultiBlockQuote.None, Is.EqualTo(block1.MultiBlockQuote));
			Assert.That(MultiBlockQuote.None, Is.EqualTo(block2.MultiBlockQuote));
			Assert.That(MultiBlockQuote.None, Is.EqualTo(block3.MultiBlockQuote));
		}

		[TestCase(MultiBlockQuote.Continuation)]
		[TestCase(MultiBlockQuote.ChangeOfDelivery)]
		public void CleanUpOrphanedMultiBlockQuoteStati_TwoStarts_FirstStartChangedToNone(MultiBlockQuote continuingStatus)
		{
			var block1 = CreateTestBlock(1, MultiBlockQuote.None);
			var block2 = CreateTestBlock(2, MultiBlockQuote.Start);
			var block3 = CreateTestBlock(3, MultiBlockQuote.Start);
			var block4 = CreateTestBlock(4, continuingStatus);

			var block1Original = block1.Clone();
			var block2Original = block2.Clone();
			var block3Original = block3.Clone();
			var block4Original = block4.Clone();

			var book = new BookScript("MAT", new List<Block> { block1, block2, block3, block4 }, ScrVers.English);
			var books = new List<BookScript> { book };
			ProjectDataMigrator.CleanUpOrphanedMultiBlockQuoteStati(books);

			Assert.That(book.Blocks.Count, Is.EqualTo(4));
			Assert.That(block1Original.GetText(true), Is.EqualTo(block1.GetText(true)));
			Assert.That(block2Original.GetText(true), Is.EqualTo(block2.GetText(true)));
			Assert.That(block3Original.GetText(true), Is.EqualTo(block3.GetText(true)));
			Assert.That(block4Original.GetText(true), Is.EqualTo(block4.GetText(true)));
			Assert.That(MultiBlockQuote.None, Is.EqualTo(block1.MultiBlockQuote));
			Assert.That(MultiBlockQuote.None, Is.EqualTo(block2.MultiBlockQuote));
			Assert.That(MultiBlockQuote.Start, Is.EqualTo(block3.MultiBlockQuote));
			Assert.That(continuingStatus, Is.EqualTo(block4.MultiBlockQuote));
		}

		[Test]
		public void CleanUpOrphanedMultiBlockQuoteStati_TwoStartsFollowedByNone_BothStartsChangedToNone()
		{
			var block1 = CreateTestBlock(1, MultiBlockQuote.None);
			var block2 = CreateTestBlock(2, MultiBlockQuote.Start);
			var block3 = CreateTestBlock(3, MultiBlockQuote.Start);
			var block4 = CreateTestBlock(4, MultiBlockQuote.None);

			var block1Original = block1.Clone();
			var block2Original = block2.Clone();
			var block3Original = block3.Clone();
			var block4Original = block4.Clone();

			var book = new BookScript("MAT", new List<Block> { block1, block2, block3, block4 }, ScrVers.English);
			var books = new List<BookScript> { book };
			ProjectDataMigrator.CleanUpOrphanedMultiBlockQuoteStati(books);

			Assert.That(book.Blocks.Count, Is.EqualTo(4));
			Assert.That(block1Original.GetText(true), Is.EqualTo(block1.GetText(true)));
			Assert.That(block2Original.GetText(true), Is.EqualTo(block2.GetText(true)));
			Assert.That(block3Original.GetText(true), Is.EqualTo(block3.GetText(true)));
			Assert.That(block4Original.GetText(true), Is.EqualTo(block4.GetText(true)));
			Assert.That(MultiBlockQuote.None, Is.EqualTo(block1.MultiBlockQuote));
			Assert.That(MultiBlockQuote.None, Is.EqualTo(block2.MultiBlockQuote));
			Assert.That(MultiBlockQuote.None, Is.EqualTo(block3.MultiBlockQuote));
			Assert.That(MultiBlockQuote.None, Is.EqualTo(block4.MultiBlockQuote));
		}

		[Test]
		public void CleanUpOrphanedMultiBlockQuoteStati_NoneFollowedByNonStart_NonStartChangedToNone()
		{
			var block1 = CreateTestBlock(1, MultiBlockQuote.None);
			var block2 = CreateTestBlock(2, MultiBlockQuote.Continuation);

			var block1Original = block1.Clone();
			var block2Original = block2.Clone();

			var book = new BookScript("MAT", new List<Block> { block1, block2 }, ScrVers.English);
			var books = new List<BookScript> { book };
			ProjectDataMigrator.CleanUpOrphanedMultiBlockQuoteStati(books);

			Assert.That(book.Blocks.Count, Is.EqualTo(2));
			Assert.That(block1Original.GetText(true), Is.EqualTo(block1.GetText(true)));
			Assert.That(block2Original.GetText(true), Is.EqualTo(block2.GetText(true)));
			Assert.That(MultiBlockQuote.None, Is.EqualTo(block1.MultiBlockQuote));
			Assert.That(MultiBlockQuote.None, Is.EqualTo(block2.MultiBlockQuote));
		}

		[Test]
		public void CleanUpOrphanedMultiBlockQuoteStati_LastBlockIsStart_StartChangedToNone()
		{
			var block1 = CreateTestBlock(1, MultiBlockQuote.None);
			var block2 = CreateTestBlock(2, MultiBlockQuote.Start);

			var block1Original = block1.Clone();
			var block2Original = block2.Clone();

			var book = new BookScript("MAT", new List<Block> { block1, block2 }, ScrVers.English);
			var books = new List<BookScript> { book };
			ProjectDataMigrator.CleanUpOrphanedMultiBlockQuoteStati(books);

			Assert.That(book.Blocks.Count, Is.EqualTo(2));
			Assert.That(block1Original.GetText(true), Is.EqualTo(block1.GetText(true)));
			Assert.That(block2Original.GetText(true), Is.EqualTo(block2.GetText(true)));
			Assert.That(MultiBlockQuote.None, Is.EqualTo(block1.MultiBlockQuote));
			Assert.That(MultiBlockQuote.None, Is.EqualTo(block2.MultiBlockQuote));
		}

		[Test]
		public void CleanUpOrphanedMultiBlockQuoteStati_NoneFollowedByMultipleNonStarts_NonStartsChangedToNone()
		{
			var block1 = CreateTestBlock(1, MultiBlockQuote.None);
			var block2 = CreateTestBlock(2, MultiBlockQuote.Continuation);
			var block3 = CreateTestBlock(3, MultiBlockQuote.Continuation);

			var block1Original = block1.Clone();
			var block2Original = block2.Clone();
			var block3Original = block3.Clone();

			var book = new BookScript("MAT", new List<Block> { block1, block2, block3 }, ScrVers.English);
			var books = new List<BookScript> { book };
			ProjectDataMigrator.CleanUpOrphanedMultiBlockQuoteStati(books);

			Assert.That(book.Blocks.Count, Is.EqualTo(3));
			Assert.That(block1Original.GetText(true), Is.EqualTo(block1.GetText(true)));
			Assert.That(block2Original.GetText(true), Is.EqualTo(block2.GetText(true)));
			Assert.That(block3Original.GetText(true), Is.EqualTo(block3.GetText(true)));
			Assert.That(MultiBlockQuote.None, Is.EqualTo(block1.MultiBlockQuote));
			Assert.That(MultiBlockQuote.None, Is.EqualTo(block2.MultiBlockQuote));
			Assert.That(MultiBlockQuote.None, Is.EqualTo(block3.MultiBlockQuote));
		}

		[Test]
		public void CleanUpMultiBlockQuotesAssignedToNarrator_MultiBlockQuoteChainAssignedToNarrator_AllBlocksInChainSetToNone()
		{
			var blocks = new List<Block>(7);
			var narrator = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator);
			blocks.Add(CreateTestBlock(1, MultiBlockQuote.None));
			blocks.Last().SetNonDramaticCharacterId(narrator);
			blocks.Add(CreateTestBlock(2, MultiBlockQuote.Start));
			blocks.Last().SetNonDramaticCharacterId(narrator);
			blocks.Add(CreateTestBlock(3, MultiBlockQuote.Continuation));
			blocks.Last().SetNonDramaticCharacterId(narrator);
			blocks.Add(CreateTestBlock(4, MultiBlockQuote.Continuation));
			blocks.Last().SetNonDramaticCharacterId(narrator);
			blocks.Add(CreateTestBlock(5, MultiBlockQuote.Start));
			blocks.Last().CharacterId = "Jesus";
			blocks.Add(CreateTestBlock(6, MultiBlockQuote.Continuation));
			blocks.Last().CharacterId = "Jesus";
			blocks.Add(CreateTestBlock(7, MultiBlockQuote.None));
			blocks.Last().SetNonDramaticCharacterId(narrator);

			var book = new BookScript("MAT", blocks.Select(b => b.Clone()), ScrVers.English);
			Assert.That(book.Blocks, Is.Not.EqualTo(blocks), "Sanity check: blocks should have been cloned.");
			var books = new List<BookScript> { book };
			ProjectDataMigrator.CleanUpMultiBlockQuotesAssignedToNarrator(books);

			Assert.That(blocks.SequenceEqual(book.Blocks, new SplitBlockComparer()), Is.True,
				"Block content should not have changed!");
			Assert.That(2, Is.EqualTo(book.Blocks.Count(b => b.MultiBlockQuote != MultiBlockQuote.None && b.CharacterId == "Jesus")),
				"Multi-block quote chain for Jesus should not have been affected.");
			Assert.That(5, Is.EqualTo(book.Blocks.Count(b => b.CharacterId == narrator)),
				"Narrator should still be assigned to the 5 blocks not assigned to Jesus.");
			Assert.That(book.Blocks.Where(b => b.CharacterId == narrator).Select(b => b.MultiBlockQuote),
				Is.All.EqualTo(MultiBlockQuote.None),
				"Multi-block quote chain should have gotten broken up for all narrator blocks.");
		}

		[Test]
		public void ResolveUnclearCharacterIdsForVernBlocksMatchedToRefBlocks_SingleUnclearBlocksMatched_UnclearBlocksSetToCorrespondingCharacterAndDeliveryFromRefBlocks()
		{
			var blocks = new List<Block>(4);
			var narrator = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator);
			blocks.Add(CreateTestBlock(1, MultiBlockQuote.None));
			blocks.Last().SetNonDramaticCharacterId(narrator);
			blocks.Add(CreateTestBlock(2, MultiBlockQuote.None));
			blocks.Last().CharacterId = CharacterVerseData.kUnexpectedCharacter;
			blocks.Last().SetMatchedReferenceBlock(new Block("p", 1, 2) { CharacterId = "Phil", Delivery = "Panicky" });
			blocks.Add(CreateTestBlock(3, MultiBlockQuote.Start));
			blocks.Last().CharacterId = "Jesus";
			blocks.Add(CreateTestBlock(4, MultiBlockQuote.Continuation));
			blocks.Last().CharacterId = CharacterVerseData.kAmbiguousCharacter;
			blocks.Last().SetMatchedReferenceBlock(new Block("p", 1, 2) { CharacterId = "Phil/Kelsy", CharacterIdInScript = "Kelsy" });

			var book = new BookScript("MAT", blocks.Select(b => b.Clone()), ScrVers.English);
			Assert.That(blocks, Is.Not.EquivalentTo(book.Blocks), "Sanity check: blocks should have been cloned.");
			var books = new List<BookScript> { book };
			ProjectDataMigrator.ResolveUnclearCharacterIdsForVernBlocksMatchedToRefBlocks(books);

			Assert.That(blocks.SequenceEqual(book.Blocks, new SplitBlockComparer()), Is.True,
				"Block content should not have changed!");
			Assert.That(narrator, Is.EqualTo(book.GetScriptBlocks()[0].CharacterId));
			Assert.That(book.GetScriptBlocks()[1].CharacterId, Is.EqualTo("Phil"));
			Assert.That(book.GetScriptBlocks()[1].CharacterIdInScript, Is.EqualTo("Phil"));
			Assert.That(book.GetScriptBlocks()[1].Delivery, Is.EqualTo("Panicky"));
			Assert.That(book.GetScriptBlocks()[2].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(book.GetScriptBlocks()[3].CharacterId, Is.EqualTo("Phil/Kelsy"));
			Assert.That(book.GetScriptBlocks()[3].CharacterIdInScript, Is.EqualTo("Kelsy"));
			Assert.That(book.GetScriptBlocks()[3].Delivery, Is.Null);
		}

		[Test]
		public void ResolveUnclearCharacterIdsForVernBlocksMatchedToRefBlocks_SomeUnclearBlocksInQuoteChainMatched_AllBlocksInEntireChainSetToSameCharacter()
		{
			var blocks = new List<Block>(5);
			var narrator = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator);
			blocks.Add(CreateTestBlock(1, MultiBlockQuote.None));
			blocks.Last().SetNonDramaticCharacterId(narrator);
			blocks.Add(CreateTestBlock(2, MultiBlockQuote.Start));
			blocks.Last().CharacterId = CharacterVerseData.kUnexpectedCharacter;
			blocks.Last().SetMatchedReferenceBlock(new Block("p", 1, 2) { CharacterId = "Phil", Delivery = "panicky" }
				.AddVerse(2, "This is the start of verse two."));
			blocks.Add(new Block("p", 1, 2) { MultiBlockQuote = MultiBlockQuote.Continuation, CharacterId = CharacterVerseData.kUnexpectedCharacter }
				.AddText("Esto es el resto de ello."));
			blocks.Last().SetMatchedReferenceBlock(new Block("p", 1, 2) { CharacterId = "Phil", Delivery = "calmer" }
				.AddText("This is the rest of it."));
			blocks.Add(CreateTestBlock(3, MultiBlockQuote.Continuation));
			blocks.Last().CharacterId = CharacterVerseData.kUnexpectedCharacter;
			blocks.Add(CreateTestBlock(4, MultiBlockQuote.None));
			blocks.Last().CharacterId = "the people";

			var book = new BookScript("MAT", blocks.Select(b => b.Clone()), ScrVers.English);
			Assert.That(blocks, Is.Not.EquivalentTo(book.Blocks),
				"Sanity check: blocks should have been cloned.");
			var books = new List<BookScript> { book };
			ProjectDataMigrator.ResolveUnclearCharacterIdsForVernBlocksMatchedToRefBlocks(books);

			Assert.That(blocks.SequenceEqual(book.Blocks, new SplitBlockComparer()), Is.True,
				"Block content should not have changed!");
			Assert.That(narrator, Is.EqualTo(book.GetScriptBlocks()[0].CharacterId));
			Assert.That(book.GetScriptBlocks()[1].CharacterId, Is.EqualTo("Phil"));
			Assert.That(book.GetScriptBlocks()[1].Delivery, Is.EqualTo("panicky"));
			Assert.That(book.GetScriptBlocks()[2].CharacterId, Is.EqualTo("Phil"));
			Assert.That(book.GetScriptBlocks()[2].Delivery, Is.EqualTo("calmer"));
			Assert.That(book.GetScriptBlocks()[3].CharacterId, Is.EqualTo("Phil"));
			Assert.That(book.GetScriptBlocks()[3].Delivery, Is.EqualTo("calmer"));
			Assert.That(book.GetScriptBlocks()[4].CharacterId, Is.EqualTo("the people"));
			Assert.That(book.GetScriptBlocks()[4].Delivery, Is.Null);
		}

		[Test]
		public void MigrateInvalidCharacterIdForScriptData_ValidDataWithNoMultipleCharacterIds_Unchanged()
		{
			var block1 = CreateTestBlock("Andrew");
			var block2 = CreateTestBlock("Peter");
			var book = new BookScript("MAT", new List<Block> { block1, block2 }, ScrVers.English);
			var books = new List<BookScript> { book };
			ProjectDataMigrator.MigrateInvalidCharacterIdForScriptData(books);

			Assert.That(block1.CharacterId, Is.EqualTo("Andrew"));
			Assert.That(block1.CharacterIdInScript, Is.EqualTo("Andrew"));
			Assert.That(block2.CharacterId, Is.EqualTo("Peter"));
			Assert.That(block2.CharacterIdInScript, Is.EqualTo("Peter"));
		}

		[TestCase(CharacterVerseData.kAmbiguousCharacter)]
		[TestCase(CharacterVerseData.kUnexpectedCharacter)]
		public void MigrateInvalidCharacterIdForScriptDataToVersion88_CharacterIdUnclearAndCharacterIdInScriptNotNull_CharacterIdInScriptSetToNull(string unclearCharacterId)
		{
			var block1 = CreateTestBlock("Andrew");
			block1.UserConfirmed = true;
			block1.CharacterId = unclearCharacterId;
			Assert.That(unclearCharacterId, Is.EqualTo(block1.CharacterId));
			Assert.That(block1.CharacterIdInScript, Is.EqualTo("Andrew"));

			var block2 = CreateTestBlock("Peter");
			block2.UserConfirmed = true;
			block2.CharacterId = unclearCharacterId;
			Assert.That(unclearCharacterId, Is.EqualTo(block2.CharacterId));
			Assert.That(block2.CharacterIdInScript, Is.EqualTo("Peter"));

			var book = new BookScript("MAT", new List<Block> { block1, block2 }, ScrVers.English);
			var books = new List<BookScript> { book };

			Assert.That(block1.UserConfirmed, Is.True);
			Assert.That(block2.UserConfirmed, Is.True);

			ProjectDataMigrator.MigrateInvalidCharacterIdForScriptData(books);

			Assert.That(unclearCharacterId, Is.EqualTo(block1.CharacterId));
			Assert.That(unclearCharacterId, Is.EqualTo(block1.CharacterIdInScript));
			Assert.That(block1.UserConfirmed, Is.False);
			Assert.That(unclearCharacterId, Is.EqualTo(block2.CharacterId));
			Assert.That(unclearCharacterId, Is.EqualTo(block2.CharacterIdInScript));
			Assert.That(block2.UserConfirmed, Is.False);
		}

		[Test]
		public void MigrateInvalidCharacterIdForScriptData_ValidDataWithMultipleCharacterIds_Unchanged()
		{
			var block1 = CreateTestBlock("Andrew/James");
			block1.CharacterIdInScript = "James";
			var block2 = CreateTestBlock("Peter");
			var book = new BookScript("MAT", new List<Block> { block1, block2 }, ScrVers.English);
			var books = new List<BookScript> { book };
			ProjectDataMigrator.MigrateInvalidCharacterIdForScriptData(books);

			Assert.That(block1.CharacterId, Is.EqualTo("Andrew/James"));
			Assert.That(block1.CharacterIdInScript, Is.EqualTo("James"));
			Assert.That(block2.CharacterId, Is.EqualTo("Peter"));
			Assert.That(block2.CharacterIdInScript, Is.EqualTo("Peter"));
		}

		[Test]
		public void MigrateInvalidCharacterIdForScriptData_NarratorBlocksWithNonNullCharacterIdInScript_CharacterIdInScriptSetToNull()
		{
			var bcMat = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.BookOrChapter);
			var narratorMat = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator);
			var block1 = CreateTestBlock("Andrew");
			block1.UserConfirmed = false;
			block1.CharacterId = bcMat;
			Assert.That(bcMat, Is.EqualTo(block1.CharacterId));
			Assert.That(block1.CharacterIdInScript, Is.EqualTo("Andrew"));

			var block2 = CreateTestBlock("Peter");
			block2.UserConfirmed = true;
			block2.CharacterId = narratorMat;
			Assert.That(narratorMat, Is.EqualTo(block2.CharacterId));
			Assert.That(block2.CharacterIdInScript, Is.EqualTo("Peter"));

			var book = new BookScript("MAT", new List<Block> { block1, block2 }, ScrVers.English);
			var books = new List<BookScript> { book };

			Assert.That(block1.UserConfirmed, Is.False);
			Assert.That(block2.UserConfirmed, Is.True);

			ProjectDataMigrator.MigrateInvalidCharacterIdForScriptData(books);

			Assert.That(bcMat, Is.EqualTo(block1.CharacterId));
			Assert.That(bcMat, Is.EqualTo(block1.CharacterIdInScript));
			Assert.That(block1.CharacterIdOverrideForScript, Is.Null);
			Assert.That(block1.UserConfirmed, Is.False);
			Assert.That(narratorMat, Is.EqualTo(block2.CharacterId));
			Assert.That(narratorMat, Is.EqualTo(block2.CharacterIdInScript));
			Assert.That(block2.CharacterIdOverrideForScript, Is.Null);
			Assert.That(block2.UserConfirmed, Is.True);
		}

		[Test]
		public void MigrateDeprecatedCharacterIds_OneOfTwoCharacterIdsInVerseReplacedWithDifferentId_CharacterIdInScriptSetToAmbiguous()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.REV);
			TestProject.SimulateDisambiguationForAllBooks(testProject);
			var johnSpeakingInRev714 = testProject.IncludedBooks.Single().GetBlocksForVerse(7, 14).ElementAt(1);
			johnSpeakingInRev714.SetCharacterIdAndCharacterIdInScript("John", 66);
			johnSpeakingInRev714.UserConfirmed = true;
			var elderSpeakingInRev714 = testProject.IncludedBooks.Single().GetBlocksForVerse(7, 14).ElementAt(3);
			elderSpeakingInRev714.SetCharacterIdAndCharacterIdInScript("elders, one of the", 66);
			Assert.That(johnSpeakingInRev714.UserConfirmed, Is.True);

			Assert.That(ProjectDataMigrator.MigrateDeprecatedCharacterIds(testProject), Is.EqualTo(1));

			Assert.That(CharacterVerseData.kAmbiguousCharacter, Is.EqualTo(johnSpeakingInRev714.CharacterId));
			Assert.That(CharacterVerseData.kAmbiguousCharacter, Is.EqualTo(johnSpeakingInRev714.CharacterIdInScript));
			Assert.That(johnSpeakingInRev714.UserConfirmed, Is.False);
			Assert.That(elderSpeakingInRev714.CharacterId, Is.EqualTo("elders, one of the"));
			Assert.That(elderSpeakingInRev714.CharacterIdInScript, Is.EqualTo("elders, one of the"));
		}

		[Test]
		public void MigrateDeprecatedCharacterIds_CharacterIdReplacedWithSingleId_CharacterIdInScriptSetToReplacementId()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.REV);
			TestProject.SimulateDisambiguationForAllBooks(testProject);
			var angelsSpeakingInRev712 = testProject.IncludedBooks.Single().GetBlocksForVerse(7, 12).ElementAt(1);
			angelsSpeakingInRev712.SetCharacterIdAndCharacterIdInScript("tons of angelic beings", 66);

			Assert.That(ProjectDataMigrator.MigrateDeprecatedCharacterIds(testProject), Is.EqualTo(1));

			Assert.That(angelsSpeakingInRev712.CharacterId, Is.EqualTo("angels, all the"));
			Assert.That(angelsSpeakingInRev712.CharacterIdInScript, Is.EqualTo("angels, all the"));
		}

		[TestCase("humming")]
		[TestCase("")]
		[TestCase(null)]
		public void MigrateDeprecatedCharacterIds_DeliveryChanged_DeliveryChangedInBlock(string initialDelivery)
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.REV);
			TestProject.SimulateDisambiguationForAllBooks(testProject);
			var singersInRev59 = testProject.IncludedBooks.Single().GetBlocksForVerse(5, 9).Skip(1).ToList();
			foreach (var block in singersInRev59)
				block.Delivery = initialDelivery;

			Assert.That(singersInRev59.Count, Is.EqualTo(ProjectDataMigrator.MigrateDeprecatedCharacterIds(testProject)));

			Assert.That(singersInRev59.All(b => b.CharacterId == "living creature, first/living creature, second/living creature, third/living creature, fourth/twenty-four elders" &&
				b.Delivery == "singing"), Is.True);
		}

		[TestCase("Bartimaeus (a blind man)", "humming", "shouting")]
		[TestCase("Bartimaeus (a blind man)", "", "shouting")]
		[TestCase("crowd, many in the", null, "rebuking")]
		public void MigrateDeprecatedCharacterIds_DeliveryChangedForOneOfTwoCharactersInVerse_DeliveryChangedInBlock(string character, string initialDelivery, string expectedDelivery)
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.LUK);
			TestProject.SimulateDisambiguationForAllBooks(testProject);
			var block = testProject.IncludedBooks.Single().GetBlocksForVerse(18, 39).Last();
			block.CharacterId = character;
			block.Delivery = initialDelivery;

			Assert.That(ProjectDataMigrator.MigrateDeprecatedCharacterIds(testProject), Is.EqualTo(1));
			Assert.That(expectedDelivery, Is.EqualTo(block.Delivery));
		}

		[Test]
		public void MigrateDeprecatedCharacterIds_BlockHasNoDeliveryButSomeOfTheVersesHaveOnlyCvEntryWithDelivery_DeliveryLeftUnspecified()
		{
			var vernacularBlocks = new List<Block>
			{
				new Block("c", 16)
				{
					CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.BookOrChapter),
					BlockElements = new List<BlockElement>(new[] { new ScriptText("16") })
				},
				CreateNarratorBlockForVerse(23, "Jesus rebuked him, saying:", true,  16)
			};
			AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "«Get the behind me Satan! ")
				.AddVerse(24, "If you disciples are serious about following me, this is the deal.");

			var testProject = TestProject.CreateTestProject(TestProject.TestBook.MAT);
			testProject.Books[0].Blocks = vernacularBlocks;

			Assert.That(ProjectDataMigrator.MigrateDeprecatedCharacterIds(testProject), Is.EqualTo(0));
			Assert.That(testProject.Books[0].Blocks.Select(b => b.Delivery), Is.All.Null);
		}

		[Test]
		public void MigrateDeprecatedCharacterIds_ExistingAmbiguousUserConfirmed_ClearsUserConfirmed()
		{
			// Note: this scenario was caused by a bug in a previous version of this method.
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.LUK);
			TestProject.SimulateDisambiguationForAllBooks(testProject);
			var block = testProject.IncludedBooks.Single().GetBlocksForVerse(18, 39).Last();
			block.CharacterId = CharacterVerseData.kAmbiguousCharacter;
			block.UserConfirmed = true;

			Assert.That(ProjectDataMigrator.MigrateDeprecatedCharacterIds(testProject), Is.EqualTo(1));
			Assert.That(false, Is.EqualTo(block.UserConfirmed));
		}

		[Test]
		public void MigrateDeprecatedCharacterIds_ExistingAmbiguousUserNotConfirmed_NoChanges()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.LUK);
			TestProject.SimulateDisambiguationForAllBooks(testProject);
			var block = testProject.IncludedBooks.Single().GetBlocksForVerse(18, 39).Last();
			block.CharacterId = CharacterVerseData.kAmbiguousCharacter;
			block.UserConfirmed = false;

			Assert.That(ProjectDataMigrator.MigrateDeprecatedCharacterIds(testProject), Is.EqualTo(0));
			Assert.That(false, Is.EqualTo(block.UserConfirmed));
		}

		[Test]
		public void MigrateDeprecatedCharacterIds_OneMemberOfMultiCharacterIdChanged_CharacterIdInScriptSetToReplacementId()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.REV);
			TestProject.SimulateDisambiguationForAllBooks(testProject);
			var singersInRev59 = testProject.IncludedBooks.Single().GetBlocksForVerse(5, 9).Skip(1).ToList();
			foreach (var block in singersInRev59)
				block.SetCharacterIdAndCharacterIdInScript("cuatro living creatures/twenty-four elders", 66);

			Assert.That(singersInRev59.Count, Is.EqualTo(ProjectDataMigrator.MigrateDeprecatedCharacterIds(testProject)));

			Assert.That(singersInRev59.All(b => b.CharacterId == "living creature, first/living creature, second/living creature, third/living creature, fourth/twenty-four elders" &&
				b.CharacterIdInScript == "living creature, first"), Is.True);
		}

		[Test]
		public void MigrateDeprecatedCharacterIds_CharacterIdRemoved_NoOtherCharactersInVerse_CharacterIdInScriptSetToUnknown()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.REV);
			TestProject.SimulateDisambiguationForAllBooks(testProject);
			var blockInRev43 = testProject.IncludedBooks.Single().GetBlocksForVerse(4, 3).First();
			blockInRev43.SetCharacterIdAndCharacterIdInScript("angels, all the", 66);
			blockInRev43.CharacterIdInScript = "angels, all, the";
			Assert.That(blockInRev43.CharacterIdInScript, Is.EqualTo("angels, all, the"));

			Assert.That(ProjectDataMigrator.MigrateDeprecatedCharacterIds(testProject), Is.EqualTo(1));

			Assert.That(CharacterVerseData.kUnexpectedCharacter, Is.EqualTo(blockInRev43.CharacterId));
			Assert.That(CharacterVerseData.kUnexpectedCharacter, Is.EqualTo(blockInRev43.CharacterIdInScript));
		}

		[Test]
		public void MigrateDeprecatedCharacterIds_CharacterIdRemoved_MultipleCharactersStillInVerse_CharacterIdInScriptSetToAmbiguous()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.REV);
			TestProject.SimulateDisambiguationForAllBooks(testProject);
			var blockInRev13V10 = testProject.IncludedBooks.Single().GetBlocksForVerse(13, 10).First();
			blockInRev13V10.SetCharacterIdAndCharacterIdInScript("angels, all the", 66);
			blockInRev13V10.CharacterIdInScript = "angels, all, the";
			Assert.That(blockInRev13V10.CharacterIdInScript, Is.EqualTo("angels, all, the"));

			Assert.That(ProjectDataMigrator.MigrateDeprecatedCharacterIds(testProject), Is.EqualTo(1));

			Assert.That(CharacterVerseData.kAmbiguousCharacter, Is.EqualTo(blockInRev13V10.CharacterId));
			Assert.That(CharacterVerseData.kAmbiguousCharacter, Is.EqualTo(blockInRev13V10.CharacterIdInScript));
		}

		[Test]
		public void MigrateDeprecatedCharacterIds_NormalQuoteChangedToAlternate_CharacterIdUnchanged()
		{
			Assert.That(!ControlCharacterVerseData.Singleton.GetCharacters(66, 1, new SingleVerse(8)).Any(cv => cv.Character == "God"),
				"Test setup condition not met: God should not be returned as a character when includeAlternatesAndRareQuotes is false.");
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(66, 1, new SingleVerse(8), includeAlternatesAndRareQuotes: true)
					.Any(cv => cv.Character == "God"),
				"Test setup condition not met: God should be returned as a character when includeAlternatesAndRareQuotes is true.");
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.REV);
			TestProject.SimulateDisambiguationForAllBooks(testProject);
			var stringRepresentationOfQuoteBlocksInRevC1V8 = new List<string>(2);
			foreach (var block in testProject.IncludedBooks.Single().GetBlocksForVerse(1, 8).Where(b => b.IsQuote))
			{
				block.SetNonDramaticCharacterId("God");
				stringRepresentationOfQuoteBlocksInRevC1V8.Add(block.ToString(true, "REV"));
			}
			Assert.That(stringRepresentationOfQuoteBlocksInRevC1V8.Any(),
				"Test setup condition not met: There should be a direct quote in Rev 1:8");
			Assert.That(ProjectDataMigrator.MigrateDeprecatedCharacterIds(testProject), Is.EqualTo(0));
			Assert.That(stringRepresentationOfQuoteBlocksInRevC1V8.SequenceEqual(
				testProject.IncludedBooks.Single().GetBlocksForVerse(1, 8).Where(b => b.IsQuote).Select(b => b.ToString(true, "REV"))), Is.True);
		}

		/// <summary>
		/// PG-471
		/// </summary>
		[Test]
		public void MigrateDeprecatedCharacterIds_StandardCharacterIdUsedInUnexpectedPlaceIsLaterRenamed_CharacterIdInScriptSetToUnknown()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.REV);
			TestProject.SimulateDisambiguationForAllBooks(testProject);
			var unexpectedPeterInRev711 = testProject.IncludedBooks.Single().GetBlocksForVerse(7, 11).First();
			unexpectedPeterInRev711.SetCharacterIdAndCharacterIdInScript("peter", 66);
			testProject.ProjectCharacterVerseData.AddEntriesFor(66, unexpectedPeterInRev711);

			Assert.That(ProjectDataMigrator.MigrateDeprecatedCharacterIds(testProject), Is.EqualTo(1));

			Assert.That(CharacterVerseData.kUnexpectedCharacter, Is.EqualTo(unexpectedPeterInRev711.CharacterId));
			Assert.That(testProject.ProjectCharacterVerseData.Any(), Is.False);
		}

		[Test]
		public void MigrateDeprecatedCharacterIds_StandardCharacterIdPreviouslyAddedAsProjectSpecific_ProjectSpecificCharacterIdRemoved()
		{
			List<Block> altarBlocksInRev16v7;
			Project testProject;
			try
			{
				ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerse;
				CharacterDetailData.TabDelimitedCharacterDetailData = Resources.TestCharacterDetail;

				testProject = TestProject.CreateTestProject(TestProject.TestBook.REV);
				//TestProject.SimulateDisambiguationForAllBooks(testProject);
				// The following setup steps simulate a condition where the user added (and used) a character ID that was not
				// present in the control files, but was subsequently added. Rev 16:7 used to have a character called "altar, the",
				// but it was renamed to "altar". This forward-thinking user added their own "altar" character before that became
				// the official character ID.
				altarBlocksInRev16v7 = testProject.IncludedBooks.Single().GetBlocksForVerse(16, 7).Where(b => b.CharacterId == "altar, the").ToList();
				Assert.That(altarBlocksInRev16v7.Any(), Is.True);
				var projectSpecificDetail = new CharacterDetail
				{
					CharacterId = "altar",
					Age = CharacterAge.Adult,
					Gender = CharacterGender.PreferMale,
				};
				testProject.AddProjectCharacterDetail(projectSpecificDetail);
				foreach (var block in altarBlocksInRev16v7)
				{
					block.CharacterId = projectSpecificDetail.CharacterId;
					testProject.ProjectCharacterVerseData.AddEntriesFor(66, block);
				}
				Assert.That(testProject.ProjectCharacterVerseData.Any(), Is.True);
			}
			finally
			{
				// Fast-forward into the future!
				ControlCharacterVerseData.TabDelimitedCharacterVerseData = null;
				CharacterDetailData.TabDelimitedCharacterDetailData = null;
			}

			Assert.That(ProjectDataMigrator.MigrateDeprecatedCharacterIds(testProject) >= 1, Is.True);

			Assert.That(altarBlocksInRev16v7.Select(b => b.CharacterId), Is.All.EqualTo("altar"),
				"The blocks themselves should not have changed.");
			Assert.That(testProject.ProjectCharacterVerseData.Any(), Is.False,
				"The only entry in ProjectCharacterVerseData should have been removed.");
			Assert.That(testProject.AllCharacterDetailDictionary.ContainsKey("altar"), Is.True,
				"The only entry in ProjectCharacterDetail should have been removed. This call " +
				"should throw an exception if the key is present in both.");
		}

		[Test]
		public void MigrateDeprecatedCharacterIds_BlockAssignedToDefaultCharacter_AssignmentChangedToGroup()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.REV);
			TestProject.SimulateDisambiguationForAllBooks(testProject);
			var block = testProject.IncludedBooks.Single().GetScriptBlocks().First(b => b.CharacterIdOverrideForScript != null);

			var origCharacterId = block.CharacterId;
			var origCharacterIdForScript = block.CharacterIdOverrideForScript;
			// The following setup steps simulate a condition where the character ID (in the control file, and therefore also
			// set for the block) was a simple, single character
			// In the current version of the control file (represented by the *orig* values above), it has been turned into
			// a multiple-character id (i.e., with slashes).
			block.CharacterId = block.CharacterIdOverrideForScript;
			block.CharacterIdOverrideForScript = null;

			Assert.That(ProjectDataMigrator.MigrateDeprecatedCharacterIds(testProject), Is.EqualTo(1));
			Assert.That(origCharacterId, Is.EqualTo(block.CharacterId));
			Assert.That(origCharacterIdForScript, Is.EqualTo(block.CharacterIdOverrideForScript));
		}

		[Test]
		public void MigrateDeprecatedCharacterIds_FirstVerseInQuoteIsUnexpectedForCharacter_CharacterIdSetToUnknown()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.JUD);
			TestProject.SimulateDisambiguationForAllBooks(testProject);

			var bookScript = testProject.IncludedBooks.Single();
			var verses13And14Block = bookScript.GetBlocksForVerse(1, 13).Single();
			var originalVerse14Blocks = bookScript.GetBlocksForVerse(1, 14);

			// Use reflection to get around a check to ensure we don't do this in production code
			List<Block> blocks = (List<Block>)ReflectionHelper.GetField(bookScript, "m_blocks");
			int blockCount = (int)ReflectionHelper.GetField(bookScript, "m_blockCount");

			//Combine verse 13 and 14 blocks
			foreach (var block in originalVerse14Blocks)
			{
				verses13And14Block.BlockElements.AddRange(block.BlockElements);

				blocks.Remove(block);
				blockCount--;
			}
			ReflectionHelper.SetField(bookScript, "m_blockCount", blockCount);

			verses13And14Block.CharacterId = "Enoch";

			//Setup check
			Assert.That(verses13And14Block.CharacterId, Is.EqualTo("Enoch"));

			//SUT
			Assert.That(ProjectDataMigrator.MigrateDeprecatedCharacterIds(testProject), Is.EqualTo(1));

			Assert.That(CharacterVerseData.kUnexpectedCharacter, Is.EqualTo(verses13And14Block.CharacterId));
			Assert.That(verses13And14Block.UserConfirmed, Is.False);
		}

		[Test]
		public void MigrateDeprecatedCharacterIds_FirstVerseInQuoteIsProjectSpecificCvButSecondVerseIsControlCv_NoChangesAndEntryNotRemovedFromProjectCvFile()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.JUD);
			TestProject.SimulateDisambiguationForAllBooks(testProject);

			var bookScript = testProject.IncludedBooks.Single();
			var verses13And14Block = bookScript.GetBlocksForVerse(1, 13).Single();
			var originalVerse14Blocks = bookScript.GetBlocksForVerse(1, 14);

			// Use reflection to get around a check to ensure we don't do this in production code
			List<Block> blocks = (List<Block>)ReflectionHelper.GetField(bookScript, "m_blocks");
			int blockCount = (int)ReflectionHelper.GetField(bookScript, "m_blockCount");

			//Combine verse 13 and 14 blocks
			foreach (var block in originalVerse14Blocks)
			{
				verses13And14Block.BlockElements.AddRange(block.BlockElements);

				blocks.Remove(block);
				blockCount--;
			}
			ReflectionHelper.SetField(bookScript, "m_blockCount", blockCount);

			verses13And14Block.CharacterId = "Enoch";
			verses13And14Block.UserConfirmed = true;
			testProject.ProjectCharacterVerseData.AddEntriesFor(65, verses13And14Block);

			//Setup check
			Assert.That(verses13And14Block.CharacterId, Is.EqualTo("Enoch"));

			//SUT - Call it twice to make sure first time doesn't delete project CV entry
			Assert.That(ProjectDataMigrator.MigrateDeprecatedCharacterIds(testProject), Is.EqualTo(0));
			Assert.That(ProjectDataMigrator.MigrateDeprecatedCharacterIds(testProject), Is.EqualTo(0));

			Assert.That(verses13And14Block.CharacterId, Is.EqualTo("Enoch"));
			Assert.That(verses13And14Block.UserConfirmed, Is.True);
			Assert.That(testProject.ProjectCharacterVerseData.Any(), Is.True);
		}

		/// <summary>
		/// This test is mainly designed to verify that the migrator doesn't clear all the explicitly set Character IDs
		/// for a reference text in "narrator" blocks. We don't want to require the C-V control file to have a
		/// "Potential" quote for all the places where we really wouldn't expect there to be explicit quotes in the text.
		/// </summary>
		[Test]
		public void MigrateDeprecatedCharacterIds_BlocksExplicitlyAssignedToNarratorOverrideCharacter_CharacterIdNotChanged()
		{
			const string kObadiahTheProphet = "Obadiah, prophet";
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.OBA);
			TestProject.SimulateDisambiguationForAllBooks(testProject);

			var bookScript = testProject.IncludedBooks.Single();
			var overrideOba = NarratorOverrides.Singleton.Books.Single(b => b.Id == bookScript.BookId).Overrides.Single();
			Assert.That(1, Is.EqualTo(overrideOba.StartChapter), "Test setup conditions not met!");
			Assert.That(1, Is.EqualTo(overrideOba.EndChapter), "Test setup conditions not met!");
			Assert.That(kObadiahTheProphet, Is.EqualTo(overrideOba.Character), "Test setup conditions not met!");
			Assert.That(overrideOba.StartVerse, Is.LessThanOrEqualTo(19), "Test setup conditions not met!");
			Assert.That(21, Is.EqualTo(overrideOba.EndVerse), "Test setup conditions not met!");

			var explicitObadiahBlocks = new HashSet<Block>();
			for (int i = 19; i < 21; i++)
			{
				foreach (var block in bookScript.GetBlocksForVerse(1, i))
				{
					block.CharacterId = kObadiahTheProphet;
					explicitObadiahBlocks.Add(block);
				}
			}
			Assert.That(explicitObadiahBlocks.Any(), Is.True, "Test setup conditions not met!");

			//SUT
			Assert.That(ProjectDataMigrator.MigrateDeprecatedCharacterIds(testProject), Is.EqualTo(0));

			foreach (var block in explicitObadiahBlocks)
				Assert.That(kObadiahTheProphet, Is.EqualTo(block.CharacterId));
		}

		[Test]
		public void MigrateInvalidCharacterIdsWithoutCharacterIdInScriptOverrides_ControlFileHasNoExplicitDefault_FirstCharacterIsUsed()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.MRK);
			TestProject.SimulateDisambiguationForAllBooks(testProject);
			var jesusSpeakingInMrk59 = testProject.IncludedBooks.Single().GetBlocksForVerse(5, 9).ElementAt(1);
			jesusSpeakingInMrk59.SetCharacterIdAndCharacterIdInScript("Jesus", 66);
			jesusSpeakingInMrk59.UserConfirmed = true;
			var demonSpeakingInMrk59 = testProject.IncludedBooks.Single().GetBlocksForVerse(5, 9).ElementAt(3);
			demonSpeakingInMrk59.CharacterId = "demons (Legion)/man delivered from Legion of demons";
			demonSpeakingInMrk59.CharacterIdInScript = null;
			demonSpeakingInMrk59.UserConfirmed = true;

			Assert.That(ProjectDataMigrator.MigrateInvalidCharacterIdsWithoutCharacterIdInScriptOverrides(testProject), Is.EqualTo(1));

			Assert.That(jesusSpeakingInMrk59.CharacterId, Is.EqualTo("Jesus"));
			Assert.That(jesusSpeakingInMrk59.CharacterIdInScript, Is.EqualTo("Jesus"));
			Assert.That(jesusSpeakingInMrk59.UserConfirmed, Is.True);
			Assert.That(demonSpeakingInMrk59.CharacterId, Is.EqualTo("demons (Legion)/man delivered from Legion of demons"));
			Assert.That(demonSpeakingInMrk59.CharacterIdInScript, Is.EqualTo("demons (Legion)"));
			Assert.That(demonSpeakingInMrk59.UserConfirmed, Is.True);
		}

		// The only place in the NT where this is likely to have occurred was John 11:34, but we don't have John test data, and it didn't seem worth it.
		//[Test]
		//public void MigrateInvalidCharacterIdsWithoutCharacterIdInScriptOverrides_ControlFileHasExplicitDefault_ExplicitDefaultCharacterIsUsed()
		//{
		//	var testProject = TestProject.CreateTestProject(TestProject.TestBook.JHN);
		//	TestProject.SimulateDisambiguationForAllBooks(testProject);
		//	var jesusSpeakingInJhn1134 = testProject.IncludedBooks.Single().GetBlocksForVerse(11, 34).ElementAt(1);
		//	jesusSpeakingInJhn1134.SetCharacterAndCharacterIdInScript("Jesus", 66);
		//	jesusSpeakingInJhn1134.UserConfirmed = true;
		//	var marySpeakingInJhn1134 = testProject.IncludedBooks.Single().GetBlocksForVerse(11, 34).ElementAt(3);
		//	marySpeakingInJhn1134.CharacterId = "Jews, the/Mary, sister of Martha/Martha";
		//	marySpeakingInJhn1134.CharacterIdInScript = null;
		//	marySpeakingInJhn1134.UserConfirmed = true;

		//	Assert.That(ProjectDataMigrator.MigrateInvalidCharacterIdsWithoutCharacterIdInScriptOverrides(testProject), Is.EqualTo(1));

		//	Assert.That(jesusSpeakingInJhn1134.CharacterId, Is.EqualTo("Jesus"));
		//	Assert.That(jesusSpeakingInJhn1134.CharacterIdInScript, Is.EqualTo("Jesus"));
		//	Assert.That(jesusSpeakingInJhn1134.UserConfirmed, Is.True);
		//	Assert.That(marySpeakingInJhn1134.CharacterId, Is.EqualTo("Jews, the/Mary, sister of Martha/Martha"));
		//	Assert.That(marySpeakingInJhn1134.CharacterIdInScript, Is.EqualTo("Mary, sister of Martha"));
		//	Assert.That(marySpeakingInJhn1134.UserConfirmed, Is.True);
		//}

		[Test]
		public void MigrateDeprecatedCharacterIds_PredeterminedCharacterMarkedNeedsReviewLaterAddedToControlFile_CharacterIdSetAndNoLongerNeedsReview()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.REV);
			// The following setup steps simulate a condition where the original translators used a quote
			// milestone to specify a character ID that was not in the control file at the time of the
			// original parse. (But then that character was later added.)
			var altarBlocksInRev16v7 = testProject.IncludedBooks.Single().GetBlocksForVerse(16, 7).Where(b => b.CharacterId == "altar").ToList();
			Assert.That(altarBlocksInRev16v7.Any(), Is.True);
			foreach (var block in altarBlocksInRev16v7)
			{
				block.StyleTag = "qt1-s";
				block.CharacterId = CharacterVerseData.kNeedsReview;
				block.CharacterIdInScript = "altar";
			}

			Assert.That(altarBlocksInRev16v7.Count, Is.EqualTo(ProjectDataMigrator.MigrateDeprecatedCharacterIds(testProject)));

			Assert.That(altarBlocksInRev16v7.All(b => b.CharacterId == "altar" &&
				b.CharacterIdInScript == b.CharacterId &&
				b.IsPredeterminedFirstLevelQuoteStart), Is.True,
				"The blocks should no longer be \"Needs Review\".");
		}

		[TestCase("c")]
		[TestCase("cl")]
		public void SetBookIdForChapterBlocks_Normal_AllChapterBlocksGetBookIdSet(string chapterStyleTag)
		{
			var genesis = new BookScript("GEN",
				new List<Block>
				{
					CreateTestBlock(CharacterVerseData.GetStandardCharacterId("GEN", CharacterVerseData.StandardCharacter.BookOrChapter)),
					CreateChapterBlock("GEN", 1, chapterStyleTag),
					CreateTestBlock(1, MultiBlockQuote.None),
					CreateChapterBlock("GEN", 2, chapterStyleTag),
					CreateTestBlock(1, MultiBlockQuote.None),
					CreateTestBlock(2, MultiBlockQuote.None),
					CreateChapterBlock("GEN", 3, chapterStyleTag),
					CreateTestBlock(1, MultiBlockQuote.None),
					CreateChapterBlock("GEN", 4, chapterStyleTag),
					CreateTestBlock(1, MultiBlockQuote.None),
					CreateTestBlock(2, MultiBlockQuote.None),
					CreateTestBlock(3, MultiBlockQuote.None),
					CreateTestBlock(4, MultiBlockQuote.None),
					CreateChapterBlock("GEN", 5, chapterStyleTag),
					CreateTestBlock(1, MultiBlockQuote.None),
				},
				ScrVers.English
			);
			var matthew = new BookScript("MAT",
				new List<Block>
				{
					CreateTestBlock(CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.BookOrChapter)),
					CreateChapterBlock("MAT", 1, chapterStyleTag),
					CreateTestBlock(2, MultiBlockQuote.None),
					CreateTestBlock(1, MultiBlockQuote.None),
					CreateChapterBlock("MAT", 2, chapterStyleTag),
					CreateTestBlock(1, MultiBlockQuote.None),
					CreateTestBlock(2, MultiBlockQuote.None),
					CreateTestBlock(3, MultiBlockQuote.None),
					CreateTestBlock(4, MultiBlockQuote.None),
					CreateChapterBlock("MAT", 3, chapterStyleTag),
					CreateTestBlock(1, MultiBlockQuote.None),
					CreateChapterBlock("MAT", 4, chapterStyleTag),
					CreateTestBlock(1, MultiBlockQuote.None),
					CreateChapterBlock("MAT", 5, chapterStyleTag),
					CreateTestBlock(1, MultiBlockQuote.None),
					CreateTestBlock(2, MultiBlockQuote.None),
					CreateChapterBlock("MAT", 6, chapterStyleTag),
					CreateTestBlock(1, MultiBlockQuote.None),
					CreateTestBlock(2, MultiBlockQuote.None),
					CreateChapterBlock("MAT", 20, chapterStyleTag),
					CreateTestBlock(1, MultiBlockQuote.None),
					CreateTestBlock(2, MultiBlockQuote.None),
					CreateTestBlock(3, MultiBlockQuote.None),
				},
				ScrVers.English
			);

			var books = new List<BookScript> { genesis, matthew };
			ProjectDataMigrator.SetBookIdForChapterBlocks(books);

			Assert.That(books.SelectMany(book => book.Blocks).Where(bl => bl.StyleTag == "c" || bl.StyleTag == "cl").Any(bl => bl.BookCode == null), Is.False);
		}

		[Test]
		public void MigrateNonContiguousUserSplitsSeparatedByReferenceTextAlignmentSplits_OneProgramSplit_InterveningBlocksGetSameSplitId()
		{
			var blocks = new List<Block>
			{
				new Block
				{
					BlockElements = new List<BlockElement>
					{
						new Verse("1"),
						new ScriptText("Text of verse "),
					},
					MultiBlockQuote = MultiBlockQuote.Continuation,
					SplitId = 0,
					CharacterId = "Walter",
					UserConfirmed = true,
				},
				new Block
				{
					BlockElements = new List<BlockElement>
					{
						new ScriptText("one got split back there. "),
					},
					MultiBlockQuote = MultiBlockQuote.None,
					SplitId = 0,
					CharacterId = "Fred",
					UserConfirmed = true,
				},
				new Block
				{
					BlockElements = new List<BlockElement>
					{
						new Verse("2"),
						new ScriptText("Text of verse two "),
					},
					MultiBlockQuote = MultiBlockQuote.None,
					SplitId = -1,
					CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator),
				},
				new Block
				{
					BlockElements = new List<BlockElement>
					{
						new ScriptText("got split back there. "),
						new Verse("3"),
						new ScriptText("Text of verse three. "),
					},
					MultiBlockQuote = MultiBlockQuote.None,
					SplitId = 0,
					CharacterId = "Fred",
					UserConfirmed = true,
				}
			};

			SetDummyReferenceText(blocks);

			var origBlocks = blocks.Select(b => b.Clone()).ToList();

			var book = new BookScript("MAT", blocks, ScrVers.English);
			var books = new List<BookScript> { book };
			ProjectDataMigrator.MigrateNonContiguousUserSplitsSeparatedByReferenceTextAlignmentSplits(books);

			var resultingBlocks = book.GetScriptBlocks();
			Assert.That(origBlocks.Count, Is.EqualTo(resultingBlocks.Count));
			Assert.That(origBlocks.Select(b => b.GetText(true)).SequenceEqual(resultingBlocks.Select(b => b.GetText(true))), Is.True);
			Assert.That(origBlocks.Select(b => b.MultiBlockQuote).SequenceEqual(resultingBlocks.Select(b => b.MultiBlockQuote)), Is.True);
			Assert.That(resultingBlocks.All(b => b.SplitId == 0), Is.True);
			Assert.That(origBlocks.Select(b => b.MatchesReferenceText).SequenceEqual(resultingBlocks.Select(b => b.MatchesReferenceText)), Is.True);
			Assert.That(origBlocks.Select(b => b.GetPrimaryReferenceText()).SequenceEqual(resultingBlocks.Select(b => b.GetPrimaryReferenceText())), Is.True);
			Assert.That(origBlocks.Select(b => b.CharacterId).SequenceEqual(resultingBlocks.Select(b => b.CharacterId)), Is.True);
			Assert.That(origBlocks.Select(b => b.UserConfirmed).SequenceEqual(resultingBlocks.Select(b => b.UserConfirmed)), Is.True);
		}

		[Test]
		public void MigrateNonContiguousUserSplitsSeparatedByReferenceTextAlignmentSplits_MultipleBooksWithMultipleProgramSplitsBetweenMultipleBlocksInSingleUserSplit_InterveningBlocksGetSameSplitId()
		{
			var books = new List<BookScript>(2);
			var origBlocks = new List<List<Block>>(2);
			do
			{
				var bookId = BCVRef.NumberToBookCode(40 + books.Count);

				var blocks = new List<Block>
				{
					new Block
					{
						IsParagraphStart = true,
						BlockElements = new List<BlockElement>
						{
							new Verse("1"),
							new ScriptText("Text of verse "),
						},
						MultiBlockQuote = MultiBlockQuote.Continuation,
						SplitId = books.Count,
						CharacterId = "Walter",
						UserConfirmed = true,
					},
					new Block
					{
						BlockElements = new List<BlockElement>
						{
							new ScriptText("one got split back there. "),
						},
						MultiBlockQuote = MultiBlockQuote.None,
						SplitId = books.Count,
						CharacterId = "Fred",
						UserConfirmed = true,
					},
					new Block
					{
						BlockElements = new List<BlockElement>
						{
							new Verse("2"),
							new ScriptText("Text of verse two. "),
						},
						MultiBlockQuote = MultiBlockQuote.None,
						SplitId = -1,
						CharacterId = CharacterVerseData.GetStandardCharacterId(bookId, CharacterVerseData.StandardCharacter.Narrator),
					},
					new Block
					{
						BlockElements = new List<BlockElement>
						{
							new Verse("3"),
							new ScriptText("Text of verse three "),
						},
						MultiBlockQuote = MultiBlockQuote.None,
						SplitId = -1,
						CharacterId = CharacterVerseData.GetStandardCharacterId(bookId, CharacterVerseData.StandardCharacter.Narrator),
					},
					new Block
					{
						BlockElements = new List<BlockElement>
						{
							new ScriptText("got split back there. "),
						},
						MultiBlockQuote = MultiBlockQuote.None,
						SplitId = books.Count,
						CharacterId = "Fred",
						UserConfirmed = true,
					},
					new Block
					{
						BlockElements = new List<BlockElement>
						{
							new Verse("4"),
							new ScriptText("Text of verse four "),
						},
						MultiBlockQuote = MultiBlockQuote.None,
						SplitId = -1,
						CharacterId = CharacterVerseData.GetStandardCharacterId(bookId, CharacterVerseData.StandardCharacter.Narrator),
					},
					new Block
					{
						BlockElements = new List<BlockElement>
						{
							new ScriptText("got split twice. Once here, "),
						},
						MultiBlockQuote = MultiBlockQuote.None,
						SplitId = books.Count,
						CharacterId = "Elijah",
						UserConfirmed = true,
					},
					new Block
					{
						BlockElements = new List<BlockElement>
						{
							new ScriptText("and again there. "),
						},
						MultiBlockQuote = MultiBlockQuote.None,
						SplitId = books.Count,
						CharacterId = "Elijah",
						UserConfirmed = true,
					},
				};

				SetDummyReferenceText(blocks);

				origBlocks.Add(blocks.Select(b => b.Clone()).ToList());

				books.Add(new BookScript(bookId, blocks, ScrVers.English));

			} while (books.Count < books.Capacity);

			ProjectDataMigrator.MigrateNonContiguousUserSplitsSeparatedByReferenceTextAlignmentSplits(books);

			for (int i = 0; i < books.Count; i++)
			{
				var origBookBlocks = origBlocks[i];
				var resultingBlocks = books[i].GetScriptBlocks();
				Assert.That(origBookBlocks.Count, Is.EqualTo(resultingBlocks.Count));
				Assert.That(origBookBlocks.Select(b => b.GetText(true)).SequenceEqual(resultingBlocks.Select(b => b.GetText(true))), Is.True);
				Assert.That(origBookBlocks.Select(b => b.MultiBlockQuote).SequenceEqual(resultingBlocks.Select(b => b.MultiBlockQuote)), Is.True);
				Assert.That(resultingBlocks.All(b => b.SplitId == i), Is.True);
				Assert.That(origBookBlocks.Select(b => b.MatchesReferenceText).SequenceEqual(resultingBlocks.Select(b => b.MatchesReferenceText)), Is.True);
				Assert.That(origBookBlocks.Select(b => b.GetPrimaryReferenceText()).SequenceEqual(resultingBlocks.Select(b => b.GetPrimaryReferenceText())), Is.True);
				Assert.That(origBookBlocks.Select(b => b.CharacterId).SequenceEqual(resultingBlocks.Select(b => b.CharacterId)), Is.True);
				Assert.That(origBookBlocks.Select(b => b.UserConfirmed).SequenceEqual(resultingBlocks.Select(b => b.UserConfirmed)), Is.True);
			}
		}

		[TestCase(true)]
		[TestCase(false)]
		public void MigrateNonContiguousUserSplitsSeparatedByReferenceTextAlignmentSplits_ProgramSplitsBetweenDifferentUserSplits_NoChange(bool matchRefText)
		{
			var blocks = new List<Block>
			{
				new Block
				{
					IsParagraphStart = true,
					BlockElements = new List<BlockElement>
					{
						new Verse("1"),
						new ScriptText("Text of verse "),
					},
					MultiBlockQuote = MultiBlockQuote.Continuation,
					SplitId = 2,
					CharacterId = "Walter",
					UserConfirmed = true,
				},
				new Block
				{
					BlockElements = new List<BlockElement>
					{
						new ScriptText("one got split back there. "),
					},
					MultiBlockQuote = MultiBlockQuote.None,
					SplitId = 2,
					CharacterId = "Fred",
					UserConfirmed = true,
				},
				new Block
				{
					BlockElements = new List<BlockElement>
					{
						new Verse("2"),
						new ScriptText("Text of verse two "),
					},
					MultiBlockQuote = MultiBlockQuote.None,
					SplitId = -1,
					CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator),
				},
				new Block
				{
					IsParagraphStart = true,
					BlockElements = new List<BlockElement>
					{
						new Verse("3"),
						new ScriptText("Text of verse three "),
					},
					MultiBlockQuote = MultiBlockQuote.None,
					SplitId = -1,
					CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator),
					UserConfirmed = true,
				},
				new Block
				{
					BlockElements = new List<BlockElement>
					{
						new ScriptText("got split back there. "),
						new Verse("4"),
						new ScriptText("And then we end with verse four."),
					},
					MultiBlockQuote = MultiBlockQuote.Start,
					SplitId = 1,
					CharacterId = "Fred",
					UserConfirmed = true,
				},
				new Block
				{
					IsParagraphStart = true,
					BlockElements = new List<BlockElement>
					{
						new Verse("5"),
						new ScriptText("But we also continue that quote at verse five, which was not split at all."),
					},
					MultiBlockQuote = MultiBlockQuote.Continuation,
					SplitId = -1,
					CharacterId = "Fred",
					UserConfirmed = true,
				},
			};

			if (matchRefText)
				SetDummyReferenceText(blocks);

			var origBlocks = blocks.Select(b => b.Clone()).ToList();

			var book = new BookScript("MAT", blocks, ScrVers.English);
			var books = new List<BookScript> { book };
			ProjectDataMigrator.MigrateNonContiguousUserSplitsSeparatedByReferenceTextAlignmentSplits(books);

			var resultingBlocks = book.GetScriptBlocks();
			Assert.That(origBlocks.Count, Is.EqualTo(resultingBlocks.Count));
			Assert.That(origBlocks.Select(b => b.GetText(true)).SequenceEqual(resultingBlocks.Select(b => b.GetText(true))), Is.True);
			Assert.That(origBlocks.Select(b => b.MultiBlockQuote).SequenceEqual(resultingBlocks.Select(b => b.MultiBlockQuote)), Is.True);
			Assert.That(origBlocks.Select(b => b.SplitId).SequenceEqual(resultingBlocks.Select(b => b.SplitId)), Is.True);
			Assert.That(origBlocks.Select(b => b.MatchesReferenceText).SequenceEqual(resultingBlocks.Select(b => b.MatchesReferenceText)), Is.True);
			Assert.That(origBlocks.Select(b => b.GetPrimaryReferenceText()).SequenceEqual(resultingBlocks.Select(b => b.GetPrimaryReferenceText())), Is.True);
			Assert.That(origBlocks.Select(b => b.CharacterId).SequenceEqual(resultingBlocks.Select(b => b.CharacterId)), Is.True);
			Assert.That(origBlocks.Select(b => b.UserConfirmed).SequenceEqual(resultingBlocks.Select(b => b.UserConfirmed)), Is.True);
		}

		[Test]
		public void MigrateNonContiguousUserSplitsSeparatedByReferenceTextAlignmentSplits_NoProgramSplits_NoChange()
		{
			var blocks = new List<Block>
			{
				new Block
				{
					BlockElements = new List<BlockElement>
					{
						new Verse("1"),
						new ScriptText("Text of verse "),
					},
					MultiBlockQuote = MultiBlockQuote.None,
					SplitId = 1,
					CharacterId = "Fred",
					UserConfirmed = true,
				},
				new Block
				{
					BlockElements = new List<BlockElement>
					{
						new ScriptText("one got split back there. "),
					},
					MultiBlockQuote = MultiBlockQuote.None,
					SplitId = 1,
					CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator),
					UserConfirmed = true,
				},
				new Block
				{
					BlockElements = new List<BlockElement>
					{
						new Verse("2-3"),
						new ScriptText("An this split happened at the verse break. "),
					},
					MultiBlockQuote = MultiBlockQuote.None,
					SplitId = 1,
					CharacterId = "Fred",
					UserConfirmed = true,
				}
			};

			var origBlocks = blocks.Select(b => b.Clone()).ToList();

			var book = new BookScript("MAT", blocks, ScrVers.English);
			var books = new List<BookScript> { book };
			ProjectDataMigrator.MigrateNonContiguousUserSplitsSeparatedByReferenceTextAlignmentSplits(books);

			var resultingBlocks = book.GetScriptBlocks();
			Assert.That(origBlocks.Count, Is.EqualTo(resultingBlocks.Count));
			Assert.That(origBlocks.Select(b => b.GetText(true)).SequenceEqual(resultingBlocks.Select(b => b.GetText(true))), Is.True);
			Assert.That(origBlocks.Select(b => b.MultiBlockQuote).SequenceEqual(resultingBlocks.Select(b => b.MultiBlockQuote)), Is.True);
			Assert.That(origBlocks.Select(b => b.SplitId).SequenceEqual(resultingBlocks.Select(b => b.SplitId)), Is.True);
			Assert.That(origBlocks.Select(b => b.MatchesReferenceText).SequenceEqual(resultingBlocks.Select(b => b.MatchesReferenceText)), Is.True);
			Assert.That(origBlocks.Select(b => b.CharacterId).SequenceEqual(resultingBlocks.Select(b => b.CharacterId)), Is.True);
			Assert.That(origBlocks.Select(b => b.UserConfirmed).SequenceEqual(resultingBlocks.Select(b => b.UserConfirmed)), Is.True);
		}

		[Test]
		public void MigrateNonContiguousUserSplitsSeparatedByReferenceTextAlignmentSplits_PrecedingProgramSplit_NoChange()
		{
			var blocks = new List<Block>
			{
				new Block
				{
					BlockElements = new List<BlockElement>
					{
						new Verse("1"),
						new ScriptText("Text of verse one. "),
					},
					MultiBlockQuote = MultiBlockQuote.Start,
					SplitId = -1,
					CharacterId = "Fred",
					UserConfirmed = true,
				},
				new Block
				{
					BlockElements = new List<BlockElement>
					{
						new Verse("2"),
						new ScriptText("Text of verse "),
					},
					MultiBlockQuote = MultiBlockQuote.Continuation,
					SplitId = 0,
					CharacterId = "Fred",
					UserConfirmed = true,
				},
				new Block
				{
					BlockElements = new List<BlockElement>
					{
						new ScriptText("two got split back there. "),
					},
					MultiBlockQuote = MultiBlockQuote.None,
					SplitId = 0,
					CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator),
					UserConfirmed = true,
				}
			};

			SetDummyReferenceText(blocks);

			var origBlocks = blocks.Select(b => b.Clone()).ToList();

			var book = new BookScript("MAT", blocks, ScrVers.English);
			var books = new List<BookScript> { book };
			ProjectDataMigrator.MigrateNonContiguousUserSplitsSeparatedByReferenceTextAlignmentSplits(books);

			var resultingBlocks = book.GetScriptBlocks();
			Assert.That(origBlocks.Count, Is.EqualTo(resultingBlocks.Count));
			Assert.That(origBlocks.Select(b => b.GetText(true)).SequenceEqual(resultingBlocks.Select(b => b.GetText(true))), Is.True);
			Assert.That(origBlocks.Select(b => b.MultiBlockQuote).SequenceEqual(resultingBlocks.Select(b => b.MultiBlockQuote)), Is.True);
			Assert.That(origBlocks.Select(b => b.SplitId).SequenceEqual(resultingBlocks.Select(b => b.SplitId)), Is.True);
			Assert.That(origBlocks.Select(b => b.MatchesReferenceText).SequenceEqual(resultingBlocks.Select(b => b.MatchesReferenceText)), Is.True);
			Assert.That(origBlocks.Select(b => b.GetPrimaryReferenceText()).SequenceEqual(resultingBlocks.Select(b => b.GetPrimaryReferenceText())), Is.True);
			Assert.That(origBlocks.Select(b => b.CharacterId).SequenceEqual(resultingBlocks.Select(b => b.CharacterId)), Is.True);
			Assert.That(origBlocks.Select(b => b.UserConfirmed).SequenceEqual(resultingBlocks.Select(b => b.UserConfirmed)), Is.True);
		}

		[Test]
		public void MigrateNonContiguousUserSplitsSeparatedByReferenceTextAlignmentSplits_FollowingProgramSplits_NoChange()
		{
			var blocks = new List<Block>
			{
				new Block
				{
					BlockElements = new List<BlockElement>
					{
						new Verse("1"),
						new ScriptText("Text of verse "),
					},
					MultiBlockQuote = MultiBlockQuote.None,
					SplitId = 0,
					CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator),
					UserConfirmed = true,
				},
				new Block
				{
					BlockElements = new List<BlockElement>
					{
						new ScriptText("one got split in the middle. "),
					},
					MultiBlockQuote = MultiBlockQuote.Start,
					SplitId = 0,
					CharacterId = "Jesus",
					UserConfirmed = true,
				},
				new Block
				{
					BlockElements = new List<BlockElement>
					{
						new Verse("2"),
						new ScriptText("Text of verse two."),
					},
					MultiBlockQuote = MultiBlockQuote.Continuation,
					SplitId = -1,
					CharacterId = "Jesus",
					UserConfirmed = true,
				},
				new Block
					{
					BlockElements = new List<BlockElement>
					{
						new Verse("3"),
						new ScriptText("Text of verse three."),
					},
					MultiBlockQuote = MultiBlockQuote.Continuation,
					SplitId = -1,
					CharacterId = "Jesus",
				}
			};

			SetDummyReferenceText(blocks);

			var origBlocks = blocks.Select(b => b.Clone()).ToList();

			var book = new BookScript("MAT", blocks, ScrVers.English);
			var books = new List<BookScript> { book };
			ProjectDataMigrator.MigrateNonContiguousUserSplitsSeparatedByReferenceTextAlignmentSplits(books);

			var resultingBlocks = book.GetScriptBlocks();
			Assert.That(origBlocks.Count, Is.EqualTo(resultingBlocks.Count));
			Assert.That(origBlocks.Select(b => b.GetText(true)).SequenceEqual(resultingBlocks.Select(b => b.GetText(true))), Is.True);
			Assert.That(origBlocks.Select(b => b.MultiBlockQuote).SequenceEqual(resultingBlocks.Select(b => b.MultiBlockQuote)), Is.True);
			Assert.That(origBlocks.Select(b => b.SplitId).SequenceEqual(resultingBlocks.Select(b => b.SplitId)), Is.True);
			Assert.That(origBlocks.Select(b => b.MatchesReferenceText).SequenceEqual(resultingBlocks.Select(b => b.MatchesReferenceText)), Is.True);
			Assert.That(origBlocks.Select(b => b.GetPrimaryReferenceText()).SequenceEqual(resultingBlocks.Select(b => b.GetPrimaryReferenceText())), Is.True);
			Assert.That(origBlocks.Select(b => b.CharacterId).SequenceEqual(resultingBlocks.Select(b => b.CharacterId)), Is.True);
			Assert.That(origBlocks.Select(b => b.UserConfirmed).SequenceEqual(resultingBlocks.Select(b => b.UserConfirmed)), Is.True);
		}

		#region PG-1291/PG-1286
		[TestCase("{1} This is not empty. ", " ")]
		[TestCase("{1} Neither is this; but it could have been but it could have been but it could have been ", "but it could have been ")]
		[TestCase("{1} The other block's ref text was just a verse number.", "The other block's ref text was just a verse number.")]
		public void MigrateDuplicatedReferenceTextFromJoining_SingleVoiceBook_NoChanges(string block1RefText, string block2RefText)
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.MRK);
			var mark = testProject.IncludedBooks.Single();
			var poetryBlockInMarkC1V2 = mark.GetBlocksForVerse(1, 2).Single(b => b.StyleTag == "q1");
			mark.SplitBlock(poetryBlockInMarkC1V2, "2", poetryBlockInMarkC1V2.GetText(true).IndexOf("ma bigero yoni;", StringComparison.Ordinal));
			TestProject.SimulateDisambiguationForAllBooks(testProject);
			var poetryBlocksInMarkC1V2 = mark.GetBlocksForVerse(1, 2).Where(b => b.StyleTag == "q1").ToList();
			poetryBlocksInMarkC1V2[0].SetMatchedReferenceBlock(block1RefText);
			poetryBlocksInMarkC1V2[0].UserConfirmed = true;
			poetryBlocksInMarkC1V2[1].SetMatchedReferenceBlock(block2RefText);
			poetryBlocksInMarkC1V2[1].UserConfirmed = true;

			var origBlock1RefText = poetryBlocksInMarkC1V2[0].GetPrimaryReferenceText();
			var origBlock2RefText = poetryBlocksInMarkC1V2[1].GetPrimaryReferenceText();

			mark.SingleVoice = true;

			ProjectDataMigrator.MigrateDuplicatedReferenceTextFromJoining(testProject.Books);

			Assert.That(origBlock1RefText, Is.EqualTo(poetryBlocksInMarkC1V2[0].GetPrimaryReferenceText()));
			Assert.That(origBlock2RefText, Is.EqualTo(poetryBlocksInMarkC1V2[1].GetPrimaryReferenceText()));
		}

		[TestCase("{1} This is not empty.  ", " ", 1)]
		[TestCase("{1} This is not empty. ", "", 2)]
		[TestCase("{1} Neither is this; ", "but it could have been ", 3)]
		[TestCase("{1} ", "The other block's ref text was just a verse number.", 1)]
		public void MigrateDuplicatedReferenceTextFromJoining_MultiVoiceBook_RepeatedBlock2ReferenceTextRemovedFromBlock1(string block1RefText, string block2RefText,
			int numberOfTimesToJoin)
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.MRK);
			var mark = testProject.IncludedBooks.Single();
			var poetryBlockInMarkC1V2 = mark.GetBlocksForVerse(1, 2).Single(b => b.StyleTag == "q1");
			var newBlock = mark.SplitBlock(poetryBlockInMarkC1V2, "2", poetryBlockInMarkC1V2.GetText(true).IndexOf("ma bigero yoni;", StringComparison.Ordinal));
			poetryBlockInMarkC1V2.MultiBlockQuote = MultiBlockQuote.Start;
			newBlock.MultiBlockQuote = MultiBlockQuote.Continuation;
			newBlock.CharacterId = poetryBlockInMarkC1V2.CharacterId;
			var poetryBlocksInMarkC1V2 = mark.GetBlocksForVerse(1, 2).Where(b => b.StyleTag == "q1").ToList();
			poetryBlocksInMarkC1V2[0].SetMatchedReferenceBlock(block1RefText);
			poetryBlocksInMarkC1V2[0].UserConfirmed = true;
			poetryBlocksInMarkC1V2[1].SetMatchedReferenceBlock(block2RefText);
			poetryBlocksInMarkC1V2[1].UserConfirmed = true;

			var origBlock1RefText = poetryBlocksInMarkC1V2[0].GetPrimaryReferenceText();
			var origBlock2RefText = poetryBlocksInMarkC1V2[1].GetPrimaryReferenceText();

			for (int i = 0; i < numberOfTimesToJoin; i++)
				poetryBlocksInMarkC1V2[0].ReferenceBlocks.Single().CombineWith(poetryBlocksInMarkC1V2[1].ReferenceBlocks.Single());

			var joinedRefTextToVerifySetup = poetryBlocksInMarkC1V2[0].GetPrimaryReferenceText();
			if (!IsNullOrWhiteSpace(block2RefText))
				Assert.That(joinedRefTextToVerifySetup, Is.Not.EqualTo(origBlock1RefText));
			Assert.That(joinedRefTextToVerifySetup, Does.StartWith(origBlock1RefText));
			Assert.That(joinedRefTextToVerifySetup, Does.EndWith(origBlock2RefText));

			ProjectDataMigrator.MigrateDuplicatedReferenceTextFromJoining(testProject.Books);

			Assert.That(origBlock1RefText, Is.EqualTo(poetryBlocksInMarkC1V2[0].GetPrimaryReferenceText()));
			Assert.That(origBlock2RefText, Is.EqualTo(poetryBlocksInMarkC1V2[1].GetPrimaryReferenceText()));
		}

		[TestCase("{1} This is not  ", " ", " empty ", 2)]
		[TestCase("{1} This is not ", "", " empty ", 2)]
		[TestCase("{1} This is not", "", "empty", 3)]
		[TestCase("{1} This is not", "", " ", 1)]
		[TestCase("{1} Neither is this; ", "but it could have been, ", "you know ", 3)]
		[TestCase("{1} ", "The other block's ref text was just a verse number.", " ", 1)]
		[TestCase("{1}", "The other block's ref text was just a verse number.", " ", 2)]
		public void MigrateDuplicatedReferenceTextFromJoining_MultiVoiceBookWithMultipleJoinedBlocks_RepeatedBlock2And3ReferenceTextRemovedFromBlock1(
			string block1RefText, string block2RefText, string block3RefText, int numberOfTimesToJoin)
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.MRK);
			var mark = testProject.IncludedBooks.Single();
			var poetryBlockInMarkC1V2 = mark.GetBlocksForVerse(1, 2).Single(b => b.StyleTag == "q1");
			var newBlock = mark.SplitBlock(poetryBlockInMarkC1V2, "2", 8);
			poetryBlockInMarkC1V2.MultiBlockQuote = MultiBlockQuote.Start;
			newBlock.MultiBlockQuote = MultiBlockQuote.Continuation;
			newBlock.CharacterId = poetryBlockInMarkC1V2.CharacterId;
			newBlock = mark.SplitBlock(newBlock, "2", 9);
			newBlock.MultiBlockQuote = MultiBlockQuote.Continuation;
			newBlock.CharacterId = poetryBlockInMarkC1V2.CharacterId;
			var poetryBlocksInMarkC1V2 = mark.GetBlocksForVerse(1, 2).Where(b => b.StyleTag == "q1").ToList();
			poetryBlocksInMarkC1V2[0].SetMatchedReferenceBlock(block1RefText);
			poetryBlocksInMarkC1V2[0].UserConfirmed = true;
			poetryBlocksInMarkC1V2[1].SetMatchedReferenceBlock(block2RefText);
			poetryBlocksInMarkC1V2[1].UserConfirmed = true;
			poetryBlocksInMarkC1V2[2].SetMatchedReferenceBlock(block3RefText);
			poetryBlocksInMarkC1V2[2].UserConfirmed = true;

			var origBlock1RefText = poetryBlocksInMarkC1V2[0].GetPrimaryReferenceText();
			var origBlock2RefText = poetryBlocksInMarkC1V2[1].GetPrimaryReferenceText();
			var origBlock3RefText = poetryBlocksInMarkC1V2[2].GetPrimaryReferenceText();

			for (int i = 0; i < numberOfTimesToJoin; i++)
			{
				poetryBlocksInMarkC1V2[0].ReferenceBlocks.Single().CombineWith(poetryBlocksInMarkC1V2[1].ReferenceBlocks.Single());
				poetryBlocksInMarkC1V2[0].ReferenceBlocks.Single().CombineWith(poetryBlocksInMarkC1V2[2].ReferenceBlocks.Single());
			}

			var joinedRefTextToVerifySetup = poetryBlocksInMarkC1V2[0].GetPrimaryReferenceText();
			if (!IsNullOrWhiteSpace(block2RefText) || !IsNullOrWhiteSpace(block3RefText))
				Assert.That(joinedRefTextToVerifySetup, Is.Not.EqualTo(origBlock1RefText));
			Assert.That(joinedRefTextToVerifySetup, Does.StartWith(origBlock1RefText));
			Assert.That(joinedRefTextToVerifySetup.Contains(origBlock2RefText), Is.True);
			Assert.That(joinedRefTextToVerifySetup, Does.EndWith(origBlock3RefText));

			ProjectDataMigrator.MigrateDuplicatedReferenceTextFromJoining(testProject.Books);

			Assert.That(origBlock1RefText, Is.EqualTo(poetryBlocksInMarkC1V2[0].GetPrimaryReferenceText()));
			Assert.That(origBlock2RefText, Is.EqualTo(poetryBlocksInMarkC1V2[1].GetPrimaryReferenceText()));
			Assert.That(origBlock3RefText, Is.EqualTo(poetryBlocksInMarkC1V2[2].GetPrimaryReferenceText()));
		}

		[TestCase("{1} This is not  ", " ", " empty ", 2)]
		[TestCase("{1} This is not ", "", " empty ", 2)]
		[TestCase("{1} This is not", "", "empty", 3)]
		[TestCase("{1} This is not", "", " ", 1)]
		[TestCase("{1} Neither is this; ", "but it could have been, ", "you know ", 3)]
		[TestCase("{1} ", "The other block's ref text was just a verse number.", " ", 1)]
		[TestCase("{1}", "The other block's ref text was just a verse number.", " ", 2)]
		public void MigrateDuplicatedReferenceTextFromJoining_MultiVoiceBookWithMultipleJoinedBlocksAndSecondaryReferenceText_RepeatedBlock2And3ReferenceTextRemovedFromBlock1(
			string block1RefText, string block2RefText, string block3RefText, int numberOfTimesToJoin)
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.MRK);
			testProject.ReferenceText = ReferenceText.GetStandardReferenceText(ReferenceTextType.Russian);
			var mark = testProject.IncludedBooks.Single();
			var poetryBlockInMarkC1V2 = mark.GetBlocksForVerse(1, 2).Single(b => b.StyleTag == "q1");
			var newBlock = mark.SplitBlock(poetryBlockInMarkC1V2, "2", 8);
			poetryBlockInMarkC1V2.MultiBlockQuote = MultiBlockQuote.Start;
			newBlock.MultiBlockQuote = MultiBlockQuote.Continuation;
			newBlock.CharacterId = poetryBlockInMarkC1V2.CharacterId;
			newBlock = mark.SplitBlock(newBlock, "2", 9);
			newBlock.MultiBlockQuote = MultiBlockQuote.Continuation;
			newBlock.CharacterId = poetryBlockInMarkC1V2.CharacterId;
			var poetryBlocksInMarkC1V2 = mark.GetBlocksForVerse(1, 2).Where(b => b.StyleTag == "q1").ToList();
			poetryBlocksInMarkC1V2[0].SetMatchedReferenceBlock(ToCyrillic(block1RefText)).SetMatchedReferenceBlock(block1RefText);
			poetryBlocksInMarkC1V2[0].UserConfirmed = true;
			poetryBlocksInMarkC1V2[1].SetMatchedReferenceBlock(ToCyrillic(block2RefText)).SetMatchedReferenceBlock(block2RefText);
			poetryBlocksInMarkC1V2[1].UserConfirmed = true;
			poetryBlocksInMarkC1V2[2].SetMatchedReferenceBlock(ToCyrillic(block3RefText)).SetMatchedReferenceBlock(block3RefText);
			poetryBlocksInMarkC1V2[2].UserConfirmed = true;

			var origBlock1PrimaryRefText = poetryBlocksInMarkC1V2[0].GetPrimaryReferenceText();
			var origBlock1SecondaryRefText = poetryBlocksInMarkC1V2[0].ReferenceBlocks.Single().GetPrimaryReferenceText();
			var origBlock2PrimaryRefText = poetryBlocksInMarkC1V2[1].GetPrimaryReferenceText();
			var origBlock2SecondaryRefText = poetryBlocksInMarkC1V2[1].ReferenceBlocks.Single().GetPrimaryReferenceText();
			var origBlock3PrimaryRefText = poetryBlocksInMarkC1V2[2].GetPrimaryReferenceText();
			var origBlock3SecondaryRefText = poetryBlocksInMarkC1V2[2].ReferenceBlocks.Single().GetPrimaryReferenceText();

			for (int i = 0; i < numberOfTimesToJoin; i++)
			{
				poetryBlocksInMarkC1V2[0].ReferenceBlocks.Single().CombineWith(poetryBlocksInMarkC1V2[1].ReferenceBlocks.Single());
				poetryBlocksInMarkC1V2[0].ReferenceBlocks.Single().CombineWith(poetryBlocksInMarkC1V2[2].ReferenceBlocks.Single());
			}

			var joinedPrimaryRefTextToVerifySetup = poetryBlocksInMarkC1V2[0].GetPrimaryReferenceText();
			var joinedSecondaryRefTextToVerifySetup = poetryBlocksInMarkC1V2[0].ReferenceBlocks.Single().GetPrimaryReferenceText();
			if (!IsNullOrWhiteSpace(block2RefText) || !IsNullOrWhiteSpace(block3RefText))
			{
				Assert.That(joinedPrimaryRefTextToVerifySetup, Is.Not.EqualTo(origBlock1PrimaryRefText));
				Assert.That(joinedSecondaryRefTextToVerifySetup, Is.Not.EqualTo(origBlock1SecondaryRefText));
			}

			Assert.That(joinedPrimaryRefTextToVerifySetup, Does.StartWith(origBlock1PrimaryRefText));
			Assert.That(joinedPrimaryRefTextToVerifySetup.Contains(origBlock2PrimaryRefText), Is.True);
			Assert.That(joinedPrimaryRefTextToVerifySetup, Does.EndWith(origBlock3PrimaryRefText));
			Assert.That(joinedSecondaryRefTextToVerifySetup, Does.StartWith(origBlock1SecondaryRefText));
			Assert.That(joinedSecondaryRefTextToVerifySetup.Contains(origBlock2SecondaryRefText), Is.True);
			Assert.That(joinedSecondaryRefTextToVerifySetup, Does.EndWith(origBlock3SecondaryRefText));

			ProjectDataMigrator.MigrateDuplicatedReferenceTextFromJoining(testProject.Books);

			Assert.That(origBlock1PrimaryRefText, Is.EqualTo(poetryBlocksInMarkC1V2[0].GetPrimaryReferenceText()));
			Assert.That(origBlock1SecondaryRefText, Is.EqualTo(poetryBlocksInMarkC1V2[0].ReferenceBlocks.Single().GetPrimaryReferenceText()));
			Assert.That(origBlock2PrimaryRefText, Is.EqualTo(poetryBlocksInMarkC1V2[1].GetPrimaryReferenceText()));
			Assert.That(origBlock2SecondaryRefText, Is.EqualTo(poetryBlocksInMarkC1V2[1].ReferenceBlocks.Single().GetPrimaryReferenceText()));
			Assert.That(origBlock3PrimaryRefText, Is.EqualTo(poetryBlocksInMarkC1V2[2].GetPrimaryReferenceText()));
			Assert.That(origBlock3SecondaryRefText, Is.EqualTo(poetryBlocksInMarkC1V2[2].ReferenceBlocks.Single().GetPrimaryReferenceText()));
		}

		private string ToCyrillic(string english)
		{
			return english.Replace('s', 'с').Replace('n', 'н').Replace('e', 'э');
		}
		#endregion

		#region PG-1311
		[TestCase("[ ", "37", 37, 0)]
		[TestCase("?» ", "37", 37, 0)]
		[TestCase(". ", "36-37", 36, 37)]
		public void MigrateInvalidInitialStartVerseNumberFromSplitBeforePunctuation_InitialStartVerseNumberCorrected(string trailingPunctuation,
			string verseRef, int expectedInitialStartVerse, int expectedInitialEndVerse)
		{
			var origBlocks = new List<Block>();
			origBlocks.Add(CreateChapterBlock("LUK", 17, "c", true));
			origBlocks.Add(CreateBlockForVerse("Jesus", 31, "«Dœ olonœ asœmœ, uzu á nœ ye sœ ɓa sœnda, tshe ye ga mangba ye nene. ", false, 17)
				.AddVerse(32, "'E gbetshelœ 'e tœ upu nœ awo Lota kane. ")
				.AddVerse(33, "Uzu neke á tshe para awa ndœ kœgbɔndœ soro tshu; kashe tsheneke nœ nene dá she. ")
				.AddVerse(34, "Mœ sœ 'e, lœ butshɔnœ asœmœ, ayakoshe: endje za anga bale, yé anga œ sœpe. ")
				.AddVerse(35, "Ayashe bisha œ sœ kœtɔ œrœ tœ œsœnœ bale: endje za anga bale, yé anga œ sœpe. "));
			// The following call sets up the bogus data condition because it sets the Initial start/end verse numbers to 35.
			AddBlockForVerseInProgress(origBlocks, CharacterVerseData.kUnexpectedCharacter, trailingPunctuation, "p")
				.AddVerse(verseRef, "Ayambarœ nœ Yisu yu she adœke:");
			AddBlockForVerseInProgress(origBlocks, CharacterVerseData.kUnexpectedCharacter, " «Œrœnœ atamœ œ mbœrœtœ endje kpœta Gbozu?» ", "p");
			AddNarratorBlockForVerseInProgress(origBlocks, "é tshe kœgi fœ endje adœke: ", "LUK");
			AddBlockForVerseInProgress(origBlocks, CharacterVerseData.kAmbiguousCharacter, "«Osho á oko sœ tœnœ, œ kœngbɔtœ endje ɓa zœ.»", "p");
			var book = new BookScript("LUK", origBlocks, ScrVers.English);
			var books = new List<BookScript> { book };
			ProjectDataMigrator.MigrateInvalidInitialStartVerseNumberFromSplitBeforePunctuation(books);

			var resultingBlocks = book.GetScriptBlocks();
			Assert.That(origBlocks.Count, Is.EqualTo(resultingBlocks.Count));
			for (int i = 2; i < resultingBlocks.Count; i++)
			{
				Assert.That(expectedInitialStartVerse, Is.EqualTo(resultingBlocks[i].InitialStartVerseNumber));
				Assert.That(expectedInitialEndVerse, Is.EqualTo(resultingBlocks[i].InitialEndVerseNumber));
			}
		}
		#endregion

		#region PG-1334
		[TestCase(true, true)]
		[TestCase(true, false)]
		[TestCase(false, true)]
		[TestCase(false, false)]
		public void SetBlocksFollowingSectionHeadToParagraphStart_SetsIsParagraphStartTrueForVersesFollowingSectionHeads(
			bool initialValueForBlocksFollowingSection, bool initialValueForOtherBlocks)
		{
			var sectionHead = CharacterVerseData.GetStandardCharacterId("LEV", CharacterVerseData.StandardCharacter.ExtraBiblical);
			var origBlocks = new List<Block>();
			var indicesOfVersesFollowingSectionHeads = new List<int>(3);
			origBlocks.Add(CreateChapterBlock("LEV", 1, "c", true));
			origBlocks.Add(new Block("s", 1) { CharacterId = sectionHead, IsParagraphStart = initialValueForOtherBlocks });
			indicesOfVersesFollowingSectionHeads.Add(origBlocks.Count);
			origBlocks.Add(CreateNarratorBlockForVerse(1, "Verse one.", initialValueForBlocksFollowingSection, 1, "LEV"));
			origBlocks.Add(CreateBlockForVerse("God", 2, "Verse two.", initialValueForOtherBlocks).AddVerse(3, "Verse three."));
			origBlocks.Add(CreateBlockForVerse("God", 21, "Verse twenty-one.", initialValueForOtherBlocks));
			origBlocks.Add(new Block("s2", 1) { CharacterId = sectionHead, IsParagraphStart = initialValueForOtherBlocks });
			indicesOfVersesFollowingSectionHeads.Add(origBlocks.Count);
			AddBlockForVerseInProgress(origBlocks, "Moses", "That section head was in the middle of a verse.", "p").IsParagraphStart = initialValueForBlocksFollowingSection;
			AddBlockForVerseInProgress(origBlocks, "Aaron", "Some other paragraph.", "p").IsParagraphStart = initialValueForOtherBlocks;
			origBlocks.Add(CreateChapterBlock("LEV", 23, "c", true));
			origBlocks.Add(new Block("s", 23) { CharacterId = sectionHead, IsParagraphStart = initialValueForOtherBlocks });
			indicesOfVersesFollowingSectionHeads.Add(origBlocks.Count);
			origBlocks.Add(CreateNarratorBlockForVerse(1, "Verse one of chapter twenty-three.", initialValueForBlocksFollowingSection, 23, "LEV"));
			origBlocks.Add(CreateBlockForVerse("God", 2, "Verse two of chapter twenty-three.", initialValueForOtherBlocks, 23).AddVerse(3, "Verse three."));

			var book = new BookScript("LEV", origBlocks, ScrVers.English);
			var books = new List<BookScript> { book };
			ProjectDataMigrator.SetBlocksFollowingSectionHeadToParagraphStart(books);

			var resultingBlocks = book.GetScriptBlocks();
			Assert.That(origBlocks.Count, Is.EqualTo(resultingBlocks.Count));
			for (var i = 0; i < resultingBlocks.Count; i++)
			{
				var block = resultingBlocks[i];
				if (block.IsChapterAnnouncement)
					continue;
				if (indicesOfVersesFollowingSectionHeads.Contains(i))
					Assert.That(block.IsParagraphStart, Is.True);
				else
					Assert.That(initialValueForOtherBlocks, Is.EqualTo(block.IsParagraphStart));
			}
		}
		#endregion

		private void SetDummyReferenceText(IEnumerable<Block> blocks)
		{
			foreach (var block in blocks)
				block.SetMatchedReferenceBlock(GetDummyRefBlock(block));
		}

		private Block GetDummyRefBlock(Block vernBlock)
		{
			var refBlock = new Block(vernBlock.StyleTag, vernBlock.ChapterNumber, vernBlock.InitialStartVerseNumber, vernBlock.InitialEndVerseNumber)
				{ BlockElements = new List<BlockElement>() };
			foreach (var element in vernBlock.BlockElements)
			{
				if (element is Verse verse)
					refBlock.BlockElements.Add(new Verse(verse.Number));
				else if (element is ScriptText text)
					refBlock.BlockElements.Add(new ScriptText(text.Content.Reverse().ToString()));
			}
			return refBlock;
		}

		private Block CreateTestBlock(int index, MultiBlockQuote multiBlockQuote)
		{
			string indexStr = index.ToString();
			return new Block
			{
				BlockElements = new List<BlockElement>
				{
					new Verse(indexStr),
					new ScriptText("Text of verse " + indexStr + ". "),
				},
				MultiBlockQuote = multiBlockQuote,
			};
		}

		private Block CreateTestBlock(string characterId)
		{
			return new Block
			{
				CharacterIdInScript = characterId,
				CharacterId = characterId,
			};
		}

		private Block CreateChapterBlock(string bookId, int chapter, string styleTag, bool omitLabel = false)
		{
			var chapterVerse = CharacterVerseData.GetStandardCharacterId(bookId, CharacterVerseData.StandardCharacter.BookOrChapter);

			return new Block
			{
				CharacterId = chapterVerse,
				CharacterIdInScript = chapterVerse,
				StyleTag = styleTag,
				BlockElements = new List<BlockElement> {new ScriptText((omitLabel ? "" : "Chapter ") + chapter)}
			};
		}
	}
}
