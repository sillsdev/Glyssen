using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using NUnit.Framework;
using Palaso.TestUtilities;
using ProtoScript.Bundle;

namespace ProtoScriptTests.Bundle
{
	class DblMetadataTests
	{
		private DblMetadata m_metadata;

		private const string TestXml =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<DBLMetadata id=""3b9fdc679b9319c3"" revision=""1"" mediatype=""text"" typeVersion=""1.3"">
  <identification>
    <name>Acholi New Testament 1985</name>
    <nameLocal>Acoli Baibul 1985</nameLocal>
    <systemId type=""tms"">b9236acd-66f3-44d0-98fc-3970b3d017cd</systemId>
    <systemId type=""paratext"">3b9fdc679b9319c3ee45ab86cc1c0c42930c2979</systemId>
  </identification>
  <language>
    <iso>ach</iso>
  </language>
  <promotion>    
    <promoVersionInfo contentType=""xhtml"">
      <h1>Acholi New Testament 1985</h1>
      <p>This translation, published by the Bible Society of Uganda, was first published in 1985.</p>
      <p>If you are interested in obtaining a printed copy, please contact the Bible Society of Uganda at <a href=""http://www.biblesociety-uganda.org/"">www.biblesociety-uganda.org</a>.</p>
    </promoVersionInfo>
    <promoEmail contentType=""xhtml"">
      <p>Hi YouVersion friend,</p>
      <p>Sincerely, Your Friends at YouVersion</p>
    </promoEmail>
  </promotion>
  <archiveStatus>
    <archivistName>Emma Canales -archivist</archivistName>
    <dateArchived>2014-05-28T15:18:31.080800</dateArchived>
    <dateUpdated>2014-05-28T15:18:31.080800</dateUpdated>
    <comments>First submit</comments>
  </archiveStatus>
</DBLMetadata>";

		[TestFixtureSetUp]
		public void TestFixtureSetup()
		{
			var xs = new XmlSerializer(typeof(DblMetadata));
			using (TextReader reader = new StringReader(TestXml))
				m_metadata = (DblMetadata)xs.Deserialize(reader);
		}

		[Test]
		public void GetId()
		{
			Assert.AreEqual("3b9fdc679b9319c3", m_metadata.id);
		}

		[Test]
		public void GetName()
		{
			Assert.AreEqual("Acholi New Testament 1985", m_metadata.identification.name);
		}

		[Test]
		public void GetParatextSystemId()
		{
			Assert.AreEqual("3b9fdc679b9319c3ee45ab86cc1c0c42930c2979", m_metadata.identification.systemIds.FirstOrDefault(sid => sid.type.Equals("paratext")).value);
		}

		[Test]
		public void GetLanguageIso()
		{
			Assert.AreEqual("ach", m_metadata.language.iso);
		}

		[Test]
		public void GetPromoVersionInfo()
		{
			const string expectedValue = @"<h1>Acholi New Testament 1985</h1><p>This translation, published by the Bible Society " +
				@"of Uganda, was first published in 1985.</p><p>If you are interested in obtaining a printed copy, please contact " +
				@"the Bible Society of Uganda at <a href=""http://www.biblesociety-uganda.org/"">www.biblesociety-uganda.org</a>.</p>";
			Assert.AreEqual(expectedValue, m_metadata.promotion.promoVersionInfo.value);
			Assert.AreEqual("xhtml", m_metadata.promotion.promoVersionInfo.contentType);
		}

		[Test]
		public void GetPromoEmail()
		{
			const string expectedValue = @"<p>Hi YouVersion friend,</p><p>Sincerely, Your Friends at YouVersion</p>";
			Assert.AreEqual(expectedValue, m_metadata.promotion.promoEmail.value);
		}

		[Test]
		public void GetDateArchived()
		{
			Assert.AreEqual("2014-05-28T15:18:31.080800", m_metadata.archiveStatus.dateArchived);
		}

		[Test]
		public void Serialize()
		{
			var metadata = new DblMetadata
			{
				id = "id",
				identification = new DblMetadataIdentification
				{
					name = "name",
					systemIds = new HashSet<DblMetadataSystemId> { new DblMetadataSystemId { type = "type", value = "value" } }
				},
				promotion = new DblMetadataPromotion
				{
					promoVersionInfo = new DblMetadataXhtmlContentNode { value = @"<h1>Acholi New Testament 1985</h1>" },
					promoEmail = new DblMetadataXhtmlContentNode { value = "<p>Email Text</p>" }
				},
				archiveStatus = new DblMetadataArchiveStatus { dateArchived = "dateArchived" }
			};

			const string expectedResult =
@"<?xml version=""1.0"" encoding=""utf-16""?>
<DBLMetadata id=""id"">
  <identification>
    <name>name</name>
    <systemId type=""type"">value</systemId>
  </identification>
  <promotion>
    <promoVersionInfo contentType=""xhtml"">
      <h1>Acholi New Testament 1985</h1>
    </promoVersionInfo>
    <promoEmail contentType=""xhtml"">
      <p>Email Text</p>
    </promoEmail>
  </promotion>
  <archiveStatus>
    <dateArchived>dateArchived</dateArchived>
  </archiveStatus>
</DBLMetadata>";

			AssertThatXmlIn.String(expectedResult).EqualsIgnoreWhitespace(metadata.GetAsXml());
		}
	}
}
