using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Glyssen;
using Glyssen.Character;
using NUnit.Framework;
using Paratext;
using SIL.Xml;

namespace GlyssenTests
{
	[TestFixture]
	class BookScriptTests
	{
		private int m_curSetupChapter;
		private int m_curSetupVerse;
		private int m_curSetupVerseEnd;
		private string m_curStyleTag;

		#region GetVerseText Tests
		[Test]
		public void GetVerseText_NoBlocks_ReturnsEmptyString()
		{
			var bookScript = new BookScript("MRK", new List<Block>());
			Assert.AreEqual(String.Empty, bookScript.GetVerseText(1, 1));
		}

		[Test]
		public void GetVerseText_BlocksConsistEntirelyOfRequestedVerse_ReturnsBlockContentsWithoutVerseNumber()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			mrkBlocks.Add(NewSingleVersePara(2));
			mrkBlocks.Add(NewSingleVersePara(3, "This is it!"));
			mrkBlocks.Add(NewSingleVersePara(4));
			mrkBlocks.Add(NewSingleVersePara(5));
			var bookScript = new BookScript("MRK", mrkBlocks);
			Assert.AreEqual("This is it!", bookScript.GetVerseText(1, 3));
		}

		[Test]
		public void GetVerseText_FirstVerseInBlockIsRequestedVerse_ReturnsVerseContents()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			mrkBlocks.Add(NewSingleVersePara(2, "This is it!").AddVerse(3).AddVerse(4).AddVerse(5));
			mrkBlocks.Add(NewSingleVersePara(6));
			var bookScript = new BookScript("MRK", mrkBlocks);
			Assert.AreEqual("This is it!", bookScript.GetVerseText(1, 2));
		}

		[Test]
		public void GetVerseText_SubsequentVerseInBlockIsRequestedVerse_ReturnsVerseContents()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			mrkBlocks.Add(NewSingleVersePara(2).AddVerse(3).AddVerse(4, "This is it!").AddVerse(5));
			mrkBlocks.Add(NewSingleVersePara(6));
			var bookScript = new BookScript("MRK", mrkBlocks);
			Assert.AreEqual("This is it!", bookScript.GetVerseText(1, 4));
		}

		[Test]
		public void GetVerseText_VerseInLastBlockInChapterIsRequested_ReturnsVerseContents()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			mrkBlocks.Add(NewSingleVersePara(2).AddVerse(3).AddVerse(4, "This is it!").AddVerse(5));
			var bookScript = new BookScript("MRK", mrkBlocks);
			Assert.AreEqual("This is it!", bookScript.GetVerseText(1, 4));
		}

		[Test]
		public void GetVerseText_RequestedVerseInChapterTwo_ReturnsVerseContents()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1).AddVerse(2).AddVerse(3).AddVerse(4));
			mrkBlocks.Add(NewSingleVersePara(5).AddVerse(6).AddVerse(7));
			mrkBlocks.Add(NewChapterBlock(2));
			mrkBlocks.Add(NewSingleVersePara(1));
			mrkBlocks.Add(NewSingleVersePara(2).AddVerse(3));
			mrkBlocks.Add(NewSingleVersePara(4).AddVerse(5, "This is it!").AddVerse(6));
			mrkBlocks.Add(NewSingleVersePara(7).AddVerse(8));
			var bookScript = new BookScript("MRK", mrkBlocks);
			Assert.AreEqual("This is it!", bookScript.GetVerseText(2, 5));
		}

		[Test]
		public void GetVerseText_RequestedLastVerseInChapterOneOfTwo_ReturnsVerseContents()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1).AddVerse(2).AddVerse(3).AddVerse(4));
			mrkBlocks.Add(NewSingleVersePara(5).AddVerse(6).AddVerse(7, "This is it!"));
			mrkBlocks.Add(NewChapterBlock(2));
			mrkBlocks.Add(NewSingleVersePara(1));
			mrkBlocks.Add(NewSingleVersePara(2).AddVerse(3));
			mrkBlocks.Add(NewSingleVersePara(4).AddVerse(5).AddVerse(6));
			mrkBlocks.Add(NewSingleVersePara(7).AddVerse(8));
			var bookScript = new BookScript("MRK", mrkBlocks);
			Assert.AreEqual("This is it!", bookScript.GetVerseText(1, 7));
		}

		[Test]
		public void GetVerseText_RequestedVerseIsPartOfVerseBridge_ReturnsVerseBridgeContents()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			mrkBlocks.Add(NewSingleVersePara(2).AddVerse(3).AddVerse("4-6", "This is it!"));
			mrkBlocks.Add(NewSingleVersePara(7));
			var bookScript = new BookScript("MRK", mrkBlocks);
			Assert.AreEqual("This is it!", bookScript.GetVerseText(1, 5));
		}

		[Test]
		public void GetVerseText_RequestedVerseSpansBlocks_ContentsJoinedToGetAllVerseText()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			mrkBlocks.Add(NewSingleVersePara(2).AddVerse(3, "This is it!"));
			m_curSetupVerse = 3;
			mrkBlocks.Add(NewPara("q1", "This is more of it."));
			mrkBlocks.Add(NewPara("q2", "This is the rest of it."));
			mrkBlocks.Add(NewSingleVersePara(4));
			var bookScript = new BookScript("MRK", mrkBlocks);
			Assert.AreEqual("This is it!" + Environment.NewLine + "This is more of it." + Environment.NewLine + "This is the rest of it.", bookScript.GetVerseText(1, 3));
		}
		#endregion

		#region GetFirstBlockForVerse Tests
		[Test]
		public void GetFirstBlockForVerse_NoBlocks_ReturnsNull()
		{
			var bookScript = new BookScript("MRK", new List<Block>());
			Assert.IsNull(bookScript.GetFirstBlockForVerse(1, 1));
		}

		[Test]
		public void GetFirstBlockForVerse_BlocksConsistEntirelyOfRequestedVerse_ReturnsBlockWithVerse()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			mrkBlocks.Add(NewSingleVersePara(2));
			mrkBlocks.Add(NewSingleVersePara(3, "This is it!"));
			mrkBlocks.Add(NewSingleVersePara(4));
			mrkBlocks.Add(NewSingleVersePara(5));
			var bookScript = new BookScript("MRK", mrkBlocks);
			var block = bookScript.GetFirstBlockForVerse(1, 3);
			Assert.AreEqual("[3]\u00A0This is it!", block.GetText(true));
		}

		[Test]
		public void GetFirstBlockForVerse_FirstVerseInBlockIsRequestedVerse_ReturnsVerseContents()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			mrkBlocks.Add(NewSingleVersePara(2, "This is it!").AddVerse(3).AddVerse(4).AddVerse(5));
			mrkBlocks.Add(NewSingleVersePara(6));
			var bookScript = new BookScript("MRK", mrkBlocks);
			var block = bookScript.GetFirstBlockForVerse(1, 2);
			Assert.IsTrue(block.GetText(true).StartsWith("[2]\u00A0This is it![3]\u00A0"));
		}

		[Test]
		public void GetFirstBlockForVerse_SubsequentVerseInBlockIsRequestedVerse_ReturnsVerseContents()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			mrkBlocks.Add(NewSingleVersePara(2).AddVerse(3).AddVerse(4, "This is it!").AddVerse(5));
			mrkBlocks.Add(NewSingleVersePara(6));
			var bookScript = new BookScript("MRK", mrkBlocks);
			var block = bookScript.GetFirstBlockForVerse(1, 4);
			Assert.IsTrue(block.GetText(true).Contains("[4]\u00A0This is it![5]\u00A0"));
		}

		[Test]
		public void GetFirstBlockForVerse_VerseInLastBlockInChapterIsRequested_ReturnsVerseContents()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			mrkBlocks.Add(NewSingleVersePara(2).AddVerse(3).AddVerse(4, "This is it!").AddVerse(5));
			var bookScript = new BookScript("MRK", mrkBlocks);
			var block = bookScript.GetFirstBlockForVerse(1, 4);
			Assert.AreEqual(mrkBlocks.Last(), block);
		}

		[Test]
		public void GetFirstBlockForVerse_RequestedVerseInChapterTwo_ReturnsVerseContents()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1).AddVerse(2).AddVerse(3).AddVerse(4));
			mrkBlocks.Add(NewSingleVersePara(5).AddVerse(6).AddVerse(7));
			mrkBlocks.Add(NewChapterBlock(2));
			mrkBlocks.Add(NewSingleVersePara(1));
			mrkBlocks.Add(NewSingleVersePara(2).AddVerse(3));
			mrkBlocks.Add(NewSingleVersePara(4).AddVerse(5, "This is it!").AddVerse(6));
			mrkBlocks.Add(NewSingleVersePara(7).AddVerse(8));
			var bookScript = new BookScript("MRK", mrkBlocks);
			var block = bookScript.GetFirstBlockForVerse(2, 5);
			Assert.IsTrue(block.GetText(true).Contains("[5]\u00A0This is it![6]\u00A0"));
		}

		[Test]
		public void GetFirstBlockForVerse_RequestedVerseIsPartOfVerseBridge_ReturnsVerseBridgeContents()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			mrkBlocks.Add(NewSingleVersePara(2).AddVerse(3).AddVerse("4-6", "This is it!"));
			mrkBlocks.Add(NewSingleVersePara(7));
			var bookScript = new BookScript("MRK", mrkBlocks);
			var block = bookScript.GetFirstBlockForVerse(1, 5);
			Assert.IsTrue(block.GetText(true).EndsWith("[4-6]\u00A0This is it!"));
		}

		[Test]
		public void GetFirstBlockForVerse_RequestedVerseSpansBlocks_ContentsJoinedToGetAllVerseText()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			mrkBlocks.Add(NewSingleVersePara(2).AddVerse(3, "This is it!"));
			var block = new Block("q1", m_curSetupChapter, 3);
			block.BlockElements.Add(new ScriptText("This is more of it."));
			mrkBlocks.Add(block);
			block = new Block("q2", m_curSetupChapter, 3);
			block.BlockElements.Add(new ScriptText("This is the rest of it."));
			mrkBlocks.Add(block);
			mrkBlocks.Add(NewSingleVersePara(4));
			var bookScript = new BookScript("MRK", mrkBlocks);
			var firstBlockForVerse1_3 = bookScript.GetFirstBlockForVerse(1, 3);
			Assert.IsTrue(firstBlockForVerse1_3.GetText(true).EndsWith("[3]\u00A0This is it!"));
		}
		#endregion

		#region GetBlocksForVerse Tests
		[Test]
		public void GetBlocksForVerse_SecondVerseInBridge_ReturnsBlockWithVerse()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(73));
			Block expected;
			mrkBlocks.Add(expected = NewVerseBridgePara(74, 75));
			mrkBlocks.Add(NewSingleVersePara(76));
			var bookScript = new BookScript("MRK", mrkBlocks);
			var list = bookScript.GetBlocksForVerse(1, 75).ToList();
			Assert.AreEqual(1, list.Count);
			Assert.AreEqual(expected, list[0]);
		}

		[Test]
		public void GetBlocksForVerse_SecondVerseInBridgeWithFollowingParasForSameVerseBridge_ReturnsBlockWithVerse()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(73));
			Block expected;
			mrkBlocks.Add(expected = NewVerseBridgePara(74, 75));
			mrkBlocks.Add(NewPara("q1", "more"));
			mrkBlocks.Add(NewPara("q2", "even more"));
			mrkBlocks.Add(NewSingleVersePara(76));
			var bookScript = new BookScript("MRK", mrkBlocks);
			var list = bookScript.GetBlocksForVerse(1, 75).ToList();
			Assert.AreEqual(3, list.Count);
			Assert.AreEqual(expected, list[0]);
		}

		[Test]
		public void GetBlocksForVerse_MultipleBlocksForVerseButSecondBlockDoesNotHaveAnotherVerseNumber_ReturnsAllBlocksForVerse()
		{
			var revBlocks = new List<Block>();
			revBlocks.Add(NewChapterBlock(15));
			revBlocks.Add(NewSingleVersePara(2));
			Block expected;
			revBlocks.Add(expected = NewSingleVersePara(3));
			revBlocks.Add(NewPara("q1", "more"));
			revBlocks.Add(NewPara("q2", "even more"));
			revBlocks.Add(NewSingleVersePara(4));
			var bookScript = new BookScript("REV", revBlocks);
			var list = bookScript.GetBlocksForVerse(15, 3).ToList();
			Assert.AreEqual(3, list.Count);
			Assert.AreEqual(expected, list[0]);
		}
		#endregion

		#region GetScriptBlocks Tests
		[Test]
		public void GetScriptBlocks_NoJoining_GetsEntireBlocksList()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			mrkBlocks.Add(NewSingleVersePara(2));
			mrkBlocks.Add(NewSingleVersePara(3));
			var bookScript = new BookScript("MRK", mrkBlocks);
			Assert.IsTrue(bookScript.GetScriptBlocks(false).SequenceEqual(mrkBlocks));
			Assert.IsTrue(bookScript.GetScriptBlocks().SequenceEqual(mrkBlocks)); // Same as above but proves that false is the default
		}

		[Test]
		public void GetScriptBlocks_JoiningConsecutiveNarratorBlocksInTheSameParagraph_ResultsIncludeJoinedBlocks()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));

			var block = NewSingleVersePara(1).AddVerse(2);
			block.SetStandardCharacter("MRK", CharacterVerseData.StandardCharacter.Narrator);
			mrkBlocks.Add(block);
			m_curSetupVerse = 2;

			block = NewBlock(" «Sons of Thunder»");
			block.SetStandardCharacter("MRK", CharacterVerseData.StandardCharacter.Narrator);
			mrkBlocks.Add(block);

			block = NewBlock(" the rest of verse 2. ");
			block.SetStandardCharacter("MRK", CharacterVerseData.StandardCharacter.Narrator);
			block.Delivery = ""; // There was a bug in which null and blank delivery were not considered the same, thus causing blocks not to combine
			mrkBlocks.Add(block.AddVerse(3));

			block = NewSingleVersePara(4).AddVerse(5);
			block.SetStandardCharacter("MRK", CharacterVerseData.StandardCharacter.Narrator);
			mrkBlocks.Add(block);

			var bookScript = new BookScript("MRK", mrkBlocks);
			var result = bookScript.GetScriptBlocks(true);
			Assert.IsTrue(result[1].CharacterIs("MRK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.IsTrue(result[1].IsParagraphStart);
			var textOfFirstVerseBlock = result[1].GetText(true);
			Assert.IsTrue(textOfFirstVerseBlock.StartsWith("[1]"));
			Assert.IsTrue(textOfFirstVerseBlock.Contains("[2]"));
			Assert.IsTrue(textOfFirstVerseBlock.Contains(" «Sons of Thunder» the rest of verse 2. [3]"));
			Assert.IsTrue(result[2].CharacterIs("MRK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.IsTrue(result[2].IsParagraphStart);
			Assert.IsTrue(result[2].GetText(true).StartsWith("[4]"));
		}

		[Test]
		public void GetScriptBlocks_JoiningConsecutiveBcBlocksInDifferentParagraphs_ResultsNotJoined()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewTitleBlock("The Gospel According to Mark"));
			mrkBlocks.Add(NewChapterBlock(1));

			var bookScript = new BookScript("MRK", mrkBlocks);
			var result = bookScript.GetScriptBlocks(true);
			Assert.AreEqual(2, result.Count);
		}
		#endregion

		#region ApplyUserDecisions Character Assignments Tests
		[Test]
		public void ApplyUserDecisions_NoUserDecisionsOrSplitsInSource_NoChangesInTarget()
		{
			var source = CreateStandardMarkScript();
			var target = CreateStandardMarkScript();
			target.ApplyUserDecisions(source);
			Assert.IsFalse(target.GetScriptBlocks().Any(b => b.UserConfirmed));
			Assert.IsTrue(target.GetScriptBlocks().All(b => b.CharacterIsStandard || b.CharacterId == CharacterVerseData.UnknownCharacter));
			Assert.True(source.GetScriptBlocks().SequenceEqual(target.GetScriptBlocks(), new BlockComparer()));
			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(0, target.UnappliedSplits.Count);
		}

		[Test]
		public void ApplyUserDecisions_UserDecisionsInSourceVerseNumbersAndTextMatchExactly_DecisionsGetCopiedToTarget()
		{
			var source = CreateStandardMarkScript();
			var userConfirmedCharacterBlockIndices = new List<int>();
			for (int i = 0; i < source.GetScriptBlocks().Count; i++)
			{
				if (source[i].CharacterId == CharacterVerseData.UnknownCharacter)
				{
					source[i].CharacterId = "Fred the Frog";
					source[i].Delivery = "with certitude";
					source[i].UserConfirmed = true;
					userConfirmedCharacterBlockIndices.Add(i);
				}
			}
			Assert.IsTrue(userConfirmedCharacterBlockIndices.Count > 0);
			var target = CreateStandardMarkScript();
			target.ApplyUserDecisions(source);

			for (int i = 0; i < target.GetScriptBlocks().Count; i++)
			{
				if (userConfirmedCharacterBlockIndices.Contains(i))
				{
					Assert.AreEqual("Fred the Frog", target[i].CharacterId);
					Assert.AreEqual("with certitude", target[i].Delivery);
					Assert.IsTrue(target[i].UserConfirmed);
				}
				else
				{
					Assert.IsTrue(target[i].CharacterIsStandard);
					Assert.IsNull(target[i].Delivery);
					Assert.IsFalse(target[i].UserConfirmed);
				}
			}
		}

		[Test]
		public void ApplyUserDecisions_UserDecisionsInSourceSomeTextDoesNotMatchExactly_DecisionsGetCopiedToTargetOnlyForExactMatches()
		{
			var source = CreateStandardMarkScript();
			var userConfirmedCharacterBlockIndices = new List<int>();
			for (int i = 0; i < source.GetScriptBlocks().Count; i++)
			{
				if (source[i].CharacterId == CharacterVerseData.UnknownCharacter)
				{
					if (i % 2 == 0)
					{
						int iText = source[i].BlockElements.FindIndex(e => e is ScriptText);
						source[i].BlockElements[iText] = new ScriptText("This is not a match.");
					}
					source[i].CharacterId = "Fred the Frog";
					source[i].Delivery = "with certitude";
					source[i].UserConfirmed = true;
					userConfirmedCharacterBlockIndices.Add(i);
				}
			}
			Assert.IsTrue(userConfirmedCharacterBlockIndices.Count > 0);
			var target = CreateStandardMarkScript();
			target.ApplyUserDecisions(source);

			for (int i = 0; i < target.GetScriptBlocks().Count; i++)
			{
				if (userConfirmedCharacterBlockIndices.Contains(i))
				{
					if (i % 2 == 0)
					{
						Assert.IsTrue(target[i].CharacterId == CharacterVerseData.UnknownCharacter);
						Assert.IsNull(target[i].Delivery);
						Assert.IsFalse(target[i].UserConfirmed);
					}
					else
					{
						Assert.AreEqual("Fred the Frog", target[i].CharacterId);
						Assert.AreEqual("with certitude", target[i].Delivery);
						Assert.IsTrue(target[i].UserConfirmed);
					}
				}
				else
				{
					Assert.IsTrue(target[i].CharacterIsStandard);
					Assert.IsNull(target[i].Delivery);
					Assert.IsFalse(target[i].UserConfirmed);
				}
			}
		}

		[Test]
		public void ApplyUserDecisions_ExtraVersesInTarget_DecisionsGetCopiedToTargetOnlyForExactMatches()
		{
			var source = CreateStandardMarkScript();
			for (int i = 0; i < source.GetScriptBlocks().Count; i++)
			{
				if (source[i].CharacterId == CharacterVerseData.UnknownCharacter)
				{
					source[i].CharacterId = "Fred the Frog";
					source[i].Delivery = "with certitude";
					source[i].UserConfirmed = true;
				}
			}

			var target = CreateStandardMarkScript(true);
			var quoteBlockIndices = new List<int>();
			int iBlockAtVerse7 = -1;
			for (int i = 0; i < target.GetScriptBlocks().Count; i++)
			{
				if (target[i].CharacterId == CharacterVerseData.UnknownCharacter)
					quoteBlockIndices.Add(i);

				if (target[i].InitialStartVerseNumber == 7 && iBlockAtVerse7 < 0)
					iBlockAtVerse7 = i;  // this block has extra verses
			}
			Assert.IsTrue(quoteBlockIndices.Count > 0);
			Assert.IsTrue(iBlockAtVerse7 > 0);
			int iLastBlock = target.GetScriptBlocks().Count - 1; // this block has extra verses
			Assert.IsTrue(iLastBlock > iBlockAtVerse7);
			var indicesOfQuoteBlocksWithExtraVerses = new[] { iBlockAtVerse7, iLastBlock };
			Assert.IsFalse(indicesOfQuoteBlocksWithExtraVerses.Except(quoteBlockIndices).Any());

			target.ApplyUserDecisions(source);

			for (int i = 0; i < target.GetScriptBlocks().Count; i++)
			{
				if (quoteBlockIndices.Contains(i))
				{
					if (indicesOfQuoteBlocksWithExtraVerses.Contains(i))
					{
						Assert.IsTrue(target[i].CharacterId == CharacterVerseData.UnknownCharacter);
						Assert.IsFalse(target[i].UserConfirmed);
					}
					else
					{
						Assert.AreEqual("Fred the Frog", target[i].CharacterId);
						Assert.AreEqual("with certitude", target[i].Delivery);
						Assert.IsTrue(target[i].UserConfirmed);
					}
				}
				else
				{
					Assert.IsTrue(target[i].CharacterIsStandard);
					Assert.IsNull(target[i].Delivery);
					Assert.IsFalse(target[i].UserConfirmed);
				}
			}
		}

		[Test]
		public void ApplyUserDecisions_ExtraVersesInTarget_BookTitleIncluded_DecisionsGetCopiedToTargetOnlyForExactMatches()
		{
			var source = CreateStandardMarkScript(false, true);
			for (int i = 0; i < source.GetScriptBlocks().Count; i++)
			{
				if (source[i].CharacterId == CharacterVerseData.UnknownCharacter)
				{
					source[i].CharacterId = "Fred the Frog";
					source[i].Delivery = "with certitude";
					source[i].UserConfirmed = true;
				}
			}

			var target = CreateStandardMarkScript(true, true);
			var quoteBlockIndices = new List<int>();
			int iBlockAtVerse7 = -1;
			for (int i = 0; i < target.GetScriptBlocks().Count; i++)
			{
				if (target[i].CharacterId == CharacterVerseData.UnknownCharacter)
					quoteBlockIndices.Add(i);

				if (target[i].InitialStartVerseNumber == 7 && iBlockAtVerse7 < 0)
					iBlockAtVerse7 = i;  // this block has extra verses
			}
			Assert.IsTrue(quoteBlockIndices.Count > 0);
			Assert.IsTrue(iBlockAtVerse7 > 0);
			int iLastBlock = target.GetScriptBlocks().Count - 1; // this block has extra verses
			Assert.IsTrue(iLastBlock > iBlockAtVerse7);
			var indicesOfQuoteBlocksWithExtraVerses = new[] { iBlockAtVerse7, iLastBlock };
			Assert.IsFalse(indicesOfQuoteBlocksWithExtraVerses.Except(quoteBlockIndices).Any());

			target.ApplyUserDecisions(source);

			for (int i = 0; i < target.GetScriptBlocks().Count; i++)
			{
				if (quoteBlockIndices.Contains(i))
				{
					if (indicesOfQuoteBlocksWithExtraVerses.Contains(i))
					{
						Assert.IsTrue(target[i].CharacterId == CharacterVerseData.UnknownCharacter);
						Assert.IsFalse(target[i].UserConfirmed);
					}
					else
					{
						Assert.AreEqual("Fred the Frog", target[i].CharacterId);
						Assert.AreEqual("with certitude", target[i].Delivery);
						Assert.IsTrue(target[i].UserConfirmed);
					}
				}
				else
				{
					Assert.IsTrue(target[i].CharacterIsStandard);
					Assert.IsNull(target[i].Delivery);
					Assert.IsFalse(target[i].UserConfirmed);
				}
			}
		}

		[Test]
		public void ApplyUserDecisions_ExtraVersesInSource_DecisionsGetCopiedToTargetOnlyForExactMatches()
		{
			var source = CreateStandardMarkScript(true);
			for (int i = 0; i < source.GetScriptBlocks().Count; i++)
			{
				if (source[i].CharacterId == CharacterVerseData.UnknownCharacter)
				{
					source[i].CharacterId = "Fred the Frog";
					source[i].Delivery = "with certitude";
					source[i].UserConfirmed = true;
				}
			}

			var target = CreateStandardMarkScript();
			var quoteBlockIndices = new List<int>();
			for (int i = 0; i < target.GetScriptBlocks().Count; i++)
			{
				if (target[i].CharacterId == CharacterVerseData.UnknownCharacter)
				{
					quoteBlockIndices.Add(i);
					if (i % 2 != 0)
					{
						int iText = target[i].BlockElements.FindIndex(e => e is ScriptText);
						target[i].BlockElements[iText] = new ScriptText("This is not a match.");
					}
				}
			}
			Assert.IsTrue(quoteBlockIndices.Count > 0);
			Assert.IsTrue(quoteBlockIndices.Count < source.GetScriptBlocks().Count(b => b.CharacterId == "Fred the Frog"));

			target.ApplyUserDecisions(source);

			for (int i = 0; i < target.GetScriptBlocks().Count; i++)
			{
				if (quoteBlockIndices.Contains(i))
				{
					if (i % 2 != 0)
					{
						Assert.AreEqual(CharacterVerseData.UnknownCharacter, target[i].CharacterId);
						Assert.Null(target[i].Delivery);
						Assert.IsFalse(target[i].UserConfirmed);
					}
					else
					{
						Assert.AreEqual("Fred the Frog", target[i].CharacterId);
						Assert.AreEqual("with certitude", target[i].Delivery);
						Assert.IsTrue(target[i].UserConfirmed);
					}
				}
				else
				{
					Assert.IsTrue(target[i].CharacterIsStandard);
					Assert.IsNull(target[i].Delivery);
					Assert.IsFalse(target[i].UserConfirmed);
				}
			}
		}

		[Test]
		public void ApplyUserDecisions_UserConfirmedMultiCharacterIdWithoutCharacterInScriptSet_CharacterInScriptGetsSetToCurrentDefault()
		{
			var source = CreateStandardMarkScript();
			var userConfirmedCharacterBlockIndices = new List<int>();
			for (int i = 0; i < source.GetScriptBlocks().Count; i++)
			{
				if (source[i].CharacterId == CharacterVerseData.UnknownCharacter)
				{
					source[i].CharacterId = "Thomas/James";
					Assert.AreEqual(source[i].CharacterId, source[i].CharacterIdInScript, "CharacterId setter should not set CharacterInScript to default character");
					source[i].UserConfirmed = true;
					userConfirmedCharacterBlockIndices.Add(i);
				}
			}
			Assert.IsTrue(userConfirmedCharacterBlockIndices.Count > 0);
			var target = CreateStandardMarkScript();
			target.ApplyUserDecisions(source);

			for (int i = 0; i < target.GetScriptBlocks().Count; i++)
			{
				if (userConfirmedCharacterBlockIndices.Contains(i))
				{
					Assert.AreEqual("Thomas/James", target[i].CharacterId);
					Assert.AreEqual("Thomas", target[i].CharacterIdInScript);
					Assert.IsTrue(target[i].UserConfirmed);
				}
			}
		}

		[Test]
		public void ApplyUserDecisions_UserConfirmedMultiCharacterIdWithCharacterInScriptSet_CharacterInScriptGetsCopied()
		{
			var source = CreateStandardMarkScript();
			var userConfirmedCharacterBlockIndices = new List<int>();
			for (int i = 0; i < source.GetScriptBlocks().Count; i++)
			{
				if (source[i].CharacterId == CharacterVerseData.UnknownCharacter)
				{
					source[i].CharacterId = "Thomas/James";
					source[i].CharacterIdInScript = "James";
					source[i].UserConfirmed = true;
					userConfirmedCharacterBlockIndices.Add(i);
				}
			}
			Assert.IsTrue(userConfirmedCharacterBlockIndices.Count > 0);
			var target = CreateStandardMarkScript();
			target.ApplyUserDecisions(source);

			for (int i = 0; i < target.GetScriptBlocks().Count; i++)
			{
				if (userConfirmedCharacterBlockIndices.Contains(i))
				{
					Assert.AreEqual("Thomas/James", target[i].CharacterId);
					Assert.AreEqual("James", target[i].CharacterIdInScript);
					Assert.IsTrue(target[i].UserConfirmed);
				}
			}
		}
		#endregion

		#region ApplyUserDecisions Block Splits Tests
		[Test]
		public void ApplyUserDecisions_SplitOneBlockInTwo_TargetBlockCorrespondsToSplitSourceBlocks_SplitReapplied()
		{
			var source = CreateStandardMarkScript();
			var blockToSplit = source.Blocks.First(b => b.InitialStartVerseNumber > 0);
			source.SplitBlock(blockToSplit, "1", 5);

			var target = CreateStandardMarkScript();

			target.ApplyUserDecisions(source);
			Assert.True(source.GetScriptBlocks().SequenceEqual(target.GetScriptBlocks(), new BlockComparer()));
			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(0, target.UnappliedSplits.Count);
		}

		[Test]
		public void ApplyUserDecisions_SplitOneBlockInTwo_TargetBlockAlreadyHasSplitsBecauseParserHasBeenImproved_SplitsIgnored()
		{
			var source = CreateStandardMarkScript();
			var blockToSplit = source.Blocks.First(b => b.InitialStartVerseNumber > 0);
			source.SplitBlock(blockToSplit, "1", 5);

			var target = CreateStandardMarkScript();
			var targetBlockToSplit = target.Blocks.First(b => b.InitialStartVerseNumber > 0);
			var newTargetBlock = target.SplitBlock(targetBlockToSplit, "1", 5);
			targetBlockToSplit.SplitId = Block.kNotSplit;
			newTargetBlock.SplitId = Block.kNotSplit;

			var expected = CreateStandardMarkScript();
			var expectedBlockToSplit = expected.Blocks.First(b => b.InitialStartVerseNumber > 0);
			var newExpectedBlock = expected.SplitBlock(expectedBlockToSplit, "1", 5);
			expectedBlockToSplit.SplitId = Block.kNotSplit;
			newExpectedBlock.SplitId = Block.kNotSplit;

			target.ApplyUserDecisions(source);
			Assert.True(expected.GetScriptBlocks().SequenceEqual(target.GetScriptBlocks(), new BlockComparer()));
			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(0, target.UnappliedSplits.Count);
		}

		[Test]
		public void ApplyUserDecisions_SplitOneBlockInTwo_TargetBlockOnlyMatchesFirstSplitBlock_SplitAddedToUnappliedSplits()
		{
			var source = CreateStandardMarkScript();
			var blockToSplit = source.Blocks.Last(b => b.InitialStartVerseNumber > 0);
			var newBlock = source.SplitBlock(blockToSplit, blockToSplit.LastVerse.ToString(), 5);

			var target = CreateStandardMarkScript();
			var blockToModify = target.Blocks.Last(b => b.InitialStartVerseNumber > 0);
			blockToModify.AddVerse(12, "This is another verse that was added to the new bundle.");

			target.ApplyUserDecisions(source);
			Assert.AreEqual(12, blockToModify.LastVerse);
			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(1, target.UnappliedSplits.Count);
			Assert.IsTrue(target.UnappliedSplits[0].SequenceEqual(new[] { blockToSplit, newBlock }));
		}

		[Test]
		public void ApplyUserDecisions_SplitOneBlockInTwo_ChangeInEarlierVerseInTarget_SplitReapplied()
		{
			var source = CreateStandardMarkScript();
			var blockToSplit = source.Blocks.Last(b => b.InitialStartVerseNumber > 0);
			source.SplitBlock(blockToSplit, blockToSplit.LastVerse.ToString(), 5);

			var target = CreateStandardMarkScript();
			var blockToModify = target.Blocks.First(b => b.InitialStartVerseNumber > 0);
			blockToModify.AddVerse(6, "This is another verse that was added to the new bundle.");

			target.ApplyUserDecisions(source);
			Assert.AreEqual(6, blockToModify.LastVerse);
			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(0, target.UnappliedSplits.Count);
			Assert.AreEqual(source.Blocks.Count, target.Blocks.Count);
			var comparer = new BlockComparer();
			for (int i = 0; i < target.Blocks.Count; i++)
			{
				if (target.Blocks[i] != blockToModify)
					Assert.IsTrue(comparer.Equals(source.Blocks[i], target.Blocks[i]));
			}
		}

		[Test]
		public void ApplyUserDecisions_SplitOneBlockInThree_TargetBlockCorrespondsToSplitSourceBlocks_SplitReapplied()
		{
			var source = CreateStandardMarkScript();
			var blockToSplit = source.Blocks.First(b => b.InitialStartVerseNumber > 0);
			var newBlockToSplit = source.SplitBlock(blockToSplit, "1", 10);
			source.SplitBlock(newBlockToSplit, "1", 5);

			var target = CreateStandardMarkScript();

			target.ApplyUserDecisions(source);
			Assert.True(source.GetScriptBlocks().SequenceEqual(target.GetScriptBlocks(), new BlockComparer()));
			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(0, target.UnappliedSplits.Count);
		}

		[Test]
		public void ApplyUserDecisions_SplitSecondBlockInVerse_TargetBlockAlreadyHasSplitsBecauseParserHasBeenImproved_SplitsIgnored()
		{
			var source = CreateStandardMarkScript();
			Block blockToSplit = GetSecondBlockInVerse(source);
			source.SplitBlock(blockToSplit, blockToSplit.InitialStartVerseNumber.ToString(), 40);

			var target = CreateStandardMarkScript();
			Block targetBlockToSplit = GetSecondBlockInVerse(target);
			var newTargetBlock = target.SplitBlock(targetBlockToSplit, targetBlockToSplit.InitialStartVerseNumber.ToString(), 40);
			targetBlockToSplit.SplitId = Block.kNotSplit;
			newTargetBlock.SplitId = Block.kNotSplit;

			var expected = CreateStandardMarkScript();
			var expectedBlockToSplit = GetSecondBlockInVerse(expected);
			var newExpectedBlock = expected.SplitBlock(expectedBlockToSplit, expectedBlockToSplit.InitialStartVerseNumber.ToString(), 40);
			expectedBlockToSplit.SplitId = Block.kNotSplit;
			newExpectedBlock.SplitId = Block.kNotSplit;

			target.ApplyUserDecisions(source);
			Assert.True(expected.GetScriptBlocks().SequenceEqual(target.GetScriptBlocks(), new BlockComparer()));
			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(0, target.UnappliedSplits.Count);
		}

		[Test]
		public void ApplyUserDecisions_SplitSecondBlockInVerse_TargetBlockCorrespondsToSplitSourceBlocks_SplitReapplied()
		{
			var source = CreateStandardMarkScript();
			Block blockToSplit = GetSecondBlockInVerse(source);
			source.SplitBlock(blockToSplit, blockToSplit.InitialStartVerseNumber.ToString(), 40);

			var target = CreateStandardMarkScript();

			target.ApplyUserDecisions(source);
			Assert.True(source.Blocks.SequenceEqual(target.Blocks, new BlockComparer()));
			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(0, target.UnappliedSplits.Count);
		}

		[Test]
		public void ApplyUserDecisions_SplitSecondBlockInVerse_TargetBlockDoesNotMatch_SplitAddedToUnappliedSplits()
		{
			var source = CreateStandardMarkScript();
			Block blockToSplit = GetSecondBlockInVerse(source);
			var newBlock = source.SplitBlock(blockToSplit, blockToSplit.InitialStartVerseNumber.ToString(), 40);

			var target = CreateStandardMarkScript();
			Block blockToModify = GetSecondBlockInVerse(target);
			var firstScriptTextInSecondVerse = blockToModify.BlockElements.OfType<ScriptText>().First();
			firstScriptTextInSecondVerse.Content = firstScriptTextInSecondVerse.Content.Insert(5, "blah");

			target.ApplyUserDecisions(source);
			Assert.IsTrue(blockToModify.BlockElements.OfType<ScriptText>().First().Content.Contains("blah"));
			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(1, target.UnappliedSplits.Count);
			Assert.IsTrue(target.UnappliedSplits[0].SequenceEqual(new[] { blockToSplit, newBlock }));
		}

		private Block GetSecondBlockInVerse(BookScript bookScript)
		{
			Block blockToSplit = null;
			int currentVerse = -1;
			foreach (var block in bookScript.Blocks.Where(b => b.InitialStartVerseNumber > 0))
			{
				if (block.InitialStartVerseNumber == currentVerse)
				{
					blockToSplit = block;
					break;
				}
				currentVerse = block.LastVerse;
			}
			Assert.IsNotNull(blockToSplit);
			return blockToSplit;
		}

		[Test]
		public void ApplyUserDecisions_SplitMultipleVersesInBlock_TargetBlockAlreadyHasSplitsBecauseParserHasBeenImproved_SplitsIgnored()
		{
			var source = CreateStandardMarkScript();
			Block blockToSplit = source.Blocks.First(b => b.InitialEndVerseNumber != b.LastVerse);
			foreach (var verse in blockToSplit.BlockElements.OfType<Verse>().Select(v => v.StartVerse).ToList())
				blockToSplit = source.SplitBlock(blockToSplit, verse.ToString(), 4);

			var target = CreateStandardMarkScript();
			blockToSplit = target.Blocks.First(b => b.InitialEndVerseNumber != b.LastVerse);
			foreach (var verse in blockToSplit.BlockElements.OfType<Verse>().Select(v => v.StartVerse).ToList())
			{
				var newBlock = target.SplitBlock(blockToSplit, verse.ToString(), 4);
				blockToSplit.SplitId = Block.kNotSplit;
				newBlock.SplitId = Block.kNotSplit;
				blockToSplit = newBlock;
			}

			var expected = CreateStandardMarkScript();
			blockToSplit = expected.Blocks.First(b => b.InitialEndVerseNumber != b.LastVerse);
			foreach (var verse in blockToSplit.BlockElements.OfType<Verse>().Select(v => v.StartVerse).ToList())
			{
				var newBlock = expected.SplitBlock(blockToSplit, verse.ToString(), 4);
				blockToSplit.SplitId = Block.kNotSplit;
				newBlock.SplitId = Block.kNotSplit;
				blockToSplit = newBlock;
			}

			target.ApplyUserDecisions(source);
			Assert.True(expected.GetScriptBlocks().SequenceEqual(target.GetScriptBlocks(), new BlockComparer()));
			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(0, target.UnappliedSplits.Count);
		}

		[Test]
		public void ApplyUserDecisions_SplitMultipleVersesInBlock_TargetBlockCorrespondsToSplitSourceBlocks_SplitReapplied()
		{
			var source = CreateStandardMarkScript();
			Block blockToSplit = source.Blocks.First(b => b.InitialEndVerseNumber != b.LastVerse);
			foreach (var verse in blockToSplit.BlockElements.OfType<Verse>().Select(v => v.StartVerse).ToList())
				blockToSplit = source.SplitBlock(blockToSplit, verse.ToString(), 4);

			var target = CreateStandardMarkScript();

			target.ApplyUserDecisions(source);
			Assert.True(source.Blocks.SequenceEqual(target.Blocks, new BlockComparer()));
			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(0, target.UnappliedSplits.Count);
		}

		[Test]
		public void ApplyUserDecisions_SplitMultipleVersesInBlock_TargetBlockDoesNotMatchLastPieceOfSplit_SplitAddedToUnappliedSplits()
		{
			var source = CreateStandardMarkScript();
			List<Block> splits = new List<Block>();
			Block blockToSplit = source.Blocks.First(b => b.InitialEndVerseNumber != b.LastVerse);
			splits.Add(blockToSplit);
			foreach (var verse in blockToSplit.BlockElements.OfType<Verse>().Select(v => v.StartVerse).ToList())
			{
				blockToSplit = source.SplitBlock(blockToSplit, verse.ToString(), 4);
				splits.Add(blockToSplit);
			}

			var target = CreateStandardMarkScript();
			blockToSplit = target.Blocks.First(b => b.InitialEndVerseNumber != b.LastVerse);
			Block newBlock = null;
			foreach (var verse in blockToSplit.BlockElements.OfType<Verse>().Select(v => v.StartVerse).ToList())
			{
				newBlock = target.SplitBlock(blockToSplit, verse.ToString(), 4);
				blockToSplit.SplitId = Block.kNotSplit;
				newBlock.SplitId = Block.kNotSplit;
				blockToSplit = newBlock;
			}
			var last = newBlock.BlockElements.OfType<ScriptText>().Last();
			last.Content = last.Content.Insert(6, "blah");

			target.ApplyUserDecisions(source);
			Assert.IsTrue(newBlock.BlockElements.OfType<ScriptText>().Last().Content.Contains("blah"));
			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(1, target.UnappliedSplits.Count);
			Assert.IsTrue(target.UnappliedSplits[0].SequenceEqual(splits));
		}

		[Test]
		public void ApplyUserDecisions_SplitImmediatelyBeforeVerseNumber_TargetBlockCorrespondsToSplitSourceBlocks_SplitReapplied()
		{
			var source = CreateStandardMarkScript();
			Block blockToSplit = source.Blocks.First(b => b.InitialStartVerseNumber > 0 && b.BlockElements.First() is ScriptText && b.BlockElements.OfType<Verse>().Any());
			source.SplitBlock(blockToSplit, blockToSplit.InitialStartVerseNumber.ToString(), BookScript.kSplitAtEndOfVerse);

			var target = CreateStandardMarkScript();

			target.ApplyUserDecisions(source);
			Assert.True(source.Blocks.SequenceEqual(target.Blocks, new BlockComparer()));
			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(0, target.UnappliedSplits.Count);
		}

		[Test]
		public void ApplyUserDecisions_SplitImmediatelyBeforeThirdVerseNumberInBlock_TargetBlockCorrespondsToSplitSourceBlocks_SplitReapplied()
		{
			var source = CreateStandardMarkScript();
			Block blockToSplit = source.Blocks.First(b => b.InitialStartVerseNumber > 0 && b.BlockElements.OfType<Verse>().Count() >= 3);
			source.SplitBlock(blockToSplit, blockToSplit.BlockElements.OfType<Verse>().Skip(1).First().Number, BookScript.kSplitAtEndOfVerse);

			var target = CreateStandardMarkScript();

			target.ApplyUserDecisions(source);
			Assert.True(source.Blocks.SequenceEqual(target.Blocks, new BlockComparer()));
			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(0, target.UnappliedSplits.Count);
		}

		[Test]
		public void ApplyUserDecisions_SplitInVerseWithVerseBridge_TargetBlockCorrespondsToSplitSourceBlocks_SplitReapplied()
		{
			var source = CreateStandardMarkScript();
			Block blockToSplit = source.Blocks.Last();
			blockToSplit.AddVerse("12-14", "The magic frog ate a monkey for breakfast.");
			source.SplitBlock(blockToSplit, "12-14", 9);

			var target = CreateStandardMarkScript();
			Block lastBlockInTarget = target.Blocks.Last();
			lastBlockInTarget.AddVerse("12-14", "The magic frog ate a monkey for breakfast.");

			target.ApplyUserDecisions(source);
			Assert.True(source.Blocks.SequenceEqual(target.Blocks, new BlockComparer()));
			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(0, target.UnappliedSplits.Count);
		}

		[Test]
		public void ApplyUserDecisions_SplitTwoUnrelatedBlocks_TargetBlocksCorrespondsToSplitSourceBlocks_SplitReapplied()
		{
			var source = CreateStandardMarkScript();
			Block blockToSplit1 = source.Blocks.First(b => b.InitialStartVerseNumber > 0);
			Block blockToSplit2 = source.Blocks.Last();
			source.SplitBlock(blockToSplit1, blockToSplit1.InitialVerseNumberOrBridge, 3);
			source.SplitBlock(blockToSplit2, blockToSplit2.InitialVerseNumberOrBridge, 3);

			var target = CreateStandardMarkScript();

			target.ApplyUserDecisions(source);
			Assert.True(source.Blocks.SequenceEqual(target.Blocks, new BlockComparer()));
			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(0, target.UnappliedSplits.Count);
		}
		#endregion

		#region ApplyUserDecisions Other Tests
		[Test]
		public void ApplyUserDecisions_UnappliedSplitsExist_MaintainsOriginalUnappliedSplits()
		{
			var source = CreateStandardMarkScript();
			var blockToSplit = source.Blocks.Last(b => b.InitialStartVerseNumber > 0);
			var newBlock = source.SplitBlock(blockToSplit, blockToSplit.LastVerse.ToString(), 5);

			var target = CreateStandardMarkScript();
			var blockToModify = target.Blocks.Last(b => b.InitialStartVerseNumber > 0);
			blockToModify.AddVerse(12, "This is another verse that was added to the new bundle.");

			target.ApplyUserDecisions(source);
			Assert.AreEqual(12, blockToModify.LastVerse);
			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(1, target.UnappliedSplits.Count);
			Assert.IsTrue(target.UnappliedSplits[0].SequenceEqual(new[] { blockToSplit, newBlock }));

			var newSource = target;
			var blockToSplit2 = newSource.Blocks.Last(b => b.InitialStartVerseNumber > 0);
			var newBlock2 = newSource.SplitBlock(blockToSplit2, blockToSplit2.LastVerse.ToString(), 5);

			var newTarget = CreateStandardMarkScript();
			var newBlockToModify = newTarget.Blocks.Last(b => b.InitialStartVerseNumber > 0);
			newBlockToModify.AddVerse(12, "This is another verse that was added to the new bundle, but now the text is different.");

			newTarget.ApplyUserDecisions(newSource);
			Assert.AreEqual(12, newBlockToModify.LastVerse);
			Assert.IsNotNull(newTarget.UnappliedSplits);
			Assert.AreEqual(2, newTarget.UnappliedSplits.Count);
			Assert.IsTrue(newTarget.UnappliedSplits[0].SequenceEqual(new[] { blockToSplit, newBlock }, new BlockComparer()));
			Assert.IsTrue(newTarget.UnappliedSplits[1].SequenceEqual(new[] { blockToSplit2, newBlock2 }, new BlockComparer()));
		}
		#endregion

		#region SplitBlock Tests
		[Test]
		public void SplitBlock_BlockNotInList_ThrowsArgumentException()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			mrkBlocks.Add(NewSingleVersePara(2));
			mrkBlocks.Add(NewSingleVersePara(3));
			var bookScript = new BookScript("MRK", mrkBlocks);
			Assert.Throws<ArgumentException>(() => bookScript.SplitBlock(NewSingleVersePara(5, "Split here, dude."), "5", 11));
			Assert.IsTrue(bookScript.GetScriptBlocks().SequenceEqual(mrkBlocks));
		}

		[Test]
		public void SplitBlock_VerseNotInBlock_ThrowsArgumentException()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			var blockForVerse2 = NewSingleVersePara(2, "It doesn't matter what this is.");
			mrkBlocks.Add(blockForVerse2);
			mrkBlocks.Add(NewSingleVersePara(3));
			var bookScript = new BookScript("MRK", mrkBlocks);
			Assert.Throws<ArgumentException>(() => bookScript.SplitBlock(blockForVerse2, "3", 11));
			Assert.IsTrue(bookScript.GetScriptBlocks().SequenceEqual(mrkBlocks));
		}

		[Test]
		public void SplitBlock_CharacterOffsetNotInVerse_ThrowsArgumentOutOfRangeException()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			var blockForVerse2 = NewSingleVersePara(2, "Short.");
			mrkBlocks.Add(blockForVerse2);
			mrkBlocks.Add(NewSingleVersePara(3));
			var bookScript = new BookScript("MRK", mrkBlocks);
			Assert.Throws<ArgumentOutOfRangeException>(() => bookScript.SplitBlock(blockForVerse2, "2", 400));
			Assert.IsTrue(bookScript.GetScriptBlocks().SequenceEqual(mrkBlocks));
		}

		[Test]
		public void SplitBlock_SingleVerseBlock_SplitsBlockAtSpecifiedOffset()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(5));
			//                                        0         1         2         3         4         5         6         7         8
			//                                        012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
			var blockToSplit = NewSingleVersePara(36, "Ignoring what they said, Jesus told the synagogue ruler: Don't be afraid; just believe.");
			mrkBlocks.Add(blockToSplit);
			var bookScript = new BookScript("MRK", mrkBlocks);
			var newBlock = bookScript.SplitBlock(blockToSplit, "36", 57);
			var blocks = bookScript.GetScriptBlocks();
			Assert.AreEqual(3, blocks.Count);
			Assert.AreEqual(blocks[2], newBlock);
			Assert.AreEqual("[36]\u00A0Ignoring what they said, Jesus told the synagogue ruler: ", blocks[1].GetText(true));
			Assert.AreEqual(36, newBlock.InitialStartVerseNumber);
			Assert.AreEqual(0, newBlock.InitialEndVerseNumber);
			Assert.AreEqual("Don't be afraid; just believe.", newBlock.GetText(true));
		}

		[Test]
		public void SplitBlock_MultiVerseBlock_SubsequentTextAndVersesMovedToNewBlock()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(5));
			//                                        0         1         2         3         4         5         6         7         8
			//                                        012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
			var blockToSplit = NewSingleVersePara(36, "Ignoring what they said, Jesus told the synagogue ruler: Don't be afraid; just believe. ");
			blockToSplit.AddVerse("37-38", "This is the text of following verses. ");
			blockToSplit.AddVerse("39", "This is the text of final verse. ");
			mrkBlocks.Add(blockToSplit);
			var bookScript = new BookScript("MRK", mrkBlocks);
			var newBlock = bookScript.SplitBlock(blockToSplit, "36", 57);
			var blocks = bookScript.GetScriptBlocks();
			Assert.AreEqual(3, blocks.Count);
			Assert.AreEqual(blocks[2], newBlock);
			Assert.AreEqual("[36]\u00A0Ignoring what they said, Jesus told the synagogue ruler: ", blocks[1].GetText(true));
			Assert.AreEqual(36, newBlock.InitialStartVerseNumber);
			Assert.AreEqual(0, newBlock.InitialEndVerseNumber);
			Assert.AreEqual("Don't be afraid; just believe. [37-38]\u00A0This is the text of following verses. [39]\u00A0This is the text of final verse. ", newBlock.GetText(true));
		}

		[Test]
		public void SplitBlock_SplitAtEndOfVerse_SubsequentVersesMovedToNewBlock()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(5));
			//                                        0         1         2         3         4         5         6         7         8
			//                                        012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
			var blockToSplit = NewSingleVersePara(36, "Ignoring what they said, Jesus told the synagogue ruler: Don't be afraid; just believe. ");
			blockToSplit.AddVerse("37-38", "This is the text of following verses. ");
			blockToSplit.AddVerse("39", "This is the text of final verse. ");
			mrkBlocks.Add(blockToSplit);
			var bookScript = new BookScript("MRK", mrkBlocks);
			var newBlock = bookScript.SplitBlock(blockToSplit, "36", 88);
			var blocks = bookScript.GetScriptBlocks();
			Assert.AreEqual(3, blocks.Count);
			Assert.AreEqual(blocks[2], newBlock);
			Assert.AreEqual("[36]\u00A0Ignoring what they said, Jesus told the synagogue ruler: Don't be afraid; just believe. ", blocks[1].GetText(true));
			Assert.AreEqual(37, newBlock.InitialStartVerseNumber);
			Assert.AreEqual(38, newBlock.InitialEndVerseNumber);
			Assert.AreEqual("[37-38]\u00A0This is the text of following verses. [39]\u00A0This is the text of final verse. ", newBlock.GetText(true));
			Assert.AreEqual(2, newBlock.BlockElements.OfType<ScriptText>().Count());
		}

		[Test]
		public void SplitBlock_SplitInSecondVerseInBlock_SubsequentVersesMovedToNewBlock()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(5));
			var blockToSplit = NewSingleVersePara(35, "This is the first verse. ");
			//                          0         1         2         3         4         5         6         7         8
			//                          012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
			blockToSplit.AddVerse("36", "Ignoring what they said, Jesus told the synagogue ruler: Don't be afraid; just believe. ");
			blockToSplit.AddVerse("37", "This is the final verse. ");
			mrkBlocks.Add(blockToSplit);
			var bookScript = new BookScript("MRK", mrkBlocks);
			var newBlock = bookScript.SplitBlock(blockToSplit, "36", 57);
			var blocks = bookScript.GetScriptBlocks();
			Assert.AreEqual(3, blocks.Count);
			Assert.AreEqual(blocks[2], newBlock);
			Assert.AreEqual("[35]\u00A0This is the first verse. [36]\u00A0Ignoring what they said, Jesus told the synagogue ruler: ", blocks[1].GetText(true));
			Assert.AreEqual(36, newBlock.InitialStartVerseNumber);
			Assert.AreEqual(0, newBlock.InitialEndVerseNumber);
			Assert.AreEqual("Don't be afraid; just believe. [37]\u00A0This is the final verse. ", newBlock.GetText(true));
		}

		[Test]
		public void SplitBlock_SplitInVerseBridge_NewBlockHasCorrectStartAndEndVerse()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(5));
			var blockToSplit = NewSingleVersePara(35, "This is the first verse. ");
			//                             0                                                                                                   1
			//                             0         1         2         3         4         5         6         7         8         9         0         1         2         3         4         5         6
			//                             0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012
			blockToSplit.AddVerse("36-37", "Ignoring what they said and prohibiting anyone except Peter, James and John from following him, Jesus told the synagogue ruler: Don't be afraid; just believe. ");
			blockToSplit.AddVerse("38", "This is the final verse. ");
			mrkBlocks.Add(blockToSplit);
			var bookScript = new BookScript("MRK", mrkBlocks);
			var newBlock = bookScript.SplitBlock(blockToSplit, "36-37", 128);
			var blocks = bookScript.GetScriptBlocks();
			Assert.AreEqual(3, blocks.Count);
			Assert.AreEqual(blocks[2], newBlock);
			Assert.AreEqual("[35]\u00A0This is the first verse. [36-37]\u00A0Ignoring what they said and prohibiting anyone except Peter, James and John from following him, Jesus told the synagogue ruler: ", blocks[1].GetText(true));
			Assert.AreEqual(36, newBlock.InitialStartVerseNumber);
			Assert.AreEqual(37, newBlock.InitialEndVerseNumber);
			Assert.AreEqual("Don't be afraid; just believe. [38]\u00A0This is the final verse. ", newBlock.GetText(true));
		}

		[Test]
		public void SplitBlock_AttemptToSplitAtBeginningOfBlockThatIsNotPartOfMultiBlockQuote_ThrowsInvalidOperationException()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			var splitBeforeBlock = NewSingleVersePara(2);
			mrkBlocks.Add(splitBeforeBlock);
			mrkBlocks.Add(NewSingleVersePara(3));
			var bookScript = new BookScript("MRK", mrkBlocks);
			Assert.Throws<InvalidOperationException>(() => bookScript.SplitBlock(splitBeforeBlock, null, 0));
			Assert.IsTrue(bookScript.GetScriptBlocks().SequenceEqual(mrkBlocks));
		}

		[Test]
		public void SplitBlock_AttemptToSplitAtEndOfBlockThatIsNotPartOfMultiBlockQuote_ThrowsInvalidOperationException()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			var splitAfterBlock = NewSingleVersePara(2, "Verse text");
			mrkBlocks.Add(splitAfterBlock);
			mrkBlocks.Add(NewSingleVersePara(3));
			var bookScript = new BookScript("MRK", mrkBlocks);
			Assert.Throws<InvalidOperationException>(() => bookScript.SplitBlock(splitAfterBlock, "2", "Verse text".Length));
			Assert.IsTrue(bookScript.GetScriptBlocks().SequenceEqual(mrkBlocks));
		}

		[Test]
		public void SplitBlock_SplitAtEndOfVerse_OffsetIsSpecialValue_NewBlockHasCorrectInitialVerseNumber()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			var blockToSplit = NewSingleVersePara(2, "Verse text").AddVerse("3-4").AddVerse(5);
			mrkBlocks.Add(blockToSplit);
			var bookScript = new BookScript("MRK", mrkBlocks);
			var newBlock = bookScript.SplitBlock(blockToSplit, "2", BookScript.kSplitAtEndOfVerse);
			Assert.AreEqual(3, newBlock.InitialStartVerseNumber);
			Assert.AreEqual(4, newBlock.InitialEndVerseNumber);
			Assert.AreEqual(2, blockToSplit.BlockElements.Count);
			Assert.AreEqual("2", ((Verse)blockToSplit.BlockElements.First()).Number);
			Assert.AreEqual("Verse text", ((ScriptText)blockToSplit.BlockElements.Last()).Content);
			Assert.AreEqual(4, newBlock.BlockElements.Count);
			Assert.AreEqual("3-4", ((Verse)newBlock.BlockElements.First()).Number);
		}

		[Test]
		public void SplitBlock_SplitAtEndOfVerse_OffsetIsLengthOfText_NewBlockHasCorrectInitialVerseNumber()
		{
			// This tests it with the offset passed in the way it makes logical sense, even though the split dialog never calculates it this way.
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			var blockToSplit = NewSingleVersePara(2, "Verse text").AddVerse("3-4").AddVerse(5);
			mrkBlocks.Add(blockToSplit);
			var bookScript = new BookScript("MRK", mrkBlocks);
			var newBlock = bookScript.SplitBlock(blockToSplit, "2", "Verse text".Length);
			Assert.AreEqual(3, newBlock.InitialStartVerseNumber);
			Assert.AreEqual(4, newBlock.InitialEndVerseNumber);
			Assert.AreEqual(2, blockToSplit.BlockElements.Count);
			Assert.AreEqual("2", ((Verse)blockToSplit.BlockElements.First()).Number);
			Assert.AreEqual("Verse text", ((ScriptText)blockToSplit.BlockElements.Last()).Content);
			Assert.AreEqual(4, newBlock.BlockElements.Count);
			Assert.AreEqual("3-4", ((Verse)newBlock.BlockElements.First()).Number);
		}

		[Test]
		public void SplitBlock_SplitAfterFirstBlockInMultiBlockQuote_UsesSpecialNullParameterCase_FirstBlockChangedToNone()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			var block2 = NewSingleVersePara(1).AddVerse("2-34");
			block2.MultiBlockQuote = MultiBlockQuote.Start;
			mrkBlocks.Add(block2);
			var blockToSplitBefore = NewSingleVersePara(35, "This is the first verse. ");
			blockToSplitBefore.AddVerse("36-37", "Ignoring what they said and prohibiting anyone except Peter, James and John from following him, Jesus told the synagogue ruler: Don't be afraid; just believe. ");
			blockToSplitBefore.MultiBlockQuote = MultiBlockQuote.Continuation;
			mrkBlocks.Add(blockToSplitBefore);
			var block4 = NewSingleVersePara(38).AddVerse(39);
			block4.MultiBlockQuote = MultiBlockQuote.Continuation;
			mrkBlocks.Add(block4);
			var bookScript = new BookScript("MRK", mrkBlocks);
			Assert.AreEqual(4, bookScript.GetScriptBlocks().Count);
			Assert.AreEqual(blockToSplitBefore, bookScript.SplitBlock(blockToSplitBefore, null, 0));
			var blocks = bookScript.GetScriptBlocks();
			Assert.AreEqual(4, blocks.Count);
			Assert.AreEqual(MultiBlockQuote.None, block2.MultiBlockQuote);
			Assert.AreEqual("[35]\u00A0This is the first verse. [36-37]\u00A0Ignoring what they said and prohibiting anyone except Peter, James and John from following him, Jesus told the synagogue ruler: Don't be afraid; just believe. ", blocks[2].GetText(true));
			Assert.AreEqual(MultiBlockQuote.Start, blockToSplitBefore.MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Continuation, block4.MultiBlockQuote);
		}

		[Test]
		public void SplitBlock_SplitAfterFirstBlockInMultiBlockQuote_UsesActualOffset_FirstBlockChangedToNone()
		{
			// This tests it with the offset passed in the way it makes logical sense, even though the split dialog never calculates it this way.
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			var blockToSplitAfter = NewSingleVersePara(1).AddVerse("2-34");
			blockToSplitAfter.MultiBlockQuote = MultiBlockQuote.Start;
			mrkBlocks.Add(blockToSplitAfter);
			var blockToSplitBefore = NewSingleVersePara(35, "This is the first verse. ");
			blockToSplitBefore.AddVerse("36-37", "Ignoring what they said and prohibiting anyone except Peter, James and John from following him, Jesus told the synagogue ruler: Don't be afraid; just believe. ");
			blockToSplitBefore.MultiBlockQuote = MultiBlockQuote.Continuation;
			mrkBlocks.Add(blockToSplitBefore);
			var block4 = NewSingleVersePara(38).AddVerse(39);
			block4.MultiBlockQuote = MultiBlockQuote.Continuation;
			mrkBlocks.Add(block4);
			var bookScript = new BookScript("MRK", mrkBlocks);
			Assert.AreEqual(4, bookScript.GetScriptBlocks().Count);
			Assert.AreEqual(blockToSplitBefore, bookScript.SplitBlock(blockToSplitAfter, "2-34", blockToSplitAfter.BlockElements.OfType<ScriptText>().Last().Content.Length));
			var blocks = bookScript.GetScriptBlocks();
			Assert.AreEqual(4, blocks.Count);
			Assert.AreEqual(MultiBlockQuote.None, blockToSplitAfter.MultiBlockQuote);
			Assert.AreEqual("[35]\u00A0This is the first verse. [36-37]\u00A0Ignoring what they said and prohibiting anyone except Peter, James and John from following him, Jesus told the synagogue ruler: Don't be afraid; just believe. ", blocks[2].GetText(true));
			Assert.AreEqual(MultiBlockQuote.Start, blockToSplitBefore.MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Continuation, block4.MultiBlockQuote);
		}

		[Test]
		public void SplitBlock_SplitBetweenBlocksInMultiBlockQuote_BlockAtStartOfSplitChangedToStart()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			var block2 = NewSingleVersePara(1).AddVerse("2");
			block2.MultiBlockQuote = MultiBlockQuote.Start;
			mrkBlocks.Add(block2);
			var block3 = NewSingleVersePara(1).AddVerse("3-34");
			block3.MultiBlockQuote = MultiBlockQuote.Continuation;
			mrkBlocks.Add(block3);
			var blockToSplitBefore = NewSingleVersePara(35, "This is the first verse. ");
			blockToSplitBefore.AddVerse("36-37", "Ignoring what they said and prohibiting anyone except Peter, James and John from following him, Jesus told the synagogue ruler: Don't be afraid; just believe. ");
			blockToSplitBefore.MultiBlockQuote = MultiBlockQuote.Continuation;
			mrkBlocks.Add(blockToSplitBefore);
			var block5 = NewSingleVersePara(38).AddVerse(39);
			block5.MultiBlockQuote = MultiBlockQuote.Continuation;
			mrkBlocks.Add(block5);
			var bookScript = new BookScript("MRK", mrkBlocks);
			Assert.AreEqual(5, bookScript.GetScriptBlocks().Count);
			Assert.AreEqual(blockToSplitBefore, bookScript.SplitBlock(blockToSplitBefore, null, 0));
			var blocks = bookScript.GetScriptBlocks();
			Assert.AreEqual(5, blocks.Count);
			Assert.AreEqual(MultiBlockQuote.Start, block2.MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Continuation, block3.MultiBlockQuote);
			Assert.AreEqual("[35]\u00A0This is the first verse. [36-37]\u00A0Ignoring what they said and prohibiting anyone except Peter, James and John from following him, Jesus told the synagogue ruler: Don't be afraid; just believe. ", blocks[3].GetText(true));
			Assert.AreEqual(MultiBlockQuote.Start, blockToSplitBefore.MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Continuation, block5.MultiBlockQuote);
		}

		[Test]
		public void SplitBlock_SplitBeforeLastBlockInMultiBlockQuote_BlockChangedToNone()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			var block2 = NewSingleVersePara(1).AddVerse("2-32");
			block2.MultiBlockQuote = MultiBlockQuote.Start;
			mrkBlocks.Add(block2);
			var block3 = NewSingleVersePara(33).AddVerse(34);
			block3.MultiBlockQuote = MultiBlockQuote.Continuation;
			mrkBlocks.Add(block3);
			var blockToSplitBefore = NewSingleVersePara(35, "This is the first verse. ");
			blockToSplitBefore.AddVerse("36-37", "Ignoring what they said and prohibiting anyone except Peter, James and John from following him, Jesus told the synagogue ruler: Don't be afraid; just believe. ");
			blockToSplitBefore.MultiBlockQuote = MultiBlockQuote.Continuation;
			mrkBlocks.Add(blockToSplitBefore);
			var bookScript = new BookScript("MRK", mrkBlocks);
			Assert.AreEqual(4, bookScript.GetScriptBlocks().Count);
			Assert.AreEqual(blockToSplitBefore, bookScript.SplitBlock(blockToSplitBefore, null, 0));
			var blocks = bookScript.GetScriptBlocks();
			Assert.AreEqual(4, blocks.Count);
			Assert.AreEqual(MultiBlockQuote.Start, block2.MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Continuation, block3.MultiBlockQuote);
			Assert.AreEqual("[35]\u00A0This is the first verse. [36-37]\u00A0Ignoring what they said and prohibiting anyone except Peter, James and John from following him, Jesus told the synagogue ruler: Don't be afraid; just believe. ", blocks[3].GetText(true));
			Assert.AreEqual(MultiBlockQuote.None, blockToSplitBefore.MultiBlockQuote);
		}

		[Test]
		public void SplitBlock_SplitBetweenBlocksInTwoBlockQuote_BothBlocksChangedToNone()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			var block2 = NewSingleVersePara(1).AddVerse("2-34");
			block2.MultiBlockQuote = MultiBlockQuote.Start;
			mrkBlocks.Add(block2);
			var blockToSplitBefore = NewSingleVersePara(35, "This is the first verse. ");
			blockToSplitBefore.AddVerse("36-37", "Ignoring what they said and prohibiting anyone except Peter, James and John from following him, Jesus told the synagogue ruler: Don't be afraid; just believe. ");
			blockToSplitBefore.MultiBlockQuote = MultiBlockQuote.Continuation;
			mrkBlocks.Add(blockToSplitBefore);
			var bookScript = new BookScript("MRK", mrkBlocks);
			Assert.AreEqual(3, bookScript.GetScriptBlocks().Count);
			Assert.AreEqual(blockToSplitBefore, bookScript.SplitBlock(blockToSplitBefore, null, 0));
			var blocks = bookScript.GetScriptBlocks();
			Assert.AreEqual(3, blocks.Count);
			Assert.AreEqual(MultiBlockQuote.None, block2.MultiBlockQuote);
			Assert.AreEqual("[35]\u00A0This is the first verse. [36-37]\u00A0Ignoring what they said and prohibiting anyone except Peter, James and John from following him, Jesus told the synagogue ruler: Don't be afraid; just believe. ", blocks[2].GetText(true));
			Assert.AreEqual(MultiBlockQuote.None, blockToSplitBefore.MultiBlockQuote);
		}

		[TestCase(MultiBlockQuote.Continuation)]
		[TestCase(MultiBlockQuote.ChangeOfDelivery)]
		public void SplitBlock_SplitInMiddleOfMultiBlockStart_FirstPartIsNoneAndSecondPartIsStart(MultiBlockQuote continuingStatus)
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));

			var blockToSplit = NewSingleVersePara(1, "Original start block. ");
			blockToSplit.MultiBlockQuote = MultiBlockQuote.Start;
			mrkBlocks.Add(blockToSplit);

			var continuationBlock = NewSingleVersePara(2, "Original continuation block. ");
			continuationBlock.MultiBlockQuote = continuingStatus;
			mrkBlocks.Add(continuationBlock);

			var bookScript = new BookScript("MRK", mrkBlocks);
			var newBlock = bookScript.SplitBlock(blockToSplit, "1", 8);

			Assert.AreEqual(MultiBlockQuote.None, blockToSplit.MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Start, newBlock.MultiBlockQuote);
		}

		[TestCase(MultiBlockQuote.Continuation, MultiBlockQuote.ChangeOfDelivery)]
		[TestCase(MultiBlockQuote.ChangeOfDelivery, MultiBlockQuote.ChangeOfDelivery)]
		[TestCase(MultiBlockQuote.Continuation, MultiBlockQuote.Continuation)]
		[TestCase(MultiBlockQuote.ChangeOfDelivery, MultiBlockQuote.Continuation)]
		public void SplitBlock_SplitInMiddleOfMultiBlockContinuationFollowedByContinuationBlock_FirstPartIsContinuationAndSecondPartIsStart(MultiBlockQuote continuingStatus1, MultiBlockQuote continuingStatus2)
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));

			var startBlock = NewSingleVersePara(1, "Original start block. ");
			startBlock.MultiBlockQuote = MultiBlockQuote.Start;
			mrkBlocks.Add(startBlock);

			var blockToSplit = NewSingleVersePara(2, "Original continuation block (first). ");
			blockToSplit.MultiBlockQuote = continuingStatus1;
			mrkBlocks.Add(blockToSplit);

			var nextContinuationBlock = NewSingleVersePara(3, "Original continuation block (second). ");
			nextContinuationBlock.MultiBlockQuote = continuingStatus2;
			mrkBlocks.Add(nextContinuationBlock);

			var bookScript = new BookScript("MRK", mrkBlocks);
			var newBlock = bookScript.SplitBlock(blockToSplit, "2", 8);

			Assert.AreEqual(continuingStatus1, blockToSplit.MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Start, newBlock.MultiBlockQuote);
		}

		[TestCase(MultiBlockQuote.Continuation)]
		[TestCase(MultiBlockQuote.ChangeOfDelivery)]
		public void SplitBlock_SplitInMiddleOfMultiBlockContinuationFollowedByNoneBlock_FirstPartIsContinuationAndSecondPartIsNone(MultiBlockQuote continuingStatus)
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));

			var startBlock = NewSingleVersePara(1, "Original start block. ");
			startBlock.MultiBlockQuote = MultiBlockQuote.Start;
			mrkBlocks.Add(startBlock);

			var blockToSplit = NewSingleVersePara(2, "Original continuation block. ");
			blockToSplit.MultiBlockQuote = continuingStatus;
			mrkBlocks.Add(blockToSplit);

			var noneBlock = NewSingleVersePara(3, "Original none block. ");
			noneBlock.MultiBlockQuote = MultiBlockQuote.None;
			mrkBlocks.Add(noneBlock);

			var bookScript = new BookScript("MRK", mrkBlocks);
			var newBlock = bookScript.SplitBlock(blockToSplit, "2", 8);

			Assert.AreEqual(continuingStatus, blockToSplit.MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.None, newBlock.MultiBlockQuote);
		}

		[Test]
		public void SplitBlock_FirstBlockHasOriginalCharacterIdAndUserConfirmedTrueAndSecondBlockHasUnknownCharacterIdAndUserConfirmedFalse()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			var blockToSplit = NewSingleVersePara(1).AddVerse("2", "Some text");
			blockToSplit.CharacterId = "Bill";
			blockToSplit.UserConfirmed = true;
			mrkBlocks.Add(blockToSplit);
			var bookScript = new BookScript("MRK", mrkBlocks);
			var newBlock = bookScript.SplitBlock(blockToSplit, "2", 5);
			Assert.AreEqual("Bill", blockToSplit.CharacterId);
			Assert.AreEqual(CharacterVerseData.UnknownCharacter, newBlock.CharacterId);
			Assert.IsTrue(blockToSplit.UserConfirmed);
			Assert.IsFalse(newBlock.UserConfirmed);
		}

		[Test]
		public void SplitBlock_GetFirstBlockForVerse_SplitBlockInPreviousChapterAfterCallingGetFirstBlockForVerseInSubsequentChapter()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			var blockToSplit = NewSingleVersePara(1).AddVerse("1", "Block to split");
			mrkBlocks.Add(blockToSplit);
			mrkBlocks.Add(NewChapterBlock(2));
			var block2 = NewSingleVersePara(1).AddVerse("1", "A verse in a subsequent chapter");
			mrkBlocks.Add(block2);
			var bookScript = new BookScript("MRK", mrkBlocks);
			Assert.AreEqual(4, bookScript.GetScriptBlocks().Count);

			// Call GetFirstBlockForVerse on a chapter after the block we are splitting.
			// There was a bug in the code which updates an index in the split code.
			// That index is not set up until we call this.
			var blockResultBeforeSplit = bookScript.GetFirstBlockForVerse(2, 1);
			bookScript.SplitBlock(blockToSplit, "1", 5);

			var blockResultAfterSplit = bookScript.GetFirstBlockForVerse(2, 1);
			Assert.AreEqual(blockResultBeforeSplit, blockResultAfterSplit);
		}

		[Test]
		public void SplitBlock_NoBlockInBookHasBeenSplitPreviously_ResultBlocksHaveSplitIdOfZero()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(5));
			mrkBlocks.Add(NewSingleVersePara(35, "This block is not split"));
			//                                        0         1         2
			//                                        01234567890123456789012345
			var blockToSplit = NewSingleVersePara(36, "Before split: After Split");
			mrkBlocks.Add(blockToSplit);
			var bookScript = new BookScript("MRK", mrkBlocks);

			bookScript.SplitBlock(blockToSplit, "36", 15);

			var blocks = bookScript.GetScriptBlocks();
			Assert.AreEqual(Block.kNotSplit, blocks[0].SplitId); // chapter number
			Assert.AreEqual(Block.kNotSplit, blocks[1].SplitId);
			Assert.AreEqual(0, blocks[2].SplitId);
			Assert.AreEqual(0, blocks[3].SplitId);
		}

		[Test]
		public void SplitBlock_BlockInBookHasBeenSplitPreviously_ResultBlocksHaveMaxSplitIdPlusOne()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(5));
			Block previouslySplitBlock1 = NewSingleVersePara(35, "This block was split previously");
			previouslySplitBlock1.SplitId = 5;
			mrkBlocks.Add(previouslySplitBlock1);
			Block previouslySplitBlock2 = NewSingleVersePara(35, "This block was split previously");
			previouslySplitBlock2.SplitId = 5;
			mrkBlocks.Add(previouslySplitBlock2);
			//                                        0         1         2
			//                                        01234567890123456789012345
			var blockToSplit = NewSingleVersePara(36, "Before split: After Split");
			mrkBlocks.Add(blockToSplit);
			var bookScript = new BookScript("MRK", mrkBlocks);

			bookScript.SplitBlock(blockToSplit, "36", 15);

			var blocks = bookScript.GetScriptBlocks();
			Assert.AreEqual(Block.kNotSplit, blocks[0].SplitId); // chapter number
			Assert.AreEqual(5, blocks[1].SplitId);
			Assert.AreEqual(5, blocks[2].SplitId);
			Assert.AreEqual(6, blocks[3].SplitId);
			Assert.AreEqual(6, blocks[3].SplitId);
		}

		[Test]
		public void SplitBlock_BlockBeingSplitHasBeenSplitPreviously_ResultBlocksHaveSameSplitIdAsBlockToSplit()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(5));
			mrkBlocks.Add(NewSingleVersePara(35, "This block is not split"));
			//                                        0         1         2
			//                                        01234567890123456789012345
			var blockToSplit = NewSingleVersePara(36, "Before split: After Split");
			blockToSplit.SplitId = 3;
			mrkBlocks.Add(blockToSplit);
			var bookScript = new BookScript("MRK", mrkBlocks);

			bookScript.SplitBlock(blockToSplit, "36", 15);

			var blocks = bookScript.GetScriptBlocks();
			Assert.AreEqual(Block.kNotSplit, blocks[0].SplitId); // chapter number
			Assert.AreEqual(Block.kNotSplit, blocks[1].SplitId);
			Assert.AreEqual(3, blocks[2].SplitId);
			Assert.AreEqual(3, blocks[3].SplitId);
		}

		[Test]
		public void SplitBlock_MultiBlock_NoBlockInBookHasBeenSplitPreviously_ResultBlocksHaveSplitIdOfZero()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(5));
			mrkBlocks.Add(NewSingleVersePara(35, "This block is not split"));
			var blockToSplitAfter = NewSingleVersePara(36, "Before split: ");
			blockToSplitAfter.MultiBlockQuote = MultiBlockQuote.Start;
			mrkBlocks.Add(blockToSplitAfter);
			var blockToSplitBefore = NewSingleVersePara(36, "After Split");
			blockToSplitBefore.MultiBlockQuote = MultiBlockQuote.Continuation;
			mrkBlocks.Add(blockToSplitBefore);
			var bookScript = new BookScript("MRK", mrkBlocks);

			bookScript.SplitBlock(blockToSplitBefore, null, 0);

			var blocks = bookScript.GetScriptBlocks();
			Assert.AreEqual(Block.kNotSplit, blocks[0].SplitId); // chapter number
			Assert.AreEqual(Block.kNotSplit, blocks[1].SplitId);
			Assert.AreEqual(0, blocks[2].SplitId);
			Assert.AreEqual(0, blocks[3].SplitId);
		}

		[Test]
		public void TrySplitBlockAtEndOfVerse_NoVerseElementsInBlock_ReturnsFalse()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			var blockToSplit = new Block("p", m_curSetupChapter, 1) { IsParagraphStart = true };
			blockToSplit.BlockElements.Add(new ScriptText("Blah."));
			mrkBlocks.Add(blockToSplit);
			var bookScript = new BookScript("MRK", mrkBlocks);
			Assert.IsFalse(bookScript.TrySplitBlockAtEndOfVerse(blockToSplit, 1));
			Assert.AreEqual(2, bookScript.GetScriptBlocks().Count);
		}

		[Test]
		public void TrySplitBlockAtEndOfVerse_BlockStartsInTheMiddleOfAVerseBridgeThatEndsWithTheVerseNumberToSplitAfter_SplitsAtEndOfVerseBridge()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			var precedingBlock = NewVerseBridgePara(1, 2,
				"This is the part of the bridged verse that occurs in the preceding paragraph.");
			mrkBlocks.Add(precedingBlock);
			var blockToSplit = NewPara("p", "This is the remainder of the verse bridge in the block to split. ");
			blockToSplit.AddVerse("3", "This is the text of the following verse.");
			mrkBlocks.Add(blockToSplit);
			var bookScript = new BookScript("MRK", mrkBlocks);
			Assert.IsTrue(bookScript.TrySplitBlockAtEndOfVerse(blockToSplit, 2));
			var blocks = bookScript.GetScriptBlocks();
			Assert.AreEqual(4, blocks.Count);
			Assert.AreEqual("This is the remainder of the verse bridge in the block to split. ", blocks[2].GetText(true));
			Assert.AreEqual(1, blocks[2].InitialStartVerseNumber);
			Assert.AreEqual(2, blocks[2].InitialEndVerseNumber);
			Assert.AreEqual("[3]\u00A0This is the text of the following verse.", blocks[3].GetText(true));
			Assert.AreEqual(3, blocks[3].InitialStartVerseNumber);
			Assert.AreEqual(0, blocks[3].InitialEndVerseNumber);
		}

		[Test]
		public void TrySplitBlockAtEndOfVerse_VerseToSplitAfterHasABridgeAndASubVerseLetter_SplitsAtEndOfLastPartOfVerse()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			var blockToSplit = new Block("p", m_curSetupChapter, 1, 2) { IsParagraphStart = true }.AddVerse(
				"1-2a", "This is the bridge that has the first part of verse 2. ")
				.AddVerse("2b", "This is the rest of verse two. ")
				.AddVerse("3", "This is the text of the following verse.");
			mrkBlocks.Add(blockToSplit);
			var bookScript = new BookScript("MRK", mrkBlocks);
			Assert.IsTrue(bookScript.TrySplitBlockAtEndOfVerse(blockToSplit, 2));
			var blocks = bookScript.GetScriptBlocks();
			Assert.AreEqual(3, blocks.Count);
			Assert.AreEqual("[1-2a]\u00A0This is the bridge that has the first part of verse 2. [2b]\u00A0This is the rest of verse two. ",
				blocks[1].GetText(true));
			Assert.AreEqual(1, blocks[1].InitialStartVerseNumber);
			Assert.AreEqual(2, blocks[1].InitialEndVerseNumber);
			Assert.AreEqual("[3]\u00A0This is the text of the following verse.", blocks[2].GetText(true));
			Assert.AreEqual(3, blocks[2].InitialStartVerseNumber);
			Assert.AreEqual(0, blocks[2].InitialEndVerseNumber);
		}

		[Test]
		public void TrySplitBlockAtEndOfVerse_VerseToSplitAfterHasABridgeAndASubVerseLetterButSecondOPartOfVerseIsInFollowingBlock_ReturnsFalse()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			var blockToSplit = new Block("p", m_curSetupChapter, 1, 2) { IsParagraphStart = true }
				.AddVerse("1-2a", "This is the bridge that has the first part of verse 2. ");
			mrkBlocks.Add(blockToSplit);
			var followingBlock = new Block("p", m_curSetupChapter, 2) { CharacterId = "Jesus" }
				.AddVerse("2b", "This is the rest of verse two. ")
				.AddVerse("3", "This is the text of the following verse.");
			mrkBlocks.Add(followingBlock);
			var bookScript = new BookScript("MRK", mrkBlocks);
			Assert.IsFalse(bookScript.TrySplitBlockAtEndOfVerse(blockToSplit, 2));
			Assert.AreEqual(3, bookScript.GetScriptBlocks().Count);
		}

		[Test]
		public void TrySplitBlockAtEndOfVerse_VerseToSplitBeginsWithSecondOPartOfVerseToSplit_SplitsAtEndOfLastPartOfVerse()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			var precedingBlock = new Block("p", m_curSetupChapter, 1, 2) { IsParagraphStart = true }
				.AddVerse("1-2a", "This is the bridge that has the first part of verse 2. ");
			mrkBlocks.Add(precedingBlock);
			var blockToSplit = new Block("p", m_curSetupChapter, 2) { CharacterId = "Jesus" }
				.AddVerse("2b", "This is the rest of verse two. ")
				.AddVerse("3", "This is the text of the following verse.");
			mrkBlocks.Add(blockToSplit);
			var bookScript = new BookScript("MRK", mrkBlocks);
			Assert.IsTrue(bookScript.TrySplitBlockAtEndOfVerse(blockToSplit, 2));
			var blocks = bookScript.GetScriptBlocks();
			Assert.AreEqual(4, blocks.Count);
			Assert.AreEqual("[1-2a]\u00A0This is the bridge that has the first part of verse 2. ",
				blocks[1].GetText(true));
			Assert.AreEqual(1, blocks[1].InitialStartVerseNumber);
			Assert.AreEqual(2, blocks[1].InitialEndVerseNumber);
			Assert.AreEqual("[2b]\u00A0This is the rest of verse two. ", blocks[2].GetText(true));
			Assert.AreEqual(2, blocks[2].InitialStartVerseNumber);
			Assert.AreEqual(0, blocks[2].InitialEndVerseNumber);
			Assert.AreEqual("[3]\u00A0This is the text of the following verse.", blocks[3].GetText(true));
			Assert.AreEqual(3, blocks[3].InitialStartVerseNumber);
			Assert.AreEqual(0, blocks[3].InitialEndVerseNumber);
		}
		#endregion

		#region CleanUpMultiBlockQuotes Tests
		[Test]
		public void CleanUpMultiBlockQuotes_MultiBlockHasSingleCharacter_DataUnchanged()
		{
			var mrkBlocks = new List<Block>();

			var block = NewSingleVersePara(1);
			block.CharacterId = "Andrew";
			block.CharacterIdInScript = "Andrew";
			block.MultiBlockQuote = MultiBlockQuote.Start;
			block.UserConfirmed = true;
			mrkBlocks.Add(block);

			block = NewSingleVersePara(2);
			block.CharacterId = "Andrew";
			block.CharacterIdInScript = "Andrew";
			block.MultiBlockQuote = MultiBlockQuote.Continuation;
			block.UserConfirmed = true;
			mrkBlocks.Add(block);

			block = NewSingleVersePara(2);
			block.SetStandardCharacter("MRK", CharacterVerseData.StandardCharacter.Narrator);
			block.MultiBlockQuote = MultiBlockQuote.None;
			mrkBlocks.Add(block);

			var bookScript = new BookScript("MRK", mrkBlocks);

			bookScript.CleanUpMultiBlockQuotes(ScrVers.English);
			var result = bookScript.GetScriptBlocks();

			Assert.AreEqual(3, result.Count);
			Assert.AreEqual("Andrew", result[0].CharacterId);
			Assert.AreEqual("Andrew", result[0].CharacterIdInScript);
			Assert.True(result[0].UserConfirmed);
			Assert.AreEqual("Andrew", result[1].CharacterId);
			Assert.AreEqual("Andrew", result[1].CharacterIdInScript);
			Assert.True(result[1].UserConfirmed);
			Assert.IsTrue(result[2].CharacterIs("MRK", CharacterVerseData.StandardCharacter.Narrator));
		}

		[Test]
		public void CleanUpMultiBlockQuotes_MultiBlockHasMultipleCharacters_CharacterIsAmbiguousAndUserConfirmedIsFalse()
		{
			var mrkBlocks = new List<Block>();

			var block = NewSingleVersePara(1);
			block.CharacterId = "Andrew";
			block.CharacterIdInScript = "Andrew";
			block.MultiBlockQuote = MultiBlockQuote.Start;
			block.UserConfirmed = true;
			mrkBlocks.Add(block);

			block = NewSingleVersePara(2);
			block.CharacterId = "Peter";
			block.CharacterIdInScript = "Peter";
			block.MultiBlockQuote = MultiBlockQuote.Continuation;
			block.UserConfirmed = true;
			mrkBlocks.Add(block);

			block = NewSingleVersePara(2);
			block.CharacterId = "Peter";
			block.CharacterIdInScript = "Peter";
			block.MultiBlockQuote = MultiBlockQuote.None;
			block.UserConfirmed = true;
			mrkBlocks.Add(block);

			var bookScript = new BookScript("MRK", mrkBlocks);

			bookScript.CleanUpMultiBlockQuotes(ScrVers.English);
			var result = bookScript.GetScriptBlocks();

			Assert.AreEqual(3, result.Count);
			Assert.AreEqual(CharacterVerseData.AmbiguousCharacter, result[0].CharacterId);
			Assert.AreEqual(CharacterVerseData.AmbiguousCharacter, result[0].CharacterIdInScript);
			Assert.False(result[0].UserConfirmed);
			Assert.AreEqual(CharacterVerseData.AmbiguousCharacter, result[1].CharacterId);
			Assert.AreEqual(CharacterVerseData.AmbiguousCharacter, result[1].CharacterIdInScript);
			Assert.False(result[1].UserConfirmed);
			Assert.AreEqual("Peter", result[2].CharacterId);
			Assert.AreEqual("Peter", result[2].CharacterIdInScript);
			Assert.True(result[2].UserConfirmed);
		}
		#endregion

		#region Private Helper methods
		private Block NewTitleBlock(string text)
		{
			var block = new Block("mt");
			block.IsParagraphStart = true;
			block.BlockElements.Add(new ScriptText(text));
			return block;
		}

		private Block NewChapterBlock(int chapterNum)
		{
			var block = new Block("c", chapterNum);
			block.IsParagraphStart = true;
			block.BlockElements.Add(new ScriptText(chapterNum.ToString()));
			block.IsParagraphStart = true;
			m_curSetupChapter = chapterNum;
			m_curSetupVerse = 0;
			m_curSetupVerseEnd = 0;
			m_curStyleTag = null;
			return block;
		}

		private Block NewSingleVersePara(int verseNum, string text = null)
		{
			m_curSetupVerse = verseNum;
			m_curSetupVerseEnd = 0;
			var block = new Block("p", m_curSetupChapter, verseNum).AddVerse(verseNum, text);
			block.IsParagraphStart = true;
			m_curStyleTag = "p";
			return block;
		}

		private Block NewVerseBridgePara(int verseNumStart, int verseNumEnd, string text = null)
		{
			var block = new Block("p", m_curSetupChapter, verseNumStart, verseNumEnd).AddVerse(
				string.Format("{0}-{1}", verseNumStart, verseNumEnd), text);
			block.IsParagraphStart = true;
			m_curStyleTag = "p";
			m_curSetupVerse = verseNumStart;
			m_curSetupVerseEnd = verseNumEnd;
			return block;
		}

		private Block NewPara(string styleTag, string text)
		{
			m_curStyleTag = styleTag;
			var block = NewBlock(text);
			block.IsParagraphStart = true;
			return block;
		}

		private Block NewBlock(string text)
		{
			Debug.Assert(m_curStyleTag != null);
			var block = new Block(m_curStyleTag, m_curSetupChapter, m_curSetupVerse, m_curSetupVerseEnd);
			block.BlockElements.Add(new ScriptText(text));
			return block;
		}

		private BookScript CreateStandardMarkScript(bool includeExtraVersesInChapter1 = false, bool includeBookTitle = false)
		{
			return new BookScript("MRK", GetStandardMarkScriptBlocks(includeExtraVersesInChapter1, includeBookTitle));
		}

		private IList<Block> GetStandardMarkScriptBlocks(bool includeExtraVersesInChapter1 = false, bool includeBookTitle = false)
		{
			m_curSetupChapter = 1;
			m_curSetupVerse = 0;
			m_curSetupVerseEnd = 0;
			m_curStyleTag = null;

			int i = 0;
			var mrkBlocks = new List<Block>();

			if (includeBookTitle)
			{
				mrkBlocks.Add(NewTitleBlock("Gospel According to Mark"));
				mrkBlocks[i++].SetStandardCharacter("MRK", CharacterVerseData.StandardCharacter.BookOrChapter);
			}

			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks[i++].SetStandardCharacter("MRK", CharacterVerseData.StandardCharacter.BookOrChapter);
			mrkBlocks.Add(NewPara("s", "Predicación de Juan el Bautista"));
			mrkBlocks[i++].SetStandardCharacter("MRK", CharacterVerseData.StandardCharacter.ExtraBiblical);
			mrkBlocks.Add(NewSingleVersePara(1, "Principio del evangelio de Jesucristo, el Hijo de Dios. ")
				.AddVerse(2, "Como está escrito en el profeta Isaías:"));
			m_curSetupVerse = 2;
			mrkBlocks[i++].SetStandardCharacter("MRK", CharacterVerseData.StandardCharacter.Narrator);
			mrkBlocks.Add(NewPara("q1", "«Yo envío a mi mensajero delante de ti, El cual preparará tu camino.")
				.AddVerse(3, "Una voz clama en el desierto: “Preparen el camino del Señor; Enderecen sus sendas.”»"));
			m_curSetupVerse = 3;
			mrkBlocks[i++].SetCharacterAndDelivery(new CharacterVerse[0]);
			mrkBlocks.Add(NewSingleVersePara(4, "Juan se presentó en el desierto, y bautizaba y proclamaba el bautismo de arrepentimiento para el perdón de pecados. ")
				.AddVerse(5, "Toda la gente de la provincia de Judea y de Jerusalén acudía a él, y allí en el río Jordán confesaban sus pecados, y Juan los bautizaba."));
			m_curSetupVerse = 5;
			mrkBlocks[i++].SetStandardCharacter("MRK", CharacterVerseData.StandardCharacter.Narrator);

			if (includeExtraVersesInChapter1)
			{
				mrkBlocks[i - 1].AddVerse(6, "La ropa de Juan era de pelo de camello, alrededor de la cintura llevaba un cinto de cuero, y se alimentaba de langostas y miel silvestre. ")
					.AddVerse(7, "Al predicar, Juan decía: ");

				m_curSetupVerse = 7;

				mrkBlocks.Add(NewBlock(
						"«Después de mí viene uno más poderoso que yo. ¡Yo no soy digno de inclinarme ante él para desatarle la correa de su calzado! ")
						.AddVerse(8, "A ustedes yo los he bautizado con agua, pero él los bautizará con el Espíritu Santo.»"));
				m_curSetupVerse = 8;
				mrkBlocks[i++].SetCharacterAndDelivery(new CharacterVerse[0]);
			}

			mrkBlocks.Add(NewPara("s", "El bautismo de Jesús"));
			mrkBlocks[i++].SetStandardCharacter("MRK", CharacterVerseData.StandardCharacter.ExtraBiblical);
			mrkBlocks.Add(NewSingleVersePara(9, "Por esos días llegó Jesús desde Nazaret de Galilea, y fue bautizado por Juan en el Jordán. ")
				.AddVerse(10, "En cuanto Jesús salió del agua, vio que los cielos se abrían y que el Espíritu descendía sobre él como una paloma. ")
				.AddVerse(11, "Y desde los cielos se oyó una voz que decía: "));
			m_curSetupVerse = 11;
			mrkBlocks[i++].SetStandardCharacter("MRK", CharacterVerseData.StandardCharacter.Narrator);
			mrkBlocks.Add(NewBlock("«Tú eres mi Hijo amado, en quien me complazco.»"));
			mrkBlocks[i++].SetCharacterAndDelivery(new CharacterVerse[0]);

			if (includeExtraVersesInChapter1)
			{
				mrkBlocks.Add(NewSingleVersePara(14,
					"Después de que Juan fue encarcelado, Jesús fue a Galilea para proclamar el evangelio del reino de Dios.")
					.AddVerse(15, "Decía: "));
				m_curSetupVerse = 15;
				mrkBlocks[i++].SetStandardCharacter("MRK", CharacterVerseData.StandardCharacter.Narrator);
				mrkBlocks.Add(NewBlock("«El tiempo se ha cumplido, y el reino de Dios se ha acercado. ¡Arrepiéntanse, y crean en el evangelio!»"));
				mrkBlocks[i++].SetCharacterAndDelivery(new CharacterVerse[0]);
			}

			return mrkBlocks;
		}
		#endregion

		#region Serialization/Deserialization Tests
		[Test]
		public void Roundtrip_HasUnappliedSplits_DataIsTheSame()
		{
			var bookScript = CreateStandardMarkScript();
			var blockToSplit = bookScript.Blocks.First(b => b.InitialStartVerseNumber > 0);
			var newBlock = bookScript.SplitBlock(blockToSplit, "1", 5);
			var newBookScript = CreateStandardMarkScript();
			newBookScript.UnappliedBlockSplits_DoNotUse.Add(new List<Block> { blockToSplit, newBlock });
			Assert.AreEqual(1, newBookScript.UnappliedSplits.Count);

			var serializedBookScript = XmlSerializationHelper.SerializeToString(newBookScript);

			var deserializedBookScript = XmlSerializationHelper.DeserializeFromString<BookScript>(serializedBookScript);
			Assert.IsTrue(newBookScript.UnappliedSplits.SequenceEqual(deserializedBookScript.UnappliedSplits, new BlockListComparer()));
		}

		public class BlockListComparer : IEqualityComparer<IEnumerable<Block>>
		{
			public bool Equals(IEnumerable<Block> x, IEnumerable<Block> y)
			{
				if (x == null && y == null)
					return true;
				if (x == null || y == null)
					return false;

				return x.SequenceEqual(y, new BlockComparer());
			}

			public int GetHashCode(IEnumerable<Block> obj)
			{
				return obj.GetHashCode();
			}
		}
		#endregion

		[Test]
		public void Clone_AllMembersAndAutoPropertiesDeepCopied()
		{
			var blocks = GetStandardMarkScriptBlocks(true, true);
			var i = blocks.Count;
			blocks.Add(NewChapterBlock(2));
			blocks[i++].SetStandardCharacter("MRK", CharacterVerseData.StandardCharacter.BookOrChapter);
			blocks.Add(NewPara("s", "Predicación de Juan el Bautista (chapter 2)"));
			blocks[i++].SetStandardCharacter("MRK", CharacterVerseData.StandardCharacter.ExtraBiblical);
			blocks.Add(NewSingleVersePara(1, "Principio del evangelio de Jesucristo, el Hijo de Dios. (chapter 2) ")
				.AddVerse(2, "Como está escrito en el profeta Isaías: (chapter 2) "));
			var orig = new BookScript("MRK", blocks);

			orig.MainTitle = "Main Title";
			orig.PageHeader = "Page Header";
			orig.SingleVoice = true;

			var block1 = new Block("m", 3, 2).AddVerse("2", "Verse 2 text.");
			var block2 = new Block("m", 3, 3).AddVerse("3", "Verse 3 text.");
			orig.UnappliedBlockSplits_DoNotUse.Add(new List<Block> { block1, block2 });
			var origMark1_4Blocks = orig.GetBlocksForVerse(1, 4).ToList(); // Populates orig.m_chapterStartBlockIndices
			var origMark2_1Blocks = orig.GetBlocksForVerse(2, 1).ToList();

			var clone = orig.Clone(true);
			Assert.AreEqual("Main Title", clone.MainTitle);
			Assert.AreEqual("Page Header", clone.PageHeader);
			Assert.AreEqual("MRK", clone.BookId);
			Assert.IsTrue(clone.SingleVoice);
			Assert.IsTrue(orig.Blocks.Select(b => b.GetText(true)).SequenceEqual(clone.Blocks.Select(b => b.GetText(true))));
			Assert.IsFalse(orig.Blocks.SequenceEqual(clone.Blocks));
			Assert.AreEqual(1, clone.UnappliedSplits.Count);
			Assert.IsTrue(orig.UnappliedSplits[0].Select(b => b.GetText(true)).SequenceEqual(clone.UnappliedSplits[0].Select(b => b.GetText(true))));
			Assert.IsFalse(orig.UnappliedSplits.SequenceEqual(clone.UnappliedSplits));
			Assert.IsFalse(orig.UnappliedSplits[0].SequenceEqual(clone.UnappliedSplits[0]));

			var clonedMark1_4Blocks = clone.GetBlocksForVerse(1, 4);
			Assert.IsTrue(origMark1_4Blocks.Select(b => b.GetText(true)).SequenceEqual(orig.GetBlocksForVerse(1, 4).Select(b => b.GetText(true))));
			clone.SplitBlock(clonedMark1_4Blocks.First(), "4", BookScript.kSplitAtEndOfVerse, false);
			Assert.IsTrue(origMark1_4Blocks.SequenceEqual(orig.GetBlocksForVerse(1, 5)));
			Assert.IsTrue(origMark2_1Blocks.SequenceEqual(orig.GetBlocksForVerse(2, 1)));
			Assert.IsFalse(origMark1_4Blocks.Select(b => b.GetText(true)).SequenceEqual(clone.GetBlocksForVerse(1, 4).Select(b => b.GetText(true))));
			Assert.IsTrue(origMark2_1Blocks.Select(b => b.GetText(true)).SequenceEqual(clone.GetBlocksForVerse(2, 1).Select(b => b.GetText(true))));
		}
	}

	internal static class BlockTestExtensions
	{
		static readonly Random Random = new Random(42);

		internal static Block AddVerse(this Block block, int verseNum, string text = null)
		{
			return block.AddVerse(verseNum.ToString(), text);
		}

		internal static Block AddVerse(this Block block, string verse, string text = null)
		{
			if (text == null)
				text = RandomString();
			block.BlockElements.Add(new Verse(verse));
			block.BlockElements.Add(new ScriptText(text));
			return block;
		}

		internal static string RandomString()
		{
			var chars = " AAAAABB CCDDD EEEEFF GGHHIIJK LLMMNNN OOPPP QRRRS SSTTTTU VWWXYYZ aaaaaabb cccddd eeeeefff gggghhh iiiiijjk llll mmmnnnn ooooo pppp qqrrrr sssss tttttuu vvwwwxyyz ,,,.... !?? AAAAABB CCDDD EEEEFF GGHHIIJK LLMMNNN OOPPP QRRRS SSTTTTU VWWXYYZ aaaaaabb cccddd eeeeefff gggghhh iiiiijjk llll mmmnnnn ooooo pppp qqrrrr sssss tttttuu vvwwwxyyz ,,,.... !??\u2014";
			var randomString = new StringBuilder();
			var length = 4 + Random.Next(80);

			for (int i = 0; i < length; i++)
				randomString.Append(chars[Random.Next(chars.Length)]);

			return randomString.ToString();
		}
	}
}
