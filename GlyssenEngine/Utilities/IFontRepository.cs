using System.Collections.Generic;

namespace GlyssenEngine.Utilities
{
     public interface IFontRepository
     {
	     bool IsFontInstalled(string fontFamilyIdentifier);
	     bool DoesTrueTypeFontFileContainFontFamily(string ttfFile, string fontFamilyIdentifier);
	     void TryToInstall(string fontFamilyIdentifier, IReadOnlyCollection<string> ttfFile);
	     void ReportMissingFontFamily(string fontFamilyIdentifier);
     }
}
