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

		public static int GetCharacter(string bookId, int chapter, int verse)
		{
			IList<CharacterVerse> matches = s_data.Where(cv => cv.BookId == bookId && cv.Chapter == chapter && cv.Verse == verse).ToList();
			if (matches.Count == 1)
				return matches.First().CharacterId;
			return Block.kUnknownCharacterId;
		}

		private static void LoadAll()
		{
			if (s_data != null)
				return;

			var list = new List<CharacterVerse>();
			foreach (var line in Resources.CharacterVerseData.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
			{
				string[] items = line.Split(new[] { "\t" }, StringSplitOptions.None);
				list.Add(new CharacterVerse
				{
					BookId = items[0], 
					Chapter = Int32.Parse(items[1]),
					Verse = ScrReference.VerseToIntStart(items[2]), 
					Character = items[3], 
					CharacterId = Int32.Parse(items[4]), 
					Delivery = items[5]//, 
					//Alias = items[6]
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
