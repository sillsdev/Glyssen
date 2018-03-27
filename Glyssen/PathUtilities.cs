using System;
using System.Diagnostics;

namespace Glyssen
{
	public interface IPathUtilities
	{
		void OpenDirectoryInExplorer(string directory);
	}

	public class PathUtilities
	{
		private static IPathUtilities s_instance;

		public static void SetInstance(IPathUtilities instance)
		{
			s_instance = instance;
		}

		internal static void OpenDirectoryInExplorer(string directory)
		{
			if (s_instance == null)
				throw new InvalidOperationException("You must call SetInstance() first.");

			s_instance.OpenDirectoryInExplorer(directory);
		}

		internal static void OpenFileInApplication(string filePath)
		{
			Process.Start(filePath);
		}
	}
}