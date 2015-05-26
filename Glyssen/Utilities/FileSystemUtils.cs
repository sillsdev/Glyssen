using System.Runtime.InteropServices;
using System.Text;

namespace Glyssen.Utilities
{
	public static class FileSystemUtils
	{
		public static string GetShortName(string path)
		{
			var shortBuilder = new StringBuilder(300);
			GetShortPathName(path, shortBuilder, (uint)shortBuilder.Capacity);
			return shortBuilder.ToString();
		}

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern uint GetShortPathName(
		   [MarshalAs(UnmanagedType.LPTStr)]string lpszLongPath,
		   [MarshalAs(UnmanagedType.LPTStr)]StringBuilder lpszShortPath,
		   uint cchBuffer);
	}
}
