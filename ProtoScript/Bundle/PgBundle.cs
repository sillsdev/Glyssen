using SIL.DblBundle.Text;

namespace Glyssen.Bundle
{
	public class PgBundle : TextBundle<PgDblTextMetadata, PgDblMetadataLanguage>
	{
		public PgBundle(string pathToZippedBundle) : base(pathToZippedBundle)
		{
			Metadata.OriginalPathOfDblFile = BundlePath;
			Metadata.FontFamily = Stylesheet.FontFamily;
			Metadata.FontSizeInPoints = Stylesheet.FontSizeInPoints;
		}
	}
}
