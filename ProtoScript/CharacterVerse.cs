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

		public static string GetCharacter(string bookId, int chapter, int verse)
		{
			IList<CharacterVerse> matches = s_data.Where(cv => cv.BookId == bookId && cv.Chapter == chapter && cv.Verse == verse).ToList();
			if (matches.Count == 1)
				return matches.First().Character;
			return Block.UnknownCharacter;
		}

		public static IEnumerable<CharacterVerse> GetAllQuoteInfo(string bookId)
		{
			return s_data.Where(cv => cv.BookId == bookId);
		}

		private static void LoadAll()
		{
			if (s_data != null)
				return;

			var list = new List<CharacterVerse>();
			foreach (var line in Resources.CharacterVerseData.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
			{
				string[] items = line.Split(new[] { "\t" }, StringSplitOptions.None);
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
