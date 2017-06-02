using System;
using System.IO;

namespace Glyssen.Shared
{
	public static class GlyssenInfo
	{
		public const string kCompany = "FCBH-SIL";
		public const string kProduct = "Glyssen";
		public const string kApplicationId = "Glyssen";

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
