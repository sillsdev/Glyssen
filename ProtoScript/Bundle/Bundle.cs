using System.IO;
using System.Xml.Serialization;
using ICSharpCode.SharpZipLib.Zip;

namespace ProtoScript.Bundle
{
	public class Bundle
	{
		private DblMetadata m_dblMetadata;

		private Bundle() { }

		public string Id 
		{
			get { return m_dblMetadata.id; }
		} 
		
		public static Bundle Create(string pathToZippedBundle)
		{
			var bundle = new Bundle();

			string pathToUnzippedDirectory;
			Unzip(pathToZippedBundle, out pathToUnzippedDirectory);

			string metadataPath = Path.Combine(pathToUnzippedDirectory, "metadata.xml");

			var xs = new XmlSerializer(typeof(DblMetadata));
			bundle.m_dblMetadata = (DblMetadata)xs.Deserialize(new FileStream(metadataPath, FileMode.Open));

			return bundle;
		}

		private static void Unzip(string pathToZippedBundle, out string pathToUnzippedDirectory)
		{
			string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(tempPath);

			new FastZip().ExtractZip(pathToZippedBundle, tempPath, null);

			pathToUnzippedDirectory = tempPath;
		}
	}
}
