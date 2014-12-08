using System;
using System.Collections.Generic;
using System.Linq;
using ProtoScript.Properties;
using SIL.ScriptureUtils;

namespace ProtoScript
{
	public class CharacterVerseData : ICharacterVerseInfo
	{
		public const string kNotAQuote = "Not A Quote";

		private static CharacterVerseData s_singleton;

		private IEnumerable<CharacterVerse> m_data;

		internal static string TabDelimitedCharacterVerseData { get; set; }

		private CharacterVerseData()
		{
			// Tests can set this before accessing the Singleton.
			if (TabDelimitedCharacterVerseData == null)
				TabDelimitedCharacterVerseData = Resources.CharacterVerseData;
			LoadAll();
		}

		public static CharacterVerseData Singleton
		{
			get { return s_singleton ?? (s_singleton = new CharacterVerseData()); }
		}

		public int ControlFileVersion { get; private set; }

		public IEnumerable<CharacterVerse> GetCharacters(string bookCode, int chapter, int verse)
		{
			return m_data.Where(cv => cv.BookCode == bookCode && cv.Chapter == chapter && cv.Verse == verse);
		}

		public IEnumerable<CharacterVerse> GetUniqueCharacters()
		{
			return new SortedSet<CharacterVerse>(m_data, new CharacterDeliveryComparer());
		}

		public IEnumerable<CharacterVerse> GetUniqueCharacters(string bookCode)
		{
			return new SortedSet<CharacterVerse>(m_data.Where(cv => cv.BookCode == bookCode), new CharacterDeliveryComparer());
		}

		public IEnumerable<CharacterVerse> GetUniqueCharacters(string bookCode, int chapter)
		{
			return new SortedSet<CharacterVerse>(m_data.Where(cv => cv.BookCode == bookCode && cv.Chapter == chapter), new CharacterDeliveryComparer());
		}

		public IEnumerable<CharacterVerse> GetAllQuoteInfo(string bookId)
		{
			return m_data.Where(cv => cv.BookCode == bookId);
		}

		private void LoadAll()
		{
			if (m_data != null)
				return;

			bool firstLine = true;
			var list = new HashSet<CharacterVerse>();
			foreach (var line in TabDelimitedCharacterVerseData.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
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

				int chapter = Int32.Parse(items[1]);
				for (int verse = ScrReference.VerseToIntStart(items[2]); verse <= ScrReference.VerseToIntEnd(items[2]); verse++)
					list.Add(new CharacterVerse
					{
						BcvRef = new BCVRef(BCVRef.BookToNumber(items[0]), chapter, verse),
						Character = items[3], 
						Delivery = items[4],
						Alias = items[5]
					});
			}
			if (!list.Any())
				throw new ApplicationException("No character verse data available!");
			m_data = list;
		}

	}
}
