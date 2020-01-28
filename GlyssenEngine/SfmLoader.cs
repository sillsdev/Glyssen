using GlyssenEngine.Paratext;
using Paratext.Data;
using SIL.IO;

namespace GlyssenEngine
{
	public static class SfmLoader
	{
		private static ScrStylesheet s_usfmStyleSheet;

		public static ScrStylesheet GetUsfmScrStylesheet()
		{
			if (s_usfmStyleSheet != null)
				return s_usfmStyleSheet;
			string usfmStylesheetPath = FileLocationUtilities.GetFileDistributedWithApplication("sfm", "usfm.sty");
			return s_usfmStyleSheet = new ScrStylesheet(usfmStylesheetPath);
		}

		public static ScrStylesheetAdapter GetUsfmStylesheet()
		{
			return new ScrStylesheetAdapter(GetUsfmScrStylesheet());
		}
	}
}
