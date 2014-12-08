using NUnit.Framework;
using Palaso.Xml;
using ProtoScript;
using Rhino.Mocks;

namespace ProtoScriptTests
{
	[TestFixture]
	class CharacterAssignerTests
	{
		private BookScript m_bookScript;

		[SetUp]
		public void SetUp()
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
			var cvInfo = MockRepository.GenerateMock<ICharacterVerseInfo>();
			cvInfo.Stub(x => x.GetCharacters("MRK", 1, 4)).Return(new[] { new CharacterVerse { Character = "John the Baptist" } });
			new CharacterAssigner(cvInfo).Assign(m_bookScript);
			Assert.AreEqual("Made Up Guy", m_bookScript.ScriptBlocks[1].CharacterId);
		}

		[Test]
		public void Assign_OverwriteUserConfirmedTrue_DoesOverwrite()
		{
			var cvInfo = MockRepository.GenerateMock<ICharacterVerseInfo>();
			cvInfo.Stub(x => x.GetCharacters("MRK", 1, 4)).Return(new[] { new CharacterVerse { Character = "John the Baptist" } });
			new CharacterAssigner(cvInfo).Assign(m_bookScript, true);
			Assert.AreEqual("John the Baptist", m_bookScript.ScriptBlocks[1].CharacterId);
		}

		[Test]
		public void Assign_BlockIsStandardCharacter_DoesNotOverwrite()
		{
			var cvInfo = MockRepository.GenerateMock<ICharacterVerseInfo>();
			cvInfo.Stub(x => x.GetCharacters("MRK", 1, 4)).Return(new[] { new CharacterVerse { Character = "John the Baptist" } });
			new CharacterAssigner(cvInfo).Assign(m_bookScript, true);
			Assert.AreEqual("narrator-MRK", m_bookScript.ScriptBlocks[0].CharacterId);
		}
	}
}
