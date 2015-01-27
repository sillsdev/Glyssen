using System;
using System.IO;
using System.Text;
using SIL.ScriptureUtils;

namespace ProtoScript.Character
{
	public class ProjectCharacterVerseData : CharacterVerseData
	{
		public ProjectCharacterVerseData(string fullPath)
		{
			if (File.Exists(fullPath))
				LoadData(File.ReadAllText(fullPath));
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
				sb.Append(cv.ToTabDelimited()).Append(Environment.NewLine);
			return sb.ToString();
		}

		protected override CharacterVerse CreateCharacterVerse(BCVRef bcvRef, string[] items)
		{
			return new CharacterVerse(bcvRef, items[3], items[4], items[5], true);
		}


		private void RemoveDataAlsoInControlFile()
		{
			RemoveAll(ControlCharacterVerseData.Singleton.GetAllQuoteInfo(), new BcvCharacterDeliveryComparer());
		}
	}
}
