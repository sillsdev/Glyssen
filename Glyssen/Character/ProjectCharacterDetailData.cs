using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Glyssen.Character
{
	public static class ProjectCharacterDetailData
	{
		public static void WriteToFile(ISet<CharacterDetail> projectCharacterDetailData, string fullPath)
		{
			File.WriteAllText(fullPath, ToTabDelimited(projectCharacterDetailData));
		}

		private static string ToTabDelimited(ISet<CharacterDetail> projectCharacterDetailData)
		{
			var sb = new StringBuilder();
			foreach (CharacterDetail detail in projectCharacterDetailData)
				sb.Append(GetTabDelimitedFields(detail)).Append(Environment.NewLine);
			return sb.ToString();
		}

		private static string GetTabDelimitedFields(CharacterDetail detail)
		{
			return detail.CharacterId + '\t' + detail.MaxSpeakers + '\t' + detail.Gender + '\t' + detail.Age;
		}

		public static ISet<CharacterDetail> Load(string fullPath)
		{
			if (!File.Exists(fullPath))
				return new HashSet<CharacterDetail>();

			var tabDelimitedCharacterDetailData = File.ReadAllText(fullPath);
			var list = new HashSet<CharacterDetail>();
			foreach (var line in tabDelimitedCharacterDetailData.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
				list.Add(ProcessLine(line.Split(new[] { "\t" }, StringSplitOptions.None)));
			return list;
		}

		private static CharacterDetail ProcessLine(string[] items)
		{
			CharacterGender gender;
			if (!Enum.TryParse(items[2], false, out gender))
				gender = CharacterGender.Either;
			CharacterAge age;
			if (!Enum.TryParse(items[3], false, out age))
				age = CharacterAge.Adult;
			return new CharacterDetail
			{
				CharacterId = items[0],
				MaxSpeakers = Int32.Parse(items[1]),
				Gender = gender,
				Age = age
			};
		}
	}
}
