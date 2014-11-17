using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using NUnit.Framework;
using Palaso.TestUtilities;
using ProtoScript;

namespace ProtoScriptTests
{
	[TestFixture]
	class BlockTests
	{
		[Test]
		public void Serialization_VerseAndTextElements_Works()
		{
			var block = new Block("p");
			block.BlockElements.Add(new Verse("1"));
			block.BlockElements.Add(new ScriptText("Text of verse one."));
			block.BlockElements.Add(new Verse("2"));
			block.BlockElements.Add(new ScriptText("Text of verse two."));

			AssertThatXmlIn.String("<?xml version=\"1.0\" encoding=\"utf-16\"?><block style=\"p\" characterId=\"0\">" +
				"<verse num=\"1\"/>" +
				"<text>Text of verse one.</text>" +
				"<verse num=\"2\"/>" +
				"<text>Text of verse two.</text>" +
				"</block>")
				.EqualsIgnoreWhitespace(block.GetAsXml());
		}
	}
}
