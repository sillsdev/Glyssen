using System;
using System.Diagnostics;
using System.IO;

namespace Waxuquerque.Utilities
{
	public static class FileSystemUtils
	{
		public static void SafeCreateAndOpenFolder(string folderPath)
		{
			try
			{
				Directory.CreateDirectory(folderPath);
				Process.Start(folderPath);
			}
			catch (Exception)
			{
				// Ignore;
			}
		}
	}
}
