using System.Collections.Generic;
using SIL.Scripture;

namespace Glyssen.Shared
{
	public static class StandardCanon
	{
		public static IEnumerable<string> AllBookCodes
		{
			get
			{
				for (var i = 1; i <= BCVRef.LastBook; i++)
					yield return BCVRef.NumberToBookCode(i);
			}
		}
	}
}
