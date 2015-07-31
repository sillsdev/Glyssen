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
				TabDelimitedCharacterDetailData = Resources.CharacterDetail;
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
				MaxSpeakers = Int32.Parse(items[1]),
				Gender = items[2],
				Age = items[3],
				Status = items[4].Equals("Y", StringComparison.OrdinalIgnoreCase) || items[4].Equals("True", StringComparison.OrdinalIgnoreCase),
				Comment = items[5]
			};
		}
	}
}
