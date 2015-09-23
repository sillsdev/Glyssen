using Glyssen;
using NUnit.Framework;

namespace GlyssenTests
{
	[TestFixture]
	class ProjectExportTests
	{
		[Test]
		public void GetTabSeparatedLine_GetExportDataForBlock_VerseAndTextElements_ExpectedColumnsIncludingJoinedText()
		{
			var block = new Block("p", 4);
			block.IsParagraphStart = true;
			block.CharacterId = "Fred";
			block.Delivery = "With great gusto and quivering frustration";
			block.BlockElements.Add(new Verse("1"));
			block.BlockElements.Add(new ScriptText("Text of verse one. "));
			block.BlockElements.Add(new Verse("2"));
			block.BlockElements.Add(new ScriptText("Text of verse two."));

			int textLength = "Text of verse one. ".Length + "Text of verse two.".Length;
			Assert.AreEqual("0\tp\tMRK\t4\t1\tFred\tWith great gusto and quivering frustration\t[1]\u00A0Text of verse one. [2]\u00A0Text of verse two.\t" + textLength,
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK")));
			Assert.AreEqual("0\tActorGuy1\tp\tMRK\t4\t1\tFred\tWith great gusto and quivering frustration\t[1]\u00A0Text of verse one. [2]\u00A0Text of verse two.\t" + textLength,
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", "ActorGuy1")));
		}

		[Test]
		public void GetTabSeparatedLine_GetExportDataForBlock_TextBeginsMidVerse_ResultHasCorrectVerseInfo()
		{
			var block = new Block("p", 4, 3);
			block.BlockElements.Add(new ScriptText("Text of verse three, part two. "));
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Text of verse four. "));
			block.BlockElements.Add(new Verse("5"));
			block.BlockElements.Add(new ScriptText("Text of verse five."));

			int textLength = "Text of verse three, part two. ".Length + "Text of verse four. ".Length + "Text of verse five.".Length;
			Assert.AreEqual("0\tp\tMRK\t4\t3\t\t\tText of verse three, part two. [4]\u00A0Text of verse four. [5]\u00A0Text of verse five.\t" + textLength,
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK")));
			Assert.AreEqual("0\tActorGuy1\tp\tMRK\t4\t3\t\t\tText of verse three, part two. [4]\u00A0Text of verse four. [5]\u00A0Text of verse five.\t" + textLength,
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", "ActorGuy1")));
		}

		[Test]
		public void GetTabSeparatedLine_GetExportDataForBlock_SpecifyNarratorCharacter_OutputContainsNarrator()
		{
			var block = new Block("p", 4);
			block.IsParagraphStart = true;
			block.CharacterId = "Fred";
			block.Delivery = "With great gusto and quivering frustration";
			block.BlockElements.Add(new Verse("1"));
			block.BlockElements.Add(new ScriptText("Text of verse one. "));
			block.BlockElements.Add(new Verse("2"));
			block.BlockElements.Add(new ScriptText("Text of verse two."));

			int textLength = "Text of verse one. ".Length + "Text of verse two.".Length;
			Assert.AreEqual("0\tp\tMRK\t4\t1\tnarrator (MRK)\tWith great gusto and quivering frustration\t[1]\u00A0Text of verse one. [2]\u00A0Text of verse two.\t" + textLength,
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", null, "narrator-MRK")));
			Assert.AreEqual("0\tActorGuy1\tp\tMRK\t4\t1\tnarrator (MRK)\tWith great gusto and quivering frustration\t[1]\u00A0Text of verse one. [2]\u00A0Text of verse two.\t" + textLength,
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", "ActorGuy1", "narrator-MRK")));
		}

		[Test]
		public void GetTabSeparatedLine_GetExportDataForBlock_MultiCharacterWithResolvedId_OutputContainsCharacterIdToUseInScript()
		{
			var block = new Block("p", 4);
			block.IsParagraphStart = true;
			block.CharacterId = "Fred/Marko";
			block.CharacterIdInScript = "Marko";
			block.Delivery = "With great gusto and quivering frustration";
			block.BlockElements.Add(new Verse("1"));
			block.BlockElements.Add(new ScriptText("Text of verse one. "));
			block.BlockElements.Add(new Verse("2"));
			block.BlockElements.Add(new ScriptText("Text of verse two."));

			int textLength = "Text of verse one. ".Length + "Text of verse two.".Length;
			Assert.AreEqual("0\tp\tMRK\t4\t1\tMarko\tWith great gusto and quivering frustration\t[1]\u00A0Text of verse one. [2]\u00A0Text of verse two.\t" + textLength,
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK")));
			Assert.AreEqual("0\tActorGuy1\tp\tMRK\t4\t1\tMarko\tWith great gusto and quivering frustration\t[1]\u00A0Text of verse one. [2]\u00A0Text of verse two.\t" + textLength,
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", "ActorGuy1")));
		}

		[Test]
		public void GetTabSeparatedLine_GetExportDataForBlock_UseCharacterIdInScriptFalse_OutputContainsUnresolvedCharacterIds()
		{
			var block = new Block("p", 4);
			block.IsParagraphStart = true;
			block.CharacterId = "Fred/Marko";
			block.CharacterIdInScript = "Marko";
			block.Delivery = "With great gusto and quivering frustration";
			block.BlockElements.Add(new Verse("1"));
			block.BlockElements.Add(new ScriptText("Text of verse one. "));
			block.BlockElements.Add(new Verse("2"));
			block.BlockElements.Add(new ScriptText("Text of verse two."));

			int textLength = "Text of verse one. ".Length + "Text of verse two.".Length;
			Assert.AreEqual("0\tp\tMRK\t4\t1\tFred/Marko\tWith great gusto and quivering frustration\t[1]\u00A0Text of verse one. [2]\u00A0Text of verse two.\t" + textLength,
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", useCharacterIdInScript: false)));
			Assert.AreEqual("0\tActorGuy1\tp\tMRK\t4\t1\tFred/Marko\tWith great gusto and quivering frustration\t[1]\u00A0Text of verse one. [2]\u00A0Text of verse two.\t" + textLength,
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", "ActorGuy1", useCharacterIdInScript: false)));
		}
	}
}
