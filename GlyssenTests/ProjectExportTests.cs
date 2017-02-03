﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Glyssen;
using Glyssen.Bundle;
using Glyssen.Character;
using Glyssen.Dialogs;
using Glyssen.Rules;
using Glyssen.VoiceActor;
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

		[TearDown]
		public void Teardown()
		{
			TestReferenceText.DeleteTempCustomReferenceProjectFolder();
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
				new Glyssen.VoiceActor.VoiceActor {Id = 1}
			};
			project.CharacterGroupList.CharacterGroups.AddRange(new[]
			{
				new CharacterGroup(project),
				new CharacterGroup(project)
			});
			project.CharacterGroupList.CharacterGroups[0].AssignVoiceActor(1);

			var exporter = new ProjectExporter(project);
			var data = exporter.GetExportData();
			Assert.True(data.Any());
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
		public void GetExportData_IntrosIncluded_IntroMaterialInExportData()
		{
			var expectedIntroParagraphs = Regex.Matches(Properties.Resources.TestJOS, "para style=\"i", RegexOptions.Compiled).Count;
			Assert.IsTrue(expectedIntroParagraphs > 0, "The test resource \"TestJos.xml\" has been modified to remove intro material. It won't work for this test.");
			var project = TestProject.CreateTestProject(TestProject.TestBook.JOS);
			project.DramatizationPreferences.BookIntroductionsDramatization = ExtraBiblicalMaterialSpeakerOption.Narrator;
			var exporter = new ProjectExporter(project);
			var data = exporter.GetExportData();
			Assert.IsTrue(data.Any());
			Assert.AreEqual(expectedIntroParagraphs, data.Count(t => ((string)t[1]).StartsWith("i", StringComparison.Ordinal)));
		}

		[Test]
		public void GetExportData_IntrosOmitted_NoIntroMaterialInExportData()
		{
			Assert.IsTrue(Regex.Matches(Properties.Resources.TestJOS, "para style=\"i", RegexOptions.Compiled).Count > 0,
				"The test resource \"TestJos.xml\" has been modified to remove intro material. It won't work for this test.");
			var project = TestProject.CreateTestProject(TestProject.TestBook.JOS);
			project.DramatizationPreferences.BookIntroductionsDramatization = ExtraBiblicalMaterialSpeakerOption.Omitted;
			var exporter = new ProjectExporter(project);
			var data = exporter.GetExportData();
			Assert.IsTrue(data.Any());
			Assert.IsFalse(data.Any(t => ((string)t[1]).StartsWith("i", StringComparison.Ordinal)));
		}

		[Test]
		public void GetExportData_SectionHeadsIncluded_SectionHeadsInExportData()
		{
			var expectedSectionHeadParagraphs = Regex.Matches(Properties.Resources.TestJUD, "para style=\"s", RegexOptions.Compiled).Count;
			Assert.IsTrue(expectedSectionHeadParagraphs > 0, "The test resource \"TestJud.xml\" has been modified to remove section heads. It won't work for this test.");
			var project = TestProject.CreateTestProject(TestProject.TestBook.JUD);
			project.DramatizationPreferences.SectionHeadDramatization = ExtraBiblicalMaterialSpeakerOption.Narrator;
			var exporter = new ProjectExporter(project);
			var data = exporter.GetExportData();
			Assert.IsTrue(data.Any());
			Assert.AreEqual(expectedSectionHeadParagraphs, data.Count(t => ((string)t[1]).StartsWith("s", StringComparison.Ordinal)));
		}

		[Test]
		public void GetExportData_SectionHeadsOmitted_NoSectionHeadsInExportData()
		{
			Assert.IsTrue(Regex.Matches(Properties.Resources.TestJUD, "para style=\"s", RegexOptions.Compiled).Count > 0,
				"The test resource \"TestJud.xml\" has been modified to remove section heads. It won't work for this test.");
			var project = TestProject.CreateTestProject(TestProject.TestBook.JUD);
			project.DramatizationPreferences.SectionHeadDramatization = ExtraBiblicalMaterialSpeakerOption.Omitted;
			var exporter = new ProjectExporter(project);
			var data = exporter.GetExportData();
			Assert.IsTrue(data.Any());
			Assert.IsFalse(data.Any(t => ((string)t[1]).StartsWith("s", StringComparison.Ordinal)));
		}

		[Test]
		public void GetExportData_TitlesAndChaptersIncluded_TitlesAndChaptersInExportData()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.EPH);
			var expected = project.SkipChapterAnnouncementForFirstChapter ? 6 : 7;
			project.DramatizationPreferences.BookTitleAndChapterDramatization = ExtraBiblicalMaterialSpeakerOption.Narrator;
			var exporter = new ProjectExporter(project);
			var data = exporter.GetExportData();
			Assert.IsTrue(data.Any());
			Assert.AreEqual(expected, data.Count(t => (string)t[5] == "book title or chapter (EPH)"));
		}

		[Test]
		public void GetExportData_TitlesAndChaptersOmitted_NoTitlesOrChaptersInExportData()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.EPH);
			project.DramatizationPreferences.BookTitleAndChapterDramatization = ExtraBiblicalMaterialSpeakerOption.Omitted;
			var exporter = new ProjectExporter(project);
			var data = exporter.GetExportData();
			Assert.IsTrue(data.Any());
			Assert.IsFalse(data.Any(t => ((string)t[5]) == "book title or chapter (EPH)"));
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
				new Glyssen.VoiceActor.VoiceActor {Id = 1, Name = "Marlon"}
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
		public void GetExportData_SingleVoice_DeliveryColumnOmitted()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.GAL, TestProject.TestBook.JUD);
			project.IncludedBooks[0].SingleVoice = true;
			project.IncludedBooks[1].SingleVoice = true;
			project.VoiceActorList.AllActors = new List<Glyssen.VoiceActor.VoiceActor>
			{
				new Glyssen.VoiceActor.VoiceActor {Id = 1, Name = "Marlon"},
				new Glyssen.VoiceActor.VoiceActor {Id = 2, Name = "Aiden"}
			};
			project.CharacterGroupList.CharacterGroups.AddRange(new[]
			{
				new CharacterGroup(project),
				new CharacterGroup(project)
			});
			project.CharacterGroupList.CharacterGroups[0].CharacterIds.Add(CharacterVerseData.GetStandardCharacterId("GAL", CharacterVerseData.StandardCharacter.Narrator));
			project.CharacterGroupList.CharacterGroups[0].AssignVoiceActor(1);
			project.CharacterGroupList.CharacterGroups[1].CharacterIds.Add(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.Narrator));
			project.CharacterGroupList.CharacterGroups[1].AssignVoiceActor(2);

			var exporter = new ProjectExporter(project);
			var data = exporter.GetExportData();
			Assert.IsTrue(data.Count > 2);
			Assert.IsTrue(data.All(d => d.Count == 10));
			var iColBook = exporter.GetColumnIndex(ExportColumn.BookId);
			var iColCharacter = exporter.GetColumnIndex(ExportColumn.CharacterId);
			var iColActor = exporter.GetColumnIndex(ExportColumn.Actor);
			var iStartOfJude = data.IndexOf(d => (string)d[iColBook] == "JUD");
			Assert.IsTrue(iStartOfJude > 0);
			Assert.IsTrue(data.Take(iStartOfJude).All(d => (string)d[iColBook] == "GAL"));
			Assert.IsTrue(data.Skip(iStartOfJude).All(d => (string)d[iColBook] == "JUD"));
			Assert.IsTrue(data.Take(iStartOfJude).All(d => (string)d[iColCharacter] == "narrator (GAL)"));
			Assert.IsTrue(data.Skip(iStartOfJude).All(d => (string)d[iColCharacter] == "narrator (JUD)"));
			Assert.IsTrue(data.Take(iStartOfJude).All(d => (string)d[iColActor] == "Marlon"));
			Assert.IsTrue(data.Skip(iStartOfJude).All(d => (string)d[iColActor] == "Aiden"));
		}

		[Test]
		public void GetExportData_BlocksAreJoinedToReferenceText_OutputContainsMatchedAndUnmatchedReferenceText()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.JUD);
			project.DramatizationPreferences.SectionHeadDramatization = ExtraBiblicalMaterialSpeakerOption.ActorOfEitherGender;
			var narrator = CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.Narrator);
			var sectionHead = CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.ExtraBiblical);
			var jude = project.IncludedBooks.Single();
			jude.Blocks = new List<Block>(new[]
			{
				new Block("p", 1, 1) {CharacterId = narrator}.AddVerse("1", "A"),
				new Block("s", 1, 1) {CharacterId = sectionHead, BlockElements = new List<BlockElement> {new ScriptText("Jude complains")}},
				new Block("p", 1, 2) {CharacterId = "Enoch"}.AddVerse("2", "B"),
				new Block("p", 1, 3) {CharacterId = narrator}.AddVerse("3", "C"),
				new Block("p", 1, 4) {CharacterId = "Michael"}.AddVerse("4", "D"),
				new Block("p", 1, 5) {CharacterId = narrator}.AddVerse("5", "E ").AddVerse("6", "F"),
			});

			var blocks = new List<Block>
			{
				new Block("p", 1, 1) {CharacterId = narrator}.AddVerse("1", "Ayy"),
				new Block("p", 1, 2, 3) {CharacterId = narrator}.AddVerse("2-3", "Bee Cee"),
				new Block("p", 1, 4) {CharacterId = "Michael"}.AddVerse(4, "Dee, "),
				new Block("p", 1, 4) {CharacterId = narrator, BlockElements = new List<BlockElement> {new ScriptText("Michael said.")}},
				new Block("p", 1, 5) {CharacterId = narrator}.AddVerse(5, "Ey"),
				new Block("p", 1, 6) {CharacterId = narrator}.AddVerse(6, "Ef"),
			};
			foreach (var refBlock in blocks)
			{
				var secondaryRefBlock = new Block(refBlock.StyleTag, refBlock.ChapterNumber, refBlock.InitialStartVerseNumber, refBlock.InitialEndVerseNumber)
					{CharacterId = refBlock.CharacterId};
				Verse verseElement = refBlock.BlockElements.First() as Verse;
				if (verseElement != null)
					secondaryRefBlock.AddVerse(verseElement.Number, "Secondary");
				else
					secondaryRefBlock.BlockElements = new List<BlockElement> {new ScriptText("the angel named Mike verbalized.")};
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
			Assert.AreEqual("{1}\u00A0A", row[exporter.GetColumnIndex(ExportColumn.VernacularText)]);
			Assert.AreEqual("{1}\u00A0Ayy", row[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)]);
			Assert.AreEqual("{1}\u00A0Secondary", row[exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)]);

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
			Assert.AreEqual("{2}\u00A0B", row[exporter.GetColumnIndex(ExportColumn.VernacularText)]);
			Assert.IsTrue(string.IsNullOrEmpty(row[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)] as string));
			Assert.IsTrue(string.IsNullOrEmpty(row[exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)] as string));

			row = data[i++];
			Assert.AreEqual(i, row[exporter.GetColumnIndex(ExportColumn.BlockId)]); // Row 4
			Assert.AreEqual("p", row[exporter.GetColumnIndex(ExportColumn.ParaTag)]);
			Assert.AreEqual("3", row[exporter.GetColumnIndex(ExportColumn.Verse)]);
			Assert.AreEqual("narrator (JUD)", row[exporter.GetColumnIndex(ExportColumn.CharacterId)]);
			Assert.AreEqual("{3}\u00A0C", row[exporter.GetColumnIndex(ExportColumn.VernacularText)]);
			Assert.IsTrue(string.IsNullOrEmpty(row[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)] as string));
			Assert.IsTrue(string.IsNullOrEmpty(row[exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)] as string));

			row = data[i++];
			Assert.IsTrue(string.IsNullOrEmpty(row[exporter.GetColumnIndex(ExportColumn.BlockId)] as string));
			Assert.AreEqual("p", row[exporter.GetColumnIndex(ExportColumn.ParaTag)]);
			Assert.AreEqual("2-3", row[exporter.GetColumnIndex(ExportColumn.Verse)]);
			Assert.AreEqual("narrator (JUD)", row[exporter.GetColumnIndex(ExportColumn.CharacterId)]);
			Assert.IsTrue(string.IsNullOrEmpty(row[exporter.GetColumnIndex(ExportColumn.VernacularText)] as string));
			Assert.AreEqual("{2-3}\u00A0Bee Cee", row[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)]);
			Assert.AreEqual("{2-3}\u00A0Secondary", row[exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)]);
			Assert.AreEqual(0, row[exporter.GetColumnIndex(ExportColumn.VernacularTextLength)]);

			row = data[i++];
			Assert.AreEqual(5, row[exporter.GetColumnIndex(ExportColumn.BlockId)]); // Row 5
			Assert.AreEqual("p", row[exporter.GetColumnIndex(ExportColumn.ParaTag)]);
			Assert.AreEqual("4", row[exporter.GetColumnIndex(ExportColumn.Verse)]);
			Assert.AreEqual("Michael", row[exporter.GetColumnIndex(ExportColumn.CharacterId)]);
			Assert.AreEqual("{4}\u00A0D", row[exporter.GetColumnIndex(ExportColumn.VernacularText)]);
			Assert.AreEqual("{4}\u00A0Dee, Michael said.", row[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)]);
			Assert.AreEqual("{4}\u00A0Secondary the angel named Mike verbalized.", row[exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)]);

			row = data[i++];
			Assert.AreEqual(6, row[exporter.GetColumnIndex(ExportColumn.BlockId)]);
			Assert.AreEqual("p", row[exporter.GetColumnIndex(ExportColumn.ParaTag)]);
			Assert.AreEqual("5", row[exporter.GetColumnIndex(ExportColumn.Verse)]);
			Assert.AreEqual("narrator (JUD)", row[exporter.GetColumnIndex(ExportColumn.CharacterId)]);
			Assert.AreEqual("{5}\u00A0E ", row[exporter.GetColumnIndex(ExportColumn.VernacularText)]);
			Assert.AreEqual("{5}\u00A0Ey", row[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)]);
			Assert.AreEqual("{5}\u00A0Secondary", row[exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)]);

			row = data[i++];
			Assert.AreEqual(7, row[exporter.GetColumnIndex(ExportColumn.BlockId)]);
			Assert.AreEqual("p", row[exporter.GetColumnIndex(ExportColumn.ParaTag)]);
			Assert.AreEqual("6", row[exporter.GetColumnIndex(ExportColumn.Verse)]);
			Assert.AreEqual("narrator (JUD)", row[exporter.GetColumnIndex(ExportColumn.CharacterId)]);
			Assert.AreEqual("{6}\u00A0F", row[exporter.GetColumnIndex(ExportColumn.VernacularText)]);
			Assert.AreEqual("{6}\u00A0Ef", row[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)]);
			Assert.AreEqual("{6}\u00A0Secondary", row[exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)]);

			Assert.AreEqual(i, data.Count);
		}

		[Test]
		public void GetExportData_AnnotationWithNegativeOffsetInVerseThatDoesNotMatchReferenceText_AnnotationInsertedCorrectlyWithoutCrashing()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.MRK);
			var narrator = CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.Narrator);
			var mark = project.IncludedBooks.Single();
			mark.Blocks = new List<Block>
			{
				new Block("p", 4, 39) {IsParagraphStart = true, CharacterId = narrator}.AddVerse(39, "Jedus stanop, taak scrong ta de big breeze say, "),
				new Block("p", 4, 39) {CharacterId = "Jesus", Delivery = "forcefully", BlockElements = new List<BlockElement> {new ScriptText("“Hush, stop blow.” ")}},
				new Block("p", 4, 39) {CharacterId = narrator, BlockElements = new List<BlockElement> {new ScriptText("An e say ta de swellin wata, ")}},
				new Block("p", 4, 39) {CharacterId = "Jesus", Delivery = "forcefully", BlockElements = new List<BlockElement> {new ScriptText("“Go down.” ")}},
				new Block("p", 4, 39)
				{
					CharacterId = narrator, BlockElements = new List<BlockElement>
					{
						new ScriptText("De big breeze done hush an stop fa blow, an de swellin wata gone down an been peaceable an steady. "),
						new Verse("40"),
						new ScriptText("Den Jedus ton roun ta e ciple dem an e say, ")
					}
				},
				new Block("p", 4, 40)
				{
					CharacterId = "Jesus", Delivery = "questioning", BlockElements = new List<BlockElement>
					{
						new ScriptText("“Hoccome oona so scaid? Stillyet oona ain bleebe pon God, ainty?”")
					}
				}
			};

			var refBlocks = new List<Block>
			{
				new Block("p", 4, 39) {IsParagraphStart = true, CharacterId = narrator}.AddVerse(39, "He awoke, and rebuked the wind, and said to the sea, "),
				new Block("p", 4, 39) {CharacterId = "Jesus", BlockElements = new List<BlockElement> {new ScriptText("“Peace! Be still!” ")}},
				new Block("p", 4, 39)
				{
					CharacterId = narrator, BlockElements = new List<BlockElement>
					{
						new ScriptText("The wind ceased, and there was a great calm. "),
						new Verse("40"),
						new ScriptText("He said to them, ")
					}
				},
				new Block("p", 4, 40)
				{
					CharacterId = "Jesus", Delivery = "questioning", BlockElements = new List<BlockElement>
					{
						new ScriptText("“Why are you so afraid? How is it that you have no faith?”")
					}
				}
			};
			foreach (var refBlock in refBlocks)
			{
				var secondaryRefBlock = new Block(refBlock.StyleTag, refBlock.ChapterNumber, refBlock.InitialStartVerseNumber, refBlock.InitialEndVerseNumber) {CharacterId = refBlock.CharacterId};
				Verse verseElement = refBlock.BlockElements.First() as Verse;
				if (verseElement != null)
					secondaryRefBlock.AddVerse(verseElement.Number, "Some secondary reference text");
				else
					secondaryRefBlock.BlockElements = new List<BlockElement> {new ScriptText("Some secondary reference text")};
				refBlock.SetMatchedReferenceBlock(secondaryRefBlock);
			}
			var primaryReferenceText = TestReferenceText.CreateTestReferenceText("MRK", refBlocks);
			project.ReferenceText = primaryReferenceText;
			var exporter = new ProjectExporter(project);
			var data = exporter.GetExportData().ToList();
			Assert.AreEqual(mark.Blocks.Count, data.Count);

			var narratorInOutput = CharacterVerseData.GetCharacterNameForUi(narrator);

			Assert.IsTrue(data.All(d => (string)d[exporter.GetColumnIndex(ExportColumn.BookId)] == "MRK" && (int)d[exporter.GetColumnIndex(ExportColumn.Chapter)] == 4));
			var i = 0;
			var row = data[i];
			Assert.AreEqual("39", row[exporter.GetColumnIndex(ExportColumn.Verse)]);
			Assert.AreEqual(narratorInOutput, row[exporter.GetColumnIndex(ExportColumn.CharacterId)]);
			Assert.AreEqual(mark.GetScriptBlocks()[i].GetText(true), row[exporter.GetColumnIndex(ExportColumn.VernacularText)]);
			Assert.AreEqual(refBlocks[i].GetText(true), row[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)]);

			row = data[++i];
			Assert.AreEqual("39", row[exporter.GetColumnIndex(ExportColumn.Verse)]);
			Assert.AreEqual("Jesus", row[exporter.GetColumnIndex(ExportColumn.CharacterId)]);
			Assert.AreEqual(mark.GetScriptBlocks()[i].GetText(true), row[exporter.GetColumnIndex(ExportColumn.VernacularText)]);
			Assert.AreEqual(refBlocks[i].GetText(true) + "||| + 1.5 SECs |||", row[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)]);
			Assert.AreEqual("Some secondary reference text ||| + 1.5 SECs |||", row[exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)]);

			row = data[++i];
			Assert.AreEqual("39", row[exporter.GetColumnIndex(ExportColumn.Verse)]);
			Assert.AreEqual(narratorInOutput, row[exporter.GetColumnIndex(ExportColumn.CharacterId)]);
			Assert.AreEqual(mark.GetScriptBlocks()[i].GetText(true), row[exporter.GetColumnIndex(ExportColumn.VernacularText)]);
			Assert.IsTrue(string.IsNullOrEmpty(row[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)] as string));

			row = data[++i];
			Assert.AreEqual("39", row[exporter.GetColumnIndex(ExportColumn.Verse)]);
			Assert.AreEqual("Jesus", row[exporter.GetColumnIndex(ExportColumn.CharacterId)]);
			Assert.AreEqual(mark.GetScriptBlocks()[i].GetText(true), row[exporter.GetColumnIndex(ExportColumn.VernacularText)]);
			Assert.IsTrue(string.IsNullOrEmpty(row[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)] as string));

			row = data[++i];
			Assert.IsTrue(string.IsNullOrEmpty(row[exporter.GetColumnIndex(ExportColumn.BlockId)] as string));
			Assert.AreEqual("39", row[exporter.GetColumnIndex(ExportColumn.Verse)]);
			Assert.AreEqual(narratorInOutput, row[exporter.GetColumnIndex(ExportColumn.CharacterId)]);
			Assert.AreEqual(mark.GetScriptBlocks()[i].GetText(true), row[exporter.GetColumnIndex(ExportColumn.VernacularText)]);
			Assert.AreEqual(refBlocks[i - 2].GetText(true), row[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)]);

			row = data[++i];
			Assert.IsTrue(string.IsNullOrEmpty(row[exporter.GetColumnIndex(ExportColumn.BlockId)] as string));
			Assert.AreEqual("40", row[exporter.GetColumnIndex(ExportColumn.Verse)]);
			Assert.AreEqual("Jesus", row[exporter.GetColumnIndex(ExportColumn.CharacterId)]);
			Assert.AreEqual(mark.GetScriptBlocks()[i].GetText(true), row[exporter.GetColumnIndex(ExportColumn.VernacularText)]);
			Assert.AreEqual(refBlocks[i - 2].GetText(true), row[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)]);
		}

		[Test]
		public void GetExportData_BlocksAreJoinedToCustomReferenceTextWhosePageHeaderIsDifferentFromTheMainTitle_ChapterAnnouncementBasedOnPageHeader()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.JUD);
			project.ReferenceText = TestReferenceText.CreateCustomReferenceText(TestReferenceText.TestReferenceTextResource.AzeriJUD);
			var metadata = (GlyssenDblTextMetadata)ReflectionHelper.GetField(project, "m_metadata");
			metadata.IncludeChapterAnnouncementForFirstChapter = true;
			metadata.IncludeChapterAnnouncementForSingleChapterBooks = true;
			var exporter = new ProjectExporter(project);

			var data = exporter.GetExportData().ToList();

			Assert.IsTrue(data.All(d => d.Count == 11));
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
			// Since the test version of Jude does not match perfectly with this reference text, we expect two rows
			// where the vernacular has no corresponding reference text.
			var extra = CharacterVerseData.GetCharacterNameForUi(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.ExtraBiblical));
			var narrator = CharacterVerseData.GetCharacterNameForUi(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.Narrator));
			Assert.IsTrue(data.Where(d => d[exporter.GetColumnIndex(ExportColumn.ParaTag)] as string == "s1")
				.All(d => d[exporter.GetColumnIndex(ExportColumn.CharacterId)] as string == extra));
			var scriptureRowsWithNoReferenceText = data.Where(d => d[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)] == null &&
				d[exporter.GetColumnIndex(ExportColumn.ParaTag)] as string != "s1").ToList();
			Assert.AreEqual(2, scriptureRowsWithNoReferenceText.Count);
			Assert.AreEqual(1, scriptureRowsWithNoReferenceText.Count(d => d[exporter.GetColumnIndex(ExportColumn.CharacterId)] as string == narrator));
			Assert.IsTrue(scriptureRowsWithNoReferenceText.All(d => d[exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)] == null));
		}

		[Test]
		public void GetExportData_BlocksAreJoinedToStandardNonEnglishReferenceText_OutputContainsPrimaryAndEnglishReferenceText()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.JUD);
			project.ReferenceText = ReferenceText.GetStandardReferenceText(ReferenceTextType.Russian);
			var metadata = (GlyssenDblTextMetadata)ReflectionHelper.GetField(project, "m_metadata");
			metadata.IncludeChapterAnnouncementForFirstChapter = true;
			metadata.IncludeChapterAnnouncementForSingleChapterBooks = true;
			var exporter = new ProjectExporter(project);

			var data = exporter.GetExportData().ToList();

			Assert.IsTrue(data.All(d => d.Count == 11));
			Assert.IsTrue(data.All(d => (string)d[exporter.GetColumnIndex(ExportColumn.BookId)] == "JUD"));
			Assert.IsTrue(data.All(d => d.Count == exporter.GetColumnIndex(ExportColumn.VernacularTextLength) + 1));
			Assert.AreEqual("Иуда", data[0][exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)]);
			Assert.AreEqual("JUDE", data[0][exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)]);
			Assert.IsTrue(data.Skip(1).All(d => (int)d[exporter.GetColumnIndex(ExportColumn.Chapter)] == 1));
			Assert.AreEqual("Иуда 1", data[1][exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)]);
			Assert.AreEqual("JUDE CHP 1", data[1][exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)]);
			var matchedRows = data.Where(d => (string)d[exporter.GetColumnIndex(ExportColumn.VernacularText)] != null && (string)d[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)] != null).ToList();
			Assert.IsTrue(matchedRows.Count > data.Count / 2); // This is kind of arbitrary, but I just want to say we got a reasonable number of matches
			Assert.IsTrue(matchedRows.Any(d => ((string)d[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)]).Contains("п"))); // A letter that should be in Russian, but not English
			Assert.IsTrue(matchedRows.All(d => (string)d[exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)] != null));
			Assert.IsTrue(matchedRows.Any(d => ((string)d[exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)]).Contains(" the "))); // A word that should be in English, but not Russian
			// Since the test version of Jude does not match perfectly with the standard reference texts, we expect two Scripture rows
			// where the vernacular has no corresponding reference text.
			var extra = CharacterVerseData.GetCharacterNameForUi(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.ExtraBiblical));
			var narrator = CharacterVerseData.GetCharacterNameForUi(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.Narrator));
			Assert.IsTrue(data.Where(d => d[exporter.GetColumnIndex(ExportColumn.ParaTag)] as string == "s1")
				.All(d => d[exporter.GetColumnIndex(ExportColumn.CharacterId)] as string == extra));
			var scriptureRowsWithNoReferenceText = data.Where(d => d[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)] == null &&
				d[exporter.GetColumnIndex(ExportColumn.ParaTag)] as string != "s1").ToList();
			Assert.AreEqual(2, scriptureRowsWithNoReferenceText.Count);
			Assert.AreEqual(1, scriptureRowsWithNoReferenceText.Count(d => d[exporter.GetColumnIndex(ExportColumn.CharacterId)] as string == narrator));
			Assert.IsTrue(scriptureRowsWithNoReferenceText.All(d => d[exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)] == null));
		}

		[Test]
		public void GetExportData_ExportAnnotationsInSeparateRows_ReferenceTextsContainAnnotations()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.JUD, TestProject.TestBook.REV);
			project.DramatizationPreferences.SectionHeadDramatization = ExtraBiblicalMaterialSpeakerOption.ActorOfEitherGender;
			project.ReferenceText = TestReferenceText.CreateCustomReferenceText(TestReferenceText.TestReferenceTextResource.AzeriJUD, TestReferenceText.TestReferenceTextResource.AzeriREV);
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
			project.DramatizationPreferences.SectionHeadDramatization = ExtraBiblicalMaterialSpeakerOption.ActorOfEitherGender;
			project.ReferenceText = TestReferenceText.CreateCustomReferenceText(TestReferenceText.TestReferenceTextResource.AzeriJUD, TestReferenceText.TestReferenceTextResource.AzeriREV);
			var exporter = new ProjectExporter(project) {SelectedFileType = exportFileType};
			// This is the default: exporter.ExportAnnotationsInSeparateRows = false;

			var data = exporter.GetExportData().ToList();

			//SFX (music/sfx come before verse text)
			var rowsForVerse12 = data.Where(d => (string)d[exporter.GetColumnIndex(ExportColumn.BookId)] == "JUD" && (int)d[exporter.GetColumnIndex(ExportColumn.Chapter)] == 1 && (string)d[exporter.GetColumnIndex(ExportColumn.Verse)] == "12").ToList();
			var textRowForVerse12 = rowsForVerse12.Single();
			var annotationInfoPlusVerseNum = Sound.kDoNotCombine + exporter.AnnotationElementSeparator + "{SFX--Eerie--Starts @ v12} {12}\u00A0";
			Assert.IsTrue(((string)textRowForVerse12[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)]).StartsWith(annotationInfoPlusVerseNum));
			Assert.IsTrue(((string)textRowForVerse12[exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)]).StartsWith(annotationInfoPlusVerseNum));
			Assert.AreEqual("{12}\u00A0Gikelo lewic i karamawu me mar ka gicamo matek mukato kare laboŋo lworo, kun giparo pi komgi keken. " +
				"Gubedo calo pol ma pii pe iye ma yamo kolo; girom ki yadi ma nyiggi pe nen i kare me cekgi, ma giputo lwitgi woko, " +
				"yam guto kiryo. ",
				(string)textRowForVerse12[exporter.GetColumnIndex(ExportColumn.VernacularText)]);

			//Pause for final verse in book (pauses come after verse text)
			var rowsForJude25 = data.Where(d => (string)d[exporter.GetColumnIndex(ExportColumn.BookId)] == "JUD" && (int)d[exporter.GetColumnIndex(ExportColumn.Chapter)] == 1 && (string)d[exporter.GetColumnIndex(ExportColumn.Verse)] == "25").ToList();
			var textRowForJude25 = rowsForJude25.Single();
			var annotationInfo = " " + string.Format(Pause.kPauseSecondsFormat, "5");
			Assert.IsTrue(((string)textRowForJude25[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)]).EndsWith(annotationInfo));
			Assert.IsTrue(((string)textRowForJude25[exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)]).EndsWith(annotationInfo));
			Assert.AreEqual("{25}\u00A0Deyo, dit, loc ki twer ducu obed bot Lubaŋa acel keken, ma Lalarwa, pi Yecu Kricito Rwotwa, " +
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
			Assert.AreEqual("{3}\u00A0Ŋat ma kwano lok ma gitito i buk man i nyim lwak tye ki gum, jo ma winyo bene tye ki gum, ki jo ma lubo " +
				"gin ma gicoyo iye bene tye ki gum, pien kare doŋ cok.",
				(string)textRowForRev1V3[exporter.GetColumnIndex(ExportColumn.VernacularText)]);
			Assert.IsTrue(sectionHeadRowForRev1V3[exporter.GetColumnIndex(ExportColumn.CharacterId)].Equals(CharacterVerseData.GetStandardCharacterIdAsEnglish(CharacterVerseData.GetStandardCharacterId("REV", CharacterVerseData.StandardCharacter.ExtraBiblical))));

			//Pause for final verse in chapter (pauses come after verse text)
			var rowsForRev1V20 = data.Where(d => (string)d[exporter.GetColumnIndex(ExportColumn.BookId)] == "REV" && (int)d[exporter.GetColumnIndex(ExportColumn.Chapter)] == 1 && (string)d[exporter.GetColumnIndex(ExportColumn.Verse)] == "20").ToList();
			var textRowForRev1V20 = rowsForRev1V20.Single();
			annotationInfo = " " + string.Format(Pause.kPauseSecondsFormat, "2");
			Assert.IsTrue(((string)textRowForRev1V20[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)]).EndsWith(annotationInfo));
			Assert.IsTrue(((string)textRowForRev1V20[exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)]).EndsWith(annotationInfo));
			Assert.AreEqual("{20}\u00A0Koŋ agonnyi tyen lok me muŋ me lakalatwe abiro ma ineno i ciŋa tuŋ lacuc, ki okar-mac abiro me jabu. " +
				"Lakalatwe abiro gin aye lumalaika pa lwak muye Kricito ma gitye i kabedo abiro mapatpat, doŋ okar-mac abiro-ni gin " +
				"aye lwak muye Kricito ma gitye i kabedo abiro mapatpat.”",
				(string)textRowForRev1V20[exporter.GetColumnIndex(ExportColumn.VernacularText)]);
		}
		
		/// <summary>
		/// PG-905
		/// </summary>
		[Test]
		public void GetExportData_NullPrimaryReferenceTextForAppendedAnnotation_PrimaryReferenceTextContainsOnlyAnnotation()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.REV);
			project.DramatizationPreferences.SectionHeadDramatization = ExtraBiblicalMaterialSpeakerOption.ActorOfEitherGender;
			project.ReferenceText = TestReferenceText.CreateCustomReferenceText(TestReferenceText.TestReferenceTextResource.AzeriREV);

			var rev =project.IncludedBooks.First();

			// Force a block that has an appended annotation to have a null primary reference.
			foreach (var block in rev.GetScriptBlocks().Where(b => b.ChapterNumber == 22 && b.InitialStartVerseNumber == 17))
				block.SetMatchedReferenceBlock("Get, sәn dә elә et");

			var exporter = new ProjectExporter(project) { SelectedFileType = ExportFileType.Excel };

			var data = exporter.GetExportData().ToList();

			//Pause for final verse in chapter (pauses come after verse text)
			var rowsForRev22V17 = data.Where(d => (string)d[exporter.GetColumnIndex(ExportColumn.BookId)] == "REV" && (int)d[exporter.GetColumnIndex(ExportColumn.Chapter)] == 22 && (string)d[exporter.GetColumnIndex(ExportColumn.Verse)] == "17").ToList();
			var textRowForRev22V17 = rowsForRev22V17.Last();
			var annotationInfo = " " + string.Format(Pause.kPauseSecondsFormat, "2");
			Assert.IsTrue(((string)textRowForRev22V17[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)]).EndsWith(annotationInfo));
			Assert.AreEqual(annotationInfo, (string)textRowForRev22V17[exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)]);
		}

		/// <summary>
		/// PG-905
		/// </summary>
		[Test]
		public void GetExportData_NullPrimaryReferenceTextForPrependedAnnotation_PrimaryReferenceTextContainsOnlyAnnotation()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.REV);
			project.DramatizationPreferences.SectionHeadDramatization = ExtraBiblicalMaterialSpeakerOption.ActorOfEitherGender;
			project.ReferenceText = TestReferenceText.CreateCustomReferenceText(TestReferenceText.TestReferenceTextResource.AzeriREV);

			// Force a block that has a prepended annotation to have a null primary reference.
			var vernBlock = project.IncludedBooks.First().GetScriptBlocks().Single(b => b.ChapterNumber == 1 && b.InitialStartVerseNumber == 7);
			vernBlock.SetMatchedReferenceBlock("{7}Verse Seven in Azeri.");

			var exporter = new ProjectExporter(project) { SelectedFileType = ExportFileType.Excel };

			var data = exporter.GetExportData().ToList();

			//SFX (music/sfx come before verse text)
			var rowsForRev1V7 = data.Where(d => (string)d[exporter.GetColumnIndex(ExportColumn.BookId)] == "REV" && (int)d[exporter.GetColumnIndex(ExportColumn.Chapter)] == 1 && (string)d[exporter.GetColumnIndex(ExportColumn.Verse)] == "7").ToList();
			var textRowForRev1V7 = rowsForRev1V7.Single();
			var annotationInfo = Sound.kDoNotCombine + exporter.AnnotationElementSeparator + "{Music--Starts @ v7} ";
			Assert.IsTrue(((string)textRowForRev1V7[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)]).StartsWith(annotationInfo));
			Assert.AreEqual(annotationInfo, (string)textRowForRev1V7[exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)]);
		}

		[Test]
		public void GetExportData_ExportAnnotationsInSeparateRows_AnnotationWithOffset_ReferenceTextContainsAnnotationInCorrectLocation()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.MRK);
			project.ReferenceText = TestReferenceText.CreateCustomReferenceText(TestReferenceText.TestReferenceTextResource.FrenchMRK);
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
			project.ReferenceText = TestReferenceText.CreateCustomReferenceText(TestReferenceText.TestReferenceTextResource.FrenchMRK);
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

		[TestCase(true)]
		[TestCase(false)]
		public void GeneratePreviewTable_BlocksAreJoinedToStandardNonEnglishReferenceText_HeadersIncludeNonEnglishAndEnglishDirectorsGuide(bool includeClipColumn)
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.JUD);
			project.ReferenceText = ReferenceText.GetStandardReferenceText(ReferenceTextType.Russian);
			var exporter = new ProjectExporter(project);
			exporter.IncludeCreateClips = includeClipColumn;

			var data = exporter.GeneratePreviewTable();

			Assert.IsTrue(data.Columns[exporter.GetColumnIndex(ExportColumn.BlockId)].ColumnName == "#");
			Assert.IsTrue(data.Columns[exporter.GetColumnIndex(ExportColumn.ParaTag)].ColumnName == "Tag");
			Assert.IsTrue(data.Columns[exporter.GetColumnIndex(ExportColumn.BookId)].ColumnName == "Book");
			Assert.IsTrue(data.Columns[exporter.GetColumnIndex(ExportColumn.Chapter)].ColumnName == "Chapter");
			Assert.IsTrue(data.Columns[exporter.GetColumnIndex(ExportColumn.Verse)].ColumnName == "Verse");
			Assert.IsTrue(data.Columns[exporter.GetColumnIndex(ExportColumn.CharacterId)].ColumnName == "Character");
			Assert.IsTrue(data.Columns[exporter.GetColumnIndex(ExportColumn.Delivery)].ColumnName == "Delivery");
			Assert.IsTrue(data.Columns[exporter.GetColumnIndex(ExportColumn.VernacularText)].ColumnName == "Text");
			Assert.IsTrue(data.Columns[exporter.GetColumnIndex(ExportColumn.PrimaryReferenceText)].ColumnName == "Russian Director's Guide");
			Assert.IsTrue(data.Columns[exporter.GetColumnIndex(ExportColumn.SecondaryReferenceText)].ColumnName == "English Director's Guide");
			Assert.IsTrue(data.Columns[exporter.GetColumnIndex(ExportColumn.VernacularTextLength)].ColumnName == "Size");
			if (includeClipColumn)
			{
				Assert.IsTrue(data.Columns[exporter.GetColumnIndex(ExportColumn.ClipFileLink)].ColumnName == "Clip File");
				Assert.AreEqual(12, data.Columns.Count);
			}
			else
				Assert.AreEqual(11, data.Columns.Count);
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

			var actor = new Glyssen.VoiceActor.VoiceActor {Name = "ActorGuy1"};

			int textLength = "Text of verse one. ".Length + "Text of verse two.".Length;
			var expectedLine = new StringBuilder("0\tp\tMRK\t4\t1\tFred\tWith great gusto and quivering frustration\t{1}\u00A0Text of verse one. {2}\u00A0Text of verse two.\t\t");
			if (includeSecondaryReferenceText)
				expectedLine.Append("\t");
			expectedLine.Append(textLength);
			Assert.AreEqual(expectedLine.ToString(),
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", null, null, true, true, includeSecondaryReferenceText, null, null)));
			expectedLine.Insert(1, "\tActorGuy1");
			Assert.AreEqual(expectedLine.ToString(),
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", actor, null, true, true, includeSecondaryReferenceText, null, null)));
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

			var actor = new Glyssen.VoiceActor.VoiceActor {Name = "ActorGuy1"};

			int textLength = "Text of verse three, part two. ".Length + "Text of verse four. ".Length + "Text of verse five.".Length;
			var expectedLine = new StringBuilder("0\tp\tMRK\t4\t3\t\t\tText of verse three, part two. {4}\u00A0Text of verse four. {5}\u00A0Text of verse five.\t\t");
			if (includeSecondaryReferenceText)
				expectedLine.Append("\t");
			expectedLine.Append(textLength);
			Assert.AreEqual(expectedLine.ToString(),
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", null, null, true, true, includeSecondaryReferenceText, null, null)));
			expectedLine.Insert(1, "\tActorGuy1");
			Assert.AreEqual(expectedLine.ToString(),
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", actor, null, true, true, includeSecondaryReferenceText, null, null)));
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

			var actor = new Glyssen.VoiceActor.VoiceActor {Name = "ActorGuy1"};

			int textLength = "Text of verse one. ".Length + "Text of verse two.".Length;
			var expectedLine = new StringBuilder("0\tp\tMRK\t4\t1\tnarrator (MRK)\tWith great gusto and quivering frustration\t{1}\u00A0Text of verse one. {2}\u00A0Text of verse two.\t\t");
			if (includeSecondaryReferenceText)
				expectedLine.Append("\t");
			expectedLine.Append(textLength);
			Assert.AreEqual(expectedLine.ToString(),
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", null, "narrator-MRK", true, true, includeSecondaryReferenceText, null, null)));
			expectedLine.Insert(1, "\tActorGuy1");
			Assert.AreEqual(expectedLine.ToString(),
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", actor, "narrator-MRK", true, true, includeSecondaryReferenceText, null, null)));
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

			var actor = new Glyssen.VoiceActor.VoiceActor {Name = "ActorGuy1"};

			int textLength = "Text of verse one. ".Length + "Text of verse two.".Length;
			var expectedLine = new StringBuilder("0\tp\tMRK\t4\t1\tMarko\tWith great gusto and quivering frustration\t{1}\u00A0Text of verse one. {2}\u00A0Text of verse two.\t\t");
			expectedLine.Append(textLength);
			Assert.AreEqual(expectedLine.ToString(),
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", null, null, true, true, false, null, null)));
			expectedLine.Insert(1, "\tActorGuy1");
			Assert.AreEqual(expectedLine.ToString(),
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", actor, null, true, true, false, null, null)));
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

			var actor = new Glyssen.VoiceActor.VoiceActor {Name = "ActorGuy1"};

			int textLength = "Text of verse one. ".Length + "Text of verse two.".Length;
			var expectedLine = new StringBuilder("0\tp\tMRK\t4\t1\tFred/Marko\tWith great gusto and quivering frustration\t{1}\u00A0Text of verse one. {2}\u00A0Text of verse two.\t\t");
			expectedLine.Append(textLength);
			Assert.AreEqual(expectedLine.ToString(),
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", null, null, false, true, false, null, null)));
			expectedLine.Insert(1, "\tActorGuy1");
			Assert.AreEqual(expectedLine.ToString(),
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", actor, null, false, true, false, null, null)));
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

			var actor = new Glyssen.VoiceActor.VoiceActor {Name = "ActorGuy1"};

			int textLength = "Text of verse one. ".Length + "Text of verse two.".Length;
			var expectedLine = new StringBuilder("0\tActorGuy1\tp\tMRK\t4\t1\tFred\tWith great gusto and quivering frustration\t{1}\u00A0Text of verse one. {2}\u00A0Text of verse two.\t{1-2}\u00A0Text of verses one and two bridged in harmony and goodness.\t");
			if (includeSecondaryReferenceText)
				expectedLine.Append("\t");
			expectedLine.Append(textLength);
			Assert.AreEqual(expectedLine.ToString(),
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", actor, null, true, true, includeSecondaryReferenceText, null, null)));
		}

		[Test]
		public void GetExportDataForBlock_IncludeClipFiles_LastElementIsClipFile()
		{
			var block = new Block("p", 4);
			block.IsParagraphStart = true;
			block.CharacterId = "Fred";
			block.BlockElements.Add(new Verse("1-2"));
			block.BlockElements.Add(new ScriptText("Texto de versos uno y dos. "));
			block.SetMatchedReferenceBlock(new Block("p", 4, 1, 2).AddVerse("1-2", "Text of verses one and two bridged in harmony and goodness."));

			var actor = new Glyssen.VoiceActor.VoiceActor {Name = "ActorGuy1"};

			var data = ProjectExporter.GetExportDataForBlock(block, 465, "MRK", actor, null, true, true, true, @"c:\wherever\whenever\however", "MyProject");
			Assert.AreEqual(13, data.Count);
			Assert.AreEqual(@"c:\wherever\whenever\however\MRK\MyProject_00465_MRK_004_001.wav", data.Last());
		}

		/// <summary>
		/// PG-855: Generating actor script files shouldn't fail
		/// </summary>
		[Test]
		public void ExportNow_ExportActorExcelScripts_ScriptsCreated()
		{
			var project = TestProject.CreateBasicTestProject();
			TestProject.SimulateDisambiguationForAllBooks(project);
			var generator = new CharacterGroupGenerator(project, new CastSizeRowValues(4, 1, 0));
			generator.GenerateCharacterGroups();
			generator.ApplyGeneratedGroupsToProject();

			int i = 1;
			foreach (var group in project.CharacterGroupList.CharacterGroups)
			{
				var actor = (group.ContainsCharacterWithGender(CharacterGender.Female)) ?
					new Glyssen.VoiceActor.VoiceActor() { Id = i, Gender = ActorGender.Female, Name = "Judy" + i++ } :
					new Glyssen.VoiceActor.VoiceActor() { Id = i, Gender = ActorGender.Male, Name = "Bob" + i++ };
				project.VoiceActorList.AllActors.Add(actor);
				group.VoiceActorId = actor.Id;
			}
			Assert.IsTrue(project.CharacterGroupList.AnyVoiceActorAssigned());
			var exporter = new ProjectExporter(project);
			exporter.SelectedFileType = ExportFileType.Excel;
			exporter.IncludeActorBreakdown = true;
			using (var tempDir = new SIL.TestUtilities.TemporaryFolder("PG855ExportActorExcelScripts"))
			{
				exporter.FullFileName = Path.Combine(tempDir.Path, Path.ChangeExtension("base", ProjectExporter.kExcelFileExtension));
				Assert.IsFalse(exporter.ExportNow(false).Any());
				Assert.IsTrue(Directory.Exists(exporter.ActorDirectory));
				foreach (var actor in project.CharacterGroupList.AssignedGroups.Select(g => g.VoiceActor.Name))
				{
					Assert.IsTrue(File.Exists(Path.Combine(exporter.ActorDirectory,
						Path.ChangeExtension(actor, ProjectExporter.kExcelFileExtension))));
				}
			}
		}

		/// <summary>
		/// PG-855: Generating book-by-book script files shouldn't fail
		/// </summary>
		[Test]
		public void ExportNow_ExportBookBreakdownExcelScripts_ScriptsCreated()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.IIIJN, TestProject.TestBook.JUD);
			var exporter = new ProjectExporter(project);
			exporter.SelectedFileType = ExportFileType.Excel;
			exporter.IncludeBookBreakdown = true;
			using (var tempDir = new SIL.TestUtilities.TemporaryFolder("PG855ExportBookBreakdownExcelScripts"))
			{
				exporter.FullFileName = Path.Combine(tempDir.Path, Path.ChangeExtension("base", ProjectExporter.kExcelFileExtension));
				Assert.IsFalse(exporter.ExportNow(false).Any());
				Assert.IsTrue(Directory.Exists(exporter.BookDirectory));
				Assert.IsTrue(File.Exists(Path.Combine(exporter.BookDirectory, Path.ChangeExtension("3JN", ProjectExporter.kExcelFileExtension))));
				Assert.IsTrue(File.Exists(Path.Combine(exporter.BookDirectory, Path.ChangeExtension("JUD", ProjectExporter.kExcelFileExtension))));
			}
		}
	}
}
