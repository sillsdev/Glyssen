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
			Assert.IsFalse(matchup.IncludesBlock(vernacularBlocks[0]));
			Assert.IsTrue(matchup.IncludesBlock(vernacularBlocks[1]));
			Assert.IsTrue(matchup.IncludesBlock(vernacularBlocks[2]));
			Assert.IsFalse(matchup.IncludesBlock(vernacularBlocks[3]));
			Assert.IsFalse(matchup.IncludesBlock(vernacularBlocks[4]));
		}

		[Test]
		public void IncludesBlock_VernVersesSplitByReferenceText_ReturnValuesReflectOriginalBlocks()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus abrio su boca y enseno.", true));
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "Este es versiculo dos, ", true));
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Jesus.");
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(3, "Despues se fue. ", true));
			var vernBook = new BookScript("MAT", vernacularBlocks);
			var matchup = new BlockMatchup(vernBook, 1, p =>
			{
				for (int b = 0; b < p.GetScriptBlocks().Count; b += 2)
				{
					var block = p.GetScriptBlocks()[b];
					int i = block.BlockElements.OfType<ScriptText>().First().Content.IndexOf(" ", StringComparison.Ordinal) + 1;
					p.SplitBlock(block, "2", i, false);
				}
			});

			Assert.IsFalse(matchup.IncludesBlock(vernacularBlocks[0]));
			Assert.IsTrue(matchup.IncludesBlock(vernacularBlocks[1]));
			Assert.IsTrue(matchup.IncludesBlock(vernacularBlocks[2]));
			Assert.IsFalse(matchup.IncludesBlock(vernacularBlocks[3]));
		}

		[Test]
		public void MatchAllBlocks_AllMatchedDifferentCharacters_NoChanges()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus abrio su boca y enseno.", true));
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse("someone", 2, "Este es versiculo dos, ", true));
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo alguien. ");
			var narrator = vernacularBlocks[2].CharacterId;
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "disciples", "Ya sabiamos, ");
			vernacularBlocks.Last().Delivery = "confident";
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "contesto Mateo.");
			var vernBook = new BookScript("MAT", vernacularBlocks);
			var matchup = new BlockMatchup(vernBook, 1, null);
			var refBlockJesus = ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "“This is verse two,” ", true);
			refBlockJesus.Delivery = "commanding";
			matchup.CorrelatedBlocks[0].SetMatchedReferenceBlock(refBlockJesus);
			var refBlockNarrator1 = new Block("p", 1, 2) { CharacterId = narrator };
			refBlockNarrator1.BlockElements.Add(new ScriptText("said Jesus. "));
			matchup.CorrelatedBlocks[1].SetMatchedReferenceBlock(refBlockNarrator1);
			var refBlockMatthew = new Block("p", 1, 2) { CharacterId = "Matthew", Delivery = "smug" };
			refBlockMatthew.BlockElements.Add(new ScriptText("“We knew that,” "));
			matchup.CorrelatedBlocks[2].SetMatchedReferenceBlock(refBlockMatthew);
			var refBlockNarrator2 = new Block("p", 1, 2) { CharacterId = narrator };
			refBlockNarrator2.BlockElements.Add(new ScriptText("replied Matthew."));
			matchup.CorrelatedBlocks[3].SetMatchedReferenceBlock(refBlockNarrator2);

			matchup.MatchAllBlocks();
			Assert.IsTrue(matchup.CorrelatedBlocks.All(b => b.MatchesReferenceText));
			Assert.AreEqual("someone", matchup.CorrelatedBlocks[0].CharacterId);
			Assert.IsNull(matchup.CorrelatedBlocks[0].Delivery);
			Assert.AreEqual("[2]\u00A0“This is verse two,” ", matchup.CorrelatedBlocks[0].PrimaryReferenceText);
			Assert.AreEqual(narrator, matchup.CorrelatedBlocks[1].CharacterId);
			Assert.AreEqual("said Jesus. ", matchup.CorrelatedBlocks[1].PrimaryReferenceText);
			Assert.AreEqual("disciples", matchup.CorrelatedBlocks[2].CharacterId);
			Assert.AreEqual("confident", matchup.CorrelatedBlocks[2].Delivery);
			Assert.AreEqual("“We knew that,” ", matchup.CorrelatedBlocks[2].PrimaryReferenceText);
			Assert.AreEqual(narrator, matchup.CorrelatedBlocks[3].CharacterId);
			Assert.AreEqual("replied Matthew.", matchup.CorrelatedBlocks[3].PrimaryReferenceText);
			Assert.IsFalse(matchup.HasOutstandingChangesToApply);
		}

		[Test]
		public void MatchAllBlocks_AllMatchedButWithUnassignedCharacters_CharactersAndDeliveriesCopiedToCorrelatedBlocks()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus abrio su boca y enseno.", true));
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse(CharacterVerseData.kUnknownCharacter, 2, "Este es versiculo dos, ", true));
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Jesus. ");
			var narrator = vernacularBlocks[2].CharacterId;
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, CharacterVerseData.kUnknownCharacter, "Ya sabiamos, ");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "contesto Mateo.");
			var vernBook = new BookScript("MAT", vernacularBlocks);
			var matchup = new BlockMatchup(vernBook, 1, null);
			matchup.CorrelatedBlocks[0].SetMatchedReferenceBlock(
				ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "“This is verse two,” ", true));
			var refBlockNarrator1 = new Block("p", 1, 2) { CharacterId = narrator };
			refBlockNarrator1.BlockElements.Add(new ScriptText("said Jesus. "));
			matchup.CorrelatedBlocks[1].SetMatchedReferenceBlock(refBlockNarrator1);
			var refBlockMatthew = new Block("p", 1, 2) { CharacterId = "Batholomew/Matthew", Delivery = "smug", CharacterIdOverrideForScript = "Matthew"};
			refBlockMatthew.BlockElements.Add(new ScriptText("“We knew that,” "));
			matchup.CorrelatedBlocks[2].SetMatchedReferenceBlock(refBlockMatthew);
			var refBlockNarrator2 = new Block("p", 1, 2) { CharacterId = narrator };
			refBlockNarrator2.BlockElements.Add(new ScriptText("replied Matthew."));
			matchup.CorrelatedBlocks[3].SetMatchedReferenceBlock(refBlockNarrator2);

			matchup.MatchAllBlocks();
			Assert.IsTrue(matchup.CorrelatedBlocks.All(b => b.MatchesReferenceText));
			Assert.AreEqual("Jesus", matchup.CorrelatedBlocks[0].CharacterId);
			Assert.AreEqual("[2]\u00A0“This is verse two,” ", matchup.CorrelatedBlocks[0].PrimaryReferenceText);
			Assert.AreEqual(narrator, matchup.CorrelatedBlocks[1].CharacterId);
			Assert.AreEqual("said Jesus. ", matchup.CorrelatedBlocks[1].PrimaryReferenceText);
			Assert.AreEqual("Batholomew/Matthew", matchup.CorrelatedBlocks[2].CharacterId);
			Assert.AreEqual("Matthew", matchup.CorrelatedBlocks[2].CharacterIdInScript);
			Assert.AreEqual("smug", matchup.CorrelatedBlocks[2].Delivery);
			Assert.AreEqual("“We knew that,” ", matchup.CorrelatedBlocks[2].PrimaryReferenceText);
			Assert.AreEqual(narrator, matchup.CorrelatedBlocks[3].CharacterId);
			Assert.AreEqual("replied Matthew.", matchup.CorrelatedBlocks[3].PrimaryReferenceText);
			Assert.IsTrue(matchup.HasOutstandingChangesToApply);
		}

		[Test]
		public void MatchAllBlocks_NotAllMatched_MutipleRefBlocksCombinedAndEmptyOnesCreatedAsNeeded()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus abrio su boca y enseno.", true));
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse(CharacterVerseData.kUnknownCharacter, 2, "Este es versiculo dos, ", true));
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Jesus. ");
			var narrator = vernacularBlocks[2].CharacterId;
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, CharacterVerseData.kUnknownCharacter, "Ya sabiamos, ");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "contesto Mateo.")
				.AddVerse(3, "Dijo asi porque Jesus les habia dicho la misma cosa el dia anterior.");
			var vernBook = new BookScript("MAT", vernacularBlocks);
			var matchup = new BlockMatchup(vernBook, 1, null);
			matchup.CorrelatedBlocks[0].SetMatchedReferenceBlock(
				ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "“This is verse two,” ", true));
			var refBlockNarrator1 = new Block("p", 1, 2) { CharacterId = narrator };
			refBlockNarrator1.BlockElements.Add(new ScriptText("said Jesus. To which Matthew replied, "));
			var refBlockMatthew = new Block("p", 1, 2) { CharacterId = "Matthew" };
			refBlockMatthew.BlockElements.Add(new ScriptText("“We knew that.”"));
			var refBlockNarrator2 = new Block("p", 1, 2) { CharacterId = narrator };
			refBlockNarrator2.BlockElements.Add(new ScriptText("He said this "));
			refBlockNarrator2.BlockElements.Add(new Verse("3"));
			refBlockNarrator2.BlockElements.Add(new ScriptText("because the day before Jesus had said, "));
			var refBlockJesus2 = new Block("p", 1, 3) { CharacterId = "Jesus" };
			refBlockJesus2.BlockElements.Add(new ScriptText("“Tomorrow will be verse two.”"));

			matchup.CorrelatedBlocks[1].ReferenceBlocks.Add(refBlockNarrator1);
			matchup.CorrelatedBlocks[1].ReferenceBlocks.Add(refBlockMatthew);
			matchup.CorrelatedBlocks[3].ReferenceBlocks.Add(refBlockNarrator2);
			matchup.CorrelatedBlocks[3].ReferenceBlocks.Add(refBlockJesus2);
			matchup.MatchAllBlocks();
			Assert.IsTrue(matchup.CorrelatedBlocks.All(b => b.MatchesReferenceText));
			Assert.AreEqual("Jesus", matchup.CorrelatedBlocks[0].CharacterId);
			Assert.AreEqual("[2]\u00A0“This is verse two,” ", matchup.CorrelatedBlocks[0].PrimaryReferenceText);
			Assert.AreEqual(narrator, matchup.CorrelatedBlocks[1].CharacterId, "If any of the ref blocks is narrator, default vern block to narrator.");
			Assert.AreEqual("said Jesus. To which Matthew replied, “We knew that.”", matchup.CorrelatedBlocks[1].PrimaryReferenceText);
			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, matchup.CorrelatedBlocks[2].CharacterId);
			Assert.AreEqual(string.Empty, matchup.CorrelatedBlocks[2].PrimaryReferenceText);
			Assert.AreEqual(narrator, matchup.CorrelatedBlocks[3].CharacterId);
			Assert.AreEqual("He said this [3]\u00A0because the day before Jesus had said, “Tomorrow will be verse two.”", matchup.CorrelatedBlocks[3].PrimaryReferenceText);
			var joinedRefBlock = matchup.CorrelatedBlocks[3].ReferenceBlocks.Single();
			// We may not technically really care too much about the next four lines (at least right now), but this is how we expect the reference block
			// to be built.
			Assert.AreEqual(narrator, joinedRefBlock.CharacterId);
			Assert.AreEqual("He said this ", ((ScriptText)joinedRefBlock.BlockElements[0]).Content);
			Assert.AreEqual("3", ((Verse)joinedRefBlock.BlockElements[1]).Number);
			Assert.AreEqual("because the day before Jesus had said, “Tomorrow will be verse two.”", ((ScriptText)joinedRefBlock.BlockElements[2]).Content);
			Assert.IsTrue(matchup.HasOutstandingChangesToApply);
		}

		[Test]
		public void MatchAllBlocks_PrimaryReferenceTextIsNotEnglish_MutipleRefBlocksCombinedForPrimaryAndEnglish()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus abrio su boca y enseno.", true));
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse(CharacterVerseData.kUnknownCharacter, 2, "Este es versiculo dos, ", true));
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Jesus. ");
			var narrator = vernacularBlocks[2].CharacterId;
			var vernBook = new BookScript("MAT", vernacularBlocks);
			var matchup = new BlockMatchup(vernBook, 1, null);
			matchup.CorrelatedBlocks[0].SetMatchedReferenceBlock(
				ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "“This is verse two,” ", true));

			var refBlockNarratorFrench = new Block("p", 1, 2) { CharacterId = narrator };
			refBlockNarratorFrench.BlockElements.Add(new ScriptText("Jésus a dit. Pour que Matthieu a répondu, "));
			var refBlockNarratorEnglish = new Block("p", 1, 2) { CharacterId = narrator };
			refBlockNarratorEnglish.BlockElements.Add(new ScriptText("said Jesus. To which Matthew replied, "));
			refBlockNarratorFrench.SetMatchedReferenceBlock(refBlockNarratorEnglish);

			var refBlockMatthewFrench = new Block("p", 1, 2) { CharacterId = "Matthew" };
			refBlockMatthewFrench.BlockElements.Add(new ScriptText("«Nous savions que.»"));
			var refBlockMatthewEnglish = new Block("p", 1, 2) { CharacterId = "Matthew" };
			refBlockMatthewEnglish.BlockElements.Add(new ScriptText("“We knew that.”"));
			refBlockMatthewFrench.SetMatchedReferenceBlock(refBlockMatthewEnglish);

			matchup.CorrelatedBlocks[1].ReferenceBlocks.Add(refBlockNarratorFrench);
			matchup.CorrelatedBlocks[1].ReferenceBlocks.Add(refBlockMatthewFrench);

			matchup.MatchAllBlocks();
			Assert.IsTrue(matchup.CorrelatedBlocks.All(b => b.MatchesReferenceText));
			Assert.AreEqual("Jesus", matchup.CorrelatedBlocks[0].CharacterId);
			Assert.AreEqual("[2]\u00A0“This is verse two,” ", matchup.CorrelatedBlocks[0].PrimaryReferenceText);
			Assert.AreEqual(narrator, matchup.CorrelatedBlocks[1].CharacterId, "If any of the ref blocks is narrator, default vern block to narrator.");
			Assert.AreEqual("Jésus a dit. Pour que Matthieu a répondu, «Nous savions que.»", matchup.CorrelatedBlocks[1].PrimaryReferenceText);
			Assert.AreEqual("said Jesus. To which Matthew replied, “We knew that.”", matchup.CorrelatedBlocks[1].ReferenceBlocks.Single().PrimaryReferenceText);
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
			refBlock3.BlockElements.Add(new ScriptText("that is awesome!"));
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

		[Test]
		public void SetReferenceText_NoVerseNumbers_NoExistingRefBlock_SpecifiedBlockMatchedWithReferenceBlockHavingSingleScriptTextElement()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "Este es versiculo dos y tres, ", true, 1, "p", 3));
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Jesus. ");
			vernacularBlocks.Last().Delivery = "expressionless";
			var narrator = vernacularBlocks.Last().CharacterId;
			var vernBook = new BookScript("MAT", vernacularBlocks);
			var matchup = new BlockMatchup(vernBook, 0, null);
			matchup.CorrelatedBlocks[0].SetMatchedReferenceBlock(
				ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "“This is verse two,” ", true));
			Assert.IsFalse(matchup.CorrelatedBlocks[1].MatchesReferenceText);

			matchup.SetReferenceText(1, "said Jesus.");
			Assert.IsTrue(matchup.CorrelatedBlocks.All(b => b.MatchesReferenceText));
			// Ensure block 0 not changed
			Assert.AreEqual("Jesus", matchup.CorrelatedBlocks[0].CharacterId);
			Assert.AreEqual("[2]\u00A0“This is verse two,” ", matchup.CorrelatedBlocks[0].PrimaryReferenceText);

			Assert.AreEqual("said Jesus.", matchup.CorrelatedBlocks[1].PrimaryReferenceText);
			var newRefBlock = matchup.CorrelatedBlocks[1].ReferenceBlocks.Single();
			Assert.AreEqual(narrator, newRefBlock.CharacterId, "Should get character/delivery info from vern block");
			Assert.AreEqual("expressionless", newRefBlock.Delivery, "Should get character/delivery info from vern block");
			Assert.AreEqual(2, newRefBlock.InitialStartVerseNumber);
			Assert.AreEqual(0, newRefBlock.InitialEndVerseNumber);
		}

		[TestCase("\u00A0")]
		[TestCase(" ")]
		[TestCase("")]
		public void SetReferenceText_FirstBlock_VernBlockHasVerseBridgeThatCoversNumberInRefBlock_SpecifiedBlockMatchedWithReferenceBlockHavingStartVerseNumberFromVernBlock(string separator)
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse(CharacterVerseData.kUnknownCharacter, 1, "Entonces Jesus hablo, diciendo: ", true, 1, "p", 3));
			vernacularBlocks.Last().Delivery = "expressionless";
			vernacularBlocks.Last().SetMatchedReferenceBlock(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Then Jesus spoke unto them, ", true));

			var block2 = ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "“Este es versiculo dos y tres.”", "p");
			block2.SetMatchedReferenceBlock(ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "“This is verse two.”", false, 1, "p", 3));

			var vernBook = new BookScript("MAT", vernacularBlocks);
			var matchup = new BlockMatchup(vernBook, 0, null);
			Assert.IsTrue(matchup.CorrelatedBlocks.All(b => b.MatchesReferenceText));

			var newRefBlock = matchup.SetReferenceText(0, "Then Jesus told them [2]" + separator + "that it was just verse two.");
			Assert.IsTrue(matchup.CorrelatedBlocks.All(b => b.MatchesReferenceText));
			// Ensure block 1 not changed
			Assert.AreEqual("Jesus", matchup.CorrelatedBlocks[1].CharacterId);
			Assert.AreEqual("[2-3]\u00A0“This is verse two.”", matchup.CorrelatedBlocks[1].PrimaryReferenceText);

			Assert.AreEqual("Then Jesus told them [2]\u00A0that it was just verse two.", matchup.CorrelatedBlocks[0].PrimaryReferenceText);
			Assert.AreEqual(matchup.CorrelatedBlocks[0].ReferenceBlocks.Single(), newRefBlock);
			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, newRefBlock.CharacterId, "Should get character/delivery info from vern block");
			Assert.AreEqual("expressionless", newRefBlock.Delivery, "Should get character/delivery info from vern block");
			Assert.AreEqual(1, newRefBlock.InitialStartVerseNumber);
			Assert.AreEqual(0, newRefBlock.InitialEndVerseNumber);
			Assert.AreEqual(2, newRefBlock.LastVerse);
		}

		[Test]
		public void SetReferenceText_FirstBlock_NoVerseNumber_ExistingRefBlock_SpecifiedBlockMatchedWithReferenceBlockHavingVerseNumberFromVernBlock()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse(CharacterVerseData.kUnknownCharacter, 1, "Entonces Jesus hablo, diciendo: ", true, 1, "p", 3));
			vernacularBlocks.Last().Delivery = "expressionless";
			vernacularBlocks.Last().SetMatchedReferenceBlock(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Then Jesus spoke unto them, ", true));

			var block2 = ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "“Este es versiculo dos y tres.”", "p");
			block2.SetMatchedReferenceBlock(ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "“This is verse two.”", false, 1, "p", 3));

			var vernBook = new BookScript("MAT", vernacularBlocks);
			var matchup = new BlockMatchup(vernBook, 0, null);
			Assert.IsTrue(matchup.CorrelatedBlocks.All(b => b.MatchesReferenceText));

			var newRefBlock = matchup.SetReferenceText(0, "Then Jesus told them that it was just verse two.");
			Assert.IsTrue(matchup.CorrelatedBlocks.All(b => b.MatchesReferenceText));
			// Ensure block 1 not changed
			Assert.AreEqual("Jesus", matchup.CorrelatedBlocks[1].CharacterId);
			Assert.AreEqual("[2-3]\u00A0“This is verse two.”", matchup.CorrelatedBlocks[1].PrimaryReferenceText);

			Assert.AreEqual("Then Jesus told them that it was just verse two.", matchup.CorrelatedBlocks[0].PrimaryReferenceText);
			Assert.AreEqual(matchup.CorrelatedBlocks[0].ReferenceBlocks.Single(), newRefBlock);
			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, newRefBlock.CharacterId, "Should get character/delivery info from vern block");
			Assert.AreEqual("expressionless", newRefBlock.Delivery, "Should get character/delivery info from vern block");
			Assert.AreEqual(1, newRefBlock.InitialStartVerseNumber);
			Assert.AreEqual(3, newRefBlock.InitialEndVerseNumber);
			Assert.AreEqual(3, newRefBlock.LastVerse);
		}

		[TestCase("\u00A0")]
		[TestCase(" ")]
		[TestCase("")]
		public void SetReferenceText_SubsequentBlock_VerseNumberInMiddle_SpecifiedBlockMatchedWithReferenceBlockWithInitialVerseNumberFromPreviousRefBlock(string separator)
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse(CharacterVerseData.kUnknownCharacter, 1, "Entonces Jesus hablo, diciendo: ", true, 1, "p", 3));
			vernacularBlocks.Last().SetMatchedReferenceBlock(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Then Jesus spoke unto them, ", true));
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "Este es versiculo dos y tres, ", "p");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Jesus. ");
			var vernBook = new BookScript("MAT", vernacularBlocks);
			var matchup = new BlockMatchup(vernBook, 0, null);

			var newRefBlock = matchup.SetReferenceText(1, "saying, [2-3]" + separator + "“This is verse two and three.”");
			// Ensure block 0 not changed
			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, matchup.CorrelatedBlocks[0].CharacterId);
			Assert.IsTrue(matchup.CorrelatedBlocks[0].MatchesReferenceText);
			Assert.AreEqual("[1]\u00A0Then Jesus spoke unto them, ", matchup.CorrelatedBlocks[0].PrimaryReferenceText);

			Assert.IsTrue(matchup.CorrelatedBlocks[0].MatchesReferenceText);
			Assert.AreEqual("saying, [2-3]\u00A0“This is verse two and three.”", matchup.CorrelatedBlocks[1].PrimaryReferenceText);
			Assert.AreEqual(matchup.CorrelatedBlocks[1].ReferenceBlocks.Single(), newRefBlock);
			Assert.AreEqual("Jesus", newRefBlock.CharacterId, "Should get character info from vern block");
			Assert.AreEqual(1, newRefBlock.InitialStartVerseNumber);
			Assert.AreEqual(0, newRefBlock.InitialEndVerseNumber);
			Assert.AreEqual(3, newRefBlock.LastVerse);
		}

		[TestCase("\u00A0")]
		[TestCase(" ")]
		[TestCase("")]
		public void SetReferenceText_VerseNumberAtStart_SpecifiedBlockMatchedWithReferenceBlockHavingVerseNumberFromText(string separator)
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "Este es versiculo dos y tres, ", true, 1, "p", 3));
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Jesus. ");
			vernacularBlocks.Last().Delivery = "expressionless";
			var vernBook = new BookScript("MAT", vernacularBlocks);
			var matchup = new BlockMatchup(vernBook, 0, null);
			matchup.CorrelatedBlocks[0].SetMatchedReferenceBlock(
				ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "“This is verse two,” ", true));
			matchup.MatchAllBlocks();
			Assert.IsTrue(matchup.CorrelatedBlocks.All(b => b.MatchesReferenceText));
			Assert.AreEqual("", matchup.CorrelatedBlocks[1].PrimaryReferenceText);

			var newRefBlock = matchup.SetReferenceText(1, "[3]" + separator + "“And this is verse three [4]" + separator + "or maybe four.”");
			Assert.IsTrue(matchup.CorrelatedBlocks.All(b => b.MatchesReferenceText));
			// Ensure block 0 not changed
			Assert.AreEqual("Jesus", matchup.CorrelatedBlocks[0].CharacterId);
			Assert.AreEqual("[2]\u00A0“This is verse two,” ", matchup.CorrelatedBlocks[0].PrimaryReferenceText);

			Assert.AreEqual("[3]\u00A0“And this is verse three [4]\u00A0or maybe four.”", matchup.CorrelatedBlocks[1].PrimaryReferenceText);
			Assert.AreEqual(vernacularBlocks.Last().CharacterId, newRefBlock.CharacterId, "Should get character/delivery info from vern block");
			Assert.AreEqual("expressionless", newRefBlock.Delivery, "Should get character/delivery info from vern block");
			Assert.AreEqual(3, newRefBlock.InitialStartVerseNumber);
			Assert.AreEqual(0, newRefBlock.InitialEndVerseNumber);
			Assert.AreEqual(4, newRefBlock.LastVerse);
		}
	}
}
