using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Text;
using System.IO;
using Glyssen.Shared;
using GlyssenEngine.Utilities;
using SIL;
using SIL.Reporting;
using SIL.Windows.Forms;
using static System.String;

namespace Glyssen.Utilities
{
	public class WinFormsFonts : IFonts
	{
		public void InstallIfNecessary(string fontFamily, string languageFolder, ref bool fontInstallationAttempted)
		{
			if (fontInstallationAttempted || FontHelper.FontInstalled(fontFamily))
				return;

			List<string> ttfFilesToInstall = new List<string>();
			// There could be more than one if different styles (Regular, Italics, etc.) are in different files
			foreach (var ttfFile in Directory.GetFiles(languageFolder, "*.ttf"))
			{
				using (PrivateFontCollection fontCol = new PrivateFontCollection())
				{
					fontCol.AddFontFile(ttfFile);
					if (fontCol.Families[0].Name == fontFamily)
						ttfFilesToInstall.Add(ttfFile);
				}
			}
			int count = ttfFilesToInstall.Count;
			if (count > 0)
			{
				fontInstallationAttempted = true;

				if (count > 1)
					MessageModal.Show(
						Format(
							Localizer.GetString("Font.InstallInstructionsMultipleStyles",
								"The font ({0}) used by this project has not been installed on this computer. We will now launch multiple font preview windows, one for each font style. In the top left of each window, click Install. After installing all the styles, you will need to restart {1} to make use of the font."),
							fontFamily, GlyssenInfo.kProduct));
				else
					MessageModal.Show(
						Format(
							Localizer.GetString("Font.InstallInstructions",
								"The font used by this project ({0}) has not been installed on this computer. We will now launch a font preview window. In the top left, click Install. After installing the font, you will need to restart {1} to make use of it."),
							fontFamily, GlyssenInfo.kProduct));

				foreach (var ttfFile in ttfFilesToInstall)
				{
					try
					{
						Process.Start(ttfFile);
					}
					catch (Exception ex)
					{
						Logger.WriteError("There was a problem launching the font preview. Please install the font manually:" + ttfFile, ex);
						MessageModal.Show(
							Format(
								Localizer.GetString("Font.UnableToLaunchFontPreview",
									"There was a problem launching the font preview. Please install the font manually. {0}"), ttfFile));
					}
				}
			}
			else
				MessageModal.Show(
					Format(
						Localizer.GetString("Font.FontFilesNotFound",
							"The font ({0}) used by this project has not been installed on this computer, and {1} could not find the relevant font files. Either they were not copied from the bundle correctly, or they have been moved. You will need to install {0} yourself. After installing the font, you will need to restart {1} to make use of it."),
						fontFamily, GlyssenInfo.kProduct));
		}
	}
}
