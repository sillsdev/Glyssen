using System;
using System.IO;
using System.Xml.Serialization;
using Glyssen.Shared;
using Glyssen.Shared.Bundle;
using GlyssenEngine.Bundle;
using NUnit.Framework;
using static GlyssenEngineTests.XmlComparisonTestUtils;

namespace GlyssenEngineTests.Bundle
{
	class GlyssenDblTextMetadataTests
	{
		private GlyssenDblTextMetadata m_metadata;
		private GlyssenDblTextMetadata m_metadataWithDeprecatedFields;

		private const string kTestXml =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<DBLMetadata id=""3b9fdc679b9319c3"" revision=""1"" mediatype=""text"" typeVersion=""1.3"">
	<projectStatus>
		<quoteSystemStatus>UserSet</quoteSystemStatus>
		<bookSelectionStatus>Reviewed</bookSelectionStatus>
	</projectStatus>
	<language>
		<iso>ach</iso>
		<fontFamily>Charis SIL</fontFamily>
		<fontSizeInPoints>12</fontSizeInPoints>
	</language>
</DBLMetadata>";

		private const string kTestWithDeprecatedFieldsXml =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<DBLMetadata id=""3b9fdc679b9319c3"" revision=""1"" mediatype=""text"" typeVersion=""1.3"">
	<fontFamily>Charis SIL</fontFamily>
	<fontSizeInPoints>12</fontSizeInPoints>
	<language>
		<iso>ach</iso>
	</language>
	<isBookSelectionUserConfirmed>true</isBookSelectionUserConfirmed>
</DBLMetadata>";

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			var xs = new XmlSerializer(typeof(GlyssenDblTextMetadata));
			using (TextReader reader = new StringReader(kTestXml))
				m_metadata = (GlyssenDblTextMetadata)xs.Deserialize(reader);
			using (TextReader reader = new StringReader(kTestWithDeprecatedFieldsXml))
				m_metadataWithDeprecatedFields = (GlyssenDblTextMetadata)xs.Deserialize(reader);
		}

		[Test]
		public void GetFontFamily()
		{
			Assert.That(m_metadata.FontFamily, Is.EqualTo("Charis SIL"));
			Assert.That(m_metadataWithDeprecatedFields.FontFamily, Is.EqualTo("Charis SIL"));
		}

		[Test]
		public void GetFontSizeInPoints()
		{
			Assert.That(m_metadata.FontSizeInPoints, Is.EqualTo(12));
			Assert.That(m_metadataWithDeprecatedFields.FontSizeInPoints, Is.EqualTo(12));
		}

		[Test]
		public void GetProjectSettingsStatus()
		{
			Assert.That(ProjectSettingsStatus.UnReviewed, Is.EqualTo(m_metadata.ProjectStatus.ProjectSettingsStatus));
			Assert.That(ProjectSettingsStatus.UnReviewed, Is.EqualTo(m_metadataWithDeprecatedFields.ProjectStatus.ProjectSettingsStatus));
		}

		[Test]
		public void GetQuoteSystemStatus()
		{
			Assert.That(QuoteSystemStatus.UserSet, Is.EqualTo(m_metadata.ProjectStatus.QuoteSystemStatus));
			Assert.That(QuoteSystemStatus.Unknown, Is.EqualTo(m_metadataWithDeprecatedFields.ProjectStatus.QuoteSystemStatus));
		}

		[Test]
		public void GetBookSelectionStatus()
		{
			Assert.That(BookSelectionStatus.Reviewed, Is.EqualTo(m_metadata.ProjectStatus.BookSelectionStatus));
			Assert.That(BookSelectionStatus.Reviewed, Is.EqualTo(m_metadataWithDeprecatedFields.ProjectStatus.BookSelectionStatus));
		}
		[Test]
		public void Serialize()
		{
			var metadata = new GlyssenDblTextMetadata
			{
				Id = "id",
				Revision = 1,
				Language = new GlyssenDblMetadataLanguage(),
				ReferenceTextType = ReferenceTextType.Russian,
				UniqueRecordingProjectId = new Guid("a6e56d00-5796-4dbc-b1a7-e78904ca7034")
			};

			const string expectedResult =
				@"<?xml version=""1.0"" encoding=""utf-16""?>
<DBLMetadata id=""id"" revision=""1"" modifieddate=""0001-01-01T00:00:00"" uniqueProjectId=""a6e56d00-5796-4dbc-b1a7-e78904ca7034""
    controlfileversion=""0"" referenceTextType=""Russian"">
    <language>
        <fontSizeInPoints>10</fontSizeInPoints>
    </language>
    <projectStatus>
        <assignCharacterMode />
        <quoteSystemDate>0001-01-01T00:00:00</quoteSystemDate>
    </projectStatus>
    <characterGroupGenerationPreferences />
    <projectDramatizationPreferences />
</DBLMetadata>";

			AssertXmlEqual(expectedResult, metadata.GetAsXml());
		}
	}
}
