using NUnit.Framework;
using SIL.TestUtilities;
using SIL.Xml;
using Waxuquerque;
using Waxuquerque.Utilities;

namespace WaxuquerqueTests
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
			Assert.AreEqual(2, deserializedBiblicalAuthors.Count);
			var peter = deserializedBiblicalAuthors[0];
			Assert.AreEqual("Peter", peter.Name);
			Assert.AreEqual(2, peter.Books.Count);
			Assert.AreEqual("1PE", peter.Books[0]);
			Assert.AreEqual("2PE", peter.Books[1]);
			var obadiah = deserializedBiblicalAuthors[1];
			Assert.AreEqual("Obadiah", obadiah.Name);
			Assert.AreEqual(1, obadiah.Books.Count);
			Assert.AreEqual("OBA", obadiah.Books[0]);

			AssertThatXmlIn.String(startingAndExpectedXml).EqualsIgnoreWhitespace(deserializedBiblicalAuthors.GetAsXml());
		}

		[Test]
		public void GetAuthorCount_OldTestament_Returns29()
		{
			Assert.AreEqual(29, BiblicalAuthors.GetAuthorCount(BookSetUtils.OldTestament.SelectedBookIds));
		}

		[Test]
		public void GetAuthorCount_NewTestament_Returns9()
		{
			Assert.AreEqual(9, BiblicalAuthors.GetAuthorCount(BookSetUtils.NewTestament.SelectedBookIds));
		}
	}
}
