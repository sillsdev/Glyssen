using System;
using Glyssen.Shared;
using GlyssenEngine;
using GlyssenEngine.Character;
using GlyssenEngine.Utilities;
using NUnit.Framework;
using SplitBlockViewModel = GlyssenEngine.ViewModels.SplitBlockViewModel<object>;

namespace GlyssenEngineTests.ViewModelTests
{
	[TestFixture]
	class SplitBlockViewModelTests
	{
		private class TestFont : IFontInfo<object>
		{
			public bool RightToLeftScript => false;
			public string FontFamily => "Charis SIL";
			public int Size => 12;
			public object Font => new object();
		}

		[Test]
		public void GetSplitTextAsHtml_OffsetTooHigh_ThrowsIndexOutOfRangeException()
		{
			var block = new Block("p", 4, 3);
			block.BlockElements.Add(new ScriptText("Text"));
			var model = new SplitBlockViewModel(new TestFont(), new [] {block}, new ICharacter[0], "MAT");
			Assert.Throws<IndexOutOfRangeException>(
				() => model.GetSplitTextAsHtml(block, 0, false, new[] {new BlockSplitData(1, block, "3", 5)}));
		}

		[TestCase("[", "]")]
		[TestCase("{", "}")]
		public void GetSplitTextAsHtml_BlockSplitProvided_InsertsBlockSplit(string open, string close)
		{
			var block = new Block("p", 4, 3);
			block.BlockElements.Add(new ScriptText("Text of verse three, part two " + open + "2" + close + ". "));
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Text of vers " + open + "sic" + close + " four. "));
			block.BlockElements.Add(new Verse("5"));
			block.BlockElements.Add(new ScriptText("Text of verse five."));
			var model = new SplitBlockViewModel(new TestFont(), new[] { block }, new ICharacter[0], "MAT");

			var expected = SplitBlockViewModel.BuildSplitLineHtml(1);
			var actual = model.GetSplitTextAsHtml(block, 0, false, new[] { new BlockSplitData(1, block, "4", 5) });

			Assert.IsTrue(actual.Contains(expected), string.Format("The output string did not contain: {0}", expected));
		}

		[TestCase("[", "]")]
		[TestCase("{", "}")]
		public void GetSplitTextAsHtml_MultipleBlockSplitsProvided_InsertsBlockSplits(string open, string close)
		{
			var block = new Block("p", 4, 3);
			block.BlockElements.Add(new ScriptText("Text of verse three, part two " + open + "2" + close + ". "));
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Text of vers " + open + "sic" + close + " four. "));
			block.BlockElements.Add(new Verse("5"));
			block.BlockElements.Add(new ScriptText("Text of verse five."));

			var expected = "<div class=\"splittext\" data-blockid=\"0\" data-verse=\"3\">Text of verse three, part two " + open + "2" + close + ". </div>" +
				"<div class=\"splittext\" data-blockid=\"0\" data-verse=\"4\"><sup>4&#160;</sup>Text </div>" +
				SplitBlockViewModel.BuildSplitLineHtml(1) +
				"<div class=\"splittext\" data-blockid=\"0\" data-verse=\"4\">of </div>" +
				SplitBlockViewModel.BuildSplitLineHtml(2) +
				"<div class=\"splittext\" data-blockid=\"0\" data-verse=\"4\">vers " + open + "sic" + close + " </div>" +
				SplitBlockViewModel.BuildSplitLineHtml(3) +
				"<div class=\"splittext\" data-blockid=\"0\" data-verse=\"4\">four. </div>" +
				"<div class=\"splittext\" data-blockid=\"0\" data-verse=\"5\"><sup>5&#160;</sup>Text</div>" +
				SplitBlockViewModel.BuildSplitLineHtml(4) +
				"<div class=\"splittext\" data-blockid=\"0\" data-verse=\"5\"> of verse five.</div>";

			var model = new SplitBlockViewModel(new TestFont(), new[] { block }, new ICharacter[0], "MAT");
			var actual = model.GetSplitTextAsHtml(block, 0, false, new[]
			{
				new BlockSplitData(1, block, "4", 5),
				new BlockSplitData(2, block, "4", 8),
				new BlockSplitData(3, block, "4", 19),
				new BlockSplitData(4, block, "5", 4)
			});
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void GetSplitTextAsHtml_MultipleBlockSplitsProvided_RealData_InsertsBlockSplits()
		{
			var block = new Block("p", 4, 3);
			block.BlockElements.Add(new ScriptText("—Ananías, ¿Ibiga nia-saila-Satanás burba-isgana begi oubononiki? Emide, Bab-Dummad-Burba-Isligwaledga be gakansanonigu, mani-abala be susgu. "));
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Yoo be nainu-ukegu, ¿nainu begadinsursi? Nainu be uksagu, ¿a-manide begadinsursi? ¿Ibiga be-gwagegi anmarga gakansaedgi be binsanoniki? Dulemarga be gakan-imaksasulid, Bab-Dummadga be gakan-imaksad."));

			var expected = "<div class=\"splittext\" data-blockid=\"0\" data-verse=\"3\">—Anan&#237;as, &#191;Ibiga nia-saila-Satan&#225;s burba-isgana begi oubononiki? Emide, Bab-Dummad-Burba-Isligwaledga be gakans</div>" +
				SplitBlockViewModel.BuildSplitLineHtml(1) +
				"<div class=\"splittext\" data-blockid=\"0\" data-verse=\"3\">anonigu, mani-abala be susgu. </div>" +
				"<div class=\"splittext\" data-blockid=\"0\" data-verse=\"4\"><sup>4&#160;</sup>Yoo be nainu-ukegu, &#191;nainu begadin</div>" +
				SplitBlockViewModel.BuildSplitLineHtml(2) +
				"<div class=\"splittext\" data-blockid=\"0\" data-verse=\"4\">sursi? </div>" +
				SplitBlockViewModel.BuildSplitLineHtml(4) +
				"<div class=\"splittext\" data-blockid=\"0\" data-verse=\"4\">Nainu be uksagu, &#191;a-manide begadinsursi? &#191;Ibiga be-gwagegi anmarga gakan</div>" +
				SplitBlockViewModel.BuildSplitLineHtml(3) +
				"<div class=\"splittext\" data-blockid=\"0\" data-verse=\"4\">saedgi be binsanoniki? Dulemarga be gakan-imaksasulid, Bab-Dummadga be gakan-imaksad.</div>";

			var model = new SplitBlockViewModel(new TestFont(), new[] { block }, new ICharacter[0], "ACT");
			var actual = model.GetSplitTextAsHtml(block, 0, false, new[]
			{
				new BlockSplitData(1, block, "3", 111),
				new BlockSplitData(2, block, "4", 34),
				new BlockSplitData(3, block, "4", 113),
				new BlockSplitData(4, block, "4", 41)
			});
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void GetSplitTextAsHtml_SpecialCharactersInText_InsertsInCorrectLocation()
		{
			var block = new Block("p", 4, 3);
			block.BlockElements.Add(new ScriptText("Нылыс эз кув, сiйö узьö"));

			var expected = "<div class=\"splittext\" data-blockid=\"0\" data-verse=\"3\">Нылыс эз кув, сiй&#246; </div>" +
				SplitBlockViewModel.BuildSplitLineHtml(1) +
				"<div class=\"splittext\" data-blockid=\"0\" data-verse=\"3\">узь&#246;</div>";

			var model = new SplitBlockViewModel(new TestFont(), new[] { block }, new ICharacter[0], "ACT");
			var actual = model.GetSplitTextAsHtml(block, 0, false, new[] { new BlockSplitData(1, block, "3", 19) });

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void GetSplitTextAsHtml_SpecialCharactersInTextWithSplitJustBeforeVerseNumber_InsertsInCorrectLocation()
		{
			var block = new Block("p", 4, 3);
			block.BlockElements.Add(new ScriptText("Нылыс эз кув, сiйö узьö"));
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Нылыс эз кув, сiйö узьö"));

			var expected = "<div class=\"splittext\" data-blockid=\"0\" data-verse=\"3\">Ны</div>" +
				SplitBlockViewModel.BuildSplitLineHtml(1) +
				"<div class=\"splittext\" data-blockid=\"0\" data-verse=\"3\">лыс эз кув, сiй&#246; узь&#246;</div>" +
				SplitBlockViewModel.BuildSplitLineHtml(2) +
				"<div class=\"splittext\" data-blockid=\"0\" data-verse=\"3\"></div>" +
				"<div class=\"splittext\" data-blockid=\"0\" data-verse=\"4\"><sup>4&#160;</sup>Нылыс эз кув, сiй&#246; узь&#246;</div>";

			var model = new SplitBlockViewModel(new TestFont(), new[] { block }, new ICharacter[0], "ACT");
			var actual = model.GetSplitTextAsHtml(block, 0, false, new[]
			{
				new BlockSplitData(2, block, "3", BookScript.kSplitAtEndOfVerse),
				new BlockSplitData(1, block, "3", 2),
			});

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void GetSplitTextAsHtml_ExpectedSpecialCharacters_InsertsInCorrectLocation()
		{
			var block = new Block("p", 4, 3);
			block.BlockElements.Add(new ScriptText("A & <<B>> C"));
			var expected = "<div class=\"splittext\" data-blockid=\"0\" data-verse=\"3\">A &amp; &lt;&lt;B&gt;&gt; </div>" + SplitBlockViewModel.BuildSplitLineHtml(1) + "<div class=\"splittext\" data-blockid=\"0\" data-verse=\"3\">C</div>";
			var model = new SplitBlockViewModel(new TestFont(), new[] { block }, new ICharacter[0], "ACT");
			var actual = model.GetSplitTextAsHtml(block, 0, false, new[] { new BlockSplitData(1, block, "3", 10) });
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void GetSplitTextAsHtml_HasLeadingPunctuation_MultipleSplitsAndMultipleVerses_SplitsInCorrectLocations()
		{
			var block = new Block("p", 4, 3);
			block.BlockElements.Add(new ScriptText("("));
			block.BlockElements.Add(new Verse("3"));
			block.BlockElements.Add(new ScriptText("main text.) "));
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Verse 4."));

			var expected = "<div class=\"splittext\" data-blockid=\"0\" data-verse=\"3\"><span class=\"leading-punctuation\">(</span><sup>3&#160;</sup>main</div>" + SplitBlockViewModel.BuildSplitLineHtml(1) + "<div class=\"splittext\" data-blockid=\"0\" data-verse=\"3\"> text.) </div>" +
				"<div class=\"splittext\" data-blockid=\"0\" data-verse=\"4\"><sup>4&#160;</sup>Ver</div>" + SplitBlockViewModel.BuildSplitLineHtml(2) + "<div class=\"splittext\" data-blockid=\"0\" data-verse=\"4\">se 4.</div>";
			var model = new SplitBlockViewModel(new TestFont(), new[] { block }, new ICharacter[0], "ACT");
			var actual = model.GetSplitTextAsHtml(block, 0, false, new[]
			{
				new BlockSplitData(1, block, "3", 4),
				new BlockSplitData(2, block, "4", 3)
			});
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void GetSplitTextAsHtml_HasLeadingPunctuation_SplitAtEndOfVerse_SplitsInCorrectLocations()
		{
			var block = new Block("p", 4, 3);
			block.BlockElements.Add(new ScriptText("("));
			block.BlockElements.Add(new Verse("3"));
			block.BlockElements.Add(new ScriptText("main text.) "));
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Verse 4."));

			var expected = "<div class=\"splittext\" data-blockid=\"0\" data-verse=\"3\"><span class=\"leading-punctuation\">(</span><sup>3&#160;</sup>main text.) </div>" + SplitBlockViewModel.BuildSplitLineHtml(1) +
				"<div class=\"splittext\" data-blockid=\"0\" data-verse=\"3\"></div>" +
				"<div class=\"splittext\" data-blockid=\"0\" data-verse=\"4\"><sup>4&#160;</sup>Ver</div>" + SplitBlockViewModel.BuildSplitLineHtml(2) + "<div class=\"splittext\" data-blockid=\"0\" data-verse=\"4\">se 4.</div>";
			var model = new SplitBlockViewModel(new TestFont(), new[] { block }, new ICharacter[0], "ACT");
			var actual = model.GetSplitTextAsHtml(block, 0, false, new[]
			{
				new BlockSplitData(1, block, "3", PortionScript.kSplitAtEndOfVerse),
				new BlockSplitData(2, block, "4", 3)
			});
			Assert.AreEqual(expected, actual);
		}

		[TestCase(0, "", "main text.) ")]
		[TestCase(1, "m", "ain text.) ")]
		[TestCase(3, "mai", "n text.) ")]
		public void GetSplitTextAsHtml_HasLeadingPunctuation_SplitsInCorrectLocation(int offset, string text1, string text2)
		{

			var block = new Block("p", 4, 3);
			block.BlockElements.Add(new ScriptText("("));
			block.BlockElements.Add(new Verse("3"));
			block.BlockElements.Add(new ScriptText("main text.) "));

			var expected = $"<div class=\"splittext\" data-blockid=\"0\" data-verse=\"3\"><span class=\"leading-punctuation\">(</span><sup>3&#160;</sup>{text1}</div>" +
				SplitBlockViewModel.BuildSplitLineHtml(1) +
				$"<div class=\"splittext\" data-blockid=\"0\" data-verse=\"3\">{text2}</div>";

			var model = new SplitBlockViewModel(new TestFont(), new[] { block }, new ICharacter[0], "ACT");
			var actual = model.GetSplitTextAsHtml(block, 0, false, new[]
			{
				new BlockSplitData(1, block, "3", offset)
			});
			Assert.AreEqual(expected, actual);
		}
	}
}
