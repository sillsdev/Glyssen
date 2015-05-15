using System;
using System.Collections.Generic;
using Glyssen.Properties;

namespace Glyssen.Character
{
	public class CharacterDetailData
	{
		private static CharacterDetailData s_singleton;
		private IList<CharacterDetail> m_data;

		internal static string TabDelimitedCharacterDetailData { get; set; }

		private CharacterDetailData()
		{
			// Tests can set this before accessing the Singleton.
			if (TabDelimitedCharacterDetailData == null)
				TabDelimitedCharacterDetailData = Resources.CharacterIdMap;
			LoadData(TabDelimitedCharacterDetailData);
		}

		public static CharacterDetailData Singleton
		{
			get { return s_singleton ?? (s_singleton = new CharacterDetailData()); }
		}

		public IEnumerable<CharacterDetail> GetAll()
		{
			return m_data;
		}

		private void LoadData(string tabDelimitedCharacterDetailData)
		{
			var list = new List<CharacterDetail>();
			int lineNumber = 0;
			foreach (var line in tabDelimitedCharacterDetailData.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
			{
				string[] items = line.Split(new[] { "\t" }, StringSplitOptions.None);
				CharacterDetail detail = ProcessLine(items, lineNumber++);
				if (detail != null)
					list.Add(detail);
			}
			m_data = list;
		}

		private CharacterDetail ProcessLine(string[] items, int lineNumber)
		{
			if (lineNumber == 0)
				return null;
			return new CharacterDetail
			{
				Character = items[0],
				MultipleSpeakers = items[2].Equals("True", StringComparison.OrdinalIgnoreCase),
				Gender = items[3],
				Age = items[4],
				Comment = items[5]
			};
		}
	}
}
