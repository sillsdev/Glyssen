using System.Collections.Generic;
using System.Linq;
using Glyssen;
using Glyssen.Bundle;
using Glyssen.Character;
using NUnit.Framework;
using SIL.ObjectModel;
using SIL.Windows.Forms;

namespace GlyssenTests
{
	[TestFixture]
	class ProjectExportTests
	{
		[Test]
		public void GetExportData_NoActorsAssigned_ActorColumnNotPresent()
		{
			var project = TestProject.CreateBasicTestProject();
			var metadata = (GlyssenDblTextMetadata)ReflectionHelper.GetField(project, "m_metadata");
			metadata.IncludeChapterAnnouncementForFirstChapter = true;
			metadata.IncludeChapterAnnouncementForSingleChapterBooks = true;
			var exporter = new ProjectExporter(project);
			var data = exporter.GetExportData();
			Assert.True(data.TrueForAll(t => t.Item3.Count == 9));
		}

		[Test]
		public void GetExportData_SomeButNotAllActorsAssigned_AllColumnsPresent()
		{
			var project = TestProject.CreateBasicTestProject();
			var metadata = (GlyssenDblTextMetadata)ReflectionHelper.GetField(project, "m_metadata");
			metadata.IncludeChapterAnnouncementForFirstChapter = true;
			metadata.IncludeChapterAnnouncementForSingleChapterBooks = true;
			project.VoiceActorList.AllActors = new List<Glyssen.VoiceActor.VoiceActor>
			{
				new Glyssen.VoiceActor.VoiceActor { Id = 1 }
			};
			project.CharacterGroupList.CharacterGroups = new BulkObservableList<CharacterGroup>
			{
				new CharacterGroup(project),
				new CharacterGroup(project)
			};
			project.CharacterGroupList.CharacterGroups[0].AssignVoiceActor(1);

			var exporter = new ProjectExporter(project);
			var data = exporter.GetExportData();
			Assert.True(data.TrueForAll(t => t.Item3.Count == 10));
		}

		[Test]
		public void GetExportData_ChapterAnnouncementsUseClStyleTage_SkippingRulesAreAppliedCorrectly()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.EPH, TestProject.TestBook.JUD);
			foreach (var block in project.Books.SelectMany(b => b.Blocks))
			{
				if (block.IsChapterAnnouncement)
					block.StyleTag = "cl";
			}
			var metadata = (GlyssenDblTextMetadata)ReflectionHelper.GetField(project, "m_metadata");
			metadata.IncludeChapterAnnouncementForFirstChapter = true;
			metadata.IncludeChapterAnnouncementForSingleChapterBooks = false;
			var exporter = new ProjectExporter(project);
			var data = exporter.GetExportData();
			var chapterBlockForEphesians = data.Single(t => (string)t.Item3[1] == "cl" && (int)t.Item3[3] == 1);
			Assert.AreEqual("EPH", chapterBlockForEphesians.Item3[2]);
		}

		[Test]
		public void GetExportData_IncludeChapterOne_OutputIncludesChapterAnnouncementForFirstChapter()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.EPH, TestProject.TestBook.JUD);
			var metadata = (GlyssenDblTextMetadata)ReflectionHelper.GetField(project, "m_metadata");
			metadata.IncludeChapterAnnouncementForFirstChapter = true;
			metadata.IncludeChapterAnnouncementForSingleChapterBooks = true;
			var exporter = new ProjectExporter(project);
			var data = exporter.GetExportData();
			Assert.AreEqual(2, data.Count(t => (string)t.Item3[1] == "c" && (int)t.Item3[3] == 1));
		}

		[Test]
		public void GetExportData_SkipChapterOne_OutputDoesNotIncludeChapterAnnouncementForFirstChapter()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.EPH, TestProject.TestBook.JUD);
			var metadata = (GlyssenDblTextMetadata)ReflectionHelper.GetField(project, "m_metadata");
			metadata.IncludeChapterAnnouncementForFirstChapter = false;
			var exporter = new ProjectExporter(project);
			var data = exporter.GetExportData();
			Assert.False(data.Any(t => (string)t.Item3[1] == "c" && (int)t.Item3[3] == 1));
		}

		[Test]
		public void GetExportData_SkipChapterAnnouncementInSingleChapterBooks_OutputDoesNotIncludeChapterAnnouncementForJude()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.EPH, TestProject.TestBook.JUD);
			var metadata = (GlyssenDblTextMetadata)ReflectionHelper.GetField(project, "m_metadata");
			metadata.IncludeChapterAnnouncementForFirstChapter = true;
			metadata.IncludeChapterAnnouncementForSingleChapterBooks = false;
			var exporter = new ProjectExporter(project);
			var data = exporter.GetExportData();
			var chapterBlockForEphesians = data.Single(t => (string)t.Item3[1] == "c" && (int)t.Item3[3] == 1);
			Assert.AreEqual("EPH", chapterBlockForEphesians.Item3[2]);
		}

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

			var actor = new Glyssen.VoiceActor.VoiceActor { Name = "ActorGuy1" };

			int textLength = "Text of verse one. ".Length + "Text of verse two.".Length;
			Assert.AreEqual("0\tp\tMRK\t4\t1\tFred\tWith great gusto and quivering frustration\t[1]\u00A0Text of verse one. [2]\u00A0Text of verse two.\t" + textLength,
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK").Item3));
			Assert.AreEqual("0\tActorGuy1\tp\tMRK\t4\t1\tFred\tWith great gusto and quivering frustration\t[1]\u00A0Text of verse one. [2]\u00A0Text of verse two.\t" + textLength,
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", actor).Item3));
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

			var actor = new Glyssen.VoiceActor.VoiceActor { Name = "ActorGuy1" };

			int textLength = "Text of verse three, part two. ".Length + "Text of verse four. ".Length + "Text of verse five.".Length;
			Assert.AreEqual("0\tp\tMRK\t4\t3\t\t\tText of verse three, part two. [4]\u00A0Text of verse four. [5]\u00A0Text of verse five.\t" + textLength,
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK").Item3));
			Assert.AreEqual("0\tActorGuy1\tp\tMRK\t4\t3\t\t\tText of verse three, part two. [4]\u00A0Text of verse four. [5]\u00A0Text of verse five.\t" + textLength,
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", actor).Item3));
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

			var actor = new Glyssen.VoiceActor.VoiceActor { Name = "ActorGuy1" };

			int textLength = "Text of verse one. ".Length + "Text of verse two.".Length;
			Assert.AreEqual("0\tp\tMRK\t4\t1\tnarrator (MRK)\tWith great gusto and quivering frustration\t[1]\u00A0Text of verse one. [2]\u00A0Text of verse two.\t" + textLength,
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", null, "narrator-MRK").Item3));
			Assert.AreEqual("0\tActorGuy1\tp\tMRK\t4\t1\tnarrator (MRK)\tWith great gusto and quivering frustration\t[1]\u00A0Text of verse one. [2]\u00A0Text of verse two.\t" + textLength,
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", actor, "narrator-MRK").Item3));
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

			var actor = new Glyssen.VoiceActor.VoiceActor { Name = "ActorGuy1" };

			int textLength = "Text of verse one. ".Length + "Text of verse two.".Length;
			Assert.AreEqual("0\tp\tMRK\t4\t1\tMarko\tWith great gusto and quivering frustration\t[1]\u00A0Text of verse one. [2]\u00A0Text of verse two.\t" + textLength,
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK").Item3));
			Assert.AreEqual("0\tActorGuy1\tp\tMRK\t4\t1\tMarko\tWith great gusto and quivering frustration\t[1]\u00A0Text of verse one. [2]\u00A0Text of verse two.\t" + textLength,
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", actor).Item3));
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

			var actor = new Glyssen.VoiceActor.VoiceActor { Name = "ActorGuy1" };

			int textLength = "Text of verse one. ".Length + "Text of verse two.".Length;
			Assert.AreEqual("0\tp\tMRK\t4\t1\tFred/Marko\tWith great gusto and quivering frustration\t[1]\u00A0Text of verse one. [2]\u00A0Text of verse two.\t" + textLength,
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", useCharacterIdInScript: false).Item3));
			Assert.AreEqual("0\tActorGuy1\tp\tMRK\t4\t1\tFred/Marko\tWith great gusto and quivering frustration\t[1]\u00A0Text of verse one. [2]\u00A0Text of verse two.\t" + textLength,
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", actor, useCharacterIdInScript: false).Item3));
		}
	}
}
