using SIL.DblBundle.Text;

namespace Glyssen.Bundle
{
	public class GlyssenBundle : TextBundle<GlyssenDblTextMetadata, GlyssenDblMetadataLanguage>
	{
		static GlyssenBundle()
		{
			DefaultLanguageIsoCode = SIL.WritingSystems.WellKnownSubtags.UnlistedLanguage;
		}

		public GlyssenBundle(string pathToZippedBundle) : base(pathToZippedBundle)
		{
			Metadata.OriginalPathBundlePath = BundlePath;
			Metadata.FontFamily = Stylesheet.FontFamily;
			Metadata.FontSizeInPoints = Stylesheet.FontSizeInPoints;
			if (string.IsNullOrEmpty(Metadata.Language.Iso))
				Metadata.Language.Iso = LanguageIso;
		}

		public string LanguageAsString { get { return Metadata.Language.ToString(); } }
	}
}
