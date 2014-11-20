using System;

namespace ProtoScript.ToPalaso
{
	/// <summary>
	/// These methods have been copied from the LiSaFT project in Paratext
	/// We want them to live in libpalaso
	/// </summary>
	public class ScrReference
	{
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// A version of VerseToInt that returns the starting verse value. 
		/// </summary>
		/// <param name="sourceString"></param>
		/// <returns>the starting verse value</returns>
		/// ------------------------------------------------------------------------------------
		public static int VerseToIntStart(string sourceString)
		{
			int startVerse, endVerse;
			VerseToInt(sourceString, out startVerse, out endVerse);
			return startVerse;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// A version of VerseToInt that returns the ending verse value. 
		/// </summary>
		/// <param name="sourceString"></param>
		/// <returns>the ending verse value</returns>
		/// ------------------------------------------------------------------------------------
		public static int VerseToIntEnd(string sourceString)
		{
			int startVerse, endVerse;
			VerseToInt(sourceString, out startVerse, out endVerse);
			return endVerse;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// This is a helper function to get a starting and ending verse number from a string
		/// which may or may not represent a verse bridge. Ignore any unusual syntax.
		/// </summary>
		/// <param name="sVerseNum">the string representing the verse number(s).</param>
		/// <param name="nVerseStart">the starting verse number in sVerseNum.</param>
		/// <param name="nVerseEnd">the ending verse number in sVerseNum (will be different from
		/// startRef if sVerseNum represents a verse bridge).</param>
		/// ------------------------------------------------------------------------------------
		public static void VerseToInt(string sVerseNum, out int nVerseStart, out int nVerseEnd)
		{
			int nFactor = 1;
			int nVerseT = 0;
			nVerseStart = nVerseEnd = 0;
			// nVerseFirst is the left-most (or right-most if R2L) non-zero number found.
			int nVerseFirst = nVerseT;
			bool fVerseBridge = false;
			if (sVerseNum == null)
				return;
			// REVIEW JohnW (TomB): For robustness, our initial implementation will assume
			// that the first set of contiguous numbers is the starting verse number and
			// the last set of contiguous numbers is the ending verse number. This way, we
			// don't have to know what all the legal possibilities of bridge markers and
			// sub-verse segment indicators are.
			for (int i = sVerseNum.Length - 1; i >= 0; i--)
			{
				int numVal = -1;
				if (Char.IsDigit(sVerseNum[i]))
					numVal = (int)Char.GetNumericValue(sVerseNum[i]);

				if (numVal >= 0 && numVal <= 9)
				{
					if (nFactor > 100) // verse number greater than 999
					{
						// REVIEW JohnW (TomB): Need to decide how we want to display this.
						nVerseT = 999;
					}
					else
					{
						nVerseT += nFactor * numVal;
						nFactor *= 10;
					}
					nVerseFirst = nVerseT;
				}
				else if (nVerseT > 0)
				{
					if (!fVerseBridge)
					{
						fVerseBridge = true;
						nVerseFirst = nVerseEnd = nVerseT;
					}
					nVerseT = 0;
					nFactor = 1;
				}
			}
			nVerseStart = nVerseFirst;
			if (!fVerseBridge)
				nVerseEnd = nVerseFirst;

			// Don't want to use an assertion for this because it could happen due to bad input data.
			// If this causes problems, just pick one ref and use it for both or something.
			// TODO TomB: Later, we need to catch this and flag it as an error.
			//Assert(nVerseStart <= nVerseEnd);
		}
	}
}
