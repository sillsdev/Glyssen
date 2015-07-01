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
	public partial class VoiceActorAssignmentDlg : Form
	{
		private Project m_project;

		public VoiceActorAssignmentDlg(Project project)
		{
			InitializeComponent();

			m_project = project;
		}
	}
}
