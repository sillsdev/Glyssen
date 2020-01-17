using System;
using System.IO;

namespace Glyssen.Shared
{
	/// <summary>
	/// Information about the Glyssen program which may be needed outside the assembly
	/// </summary>
	public static class GlyssenInfo
	{
		public const string kCompany = "FCBH-SIL";
		public const string kProduct = "Glyssen";
		public const string kApplicationId = "Glyssen";

		public static string BaseDataFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
			kCompany, kProduct);

        public static string GetProjectFilePath(string langId, string publicationId, string recordingProjectId)
        {
            return Path.Combine(GetProjectFolderPath(langId, publicationId, recordingProjectId), langId + Constants.kProjectFileExtension);
        }

        public static string GetProjectFolderPath(string langId, string publicationId, string recordingProjectId)
        {
            return Path.Combine(GlyssenInfo.BaseDataFolder, langId, publicationId, recordingProjectId);
        }
	}
}
