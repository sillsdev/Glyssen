using System.Collections.Generic;
using System.Linq;
using Glyssen;
using Glyssen.Character;
using NUnit.Framework;
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

		[TestCase(MultiBlockQuote.Continuation)]
		[TestCase(MultiBlockQuote.ChangeOfDelivery)]
		public void CleanUpOrphanedMultiBlockQuoteStati_NoneFollowedByNonStart_NonStartChangedToNone(MultiBlockQuote continuingStatus)
		{
			var block1 = CreateTestBlock(1, MultiBlockQuote.None);
			var block2 = CreateTestBlock(2, continuingStatus);

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

		[TestCase(MultiBlockQuote.Continuation, MultiBlockQuote.Continuation)]
		[TestCase(MultiBlockQuote.ChangeOfDelivery, MultiBlockQuote.Continuation)]
		[TestCase(MultiBlockQuote.Continuation, MultiBlockQuote.ChangeOfDelivery)]
		[TestCase(MultiBlockQuote.ChangeOfDelivery, MultiBlockQuote.ChangeOfDelivery)]
		public void CleanUpOrphanedMultiBlockQuoteStati_NoneFollowedByMultipleNonStarts_NonStartsChangedToNone(MultiBlockQuote continuingStatus1, MultiBlockQuote continuingStatus2)
		{
			var block1 = CreateTestBlock(1, MultiBlockQuote.None);
			var block2 = CreateTestBlock(2, continuingStatus1);
			var block3 = CreateTestBlock(3, continuingStatus2);

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
		public void MigrateInvalidCharacterIdForScriptDataToVersion88_ValidData_Unchanged()
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
		public void MigrateDeprecatedCharacterIds_OneOfTwoCharacterIdsInVerseReplacedWithDifferentId_CharacterIdInScriptSetToAmbiguous()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.REV);
			TestProject.SimulateDisambiguationForAllBooks(testProject);
			var johnSpeakingInRev714 = testProject.IncludedBooks.Single().GetBlocksForVerse(7, 14).ElementAt(1);
			johnSpeakingInRev714.SetCharacterAndCharacterIdInScript("John", 66);
			johnSpeakingInRev714.UserConfirmed = true;
			var elderSpeakingInRev714 = testProject.IncludedBooks.Single().GetBlocksForVerse(7, 14).ElementAt(3);
			elderSpeakingInRev714.SetCharacterAndCharacterIdInScript("elders, one of the", 66);
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
			angelsSpeakingInRev712.SetCharacterAndCharacterIdInScript("tons of angelic beings", 66);

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
				block.SetCharacterAndCharacterIdInScript("cuatro living creatures/twenty-four elders", 66);

			Assert.AreEqual(singersInRev59.Count, ProjectDataMigrator.MigrateDeprecatedCharacterIds(testProject));

			Assert.True(singersInRev59.All(b => b.CharacterId == "living creature, first/living creature, second/living creature, third/living creature, fourth/twenty-four elders" &&
				b.CharacterIdInScript == "living creature, first"));
		}

		[Test]
		public void MigrateDeprecatedCharacterIds_CharacterIdRemoved_NoOtherCharactersInVerse_CharacterIdInScriptSetToUnknown()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.REV);
			TestProject.SimulateDisambiguationForAllBooks(testProject);
			var blockInRev711 = testProject.IncludedBooks.Single().GetBlocksForVerse(7, 11).First();
			blockInRev711.SetCharacterAndCharacterIdInScript("angels, all the", 66);
			blockInRev711.CharacterIdOverrideForScript = "angels, all, the";
			Assert.AreEqual("angels, all, the", blockInRev711.CharacterIdInScript);

			Assert.AreEqual(1, ProjectDataMigrator.MigrateDeprecatedCharacterIds(testProject));

			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, blockInRev711.CharacterId);
			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, blockInRev711.CharacterIdInScript);
		}

		[Test]
		public void MigrateDeprecatedCharacterIds_CharacterIdRemoved_MultipleCharactersStillInVerse_CharacterIdInScriptSetToAmbiguous()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.REV);
			TestProject.SimulateDisambiguationForAllBooks(testProject);
			var blockInRev13V10 = testProject.IncludedBooks.Single().GetBlocksForVerse(13, 10).First();
			blockInRev13V10.SetCharacterAndCharacterIdInScript("angels, all the", 66);
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
			unexpectedPeterInRev711.SetCharacterAndCharacterIdInScript("peter", 66);

			Assert.AreEqual(1, ProjectDataMigrator.MigrateDeprecatedCharacterIds(testProject));

			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, unexpectedPeterInRev711.CharacterId);
			Assert.IsFalse(testProject.ProjectCharacterVerseData.Any());
		}

		[Test]
		public void MigrateInvalidCharacterIdsWithoutCharacterIdInScriptOverrides_ControlFileHasNoExplicitDefault_FirstCharacterIsUsed()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.MRK);
			TestProject.SimulateDisambiguationForAllBooks(testProject);
			var jesusSpeakingInMrk59 = testProject.IncludedBooks.Single().GetBlocksForVerse(5, 9).ElementAt(1);
			jesusSpeakingInMrk59.SetCharacterAndCharacterIdInScript("Jesus", 66);
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
