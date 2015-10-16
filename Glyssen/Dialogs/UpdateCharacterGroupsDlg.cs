using System.Windows.Forms;
using L10NSharp.UI;

namespace Glyssen.Dialogs
{
	public partial class UpdateCharacterGroupsDlg : Form
	{
		public enum SelectionType
		{
			AutoGenAndMaintain,
			AutoGen
		}

		public UpdateCharacterGroupsDlg()
		{
			InitializeComponent();

			HandleStringsLocalized();
			LocalizeItemDlg.StringsLocalized += HandleStringsLocalized;
		}

		public SelectionType SelectedOption
		{
			get { return m_radioMaintainActors.Checked ? SelectionType.AutoGenAndMaintain : SelectionType.AutoGen; }
		}

		private void HandleStringsLocalized()
		{
			m_lblHeading.Text = string.Format(m_lblHeading.Text, Program.kProduct);
		}
	}
}
