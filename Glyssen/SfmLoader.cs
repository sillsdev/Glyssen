using Paratext;
using SIL.IO;

namespace Glyssen
{
	public static class SfmLoader
	{
		private static ScrStylesheet s_usfmStyleSheet;

		public static ScrStylesheet GetUsfmScrStylesheet()
		{
			if (s_usfmStyleSheet != null)
				return s_usfmStyleSheet;
			string usfmStylesheetPath = FileLocator.GetFileDistributedWithApplication("sfm", "usfm.sty");
			return s_usfmStyleSheet = new ScrStylesheet(usfmStylesheetPath);
		}

		public static ScrStylesheetAdapter GetUsfmStylesheet()
		{
			return new ScrStylesheetAdapter(GetUsfmScrStylesheet());
		}
	}
}
