using System.Collections.Generic;
using System.Xml.Serialization;
using SIL.Scripture;

namespace Glyssen
{
	[XmlInclude(typeof(ScriptText))]
	[XmlInclude(typeof(Verse))]
	public abstract class BlockElement
	{
		public virtual BlockElement Clone()
		{
			return (BlockElement)MemberwiseClone();
		}
	}

	public class BlockElementContentsComparer : IEqualityComparer<BlockElement>
	{
		public bool Equals(BlockElement x, BlockElement y)
		{
			var xAsVerse = x as Verse;
			if (xAsVerse != null)
			{
				var yAsVerse = y as Verse;
				return yAsVerse != null && xAsVerse.Number == yAsVerse.Number;
			}

			var yAsScriptText = y as ScriptText;
			return yAsScriptText != null && ((ScriptText)x).Content == yAsScriptText.Content;
		}

		public int GetHashCode(BlockElement obj)
		{
			return obj.GetHashCode();
		}
	}

	public class ScriptText : BlockElement
	{
		public ScriptText()
		{
			// Needed for deserialization
		}

		public ScriptText(string content)
		{
			Content = content;
		}

		[XmlText]
		public string Content { get; set; }
	}

	public class Verse : BlockElement
	{
		public Verse()
		{
			// Needed for deserialization
		}

		public Verse(string number)
		{
			Number = number;
		}

		[XmlAttribute("num")]
		public string Number { get; set; }

		/// <summary>
		/// Gets the verse number as an integer. If the Verse number represents a verse bridge, this will be the
		/// starting number in the bridge.
		/// </summary>
		public int StartVerse
		{
			get { return ScrReference.VerseToIntStart(Number); }
		}

		/// <summary>
		/// Gets the verse number as an integer. If the Verse number represents a verse bridge, this will be the
		/// ending number in the bridge.
		/// </summary>
		public int EndVerse
		{
			get { return ScrReference.VerseToIntEnd(Number); }
		}
	}
}
