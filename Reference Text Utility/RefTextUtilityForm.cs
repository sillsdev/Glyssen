using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Glyssen.Bundle;
using Glyssen.ReferenceTextUtility.Properties;
using Glyssen.RefTextDevUtilities;
using Glyssen.Shared;
using Glyssen.Shared.Bundle;
using SIL.DblBundle.Text;
using SIL.Xml;

namespace Glyssen.ReferenceTextUtility
{
	public partial class RefTextUtilityForm : Form
	{
		private OutputForm m_outputForm;
		private ReferenceTextData m_data;
		private bool m_manualSettingsChangesMade = false;
		private Stack<RefTextDevUtilities.ReferenceTextUtility.Mode> m_passes = new Stack<RefTextDevUtilities.ReferenceTextUtility.Mode>();
		private readonly string m_distFilesDir;
		private static readonly string[] s_createActions = { "Create in Temp Folder", "Create/Overwrite" };

		private BackgroundWorker ExcelSpreadsheetLoader { get; }
		private BackgroundWorker BackgroundProcessor { get; }
		private ReferenceTextData Data
		{
			get { return m_data; }
			set
			{
				m_data = value;
				if (m_data == null)
					return;
				foreach (var languageInfo in Data.LanguagesToProcess)
				{
					int iOption = 0;
					ReferenceTextType refTextType;
					// m_distFilesDir will be null except on a dev machine
					if (m_distFilesDir != null && Enum.TryParse(languageInfo.Name, out refTextType))
					{
						if (languageInfo.IsEnglish)
							languageInfo.OutputFolder = Path.Combine(m_distFilesDir, RefTextDevUtilities.ReferenceTextUtility.kTempFolderPrefix + languageInfo.Name);
						else
						{
							iOption = 1;
							languageInfo.OutputFolder = Path.Combine(m_distFilesDir, languageInfo.Name);
						}
					}
					else
					{
						languageInfo.OutputFolder = Path.Combine(RefTextDevUtilities.ReferenceTextUtility.ProprietaryRefTextTempBaseFolder, languageInfo.Name);
					}

					string heSaidText = null;
					string isoCode = null;
					try
					{
						var existingRefTextId = RefTextDevUtilities.ReferenceTextUtility.GetReferenceTextIdFromString(languageInfo.Name);
						if (existingRefTextId != null && !existingRefTextId.Missing)
						{
							var language = existingRefTextId.Metadata?.Language;
							if (language != null)
							{
								heSaidText = language.HeSaidText;
								isoCode = language.Iso;
							}
						}
					}
					catch (Exception)
					{
					}
					int iRow = m_dataGridRefTexts.Rows.Add(languageInfo.Name, colAction.Items[iOption], languageInfo.OutputFolder, heSaidText, isoCode);
					if (iOption != 0)
						m_dataGridRefTexts.Rows[iRow].Cells[colDestination.Index].ReadOnly = true;
					if (!String.IsNullOrEmpty(heSaidText))
						m_dataGridRefTexts.Rows[iRow].Cells[colHeSaidText.Index].ReadOnly = true;
					if (!String.IsNullOrEmpty(isoCode))
						m_dataGridRefTexts.Rows[iRow].Cells[colIsoCode.Index].ReadOnly = true;

					m_btnOk.Enabled = m_btnSkipAll.Enabled = true;
					m_manualSettingsChangesMade = false;
				}
				m_lblLoading.Visible = false;
			}
		}

		public RefTextUtilityForm()
		{
			InitializeComponent();

			m_distFilesDir = Path.GetFullPath(RefTextDevUtilities.ReferenceTextUtility.kOutputDirDistfiles);
			if (!Directory.Exists(m_distFilesDir))
				m_distFilesDir = null;

			ExcelSpreadsheetLoader = new BackgroundWorker { WorkerReportsProgress = false };
			ExcelSpreadsheetLoader.DoWork += ExcelLoader_DoWork;
			ExcelSpreadsheetLoader.RunWorkerCompleted += ExcelLoader_RunWorkerCompleted;

			BackgroundProcessor = new BackgroundWorker { WorkerReportsProgress = false };
			BackgroundProcessor.DoWork += (s, args) => { DoNextPass(); };
			BackgroundProcessor.RunWorkerCompleted += OnBackgroundProcessorWorkCompleted;

			RefTextDevUtilities.ReferenceTextUtility.OnMessageRaised += HandleMessageRaised;

			if (File.Exists(RefTextDevUtilities.ReferenceTextUtility.kDirectorGuideInput))
			{
				m_lblSpreadsheetFilePath.Text = Path.GetFullPath(RefTextDevUtilities.ReferenceTextUtility.kDirectorGuideInput);
			}
			else
				m_lblSpreadsheetFilePath.Text = "";
		}

		private void m_btnSelectSpreadsheetFile_Click(object sender, EventArgs e)
		{
			using (var openDlg = new OpenFileDialog {CheckFileExists = true})
			{
				openDlg.Filter = string.Format("Excel files ({0})|{0}", "*" + Constants.kExcelFileExtension);
				openDlg.CheckFileExists = true;
				if (openDlg.ShowDialog() == DialogResult.OK)
				{
					lock (this)
					{
						if (ExcelSpreadsheetLoader.IsBusy)
							ExcelSpreadsheetLoader.CancelAsync();
					}
					if (File.Exists(openDlg.FileName))
					{
						if (m_lblSpreadsheetFilePath.Text == openDlg.FileName)
							LoadExcelSpreadsheet(m_lblSpreadsheetFilePath.Text); // Force attempted reload
						else
							m_lblSpreadsheetFilePath.Text = openDlg.FileName;
					}
				}
			}
		}

		private void LoadExcelSpreadsheet(string path)
		{
			m_btnOk.Enabled = m_btnSkipAll.Enabled = false;
			m_dataGridRefTexts.RowCount = 0;
			m_lblLoading.Visible = true;

			object[] parameters = {path};
			lock (this)
			{
				ExcelSpreadsheetLoader.RunWorkerAsync(parameters);
			}
		}

		private void DoNextPass()
		{
			var mode = m_passes.Pop();

			var languagesToDiff = GetLanguages(mode);
			if (languagesToDiff.Any())
			{
				Data.FilterBy(languagesToDiff);
				RefTextDevUtilities.ReferenceTextUtility.ProcessReferenceTextData(mode, Data, null);
			}
		}

		private void OnBackgroundProcessorWorkCompleted(object s, RunWorkerCompletedEventArgs args)
		{
			if (m_passes.Count == 0)
			{
				foreach (var newRefTextRow in m_dataGridRefTexts.Rows.OfType<DataGridViewRow>().Where(r => s_createActions.Contains((string)r.Cells[colAction.Index].Value) && !r.Cells[colHeSaidText.Index].ReadOnly && !r.Cells[colIsoCode.Index].ReadOnly))
				{
					// Generate a new metadata file with the above info
					var languageName = (string)newRefTextRow.Cells[colName.Index].Value;
					var folder = Data.GetLanguageInfo(languageName).OutputFolder;
					var projectPath = Path.Combine(folder, languageName + Constants.kProjectFileExtension);
					if (File.Exists(projectPath))
						HandleMessageRaised($"File {projectPath} already exists! Skipping. Please verify contents.", true);
					else
					{
						var metadata = XmlSerializationHelper.DeserializeFromString<GlyssenDblTextMetadata>(Resources.refTextMetadata);
						metadata.Language = new GlyssenDblMetadataLanguage
						{
							Name = languageName,
							HeSaidText = newRefTextRow.Cells[colHeSaidText.Index].Value as string,
							Iso = newRefTextRow.Cells[colIsoCode.Index].Value as string
						};
						metadata.AvailableBooks = new List<Book>();
						ProjectUtilities.ForEachBookFileInProject(folder,
							(bookId, fileName) => metadata.AvailableBooks.Add(new Book { Code = bookId, IncludeInScript = true}));
						metadata.LastModified = DateTime.Now;

						Exception error;
						XmlSerializationHelper.SerializeToFile(projectPath, metadata, out error);
						if (error != null)
							HandleMessageRaised(error.Message, true);
					}
				}
				m_btnOk.Enabled = m_btnSkipAll.Enabled = true;
			}
			else
				BackgroundProcessor.RunWorkerAsync();
		}

		private void ExcelLoader_DoWork(object sender, DoWorkEventArgs e)
		{
			var parameters = (object[])e.Argument;
			var path = (string)parameters[0];

			e.Result = RefTextDevUtilities.ReferenceTextUtility.GetDataFromExcelFile(path);
		}

		private void ExcelLoader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			lock (this)
			{
				m_lblLoading.Visible = false;
				if (e.Error != null)
				{
					MessageBox.Show(this, $"The file {m_lblSpreadsheetFilePath.Text} could not be read.", "Invalid Excel Spreadsheet", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return;
				}

				if (!ExcelSpreadsheetLoader.CancellationPending)
				{
					Data = e.Result as ReferenceTextData;
					if (Data == null)
						MessageBox.Show(this, $"No error was reported, but no data was loaded from file {m_lblSpreadsheetFilePath.Text}.", "Something bad happened", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					colAction.MinimumWidth = colAction.Width;
				}
			}
		}

		private void m_lblSpreadsheetFilePath_TextChanged(object sender, EventArgs e)
		{
			if (File.Exists(m_lblSpreadsheetFilePath.Text))
			{
				LoadExcelSpreadsheet(m_lblSpreadsheetFilePath.Text);
			}
		}

		protected override void OnFormClosed(FormClosedEventArgs e)
		{
			lock (this)
			{
				RefTextDevUtilities.ReferenceTextUtility.OnMessageRaised -= HandleMessageRaised;

				if (m_outputForm != null)
				{
					m_outputForm.Close();
					m_outputForm.Dispose();
					m_outputForm = null;
				}

				base.OnFormClosed(e);
			}
		}

		private void m_btnProcess_Click(object sender, EventArgs e)
		{
			m_btnOk.Enabled = m_btnSkipAll.Enabled = false;
			lock (this)
			{
				if (m_outputForm == null || m_outputForm.IsDisposed)
				{
					m_outputForm = new OutputForm();
					m_outputForm.Show(this);
				}
				else
				{
					m_outputForm.Clear();
				}
			}

			RefTextDevUtilities.ReferenceTextUtility.DifferencesToIgnore = RefTextDevUtilities.ReferenceTextUtility.Ignore.Nothing;

			if (m_chkPunctuation.Checked)
				RefTextDevUtilities.ReferenceTextUtility.DifferencesToIgnore = RefTextDevUtilities.ReferenceTextUtility.Ignore.Punctuation;
			else if (m_chkQuoteMarkDifferences.Checked)
				RefTextDevUtilities.ReferenceTextUtility.DifferencesToIgnore |= RefTextDevUtilities.ReferenceTextUtility.Ignore.QuotationMarkDifferences;
			if (m_chkWhitespace.Checked)
				RefTextDevUtilities.ReferenceTextUtility.DifferencesToIgnore |= RefTextDevUtilities.ReferenceTextUtility.Ignore.WhitespaceDifferences;
			if (m_chkSymbols.Checked)
				RefTextDevUtilities.ReferenceTextUtility.DifferencesToIgnore |= RefTextDevUtilities.ReferenceTextUtility.Ignore.Symbols;

			m_passes.Clear();
			m_passes.Push(RefTextDevUtilities.ReferenceTextUtility.Mode.Generate);
			m_passes.Push(RefTextDevUtilities.ReferenceTextUtility.Mode.FindDifferencesBetweenCurrentVersionAndNewText);
			BackgroundProcessor.RunWorkerAsync();
		}

		private string[] GetLanguages(RefTextDevUtilities.ReferenceTextUtility.Mode mode)
		{
			switch (mode)
			{
				case RefTextDevUtilities.ReferenceTextUtility.Mode.FindDifferencesBetweenCurrentVersionAndNewText:
					return m_dataGridRefTexts.Rows.OfType<DataGridViewRow>().
					Where(r => (string)r.Cells[colAction.Index].Value == "Compare to Current").Select(r => r.Cells[colName.Index].Value as string).ToArray();
				case RefTextDevUtilities.ReferenceTextUtility.Mode.Generate:
					return m_dataGridRefTexts.Rows.OfType<DataGridViewRow>().
						Where(r => s_createActions.Contains((string)r.Cells[colAction.Index].Value)).Select(r => r.Cells[colName.Index].Value as string).ToArray();
				default:
					throw new InvalidOperationException("Unexpected mode");
			}
		}

		private void HandleMessageRaised(string message, bool error)
		{
			lock (this)
				m_outputForm?.DisplayMessage(message, error);
		}

		private void m_dataGridRefTexts_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex < 0)
				return;
			if (e.ColumnIndex == colName.Index)
				throw new Exception("The Name column is read-only. This can't happen!");

			var languageInfo = Data.GetLanguageInfo((string)m_dataGridRefTexts.Rows[e.RowIndex].Cells[colName.Index].Value);

			if (e.ColumnIndex == colAction.Index)
			{
				ReferenceTextType refTextType;
				var isFactoryRefText = Enum.TryParse(languageInfo.Name, out refTextType);
				string defaultCreateDestination;
				if (isFactoryRefText && m_distFilesDir != null)
					defaultCreateDestination = Path.Combine(m_distFilesDir, languageInfo.Name);
				else
					defaultCreateDestination = Path.Combine(GlyssenInfo.BaseDataFolder, Constants.kLocalReferenceTextDirectoryName, languageInfo.Name);
				switch ((string)m_dataGridRefTexts.Rows[e.RowIndex].Cells[e.ColumnIndex].Value)
				{
					case "Create in Temp Folder":
						if (String.IsNullOrWhiteSpace(m_dataGridRefTexts.Rows[e.RowIndex].Cells[colDestination.Index].Value as string) ||
							languageInfo.OutputFolder == defaultCreateDestination)
						{
							if (isFactoryRefText && m_distFilesDir != null)
								languageInfo.OutputFolder = Path.Combine(m_distFilesDir, RefTextDevUtilities.ReferenceTextUtility.kTempFolderPrefix + languageInfo.Name);
							else
								languageInfo.OutputFolder = Path.Combine(RefTextDevUtilities.ReferenceTextUtility.ProprietaryRefTextTempBaseFolder, languageInfo.Name);
							m_dataGridRefTexts.Rows[e.RowIndex].Cells[colDestination.Index].Value = languageInfo.OutputFolder;
						}
						m_dataGridRefTexts.Rows[e.RowIndex].Cells[colDestination.Index].ReadOnly = false;
						return;
					case "Create/Overwrite":
						m_dataGridRefTexts.Rows[e.RowIndex].Cells[colDestination.Index].Value = languageInfo.OutputFolder = defaultCreateDestination;
						SkipAllOtherLanguagesIfThisRowIsEnglish(e.RowIndex, languageInfo);
						break;
					case "Compare to Current":
						SkipAllOtherLanguagesIfThisRowIsEnglish(e.RowIndex, languageInfo);
						goto case "Skip";
					case "Skip":
						m_dataGridRefTexts.Rows[e.RowIndex].Cells[colDestination.Index].Value = languageInfo.OutputFolder = "";
						SetStateForValidDestination(e.RowIndex);
						break;
					default:
						throw new Exception("Unexpected Action. This can't happen!");
				}
				m_dataGridRefTexts.Rows[e.RowIndex].Cells[colDestination.Index].ReadOnly = true;
				m_manualSettingsChangesMade = true;
				return;
			}

			if (e.ColumnIndex == colDestination.Index)
			{
				var newValue = (string)m_dataGridRefTexts.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
				bool invalidDestination = false;
				if ((string)m_dataGridRefTexts.Rows[e.RowIndex].Cells[colAction.Index].Value == "Create in Temp Folder")
				{
					try
					{
						var finfo = new FileInfo(newValue);
						if (!Path.IsPathRooted(newValue))
						{
							if (!Directory.Exists(finfo.DirectoryName))
								invalidDestination = true;
						}
					}
					catch (Exception)
					{
						invalidDestination = true;
					}
				}
				if (invalidDestination)
				{
					m_dataGridRefTexts.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.ForeColor = Color.Red;
					m_btnOk.Enabled = false;
				}
				else
				{
					SetStateForValidDestination(e.RowIndex);
					languageInfo.OutputFolder = newValue;
				}
			}
		}

		private void SetStateForValidDestination(int iRow)
		{
			m_dataGridRefTexts.Rows[iRow].Cells[colDestination.Index].Style.ForeColor = m_dataGridRefTexts.DefaultCellStyle.ForeColor;
			m_btnOk.Enabled = m_dataGridRefTexts.Rows.OfType<DataGridViewRow>().All(r =>
				{
					var destinationCellForeColor = r.Cells[colDestination.Index].Style.ForeColor;
					var destValue = r.Cells[colDestination.Index].Value as string;
					var action = r.Cells[colAction.Index].Value as string;
					return (!String.IsNullOrWhiteSpace(destValue) || action == "Compare to Current" || action == "Skip") &&
						(destinationCellForeColor == default(Color) || destinationCellForeColor == m_dataGridRefTexts.DefaultCellStyle.ForeColor);
				}) &&
				m_dataGridRefTexts.Rows.OfType<DataGridViewRow>().Any(r => r.Cells[colAction.Index].Value as string != "Skip");
		}

		private void SkipAllOtherLanguagesIfThisRowIsEnglish(int indexOfRowBeingSet, ReferenceTextLanguageInfo languageInfo)
		{
			if (languageInfo.IsEnglish)
				SetRowsToSkip(i => indexOfRowBeingSet != i);
		}

		private void SetRowsToSkip(Func<int, bool> iff = null)
		{
			for (var i = 0; i < m_dataGridRefTexts.RowCount; i++)
			{
				if (iff?.Invoke(i) ?? true)
					m_dataGridRefTexts.Rows[i].Cells[colAction.Index].Value = "Skip";
			}
		}

		private void m_btnCancel_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void m_chkPunctuation_CheckedChanged(object sender, EventArgs e)
		{
			if (m_chkPunctuation.Checked)
			{
				m_chkQuoteMarkDifferences.Checked = true;
				m_chkQuoteMarkDifferences.Enabled = false;
			}
			else
				m_chkQuoteMarkDifferences.Enabled = true;
		}

		private void m_btnSkipAll_Click(object sender, EventArgs e)
		{
			SetRowsToSkip();
		}
	}
}
