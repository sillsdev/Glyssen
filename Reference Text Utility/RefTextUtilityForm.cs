using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Glyssen.RefTextDevUtilities;

namespace Glyssen.ReferenceTextUtility
{
	public partial class RefTextUtilityForm : Form
	{
		private OutputForm m_outputForm;
		private ReferenceTextData m_data;
		private Stack<RefTextDevUtilities.ReferenceTextUtility.Mode> m_passes = new Stack<RefTextDevUtilities.ReferenceTextUtility.Mode>();
		private static string s_distFilesDir = Path.GetFullPath(RefTextDevUtilities.ReferenceTextUtility.kOutputDirDistfiles);

		private BackgroundWorker ExcelSpreadsheetLoader { get; }
		private BackgroundWorker BackgroundProcessor { get; }
		private ReferenceTextData Data
		{
			get { return m_data; }
			set
			{
				m_data = value;
				// TODO: Load data grid view
				foreach (var languageInfo in Data.LanguagesToProcess)
				{
					int iOption = 0;
					ReferenceTextType refTextType;
					if (Enum.TryParse(languageInfo.Name, out refTextType))
					{
						if (languageInfo.IsEnglish)
							languageInfo.OutputFolder = Path.Combine(s_distFilesDir, RefTextDevUtilities.ReferenceTextUtility.kTempFolderPrefix + languageInfo.Name);
						else
						{
							iOption = 1;
							languageInfo.OutputFolder = Path.Combine(s_distFilesDir, languageInfo.Name);
						}
					}
					else
					{
						languageInfo.OutputFolder = Path.Combine(RefTextDevUtilities.ReferenceTextUtility.ProprietaryRefTextTempBaseFolder, languageInfo.Name);
					}
					int iRow = m_dataGridRefTexts.Rows.Add(languageInfo.Name, colAction.Items[iOption], languageInfo.OutputFolder);
					if (iOption != 0)
						m_dataGridRefTexts.Rows[iRow].Cells[colDestination.Index].ReadOnly = true;

					m_btnOk.Enabled = true;
				}
				m_lblLoading.Visible = false;
			}
		}

		public RefTextUtilityForm()
		{
			InitializeComponent();

			ExcelSpreadsheetLoader = new BackgroundWorker { WorkerReportsProgress = false };
			ExcelSpreadsheetLoader.DoWork += ExcelLoader_DoWork;
			ExcelSpreadsheetLoader.RunWorkerCompleted += ExcelLoader_RunWorkerCompleted;

			BackgroundProcessor = new BackgroundWorker { WorkerReportsProgress = false };
			BackgroundProcessor.DoWork += (s, args) => { DoNextPass(); };
			BackgroundProcessor.RunWorkerCompleted += (s, args) =>
			{
				if (m_passes.Count == 0)
					m_btnOk.Enabled = true;
				else
					BackgroundProcessor.RunWorkerAsync();
			};

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
				if (openDlg.ShowDialog() == DialogResult.OK)
				{
					lock (this)
					{
						if (ExcelSpreadsheetLoader.IsBusy)
							ExcelSpreadsheetLoader.CancelAsync();
					}
					m_lblSpreadsheetFilePath.Text = openDlg.FileName;
				}
			}
		}

		private void LoadExcelSpreadsheet(string path)
		{
			m_btnOk.Enabled = false;
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
				RefTextDevUtilities.ReferenceTextUtility.ProcessReferenceTextData(mode, Data);
			}
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
				if (e.Error != null)
				{
					m_lblLoading.Visible = false;
					MessageBox.Show(this, $"The file {m_lblSpreadsheetFilePath.Text} could not be read.", "Invalid Excel Spreadsheet", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}

				if (!ExcelSpreadsheetLoader.CancellationPending)
					Data = e.Result as ReferenceTextData;
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
			m_btnOk.Enabled = false;
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
					var createActions = new[] { "Create in Temp Folder", "Create/Overwrite" };
					return m_dataGridRefTexts.Rows.OfType<DataGridViewRow>().
						Where(r => createActions.Contains((string)r.Cells[colAction.Index].Value)).Select(r => r.Cells[colName.Index].Value as string).ToArray();
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
				switch ((string)m_dataGridRefTexts.Rows[e.RowIndex].Cells[e.ColumnIndex].Value)
				{
					case "Create in Temp Folder":
						if (String.IsNullOrWhiteSpace((string)m_dataGridRefTexts.Rows[e.RowIndex].Cells[colDestination.Index].Value) ||
							languageInfo.OutputFolder == Path.Combine(s_distFilesDir, languageInfo.Name))
						{
							m_dataGridRefTexts.Rows[e.RowIndex].Cells[colDestination.Index].Value = languageInfo.OutputFolder =
								Path.Combine(s_distFilesDir, RefTextDevUtilities.ReferenceTextUtility.kTempFolderPrefix + languageInfo.Name);
						}
						m_dataGridRefTexts.Rows[e.RowIndex].Cells[colDestination.Index].ReadOnly = false;
						return;
					case "Create/Overwrite":
						m_dataGridRefTexts.Rows[e.RowIndex].Cells[colDestination.Index].Value = languageInfo.OutputFolder =
							Path.Combine(s_distFilesDir, languageInfo.Name);
						break;
					case "Compare to Current":
					case "Skip":
						m_dataGridRefTexts.Rows[e.RowIndex].Cells[colDestination.Index].Value = "";
						break;
					default:
						throw new Exception("Unexpected Action. This can't happen!");
				}
				m_dataGridRefTexts.Rows[e.RowIndex].Cells[colDestination.Index].ReadOnly = true;
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
					m_dataGridRefTexts.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.ForeColor = m_dataGridRefTexts.DefaultCellStyle.ForeColor;
					m_btnOk.Enabled = m_dataGridRefTexts.Rows.OfType<DataGridViewRow>().All(r =>
					{
						var destinationCellForeColor = r.Cells[e.ColumnIndex].Style.ForeColor;
						return destinationCellForeColor == default(Color) || destinationCellForeColor == m_dataGridRefTexts.DefaultCellStyle.ForeColor;
					});
					languageInfo.OutputFolder = newValue;
				}
			}
		}
	}
}
