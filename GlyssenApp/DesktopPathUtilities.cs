using Waxuquerque;
using PathUtilities = SIL.IO.PathUtilities;

namespace GlyssenApp
{
	internal class DesktopPathUtilities : IPathUtilities
	{
		public void OpenDirectoryInExplorer(string directory)
		{
			PathUtilities.OpenDirectoryInExplorer(directory);
		}
	}
}