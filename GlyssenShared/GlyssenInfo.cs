namespace Glyssen.Shared
{
	/// <summary>
	/// Information about the program which may be needed outside the assembly
	/// </summary>
	public static class GlyssenInfo
	{
		private const string kCompany = "FCBH-SIL";
		private const string kProduct = "Glyssen";
		private const string kApplicationId = "Glyssen";

		private static string s_company;
		private static string s_applicationId;
		private static string s_product;

		public static string Company
		{
			get => s_company ?? kCompany;
			set => s_company = value;
		}

		public static string ApplicationId
		{
			get => s_applicationId ?? kApplicationId;
			set => s_applicationId = value;
		}

		public static string Product
		{
			get => s_product ?? kProduct;
			set => s_product = value;
		}
	}
}
