using System.Collections.Generic;
using Glyssen;
using NUnit.Framework;

namespace GlyssenTests
{
	class ProjectDataMigratorTests
	{
		[Test]
		public void MigrateToVersion87_NoSplits_DataUnchanged()
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
				MultiBlockQuote = MultiBlockQuote.Continuation,
			};

			var block1Original = block1.Clone();
			var block2Original = block2.Clone();

			var book = new BookScript { Blocks = new List<Block> { block1, block2 } };
			var books = new List<BookScript> { book };
			ProjectDataMigrator.MigrateToVersion87(books);

			Assert.AreEqual(2, book.Blocks.Count);
			Assert.AreEqual(block1Original.GetText(true), block1.GetText(true));
			Assert.AreEqual(block2Original.GetText(true), block2.GetText(true));
		}

		[Test]
		public void MigrateToVersion87_SplitDoesntStartWithMultiBlockStart_DataUnchanged()
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

			var book = new BookScript { Blocks = new List<Block> { block1, block2, block3 } };
			var books = new List<BookScript> { book };
			ProjectDataMigrator.MigrateToVersion87(books);

			Assert.AreEqual(3, book.Blocks.Count);
			Assert.AreEqual(block1Original.GetText(true), block1.GetText(true));
			Assert.AreEqual(block2Original.GetText(true), block2.GetText(true));
			Assert.AreEqual(block3Original.GetText(true), block3.GetText(true));
			Assert.AreEqual(MultiBlockQuote.None, block1.MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Start, block2.MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Continuation, block3.MultiBlockQuote);
		}

		[Test]
		public void MigrateToVersion87_SplitIsLastBlock_SplitWithMultiBlockStartAndNone_ChangedToNoneAndStart()
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

			var book = new BookScript { Blocks = new List<Block> { block1, block2 } };
			var books = new List<BookScript> { book };
			ProjectDataMigrator.MigrateToVersion87(books);

			Assert.AreEqual(2, book.Blocks.Count);
			Assert.AreEqual(block1Original.GetText(true), block1.GetText(true));
			Assert.AreEqual(block2Original.GetText(true), block2.GetText(true));
			Assert.AreEqual(MultiBlockQuote.None, block1.MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Start, block2.MultiBlockQuote);
		}

		[Test]
		public void MigrateToVersion87_SplitWithMultiBlockStartAndNone_ChangedToNoneAndStart()
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
				MultiBlockQuote = MultiBlockQuote.Continuation
			};

			var block1Original = block1.Clone();
			var block2Original = block2.Clone();
			var block3Original = block3.Clone();

			var book = new BookScript { Blocks = new List<Block> { block1, block2, block3 } };
			var books = new List<BookScript> { book };
			ProjectDataMigrator.MigrateToVersion87(books);

			Assert.AreEqual(3, book.Blocks.Count);
			Assert.AreEqual(block1Original.GetText(true), block1.GetText(true));
			Assert.AreEqual(block2Original.GetText(true), block2.GetText(true));
			Assert.AreEqual(block3Original.GetText(true), block3.GetText(true));
			Assert.AreEqual(MultiBlockQuote.None, block1.MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Start, block2.MultiBlockQuote);
		}

		[Test]
		public void MigrateToVersion87_MultiSplitWithMultiBlockStartNoneNone_ChangedToNoneNoneStart()
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
				MultiBlockQuote = MultiBlockQuote.Continuation
			};

			var block1Original = block1.Clone();
			var block2Original = block2.Clone();
			var block3Original = block3.Clone();
			var block4Original = block4.Clone();

			var book = new BookScript { Blocks = new List<Block> { block1, block2, block3, block4 } };
			var books = new List<BookScript> { book };
			ProjectDataMigrator.MigrateToVersion87(books);

			Assert.AreEqual(4, book.Blocks.Count);
			Assert.AreEqual(block1Original.GetText(true), block1.GetText(true));
			Assert.AreEqual(block2Original.GetText(true), block2.GetText(true));
			Assert.AreEqual(block3Original.GetText(true), block3.GetText(true));
			Assert.AreEqual(block4Original.GetText(true), block4.GetText(true));
			Assert.AreEqual(MultiBlockQuote.None, block1.MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.None, block2.MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Start, block3.MultiBlockQuote);
		}
	}
}
