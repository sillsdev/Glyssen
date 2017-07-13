using System.Collections.Generic;
using System.Diagnostics;
using Glyssen.Shared.Script;
using SIL.Xml;

namespace Glyssen
{
	/// <summary>
	/// Handles the generation of an xml-based script which can be provided to other applications (e.g. HearThis)
	/// </summary>
	public static class ScriptExporter
	{
		public static void MakeGlyssenScriptFile(Project project, IEnumerable<List<object>> data, string outputPath)
		{
			var gs = new GlyssenScript(project.Metadata) {Script = new Script {Books = new List<ScriptBook>()}};

			string bookCode = null;
			int blockId = 1;
			List<ScriptChapter> chapters = new List<ScriptChapter>();
			int chapter = 0;
			List<ScriptBlock> blocks = new List<ScriptBlock>();
			foreach (var row in data)
			{
				string blockBookCode = (string) row[ProjectExporter.GetColumnIndex(ExportColumn.BookId, project)];
				int blockChapterNumber = (int) row[ProjectExporter.GetColumnIndex(ExportColumn.Chapter, project)];
				string blockCharacterId = (string) row[ProjectExporter.GetColumnIndex(ExportColumn.CharacterId, project)];

				if (blockChapterNumber != chapter)
				{
					chapters.Add(new ScriptChapter {Id = chapter, Blocks = blocks});
					blocks = new List<ScriptBlock>();
					blockId = 1;
					chapter = blockChapterNumber;
				}
				if (bookCode != null && blockBookCode != bookCode)
				{
					gs.Script.Books.Add(new ScriptBook {Id = bookCode, Chapters = chapters});
					chapters = new List<ScriptChapter>();
					bookCode = blockBookCode;
					chapter = 0;
				}
				if (!project.DramatizationPreferences.IncludeCharacter(blockCharacterId))
					continue;

				// I don't see any point in exporting a block with no vernacular text
				string vernacularText = (string) row[ProjectExporter.GetColumnIndex(ExportColumn.VernacularText, project)];
				if (!string.IsNullOrWhiteSpace(vernacularText))
				{
					var gsBlock = new ScriptBlock
					{
						Character = (string) row[ProjectExporter.GetColumnIndex(ExportColumn.CharacterId, project)],
						Id = blockId++,
						PrimaryReferenceTextContent =
							new TextWithLanguage
							{
								LanguageCode = project.ReferenceText.LanguageLdml,
								Text = (string) row[ProjectExporter.GetColumnIndex(ExportColumn.PrimaryReferenceText, project)]
							},
						SecondaryReferenceTextContent =
							project.ReferenceText.HasSecondaryReferenceText ?
							new TextWithLanguage
							{
								LanguageCode = project.ReferenceText.SecondaryReferenceText.LanguageLdml,
								Text = (string) row[ProjectExporter.GetColumnIndex(ExportColumn.SecondaryReferenceText, project)]
							} :
							null,
						Tag = (string) row[ProjectExporter.GetColumnIndex(ExportColumn.ParaTag, project)],
						VernacularText = new TextWithLanguage { Text = vernacularText},
						Verse = ((int) row[ProjectExporter.GetColumnIndex(ExportColumn.Verse, project)]).ToString(),
					};
					var actor = (string) row[ProjectExporter.GetColumnIndex(ExportColumn.Actor, project)];
					gsBlock.Actor = !string.IsNullOrEmpty(actor) ? actor : "unassigned";
					Debug.Assert(gsBlock.Actor != "unassigned");

					blocks.Add(gsBlock);
				}
				bookCode = blockBookCode;
				chapter = blockChapterNumber;
			}

			chapters.Add(new ScriptChapter {Id = chapter, Blocks = blocks});
			gs.Script.Books.Add(new ScriptBook {Id = bookCode, Chapters = chapters});

			XmlSerializationHelper.SerializeToFile(outputPath, gs);
		}
	}
}