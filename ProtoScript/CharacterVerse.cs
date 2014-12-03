using System;
using System.Collections.Generic;
using System.Linq;
using ProtoScript.Properties;
using SIL.ScriptureUtils;

namespace ProtoScript
{
	public class CharacterVerse
	{
		private static IEnumerable<CharacterVerse> s_data;

		static CharacterVerse()
		{
			LoadAll();
		}

		public static int ControlFileVersion { get; private set; }

		public static string GetCharacter(string bookId, int chapter, int verse)
		{
			IList<CharacterVerse> matches = s_data.Where(cv => cv.BookId == bookId && cv.Chapter == chapter && cv.Verse == verse).ToList();
			if (matches.Count == 1)
				return matches.First().Character;
			if (matches.Count > 1)
			{
				string character = null;
				foreach (CharacterVerse cv in matches)
				{
					if (character == null)
					{
						character = cv.Character;
						continue;
					}
					if (character != cv.Character)
						return Block.AmbiguousCharacter;
				}
				return character;
			}
			return Block.UnknownCharacter;
		}

		private static void LoadAll()
		{
			if (s_data != null)
				return;

			bool firstLine = true;
			var list = new List<CharacterVerse>();
			foreach (var line in Resources.CharacterVerseData.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
			{
				string[] items = line.Split(new[] { "\t" }, StringSplitOptions.None);
				if (firstLine)
				{
					int cfv;
					if (Int32.TryParse(items[1], out cfv) && items[0].StartsWith("Control File"))
						ControlFileVersion = cfv;
					else
						throw new ApplicationException("Bad format in CharacterVerseData metadata: " + line);
					firstLine = false;
					continue;
				}
				if (items.Length != 6)
					throw new ApplicationException("Bad format in CharacterVerseData! Line #: " + list.Count + "; Line contents: " + line);

				list.Add(new CharacterVerse
				{
					BookId = items[0], 
					Chapter = Int32.Parse(items[1]),
					Verse = ScrReference.VerseToIntStart(items[2]), 
					Character = items[3], 
					Delivery = items[4],
					Alias = items[5]
				});
			}
			if (!list.Any())
				throw new ApplicationException("No character verse data available!");
			s_data = list;
		}

		public string Character;
		public int CharacterId;
		public string BookId;
		public int Chapter;
		public int Verse;
		public string Delivery;
		public string Alias;
	}
}
