using SIL.DblBundle.Text;

namespace Glyssen.Bundle
{
	public class GlyssenBundle : TextBundle<GlyssenDblTextMetadata, GlyssenDblMetadataLanguage>
	{
		public GlyssenBundle(string pathToZippedBundle) : base(pathToZippedBundle)
		{
			Metadata.OriginalPathBundlePath = BundlePath;
			Metadata.FontFamily = Stylesheet.FontFamily;
			Metadata.FontSizeInPoints = Stylesheet.FontSizeInPoints;
		}

		public string LanguageAsString { get { return Metadata.Language.ToString(); } }
	}
}
