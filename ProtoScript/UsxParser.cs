using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ProtoScript.Bundle;

namespace ProtoScript
{
	public class UsxParser
	{
		private readonly XmlNodeList m_nodeList;

		public UsxParser(XmlNodeList nodeList)
		{
			m_nodeList = nodeList;
		}

		public IEnumerable<Block> Parse()
		{
			foreach (XmlNode node in m_nodeList)
			{
				Block block = null;
				switch (node.Name)
				{
					case "chapter":
						var usxChapter = new UsxChapter(node);
						block = new Block(usxChapter.StyleTag);
						block.BlockElements.Add(new ScriptText(usxChapter.ChapterNumber));
						break;
					case "para":
						var usxPara = new UsxPara(node);
						block = new Block(usxPara.StyleTag);
						// <verse number="1" style="v" />
						// Acakki me lok me kwena maber i kom Yecu Kricito, Wod pa Lubaŋa,
						// <verse number="2" style="v" />
						// <note caller="-" style="x"><char style="xo" closed="false">1.2: </char><char style="xt" closed="false">Mal 3.1</char></note>
						// kit ma gicoyo kwede i buk pa lanebi Icaya ni,</para>
						foreach (XmlNode childNode in usxPara.ChildNodes)
						{
							switch (childNode.Name)
							{
								case "verse":
									block.BlockElements.Add(new Verse(childNode.Attributes.GetNamedItem("number").Value));
									break;
								case "#text":
									block.BlockElements.Add(new ScriptText(childNode.InnerText));
									break;
							}
						}
						break;
				}
				yield return block;
			}
		}
	}
}