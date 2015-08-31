using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Glyssen.Dialogs
{
	public partial class UpdateCharacterGroupsDlg : Form
	{
		public enum SelectionType
		{
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
					return SelectionType.AutoGen;
				return SelectionType.SplitCurrent;
			}
		}
	}
}
