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
			if (baseCell == null)
				baseCell = CurrentCell;

			int nextColumn, nextRow;

			if (baseCell.ColumnIndex + 1 < ColumnCount)
			{
				nextColumn = baseCell.ColumnIndex + 1;
				nextRow = baseCell.RowIndex;
			}
			else if (baseCell.RowIndex + 1 < RowCount)
			{
				nextColumn = 0;
				nextRow = baseCell.RowIndex + 1;
			}
			else
			{
				return;
			}

			var nextCell = Rows[nextRow].Cells[nextColumn];
			if (nextCell.Visible)
				CurrentCell = Rows[nextRow].Cells[nextColumn];
			else
				MoveToNextField(nextCell);
		}
	}
}
