using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using Glyssen;
using SIL.DblBundle.Usx;
using SIL.Scripture;
using SIL.Xml;

namespace DevTools
{
	class AnnotationExtractor
	{
		public static void ExtractAll()
		{
			ExtractAnnotationsMarkedWithSTag();
			ExtractAnnotationsMarkedWithRqTag();
		}

		private static void ExtractAnnotationsMarkedWithSTag()
		{
			var directoryWithXmlFiles = @"C:\Users\Polk\Documents\Protoscript Generator\bundles\English Director Guide\WithApostrophes\English DG 9 apo\English DG 9 apo";

			var bookList = Directory.GetFiles(directoryWithXmlFiles, "*.xml").Select(XmlSerializationHelper.DeserializeFromFile<BookScript>).ToList();

			foreach (var book in bookList.OrderBy(b => BCVRef.BookToNumber(b.BookId)))
			{
				foreach (var block in book.Blocks)
				{
					if (block.StyleTag == "s")
						Debug.WriteLine(book.BookId + "\t" + block.ChapterNumber + "\t" + block.InitialStartVerseNumber + "\t" + block.GetText(true));
				}
			}
		}

		private static void ExtractAnnotationsMarkedWithRqTag()
		{
			var directoryWithUsxFiles = @"C:\Users\Polk\Documents\Protoscript Generator\bundles\English Director Guide\WithApostrophes\bundle_text_dbl-5453af4c7644271c_pt-ENGDG_20160308-122600_release\USX_0";

			var bookList = Directory.GetFiles(directoryWithUsxFiles, "*.usx").Select(f => new UsxDocument(f)).ToList();

			string currentChapter = "0";
			string currentVerse = "0";
			foreach (var usxDocument in bookList.OrderBy(b => BCVRef.BookToNumber(b.BookId)))
			{
				foreach (XmlNode node in usxDocument.GetChaptersAndParas())
				{
					if (node.Name == "chapter")
					{
						var usxChapter = new UsxChapter(node);
						currentChapter = usxChapter.ChapterNumber;
						currentVerse = "0";
					}
					foreach (XmlNode childNode in node.ChildNodes)
					{
						if (childNode.Name == "verse")
							currentVerse = childNode.Attributes.GetNamedItem("number").Value;
						if (childNode.Name == "char")
							if (childNode.Attributes["style"].Value == "rq")
								Debug.WriteLine(usxDocument.BookId + "\t" + currentChapter + "\t" + currentVerse + "\t" + node.InnerText.Trim());
					}
				}
			}
		}
	}
}
