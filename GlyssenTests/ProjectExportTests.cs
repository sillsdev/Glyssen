using System;
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
using Glyssen.Shared;
using Glyssen.VoiceActor;
using NUnit.Framework;
using SIL.Extensions;
using SIL.Reflection;

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
		public void GetExportData_NoActorsAssigned_VoiceActorNotSet()
		{
			var project = TestProject.CreateBasicTestProject();
			var metadata = (GlyssenDblTextMetadata)ReflectionHelper.GetField(project, "m_metadata");
			metadata.IncludeChapterAnnouncementForFirstChapter = true;
			metadata.IncludeChapterAnnouncementForSingleChapterBooks = true;
			var exporter = new ProjectExporter(project);
			var data = exporter.GetExportData();
			Assert.True(data.All(t => t.VoiceActor == null));
		}

		[Test]
		public void GetExportData_SomeButNotAllActorsAssigned_VoiceActorsAssignedOnlyForBlocks()
		{
			var project = TestProject.CreateBasicTestProject();
			var metadata = (GlyssenDblTextMetadata)ReflectionHelper.GetField(project, "m_metadata");
			metadata.IncludeChapterAnnouncementForFirstChapter = true;
			metadata.IncludeChapterAnnouncementForSingleChapterBooks = true;
			project.VoiceActorList.AllActors = new List<Glyssen.VoiceActor.VoiceActor>
			{
				new Glyssen.VoiceActor.VoiceActor {Id = 1, Name = "Ralphy"}
			};
			project.CharacterGroupList.CharacterGroups.AddRange(new[]
			{
				new CharacterGroup(project),
				new CharacterGroup(project)
			});
			var characterIdAssignedToGroup1 = project.IncludedBooks.First().GetScriptBlocks().First(b => !b.CharacterIsStandard).CharacterId;
			project.CharacterGroupList.CharacterGroups[0].CharacterIds.Add(characterIdAssignedToGroup1);
			project.CharacterGroupList.CharacterGroups[0].AssignVoiceActor(1);

			var exporter = new ProjectExporter(project);
			var data = exporter.GetExportData();
			var rowsForCharacterAssignedToActor = data.Where(d => d.CharacterId == characterIdAssignedToGroup1).ToList();
			var rowsForCharacterNotAssignedToActor = data.Where(d => d.CharacterId != characterIdAssignedToGroup1).ToList();
			Assert.True(rowsForCharacterAssignedToActor.Any());
			Assert.True(rowsForCharacterNotAssignedToActor.Any());
			Assert.True(rowsForCharacterAssignedToActor.All(t => t.VoiceActor == "Ralphy"));
			Assert.True(rowsForCharacterNotAssignedToActor.All(t => String.IsNullOrEmpty(t.VoiceActor)));
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
			var chapterBlockForEphesians = data.Single(t => t.StyleTag == "cl" && t.ChapterNumber == 1);
			Assert.AreEqual("EPH", chapterBlockForEphesians.BookId);
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
			Assert.AreEqual(2, data.Count(t => t.StyleTag == "c" && t.ChapterNumber == 1));
		}

		[Test]
		public void GetExportData_SkipChapterOne_OutputDoesNotIncludeChapterAnnouncementForFirstChapter()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.EPH, TestProject.TestBook.JUD);
			var metadata = (GlyssenDblTextMetadata)ReflectionHelper.GetField(project, "m_metadata");
			metadata.IncludeChapterAnnouncementForFirstChapter = false;
			var exporter = new ProjectExporter(project);
			var data = exporter.GetExportData();
			Assert.False(data.Any(t => t.StyleTag == "c" && t.ChapterNumber == 1));
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
			var chapterBlockForEphesians = data.Single(t => t.StyleTag == "c" && t.ChapterNumber == 1);
			Assert.AreEqual("EPH", chapterBlockForEphesians.BookId);
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
			Assert.AreEqual(expectedIntroParagraphs, data.Count(t => t.StyleTag.StartsWith("i", StringComparison.Ordinal)));
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
			Assert.IsFalse(data.Any(t => t.StyleTag.StartsWith("i", StringComparison.Ordinal)));
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
			Assert.AreEqual(expectedSectionHeadParagraphs, data.Count(t => t.StyleTag.StartsWith("s", StringComparison.Ordinal)));
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
			Assert.IsFalse(data.Any(t => t.StyleTag.StartsWith("s", StringComparison.Ordinal)));
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
			Assert.AreEqual(expected, data.Count(t => t.CharacterId == "book title or chapter (EPH)"));
		}

		[Test]
		public void GetExportData_TitlesAndChaptersOmitted_NoTitlesOrChaptersInExportData()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.EPH);
			project.DramatizationPreferences.BookTitleAndChapterDramatization = ExtraBiblicalMaterialSpeakerOption.Omitted;
			var exporter = new ProjectExporter(project);
			var data = exporter.GetExportData();
			Assert.IsTrue(data.Any());
			Assert.IsFalse(data.Any(t => t.CharacterId == "book title or chapter (EPH)"));
		}

		[Test]
		public void GetExportData_SpecifiedBook_OutputOnlyIncludesBlocksForThatBook()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.GAL, TestProject.TestBook.IIJN);
			var metadata = (GlyssenDblTextMetadata)ReflectionHelper.GetField(project, "m_metadata");
			metadata.IncludeChapterAnnouncementForFirstChapter = true;
			metadata.IncludeChapterAnnouncementForSingleChapterBooks = true;
			var exporter = new ProjectExporter(project);
			var data = exporter.GetExportData("2JN");
			Assert.True(data.All(t => t.BookId == "2JN"));
		}

		[Test]
		public void GetExportData_SpecifiedActor_OutputOnlyIncludesBlocksForThatActor()
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
			Assert.AreEqual("Marlon", data.VoiceActor);
			Assert.AreEqual(1, data.ChapterNumber);
			Assert.AreEqual(9, data.VerseNumber);
			Assert.AreEqual("Michael, archangel", data.CharacterId);
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
			var iStartOfJude = data.IndexOf(d => d.BookId == "JUD");
			Assert.IsTrue(iStartOfJude > 0);
			Assert.IsTrue(data.Take(iStartOfJude).All(d => d.BookId == "GAL"));
			Assert.IsTrue(data.Skip(iStartOfJude).All(d => d.BookId == "JUD"));
			Assert.IsTrue(data.Take(iStartOfJude).All(d => d.CharacterId == "narrator (GAL)"));
			Assert.IsTrue(data.Skip(iStartOfJude).All(d => d.CharacterId == "narrator (JUD)"));
			Assert.IsTrue(data.Take(iStartOfJude).All(d => d.VoiceActor == "Marlon"));
			Assert.IsTrue(data.Skip(iStartOfJude).All(d => d.VoiceActor == "Aiden"));
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

			Assert.IsTrue(data.All(d => d.BookId == "JUD" && d.ChapterNumber == 1));
			var i = 0;
			var row = data[i++];
			Assert.AreEqual(i, row.Number); // Row 1
			Assert.AreEqual("p", row.StyleTag);
			Assert.AreEqual(1, row.VerseNumber);
			Assert.AreEqual("narrator (JUD)", row.CharacterId);
			Assert.AreEqual("{1}\u00A0A", row.VernacularText);
			Assert.AreEqual("{1}\u00A0Ayy", row.AdditionalReferenceText);
			Assert.AreEqual("{1}\u00A0Secondary", row.EnglishReferenceText);

			row = data[i++];
			Assert.AreEqual(i, row.Number); // Row 2
			Assert.AreEqual("s", row.StyleTag);
			Assert.AreEqual(1, row.VerseNumber);
			Assert.AreEqual("section head (JUD)", row.CharacterId);
			Assert.AreEqual("Jude complains", row.VernacularText);
			Assert.IsTrue(string.IsNullOrEmpty(row.AdditionalReferenceText));
			Assert.IsTrue(string.IsNullOrEmpty(row.EnglishReferenceText));

			row = data[i++];
			Assert.AreEqual(i, row.Number); // Row 3
			Assert.AreEqual("p", row.StyleTag);
			Assert.AreEqual(2, row.VerseNumber);
			Assert.AreEqual("Enoch", row.CharacterId);
			Assert.AreEqual("{2}\u00A0B", row.VernacularText);
			Assert.IsTrue(string.IsNullOrEmpty(row.AdditionalReferenceText));
			Assert.IsTrue(string.IsNullOrEmpty(row.EnglishReferenceText));

			row = data[i++];
			Assert.AreEqual(i, row.Number); // Row 4
			Assert.AreEqual("p", row.StyleTag);
			Assert.AreEqual(3, row.VerseNumber);
			Assert.AreEqual("narrator (JUD)", row.CharacterId);
			Assert.AreEqual("{3}\u00A0C", row.VernacularText);
			Assert.IsTrue(string.IsNullOrEmpty(row.AdditionalReferenceText));
			Assert.IsTrue(string.IsNullOrEmpty(row.EnglishReferenceText));

			row = data[i++];
			Assert.AreEqual(0, row.Number);
			Assert.AreEqual("p", row.StyleTag);
			Assert.AreEqual(2, row.VerseNumber);
			Assert.AreEqual("narrator (JUD)", row.CharacterId);
			Assert.IsTrue(string.IsNullOrEmpty(row.VernacularText));
			Assert.AreEqual("{2-3}\u00A0Bee Cee", row.AdditionalReferenceText);
			Assert.AreEqual("{2-3}\u00A0Secondary", row.EnglishReferenceText);
			Assert.AreEqual(0, row.Length);

			row = data[i++];
			Assert.AreEqual(5, row.Number); // Row 5
			Assert.AreEqual("p", row.StyleTag);
			Assert.AreEqual(4, row.VerseNumber);
			Assert.AreEqual("Michael", row.CharacterId);
			Assert.AreEqual("{4}\u00A0D", row.VernacularText);
			Assert.AreEqual("{4}\u00A0Dee, Michael said.", row.AdditionalReferenceText);
			Assert.AreEqual("{4}\u00A0Secondary the angel named Mike verbalized.", row.EnglishReferenceText);

			row = data[i++];
			Assert.AreEqual(6, row.Number);
			Assert.AreEqual("p", row.StyleTag);
			Assert.AreEqual(5, row.VerseNumber);
			Assert.AreEqual("narrator (JUD)", row.CharacterId);
			Assert.AreEqual("{5}\u00A0E ", row.VernacularText);
			Assert.AreEqual("{5}\u00A0Ey", row.AdditionalReferenceText);
			Assert.AreEqual("{5}\u00A0Secondary", row.EnglishReferenceText);

			row = data[i++];
			Assert.AreEqual(7, row.Number);
			Assert.AreEqual("p", row.StyleTag);
			Assert.AreEqual(6, row.VerseNumber);
			Assert.AreEqual("narrator (JUD)", row.CharacterId);
			Assert.AreEqual("{6}\u00A0F", row.VernacularText);
			Assert.AreEqual("{6}\u00A0Ef", row.AdditionalReferenceText);
			Assert.AreEqual("{6}\u00A0Secondary", row.EnglishReferenceText);

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

			Assert.IsTrue(data.All(d => d.BookId == "MRK" && d.ChapterNumber == 4));
			var i = 0;
			var row = data[i];
			Assert.AreEqual(39, row.VerseNumber);
			Assert.AreEqual(narratorInOutput, row.CharacterId);
			Assert.AreEqual(mark.GetScriptBlocks()[i].GetText(true), row.VernacularText);
			Assert.AreEqual(refBlocks[i].GetText(true), row.AdditionalReferenceText);

			row = data[++i];
			Assert.AreEqual(39, row.VerseNumber);
			Assert.AreEqual("Jesus", row.CharacterId);
			Assert.AreEqual(mark.GetScriptBlocks()[i].GetText(true), row.VernacularText);
			Assert.AreEqual(refBlocks[i].GetText(true) + "||| + 1.5 SECs |||", row.AdditionalReferenceText);
			Assert.AreEqual("Some secondary reference text ||| + 1.5 SECs |||", row.EnglishReferenceText);

			row = data[++i];
			Assert.AreEqual(39, row.VerseNumber);
			Assert.AreEqual(narratorInOutput, row.CharacterId);
			Assert.AreEqual(mark.GetScriptBlocks()[i].GetText(true), row.VernacularText);
			Assert.IsTrue(string.IsNullOrEmpty(row.AdditionalReferenceText as string));

			row = data[++i];
			Assert.AreEqual(39, row.VerseNumber);
			Assert.AreEqual("Jesus", row.CharacterId);
			Assert.AreEqual(mark.GetScriptBlocks()[i].GetText(true), row.VernacularText);
			Assert.IsTrue(string.IsNullOrEmpty(row.AdditionalReferenceText as string));

			row = data[++i];
			Assert.IsTrue(string.IsNullOrEmpty(row.AsObjectArray()[0] as string));
			Assert.AreEqual(39, row.VerseNumber);
			Assert.AreEqual(narratorInOutput, row.CharacterId);
			Assert.AreEqual(mark.GetScriptBlocks()[i].GetText(true), row.VernacularText);
			Assert.AreEqual(refBlocks[i - 2].GetText(true), row.AdditionalReferenceText);

			row = data[++i];
			Assert.IsTrue(string.IsNullOrEmpty(row.AsObjectArray()[0] as string));
			Assert.AreEqual(40, row.VerseNumber);
			Assert.AreEqual("Jesus", row.CharacterId);
			Assert.AreEqual(mark.GetScriptBlocks()[i].GetText(true), row.VernacularText);
			Assert.AreEqual(refBlocks[i - 2].GetText(true), row.AdditionalReferenceText);
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

			Assert.IsTrue(data.All(d => d.BookId == "JUD"));
			Assert.AreEqual("YӘHUDANIN MӘKTUBU", data[0].AdditionalReferenceText);
			Assert.AreEqual("JUDE", data[0].EnglishReferenceText);
			Assert.IsTrue(data.Skip(1).All(d => d.ChapterNumber == 1));
			Assert.AreEqual("YӘHUDA 1", data[1].AdditionalReferenceText);
			Assert.AreEqual("JUDE CHP 1", data[1].EnglishReferenceText);
			var matchedRows = data.Where(d => d.VernacularText != null && d.AdditionalReferenceText != null).ToList();
			Assert.IsTrue(matchedRows.Count > data.Count / 2); // This is kind of arbitrary, but I just want to say we got a reasonable number of matches
			Assert.IsTrue(matchedRows.Any(d => d.AdditionalReferenceText.Contains("Ә"))); // A letter that should be in Azeri, but not English
			Assert.IsTrue(matchedRows.All(d => d.EnglishReferenceText != null));
			Assert.IsTrue(matchedRows.Any(d => d.EnglishReferenceText.Contains(" the "))); // A word that should be in English, but not Azeri
			// Since the test version of Jude does not match perfectly with this reference text, we expect two rows
			// where the vernacular has no corresponding reference text.
			var extra = CharacterVerseData.GetCharacterNameForUi(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.ExtraBiblical));
			var narrator = CharacterVerseData.GetCharacterNameForUi(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.Narrator));
			Assert.IsTrue(data.Where(d => d.StyleTag == "s1").All(d => d.CharacterId == extra));
			var scriptureRowsWithNoReferenceText = data.Where(d => d.AdditionalReferenceText == null && d.StyleTag != "s1").ToList();
			Assert.AreEqual(2, scriptureRowsWithNoReferenceText.Count);
			Assert.AreEqual(1, scriptureRowsWithNoReferenceText.Count(d => d.CharacterId == narrator));
			Assert.IsTrue(scriptureRowsWithNoReferenceText.All(d => d.EnglishReferenceText == null));
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

			Assert.IsTrue(data.All(d => d.BookId == "JUD"));
			Assert.AreEqual("Иуда", data[0].AdditionalReferenceText);
			Assert.AreEqual("JUDE", data[0].EnglishReferenceText);
			Assert.IsTrue(data.Skip(1).All(d => d.ChapterNumber == 1));
			Assert.AreEqual("Иуда 1", data[1].AdditionalReferenceText);
			Assert.AreEqual("JUDE CHP 1", data[1].EnglishReferenceText);
			var matchedRows = data.Where(d => d.VernacularText != null && d.AdditionalReferenceText != null).ToList();
			Assert.IsTrue(matchedRows.Count > data.Count / 2); // This is kind of arbitrary, but I just want to say we got a reasonable number of matches
			Assert.IsTrue(matchedRows.Any(d => d.AdditionalReferenceText.Contains("п"))); // A letter that should be in Russian, but not English
			Assert.IsTrue(matchedRows.All(d => d.EnglishReferenceText != null));
			Assert.IsTrue(matchedRows.Any(d => d.EnglishReferenceText.Contains(" the "))); // A word that should be in English, but not Russian
			// Since the test version of Jude does not match perfectly with the standard reference texts, we expect two Scripture rows
			// where the vernacular has no corresponding reference text.
			var extra = CharacterVerseData.GetCharacterNameForUi(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.ExtraBiblical));
			var narrator = CharacterVerseData.GetCharacterNameForUi(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.Narrator));
			Assert.IsTrue(data.Where(d => d.StyleTag == "s1")
				.All(d => d.CharacterId == extra));
			var scriptureRowsWithNoReferenceText = data.Where(d => d.AdditionalReferenceText == null &&
				d.StyleTag != "s1").ToList();
			Assert.AreEqual(2, scriptureRowsWithNoReferenceText.Count);
			Assert.AreEqual(1, scriptureRowsWithNoReferenceText.Count(d => d.CharacterId == narrator));
			Assert.IsTrue(scriptureRowsWithNoReferenceText.All(d => d.EnglishReferenceText == null));
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
			var rowsForVerse12 = data.Where(d => d.BookId == "JUD" && d.ChapterNumber == 1 && d.VerseNumber == 12).ToList();
			Assert.AreEqual(2, rowsForVerse12.Count);
			var annotationRowForVerse12 = rowsForVerse12[0];
			var rowForVerse12 = rowsForVerse12[1];
			Assert.IsTrue(annotationRowForVerse12.AdditionalReferenceText.Equals(annotationRowForVerse12.EnglishReferenceText) &&
				annotationRowForVerse12.AdditionalReferenceText.StartsWith(Sound.kDoNotCombine + exporter.AnnotationElementSeparator + "{SFX"));
			Assert.IsFalse(rowForVerse12.AdditionalReferenceText.Contains("|||"));

			//Pause for final verse in book (pauses come after verse text)
			var rowsForJude25 = data.Where(d => d.BookId == "JUD" && d.ChapterNumber == 1 && d.VerseNumber == 25).ToList();
			Assert.AreEqual(2, rowsForJude25.Count);
			var rowForJude25 = rowsForJude25[0];
			var annotationRowForJude25 = rowsForJude25[1];
			Assert.IsFalse(rowForJude25.AdditionalReferenceText.Contains("|||"));
			Assert.IsTrue(annotationRowForJude25.AdditionalReferenceText.Equals(annotationRowForJude25.EnglishReferenceText) &&
				annotationRowForJude25.AdditionalReferenceText.Equals(string.Format(Pause.kPauseSecondsFormat, "5")));

			//Pause for non-final verse in book (pauses come after verse text)
			var rowsForRev1V3 = data.Where(d => d.BookId == "REV" && d.ChapterNumber == 1 && d.VerseNumber == 3).ToList();
			Assert.AreEqual(3, rowsForRev1V3.Count);
			var rowForRev1V3 = rowsForRev1V3[0];
			var annotationRowForRev1V3 = rowsForRev1V3[1];
			var sectionHeadRowForRev1V3 = rowsForRev1V3[2];
			Assert.IsFalse(rowForRev1V3.AdditionalReferenceText.Contains("|||"));
			Assert.IsTrue(annotationRowForRev1V3.AdditionalReferenceText.Equals(annotationRowForRev1V3.EnglishReferenceText) &&
				annotationRowForRev1V3.AdditionalReferenceText.Equals(string.Format(Pause.kPauseSecondsFormat, "2")));
			Assert.AreEqual(CharacterVerseData.GetStandardCharacterNameForUi(CharacterVerseData.StandardCharacter.ExtraBiblical, "REV"),
				sectionHeadRowForRev1V3.CharacterId);

			//Pause for final verse in chapter (pauses come after verse text)
			var rowsForRev1V20 = data.Where(d => d.BookId == "REV" && d.ChapterNumber == 1 && d.VerseNumber == 20).ToList();
			Assert.AreEqual(2, rowsForRev1V20.Count);
			var rowForRev1V20 = rowsForRev1V20[0];
			var annotationRowForRev1V20 = rowsForRev1V20[1];
			Assert.IsFalse(rowForRev1V20.AdditionalReferenceText.Contains("|||"));
			Assert.IsTrue(annotationRowForRev1V20.AdditionalReferenceText.Equals(annotationRowForRev1V20.EnglishReferenceText) &&
				annotationRowForRev1V20.AdditionalReferenceText.Equals(string.Format(Pause.kPauseSecondsFormat, "2")));
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
			var rowsForVerse12 = data.Where(d => d.BookId == "JUD" && d.ChapterNumber == 1 && d.VerseNumber == 12).ToList();
			var rowForVerse12 = rowsForVerse12.Single();
			var annotationInfoPlusVerseNum = Sound.kDoNotCombine + exporter.AnnotationElementSeparator + "{SFX--Eerie--Starts @ v12} {12}\u00A0";
			Assert.IsTrue(rowForVerse12.AdditionalReferenceText.StartsWith(annotationInfoPlusVerseNum));
			Assert.IsTrue(rowForVerse12.EnglishReferenceText.StartsWith(annotationInfoPlusVerseNum));
			Assert.AreEqual("{12}\u00A0Gikelo lewic i karamawu me mar ka gicamo matek mukato kare laboŋo lworo, kun giparo pi komgi keken. " +
				"Gubedo calo pol ma pii pe iye ma yamo kolo; girom ki yadi ma nyiggi pe nen i kare me cekgi, ma giputo lwitgi woko, " +
				"yam guto kiryo. ",
				rowForVerse12.VernacularText);

			//Pause for final verse in book (pauses come after verse text)
			var rowsForJude25 = data.Where(d => d.BookId == "JUD" && d.ChapterNumber == 1 && d.VerseNumber == 25).ToList();
			var rowForJude25 = rowsForJude25.Single();
			var annotationInfo = " " + string.Format(Pause.kPauseSecondsFormat, "5");
			Assert.IsTrue(rowForJude25.AdditionalReferenceText.EndsWith(annotationInfo));
			Assert.IsTrue(rowForJude25.EnglishReferenceText.EndsWith(annotationInfo));
			Assert.AreEqual("{25}\u00A0Deyo, dit, loc ki twer ducu obed bot Lubaŋa acel keken, ma Lalarwa, pi Yecu Kricito Rwotwa, " +
				"cakke ma peya giketo lobo, nio koni, ki kare ma pe gik. Amen.",
				rowForJude25.VernacularText);

			//Pause for non-final verse in book (pauses come after verse text)
			var rowsForRev1V3 = data.Where(d => d.BookId == "REV" && d.ChapterNumber == 1 && d.VerseNumber == 3).ToList();
			Assert.AreEqual(2, rowsForRev1V3.Count);
			var rowForRev1V3 = rowsForRev1V3[0];
			var sectionHeadRowForRev1V3 = rowsForRev1V3[1];
			annotationInfo = " " + string.Format(Pause.kPauseSecondsFormat, "2");
			Assert.IsTrue(rowForRev1V3.AdditionalReferenceText.EndsWith(annotationInfo));
			Assert.IsTrue(rowForRev1V3.EnglishReferenceText.EndsWith(annotationInfo));
			Assert.AreEqual("{3}\u00A0Ŋat ma kwano lok ma gitito i buk man i nyim lwak tye ki gum, jo ma winyo bene tye ki gum, ki jo ma lubo " +
				"gin ma gicoyo iye bene tye ki gum, pien kare doŋ cok.",
				rowForRev1V3.VernacularText);
			Assert.AreEqual(CharacterVerseData.GetStandardCharacterNameForUi(CharacterVerseData.StandardCharacter.ExtraBiblical, "REV"),
				sectionHeadRowForRev1V3.CharacterId);

			//Pause for final verse in chapter (pauses come after verse text)
			var rowsForRev1V20 = data.Where(d => d.BookId == "REV" && d.ChapterNumber == 1 && d.VerseNumber == 20).ToList();
			var rowForRev1V20 = rowsForRev1V20.Single();
			annotationInfo = " " + string.Format(Pause.kPauseSecondsFormat, "2");
			Assert.IsTrue(rowForRev1V20.AdditionalReferenceText.EndsWith(annotationInfo));
			Assert.IsTrue(rowForRev1V20.EnglishReferenceText.EndsWith(annotationInfo));
			Assert.AreEqual("{20}\u00A0Koŋ agonnyi tyen lok me muŋ me lakalatwe abiro ma ineno i ciŋa tuŋ lacuc, ki okar-mac abiro me jabu. " +
				"Lakalatwe abiro gin aye lumalaika pa lwak muye Kricito ma gitye i kabedo abiro mapatpat, doŋ okar-mac abiro-ni gin " +
				"aye lwak muye Kricito ma gitye i kabedo abiro mapatpat.”",
				rowForRev1V20.VernacularText);
		}

		[Test]
		public void GetExportData_EmptyReferenceTextForVersesWithAnnotations_AnnotationsInsertedIntoEmptyReferenceTextsWithoutCrashing()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.JUD, TestProject.TestBook.REV);
			project.DramatizationPreferences.SectionHeadDramatization = ExtraBiblicalMaterialSpeakerOption.ActorOfEitherGender;
			project.ReferenceText = ReferenceText.GetStandardReferenceText(ReferenceTextType.Russian);
			var jud = project.IncludedBooks[0];
			var blockJud12 = jud.GetScriptBlocks().Single(b => b.BlockElements.OfType<Verse>().Any(v => v.Number == "12"));
			if (blockJud12.InitialStartVerseNumber != 12)
				blockJud12 = jud.SplitBlock(blockJud12, "11", PortionScript.kSplitAtEndOfVerse);
			if (blockJud12.LastVerseNum != 12)
				jud.SplitBlock(blockJud12, "12", PortionScript.kSplitAtEndOfVerse);
			blockJud12.SetMatchedReferenceBlock("{12}").SetMatchedReferenceBlock("");
			var blockJud25 = jud.GetScriptBlocks().Single(b => b.BlockElements.OfType<Verse>().Any(v => v.Number == "25"));
			if (blockJud25.InitialStartVerseNumber != 25)
				blockJud25 = jud.SplitBlock(blockJud25, "24", PortionScript.kSplitAtEndOfVerse);
			Assert.AreEqual(25, blockJud25.LastVerseNum);
			blockJud25.SetMatchedReferenceBlock("{25}").SetMatchedReferenceBlock("");
			var rev = project.IncludedBooks[1];
			var blockRev1V3 = rev.GetScriptBlocks().Single(b => b.ChapterNumber == 1 && b.BlockElements.OfType<Verse>().Any(v => v.Number == "3"));
			if (blockRev1V3.InitialStartVerseNumber != 3)
				blockRev1V3 = rev.SplitBlock(blockRev1V3, "2", PortionScript.kSplitAtEndOfVerse);
			if (blockRev1V3.LastVerseNum != 3)
				rev.SplitBlock(blockRev1V3, "3", PortionScript.kSplitAtEndOfVerse);
			blockRev1V3.SetMatchedReferenceBlock("{3}").SetMatchedReferenceBlock("");
			var blockRev1V20 = rev.GetScriptBlocks().Single(b => b.ChapterNumber == 1 && b.BlockElements.OfType<Verse>().Any(v => v.Number == "20"));
			if (blockRev1V20.InitialStartVerseNumber != 20)
				blockRev1V20 = rev.SplitBlock(blockRev1V20, "19", PortionScript.kSplitAtEndOfVerse);
			if (blockRev1V20.LastVerseNum != 20)
				rev.SplitBlock(blockRev1V20, "20", PortionScript.kSplitAtEndOfVerse);
			blockRev1V20.SetMatchedReferenceBlock("{20}").SetMatchedReferenceBlock("");

			var exporter = new ProjectExporter(project) { SelectedFileType = ExportFileType.Excel };

			var data = exporter.GetExportData().ToList();

			//SFX (music/sfx come before verse text)
			var rowForVerse12 = data.Single(d => d.BookId == "JUD" && d.ChapterNumber == 1 && d.VerseNumber == 12);
			var annotationInfo = Sound.kDoNotCombine + exporter.AnnotationElementSeparator + "{SFX--Eerie--Starts @ v12}";
			Assert.AreEqual(annotationInfo + " {12}\u00A0", rowForVerse12.AdditionalReferenceText);
			Assert.AreEqual(annotationInfo, rowForVerse12.EnglishReferenceText.TrimEnd());
			Assert.IsTrue(rowForVerse12.AdditionalReferenceText.StartsWith(annotationInfo + " {12}\u00A0"));
			Assert.AreEqual("{12}\u00A0Gikelo lewic i karamawu me mar ka gicamo matek mukato kare laboŋo lworo, kun giparo pi komgi keken. " +
				"Gubedo calo pol ma pii pe iye ma yamo kolo; girom ki yadi ma nyiggi pe nen i kare me cekgi, ma giputo lwitgi woko, " +
				"yam guto kiryo. ",
				rowForVerse12.VernacularText);

			//Pause for final verse in book (pauses come after verse text)
			var rowsForJude25 = data.Where(d => d.BookId == "JUD" && d.ChapterNumber == 1 && d.VerseNumber == 25).ToList();
			var rowForJude25 = rowsForJude25.Single();
			annotationInfo = string.Format(Pause.kPauseSecondsFormat, "5");
			Assert.AreEqual("{25}\u00A0" + annotationInfo, rowForJude25.AdditionalReferenceText);
			Assert.AreEqual(annotationInfo, rowForJude25.EnglishReferenceText.TrimStart());
			Assert.AreEqual("{25}\u00A0Deyo, dit, loc ki twer ducu obed bot Lubaŋa acel keken, ma Lalarwa, pi Yecu Kricito Rwotwa, " +
				"cakke ma peya giketo lobo, nio koni, ki kare ma pe gik. Amen.",
				rowForJude25.VernacularText);

			//Pause for non-final verse in book (pauses come after verse text)
			var rowForRev1V3 = data.First(d => d.BookId == "REV" && d.ChapterNumber == 1 && d.VerseNumber == 3);
			annotationInfo = string.Format(Pause.kPauseSecondsFormat, "2");
			Assert.AreEqual("{3}\u00A0" + annotationInfo, rowForRev1V3.AdditionalReferenceText);
			Assert.AreEqual(annotationInfo, rowForRev1V3.EnglishReferenceText.TrimStart());
			Assert.AreEqual("{3}\u00A0Ŋat ma kwano lok ma gitito i buk man i nyim lwak tye ki gum, jo ma winyo bene tye ki gum, ki jo ma lubo " +
				"gin ma gicoyo iye bene tye ki gum, pien kare doŋ cok.",
				rowForRev1V3.VernacularText);

			//Pause for final verse in chapter (pauses come after verse text)
			var rowForRev1V20 = data.Single(d => d.BookId == "REV" && d.ChapterNumber == 1 && d.VerseNumber == 20);
			Assert.AreEqual("{20}\u00A0" + annotationInfo, rowForRev1V20.AdditionalReferenceText);
			Assert.AreEqual(annotationInfo, rowForRev1V20.EnglishReferenceText.TrimStart());
			Assert.AreEqual("{20}\u00A0Koŋ agonnyi tyen lok me muŋ me lakalatwe abiro ma ineno i ciŋa tuŋ lacuc, ki okar-mac abiro me jabu. " +
				"Lakalatwe abiro gin aye lumalaika pa lwak muye Kricito ma gitye i kabedo abiro mapatpat, doŋ okar-mac abiro-ni gin " +
				"aye lwak muye Kricito ma gitye i kabedo abiro mapatpat.”",
				rowForRev1V20.VernacularText);
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

			// Force a block that has an appended annotation to have a null secondary reference.
			foreach (var block in rev.GetScriptBlocks().Where(b => b.ChapterNumber == 22 && b.InitialStartVerseNumber == 17))
				block.SetMatchedReferenceBlock("Get, sәn dә elә et");

			var exporter = new ProjectExporter(project) { SelectedFileType = ExportFileType.Excel };

			var data = exporter.GetExportData().ToList();

			//Pause for final verse in chapter (pauses come after verse text)
			var rowsForRev22V17 = data.Where(d => d.BookId == "REV" && d.ChapterNumber == 22 && d.VerseNumber == 17).ToList();
			var rowForRev22V17 = rowsForRev22V17.Last();
			var annotationInfo = " " + string.Format(Pause.kPauseSecondsFormat, "2");
			Assert.IsTrue(rowForRev22V17.AdditionalReferenceText.EndsWith(annotationInfo));
			Assert.AreEqual(annotationInfo, rowForRev22V17.EnglishReferenceText);
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

			// Force a block that has a prepended annotation to have a null secondary reference.
			var vernBlock = project.IncludedBooks.First().GetScriptBlocks().Single(b => b.ChapterNumber == 1 && b.InitialStartVerseNumber == 7);
			vernBlock.SetMatchedReferenceBlock("{7}Verse Seven in Azeri.");

			var exporter = new ProjectExporter(project) { SelectedFileType = ExportFileType.Excel };

			var data = exporter.GetExportData().ToList();

			//SFX (music/sfx come before verse text)
			var rowsForRev1V7 = data.Where(d => d.BookId == "REV" && d.ChapterNumber == 1 && d.VerseNumber == 7).ToList();
			var rowForRev1V7 = rowsForRev1V7.Single();
			var annotationInfo = Sound.kDoNotCombine + exporter.AnnotationElementSeparator + "{Music--Starts @ v7} ";
			Assert.IsTrue(rowForRev1V7.AdditionalReferenceText.StartsWith(annotationInfo));
			Assert.AreEqual(annotationInfo, rowForRev1V7.EnglishReferenceText);
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
			var rowsForMark4V39 = data.Where(d => d.BookId == "MRK" && d.ChapterNumber == 4 && d.VerseNumber == 39).ToList();
			Assert.AreEqual(4, rowsForMark4V39.Count);
			var narratorTextRow1ForMark4V39 = rowsForMark4V39[0];
			var jesusTextRowForMark4V39 = rowsForMark4V39[0];
			var annotationRowForMark4V39 = rowsForMark4V39[2];
			var narratorTextRow2ForMark4V39 = rowsForMark4V39[3];
			Assert.IsFalse(narratorTextRow1ForMark4V39.AdditionalReferenceText.Contains("|||"));
			Assert.IsFalse(jesusTextRowForMark4V39.AdditionalReferenceText.Contains("|||"));
			Assert.IsTrue(annotationRowForMark4V39.AdditionalReferenceText.Equals(annotationRowForMark4V39.EnglishReferenceText) &&
				annotationRowForMark4V39.AdditionalReferenceText.Equals(string.Format(Pause.kPauseSecondsFormat, "1.5")));
			Assert.IsFalse(narratorTextRow2ForMark4V39.AdditionalReferenceText.Contains("|||"));
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
			var rowsForMark4V39 = data.Where(d => d.BookId == "MRK" && d.ChapterNumber == 4 && d.VerseNumber == 39).ToList();
			Assert.AreEqual(3, rowsForMark4V39.Count);
			var narratorTextRow1ForMark4V39 = rowsForMark4V39[0];
			var jesusTextRowForMark4V39 = rowsForMark4V39[1];
			var narratorTextRow2ForMark4V39 = rowsForMark4V39[2];
			Assert.IsFalse(narratorTextRow1ForMark4V39.AdditionalReferenceText.Contains("|||"));
			var annotationInfo = " " + string.Format(Pause.kPauseSecondsFormat, "1.5");
			Assert.IsTrue(jesusTextRowForMark4V39.AdditionalReferenceText.EndsWith(annotationInfo));
			Assert.IsTrue(jesusTextRowForMark4V39.EnglishReferenceText.EndsWith(annotationInfo));
			Assert.IsFalse(narratorTextRow2ForMark4V39.AdditionalReferenceText.Contains("|||"));
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

			Assert.AreEqual("#", data.Columns[(int)ExportColumn.BlockId].ColumnName);
			Assert.AreEqual("Actor", data.Columns[(int)ExportColumn.Actor].ColumnName);
			Assert.AreEqual("Tag", data.Columns[(int)ExportColumn.ParaTag].ColumnName);
			Assert.AreEqual("Book", data.Columns[(int)ExportColumn.BookId].ColumnName);
			Assert.AreEqual("Chapter", data.Columns[(int)ExportColumn.Chapter].ColumnName);
			Assert.AreEqual("Verse", data.Columns[(int)ExportColumn.Verse].ColumnName);
			Assert.AreEqual("Character", data.Columns[(int)ExportColumn.CharacterId].ColumnName);
			Assert.AreEqual("Character (localized)", data.Columns[(int)ExportColumn.CharacterIdLocalized].ColumnName);
			Assert.AreEqual("Delivery", data.Columns[(int)ExportColumn.Delivery].ColumnName);
			Assert.AreEqual("Text", data.Columns[(int)ExportColumn.VernacularText].ColumnName);
			Assert.AreEqual("English Director's Guide", data.Columns[(int)ExportColumn.EnglishReferenceText].ColumnName);
			Assert.AreEqual("Russian Director's Guide", data.Columns[(int)ExportColumn.AdditionalReferenceText].ColumnName);
			Assert.AreEqual("Size", data.Columns[(int)ExportColumn.VernacularTextLength].ColumnName);
			if (includeClipColumn)
			{
				Assert.AreEqual("Clip File", data.Columns[(int)ExportColumn.ClipFileLink].ColumnName);
				Assert.AreEqual(14, data.Columns.Count);
			}
			else
				Assert.AreEqual(13, data.Columns.Count);
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


			int textLength = "Text of verse one. ".Length + "Text of verse two.".Length;
			var expectedLine = new StringBuilder("0\t\tp\tMRK\t4\t1\tFred\t\tWith great gusto and quivering frustration\t{1}\u00A0Text of verse one. {2}\u00A0Text of verse two.\t\t\t");
			expectedLine.Append(textLength);
			Assert.AreEqual(expectedLine.ToString(),
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", null, null, true, true, includeSecondaryReferenceText, null, null).AsObjectArray().ToList()));
			var actor = new Glyssen.VoiceActor.VoiceActor {Name = "ActorGuy1"};
			expectedLine.Insert(2, "ActorGuy1");
			Assert.AreEqual(expectedLine.ToString(),
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", actor, null, true, true, includeSecondaryReferenceText, null, null).AsObjectArray().ToList()));
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

			int textLength = "Text of verse three, part two. ".Length + "Text of verse four. ".Length + "Text of verse five.".Length;
			var expectedLine = new StringBuilder("0\t\tp\tMRK\t4\t3\t\t\t\tText of verse three, part two. {4}\u00A0Text of verse four. {5}\u00A0Text of verse five.\t\t\t");
			expectedLine.Append(textLength);
			Assert.AreEqual(expectedLine.ToString(),
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", null, null, true, true, includeSecondaryReferenceText, null, null).AsObjectArray().ToList()));
			var actor = new Glyssen.VoiceActor.VoiceActor {Name = "ActorGuy1"};
			expectedLine.Insert(2, "ActorGuy1");
			Assert.AreEqual(expectedLine.ToString(),
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", actor, null, true, true, includeSecondaryReferenceText, null, null).AsObjectArray().ToList()));
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

			int textLength = "Text of verse one. ".Length + "Text of verse two.".Length;
			var expectedLine = new StringBuilder("0\t\tp\tMRK\t4\t1\tnarrator (MRK)\t\tWith great gusto and quivering frustration\t{1}\u00A0Text of verse one. {2}\u00A0Text of verse two.\t\t\t");
			expectedLine.Append(textLength);
			Assert.AreEqual(expectedLine.ToString(),
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", null, "narrator-MRK", true, true, includeSecondaryReferenceText, null, null).AsObjectArray().ToList()));
			var actor = new Glyssen.VoiceActor.VoiceActor {Name = "ActorGuy1"};
			expectedLine.Insert(2, "ActorGuy1");
			Assert.AreEqual(expectedLine.ToString(),
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", actor, "narrator-MRK", true, true, includeSecondaryReferenceText, null, null).AsObjectArray().ToList()));
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
			var expectedLine = new StringBuilder("0\t\tp\tMRK\t4\t1\tMarko\t\tWith great gusto and quivering frustration\t{1}\u00A0Text of verse one. {2}\u00A0Text of verse two.\t\t\t");
			expectedLine.Append(textLength);
			Assert.AreEqual(expectedLine.ToString(),
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", null, null, true, true, false, null, null).AsObjectArray().ToList()));
			var actor = new Glyssen.VoiceActor.VoiceActor {Name = "ActorGuy1"};
			expectedLine.Insert(2, "ActorGuy1");
			Assert.AreEqual(expectedLine.ToString(),
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", actor, null, true, true, false, null, null).AsObjectArray().ToList()));
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
			var expectedLine = new StringBuilder("0\t\tp\tMRK\t4\t1\tFred/Marko\t\tWith great gusto and quivering frustration\t{1}\u00A0Text of verse one. {2}\u00A0Text of verse two.\t\t\t");
			expectedLine.Append(textLength);
			Assert.AreEqual(expectedLine.ToString(),
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", null, null, false, true, false, null, null).AsObjectArray().ToList()));
			expectedLine.Insert(2, "ActorGuy1");
			Assert.AreEqual(expectedLine.ToString(),
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", actor, null, false, true, false, null, null).AsObjectArray().ToList()));
		}

		[Test]
		public void GetTabSeparatedLine_GetExportDataForBlock_SpecifyOnlyEnglishReferenceText_OutputContainsReferenceText()
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
			var expectedLine = new StringBuilder("0\tActorGuy1\tp\tMRK\t4\t1\tFred\t\tWith great gusto and quivering frustration\t{1}\u00A0Text of verse one. {2}\u00A0Text of verse two.\t{1-2}\u00A0Text of verses one and two bridged in harmony and goodness.\t\t");
			expectedLine.Append(textLength);
			Assert.AreEqual(expectedLine.ToString(),
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", actor, null, true, true, false, null, null).AsObjectArray().ToList()));
		}

		[Test]
		public void GetTabSeparatedLine_GetExportDataForBlock_PrimaryAndSecondaryReferenceTexts_OutputContainsReferenceText()
		{
			var block = new Block("p", 4);
			block.IsParagraphStart = true;
			block.CharacterId = "Fred";
			block.Delivery = "With great gusto and quivering frustration";
			block.BlockElements.Add(new Verse("1"));
			block.BlockElements.Add(new ScriptText("Text of verse one. "));
			block.BlockElements.Add(new Verse("2"));
			block.BlockElements.Add(new ScriptText("Text of verse two."));
			block.SetMatchedReferenceBlock(new Block("p", 4, 1, 2).AddVerse("1-2", "Texto de versiculos uno y dos en harmonia y bondad."));
			block.ReferenceBlocks.Single().SetMatchedReferenceBlock(new Block("p", 4, 1, 2).AddVerse("1-2", "Text of verses one and two bridged in harmony and goodness."));

			var actor = new Glyssen.VoiceActor.VoiceActor { Name = "ActorGuy1" };

			int textLength = "Text of verse one. ".Length + "Text of verse two.".Length;
			var expectedLine = new StringBuilder("0\tActorGuy1\tp\tMRK\t4\t1\tFred\t\tWith great gusto and quivering frustration\t" +
				"{1}\u00A0Text of verse one. {2}\u00A0Text of verse two.\t{1-2}\u00A0Text of verses one and two bridged in harmony and goodness.\t" +
				"{1-2}\u00A0Texto de versiculos uno y dos en harmonia y bondad.\t");
			expectedLine.Append(textLength);
			Assert.AreEqual(expectedLine.ToString(),
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", actor, null, true, true, true, null, null).AsObjectArray().ToList()));
		}

		[Test]
		public void GetTabSeparatedLine_GetExportDataForBlock_EnglishReferenceTextMissing_OutputContainsReferenceText()
		{
			var block = new Block("p", 4);
			block.IsParagraphStart = true;
			block.CharacterId = "Fred";
			block.Delivery = "With great gusto and quivering frustration";
			block.BlockElements.Add(new Verse("1"));
			block.BlockElements.Add(new ScriptText("Text of verse one. "));
			block.BlockElements.Add(new Verse("2"));
			block.BlockElements.Add(new ScriptText("Text of verse two."));
			block.SetMatchedReferenceBlock(new Block("p", 4, 1, 2).AddVerse("1-2", "Texto de versiculos uno y dos en harmonia y bondad."));

			var actor = new Glyssen.VoiceActor.VoiceActor { Name = "ActorGuy1" };

			int textLength = "Text of verse one. ".Length + "Text of verse two.".Length;
			var expectedLine = new StringBuilder("0\tActorGuy1\tp\tMRK\t4\t1\tFred\t\tWith great gusto and quivering frustration\t" +
				"{1}\u00A0Text of verse one. {2}\u00A0Text of verse two.\t\t{1-2}\u00A0Texto de versiculos uno y dos en harmonia y bondad.\t");
			expectedLine.Append(textLength);
			Assert.AreEqual(expectedLine.ToString(),
				ProjectExporter.GetTabSeparatedLine(ProjectExporter.GetExportDataForBlock(block, 0, "MRK", actor, null, true, true, true, null, null).AsObjectArray().ToList()));
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

			var data = ProjectExporter.GetExportDataForBlock(block, 465, "MRK", actor, null, true, true, true, @"c:\wherever\whenever\however", "MyProject").AsObjectArray();
			Assert.AreEqual(14, data.Length);
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
				exporter.FullFileName = Path.Combine(tempDir.Path, Path.ChangeExtension("base", Constants.kExcelFileExtension));
				Assert.IsFalse(exporter.ExportNow(false).Any());
				Assert.IsTrue(Directory.Exists(exporter.ActorDirectory));
				foreach (var actor in project.CharacterGroupList.AssignedGroups.Select(g => g.VoiceActor.Name))
				{
					Assert.IsTrue(File.Exists(Path.Combine(exporter.ActorDirectory,
						Path.ChangeExtension(actor, Constants.kExcelFileExtension))));
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
				exporter.FullFileName = Path.Combine(tempDir.Path, Path.ChangeExtension("base", Constants.kExcelFileExtension));
				Assert.IsFalse(exporter.ExportNow(false).Any());
				Assert.IsTrue(Directory.Exists(exporter.BookDirectory));
				Assert.IsTrue(File.Exists(Path.Combine(exporter.BookDirectory, Path.ChangeExtension("3JN", Constants.kExcelFileExtension))));
				Assert.IsTrue(File.Exists(Path.Combine(exporter.BookDirectory, Path.ChangeExtension("JUD", Constants.kExcelFileExtension))));
			}
		}
	}
}
