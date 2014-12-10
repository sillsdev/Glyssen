using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ProtoScript;

namespace ProtoScriptTests
{
	[TestFixture]
	class BookScriptTests
	{
		private int m_curSetupChapter = 1;
		private int m_curSetupVerse = 0;
		private string m_curStyleTag = null;

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
			Assert.AreEqual("[3]This is it!", block.GetText(true));
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
			Assert.IsTrue(block.GetText(true).StartsWith("[2]This is it![3]"));
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
			Assert.IsTrue(block.GetText(true).Contains("[4]This is it![5]"));
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
			Assert.IsTrue(block.GetText(true).Contains("[5]This is it![6]"));
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
			Assert.IsTrue(block.GetText(true).EndsWith("[4-6]This is it!"));
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
			Assert.IsTrue(firstBlockForVerse1_3.GetText(true).EndsWith("[3]This is it!"));
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
			block.SetStandardCharacter("MRK", Block.StandardCharacter.Narrator);
			mrkBlocks.Add(block);

			block = NewBlock(" «Sons of Thunder»");
			block.SetStandardCharacter("MRK", Block.StandardCharacter.Narrator);
			mrkBlocks.Add(block);

			block = NewBlock(" the rest of verse 2. ");
			block.SetStandardCharacter("MRK", Block.StandardCharacter.Narrator);
			mrkBlocks.Add(block.AddVerse(3));

			block = NewSingleVersePara(4).AddVerse(5);
			block.SetStandardCharacter("MRK", Block.StandardCharacter.Narrator);
			mrkBlocks.Add(block);

			var bookScript = new BookScript("MRK", mrkBlocks);
			var result = bookScript.GetScriptBlocks(true);
			Assert.IsTrue(result[1].CharacterIs(Block.StandardCharacter.Narrator));
			Assert.IsTrue(result[1].IsParagraphStart);
			var textOfFirstVerseBlock = result[1].GetText(true);
			Assert.IsTrue(textOfFirstVerseBlock.StartsWith("[1]"));
			Assert.IsTrue(textOfFirstVerseBlock.Contains("[2]"));
			Assert.IsTrue(textOfFirstVerseBlock.Contains(" «Sons of Thunder» the rest of verse 2. [3]"));
			Assert.IsTrue(result[2].CharacterIs(Block.StandardCharacter.Narrator));
			Assert.IsTrue(result[2].IsParagraphStart);
			Assert.IsTrue(result[2].GetText(true).StartsWith("[4]"));
		}
		#endregion

		#region ApplyUserDecisions Tests
		[Test]
		public void ApplyUserDecisions_NoUserDecisionsInSource_NoChangesInTarget()
		{
			var source = CreateStandardMarkScript();
			var target = CreateStandardMarkScript();
			target.ApplyUserDecisions(source);
			Assert.IsFalse(target.GetScriptBlocks().Any(b => b.UserConfirmed));
			Assert.IsTrue(target.GetScriptBlocks().All(b => b.CharacterIsStandard || b.CharacterIs(Block.UnknownCharacter)));
		}

		[Test]
		public void ApplyUserDecisions_UserDecisionsInSourceVerseNumbersAndTextMatchExactly_DecisionsGetCopiedToTarget()
		{
			var source = CreateStandardMarkScript();
			var userConfirmedCharacterBlockIndices = new List<int>();
			for (int i = 0; i < source.GetScriptBlocks().Count; i++)
			{
				if (source[i].CharacterIs(Block.UnknownCharacter))
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
				if (source[i].CharacterIs(Block.UnknownCharacter))
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
						Assert.IsTrue(target[i].CharacterIs(Block.UnknownCharacter));
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
		public void ApplyUserDecisions_UserDecisionsInSourceSomeExtraVerses_DecisionsGetCopiedToTargetOnlyForExactMatches()
		{
			var source = CreateStandardMarkScript();
			var userConfirmedCharacterBlockIndices = new List<int>();
			for (int i = 0; i < source.GetScriptBlocks().Count; i++)
			{
				if (source[i].CharacterIs(Block.UnknownCharacter))
				{
					source[i].CharacterId = "Fred the Frog";
					source[i].Delivery = "with certitude";
					source[i].UserConfirmed = true;
					userConfirmedCharacterBlockIndices.Add(i);
				}
			}
			Assert.IsTrue(userConfirmedCharacterBlockIndices.Count > 0);
			var targetBlocks = StandardMarkScriptBlocks();
			var iBlockAtVerse4 = targetBlocks.FindIndex(b => b.InitialVerseNumber == 4);
			targetBlocks[iBlockAtVerse4]
				.AddVerse(6, "La ropa de Juan era de pelo de camello, alrededor de la cintura llevaba un cinto de cuero, y se alimentaba de langostas y miel silvestre. ")
				.AddVerse(7, "Al predicar, Juan decía: ");
			targetBlocks.Insert(iBlockAtVerse4 + 1,
				NewBlock("«Después de mí viene uno más poderoso que yo. ¡Yo no soy digno de inclinarme ante él para desatarle la correa de su calzado! ")
				.AddVerse(8, "A ustedes yo los he bautizado con agua, pero él los bautizará con el Espíritu Santo.»"));
			targetBlocks[iBlockAtVerse4 + 1].SetCharacterAndDelivery(new CharacterVerse[0]);
			targetBlocks.Add(NewSingleVersePara(14, "Después de que Juan fue encarcelado, Jesús fue a Galilea para proclamar el evangelio del reino de Dios.")
				.AddVerse(15, "Decía: "));
			targetBlocks.Last().SetStandardCharacter("MRK", Block.StandardCharacter.Narrator);
			targetBlocks.Add(NewBlock("«El tiempo se ha cumplido, y el reino de Dios se ha acercado. ¡Arrepiéntanse, y crean en el evangelio!»"));
			targetBlocks.Last().SetCharacterAndDelivery(new CharacterVerse[0]);

			var target = new BookScript("MRK", targetBlocks);
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
					if (i == iBlockAtVerse4 + 1 || i == target.GetScriptBlocks().Count - 1)
						Assert.IsTrue(target[i].CharacterIs(Block.UnknownCharacter));
					else
						Assert.IsTrue(target[i].CharacterIsStandard);
					Assert.IsNull(target[i].Delivery);
					Assert.IsFalse(target[i].UserConfirmed);
				}
			}
		}
		#endregion

		#region Private Helper methods
		private Block NewChapterBlock(int chapterNum)
		{
			var block = new Block("c", chapterNum);
			block.BlockElements.Add(new ScriptText(chapterNum.ToString()));
			block.IsParagraphStart = true;
			m_curSetupChapter = chapterNum;
			m_curStyleTag = null;
			return block;
		}

		private Block NewSingleVersePara(int verseNum, string text = null)
		{
			m_curSetupVerse = verseNum;
			var block = new Block("p", m_curSetupChapter, verseNum).AddVerse(verseNum, text);
			block.IsParagraphStart = true;
			m_curStyleTag = "p";
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
			var block = new Block(m_curStyleTag, m_curSetupChapter, m_curSetupVerse);
			block.BlockElements.Add(new ScriptText(text));
			return block;
		}

		private BookScript CreateStandardMarkScript()
		{
			return new BookScript("MRK", StandardMarkScriptBlocks());
		}

		private List<Block> StandardMarkScriptBlocks()
		{
			int i = 0;
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks[i++].SetStandardCharacter("MRK", Block.StandardCharacter.BookOrChapter);
			mrkBlocks.Add(NewPara("s", "Predicación de Juan el Bautista"));
			mrkBlocks[i++].SetStandardCharacter("MRK", Block.StandardCharacter.ExtraBiblical);
			mrkBlocks.Add(NewSingleVersePara(1, "Principio del evangelio de Jesucristo, el Hijo de Dios. ")
				.AddVerse(2, "Como está escrito en el profeta Isaías:"));
			mrkBlocks[i++].SetStandardCharacter("MRK", Block.StandardCharacter.Narrator);
			mrkBlocks.Add(NewPara("q1", "«Yo envío a mi mensajero delante de ti, El cual preparará tu camino.")
				.AddVerse(3, "Una voz clama en el desierto: “Preparen el camino del Señor; Enderecen sus sendas.”»"));
			mrkBlocks[i++].SetCharacterAndDelivery(new CharacterVerse[0]);
			mrkBlocks.Add(NewSingleVersePara(4, "Juan se presentó en el desierto, y bautizaba y proclamaba el bautismo de arrepentimiento para el perdón de pecados. ")
				.AddVerse(5, "Toda la gente de la provincia de Judea y de Jerusalén acudía a él, y allí en el río Jordán confesaban sus pecados, y Juan los bautizaba."));
			mrkBlocks[i++].SetCharacterAndDelivery(new CharacterVerse[0]);
			mrkBlocks.Add(NewPara("s", "El bautismo de Jesús"));
			mrkBlocks[i++].SetStandardCharacter("MRK", Block.StandardCharacter.ExtraBiblical);
			mrkBlocks.Add(NewSingleVersePara(9, "Por esos días llegó Jesús desde Nazaret de Galilea, y fue bautizado por Juan en el Jordán. ")
				.AddVerse(10, "En cuanto Jesús salió del agua, vio que los cielos se abrían y que el Espíritu descendía sobre él como una paloma. ")
				.AddVerse(11, "Y desde los cielos se oyó una voz que decía: "));
			mrkBlocks[i++].SetStandardCharacter("MRK", Block.StandardCharacter.Narrator);
			mrkBlocks.Add(NewBlock("«Tú eres mi Hijo amado, en quien me complazco.»"));
			mrkBlocks[i].SetCharacterAndDelivery(new CharacterVerse[0]);
			return mrkBlocks;
		}
		#endregion
	}

	internal static class BlockTestExtensions
	{
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

		// ENHANCE: Consider moving this to Palaso test utilities
		internal static string RandomString()
		{
			var chars = " ABCDEF GHIJK LMNOP QRSTU VWXYZ abcdef ghijk lmnop qrstu vwxyz ,.!?";
			var randomString = new StringBuilder();
			var random = new Random();
			var length = random.Next(80);

			for (int i = 0; i < length; i++)
				randomString.Append(chars[random.Next(chars.Length)]);

			return randomString.ToString();
		}
	}
}
