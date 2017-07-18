using System.Collections.Generic;
using System.Linq;
using Glyssen;
using NUnit.Framework;

namespace GlyssenTests
{
	class ScriptExporterTests
	{
		private static readonly ProjectExporter.ExportBlock s_matChapter0BookTitle = new ProjectExporter.ExportBlock
		{
			BookId = "MAT",
			ChapterNumber = 0,
			CharacterId = "book title or chapter (MAT)",
			Text = "De Good Nyews Bout Jedus Christ Wa Matthew Write"
		};

		private static readonly IEnumerable<ProjectExporter.ExportBlock> s_matChapter1 = new List<ProjectExporter.ExportBlock>
		{
			new ProjectExporter.ExportBlock
			{
				BookId = "MAT",
				ChapterNumber = 1,
				CharacterId = "narrator (MAT)",
				Text = "Dis yah de people wa dey write down say been kin ta Jedus Christ, fom way back ta Abraham time. Jedus come out fom King David fambly, an King David come out fom Abraham fambly."
			}
		};

		private static readonly IEnumerable<ProjectExporter.ExportBlock> s_judChapter1 = new List<ProjectExporter.ExportBlock>
		{
			new ProjectExporter.ExportBlock
			{
				BookId = "JUD",
				ChapterNumber = 1,
				CharacterId = "narrator (JUD)",
				Text = "A Jude, wa da wok fa Jedus Christ. A James broda."
			}
		};

		private static readonly IEnumerable<ProjectExporter.ExportBlock> s_revChapter1 = new List<ProjectExporter.ExportBlock>
		{
			new ProjectExporter.ExportBlock
			{
				BookId = "REV",
				ChapterNumber = 1,
				CharacterId = "narrator (REV)",
				Text = "Dis book laan we bout de ting dem wa Jedus Christ show ta dem people wa da do God wok. God fus show Jedus dem ting yah, so dat Jedus kin mek dem wa da do e wok know dem ting wa gwine happen tareckly. Christ sen e angel ta John wa beena wok fa um, an e mek de angel show John dem ting yah. "
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
		public void FirstChapterIs0_FirstChapterInScriptIs0()
		{
			var project = TestProject.CreateBasicTestProject();
			var glyssenScript = ScriptExporter.CreateGlyssenScript(project, GetTestData(includeChapter0BookTitle: true));

			Assert.That(glyssenScript.Script.Books[0].Chapters[0].Id, Is.EqualTo(0));
		}

		[Test]
		public void FirstChapterIs1_FirstChapterInScriptIs1()
		{
			var project = TestProject.CreateBasicTestProject();
			var glyssenScript = ScriptExporter.CreateGlyssenScript(project, GetTestData(includeChapter0BookTitle: false));

			Assert.That(glyssenScript.Script.Books[0].Chapters[0].Id, Is.EqualTo(1));
		}

		[Test]
		public void SingleChapterBook_ProcessedCorrectly()
		{
			var project = TestProject.CreateBasicTestProject();
			var glyssenScript = ScriptExporter.CreateGlyssenScript(project, s_judChapter1.Union(s_revChapter1));

			Assert.That(glyssenScript.Script.Books[0].Id, Is.EqualTo("JUD"));
			Assert.That(glyssenScript.Script.Books[0].Chapters[0].Id, Is.EqualTo(1));
			Assert.That(glyssenScript.Script.Books[0].Chapters[0].Blocks[0].Character, Is.EqualTo("narrator (JUD)"));
			Assert.That(glyssenScript.Script.Books[0].Chapters[0].Blocks[0].VernacularText.Text, Is.EqualTo("A Jude, wa da wok fa Jedus Christ. A James broda."));

			Assert.That(glyssenScript.Script.Books[1].Id, Is.EqualTo("REV"));
			Assert.That(glyssenScript.Script.Books[1].Chapters[0].Id, Is.EqualTo(1));
			Assert.That(glyssenScript.Script.Books[1].Chapters[0].Blocks[0].Character, Is.EqualTo("narrator (REV)"));
			Assert.That(glyssenScript.Script.Books[1].Chapters[0].Blocks[0].VernacularText.Text, Is.EqualTo("Dis book laan we bout de ting dem wa Jedus Christ show ta dem people wa da do God wok. God fus show Jedus dem ting yah, so dat Jedus kin mek dem wa da do e wok know dem ting wa gwine happen tareckly. Christ sen e angel ta John wa beena wok fa um, an e mek de angel show John dem ting yah. "));
		}
	}
}
