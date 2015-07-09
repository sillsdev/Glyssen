using System.Windows.Forms;
using SIL.Windows.Forms.Widgets.BetterGrid;

namespace Glyssen.Controls
{
	/// <summary>
	/// DataGridView with Enter moving to right (instead of down)
	/// </summary>
	public class DataGridViewOverrideEnter : BetterGrid
	{
		public DataGridViewOverrideEnter()
		{
			AllowUserToAddRows = true;
			MultiSelect = true;
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == Keys.Enter)
			{
				MoveToNextField();
				return true;
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}

		public void MoveToNextField(DataGridViewCell baseCell = null)
		{
			if (Columns.GetLastColumn(DataGridViewElementStates.Visible, DataGridViewElementStates.None) == CurrentCell.OwningColumn &&
			    CurrentRow != null &&
			    Rows.GetLastRow(DataGridViewElementStates.Visible) == CurrentRow.Index)
				return;
			SendKeys.Send("{TAB}");
		}
	}
}
