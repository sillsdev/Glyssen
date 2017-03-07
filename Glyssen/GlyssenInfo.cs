using System;
using System.Collections.Generic;
using System.IO;

namespace Glyssen
{
	public static class GlyssenInfo
	{
		public const string kCompany = "FCBH-SIL";
		public const string kProduct = "Glyssen";
		public const string kApplicationId = "Glyssen";

		public const double kKeyStrokesPerHour = 6000;
		public const double kCameoCharacterEstimatedHoursLimit = 0.2;

		public static string BaseDataFolder
		{
			get
			{
				return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
					kCompany, kProduct);
			}
		}
	}
}
