using System.Xml;

namespace ProtoScript.Bundle
{
	public class UsxDocument
	{
		private readonly XmlDocument m_document;

		public UsxDocument(XmlDocument document)
		{
			m_document = document;
		}

		public UsxDocument(string path)
		{
			m_document = new XmlDocument { PreserveWhitespace = true };
			m_document.Load(path);
		}

		public XmlNode GetBook()
		{
			return m_document.SelectSingleNode("//book");
		}

		public XmlNodeList GetParas()
		{
			return m_document.SelectNodes("//para");
		}

		public XmlNodeList GetChaptersAndParas()
		{
			return m_document.SelectNodes("//para | //chapter");
		}
	}
}
