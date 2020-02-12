using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Text;
using System.Windows.Forms;
using Glyssen.Shared;
using GlyssenEngine.Utilities;
using L10NSharp;
using SIL.Reporting;
using SIL.Windows.Forms;
using static System.String;

namespace Glyssen.Utilities
{
	public class WinFormsFontRepositoryAdapter : IFontRepository
	{
		public bool IsFontInstalled(string fontFamilyIdentifier)
		{
			return FontHelper.FontInstalled(fontFamilyIdentifier);
		}

		public bool DoesTrueTypeFontFileContainFontFamily(string ttfFile, string fontFamilyIdentifier)
		{
			using (PrivateFontCollection fontCol = new PrivateFontCollection())
			{
				fontCol.AddFontFile(ttfFile);
				return (fontCol.Families[0].Name == fontFamilyIdentifier);
			}
		}

		public void TryToInstall(string fontFamilyIdentifier, IReadOnlyCollection<string> ttfFiles)
		{
			if (ttfFiles.Count > 1)
				MessageBox.Show(
					Format(
						LocalizationManager.GetString("Font.InstallInstructionsMultipleStyles",
							"The font ({0}) used by this project has not been installed on this computer. We will now launch multiple font preview windows, one for each font style. In the top left of each window, click Install. After installing all the styles, you will need to restart {1} to make use of the font."),
						fontFamilyIdentifier, GlyssenInfo.Product));
			else
				MessageBox.Show(
					Format(
						LocalizationManager.GetString("Font.InstallInstructions",
							"The font used by this project ({0}) has not been installed on this computer. We will now launch a font preview window. In the top left, click Install. After installing the font, you will need to restart {1} to make use of it."),
						fontFamilyIdentifier, GlyssenInfo.Product));

			foreach (var ttfFile in ttfFiles)
			{
				try
				{
					Process.Start(ttfFile);
				}
				catch (Exception ex)
				{
					Logger.WriteError("There was a problem launching the font preview. Please install the font manually:" + ttfFile, ex);
					MessageBox.Show(Format(LocalizationManager.GetString("Font.UnableToLaunchFontPreview",
						"There was a problem launching the font preview. Please install the font manually. {0}"), ttfFile));
				}
			}
		}

		public void ReportMissingFontFamily(string fontFamilyIdentifier)
		{
			MessageBox.Show(Format(LocalizationManager.GetString("Font.FontFilesNotFound",
				"The font ({0}) used by this project has not been installed on this computer, and {1} could not find the relevant font files. " +
				"Either they were not copied from the bundle correctly, or they have been moved. You will need to install {0} yourself. " +
				"After installing the font, you will need to restart {1} to make use of it."),
				fontFamilyIdentifier, GlyssenInfo.Product));
		}
	}
}
