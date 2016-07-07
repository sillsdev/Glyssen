using System;
using System.Collections.Generic;
using System.Linq;
using Glyssen;
using Glyssen.Character;
using NUnit.Framework;
namespace GlyssenTests
{
	[TestFixture]
	class BlockMatchupTests
	{
		[TestCase(0)]
		[TestCase(1)]
		[TestCase(2)]
		public void Constructor_SingleVerseInSingleBlock_CorrelatedBlocksContainsOnlyCloneOfThatBlock(int iBlock)
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces dijo Jesus: ", true));
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "Este es versiculo dos, ", true));
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(3, "Asi dijo. Despues se fue. ", true));
			var vernBook = new BookScript("MAT", vernacularBlocks);
			var matchup = new BlockMatchup(vernBook, iBlock, null);
			Assert.AreEqual(vernacularBlocks[iBlock].GetText(true), matchup.CorrelatedBlocks.Single().GetText(true));
			Assert.IsFalse(vernacularBlocks.Contains(matchup.CorrelatedBlocks.Single()));
		}

		[TestCase(1)]
		[TestCase(2)]
		[TestCase(3)]
		[TestCase(4)]
		[TestCase(5)]
		[TestCase(6)]
		public void Constructor_VernVerseStartsAndEndsMidBlock_CorrelatedBlocksContainAllBlocksUntilCleanVerseBreak(int iBlock)
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(1, "This is a leading verse that should not be included.", true));
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(2, "Partieron de alli para Jerico. ", true).AddVerse(3, "Entonces dijo Jesus: "));
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "Come to me you who are weary,", "q1");
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "And I will give you rest.", "q1");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "Pretty sweet invitation!").AddVerse(4, "Start of another verse.");
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Peter", "Okay, I guess I will. ");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "said Peter.");
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(5, "This is a trailing verse that should not be included.", true));
			var vernBook = new BookScript("MAT", vernacularBlocks);
			var matchup = new BlockMatchup(vernBook, iBlock, null);
			Assert.IsTrue(matchup.CorrelatedBlocks.Select(b => b.GetText(true))
				.SequenceEqual(vernacularBlocks.Skip(1).Take(6).Select(b => b.GetText(true))));
			Assert.AreEqual(0, vernacularBlocks.Intersect(matchup.CorrelatedBlocks).Count());
		}

		[TestCase(1)]
		[TestCase(3)]
		[TestCase(4)]
		public void Constructor_VernVerseCrossesSectionHead_CorrelatedBlocksIncludeSectionHeadAndVerseBlocksOnEitherSide(int iBlock)
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(1, "This is a leading verse that should not be included.", true));
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(2, "Partieron de alli para Jerico. ", true));
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks,
				CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.ExtraBiblical),
				"Big change in Topic", "s").IsParagraphStart = true;
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "Peter said: ").IsParagraphStart = true;
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Peter", "Nice place you got here!");
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(3, "This is a trailing verse that should not be included.", true));
			var vernBook = new BookScript("MAT", vernacularBlocks);
			var matchup = new BlockMatchup(vernBook, iBlock, null);
			Assert.IsTrue(matchup.CorrelatedBlocks.Select(b => b.GetText(true))
				.SequenceEqual(vernacularBlocks.Skip(1).Take(4).Select(b => b.GetText(true))));
			Assert.AreEqual(0, vernacularBlocks.Intersect(matchup.CorrelatedBlocks).Count());
		}

		[TestCase(1)]
		[TestCase(2)]
		public void Constructor_VernVerseEndsAtSectionHead_CorrelatedBlocksExcludesSectionHead(int iBlock)
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(1, "This is a leading verse that should not be included.", true));
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(2, "Partieron de alli para Jerico. ", true));
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Peter", "Nice place you got here!");
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks,
				CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.ExtraBiblical),
				"Big change in Topic", "s").IsParagraphStart = true;
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(3, "This is a trailing verse that should not be included.", true));
			var vernBook = new BookScript("MAT", vernacularBlocks);
			var matchup = new BlockMatchup(vernBook, iBlock, null);
			Assert.IsTrue(matchup.CorrelatedBlocks.Select(b => b.GetText(true))
				.SequenceEqual(vernacularBlocks.Skip(1).Take(2).Select(b => b.GetText(true))));
			Assert.AreEqual(0, vernacularBlocks.Intersect(matchup.CorrelatedBlocks).Count());
		}

		[TestCase(0)]
		[TestCase(1)]
		[TestCase(2)]
		public void Apply_SingleCorrelatedBlockHasReferenceText_OriginalBlockSetAsMatchWithReferenceBlock(int iBlock)
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces dijo Jesus: ", true));
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "Este es versiculo dos, ", true));
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(3, "Asi dijo. Despues se fue. ", true));
			var vernBook = new BookScript("MAT", vernacularBlocks);
			var matchup = new BlockMatchup(vernBook, iBlock, null);
			var correspondingVernBlock = vernacularBlocks[iBlock];
			Assert.AreEqual(correspondingVernBlock.GetText(true), matchup.CorrelatedBlocks.Single().GetText(true));
			var verseNum = correspondingVernBlock.InitialStartVerseNumber;
			var refBlock = ReferenceTextTests.CreateBlockForVerse("Jesus", verseNum, String.Format("This is verse {0}, ", verseNum), true);
			matchup.CorrelatedBlocks.Single().SetMatchedReferenceBlock(refBlock);
			Assert.AreEqual(0, matchup.Apply());
			Assert.IsFalse(vernacularBlocks.Except(vernacularBlocks.Skip(iBlock).Take(1)).Any(b => b.MatchesReferenceText));
			Assert.AreEqual(refBlock, vernacularBlocks[iBlock].ReferenceBlocks.Single());
		}

		[TestCase(1)]
		[TestCase(2)]
		public void Apply_MultipleCorrelatedBlocksHaveReferenceText_CorrectOriginalBlocksSetAsMatchWithReferenceBlocks(int iBlock)
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus abrio su boca y enseno.", true));
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse(CharacterVerseData.kUnknownCharacter, 2, "Este es versiculo dos, ", true));
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Jesus. To which Peter and John replied, ");
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, CharacterVerseData.kUnknownCharacter, "Que pinta!");
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(3, "Despues se fue. ", true));
			var vernBook = new BookScript("MAT", vernacularBlocks);
			var matchup = new BlockMatchup(vernBook, iBlock, null);
			Assert.IsTrue(matchup.CorrelatedBlocks.Select(b => b.GetText(true))
				.SequenceEqual(vernacularBlocks.Skip(1).Take(3).Select(b => b.GetText(true))));
			var refBlock1 = ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "This is verse two, ", true);
			matchup.CorrelatedBlocks[0].SetMatchedReferenceBlock(refBlock1);
			var refBlock2 = new Block("p", 1, 2) { CharacterId = vernacularBlocks[2].CharacterId };
			refBlock2.BlockElements.Add(new ScriptText("said Jesus. To which Peter and John replied, "));
			matchup.CorrelatedBlocks[1].SetMatchedReferenceBlock(refBlock2);
			var refBlock3 = new Block("p", 1, 2) { CharacterId = "Peter/John", CharacterIdInScript = "John" };
			refBlock2.BlockElements.Add(new ScriptText("that is awesome!"));
			matchup.CorrelatedBlocks[2].SetMatchedReferenceBlock(refBlock3);
			Assert.AreEqual(0, matchup.Apply());
			Assert.IsFalse(vernacularBlocks[0].MatchesReferenceText);
			Assert.AreEqual(refBlock1, vernacularBlocks[1].ReferenceBlocks.Single());
			Assert.AreEqual(refBlock1.CharacterId, vernacularBlocks[1].CharacterId);
			Assert.AreEqual(refBlock2, vernacularBlocks[2].ReferenceBlocks.Single());
			Assert.AreEqual(refBlock2.CharacterId, vernacularBlocks[2].CharacterId);
			Assert.AreEqual(refBlock3, vernacularBlocks[3].ReferenceBlocks.Single());
			Assert.AreEqual(refBlock3.CharacterId, vernacularBlocks[3].CharacterId);
			Assert.AreEqual(refBlock3.CharacterIdInScript, vernacularBlocks[3].CharacterIdInScript);
			Assert.IsFalse(vernacularBlocks[4].MatchesReferenceText);
		}

		[TestCase(1)]
		[TestCase(2)]
		public void Apply_NotAllCorrelatedBlocksHaveReferenceText_ThrowsInvalidOperationException(int iBlock)
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus abrio su boca y enseno.", true));
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse(CharacterVerseData.kUnknownCharacter, 2, "Este es versiculo dos, ", true));
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Jesus.");
			var vernBook = new BookScript("MAT", vernacularBlocks);
			var matchup = new BlockMatchup(vernBook, iBlock, null);
			matchup.CorrelatedBlocks[0].SetMatchedReferenceBlock(
				ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "This is verse two, ", true));
			Assert.Throws<InvalidOperationException>(() => matchup.Apply());
		}

		[Test]
		public void Apply_VernVerseCrossesUnmatchedSectionHead_VerseBlocksButNotSectionHeadSetAsMatchWithReferenceBlocks()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(1, "This is a leading verse that should not be included.", true));
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(2, "Partieron de alli para Jerico. ", true));
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks,
				CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.ExtraBiblical),
				"Big Change in Topic", "s").IsParagraphStart = true;
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "Pedro dijo: ").IsParagraphStart = true;
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Peter", "Que buen lugar tienes!");
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(3, "This is a trailing verse that should not be included.", true));
			var vernBook = new BookScript("MAT", vernacularBlocks);
			var matchup = new BlockMatchup(vernBook, 1, null);

			var refBlock1 = ReferenceTextTests.CreateBlockForVerse(vernacularBlocks[1].CharacterId, 2, "The left from there for Jericho. ", true);
			matchup.CorrelatedBlocks[0].SetMatchedReferenceBlock(refBlock1);
			var refBlock2 = new Block("p", 1, 2) { CharacterId = vernacularBlocks[1].CharacterId };
			refBlock2.BlockElements.Add(new ScriptText("Peter said: "));
			matchup.CorrelatedBlocks[2].SetMatchedReferenceBlock(refBlock2);
			var refBlock3 = new Block("p", 1, 2) { CharacterId = "Peter" };
			refBlock2.BlockElements.Add(new ScriptText("Nice place you got here!"));
			matchup.CorrelatedBlocks[3].SetMatchedReferenceBlock(refBlock3);

			Assert.AreEqual(0, matchup.Apply());
			var scriptBlocks = vernBook.GetScriptBlocks(true);

			Assert.AreEqual(refBlock1, scriptBlocks[1].ReferenceBlocks.Single());
			Assert.IsFalse(scriptBlocks[2].MatchesReferenceText);
			Assert.AreEqual(refBlock2, scriptBlocks[3].ReferenceBlocks.Single());
			Assert.AreEqual(refBlock3, scriptBlocks[4].ReferenceBlocks.Single());
		}

		[TestCase(1)]
		[TestCase(2)]
		public void Apply_SplitCorrelatedBlocksHaveReferenceText_OriginalBlocksReplacedWithCorrelatedBlocks(int iBlock)
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus abrio su boca y enseno.", true));
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "Este es versiculo dos, ", true));
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Jesus.");
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(3, "Despues se fue. ", true));
			var vernBook = new BookScript("MAT", vernacularBlocks);
			var matchup = new BlockMatchup(vernBook, iBlock, p => p.SplitBlock(p.GetScriptBlocks().First(), "2", 8, false));
			Assert.AreEqual(3, matchup.CorrelatedBlocks.Count);
			Assert.AreEqual("[2]\u00A0Este es ", matchup.CorrelatedBlocks[0].GetText(true));
			Assert.AreEqual("versiculo dos, ", matchup.CorrelatedBlocks[1].GetText(true));
			Assert.AreEqual(vernacularBlocks[2].GetText(true), matchup.CorrelatedBlocks[2].GetText(true));

			var refBlock0 = ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "This is ", true);
			matchup.CorrelatedBlocks[0].SetMatchedReferenceBlock(refBlock0);
			var refBlock1 = new Block("p", 1, 2) { CharacterId = refBlock0.CharacterId };
			refBlock1.BlockElements.Add(new ScriptText("verse two, "));
			matchup.CorrelatedBlocks[1].SetMatchedReferenceBlock(refBlock1);
			var refBlock2 = new Block("p", 1, 2) { CharacterId = vernacularBlocks[2].CharacterId };
			refBlock2.BlockElements.Add(new ScriptText("said Jesus."));
			matchup.CorrelatedBlocks[2].SetMatchedReferenceBlock(refBlock2);

			Assert.AreEqual(1, matchup.Apply());
			var scriptBlocks = vernBook.GetScriptBlocks(true);
			Assert.AreEqual(vernacularBlocks.Count + 1, scriptBlocks.Count);
			Assert.IsFalse(scriptBlocks[0].MatchesReferenceText);
			Assert.AreEqual(refBlock0, scriptBlocks[1].ReferenceBlocks.Single());
			Assert.AreEqual(refBlock1, scriptBlocks[2].ReferenceBlocks.Single());
			Assert.AreEqual(refBlock2, scriptBlocks[3].ReferenceBlocks.Single());
			Assert.IsFalse(scriptBlocks[4].MatchesReferenceText);
		}

		[TestCase(1)]
		[TestCase(2)]
		public void Apply_SplitCorrelatedBlocksDoNotAllHaveReferenceText_ThrowsInvalidOperationException(int iBlock)
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus abrio su boca y enseno.", true));
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "Este es versiculo dos, ", true));
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Jesus.");
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(3, "Despues se fue. ", true));
			var vernBook = new BookScript("MAT", vernacularBlocks);
			var matchup = new BlockMatchup(vernBook, iBlock, p => p.SplitBlock(p.GetScriptBlocks().First(), "2", 8, false));
			Assert.AreEqual(3, matchup.CorrelatedBlocks.Count);
			Assert.AreEqual("[2]\u00A0Este es ", matchup.CorrelatedBlocks[0].GetText(true));
			Assert.AreEqual("versiculo dos, ", matchup.CorrelatedBlocks[1].GetText(true));
			Assert.AreEqual(vernacularBlocks[2].GetText(true), matchup.CorrelatedBlocks[2].GetText(true));

			var refBlock0 = ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "This is verse two", true);
			matchup.CorrelatedBlocks[0].SetMatchedReferenceBlock(refBlock0);
			var refBlock2 = new Block("p", 1, 2) { CharacterId = vernacularBlocks[2].CharacterId };
			refBlock2.BlockElements.Add(new ScriptText("said Jesus."));
			matchup.CorrelatedBlocks[1].SetMatchedReferenceBlock(refBlock2);

			Assert.Throws<InvalidOperationException>(() => matchup.Apply());
		}
	}
}
