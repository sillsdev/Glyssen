using System;
using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;

namespace Glyssen.Character
{
	public class CharacterGroupTemplateExcelFile : ICharacterGroupSource
	{
		private const int CharacterIdColumn = 4;
		private const int ColumnsBeforeGroupNumbers = 4;
		private const int FirstRowWithData = 2;

		private readonly Project m_project;
		private readonly string m_filePath;
		private int m_maxNumberOfActors = -1;

		public CharacterGroupTemplateExcelFile(Project project, string path)
		{
			m_project = project;
			m_filePath = path;
		}

		public CharacterGroupTemplate GetTemplate(int numberOfActors)
		{
			if (numberOfActors < 1)
				throw new ArgumentException("Number of actors must be greater than zero.", "numberOfActors");
			if (numberOfActors > MaxNumberOfActors)
				numberOfActors = MaxNumberOfActors;

			CharacterGroupTemplate template = new CharacterGroupTemplate(m_project);
			var characterIds = new HashSet<string>();

			using (ExcelPackage excelPackage = new ExcelPackage(new FileInfo(m_filePath)))
			using (ExcelWorksheet ws = excelPackage.Workbook.Worksheets[1])
			{
				object numActorsColumnHeader = ws.Cells[1, numberOfActors + ColumnsBeforeGroupNumbers].Value;
				if (!(numActorsColumnHeader is double) || Convert.ToInt32(numActorsColumnHeader) != numberOfActors)
					throw new ArgumentException("Invalid number of actors.", "numberOfActors");

				int row = FirstRowWithData;
				while (true)
				{
					string characterId = (string)(ws.Cells[row, CharacterIdColumn].Value);
					if (string.IsNullOrWhiteSpace(characterId))
						break;
					if (!characterIds.Contains(characterId))
					{
						characterIds.Add(characterId);
						double groupNumber = (double)ws.Cells[row, numberOfActors + ColumnsBeforeGroupNumbers].Value;
						template.AddCharacterToGroup(characterId, Convert.ToInt32(groupNumber));
					}
					row++;
				}
			}

			return template;
		}

		public int MaxNumberOfActors
		{
			get
			{
				if (m_maxNumberOfActors != -1)
					return m_maxNumberOfActors;

				int max = -1;
				using (ExcelPackage excelPackage = new ExcelPackage(new FileInfo(m_filePath)))
				using (ExcelWorksheet ws = excelPackage.Workbook.Worksheets[1])
				{
					for (int i = ColumnsBeforeGroupNumbers + 1;; i++)
					{
						object numActorsColumnHeader = ws.Cells[1, ColumnsBeforeGroupNumbers + i].Value;
						if (numActorsColumnHeader is double)
							max = Convert.ToInt32(numActorsColumnHeader);
						else
							break;
					}
				}
				return max;
			}
		}
	}
}
