using System;

namespace ProtoScript.Utilities
{
	public static class MathUtilities
	{
		/// <summary>
		/// Obtain a percentage as an int given two ints.
		/// </summary>
		/// <param name="numerator"></param>
		/// <param name="denominator"></param>
		/// <param name="maxPercent"></param>
		/// <returns></returns>
		public static int Percent(int numerator, int denominator, int maxPercent = 100)
		{
			//Resharper thinks the casts to double are not needed, but they are
			if (maxPercent == 0)
				return (int)((((double)numerator) / ((double)denominator)) * 100);
			return Math.Min(99, (int)((((double)numerator) / ((double)denominator)) * 100));
		}
	}
}
