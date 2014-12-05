using System;
using System.Collections.Generic;
using System.Text;

namespace DevTools
{
	class CharacterCharacterId
	{
		public static List<CharacterCharacterId> All()
		{
			var all = new List<CharacterCharacterId>();
			int lineNum = 0;
			foreach (var line in ControlFiles.modified_Character_VoiceTalent.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
			{
				lineNum++;
				if (string.IsNullOrEmpty(line))
					continue;
				var cc = new CharacterCharacterId();

				int lastComma = line.LastIndexOf(",", StringComparison.InvariantCulture);
				cc.CharacterId = line.Substring(lastComma+1);

				string theRest = line.Substring(0, lastComma);
				lastComma = theRest.LastIndexOf(",", StringComparison.InvariantCulture);
				cc.Character = theRest.Substring(0, lastComma);
				cc.VoiceTalentId = theRest.Substring(lastComma+1);
				Int32.Parse(cc.VoiceTalentId);

				all.Add(cc);
			}
			return all;
		}

		public static string AllTabDilimited(List<CharacterCharacterId> list)
		{
			var sb = new StringBuilder();
			foreach (CharacterCharacterId cv in list)
				sb.Append(cv.TabDelimited()).Append(Environment.NewLine);
			return sb.ToString();
		}

		public string Character;
		public string CharacterId;
		public string VoiceTalentId;

		public string TabDelimited()
		{
			return Character + "\t" + CharacterId;
		}
	}
}
