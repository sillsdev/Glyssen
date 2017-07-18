using System.Collections.Generic;
using Glyssen.Shared.Script;
using SIL.Xml;

namespace Glyssen
{
	/// <summary>
	/// Handles the generation of an xml-based script which can be provided to other applications (e.g. HearThis)
	/// </summary>
	public static class ScriptExporter
	{
		public static void MakeGlyssenScriptFile(Project project, IEnumerable<ProjectExporter.ExportBlock> data,
			string outputPath)
		{
			var glyssenScript = CreateGlyssenScript(project, data);
			XmlSerializationHelper.SerializeToFile(outputPath, glyssenScript);
		}

		internal static GlyssenScript CreateGlyssenScript(Project project, IEnumerable<ProjectExporter.ExportBlock> data)
		{
			var gs = new GlyssenScript(project.Metadata);

			string bookCode = null;
			int blockId = 1;
			List<ScriptChapter> chapters = new List<ScriptChapter>();
			int chapter = -1;
			List<ScriptBlock> blocks = new List<ScriptBlock>();
			foreach (var block in data)
			{
				string blockBookCode = block.BookId;
				int blockChapterNumber = block.ChapterNumber;
				string blockCharacterId = block.CharacterId;

				var newChapter = chapter != -1 && blockChapterNumber > chapter;
				var newBook = bookCode != null && blockBookCode != bookCode;
				if (newChapter || newBook)
				{
					chapters.Add(new ScriptChapter {Id = chapter, Blocks = blocks});
					blocks = new List<ScriptBlock>();
					blockId = 1;
					chapter = blockChapterNumber;
				}
				if (newBook)
				{
					gs.Script.Books.Add(new ScriptBook {Id = bookCode, Chapters = chapters});
					chapters = new List<ScriptChapter>();
					bookCode = blockBookCode;
					chapter = -1;
				}
				if (!project.DramatizationPreferences.IncludeCharacter(blockCharacterId))
					continue;

				// I don't see any point in exporting a block with no vernacular text
				string vernacularText = block.Text;
				if (!string.IsNullOrWhiteSpace(vernacularText))
				{
					var gsBlock = new ScriptBlock
					{
						Character = block.CharacterId,
						Id = blockId++,
						PrimaryReferenceTextContent =
							new TextWithLanguage
							{
								LanguageCode = project.ReferenceText.LanguageLdml,
								Text = block.PrimaryReferenceText
							},
						SecondaryReferenceTextContent =
							project.ReferenceText.HasSecondaryReferenceText ?
							new TextWithLanguage
							{
								LanguageCode = project.ReferenceText.SecondaryReferenceText.LanguageLdml,
								Text = block.SecondaryReferenceText
							} :
							null,
						Tag = block.StyleTag,
						VernacularText = new TextWithLanguage { Text = vernacularText},
						Verse = block.InitialStartVerseNumber.ToString()
					};
					var actor = block.VoiceActor;
					gsBlock.Actor = string.IsNullOrEmpty(actor) ? "unassigned" : actor;

					blocks.Add(gsBlock);
				}
				bookCode = blockBookCode;
				chapter = blockChapterNumber;
			}

			chapters.Add(new ScriptChapter {Id = chapter, Blocks = blocks});
			gs.Script.Books.Add(new ScriptBook {Id = bookCode, Chapters = chapters});

			return gs;
		}
	}
}