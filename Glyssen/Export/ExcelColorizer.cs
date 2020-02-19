using System;
using System.Drawing;
using GlyssenEngine.Export;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Glyssen.Export
{
	class ExcelColorizer : IExcelColorizer
	{
		public void ApplyColor(ExcelRichText r, AnnotationColor color)
		{
			switch (color)
			{
				case AnnotationColor.Blue:
					r.Color = Color.Blue;
					break;
				case AnnotationColor.Red:
					r.Color = Color.Red;
					break;
				case AnnotationColor.Black:
					r.Color = Color.Black;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(color), color, null);
			}
		}

		public void SetCellHotTrackColor(ExcelRange sheetCell)
		{
			sheetCell.Style.Font.Color.SetColor(SystemColors.HotTrack);

		}
	}
}
