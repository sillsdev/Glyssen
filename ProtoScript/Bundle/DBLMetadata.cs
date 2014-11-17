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
		private string m_promoVersionInfo;

		[XmlAnyElement]
		public XmlElement[] PromoVersionInfoNodes { get; set; }

		[XmlIgnore]
		public string promoVersionInfo
		{
			get
			{
				if (m_promoVersionInfo == null)
				{
					var sb = new StringBuilder();
					foreach (var node in PromoVersionInfoNodes)
						sb.Append(node.InnerXml);
					m_promoVersionInfo = sb.ToString();
				}
				return m_promoVersionInfo;
			}
		}
		public string promoEmail;
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
