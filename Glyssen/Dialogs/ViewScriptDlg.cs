using System;
using System.Windows.Forms;
using Glyssen.Shared;
using Glyssen.Utilities;
using GlyssenEngine;
using GlyssenEngine.Export;
using L10NSharp.TMXUtils;
using L10NSharp.UI;
using SIL;

namespace Glyssen.Dialogs
{
	public partial class ViewScriptDlg : FormWithPersistedSettings
	{
		private readonly ProjectExporter m_viewModel;

		public ViewScriptDlg(ProjectExporter viewModel)
		{
			InitializeComponent();
			m_viewModel = viewModel;

			HandleStringsLocalized();
			LocalizeItemDlg<TMXDocument>.StringsLocalized += HandleStringsLocalized;
		}

		private void HandleStringsLocalized()
		{
			Text = string.Format(Text, m_viewModel.Project.Name);
			m_exportToHearThisToolStripMenuItem.Text = string.Format(m_exportToHearThisToolStripMenuItem.Text,
				Constants.kHearThisProductName);
		}

		private void m_exportToSpreadsheetMenuItem_Click(object sender, EventArgs e)
		{
			using (var dlg = new ExportDlg(m_viewModel))
			{
				MainForm.LogDialogDisplay(dlg);
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
			if (!m_viewModel.IncludeVoiceActors)
				m_dataGridView.Columns[(int)ExportColumn.Actor].Visible = false;
			if (Localizer.UILanguageId == "en")
				m_dataGridView.Columns[(int)ExportColumn.CharacterIdLocalized].Visible = false;
			if (!m_viewModel.Project.ReferenceText.HasSecondaryReferenceText)
				m_dataGridView.Columns[(int)ExportColumn.AdditionalReferenceText].Visible = false;
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

		private void m_exportToHearThisToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using (var dlg = new ExportToRecordingToolDlg(m_viewModel))
			{
				dlg.ShowDialog(this);
			}
		}
	}
}
