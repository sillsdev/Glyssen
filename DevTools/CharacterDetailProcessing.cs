using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Glyssen.Character;
using SIL.Scripture;

namespace DevTools
{
	class CharacterDetailProcessing
	{
		private const char TAB = '\t';
		public static void GenerateReferences()
		{
			var data = ReadFile();
//			data.Sort((x, y) => String.Compare(x.CharacterId, y.CharacterId, StringComparison.Ordinal));
			PopulateReferences(data);
			WriteFile(Stringify(data));
		}

		private static void PopulateReferences(List<CharacterDetailLine> lines)
		{
			Dictionary<string, List<BCVRef>> dictionary = ControlCharacterVerseData.Singleton.GetAllQuoteInfo().GroupBy(c => c.Character).ToDictionary(c => c.Key, cv => cv.Select(c => c.BcvRef).ToList());
			foreach (var line in lines)
			{
				List<BCVRef> bcvRefs;
				if (dictionary.TryGetValue(line.CharacterId, out bcvRefs))
					switch (bcvRefs.Count)
					{
						case 0:
							continue;
						case 1:
							line.ReferenceComment = bcvRefs.Min().ToString();
							break;
						case 2:
							line.ReferenceComment = bcvRefs.Min() + " & " + bcvRefs.Max();
							break;
						default:
							line.ReferenceComment = bcvRefs.Min() + " <-(" + (bcvRefs.Count - 2) + " more)-> " + bcvRefs.Max();
							break;
					}
			}
		}

		private static string Stringify(List<CharacterDetailLine> lines)
		{
			StringBuilder sb = new StringBuilder();
			foreach (var line in lines)
				sb.Append(GetNewLine(line)).Append(Environment.NewLine);
			return sb.ToString();
		}

		private static string GetNewLine(CharacterDetailLine line)
		{
			return line.CurrentLine + TAB + line.ReferenceComment;
		}

		private static void WriteFile(string fileText)
		{
			fileText = "#Character ID\tMax Speakers\tGender\tAge\tStatus\tComment\tReference Comment" + Environment.NewLine + fileText;
			File.WriteAllText("..\\..\\..\\Glyssen\\Resources\\CharacterDetail.txt", fileText);
		}

		private static List<CharacterDetailLine> ReadFile()
		{
			return File.ReadAllLines("..\\..\\..\\Glyssen\\Resources\\CharacterDetail.txt").Select(ReadLine).Where(l => l != null).ToList();
		}

		private static CharacterDetailLine ReadLine(string line)
		{
			if (line.StartsWith("#"))
				return null;

			string characterId = line.Substring(0, line.IndexOf(TAB));
			return new CharacterDetailLine
			{
				CharacterId = characterId,
				CurrentLine = line
			};
		}

		private class CharacterDetailLine
		{
			public string CharacterId { get; set; }
			public string CurrentLine { get; set; }
			public string ReferenceComment { get; set; }
		}
	}
}
