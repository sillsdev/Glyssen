using GlyssenEngine;
using GlyssenEngine.Script;
using NUnit.Framework;
using SIL.Xml;
using static GlyssenEngineTests.XmlComparisonTestUtils;

namespace GlyssenEngineTests
{
	class BiblicalAuthorsTests
	{
		[Test]
		public void Deserialize_Serialize_Roundtrip()
		{
			const string startingAndExpectedXml = @"<BiblicalAuthors>
	<Author name=""Peter"">
		<Books>
			<Book>1PE</Book>
			<Book>2PE</Book>
		</Books>
	</Author>
	<Author name=""Obadiah"">
		<Books>
			<Book>OBA</Book>
		</Books>
	</Author>
</BiblicalAuthors>";

			var deserializedBiblicalAuthors = XmlSerializationHelper.DeserializeFromString<BiblicalAuthors>(startingAndExpectedXml);
			Assert.That(deserializedBiblicalAuthors.Count, Is.EqualTo(2));
			var peter = deserializedBiblicalAuthors[0];
			Assert.That(peter.Name, Is.EqualTo("Peter"));
			Assert.That(peter.Books.Count, Is.EqualTo(2));
			Assert.That(peter.Books[0], Is.EqualTo("1PE"));
			Assert.That(peter.Books[1], Is.EqualTo("2PE"));
			var obadiah = deserializedBiblicalAuthors[1];
			Assert.That(obadiah.Name, Is.EqualTo("Obadiah"));
			Assert.That(obadiah.Books.Count, Is.EqualTo(1));
			Assert.That(obadiah.Books[0], Is.EqualTo("OBA"));

			AssertXmlEqual(startingAndExpectedXml, deserializedBiblicalAuthors.GetAsXml());
		}

		[Test]
		public void GetAuthorCount_OldTestament_Returns29()
		{
			Assert.That(BiblicalAuthors.GetAuthorCount(BookSetUtils.OldTestament.SelectedBookIds), Is.EqualTo(29));
		}

		[Test]
		public void GetAuthorCount_NewTestament_Returns9()
		{
			Assert.That(BiblicalAuthors.GetAuthorCount(BookSetUtils.NewTestament.SelectedBookIds), Is.EqualTo(9));
		}
	}
}
