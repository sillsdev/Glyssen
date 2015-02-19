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
			if (maxPercent == 0)
				return (int)(((double)numerator / denominator) * 100);
			return Math.Min(maxPercent, (int)(((double)numerator / denominator) * 100));
		}

		/// <summary>
		/// Obtain a percentage as a double given two ints.
		/// </summary>
		/// <param name="numerator"></param>
		/// <param name="denominator"></param>
		/// <param name="maxPercent"></param>
		/// <returns></returns>
		public static double PercentAsDouble(int numerator, int denominator, int maxPercent = 100)
		{
			if (maxPercent == 0)
				return ((double)numerator / denominator) * 100;
			return Math.Min(maxPercent, ((double)numerator / denominator) * 100);
		}
	}
}
