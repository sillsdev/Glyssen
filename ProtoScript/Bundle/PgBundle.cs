using SIL.DblBundle.Text;

namespace ProtoScript.Bundle
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
