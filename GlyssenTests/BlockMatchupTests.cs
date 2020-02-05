using System;
using System.Collections.Generic;
using System.Linq;
using Glyssen;
using Glyssen.Shared;
using GlyssenEngine;
using GlyssenEngine.Character;
using NUnit.Framework;
using SIL.Scripture;

namespace GlyssenTests
{
	[TestFixture]
	class BlockMatchupTests
	{
		[TearDown]
		public void Teardown()
		{
			TestReferenceText.DeleteTempCustomReferenceProjectFolder();
		}

		[TestCase(0)]
		[TestCase(1)]
		[TestCase(2)]
		public void Constructor_SingleVerseInSingleBlock_CorrelatedBlocksContainsOnlyCloneOfThatBlock(int iBlock)
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces dijo Jesus: ", true));
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "Este es versiculo dos, ", true));
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(3, "Asi dijo. Despues se fue. ", true));
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, iBlock, null, i => true, null);
			Assert.AreEqual(vernacularBlocks[iBlock].GetText(true), matchup.CorrelatedBlocks.Single().GetText(true));
			Assert.IsFalse(vernacularBlocks.Contains(matchup.CorrelatedBlocks.Single()));
			//Assert.AreEqual(vernacularBlocks[iBlock], matchup.OriginalAnchorBlock);
			Assert.AreEqual(matchup.CorrelatedBlocks.Single(), matchup.CorrelatedAnchorBlock);
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
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, iBlock, null, i => true, null);
			Assert.IsTrue(matchup.CorrelatedBlocks.Select(b => b.GetText(true))
				.SequenceEqual(vernacularBlocks.Skip(1).Take(6).Select(b => b.GetText(true))));
			Assert.AreEqual(0, vernacularBlocks.Intersect(matchup.CorrelatedBlocks).Count());
			//Assert.AreEqual(vernacularBlocks[iBlock], matchup.OriginalAnchorBlock);
			Assert.AreEqual(matchup.CorrelatedBlocks[iBlock - 1], matchup.CorrelatedAnchorBlock);
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
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, iBlock, null, i => true, null);
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
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, iBlock, null, i => true, null);
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
		public void Constructor_AlternateVernVerseRepeatedLater_CorrelatedBlocksContainsCorrectAnchorVerse()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(8, "This is a leading verse that should not be included.", true, 16, "MRK"));
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(9, "Habiendo resucitado Jesús, apareció a María Magdalena. ", true, 16, "MRK")
				.AddVerse(10, "Ella lo hizo saber a los demas. ").AddVerse(11, "Ellos no creyeron. "));
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(12, "Pero después apareció a 2 de ellos en el camino. ", true, 16, "MRK"));
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(9, "Ellas dieron las instrucciones a los hombres.", true, 16, "MRK")
				.AddVerse(10, "Y después, Jesús envió por medio de ellos el mensaje de salvación. "));
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 3, null, i => true, null);
			Assert.AreEqual(vernacularBlocks.Last().GetText(true), matchup.CorrelatedBlocks.Single().GetText(true));
			Assert.AreEqual(0, vernacularBlocks.Intersect(matchup.CorrelatedBlocks).Count());
			Assert.AreEqual(matchup.CorrelatedBlocks.Single(), matchup.CorrelatedAnchorBlock);
		}

		[Test]
		public void IncludesBlock_VernVersesSplitByReferenceText_ReturnValuesReflectOriginalAndCorrelatedBlocks()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus abrio su boca y enseno.", true));
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "Este es versiculo dos, ", true));
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Jesus.");
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(3, "Despues se fue. ", true));
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 1, p =>
			{
				for (int b = 0; b < p.GetScriptBlocks().Count; b += 2)
				{
					var block = p.GetScriptBlocks()[b];
					int i = block.BlockElements.OfType<ScriptText>().First().Content.IndexOf(" ", StringComparison.Ordinal) + 1;
					p.SplitBlock(block, "2", i, false);
				}
			}, i => true, null);

			Assert.AreEqual(4, matchup.CorrelatedBlocks.Count);

			Assert.IsFalse(matchup.IncludesBlock(vernacularBlocks[0]));
			Assert.IsTrue(matchup.IncludesBlock(vernacularBlocks[1]));
			Assert.IsTrue(matchup.IncludesBlock(vernacularBlocks[2]));
			Assert.IsFalse(matchup.IncludesBlock(vernacularBlocks[3]));

			Assert.IsTrue(matchup.CorrelatedBlocks.All(b => matchup.IncludesBlock(b)));

			Assert.AreEqual(matchup.CorrelatedBlocks.First(), matchup.CorrelatedAnchorBlock);
		}

		[TestCase(2)]
		[TestCase(3)]
		public void CorrelatedAnchorBlock_PreviousVernVerseSplitByReferenceText_GetsCorrectBlock(int iBlock)
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus abrio su boca y enseno.", true));
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "Este es versiculo dos, ", true));
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Jesus. ");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "Despues se fue.");
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(3, "Verse three.", true));
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, iBlock, p =>
			{
				var block = p.GetScriptBlocks().First();
				int i = block.BlockElements.OfType<ScriptText>().First().Content.IndexOf(" ", StringComparison.Ordinal) + 1;
				p.SplitBlock(block, "2", i, false);
			}, i => true, null);

			Assert.AreEqual(4, matchup.CorrelatedBlocks.Count);

			//Assert.AreEqual(vernacularBlocks[iBlock], matchup.OriginalAnchorBlock);
			Assert.AreEqual(matchup.CorrelatedAnchorBlock.GetText(true), vernacularBlocks[iBlock].GetText(true));
			Assert.AreEqual(matchup.CorrelatedBlocks[iBlock], matchup.CorrelatedAnchorBlock);
		}

		[Test]
		public void CorrelatedAnchorBlock_RequestedVernVerseSplitByReferenceText_GetsCorrectBlock()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus abrio su boca y enseno.", true));
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "Este es versiculo dos, ", true));
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Jesus. ");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "Despues se fue.");
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(3, "Verse three.", true));
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 2, p =>
			{
				var block = p.GetScriptBlocks()[1];
				int i = block.BlockElements.OfType<ScriptText>().First().Content.IndexOf(" ", StringComparison.Ordinal) + 1;
				p.SplitBlock(block, "2", i, false);
			}, i => true, null);

			Assert.AreEqual(4, matchup.CorrelatedBlocks.Count);

			//Assert.AreEqual(vernacularBlocks[2], matchup.OriginalAnchorBlock);
			Assert.IsTrue(vernacularBlocks[2].GetText(true).StartsWith(matchup.CorrelatedAnchorBlock.GetText(true)));
			Assert.AreEqual(matchup.CorrelatedBlocks[1], matchup.CorrelatedAnchorBlock);
		}

		[Test]
		public void HasOutstandingChangesToApply_DirectChangeToOriginalCharacter_ReturnsTrue()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus abrio su boca y enseno.", true));
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse("someone", 2, "Este es versiculo dos, ", true));
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo alguien. ");
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "disciples", "Ya sabiamos, ");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "contesto Mateo.");
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 1, null, i => true, ReferenceText.GetStandardReferenceText(ReferenceTextType.English));
			matchup.MatchAllBlocks(null);

			vernacularBlocks[3].CharacterId = "Markose";
			Assert.IsTrue(matchup.HasOutstandingChangesToApply);
		}

		[Test]
		public void HasOutstandingChangesToApply_DirectChangeToCorrelatedCharacter_ReturnsTrue()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus abrio su boca y enseno.", true));
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse("someone", 2, "Este es versiculo dos, ", true));
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo alguien. ");
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "disciples", "Ya sabiamos, ");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "contesto Mateo.");
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 1, null, i => true, ReferenceText.GetStandardReferenceText(ReferenceTextType.English));
			matchup.MatchAllBlocks(null);

			matchup.CorrelatedBlocks[2].CharacterId = "Markose";
			Assert.IsTrue(matchup.HasOutstandingChangesToApply);
		}

		[Test]
		public void HasOutstandingChangesToApply_DirectChangeToOriginalDelivery_ReturnsTrue()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus abrio su boca y enseno.", true));
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse("someone", 2, "Este es versiculo dos, ", true));
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo alguien. ");
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "disciples", "Ya sabiamos, ");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "contesto Mateo.");
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 1, null, i => true, ReferenceText.GetStandardReferenceText(ReferenceTextType.English));
			matchup.MatchAllBlocks(null);

			vernacularBlocks[3].Delivery = "perplexed";
			Assert.IsTrue(matchup.HasOutstandingChangesToApply);
		}

		[Test]
		public void HasOutstandingChangesToApply_DirectChangeToCorrelatedDelivery_ReturnsTrue()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus abrio su boca y enseno.", true));
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse("someone", 2, "Este es versiculo dos, ", true));
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo alguien. ");
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "disciples", "Ya sabiamos, ");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "contesto Mateo.");
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 1, null, i => true, ReferenceText.GetStandardReferenceText(ReferenceTextType.English));
			matchup.MatchAllBlocks(null);

			matchup.CorrelatedBlocks[2].Delivery = "perturbed";
			Assert.IsTrue(matchup.HasOutstandingChangesToApply);
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
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var refBlockJesus = ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "“This is verse two,” ", true);
			refBlockJesus.Delivery = "commanding";
			vernacularBlocks[1].SetMatchedReferenceBlock(refBlockJesus);
			var refBlockNarrator1 = new Block("p", 1, 2) { CharacterId = narrator };
			refBlockNarrator1.BlockElements.Add(new ScriptText("said Jesus. "));
			vernacularBlocks[2].SetMatchedReferenceBlock(refBlockNarrator1);
			var refBlockMatthew = new Block("p", 1, 2) { CharacterId = "Matthew", Delivery = "smug" };
			refBlockMatthew.BlockElements.Add(new ScriptText("“We knew that,” "));
			vernacularBlocks[3].SetMatchedReferenceBlock(refBlockMatthew);
			var refBlockNarrator2 = new Block("p", 1, 2) { CharacterId = narrator };
			refBlockNarrator2.BlockElements.Add(new ScriptText("replied Matthew."));
			vernacularBlocks[4].SetMatchedReferenceBlock(refBlockNarrator2);

			var matchup = new BlockMatchup(vernBook, 1, null, i => true, null);

			matchup.MatchAllBlocks(null);
			Assert.IsTrue(matchup.CorrelatedBlocks.All(b => b.MatchesReferenceText));
			Assert.AreEqual("someone", matchup.CorrelatedBlocks[0].CharacterId);
			Assert.IsNull(matchup.CorrelatedBlocks[0].Delivery);
			Assert.AreEqual("{2}\u00A0“This is verse two,” ", matchup.CorrelatedBlocks[0].GetPrimaryReferenceText());
			Assert.AreEqual(narrator, matchup.CorrelatedBlocks[1].CharacterId);
			Assert.AreEqual("said Jesus. ", matchup.CorrelatedBlocks[1].GetPrimaryReferenceText());
			Assert.AreEqual("disciples", matchup.CorrelatedBlocks[2].CharacterId);
			Assert.AreEqual("confident", matchup.CorrelatedBlocks[2].Delivery);
			Assert.AreEqual("“We knew that,” ", matchup.CorrelatedBlocks[2].GetPrimaryReferenceText());
			Assert.AreEqual(narrator, matchup.CorrelatedBlocks[3].CharacterId);
			Assert.AreEqual("replied Matthew.", matchup.CorrelatedBlocks[3].GetPrimaryReferenceText());
			Assert.IsFalse(matchup.HasOutstandingChangesToApply);
		}

		[Test]
		public void MatchAllBlocks_AllMatchedButWithUnassignedCharacters_CharactersAndDeliveriesCopiedToCorrelatedBlocks()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus abrio su boca y enseno.", true));
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse(CharacterVerseData.kUnexpectedCharacter, 2, "Este es versiculo dos, ", true));
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Jesus. ");
			var narrator = vernacularBlocks[2].CharacterId;
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, CharacterVerseData.kUnexpectedCharacter, "Ya sabiamos, ");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "contesto Mateo.");
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 1, null, i => true, null);
			matchup.CorrelatedBlocks[0].SetMatchedReferenceBlock(
				ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "“This is verse two,” ", true));
			var refBlockNarrator1 = new Block("p", 1, 2) { CharacterId = narrator };
			refBlockNarrator1.BlockElements.Add(new ScriptText("said Jesus. "));
			matchup.CorrelatedBlocks[1].SetMatchedReferenceBlock(refBlockNarrator1);
			var refBlockMatthew = new Block("p", 1, 2) { CharacterId = "Batholomew/Matthew", Delivery = "smug", CharacterIdInScript = "Matthew"};
			refBlockMatthew.BlockElements.Add(new ScriptText("“We knew that,” "));
			matchup.CorrelatedBlocks[2].SetMatchedReferenceBlock(refBlockMatthew);
			var refBlockNarrator2 = new Block("p", 1, 2) { CharacterId = narrator };
			refBlockNarrator2.BlockElements.Add(new ScriptText("replied Matthew."));
			matchup.CorrelatedBlocks[3].SetMatchedReferenceBlock(refBlockNarrator2);

			matchup.MatchAllBlocks(null);
			Assert.IsTrue(matchup.CorrelatedBlocks.All(b => b.MatchesReferenceText));
			Assert.AreEqual("Jesus", matchup.CorrelatedBlocks[0].CharacterId);
			Assert.IsTrue(matchup.CorrelatedBlocks[0].UserConfirmed);
			Assert.AreEqual("{2}\u00A0“This is verse two,” ", matchup.CorrelatedBlocks[0].GetPrimaryReferenceText());
			Assert.AreEqual(narrator, matchup.CorrelatedBlocks[1].CharacterId);
			Assert.IsFalse(matchup.CorrelatedBlocks[1].UserConfirmed);
			Assert.AreEqual("said Jesus. ", matchup.CorrelatedBlocks[1].GetPrimaryReferenceText());
			Assert.AreEqual("Batholomew/Matthew", matchup.CorrelatedBlocks[2].CharacterId);
			Assert.AreEqual("Matthew", matchup.CorrelatedBlocks[2].CharacterIdInScript);
			Assert.IsTrue(matchup.CorrelatedBlocks[2].UserConfirmed);
			Assert.AreEqual("smug", matchup.CorrelatedBlocks[2].Delivery);
			Assert.AreEqual("“We knew that,” ", matchup.CorrelatedBlocks[2].GetPrimaryReferenceText());
			Assert.AreEqual(narrator, matchup.CorrelatedBlocks[3].CharacterId);
			Assert.IsFalse(matchup.CorrelatedBlocks[3].UserConfirmed);
			Assert.AreEqual("replied Matthew.", matchup.CorrelatedBlocks[3].GetPrimaryReferenceText());
			Assert.IsTrue(matchup.HasOutstandingChangesToApply);
		}

		[Test]
		public void MatchAllBlocks_NotAllMatched_MutipleRefBlocksCombinedAndEmptyOnesCreatedAsNeeded()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus abrio su boca y enseno.", true));
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse(CharacterVerseData.kUnexpectedCharacter, 2, "Este es versiculo dos, ", true));
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Jesus. ");
			var narrator = vernacularBlocks[2].CharacterId;
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, CharacterVerseData.kUnexpectedCharacter, "Ya sabiamos, ");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "contesto Mateo.")
				.AddVerse(3, "Dijo asi porque Jesus les habia dicho la misma cosa el dia anterior.");
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 1, null, i => true, ReferenceText.GetStandardReferenceText(ReferenceTextType.English));
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
			matchup.MatchAllBlocks(null);
			Assert.IsTrue(matchup.CorrelatedBlocks.All(b => b.MatchesReferenceText));
			Assert.AreEqual("Jesus", matchup.CorrelatedBlocks[0].CharacterId);
			Assert.AreEqual("{2}\u00A0“This is verse two,” ", matchup.CorrelatedBlocks[0].GetPrimaryReferenceText());
			Assert.AreEqual(narrator, matchup.CorrelatedBlocks[1].CharacterId, "If any of the ref blocks is narrator, default vern block to narrator.");
			Assert.AreEqual("said Jesus. To which Matthew replied, “We knew that.”", matchup.CorrelatedBlocks[1].GetPrimaryReferenceText());
			Assert.AreEqual(CharacterVerseData.kUnexpectedCharacter, matchup.CorrelatedBlocks[2].CharacterId);
			Assert.AreEqual(string.Empty, matchup.CorrelatedBlocks[2].GetPrimaryReferenceText());
			Assert.AreEqual(narrator, matchup.CorrelatedBlocks[3].CharacterId);
			Assert.AreEqual("He said this {3}\u00A0because the day before Jesus had said, “Tomorrow will be verse two.”", matchup.CorrelatedBlocks[3].GetPrimaryReferenceText());
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
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse(CharacterVerseData.kUnexpectedCharacter, 2, "Este es versiculo dos, ", true));
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Jesus. ");
			var narrator = vernacularBlocks[2].CharacterId;
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 1, null, i => true, TestReferenceText.CreateCustomReferenceText(TestReferenceText.TestReferenceTextResource.FrenchMAT));
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

			matchup.MatchAllBlocks(null);
			Assert.IsTrue(matchup.CorrelatedBlocks.All(b => b.MatchesReferenceText));
			Assert.AreEqual("Jesus", matchup.CorrelatedBlocks[0].CharacterId);
			Assert.AreEqual("{2}\u00A0“This is verse two,” ", matchup.CorrelatedBlocks[0].GetPrimaryReferenceText());
			Assert.AreEqual(narrator, matchup.CorrelatedBlocks[1].CharacterId, "If any of the ref blocks is narrator, default vern block to narrator.");
			Assert.AreEqual("Jésus a dit. Pour que Matthieu a répondu, «Nous savions que.»", matchup.CorrelatedBlocks[1].GetPrimaryReferenceText());
			Assert.AreEqual("said Jesus. To which Matthew replied, “We knew that.”", matchup.CorrelatedBlocks[1].ReferenceBlocks.Single().GetPrimaryReferenceText());
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
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, iBlock, null, i => true, null);
			var correspondingVernBlock = vernacularBlocks[iBlock];
			Assert.AreEqual(correspondingVernBlock.GetText(true), matchup.CorrelatedBlocks.Single().GetText(true));
			var verseNum = correspondingVernBlock.InitialStartVerseNumber;
			var refBlock = ReferenceTextTests.CreateBlockForVerse("Jesus", verseNum, String.Format("This is verse {0}, ", verseNum), true);
			matchup.CorrelatedBlocks.Single().SetMatchedReferenceBlock(refBlock);
			Assert.AreEqual(0, matchup.CountOfBlocksAddedBySplitting);

			matchup.Apply();
			Assert.IsFalse(vernacularBlocks.Except(vernacularBlocks.Skip(iBlock).Take(1)).Any(b => b.MatchesReferenceText));
			Assert.AreEqual(refBlock, vernacularBlocks[iBlock].ReferenceBlocks.Single());
			Assert.IsFalse(matchup.HasOutstandingChangesToApply);
		}

		[TestCase(1)]
		[TestCase(2)]
		[TestCase(3)]
		public void Apply_MultipleCorrelatedBlocksHaveReferenceText_CorrectOriginalBlocksSetAsMatchWithReferenceBlocks(int iBlock)
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus abrio su boca y enseno.", true));
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse(CharacterVerseData.kUnexpectedCharacter, 2, "Este es versiculo dos, ", true));
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Jesus. To which Peter and John replied, ");
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, CharacterVerseData.kUnexpectedCharacter, "Que pinta!");
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(3, "Despues se fue. ", true));

			var refBlock1 = ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "This is verse two, ", true);
			vernacularBlocks[1].SetMatchedReferenceBlock(refBlock1);
			var refBlock2 = new Block("p", 1, 2) { CharacterId = vernacularBlocks[2].CharacterId };
			refBlock2.BlockElements.Add(new ScriptText("said Jesus. To which Peter and John replied, "));
			vernacularBlocks[2].SetMatchedReferenceBlock(refBlock2);
			var refBlock3 = new Block("p", 1, 2) { CharacterId = "Peter/John", CharacterIdInScript = "John" };
			refBlock3.BlockElements.Add(new ScriptText("that is awesome!"));
			vernacularBlocks[3].SetMatchedReferenceBlock(refBlock3);

			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, iBlock, null, i => true, null);
			Assert.IsTrue(matchup.CorrelatedBlocks.Select(b => b.GetText(true))
				.SequenceEqual(vernacularBlocks.Skip(1).Take(3).Select(b => b.GetText(true))));
			matchup.MatchAllBlocks(null);
			Assert.AreEqual(0, matchup.CountOfBlocksAddedBySplitting);

			matchup.Apply();
			Assert.IsFalse(vernacularBlocks[0].MatchesReferenceText);
			Assert.AreEqual(refBlock1, vernacularBlocks[1].ReferenceBlocks.Single());
			Assert.AreEqual(refBlock1.CharacterId, vernacularBlocks[1].CharacterId);
			Assert.AreEqual(refBlock2, vernacularBlocks[2].ReferenceBlocks.Single());
			Assert.AreEqual(refBlock2.CharacterId, vernacularBlocks[2].CharacterId);
			Assert.AreEqual(refBlock3, vernacularBlocks[3].ReferenceBlocks.Single());
			Assert.AreEqual(refBlock3.CharacterId, vernacularBlocks[3].CharacterId);
			Assert.AreEqual(refBlock3.CharacterIdInScript, vernacularBlocks[3].CharacterIdInScript);
			Assert.IsFalse(vernacularBlocks[4].MatchesReferenceText);
			Assert.IsFalse(matchup.HasOutstandingChangesToApply);
		}

		[TestCase(1)]
		[TestCase(2)]
		public void Apply_NotAllCorrelatedBlocksHaveReferenceText_ThrowsInvalidOperationException(int iBlock)
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus abrio su boca y enseno.", true));
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse(CharacterVerseData.kUnexpectedCharacter, 2, "Este es versiculo dos, ", true));
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Jesus.");
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, iBlock, null, i => true, null);
			matchup.SetReferenceText(0, "[2] This is verse two, ");
			Assert.IsTrue(matchup.HasOutstandingChangesToApply);

			var e = Assert.Throws<InvalidOperationException>(() => matchup.Apply());
			Assert.AreEqual("Cannot apply reference blocks unless all Scripture blocks have corresponding reference blocks.", e.Message);
			Assert.IsTrue(matchup.HasOutstandingChangesToApply);
		}

		[Test]
		public void Apply_VernVersesInMiddleOfQuoteChainAssignedGoFromUnknownToNarrator_QuoteChainBrokenCorrectly()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(new Block("c", 1)
			{
				CharacterId = CharacterVerseData.GetStandardCharacterId("ROM", CharacterVerseData.StandardCharacter.BookOrChapter),
				BlockElements = new List<BlockElement>(new[] { new ScriptText("1") })
			});
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(20, "This ", true, 1, "ROM"));
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, CharacterVerseData.kUnexpectedCharacter, "«opener« has no closer.");
			vernacularBlocks.Last().MultiBlockQuote = MultiBlockQuote.Start;
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse(CharacterVerseData.kUnexpectedCharacter, 21, "This is verse 21. ", true)
				.AddVerse(22, "This is «verse« 22. ").AddVerse(23, "This is verse 23."));
			vernacularBlocks.Last().MultiBlockQuote = MultiBlockQuote.Continuation;
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse(CharacterVerseData.kUnexpectedCharacter, 24,
				"The quote remains open in this paragraph. ", true).AddVerse(25, "This is verse 25."));
			vernacularBlocks.Last().MultiBlockQuote = MultiBlockQuote.Continuation;
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse(CharacterVerseData.kUnexpectedCharacter, 26,
				"The quote remains open in this paragraph as well. ", true).AddVerse(27, "This is verse 27."));
			vernacularBlocks.Last().MultiBlockQuote = MultiBlockQuote.Continuation;
			var vernBook = new BookScript("ROM", vernacularBlocks, ScrVers.English);

			var refText = ReferenceText.GetStandardReferenceText(ReferenceTextType.English);
			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, 3);
			
			Assert.IsTrue(matchup.HasOutstandingChangesToApply);

			matchup.MatchAllBlocks(refText.Versification);
			matchup.Apply();
			var resultingBlocks = vernBook.GetScriptBlocks().ToList();
			Assert.AreEqual(vernacularBlocks.Count + 2, resultingBlocks.Count);
			Assert.AreEqual(MultiBlockQuote.None, resultingBlocks[2].MultiBlockQuote); // v. 20
			Assert.AreEqual(MultiBlockQuote.None, resultingBlocks[3].MultiBlockQuote); // v. 21
			Assert.AreEqual(MultiBlockQuote.None, resultingBlocks[4].MultiBlockQuote); // v. 22
			Assert.AreEqual(MultiBlockQuote.None, resultingBlocks[5].MultiBlockQuote); // v. 23
			Assert.AreEqual(MultiBlockQuote.Start, resultingBlocks[6].MultiBlockQuote); // vv. 24-25
			Assert.AreEqual(MultiBlockQuote.Continuation, resultingBlocks[7].MultiBlockQuote); // vv. 26-27
		}

		// PG-1325
		[Test]
		public void Apply_FirstBlockOfQuoteChainGoesFromAmbiguousToSpeakerButSubsequentBlocksAreNarrator_QuoteChainCleared()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(new Block("c", 18)
			{
				CharacterId = CharacterVerseData.GetStandardCharacterId("REV", CharacterVerseData.StandardCharacter.BookOrChapter),
				BlockElements = new List<BlockElement>(new[] { new ScriptText("1") })
			});
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(15, "I komersiante ira nga ibinunga ta masikan;", true, 18, "REV", "q2"));
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse(CharacterVerseData.kAmbiguousCharacter, 16, "<<Anakkoy! Anakkoy ka gapa! ", true, 18, "q1"));
			vernacularBlocks.Last().MultiBlockQuote = MultiBlockQuote.Start;
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, CharacterVerseData.kAmbiguousCharacter, "Madumu-rumug nga siudad na Babilonia!", "q2");
			vernacularBlocks.Last().MultiBlockQuote = MultiBlockQuote.Continuation;
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, CharacterVerseData.kAmbiguousCharacter, "Nabbisti ta lino, granate, anna bata nga mangina!", "q1");
			vernacularBlocks.Last().MultiBlockQuote = MultiBlockQuote.Continuation;
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse(CharacterVerseData.kAmbiguousCharacter, 17, "Ngem ngamin danaw laman i nikapawan na!>>", true, 18, "q1"));
			vernacularBlocks.Last().MultiBlockQuote = MultiBlockQuote.Continuation;
			var vernBook = new BookScript("REV", vernacularBlocks, ScrVers.English);

			var refText = ReferenceText.GetStandardReferenceText(ReferenceTextType.English);
			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, 2);
			matchup.MatchAllBlocks(refText.Versification);

			var narrator = CharacterVerseData.GetStandardCharacterId("REV", CharacterVerseData.StandardCharacter.Narrator);
			foreach (var blockInV16 in matchup.CorrelatedBlocks.Where(b => b.InitialStartVerseNumber == 16))
			{
				switch (blockInV16.MultiBlockQuote)
				{
					case MultiBlockQuote.Start:
						blockInV16.SetCharacterIdAndCharacterIdInScript("merchants of the earth", vernBook.BookNumber, vernBook.Versification);
						blockInV16.Delivery = "weeping";
						break;
					case MultiBlockQuote.Continuation:
						blockInV16.SetNonDramaticCharacterId(narrator);
						break;
					default:
						Assert.Fail("Setup problem: All blocks in verse 16 should be part of multi-block quote chain.");
						break;
				}
			}

			matchup.Apply();
			var resultingBlocks = vernBook.GetScriptBlocks().ToList();
			Assert.IsTrue(resultingBlocks.All(b => b.MultiBlockQuote == MultiBlockQuote.None));
		}

		[Test]
		public void GetInvalidReferenceBlocksAtAnyLevel_PrimaryReferenceBlockEndsWithEmptyVerse_ReturnsLevel1Block()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus abrio su boca y dijo: ", true));
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "Este es versiculo dos.");
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, null);
			matchup.SetReferenceText(0, "{1} Then Jesus opened his mouth and said: {2} ");
			matchup.SetReferenceText(1, "why isn't the verse number at the start of this block?");

			var blockWithInvalidReferences = matchup.GetInvalidReferenceBlocksAtAnyLevel().Single();
			Assert.AreEqual(0, blockWithInvalidReferences.Item1);
			Assert.AreEqual(1, blockWithInvalidReferences.Item2);
		}

		[Test]
		public void GetInvalidReferenceBlocksAtAnyLevel_SecondaryReferenceBlockEndsWithEmptyVerse_ReturnsLevel2Block()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus abrio su boca y dijo: ", true));
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "Este es").AddVerse(2, "versiculo dos.");
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var primaryRefBlock1 = new Block("p", 1, 1).AddVerse(1, "Ceci est un bloc.");
			vernacularBlocks.First().SetMatchedReferenceBlock(primaryRefBlock1);
			primaryRefBlock1.SetMatchedReferenceBlock(new Block("p", 1, 1).AddVerse(1, "This is a block."));
			var primaryRefBlock2 = new Block("p", 1, 1).AddVerse(2, "Ceci est un verset.");
			vernacularBlocks.Last().SetMatchedReferenceBlock(primaryRefBlock2);
			primaryRefBlock2.SetMatchedReferenceBlock(new Block("p", 1, 1).AddVerse(2, "This is a verse."));
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, null);
			Assert.AreEqual("{2}\u00A0This is a verse.", matchup.CorrelatedBlocks[1].ReferenceBlocks.Single().ReferenceBlocks.Single().GetText(true));
			matchup.SetReferenceText(1, "This is {2}", 1);
			Assert.AreEqual("This is {2}\u00A0", matchup.CorrelatedBlocks[1].ReferenceBlocks.Single().ReferenceBlocks.Single().GetText(true));
			Assert.IsTrue(matchup.HasOutstandingChangesToApply);

			var blockWithInvalidReferences = matchup.GetInvalidReferenceBlocksAtAnyLevel().Single();
			Assert.AreEqual(1, blockWithInvalidReferences.Item1);
			Assert.AreEqual(2, blockWithInvalidReferences.Item2);
		}

		[Test]
		public void GetInvalidReferenceBlocksAtAnyLevel_PrimaryAndSecondaryReferenceBlockConsistOfEmptyVerse_ReturnsBlocksFromBothLevels()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus abrio su boca y dijo: ", true));
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "Este es").AddVerse(2, "versiculo dos.");
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var primaryRefBlock1 = new Block("p", 1, 1).AddVerse(1, "Ceci est un bloc.");
			vernacularBlocks.First().SetMatchedReferenceBlock(primaryRefBlock1);
			primaryRefBlock1.SetMatchedReferenceBlock(new Block("p", 1, 1).AddVerse(1, "This is a block."));
			var primaryRefBlock2 = new Block("p", 1, 1).AddVerse(2, "Ceci est un verset.");
			vernacularBlocks.Last().SetMatchedReferenceBlock(primaryRefBlock2);
			primaryRefBlock2.SetMatchedReferenceBlock(new Block("p", 1, 1).AddVerse(2, "This is a verse."));
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, null);
			matchup.SetReferenceText(1, "{2}", 0);
			matchup.SetReferenceText(0, "{2}", 1);
			Assert.IsTrue(matchup.HasOutstandingChangesToApply);

			var blocksWithInvalidReferences = matchup.GetInvalidReferenceBlocksAtAnyLevel().ToList();
			Assert.AreEqual(2, blocksWithInvalidReferences.Count);
			Assert.AreEqual(1, blocksWithInvalidReferences.First().Item1);
			Assert.AreEqual(1, blocksWithInvalidReferences.First().Item2);
			Assert.AreEqual(0, blocksWithInvalidReferences.Last().Item1);
			Assert.AreEqual(2, blocksWithInvalidReferences.Last().Item2);
		}

		[Test]
		public void Apply_MatchupWithNoSplitsIsFollowedByContinuationBlocks_FollowingContinuationBlocksHaveCharacterIdUpdated()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Then an unknown person said:", true));
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse(CharacterVerseData.kAmbiguousCharacter, 2, "This is the first thing I want to say.", true));
			vernacularBlocks.Last().MultiBlockQuote = MultiBlockQuote.Start;
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse(CharacterVerseData.kAmbiguousCharacter, 3, "Also, it's worth pointing out something else.", true));
			vernacularBlocks.Last().MultiBlockQuote = MultiBlockQuote.Continuation;
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse(CharacterVerseData.kAmbiguousCharacter, 4, "But now my time has run out, ", true));
			vernacularBlocks.Last().MultiBlockQuote = MultiBlockQuote.Continuation;
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "he concluded.");

			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 1, null, index => true, null);

			Assert.AreEqual(1, matchup.OriginalBlockCount);
			Assert.AreEqual(matchup.OriginalBlockCount, matchup.CorrelatedBlocks.Count);
			Assert.AreEqual(0, matchup.CountOfBlocksAddedBySplitting);

			var refBlock1 = ReferenceTextTests.CreateBlockForVerse("Marco/his friend", 2, "Za 1st Ting 2 say. ", true);
			matchup.CorrelatedBlocks[0].SetMatchedReferenceBlock(refBlock1);
			matchup.CorrelatedBlocks[0].CharacterId = refBlock1.CharacterId;
			matchup.CorrelatedBlocks[0].CharacterIdInScript = "his friend";

			matchup.Apply();
			var scriptBlocks = vernBook.GetScriptBlocks(); /* This used to have join = true */
			int i = 1;
			Assert.AreEqual(refBlock1, scriptBlocks[i].ReferenceBlocks.Single());
			Assert.AreEqual(refBlock1.CharacterId, scriptBlocks[i].CharacterId);
			Assert.AreEqual("his friend", scriptBlocks[i].CharacterIdOverrideForScript);
			Assert.AreEqual(MultiBlockQuote.Start, scriptBlocks[i].MultiBlockQuote);
			Assert.IsFalse(scriptBlocks[++i].MatchesReferenceText);
			Assert.AreEqual(scriptBlocks[1].CharacterId, scriptBlocks[i].CharacterId);
			Assert.AreEqual(scriptBlocks[1].CharacterIdOverrideForScript, scriptBlocks[i].CharacterIdOverrideForScript);
			Assert.AreEqual(MultiBlockQuote.Continuation, scriptBlocks[i].MultiBlockQuote);
			Assert.IsFalse(scriptBlocks[++i].MatchesReferenceText);
			Assert.AreEqual(scriptBlocks[1].CharacterId, scriptBlocks[i].CharacterId);
			Assert.AreEqual(scriptBlocks[1].CharacterIdOverrideForScript, scriptBlocks[i].CharacterIdOverrideForScript);
			Assert.AreEqual(MultiBlockQuote.Continuation, scriptBlocks[i].MultiBlockQuote);
			Assert.IsFalse(scriptBlocks[++i].MatchesReferenceText);
			Assert.AreEqual(CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator),
				scriptBlocks[i].CharacterId);
			Assert.IsNull(scriptBlocks[i].CharacterIdOverrideForScript);
			Assert.AreEqual(MultiBlockQuote.None, scriptBlocks[i].MultiBlockQuote);
			Assert.IsFalse(matchup.HasOutstandingChangesToApply);
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
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, iBlock, p => p.SplitBlock(p.GetScriptBlocks().First(), "2", 8, false), i => true, null);
			Assert.AreEqual(3, matchup.CorrelatedBlocks.Count);
			Assert.AreEqual(1, matchup.CountOfBlocksAddedBySplitting);
			Assert.AreEqual("{2}\u00A0Este es ", matchup.CorrelatedBlocks[0].GetText(true));
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
			Assert.AreEqual(1, matchup.CountOfBlocksAddedBySplitting);

			matchup.Apply();
			var scriptBlocks = vernBook.GetScriptBlocks();
			Assert.AreEqual(vernacularBlocks.Count + 1, scriptBlocks.Count);
			Assert.IsFalse(scriptBlocks[0].MatchesReferenceText);
			Assert.AreEqual(refBlock0, scriptBlocks[1].ReferenceBlocks.Single());
			Assert.AreEqual(refBlock1, scriptBlocks[2].ReferenceBlocks.Single());
			Assert.AreEqual(refBlock2, scriptBlocks[3].ReferenceBlocks.Single());
			Assert.IsFalse(scriptBlocks[4].MatchesReferenceText);
			Assert.AreEqual(0, matchup.CountOfBlocksAddedBySplitting);
			Assert.IsFalse(matchup.HasOutstandingChangesToApply);
			Assert.IsTrue(matchup.OriginalBlocks.Select(b => b.GetText(true)).SequenceEqual(matchup.CorrelatedBlocks.Select(b => b.GetText(true))));
			Assert.IsFalse(matchup.OriginalBlocks.Intersect(matchup.CorrelatedBlocks).Any());
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
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, iBlock, p => p.SplitBlock(p.GetScriptBlocks().First(), "2", 8, false), i => true, null);
			Assert.AreEqual(3, matchup.CorrelatedBlocks.Count);
			Assert.AreEqual(1, matchup.CountOfBlocksAddedBySplitting);
			Assert.AreEqual("{2}\u00A0Este es ", matchup.CorrelatedBlocks[0].GetText(true));
			Assert.AreEqual("versiculo dos, ", matchup.CorrelatedBlocks[1].GetText(true));
			Assert.AreEqual(vernacularBlocks[2].GetText(true), matchup.CorrelatedBlocks[2].GetText(true));

			var refBlock0 = ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "This is verse two", true);
			matchup.CorrelatedBlocks[0].SetMatchedReferenceBlock(refBlock0);
			var refBlock2 = new Block("p", 1, 2) { CharacterId = vernacularBlocks[2].CharacterId };
			refBlock2.BlockElements.Add(new ScriptText("said Jesus."));
			matchup.CorrelatedBlocks[1].SetMatchedReferenceBlock(refBlock2);

			var e = Assert.Throws<InvalidOperationException>(() => matchup.Apply());
			Assert.AreEqual("Cannot apply reference blocks unless all Scripture blocks have corresponding reference blocks.", e.Message);
			Assert.AreEqual(1, matchup.CountOfBlocksAddedBySplitting);
			Assert.IsTrue(matchup.HasOutstandingChangesToApply);
		}

		[Test]
		public void Apply_ReferenceBlockHasCharacterWithASlash_CharacterIdOverrideForScriptSetToFirstCharacter()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(9, "Jesús le preguntó: ", false, 5, "MRK"));
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, CharacterVerseData.kAmbiguousCharacter,
				"¿Cómo te llamas?");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "Él contestó: ", "MRK");
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, CharacterVerseData.kAmbiguousCharacter,
				"Me llamo Legión, porque somos muchos.");
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 1, null, i => true, null);

			var referenceBlocks = new List<Block>();
			referenceBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(9, "He asked him, ", false, 8, "MRK"));
			ReferenceTextTests.AddBlockForVerseInProgress(referenceBlocks, "Jesus", "“What is your name?”");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(referenceBlocks, "He said to him, ", "MRK");
			ReferenceTextTests.AddBlockForVerseInProgress(referenceBlocks, "demons (Legion)/man delivered from Legion of demons",
				"“My name is Legion, for we are many.”");

			for (int i = 0; i < 4; i++)
				matchup.CorrelatedBlocks[i].SetMatchedReferenceBlock(referenceBlocks[i]);
			
			matchup.MatchAllBlocks(null);
			for (int i = 0; i < 4; i++)
				Assert.AreEqual(referenceBlocks[i].CharacterId, matchup.CorrelatedBlocks[i].CharacterId);
			Assert.AreEqual("demons (Legion)", matchup.CorrelatedBlocks[3].CharacterIdOverrideForScript);

			matchup.Apply();
			for (int i = 0; i < 4; i++)
			{
				Assert.IsTrue(vernacularBlocks[i].MatchesReferenceText);
				Assert.AreEqual(vernacularBlocks[i].CharacterId, referenceBlocks[i].CharacterId);
			}
			Assert.AreEqual("demons (Legion)", vernacularBlocks[3].CharacterIdOverrideForScript);
			Assert.IsFalse(matchup.HasOutstandingChangesToApply);
		}

		[TestCase("")]
		[TestCase(null)]
		public void SetReferenceText_NullOrEmpty_SpecifiedBlockMatchedWithReferenceBlockHavingSingleEmptyScriptTextElement(string refText)
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "Este es versiculo dos y tres, ", true, 1, "p", 3));
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Jesus. ");
			var narrator = vernacularBlocks.Last().CharacterId;
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, null);
			matchup.CorrelatedBlocks[0].SetMatchedReferenceBlock(
				ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "“This is verse two,” ", true));
			Assert.IsFalse(matchup.CorrelatedBlocks[1].MatchesReferenceText);

			matchup.SetReferenceText(1, refText);
			Assert.IsTrue(matchup.CorrelatedBlocks.All(b => b.MatchesReferenceText));

			Assert.AreEqual("", matchup.CorrelatedBlocks[1].GetPrimaryReferenceText());
			var newRefBlock = matchup.CorrelatedBlocks[1].ReferenceBlocks.Single();
			Assert.AreEqual(narrator, newRefBlock.CharacterId, "Should get character/delivery info from vern block");
			Assert.AreEqual(2, newRefBlock.InitialStartVerseNumber);
			Assert.AreEqual(0, newRefBlock.InitialEndVerseNumber);
		}

		[Test]
		public void SetReferenceText_NoVerseNumbers_NoExistingRefBlock_SpecifiedBlockMatchedWithReferenceBlockHavingSingleScriptTextElement()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "Este es versiculo dos y tres, ", true, 1, "p", 3));
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Jesus. ");
			vernacularBlocks.Last().Delivery = "expressionless";
			var narrator = vernacularBlocks.Last().CharacterId;
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, null);
			matchup.CorrelatedBlocks[0].SetMatchedReferenceBlock(
				ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "“This is verse two,” ", true));
			Assert.IsFalse(matchup.CorrelatedBlocks[1].MatchesReferenceText);

			matchup.SetReferenceText(1, "said Jesus.");
			Assert.IsTrue(matchup.CorrelatedBlocks.All(b => b.MatchesReferenceText));
			// Ensure block 0 not changed
			Assert.AreEqual("Jesus", matchup.CorrelatedBlocks[0].CharacterId);
			Assert.AreEqual("{2}\u00A0“This is verse two,” ", matchup.CorrelatedBlocks[0].GetPrimaryReferenceText());

			Assert.AreEqual("said Jesus.", matchup.CorrelatedBlocks[1].GetPrimaryReferenceText());
			var newRefBlock = matchup.CorrelatedBlocks[1].ReferenceBlocks.Single();
			Assert.AreEqual(narrator, newRefBlock.CharacterId, "Should get character/delivery info from vern block");
			Assert.AreEqual("expressionless", newRefBlock.Delivery, "Should get character/delivery info from vern block");
			Assert.AreEqual(2, newRefBlock.InitialStartVerseNumber);
			Assert.AreEqual(0, newRefBlock.InitialEndVerseNumber);
		}

		[TestCase("\u00A0")]
		[TestCase(" ")]
		[TestCase("")]
		public void SetReferenceText_FirstBlock_VernBlockHasVerseBridgeThatCoversNumberInRefBlock_SpecifiedBlockMatchedWithReferenceBlockHavingStartVerseNumberImpliedByVerseNumberInRefBlock(string separator)
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse(CharacterVerseData.kUnexpectedCharacter, 1, "Entonces Jesus hablo, diciendo: ", true, 1, "p", 3));
			vernacularBlocks.Last().Delivery = "expressionless";
			vernacularBlocks.Last().SetMatchedReferenceBlock(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Then Jesus spoke unto them, ", true));

			var block2 = ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "“Este es versiculo dos y tres.”", "p");
			block2.SetMatchedReferenceBlock(ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "“This is verse two.”", false, 1, "p", 3));

			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, null);
			Assert.IsTrue(matchup.CorrelatedBlocks.All(b => b.MatchesReferenceText));

			var newRefBlock = matchup.SetReferenceText(0, "Then Jesus told them {2}" + separator + "that it was just verse two.");
			Assert.IsTrue(matchup.CorrelatedBlocks.All(b => b.MatchesReferenceText));
			// Ensure block 1 not changed
			Assert.AreEqual("Jesus", matchup.CorrelatedBlocks[1].CharacterId);
			Assert.AreEqual("{2-3}\u00A0“This is verse two.”", matchup.CorrelatedBlocks[1].GetPrimaryReferenceText());

			Assert.AreEqual("Then Jesus told them {2}\u00A0that it was just verse two.", matchup.CorrelatedBlocks[0].GetPrimaryReferenceText());
			Assert.AreEqual(matchup.CorrelatedBlocks[0].ReferenceBlocks.Single(), newRefBlock);
			Assert.AreEqual(CharacterVerseData.kUnexpectedCharacter, newRefBlock.CharacterId, "Should get character/delivery info from vern block");
			Assert.AreEqual("expressionless", newRefBlock.Delivery, "Should get character/delivery info from vern block");
			Assert.AreEqual(1, newRefBlock.InitialStartVerseNumber);
			Assert.AreEqual(0, newRefBlock.InitialEndVerseNumber);
			Assert.AreEqual(2, newRefBlock.LastVerseNum);
		}

		[Test]
		public void SetReferenceText_FirstBlock_RefBlockContainsBridge_ReferenceBlockHasStartVerseNumberImpliedByVerseNumberInRefBlock()
		{
			// The logic represented here is entirely "fuzzy", but the idea is that since the corresponding vernacular block is a verse
			// bridge from 1-3, that the reference text is "probably" for that same range. But since it turns out that the reference
			// text has a 3-4 verse bridge in the middle of it, logically the preceding text can't be for verse 1-3, so they must be
			// for verses 1-2.
			
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse(CharacterVerseData.kUnexpectedCharacter, 1, "Entonces Jesus hablo, diciendo: ", true, 1, "p", 3));
			vernacularBlocks.Last().SetMatchedReferenceBlock(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Then Jesus spoke unto them, ", true));

			var block2 = ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "“Este es versiculo dos y tres.”", "p");
			block2.SetMatchedReferenceBlock(ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "“This is verse two.”", false, 1, "p", 3));

			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, null);
			Assert.IsTrue(matchup.CorrelatedBlocks.All(b => b.MatchesReferenceText));

			var newRefBlock = matchup.SetReferenceText(0, "Then Jesus told them {3-4}that it was actually a verse bridge starting with three.");
			Assert.IsTrue(matchup.CorrelatedBlocks.All(b => b.MatchesReferenceText));
			// Ensure block 1 not changed
			Assert.AreEqual("Jesus", matchup.CorrelatedBlocks[1].CharacterId);
			Assert.AreEqual("{2-3}\u00A0“This is verse two.”", matchup.CorrelatedBlocks[1].GetPrimaryReferenceText());

			Assert.AreEqual("Then Jesus told them {3-4}\u00A0that it was actually a verse bridge starting with three.", matchup.CorrelatedBlocks[0].GetPrimaryReferenceText());
			Assert.AreEqual(matchup.CorrelatedBlocks[0].ReferenceBlocks.Single(), newRefBlock);
			Assert.AreEqual(1, newRefBlock.InitialStartVerseNumber);
			Assert.AreEqual(2, newRefBlock.InitialEndVerseNumber);
			Assert.AreEqual(4, newRefBlock.LastVerseNum);
		}

		[Test]
		public void SetReferenceText_FirstBlock_RefBlockContainsAnnotationAndThenBridge_ReferenceBlockHasStartVerseNumberImpliedByVerseNumberInRefBlock()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse(CharacterVerseData.kUnexpectedCharacter, 1, "Entonces Jesus hablo, diciendo: ", true, 1, "p", 3));
			vernacularBlocks.Last().SetMatchedReferenceBlock(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Then Jesus spoke unto them, ", true));

			var block2 = ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "“Este es versiculo dos y tres.”", "p");
			block2.SetMatchedReferenceBlock(ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "“This is verse two.”", false, 1, "p", 3));

			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, null);
			Assert.IsTrue(matchup.CorrelatedBlocks.All(b => b.MatchesReferenceText));

			var newRefBlock = matchup.SetReferenceText(0, "{F8 Music--Ends} {3-4}This is actually a verse bridge starting with three.");
			Assert.IsTrue(matchup.CorrelatedBlocks.All(b => b.MatchesReferenceText));
			// Ensure block 1 not changed
			Assert.AreEqual("Jesus", matchup.CorrelatedBlocks[1].CharacterId);
			Assert.AreEqual("{2-3}\u00A0“This is verse two.”", matchup.CorrelatedBlocks[1].GetPrimaryReferenceText());

			Assert.AreEqual("{F8 Music--Ends} {3-4}\u00A0This is actually a verse bridge starting with three.", matchup.CorrelatedBlocks[0].GetPrimaryReferenceText());
			Assert.AreEqual(matchup.CorrelatedBlocks[0].ReferenceBlocks.Single(), newRefBlock);
			Assert.AreEqual(3, newRefBlock.InitialStartVerseNumber);
			Assert.AreEqual(4, newRefBlock.InitialEndVerseNumber);
			Assert.AreEqual(4, newRefBlock.LastVerseNum);
		}

		[Test]
		public void SetReferenceText_FirstBlock_NoVerseNumber_ExistingRefBlock_SpecifiedBlockMatchedWithReferenceBlockHavingVerseNumberFromVernBlock()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse(CharacterVerseData.kUnexpectedCharacter, 1, "Entonces Jesus hablo, diciendo: ", true, 1, "p", 3));
			vernacularBlocks.Last().Delivery = "expressionless";
			vernacularBlocks.Last().SetMatchedReferenceBlock(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Then Jesus spoke unto them, ", true));

			var block2 = ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "“Este es versiculo dos y tres.”", "p");
			block2.SetMatchedReferenceBlock(ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "“This is verse two.”", false, 1, "p", 3));

			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, null);
			Assert.IsTrue(matchup.CorrelatedBlocks.All(b => b.MatchesReferenceText));

			var newRefBlock = matchup.SetReferenceText(0, "Then Jesus told them that it was just verse two.");
			Assert.IsTrue(matchup.CorrelatedBlocks.All(b => b.MatchesReferenceText));
			// Ensure block 1 not changed
			Assert.AreEqual("Jesus", matchup.CorrelatedBlocks[1].CharacterId);
			Assert.AreEqual("{2-3}\u00A0“This is verse two.”", matchup.CorrelatedBlocks[1].GetPrimaryReferenceText());
			Assert.AreEqual(2, matchup.CorrelatedBlocks[1].ReferenceBlocks.Single().InitialStartVerseNumber);

			Assert.AreEqual("Then Jesus told them that it was just verse two.", matchup.CorrelatedBlocks[0].GetPrimaryReferenceText());
			Assert.AreEqual(matchup.CorrelatedBlocks[0].ReferenceBlocks.Single(), newRefBlock);
			Assert.AreEqual(CharacterVerseData.kUnexpectedCharacter, newRefBlock.CharacterId, "Should get character/delivery info from vern block");
			Assert.AreEqual("expressionless", newRefBlock.Delivery, "Should get character/delivery info from vern block");
			Assert.AreEqual(1, newRefBlock.InitialStartVerseNumber);
			Assert.AreEqual(3, newRefBlock.InitialEndVerseNumber);
			Assert.AreEqual(3, newRefBlock.LastVerseNum);
		}

		[TestCase("\u00A0")]
		[TestCase(" ")]
		[TestCase("")]
		public void SetReferenceText_VerseNumberAddedToRefBlockOfFirstBlock_FollowingRefBlockDoesNotStartWithVerseNumber_InitialVerseRefOfFollowingReferenceBlockChanged(string separator)
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse(CharacterVerseData.kUnexpectedCharacter, 1, "Entonces Jesus hablo, diciendo: ", true, 1, "p", 3));
			vernacularBlocks.Last().SetMatchedReferenceBlock(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Then Jesus spoke unto them, ", true));

			var block2 = ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "“Yo soy el pan de vida.”", "p");
			var refBlock2 = new Block("q", 1, 1);
			refBlock2.CharacterId = "Jesus";
			refBlock2.BlockElements.Add(new ScriptText("“Continuation of previous verse in ref text. "));
			refBlock2.AddVerse(3, "Three!”");
			block2.SetMatchedReferenceBlock(refBlock2);

			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, null);
			Assert.IsTrue(matchup.CorrelatedBlocks.All(b => b.MatchesReferenceText));

			matchup.SetReferenceText(0, "Then Jesus told them {2}" + separator + "that verse two was important, too. ");
			Assert.IsTrue(matchup.CorrelatedBlocks.All(b => b.MatchesReferenceText));
			var followingRefBlock = matchup.CorrelatedBlocks[1].ReferenceBlocks.Single();

			Assert.AreEqual("Jesus", matchup.CorrelatedBlocks[1].CharacterId);
			Assert.AreEqual("“Continuation of previous verse in ref text. {3}\u00A0Three!”", matchup.CorrelatedBlocks[1].GetPrimaryReferenceText());
			Assert.AreEqual(1, refBlock2.InitialStartVerseNumber, "Original should not have been changed -- SetReferenceText needs to make a clone to avoid corrupting original collection.");
			Assert.AreEqual(2, followingRefBlock.InitialStartVerseNumber);
			Assert.AreEqual(0, followingRefBlock.InitialEndVerseNumber);
			Assert.AreEqual("Jesus", followingRefBlock.CharacterId);
		}

		[Test]
		public void SetReferenceText_VerseNumberAddedToRefBlockOfFirstBlock_FollowingPrimaryAndSecondaryRefBlocksDoNotStartWithVerseNumbers_InitialVerseRefOfClonedFollowingReferenceBlocksChanged()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus hablo, diciendo: ", true));
			vernacularBlocks.Last().SetMatchedReferenceBlock(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Then Jesus spoke unto them, ", true));
			var narrator = vernacularBlocks.Last().CharacterId;

			var block2 = ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "“Yo soy el pan de vida.”", "p");
			var primaryRefBlock2 = new Block("q", 1, 1);
			primaryRefBlock2.CharacterId = "Jesus";
			primaryRefBlock2.BlockElements.Add(new ScriptText("“Continuation of previous verse in primary ref text.”, "));
			primaryRefBlock2.AddVerse(3, "Three!");
			var secondaryRefBlock2 = new Block("q", 1, 1);
			secondaryRefBlock2.CharacterId = "Jesus";
			secondaryRefBlock2.BlockElements.Add(new ScriptText("“Continuation of verse three in secondary ref text.” "));
			primaryRefBlock2.SetMatchedReferenceBlock(secondaryRefBlock2);
			block2.ReferenceBlocks.Add(primaryRefBlock2);

			var primaryRefBlock3 = new Block("q", 1, 3);
			primaryRefBlock3.CharacterId = narrator;
			primaryRefBlock3.BlockElements.Add(new ScriptText("so He spake."));
			var secondaryRefBlock3 = new Block("q", 1, 1);
			secondaryRefBlock3.CharacterId = narrator;
			secondaryRefBlock3.BlockElements.Add(new ScriptText("(let the reader understand) "));
			secondaryRefBlock3.AddVerse(3, "That's what Jesus said.");
			primaryRefBlock3.SetMatchedReferenceBlock(secondaryRefBlock3);
			block2.ReferenceBlocks.Add(primaryRefBlock3);

			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, ReferenceText.GetStandardReferenceText(ReferenceTextType.Russian));
			matchup.MatchAllBlocks(null);
			Assert.IsTrue(matchup.CorrelatedBlocks.All(b => b.MatchesReferenceText));

			matchup.SetReferenceText(0, "Then Jesus told them {2} that verse two was important, too. ");
			Assert.IsTrue(matchup.CorrelatedBlocks.All(b => b.MatchesReferenceText));

			Assert.AreEqual("Jesus", matchup.CorrelatedBlocks[1].CharacterId);
			Assert.AreEqual(1, primaryRefBlock2.InitialStartVerseNumber, "Original should not have been changed -- SetReferenceText needs to make a clone to avoid corrupting original collection.");
			Assert.AreEqual(3, primaryRefBlock3.InitialStartVerseNumber, "Original should not have been changed -- SetReferenceText needs to make a clone to avoid corrupting original collection.");
			Assert.AreEqual(1, secondaryRefBlock2.InitialStartVerseNumber, "Original should not have been changed -- SetReferenceText needs to make a clone to avoid corrupting original collection.");
			Assert.AreEqual(1, secondaryRefBlock3.InitialStartVerseNumber, "Original should not have been changed -- SetReferenceText needs to make a clone to avoid corrupting original collection.");
			var followingPrimaryRefBlock = matchup.CorrelatedBlocks[1].ReferenceBlocks.Single();
			Assert.AreEqual(2, followingPrimaryRefBlock.InitialStartVerseNumber);
			Assert.AreEqual(0, followingPrimaryRefBlock.InitialEndVerseNumber);
			Assert.AreEqual("Jesus", followingPrimaryRefBlock.CharacterId);
			var followingSecondaryRefBlock = followingPrimaryRefBlock.ReferenceBlocks.Single();
			Assert.AreEqual(2, followingSecondaryRefBlock.InitialStartVerseNumber);
			Assert.AreEqual(0, followingSecondaryRefBlock.InitialEndVerseNumber);
			Assert.AreEqual("Jesus", followingSecondaryRefBlock.CharacterId);
		}

		[Test]
		public void SetReferenceText_VerseNumberAddedToRefBlockOfFirstBlock_PrimaryAndSecondaryRefBlocksOfFollowingBlocksDoNotStartWithVerseNumbers_InitialVerseRefOfClonedFollowingReferenceBlocksChanged()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus hablo, diciendo: ", true));
			vernacularBlocks.Last().SetMatchedReferenceBlock(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Then Jesus spoke unto them, ", true));
			var narrator = vernacularBlocks.Last().CharacterId;

			var block2 = ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "“Yo soy el pan de vida.", "p");
			var primaryRefBlock2 = new Block("q", 1, 1);
			primaryRefBlock2.CharacterId = "Jesus";
			primaryRefBlock2.BlockElements.Add(new ScriptText("“Continuation of previous verse in primary ref text.”, "));
			primaryRefBlock2.AddVerse(4, "Four!");
			var secondaryRefBlock2 = new Block("q", 1, 1);
			secondaryRefBlock2.CharacterId = "Jesus";
			secondaryRefBlock2.BlockElements.Add(new ScriptText("“Continuation of verse three in secondary ref text.” "));
			primaryRefBlock2.SetMatchedReferenceBlock(secondaryRefBlock2);
			block2.SetMatchedReferenceBlock(primaryRefBlock2);

			var block3 = ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "El que viniere a mi vivira.”", "q");
			var primaryRefBlock3 = new Block("q", 1, 4);
			primaryRefBlock3.CharacterId = narrator;
			primaryRefBlock3.BlockElements.Add(new ScriptText("so He spake."));
			var secondaryRefBlock3 = new Block("q", 1, 1);
			secondaryRefBlock3.CharacterId = narrator;
			secondaryRefBlock3.BlockElements.Add(new ScriptText("(let the reader understand) "));
			secondaryRefBlock3.AddVerse(4, "That's what Jesus said.");
			primaryRefBlock3.SetMatchedReferenceBlock(secondaryRefBlock3);
			block3.SetMatchedReferenceBlock(primaryRefBlock3);

			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, ReferenceText.GetStandardReferenceText(ReferenceTextType.Russian));
			matchup.MatchAllBlocks(null);
			Assert.IsTrue(matchup.CorrelatedBlocks.All(b => b.MatchesReferenceText));

			matchup.SetReferenceText(0, "Then Jesus told them {2-3} that verse two was important, too. ");
			Assert.IsTrue(matchup.CorrelatedBlocks.All(b => b.MatchesReferenceText));

			Assert.AreEqual("Jesus", matchup.CorrelatedBlocks[1].CharacterId);
			Assert.AreEqual(1, primaryRefBlock2.InitialStartVerseNumber, "Original should not have been changed -- SetReferenceText needs to make a clone to avoid corrupting original collection.");
			Assert.AreEqual(4, primaryRefBlock3.InitialStartVerseNumber, "Original should not have been changed -- SetReferenceText needs to make a clone to avoid corrupting original collection.");
			Assert.AreEqual(1, secondaryRefBlock2.InitialStartVerseNumber, "Original should not have been changed -- SetReferenceText needs to make a clone to avoid corrupting original collection.");
			Assert.AreEqual(1, secondaryRefBlock3.InitialStartVerseNumber, "Original should not have been changed -- SetReferenceText needs to make a clone to avoid corrupting original collection.");
			var followingPrimaryRefBlock = matchup.CorrelatedBlocks[1].ReferenceBlocks.Single();
			Assert.AreEqual(2, followingPrimaryRefBlock.InitialStartVerseNumber);
			Assert.AreEqual(3, followingPrimaryRefBlock.InitialEndVerseNumber);
			Assert.AreEqual("Jesus", followingPrimaryRefBlock.CharacterId);
			var followingSecondaryRefBlock = followingPrimaryRefBlock.ReferenceBlocks.Single();
			Assert.AreEqual(2, followingSecondaryRefBlock.InitialStartVerseNumber);
			Assert.AreEqual(3, followingSecondaryRefBlock.InitialEndVerseNumber);
			Assert.AreEqual("Jesus", followingSecondaryRefBlock.CharacterId);
			var lastPrimaryRefBlock = matchup.CorrelatedBlocks[2].ReferenceBlocks.Single();
			Assert.AreEqual(4, lastPrimaryRefBlock.InitialStartVerseNumber);
			Assert.AreEqual(0, lastPrimaryRefBlock.InitialEndVerseNumber);
			Assert.AreEqual(narrator, lastPrimaryRefBlock.CharacterId);
			var lastSecondaryRefBlock = lastPrimaryRefBlock.ReferenceBlocks.Single();
			Assert.AreEqual(2, lastSecondaryRefBlock.InitialStartVerseNumber);
			Assert.AreEqual(3, lastSecondaryRefBlock.InitialEndVerseNumber);
			Assert.AreEqual(narrator, lastSecondaryRefBlock.CharacterId);
		}

		[Test]
		public void SetReferenceText_VerseNumberRemovedFromRefBlockOfFirstBlock_FollowingRefBlockDoesNotStartWithVerseNumber_InitialVerseRefOfFollowingReferenceBlockChanged()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse(CharacterVerseData.kUnexpectedCharacter, 1, "Entonces Jesus hablo, diciendo: ", true, 1, "p", 3));
			var refBlock1 = new Block("q", 1, 1);
			refBlock1.CharacterId = "Jesus";
			refBlock1.BlockElements.Add(new ScriptText("Then Jesus spoke, "));
			refBlock1.AddVerse(2, "saying, ");
			vernacularBlocks.Last().SetMatchedReferenceBlock(refBlock1);

			var block2 = ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "“Yo soy el pan de vida.”", "p");
			var refBlock2 = new Block("q", 1, 2);
			refBlock2.CharacterId = "Jesus";
			refBlock2.BlockElements.Add(new ScriptText("“Continuation of previous verse in ref text. "));
			refBlock2.AddVerse(3, "Three!”");
			block2.SetMatchedReferenceBlock(refBlock2);

			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, null);
			Assert.IsTrue(matchup.CorrelatedBlocks.All(b => b.MatchesReferenceText));

			matchup.SetReferenceText(0, "Then Jesus spoke, saying: ");
			Assert.IsTrue(matchup.CorrelatedBlocks.All(b => b.MatchesReferenceText));
			var followingRefBlock = matchup.CorrelatedBlocks[1].ReferenceBlocks.Single();

			Assert.AreEqual("Jesus", matchup.CorrelatedBlocks[1].CharacterId);
			Assert.AreEqual("“Continuation of previous verse in ref text. {3}\u00A0Three!”", matchup.CorrelatedBlocks[1].GetPrimaryReferenceText());
			Assert.AreEqual(1, followingRefBlock.InitialStartVerseNumber);
			Assert.AreEqual(3, followingRefBlock.InitialEndVerseNumber);
			Assert.AreEqual("Jesus", followingRefBlock.CharacterId);
		}

		[TestCase("\u00A0")]
		[TestCase(" ")]
		[TestCase("")]
		public void SetReferenceText_SubsequentBlock_VerseNumberInMiddle_SpecifiedBlockMatchedWithReferenceBlockWithInitialVerseNumberFromPreviousRefBlock(string separator)
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse(CharacterVerseData.kUnexpectedCharacter, 1, "Entonces Jesus hablo, diciendo: ", true, 1, "p", 3));
			vernacularBlocks.Last().SetMatchedReferenceBlock(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Then Jesus spoke unto them, ", true));
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "Este es versiculo dos y tres, ", "p");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Jesus. ");
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, null);

			var newRefBlock = matchup.SetReferenceText(1, "saying, {2-3}" + separator + "“This is verse two and three.”");
			// Ensure block 0 not changed
			Assert.AreEqual(CharacterVerseData.kUnexpectedCharacter, matchup.CorrelatedBlocks[0].CharacterId);
			Assert.IsTrue(matchup.CorrelatedBlocks[0].MatchesReferenceText);
			Assert.AreEqual("{1}\u00A0Then Jesus spoke unto them, ", matchup.CorrelatedBlocks[0].GetPrimaryReferenceText());

			Assert.IsTrue(matchup.CorrelatedBlocks[0].MatchesReferenceText);
			Assert.AreEqual("saying, {2-3}\u00A0“This is verse two and three.”", matchup.CorrelatedBlocks[1].GetPrimaryReferenceText());
			Assert.AreEqual(matchup.CorrelatedBlocks[1].ReferenceBlocks.Single(), newRefBlock);
			Assert.AreEqual("Jesus", newRefBlock.CharacterId, "Should get character info from vern block");
			Assert.AreEqual(1, newRefBlock.InitialStartVerseNumber);
			Assert.AreEqual(0, newRefBlock.InitialEndVerseNumber);
			Assert.AreEqual(3, newRefBlock.LastVerseNum);
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
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, ReferenceText.GetStandardReferenceText(ReferenceTextType.English));
			matchup.CorrelatedBlocks[0].SetMatchedReferenceBlock(
				ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "“This is verse two,” ", true));
			matchup.MatchAllBlocks(null);
			Assert.IsTrue(matchup.CorrelatedBlocks.All(b => b.MatchesReferenceText));
			Assert.AreEqual("", matchup.CorrelatedBlocks[1].GetPrimaryReferenceText());

			var newRefBlock = matchup.SetReferenceText(1, "{3}" + separator + "“And this is verse three {4}" + separator + "or maybe four.”");
			Assert.IsTrue(matchup.CorrelatedBlocks.All(b => b.MatchesReferenceText));
			// Ensure block 0 not changed
			Assert.AreEqual("Jesus", matchup.CorrelatedBlocks[0].CharacterId);
			Assert.AreEqual("{2}\u00A0“This is verse two,” ", matchup.CorrelatedBlocks[0].GetPrimaryReferenceText());

			Assert.AreEqual("{3}\u00A0“And this is verse three {4}\u00A0or maybe four.”", matchup.CorrelatedBlocks[1].GetPrimaryReferenceText());
			Assert.AreEqual(vernacularBlocks.Last().CharacterId, newRefBlock.CharacterId, "Should get character/delivery info from vern block");
			Assert.AreEqual("expressionless", newRefBlock.Delivery, "Should get character/delivery info from vern block");
			Assert.AreEqual(3, newRefBlock.InitialStartVerseNumber);
			Assert.AreEqual(0, newRefBlock.InitialEndVerseNumber);
			Assert.AreEqual(4, newRefBlock.LastVerseNum);
		}

		[TestCase("")]
		[TestCase("{F8 Music--Ends} ")]
		public void SetReferenceText_Secondary_AnnotationAndVerseNumberAtStart_VerseNumberNotLost(string annotationText)
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(2, "Esigit diluk nen ane yogiseke:", false, 18, "REV", "p", 3));
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "angel, another, coming down from heaven",
				"“O nggok Babel kagarogo ’bigi o! It kota Babel kagarigogo mende ma, en oba dagasinem,");
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "angel, another, coming down from heaven",
				"Kagat nabini, yiluk, akoni, kugi obabut weyak mende inom,");
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "angel, another, coming down from heaven",
				"Sewe nausak dek yiluk mende ta’bokogona inom, kagat laga o,” ");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "yiluk yogiseke.");
			var russianRefText = ReferenceText.GetStandardReferenceText(ReferenceTextType.Russian);
			var vernBook = new BookScript("REV", vernacularBlocks, russianRefText.Versification);
			var matchup = russianRefText.GetBlocksForVerseMatchedToReferenceText(vernBook, 0);
			matchup.MatchAllBlocks(null);
			matchup.SetReferenceText(0, "{2} He cried with a mighty voice, saying,", 1);
			matchup.SetReferenceText(1, "“Fallen is Babylon! She is a home of demons, spirits and unclean birds!", 1);
			matchup.SetReferenceText(2, "spirits and unclean birds!", 1);
			matchup.SetReferenceText(3, annotationText + "{3} All nations had sex and got rich with her.”", 1);
			var englishRefBlockForVernBlock3 = matchup.CorrelatedBlocks[3].ReferenceBlocks.Single().ReferenceBlocks.Single();
			if (annotationText.Length > 0)
				Assert.IsTrue(englishRefBlockForVernBlock3.BlockElements.OfType<Sound>().Single().SoundType == SoundType.Music);
			Assert.AreEqual("3", englishRefBlockForVernBlock3.BlockElements.OfType<Verse>().Single().Number);
			Assert.AreEqual("3", englishRefBlockForVernBlock3.InitialVerseNumberOrBridge);
			Assert.AreEqual("All nations had sex and got rich with her.”",
				((ScriptText)englishRefBlockForVernBlock3.BlockElements.Last()).Content);
		}

		[Test]
		public void InsertHeSaidText_EnglishOnly_SingleRow_TextSetAndCallbackCalled()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "Este es versiculo dos y tres, ", true, 1, "p", 3));
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Jesus. ");
			var narrator = vernacularBlocks.Last().CharacterId;
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, ReferenceText.GetStandardReferenceText(ReferenceTextType.English));
			matchup.CorrelatedBlocks[0].SetMatchedReferenceBlock(
				ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "“This is verse two,” ", true));
			matchup.MatchAllBlocks(null);
			Assert.AreEqual("", matchup.CorrelatedBlocks[1].GetPrimaryReferenceText());

			bool callbackCalled = false;

			matchup.InsertHeSaidText(1, (iRow, level, text) =>
			{
				Assert.AreEqual(1, iRow);
				Assert.AreEqual(0, level);
				Assert.AreEqual("he said.", text);
				Assert.IsFalse(callbackCalled);
				callbackCalled = true;
			});
			Assert.IsTrue(callbackCalled);
			Assert.IsTrue(matchup.CorrelatedBlocks.All(b => b.MatchesReferenceText));
			Assert.IsFalse(matchup.CorrelatedBlocks.Select(b => b.ReferenceBlocks.Single()).Any(rb => rb.ReferenceBlocks.Any()),
				"This matchup only has English - no additional levels should be present.");
			Assert.AreEqual(narrator, matchup.CorrelatedBlocks[1].CharacterId);
			Assert.AreEqual("he said.", matchup.CorrelatedBlocks[1].GetPrimaryReferenceText());
			Assert.AreEqual("Jesus", matchup.CorrelatedBlocks[0].CharacterId,
				"First reference text should not have been changed!");
			Assert.AreEqual("{2}\u00A0“This is verse two,” ", matchup.CorrelatedBlocks[0].GetPrimaryReferenceText(),
				"First reference text should not have been changed!");
		}

		[Test]
		public void InsertHeSaidText_ExistingVerseNumber_HeSaidTextInsertedAfterVerseNumber()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "Este es versiculo dos y tres, ", true, 1, "p", 3));
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Jesus. ");
			var narrator = vernacularBlocks.Last().CharacterId;
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, ReferenceText.GetStandardReferenceText(ReferenceTextType.English));
			matchup.CorrelatedBlocks[0].SetMatchedReferenceBlock(
				ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "“This is verse two,” ", true));
			matchup.CorrelatedBlocks[1].SetMatchedReferenceBlock(ReferenceTextTests.CreateNarratorBlockForVerse(3, ""));
			matchup.MatchAllBlocks(null);
			Assert.AreEqual("{3}\u00A0", matchup.CorrelatedBlocks[1].GetPrimaryReferenceText());

			bool callbackCalled = false;

			matchup.InsertHeSaidText(1, (iRow, level, text) =>
			{
				Assert.AreEqual(1, iRow);
				Assert.AreEqual(0, level);
				Assert.AreEqual("{3}\u00A0he said.", text);
				Assert.IsFalse(callbackCalled);
				callbackCalled = true;
			});
			Assert.IsTrue(callbackCalled);
			Assert.AreEqual(narrator, matchup.CorrelatedBlocks[1].CharacterId);
			Assert.AreEqual("{3}\u00A0he said.", matchup.CorrelatedBlocks[1].GetPrimaryReferenceText());
			Assert.AreEqual("Jesus", matchup.CorrelatedBlocks[0].CharacterId, "First reference text should not have been changed!");
			Assert.AreEqual("{2}\u00A0“This is verse two,” ", matchup.CorrelatedBlocks[0].GetPrimaryReferenceText(),
				"First reference text should not have been changed!");
		}

		[Test]
		public void InsertHeSaidText_NonNarrator_SingleRow_NoChanges()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "Este es versiculo dos y tres, ", true, 1, "p", 3));
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Jesus. ");
			var narrator = vernacularBlocks.Last().CharacterId;
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, ReferenceText.GetStandardReferenceText(ReferenceTextType.English));
			matchup.MatchAllBlocks(null);
			Assert.IsTrue(matchup.CorrelatedBlocks.Select(b => b.GetPrimaryReferenceText()).All(t => t == ""));

			bool callbackCalled = false;

			matchup.InsertHeSaidText(0, (iRow, level, text) => { callbackCalled = true; });
			Assert.IsFalse(callbackCalled);
			Assert.IsTrue(matchup.CorrelatedBlocks.Select(b => b.GetPrimaryReferenceText()).All(t => t == ""));
			Assert.IsFalse(matchup.CorrelatedBlocks.Select(b => b.ReferenceBlocks.Single()).Any(rb => rb.ReferenceBlocks.Any()),
				"This matchup only has English - no additional levels should be present.");
			Assert.AreEqual(narrator, matchup.CorrelatedBlocks[1].CharacterId);
			Assert.AreEqual("Jesus", matchup.CorrelatedBlocks[0].CharacterId);
			Assert.AreEqual(narrator, matchup.CorrelatedBlocks[1].ReferenceBlocks.Single().CharacterId, "Reference text character id should not have been changed!");
			Assert.AreEqual("Jesus", matchup.CorrelatedBlocks[0].ReferenceBlocks.Single().CharacterId, "Reference text character id should not have been changed!");
		}

		[Test]
		public void InsertHeSaidText_NonEmpty_SingleRow_NoChanges()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "Este es versiculo dos y tres, ", true, 1, "p", 3));
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Jesus. ");
			var narrator = vernacularBlocks.Last().CharacterId;
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, ReferenceText.GetStandardReferenceText(ReferenceTextType.English));
			matchup.CorrelatedBlocks[1].SetMatchedReferenceBlock(ReferenceTextTests.CreateNarratorBlockForVerse(3, "This is not empty, dude."));
			var origRefText = matchup.CorrelatedBlocks[1].GetPrimaryReferenceText();
			matchup.MatchAllBlocks(null);

			bool callbackCalled = false;

			matchup.InsertHeSaidText(1, (iRow, level, text) => { callbackCalled = true; });
			Assert.IsFalse(callbackCalled);
			Assert.AreEqual(origRefText, matchup.CorrelatedBlocks[1].GetPrimaryReferenceText(),
				"Reference text should not have been changed!");
			Assert.IsFalse(matchup.CorrelatedBlocks.Select(b => b.ReferenceBlocks.Single()).Any(rb => rb.ReferenceBlocks.Any()),
				"This matchup only has English - no additional levels should be present.");
			Assert.AreEqual(narrator, matchup.CorrelatedBlocks[1].CharacterId);
			Assert.AreEqual("Jesus", matchup.CorrelatedBlocks[0].CharacterId);
			Assert.AreEqual(narrator, matchup.CorrelatedBlocks[1].ReferenceBlocks.Single().CharacterId, "Reference text character id should not have been changed!");
			Assert.AreEqual("Jesus", matchup.CorrelatedBlocks[0].ReferenceBlocks.Single().CharacterId, "Reference text character id should not have been changed!");
		}

		[Test]
		public void InsertHeSaidText_MultipleEmptyVerses_SingleRow_NoChanges()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "Este es versiculo dos y tres, ", true, 1, "p", 3));
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Jesus. ");
			var narrator = vernacularBlocks.Last().CharacterId;
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, ReferenceText.GetStandardReferenceText(ReferenceTextType.English));
			matchup.CorrelatedBlocks[1].SetMatchedReferenceBlock(ReferenceTextTests.CreateNarratorBlockForVerse(3, "").AddVerse(4, ""));
			var origRefText = matchup.CorrelatedBlocks[1].GetPrimaryReferenceText();
			matchup.MatchAllBlocks(null);

			bool callbackCalled = false;

			matchup.InsertHeSaidText(1, (iRow, level, text) => { callbackCalled = true; });
			Assert.IsFalse(callbackCalled);
			Assert.AreEqual(origRefText, matchup.CorrelatedBlocks[1].GetPrimaryReferenceText(),
				"Reference text should not have been changed!");
			Assert.IsFalse(matchup.CorrelatedBlocks.Select(b => b.ReferenceBlocks.Single()).Any(rb => rb.ReferenceBlocks.Any()),
				"This matchup only has English - no additional levels should be present.");
			Assert.AreEqual(narrator, matchup.CorrelatedBlocks[1].CharacterId);
			Assert.AreEqual("Jesus", matchup.CorrelatedBlocks[0].CharacterId);
			Assert.AreEqual(narrator, matchup.CorrelatedBlocks[1].ReferenceBlocks.Single().CharacterId, "Reference text character id should not have been changed!");
			Assert.AreEqual("Jesus", matchup.CorrelatedBlocks[0].ReferenceBlocks.Single().CharacterId, "Reference text character id should not have been changed!");
		}

		[Test]
		public void InsertHeSaidText_UnknownCharacter_SingleRow_CharacterChangedToNarrator()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "Este es versiculo dos y tres, ", true, 1, "p", 3));
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, CharacterVerseData.kUnexpectedCharacter, "dijo Jesus. ");
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, ReferenceText.GetStandardReferenceText(ReferenceTextType.English));
			matchup.MatchAllBlocks(null);
			Assert.IsTrue(matchup.CorrelatedBlocks.Select(b => b.GetPrimaryReferenceText()).All(t => t == ""));

			bool callbackCalled = false;

			matchup.InsertHeSaidText(1, (iRow, level, text) =>
			{
				Assert.IsFalse(callbackCalled);
				callbackCalled = true;
			});
			Assert.IsTrue(callbackCalled);

			Assert.AreEqual("he said.", matchup.CorrelatedBlocks[1].GetPrimaryReferenceText());
			var narrator = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator);
			Assert.AreEqual(narrator, matchup.CorrelatedBlocks[1].CharacterId);
			Assert.AreEqual(narrator, matchup.CorrelatedBlocks[1].ReferenceBlocks.Single().CharacterId);

			Assert.AreEqual("Jesus", matchup.CorrelatedBlocks[0].CharacterId);
			Assert.AreEqual("Jesus", matchup.CorrelatedBlocks[0].ReferenceBlocks.Single().CharacterId, "Reference text character id should not have been changed!");
			Assert.AreEqual("", matchup.CorrelatedBlocks[0].GetPrimaryReferenceText());
		}

		[TestCase(ReferenceTextType.Custom, TestReferenceText.TestReferenceTextResource.AzeriJUD, "dedi.")]
		[TestCase(ReferenceTextType.Custom, TestReferenceText.TestReferenceTextResource.FrenchMAT, "il a dit.")]
		//[TestCase(ReferenceTextType.Indonesian, "katanya.")]
		//[TestCase(ReferenceTextType.Portuguese, "disse.")]
		[TestCase(ReferenceTextType.Custom, TestReferenceText.TestReferenceTextResource.SpanishMAT, "dijo.")]
		//[TestCase(ReferenceTextType.TokPisin, "i bin tok.")]
		[TestCase(ReferenceTextType.Russian, null, "сказал.")]
		public void InsertHeSaidText_NonEnglishPrimary_SingleRow_TextSetAndCallbackCalled(ReferenceTextType type,
			TestReferenceText.TestReferenceTextResource customLanguageBook, string expectedText)
		{
			ReferenceText rt = type == ReferenceTextType.Custom ? TestReferenceText.CreateCustomReferenceText(customLanguageBook) :
				ReferenceText.GetStandardReferenceText(type);

			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "“Tio estas verso du,” ", true));
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "diris Jesuo. ");
			var narrator = vernacularBlocks.Last().CharacterId;
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, rt);
			matchup.CorrelatedBlocks[0].SetMatchedReferenceBlock(
				ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "“Esto es versiculo dos,” ", true));
			matchup.CorrelatedBlocks[0].ReferenceBlocks.Single().SetMatchedReferenceBlock(
				ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "“This is verse two,” ", true));
			matchup.MatchAllBlocks(null);

			Assert.IsTrue(matchup.CorrelatedBlocks.All(b => b.MatchesReferenceText));
			Assert.AreEqual("", matchup.CorrelatedBlocks[1].GetPrimaryReferenceText());

			bool callbackCalledForEnglish = false;
			bool callbackCalledForPrimary = false;

			matchup.InsertHeSaidText(1, (iRow, level, text) =>
			{
				Assert.AreEqual(1, iRow);
				if (level == 1)
				{
					Assert.AreEqual("he said.", text);
					Assert.IsFalse(callbackCalledForEnglish);
					callbackCalledForEnglish = true;
				}
				else
				{
					Assert.AreEqual(0, level);
					Assert.AreEqual(expectedText, text);
					Assert.IsFalse(callbackCalledForPrimary);
					callbackCalledForPrimary = true;					
				}
			});
			Assert.IsTrue(callbackCalledForEnglish);
			Assert.IsTrue(callbackCalledForPrimary);
			Assert.IsTrue(matchup.CorrelatedBlocks.All(b => b.MatchesReferenceText));
			Assert.IsTrue(matchup.CorrelatedBlocks.Select(b => b.ReferenceBlocks.Single()).All(rb => rb.MatchesReferenceText),
				"This matchup should have a primary reference text plus English.");
			Assert.AreEqual(narrator, matchup.CorrelatedBlocks[1].CharacterId);
			Assert.AreEqual(narrator, matchup.CorrelatedBlocks[1].ReferenceBlocks.Single().CharacterId);
			Assert.AreEqual(expectedText, matchup.CorrelatedBlocks[1].GetPrimaryReferenceText());
			Assert.AreEqual("he said.", matchup.CorrelatedBlocks[1].ReferenceBlocks.Single().GetPrimaryReferenceText());

			Assert.AreEqual("Jesus", matchup.CorrelatedBlocks[0].CharacterId,
				"First reference text (primary) should not have been changed!");
			Assert.AreEqual("Jesus", matchup.CorrelatedBlocks[0].ReferenceBlocks.Single().CharacterId,
				"First reference text (English) should not have been changed!");
			Assert.AreEqual("{2}\u00A0“Esto es versiculo dos,” ", matchup.CorrelatedBlocks[0].GetPrimaryReferenceText(),
				"First reference text (primary) should not have been changed!");
			Assert.AreEqual("{2}\u00A0“This is verse two,” ", matchup.CorrelatedBlocks[0].ReferenceBlocks.Single().GetPrimaryReferenceText(),
				"First reference text (English) should not have been changed!");
		}

		[Test]
		public void InsertHeSaidText_NonEnglishPrimary_AllRows_TextSetAndCallbackCalledOnlyForEmptyRefTexts()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "“Tio estas verso du,” ", true));
			var vernJesusSaidBlock = ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "diris Jesuo. ");
			var narrator = vernJesusSaidBlock.CharacterId;
			var refDijoJesusBlock = new Block("p", 1, 2);
			refDijoJesusBlock.CharacterId = narrator;
			refDijoJesusBlock.BlockElements.Add(new ScriptText("dijo Jesus. "));
			vernJesusSaidBlock.SetMatchedReferenceBlock(refDijoJesusBlock);
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Peter", "“Tio estas verso tre,” ");
			var vernPeterSaidBlock = ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "diris Petro.");
			var refEmptySpanishRefTextBlock = new Block("p", 1, 2) {IsParagraphStart = false};
			refEmptySpanishRefTextBlock.BlockElements.Add(new ScriptText(""));
			var refPeterSaidBlock = new Block("p", 1, 2) { IsParagraphStart = false };
			refPeterSaidBlock.CharacterId = narrator;
			refPeterSaidBlock.BlockElements.Add(new ScriptText("Peter said."));
			refEmptySpanishRefTextBlock.SetMatchedReferenceBlock(refPeterSaidBlock);
			vernPeterSaidBlock.SetMatchedReferenceBlock(refEmptySpanishRefTextBlock);
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			ReferenceText rt = TestReferenceText.CreateCustomReferenceText(TestReferenceText.TestReferenceTextResource.SpanishMAT);
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, rt);

			var callbacks = new List<Tuple<int, int, string>>();

			matchup.InsertHeSaidText(-1, (iRow, level, text) =>
			{
				callbacks.Add(new Tuple<int, int, string>(iRow, level, text));
			});
			Assert.AreEqual(2, callbacks.Count);
			Assert.AreEqual(1, callbacks.Count(c => c.Item1 == 1 && c.Item2 == 1 && c.Item3 == "he said. "));
			Assert.AreEqual(1, callbacks.Count(c => c.Item1 == 3 && c.Item2 == 0 && c.Item3 == "dijo."));

			var row1VernBlock = matchup.CorrelatedBlocks[1];
			Assert.IsTrue(row1VernBlock.MatchesReferenceText);
			Assert.AreEqual(narrator, row1VernBlock.CharacterId);
			Assert.AreEqual("dijo Jesus. ", row1VernBlock.GetPrimaryReferenceText());
			var row1SpanishRefBlock = row1VernBlock.ReferenceBlocks.Single();
			Assert.IsTrue(row1SpanishRefBlock.MatchesReferenceText);
			Assert.AreEqual(narrator, row1SpanishRefBlock.CharacterId);
			Assert.AreEqual("he said. ", row1SpanishRefBlock.GetPrimaryReferenceText());

			var row3VernBlock = matchup.CorrelatedBlocks[3];
			Assert.IsTrue(row3VernBlock.MatchesReferenceText);
			Assert.AreEqual(narrator, row3VernBlock.CharacterId);
			Assert.AreEqual("dijo.", row3VernBlock.GetPrimaryReferenceText());
			var row3SpanishRefBlock = row3VernBlock.ReferenceBlocks.Single();
			Assert.IsTrue(row3SpanishRefBlock.MatchesReferenceText);
			Assert.AreEqual(narrator, row3SpanishRefBlock.CharacterId);
			Assert.AreEqual("Peter said.", row3SpanishRefBlock.GetPrimaryReferenceText());

			Assert.IsFalse(matchup.CorrelatedBlocks[0].MatchesReferenceText);
			Assert.IsFalse(matchup.CorrelatedBlocks[2].MatchesReferenceText);
		}

		[TestCase(CharacterVerseData.kUnexpectedCharacter, null)]
		[TestCase(CharacterVerseData.kAmbiguousCharacter, null)]
		[TestCase(CharacterVerseData.kUnexpectedCharacter, "Andrew")]
		[TestCase(CharacterVerseData.kAmbiguousCharacter, "Andrew")]
		public void InsertHeSaidText_EmptyRowsAreStartOfMultiBlockQuote_HeSaidInsertedIntoEmptyRefTextsAndBlockChainBroken(string origCharacter, string refTextCharacter)
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(1, "After these things, Jesus taught them, saying: ", true));
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "“Tio estas verso du,” "));
			var vernJesusSaidBlock = ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, origCharacter, "diris Jesuo. Some more stuff. ");
			vernJesusSaidBlock.MultiBlockQuote = MultiBlockQuote.Start;
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse(origCharacter, 3, "This is a continuation of the quote in the previous block. "));
			vernacularBlocks.Last().MultiBlockQuote = MultiBlockQuote.Continuation;
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse(origCharacter, 4, "“And this is the rest of the mumblings."));
			vernacularBlocks.Last().MultiBlockQuote = MultiBlockQuote.Continuation;

			var narrator = ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks,
				"Then it came about that the next thing happened:").CharacterId;
			if (refTextCharacter == null)
				refTextCharacter = narrator;
			vernacularBlocks.Last().IsParagraphStart = true;
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse("Andrew", 5, "“I feel like being a disciple,” "));
			var vernAndrewSaidBlock = ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, origCharacter, "diris Andreo. ");
			vernAndrewSaidBlock.MultiBlockQuote = MultiBlockQuote.Start;
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse(origCharacter, 6, "And so he did. "));
			vernacularBlocks.Last().MultiBlockQuote = MultiBlockQuote.Continuation;

			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			ReferenceText rt = ReferenceText.GetStandardReferenceText(ReferenceTextType.English);
			rt.ApplyTo(vernBook);
			Assert.AreEqual(1, vernacularBlocks[1].ReferenceBlocks.Count);
			Assert.IsFalse(vernJesusSaidBlock.MatchesReferenceText);
			Assert.IsFalse(vernJesusSaidBlock.ReferenceBlocks.Any());
			vernacularBlocks[3].SetMatchedReferenceBlock("This is some arbitrary reference text.");
			vernacularBlocks[3].ReferenceBlocks.Single().CharacterId = refTextCharacter;
			Assert.IsFalse(vernacularBlocks[4].ReferenceBlocks.Any());
			vernacularBlocks[4].SetMatchedReferenceBlock("This is some arbitrary reference text.");
			vernacularBlocks[4].ReferenceBlocks.Single().CharacterId = refTextCharacter;
			Assert.AreEqual(narrator, vernacularBlocks[5].ReferenceBlocks.Single().CharacterId);
			Assert.AreEqual(narrator, vernacularBlocks[6].ReferenceBlocks.Single().CharacterId);
			Assert.IsFalse(vernAndrewSaidBlock.MatchesReferenceText);
			Assert.IsFalse(vernAndrewSaidBlock.ReferenceBlocks.Any());
			Assert.AreEqual(2, vernacularBlocks.Count(vb => vb.GetReferenceTextAtDepth(0) == ""));
			var matchup = new BlockMatchup(vernBook, 0, null, i => false, rt);
			matchup.MatchAllBlocks(ScrVers.English);
			// MatchAllBlocks sets the correlated blocks to have the character ID of their corresponding reference text.
			// But in AssignCharacterDlg.UpdateReferenceTextTabPageDisplay, we reset all the continuation blocks' character
			// IDs back to match that of their start blocks because we can't let them get out of sync. So we need to
			// simulate that here as well:
			for (int i = 1; i < matchup.CorrelatedBlocks.Count; i++)
			{
				if (matchup.CorrelatedBlocks[i].MultiBlockQuote == MultiBlockQuote.Continuation)
					matchup.CorrelatedBlocks[i].CharacterId = matchup.CorrelatedBlocks[i - 1].CharacterId;
			}
			var origBlock3RefText = matchup.CorrelatedBlocks[3].GetPrimaryReferenceText();

			var callbacks = new List<Tuple<int, int, string>>();

			matchup.InsertHeSaidText(-1, (iRow, level, text) =>
			{
				callbacks.Add(new Tuple<int, int, string>(iRow, level, text));
			});
			Assert.AreEqual(2, callbacks.Count);
			Assert.AreEqual(1, callbacks.Count(c => c.Item1 == 2 && c.Item2 == 0 && c.Item3 == "he said. "));
			Assert.AreEqual(1, callbacks.Count(c => c.Item1 == 7 && c.Item2 == 0 && c.Item3 == "he said. "));

			var row2VernBlock = matchup.CorrelatedBlocks[2];
			Assert.IsTrue(row2VernBlock.MatchesReferenceText);
			Assert.AreEqual(narrator, row2VernBlock.CharacterId);
			Assert.AreEqual("he said. ", row2VernBlock.GetPrimaryReferenceText());
			Assert.AreEqual(MultiBlockQuote.None, row2VernBlock.MultiBlockQuote);

			Assert.AreEqual(origBlock3RefText, matchup.CorrelatedBlocks[3].GetPrimaryReferenceText());
			Assert.AreEqual(refTextCharacter, matchup.CorrelatedBlocks[3].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, matchup.CorrelatedBlocks[3].MultiBlockQuote);
			Assert.AreEqual(refTextCharacter, matchup.CorrelatedBlocks[4].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, matchup.CorrelatedBlocks[4].MultiBlockQuote);

			var row7VernBlock = matchup.CorrelatedBlocks[7];
			Assert.IsTrue(row7VernBlock.MatchesReferenceText);
			Assert.AreEqual(narrator, row7VernBlock.CharacterId);
			Assert.AreEqual("he said. ", row7VernBlock.GetPrimaryReferenceText());
			Assert.AreEqual(MultiBlockQuote.None, row7VernBlock.MultiBlockQuote);

			Assert.AreEqual(narrator, matchup.CorrelatedBlocks.Last().CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, matchup.CorrelatedBlocks.Last().MultiBlockQuote);
		}

		[TestCase(CharacterVerseData.kUnexpectedCharacter)]
		[TestCase(CharacterVerseData.kAmbiguousCharacter)]
		public void InsertHeSaidText_EmptyRowsAreStartOfMultiBlockQuote_ContBlocksNotAlignedToRefText_HeSaidInsertedIntoEmptyRefTextsAndContBlocksSetToNarrator(string origCharacter)
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Después de estas cosas, Jesús les enseño, diciendo: ", true));
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "“Tio estas verso du,” "));
			var vernJesusSaidBlock = ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, origCharacter, "diris Jesuo. Etcetera. ");
			vernJesusSaidBlock.MultiBlockQuote = MultiBlockQuote.Start;
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse(origCharacter, 3, "This is actually spoken by the narrator but was originally parsed as quoted speech. "));
			vernacularBlocks.Last().MultiBlockQuote = MultiBlockQuote.Continuation;
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse(origCharacter, 4, "And this is the rest of the narrator's mumblings."));
			vernacularBlocks.Last().MultiBlockQuote = MultiBlockQuote.Continuation;
			var narrator = ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks,
				"Luego sucedió la próxima cosa.").CharacterId;

			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			ReferenceText rt = ReferenceText.GetStandardReferenceText(ReferenceTextType.English);//TestReferenceText.CreateCustomReferenceText(TestReferenceText.TestReferenceTextResource.SpanishMAT);
			vernacularBlocks[0].SetMatchedReferenceBlock("After these things, Jesus taught them, saying: ");
			vernacularBlocks[1].SetMatchedReferenceBlock("“Uncle, you are verse two,” ");
			vernacularBlocks.Last().SetMatchedReferenceBlock("Then it came about that the next thing happened.");
			Assert.AreEqual(3, vernacularBlocks.Count(vb => vb.GetReferenceTextAtDepth(0) == ""));
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, rt, (uint)vernacularBlocks.Count);
			matchup.MatchAllBlocks(ScrVers.English);

			var callbacks = new List<Tuple<int, int, string>>();

			matchup.InsertHeSaidText(2, (iRow, level, text) =>
			{
				callbacks.Add(new Tuple<int, int, string>(iRow, level, text));
			});
			Assert.AreEqual(3, callbacks.Count);
			Assert.AreEqual(1, callbacks.Count(c => c.Item1 == 2 && c.Item2 == 0 && c.Item3 == "he said. "));
			Assert.AreEqual(1, callbacks.Count(c => c.Item1 == 3 && c.Item2 == 0 && c.Item3 == null));
			Assert.AreEqual(1, callbacks.Count(c => c.Item1 == 4 && c.Item2 == 0 && c.Item3 == null));

			var row2VernBlock = matchup.CorrelatedBlocks[2];
			Assert.IsTrue(row2VernBlock.MatchesReferenceText);
			Assert.AreEqual(narrator, row2VernBlock.CharacterId);
			Assert.AreEqual("he said. ", row2VernBlock.GetPrimaryReferenceText());
			Assert.AreEqual(MultiBlockQuote.None, row2VernBlock.MultiBlockQuote);

			Assert.AreEqual("", matchup.CorrelatedBlocks[3].GetPrimaryReferenceText());
			Assert.AreEqual(narrator, matchup.CorrelatedBlocks[3].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, matchup.CorrelatedBlocks[3].MultiBlockQuote);
			Assert.AreEqual("", matchup.CorrelatedBlocks[4].GetPrimaryReferenceText());
			Assert.AreEqual(narrator, matchup.CorrelatedBlocks[4].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, matchup.CorrelatedBlocks[4].MultiBlockQuote);
		}

		[TestCase(1)]
		[TestCase(2)]
		public void ChangeAnchor_StateReflectsCorrectAnchorBlock(int iBlock)
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
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, iBlock, null, i => true, null);
			matchup.ChangeAnchor(matchup.CorrelatedBlocks[iBlock]);
			Assert.AreEqual(matchup.CorrelatedBlocks[iBlock], matchup.CorrelatedAnchorBlock);
		}

		[Test]
		public void GetCorrespondingOriginalBlock_BlockNotInMatchup_ReturnsNull()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(1, "This is a leading verse that should not be included.", true));
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(2, "Partieron de alli para Jerico. ", true).AddVerse(3, "Entonces dijo Jesus: "));
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "Come to me you who are weary,", "q1");
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "And I will give you rest.", "q1");
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 1, null, i => true, null);
			Assert.IsNull(matchup.GetCorrespondingOriginalBlock(vernacularBlocks[0]));
		}

		[Test]
		public void GetCorrespondingOriginalBlock_NoSplits_SingleMatch_ReturnsMatchingBlock()
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
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 1, null, i => true, null);
			for (int i = 0; i < matchup.CorrelatedBlocks.Count; i++)
				Assert.AreEqual(vernacularBlocks[i + 1], matchup.GetCorrespondingOriginalBlock(matchup.CorrelatedBlocks[i]));
		}

		[Test]
		public void GetCorrespondingOriginalBlock_Splits_SingleMatch_ReturnsCorrespondingBlock()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus abrio su boca y enseno.", true));
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "Este es versiculo dos, ", true));
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Jesus.");
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(3, "Despues se fue. ", true));
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 1, p => p.SplitBlock(p.GetScriptBlocks().First(), "2", 8, false), i => true, null);
			Assert.AreEqual(3, matchup.CorrelatedBlocks.Count);
			Assert.AreEqual(1, matchup.CountOfBlocksAddedBySplitting);
			Assert.AreEqual(vernacularBlocks[1], matchup.GetCorrespondingOriginalBlock(matchup.CorrelatedBlocks[0]));
			Assert.AreEqual(vernacularBlocks[1], matchup.GetCorrespondingOriginalBlock(matchup.CorrelatedBlocks[1]));
			Assert.AreEqual(vernacularBlocks[2], matchup.GetCorrespondingOriginalBlock(matchup.CorrelatedBlocks[2]));
		}

		[Test] public void GetCorrespondingOriginalBlock_MultipleIdenticalBlocks_ReturnsCorrespondingBlock()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(21, "They were in the middle of the meal. ", true, 26));
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "“I tell you the truth, one of you will betray me,”");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "accused Jesus. ").AddVerse(22, "They were very sad. ");
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Peter", "“Surely not I!” ");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "said Peter. ");
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Matthew", "“Surely not I!” ");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "said Matthew. ");
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Thomas", "“Surely not I!” ");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "said Thomas. ");
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "disciples", "“Surely not I!” ");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "said the other disciples.");
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 0,
				p => p.SplitBlock(p.GetScriptBlocks().ElementAt(2), "21", BookScript.kSplitAtEndOfVerse, false), i => true, null);
			Assert.AreEqual(12, matchup.CorrelatedBlocks.Count);
			Assert.AreEqual(1, matchup.CountOfBlocksAddedBySplitting);
			for (int i = 3; i < matchup.CorrelatedBlocks.Count; i++)
				Assert.AreEqual(vernacularBlocks[i - 1], matchup.GetCorrespondingOriginalBlock(matchup.CorrelatedBlocks[i]));
		}

		[Test]
		public void GetCorrespondingOriginalBlock_BlockWithTextThatisSubstringOfAnotherBlock_ReturnsCorrespondingBlock()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(21, "They were in the middle of the meal. ", true, 26));
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "“I tell you the truth, one of you will betray me,”");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "accused Jesus. ").AddVerse(22, "They were very sad. ");
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Peter", "No I won't! ");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "said Peter. ");
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Matthew", "No ");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "said Matthew. ");
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Thomas", "I won't! ");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "said Thomas. ");
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "disciples", "“Surely not I!” ");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "said the other disciples.");
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 0,
				p => p.SplitBlock(p.GetScriptBlocks().ElementAt(2), "21", BookScript.kSplitAtEndOfVerse, false), i => true, null);
			Assert.AreEqual(12, matchup.CorrelatedBlocks.Count);
			Assert.AreEqual(1, matchup.CountOfBlocksAddedBySplitting);
			for (int i = 3; i < matchup.CorrelatedBlocks.Count; i++)
				Assert.AreEqual(vernacularBlocks[i - 1], matchup.GetCorrespondingOriginalBlock(matchup.CorrelatedBlocks[i]));
		}

		[Test]
		public void CanChangeCharacterAndDeliveryInfo_NoParameters_ThrowsArgumentException()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(21, "They were in the middle of the meal. ", true, 26));
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "“I tell you the truth, one of you will betray me,”");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "accused Jesus. ").AddVerse(22, "They were very sad. ");
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, null);
			Assert.Throws<ArgumentException>(() => matchup.CanChangeCharacterAndDeliveryInfo());
		}

		[Test]
		public void CanChangeCharacterAndDeliveryInfo_NoNarratorRows_ReturnsTrue()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(21, "They were in the middle of the meal. ", true, 26));
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "“I tell you the truth, one of you will betray me,”");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "accused Jesus. ");
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Peter", "No I won't! ");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "said Peter. ");
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Matthew", "No ");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "said Matthew. ");
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Thomas", "I won't! ");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "said Thomas. ");
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "disciples", "“Surely not I!” ");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "said the other disciples.");
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, null);
			Assert.IsTrue(matchup.CanChangeCharacterAndDeliveryInfo(1, 3, 5, 7, 9));
		}

		[Test]
		public void CanChangeCharacterAndDeliveryInfo_IncludesNarratorRow_ReturnsFalse()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(21, "They were in the middle of the meal. ", true, 26));
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "“I tell you the truth, one of you will betray me,”");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "accused Jesus. ");
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Peter", "No I won't! ");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "said Peter. ");
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Matthew", "No ");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "said Matthew. ");
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Thomas", "I won't! ");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "said Thomas. ");
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "disciples", "“Surely not I!” ");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "said the other disciples.");
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, null);
			Assert.IsFalse(matchup.CanChangeCharacterAndDeliveryInfo(2, 3));
		}
	}
}
