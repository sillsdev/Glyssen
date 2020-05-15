using System;

namespace GlyssenEngine.Utilities
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
			return (int)PercentAsDouble(numerator, denominator, maxPercent);
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
			if (denominator == 0)
				return maxPercent == 0 ? 100 : maxPercent;
			if (maxPercent == 0)
				return ((double)numerator / denominator) * 100;
			return Math.Min(maxPercent, ((double)numerator / denominator) * 100);
		}

		public static object FormattedPercent(double value, int minDecimalPlaces, int maxDecimalPlaces, bool suppressDecimalPlacesFor100Pct = true)
		{
			if (value == 100d && suppressDecimalPlacesFor100Pct)
				return "100%";
			int n = minDecimalPlaces;
			string output;
			bool roundedUpTo100Pct;
			do
			{
				output = String.Format("{0:N" + n + "}%", value);
				roundedUpTo100Pct = value < 100 && output.Contains("100");
			} while (roundedUpTo100Pct && ++n <= maxDecimalPlaces );
			if (roundedUpTo100Pct)
			{
				output = "99";
				if (maxDecimalPlaces > 0)
					output += ".".PadRight(maxDecimalPlaces + 1, '9');
				output += "%";
			}
			return output;
		}
	}
}
