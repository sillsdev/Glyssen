using System;
using System.IO;
using OfficeOpenXml;

namespace Glyssen.Character
{
	public class CharacterGroupTemplateExcelFile : ICharacterGroupSource
	{
		private const int CharacterIdColumn = 4;
		private const int ColumnsBeforeGroupNumbers = 4;
		private const int FirstRowWithData = 2;

		private readonly string m_filePath;

		public CharacterGroupTemplateExcelFile(string path)
		{
			m_filePath = path;
		}

		public CharacterGroupTemplate GetTemplate(int numberOfActors)
		{
			CharacterGroupTemplate template = new CharacterGroupTemplate();

			using (ExcelPackage excelPackage = new ExcelPackage(new FileInfo(m_filePath)))
			using (ExcelWorksheet ws = excelPackage.Workbook.Worksheets[1])
			{
				object numActorsColumnHeader = ws.Cells[1, numberOfActors + ColumnsBeforeGroupNumbers].Value;
				if (!(numActorsColumnHeader is double) || Convert.ToInt32(numberOfActors) != numberOfActors)
					throw new ArgumentException("Invalid number of actors.", "numberOfActors");

				int row = FirstRowWithData;
				while (true)
				{
					string characterId = (string)(ws.Cells[row, CharacterIdColumn].Value);
					if (string.IsNullOrWhiteSpace(characterId))
						break;
					double groupNumber = (double)ws.Cells[row, numberOfActors + ColumnsBeforeGroupNumbers].Value;
					template.AddCharacterToGroup(characterId, Convert.ToInt32(groupNumber));
					row++;
				}
			}

			return template;
		}
	}
}
