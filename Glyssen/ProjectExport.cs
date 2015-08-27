using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using DesktopAnalytics;
using Glyssen.Character;
using Glyssen.Properties;
using L10NSharp;
using SIL.Reporting;

namespace Glyssen
{
	public class ProjectExport
	{
		private const string Separator = "\t";

		private readonly Project m_project;
		private readonly bool m_includeVoiceActors;

		public ProjectExport(Project project)
		{
			m_project = project;
			m_includeVoiceActors = m_project.CharacterGroupList.AnyVoiceActorAssigned();
		}

		public void Export(IWin32Window owner)
		{
			var defaultDir = Settings.Default.DefaultExportDirectory;
			if (string.IsNullOrEmpty(defaultDir))
			{
				defaultDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			}

			using (var dlg = new SaveFileDialog())
			{
				dlg.Title = LocalizationManager.GetString("DialogBoxes.ExportDlg.Title", "Export Tab-Delimited Data");
				dlg.OverwritePrompt = true;
				dlg.InitialDirectory = defaultDir;
				dlg.FileName = "MRK.txt";
				dlg.Filter = string.Format("{0} ({1})|{1}|{2} ({3})|{3}",
					LocalizationManager.GetString("DialogBoxes.ExportDlg.TabDelimitedFileTypeLabel", "Tab-delimited files"), "*.txt",
					LocalizationManager.GetString("DialogBoxes.FileDlg.AllFilesLabel", "All Files"), "*.*");
				dlg.DefaultExt = ".txt";
				if (dlg.ShowDialog(owner) == DialogResult.OK)
				{
					Settings.Default.DefaultExportDirectory = Path.GetDirectoryName(dlg.FileName);
					try
					{
						GenerateFile(dlg.FileName);
						Analytics.Track("Export",
							new Dictionary<string, string> { { "includeVoiceActors", m_includeVoiceActors.ToString() } });
					}
					catch (Exception ex)
					{
						ErrorReport.ReportNonFatalExceptionWithMessage(ex, 
							string.Format(LocalizationManager.GetString("File.CouldNotExport", "Could not export data to {0}", "{0} is a file name."), dlg.FileName));
					}
				}
			}
		}

		private void GenerateFile(string path)
		{
			int blockNumber = 1;
			using (var stream = new StreamWriter(path, false, Encoding.UTF8))
			{
				foreach (var book in m_project.IncludedBooks)
				{
					string singleVoiceNarratorOverride = null;
					if (book.SingleVoice)
						singleVoiceNarratorOverride = CharacterVerseData.GetStandardCharacterId(book.BookId, CharacterVerseData.StandardCharacter.Narrator);
					foreach (var block in book.GetScriptBlocks(true))
					{
						if (m_includeVoiceActors)
						{
							VoiceActor.VoiceActor voiceActor = m_project.GetVoiceActorForCharacter(singleVoiceNarratorOverride ?? block.CharacterIdInScript);
							string voiceActorName = voiceActor != null ? voiceActor.Name : null;
							stream.WriteLine(GetExportLineForBlock(block, blockNumber++, book.BookId, voiceActorName ?? "", singleVoiceNarratorOverride, m_includeVoiceActors));
						}
						else
							stream.WriteLine(GetExportLineForBlock(block, blockNumber++, book.BookId, null, singleVoiceNarratorOverride, m_includeVoiceActors));
					}
				}
			}
		}

		internal static string GetExportLineForBlock(Block block, int blockNumber, string bookId, string voiceActor = null, string singleVoiceNarratorOverride = null, bool useCharacterIdInScript = true)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append(blockNumber);
			builder.Append(Separator);
			if (voiceActor != null)
			{
				builder.Append(voiceActor);
				builder.Append(Separator);
			}
			builder.Append(block.StyleTag);
			builder.Append(Separator);
			builder.Append(bookId);
			builder.Append(Separator);
			builder.Append(block.ChapterNumber);
			builder.Append(Separator);
			builder.Append(block.InitialStartVerseNumber);
			builder.Append(Separator);
			if (singleVoiceNarratorOverride != null)
				builder.Append(singleVoiceNarratorOverride);
			else
				builder.Append(useCharacterIdInScript ? block.CharacterIdInScript : block.CharacterId);
			builder.Append(Separator);
			builder.Append(block.Delivery);
			builder.Append(Separator);
			builder.Append(block.GetText(true));
			builder.Append(Separator);
			builder.Append(block.GetText(false).Length);
			return builder.ToString();
		}
	}
}
