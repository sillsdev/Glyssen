using System;
using System.IO;
using System.Xml.Serialization;
using SIL.Xml;

namespace ProtoScript
{
	[XmlRoot("ProtoscriptGeneratorMetadata")]
	public class ApplicationMetadata
	{
		private const string kFilename = "ApplicationMetadata.xml";

		[XmlAttribute("dataVersion")]
		public int DataVersion { get; set; }

		public static ApplicationMetadata Load(out Exception exception)
		{
			exception = null;

			if (!File.Exists(FilePath))
				return new ApplicationMetadata();

			var metadata = XmlSerializationHelper.DeserializeFromFile<ApplicationMetadata>(FilePath, out exception);
			return metadata;
		}

		public void Save()
		{
			XmlSerializationHelper.SerializeToFile(FilePath, this);
		}

		private static string FilePath
		{
			get { return Path.Combine(Project.ProjectsBaseFolder, kFilename); }
		}
	}
}
