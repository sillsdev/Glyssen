using System;
using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Excel;
using Application = Microsoft.Office.Interop.Excel.Application;

namespace Glyssen.Character
{
	public class CharacterGroupTemplateExcelFile : ICharacterGroupSource
	{
		private const int CharacterIdColumn = 4;
		private const int SerialNumberColumn = 1;
		private const int ColumnsBeforeGroupNumbers = 4;
		private const int FirstRowWithData = 2;

		private readonly string m_filePath;

		public CharacterGroupTemplateExcelFile(string path)
		{
			m_filePath = path;
		}

		public CharacterGroupTemplate GetTemplate(int numberOfActors)
		{
			if (numberOfActors < 1 || numberOfActors > 28)
				throw new ArgumentException("Number of readers must between 1 and 28, inclusive.", "numberOfActors");

			var xlApp = new Application();
			Workbook xlWorkBook = xlApp.Workbooks.Open(m_filePath, 0, true);
			Worksheet xlWorkSheet = (Worksheet)xlWorkBook.Worksheets.Item[1];
            Range range = xlWorkSheet.UsedRange;

			CharacterGroupTemplate template = new CharacterGroupTemplate();
			for (int row = FirstRowWithData; row <= range.Rows.Count; row++)
			{
				string characterId = (string)(range.Cells[row, CharacterIdColumn] as Range).Value2;
				//string characterId = (string)(range.Cells[row, CharacterIdColumn] as Range).Value2 + "|" + (double)(range.Cells[row, SerialNumberColumn] as Range).Value2;
				int groupNumber = (int)(range.Cells[row, numberOfActors + ColumnsBeforeGroupNumbers] as Range).Value2;
				template.AddCharacterToGroup(characterId, groupNumber);
			}

            xlWorkBook.Close(true, null, null);
            xlApp.Quit();

			ReleaseObject(xlWorkSheet);
			ReleaseObject(xlWorkBook);
			ReleaseObject(xlApp);

			return template;
		}

        private void ReleaseObject(object obj)
        {
            try
            {
                Marshal.ReleaseComObject(obj);
            }
            catch (Exception)
            {
            }
		}
	}
}
