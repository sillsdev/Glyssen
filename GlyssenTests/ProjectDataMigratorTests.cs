using System.Collections.Generic;
using System.Linq;
using Glyssen;
using Glyssen.Character;
using Glyssen.Shared;
using NUnit.Framework;
using SIL.Reflection;
using SIL.Scripture;

namespace GlyssenTests
{
	class ProjectDataMigratorTests
	{
		[TestFixtureSetUp]
		public void FixtureSetUp()
		{
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = null;
			CharacterDetailData.TabDelimitedCharacterDetailData = null;
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			TestProject.DeleteTestProjectFolder();
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

			var book = new BookScript("MAT", new List<Block> { block1, block2 });
			var books = new List<BookScript> { book };
			ProjectDataMigrator.MigrateInvalidMultiBlockQuoteData(books);

			Assert.AreEqual(2, book.Blocks.Count);
			Assert.AreEqual(block1Original.GetText(true), block1.GetText(true));
			Assert.AreEqual(block2Original.GetText(true), block2.GetText(true));
			Assert.AreEqual(MultiBlockQuote.Start, block1.MultiBlockQuote);
			Assert.AreEqual(lastBlockMultiBlockQuote, block2.MultiBlockQuote);
		}

		[Test]
		public void MigrateInvalidMultiBlockQuoteDataToVersion88_SplitDoesntStartWithMultiBlockStart_DataUnchanged()
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

			var book = new BookScript("MAT", new List<Block> { block1, block2, block3 });
			var books = new List<BookScript> { book };
			ProjectDataMigrator.MigrateInvalidMultiBlockQuoteData(books);

			Assert.AreEqual(3, book.Blocks.Count);
			Assert.AreEqual(block1Original.GetText(true), block1.GetText(true));
			Assert.AreEqual(block2Original.GetText(true), block2.GetText(true));
			Assert.AreEqual(block3Original.GetText(true), block3.GetText(true));
			Assert.AreEqual(MultiBlockQuote.None, block1.MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Start, block2.MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Continuation, block3.MultiBlockQuote);
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

			var book = new BookScript("MAT", new List<Block> { block1, block2 });
			var books = new List<BookScript> { book };
			ProjectDataMigrator.MigrateInvalidMultiBlockQuoteData(books);

			Assert.AreEqual(2, book.Blocks.Count);
			Assert.AreEqual(block1Original.GetText(true), block1.GetText(true));
			Assert.AreEqual(block2Original.GetText(true), block2.GetText(true));
			Assert.AreEqual(MultiBlockQuote.None, block1.MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Start, block2.MultiBlockQuote);
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

			var book = new BookScript("MAT", new List<Block> {block1, block2, block3});
			var books = new List<BookScript> { book };
			ProjectDataMigrator.MigrateInvalidMultiBlockQuoteData(books);

			Assert.AreEqual(3, book.Blocks.Count);
			Assert.AreEqual(block1Original.GetText(true), block1.GetText(true));
			Assert.AreEqual(block2Original.GetText(true), block2.GetText(true));
			Assert.AreEqual(block3Original.GetText(true), block3.GetText(true));
			Assert.AreEqual(MultiBlockQuote.None, block1.MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Start, block2.MultiBlockQuote);
			Assert.AreEqual(lastBlockMultiBlockQuote, block3.MultiBlockQuote);
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

			var book = new BookScript("MAT", new List<Block> {block1, block2, block3, block4});

		var books = new List<BookScript> { book };
			ProjectDataMigrator.MigrateInvalidMultiBlockQuoteData(books);

			Assert.AreEqual(4, book.Blocks.Count);
			Assert.AreEqual(block1Original.GetText(true), block1.GetText(true));
			Assert.AreEqual(block2Original.GetText(true), block2.GetText(true));
			Assert.AreEqual(block3Original.GetText(true), block3.GetText(true));
			Assert.AreEqual(block4Original.GetText(true), block4.GetText(true));
			Assert.AreEqual(MultiBlockQuote.None, block1.MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.None, block2.MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Start, block3.MultiBlockQuote);
			Assert.AreEqual(lastBlockMultiBlockQuote, block4.MultiBlockQuote);
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

			var book = new BookScript("MAT", new List<Block> { block1, block2, block3, block4, block5 });
			var books = new List<BookScript> { book };
			ProjectDataMigrator.MigrateInvalidMultiBlockQuoteData(books);

			Assert.AreEqual(5, book.Blocks.Count);
			Assert.AreEqual(block1Original.GetText(true), block1.GetText(true));
			Assert.AreEqual(block2Original.GetText(true), block2.GetText(true));
			Assert.AreEqual(block3Original.GetText(true), block3.GetText(true));
			Assert.AreEqual(block4Original.GetText(true), block4.GetText(true));
			Assert.AreEqual(block5Original.GetText(true), block5.GetText(true));
			Assert.AreEqual(MultiBlockQuote.Start, block1.MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.ChangeOfDelivery, block2.MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Start, block3.MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.ChangeOfDelivery, block4.MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Continuation, block5.MultiBlockQuote);
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

			var book = new BookScript("MAT", new List<Block> { block1, block2, block3, block4 });
			var books = new List<BookScript> { book };
			ProjectDataMigrator.CleanUpOrphanedMultiBlockQuoteStati(books);

			Assert.AreEqual(4, book.Blocks.Count);
			Assert.AreEqual(block1Original.GetText(true), block1.GetText(true));
			Assert.AreEqual(block2Original.GetText(true), block2.GetText(true));
			Assert.AreEqual(block3Original.GetText(true), block3.GetText(true));
			Assert.AreEqual(block4Original.GetText(true), block4.GetText(true));
			Assert.AreEqual(MultiBlockQuote.None, block1.MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Start, block2.MultiBlockQuote);
			Assert.AreEqual(continuingStatus, block3.MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.None, block4.MultiBlockQuote);
		}

		[TestCase(MultiBlockQuote.Continuation)]
		[TestCase(MultiBlockQuote.ChangeOfDelivery)]
		public void CleanUpOrphanedMultiBlockQuoteStati_FirstBlockIsNotNoneOrStart_ChangedToNone(MultiBlockQuote continuingStatus)
		{
			var block1 = CreateTestBlock(1, continuingStatus);
			var block2 = CreateTestBlock(2, MultiBlockQuote.None);

			var block1Original = block1.Clone();
			var block2Original = block2.Clone();

			var book = new BookScript("MAT", new List<Block> { block1, block2 });
			var books = new List<BookScript> { book };
			ProjectDataMigrator.CleanUpOrphanedMultiBlockQuoteStati(books);

			Assert.AreEqual(2, book.Blocks.Count);
			Assert.AreEqual(block1Original.GetText(true), block1.GetText(true));
			Assert.AreEqual(block2Original.GetText(true), block2.GetText(true));
			Assert.AreEqual(MultiBlockQuote.None, block1.MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.None, block2.MultiBlockQuote);
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

			var book = new BookScript("MAT", new List<Block> { block1, block2, block3 });
			var books = new List<BookScript> { book };
			ProjectDataMigrator.CleanUpOrphanedMultiBlockQuoteStati(books);

			Assert.AreEqual(3, book.Blocks.Count);
			Assert.AreEqual(block1Original.GetText(true), block1.GetText(true));
			Assert.AreEqual(block2Original.GetText(true), block2.GetText(true));
			Assert.AreEqual(block3Original.GetText(true), block3.GetText(true));
			Assert.AreEqual(MultiBlockQuote.None, block1.MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.None, block2.MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.None, block3.MultiBlockQuote);
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

			var book = new BookScript("MAT", new List<Block> { block1, block2, block3, block4 });
			var books = new List<BookScript> { book };
			ProjectDataMigrator.CleanUpOrphanedMultiBlockQuoteStati(books);

			Assert.AreEqual(4, book.Blocks.Count);
			Assert.AreEqual(block1Original.GetText(true), block1.GetText(true));
			Assert.AreEqual(block2Original.GetText(true), block2.GetText(true));
			Assert.AreEqual(block3Original.GetText(true), block3.GetText(true));
			Assert.AreEqual(block4Original.GetText(true), block4.GetText(true));
			Assert.AreEqual(MultiBlockQuote.None, block1.MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.None, block2.MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Start, block3.MultiBlockQuote);
			Assert.AreEqual(continuingStatus, block4.MultiBlockQuote);
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

			var book = new BookScript("MAT", new List<Block> { block1, block2, block3, block4 });
			var books = new List<BookScript> { book };
			ProjectDataMigrator.CleanUpOrphanedMultiBlockQuoteStati(books);

			Assert.AreEqual(4, book.Blocks.Count);
			Assert.AreEqual(block1Original.GetText(true), block1.GetText(true));
			Assert.AreEqual(block2Original.GetText(true), block2.GetText(true));
			Assert.AreEqual(block3Original.GetText(true), block3.GetText(true));
			Assert.AreEqual(block4Original.GetText(true), block4.GetText(true));
			Assert.AreEqual(MultiBlockQuote.None, block1.MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.None, block2.MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.None, block3.MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.None, block4.MultiBlockQuote);
		}

		[Test]
		public void CleanUpOrphanedMultiBlockQuoteStati_NoneFollowedByNonStart_NonStartChangedToNone()
		{
			var block1 = CreateTestBlock(1, MultiBlockQuote.None);
			var block2 = CreateTestBlock(2, MultiBlockQuote.Continuation);

			var block1Original = block1.Clone();
			var block2Original = block2.Clone();

			var book = new BookScript("MAT", new List<Block> { block1, block2 });
			var books = new List<BookScript> { book };
			ProjectDataMigrator.CleanUpOrphanedMultiBlockQuoteStati(books);

			Assert.AreEqual(2, book.Blocks.Count);
			Assert.AreEqual(block1Original.GetText(true), block1.GetText(true));
			Assert.AreEqual(block2Original.GetText(true), block2.GetText(true));
			Assert.AreEqual(MultiBlockQuote.None, block1.MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.None, block2.MultiBlockQuote);
		}

		[Test]
		public void CleanUpOrphanedMultiBlockQuoteStati_LastBlockIsStart_StartChangedToNone()
		{
			var block1 = CreateTestBlock(1, MultiBlockQuote.None);
			var block2 = CreateTestBlock(2, MultiBlockQuote.Start);

			var block1Original = block1.Clone();
			var block2Original = block2.Clone();

			var book = new BookScript("MAT", new List<Block> { block1, block2 });
			var books = new List<BookScript> { book };
			ProjectDataMigrator.CleanUpOrphanedMultiBlockQuoteStati(books);

			Assert.AreEqual(2, book.Blocks.Count);
			Assert.AreEqual(block1Original.GetText(true), block1.GetText(true));
			Assert.AreEqual(block2Original.GetText(true), block2.GetText(true));
			Assert.AreEqual(MultiBlockQuote.None, block1.MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.None, block2.MultiBlockQuote);
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

			var book = new BookScript("MAT", new List<Block> { block1, block2, block3 });
			var books = new List<BookScript> { book };
			ProjectDataMigrator.CleanUpOrphanedMultiBlockQuoteStati(books);

			Assert.AreEqual(3, book.Blocks.Count);
			Assert.AreEqual(block1Original.GetText(true), block1.GetText(true));
			Assert.AreEqual(block2Original.GetText(true), block2.GetText(true));
			Assert.AreEqual(block3Original.GetText(true), block3.GetText(true));
			Assert.AreEqual(MultiBlockQuote.None, block1.MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.None, block2.MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.None, block3.MultiBlockQuote);
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

			var book = new BookScript("MAT", blocks.Select(b => b.Clone()));
			Assert.IsFalse(blocks.SequenceEqual(book.Blocks), "Sanity check: blocks should have been cloned.");
			var books = new List<BookScript> { book };
			ProjectDataMigrator.CleanUpMultiBlockQuotesAssignedToNarrator(books);

			Assert.IsTrue(blocks.SequenceEqual(book.Blocks, new SplitBlockComparer()), "Block content should not have changed!");
			Assert.AreEqual(2, book.Blocks.Count(b => b.MultiBlockQuote != MultiBlockQuote.None && b.CharacterId == "Jesus"),
				"Multi-block quote chain for Jesus should not have been affected.");
			Assert.AreEqual(5, book.Blocks.Count(b => b.CharacterId == narrator),
				"Narrator should still be assigned to the 5 blocks not assigned to Jesus.");
			Assert.IsTrue(book.Blocks.Where(b => b.CharacterId == narrator).All(b => b.MultiBlockQuote == MultiBlockQuote.None),
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
			blocks.Last().CharacterId = CharacterVerseData.kUnknownCharacter;
			blocks.Last().SetMatchedReferenceBlock(new Block("p", 1, 2) { CharacterId = "Phil", Delivery = "Panicky" });
			blocks.Add(CreateTestBlock(3, MultiBlockQuote.Start));
			blocks.Last().CharacterId = "Jesus";
			blocks.Add(CreateTestBlock(4, MultiBlockQuote.Continuation));
			blocks.Last().CharacterId = CharacterVerseData.kAmbiguousCharacter;
			blocks.Last().SetMatchedReferenceBlock(new Block("p", 1, 2) { CharacterId = "Phil/Kelsy", CharacterIdOverrideForScript = "Kelsy" });

			var book = new BookScript("MAT", blocks.Select(b => b.Clone()));
			Assert.IsFalse(blocks.SequenceEqual(book.Blocks), "Sanity check: blocks should have been cloned.");
			var books = new List<BookScript> { book };
			ProjectDataMigrator.ResolveUnclearCharacterIdsForVernBlocksMatchedToRefBlocks(books, ScrVers.English);

			Assert.IsTrue(blocks.SequenceEqual(book.Blocks, new SplitBlockComparer()), "Block content should not have changed!");
			Assert.AreEqual(narrator, book.GetScriptBlocks()[0].CharacterId);
			Assert.AreEqual("Phil", book.GetScriptBlocks()[1].CharacterId);
			Assert.AreEqual("Phil", book.GetScriptBlocks()[1].CharacterIdInScript);
			Assert.AreEqual("Panicky", book.GetScriptBlocks()[1].Delivery);
			Assert.AreEqual("Jesus", book.GetScriptBlocks()[2].CharacterId);
			Assert.AreEqual("Phil/Kelsy", book.GetScriptBlocks()[3].CharacterId);
			Assert.AreEqual("Kelsy", book.GetScriptBlocks()[3].CharacterIdInScript);
			Assert.IsNull(book.GetScriptBlocks()[3].Delivery);
		}

		[Test]
		public void ResolveUnclearCharacterIdsForVernBlocksMatchedToRefBlocks_SomeUnclearBlocksInQuoteChainMatched_AllBlocksInEntireChainSetToSameCharacter()
		{
			var blocks = new List<Block>(5);
			var narrator = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator);
			blocks.Add(CreateTestBlock(1, MultiBlockQuote.None));
			blocks.Last().SetNonDramaticCharacterId(narrator);
			blocks.Add(CreateTestBlock(2, MultiBlockQuote.Start));
			blocks.Last().CharacterId = CharacterVerseData.kUnknownCharacter;
			blocks.Last().SetMatchedReferenceBlock(new Block("p", 1, 2) { CharacterId = "Phil", Delivery = "panicky" }
				.AddVerse(2, "This is the start of verse two."));
			blocks.Add(new Block("p", 1, 2) { MultiBlockQuote = MultiBlockQuote.Continuation, CharacterId = CharacterVerseData.kUnknownCharacter }
				.AddText("Esto es el resto de ello."));
			blocks.Last().SetMatchedReferenceBlock(new Block("p", 1, 2) { CharacterId = "Phil", Delivery = "calmer" }
				.AddText("This is the rest of it."));
			blocks.Add(CreateTestBlock(3, MultiBlockQuote.Continuation));
			blocks.Last().CharacterId = CharacterVerseData.kUnknownCharacter;
			blocks.Add(CreateTestBlock(4, MultiBlockQuote.None));
			blocks.Last().CharacterId = "the people";

			var book = new BookScript("MAT", blocks.Select(b => b.Clone()));
			Assert.IsFalse(blocks.SequenceEqual(book.Blocks), "Sanity check: blocks should have been cloned.");
			var books = new List<BookScript> { book };
			ProjectDataMigrator.ResolveUnclearCharacterIdsForVernBlocksMatchedToRefBlocks(books, ScrVers.English);

			Assert.IsTrue(blocks.SequenceEqual(book.Blocks, new SplitBlockComparer()), "Block content should not have changed!");
			Assert.AreEqual(narrator, book.GetScriptBlocks()[0].CharacterId);
			Assert.AreEqual("Phil", book.GetScriptBlocks()[1].CharacterId);
			Assert.AreEqual("panicky", book.GetScriptBlocks()[1].Delivery);
			Assert.AreEqual("Phil", book.GetScriptBlocks()[2].CharacterId);
			Assert.AreEqual("calmer", book.GetScriptBlocks()[2].Delivery);
			Assert.AreEqual("Phil", book.GetScriptBlocks()[3].CharacterId);
			Assert.AreEqual("calmer", book.GetScriptBlocks()[3].Delivery);
			Assert.AreEqual("the people", book.GetScriptBlocks()[4].CharacterId);
			Assert.IsNull(book.GetScriptBlocks()[4].Delivery);
		}

		[Test]
		public void MigrateInvalidCharacterIdForScriptData_ValidDataWithNoMultipleCharacterIds_Unchanged()
		{
			var block1 = CreateTestBlock("Andrew");
			var block2 = CreateTestBlock("Peter");
			var book = new BookScript("MAT", new List<Block> { block1, block2 });
			var books = new List<BookScript> { book };
			ProjectDataMigrator.MigrateInvalidCharacterIdForScriptData(books);

			Assert.AreEqual("Andrew", block1.CharacterId);
			Assert.AreEqual("Andrew", block1.CharacterIdInScript);
			Assert.AreEqual("Peter", block2.CharacterId);
			Assert.AreEqual("Peter", block2.CharacterIdInScript);
		}

		[TestCase(CharacterVerseData.kAmbiguousCharacter)]
		[TestCase(CharacterVerseData.kUnknownCharacter)]
		public void MigrateInvalidCharacterIdForScriptDataToVersion88_CharacterIdUnclearAndCharacterIdInScriptNotNull_CharacterIdInScriptSetToNull(string unclearCharacterId)
		{
			var block1 = CreateTestBlock("Andrew");
			block1.UserConfirmed = true;
			block1.CharacterId = unclearCharacterId;
			Assert.AreEqual(unclearCharacterId, block1.CharacterId);
			Assert.AreEqual("Andrew", block1.CharacterIdInScript);

			var block2 = CreateTestBlock("Peter");
			block2.UserConfirmed = true;
			block2.CharacterId = unclearCharacterId;
			Assert.AreEqual(unclearCharacterId, block2.CharacterId);
			Assert.AreEqual("Peter", block2.CharacterIdInScript);

			var book = new BookScript("MAT", new List<Block> { block1, block2 });
			var books = new List<BookScript> { book };

			Assert.True(block1.UserConfirmed);
			Assert.True(block2.UserConfirmed);

			ProjectDataMigrator.MigrateInvalidCharacterIdForScriptData(books);

			Assert.AreEqual(unclearCharacterId, block1.CharacterId);
			Assert.AreEqual(unclearCharacterId, block1.CharacterIdInScript);
			Assert.False(block1.UserConfirmed);
			Assert.AreEqual(unclearCharacterId, block2.CharacterId);
			Assert.AreEqual(unclearCharacterId, block2.CharacterIdInScript);
			Assert.False(block2.UserConfirmed);
		}

		[Test]
		public void MigrateInvalidCharacterIdForScriptData_ValidDataWithMultipleCharacterIds_Unchanged()
		{
			var block1 = CreateTestBlock("Andrew/James");
			block1.CharacterIdOverrideForScript = "James";
			var block2 = CreateTestBlock("Peter");
			var book = new BookScript("MAT", new List<Block> { block1, block2 });
			var books = new List<BookScript> { book };
			ProjectDataMigrator.MigrateInvalidCharacterIdForScriptData(books);

			Assert.AreEqual("Andrew/James", block1.CharacterId);
			Assert.AreEqual("James", block1.CharacterIdInScript);
			Assert.AreEqual("Peter", block2.CharacterId);
			Assert.AreEqual("Peter", block2.CharacterIdInScript);
		}

		[Test]
		public void MigrateInvalidCharacterIdForScriptData_NarratorBlocksWithNonNullCharacterIdInScript_CharacterIdInScriptSetToNull()
		{
			var bcMat = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.BookOrChapter);
			var narratorMat = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator);
			var block1 = CreateTestBlock("Andrew");
			block1.UserConfirmed = false;
			block1.CharacterId = bcMat;
			Assert.AreEqual(bcMat, block1.CharacterId);
			Assert.AreEqual("Andrew", block1.CharacterIdInScript);

			var block2 = CreateTestBlock("Peter");
			block2.UserConfirmed = true;
			block2.CharacterId = narratorMat;
			Assert.AreEqual(narratorMat, block2.CharacterId);
			Assert.AreEqual("Peter", block2.CharacterIdInScript);

			var book = new BookScript("MAT", new List<Block> { block1, block2 });
			var books = new List<BookScript> { book };

			Assert.False(block1.UserConfirmed);
			Assert.True(block2.UserConfirmed);

			ProjectDataMigrator.MigrateInvalidCharacterIdForScriptData(books);

			Assert.AreEqual(bcMat, block1.CharacterId);
			Assert.AreEqual(bcMat, block1.CharacterIdInScript);
			Assert.IsNull(block1.CharacterIdOverrideForScript);
			Assert.False(block1.UserConfirmed);
			Assert.AreEqual(narratorMat, block2.CharacterId);
			Assert.AreEqual(narratorMat, block2.CharacterIdInScript);
			Assert.IsNull(block2.CharacterIdOverrideForScript);
			Assert.True(block2.UserConfirmed);
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
			Assert.True(johnSpeakingInRev714.UserConfirmed);

			Assert.AreEqual(1, ProjectDataMigrator.MigrateDeprecatedCharacterIds(testProject));

			Assert.AreEqual(CharacterVerseData.kAmbiguousCharacter, johnSpeakingInRev714.CharacterId);
			Assert.AreEqual(CharacterVerseData.kAmbiguousCharacter, johnSpeakingInRev714.CharacterIdInScript);
			Assert.False(johnSpeakingInRev714.UserConfirmed);
			Assert.AreEqual("elders, one of the", elderSpeakingInRev714.CharacterId);
			Assert.AreEqual("elders, one of the", elderSpeakingInRev714.CharacterIdInScript);
		}

		[Test]
		public void MigrateDeprecatedCharacterIds_CharacterIdReplacedWithSingleId_CharacterIdInScriptSetToReplacementId()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.REV);
			TestProject.SimulateDisambiguationForAllBooks(testProject);
			var angelsSpeakingInRev712 = testProject.IncludedBooks.Single().GetBlocksForVerse(7, 12).ElementAt(1);
			angelsSpeakingInRev712.SetCharacterIdAndCharacterIdInScript("tons of angelic beings", 66);

			Assert.AreEqual(1, ProjectDataMigrator.MigrateDeprecatedCharacterIds(testProject));

			Assert.AreEqual("angels, all the", angelsSpeakingInRev712.CharacterId);
			Assert.AreEqual("angels, all the", angelsSpeakingInRev712.CharacterIdInScript);
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

			Assert.AreEqual(singersInRev59.Count, ProjectDataMigrator.MigrateDeprecatedCharacterIds(testProject));

			Assert.True(singersInRev59.All(b => b.CharacterId == "living creature, first/living creature, second/living creature, third/living creature, fourth/twenty-four elders" &&
				b.Delivery == "singing"));
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

			Assert.AreEqual(1, ProjectDataMigrator.MigrateDeprecatedCharacterIds(testProject));
			Assert.AreEqual(expectedDelivery, block.Delivery);
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

			Assert.AreEqual(1, ProjectDataMigrator.MigrateDeprecatedCharacterIds(testProject));
			Assert.AreEqual(false, block.UserConfirmed);
		}

		[Test]
		public void MigrateDeprecatedCharacterIds_ExistingAmbiguousUserNotConfirmed_NoChanges()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.LUK);
			TestProject.SimulateDisambiguationForAllBooks(testProject);
			var block = testProject.IncludedBooks.Single().GetBlocksForVerse(18, 39).Last();
			block.CharacterId = CharacterVerseData.kAmbiguousCharacter;
			block.UserConfirmed = false;

			Assert.AreEqual(0, ProjectDataMigrator.MigrateDeprecatedCharacterIds(testProject));
			Assert.AreEqual(false, block.UserConfirmed);
		}

		[Test]
		public void MigrateDeprecatedCharacterIds_OneMemberOfMultiCharacterIdChanged_CharacterIdInScriptSetToReplacementId()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.REV);
			TestProject.SimulateDisambiguationForAllBooks(testProject);
			var singersInRev59 = testProject.IncludedBooks.Single().GetBlocksForVerse(5, 9).Skip(1).ToList();
			foreach (var block in singersInRev59)
				block.SetCharacterIdAndCharacterIdInScript("cuatro living creatures/twenty-four elders", 66);

			Assert.AreEqual(singersInRev59.Count, ProjectDataMigrator.MigrateDeprecatedCharacterIds(testProject));

			Assert.True(singersInRev59.All(b => b.CharacterId == "living creature, first/living creature, second/living creature, third/living creature, fourth/twenty-four elders" &&
				b.CharacterIdInScript == "living creature, first"));
		}

		[Test]
		public void MigrateDeprecatedCharacterIds_CharacterIdRemoved_NoOtherCharactersInVerse_CharacterIdInScriptSetToUnknown()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.REV);
			TestProject.SimulateDisambiguationForAllBooks(testProject);
			var blockInRev43 = testProject.IncludedBooks.Single().GetBlocksForVerse(4, 3).First();
			blockInRev43.SetCharacterIdAndCharacterIdInScript("angels, all the", 66);
			blockInRev43.CharacterIdOverrideForScript = "angels, all, the";
			Assert.AreEqual("angels, all, the", blockInRev43.CharacterIdInScript);

			Assert.AreEqual(1, ProjectDataMigrator.MigrateDeprecatedCharacterIds(testProject));

			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, blockInRev43.CharacterId);
			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, blockInRev43.CharacterIdInScript);
		}

		[Test]
		public void MigrateDeprecatedCharacterIds_CharacterIdRemoved_MultipleCharactersStillInVerse_CharacterIdInScriptSetToAmbiguous()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.REV);
			TestProject.SimulateDisambiguationForAllBooks(testProject);
			var blockInRev13V10 = testProject.IncludedBooks.Single().GetBlocksForVerse(13, 10).First();
			blockInRev13V10.SetCharacterIdAndCharacterIdInScript("angels, all the", 66);
			blockInRev13V10.CharacterIdOverrideForScript = "angels, all, the";
			Assert.AreEqual("angels, all, the", blockInRev13V10.CharacterIdInScript);

			Assert.AreEqual(1, ProjectDataMigrator.MigrateDeprecatedCharacterIds(testProject));

			Assert.AreEqual(CharacterVerseData.kAmbiguousCharacter, blockInRev13V10.CharacterId);
			Assert.AreEqual(CharacterVerseData.kAmbiguousCharacter, blockInRev13V10.CharacterIdInScript);
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
			testProject.ProjectCharacterVerseData.Add(new CharacterVerse(new BCVRef(66, 7, 11), "peter", "", "", true));
			unexpectedPeterInRev711.SetCharacterIdAndCharacterIdInScript("peter", 66);

			Assert.AreEqual(1, ProjectDataMigrator.MigrateDeprecatedCharacterIds(testProject));

			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, unexpectedPeterInRev711.CharacterId);
			Assert.IsFalse(testProject.ProjectCharacterVerseData.Any());
		}

		[Test]
		public void MigrateDeprecatedCharacterIds_FirstVerseInQuoteIsUnexpectedForCharacter_CharacterIdNotSetToUnknown()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.JUD);
			TestProject.SimulateDisambiguationForAllBooks(testProject);

			var bookScript = testProject.IncludedBooks.Single();
			var verses13and14Block = bookScript.GetBlocksForVerse(1, 13).Single();
			var originalVerse14Blocks = bookScript.GetBlocksForVerse(1, 14);

			// Use reflection to get around a check to ensure we don't do this in production code
			List<Block> blocks = (List<Block>)ReflectionHelper.GetField(bookScript, "m_blocks");
			int blockCount = (int)ReflectionHelper.GetField(bookScript, "m_blockCount");

			//Combine verse 13 and 14 blocks
			foreach (var block in originalVerse14Blocks)
			{
				verses13and14Block.BlockElements.AddRange(block.BlockElements);

				blocks.Remove(block);
				blockCount--;
			}
			ReflectionHelper.SetField(bookScript, "m_blockCount", blockCount);

			verses13and14Block.CharacterId = "Enoch";

			//Setup check
			Assert.AreEqual("Enoch", verses13and14Block.CharacterId);

			//SUT
			Assert.AreEqual(0, ProjectDataMigrator.MigrateDeprecatedCharacterIds(testProject));

			Assert.AreEqual("Enoch", verses13and14Block.CharacterId);
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
			demonSpeakingInMrk59.CharacterIdOverrideForScript = null;
			demonSpeakingInMrk59.UserConfirmed = true;

			Assert.AreEqual(1, ProjectDataMigrator.MigrateInvalidCharacterIdsWithoutCharacterIdInScriptOverrides(testProject));

			Assert.AreEqual("Jesus", jesusSpeakingInMrk59.CharacterId);
			Assert.AreEqual("Jesus", jesusSpeakingInMrk59.CharacterIdInScript);
			Assert.IsTrue(jesusSpeakingInMrk59.UserConfirmed);
			Assert.AreEqual("demons (Legion)/man delivered from Legion of demons", demonSpeakingInMrk59.CharacterId);
			Assert.AreEqual("demons (Legion)", demonSpeakingInMrk59.CharacterIdInScript);
			Assert.IsTrue(demonSpeakingInMrk59.UserConfirmed);
		}

		// The only place in the NT where this is likely to have occurred wwas John 11:34, but we don't have John test data, and it didn't seem worth it.
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
		//	marySpeakingInJhn1134.CharacterIdOverrideForScript = null;
		//	marySpeakingInJhn1134.UserConfirmed = true;

		//	Assert.AreEqual(1, ProjectDataMigrator.MigrateInvalidCharacterIdsWithoutCharacterIdInScriptOverrides(testProject));

		//	Assert.AreEqual("Jesus", jesusSpeakingInJhn1134.CharacterId);
		//	Assert.AreEqual("Jesus", jesusSpeakingInJhn1134.CharacterIdInScript);
		//	Assert.IsTrue(jesusSpeakingInJhn1134.UserConfirmed);
		//	Assert.AreEqual("Jews, the/Mary, sister of Martha/Martha", marySpeakingInJhn1134.CharacterId);
		//	Assert.AreEqual("Mary, sister of Martha", marySpeakingInJhn1134.CharacterIdInScript);
		//	Assert.IsTrue(marySpeakingInJhn1134.UserConfirmed);
		//}

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
				}
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
				}
			);

			var books = new List<BookScript> { genesis, matthew };
			ProjectDataMigrator.SetBookIdForChapterBlocks(books);

			Assert.IsFalse(books.SelectMany(book => book.Blocks).Where(bl => bl.StyleTag == "c" || bl.StyleTag == "cl").Any(bl => bl.BookCode == null));
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

			var book = new BookScript("MAT", blocks);
			var books = new List<BookScript> { book };
			ProjectDataMigrator.MigrateNonContiguousUserSplitsSeparatedByReferenceTextAlignmentSplits(books);

			var resultingBlocks = book.GetScriptBlocks();
			Assert.AreEqual(origBlocks.Count, resultingBlocks.Count);
			Assert.IsTrue(origBlocks.Select(b => b.GetText(true)).SequenceEqual(resultingBlocks.Select(b => b.GetText(true))));
			Assert.IsTrue(origBlocks.Select(b => b.MultiBlockQuote).SequenceEqual(resultingBlocks.Select(b => b.MultiBlockQuote)));
			Assert.IsTrue(resultingBlocks.All(b => b.SplitId == 0));
			Assert.IsTrue(origBlocks.Select(b => b.MatchesReferenceText).SequenceEqual(resultingBlocks.Select(b => b.MatchesReferenceText)));
			Assert.IsTrue(origBlocks.Select(b => b.GetPrimaryReferenceText()).SequenceEqual(resultingBlocks.Select(b => b.GetPrimaryReferenceText())));
			Assert.IsTrue(origBlocks.Select(b => b.CharacterId).SequenceEqual(resultingBlocks.Select(b => b.CharacterId)));
			Assert.IsTrue(origBlocks.Select(b => b.UserConfirmed).SequenceEqual(resultingBlocks.Select(b => b.UserConfirmed)));
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

				books.Add(new BookScript(bookId, blocks));

			} while (books.Count < books.Capacity);

			ProjectDataMigrator.MigrateNonContiguousUserSplitsSeparatedByReferenceTextAlignmentSplits(books);

			for (int i = 0; i < books.Count; i++)
			{
				var origBookBlocks = origBlocks[i];
				var resultingBlocks = books[i].GetScriptBlocks();
				Assert.AreEqual(origBookBlocks.Count, resultingBlocks.Count);
				Assert.IsTrue(origBookBlocks.Select(b => b.GetText(true)).SequenceEqual(resultingBlocks.Select(b => b.GetText(true))));
				Assert.IsTrue(origBookBlocks.Select(b => b.MultiBlockQuote).SequenceEqual(resultingBlocks.Select(b => b.MultiBlockQuote)));
				Assert.IsTrue(resultingBlocks.All(b => b.SplitId == i));
				Assert.IsTrue(origBookBlocks.Select(b => b.MatchesReferenceText).SequenceEqual(resultingBlocks.Select(b => b.MatchesReferenceText)));
				Assert.IsTrue(origBookBlocks.Select(b => b.GetPrimaryReferenceText()).SequenceEqual(resultingBlocks.Select(b => b.GetPrimaryReferenceText())));
				Assert.IsTrue(origBookBlocks.Select(b => b.CharacterId).SequenceEqual(resultingBlocks.Select(b => b.CharacterId)));
				Assert.IsTrue(origBookBlocks.Select(b => b.UserConfirmed).SequenceEqual(resultingBlocks.Select(b => b.UserConfirmed)));
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

			var book = new BookScript("MAT", blocks);
			var books = new List<BookScript> { book };
			ProjectDataMigrator.MigrateNonContiguousUserSplitsSeparatedByReferenceTextAlignmentSplits(books);

			var resultingBlocks = book.GetScriptBlocks();
			Assert.AreEqual(origBlocks.Count, resultingBlocks.Count);
			Assert.IsTrue(origBlocks.Select(b => b.GetText(true)).SequenceEqual(resultingBlocks.Select(b => b.GetText(true))));
			Assert.IsTrue(origBlocks.Select(b => b.MultiBlockQuote).SequenceEqual(resultingBlocks.Select(b => b.MultiBlockQuote)));
			Assert.IsTrue(origBlocks.Select(b => b.SplitId).SequenceEqual(resultingBlocks.Select(b => b.SplitId)));
			Assert.IsTrue(origBlocks.Select(b => b.MatchesReferenceText).SequenceEqual(resultingBlocks.Select(b => b.MatchesReferenceText)));
			Assert.IsTrue(origBlocks.Select(b => b.GetPrimaryReferenceText()).SequenceEqual(resultingBlocks.Select(b => b.GetPrimaryReferenceText())));
			Assert.IsTrue(origBlocks.Select(b => b.CharacterId).SequenceEqual(resultingBlocks.Select(b => b.CharacterId)));
			Assert.IsTrue(origBlocks.Select(b => b.UserConfirmed).SequenceEqual(resultingBlocks.Select(b => b.UserConfirmed)));
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

			var book = new BookScript("MAT", blocks);
			var books = new List<BookScript> { book };
			ProjectDataMigrator.MigrateNonContiguousUserSplitsSeparatedByReferenceTextAlignmentSplits(books);

			var resultingBlocks = book.GetScriptBlocks();
			Assert.AreEqual(origBlocks.Count, resultingBlocks.Count);
			Assert.IsTrue(origBlocks.Select(b => b.GetText(true)).SequenceEqual(resultingBlocks.Select(b => b.GetText(true))));
			Assert.IsTrue(origBlocks.Select(b => b.MultiBlockQuote).SequenceEqual(resultingBlocks.Select(b => b.MultiBlockQuote)));
			Assert.IsTrue(origBlocks.Select(b => b.SplitId).SequenceEqual(resultingBlocks.Select(b => b.SplitId)));
			Assert.IsTrue(origBlocks.Select(b => b.MatchesReferenceText).SequenceEqual(resultingBlocks.Select(b => b.MatchesReferenceText)));
			Assert.IsTrue(origBlocks.Select(b => b.CharacterId).SequenceEqual(resultingBlocks.Select(b => b.CharacterId)));
			Assert.IsTrue(origBlocks.Select(b => b.UserConfirmed).SequenceEqual(resultingBlocks.Select(b => b.UserConfirmed)));
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

			var book = new BookScript("MAT", blocks);
			var books = new List<BookScript> { book };
			ProjectDataMigrator.MigrateNonContiguousUserSplitsSeparatedByReferenceTextAlignmentSplits(books);

			var resultingBlocks = book.GetScriptBlocks();
			Assert.AreEqual(origBlocks.Count, resultingBlocks.Count);
			Assert.IsTrue(origBlocks.Select(b => b.GetText(true)).SequenceEqual(resultingBlocks.Select(b => b.GetText(true))));
			Assert.IsTrue(origBlocks.Select(b => b.MultiBlockQuote).SequenceEqual(resultingBlocks.Select(b => b.MultiBlockQuote)));
			Assert.IsTrue(origBlocks.Select(b => b.SplitId).SequenceEqual(resultingBlocks.Select(b => b.SplitId)));
			Assert.IsTrue(origBlocks.Select(b => b.MatchesReferenceText).SequenceEqual(resultingBlocks.Select(b => b.MatchesReferenceText)));
			Assert.IsTrue(origBlocks.Select(b => b.GetPrimaryReferenceText()).SequenceEqual(resultingBlocks.Select(b => b.GetPrimaryReferenceText())));
			Assert.IsTrue(origBlocks.Select(b => b.CharacterId).SequenceEqual(resultingBlocks.Select(b => b.CharacterId)));
			Assert.IsTrue(origBlocks.Select(b => b.UserConfirmed).SequenceEqual(resultingBlocks.Select(b => b.UserConfirmed)));
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
					MatchesReferenceText = true,
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

			var book = new BookScript("MAT", blocks);
			var books = new List<BookScript> { book };
			ProjectDataMigrator.MigrateNonContiguousUserSplitsSeparatedByReferenceTextAlignmentSplits(books);

			var resultingBlocks = book.GetScriptBlocks();
			Assert.AreEqual(origBlocks.Count, resultingBlocks.Count);
			Assert.IsTrue(origBlocks.Select(b => b.GetText(true)).SequenceEqual(resultingBlocks.Select(b => b.GetText(true))));
			Assert.IsTrue(origBlocks.Select(b => b.MultiBlockQuote).SequenceEqual(resultingBlocks.Select(b => b.MultiBlockQuote)));
			Assert.IsTrue(origBlocks.Select(b => b.SplitId).SequenceEqual(resultingBlocks.Select(b => b.SplitId)));
			Assert.IsTrue(origBlocks.Select(b => b.MatchesReferenceText).SequenceEqual(resultingBlocks.Select(b => b.MatchesReferenceText)));
			Assert.IsTrue(origBlocks.Select(b => b.GetPrimaryReferenceText()).SequenceEqual(resultingBlocks.Select(b => b.GetPrimaryReferenceText())));
			Assert.IsTrue(origBlocks.Select(b => b.CharacterId).SequenceEqual(resultingBlocks.Select(b => b.CharacterId)));
			Assert.IsTrue(origBlocks.Select(b => b.UserConfirmed).SequenceEqual(resultingBlocks.Select(b => b.UserConfirmed)));
		}

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

		private Block CreateChapterBlock(string bookId, int chapter, string styleTag)
		{
			var chapterVerse = CharacterVerseData.GetStandardCharacterId(bookId, CharacterVerseData.StandardCharacter.BookOrChapter);

			return new Block
			{
				CharacterId = chapterVerse,
				CharacterIdInScript = chapterVerse,
				StyleTag = styleTag,
				BlockElements = new List<BlockElement> {new ScriptText("Chapter " + chapter)}
			};
		}
	}
}
