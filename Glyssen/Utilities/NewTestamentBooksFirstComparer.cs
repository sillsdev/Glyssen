using System.Collections.Generic;

namespace Glyssen.Utilities
{
	class NewTestamentBooksFirstComparer : IComparer<int>
	{
		public int Compare(int x, int y)
		{
			if (x < 40 && y < 40 || x >= 40 && y >= 40)
				return x.CompareTo(y);
			// On is a NT book number and the other is an OT book number, so higher (NT) should
			// actually be treated as LOWER.
			return -x.CompareTo(y);
		}
	}
}
