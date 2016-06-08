using System.Collections.Generic;
using System.Linq;
using System.Text;
using Glyssen;
using Glyssen.Bundle;
using Glyssen.Character;
using NUnit.Framework;
using SIL.Extensions;
using SIL.Windows.Forms;

namespace GlyssenTests
{
	[TestFixture]
	class ProjectExportTests
	{
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
			var primaryReferenceText = TestReferenceText.CreateTestReferenceText("JUD", blocks);
			project.ReferenceText = primaryReferenceText;
			var exporter = new ProjectExporter(project);
			var data = exporter.GetExportData().ToList();

			Assert.IsTrue(data.All(d => (string)d[exporter.GetColumnIndex(ExportColumn.BookId)] == "JUD" && (int)d[exporter.GetColumnIndex(ExportColumn.Chapter)] == 1));
			var i = 0;
			var row = data[i++];
			Assert.AreEqual(i, row[exporter.GetColumnIndex(ExportColumn.BlockId)]); // Row 1
			Assert.AreEqual("p", row[exporter.GetColumnIndex(ExportColumn.ParaTag)]);
			Assert.AreEqual("1", row[exporter.GetColumnIndex(ExportColumn.Verse)]);
			Assert.AreEqual("narrator (JUD)", row[exporter.GetColumnIndex(ExportColumn.CharacterId)]);
			Assert.AreEqual("[1]\u00A0A", row[exporter.GetColumnIndex(ExportColumn.VernacularText)]);
			Assert.AreEqual("[1]\u00A0Ayy", row[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)]);
			Assert.AreEqual("[1]\u00A0Secondary", row[exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)]);

			row = data[i++];
			Assert.AreEqual(i, row[exporter.GetColumnIndex(ExportColumn.BlockId)]); // Row 2
			Assert.AreEqual("s", row[exporter.GetColumnIndex(ExportColumn.ParaTag)]);
			Assert.AreEqual("1", row[exporter.GetColumnIndex(ExportColumn.Verse)]);
			Assert.AreEqual("section head (JUD)", row[exporter.GetColumnIndex(ExportColumn.CharacterId)]);
			Assert.AreEqual("Jude complains", row[exporter.GetColumnIndex(ExportColumn.VernacularText)]);
			Assert.IsTrue(string.IsNullOrEmpty(row[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)] as string));
			Assert.IsTrue(string.IsNullOrEmpty(row[exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)] as string));

			row = data[i++];
			Assert.AreEqual(i, row[exporter.GetColumnIndex(ExportColumn.BlockId)]); // Row 3
			Assert.AreEqual("p", row[exporter.GetColumnIndex(ExportColumn.ParaTag)]);
			Assert.AreEqual("2", row[exporter.GetColumnIndex(ExportColumn.Verse)]);
			Assert.AreEqual("Enoch", row[exporter.GetColumnIndex(ExportColumn.CharacterId)]);
			Assert.AreEqual("[2]\u00A0B", row[exporter.GetColumnIndex(ExportColumn.VernacularText)]);
			Assert.IsTrue(string.IsNullOrEmpty(row[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)] as string));
			Assert.IsTrue(string.IsNullOrEmpty(row[exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)] as string));

			row = data[i++];
			Assert.AreEqual(i, row[exporter.GetColumnIndex(ExportColumn.BlockId)]); // Row 4
			Assert.AreEqual("p", row[exporter.GetColumnIndex(ExportColumn.ParaTag)]);
			Assert.AreEqual("3", row[exporter.GetColumnIndex(ExportColumn.Verse)]);
			Assert.AreEqual("narrator (JUD)", row[exporter.GetColumnIndex(ExportColumn.CharacterId)]);
			Assert.AreEqual("[3]\u00A0C", row[exporter.GetColumnIndex(ExportColumn.VernacularText)]);
			Assert.IsTrue(string.IsNullOrEmpty(row[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)] as string));
			Assert.IsTrue(string.IsNullOrEmpty(row[exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)] as string));

			row = data[i++];
			Assert.IsTrue(string.IsNullOrEmpty(row[exporter.GetColumnIndex(ExportColumn.BlockId)] as string));
			Assert.AreEqual("p", row[exporter.GetColumnIndex(ExportColumn.ParaTag)]);
			Assert.AreEqual("2-3", row[exporter.GetColumnIndex(ExportColumn.Verse)]);
			Assert.AreEqual("narrator (JUD)", row[exporter.GetColumnIndex(ExportColumn.CharacterId)]);
			Assert.IsTrue(string.IsNullOrEmpty(row[exporter.GetColumnIndex(ExportColumn.VernacularText)] as string));
			Assert.AreEqual("[2-3]\u00A0Bee Cee", row[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)]);
			Assert.AreEqual("[2-3]\u00A0Secondary", row[exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)]);
			Assert.AreEqual(0, row[exporter.GetColumnIndex(ExportColumn.VernacularTextLength)]);

			row = data[i++];
			Assert.AreEqual(5, row[exporter.GetColumnIndex(ExportColumn.BlockId)]); // Row 5
			Assert.AreEqual("p", row[exporter.GetColumnIndex(ExportColumn.ParaTag)]);
			Assert.AreEqual("4", row[exporter.GetColumnIndex(ExportColumn.Verse)]);
			Assert.AreEqual("Michael", row[exporter.GetColumnIndex(ExportColumn.CharacterId)]);
			Assert.AreEqual("[4]\u00A0D", row[exporter.GetColumnIndex(ExportColumn.VernacularText)]);
			Assert.IsTrue(string.IsNullOrEmpty(row[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)] as string));
			Assert.IsTrue(string.IsNullOrEmpty(row[exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)] as string));

			row = data[i++];
			Assert.IsTrue(string.IsNullOrEmpty(row[exporter.GetColumnIndex(ExportColumn.BlockId)] as string));
			Assert.AreEqual("p", row[exporter.GetColumnIndex(ExportColumn.ParaTag)]);
			Assert.AreEqual("4", row[exporter.GetColumnIndex(ExportColumn.Verse)]);
			Assert.AreEqual("Michael", row[exporter.GetColumnIndex(ExportColumn.CharacterId)]);
			Assert.IsTrue(string.IsNullOrEmpty(row[exporter.GetColumnIndex(ExportColumn.VernacularText)] as string));
			Assert.AreEqual("[4]\u00A0Dee, ", row[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)]);
			Assert.AreEqual("[4]\u00A0Secondary", row[exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)]);
			Assert.AreEqual(0, row[exporter.GetColumnIndex(ExportColumn.VernacularTextLength)]);

			row = data[i++];
			Assert.IsTrue(string.IsNullOrEmpty(row[exporter.GetColumnIndex(ExportColumn.BlockId)] as string));
			Assert.AreEqual("p", row[exporter.GetColumnIndex(ExportColumn.ParaTag)]);
			Assert.AreEqual("4", row[exporter.GetColumnIndex(ExportColumn.Verse)]);
			Assert.AreEqual("narrator (JUD)", row[exporter.GetColumnIndex(ExportColumn.CharacterId)]);
			Assert.IsTrue(string.IsNullOrEmpty(row[exporter.GetColumnIndex(ExportColumn.VernacularText)] as string));
			Assert.AreEqual("Michael said.", row[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)]);
			Assert.AreEqual("the angel named Mike verbalized.", row[exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)]);
			Assert.AreEqual(0, row[exporter.GetColumnIndex(ExportColumn.VernacularTextLength)]);

			row = data[i++];
			Assert.AreEqual(6, row[exporter.GetColumnIndex(ExportColumn.BlockId)]);
			Assert.AreEqual("p", row[exporter.GetColumnIndex(ExportColumn.ParaTag)]);
			Assert.AreEqual("5", row[exporter.GetColumnIndex(ExportColumn.Verse)]);
			Assert.AreEqual("narrator (JUD)", row[exporter.GetColumnIndex(ExportColumn.CharacterId)]);
			Assert.AreEqual("[5]\u00A0E ", row[exporter.GetColumnIndex(ExportColumn.VernacularText)]);
			Assert.AreEqual("[5]\u00A0Ey", row[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)]);
			Assert.AreEqual("[5]\u00A0Secondary", row[exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)]);

			row = data[i++];
			Assert.AreEqual(7, row[exporter.GetColumnIndex(ExportColumn.BlockId)]);
			Assert.AreEqual("p", row[exporter.GetColumnIndex(ExportColumn.ParaTag)]);
			Assert.AreEqual("6", row[exporter.GetColumnIndex(ExportColumn.Verse)]);
			Assert.AreEqual("narrator (JUD)", row[exporter.GetColumnIndex(ExportColumn.CharacterId)]);
			Assert.AreEqual("[6]\u00A0F", row[exporter.GetColumnIndex(ExportColumn.VernacularText)]);
			Assert.AreEqual("[6]\u00A0Ef", row[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)]);
			Assert.AreEqual("[6]\u00A0Secondary", row[exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)]);

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

			Assert.IsTrue(data.All(d => (string)d[exporter.GetColumnIndex(ExportColumn.BookId)] == "JUD"));
			Assert.IsTrue(data.All(d => d.Count == exporter.GetColumnIndex(ExportColumn.VernacularTextLength) + 1));
			Assert.AreEqual("YӘHUDANIN MӘKTUBU", data[0][exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)]);
			Assert.AreEqual("JUDE", data[0][exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)]);
			Assert.IsTrue(data.Skip(1).All(d => (int)d[exporter.GetColumnIndex(ExportColumn.Chapter)] == 1));
			Assert.AreEqual("YӘHUDA 1", data[1][exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)]);
			Assert.AreEqual("JUDE CHP 1", data[1][exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)]);
			var matchedRows = data.Where(d => (string)d[exporter.GetColumnIndex(ExportColumn.VernacularText)] != null && (string)d[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)] != null).ToList();
			Assert.IsTrue(matchedRows.Count > data.Count / 2); // This is kind of arbitrary, but I just want to say we got a reasonable number of matches
			Assert.IsTrue(matchedRows.Any(d => ((string)d[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)]).Contains("Ә"))); // A letter that should be in Azeri, but not English
			Assert.IsTrue(matchedRows.All(d => (string)d[exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)] != null));
			Assert.IsTrue(matchedRows.Any(d => ((string)d[exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)]).Contains(" the "))); // A word that should be in English, but not Azeri
			// Since the test version of Jude does not match perfectly with the standard reference texts, we expect some rows
			// to come from those mis-matches.
			var rowsWithNoVernacular = data.Where(d => d[exporter.GetColumnIndex(ExportColumn.VernacularText)] == null).ToList();
			Assert.IsTrue(rowsWithNoVernacular.Any());
			Assert.IsTrue(rowsWithNoVernacular.All(d => d.Count == exporter.GetColumnIndex(ExportColumn.VernacularTextLength) + 1));
			Assert.IsTrue(rowsWithNoVernacular.All(d => d[exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)] != null));
			Assert.IsTrue(rowsWithNoVernacular.Any(d => ((string)d[exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)]).Contains(" the "))); // A word that should be in English, but not Azeri
		}

		[Test]
		public void GetExportData_ExportAnnotationsInSeparateRows_ReferenceTextsContainAnnotations()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.JUD, TestProject.TestBook.REV);
			project.ReferenceText = ReferenceText.GetStandardReferenceText(ReferenceTextType.Azeri);
			var exporter = new ProjectExporter(project);
			exporter.ExportAnnotationsInSeparateRows = true;

			var data = exporter.GetExportData().ToList();

			//SFX (sfx come before verse text)
			var rowsForVerse12 = data.Where(d => (string)d[exporter.GetColumnIndex(ExportColumn.BookId)] == "JUD" && (int)d[exporter.GetColumnIndex(ExportColumn.Chapter)] == 1 && (string)d[exporter.GetColumnIndex(ExportColumn.Verse)] == "12").ToList();
			Assert.AreEqual(2, rowsForVerse12.Count);
			var annotationRowForVerse12 = rowsForVerse12[0];
			var textRowForVerse12 = rowsForVerse12[1];
			Assert.IsTrue(annotationRowForVerse12[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)].Equals(annotationRowForVerse12[exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)]) &&
				((string)annotationRowForVerse12[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)]).StartsWith(Sound.kDoNotCombine + exporter.AnnotationElementSeparator + "{SFX"));
			Assert.IsFalse(((string)textRowForVerse12[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)]).Contains("|||"));

			//Pause for final verse in book (pauses come after verse text)
			var rowsForJude25 = data.Where(d => (string)d[exporter.GetColumnIndex(ExportColumn.BookId)] == "JUD" && (int)d[exporter.GetColumnIndex(ExportColumn.Chapter)] == 1 && (string)d[exporter.GetColumnIndex(ExportColumn.Verse)] == "25").ToList();
			Assert.AreEqual(2, rowsForJude25.Count);
			var textRowForJude25 = rowsForJude25[0];
			var annotationRowForJude25 = rowsForJude25[1];
			Assert.IsFalse(((string)textRowForJude25[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)]).Contains("|||"));
			Assert.IsTrue(annotationRowForJude25[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)].Equals(annotationRowForJude25[exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)]) &&
				((string)annotationRowForJude25[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)]).Equals(string.Format(Pause.kPauseSecondsFormat, "5")));

			//Pause for non-final verse in book (pauses come after verse text)
			var rowsForRev1V3 = data.Where(d => (string)d[exporter.GetColumnIndex(ExportColumn.BookId)] == "REV" && (int)d[exporter.GetColumnIndex(ExportColumn.Chapter)] == 1 && (string)d[exporter.GetColumnIndex(ExportColumn.Verse)] == "3").ToList();
			Assert.AreEqual(3, rowsForRev1V3.Count);
			var textRowForRev1V3 = rowsForRev1V3[0];
			var annotationRowForRev1V3 = rowsForRev1V3[1];
			var sectionHeadRowForRev1V3 = rowsForRev1V3[2];
			Assert.IsFalse(((string)textRowForRev1V3[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)]).Contains("|||"));
			Assert.IsTrue(annotationRowForRev1V3[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)].Equals(annotationRowForRev1V3[exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)]) &&
				((string)annotationRowForRev1V3[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)]).Equals(string.Format(Pause.kPauseSecondsFormat, "2")));
			Assert.IsTrue(sectionHeadRowForRev1V3[exporter.GetColumnIndex(ExportColumn.CharacterId)].Equals(CharacterVerseData.GetStandardCharacterIdAsEnglish(CharacterVerseData.GetStandardCharacterId("REV", CharacterVerseData.StandardCharacter.ExtraBiblical))));

			//Pause for final verse in chapter (pauses come after verse text)
			var rowsForRev1V20 = data.Where(d => (string)d[exporter.GetColumnIndex(ExportColumn.BookId)] == "REV" && (int)d[exporter.GetColumnIndex(ExportColumn.Chapter)] == 1 && (string)d[exporter.GetColumnIndex(ExportColumn.Verse)] == "20").ToList();
			Assert.AreEqual(2, rowsForRev1V20.Count);
			var textRowForRev1V20 = rowsForRev1V20[0];
			var annotationRowForRev1V20 = rowsForRev1V20[1];
			Assert.IsFalse(((string)textRowForRev1V20[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)]).Contains("|||"));
			Assert.IsTrue(annotationRowForRev1V20[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)].Equals(annotationRowForRev1V20[exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)]) &&
				((string)annotationRowForRev1V20[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)]).Equals(string.Format(Pause.kPauseSecondsFormat, "2")));
		}

		[TestCase(ExportFileType.Excel)]
		[TestCase(ExportFileType.TabSeparated)]
		public void GetExportData_AnnotationsCombinedWithData_ReferenceTextsContainAnnotations(ExportFileType exportFileType)
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.JUD, TestProject.TestBook.REV);
			project.ReferenceText = ReferenceText.GetStandardReferenceText(ReferenceTextType.Azeri);
			var exporter = new ProjectExporter(project) { SelectedFileType = exportFileType };
			// This is the default: exporter.ExportAnnotationsInSeparateRows = false;

			var data = exporter.GetExportData().ToList();

			//SFX (music/sfx come before verse text)
			var rowsForVerse12 = data.Where(d => (string)d[exporter.GetColumnIndex(ExportColumn.BookId)] == "JUD" && (int)d[exporter.GetColumnIndex(ExportColumn.Chapter)] == 1 && (string)d[exporter.GetColumnIndex(ExportColumn.Verse)] == "12").ToList();
			var textRowForVerse12 = rowsForVerse12.Single();
			var annotationInfoPlusVerseNum = Sound.kDoNotCombine + exporter.AnnotationElementSeparator + "{SFX--Eerie--Starts @ v12} [12]\u00A0";
			Assert.IsTrue(((string)textRowForVerse12[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)]).StartsWith(annotationInfoPlusVerseNum));
			Assert.IsTrue(((string)textRowForVerse12[exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)]).StartsWith(annotationInfoPlusVerseNum));
			Assert.AreEqual("[12]\u00A0Gikelo lewic i karamawu me mar ka gicamo matek mukato kare laboŋo lworo, kun giparo pi komgi keken. " +
							"Gubedo calo pol ma pii pe iye ma yamo kolo; girom ki yadi ma nyiggi pe nen i kare me cekgi, ma giputo lwitgi woko, " +
							"yam guto kiryo. ",
				(string)textRowForVerse12[exporter.GetColumnIndex(ExportColumn.VernacularText)]);

			//Pause for final verse in book (pauses come after verse text)
			var rowsForJude25 = data.Where(d => (string)d[exporter.GetColumnIndex(ExportColumn.BookId)] == "JUD" && (int)d[exporter.GetColumnIndex(ExportColumn.Chapter)] == 1 && (string)d[exporter.GetColumnIndex(ExportColumn.Verse)] == "25").ToList();
			var textRowForJude25 = rowsForJude25.Single();
			var annotationInfo = " " + string.Format(Pause.kPauseSecondsFormat, "5");
			Assert.IsTrue(((string)textRowForJude25[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)]).EndsWith(annotationInfo));
			Assert.IsTrue(((string)textRowForJude25[exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)]).EndsWith(annotationInfo));
			Assert.AreEqual("[25]\u00A0Deyo, dit, loc ki twer ducu obed bot Lubaŋa acel keken, ma Lalarwa, pi Yecu Kricito Rwotwa, " +
							"cakke ma peya giketo lobo, nio koni, ki kare ma pe gik. Amen.",
				(string)textRowForJude25[exporter.GetColumnIndex(ExportColumn.VernacularText)]);

			//Pause for non-final verse in book (pauses come after verse text)
			var rowsForRev1V3 = data.Where(d => (string)d[exporter.GetColumnIndex(ExportColumn.BookId)] == "REV" && (int)d[exporter.GetColumnIndex(ExportColumn.Chapter)] == 1 && (string)d[exporter.GetColumnIndex(ExportColumn.Verse)] == "3").ToList();
			Assert.AreEqual(2, rowsForRev1V3.Count);
			var textRowForRev1V3 = rowsForRev1V3[0];
			var sectionHeadRowForRev1V3 = rowsForRev1V3[1];
			annotationInfo = " " + string.Format(Pause.kPauseSecondsFormat, "2");
			Assert.IsTrue(((string)textRowForRev1V3[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)]).EndsWith(annotationInfo));
			Assert.IsTrue(((string)textRowForRev1V3[exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)]).EndsWith(annotationInfo));
			Assert.AreEqual("[3]\u00A0Ŋat ma kwano lok ma gitito i buk man i nyim lwak tye ki gum, jo ma winyo bene tye ki gum, ki jo ma lubo " +
							"gin ma gicoyo iye bene tye ki gum, pien kare doŋ cok.",
				(string)textRowForRev1V3[exporter.GetColumnIndex(ExportColumn.VernacularText)]);
			Assert.IsTrue(sectionHeadRowForRev1V3[exporter.GetColumnIndex(ExportColumn.CharacterId)].Equals(CharacterVerseData.GetStandardCharacterIdAsEnglish(CharacterVerseData.GetStandardCharacterId("REV", CharacterVerseData.StandardCharacter.ExtraBiblical))));

			//Pause for final verse in chapter (pauses come after verse text)
			var rowsForRev1V20 = data.Where(d => (string)d[exporter.GetColumnIndex(ExportColumn.BookId)] == "REV" && (int)d[exporter.GetColumnIndex(ExportColumn.Chapter)] == 1 && (string)d[exporter.GetColumnIndex(ExportColumn.Verse)] == "20").ToList();
			var textRowForRev1V20 = rowsForRev1V20.Single();
			annotationInfo = " " + string.Format(Pause.kPauseSecondsFormat, "2");
			Assert.IsTrue(((string)textRowForRev1V20[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)]).EndsWith(annotationInfo));
			Assert.IsTrue(((string)textRowForRev1V20[exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)]).EndsWith(annotationInfo));
			Assert.AreEqual("[20]\u00A0Koŋ agonnyi tyen lok me muŋ me lakalatwe abiro ma ineno i ciŋa tuŋ lacuc, ki okar-mac abiro me jabu. " +
							"Lakalatwe abiro gin aye lumalaika pa lwak muye Kricito ma gitye i kabedo abiro mapatpat, doŋ okar-mac abiro-ni gin " +
							"aye lwak muye Kricito ma gitye i kabedo abiro mapatpat.”",
				(string)textRowForRev1V20[exporter.GetColumnIndex(ExportColumn.VernacularText)]);
		}

		[Test]
		public void GetExportData_ExportAnnotationsInSeparateRows_AnnotationWithOffset_ReferenceTextContainsAnnotationInCorrectLocation()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.MRK);
			project.ReferenceText = ReferenceText.GetStandardReferenceText(ReferenceTextType.French);
			var exporter = new ProjectExporter(project);
			exporter.ExportAnnotationsInSeparateRows = true;

			var data = exporter.GetExportData().ToList();

			//Pause mid-verse
			var rowsForMark4V39 = data.Where(d => (string)d[exporter.GetColumnIndex(ExportColumn.BookId)] == "MRK" && (int)d[exporter.GetColumnIndex(ExportColumn.Chapter)] == 4 && (string)d[exporter.GetColumnIndex(ExportColumn.Verse)] == "39").ToList();
			Assert.AreEqual(4, rowsForMark4V39.Count);
			var narratorTextRow1ForMark4V39 = rowsForMark4V39[0];
			var jesusTextRowForMark4V39 = rowsForMark4V39[0];
			var annotationRowForMark4V39 = rowsForMark4V39[2];
			var narratorTextRow2ForMark4V39 = rowsForMark4V39[3];
			Assert.IsFalse(((string)narratorTextRow1ForMark4V39[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)]).Contains("|||"));
			Assert.IsFalse(((string)jesusTextRowForMark4V39[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)]).Contains("|||"));
			Assert.IsTrue(annotationRowForMark4V39[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)].Equals(annotationRowForMark4V39[exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)]) &&
				((string)annotationRowForMark4V39[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)]).Equals(string.Format(Pause.kPauseSecondsFormat, "1.5")));
			Assert.IsFalse(((string)narratorTextRow2ForMark4V39[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)]).Contains("|||"));
		}

		[Test]
		public void GetExportData_AnnotationsCombinedWithData_AnnotationWithOffset_ReferenceTextContainsAnnotationInCorrectLocation()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.MRK);
			project.ReferenceText = ReferenceText.GetStandardReferenceText(ReferenceTextType.French);
			var exporter = new ProjectExporter(project);
			// This is the default: exporter.ExportAnnotationsInSeparateRows = false;

			var data = exporter.GetExportData().ToList();

			//Pause mid-verse
			var rowsForMark4V39 = data.Where(d => (string)d[exporter.GetColumnIndex(ExportColumn.BookId)] == "MRK" && (int)d[exporter.GetColumnIndex(ExportColumn.Chapter)] == 4 && (string)d[exporter.GetColumnIndex(ExportColumn.Verse)] == "39").ToList();
			Assert.AreEqual(3, rowsForMark4V39.Count);
			var narratorTextRow1ForMark4V39 = rowsForMark4V39[0];
			var jesusTextRowForMark4V39 = rowsForMark4V39[1];
			var narratorTextRow2ForMark4V39 = rowsForMark4V39[2];
			Assert.IsFalse(((string)narratorTextRow1ForMark4V39[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)]).Contains("|||"));
			var annotationInfo = " " + string.Format(Pause.kPauseSecondsFormat, "1.5");
			Assert.IsTrue(((string)jesusTextRowForMark4V39[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)]).EndsWith(annotationInfo));
			Assert.IsTrue(((string)jesusTextRowForMark4V39[exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)]).EndsWith(annotationInfo));
			Assert.IsFalse(((string)narratorTextRow2ForMark4V39[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)]).Contains("|||"));
		}

		[Test]
		public void GeneratePreviewTable_BlocksAreJoinedToStandardNonEnglishReferenceText_HeadersIncludeNonEnglishAndEnglishDirectorsGuide()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.JUD);
			project.ReferenceText = ReferenceText.GetStandardReferenceText(ReferenceTextType.Spanish);
			var exporter = new ProjectExporter(project);

			var data = exporter.GeneratePreviewTable();

			Assert.IsTrue(data.Columns[exporter.GetColumnIndex(ExportColumn.BlockId)].ColumnName == "#");
			Assert.IsTrue(data.Columns[exporter.GetColumnIndex(ExportColumn.ParaTag)].ColumnName == "Tag");
			Assert.IsTrue(data.Columns[exporter.GetColumnIndex(ExportColumn.BookId)].ColumnName == "Book");
			Assert.IsTrue(data.Columns[exporter.GetColumnIndex(ExportColumn.Chapter)].ColumnName == "Chapter");
			Assert.IsTrue(data.Columns[exporter.GetColumnIndex(ExportColumn.Verse)].ColumnName == "Verse");
			Assert.IsTrue(data.Columns[exporter.GetColumnIndex(ExportColumn.CharacterId)].ColumnName == "Character");
			Assert.IsTrue(data.Columns[exporter.GetColumnIndex(ExportColumn.Delivery)].ColumnName == "Delivery");
			Assert.IsTrue(data.Columns[exporter.GetColumnIndex(ExportColumn.VernacularText)].ColumnName == "Text");
			Assert.IsTrue(data.Columns[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)].ColumnName == "Spanish Director's Guide");
			Assert.IsTrue(data.Columns[exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)].ColumnName == "English Director's Guide");
			Assert.IsTrue(data.Columns[exporter.GetColumnIndex(ExportColumn.VernacularTextLength)].ColumnName == "Size");
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
