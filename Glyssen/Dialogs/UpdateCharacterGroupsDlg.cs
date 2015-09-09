using System;
using System.Windows.Forms;

namespace Glyssen.Dialogs
{
	public partial class UpdateCharacterGroupsDlg : Form
	{
		public enum SelectionType
		{
			AutoGenAndMaintain,
			AutoGen,
			SplitCurrent
		}

		public UpdateCharacterGroupsDlg()
		{
			InitializeComponent();
		}

		public SelectionType SelectedOption
		{
			get
			{
				if (m_radioGenCharGrps.Checked)
					return m_chkMaintainAssignments.Checked ? SelectionType.AutoGenAndMaintain : SelectionType.AutoGen;

				return SelectionType.SplitCurrent;
			}
		}

		private void m_radioGenCharGrps_CheckedChanged(object sender, EventArgs e)
		{
			m_chkMaintainAssignments.Enabled = m_radioGenCharGrps.Checked;
		}
	}
}
