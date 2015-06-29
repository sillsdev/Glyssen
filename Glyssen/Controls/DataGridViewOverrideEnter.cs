using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SIL.Windows.Forms.Widgets.BetterGrid;

namespace Glyssen.Controls
{
	//DataGridView with Enter moving to right (instead of down)
	class DataGridViewOverrideEnter : BetterGrid
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

		public void MoveToNextField()
		{
			int nextColumn, nextRow;

			if (CurrentCell.ColumnIndex + 1 < ColumnCount)
			{
				nextColumn = CurrentCell.ColumnIndex + 1;
				nextRow = CurrentCell.RowIndex;
			}
			else
			{
				nextColumn = 0;
				nextRow = CurrentCell.RowIndex + 1;
			}

			CurrentCell = Rows[nextRow].Cells[nextColumn];
		}
	}
}
