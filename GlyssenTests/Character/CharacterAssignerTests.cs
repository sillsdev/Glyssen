using Glyssen;
using Glyssen.Character;
using NUnit.Framework;
using Rhino.Mocks;
using SIL.Scripture;
using SIL.Xml;
using ScrVers = Paratext.ScrVers;

namespace GlyssenTests.Character
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
  <block style=""p"" chapter=""1"" initialStartVerse=""4"" characterId=""narrator-MRK"" userConfirmed=""false"">
    <verse num=""4"" />
    <text>Mantsa tama, ka zlagaptá Yuhwana, mnda maga Batem ma mtak, kaʼa mantsa: </text>
  </block>
  <block style=""p"" chapter=""1"" initialStartVerse=""4"" characterId=""Made Up Guy"" userConfirmed=""true"">
    <text>«Mbəɗanafwa mbəɗa ta nzakwa ghuni, ka magaghunafta lu ta batem, ka plighunista Lazglafta ta dmakuha ghuni,» </text>
  </block>
</book>";

			m_bookScript = XmlSerializationHelper.DeserializeFromString<BookScript>(bookScript);
		}

		[Test]
		public void Assign_OverwriteUserConfirmedFalse_DoesNotOverwrite()
		{
			var cvInfo = MockRepository.GenerateMock<ICharacterVerseInfo>();
			cvInfo.Stub(x => x.GetCharacters("MRK", 1, 4)).Return(new[] { new CharacterVerse(new BCVRef(41, 1, 4), "John the Baptist", null, null, false) });
			new CharacterAssigner(cvInfo).Assign(m_bookScript, ScrVers.English);
			Assert.AreEqual("Made Up Guy", m_bookScript[1].CharacterId);
		}

		[Test]
		public void Assign_OverwriteUserConfirmedTrue_DoesOverwrite()
		{
			var cvInfo = MockRepository.GenerateMock<ICharacterVerseInfo>();
			cvInfo.Stub(x => x.GetCharacters("MRK", 1, 4, 0, 4, ScrVers.English)).Return(new[] { new CharacterVerse(new BCVRef(41, 1, 4), "John the Baptist", null, null, false) });
			new CharacterAssigner(cvInfo).Assign(m_bookScript, ScrVers.English, true);
			Assert.AreEqual("John the Baptist", m_bookScript[1].CharacterId);
		}

		[Test]
		public void Assign_BlockIsStandardCharacter_DoesNotOverwrite()
		{
			var cvInfo = MockRepository.GenerateMock<ICharacterVerseInfo>();
			cvInfo.Stub(x => x.GetCharacters("MRK", 1, 4, 0, 4, ScrVers.English)).Return(new[] { new CharacterVerse(new BCVRef(41, 1, 4), "John the Baptist", null, null, false) });
			new CharacterAssigner(cvInfo).Assign(m_bookScript, ScrVers.English, true);
			Assert.AreEqual("narrator-MRK", m_bookScript[0].CharacterId);
		}
	}
}
