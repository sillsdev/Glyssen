using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace ProtoScript.Utilities
{
	public static class Zip
	{
		public static string ExtractToTempDirectory(string pathToZipFile)
		{
			string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(tempPath);

			new FastZip().ExtractZip(pathToZipFile, tempPath, null);

			return tempPath;
		}
	}
}
