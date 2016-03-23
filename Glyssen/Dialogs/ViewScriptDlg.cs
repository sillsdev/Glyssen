using System;
using System.Data;
using System.IO;
using System.Windows.Forms;
using Glyssen.Properties;

namespace Glyssen.Dialogs
{
	public partial class ViewScriptDlg : Form
	{
		private readonly ExportViewModel m_viewModel;

		public ViewScriptDlg(ExportViewModel viewModel)
		{
			InitializeComponent();
			Icon = Resources.glyssenIcon;
			m_viewModel = viewModel;
		}

		private void m_exportMenuItem_Click(object sender, EventArgs e)
		{
			var export = m_viewModel.IsOkToExport();
			if (!export) return;
			using (var dlg = new ExportDlg(m_viewModel))
			{
				dlg.ShowDialog(this);
				if (dlg.DialogResult == DialogResult.OK)
				{
					DialogResult = DialogResult.OK;
				}
			}
		}

		private void m_btnClose_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
		}

		private void ViewScriptDlg_Load(object sender, EventArgs e)
		{
			if (Owner is MainForm)
				MainForm.SetChildFormLocation(this);
			else
				CenterToParent();

			LoadScript();
		}

		private void LoadScript()
		{
			string[] lines;

			using (var tempFile = SIL.IO.TempFile.WithExtension(ProjectExporter.kTabDelimitedFileExtension))
			{
				m_viewModel.SelectedFileType = ExportFileType.TabSeparated;
				m_viewModel.ExportNow(tempFile.Path, false, false, false);
				lines = File.ReadAllLines(m_viewModel.FullFileName);
			}

			if (lines.Length < 2)
				return;

			// add columns
			var dt = new DataTable();
			var line = lines[0].Split('\t');
			foreach (var val in line)
			{
				dt.Columns.Add(val);
			}

			// add rows
			for (var i = 1; i < lines.Length; i++)
			{
				// ReSharper disable once CoVariantArrayConversion
				dt.Rows.Add(lines[i].Split('\t'));
			}

			// grid settings
			m_dataGridView.DataSource = dt;
			m_dataGridView.RowHeadersWidth = 20;

			// column widths
			m_dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
			foreach (DataGridViewColumn col in m_dataGridView.Columns)
			{
				var colWidth = col.Width;

				if (colWidth > 200)
					colWidth = 200;

				col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
				col.Width = colWidth;
			}
		}
	}
}
