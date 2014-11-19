using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Palaso.Xml;

namespace ProtoScript.Bundle
{
	[XmlRoot("DBLMetadata")]
	public class DblMetadata
	{
		[XmlAttribute]
		public string id;

		/// <summary>This is not part of the original DBL metadata. We add this when we parse the USX to create
		/// a script. If significant changes to the parser are made and the parser version in the program does
		/// not match the stored parser version, then we know to re-parse the original USX data.</summary>
		[XmlAttribute("usxparserversion")]
		public string PgUsxParserVersion;

		/// <summary>This is not part of the original DBL metadata. We add this when we parse the USX to create
		/// a script. This tells us the original (local) path of the DBL file used to create this project.</summary>
		[XmlAttribute("origdblpath")]
		public string OriginalPathOfDblFile;

		public DblMetadataIdentification identification;
		public DblMetadataLanguage language;
		public DblMetadataPromotion promotion;
		public DblMetadataArchiveStatus archiveStatus;

		public string GetAsXml()
		{
			return XmlSerializationHelper.SerializeToString(this);
		}
	}

	public class DblMetadataIdentification
	{
		public string name;
		public string nameLocal;

		[XmlElement("systemId")]
		public HashSet<DblMetadataSystemId> systemIds;
	}

	public class DblMetadataLanguage
	{
		public string iso;
		public string name;
		public string ldml;
		public string rod;
		public string script;
		public string scriptDirection;
		public string numerals;

		public override string ToString()
		{
			return iso;
		}
	}

	public class DblMetadataPromotion
	{
		[XmlElement("promoVersionInfo")]
		public DblMetadataXhtmlContentNode promoVersionInfo;

		[XmlElement("promoEmail")]
		public DblMetadataXhtmlContentNode promoEmail;
	}

	public class DblMetadataXhtmlContentNode
	{
		private string m_value;

		public DblMetadataXhtmlContentNode()
		{
			contentType = "xhtml";
		}

		[XmlAttribute]
		public string contentType;

		[XmlAnyElement]
		public XmlElement[] InternalNodes { get; set; }

		[XmlIgnore]
		public string value
		{
			get
			{
				if (m_value == null)
				{
					var sb = new StringBuilder();
					foreach (var node in InternalNodes)
						sb.Append(node.OuterXml);
					m_value = sb.ToString();
				}
				return m_value;
			}
			set
			{
				m_value = value;
				var doc = new XmlDocument();
				doc.LoadXml(value);
				InternalNodes = new[] { doc.DocumentElement };
			}
		}
	}

	public class DblMetadataSystemId
	{
		[XmlAttribute]
		public string type;

		[XmlText]
		public string value;
	}

	public class DblMetadataArchiveStatus
	{
		public string dateArchived;
		public string dateUpdated;
	}
}
