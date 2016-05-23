using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
				foreach (var block in book.Blocks)
					if (block.StyleTag == "s")
						Debug.WriteLine(book.BookId + "\t" + block.ChapterNumber + "\t" + (block.InitialStartVerseNumber + 1) + "\t" + block.GetText(true));
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

		public static bool ConvertTextToScriptAnnotationElement(string text, out ScriptAnnotation annotation)
		{
//			var pauseRegex = new Regex("||| \\+(?:/d) |||");
//			var match = pauseRegex.Match(text);
//			if (match.Success)
//				return new Pause { Seconds = int.Parse(match.Groups[0].Value) };
//
//			var musicEndRegex = new Regex("{Music--Ends before v(\\d*)}");
//			var match = musicEndRegex.Match(text);
//			if (match.Success)
//			{
//				annotation = new Sound { EndVerse = int.Parse(match.Groups[1].Value) };
//				return true;
//			}
//
//			var musicStartRegex = new Regex("{Music--Starts @ v(\\d*)}");
//			match = musicStartRegex.Match(text);
//			if (match.Success)
//			{
//				annotation = new Sound { StartVerse = int.Parse(match.Groups[1].Value) };
//				return true;
//			}
//
//			var sfxStartRegex = new Regex("{SFX--(.*)--Starts @ v(\\d*)}");
//			match = sfxStartRegex.Match(text);
//			if (match.Success)
//			{
//				annotation = new Sound { EffectName = match.Groups[1].Value, StartVerse = int.Parse(match.Groups[2].Value) };
//				return true;
//			}

			var sfxRegex = new Regex("{F8 SFX--(.*)}");
			var match = sfxRegex.Match(text);
			if (match.Success)
			{
				annotation = new Sound { EffectName = match.Groups[1].Value, UserSpecifiesLocation = true };
				return true;
			}

			annotation = null;
			return false;

		}
	}
}
