using System;
using System.Collections.Generic;
using System.Linq;
using Glyssen.Shared;
using GlyssenEngine;
using GlyssenEngine.Script;
using GlyssenSharedTests;
using NUnit.Framework;
using SIL.Scripture;
using static GlyssenCharacters.CharacterVerseData;
using static GlyssenCharacters.CharacterVerseData.StandardCharacter;
using static GlyssenEngine.Script.PortionScript;
using static GlyssenSharedTests.CustomConstraints;
// There are lots of (pseudo) vernacular strings in this test file, so at the risk of missing a
// real typo, we disable these warnings.
// ReSharper disable CommentTypo

namespace GlyssenEngineTests.Script
{
	[TestFixture]
	public class BlockMatchupTests
	{
		[TearDown]
		public void Teardown()
		{
			TestReferenceText.ForgetCustomReferenceTexts();
		}

		[TestCase(0)]
		[TestCase(1)]
		[TestCase(2)]
		public void Constructor_SingleVerseInSingleBlock_CorrelatedBlocksContainsOnlyCloneOfThatBlock(int iBlock)
		{
			var vernacularBlocks = new List<Block>
			{
				ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces dijo Jesus: ", true),
				ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "Este es versiculo dos, ", true),
				ReferenceTextTests.CreateNarratorBlockForVerse(3, "Asi dijo. Despues se fue. ", true)
			};
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, iBlock, null, i => true, null);
			Assert.That(vernacularBlocks[iBlock].GetText(true), Is.EqualTo(matchup.CorrelatedBlocks.Single().GetText(true)));
			Assert.That(vernacularBlocks, Does.Not.Contain(matchup.CorrelatedBlocks.Single()));
			//Assert.That(vernacularBlocks[iBlock], Is.EqualTo(matchup.OriginalAnchorBlock));
			Assert.That(matchup.CorrelatedBlocks.Single(), Is.EqualTo(matchup.CorrelatedAnchorBlock));
		}

		[TestCase(1)]
		[TestCase(2)]
		[TestCase(3)]
		[TestCase(4)]
		[TestCase(5)]
		[TestCase(6)]
		public void Constructor_VernVerseStartsAndEndsMidBlock_CorrelatedBlocksContainAllBlocksUntilCleanVerseBreak(int iBlock)
		{
			var vernacularBlocks = new List<Block>
			{
				ReferenceTextTests.CreateNarratorBlockForVerse(1, "This is a leading verse that should not be included.", true),
				ReferenceTextTests.CreateNarratorBlockForVerse(2, "Partieron de alli para Jerico. ", true).AddVerse(3, "Entonces dijo Jesus: ")
			};
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "Come to me you who are weary,", "q1");
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "And I will give you rest.", "q1");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "Pretty sweet invitation!").AddVerse(4, "Start of another verse.");
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Peter", "Okay, I guess I will. ");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "said Peter.");
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(5, "This is a trailing verse that should not be included.", true));
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, iBlock, null, i => true, null);
			Assert.That(matchup.CorrelatedBlocks.Select(b => b.GetText(true))
				.SequenceEqual(vernacularBlocks.Skip(1).Take(6).Select(b => b.GetText(true))), Is.True);
			Assert.That(vernacularBlocks.Intersect(matchup.CorrelatedBlocks).Count(), Is.EqualTo(0));
			//Assert.That(vernacularBlocks[iBlock], Is.EqualTo(matchup.OriginalAnchorBlock));
			Assert.That(matchup.CorrelatedBlocks[iBlock - 1], Is.EqualTo(matchup.CorrelatedAnchorBlock));
		}

		[TestCase(1)]
		[TestCase(3)]
		[TestCase(4)]
		public void Constructor_VernVerseCrossesSectionHead_CorrelatedBlocksIncludeSectionHeadAndVerseBlocksOnEitherSide(int iBlock)
		{
			var vernacularBlocks = new List<Block>
			{
				ReferenceTextTests.CreateNarratorBlockForVerse(1, "This is a leading verse that should not be included.", true),
				ReferenceTextTests.CreateNarratorBlockForVerse(2, "Partieron de alli para Jerico. ", true)
			};
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks,
				GetStandardCharacterId("MAT", ExtraBiblical),
				"Big change in Topic", "s").IsParagraphStart = true;
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "Peter said: ").IsParagraphStart = true;
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Peter", "Nice place you got here!");
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(3, "This is a trailing verse that should not be included.", true));
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, iBlock, null, i => true, null);
			Assert.That(matchup.CorrelatedBlocks.Select(b => b.GetText(true))
				.SequenceEqual(vernacularBlocks.Skip(1).Take(4).Select(b => b.GetText(true))), Is.True);
			Assert.That(vernacularBlocks.Intersect(matchup.CorrelatedBlocks).Count(), Is.EqualTo(0));
		}

		[TestCase(1)]
		[TestCase(2)]
		public void Constructor_VernVerseEndsAtSectionHead_CorrelatedBlocksExcludesSectionHead(int iBlock)
		{
			var vernacularBlocks = new List<Block>
			{
				ReferenceTextTests.CreateNarratorBlockForVerse(1, "This is a leading verse that should not be included.", true),
				ReferenceTextTests.CreateNarratorBlockForVerse(2, "Partieron de alli para Jerico. ", true)
			};
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Peter", "Nice place you got here!");
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks,
				GetStandardCharacterId("MAT", ExtraBiblical),
				"Big change in Topic", "s").IsParagraphStart = true;
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(3, "This is a trailing verse that should not be included.", true));
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, iBlock, null, i => true, null);
			Assert.That(matchup.CorrelatedBlocks.Select(b => b.GetText(true))
				.SequenceEqual(vernacularBlocks.Skip(1).Take(2).Select(b => b.GetText(true))), Is.True);
			Assert.That(vernacularBlocks.Intersect(matchup.CorrelatedBlocks).Count(), Is.EqualTo(0));
			Assert.That(matchup.IncludesBlock(vernacularBlocks[0]), Is.False);
			Assert.That(matchup.IncludesBlock(vernacularBlocks[1]), Is.True);
			Assert.That(matchup.IncludesBlock(vernacularBlocks[2]), Is.True);
			Assert.That(matchup.IncludesBlock(vernacularBlocks[3]), Is.False);
			Assert.That(matchup.IncludesBlock(vernacularBlocks[4]), Is.False);
		}

		[Test]
		public void Constructor_AlternateVernVerseRepeatedLater_CorrelatedBlocksContainsCorrectAnchorVerse()
		{
			var vernacularBlocks = new List<Block>
			{
				ReferenceTextTests.CreateNarratorBlockForVerse(8, "This is a leading verse that should not be included.", true, 16, "MRK"),
				ReferenceTextTests.CreateNarratorBlockForVerse(9, "Habiendo resucitado Jesús, apareció a María Magdalena. ", true, 16, "MRK")
					.AddVerse(10, "Ella lo hizo saber a los demas. ").AddVerse(11, "Ellos no creyeron. "),
				ReferenceTextTests.CreateNarratorBlockForVerse(12, "Pero después apareció a 2 de ellos en el camino. ", true, 16, "MRK"),
				ReferenceTextTests.CreateNarratorBlockForVerse(9, "Ellas dieron las instrucciones a los hombres.", true, 16, "MRK")
					.AddVerse(10, "Y después, Jesús envió por medio de ellos el mensaje de salvación. ")
			};
			var vernBook = new BookScript("MRK", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 3, null, i => true, null);
			Assert.That(vernacularBlocks.Last().GetText(true), Is.EqualTo(matchup.CorrelatedBlocks.Single().GetText(true)));
			Assert.That(vernacularBlocks.Intersect(matchup.CorrelatedBlocks).Count(), Is.EqualTo(0));
			Assert.That(matchup.CorrelatedBlocks.Single(), Is.EqualTo(matchup.CorrelatedAnchorBlock));
		}

		[TestCase(1, "11", "11")]
		[TestCase(2, "11", "11")]
		[TestCase(1, "11a", "11b")]
		[TestCase(2, "11a", "11b")]
		[TestCase(2, "11a", "11")]
		[TestCase(1, "11", "11b")]
		public void Constructor_VerseBridgeWithLastVerseSplit_CorrelatedBlocksContainsBothPartsOfVerse(int iBlock, string leadingVerseBridgeEnd, string trailingVerseBridgeStart)
		{
			var vernacularBlocks = new List<Block>
			{
				ReferenceTextTests.CreateNarratorBlockForVerse(8, "This is a leading verse that should not be included."),
				ReferenceTextTests.CreateNarratorBlockForVerse(9, "Habiendo resucitado Jesús, apareció a María Magdalena. ")
					.AddVerse($"10-{leadingVerseBridgeEnd}", "Ella lo hizo saber a los demas, pero no le creian. ")
			};
			vernacularBlocks.Add(new Block("p", 1, 11, 12) { ChapterNumber = 1, CharacterId = vernacularBlocks.Last().CharacterId,
				BlockElements = new List<BlockElement> {new Verse($"{trailingVerseBridgeStart}-12"), new ScriptText("Ellas dieron las instrucciones a los hombres.") }}
				.AddVerse(13, "Y después, Jesús envió por medio de ellos el mensaje de salvación."));
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, iBlock, null, i => true, null);
			Assert.That(matchup.OriginalBlockCount, Is.EqualTo(2));
			Assert.That(vernacularBlocks[1].GetText(true), Is.EqualTo(matchup.CorrelatedBlocks[0].GetText(true)));
			Assert.That(vernacularBlocks[2].GetText(true), Is.EqualTo(matchup.CorrelatedBlocks[1].GetText(true)));
			Assert.That(matchup.CorrelatedBlocks[iBlock - 1], Is.EqualTo(matchup.CorrelatedAnchorBlock));
		}

		[Test]
		public void IncludesBlock_VernVersesSplitByReferenceText_ReturnValuesReflectOriginalAndCorrelatedBlocks()
		{
			var vernacularBlocks = new List<Block>
			{
				ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus abrio su boca y enseno.", true),
				ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "Este es versiculo dos, ", true)
			};
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

			Assert.That(matchup.CorrelatedBlocks.Count, Is.EqualTo(4));

			Assert.That(matchup.IncludesBlock(vernacularBlocks[0]), Is.False);
			Assert.That(matchup.IncludesBlock(vernacularBlocks[1]), Is.True);
			Assert.That(matchup.IncludesBlock(vernacularBlocks[2]), Is.True);
			Assert.That(matchup.IncludesBlock(vernacularBlocks[3]), Is.False);

			Assert.That(matchup.CorrelatedBlocks,
				ForEvery<Block>(b => matchup.IncludesBlock(b), Is.True, "an entry included in the matchup"),
				"One or more correlated blocks are not properly included in the matchup.");

			Assert.That(matchup.CorrelatedBlocks.First(), Is.EqualTo(matchup.CorrelatedAnchorBlock));
		}

		[TestCase(2)]
		[TestCase(3)]
		public void CorrelatedAnchorBlock_PreviousVernVerseSplitByReferenceText_GetsCorrectBlock(int iBlock)
		{
			var vernacularBlocks = new List<Block>
			{
				ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus abrio su boca y enseno.", true),
				ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "Este es versiculo dos, ", true)
			};
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

			Assert.That(matchup.CorrelatedBlocks.Count, Is.EqualTo(4));

			//Assert.That(vernacularBlocks[iBlock], Is.EqualTo(matchup.OriginalAnchorBlock));
			Assert.That(matchup.CorrelatedAnchorBlock.GetText(true), Is.EqualTo(vernacularBlocks[iBlock].GetText(true)));
			Assert.That(matchup.CorrelatedBlocks[iBlock], Is.EqualTo(matchup.CorrelatedAnchorBlock));
		}

		[Test]
		public void CorrelatedAnchorBlock_RequestedVernVerseSplitByReferenceText_GetsCorrectBlock()
		{
			var vernacularBlocks = new List<Block>
			{
				ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus abrio su boca y enseno.", true),
				ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "Este es versiculo dos, ", true)
			};
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

			Assert.That(matchup.CorrelatedBlocks.Count, Is.EqualTo(4));

			//Assert.That(vernacularBlocks[2], Is.EqualTo(matchup.OriginalAnchorBlock));
			Assert.That(vernacularBlocks[2].GetText(true), Does.StartWith(matchup.CorrelatedAnchorBlock.GetText(true)));
			Assert.That(matchup.CorrelatedBlocks[1], Is.EqualTo(matchup.CorrelatedAnchorBlock));
		}

		[Test]
		public void HasOutstandingChangesToApply_DirectChangeToOriginalCharacter_ReturnsTrue()
		{
			var vernacularBlocks = new List<Block>
			{
				ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus abrio su boca y enseno.", true),
				ReferenceTextTests.CreateBlockForVerse("someone", 2, "Este es versiculo dos, ", true)
			};
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo alguien. ");
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "disciples", "Ya sabiamos, ");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "contesto Mateo.");
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 1, null, i => true, ReferenceText.GetStandardReferenceText(ReferenceTextType.English));
			matchup.MatchAllBlocks();

			vernacularBlocks[3].CharacterId = "Markose";
			Assert.That(matchup.HasOutstandingChangesToApply, Is.True);
		}

		[Test]
		public void HasOutstandingChangesToApply_DirectChangeToCorrelatedCharacter_ReturnsTrue()
		{
			var vernacularBlocks = new List<Block>
			{
				ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus abrio su boca y enseno.", true),
				ReferenceTextTests.CreateBlockForVerse("someone", 2, "Este es versiculo dos, ", true)
			};
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo alguien. ");
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "disciples", "Ya sabiamos, ");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "contesto Mateo.");
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 1, null, i => true, ReferenceText.GetStandardReferenceText(ReferenceTextType.English));
			matchup.MatchAllBlocks();

			matchup.CorrelatedBlocks[2].CharacterId = "Markose";
			Assert.That(matchup.HasOutstandingChangesToApply, Is.True);
		}

		[Test]
		public void HasOutstandingChangesToApply_DirectChangeToOriginalDelivery_ReturnsTrue()
		{
			var vernacularBlocks = new List<Block>
			{
				ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus abrio su boca y enseno.", true),
				ReferenceTextTests.CreateBlockForVerse("someone", 2, "Este es versiculo dos, ", true)
			};
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo alguien. ");
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "disciples", "Ya sabiamos, ");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "contesto Mateo.");
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 1, null, i => true, ReferenceText.GetStandardReferenceText(ReferenceTextType.English));
			matchup.MatchAllBlocks();

			vernacularBlocks[3].Delivery = "perplexed";
			Assert.That(matchup.HasOutstandingChangesToApply, Is.True);
		}

		[Test]
		public void HasOutstandingChangesToApply_DirectChangeToCorrelatedDelivery_ReturnsTrue()
		{
			var vernacularBlocks = new List<Block>
			{
				ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus abrio su boca y enseno.", true),
				ReferenceTextTests.CreateBlockForVerse("someone", 2, "Este es versiculo dos, ", true)
			};
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo alguien. ");
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "disciples", "Ya sabiamos, ");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "contesto Mateo.");
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 1, null, i => true, ReferenceText.GetStandardReferenceText(ReferenceTextType.English));
			matchup.MatchAllBlocks();

			matchup.CorrelatedBlocks[2].Delivery = "perturbed";
			Assert.That(matchup.HasOutstandingChangesToApply, Is.True);
		}

		[Test]
		public void MatchAllBlocks_AllMatchedDifferentCharacters_NoChanges()
		{
			var vernacularBlocks = new List<Block>
			{
				ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus abrio su boca y enseno.", true),
				ReferenceTextTests.CreateBlockForVerse("someone", 2, "Este es versiculo dos, ", true)
			};
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

			matchup.MatchAllBlocks();
			Assert.That(matchup.CorrelatedBlocks,
				ForEvery<Block>(b => b.MatchesReferenceText, Is.True));
			Assert.That(matchup.CorrelatedBlocks[0].CharacterId, Is.EqualTo("someone"));
			Assert.That(matchup.CorrelatedBlocks[0].Delivery, Is.Null);
			Assert.That(matchup.CorrelatedBlocks[0].GetPrimaryReferenceText(), Is.EqualTo("{2}\u00A0“This is verse two,” "));
			Assert.That(matchup.CorrelatedBlocks[1].CharacterId, Is.EqualTo(narrator));
			Assert.That(matchup.CorrelatedBlocks[1].GetPrimaryReferenceText(), Is.EqualTo("said Jesus. "));
			Assert.That(matchup.CorrelatedBlocks[2].CharacterId, Is.EqualTo("disciples"));
			Assert.That(matchup.CorrelatedBlocks[2].Delivery, Is.EqualTo("confident"));
			Assert.That(matchup.CorrelatedBlocks[2].GetPrimaryReferenceText(), Is.EqualTo("“We knew that,” "));
			Assert.That(matchup.CorrelatedBlocks[3].CharacterId, Is.EqualTo(narrator));
			Assert.That(matchup.CorrelatedBlocks[3].GetPrimaryReferenceText(), Is.EqualTo("replied Matthew."));
			Assert.That(matchup.HasOutstandingChangesToApply, Is.False);
		}

		[Test]
		public void MatchAllBlocks_AllMatchedButWithUnassignedCharacters_CharactersAndDeliveriesCopiedToCorrelatedBlocks()
		{
			var vernacularBlocks = new List<Block>
			{
				ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus abrio su boca y enseno.", true),
				ReferenceTextTests.CreateBlockForVerse(kUnexpectedCharacter, 2, "Este es versiculo dos, ", true)
			};
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Jesus. ");
			var narrator = vernacularBlocks[2].CharacterId;
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, kUnexpectedCharacter, "Ya sabiamos, ");
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

			matchup.MatchAllBlocks();
			Assert.That(matchup.CorrelatedBlocks,
				ForEvery<Block>(b => b.MatchesReferenceText, Is.True));
			Assert.That(matchup.CorrelatedBlocks[0].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(matchup.CorrelatedBlocks[0].UserConfirmed, Is.True);
			Assert.That(matchup.CorrelatedBlocks[0].GetPrimaryReferenceText(), Is.EqualTo("{2}\u00A0“This is verse two,” "));
			Assert.That(matchup.CorrelatedBlocks[1].CharacterId, Is.EqualTo(narrator));
			Assert.That(matchup.CorrelatedBlocks[1].UserConfirmed, Is.False);
			Assert.That(matchup.CorrelatedBlocks[1].GetPrimaryReferenceText(), Is.EqualTo("said Jesus. "));
			Assert.That(matchup.CorrelatedBlocks[2].CharacterId, Is.EqualTo("Batholomew/Matthew"));
			Assert.That(matchup.CorrelatedBlocks[2].CharacterIdInScript, Is.EqualTo("Matthew"));
			Assert.That(matchup.CorrelatedBlocks[2].UserConfirmed, Is.True);
			Assert.That(matchup.CorrelatedBlocks[2].Delivery, Is.EqualTo("smug"));
			Assert.That(matchup.CorrelatedBlocks[2].GetPrimaryReferenceText(), Is.EqualTo("“We knew that,” "));
			Assert.That(matchup.CorrelatedBlocks[3].CharacterId, Is.EqualTo(narrator));
			Assert.That(matchup.CorrelatedBlocks[3].UserConfirmed, Is.False);
			Assert.That(matchup.CorrelatedBlocks[3].GetPrimaryReferenceText(), Is.EqualTo("replied Matthew."));
			Assert.That(matchup.HasOutstandingChangesToApply, Is.True);
		}

		[Test]
		public void MatchAllBlocks_NotAllMatched_MultipleRefBlocksCombinedAndEmptyOnesCreatedAsNeeded()
		{
			var vernacularBlocks = new List<Block>
			{
				ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus abrio su boca y enseno.", true),
				ReferenceTextTests.CreateBlockForVerse(kUnexpectedCharacter, 2, "Este es versiculo dos, ", true)
			};
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Jesus. ");
			var narrator = vernacularBlocks[2].CharacterId;
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, kUnexpectedCharacter, "Ya sabiamos, ");
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
			matchup.MatchAllBlocks();
			Assert.That(matchup.CorrelatedBlocks,
				ForEvery<Block>(b => b.MatchesReferenceText, Is.True));
			Assert.That(matchup.CorrelatedBlocks[0].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(matchup.CorrelatedBlocks[0].GetPrimaryReferenceText(),
				Is.EqualTo("{2}\u00A0“This is verse two,” "));
			Assert.That(matchup.CorrelatedBlocks[1].CharacterId, Is.EqualTo(narrator),
				"If any of the ref blocks is narrator, default vern block to narrator.");
			Assert.That(matchup.CorrelatedBlocks[1].GetPrimaryReferenceText(),
				Is.EqualTo("said Jesus. To which Matthew replied, “We knew that.”"));
			Assert.That(matchup.CorrelatedBlocks[2].CharacterId, Is.EqualTo(kUnexpectedCharacter));
			Assert.That(matchup.CorrelatedBlocks[2].GetPrimaryReferenceText(), Is.Empty);
			Assert.That(matchup.CorrelatedBlocks[3].CharacterId, Is.EqualTo(narrator));
			Assert.That(matchup.CorrelatedBlocks[3].GetPrimaryReferenceText(),
				Is.EqualTo("He said this {3}\u00A0because the day before Jesus had said, “Tomorrow will be verse two.”"));
			var joinedRefBlock = matchup.CorrelatedBlocks[3].ReferenceBlocks.Single();
			// We may not technically really care too much about the next four lines (at least
			// right now), but this is how we expect the reference block to be built.
			Assert.That(joinedRefBlock.CharacterId, Is.EqualTo(narrator));
			Assert.That(((ScriptText)joinedRefBlock.BlockElements[0]).Content,
				Is.EqualTo("He said this "));
			Assert.That(((Verse)joinedRefBlock.BlockElements[1]).Number, Is.EqualTo("3"));
			Assert.That(((ScriptText)joinedRefBlock.BlockElements[2]).Content,
				Is.EqualTo("because the day before Jesus had said, “Tomorrow will be verse two.”"));
			Assert.That(matchup.HasOutstandingChangesToApply, Is.True);
		}

		[Test]
		public void MatchAllBlocks_PrimaryReferenceTextIsNotEnglish_MultipleRefBlocksCombinedForPrimaryAndEnglish()
		{
			var vernacularBlocks = new List<Block>
			{
				ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus abrio su boca y enseno.", true),
				ReferenceTextTests.CreateBlockForVerse(kUnexpectedCharacter, 2, "Este es versiculo dos, ", true)
			};
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Jesus. ");
			var narrator = vernacularBlocks[2].CharacterId;
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 1, null, i => true, TestReferenceText.CreateCustomReferenceText(TestReferenceTextResource.FrenchMAT));
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
			Assert.That(matchup.CorrelatedBlocks,
				ForEvery<Block>(b => b.MatchesReferenceText, Is.True));
			Assert.That(matchup.CorrelatedBlocks[0].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(matchup.CorrelatedBlocks[0].GetPrimaryReferenceText(),
				Is.EqualTo("{2}\u00A0“This is verse two,” "));
			Assert.That(matchup.CorrelatedBlocks[1].CharacterId, Is.EqualTo(narrator),
				"If any of the ref blocks is narrator, default vern block to narrator.");
			Assert.That(matchup.CorrelatedBlocks[1].GetPrimaryReferenceText(),
				Is.EqualTo("Jésus a dit. Pour que Matthieu a répondu, «Nous savions que.»"));
			Assert.That(matchup.CorrelatedBlocks[1].ReferenceBlocks.Single().GetPrimaryReferenceText(),
				Is.EqualTo("said Jesus. To which Matthew replied, “We knew that.”"));
		}

		#region PG-1408
		/// <summary>
		/// This covers the special case where a vernacular block is misinterpreted as a quote
		/// block because it starts with a dialogue dash that was actually intended to be a closer
		/// for the previous speech. It could be argued that this case should be handled by the
		/// quote parser since it might be able to correctly detect the case where the preceding
		/// block already has a character speaking and then a dash occurs at the start of a new
		/// paragraph in a verse where only one character is known to speak. However, because
		/// the final determination depends on the matchup to the reference text, doing it in
		/// MatchAllBlocks is more likely to produce a reliable result. Arguably, this logic
		/// should only apply when the user has indicated that the dialogue dash serves as both
		/// an opener and a closer.
		/// </summary>
		[TestCase("—", ExpectedResult = true)] // the real dialogue character (in the actual text)
		[TestCase("-", ExpectedResult = false)] // any other character
		public bool MatchAllBlocks_MismatchedDialogueDashAtStartOfParagraphCorrespondsToSingleNarratorRefBlock_VernBlockCharacterSetToNarrator(string dialogueChar)
		{
			var matchup = ReferenceTextTests.GetBlockMatchupForJohn12V35And36ForPg1408();
			matchup.MatchAllBlocks(dialogueChar);
			var lastBlock = matchup.CorrelatedBlocks.Last();
			// In the case where the block starts with the dialogue quote, we expect the last block's character ID to be changed to narrator
			return lastBlock.ReferenceBlocks.Single().CharacterId == lastBlock.CharacterId;
		}
		#endregion

		[TestCase(0)]
		[TestCase(1)]
		[TestCase(2)]
		public void Apply_SingleCorrelatedBlockHasReferenceText_OriginalBlockSetAsMatchWithReferenceBlock(int iBlock)
		{
			var vernacularBlocks = new List<Block>
			{
				ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces dijo Jesus: ", true),
				ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "Este es versiculo dos, ", true),
				ReferenceTextTests.CreateNarratorBlockForVerse(3, "Asi dijo. Despues se fue. ", true)
			};
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, iBlock, null, i => true, null);
			var correspondingVernBlock = vernacularBlocks[iBlock];
			Assert.That(correspondingVernBlock.GetText(true), Is.EqualTo(matchup.CorrelatedBlocks.Single().GetText(true)));
			var verseNum = correspondingVernBlock.InitialStartVerseNumber;
			var refBlock = ReferenceTextTests.CreateBlockForVerse("Jesus", verseNum, $"This is verse {verseNum}, ", true);
			matchup.CorrelatedBlocks.Single().SetMatchedReferenceBlock(refBlock);
			Assert.That(matchup.CountOfBlocksAddedBySplitting, Is.EqualTo(0));

			matchup.Apply();
			Assert.That(vernacularBlocks.Except(vernacularBlocks.Skip(iBlock).Take(1))
				.Where(b => b.MatchesReferenceText), Is.Empty);
			VerifyMatchedToCloneOfReferenceBlock(refBlock, vernacularBlocks[iBlock]);
			Assert.That(matchup.HasOutstandingChangesToApply, Is.False);
		}

		[Test]
		public void Apply_CorrelatedBlockWithTwoLevelsOfReferenceText_OriginalBlockSetAsMatchWithReferenceBlocksClonedAtBothLevels()
		{
			var vernacularBlocks = new List<Block>
			{
				ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces dijo Jesus: ", true),
				ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "Este es versiculo dos, ", true),
				ReferenceTextTests.CreateNarratorBlockForVerse(3, "Asi dijo. Despues se fue. ", true)
			};
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 1, null, i => true, null);
			var correspondingVernBlock = vernacularBlocks[1];
			Assert.That(correspondingVernBlock.GetText(true), Is.EqualTo(matchup.CorrelatedBlocks.Single().GetText(true)));
			var verseNum = correspondingVernBlock.InitialStartVerseNumber;
			var germanofrancolatinRefBlock = ReferenceTextTests.CreateBlockForVerse("Jesus", verseNum, $"Sie est verso {verseNum}, ", true);
			var englishRefBlock = ReferenceTextTests.CreateBlockForVerse("Jesus", verseNum, $"This is verse {verseNum}, ", true);
			germanofrancolatinRefBlock.SetMatchedReferenceBlock(englishRefBlock);
			Assert.That(englishRefBlock, Is.EqualTo(germanofrancolatinRefBlock.ReferenceBlocks.Single()));

			matchup.CorrelatedBlocks.Single().SetMatchedReferenceBlock(germanofrancolatinRefBlock);
			Assert.That(matchup.CountOfBlocksAddedBySplitting, Is.EqualTo(0));

			matchup.Apply();
			VerifyMatchedToCloneOfReferenceBlock(germanofrancolatinRefBlock, vernacularBlocks[1]);
			VerifyMatchedToCloneOfReferenceBlock(englishRefBlock, vernacularBlocks[1].ReferenceBlocks.Single());
			Assert.That(matchup.HasOutstandingChangesToApply, Is.False);
		}

		[TestCase(1)]
		[TestCase(2)]
		[TestCase(3)]
		public void Apply_MultipleCorrelatedBlocksHaveReferenceText_CorrectOriginalBlocksSetAsMatchWithReferenceBlocks(int iBlock)
		{
			var vernacularBlocks = new List<Block>
			{
				ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus abrio su boca y enseno.", true),
				ReferenceTextTests.CreateBlockForVerse(kUnexpectedCharacter, 2, "Este es versiculo dos, ", true)
			};
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Jesus. To which Peter and John replied, ");
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, kUnexpectedCharacter, "Que pinta!");
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
			Assert.That(matchup.CorrelatedBlocks.Select(b => b.GetText(true)), Is.EqualTo(
				vernacularBlocks.Skip(1).Take(3).Select(b => b.GetText(true))));
			matchup.MatchAllBlocks();
			Assert.That(matchup.CountOfBlocksAddedBySplitting, Is.EqualTo(0));

			matchup.Apply();
			Assert.That(vernacularBlocks[0].MatchesReferenceText, Is.False);
			VerifyMatchedToCloneOfReferenceBlock(refBlock1, vernacularBlocks[1]);
			Assert.That(refBlock1.CharacterId, Is.EqualTo(vernacularBlocks[1].CharacterId));
			VerifyMatchedToCloneOfReferenceBlock(refBlock2, vernacularBlocks[2]);
			Assert.That(refBlock2.CharacterId, Is.EqualTo(vernacularBlocks[2].CharacterId));
			VerifyMatchedToCloneOfReferenceBlock(refBlock3, vernacularBlocks[3]);
			Assert.That(refBlock3.CharacterId, Is.EqualTo(vernacularBlocks[3].CharacterId));
			Assert.That(refBlock3.CharacterIdInScript, Is.EqualTo(vernacularBlocks[3].CharacterIdInScript));
			Assert.That(vernacularBlocks[4].MatchesReferenceText, Is.False);
			Assert.That(matchup.HasOutstandingChangesToApply, Is.False);
		}

		[TestCase(1)]
		[TestCase(2)]
		public void Apply_NotAllCorrelatedBlocksHaveReferenceText_ThrowsInvalidOperationException(int iBlock)
		{
			var vernacularBlocks = new List<Block>
			{
				ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus abrio su boca y enseno.", true),
				ReferenceTextTests.CreateBlockForVerse(kUnexpectedCharacter, 2, "Este es versiculo dos, ", true)
			};
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Jesus.");
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, iBlock, null, i => true, null);
			matchup.SetReferenceText(0, "[2] This is verse two, ");
			Assert.That(matchup.HasOutstandingChangesToApply, Is.True);

			Assert.That(() => matchup.Apply(), Throws.InvalidOperationException.With.Message.EqualTo(
				"Cannot apply reference blocks unless all Scripture blocks have corresponding reference blocks."));

			Assert.That(matchup.HasOutstandingChangesToApply, Is.True);
		}

		[Test]
		public void Apply_VernVersesInMiddleOfQuoteChainAssignedGoFromUnknownToNarrator_QuoteChainBrokenCorrectly()
		{
			var vernacularBlocks = new List<Block>
			{
				new Block("c", 1)
				{
					CharacterId = GetStandardCharacterId("ROM", BookOrChapter),
					BlockElements = new List<BlockElement>(new[] { new ScriptText("1") })
				},
				ReferenceTextTests.CreateNarratorBlockForVerse(20, "This ", true, 1, "ROM")
			};
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, kUnexpectedCharacter, "«opener« has no closer.");
			vernacularBlocks.Last().MultiBlockQuote = MultiBlockQuote.Start;
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse(kUnexpectedCharacter, 21, "This is verse 21. ", true)
				.AddVerse(22, "This is «verse« 22. ").AddVerse(23, "This is verse 23."));
			vernacularBlocks.Last().MultiBlockQuote = MultiBlockQuote.Continuation;
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse(kUnexpectedCharacter, 24,
				"The quote remains open in this paragraph. ", true).AddVerse(25, "This is verse 25."));
			vernacularBlocks.Last().MultiBlockQuote = MultiBlockQuote.Continuation;
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse(kUnexpectedCharacter, 26,
				"The quote remains open in this paragraph as well. ", true).AddVerse(27, "This is verse 27."));
			vernacularBlocks.Last().MultiBlockQuote = MultiBlockQuote.Continuation;
			var vernBook = new BookScript("ROM", vernacularBlocks, ScrVers.English);

			var refText = ReferenceText.GetStandardReferenceText(ReferenceTextType.English);
			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, 3);
			
			Assert.That(matchup.HasOutstandingChangesToApply, Is.True);

			matchup.MatchAllBlocks();
			matchup.Apply();
			var resultingBlocks = vernBook.GetScriptBlocks().ToList();
			Assert.That(vernacularBlocks.Count + 2, Is.EqualTo(resultingBlocks.Count));
			Assert.That(resultingBlocks[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None)); // v. 20
			Assert.That(resultingBlocks[3].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None)); // v. 21
			Assert.That(resultingBlocks[4].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None)); // v. 22
			Assert.That(resultingBlocks[5].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None)); // v. 23
			Assert.That(resultingBlocks[6].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start)); // vv. 24-25
			Assert.That(resultingBlocks[7].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation)); // vv. 26-27
		}

		// PG-1325
		[Test]
		public void Apply_FirstBlockOfQuoteChainGoesFromAmbiguousToSpeakerButSubsequentBlocksAreNarrator_QuoteChainCleared()
		{
			var vernacularBlocks = new List<Block>
			{
				new Block("c", 18)
				{
					CharacterId = GetStandardCharacterId("REV", BookOrChapter),
					BlockElements = new List<BlockElement>(new[] { new ScriptText("1") })
				},
				ReferenceTextTests.CreateNarratorBlockForVerse(15, "I komersiante ira nga ibinunga ta masikan;", true, 18, "REV", "q2"),
				ReferenceTextTests.CreateBlockForVerse(kAmbiguousCharacter, 16, "<<Anakkoy! Anakkoy ka gapa! ", true, 18, "q1")
			};
			vernacularBlocks.Last().MultiBlockQuote = MultiBlockQuote.Start;
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, kAmbiguousCharacter, "Madumu-rumug nga siudad na Babilonia!", "q2");
			vernacularBlocks.Last().MultiBlockQuote = MultiBlockQuote.Continuation;
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, kAmbiguousCharacter, "Nabbisti ta lino, granate, anna bata nga mangina!", "q1");
			vernacularBlocks.Last().MultiBlockQuote = MultiBlockQuote.Continuation;
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse(kAmbiguousCharacter, 17, "Ngem ngamin danaw laman i nikapawan na!>>", true, 18, "q1"));
			vernacularBlocks.Last().MultiBlockQuote = MultiBlockQuote.Continuation;
			var vernBook = new BookScript("REV", vernacularBlocks, ScrVers.English);

			var refText = ReferenceText.GetStandardReferenceText(ReferenceTextType.English);
			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, 2);
			matchup.MatchAllBlocks();

			var narrator = GetStandardCharacterId("REV", Narrator);
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
			Assert.That(resultingBlocks.All(b => b.MultiBlockQuote == MultiBlockQuote.None));
		}

		[Test]
		public void GetInvalidReferenceBlocksAtAnyLevel_PrimaryReferenceBlockEndsWithEmptyVerse_ReturnsLevel1Block()
		{
			var vernacularBlocks = new List<Block> { ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus abrio su boca y dijo: ", true) };
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "Este es versiculo dos.");
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, null);
			matchup.SetReferenceText(0, "{1} Then Jesus opened his mouth and said: {2} ");
			matchup.SetReferenceText(1, "why isn't the verse number at the start of this block?");

			var blockWithInvalidReferences = matchup.GetInvalidReferenceBlocksAtAnyLevel().Single();
			Assert.That(blockWithInvalidReferences.Item1, Is.EqualTo(0));
			Assert.That(blockWithInvalidReferences.Item2, Is.EqualTo(1));
		}

		[Test]
		public void GetInvalidReferenceBlocksAtAnyLevel_SecondaryReferenceBlockEndsWithEmptyVerse_ReturnsLevel2Block()
		{
			var vernacularBlocks = new List<Block> { ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus abrio su boca y dijo: ", true) };
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "Este es").AddVerse(2, "versiculo dos.");
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var primaryRefBlock1 = new Block("p", 1, 1).AddVerse(1, "Ceci est un bloc.");
			vernacularBlocks.First().SetMatchedReferenceBlock(primaryRefBlock1);
			primaryRefBlock1.SetMatchedReferenceBlock(new Block("p", 1, 1).AddVerse(1, "This is a block."));
			var primaryRefBlock2 = new Block("p", 1, 1).AddVerse(2, "Ceci est un verset.");
			vernacularBlocks.Last().SetMatchedReferenceBlock(primaryRefBlock2);
			primaryRefBlock2.SetMatchedReferenceBlock(new Block("p", 1, 1).AddVerse(2, "This is a verse."));
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, null);
			Assert.That(matchup.CorrelatedBlocks[1].ReferenceBlocks.Single().ReferenceBlocks.Single().GetText(true), Is.EqualTo("{2}\u00A0This is a verse."));
			matchup.SetReferenceText(1, "This is {2}", 1);
			Assert.That(matchup.CorrelatedBlocks[1].ReferenceBlocks.Single().ReferenceBlocks.Single().GetText(true), Is.EqualTo("This is {2}\u00A0"));
			Assert.That(matchup.HasOutstandingChangesToApply, Is.True);

			var blockWithInvalidReferences = matchup.GetInvalidReferenceBlocksAtAnyLevel().Single();
			Assert.That(blockWithInvalidReferences.Item1, Is.EqualTo(1));
			Assert.That(blockWithInvalidReferences.Item2, Is.EqualTo(2));
		}

		[Test]
		public void GetInvalidReferenceBlocksAtAnyLevel_PrimaryAndSecondaryReferenceBlockConsistOfEmptyVerse_ReturnsBlocksFromBothLevels()
		{
			var vernacularBlocks = new List<Block> { ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus abrio su boca y dijo: ", true) };
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
			Assert.That(matchup.HasOutstandingChangesToApply, Is.True);

			var blocksWithInvalidReferences = matchup.GetInvalidReferenceBlocksAtAnyLevel().ToList();
			Assert.That(blocksWithInvalidReferences.Count, Is.EqualTo(2));
			Assert.That(blocksWithInvalidReferences.First().Item1, Is.EqualTo(1));
			Assert.That(blocksWithInvalidReferences.First().Item2, Is.EqualTo(1));
			Assert.That(blocksWithInvalidReferences.Last().Item1, Is.EqualTo(0));
			Assert.That(blocksWithInvalidReferences.Last().Item2, Is.EqualTo(2));
		}

		[Test]
		public void Apply_MatchupWithNoSplitsIsFollowedByContinuationBlocks_FollowingContinuationBlocksHaveCharacterIdUpdated()
		{
			var vernacularBlocks = new List<Block>
			{
				ReferenceTextTests.CreateNarratorBlockForVerse(1, "Then an unknown person said:", true),
				ReferenceTextTests.CreateBlockForVerse(kAmbiguousCharacter, 2, "This is the first thing I want to say.", true)
			};
			vernacularBlocks.Last().MultiBlockQuote = MultiBlockQuote.Start;
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse(kAmbiguousCharacter, 3, "Also, it's worth pointing out something else.", true));
			vernacularBlocks.Last().MultiBlockQuote = MultiBlockQuote.Continuation;
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse(kAmbiguousCharacter, 4, "But now my time has run out, ", true));
			vernacularBlocks.Last().MultiBlockQuote = MultiBlockQuote.Continuation;
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "he concluded.");

			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 1, null, index => true, null);

			Assert.That(matchup.OriginalBlockCount, Is.EqualTo(1));
			Assert.That(matchup.OriginalBlockCount, Is.EqualTo(matchup.CorrelatedBlocks.Count));
			Assert.That(matchup.CountOfBlocksAddedBySplitting, Is.EqualTo(0));

			var refBlock1 = ReferenceTextTests.CreateBlockForVerse("Marco/his friend", 2, "Za 1st Ting 2 say. ", true);
			matchup.CorrelatedBlocks[0].SetMatchedReferenceBlock(refBlock1);
			matchup.CorrelatedBlocks[0].CharacterId = refBlock1.CharacterId;
			matchup.CorrelatedBlocks[0].CharacterIdInScript = "his friend";

			matchup.Apply();
			var scriptBlocks = vernBook.GetScriptBlocks(); /* This used to have join = true */
			int i = 1;
			VerifyMatchedToCloneOfReferenceBlock(refBlock1, scriptBlocks[i]);
			Assert.That(refBlock1.CharacterId, Is.EqualTo(scriptBlocks[i].CharacterId));
			Assert.That(scriptBlocks[i].CharacterIdOverrideForScript, Is.EqualTo("his friend"));
			Assert.That(scriptBlocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));
			Assert.That(scriptBlocks[++i].MatchesReferenceText, Is.False);
			Assert.That(scriptBlocks[1].CharacterId, Is.EqualTo(scriptBlocks[i].CharacterId));
			Assert.That(scriptBlocks[1].CharacterIdOverrideForScript, Is.EqualTo(scriptBlocks[i].CharacterIdOverrideForScript));
			Assert.That(scriptBlocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
			Assert.That(scriptBlocks[++i].MatchesReferenceText, Is.False);
			Assert.That(scriptBlocks[1].CharacterId, Is.EqualTo(scriptBlocks[i].CharacterId));
			Assert.That(scriptBlocks[1].CharacterIdOverrideForScript, Is.EqualTo(scriptBlocks[i].CharacterIdOverrideForScript));
			Assert.That(scriptBlocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
			Assert.That(scriptBlocks[++i].MatchesReferenceText, Is.False);
			Assert.That(scriptBlocks[i].CharacterId, Is.EqualTo(
				GetStandardCharacterId("MAT", Narrator)));
			Assert.That(scriptBlocks[i].CharacterIdOverrideForScript, Is.Null);
			Assert.That(scriptBlocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(matchup.HasOutstandingChangesToApply, Is.False);
		}

		[TestCase(1)]
		[TestCase(2)]
		public void Apply_SplitCorrelatedBlocksHaveReferenceText_OriginalBlocksReplacedWithCorrelatedBlocks(int iBlock)
		{
			var vernacularBlocks = new List<Block>
			{
				ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus abrio su boca y enseno.", true),
				ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "Este es versiculo dos, ", true)
			};
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Jesus.");
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(3, "Despues se fue. ", true));
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, iBlock, p => p.SplitBlock(p.GetScriptBlocks().First(), "2", 8, false), i => true, null);
			Assert.That(matchup.CorrelatedBlocks.Count, Is.EqualTo(3));
			Assert.That(matchup.CountOfBlocksAddedBySplitting, Is.EqualTo(1));
			Assert.That(matchup.CorrelatedBlocks[0].GetText(true), Is.EqualTo("{2}\u00A0Este es "));
			Assert.That(matchup.CorrelatedBlocks[1].GetText(true), Is.EqualTo("versiculo dos, "));
			Assert.That(vernacularBlocks[2].GetText(true), Is.EqualTo(matchup.CorrelatedBlocks[2].GetText(true)));

			var refBlock0 = ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "This is ", true);
			matchup.CorrelatedBlocks[0].SetMatchedReferenceBlock(refBlock0);
			var refBlock1 = new Block("p", 1, 2) { CharacterId = refBlock0.CharacterId };
			refBlock1.BlockElements.Add(new ScriptText("verse two, "));
			matchup.CorrelatedBlocks[1].SetMatchedReferenceBlock(refBlock1);
			var refBlock2 = new Block("p", 1, 2) { CharacterId = vernacularBlocks[2].CharacterId };
			refBlock2.BlockElements.Add(new ScriptText("said Jesus."));
			matchup.CorrelatedBlocks[2].SetMatchedReferenceBlock(refBlock2);
			Assert.That(matchup.CountOfBlocksAddedBySplitting, Is.EqualTo(1));

			matchup.Apply();
			var scriptBlocks = vernBook.GetScriptBlocks();
			Assert.That(vernacularBlocks.Count + 1, Is.EqualTo(scriptBlocks.Count));
			Assert.That(scriptBlocks[0].MatchesReferenceText, Is.False);
			VerifyMatchedToCloneOfReferenceBlock(refBlock0, scriptBlocks[1]);
			VerifyMatchedToCloneOfReferenceBlock(refBlock1, scriptBlocks[2]);
			VerifyMatchedToCloneOfReferenceBlock(refBlock2, scriptBlocks[3]);
			Assert.That(scriptBlocks[4].MatchesReferenceText, Is.False);
			Assert.That(matchup.CountOfBlocksAddedBySplitting, Is.EqualTo(0));
			Assert.That(matchup.HasOutstandingChangesToApply, Is.False);
			Assert.That(matchup.OriginalBlocks.Select(b => b.GetText(true)).SequenceEqual(matchup.CorrelatedBlocks.Select(b => b.GetText(true))), Is.True);
			Assert.That(matchup.OriginalBlocks.Intersect(matchup.CorrelatedBlocks), Is.Empty);
		}

		[TestCase(1)]
		[TestCase(2)]
		public void Apply_SplitCorrelatedBlocksDoNotAllHaveReferenceText_ThrowsInvalidOperationException(int iBlock)
		{
			var vernacularBlocks = new List<Block>
			{
				ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus abrio su boca y enseno.", true),
				ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "Este es versiculo dos, ", true)
			};
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Jesus.");
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(3, "Despues se fue. ", true));
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, iBlock, p => p.SplitBlock(p.GetScriptBlocks().First(), "2", 8, false), i => true, null);
			Assert.That(matchup.CorrelatedBlocks.Count, Is.EqualTo(3));
			Assert.That(matchup.CountOfBlocksAddedBySplitting, Is.EqualTo(1));
			Assert.That(matchup.CorrelatedBlocks[0].GetText(true), Is.EqualTo("{2}\u00A0Este es "));
			Assert.That(matchup.CorrelatedBlocks[1].GetText(true), Is.EqualTo("versiculo dos, "));
			Assert.That(vernacularBlocks[2].GetText(true), Is.EqualTo(matchup.CorrelatedBlocks[2].GetText(true)));

			var refBlock0 = ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "This is verse two", true);
			matchup.CorrelatedBlocks[0].SetMatchedReferenceBlock(refBlock0);
			var refBlock2 = new Block("p", 1, 2) { CharacterId = vernacularBlocks[2].CharacterId };
			refBlock2.BlockElements.Add(new ScriptText("said Jesus."));
			matchup.CorrelatedBlocks[1].SetMatchedReferenceBlock(refBlock2);

			Assert.That(() => matchup.Apply(), Throws.InvalidOperationException.With.Message.EqualTo(
				"Cannot apply reference blocks unless all Scripture blocks have corresponding reference blocks."));

			Assert.That(matchup.CountOfBlocksAddedBySplitting, Is.EqualTo(1));
			Assert.That(matchup.HasOutstandingChangesToApply, Is.True);
		}

		[Test]
		public void Apply_ReferenceBlockHasCharacterWithASlash_CharacterIdOverrideForScriptSetToFirstCharacter()
		{
			var vernacularBlocks = new List<Block> { ReferenceTextTests.CreateNarratorBlockForVerse(9, "Jesús le preguntó: ", false, 5, "MRK") };
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, kAmbiguousCharacter,
				"¿Cómo te llamas?");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "Él contestó: ", "MRK");
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, kAmbiguousCharacter,
				"Me llamo Legión, porque somos muchos.");
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 1, null, i => true, null);

			var referenceBlocks = new List<Block> { ReferenceTextTests.CreateNarratorBlockForVerse(9, "He asked him, ", false, 8, "MRK") };
			ReferenceTextTests.AddBlockForVerseInProgress(referenceBlocks, "Jesus", "“What is your name?”");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(referenceBlocks, "He said to him, ", "MRK");
			ReferenceTextTests.AddBlockForVerseInProgress(referenceBlocks, "demons (Legion)/man delivered from Legion of demons",
				"“My name is Legion, for we are many.”");

			for (int i = 0; i < 4; i++)
				matchup.CorrelatedBlocks[i].SetMatchedReferenceBlock(referenceBlocks[i]);
			
			matchup.MatchAllBlocks();
			for (int i = 0; i < 4; i++)
				Assert.That(referenceBlocks[i].CharacterId, Is.EqualTo(matchup.CorrelatedBlocks[i].CharacterId));
			Assert.That(matchup.CorrelatedBlocks[3].CharacterIdOverrideForScript, Is.EqualTo("demons (Legion)"));

			matchup.Apply();
			for (int i = 0; i < 4; i++)
			{
				Assert.That(vernacularBlocks[i].MatchesReferenceText, Is.True);
				Assert.That(vernacularBlocks[i].CharacterId, Is.EqualTo(referenceBlocks[i].CharacterId));
			}
			Assert.That(vernacularBlocks[3].CharacterIdOverrideForScript, Is.EqualTo("demons (Legion)"));
			Assert.That(matchup.HasOutstandingChangesToApply, Is.False);
		}

		[TestCase("")]
		[TestCase(null)]
		public void SetReferenceText_NullOrEmpty_SpecifiedBlockMatchedWithReferenceBlockHavingSingleEmptyScriptTextElement(string refText)
		{
			var vernacularBlocks = new List<Block> { ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "Este es versiculo dos y tres, ", true, 1, "p", 3) };
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Jesus. ");
			var narrator = vernacularBlocks.Last().CharacterId;
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, null);
			matchup.CorrelatedBlocks[0].SetMatchedReferenceBlock(
				ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "“This is verse two,” ", true));
			Assert.That(matchup.CorrelatedBlocks[1].MatchesReferenceText, Is.False);

			matchup.SetReferenceText(1, refText);
			Assert.That(matchup.CorrelatedBlocks,
				ForEvery<Block>(b => b.MatchesReferenceText, Is.True));

			Assert.That(matchup.CorrelatedBlocks[1].GetPrimaryReferenceText(), Is.EqualTo(""));
			var newRefBlock = matchup.CorrelatedBlocks[1].ReferenceBlocks.Single();
			Assert.That(newRefBlock.CharacterId, Is.EqualTo(narrator), "Should get character/delivery info from vern block");
			Assert.That(newRefBlock.InitialStartVerseNumber, Is.EqualTo(2));
			Assert.That(newRefBlock.InitialEndVerseNumber, Is.EqualTo(0));
		}

		[Test]
		public void SetReferenceText_NoVerseNumbers_NoExistingRefBlock_SpecifiedBlockMatchedWithReferenceBlockHavingSingleScriptTextElement()
		{
			var vernacularBlocks = new List<Block> { ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "Este es versiculo dos y tres, ", true, 1, "p", 3) };
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Jesus. ");
			vernacularBlocks.Last().Delivery = "expressionless";
			var narrator = vernacularBlocks.Last().CharacterId;
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, null);
			matchup.CorrelatedBlocks[0].SetMatchedReferenceBlock(
				ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "“This is verse two,” ", true));
			Assert.That(matchup.CorrelatedBlocks[1].MatchesReferenceText, Is.False);

			matchup.SetReferenceText(1, "said Jesus.");
			Assert.That(matchup.CorrelatedBlocks,
				ForEvery<Block>(b => b.MatchesReferenceText, Is.True));
			// Ensure block 0 not changed
			Assert.That(matchup.CorrelatedBlocks[0].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(matchup.CorrelatedBlocks[0].GetPrimaryReferenceText(), Is.EqualTo("{2}\u00A0“This is verse two,” "));

			Assert.That(matchup.CorrelatedBlocks[1].GetPrimaryReferenceText(), Is.EqualTo("said Jesus."));
			var newRefBlock = matchup.CorrelatedBlocks[1].ReferenceBlocks.Single();
			Assert.That(newRefBlock.CharacterId, Is.EqualTo(narrator),
				"Should get character/delivery info from vern block");
			Assert.That(newRefBlock.Delivery, Is.EqualTo("expressionless"), "Should get character/delivery info from vern block");
			Assert.That(newRefBlock.InitialStartVerseNumber, Is.EqualTo(2));
			Assert.That(newRefBlock.InitialEndVerseNumber, Is.EqualTo(0));
		}

		[TestCase("\u00A0")]
		[TestCase(" ")]
		[TestCase("")]
		public void SetReferenceText_FirstBlock_VernBlockHasVerseBridgeThatCoversNumberInRefBlock_SpecifiedBlockMatchedWithReferenceBlockHavingStartVerseNumberImpliedByVerseNumberInRefBlock(string separator)
		{
			var vernacularBlocks = new List<Block> { ReferenceTextTests.CreateBlockForVerse(kUnexpectedCharacter, 1, "Entonces Jesus hablo, diciendo: ", true, 1, "p", 3) };
			vernacularBlocks.Last().Delivery = "expressionless";
			vernacularBlocks.Last().SetMatchedReferenceBlock(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Then Jesus spoke unto them, ", true));

			var block2 = ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "“Este es versiculo dos y tres.”", "p");
			block2.SetMatchedReferenceBlock(ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "“This is verse two.”", false, 1, "p", 3));

			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, null);
			Assert.That(matchup.CorrelatedBlocks,
				ForEvery<Block>(b => b.MatchesReferenceText, Is.True));

			var newRefBlock = matchup.SetReferenceText(0, "Then Jesus told them {2}" + separator + "that it was just verse two.");
			Assert.That(matchup.CorrelatedBlocks,
				ForEvery<Block>(b => b.MatchesReferenceText, Is.True));
			// Ensure block 1 not changed
			Assert.That(matchup.CorrelatedBlocks[1].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(matchup.CorrelatedBlocks[1].GetPrimaryReferenceText(), Is.EqualTo("{2-3}\u00A0“This is verse two.”"));

			Assert.That(matchup.CorrelatedBlocks[0].GetPrimaryReferenceText(), Is.EqualTo("Then Jesus told them {2}\u00A0that it was just verse two."));
			Assert.That(matchup.CorrelatedBlocks[0].ReferenceBlocks.Single(), Is.EqualTo(newRefBlock));
			Assert.That(newRefBlock.CharacterId, Is.EqualTo(kUnexpectedCharacter), "Should get character/delivery info from vern block");
			Assert.That(newRefBlock.Delivery, Is.EqualTo("expressionless"), "Should get character/delivery info from vern block");
			Assert.That(newRefBlock.InitialStartVerseNumber, Is.EqualTo(1));
			Assert.That(newRefBlock.InitialEndVerseNumber, Is.EqualTo(0));
			Assert.That(newRefBlock.LastVerseNum, Is.EqualTo(2));
		}

		[Test]
		public void SetReferenceText_FirstBlock_RefBlockContainsBridge_ReferenceBlockHasStartVerseNumberImpliedByVerseNumberInRefBlock()
		{
			// The logic represented here is entirely "fuzzy", but the idea is that since the corresponding vernacular block is a verse
			// bridge from 1-3, that the reference text is "probably" for that same range. But since it turns out that the reference
			// text has a 3-4 verse bridge in the middle of it, logically the preceding text can't be for verse 1-3, so they must be
			// for verses 1-2.
			
			var vernacularBlocks = new List<Block> { ReferenceTextTests.CreateBlockForVerse(kUnexpectedCharacter, 1, "Entonces Jesus hablo, diciendo: ", true, 1, "p", 3) };
			vernacularBlocks.Last().SetMatchedReferenceBlock(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Then Jesus spoke unto them, ", true));

			var block2 = ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "“Este es versiculo dos y tres.”", "p");
			block2.SetMatchedReferenceBlock(ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "“This is verse two.”", false, 1, "p", 3));

			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, null);
			Assert.That(matchup.CorrelatedBlocks,
				ForEvery<Block>(b => b.MatchesReferenceText, Is.True));

			var newRefBlock = matchup.SetReferenceText(0, "Then Jesus told them {3-4}that it was actually a verse bridge starting with three.");
			Assert.That(matchup.CorrelatedBlocks,
				ForEvery<Block>(b => b.MatchesReferenceText, Is.True));
			// Ensure block 1 not changed
			Assert.That(matchup.CorrelatedBlocks[1].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(matchup.CorrelatedBlocks[1].GetPrimaryReferenceText(), Is.EqualTo("{2-3}\u00A0“This is verse two.”"));

			Assert.That(matchup.CorrelatedBlocks[0].GetPrimaryReferenceText(), Is.EqualTo("Then Jesus told them {3-4}\u00A0that it was actually a verse bridge starting with three."));
			Assert.That(matchup.CorrelatedBlocks[0].ReferenceBlocks.Single(), Is.EqualTo(newRefBlock));
			Assert.That(newRefBlock.InitialStartVerseNumber, Is.EqualTo(1));
			Assert.That(newRefBlock.InitialEndVerseNumber, Is.EqualTo(2));
			Assert.That(newRefBlock.LastVerseNum, Is.EqualTo(4));
		}

		[Test]
		public void SetReferenceText_FirstBlock_RefBlockContainsAnnotationAndThenBridge_ReferenceBlockHasStartVerseNumberImpliedByVerseNumberInRefBlock()
		{
			var vernacularBlocks = new List<Block> { ReferenceTextTests.CreateBlockForVerse(kUnexpectedCharacter, 1, "Entonces Jesus hablo, diciendo: ", true, 1, "p", 3) };
			vernacularBlocks.Last().SetMatchedReferenceBlock(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Then Jesus spoke unto them, ", true));

			var block2 = ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "“Este es versiculo dos y tres.”", "p");
			block2.SetMatchedReferenceBlock(ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "“This is verse two.”", false, 1, "p", 3));

			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, null);
			Assert.That(matchup.CorrelatedBlocks,
				ForEvery<Block>(b => b.MatchesReferenceText, Is.True));

			var newRefBlock = matchup.SetReferenceText(0, "{F8 Music--Ends} {3-4}This is actually a verse bridge starting with three.");
			Assert.That(matchup.CorrelatedBlocks,
				ForEvery<Block>(b => b.MatchesReferenceText, Is.True));
			// Ensure block 1 not changed
			Assert.That(matchup.CorrelatedBlocks[1].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(matchup.CorrelatedBlocks[1].GetPrimaryReferenceText(), Is.EqualTo("{2-3}\u00A0“This is verse two.”"));

			Assert.That(matchup.CorrelatedBlocks[0].GetPrimaryReferenceText(), Is.EqualTo("{F8 Music--Ends} {3-4}\u00A0This is actually a verse bridge starting with three."));
			Assert.That(matchup.CorrelatedBlocks[0].ReferenceBlocks.Single(), Is.EqualTo(newRefBlock));
			Assert.That(newRefBlock.InitialStartVerseNumber, Is.EqualTo(3));
			Assert.That(newRefBlock.InitialEndVerseNumber, Is.EqualTo(4));
			Assert.That(newRefBlock.LastVerseNum, Is.EqualTo(4));
		}

		[Test]
		public void SetReferenceText_FirstBlock_NoVerseNumber_ExistingRefBlock_SpecifiedBlockMatchedWithReferenceBlockHavingVerseNumberFromVernBlock()
		{
			var vernacularBlocks = new List<Block> { ReferenceTextTests.CreateBlockForVerse(kUnexpectedCharacter, 1, "Entonces Jesus hablo, diciendo: ", true, 1, "p", 3) };
			vernacularBlocks.Last().Delivery = "expressionless";
			vernacularBlocks.Last().SetMatchedReferenceBlock(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Then Jesus spoke unto them, ", true));

			var block2 = ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "“Este es versiculo dos y tres.”", "p");
			block2.SetMatchedReferenceBlock(ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "“This is verse two.”", false, 1, "p", 3));

			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, null);
			Assert.That(matchup.CorrelatedBlocks,
				ForEvery<Block>(b => b.MatchesReferenceText, Is.True));

			var newRefBlock = matchup.SetReferenceText(0, "Then Jesus told them that it was just verse two.");
			Assert.That(matchup.CorrelatedBlocks,
				ForEvery<Block>(b => b.MatchesReferenceText, Is.True));
			// Ensure block 1 not changed
			Assert.That(matchup.CorrelatedBlocks[1].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(matchup.CorrelatedBlocks[1].GetPrimaryReferenceText(), Is.EqualTo("{2-3}\u00A0“This is verse two.”"));
			Assert.That(matchup.CorrelatedBlocks[1].ReferenceBlocks.Single().InitialStartVerseNumber, Is.EqualTo(2));

			Assert.That(matchup.CorrelatedBlocks[0].GetPrimaryReferenceText(), Is.EqualTo("Then Jesus told them that it was just verse two."));
			Assert.That(matchup.CorrelatedBlocks[0].ReferenceBlocks.Single(), Is.EqualTo(newRefBlock));
			Assert.That(newRefBlock.CharacterId, Is.EqualTo(kUnexpectedCharacter), "Should get character/delivery info from vern block");
			Assert.That(newRefBlock.Delivery, Is.EqualTo("expressionless"), "Should get character/delivery info from vern block");
			Assert.That(newRefBlock.InitialStartVerseNumber, Is.EqualTo(1));
			Assert.That(newRefBlock.InitialEndVerseNumber, Is.EqualTo(3));
			Assert.That(newRefBlock.LastVerseNum, Is.EqualTo(3));
		}

		[TestCase("\u00A0")]
		[TestCase(" ")]
		[TestCase("")]
		public void SetReferenceText_VerseNumberAddedToRefBlockOfFirstBlock_FollowingRefBlockDoesNotStartWithVerseNumber_InitialVerseRefOfFollowingReferenceBlockChanged(string separator)
		{
			var vernacularBlocks = new List<Block> { ReferenceTextTests.CreateBlockForVerse(kUnexpectedCharacter, 1, "Entonces Jesus hablo, diciendo: ", true, 1, "p", 3) };
			vernacularBlocks.Last().SetMatchedReferenceBlock(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Then Jesus spoke unto them, ", true));

			var block2 = ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "“Yo soy el pan de vida.”", "p");
			var refBlock2 = new Block("q", 1, 1);
			refBlock2.CharacterId = "Jesus";
			refBlock2.BlockElements.Add(new ScriptText("“Continuation of previous verse in ref text. "));
			refBlock2.AddVerse(3, "Three!”");
			block2.SetMatchedReferenceBlock(refBlock2);

			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, null);
			Assert.That(matchup.CorrelatedBlocks,
				ForEvery<Block>(b => b.MatchesReferenceText, Is.True));

			matchup.SetReferenceText(0, "Then Jesus told them {2}" + separator + "that verse two was important, too. ");
			Assert.That(matchup.CorrelatedBlocks,
				ForEvery<Block>(b => b.MatchesReferenceText, Is.True));
			var followingRefBlock = matchup.CorrelatedBlocks[1].ReferenceBlocks.Single();

			Assert.That(matchup.CorrelatedBlocks[1].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(matchup.CorrelatedBlocks[1].GetPrimaryReferenceText(), Is.EqualTo("“Continuation of previous verse in ref text. {3}\u00A0Three!”"));
			Assert.That(refBlock2.InitialStartVerseNumber, Is.EqualTo(1), "Original should not have been changed -- SetReferenceText needs to make a clone to avoid corrupting original collection.");
			Assert.That(followingRefBlock.InitialStartVerseNumber, Is.EqualTo(2));
			Assert.That(followingRefBlock.InitialEndVerseNumber, Is.EqualTo(0));
			Assert.That(followingRefBlock.CharacterId, Is.EqualTo("Jesus"));
		}

		[Test]
		public void SetReferenceText_VerseNumberAddedToRefBlockOfFirstBlock_FollowingPrimaryAndSecondaryRefBlocksDoNotStartWithVerseNumbers_InitialVerseRefOfClonedFollowingReferenceBlocksChanged()
		{
			var vernacularBlocks = new List<Block> { ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus hablo, diciendo: ", true) };
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
			matchup.MatchAllBlocks();
			Assert.That(matchup.CorrelatedBlocks,
				ForEvery<Block>(b => b.MatchesReferenceText, Is.True));

			matchup.SetReferenceText(0, "Then Jesus told them {2} that verse two was important, too. ");
			Assert.That(matchup.CorrelatedBlocks,
				ForEvery<Block>(b => b.MatchesReferenceText, Is.True));

			Assert.That(matchup.CorrelatedBlocks[1].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(primaryRefBlock2.InitialStartVerseNumber, Is.EqualTo(1), "Original should not have been changed -- SetReferenceText needs to make a clone to avoid corrupting original collection.");
			Assert.That(primaryRefBlock3.InitialStartVerseNumber, Is.EqualTo(3), "Original should not have been changed -- SetReferenceText needs to make a clone to avoid corrupting original collection.");
			Assert.That(secondaryRefBlock2.InitialStartVerseNumber, Is.EqualTo(1), "Original should not have been changed -- SetReferenceText needs to make a clone to avoid corrupting original collection.");
			Assert.That(secondaryRefBlock3.InitialStartVerseNumber, Is.EqualTo(1), "Original should not have been changed -- SetReferenceText needs to make a clone to avoid corrupting original collection.");
			var followingPrimaryRefBlock = matchup.CorrelatedBlocks[1].ReferenceBlocks.Single();
			Assert.That(followingPrimaryRefBlock.InitialStartVerseNumber, Is.EqualTo(2));
			Assert.That(followingPrimaryRefBlock.InitialEndVerseNumber, Is.EqualTo(0));
			Assert.That(followingPrimaryRefBlock.CharacterId, Is.EqualTo("Jesus"));
			var followingSecondaryRefBlock = followingPrimaryRefBlock.ReferenceBlocks.Single();
			Assert.That(followingSecondaryRefBlock.InitialStartVerseNumber, Is.EqualTo(2));
			Assert.That(followingSecondaryRefBlock.InitialEndVerseNumber, Is.EqualTo(0));
			Assert.That(followingSecondaryRefBlock.CharacterId, Is.EqualTo("Jesus"));
		}

		[Test]
		public void SetReferenceText_VerseNumberAddedToRefBlockOfFirstBlock_PrimaryAndSecondaryRefBlocksOfFollowingBlocksDoNotStartWithVerseNumbers_InitialVerseRefOfClonedFollowingReferenceBlocksChanged()
		{
			var vernacularBlocks = new List<Block> { ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus hablo, diciendo: ", true) };
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
			matchup.MatchAllBlocks();
			Assert.That(matchup.CorrelatedBlocks,
				ForEvery<Block>(b => b.MatchesReferenceText, Is.True));

			matchup.SetReferenceText(0, "Then Jesus told them {2-3} that verse two was important, too. ");
			Assert.That(matchup.CorrelatedBlocks,
				ForEvery<Block>(b => b.MatchesReferenceText, Is.True));

			Assert.That(matchup.CorrelatedBlocks[1].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(primaryRefBlock2.InitialStartVerseNumber, Is.EqualTo(1), "Original should not have been changed -- SetReferenceText needs to make a clone to avoid corrupting original collection.");
			Assert.That(primaryRefBlock3.InitialStartVerseNumber, Is.EqualTo(4), "Original should not have been changed -- SetReferenceText needs to make a clone to avoid corrupting original collection.");
			Assert.That(secondaryRefBlock2.InitialStartVerseNumber, Is.EqualTo(1), "Original should not have been changed -- SetReferenceText needs to make a clone to avoid corrupting original collection.");
			Assert.That(secondaryRefBlock3.InitialStartVerseNumber, Is.EqualTo(1), "Original should not have been changed -- SetReferenceText needs to make a clone to avoid corrupting original collection.");
			var followingPrimaryRefBlock = matchup.CorrelatedBlocks[1].ReferenceBlocks.Single();
			Assert.That(followingPrimaryRefBlock.InitialStartVerseNumber, Is.EqualTo(2));
			Assert.That(followingPrimaryRefBlock.InitialEndVerseNumber, Is.EqualTo(3));
			Assert.That(followingPrimaryRefBlock.CharacterId, Is.EqualTo("Jesus"));
			var followingSecondaryRefBlock = followingPrimaryRefBlock.ReferenceBlocks.Single();
			Assert.That(followingSecondaryRefBlock.InitialStartVerseNumber, Is.EqualTo(2));
			Assert.That(followingSecondaryRefBlock.InitialEndVerseNumber, Is.EqualTo(3));
			Assert.That(followingSecondaryRefBlock.CharacterId, Is.EqualTo("Jesus"));
			var lastPrimaryRefBlock = matchup.CorrelatedBlocks[2].ReferenceBlocks.Single();
			Assert.That(lastPrimaryRefBlock.InitialStartVerseNumber, Is.EqualTo(4));
			Assert.That(lastPrimaryRefBlock.InitialEndVerseNumber, Is.EqualTo(0));
			Assert.That(lastPrimaryRefBlock.CharacterId, Is.EqualTo(narrator));
			var lastSecondaryRefBlock = lastPrimaryRefBlock.ReferenceBlocks.Single();
			Assert.That(lastSecondaryRefBlock.InitialStartVerseNumber, Is.EqualTo(2));
			Assert.That(lastSecondaryRefBlock.InitialEndVerseNumber, Is.EqualTo(3));
			Assert.That(lastSecondaryRefBlock.CharacterId, Is.EqualTo(narrator));
		}

		[Test]
		public void SetReferenceText_VerseNumberRemovedFromRefBlockOfFirstBlock_FollowingRefBlockDoesNotStartWithVerseNumber_InitialVerseRefOfFollowingReferenceBlockChanged()
		{
			var vernacularBlocks = new List<Block> { ReferenceTextTests.CreateBlockForVerse(kUnexpectedCharacter, 1, "Entonces Jesus hablo, diciendo: ", true, 1, "p", 3) };
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
			Assert.That(matchup.CorrelatedBlocks,
				ForEvery<Block>(b => b.MatchesReferenceText, Is.True));

			matchup.SetReferenceText(0, "Then Jesus spoke, saying: ");
			Assert.That(matchup.CorrelatedBlocks,
				ForEvery<Block>(b => b.MatchesReferenceText, Is.True));
			var followingRefBlock = matchup.CorrelatedBlocks[1].ReferenceBlocks.Single();

			Assert.That(matchup.CorrelatedBlocks[1].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(matchup.CorrelatedBlocks[1].GetPrimaryReferenceText(), Is.EqualTo("“Continuation of previous verse in ref text. {3}\u00A0Three!”"));
			Assert.That(followingRefBlock.InitialStartVerseNumber, Is.EqualTo(1));
			Assert.That(followingRefBlock.InitialEndVerseNumber, Is.EqualTo(3));
			Assert.That(followingRefBlock.CharacterId, Is.EqualTo("Jesus"));
		}

		[TestCase("\u00A0")]
		[TestCase(" ")]
		[TestCase("")]
		public void SetReferenceText_SubsequentBlock_VerseNumberInMiddle_SpecifiedBlockMatchedWithReferenceBlockWithInitialVerseNumberFromPreviousRefBlock(string separator)
		{
			var vernacularBlocks = new List<Block> { ReferenceTextTests.CreateBlockForVerse(kUnexpectedCharacter, 1, "Entonces Jesus hablo, diciendo: ", true, 1, "p", 3) };
			vernacularBlocks.Last().SetMatchedReferenceBlock(ReferenceTextTests.CreateNarratorBlockForVerse(1, "Then Jesus spoke unto them, ", true));
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "Este es versiculo dos y tres, ", "p");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Jesus. ");
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, null);

			var newRefBlock = matchup.SetReferenceText(1, "saying, {2-3}" + separator + "“This is verse two and three.”");
			// Ensure block 0 not changed
			Assert.That(matchup.CorrelatedBlocks[0].CharacterId, Is.EqualTo(kUnexpectedCharacter));
			Assert.That(matchup.CorrelatedBlocks[0].MatchesReferenceText, Is.True);
			Assert.That(matchup.CorrelatedBlocks[0].GetPrimaryReferenceText(), Is.EqualTo("{1}\u00A0Then Jesus spoke unto them, "));

			Assert.That(matchup.CorrelatedBlocks[0].MatchesReferenceText, Is.True);
			Assert.That(matchup.CorrelatedBlocks[1].GetPrimaryReferenceText(), Is.EqualTo("saying, {2-3}\u00A0“This is verse two and three.”"));
			Assert.That(matchup.CorrelatedBlocks[1].ReferenceBlocks.Single(), Is.EqualTo(newRefBlock));
			Assert.That(newRefBlock.CharacterId, Is.EqualTo("Jesus"), "Should get character info from vern block");
			Assert.That(newRefBlock.InitialStartVerseNumber, Is.EqualTo(1));
			Assert.That(newRefBlock.InitialEndVerseNumber, Is.EqualTo(0));
			Assert.That(newRefBlock.LastVerseNum, Is.EqualTo(3));
		}

		[TestCase("\u00A0")]
		[TestCase(" ")]
		[TestCase("")]
		public void SetReferenceText_VerseNumberAtStart_SpecifiedBlockMatchedWithReferenceBlockHavingVerseNumberFromText(string separator)
		{
			var vernacularBlocks = new List<Block> { ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "Este es versiculo dos y tres, ", true, 1, "p", 3) };
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Jesus. ");
			vernacularBlocks.Last().Delivery = "expressionless";
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, ReferenceText.GetStandardReferenceText(ReferenceTextType.English));
			matchup.CorrelatedBlocks[0].SetMatchedReferenceBlock(
				ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "“This is verse two,” ", true));
			matchup.MatchAllBlocks();
			Assert.That(matchup.CorrelatedBlocks,
				ForEvery<Block>(b => b.MatchesReferenceText, Is.True));
			Assert.That(matchup.CorrelatedBlocks[1].GetPrimaryReferenceText(), Is.EqualTo(""));

			var newRefBlock = matchup.SetReferenceText(1, "{3}" + separator + "“And this is verse three {4}" + separator + "or maybe four.”");
			Assert.That(matchup.CorrelatedBlocks,
				ForEvery<Block>(b => b.MatchesReferenceText, Is.True));
			// Ensure block 0 not changed
			Assert.That(matchup.CorrelatedBlocks[0].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(matchup.CorrelatedBlocks[0].GetPrimaryReferenceText(), Is.EqualTo("{2}\u00A0“This is verse two,” "));

			Assert.That(matchup.CorrelatedBlocks[1].GetPrimaryReferenceText(), Is.EqualTo("{3}\u00A0“And this is verse three {4}\u00A0or maybe four.”"));
			Assert.That(vernacularBlocks.Last().CharacterId, Is.EqualTo(newRefBlock.CharacterId), "Should get character/delivery info from vern block");
			Assert.That(newRefBlock.Delivery, Is.EqualTo("expressionless"), "Should get character/delivery info from vern block");
			Assert.That(newRefBlock.InitialStartVerseNumber, Is.EqualTo(3));
			Assert.That(newRefBlock.InitialEndVerseNumber, Is.EqualTo(0));
			Assert.That(newRefBlock.LastVerseNum, Is.EqualTo(4));
		}

		[TestCase("")]
		[TestCase("{F8 Music--Ends} ")]
		public void SetReferenceText_Secondary_AnnotationAndVerseNumberAtStart_VerseNumberNotLost(string annotationText)
		{
			var vernacularBlocks = new List<Block> { ReferenceTextTests.CreateNarratorBlockForVerse(2, "Esigit diluk nen ane yogiseke:", false, 18, "REV", "p", 3) };
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
			matchup.MatchAllBlocks();
			matchup.SetReferenceText(0, "{2} He cried with a mighty voice, saying,", 1);
			matchup.SetReferenceText(1, "“Fallen is Babylon! She is a home of demons, spirits and unclean birds!", 1);
			matchup.SetReferenceText(2, "spirits and unclean birds!", 1);
			matchup.SetReferenceText(3, annotationText + "{3} All nations had sex and got rich with her.”", 1);
			var englishRefBlockForVernBlock3 = matchup.CorrelatedBlocks[3].ReferenceBlocks.Single().ReferenceBlocks.Single();
			if (annotationText.Length > 0)
				Assert.That(englishRefBlockForVernBlock3.BlockElements.OfType<Sound>().Single().SoundType == SoundType.Music, Is.True);
			Assert.That(englishRefBlockForVernBlock3.BlockElements.OfType<Verse>().Single().Number, Is.EqualTo("3"));
			Assert.That(englishRefBlockForVernBlock3.InitialVerseNumberOrBridge, Is.EqualTo("3"));
			Assert.That(((ScriptText)englishRefBlockForVernBlock3.BlockElements.Last()).Content,
				Is.EqualTo("All nations had sex and got rich with her.”"));
		}

		[Test]
		public void InsertHeSaidText_EnglishOnly_SingleRow_TextSetAndCallbackCalled()
		{
			var vernacularBlocks = new List<Block> { ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "Este es versiculo dos y tres, ", true, 1, "p", 3) };
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Jesus. ");
			var narrator = vernacularBlocks.Last().CharacterId;
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, ReferenceText.GetStandardReferenceText(ReferenceTextType.English));
			matchup.CorrelatedBlocks[0].SetMatchedReferenceBlock(
				ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "“This is verse two,” ", true));
			matchup.MatchAllBlocks();
			Assert.That(matchup.CorrelatedBlocks[1].GetPrimaryReferenceText(), Is.EqualTo(""));

			bool callbackCalled = false;

			matchup.InsertHeSaidText(1, (iRow, level, text) =>
			{
				Assert.That(iRow, Is.EqualTo(1));
				Assert.That(level, Is.EqualTo(0));
				Assert.That(text, Is.EqualTo("he said."));
				Assert.That(callbackCalled, Is.False);
				callbackCalled = true;
			});
			Assert.That(callbackCalled, Is.True);
			Assert.That(matchup.CorrelatedBlocks,
				ForEvery<Block>(b => b.MatchesReferenceText, Is.True));
			Assert.That(matchup.CorrelatedBlocks.Select(b => b.ReferenceBlocks.Single()).Any(rb => rb.ReferenceBlocks.Any()), Is.False,
				"This matchup only has English - no additional levels should be present.");
			Assert.That(matchup.CorrelatedBlocks[1].CharacterId, Is.EqualTo(narrator));
			Assert.That(matchup.CorrelatedBlocks[1].GetPrimaryReferenceText(), Is.EqualTo("he said."));
			Assert.That(matchup.CorrelatedBlocks[0].CharacterId, Is.EqualTo("Jesus"),
				"First reference text should not have been changed!");
			Assert.That(matchup.CorrelatedBlocks[0].GetPrimaryReferenceText(),
				Is.EqualTo("{2}\u00A0“This is verse two,” "),
				"First reference text should not have been changed!");
		}

		[Test]
		public void InsertHeSaidText_ExistingVerseNumber_HeSaidTextInsertedAfterVerseNumber()
		{
			var vernacularBlocks = new List<Block> { ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "Este es versiculo dos y tres, ", true, 1, "p", 3) };
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Jesus. ");
			var narrator = vernacularBlocks.Last().CharacterId;
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, ReferenceText.GetStandardReferenceText(ReferenceTextType.English));
			matchup.CorrelatedBlocks[0].SetMatchedReferenceBlock(
				ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "“This is verse two,” ", true));
			matchup.CorrelatedBlocks[1].SetMatchedReferenceBlock(ReferenceTextTests.CreateNarratorBlockForVerse(3, ""));
			matchup.MatchAllBlocks();
			Assert.That(matchup.CorrelatedBlocks[1].GetPrimaryReferenceText(), Is.EqualTo("{3}\u00A0"));

			bool callbackCalled = false;

			matchup.InsertHeSaidText(1, (iRow, level, text) =>
			{
				Assert.That(iRow, Is.EqualTo(1));
				Assert.That(level, Is.EqualTo(0));
				Assert.That(text, Is.EqualTo("{3}\u00A0he said."));
				Assert.That(callbackCalled, Is.False);
				callbackCalled = true;
			});
			Assert.That(callbackCalled, Is.True);
			Assert.That(matchup.CorrelatedBlocks[1].CharacterId, Is.EqualTo(narrator));
			Assert.That(matchup.CorrelatedBlocks[1].GetPrimaryReferenceText(), Is.EqualTo("{3}\u00A0he said."));
			Assert.That(matchup.CorrelatedBlocks[0].CharacterId, Is.EqualTo("Jesus"), "First reference text should not have been changed!");
			Assert.That(matchup.CorrelatedBlocks[0].GetPrimaryReferenceText(),
				Is.EqualTo("{2}\u00A0“This is verse two,” "),
				"First reference text should not have been changed!");
		}

		[Test]
		public void InsertHeSaidText_NonNarrator_SingleRow_NoChanges()
		{
			var vernacularBlocks = new List<Block>
			{
				ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "Este es versiculo dos y tres, ", true, 1, "p", 3)
			};
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Jesus. ");
			var narrator = vernacularBlocks.Last().CharacterId;
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 0, null, i => true,
				ReferenceText.GetStandardReferenceText(ReferenceTextType.English));
			matchup.MatchAllBlocks();
			Assert.That(matchup.CorrelatedBlocks,
				ForEvery<Block>(b => b.GetPrimaryReferenceText(), Is.Empty));

			bool callbackCalled = false;

			matchup.InsertHeSaidText(0, (iRow, level, text) => { callbackCalled = true; });
			Assert.That(callbackCalled, Is.False);
			Assert.That(matchup.CorrelatedBlocks,
				ForEvery<Block>(b => b.GetPrimaryReferenceText(), Is.Empty));
			Assert.That(matchup.CorrelatedBlocks.Select(b => b.ReferenceBlocks.Single()),
				ForEvery<Block>(rb => rb.ReferenceBlocks, Is.Empty,
				"This matchup only has English - no additional levels should be present."));
			Assert.That(matchup.CorrelatedBlocks[1].CharacterId, Is.EqualTo(narrator));
			Assert.That(matchup.CorrelatedBlocks[0].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(matchup.CorrelatedBlocks[1].ReferenceBlocks.Single().CharacterId, Is.EqualTo(narrator),
				"Reference text character id should not have been changed!");
			Assert.That(matchup.CorrelatedBlocks[0].ReferenceBlocks.Single().CharacterId, Is.EqualTo("Jesus"),
				"Reference text character id should not have been changed!");
		}

		[Test]
		public void InsertHeSaidText_NonEmpty_SingleRow_NoChanges()
		{
			var vernacularBlocks = new List<Block> { ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "Este es versiculo dos y tres, ", true, 1, "p", 3) };
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Jesus. ");
			var narrator = vernacularBlocks.Last().CharacterId;
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, ReferenceText.GetStandardReferenceText(ReferenceTextType.English));
			matchup.CorrelatedBlocks[1].SetMatchedReferenceBlock(ReferenceTextTests.CreateNarratorBlockForVerse(3, "This is not empty, dude."));
			var origRefText = matchup.CorrelatedBlocks[1].GetPrimaryReferenceText();
			matchup.MatchAllBlocks();

			bool callbackCalled = false;

			matchup.InsertHeSaidText(1, (iRow, level, text) => { callbackCalled = true; });
			Assert.That(callbackCalled, Is.False);
			Assert.That(origRefText, Is.EqualTo(matchup.CorrelatedBlocks[1].GetPrimaryReferenceText()),
				"Reference text should not have been changed!");
			Assert.That(matchup.CorrelatedBlocks.Select(b => b.ReferenceBlocks.Single()).Any(rb => rb.ReferenceBlocks.Any()), Is.False,
				"This matchup only has English - no additional levels should be present.");
			Assert.That(matchup.CorrelatedBlocks[1].CharacterId, Is.EqualTo(narrator));
			Assert.That(matchup.CorrelatedBlocks[0].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(matchup.CorrelatedBlocks[1].ReferenceBlocks.Single().CharacterId, Is.EqualTo(narrator),
				"Reference text character id should not have been changed!");
			Assert.That(matchup.CorrelatedBlocks[0].ReferenceBlocks.Single().CharacterId, Is.EqualTo("Jesus"),
				"Reference text character id should not have been changed!");
		}

		[Test]
		public void InsertHeSaidText_MultipleEmptyVerses_SingleRow_NoChanges()
		{
			var vernacularBlocks = new List<Block> { ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "Este es versiculo dos y tres, ", true, 1, "p", 3) };
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Jesus. ");
			var narrator = vernacularBlocks.Last().CharacterId;
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, ReferenceText.GetStandardReferenceText(ReferenceTextType.English));
			matchup.CorrelatedBlocks[1].SetMatchedReferenceBlock(ReferenceTextTests.CreateNarratorBlockForVerse(3, "").AddVerse(4, ""));
			var origRefText = matchup.CorrelatedBlocks[1].GetPrimaryReferenceText();
			matchup.MatchAllBlocks();

			bool callbackCalled = false;

			matchup.InsertHeSaidText(1, (iRow, level, text) => { callbackCalled = true; });
			Assert.That(callbackCalled, Is.False);
			Assert.That(origRefText, Is.EqualTo(matchup.CorrelatedBlocks[1].GetPrimaryReferenceText()),
				"Reference text should not have been changed!");
			Assert.That(matchup.CorrelatedBlocks.Select(b => b.ReferenceBlocks.Single()).Any(rb => rb.ReferenceBlocks.Any()), Is.False,
				"This matchup only has English - no additional levels should be present.");
			Assert.That(matchup.CorrelatedBlocks[1].CharacterId, Is.EqualTo(narrator));
			Assert.That(matchup.CorrelatedBlocks[0].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(matchup.CorrelatedBlocks[1].ReferenceBlocks.Single().CharacterId, Is.EqualTo(narrator),
				"Reference text character id should not have been changed!");
			Assert.That(matchup.CorrelatedBlocks[0].ReferenceBlocks.Single().CharacterId, Is.EqualTo("Jesus"),
				"Reference text character id should not have been changed!");
		}

		[Test]
		public void InsertHeSaidText_UnknownCharacter_SingleRow_CharacterChangedToNarrator()
		{
			var vernacularBlocks = new List<Block> { ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "Este es versiculo dos y tres, ", true, 1, "p", 3) };
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, kNeedsReview, "dijo Jesus. ");
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, ReferenceText.GetStandardReferenceText(ReferenceTextType.English));
			matchup.MatchAllBlocks();
			Assert.That(matchup.CorrelatedBlocks,
				ForEvery<Block>(b => b.GetPrimaryReferenceText(), Is.Empty));

			bool callbackCalled = false;

			matchup.InsertHeSaidText(1, (iRow, level, text) =>
			{
				Assert.That(callbackCalled, Is.False);
				callbackCalled = true;
			});
			Assert.That(callbackCalled, Is.True);

			Assert.That(matchup.CorrelatedBlocks[1].GetPrimaryReferenceText(),
				Is.EqualTo("he said."));
			var narrator = GetStandardCharacterId("MAT", Narrator);
			Assert.That(matchup.CorrelatedBlocks[1].CharacterId, Is.EqualTo(narrator));
			Assert.That(matchup.CorrelatedBlocks[1].ReferenceBlocks.Single().CharacterId,
				Is.EqualTo(narrator));

			Assert.That(matchup.CorrelatedBlocks[0].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(matchup.CorrelatedBlocks[0].ReferenceBlocks.Single().CharacterId,
				Is.EqualTo("Jesus"),
				"Reference text character id should not have been changed!");
			Assert.That(matchup.CorrelatedBlocks[0].GetPrimaryReferenceText(), Is.EqualTo(""));
		}

		[TestCase(ReferenceTextType.Custom, TestReferenceTextResource.AzeriJUD, "dedi.")]
		[TestCase(ReferenceTextType.Custom, TestReferenceTextResource.FrenchMAT, "il a dit.")]
		//[TestCase(ReferenceTextType.Indonesian, "katanya.")]
		//[TestCase(ReferenceTextType.Portuguese, "disse.")]
		[TestCase(ReferenceTextType.Custom, TestReferenceTextResource.SpanishMAT, "dijo.")]
		//[TestCase(ReferenceTextType.TokPisin, "i bin tok.")]
		[TestCase(ReferenceTextType.Russian, null, "сказал.")]
		public void InsertHeSaidText_NonEnglishPrimary_SingleRow_TextSetAndCallbackCalled(ReferenceTextType type,
			TestReferenceTextResource customLanguageBook, string expectedText)
		{
			ReferenceText rt = type == ReferenceTextType.Custom ? TestReferenceText.CreateCustomReferenceText(customLanguageBook) :
				ReferenceText.GetStandardReferenceText(type);

			var vernacularBlocks = new List<Block> { ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "“Tio estas verso du,” ", true) };
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "diris Jesuo. ");
			var narrator = vernacularBlocks.Last().CharacterId;
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, rt);
			matchup.CorrelatedBlocks[0].SetMatchedReferenceBlock(
				ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "“Esto es versiculo dos,” ", true));
			matchup.CorrelatedBlocks[0].ReferenceBlocks.Single().SetMatchedReferenceBlock(
				ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "“This is verse two,” ", true));
			matchup.MatchAllBlocks();

			Assert.That(matchup.CorrelatedBlocks,
				ForEvery<Block>(b => b.MatchesReferenceText, Is.True));
			Assert.That(matchup.CorrelatedBlocks[1].GetPrimaryReferenceText(), Is.EqualTo(""));

			bool callbackCalledForEnglish = false;
			bool callbackCalledForPrimary = false;

			matchup.InsertHeSaidText(1, (iRow, level, text) =>
			{
				Assert.That(iRow, Is.EqualTo(1));
				if (level == 1)
				{
					Assert.That(text, Is.EqualTo("he said."));
					Assert.That(callbackCalledForEnglish, Is.False);
					callbackCalledForEnglish = true;
				}
				else
				{
					Assert.That(level, Is.EqualTo(0));
					Assert.That(text, Is.EqualTo(expectedText));
					Assert.That(callbackCalledForPrimary, Is.False);
					callbackCalledForPrimary = true;					
				}
			});
			Assert.That(callbackCalledForEnglish, Is.True);
			Assert.That(callbackCalledForPrimary, Is.True);
			Assert.That(matchup.CorrelatedBlocks,
				ForEvery<Block>(b => b.MatchesReferenceText, Is.True));
			Assert.That(matchup.CorrelatedBlocks.Select(b => b.ReferenceBlocks.Single()),
				ForEvery<Block>(b => b.MatchesReferenceText, Is.True),
				"This matchup should have a primary reference text plus English.");
			Assert.That(matchup.CorrelatedBlocks[1].CharacterId, Is.EqualTo(narrator));
			Assert.That(matchup.CorrelatedBlocks[1].ReferenceBlocks.Single().CharacterId, Is.EqualTo(narrator));
			Assert.That(matchup.CorrelatedBlocks[1].GetPrimaryReferenceText(),
				Is.EqualTo(expectedText));
			Assert.That(matchup.CorrelatedBlocks[1].ReferenceBlocks.Single().GetPrimaryReferenceText(),
				Is.EqualTo("he said."));

			Assert.That(matchup.CorrelatedBlocks[0].CharacterId, Is.EqualTo("Jesus"),
				"First reference text (primary) should not have been changed!");
			Assert.That(matchup.CorrelatedBlocks[0].ReferenceBlocks.Single().CharacterId,
				Is.EqualTo("Jesus"),
				"First reference text (English) should not have been changed!");
			Assert.That(matchup.CorrelatedBlocks[0].GetPrimaryReferenceText(),
				Is.EqualTo("{2}\u00A0“Esto es versiculo dos,” "),
				"First reference text (primary) should not have been changed!");
			Assert.That(matchup.CorrelatedBlocks[0].ReferenceBlocks.Single().GetPrimaryReferenceText(),
				Is.EqualTo("{2}\u00A0“This is verse two,” "),
				"First reference text (English) should not have been changed!");
		}

		[Test]
		public void InsertHeSaidText_NonEnglishPrimary_AllRows_TextSetAndCallbackCalledOnlyForEmptyRefTexts()
		{
			var vernacularBlocks = new List<Block> { ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "“Tio estas verso du,” ", true) };
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
			var rt = TestReferenceText.CreateCustomReferenceText(TestReferenceTextResource.SpanishMAT);
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, rt);

			var callbacks = new List<Tuple<int, int, string>>();

			matchup.InsertHeSaidText(-1, (iRow, level, text) =>
			{
				callbacks.Add(new Tuple<int, int, string>(iRow, level, text));
			});
			Assert.That(callbacks.Count, Is.EqualTo(2));
			Assert.That(callbacks.Count(c => c.Item1 == 1 && c.Item2 == 1 && c.Item3 == "he said. "),
				Is.EqualTo(1));
			Assert.That(callbacks.Count(c => c.Item1 == 3 && c.Item2 == 0 && c.Item3 == "dijo."),
				Is.EqualTo(1));

			var row1VernBlock = matchup.CorrelatedBlocks[1];
			Assert.That(row1VernBlock.MatchesReferenceText, Is.True);
			Assert.That(row1VernBlock.CharacterId, Is.EqualTo(narrator));
			Assert.That(row1VernBlock.GetPrimaryReferenceText(), Is.EqualTo("dijo Jesus. "));
			var row1SpanishRefBlock = row1VernBlock.ReferenceBlocks.Single();
			Assert.That(row1SpanishRefBlock.MatchesReferenceText, Is.True);
			Assert.That(row1SpanishRefBlock.CharacterId, Is.EqualTo(narrator));
			Assert.That(row1SpanishRefBlock.GetPrimaryReferenceText(), Is.EqualTo("he said. "));

			var row3VernBlock = matchup.CorrelatedBlocks[3];
			Assert.That(row3VernBlock.MatchesReferenceText, Is.True);
			Assert.That(row3VernBlock.CharacterId, Is.EqualTo(narrator));
			Assert.That(row3VernBlock.GetPrimaryReferenceText(), Is.EqualTo("dijo."));
			var row3SpanishRefBlock = row3VernBlock.ReferenceBlocks.Single();
			Assert.That(row3SpanishRefBlock.MatchesReferenceText, Is.True);
			Assert.That(row3SpanishRefBlock.CharacterId, Is.EqualTo(narrator));
			Assert.That(row3SpanishRefBlock.GetPrimaryReferenceText(), Is.EqualTo("Peter said."));

			Assert.That(matchup.CorrelatedBlocks[0].MatchesReferenceText, Is.False);
			Assert.That(matchup.CorrelatedBlocks[2].MatchesReferenceText, Is.False);
		}

		[TestCase(null)]
		[TestCase("Andrew")]
		public void InsertHeSaidText_EmptyRowsAreStartOfMultiBlockQuote_HeSaidInsertedIntoEmptyRefTextsAndBlockChainBroken(string refTextCharacter)
		{
			string origCharacter = kNeedsReview;
			var vernacularBlocks = new List<Block>
			{
				ReferenceTextTests.CreateNarratorBlockForVerse(1, "After these things, Jesus taught them, saying: ", true),
				ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "“Tio estas verso du,” ")
			};
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
			Assert.That(vernacularBlocks[1].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(vernJesusSaidBlock.MatchesReferenceText, Is.False);
			Assert.That(vernJesusSaidBlock.ReferenceBlocks, Is.Empty);
			vernacularBlocks[3].SetMatchedReferenceBlock("This is some arbitrary reference text.");
			vernacularBlocks[3].ReferenceBlocks.Single().CharacterId = refTextCharacter;
			Assert.That(vernacularBlocks[4].ReferenceBlocks, Is.Empty);
			vernacularBlocks[4].SetMatchedReferenceBlock("This is some arbitrary reference text.");
			vernacularBlocks[4].ReferenceBlocks.Single().CharacterId = refTextCharacter;
			Assert.That(vernacularBlocks[5].ReferenceBlocks.Single().CharacterId, Is.EqualTo(narrator));
			Assert.That(vernacularBlocks[6].ReferenceBlocks.Single().CharacterId, Is.EqualTo(narrator));
			Assert.That(vernAndrewSaidBlock.MatchesReferenceText, Is.False);
			Assert.That(vernAndrewSaidBlock.ReferenceBlocks, Is.Empty);
			Assert.That(vernacularBlocks.Count(vb => vb.GetReferenceTextAtDepth(0) == ""), Is.EqualTo(2));
			var matchup = new BlockMatchup(vernBook, 0, null, i => false, rt);
			matchup.MatchAllBlocks();
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
			Assert.That(callbacks.Count, Is.EqualTo(2));
			Assert.That(callbacks.Count(c => c.Item1 == 2 && c.Item2 == 0 && c.Item3 == "he said. "), Is.EqualTo(1));
			Assert.That(callbacks.Count(c => c.Item1 == 7 && c.Item2 == 0 && c.Item3 == "he said. "), Is.EqualTo(1));

			var row2VernBlock = matchup.CorrelatedBlocks[2];
			Assert.That(row2VernBlock.MatchesReferenceText, Is.True);
			Assert.That(row2VernBlock.CharacterId, Is.EqualTo(narrator));
			Assert.That(row2VernBlock.GetPrimaryReferenceText(), Is.EqualTo("he said. "));
			Assert.That(row2VernBlock.MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(origBlock3RefText, Is.EqualTo(matchup.CorrelatedBlocks[3].GetPrimaryReferenceText()));
			Assert.That(refTextCharacter, Is.EqualTo(matchup.CorrelatedBlocks[3].CharacterId));
			Assert.That(matchup.CorrelatedBlocks[3].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(refTextCharacter, Is.EqualTo(matchup.CorrelatedBlocks[4].CharacterId));
			Assert.That(matchup.CorrelatedBlocks[4].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			var row7VernBlock = matchup.CorrelatedBlocks[7];
			Assert.That(row7VernBlock.MatchesReferenceText, Is.True);
			Assert.That(row7VernBlock.CharacterId, Is.EqualTo(narrator));
			Assert.That(row7VernBlock.GetPrimaryReferenceText(), Is.EqualTo("he said. "));
			Assert.That(row7VernBlock.MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(matchup.CorrelatedBlocks.Last().CharacterId, Is.EqualTo(narrator));
			Assert.That(matchup.CorrelatedBlocks.Last().MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
		}

		[Test]
		public void InsertHeSaidText_EmptyRowsAreStartOfMultiBlockQuote_ContBlocksNotAlignedToRefText_HeSaidInsertedIntoEmptyRefTextsAndContBlocksSetToNarrator()
		{
			var vernacularBlocks = new List<Block>
			{
				ReferenceTextTests.CreateNarratorBlockForVerse(1, "Después de estas cosas, Jesús les enseño, diciendo: ", true),
				ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "“Tio estas verso du,” ")
			};
			var vernJesusSaidBlock = ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, kNeedsReview, "diris Jesuo. Etcetera. ");
			vernJesusSaidBlock.MultiBlockQuote = MultiBlockQuote.Start;
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse(kNeedsReview, 3, "This is actually spoken by the narrator but was originally parsed as quoted speech. "));
			vernacularBlocks.Last().MultiBlockQuote = MultiBlockQuote.Continuation;
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse(kNeedsReview, 4, "And this is the rest of the narrator's mumblings."));
			vernacularBlocks.Last().MultiBlockQuote = MultiBlockQuote.Continuation;
			var narrator = ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks,
				"Luego sucedió la próxima cosa.").CharacterId;

			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			ReferenceText rt = ReferenceText.GetStandardReferenceText(ReferenceTextType.English);
			vernacularBlocks[0].SetMatchedReferenceBlock("After these things, Jesus taught them, saying: ");
			vernacularBlocks[1].SetMatchedReferenceBlock("“Uncle, you are verse two,” ");
			vernacularBlocks.Last().SetMatchedReferenceBlock("Then it came about that the next thing happened.");
			Assert.That(vernacularBlocks.Count(vb => vb.GetReferenceTextAtDepth(0) == ""), Is.EqualTo(3));
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, rt, (uint)vernacularBlocks.Count);
			matchup.MatchAllBlocks();

			var callbacks = new List<Tuple<int, int, string>>();

			matchup.InsertHeSaidText(2, (iRow, level, text) =>
			{
				callbacks.Add(new Tuple<int, int, string>(iRow, level, text));
			});
			Assert.That(callbacks.Count, Is.EqualTo(3));
			Assert.That(callbacks.Count(c => c.Item1 == 2 && c.Item2 == 0 && c.Item3 == "he said. "), Is.EqualTo(1));
			Assert.That(callbacks.Count(c => c.Item1 == 3 && c.Item2 == 0 && c.Item3 == null), Is.EqualTo(1));
			Assert.That(callbacks.Count(c => c.Item1 == 4 && c.Item2 == 0 && c.Item3 == null), Is.EqualTo(1));

			var row2VernBlock = matchup.CorrelatedBlocks[2];
			Assert.That(row2VernBlock.MatchesReferenceText, Is.True);
			Assert.That(row2VernBlock.CharacterId, Is.EqualTo(narrator));
			Assert.That(row2VernBlock.GetPrimaryReferenceText(), Is.EqualTo("he said. "));
			Assert.That(row2VernBlock.MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(matchup.CorrelatedBlocks[3].GetPrimaryReferenceText(), Is.EqualTo(""));
			Assert.That(matchup.CorrelatedBlocks[3].CharacterId, Is.EqualTo(narrator));
			Assert.That(matchup.CorrelatedBlocks[3].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(matchup.CorrelatedBlocks[4].GetPrimaryReferenceText(), Is.EqualTo(""));
			Assert.That(matchup.CorrelatedBlocks[4].CharacterId, Is.EqualTo(narrator));
			Assert.That(matchup.CorrelatedBlocks[4].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
		}

		[Test]
		public void HeSaidBlocks_NoBlocksCorrespondToHeSaid_Empty()
		{
			var vernacularBlocks = new List<Block>
			{
				ReferenceTextTests.CreateNarratorBlockForVerse(1, "Después de estas cosas, Jesús les enseño, diciendo: ", true),
				ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "“Tio estas verso du,” ")
			};
			var vernJesusSaidBlock = ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, kUnexpectedCharacter, "diris Jesuo. Etcetera. ");
			vernJesusSaidBlock.MultiBlockQuote = MultiBlockQuote.Start;
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse(kUnexpectedCharacter, 3, "This is actually spoken by the narrator but was originally parsed as quoted speech. "));
			vernacularBlocks.Last().MultiBlockQuote = MultiBlockQuote.Continuation;
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse(kUnexpectedCharacter, 4, "And this is the rest of the narrator's mumblings."));
			vernacularBlocks.Last().MultiBlockQuote = MultiBlockQuote.Continuation;
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "Luego sucedió la próxima cosa.");

			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			ReferenceText rt = ReferenceText.GetStandardReferenceText(ReferenceTextType.English);

			var matchup = new BlockMatchup(vernBook, 0, null, i => true, rt, (uint)vernacularBlocks.Count);
			matchup.MatchAllBlocks();

			Assert.That(matchup.HeSaidBlocks, Is.Empty);
		}

		[Test]
		public void HeSaidBlocks_BlockIsMatchedToHeSaidButNotApplied_Empty()
		{
			var vernacularBlocks = new List<Block>
			{
				ReferenceTextTests.CreateNarratorBlockForVerse(1, "Después de estas cosas, Jesús les enseño, diciendo: ", true),
				ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "“Tio estas verso du,” ")
			};
			var vernJesusSaidBlock = ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, kUnexpectedCharacter, "diris Jesuo. Etcetera. ");
			vernJesusSaidBlock.MultiBlockQuote = MultiBlockQuote.Start;
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse(kUnexpectedCharacter, 3, "This is actually spoken by the narrator but was originally parsed as quoted speech. "));
			vernacularBlocks.Last().MultiBlockQuote = MultiBlockQuote.Continuation;
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse(kUnexpectedCharacter, 4, "And this is the rest of the narrator's mumblings."));
			vernacularBlocks.Last().MultiBlockQuote = MultiBlockQuote.Continuation;
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "Luego sucedió la próxima cosa.");

			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			ReferenceText rt = ReferenceText.GetStandardReferenceText(ReferenceTextType.English);

			var matchup = new BlockMatchup(vernBook, 0, null, i => true, rt, (uint)vernacularBlocks.Count);
			matchup.MatchAllBlocks();
			matchup.SetReferenceText(2, rt.HeSaidText);

			Assert.That(matchup.HeSaidBlocks, Is.Empty);
		}

		[Test]
		public void HeSaidBlocks_BlockIsMatchedToHeSaidAndApplied_ReturnsHeSaidBlock()
		{
			var vernacularBlocks = new List<Block>
			{
				ReferenceTextTests.CreateNarratorBlockForVerse(1, "Después de estas cosas, Jesús les enseño, diciendo: ", true),
				ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "“Tio estas verso du,” ")
			};
			var vernJesusSaidBlock = ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, kUnexpectedCharacter, "diris Jesuo. Etcetera. ");
			vernJesusSaidBlock.MultiBlockQuote = MultiBlockQuote.Start;
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(3, "This is actually spoken by the narrator but was originally parsed as quoted speech. "));
			vernacularBlocks.Last().MultiBlockQuote = MultiBlockQuote.Continuation;
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(4, "And this is the rest of the narrator's mumblings."));
			vernacularBlocks.Last().MultiBlockQuote = MultiBlockQuote.Continuation;
			var narrator = ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "Luego sucedió la próxima cosa.").CharacterId;

			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			ReferenceText rt = ReferenceText.GetStandardReferenceText(ReferenceTextType.English);

			var matchup = new BlockMatchup(vernBook, 0, null, i => true, rt, (uint)vernacularBlocks.Count);
			matchup.MatchAllBlocks();
			matchup.SetReferenceText(2, rt.HeSaidText);
			matchup.CorrelatedBlocks[2].CharacterId = narrator;
			matchup.Apply();

			Assert.That(matchup.HeSaidBlocks.Single().GetText(true), Is.EqualTo("diris Jesuo. Etcetera. "));
		}

		[Test]
		public void HeSaidBlocks_TwoBlocksMatchHeSaidButOnlyOneIsAssignedToNarrator_ReturnsOnlyTheHeSaidBlockSpokenByTheNarrator()
		{
			var vernacularBlocks = new List<Block>
			{
				ReferenceTextTests.CreateNarratorBlockForVerse(1, "Jesús les contó lo que dijo aquel hombre: ", true),
				ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "“\u2014Ya no quiero seguir en cuarentena, ")
			};
			vernacularBlocks.Last().MultiBlockQuote = MultiBlockQuote.Start;
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, kUnexpectedCharacter, "dijo.", "qm");
			vernacularBlocks.Last().MultiBlockQuote = MultiBlockQuote.Continuation;
			vernacularBlocks.Add(ReferenceTextTests.CreateBlockForVerse("Jesus", 3, "“Sin embargo, la cuarentena seguía,”"));
			vernacularBlocks.Last().MultiBlockQuote = MultiBlockQuote.Continuation;
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "recounted Jesus.");
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(4, "Luego sucedió la próxima cosa."));

			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			ReferenceText rt = ReferenceText.GetStandardReferenceText(ReferenceTextType.English);

			var matchup = new BlockMatchup(vernBook, 0, null, i => true, rt, (uint)vernacularBlocks.Count);
			matchup.MatchAllBlocks();
			matchup.SetReferenceText(2, rt.HeSaidText);
			matchup.CorrelatedBlocks[2].CharacterId = "Jesus";
			matchup.InsertHeSaidText(4, (i, i1, arg3) => { });
			
			matchup.Apply();

			Assert.That(matchup.HeSaidBlocks.Single().GetText(true), Is.EqualTo("recounted Jesus."));
		}

		[TestCase(1)]
		[TestCase(2)]
		public void ChangeAnchor_StateReflectsCorrectAnchorBlock(int iBlock)
		{
			var vernacularBlocks = new List<Block>
			{
				ReferenceTextTests.CreateNarratorBlockForVerse(1, "This is a leading verse that should not be included.", true),
				ReferenceTextTests.CreateNarratorBlockForVerse(2, "Partieron de alli para Jerico. ", true).AddVerse(3, "Entonces dijo Jesus: ")
			};
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "Come to me you who are weary,", "q1");
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "And I will give you rest.", "q1");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "Pretty sweet invitation!").AddVerse(4, "Start of another verse.");
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Peter", "Okay, I guess I will. ");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "said Peter.");
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(5, "This is a trailing verse that should not be included.", true));
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, iBlock, null, i => true, null);
			matchup.ChangeAnchor(matchup.CorrelatedBlocks[iBlock]);
			Assert.That(matchup.CorrelatedBlocks[iBlock], Is.EqualTo(matchup.CorrelatedAnchorBlock));
		}

		[Test]
		public void GetCorrespondingOriginalBlock_BlockNotInMatchup_ReturnsNull()
		{
			var vernacularBlocks = new List<Block>
			{
				ReferenceTextTests.CreateNarratorBlockForVerse(1, "This is a leading verse that should not be included.", true),
				ReferenceTextTests.CreateNarratorBlockForVerse(2, "Partieron de alli para Jerico. ", true).AddVerse(3, "Entonces dijo Jesus: ")
			};
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "Come to me you who are weary,", "q1");
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "And I will give you rest.", "q1");
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 1, null, i => true, null);
			Assert.That(matchup.GetCorrespondingOriginalBlock(vernacularBlocks[0]), Is.Null);
		}

		[Test]
		public void GetCorrespondingOriginalBlock_NoSplits_SingleMatch_ReturnsMatchingBlock()
		{
			var vernacularBlocks = new List<Block>
			{
				ReferenceTextTests.CreateNarratorBlockForVerse(1, "This is a leading verse that should not be included.", true),
				ReferenceTextTests.CreateNarratorBlockForVerse(2, "Partieron de alli para Jerico. ", true).AddVerse(3, "Entonces dijo Jesus: ")
			};
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "Come to me you who are weary,", "q1");
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "And I will give you rest.", "q1");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "Pretty sweet invitation!").AddVerse(4, "Start of another verse.");
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Peter", "Okay, I guess I will. ");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "said Peter.");
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(5, "This is a trailing verse that should not be included.", true));
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 1, null, i => true, null);
			for (int i = 0; i < matchup.CorrelatedBlocks.Count; i++)
				Assert.That(vernacularBlocks[i + 1], Is.EqualTo(matchup.GetCorrespondingOriginalBlock(matchup.CorrelatedBlocks[i])));
		}

		[Test]
		public void GetCorrespondingOriginalBlock_Splits_SingleMatch_ReturnsCorrespondingBlock()
		{
			var vernacularBlocks = new List<Block>
			{
				ReferenceTextTests.CreateNarratorBlockForVerse(1, "Entonces Jesus abrio su boca y enseno.", true),
				ReferenceTextTests.CreateBlockForVerse("Jesus", 2, "Este es versiculo dos, ", true)
			};
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Jesus.");
			vernacularBlocks.Add(ReferenceTextTests.CreateNarratorBlockForVerse(3, "Despues se fue. ", true));
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 1, p => p.SplitBlock(p.GetScriptBlocks().First(), "2", 8, false), i => true, null);
			Assert.That(matchup.CorrelatedBlocks.Count, Is.EqualTo(3));
			Assert.That(matchup.CountOfBlocksAddedBySplitting, Is.EqualTo(1));
			Assert.That(vernacularBlocks[1], Is.EqualTo(matchup.GetCorrespondingOriginalBlock(matchup.CorrelatedBlocks[0])));
			Assert.That(vernacularBlocks[1], Is.EqualTo(matchup.GetCorrespondingOriginalBlock(matchup.CorrelatedBlocks[1])));
			Assert.That(vernacularBlocks[2], Is.EqualTo(matchup.GetCorrespondingOriginalBlock(matchup.CorrelatedBlocks[2])));
		}

		[Test] public void GetCorrespondingOriginalBlock_MultipleIdenticalBlocks_ReturnsCorrespondingBlock()
		{
			var vernacularBlocks = new List<Block> { ReferenceTextTests.CreateNarratorBlockForVerse(21, "They were in the middle of the meal. ", true, 26) };
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
				p => p.SplitBlock(p.GetScriptBlocks().ElementAt(2), "21", kSplitAtEndOfVerse, false), i => true, null);
			Assert.That(matchup.CorrelatedBlocks.Count, Is.EqualTo(12));
			Assert.That(matchup.CountOfBlocksAddedBySplitting, Is.EqualTo(1));
			for (int i = 3; i < matchup.CorrelatedBlocks.Count; i++)
				Assert.That(vernacularBlocks[i - 1], Is.EqualTo(matchup.GetCorrespondingOriginalBlock(matchup.CorrelatedBlocks[i])));
		}

		[Test]
		public void GetCorrespondingOriginalBlock_BlockWithTextThatisSubstringOfAnotherBlock_ReturnsCorrespondingBlock()
		{
			var vernacularBlocks = new List<Block> { ReferenceTextTests.CreateNarratorBlockForVerse(21, "They were in the middle of the meal. ", true, 26) };
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
				p => p.SplitBlock(p.GetScriptBlocks().ElementAt(2), "21", kSplitAtEndOfVerse, false), i => true, null);
			Assert.That(matchup.CorrelatedBlocks.Count, Is.EqualTo(12));
			Assert.That(matchup.CountOfBlocksAddedBySplitting, Is.EqualTo(1));
			for (int i = 3; i < matchup.CorrelatedBlocks.Count; i++)
				Assert.That(vernacularBlocks[i - 1], Is.EqualTo(matchup.GetCorrespondingOriginalBlock(matchup.CorrelatedBlocks[i])));
		}

		[Test]
		public void CanChangeCharacterAndDeliveryInfo_NoParameters_ThrowsArgumentException()
		{
			var vernacularBlocks = new List<Block> { ReferenceTextTests.CreateNarratorBlockForVerse(21, "They were in the middle of the meal. ", true, 26) };
			ReferenceTextTests.AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "“I tell you the truth, one of you will betray me,”");
			ReferenceTextTests.AddNarratorBlockForVerseInProgress(vernacularBlocks, "accused Jesus. ").AddVerse(22, "They were very sad. ");
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			var matchup = new BlockMatchup(vernBook, 0, null, i => true, null);
			Assert.Throws<ArgumentException>(() => matchup.CanChangeCharacterAndDeliveryInfo());
		}

		[Test]
		public void CanChangeCharacterAndDeliveryInfo_NoNarratorRows_ReturnsTrue()
		{
			var vernacularBlocks = new List<Block> { ReferenceTextTests.CreateNarratorBlockForVerse(21, "They were in the middle of the meal. ", true, 26) };
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
			Assert.That(matchup.CanChangeCharacterAndDeliveryInfo(1, 3, 5, 7, 9), Is.True);
		}

		[Test]
		public void CanChangeCharacterAndDeliveryInfo_IncludesNarratorRow_ReturnsFalse()
		{
			var vernacularBlocks = new List<Block> { ReferenceTextTests.CreateNarratorBlockForVerse(21, "They were in the middle of the meal. ", true, 26) };
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
			Assert.That(matchup.CanChangeCharacterAndDeliveryInfo(2, 3), Is.False);
		}

		private void VerifyMatchedToCloneOfReferenceBlock(Block origRefBlock, Block vernacularBlock)
		{
			Assert.That(origRefBlock, Is.Not.EqualTo(vernacularBlock.ReferenceBlocks.Single()),
				"Apply should have made a clone of the reference block to avoid nasty cross-linking");
			Assert.That(origRefBlock.GetText(true), Is.EqualTo(vernacularBlock.GetPrimaryReferenceText()));
		}
	}
}
