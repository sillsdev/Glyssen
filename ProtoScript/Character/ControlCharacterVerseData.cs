using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using ProtoScript.Properties;
using SIL.ScriptureUtils;

namespace ProtoScript.Character
{
	public class ControlCharacterVerseData : CharacterVerseData
	{
		private static ControlCharacterVerseData s_singleton;

		internal static string TabDelimitedCharacterVerseData { get; set; }

		private ControlCharacterVerseData()
		{
			// Tests can set this before accessing the Singleton.
			if (TabDelimitedCharacterVerseData == null)
				TabDelimitedCharacterVerseData = Resources.CharacterVerseData;
			LoadAll();
		}

		public static ControlCharacterVerseData Singleton
		{
			get { return s_singleton ?? (s_singleton = new ControlCharacterVerseData()); }
		}

		public int ControlFileVersion { get; private set; }

		private void LoadAll()
		{
			if (Any())
				return;

			LoadData(TabDelimitedCharacterVerseData);

			if (!Any())
				throw new ApplicationException("No character verse data available!");
		}

		protected override ISet<CharacterVerse> ProcessLine(string[] items, int lineNumber)
		{
			if (lineNumber == 0)
			{
				ProcessFirstLine(items);
				return null;
			}
			return base.ProcessLine(items, lineNumber);
		}

		private void ProcessFirstLine(string[] items)
		{
			int cfv;
			if (Int32.TryParse(items[1], out cfv) && items[0].StartsWith("Control File"))
				ControlFileVersion = cfv;
			else
				throw new ApplicationException("Bad format in CharacterVerseDataBase metadata: " + items);
		}

		protected override CharacterVerse CreateCharacterVerse(BCVRef bcvRef, string[] items)
		{
			QuoteType quoteType = QuoteType.Normal;
			if (items.Length > kiQuoteType)
			{
				var value = items[kiQuoteType];
				if (value != "FALSE" && value != String.Empty && !Enum.TryParse(value, out quoteType))
				{
					throw new InvalidDataException(string.Format("items[{0}] has a value of {1}, which is not a valid {2}",
						kiQuoteType, value, typeof (QuoteType).Name));
				}
			}
			return new CharacterVerse(bcvRef, items[3], items[4], items[5], false, quoteType,
				(items.Length > kiDefaultCharacter) ? items[kiDefaultCharacter] : null,
				(items.Length > kiParallelPassageInfo) ? items[kiParallelPassageInfo] : null);
		}

		#region Temporary stuff to distinguish between implicit and explicit quotes
		private readonly ISet<CharacterVerse> m_usedCharacters = new HashSet<CharacterVerse>();
		public bool AddUsedCharacters(IEnumerable<CharacterVerse> cvUsed)
		{
			bool gotOne = false;
			foreach (var cv in cvUsed)
			{
				m_usedCharacters.Add(cv);
				gotOne = true;
			}
			return gotOne;
		}

		public void AddUsedCharacters(CharacterVerse cvUsed, int startVerse, int endVerse)
		{
			int book = BCVRef.BookToNumber(cvUsed.BookCode);
			for (int i = startVerse; i <= endVerse; i++)
			{
				var cv = new CharacterVerse(new BCVRef(book, cvUsed.Chapter, i), cvUsed.Character, cvUsed.Delivery, cvUsed.Alias,
					true, cvUsed.QuoteType, cvUsed.DefaultCharacter, cvUsed.ParallelPassageReferences); 
				m_usedCharacters.Add(cv);
			}
		}

		public void WriteToFile(string fullPath)
		{
			File.WriteAllText(fullPath, ToTabDelimited());
		}

		private string ToTabDelimited()
		{
			var sb = new System.Text.StringBuilder();
			foreach (CharacterVerse cv in GetAllQuoteInfo().Except(m_usedCharacters).Where(c => c.QuoteType == QuoteType.Normal))
				sb.Append(GetTabDelimitedFields(cv)).Append(Environment.NewLine);
			return sb.ToString();
		}

		private string GetTabDelimitedFields(CharacterVerse cv)
		{
			return cv.BookCode + "\t" + cv.Chapter + "\t" + cv.Verse + "\t" + cv.Character + "\t" + cv.Delivery + "\t" + cv.Alias +
				"\tIndirect\t" + cv.DefaultCharacter + "\t" + cv.ParallelPassageReferences;
		}
		#endregion
	}
}
