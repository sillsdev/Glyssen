using NUnit.Framework;
using ProtoScript;
using SIL.Xml;

namespace ProtoScriptTests
{
	[TestFixture]
	class UsxStylesheetTests
	{
		private const string xml = @"
<?xml version=""1.0"" encoding=""utf-8""?>
<stylesheet>
  <property name=""font-family"">Charis SIL</property>
  <property name=""font-size"" unit=""pt"">12</property>
  <style id=""h"" publishable=""true"" versetext=""false"">
    <name>h - File - Header</name>
    <description>Running header text for a book (basic)</description>
    <property name=""text-align"">left</property>
  </style>
</stylesheet>";

		[Test]
		public void Deserialize_FontFamilyAndFontSize()
		{
			var stylesheet = XmlSerializationHelper.DeserializeFromString<UsxStylesheet>(xml);
			Assert.AreEqual("Charis SIL", stylesheet.FontFamily);
			Assert.AreEqual(12, stylesheet.FontSizeInPoints);
		}
	}
}
