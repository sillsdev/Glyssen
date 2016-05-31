using System.Collections.Generic;
using System.Linq;
using System.Text;
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
			foreach (var refBlock in blocks)
			{
				var secondaryRefBlock = new Block(refBlock.StyleTag, refBlock.ChapterNumber, refBlock.InitialStartVerseNumber, refBlock.InitialEndVerseNumber)
					{ CharacterId = refBlock.CharacterId };
				Verse verseElement = refBlock.BlockElements.First() as Verse;
				if (verseElement != null)
					secondaryRefBlock.AddVerse(verseElement.Number, "Secondary");
				else
					secondaryRefBlock.BlockElements = new List<BlockElement> { new ScriptText("the angel named Mike verbalized.") };
				refBlock.SetMatchedReferenceBlock(secondaryRefBlock);
			}
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
			Assert.AreEqual("[1]\u00A0Secondary", row[kSecondaryReferenceText]);

			row = data[i++];
			Assert.AreEqual(i, row[kBlockId]); // Row 2
			Assert.AreEqual("s", row[kParaTag]);
			Assert.AreEqual("1", row[kVerse]);
			Assert.AreEqual("section head (JUD)", row[kCharacterId]);
			Assert.AreEqual("Jude complains", row[kVernacularText]);
			Assert.IsTrue(string.IsNullOrEmpty(row[kPrimaryReferenceText] as string));
			Assert.IsTrue(string.IsNullOrEmpty(row[kSecondaryReferenceText] as string));

			row = data[i++];
			Assert.AreEqual(i, row[kBlockId]); // Row 3
			Assert.AreEqual("p", row[kParaTag]);
			Assert.AreEqual("2", row[kVerse]);
			Assert.AreEqual("Enoch", row[kCharacterId]);
			Assert.AreEqual("[2]\u00A0B", row[kVernacularText]);
			Assert.IsTrue(string.IsNullOrEmpty(row[kPrimaryReferenceText] as string));
			Assert.IsTrue(string.IsNullOrEmpty(row[kSecondaryReferenceText] as string));

			row = data[i++];
			Assert.AreEqual(i, row[kBlockId]); // Row 4
			Assert.AreEqual("p", row[kParaTag]);
			Assert.AreEqual("3", row[kVerse]);
			Assert.AreEqual("narrator (JUD)", row[kCharacterId]);
			Assert.AreEqual("[3]\u00A0C", row[kVernacularText]);
			Assert.IsTrue(string.IsNullOrEmpty(row[kPrimaryReferenceText] as string));
			Assert.IsTrue(string.IsNullOrEmpty(row[kSecondaryReferenceText] as string));

			row = data[i++];
			Assert.IsTrue(string.IsNullOrEmpty(row[kBlockId] as string));
			Assert.AreEqual("p", row[kParaTag]);
			Assert.AreEqual("2-3", row[kVerse]);
			Assert.AreEqual("narrator (JUD)", row[kCharacterId]);
			Assert.IsTrue(string.IsNullOrEmpty(row[kVernacularText] as string));
			Assert.AreEqual("[2-3]\u00A0Bee Cee", row[kPrimaryReferenceText]);
			Assert.AreEqual("[2-3]\u00A0Secondary", row[kSecondaryReferenceText]);
			Assert.AreEqual(0, row[kVernacularTextLengthWithSecondaryRef]);

			row = data[i++];
			Assert.AreEqual(5, row[kBlockId]); // Row 5
			Assert.AreEqual("p", row[kParaTag]);
			Assert.AreEqual("4", row[kVerse]);
			Assert.AreEqual("Michael", row[kCharacterId]);
			Assert.AreEqual("[4]\u00A0D", row[kVernacularText]);
			Assert.IsTrue(string.IsNullOrEmpty(row[kPrimaryReferenceText] as string));
			Assert.IsTrue(string.IsNullOrEmpty(row[kSecondaryReferenceText] as string));

			row = data[i++];
			Assert.IsTrue(string.IsNullOrEmpty(row[kBlockId] as string));
			Assert.AreEqual("p", row[kParaTag]);
			Assert.AreEqual("4", row[kVerse]);
			Assert.AreEqual("Michael", row[kCharacterId]);
			Assert.IsTrue(string.IsNullOrEmpty(row[kVernacularText] as string));
			Assert.AreEqual("[4]\u00A0Dee, ", row[kPrimaryReferenceText]);
			Assert.AreEqual("[4]\u00A0Secondary", row[kSecondaryReferenceText]);
			Assert.AreEqual(0, row[kVernacularTextLengthWithSecondaryRef]);

			row = data[i++];
			Assert.IsTrue(string.IsNullOrEmpty(row[kBlockId] as string));
			Assert.AreEqual("p", row[kParaTag]);
			Assert.AreEqual("4", row[kVerse]);
			Assert.AreEqual("narrator (JUD)", row[kCharacterId]);
			Assert.IsTrue(string.IsNullOrEmpty(row[kVernacularText] as string));
			Assert.AreEqual("Michael said.", row[kPrimaryReferenceText]);
			Assert.AreEqual("the angel named Mike verbalized.", row[kSecondaryReferenceText]);
			Assert.AreEqual(0, row[kVernacularTextLengthWithSecondaryRef]);

			row = data[i++];
			Assert.AreEqual(6, row[kBlockId]);
			Assert.AreEqual("p", row[kParaTag]);
			Assert.AreEqual("5", row[kVerse]);
			Assert.AreEqual("narrator (JUD)", row[kCharacterId]);
			Assert.AreEqual("[5]\u00A0E ", row[kVernacularText]);
			Assert.AreEqual("[5]\u00A0Ey", row[kPrimaryReferenceText]);
			Assert.AreEqual("[5]\u00A0Secondary", row[kSecondaryReferenceText]);

			row = data[i++];
			Assert.AreEqual(7, row[kBlockId]);
			Assert.AreEqual("p", row[kParaTag]);
			Assert.AreEqual("6", row[kVerse]);
			Assert.AreEqual("narrator (JUD)", row[kCharacterId]);
			Assert.AreEqual("[6]\u00A0F", row[kVernacularText]);
			Assert.AreEqual("[6]\u00A0Ef", row[kPrimaryReferenceText]);
			Assert.AreEqual("[6]\u00A0Secondary", row[kSecondaryReferenceText]);

			Assert.AreEqual(i, data.Count);
		}

		[Test]
		public void GetExportData_BlocksAreJoinedToStandardNonEnglishReferenceText_OutputContainsPrimaryAndEnglishReferenceText()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.JUD);
			project.ReferenceText = ReferenceText.GetStandardReferenceText(ReferenceTextType.Azeri);
			var metadata = (GlyssenDblTextMetadata)ReflectionHelper.GetField(project, "m_metadata");
			metadata.IncludeChapterAnnouncementForFirstChapter = true;
			metadata.IncludeChapterAnnouncementForSingleChapterBooks = true;
			var exporter = new ProjectExporter(project);

			var data = exporter.GetExportData().ToList();

			Assert.IsTrue(data.All(d => (string)d[kBookId] == "JUD"));
			Assert.IsTrue(data.All(d => d.Count == kVernacularTextLengthWithSecondaryRef + 1));
			Assert.AreEqual("YӘHUDANIN MӘKTUBU", data[0][kPrimaryReferenceText]);
			Assert.AreEqual("JUDE", data[0][kSecondaryReferenceText]);
			Assert.IsTrue(data.Skip(1).All(d => (int)d[kChapter] == 1));
			Assert.AreEqual("YӘHUDA 1", data[1][kPrimaryReferenceText]);
			Assert.AreEqual("JUDE CHP 1", data[1][kSecondaryReferenceText]);
			var matchedRows = data.Where(d => (string)d[kVernacularText] != null && (string)d[kPrimaryReferenceText] != null).ToList();
			Assert.IsTrue(matchedRows.Count > data.Count / 2); // This is kind of arbitrary, but I just want to say we got a reasonable number of matches
			Assert.IsTrue(matchedRows.Any(d => ((string)d[kPrimaryReferenceText]).Contains("Ә"))); // A letter that should be in Azeri, but not English
			Assert.IsTrue(matchedRows.All(d => (string)d[kSecondaryReferenceText] != null));
			Assert.IsTrue(matchedRows.Any(d => ((string)d[kSecondaryReferenceText]).Contains(" the "))); // A word that should be in English, but not Azeri
			// Since the test version of Jude does not match perfectly with the standard reference texts, we expect some rows
			// to come from those mis-matches.
			var rowsWithNoVernacular = data.Where(d => d[kVernacularText] == null).ToList();
			Assert.IsTrue(rowsWithNoVernacular.Any());
			Assert.IsTrue(rowsWithNoVernacular.All(d => d.Count == kVernacularTextLengthWithSecondaryRef + 1));
			Assert.IsTrue(rowsWithNoVernacular.All(d => d[kSecondaryReferenceText] != null));
			Assert.IsTrue(rowsWithNoVernacular.Any(d => ((string)d[kSecondaryReferenceText]).Contains(" the "))); // A word that should be in English, but not Azeri
		}

		[Test]
		public void GetExportData_ReferenceTextsContainAnnotations()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.JUD, TestProject.TestBook.REV);
			project.ReferenceText = ReferenceText.GetStandardReferenceText(ReferenceTextType.Azeri);
			var exporter = new ProjectExporter(project);

			var data = exporter.GetExportData().ToList();

			//SFX (sfx come before verse text)
			var rowsForVerse12 = data.Where(d => (string)d[kBookId] == "JUD" && (int)d[kChapter] == 1 && (string)d[kVerse] == "12").ToList();
			Assert.AreEqual(2, rowsForVerse12.Count);
			var annotationRowForVerse12 = rowsForVerse12[0];
			var textRowForVerse12 = rowsForVerse12[1];
			Assert.IsTrue(annotationRowForVerse12[kPrimaryReferenceText].Equals(annotationRowForVerse12[kSecondaryReferenceText]) &&
				((string)annotationRowForVerse12[kPrimaryReferenceText]).StartsWith(Sound.kDoNotCombine + "{SFX"));
			Assert.IsTrue(!((string)textRowForVerse12[kPrimaryReferenceText]).Contains("|||"));

			//Pause for final verse in book (pauses come after verse text)
			var rowsForJude25 = data.Where(d => (string)d[kBookId] == "JUD" && (int)d[kChapter] == 1 && (string)d[kVerse] == "25").ToList();
			Assert.AreEqual(2, rowsForJude25.Count);
			var textRowForJude25 = rowsForJude25[0];
			var annotationRowForJude25 = rowsForJude25[1];
			Assert.IsTrue(!((string)textRowForJude25[kPrimaryReferenceText]).Contains("|||"));
			Assert.IsTrue(annotationRowForJude25[kPrimaryReferenceText].Equals(annotationRowForJude25[kSecondaryReferenceText]) &&
				((string)annotationRowForJude25[kPrimaryReferenceText]).Equals(string.Format(Pause.kPauseSecondsFormat, "5")));

			//Pause for non-final verse in book (pauses come after verse text)
			var rowsForRev1V3 = data.Where(d => (string)d[kBookId] == "REV" && (int)d[kChapter] == 1 && (string)d[kVerse] == "3").ToList();
			Assert.AreEqual(3, rowsForRev1V3.Count);
			var textRowForRev1V3 = rowsForRev1V3[0];
			var annotationRowForRev1V3 = rowsForRev1V3[1];
			var sectionHeadRowForRev1V3 = rowsForRev1V3[2];
			Assert.IsTrue(!((string)textRowForRev1V3[kPrimaryReferenceText]).Contains("|||"));
			Assert.IsTrue(annotationRowForRev1V3[kPrimaryReferenceText].Equals(annotationRowForRev1V3[kSecondaryReferenceText]) &&
				((string)annotationRowForRev1V3[kPrimaryReferenceText]).Equals(string.Format(Pause.kPauseSecondsFormat, "2")));
			Assert.IsTrue(sectionHeadRowForRev1V3[kCharacterId].Equals(CharacterVerseData.GetStandardCharacterIdAsEnglish(CharacterVerseData.GetStandardCharacterId("REV", CharacterVerseData.StandardCharacter.ExtraBiblical))));

			//Pause for final verse in chapter (pauses come after verse text)
			var rowsForRev1V20 = data.Where(d => (string)d[kBookId] == "REV" && (int)d[kChapter] == 1 && (string)d[kVerse] == "20").ToList();
			Assert.AreEqual(2, rowsForRev1V20.Count);
			var textRowForRev1V20 = rowsForRev1V20[0];
			var annotationRowForRev1V20 = rowsForRev1V20[1];
			Assert.IsTrue(!((string)textRowForRev1V20[kPrimaryReferenceText]).Contains("|||"));
			Assert.IsTrue(annotationRowForRev1V20[kPrimaryReferenceText].Equals(annotationRowForRev1V20[kSecondaryReferenceText]) &&
				((string)annotationRowForRev1V20[kPrimaryReferenceText]).Equals(string.Format(Pause.kPauseSecondsFormat, "2")));
		}

		[Test]
		public void GetExportData_AnnotationWithOffset_ReferenceTextContainsAnnotationInCorrectLocation()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.MRK);
			project.ReferenceText = ReferenceText.GetStandardReferenceText(ReferenceTextType.French);
			var exporter = new ProjectExporter(project);

			var data = exporter.GetExportData().ToList();

			//Pause mid-verse
			var rowsForMark4V39 = data.Where(d => (string)d[kBookId] == "MRK" && (int)d[kChapter] == 4 && (string)d[kVerse] == "39").ToList();
			Assert.AreEqual(4, rowsForMark4V39.Count);
			var narratorTextRow1ForMark4V39 = rowsForMark4V39[0];
			var jesusTextRowForMark4V39 = rowsForMark4V39[0];
			var annotationRowForMark4V39 = rowsForMark4V39[2];
			var narratorTextRow2ForMark4V39 = rowsForMark4V39[3];
			Assert.IsFalse(((string)narratorTextRow1ForMark4V39[kPrimaryReferenceText]).Contains("|||"));
			Assert.IsFalse(((string)jesusTextRowForMark4V39[kPrimaryReferenceText]).Contains("|||"));
			Assert.IsTrue(annotationRowForMark4V39[kPrimaryReferenceText].Equals(annotationRowForMark4V39[kSecondaryReferenceText]) &&
				((string)annotationRowForMark4V39[kPrimaryReferenceText]).Equals(string.Format(Pause.kPauseSecondsFormat, "1.5")));
			Assert.IsFalse(((string)narratorTextRow2ForMark4V39[kPrimaryReferenceText]).Contains("|||"));
		}

		[Test]
		public void GeneratePreviewTable_BlocksAreJoinedToStandardNonEnglishReferenceText_HeadersIncludeNonEnglishAndEnglishDirectorsGuide()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.JUD);
			project.ReferenceText = ReferenceText.GetStandardReferenceText(ReferenceTextType.Spanish);
			var exporter = new ProjectExporter(project);

			var data = exporter.GeneratePreviewTable();

			var iCol = 0;
			Assert.IsTrue(data.Columns[iCol++].ColumnName == "#");
			Assert.IsTrue(data.Columns[iCol++].ColumnName == "Tag");
			Assert.IsTrue(data.Columns[iCol++].ColumnName == "Book");
			Assert.IsTrue(data.Columns[iCol++].ColumnName == "Chapter");
			Assert.IsTrue(data.Columns[iCol++].ColumnName == "Verse");
			Assert.IsTrue(data.Columns[iCol++].ColumnName == "Character");
			Assert.IsTrue(data.Columns[iCol++].ColumnName == "Delivery");
			Assert.IsTrue(data.Columns[iCol++].ColumnName == "Text");
			Assert.IsTrue(data.Columns[iCol++].ColumnName == "Spanish Director's Guide");
			Assert.IsTrue(data.Columns[iCol++].ColumnName == "English Director's Guide");
			Assert.IsTrue(data.Columns[iCol].ColumnName == "Size");
			//Assert.AreEqual("YӘHUDANIN MӘKTUBU", data[0][kPrimaryReferenceText]);
			//Assert.AreEqual("JUDE", data[0][kSecondaryReferenceText]);
			//Assert.IsTrue(data.Skip(1).All(d => (int)d[kChapter] == 1));
			//Assert.AreEqual("YӘHUDA 1", data[1][kPrimaryReferenceText]);
			//Assert.AreEqual("JUDE CHP 1", data[1][kSecondaryReferenceText]);
			//var matchedRows = data.Where(d => (string)d[kVernacularText] != null && (string)d[kPrimaryReferenceText] != null).ToList();
			//Assert.IsTrue(matchedRows.Count > data.Count / 2); // This is kind of arbirary, but I just want to say we got a reasonable number of matches
			//Assert.IsTrue(matchedRows.Any(d => ((string)d[kPrimaryReferenceText]).Contains("Ә"))); // A letter that should be in Azeri, but not English
			//Assert.IsTrue(matchedRows.All(d => (string)d[kSecondaryReferenceText] != null));
			//Assert.IsTrue(matchedRows.Any(d => ((string)d[kSecondaryReferenceText]).Contains(" the "))); // A word that should be in English, but not Azeri
		}

		[TestCase(true)]
		[TestCase(false)]
		public void GetTabSeparatedLine_GetExportDataForBlock_VerseAndTextElements_ExpectedColumnsIncludingJoinedText(bool includeSecondaryReferenceText)
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
			var expectedLine = new StringBuilder("0\tp\tMRK\t4\t1\tFred\tWith great gusto and quivering frustration\t[1]\u00A0Text of verse one. [2]\u00A0Text of verse two.\t\t");
			if (includeSecondaryReferenceText)
				expectedLine.Append("\t");
			expectedLine.Append(textLength);
			Assert.AreEqual(expectedLine.ToString(),
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", null, null, true, includeSecondaryReferenceText)));
			expectedLine.Insert(1, "\tActorGuy1");
			Assert.AreEqual(expectedLine.ToString(),
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", actor, null, true, includeSecondaryReferenceText)));
		}

		[TestCase(true)]
		[TestCase(false)]
		public void GetTabSeparatedLine_GetExportDataForBlock_TextBeginsMidVerse_ResultHasCorrectVerseInfo(bool includeSecondaryReferenceText)
		{
			var block = new Block("p", 4, 3);
			block.BlockElements.Add(new ScriptText("Text of verse three, part two. "));
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Text of verse four. "));
			block.BlockElements.Add(new Verse("5"));
			block.BlockElements.Add(new ScriptText("Text of verse five."));

			var actor = new Glyssen.VoiceActor.VoiceActor { Name = "ActorGuy1" };

			int textLength = "Text of verse three, part two. ".Length + "Text of verse four. ".Length + "Text of verse five.".Length;
			var expectedLine = new StringBuilder("0\tp\tMRK\t4\t3\t\t\tText of verse three, part two. [4]\u00A0Text of verse four. [5]\u00A0Text of verse five.\t\t");
			if (includeSecondaryReferenceText)
				expectedLine.Append("\t");
			expectedLine.Append(textLength);
			Assert.AreEqual(expectedLine.ToString(),
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", null, null, true, includeSecondaryReferenceText)));
			expectedLine.Insert(1, "\tActorGuy1");
			Assert.AreEqual(expectedLine.ToString(),
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", actor, null, true, includeSecondaryReferenceText)));
		}

		[TestCase(true)]
		[TestCase(false)]
		public void GetTabSeparatedLine_GetExportDataForBlock_SpecifyNarratorCharacter_OutputContainsNarrator(bool includeSecondaryReferenceText)
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
			var expectedLine = new StringBuilder("0\tp\tMRK\t4\t1\tnarrator (MRK)\tWith great gusto and quivering frustration\t[1]\u00A0Text of verse one. [2]\u00A0Text of verse two.\t\t");
			if (includeSecondaryReferenceText)
				expectedLine.Append("\t");
			expectedLine.Append(textLength);
			Assert.AreEqual(expectedLine.ToString(),
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", null, "narrator-MRK", true, includeSecondaryReferenceText)));
			expectedLine.Insert(1, "\tActorGuy1");
			Assert.AreEqual(expectedLine.ToString(),
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", actor, "narrator-MRK", true, includeSecondaryReferenceText)));
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
			var expectedLine = new StringBuilder("0\tp\tMRK\t4\t1\tMarko\tWith great gusto and quivering frustration\t[1]\u00A0Text of verse one. [2]\u00A0Text of verse two.\t\t");
			expectedLine.Append(textLength);
			Assert.AreEqual(expectedLine.ToString(),
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", null, null, true, false)));
			expectedLine.Insert(1, "\tActorGuy1");
			Assert.AreEqual(expectedLine.ToString(),
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", actor, null, true, false)));
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
			var expectedLine = new StringBuilder("0\tp\tMRK\t4\t1\tFred/Marko\tWith great gusto and quivering frustration\t[1]\u00A0Text of verse one. [2]\u00A0Text of verse two.\t\t");
			expectedLine.Append(textLength);
			Assert.AreEqual(expectedLine.ToString(),
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", null, null, false, false)));
			expectedLine.Insert(1, "\tActorGuy1");
			Assert.AreEqual(expectedLine.ToString(),
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", actor, null, false, false)));
		}

		[TestCase(true)]
		[TestCase(false)]
		public void GetTabSeparatedLine_GetExportDataForBlock_SpecifyReferenceText_OutputContainsReferenceText(bool includeSecondaryReferenceText)
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
			var expectedLine = new StringBuilder("0\tActorGuy1\tp\tMRK\t4\t1\tFred\tWith great gusto and quivering frustration\t[1]\u00A0Text of verse one. [2]\u00A0Text of verse two.\t[1-2]\u00A0Text of verses one and two bridged in harmony and goodness.\t");
			if (includeSecondaryReferenceText)
				expectedLine.Append("\t");
			expectedLine.Append(textLength);
			Assert.AreEqual(expectedLine.ToString(),
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", actor, null, true, includeSecondaryReferenceText)));
		}
	}
}
