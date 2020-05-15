﻿using System.Collections.Generic;
using System.Linq;
using Glyssen.Shared;
using GlyssenEngine;
using GlyssenEngine.Export;
using NUnit.Framework;

namespace GlyssenEngineTests.Export
{
	class ScriptExporterTests
	{
		private static readonly ProjectExporter.ExportBlock s_matChapter0BookTitle = new ProjectExporter.ExportBlock
		{
			BookId = "MAT",
			ChapterNumber = 0,
			CharacterId = "book title or chapter (MAT)",
			VernacularBlockElements = new List<BlockElement>
			{
				new ScriptText("De Good Nyews Bout Jedus Christ Wa Matthew Write")

			}
		};

		private static readonly IEnumerable<ProjectExporter.ExportBlock> s_matChapter1 = new List<ProjectExporter.ExportBlock>
		{
			new ProjectExporter.ExportBlock
			{
				BookId = "MAT",
				ChapterNumber = 1,
				CharacterId = "narrator (MAT)",
				VernacularBlockElements = new List<BlockElement>
				{
					new ScriptText("Dis yah de people wa dey write down say been kin ta Jedus Christ, fom way back ta Abraham time. Jedus come out fom King David fambly, an King David come out fom Abraham fambly.")
				}
			}
		};

		private static readonly IEnumerable<ProjectExporter.ExportBlock> s_lukChapter1 = new List<ProjectExporter.ExportBlock>
		{
			new ProjectExporter.ExportBlock
			{
				BookId = "LUK",
				ChapterNumber = 1,
				CharacterId = "narrator (LUK)",
				VernacularBlockElements = new List<BlockElement>
				{
					new Verse("1-2"),
					new ScriptText("Ye paa awili Tiopillas")
				},
				AdditionalReferenceTextBlockElements = new List<BlockElement>
				{
					new Verse("1"),
					new ScriptText("Bikman Tiofilus, "),
					new Verse("2"),
					new ScriptText("Ol i bihainim tok bilong")
				},
				EnglishReferenceTextBlockElements = new List<BlockElement>
				{
					new Verse("1"),
					new ScriptText("Many have undertaken to set "),
					new Verse("2"),
					new ScriptText("just as they were handed")
				}
			}
		};

		private static readonly IEnumerable<ProjectExporter.ExportBlock> s_judChapter1 = new List<ProjectExporter.ExportBlock>
		{
			new ProjectExporter.ExportBlock
			{
				BookId = "JUD",
				ChapterNumber = 1,
				CharacterId = "narrator (JUD)",
				VernacularBlockElements = new List<BlockElement>
				{
					new Verse("1"),
					new ScriptText("A Jude, wa da wok fa Jedus Christ. A James broda.")
				}
			}
		};

		private static readonly IEnumerable<ProjectExporter.ExportBlock> s_revChapter1 = new List<ProjectExporter.ExportBlock>
		{
			new ProjectExporter.ExportBlock
			{
				BookId = "REV",
				ChapterNumber = 1,
				CharacterId = "narrator (REV)",
				VernacularBlockElements = new List<BlockElement>
				{
					new ScriptText("Dis book laan we bout de ting dem wa Jedus Christ show ta dem people wa da do God wok. "),
					new Verse("3"),
					new ScriptText("God fus show Jedus dem ting yah, so dat Jedus kin mek dem wa da do e wok know dem ting wa gwine happen tareckly. "),
					new Verse("4"),
					new ScriptText("Christ sen e angel ta John wa beena wok fa um, an e mek de angel show John dem ting yah. ")
				}
			}
		};


		private IEnumerable<ProjectExporter.ExportBlock> GetTestData(bool includeChapter0BookTitle)
		{
			var result = new List<ProjectExporter.ExportBlock>();
			if (includeChapter0BookTitle)
				result.Add(s_matChapter0BookTitle);
			result.AddRange(s_matChapter1);
			return result;
		}

		[Test]
		public void CreateGlyssenScript_FirstChapterIs0_FirstChapterInScriptIs0()
		{
			var project = TestProject.CreateBasicTestProject();
			var glyssenScript = ScriptExporter.CreateGlyssenScript(project, GetTestData(true));

			Assert.That(glyssenScript.Script.Books[0].Chapters[0].Id, Is.EqualTo(0));
		}

		[Test]
		public void CreateGlyssenScript_FirstChapterIs1_FirstChapterInScriptIs1()
		{
			var project = TestProject.CreateBasicTestProject();
			var glyssenScript = ScriptExporter.CreateGlyssenScript(project, GetTestData(false));

			Assert.That(glyssenScript.Script.Books[0].Chapters[0].Id, Is.EqualTo(1));
		}

		[Test]
		public void CreateGlyssenScript_DeliveryIsSet_DeliveryIsExported()
		{
			var project = TestProject.CreateBasicTestProject();
			var blockWhereMichaelSpeaks = project.IncludedBooks.Single().GetScriptBlocks().Single(b => b.InitialStartVerseNumber == 9 && b.IsQuote);
			blockWhereMichaelSpeaks.CharacterId = "Michael, archangel";
			blockWhereMichaelSpeaks.Delivery = "rebuking";
			var exporter = new ProjectExporter(project);
			var glyssenScript = ScriptExporter.CreateGlyssenScript(project, exporter.GetExportData(getBlockElements:true));

			var scriptBlocks = glyssenScript.Script.Books[0].Chapters[1].Blocks; // Chapter 0 is just the book title
			var michael = scriptBlocks.Single(b => b.Character == "Michael, archangel");
			Assert.AreEqual("rebuking", michael.Delivery);
		}

		[Test]
		public void CreateGlyssenScript_Jude_ParagraphStartInfoIsExported()
		{
			var project = TestProject.CreateBasicTestProject();
			var exporter = new ProjectExporter(project);
			var glyssenScript = ScriptExporter.CreateGlyssenScript(project, exporter.GetExportData(getBlockElements: true));

			string prevVerse = null;
			foreach (var block in glyssenScript.Script.Books[0].Chapters[1].Blocks)
			{
				if (block.Verse != prevVerse)
				{
					switch (block.Verse)
					{
						// These are the verses that correspond to paragraph breaks in TestJUD.xml
						case "1":
						case "2":
						case "3":
						case "5":
						case "8":
						case "14":
						case "16":
						case "17":
						case "24":
							Assert.IsTrue(block.IsParagraphStart);
							break;
						default:
							Assert.IsFalse(block.IsParagraphStart);
							break;
					}
					prevVerse = block.Verse;
				}
				else
				{
					// The first verse in Jude (in TestJUD.xml) is broken into to two paragraphs 
					Assert.AreEqual(block.Verse == "1", block.IsParagraphStart, $"Block should not have been a paragraph start: {block.Verse}");
				}
			}
		}

		[Test]
		public void CreateGlyssenScript_SingleChapterBook_ProcessedCorrectly()
		{
			var project = TestProject.CreateBasicTestProject();
			var glyssenScript = ScriptExporter.CreateGlyssenScript(project, s_judChapter1.Union(s_revChapter1));

			Assert.That(glyssenScript.Script.Books[0].Id, Is.EqualTo("JUD"));
			Assert.That(glyssenScript.Script.Books[0].Chapters[0].Id, Is.EqualTo(1));
			Assert.That(glyssenScript.Script.Books[0].Chapters[0].Blocks[0].Character, Is.EqualTo("narrator (JUD)"));
			Assert.That(glyssenScript.Script.Books[0].Chapters[0].Blocks[0].VernacularText.Text, Is.EqualTo("A Jude, wa da wok fa Jedus Christ. A James broda."));
			Assert.That(glyssenScript.Script.Books[0].Chapters[0].Blocks[0].VernacularText.BlockElements.Count, Is.EqualTo(2));
			Assert.That(glyssenScript.Script.Books[0].Chapters[0].Blocks[0].VernacularText.BlockElements.First(), Is.TypeOf(typeof(Verse)));
			Assert.That(((Verse)glyssenScript.Script.Books[0].Chapters[0].Blocks[0].VernacularText.BlockElements.First()).Number, Is.EqualTo("1"));
			Assert.That(glyssenScript.Script.Books[0].Chapters[0].Blocks[0].VernacularText.BlockElements.Skip(1).Single(), Is.TypeOf(typeof(ScriptText)));
			Assert.That(((ScriptText)glyssenScript.Script.Books[0].Chapters[0].Blocks[0].VernacularText.BlockElements.Skip(1).Single()).Content, Is.EqualTo("A Jude, wa da wok fa Jedus Christ. A James broda."));

			Assert.That(glyssenScript.Script.Books[1].Id, Is.EqualTo("REV"));
			Assert.That(glyssenScript.Script.Books[1].Chapters[0].Id, Is.EqualTo(1));
			Assert.That(glyssenScript.Script.Books[1].Chapters[0].Blocks[0].Character, Is.EqualTo("narrator (REV)"));
			Assert.That(glyssenScript.Script.Books[1].Chapters[0].Blocks[0].VernacularText.Text, Is.EqualTo("Dis book laan we bout de ting dem wa Jedus Christ show ta dem people wa da do God wok. God fus show Jedus dem ting yah, so dat Jedus kin mek dem wa da do e wok know dem ting wa gwine happen tareckly. Christ sen e angel ta John wa beena wok fa um, an e mek de angel show John dem ting yah. "));
			Assert.That(glyssenScript.Script.Books[1].Chapters[0].Blocks[0].VernacularText.BlockElements.Count, Is.EqualTo(5));
		}

		[Test]
		public void CreateGlyssenScript_HasPrimaryAndSecondaryReferenceTexts_ProcessedCorrectly()
		{
			var project = TestProject.CreateBasicTestProject();
			project.ReferenceText = ReferenceText.GetStandardReferenceText(ReferenceTextType.Russian);
			var glyssenScript = ScriptExporter.CreateGlyssenScript(project, s_lukChapter1);

			var block1 = glyssenScript.Script.Books[0].Chapters[0].Blocks[0];

			Assert.That(block1.VernacularText.BlockElements.Count, Is.EqualTo(2));

			var primaryRefText = block1.ReferenceTexts[0];
			Assert.That(primaryRefText.BlockElements.Count, Is.EqualTo(4));
			Assert.That(primaryRefText.BlockElements.First(), Is.TypeOf(typeof(Verse)));
			Assert.That(((Verse)primaryRefText.BlockElements.First()).Number, Is.EqualTo("1"));
			Assert.That(primaryRefText.BlockElements.Skip(1).First(), Is.TypeOf(typeof(ScriptText)));
			Assert.That(((ScriptText)primaryRefText.BlockElements.Skip(1).First()).Content, Is.EqualTo("Bikman Tiofilus, "));
			Assert.That(primaryRefText.BlockElements.Skip(2).First(), Is.TypeOf(typeof(Verse)));
			Assert.That(((Verse)primaryRefText.BlockElements.Skip(2).First()).Number, Is.EqualTo("2"));
			Assert.That(primaryRefText.BlockElements.Skip(3).First(), Is.TypeOf(typeof(ScriptText)));
			Assert.That(((ScriptText)primaryRefText.BlockElements.Skip(3).First()).Content, Is.EqualTo("Ol i bihainim tok bilong"));
			Assert.That(primaryRefText.Text, Is.EqualTo("Bikman Tiofilus, Ol i bihainim tok bilong"));

			var secondaryRefText = block1.ReferenceTexts[1];
			Assert.That(secondaryRefText.BlockElements.Count, Is.EqualTo(4));
			Assert.That(secondaryRefText.BlockElements.First(), Is.TypeOf(typeof(Verse)));
			Assert.That(((Verse)secondaryRefText.BlockElements.First()).Number, Is.EqualTo("1"));
			Assert.That(secondaryRefText.BlockElements.Skip(1).First(), Is.TypeOf(typeof(ScriptText)));
			Assert.That(((ScriptText)secondaryRefText.BlockElements.Skip(1).First()).Content, Is.EqualTo("Many have undertaken to set "));
			Assert.That(secondaryRefText.BlockElements.Skip(2).First(), Is.TypeOf(typeof(Verse)));
			Assert.That(((Verse)secondaryRefText.BlockElements.Skip(2).First()).Number, Is.EqualTo("2"));
			Assert.That(secondaryRefText.BlockElements.Skip(3).First(), Is.TypeOf(typeof(ScriptText)));
			Assert.That(((ScriptText)secondaryRefText.BlockElements.Skip(3).First()).Content, Is.EqualTo("just as they were handed"));
			Assert.That(secondaryRefText.Text, Is.EqualTo("Many have undertaken to set just as they were handed"));
		}
	}
}
