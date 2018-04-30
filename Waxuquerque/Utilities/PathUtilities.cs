using System;
using System.Diagnostics;

namespace Waxuquerque.Utilities
{
	public interface IPathUtilities
	{
		void OpenDirectoryInExplorer(string directory);
	}

	public class PathUtilities
	{
		private static IPathUtilities s_instance;

		public static IPathUtilities Default
		{
			get
			{
				if (s_instance == null)
					throw new InvalidOperationException("Not Initialized. Set PathUtilities.Default first.");
				return s_instance;
			}
			set => s_instance = value;
		}

		internal static void OpenDirectoryInExplorer(string directory)
		{
			Default.OpenDirectoryInExplorer(directory);
		}

		internal static void OpenFileInApplication(string filePath)
		{
			Process.Start(filePath);
		}
	}
}