using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Palaso.Xml;
using ProtoScript;

namespace ProtoScriptTests
{
	[TestFixture]
	class CharacterAssignerTests
	{
		private BookScript m_bookScript;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			const string bookScript = @"
<book id=""MRK"">
  <block style=""p"" chapter=""1"" verse=""4"" characterId=""narrator-MRK"" userConfirmed=""false"">
    <verse num=""4"" />
    <text>Mantsa tama, ka zlagaptá Yuhwana, mnda maga Batem ma mtak, kaʼa mantsa: </text>
  </block>
  <block style=""p"" chapter=""1"" verse=""4"" characterId=""Made Up Guy"" userConfirmed=""true"">
    <text>«Mbəɗanafwa mbəɗa ta nzakwa ghuni, ka magaghunafta lu ta batem, ka plighunista Lazglafta ta dmakuha ghuni,» </text>
  </block>
</book>";

			m_bookScript = XmlSerializationHelper.DeserializeFromString<BookScript>(bookScript);
		}

		[Test]
		public void Assign_OverwriteUserConfirmedFalse_DoesNotOverwrite()
		{
			new CharacterAssigner().Assign(m_bookScript);
			Assert.AreEqual("Made Up Guy", m_bookScript.ScriptBlocks[1].CharacterId);
		}

		[Test]
		public void Assign_OverwriteUserConfirmedTrue_DoesOverwrite()
		{
			new CharacterAssigner().Assign(m_bookScript, true);
			Assert.AreEqual("John the Baptist", m_bookScript.ScriptBlocks[1].CharacterId);
		}
	}
}
