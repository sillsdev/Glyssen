using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace GlyssenEngine.Export
{
	public enum AnnotationColor
	{
		Blue,
		Red,
		Black,
	}

	public interface IExcelColorizer
	{
		void ApplyColor(ExcelRichText r, AnnotationColor color);
		void SetCellHotTrackColor(ExcelRange sheetCell);
	}
}
