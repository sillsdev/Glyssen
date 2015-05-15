using System;
using System.IO;
using System.Text;
using SIL.ScriptureUtils;

namespace Glyssen.Character
{
	public class ProjectCharacterVerseData : CharacterVerseData
	{
		public ProjectCharacterVerseData(string fullPath)
		{
			if (File.Exists(fullPath))
				LoadData(File.ReadAllText(fullPath));
			else
				LoadData("");
		}

		public virtual void Add(CharacterVerse cv)
		{
			AddCharacterVerse(cv);
		}

		public void WriteToFile(string fullPath)
		{
			RemoveDataAlsoInControlFile();
			File.WriteAllText(fullPath, ToTabDelimited());
		}

		private string ToTabDelimited()
		{
			var sb = new StringBuilder();
			foreach (CharacterVerse cv in GetAllQuoteInfo())
				sb.Append(GetTabDelimitedFields(cv)).Append(Environment.NewLine);
			return sb.ToString();
		}

		private string GetTabDelimitedFields(CharacterVerse cv)
		{
			return cv.BookCode + "\t" + cv.Chapter + "\t" + cv.Verse + "\t" + cv.Character + "\t" + cv.Delivery + "\t" + cv.Alias;
		}

		protected override CharacterVerse CreateCharacterVerse(BCVRef bcvRef, string[] items)
		{
			return new CharacterVerse(bcvRef, items[3], items[4], items[5], true);
		}

		private void RemoveDataAlsoInControlFile()
		{
			RemoveAll(ControlCharacterVerseData.Singleton.GetAllQuoteInfo(), new BcvCharacterDeliveryEqualityComparer());
		}
	}
}
