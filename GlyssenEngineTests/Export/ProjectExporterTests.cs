using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Glyssen.Shared;
using GlyssenCharacters;
using GlyssenEngine;
using GlyssenEngine.Bundle;
using GlyssenEngine.Casting;
using GlyssenEngine.Character;
using GlyssenEngine.Export;
using GlyssenEngine.Rules;
using GlyssenEngine.Script;
using GlyssenEngine.ViewModels;
using GlyssenEngineTests.Script;
using GlyssenSharedTests;
using NUnit.Framework;
using SIL.Extensions;
using SIL.Reflection;
using static System.IO.Path;
using static GlyssenEngine.Bundle.ExtraBiblicalMaterialSpeakerOption;
using static GlyssenEngine.Export.ProjectExporter;

namespace GlyssenEngineTests.Export
{
	[TestFixture]
	class ProjectExporterTests
	{
		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = null;
			CharacterDetailData.TabDelimitedCharacterDetailData = null;
		}

		[TearDown]
		public void Teardown()
		{
			TestReferenceText.ForgetCustomReferenceTexts();
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
			Assert.That(data.All(t => t.VoiceActor == null), Is.True);
		}

		[Test]
		public void GetExportData_SomeButNotAllActorsAssigned_VoiceActorsAssignedOnlyForBlocks()
		{
			var project = TestProject.CreateBasicTestProject();
			var metadata = (GlyssenDblTextMetadata)ReflectionHelper.GetField(project, "m_metadata");
			metadata.IncludeChapterAnnouncementForFirstChapter = true;
			metadata.IncludeChapterAnnouncementForSingleChapterBooks = true;
			project.VoiceActorList.AllActors = new List<VoiceActor>
			{
				new VoiceActor {Id = 1, Name = "Ralphy"}
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
			Assert.That(rowsForCharacterAssignedToActor.Any(), Is.True);
			Assert.That(rowsForCharacterNotAssignedToActor.Any(), Is.True);
			Assert.That(rowsForCharacterAssignedToActor.All(t => t.VoiceActor == "Ralphy"), Is.True);
			Assert.That(rowsForCharacterNotAssignedToActor.All(t => String.IsNullOrEmpty(t.VoiceActor)), Is.True);
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
			Assert.That(chapterBlockForEphesians.BookId, Is.EqualTo("EPH"));
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
			Assert.That(data.Count(t => t.StyleTag == "c" && t.ChapterNumber == 1), Is.EqualTo(2));
		}

		[Test]
		public void GetExportData_SkipChapterOne_OutputDoesNotIncludeChapterAnnouncementForFirstChapter()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.EPH, TestProject.TestBook.JUD);
			var metadata = (GlyssenDblTextMetadata)ReflectionHelper.GetField(project, "m_metadata");
			metadata.IncludeChapterAnnouncementForFirstChapter = false;
			var exporter = new ProjectExporter(project);
			var data = exporter.GetExportData();
			Assert.That(data.Any(t => t.StyleTag == "c" && t.ChapterNumber == 1), Is.False);
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
			Assert.That(chapterBlockForEphesians.BookId, Is.EqualTo("EPH"));
		}

		[Test]
		public void GetExportData_IntrosIncluded_IntroMaterialInExportData()
		{
			var expectedIntroParagraphs = Regex.Matches(Properties.Resources.TestJOS, "para style=\"i", RegexOptions.Compiled).Count;
			Assert.That(expectedIntroParagraphs, Is.GreaterThan(0),
				"The test resource \"TestJos.xml\" has been modified to remove intro material. It won't work for this test.");
			var project = TestProject.CreateTestProject(TestProject.TestBook.JOS);
			project.DramatizationPreferences.BookIntroductionsDramatization = Narrator;
			var exporter = new ProjectExporter(project);
			var data = exporter.GetExportData();
			Assert.That(data.Any(), Is.True);
			Assert.That(data.Count(t => t.StyleTag.StartsWith("i", StringComparison.Ordinal)),
				Is.EqualTo(expectedIntroParagraphs));
		}

		[Test]
		public void GetExportData_IntrosOmitted_NoIntroMaterialInExportData()
		{
			Assert.That(Regex.Matches(Properties.Resources.TestJOS, "para style=\"i", RegexOptions.Compiled).Count, Is.GreaterThan(0),
				"The test resource \"TestJos.xml\" has been modified to remove intro material. It won't work for this test.");
			var project = TestProject.CreateTestProject(TestProject.TestBook.JOS);
			project.DramatizationPreferences.BookIntroductionsDramatization = Omitted;
			var exporter = new ProjectExporter(project);
			var data = exporter.GetExportData();
			Assert.That(data.Any(), Is.True);
			Assert.That(data.Any(t => t.StyleTag.StartsWith("i", StringComparison.Ordinal)), Is.False);
		}

		[Test]
		public void GetExportData_SectionHeadsIncluded_SectionHeadsInExportData()
		{
			var expectedSectionHeadParagraphs = Regex.Matches(Properties.Resources.TestJUD, "para style=\"s", RegexOptions.Compiled).Count;
			Assert.That(expectedSectionHeadParagraphs, Is.GreaterThan(0),
				"The test resource \"TestJud.xml\" has been modified to remove section heads. It won't work for this test.");
			var project = TestProject.CreateTestProject(TestProject.TestBook.JUD);
			project.DramatizationPreferences.SectionHeadDramatization = Narrator;
			var exporter = new ProjectExporter(project);
			var data = exporter.GetExportData();
			Assert.That(data.Any(), Is.True);
			Assert.That(data.Count(t => t.StyleTag.StartsWith("s", StringComparison.Ordinal)),
				Is.EqualTo(expectedSectionHeadParagraphs));
		}

		[Test]
		public void GetExportData_SectionHeadsOmitted_NoSectionHeadsInExportData()
		{
			Assert.That(Regex.Matches(Properties.Resources.TestJUD, "para style=\"s", RegexOptions.Compiled).Count,
				Is.GreaterThan(0),
				"The test resource \"TestJud.xml\" has been modified to remove section heads. It won't work for this test.");
			var project = TestProject.CreateTestProject(TestProject.TestBook.JUD);
			project.DramatizationPreferences.SectionHeadDramatization = Omitted;
			var exporter = new ProjectExporter(project);
			var data = exporter.GetExportData();
			Assert.That(data.Any(), Is.True);
			Assert.That(data.Any(t => t.StyleTag.StartsWith("s", StringComparison.Ordinal)), Is.False);
		}

		[Test]
		public void GetExportData_TitlesAndChaptersIncluded_TitlesAndChaptersInExportData()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.EPH);
			var expected = project.SkipChapterAnnouncementForFirstChapter ? 6 : 7;
			project.DramatizationPreferences.BookTitleAndChapterDramatization = Narrator;
			var exporter = new ProjectExporter(project);
			var data = exporter.GetExportData();
			Assert.That(data.Any(), Is.True);
			Assert.That(data.Count(t => t.CharacterId == "book title or chapter (EPH)"),
				Is.EqualTo(expected));
		}

		[Test]
		public void GetExportData_TitlesAndChaptersOmitted_NoTitlesOrChaptersInExportData()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.EPH);
			project.DramatizationPreferences.BookTitleAndChapterDramatization = Omitted;
			var exporter = new ProjectExporter(project);
			var data = exporter.GetExportData();
			Assert.That(data.Any(), Is.True);
			Assert.That(data.Any(t => t.CharacterId == "book title or chapter (EPH)"), Is.False);
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
			int prevNumber = data[0].Number;
			Assert.That(prevNumber, Is.GreaterThanOrEqualTo(project.IncludedBooks.First().GetScriptBlocks().Count));
			Assert.That(prevNumber, Is.EqualTo(exporter.GetExportData("GAL").Count + 1));
			for (var index = 1; index < data.Count; index++)
			{
				var block = data[index];
				Assert.That(++prevNumber, Is.EqualTo(block.Number));
			}
			Assert.That(data.All(t => t.BookId == "2JN"), Is.True);
		}

		[Test]
		public void GetExportData_SpecifiedActor_OutputOnlyIncludesBlocksForThatActor()
		{
			var project = TestProject.CreateBasicTestProject();
			TestProject.SimulateDisambiguationForAllBooks(project);
			var metadata = (GlyssenDblTextMetadata)ReflectionHelper.GetField(project, "m_metadata");
			metadata.IncludeChapterAnnouncementForFirstChapter = true;
			metadata.IncludeChapterAnnouncementForSingleChapterBooks = true;
			project.IncludedBooks[0].SingleVoice = false;
			project.VoiceActorList.AllActors = new List<VoiceActor>
			{
				new VoiceActor {Id = 1, Name = "Marlon"}
			};
			project.CharacterGroupList.CharacterGroups.AddRange(new[]
			{
				new CharacterGroup(project),
				new CharacterGroup(project)
			});
			project.CharacterGroupList.CharacterGroups[0].CharacterIds.Add("Michael, archangel");
			project.CharacterGroupList.CharacterGroups[0].AssignVoiceActor(1);

			var exporter = new ProjectExporter(project);

			var expectedBlockNumber = exporter.GetExportData().Single(b => b.CharacterId == "Michael, archangel").Number;

			var data = exporter.GetExportData(voiceActorId: 1).Single();
			Assert.That(data.VoiceActor, Is.EqualTo("Marlon"));
			Assert.That(data.ChapterNumber, Is.EqualTo(1));
			Assert.That(data.VerseNumber, Is.EqualTo(9));
			Assert.That(data.CharacterId, Is.EqualTo("Michael, archangel"));
			Assert.That(data.Number, Is.EqualTo(expectedBlockNumber));
		}

		[Test]
		public void GetExportData_SingleVoice_DeliveryColumnOmitted()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.GAL, TestProject.TestBook.JUD);
			project.IncludedBooks[0].SingleVoice = true;
			project.IncludedBooks[1].SingleVoice = true;
			project.VoiceActorList.AllActors = new List<VoiceActor>
			{
				new VoiceActor {Id = 1, Name = "Marlon"},
				new VoiceActor {Id = 2, Name = "Aiden"}
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
			Assert.That(data.Count, Is.GreaterThan(2));
			for (var index = 0; index < data.Count; index++)
			{
				var block = data[index];
				Assert.That(index + 1, Is.EqualTo(block.Number));
			}

			var iStartOfJude = data.IndexOf(d => d.BookId == "JUD");
			Assert.That(iStartOfJude > 0, Is.True);
			Assert.That(data.Take(iStartOfJude).All(d => d.BookId == "GAL"), Is.True);
			Assert.That(data.Skip(iStartOfJude).All(d => d.BookId == "JUD"), Is.True);
			Assert.That(data.Take(iStartOfJude).All(d => d.CharacterId == "narrator (GAL)"), Is.True);
			Assert.That(data.Skip(iStartOfJude).All(d => d.CharacterId == "narrator (JUD)"), Is.True);
			Assert.That(data.Take(iStartOfJude).All(d => d.VoiceActor == "Marlon"), Is.True);
			Assert.That(data.Skip(iStartOfJude).All(d => d.VoiceActor == "Aiden"), Is.True);
		}

		[Test]
		public void GetExportData_BlocksAreJoinedToReferenceText_OutputContainsMatchedAndUnmatchedReferenceText()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.JUD);
			project.DramatizationPreferences.SectionHeadDramatization = ActorOfEitherGender;
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
				if (refBlock.BlockElements.First() is Verse verseElement)
					secondaryRefBlock.AddVerse(verseElement.Number, "Secondary");
				else
					secondaryRefBlock.BlockElements = new List<BlockElement> {new ScriptText("the angel named Mike verbalized.")};
				refBlock.SetMatchedReferenceBlock(secondaryRefBlock);
			}
			var primaryReferenceText = TestReferenceText.CreateTestReferenceText("JUD", blocks);
			project.ReferenceText = primaryReferenceText;
			var exporter = new ProjectExporter(project);
			var data = exporter.GetExportData().ToList();

			Assert.That(data.All(d => d.BookId == "JUD" && d.ChapterNumber == 1), Is.True);
			var i = 0;
			var row = data[i++];
			Assert.That(i, Is.EqualTo(row.Number)); // Row 1
			Assert.That(row.StyleTag, Is.EqualTo("p"));
			Assert.That(row.VerseNumber, Is.EqualTo(1));
			Assert.That(row.CharacterId, Is.EqualTo("narrator (JUD)"));
			Assert.That(row.VernacularText, Is.EqualTo("{1}\u00A0A"));
			Assert.That(row.AdditionalReferenceText, Is.EqualTo("{1}\u00A0Ayy"));
			Assert.That(row.EnglishReferenceText, Is.EqualTo("{1}\u00A0Secondary"));

			row = data[i++];
			Assert.That(i, Is.EqualTo(row.Number)); // Row 2
			Assert.That(row.StyleTag, Is.EqualTo("s"));
			Assert.That(row.VerseNumber, Is.EqualTo(1));
			Assert.That(row.CharacterId, Is.EqualTo("section head (JUD)"));
			Assert.That(row.VernacularText, Is.EqualTo("Jude complains"));
			Assert.That(string.IsNullOrEmpty(row.AdditionalReferenceText), Is.True);
			Assert.That(string.IsNullOrEmpty(row.EnglishReferenceText), Is.True);

			row = data[i++];
			Assert.That(i, Is.EqualTo(row.Number)); // Row 3
			Assert.That(row.StyleTag, Is.EqualTo("p"));
			Assert.That(row.VerseNumber, Is.EqualTo(2));
			Assert.That(row.CharacterId, Is.EqualTo("Enoch"));
			Assert.That(row.VernacularText, Is.EqualTo("{2}\u00A0B"));
			Assert.That(string.IsNullOrEmpty(row.AdditionalReferenceText), Is.True);
			Assert.That(string.IsNullOrEmpty(row.EnglishReferenceText), Is.True);

			row = data[i++];
			Assert.That(i, Is.EqualTo(row.Number)); // Row 4
			Assert.That(row.StyleTag, Is.EqualTo("p"));
			Assert.That(row.VerseNumber, Is.EqualTo(3));
			Assert.That(row.CharacterId, Is.EqualTo("narrator (JUD)"));
			Assert.That(row.VernacularText, Is.EqualTo("{3}\u00A0C"));
			Assert.That(string.IsNullOrEmpty(row.AdditionalReferenceText), Is.True);
			Assert.That(string.IsNullOrEmpty(row.EnglishReferenceText), Is.True);

			row = data[i++];
			Assert.That(row.Number, Is.EqualTo(0));
			Assert.That(row.StyleTag, Is.EqualTo("p"));
			Assert.That(row.VerseNumber, Is.EqualTo(2));
			Assert.That(row.CharacterId, Is.EqualTo("narrator (JUD)"));
			Assert.That(string.IsNullOrEmpty(row.VernacularText), Is.True);
			Assert.That(row.AdditionalReferenceText, Is.EqualTo("{2-3}\u00A0Bee Cee"));
			Assert.That(row.EnglishReferenceText, Is.EqualTo("{2-3}\u00A0Secondary"));
			Assert.That(row.Length, Is.EqualTo(0));

			row = data[i++];
			Assert.That(row.Number, Is.EqualTo(5)); // Row 5
			Assert.That(row.StyleTag, Is.EqualTo("p"));
			Assert.That(row.VerseNumber, Is.EqualTo(4));
			Assert.That(row.CharacterId, Is.EqualTo("Michael"));
			Assert.That(row.VernacularText, Is.EqualTo("{4}\u00A0D"));
			Assert.That(row.AdditionalReferenceText, Is.EqualTo("{4}\u00A0Dee, Michael said."));
			Assert.That(row.EnglishReferenceText, Is.EqualTo("{4}\u00A0Secondary the angel named Mike verbalized."));

			row = data[i++];
			Assert.That(row.Number, Is.EqualTo(6));
			Assert.That(row.StyleTag, Is.EqualTo("p"));
			Assert.That(row.VerseNumber, Is.EqualTo(5));
			Assert.That(row.CharacterId, Is.EqualTo("narrator (JUD)"));
			Assert.That(row.VernacularText, Is.EqualTo("{5}\u00A0E "));
			Assert.That(row.AdditionalReferenceText, Is.EqualTo("{5}\u00A0Ey"));
			Assert.That(row.EnglishReferenceText, Is.EqualTo("{5}\u00A0Secondary"));

			row = data[i++];
			Assert.That(row.Number, Is.EqualTo(7));
			Assert.That(row.StyleTag, Is.EqualTo("p"));
			Assert.That(row.VerseNumber, Is.EqualTo(6));
			Assert.That(row.CharacterId, Is.EqualTo("narrator (JUD)"));
			Assert.That(row.VernacularText, Is.EqualTo("{6}\u00A0F"));
			Assert.That(row.AdditionalReferenceText, Is.EqualTo("{6}\u00A0Ef ||| + 5 SECs |||"));
			Assert.That(row.EnglishReferenceText, Is.EqualTo("{6}\u00A0Secondary ||| + 5 SECs |||"));

			Assert.That(i, Is.EqualTo(data.Count));
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
				if (refBlock.BlockElements.First() is Verse verseElement)
					secondaryRefBlock.AddVerse(verseElement.Number, "Some secondary reference text");
				else
					secondaryRefBlock.BlockElements = new List<BlockElement> {new ScriptText("Some secondary reference text")};
				refBlock.SetMatchedReferenceBlock(secondaryRefBlock);
			}
			var primaryReferenceText = TestReferenceText.CreateTestReferenceText("MRK", refBlocks);
			project.ReferenceText = primaryReferenceText;
			var exporter = new ProjectExporter(project);
			var data = exporter.GetExportData().ToList();
			Assert.That(mark.Blocks.Count, Is.EqualTo(data.Count));

			var narratorInOutput = CharacterVerseData.GetCharacterNameForUi(narrator);

			Assert.That(data.All(d => d.BookId == "MRK" && d.ChapterNumber == 4), Is.True);
			var i = 0;
			var row = data[i];
			Assert.That(row.VerseNumber, Is.EqualTo(39));
			Assert.That(narratorInOutput, Is.EqualTo(row.CharacterId));
			Assert.That(mark.GetScriptBlocks()[i].GetText(true), Is.EqualTo(row.VernacularText));
			Assert.That(refBlocks[i].GetText(true), Is.EqualTo(row.AdditionalReferenceText));

			row = data[++i];
			Assert.That(row.VerseNumber, Is.EqualTo(39));
			Assert.That(row.CharacterId, Is.EqualTo("Jesus"));
			Assert.That(mark.GetScriptBlocks()[i].GetText(true), Is.EqualTo(row.VernacularText));
			Assert.That(refBlocks[i].GetText(true) + "||| + 1.5 SECs |||", Is.EqualTo(row.AdditionalReferenceText));
			Assert.That(row.EnglishReferenceText, Is.EqualTo("Some secondary reference text ||| + 1.5 SECs |||"));

			row = data[++i];
			Assert.That(row.VerseNumber, Is.EqualTo(39));
			Assert.That(narratorInOutput, Is.EqualTo(row.CharacterId));
			Assert.That(mark.GetScriptBlocks()[i].GetText(true), Is.EqualTo(row.VernacularText));
			Assert.That(string.IsNullOrEmpty(row.AdditionalReferenceText as string), Is.True);

			row = data[++i];
			Assert.That(row.VerseNumber, Is.EqualTo(39));
			Assert.That(row.CharacterId, Is.EqualTo("Jesus"));
			Assert.That(mark.GetScriptBlocks()[i].GetText(true), Is.EqualTo(row.VernacularText));
			Assert.That(string.IsNullOrEmpty(row.AdditionalReferenceText as string), Is.True);

			row = data[++i];
			Assert.That(string.IsNullOrEmpty(row.AsObjectArray()[0] as string), Is.True);
			Assert.That(row.VerseNumber, Is.EqualTo(39));
			Assert.That(narratorInOutput, Is.EqualTo(row.CharacterId));
			Assert.That(mark.GetScriptBlocks()[i].GetText(true), Is.EqualTo(row.VernacularText));
			Assert.That(refBlocks[i - 2].GetText(true), Is.EqualTo(row.AdditionalReferenceText));

			row = data[++i];
			Assert.That(string.IsNullOrEmpty(row.AsObjectArray()[0] as string), Is.True);
			Assert.That(row.VerseNumber, Is.EqualTo(40));
			Assert.That(row.CharacterId, Is.EqualTo("Jesus"));
			Assert.That(mark.GetScriptBlocks()[i].GetText(true), Is.EqualTo(row.VernacularText));
			Assert.That(refBlocks[i - 2].GetText(true) + " ||| + 5 SECs |||", Is.EqualTo(row.AdditionalReferenceText));
		}

		[Test]
		public void GetExportData_BlocksAreJoinedToCustomReferenceTextWhosePageHeaderIsDifferentFromTheMainTitle_ChapterAnnouncementBasedOnPageHeader()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.JUD);
			project.ReferenceText = TestReferenceText.CreateCustomReferenceText(TestReferenceTextResource.AzeriJUD);
			var metadata = (GlyssenDblTextMetadata)ReflectionHelper.GetField(project, "m_metadata");
			metadata.IncludeChapterAnnouncementForFirstChapter = true;
			metadata.IncludeChapterAnnouncementForSingleChapterBooks = true;
			var exporter = new ProjectExporter(project);

			var data = exporter.GetExportData().ToList();

			Assert.That(data.All(d => d.BookId == "JUD"), Is.True);
			Assert.That(data[0].AdditionalReferenceText, Is.EqualTo("YӘHUDANIN MӘKTUBU"));
			Assert.That(data[0].EnglishReferenceText, Is.EqualTo("JUDE"));
			Assert.That(data.Skip(1).All(d => d.ChapterNumber == 1), Is.True);
			Assert.That(data[1].AdditionalReferenceText, Is.EqualTo("YӘHUDA 1"));
			Assert.That(data[1].EnglishReferenceText, Is.EqualTo("JUDE CHP 1"));
			var matchedRows = data.Where(d => d.VernacularText != null && d.AdditionalReferenceText != null).ToList();
			Assert.That(matchedRows.Count > data.Count / 2, Is.True); // This is kind of arbitrary, but I just want to say we got a reasonable number of matches
			Assert.That(matchedRows.Any(d => d.AdditionalReferenceText.Contains("Ә")), Is.True); // A letter that should be in Azeri, but not English
			Assert.That(matchedRows.All(d => d.EnglishReferenceText != null), Is.True);
			Assert.That(matchedRows.Any(d => d.EnglishReferenceText.Contains(" the ")), Is.True); // A word that should be in English, but not Azeri
			// Since the test version of Jude does not match perfectly with this reference text, we expect two rows
			// where the vernacular has no corresponding reference text.
			var extra = CharacterVerseData.GetCharacterNameForUi(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.ExtraBiblical));
			var narrator = CharacterVerseData.GetCharacterNameForUi(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.Narrator));
			Assert.That(data.Where(d => d.StyleTag == "s1").All(d => d.CharacterId == extra), Is.True);
			var scriptureRowsWithNoReferenceText = data.Where(d => d.AdditionalReferenceText == null && d.StyleTag != "s1").ToList();
			Assert.That(scriptureRowsWithNoReferenceText.Count, Is.EqualTo(2));
			Assert.That(scriptureRowsWithNoReferenceText.Count(d => d.CharacterId == narrator), Is.EqualTo(1));
			Assert.That(scriptureRowsWithNoReferenceText.All(d => d.EnglishReferenceText == null), Is.True);
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

			Assert.That(data.All(d => d.BookId == "JUD"), Is.True);
			Assert.That(data[0].AdditionalReferenceText, Is.EqualTo("Иуда"));
			Assert.That(data[0].EnglishReferenceText, Is.EqualTo("JUDE"));
			Assert.That(data.Skip(1).All(d => d.ChapterNumber == 1), Is.True);
			Assert.That(data[1].AdditionalReferenceText, Is.EqualTo("Иуда 1"));
			Assert.That(data[1].EnglishReferenceText, Is.EqualTo("JUDE CHP 1"));
			var matchedRows = data.Where(d => d.VernacularText != null && d.AdditionalReferenceText != null).ToList();
			Assert.That(matchedRows.Count > data.Count / 2, Is.True); // This is kind of arbitrary, but I just want to say we got a reasonable number of matches
			Assert.That(matchedRows.Any(d => d.AdditionalReferenceText.Contains("п")), Is.True); // A letter that should be in Russian, but not English
			Assert.That(matchedRows.All(d => d.EnglishReferenceText != null), Is.True);
			Assert.That(matchedRows.Any(d => d.EnglishReferenceText.Contains(" the ")), Is.True); // A word that should be in English, but not Russian
			// Since the test version of Jude does not match perfectly with the standard reference texts, we expect two Scripture rows
			// where the vernacular has no corresponding reference text.
			var extra = CharacterVerseData.GetCharacterNameForUi(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.ExtraBiblical));
			var narrator = CharacterVerseData.GetCharacterNameForUi(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.Narrator));
			Assert.That(data.Where(d => d.StyleTag == "s1")
				.All(d => d.CharacterId == extra), Is.True);
			var scriptureRowsWithNoReferenceText = data.Where(d => d.AdditionalReferenceText == null &&
				d.StyleTag != "s1").ToList();
			Assert.That(scriptureRowsWithNoReferenceText.Count, Is.EqualTo(2));
			Assert.That(scriptureRowsWithNoReferenceText.Count(d => d.CharacterId == narrator), Is.EqualTo(1));
			Assert.That(scriptureRowsWithNoReferenceText.All(d => d.EnglishReferenceText == null), Is.True);
		}

		[Test]
		public void GetExportData_ExportAnnotationsInSeparateRows_ReferenceTextsContainAnnotations()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.JUD, TestProject.TestBook.REV);
			project.DramatizationPreferences.SectionHeadDramatization = ActorOfEitherGender;
			project.ReferenceText = TestReferenceText.CreateCustomReferenceText(TestReferenceTextResource.AzeriJUD, TestReferenceTextResource.AzeriREV);
			var exporter = new ProjectExporter(project);
			exporter.ExportAnnotationsInSeparateRows = true;

			var data = exporter.GetExportData().ToList();

			//SFX (sfx come before verse text)
			var rowsForVerse12 = data.Where(d => d.BookId == "JUD" && d.ChapterNumber == 1 && d.VerseNumber == 12).ToList();
			Assert.That(rowsForVerse12.Count, Is.EqualTo(2));
			var annotationRowForVerse12 = rowsForVerse12[0];
			var rowForVerse12 = rowsForVerse12[1];
			Assert.That(annotationRowForVerse12.AdditionalReferenceText,
				Is.EqualTo(annotationRowForVerse12.EnglishReferenceText));
			Assert.That(annotationRowForVerse12.AdditionalReferenceText,
				Does.StartWith(Sound.kDoNotCombine + exporter.AnnotationElementSeparator + "{SFX"));
			Assert.That(rowForVerse12.AdditionalReferenceText, Does.Not.Contain("|||"));

			//Pause for final verse in book (pauses come after verse text)
			var rowsForJude25 = data.Where(d => d.BookId == "JUD" && d.ChapterNumber == 1 && d.VerseNumber == 25).ToList();
			Assert.That(rowsForJude25.Count, Is.EqualTo(2));
			var rowForJude25 = rowsForJude25[0];
			var annotationRowForJude25 = rowsForJude25[1];
			Assert.That(rowForJude25.AdditionalReferenceText, Does.Not.Contain("|||"));
			Assert.That(annotationRowForJude25.AdditionalReferenceText.Equals(annotationRowForJude25.EnglishReferenceText) &&
				annotationRowForJude25.AdditionalReferenceText.Equals(string.Format(Pause.kPauseSecondsFormat, "5")), Is.True);

			//Pause for non-final verse in book (pauses come after verse text)
			var rowsForRev1V3 = data.Where(d => d.BookId == "REV" && d.ChapterNumber == 1 && d.VerseNumber == 3).ToList();
			Assert.That(rowsForRev1V3.Count, Is.EqualTo(3));
			var rowForRev1V3 = rowsForRev1V3[0];
			var annotationRowForRev1V3 = rowsForRev1V3[1];
			var sectionHeadRowForRev1V3 = rowsForRev1V3[2];
			Assert.That(rowForRev1V3.AdditionalReferenceText, Does.Not.Contain("|||"));
			Assert.That(annotationRowForRev1V3.AdditionalReferenceText.Equals(annotationRowForRev1V3.EnglishReferenceText) &&
				annotationRowForRev1V3.AdditionalReferenceText.Equals(string.Format(Pause.kPauseSecondsFormat, "2")), Is.True);
			Assert.That(sectionHeadRowForRev1V3.CharacterId,
				Is.EqualTo(CharacterVerseData.GetStandardCharacterNameForUi(CharacterVerseData.StandardCharacter.ExtraBiblical, "REV")));

			//Pause for final verse in chapter (pauses come after verse text)
			var rowsForRev1V20 = data.Where(d => d.BookId == "REV" && d.ChapterNumber == 1 && d.VerseNumber == 20).ToList();
			Assert.That(rowsForRev1V20.Count, Is.EqualTo(2));
			var rowForRev1V20 = rowsForRev1V20[0];
			var annotationRowForRev1V20 = rowsForRev1V20[1];
			Assert.That(rowForRev1V20.AdditionalReferenceText, Does.Not.Contain("|||"));
			Assert.That(annotationRowForRev1V20.AdditionalReferenceText.Equals(annotationRowForRev1V20.EnglishReferenceText) &&
				annotationRowForRev1V20.AdditionalReferenceText.Equals(string.Format(Pause.kPauseSecondsFormat, "2")), Is.True);
		}

		[TestCase(ExportFileType.Excel)]
		[TestCase(ExportFileType.TabSeparated)]
		public void GetExportData_AnnotationsCombinedWithData_ReferenceTextsContainAnnotations(ExportFileType exportFileType)
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.JUD, TestProject.TestBook.REV);
			project.DramatizationPreferences.SectionHeadDramatization = ActorOfEitherGender;
			project.ReferenceText = TestReferenceText.CreateCustomReferenceText(TestReferenceTextResource.AzeriJUD, TestReferenceTextResource.AzeriREV);
			var exporter = new ProjectExporter(project) {SelectedFileType = exportFileType};
			// This is the default: exporter.ExportAnnotationsInSeparateRows = false;

			var data = exporter.GetExportData().ToList();

			//SFX (music/sfx come before verse text)
			var rowsForVerse12 = data.Where(d => d.BookId == "JUD" && d.ChapterNumber == 1 && d.VerseNumber == 12).ToList();
			var rowForVerse12 = rowsForVerse12.Single();
			var annotationInfoPlusVerseNum = Sound.kDoNotCombine + exporter.AnnotationElementSeparator + "{SFX--Eerie--Starts @ v12} {12}\u00A0";
			Assert.That(rowForVerse12.AdditionalReferenceText, Does.StartWith(annotationInfoPlusVerseNum));
			Assert.That(rowForVerse12.EnglishReferenceText, Does.StartWith(annotationInfoPlusVerseNum));
			Assert.That(rowForVerse12.VernacularText,
				Is.EqualTo("{12}\u00A0Gikelo lewic i karamawu me mar ka gicamo matek mukato kare laboŋo lworo, kun giparo pi komgi keken. " +
				"Gubedo calo pol ma pii pe iye ma yamo kolo; girom ki yadi ma nyiggi pe nen i kare me cekgi, ma giputo lwitgi woko, " +
				"yam guto kiryo. "));

			//Pause for final verse in book (pauses come after verse text)
			var annotationForEndOfBook = " " + string.Format(Pause.kPauseSecondsFormat, "5");
			var rowsForJude25 = data.Where(d => d.BookId == "JUD" && d.ChapterNumber == 1 && d.VerseNumber == 25).ToList();
			var rowForJude25 = rowsForJude25.Single();
			
			Assert.That(rowForJude25.AdditionalReferenceText, Does.EndWith(annotationForEndOfBook));
			Assert.That(rowForJude25.EnglishReferenceText, Does.EndWith(annotationForEndOfBook));
			Assert.That(rowForJude25.VernacularText,
				Is.EqualTo("{25}\u00A0Deyo, dit, loc ki twer ducu obed bot Lubaŋa acel keken, ma Lalarwa, pi Yecu Kricito Rwotwa, " +
				"cakke ma peya giketo lobo, nio koni, ki kare ma pe gik. Amen."));

			//Pause for non-final verse in book (pauses come after verse text)
			var rowsForRev1V3 = data.Where(d => d.BookId == "REV" && d.ChapterNumber == 1 && d.VerseNumber == 3).ToList();
			Assert.That(rowsForRev1V3.Count, Is.EqualTo(2));
			var rowForRev1V3 = rowsForRev1V3[0];
			var sectionHeadRowForRev1V3 = rowsForRev1V3[1];
			var annotationForTwoSecondPause = " " + string.Format(Pause.kPauseSecondsFormat, "2");
			Assert.That(rowForRev1V3.AdditionalReferenceText, Does.EndWith(annotationForTwoSecondPause));
			Assert.That(rowForRev1V3.EnglishReferenceText, Does.EndWith(annotationForTwoSecondPause));
			Assert.That(rowForRev1V3.VernacularText,
				Is.EqualTo("{3}\u00A0Ŋat ma kwano lok ma gitito i buk man i nyim lwak tye ki gum, jo ma winyo bene tye ki gum, ki jo ma lubo " +
					"gin ma gicoyo iye bene tye ki gum, pien kare doŋ cok."));
			Assert.That(sectionHeadRowForRev1V3.CharacterId,
				Is.EqualTo(CharacterVerseData.GetStandardCharacterNameForUi(CharacterVerseData.StandardCharacter.ExtraBiblical, "REV")));

			//Pause for final verse in chapter (pauses come after verse text)
			var rowForRev1V20 = data.Single(d => d.BookId == "REV" && d.ChapterNumber == 1 && d.VerseNumber == 20);
			Assert.That(rowForRev1V20.AdditionalReferenceText, Does.EndWith(annotationForTwoSecondPause));
			Assert.That(rowForRev1V20.EnglishReferenceText, Does.EndWith(annotationForTwoSecondPause));
			Assert.That(rowForRev1V20.VernacularText,
				Is.EqualTo("{20}\u00A0Koŋ agonnyi tyen lok me muŋ me lakalatwe abiro ma ineno i ciŋa tuŋ lacuc, ki okar-mac abiro me jabu. " +
					"Lakalatwe abiro gin aye lumalaika pa lwak muye Kricito ma gitye i kabedo abiro mapatpat, doŋ okar-mac abiro-ni gin " +
					"aye lwak muye Kricito ma gitye i kabedo abiro mapatpat.”"));

			// PG-1399: 5-second pause at end of book
			var rowForRev22V21 = data.Single(d => d.BookId == "REV" && d.ChapterNumber == 22 && d.VerseNumber >= 21);
			Assert.That(rowForRev22V21.AdditionalReferenceText, Does.EndWith(annotationForEndOfBook));
			Assert.That(rowForRev22V21.EnglishReferenceText, Does.EndWith(annotationForEndOfBook));
			Assert.That(rowForRev22V21.VernacularText, Is.EqualTo("{21}\u00A0Kica pa Rwot Yecu obed ki jo pa Lubaŋa ducu. Amen."));
		}

		[Test]
		public void GetExportData_EmptyReferenceTextForVersesWithAnnotations_AnnotationsInsertedIntoEmptyReferenceTextsWithoutCrashing()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.JUD, TestProject.TestBook.REV);
			project.DramatizationPreferences.SectionHeadDramatization = ActorOfEitherGender;
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
			Assert.That(blockJud25.LastVerseNum, Is.EqualTo(25));
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
			Assert.That(annotationInfo + " {12}\u00A0", Is.EqualTo(rowForVerse12.AdditionalReferenceText));
			Assert.That(annotationInfo, Is.EqualTo(rowForVerse12.EnglishReferenceText.TrimEnd()));
			Assert.That(rowForVerse12.AdditionalReferenceText, Does.StartWith(annotationInfo + " {12}\u00A0"));
			Assert.That(rowForVerse12.VernacularText, Is.EqualTo("{12}\u00A0Gikelo lewic i karamawu me mar ka gicamo matek mukato kare laboŋo lworo, kun giparo pi komgi keken. " +
				"Gubedo calo pol ma pii pe iye ma yamo kolo; girom ki yadi ma nyiggi pe nen i kare me cekgi, ma giputo lwitgi woko, " +
				"yam guto kiryo. "));

			//Pause for final verse in book (pauses come after verse text)
			var rowsForJude25 = data.Where(d => d.BookId == "JUD" && d.ChapterNumber == 1 && d.VerseNumber == 25).ToList();
			var rowForJude25 = rowsForJude25.Single();
			annotationInfo = string.Format(Pause.kPauseSecondsFormat, "5");
			Assert.That(rowForJude25.AdditionalReferenceText, Is.EqualTo("{25}\u00A0" + annotationInfo));
			Assert.That(annotationInfo, Is.EqualTo(rowForJude25.EnglishReferenceText.TrimStart()));
			Assert.That(rowForJude25.VernacularText,
				Is.EqualTo("{25}\u00A0Deyo, dit, loc ki twer ducu obed bot Lubaŋa acel keken, ma Lalarwa, pi Yecu Kricito Rwotwa, " +
				"cakke ma peya giketo lobo, nio koni, ki kare ma pe gik. Amen."));

			//Pause for non-final verse in book (pauses come after verse text)
			var rowForRev1V3 = data.First(d => d.BookId == "REV" && d.ChapterNumber == 1 && d.VerseNumber == 3);
			annotationInfo = string.Format(Pause.kPauseSecondsFormat, "2");
			Assert.That(rowForRev1V3.AdditionalReferenceText, Is.EqualTo("{3}\u00A0" + annotationInfo));
			Assert.That(annotationInfo, Is.EqualTo(rowForRev1V3.EnglishReferenceText.TrimStart()));
			Assert.That(rowForRev1V3.VernacularText,
				Is.EqualTo("{3}\u00A0Ŋat ma kwano lok ma gitito i buk man i nyim lwak tye ki gum, jo ma winyo bene tye ki gum, ki jo ma lubo " +
				"gin ma gicoyo iye bene tye ki gum, pien kare doŋ cok."));

			//Pause for final verse in chapter (pauses come after verse text)
			var rowForRev1V20 = data.Single(d => d.BookId == "REV" && d.ChapterNumber == 1 && d.VerseNumber == 20);
			Assert.That(rowForRev1V20.AdditionalReferenceText, Is.EqualTo("{20}\u00A0" + annotationInfo));
			Assert.That(annotationInfo, Is.EqualTo(rowForRev1V20.EnglishReferenceText.TrimStart()));
			Assert.That(rowForRev1V20.VernacularText,
				Is.EqualTo("{20}\u00A0Koŋ agonnyi tyen lok me muŋ me lakalatwe abiro ma ineno i ciŋa tuŋ lacuc, ki okar-mac abiro me jabu. " +
				"Lakalatwe abiro gin aye lumalaika pa lwak muye Kricito ma gitye i kabedo abiro mapatpat, doŋ okar-mac abiro-ni gin " +
				"aye lwak muye Kricito ma gitye i kabedo abiro mapatpat.”"));
		}

		/// <summary>
		/// PG-905
		/// </summary>
		[Test]
		public void GetExportData_NullPrimaryReferenceTextForAppendedAnnotation_PrimaryReferenceTextContainsOnlyAnnotation()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.REV);
			project.DramatizationPreferences.SectionHeadDramatization = ActorOfEitherGender;
			project.ReferenceText = TestReferenceText.CreateCustomReferenceText(TestReferenceTextResource.AzeriREV);

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
			Assert.That(rowForRev22V17.AdditionalReferenceText, Does.EndWith(annotationInfo));
			Assert.That(annotationInfo, Is.EqualTo(rowForRev22V17.EnglishReferenceText));
		}

		/// <summary>
		/// PG-905
		/// </summary>
		[Test]
		public void GetExportData_NullPrimaryReferenceTextForPrependedAnnotation_PrimaryReferenceTextContainsOnlyAnnotation()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.REV);
			project.DramatizationPreferences.SectionHeadDramatization = ActorOfEitherGender;
			project.ReferenceText = TestReferenceText.CreateCustomReferenceText(TestReferenceTextResource.AzeriREV);

			// Force a block that has a prepended annotation to have a null secondary reference.
			var vernBlock = project.IncludedBooks.First().GetScriptBlocks().Single(b => b.ChapterNumber == 1 && b.InitialStartVerseNumber == 7);
			vernBlock.SetMatchedReferenceBlock("{7}Verse Seven in Azeri.");

			var exporter = new ProjectExporter(project) { SelectedFileType = ExportFileType.Excel };

			var data = exporter.GetExportData().ToList();

			//SFX (music/sfx come before verse text)
			var rowsForRev1V7 = data.Where(d => d.BookId == "REV" && d.ChapterNumber == 1 && d.VerseNumber == 7).ToList();
			var rowForRev1V7 = rowsForRev1V7.Single();
			var annotationInfo = Sound.kDoNotCombine + exporter.AnnotationElementSeparator + "{Music--Starts @ v7} ";
			Assert.That(rowForRev1V7.AdditionalReferenceText, Does.StartWith(annotationInfo));
			Assert.That(annotationInfo, Is.EqualTo(rowForRev1V7.EnglishReferenceText));
		}

		[Test]
		public void GetExportData_ExportAnnotationsInSeparateRows_AnnotationWithOffset_ReferenceTextContainsAnnotationInCorrectLocation()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.MRK);
			project.ReferenceText = TestReferenceText.CreateCustomReferenceText(TestReferenceTextResource.FrenchMRK);
			var exporter = new ProjectExporter(project);
			exporter.ExportAnnotationsInSeparateRows = true;

			var data = exporter.GetExportData().ToList();

			//Pause mid-verse
			var rowsForMark4V39 = data.Where(d => d.BookId == "MRK" && d.ChapterNumber == 4 && d.VerseNumber == 39).ToList();
			Assert.That(rowsForMark4V39.Count, Is.EqualTo(4));
			var narratorTextRow1ForMark4V39 = rowsForMark4V39[0];
			var jesusTextRowForMark4V39 = rowsForMark4V39[0];
			var annotationRowForMark4V39 = rowsForMark4V39[2];
			var narratorTextRow2ForMark4V39 = rowsForMark4V39[3];
			Assert.That(narratorTextRow1ForMark4V39.AdditionalReferenceText, Does.Not.Contain("|||"));
			Assert.That(jesusTextRowForMark4V39.AdditionalReferenceText, Does.Not.Contain("|||"));
			Assert.That(annotationRowForMark4V39.AdditionalReferenceText.Equals(annotationRowForMark4V39.EnglishReferenceText) &&
				annotationRowForMark4V39.AdditionalReferenceText.Equals(string.Format(Pause.kPauseSecondsFormat, "1.5")), Is.True);
			Assert.That(narratorTextRow2ForMark4V39.AdditionalReferenceText, Does.Not.Contain("|||"));
		}

		[Test]
		public void GetExportData_AnnotationsCombinedWithData_AnnotationWithOffset_ReferenceTextContainsAnnotationInCorrectLocation()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.MRK);
			project.ReferenceText = TestReferenceText.CreateCustomReferenceText(TestReferenceTextResource.FrenchMRK);
			var exporter = new ProjectExporter(project);
			// This is the default: exporter.ExportAnnotationsInSeparateRows = false;

			var data = exporter.GetExportData().ToList();

			//Pause mid-verse
			var rowsForMark4V39 = data.Where(d => d.BookId == "MRK" && d.ChapterNumber == 4 && d.VerseNumber == 39).ToList();
			Assert.That(rowsForMark4V39.Count, Is.EqualTo(3));
			var narratorTextRow1ForMark4V39 = rowsForMark4V39[0];
			var jesusTextRowForMark4V39 = rowsForMark4V39[1];
			var narratorTextRow2ForMark4V39 = rowsForMark4V39[2];
			Assert.That(narratorTextRow1ForMark4V39.AdditionalReferenceText, Does.Not.Contain("|||"));
			var annotationInfo = " " + string.Format(Pause.kPauseSecondsFormat, "1.5");
			Assert.That(jesusTextRowForMark4V39.AdditionalReferenceText, Does.EndWith(annotationInfo));
			Assert.That(jesusTextRowForMark4V39.EnglishReferenceText, Does.EndWith(annotationInfo));
			Assert.That(narratorTextRow2ForMark4V39.AdditionalReferenceText, Does.Not.Contain("|||"));
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

			Assert.That(data.Columns[(int)ExportColumn.BlockId].ColumnName, Is.EqualTo("#"));
			Assert.That(data.Columns[(int)ExportColumn.Actor].ColumnName, Is.EqualTo("Actor"));
			Assert.That(data.Columns[(int)ExportColumn.ParaTag].ColumnName, Is.EqualTo("Tag"));
			Assert.That(data.Columns[(int)ExportColumn.BookId].ColumnName, Is.EqualTo("Book"));
			Assert.That(data.Columns[(int)ExportColumn.Chapter].ColumnName, Is.EqualTo("Chapter"));
			Assert.That(data.Columns[(int)ExportColumn.Verse].ColumnName, Is.EqualTo("Verse"));
			Assert.That(data.Columns[(int)ExportColumn.CharacterId].ColumnName, Is.EqualTo("Character"));
			Assert.That(data.Columns[(int)ExportColumn.CharacterIdLocalized].ColumnName, Is.EqualTo("Character (localized)"));
			Assert.That(data.Columns[(int)ExportColumn.Delivery].ColumnName, Is.EqualTo("Delivery"));
			Assert.That(data.Columns[(int)ExportColumn.VernacularText].ColumnName, Is.EqualTo("Text"));
			Assert.That(data.Columns[(int)ExportColumn.EnglishReferenceText].ColumnName, Is.EqualTo("English Director's Guide"));
			Assert.That(data.Columns[(int)ExportColumn.AdditionalReferenceText].ColumnName, Is.EqualTo("Russian Director's Guide"));
			Assert.That(data.Columns[(int)ExportColumn.VernacularTextLength].ColumnName, Is.EqualTo("Size"));
			if (includeClipColumn)
			{
				Assert.That(data.Columns[(int)ExportColumn.ClipFileLink].ColumnName, Is.EqualTo("Clip File"));
				Assert.That(data.Columns.Count, Is.EqualTo(14));
			}
			else
				Assert.That(data.Columns.Count, Is.EqualTo(13));
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
			Assert.That(GetTabSeparatedLine(GetExportDataForBlock(block, 0, "MRK", null, null,
					true, true, includeSecondaryReferenceText, null, null).AsObjectArray().ToList()),
				Is.EqualTo(expectedLine.ToString()));
			var actor = new VoiceActor {Name = "ActorGuy1"};
			expectedLine.Insert(2, "ActorGuy1");
			Assert.That(GetTabSeparatedLine(GetExportDataForBlock(block, 0, "MRK", actor, null,
				true, true, includeSecondaryReferenceText, null, null).AsObjectArray().ToList()),
				Is.EqualTo(expectedLine.ToString()));
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
			Assert.That(GetTabSeparatedLine(GetExportDataForBlock(block, 0, "MRK", null, null,
				true, true, includeSecondaryReferenceText, null, null).AsObjectArray().ToList()),
				Is.EqualTo(expectedLine.ToString()));
			var actor = new VoiceActor {Name = "ActorGuy1"};
			expectedLine.Insert(2, "ActorGuy1");
			Assert.That(GetTabSeparatedLine(GetExportDataForBlock(block, 0, "MRK", actor, null,
				true, true, includeSecondaryReferenceText, null, null).AsObjectArray().ToList()),
				Is.EqualTo(expectedLine.ToString()));
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
			Assert.That(GetTabSeparatedLine(GetExportDataForBlock(block, 0, "MRK", null, "narrator-MRK", 
				true, true, includeSecondaryReferenceText, null, null).AsObjectArray().ToList()),
				Is.EqualTo(expectedLine.ToString()));
			var actor = new VoiceActor {Name = "ActorGuy1"};
			expectedLine.Insert(2, "ActorGuy1");
			Assert.That(GetTabSeparatedLine(GetExportDataForBlock(block, 0, "MRK", actor, "narrator-MRK",
				true, true, includeSecondaryReferenceText, null, null).AsObjectArray().ToList()),
				Is.EqualTo(expectedLine.ToString()));
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
			Assert.That(GetTabSeparatedLine(GetExportDataForBlock(block, 0, "MRK", null, null,
				true, true, false, null, null).AsObjectArray().ToList()),
				Is.EqualTo(expectedLine.ToString()));
			var actor = new VoiceActor {Name = "ActorGuy1"};
			expectedLine.Insert(2, "ActorGuy1");
			Assert.That(GetTabSeparatedLine(GetExportDataForBlock(block, 0, "MRK", actor, null,
				true, true, false, null, null).AsObjectArray().ToList()),
				Is.EqualTo(expectedLine.ToString()));
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

			var actor = new VoiceActor {Name = "ActorGuy1"};

			int textLength = "Text of verse one. ".Length + "Text of verse two.".Length;
			var expectedLine = new StringBuilder("0\t\tp\tMRK\t4\t1\tFred/Marko\t\tWith great gusto and quivering frustration\t{1}\u00A0Text of verse one. {2}\u00A0Text of verse two.\t\t\t");
			expectedLine.Append(textLength);
			Assert.That(GetTabSeparatedLine(GetExportDataForBlock(block, 0, "MRK", null, null,
				false, true, false, null, null).AsObjectArray().ToList()),
				Is.EqualTo(expectedLine.ToString()));
			expectedLine.Insert(2, "ActorGuy1");
			Assert.That(GetTabSeparatedLine(GetExportDataForBlock(block, 0, "MRK", actor, null,
				false, true, false, null, null).AsObjectArray().ToList()),
				Is.EqualTo(expectedLine.ToString()));
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

			var actor = new VoiceActor { Name = "ActorGuy1" };

			int textLength = "Text of verse one. ".Length + "Text of verse two.".Length;
			var expectedLine = new StringBuilder("0\tActorGuy1\tp\tMRK\t4\t1\tFred\t\tWith great gusto and quivering frustration\t{1}\u00A0Text of verse one. {2}\u00A0Text of verse two.\t\t{1-2}\u00A0Text of verses one and two bridged in harmony and goodness.\t");
			expectedLine.Append(textLength);
			Assert.That(GetTabSeparatedLine(GetExportDataForBlock(block, 0, "MRK", actor, null,
				true, true, false, null, null).AsObjectArray().ToList()),
				Is.EqualTo(expectedLine.ToString()));
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

			var actor = new VoiceActor { Name = "ActorGuy1" };

			int textLength = "Text of verse one. ".Length + "Text of verse two.".Length;
			var expectedLine = new StringBuilder("0\tActorGuy1\tp\tMRK\t4\t1\tFred\t\tWith great gusto and quivering frustration\t" +
				"{1}\u00A0Text of verse one. {2}\u00A0Text of verse two.\t{1-2}\u00A0Texto de versiculos uno y dos en harmonia y bondad.\t" +
				"{1-2}\u00A0Text of verses one and two bridged in harmony and goodness.\t");
			expectedLine.Append(textLength);
			Assert.That(GetTabSeparatedLine(GetExportDataForBlock(block, 0, "MRK", actor, null,
				true, true, true, null, null).AsObjectArray().ToList()),
				Is.EqualTo(expectedLine.ToString()));
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

			var actor = new VoiceActor { Name = "ActorGuy1" };

			int textLength = "Text of verse one. ".Length + "Text of verse two.".Length;
			var expectedLine = new StringBuilder("0\tActorGuy1\tp\tMRK\t4\t1\tFred\t\tWith great gusto and quivering frustration\t" +
				"{1}\u00A0Text of verse one. {2}\u00A0Text of verse two.\t{1-2}\u00A0Texto de versiculos uno y dos en harmonia y bondad.\t\t");
			expectedLine.Append(textLength);
			Assert.That(GetTabSeparatedLine(GetExportDataForBlock(block, 0, "MRK", actor, null,
				true, true, true, null, null).AsObjectArray().ToList()),
				Is.EqualTo(expectedLine.ToString()));
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

			var actor = new VoiceActor {Name = "ActorGuy1"};

			var data = GetExportDataForBlock(block, 465, "MRK", actor, null, true, true, true, @"c:\wherever\whenever\however", "MyProject").AsObjectArray();
			Assert.That(data.Length, Is.EqualTo(14));
			Assert.That(data.Last(),
				Is.EqualTo(@"c:\wherever\whenever\however\MRK\MyProject_00465_MRK_004_001.wav"));
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
					new VoiceActor { Id = i, Gender = ActorGender.Female, Name = "Judy" + i++ } :
					new VoiceActor { Id = i, Gender = ActorGender.Male, Name = "Bob" + i++ };
				project.VoiceActorList.AllActors.Add(actor);
				group.VoiceActorId = actor.Id;
			}
			Assert.That(project.CharacterGroupList.AnyVoiceActorAssigned(), Is.True);
			var exporter = new ProjectExporter(project)
			{
				SelectedFileType = ExportFileType.Excel,
				IncludeActorBreakdown = true
			};
			using (var tempDir = new SIL.TestUtilities.TemporaryFolder("PG855ExportActorExcelScripts"))
			{
				exporter.FullFileName = Combine(tempDir.Path, ChangeExtension("base", Constants.kExcelFileExtension));
				Assert.That(exporter.ExportNow(false).Any(), Is.False);
				Assert.That(Directory.Exists(exporter.ActorDirectory), Is.True);
				foreach (var actor in project.CharacterGroupList.AssignedGroups.Select(g => g.VoiceActor.Name))
				{
					Assert.That(Combine(exporter.ActorDirectory,
						ChangeExtension(actor, Constants.kExcelFileExtension)), Does.Exist);
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
				exporter.FullFileName = Combine(tempDir.Path,
					ChangeExtension("base", Constants.kExcelFileExtension));
				Assert.That(exporter.ExportNow(false).Any(), Is.False);
				Assert.That(Directory.Exists(exporter.BookDirectory), Is.True);
				Assert.That(Combine(exporter.BookDirectory,
					ChangeExtension("3JN", Constants.kExcelFileExtension)), Does.Exist);
				Assert.That(Combine(exporter.BookDirectory,
					ChangeExtension("JUD", Constants.kExcelFileExtension)), Does.Exist);
			}
		}
	}
}
