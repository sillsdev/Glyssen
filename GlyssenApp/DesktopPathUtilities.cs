using SIL.IO;

namespace GlyssenApp
{
	internal class DesktopPathUtilities : Glyssen.IPathUtilities
	{
		public void OpenDirectoryInExplorer(string directory)
		{
			PathUtilities.OpenDirectoryInExplorer(directory);
		}
	}
}