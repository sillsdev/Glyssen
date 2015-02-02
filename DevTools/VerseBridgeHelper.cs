using System;
using System.IO;
using System.Text;
using ProtoScript.Character;

namespace DevTools
{
	public class VerseBridgeHelper
	{
		public const string TAB = "\t";

		/// <summary>
		/// CAREFUL!!!
		/// Be careful with this method!
		/// It simply reads in the data and rewrites it. 
		/// That means comment lines will be removed.
		/// </summary>
		public static void RemoveAllVerseBridges()
		{
			var allQuoteInfo = ControlCharacterVerseData.Singleton.GetAllQuoteInfo();

			var sb = new StringBuilder();
			foreach (ProtoScript.Character.CharacterVerse cv in allQuoteInfo)
			{
				sb.Append(cv.BookCode).Append(TAB)
					.Append(cv.Chapter).Append(TAB).Append(cv.Verse).Append(TAB).Append(cv.Character).Append(TAB)
					.Append(cv.Delivery).Append(TAB).Append(cv.Alias).Append(TAB)
					.Append(DialogueOutput(cv.IsDialogue)).Append(TAB).Append(cv.DefaultCharacter).Append(TAB).Append(cv.ParallelPassageReferences)
					.Append(Environment.NewLine);
			}

			File.WriteAllText(Path.Combine(CharacterListProcessing.kBaseDirForRealOutput, "CharacterVerse.txt"), sb.ToString());
		}

		private static string DialogueOutput(bool isDialogue)
		{
			return isDialogue ? "TRUE" : "FALSE";
		}
	}
}
