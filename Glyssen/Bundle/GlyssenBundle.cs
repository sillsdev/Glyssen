using SIL.DblBundle.Text;

namespace Glyssen.Bundle
{
	public class GlyssenBundle : TextBundle<GlyssenDblTextMetadata, GlyssenDblMetadataLanguage>
	{
		public GlyssenBundle(string pathToZippedBundle) : base(pathToZippedBundle)
		{
			Metadata.OriginalPathOfDblFile = BundlePath;
			Metadata.FontFamily = Stylesheet.FontFamily;
			Metadata.FontSizeInPoints = Stylesheet.FontSizeInPoints;
		}
	}
}
