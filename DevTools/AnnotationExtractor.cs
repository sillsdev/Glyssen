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
		// This class is basically useless now as the annotations are now handled in ReferenceTextUtility.
		// It turns out that the project from Amgad does not seem to have all the annotations (Mark 4:14, for example)
		public static void ExtractAll()
		{
			ExtractAnnotationsMarkedWithSTag();

			// Don't really need this anymore as the F8 ones are all handled directly in ReferenceTextUtility
			//ExtractAnnotationsMarkedWithRqTag();
		}

		private static void ExtractAnnotationsMarkedWithSTag()
		{
			var directoryWithXmlFiles = @"C:\Users\Polk\Documents\Protoscript Generator\bundles\English Director Guide\WithApostrophes\English DG 9 apo\English DG 9 apo";

			var bookList = Directory.GetFiles(directoryWithXmlFiles, "*.xml").Select(XmlSerializationHelper.DeserializeFromFile<BookScript>).ToList();

			Block prevRealBlock = null;
			foreach (var book in bookList.OrderBy(b => BCVRef.BookToNumber(b.BookId)))
			{
				var blocks = book.GetScriptBlocks();
				for (int i = 0; i < blocks.Count; i++)
				{
					var block = blocks[i];
					if (block.StyleTag == "s")
					{
						int lastVerseOfPreviousBlock = 0;
						if (prevRealBlock != null)
							lastVerseOfPreviousBlock = prevRealBlock.LastVerse;
						string firstVerseOfNextRealBlock = null;
						int j = 1;
						while (i + j < blocks.Count)
						{
							if (blocks[i + j].StyleTag != "s")
							{
								firstVerseOfNextRealBlock = blocks[i + j].InitialVerseNumberOrBridge;
								break;
							}
							j++;
						}
						Debug.WriteLine(book.BookId + "\t" + block.ChapterNumber + "\t" + lastVerseOfPreviousBlock + "\t" + firstVerseOfNextRealBlock + "\t" + block.GetText(true));
					}
					else
						prevRealBlock = block;
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
