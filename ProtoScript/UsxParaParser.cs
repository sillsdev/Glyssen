using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ProtoScript
{
	public class UsxParaParser
	{
		private readonly XmlNodeList m_paraList;

		public UsxParaParser(XmlNodeList paraList)
		{
			m_paraList = paraList;
		}

		public IEnumerable<Block> Parse()
		{
			foreach (XmlNode para in m_paraList)
			{
				var usxPara = new UsxPara(para);
				var block = new Block(usxPara.StyleTag);
				// <verse number="1" style="v" />
				// Acakki me lok me kwena maber i kom Yecu Kricito, Wod pa Lubaŋa,
				// <verse number="2" style="v" />
				// <note caller="-" style="x"><char style="xo" closed="false">1.2: </char><char style="xt" closed="false">Mal 3.1</char></note>
				// kit ma gicoyo kwede i buk pa lanebi Icaya ni,</para>
				foreach (XmlNode node in usxPara.ChildNodes)
				{
					switch (node.Name)
					{
						case "verse":
							block.BlockElements.Add(new Verse(node.Attributes.GetNamedItem("number").Value));
							break;
						case "#text":
							block.BlockElements.Add(new ScriptText(node.InnerText));
							break;
					}

				}
				yield return block;
			}
		}
	}

	public class UsxPara
	{
		private readonly XmlNode m_paraNode;

		public UsxPara(XmlNode paraNode)
		{
			m_paraNode = paraNode;
		}

		public string StyleTag
		{
			get { return m_paraNode.Attributes.GetNamedItem("style").Value; }
		}

		public XmlNodeList ChildNodes
		{
			get { return m_paraNode.ChildNodes; }
		}
	}
}