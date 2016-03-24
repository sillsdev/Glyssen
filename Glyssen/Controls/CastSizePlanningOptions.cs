using System;
using System.Drawing;
using System.Windows.Forms;
using Glyssen.Dialogs;
using Glyssen.Utilities;

namespace Glyssen.Controls
{
	public partial class CastSizePlanningOptions : UserControl
	{
		public event EventHandler<CastSizeOptionChangedEventArgs> CastSizeOptionChanged;
		public event EventHandler<CastSizeValueChangedEventArgs> CastSizeCustomValueChanged;

		private CastSizePlanningViewModel m_viewModel;

		// This was a property, but changes to designer were making it get set to null
		// I tried setting Browsable(false), but that was causing an error which was
		// disallowing all changes to the designer
		public CastSizePlanningViewModel GetViewModel()
		{
			return m_viewModel;
		}

		public void SetViewModel(CastSizePlanningViewModel viewModel)
		{
			m_viewModel = viewModel;
			SetCastSizeOptionValues(m_viewModel);
			m_viewModel.CastSizeRowValuesChanged += m_viewModel_CastSizeRowValuesChanged;
		}

		void m_viewModel_CastSizeRowValuesChanged(object sender, CastSizeValueChangedEventArgs e)
		{
			SetRowValues(e.Row, e.RowValues);

			if (e.Row != CastSizeRow.MatchVoiceActorList)
				return;

			// if there are too few actors, make sure "Match Voice Actor List" is not selected
			var small = m_viewModel.GetCastSizeRowValues(CastSizeRow.Small);

			if ((e.Male < small.Male) ||
			    (e.Female < small.Female) ||
			    (e.Child < small.Child))
			{
				SelectedCastSizeRow = CastSizeRow.Recommended;
			}
			else
			{
				SelectedCastSizeRow = CastSizeRow.MatchVoiceActorList;
			}
		}

		public CastSizePlanningOptions()
		{
			InitializeComponent();
			SetLabelBackgroundColor();
		}

		private void SetLabelBackgroundColor()
		{
			m_lblCastSize.BackColor = SystemColors.ActiveBorder;
			m_lblMen.BackColor = SystemColors.ActiveBorder;
			m_lblWomen.BackColor = SystemColors.ActiveBorder;
			m_lblChildren.BackColor = SystemColors.ActiveBorder;
			m_lblTotal.BackColor = SystemColors.ActiveBorder;
		}

		private void m_tableLayout_CellPaint(object sender, TableLayoutCellPaintEventArgs e)
		{
			if (e.Row == 0)
			{
				var brush = new SolidBrush(SystemColors.ActiveBorder);
				e.Graphics.FillRectangle(brush, e.CellBounds);
			}

			var r = e.CellBounds;
			using (var pen = new Pen(glyssenColorPalette.GetColor(GlyssenColors.ForeColor), 0))
			{
				pen.Alignment = System.Drawing.Drawing2D.PenAlignment.Center;
				// define border style
				pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;

				// decrease border rectangle height/width by pen's width for last row/column cell
				if (e.Row == (m_tableLayout.RowCount - 1))
				{
					r.Height -= 1;
				}

				if (e.Column == (m_tableLayout.ColumnCount - 1))
				{
					r.Width -= 1;
				}

				// use graphics methods to draw cell's border
				e.Graphics.DrawRectangle(pen, r);
			}
		}

		private void m_tableLayout_Resize(object sender, EventArgs e)
		{
			// The control should be the same height as the table layout panel
			if (m_tableLayout.Height > 0)
				Height = m_tableLayout.Height;
		}

		public void SetRowValues(CastSizeRow row, CastSizeRowValues values)
		{
			if (row == CastSizeRow.NotSet)
				return;

			var rowIndex = (int)row;
			SetControlValue(m_tableLayout.GetControlFromPosition(2, rowIndex), values.Male);
			SetControlValue(m_tableLayout.GetControlFromPosition(3, rowIndex), values.Female);
			SetControlValue(m_tableLayout.GetControlFromPosition(4, rowIndex), values.Child);
			SetControlValue(m_tableLayout.GetControlFromPosition(5, rowIndex), values.Total);
		}

		public CastSizeRow SelectedCastSizeRow
		{
			get
			{
				if (m_rbSmall.Checked) return CastSizeRow.Small;
				if (m_rbRecommended.Checked) return CastSizeRow.Recommended;
				if (m_rbLarge.Checked) return CastSizeRow.Large;
				if (m_rbCustom.Checked) return CastSizeRow.Custom;
				if (m_rbMatchVoiceActorList.Checked) return CastSizeRow.MatchVoiceActorList;

				// default to Recommended
				return CastSizeRow.Recommended;
			}
			set
			{
				switch(value)
				{
					case CastSizeRow.Small:
						m_rbSmall.Checked = true;
						return;

					case CastSizeRow.Recommended:
						m_rbRecommended.Checked = true;
						return;

					case CastSizeRow.Large:
						m_rbLarge.Checked = true;
						return;

					case CastSizeRow.Custom:
						m_rbCustom.Checked = true;
						return;

					case CastSizeRow.MatchVoiceActorList:
						m_rbMatchVoiceActorList.Checked = true;
						return;
				}

				// default to Recommended
				m_rbRecommended.Checked = true;
			}
		}

		public void SetCastSizeOptionValues(CastSizePlanningViewModel model)
		{
			SetRowValues(CastSizeRow.Small, model.GetCastSizeRowValues(CastSizeRow.Small)); 
			SetRowValues(CastSizeRow.Recommended, model.GetCastSizeRowValues(CastSizeRow.Recommended));
			SetRowValues(CastSizeRow.Large, model.GetCastSizeRowValues(CastSizeRow.Large));
			SetRowValues(CastSizeRow.Custom, model.GetCastSizeRowValues(CastSizeRow.Custom));
			SetRowValues(CastSizeRow.MatchVoiceActorList, model.GetCastSizeRowValues(CastSizeRow.MatchVoiceActorList));

			// disable 'Match Voice Actor List' if there are no voice actors
			m_rbMatchVoiceActorList.Enabled = model.HasVoiceActors;
			if (m_rbMatchVoiceActorList.Checked && !m_rbMatchVoiceActorList.Enabled)
				SelectedCastSizeRow = CastSizeRow.Recommended;
		}

		private static void SetControlValue(Control ctrl, int value)
		{
			var down = ctrl as NumericUpDown;
			if (down != null)
				down.Value = value;
			else
				ctrl.Text = value.ToString();
		}

		private void OptionCheckedChanged(object sender, EventArgs e)
		{
			var enable = SelectedCastSizeRow == CastSizeRow.Custom;
			const int rowIndex = (int)CastSizeRow.Custom;

			m_tableLayout.GetControlFromPosition(2, rowIndex).Enabled = enable;
			m_tableLayout.GetControlFromPosition(3, rowIndex).Enabled = enable;
			m_tableLayout.GetControlFromPosition(4, rowIndex).Enabled = enable;

			if (CastSizeOptionChanged != null)
				CastSizeOptionChanged(sender, new CastSizeOptionChangedEventArgs(SelectedCastSizeRow));
		}

		private void CastSizeValueChanged(object sender, EventArgs e)
		{
			const int rowIndex = (int)CastSizeRow.Custom;

			var male = (int)((NumericUpDown)m_tableLayout.GetControlFromPosition(2, rowIndex)).Value;
			var female = (int)((NumericUpDown)m_tableLayout.GetControlFromPosition(3, rowIndex)).Value;
			var child = (int)((NumericUpDown)m_tableLayout.GetControlFromPosition(4, rowIndex)).Value;

			SetControlValue(m_tableLayout.GetControlFromPosition(5, rowIndex), male + female + child);

			if (CastSizeCustomValueChanged != null)
				CastSizeCustomValueChanged(sender, new CastSizeValueChangedEventArgs(CastSizeRow.Custom, male, female, child));
		}
	}
}
