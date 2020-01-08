using System;

namespace GlyssenEngine.Utilities
{
    public interface IFonts
    {
        void InstallIfNecessary(string fontFamily, string languageFolder, bool fontInstallationAttempted);
    }

    public static class Fonts
    {
        private static IFonts s_fonts;

        public static IFonts Default
        {
            get
            {
                if (s_fonts == null)
                    throw new InvalidOperationException("Not Initialized. Set Fonts.Default first.");
                return s_fonts;
            }
            set => s_fonts = value;
        }

        public static void InstallIfNecessary(string fontFamily, string languageFolder, bool fontInstallationAttempted)
        {
            Default.InstallIfNecessary(fontFamily, languageFolder, fontInstallationAttempted);
        }
    }
}
