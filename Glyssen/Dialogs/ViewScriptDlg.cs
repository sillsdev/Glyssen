using System;
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
			// add columns
			var dt = m_viewModel.GeneratePreviewTable();

			// grid settings
			m_dataGridView.DataSource = dt;
			m_dataGridView.RowHeadersWidth = 20;

			// shrink column widths
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
