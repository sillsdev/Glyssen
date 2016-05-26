using System.Collections.Generic;
using System.Linq;
using Glyssen;
using Glyssen.Bundle;
using Glyssen.Character;
using NUnit.Framework;
using Paratext;
using SIL.Extensions;
using SIL.Windows.Forms;

namespace GlyssenTests
{
	[TestFixture]
	class ProjectExportTests
	{
		const int kBlockId = 0;
		// These constants refer to the columns the Actor column is NOT included
		const int kParaTag = 1;
		const int kBookId = 2;
		const int kChapter = 3;
		const int kVerse = 4;
		const int kCharacterId = 5;
		const int kDelivery = 6;
		const int kVernacularText = 7;
		const int kPrimaryReferenceText = 8;
		const int kSecondaryReferenceText = 9;
		const int kVernacularTextLengthWithNoSecondaryRef = 9;
		const int kVernacularTextLengthWithSecondaryRef = 10;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = null;
			CharacterDetailData.TabDelimitedCharacterDetailData = null;
		}

		[Test]
		public void GetExportData_NoActorsAssigned_ActorColumnNotPresent()
		{
			var project = TestProject.CreateBasicTestProject();
			var metadata = (GlyssenDblTextMetadata)ReflectionHelper.GetField(project, "m_metadata");
			metadata.IncludeChapterAnnouncementForFirstChapter = true;
			metadata.IncludeChapterAnnouncementForSingleChapterBooks = true;
			var exporter = new ProjectExporter(project);
			var data = exporter.GetExportData();
			Assert.True(data.All(t => t.Count == 10));
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
			project.CharacterGroupList.CharacterGroups.AddRange(new []
			{
				new CharacterGroup(project),
				new CharacterGroup(project)
			});
			project.CharacterGroupList.CharacterGroups[0].AssignVoiceActor(1);

			var exporter = new ProjectExporter(project);
			var data = exporter.GetExportData();
			Assert.True(data.All(t => t.Count == 11));
		}

		[Test]
		public void GetExportData_ChapterAnnouncementsUseClStyleTag_SkippingRulesAreAppliedCorrectly()
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
			var chapterBlockForEphesians = data.Single(t => (string)t[1] == "cl" && (int)t[3] == 1);
			Assert.AreEqual("EPH", chapterBlockForEphesians[2]);
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
			Assert.AreEqual(2, data.Count(t => (string)t[1] == "c" && (int)t[3] == 1));
		}

		[Test]
		public void GetExportData_SkipChapterOne_OutputDoesNotIncludeChapterAnnouncementForFirstChapter()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.EPH, TestProject.TestBook.JUD);
			var metadata = (GlyssenDblTextMetadata)ReflectionHelper.GetField(project, "m_metadata");
			metadata.IncludeChapterAnnouncementForFirstChapter = false;
			var exporter = new ProjectExporter(project);
			var data = exporter.GetExportData();
			Assert.False(data.Any(t => (string)t[1] == "c" && (int)t[3] == 1));
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
			var chapterBlockForEphesians = data.Single(t => (string)t[1] == "c" && (int)t[3] == 1);
			Assert.AreEqual("EPH", chapterBlockForEphesians[2]);
		}

		[Test]
		public void GetExportData_SpecifiedBook_OutputOnlyIncludeBlockForThatBook()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.GAL, TestProject.TestBook.IIJN);
			var metadata = (GlyssenDblTextMetadata)ReflectionHelper.GetField(project, "m_metadata");
			metadata.IncludeChapterAnnouncementForFirstChapter = true;
			metadata.IncludeChapterAnnouncementForSingleChapterBooks = true;
			var exporter = new ProjectExporter(project);
			var data = exporter.GetExportData("2JN");
			Assert.True(data.All(t => (string)t[2] == "2JN"));
		}

		[Test]
		public void GetExportData_SpecifiedActor_OutputOnlyIncludeBlockForThatActor()
		{
			var project = TestProject.CreateBasicTestProject();
			var metadata = (GlyssenDblTextMetadata)ReflectionHelper.GetField(project, "m_metadata");
			metadata.IncludeChapterAnnouncementForFirstChapter = true;
			metadata.IncludeChapterAnnouncementForSingleChapterBooks = true;
			project.IncludedBooks[0].SingleVoice = false;
			project.VoiceActorList.AllActors = new List<Glyssen.VoiceActor.VoiceActor>
			{
				new Glyssen.VoiceActor.VoiceActor { Id = 1, Name = "Marlon" }
			};
			project.CharacterGroupList.CharacterGroups.AddRange(new[]
			{
				new CharacterGroup(project),
				new CharacterGroup(project)
			});
			project.CharacterGroupList.CharacterGroups[0].CharacterIds.Add("Michael, archangel");
			project.CharacterGroupList.CharacterGroups[0].AssignVoiceActor(1);

			var exporter = new ProjectExporter(project);
			var data = exporter.GetExportData(voiceActorId: 1).Single();
			Assert.AreEqual("Marlon", (string)data[1]);
			Assert.AreEqual(1, data[4]);
			Assert.AreEqual("9", data[5]);
			Assert.AreEqual("Michael, archangel", (string)data[6]);
		}

		[Test]
		public void GetExportData_BlocksAreJoinedToReferenceText_OutputContainsMatchedAndUnmatchedReferenceText()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.JUD);
			var narrator = CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.Narrator);
			var sectionHead = CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.ExtraBiblical);
			var jude = project.IncludedBooks.Single();
			jude.Blocks = new List<Block>(new[]
				{
					new Block("p", 1, 1) {CharacterId = narrator }.AddVerse("1", "A"),
					new Block("s", 1, 1) {CharacterId = sectionHead, BlockElements = new List<BlockElement> {new ScriptText("Jude complains")}},
					new Block("p", 1, 2) {CharacterId = "Enoch" }.AddVerse("2", "B"),
					new Block("p", 1, 3) {CharacterId = narrator }.AddVerse("3", "C"),
					new Block("p", 1, 4) {CharacterId = "Michael" }.AddVerse("4", "D"),
					new Block("p", 1, 5) {CharacterId = narrator }.AddVerse("5", "E ").AddVerse("6", "F"),
				});

			var primaryReferenceText = ReferenceText.CreateCustomReferenceText(new GlyssenDblTextMetadata());
			ReflectionHelper.SetField(primaryReferenceText, "m_vers", ScrVers.English);
			var books = (List<BookScript>)primaryReferenceText.Books;
			var blocks = new List<Block>
			{
				new Block("p", 1, 1) { CharacterId = narrator }.AddVerse("1", "Ayy"),
				new Block("p", 1, 2, 3) {CharacterId = narrator}.AddVerse("2-3", "Bee Cee"),
				new Block("p", 1, 4) { CharacterId = "Michael" }.AddVerse(4, "Dee, "),
				new Block("p", 1, 4) { CharacterId = narrator, BlockElements = new List<BlockElement> {new ScriptText("Michael said.")}},
				new Block("p", 1, 5) { CharacterId = narrator }.AddVerse(5, "Ey"),
				new Block("p", 1, 6) { CharacterId = narrator }.AddVerse(6, "Ef"),
			};
			var refBook = new BookScript("JUD", blocks);
			books.Add(refBook);
			project.ReferenceText = primaryReferenceText;
			var exporter = new ProjectExporter(project);
			var data = exporter.GetExportData().ToList();

			Assert.IsTrue(data.All(d => (string)d[kBookId] == "JUD" && (int)d[kChapter] == 1));
			var i = 0;
			var row = data[i++];
			Assert.AreEqual(i, row[kBlockId]); // Row 1
			Assert.AreEqual("p", row[kParaTag]);
			Assert.AreEqual("1", row[kVerse]);
			Assert.AreEqual("narrator (JUD)", row[kCharacterId]);
			Assert.AreEqual("[1]\u00A0A", row[kVernacularText]);
			Assert.AreEqual("[1]\u00A0Ayy", row[kPrimaryReferenceText]);

			row = data[i++];
			Assert.AreEqual(i, row[kBlockId]); // Row 2
			Assert.AreEqual("s", row[kParaTag]);
			Assert.AreEqual("1", row[kVerse]);
			Assert.AreEqual("section head (JUD)", row[kCharacterId]);
			Assert.AreEqual("Jude complains", row[kVernacularText]);
			Assert.IsTrue(string.IsNullOrEmpty(row[kPrimaryReferenceText] as string));

			row = data[i++];
			Assert.AreEqual(i, row[kBlockId]); // Row 3
			Assert.AreEqual("p", row[kParaTag]);
			Assert.AreEqual("2", row[kVerse]);
			Assert.AreEqual("Enoch", row[kCharacterId]);
			Assert.AreEqual("[2]\u00A0B", row[kVernacularText]);
			Assert.IsTrue(string.IsNullOrEmpty(row[kPrimaryReferenceText] as string));

			row = data[i++];
			Assert.AreEqual(i, row[kBlockId]); // Row 4
			Assert.AreEqual("p", row[kParaTag]);
			Assert.AreEqual("3", row[kVerse]);
			Assert.AreEqual("narrator (JUD)", row[kCharacterId]);
			Assert.AreEqual("[3]\u00A0C", row[kVernacularText]);
			Assert.IsTrue(string.IsNullOrEmpty(row[kPrimaryReferenceText] as string));

			row = data[i++];
			Assert.IsTrue(string.IsNullOrEmpty(row[kBlockId] as string));
			Assert.AreEqual("p", row[kParaTag]);
			Assert.AreEqual("2-3", row[kVerse]);
			Assert.AreEqual("narrator (JUD)", row[kCharacterId]);
			Assert.IsTrue(string.IsNullOrEmpty(row[kVernacularText] as string));
			Assert.AreEqual("[2-3]\u00A0Bee Cee", row[kPrimaryReferenceText]);
			Assert.AreEqual(0, row[kVernacularTextLengthWithNoSecondaryRef]);

			row = data[i++];
			Assert.AreEqual(5, row[kBlockId]); // Row 5
			Assert.AreEqual("p", row[kParaTag]);
			Assert.AreEqual("4", row[kVerse]);
			Assert.AreEqual("Michael", row[kCharacterId]);
			Assert.AreEqual("[4]\u00A0D", row[kVernacularText]);
			Assert.IsTrue(string.IsNullOrEmpty(row[kPrimaryReferenceText] as string));

			row = data[i++];
			Assert.IsTrue(string.IsNullOrEmpty(row[kBlockId] as string));
			Assert.AreEqual("p", row[kParaTag]);
			Assert.AreEqual("4", row[kVerse]);
			Assert.AreEqual("Michael", row[kCharacterId]);
			Assert.IsTrue(string.IsNullOrEmpty(row[kVernacularText] as string));
			Assert.AreEqual("[4]\u00A0Dee, ", row[kPrimaryReferenceText]);
			Assert.AreEqual(0, row[kVernacularTextLengthWithNoSecondaryRef]);

			row = data[i++];
			Assert.IsTrue(string.IsNullOrEmpty(row[kBlockId] as string));
			Assert.AreEqual("p", row[kParaTag]);
			Assert.AreEqual("4", row[kVerse]);
			Assert.AreEqual("narrator (JUD)", row[kCharacterId]);
			Assert.IsTrue(string.IsNullOrEmpty(row[kVernacularText] as string));
			Assert.AreEqual("Michael said.", row[kPrimaryReferenceText]);
			Assert.AreEqual(0, row[kVernacularTextLengthWithNoSecondaryRef]);

			row = data[i++];
			Assert.AreEqual(6, row[kBlockId]);
			Assert.AreEqual("p", row[kParaTag]);
			Assert.AreEqual("5", row[kVerse]);
			Assert.AreEqual("narrator (JUD)", row[kCharacterId]);
			Assert.AreEqual("[5]\u00A0E ", row[kVernacularText]);
			Assert.AreEqual("[5]\u00A0Ey", row[kPrimaryReferenceText]);

			row = data[i++];
			Assert.AreEqual(7, row[kBlockId]);
			Assert.AreEqual("p", row[kParaTag]);
			Assert.AreEqual("6", row[kVerse]);
			Assert.AreEqual("narrator (JUD)", row[kCharacterId]);
			Assert.AreEqual("[6]\u00A0F", row[kVernacularText]);
			Assert.AreEqual("[6]\u00A0Ef", row[kPrimaryReferenceText]);

			Assert.AreEqual(i, data.Count);
		}

		[Test]
		public void GetExportData_BlocksAreJoinedToStandardNonEnglishReferenceText_OutputContainsPrimaryAndEnglishReferenceText()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.JUD);
			var primaryReferenceText = ReferenceText.GetStandardReferenceText(ReferenceTextType.Azeri);
			project.ReferenceText = primaryReferenceText;
			var metadata = (GlyssenDblTextMetadata)ReflectionHelper.GetField(project, "m_metadata");
			metadata.IncludeChapterAnnouncementForFirstChapter = true;
			metadata.IncludeChapterAnnouncementForSingleChapterBooks = true;
			var exporter = new ProjectExporter(project);

			var data = exporter.GetExportData().ToList();

			Assert.IsTrue(data.All(d => (string)d[kBookId] == "JUD"));
			Assert.AreEqual("YӘHUDANIN MӘKTUBU", data[0][kPrimaryReferenceText]);
			Assert.AreEqual("JUDE", data[0][kSecondaryReferenceText]);
			Assert.IsTrue(data.Skip(1).All(d => (int)d[kChapter] == 1));
			Assert.AreEqual("YӘHUDA 1", data[1][kPrimaryReferenceText]);
			Assert.AreEqual("JUDE 1", data[1][kSecondaryReferenceText]);
			var matchedRows = data.Where(d => (string)d[kVernacularText] != null && (string)d[kPrimaryReferenceText] != null).ToList();
			Assert.IsTrue(matchedRows.Count > data.Count / 2); // This is kind of arbirary, but I just want to say we got a reasonable number of matches
			Assert.IsTrue(matchedRows.Any(d => ((string)d[kPrimaryReferenceText]).Contains("Ә"))); // A letter that should be in Azeri, but not English
			Assert.IsTrue(matchedRows.All(d => (string)d[kSecondaryReferenceText] != null));
			Assert.IsTrue(matchedRows.Any(d => ((string)d[kSecondaryReferenceText]).Contains(" the "))); // A word that should be in English, but not Azeri
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
			Assert.AreEqual("0\tp\tMRK\t4\t1\tFred\tWith great gusto and quivering frustration\t[1]\u00A0Text of verse one. [2]\u00A0Text of verse two.\t\t" + textLength,
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", null, null, true)));
			Assert.AreEqual("0\tActorGuy1\tp\tMRK\t4\t1\tFred\tWith great gusto and quivering frustration\t[1]\u00A0Text of verse one. [2]\u00A0Text of verse two.\t\t" + textLength,
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", actor, null, true)));
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
			Assert.AreEqual("0\tp\tMRK\t4\t3\t\t\tText of verse three, part two. [4]\u00A0Text of verse four. [5]\u00A0Text of verse five.\t\t" + textLength,
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", null, null, true)));
			Assert.AreEqual("0\tActorGuy1\tp\tMRK\t4\t3\t\t\tText of verse three, part two. [4]\u00A0Text of verse four. [5]\u00A0Text of verse five.\t\t" + textLength,
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", actor, null, true)));
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
			Assert.AreEqual("0\tp\tMRK\t4\t1\tnarrator (MRK)\tWith great gusto and quivering frustration\t[1]\u00A0Text of verse one. [2]\u00A0Text of verse two.\t\t" + textLength,
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", null, "narrator-MRK", true)));
			Assert.AreEqual("0\tActorGuy1\tp\tMRK\t4\t1\tnarrator (MRK)\tWith great gusto and quivering frustration\t[1]\u00A0Text of verse one. [2]\u00A0Text of verse two.\t\t" + textLength,
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", actor, "narrator-MRK", true)));
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
			Assert.AreEqual("0\tp\tMRK\t4\t1\tMarko\tWith great gusto and quivering frustration\t[1]\u00A0Text of verse one. [2]\u00A0Text of verse two.\t\t" + textLength,
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", null, null, true)));
			Assert.AreEqual("0\tActorGuy1\tp\tMRK\t4\t1\tMarko\tWith great gusto and quivering frustration\t[1]\u00A0Text of verse one. [2]\u00A0Text of verse two.\t\t" + textLength,
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", actor, null, true)));
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
			Assert.AreEqual("0\tp\tMRK\t4\t1\tFred/Marko\tWith great gusto and quivering frustration\t[1]\u00A0Text of verse one. [2]\u00A0Text of verse two.\t\t" + textLength,
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", null, null, false)));
			Assert.AreEqual("0\tActorGuy1\tp\tMRK\t4\t1\tFred/Marko\tWith great gusto and quivering frustration\t[1]\u00A0Text of verse one. [2]\u00A0Text of verse two.\t\t" + textLength,
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", actor, null, false)));
		}

		[Test]
		public void GetTabSeparatedLine_GetExportDataForBlock_SpecifyReferenceText_OutputContainsReferenceText()
		{
			var block = new Block("p", 4);
			block.IsParagraphStart = true;
			block.CharacterId = "Fred";
			block.Delivery = "With great gusto and quivering frustration";
			block.BlockElements.Add(new Verse("1"));
			block.BlockElements.Add(new ScriptText("Text of verse one. "));
			block.BlockElements.Add(new Verse("2"));
			block.BlockElements.Add(new ScriptText("Text of verse two."));
			block.SetMatchedReferenceBlock(new Block("p", 4, 1, 2).AddVerse("1-2", "Text of verses one and two bridged in harmony and goodness."));

			var actor = new Glyssen.VoiceActor.VoiceActor { Name = "ActorGuy1" };

			int textLength = "Text of verse one. ".Length + "Text of verse two.".Length;
			Assert.AreEqual("0\tActorGuy1\tp\tMRK\t4\t1\tFred\tWith great gusto and quivering frustration\t[1]\u00A0Text of verse one. [2]\u00A0Text of verse two.\t[1-2]\u00A0Text of verses one and two bridged in harmony and goodness.\t" + textLength,
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", actor, null, true)));
		}
	}
}
