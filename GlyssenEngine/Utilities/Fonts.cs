using System;

namespace GlyssenEngine.Utilities
{
     public interface IFonts
    {
        /// <summary>
        /// <c>InstallIfNecessary</c> tests to see if an attempt has already been made to install the font,
        /// and if not, will install the font from the folder provided.
        /// </summary>
        /// <param fontFamily="The font family to be installed"></param>
        /// <param languageFolder="The folder that contains all available fonts"></param>
        /// <param fontInstallationAttempted="A flag that indicates if an attempt has already been made to instal a font"></param>
        /// <returns></returns>
        void InstallIfNecessary(string fontFamily, string languageFolder, ref bool fontInstallationAttempted);
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

        public static void InstallIfNecessary(string fontFamily, string languageFolder, ref bool fontInstallationAttempted)
        {
            Default.InstallIfNecessary(fontFamily, languageFolder, ref fontInstallationAttempted);
        }
    }
}
