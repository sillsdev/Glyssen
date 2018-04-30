using System.Collections.Generic;
using System.Linq;
using Glyssen.Shared.Script;
using SIL.Xml;

namespace Waxuquerque.Export
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
			var gs = new GlyssenScript(project.Name, project.Metadata);

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

				if (block.VernacularBlockElements != null && block.VernacularBlockElements.Any())
				{
					var gsBlock = new ScriptBlock
					{
						// Enhance: add localizedCharacterId to script
						Character = block.CharacterId,
						Id = blockId++,
						ReferenceTexts = GetReferenceTexts(project, block),
						Tag = block.StyleTag,
						VernacularText = new TextWithLanguage {BlockElements = block.VernacularBlockElements.ToList()},
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

		private static List<TextWithLanguage> GetReferenceTexts(Project project, ProjectExporter.ExportBlock block)
		{
			var referenceTexts = new List<TextWithLanguage>(2)
			{
				new TextWithLanguage
				{
					LanguageCode = project.ReferenceText.LanguageLdml,
					BlockElements = block.PrimaryReferenceTextBlockElements?.ToList()
				}
			};
			if (project.ReferenceText.HasSecondaryReferenceText)
			{
				referenceTexts.Add(new TextWithLanguage
				{
					LanguageCode = project.ReferenceText.SecondaryReferenceText.LanguageLdml,
					BlockElements = block.SecondaryReferenceTextBlockElements?.ToList()
				});
			}
			return referenceTexts;
		}
	}
}