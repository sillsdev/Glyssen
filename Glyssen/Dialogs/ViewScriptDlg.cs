using System;
using System.Windows.Forms;
using Glyssen.Properties;

namespace Glyssen.Dialogs
{
	public partial class ViewScriptDlg : Form
	{
		private readonly ProjectExporter m_viewModel;

		public ViewScriptDlg(ProjectExporter viewModel)
		{
			InitializeComponent();
			Icon = Resources.glyssenIcon;
			m_viewModel = viewModel;
		}

		private void m_exportMenuItem_Click(object sender, EventArgs e)
		{
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

		private void ViewScriptDlg_Shown(object sender, EventArgs e)
		{
			try
			{
				// these lines are to improve the appearance while the data is loading
				Cursor = Cursors.WaitCursor;
				m_dataGridView.SuspendLayout();
				m_dataGridView.ScrollBars = ScrollBars.None;
				Refresh();

				LoadScript();
			}
			finally
			{
				// now resume normal UI behavior
				m_dataGridView.ResumeLayout();
				m_dataGridView.ScrollBars = ScrollBars.Both;
				m_lblLoading.Visible = false;
				Cursor = Cursors.Default;
			}
		}
	}
}
