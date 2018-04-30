using Waxuquerque;
using Waxuquerque.Utilities;
using PathUtilities = SIL.IO.PathUtilities;

namespace GlyssenApp.Utilities
{
	internal class DesktopPathUtilities : IPathUtilities
	{
		public void OpenDirectoryInExplorer(string directory)
		{
			PathUtilities.OpenDirectoryInExplorer(directory);
		}
	}
}