using System;
using System.IO;
using System.Linq;
using System.Text;
using Glyssen.Character;

namespace DevTools
{
	public class VerseBridgeHelper
	{
		public const string kTab = "\t";

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
			foreach (Glyssen.Character.CharacterVerse cv in allQuoteInfo.Distinct().OrderBy(cv => cv.BcvRef))
			{
				sb.Append(cv.BookCode).Append(kTab)
					.Append(cv.Chapter).Append(kTab).Append(cv.Verse).Append(kTab).Append(cv.Character).Append(kTab)
					.Append(cv.Delivery).Append(kTab).Append(cv.Alias).Append(kTab)
					.Append(cv.QuoteType).Append(kTab).Append(cv.DefaultCharacter).Append(kTab).Append(cv.ParallelPassageReferences)
					.Append(Environment.NewLine);
			}

			File.WriteAllText(Path.Combine(CharacterListProcessing.kBaseDirForRealOutput, "CharacterVerse.txt"), sb.ToString());
		}
	}
}
