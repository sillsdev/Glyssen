using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ProtoScript.Dialogs
{
	public partial class ProjectSettingsDialog : Form
	{
		private Project m_project;

		internal ProjectSettingsDialog(Project project)
		{
			InitializeComponent();

			m_project = project;

			m_comboQuoteMarks.DrawMode = DrawMode.OwnerDrawFixed;
			m_comboQuoteMarks.DrawItem += comboQuoteMarks_DrawItem;

			SetupQuoteMarksComboBox();
		}

		private void SetupQuoteMarksComboBox()
		{
			m_comboQuoteMarks.Items.AddRange(QuoteSystem.AllSystems.ToArray());
		}

		private void comboQuoteMarks_DrawItem(object sender, DrawItemEventArgs e)
		{
			e.DrawBackground();
			if (e.Index != ((ComboBox)sender).SelectedIndex)
				e.Graphics.DrawLine(Pens.Black, new Point(e.Bounds.Left, e.Bounds.Bottom - 1), new Point(e.Bounds.Right, e.Bounds.Bottom - 1));
			string text = e.Index > -1 ? m_comboQuoteMarks.Items[e.Index].ToString() : "";
			TextRenderer.DrawText(e.Graphics, text, m_comboQuoteMarks.Font, e.Bounds, m_comboQuoteMarks.ForeColor, TextFormatFlags.Left);
			e.DrawFocusRectangle();
		}

		private void m_btnOk_Click(object sender, EventArgs e)
		{
			m_project.QuoteSystem = (QuoteSystem)m_comboQuoteMarks.SelectedItem;
			Close();
		}

		private void m_btnCancel_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
