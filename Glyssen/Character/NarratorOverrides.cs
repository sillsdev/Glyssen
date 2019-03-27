using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using Glyssen.Properties;
using SIL.Scripture;
using SIL.Xml;

namespace Glyssen.Character
{
	[XmlRoot("NarratorOverrides")]
	public class NarratorOverrides
	{
		private static NarratorOverrides s_singleton;
		private Dictionary<string, List<NarratorOverrideDetail>> m_dictionary;

		public static NarratorOverrides Singleton
		{
			get
			{
				if (s_singleton == null)
				{
					s_singleton = XmlSerializationHelper.DeserializeFromString<NarratorOverrides>(Resources.NarratorOverrides);
					s_singleton.m_dictionary = s_singleton.Books.ToDictionary(b => b.Id, b => b.Overrides);
					foreach (var book in s_singleton.Books)
					{
						var bookNum = BCVRef.BookToNumber(book.Id);
						foreach (var overrideDetail in book.Overrides.Where(o => o.EndVerse == 0))
							overrideDetail.EndVerse = ScrVers.English.GetLastVerse(bookNum, overrideDetail.EndChapter);
					}
				}
				return s_singleton;
			}
		}

		public static List<NarratorOverrideDetail> GetNarratorOverridesForBook(string bookId)
		{
			return Singleton.m_dictionary.TryGetValue(bookId, out List<NarratorOverrideDetail> details) ? details : null;
		}

		public static IDictionary<string, List<NarratorOverrideDetail>> NarratorOverridesByBookId => Singleton.m_dictionary;

		[XmlElement(ElementName = "Book")]
		public List<BookNarratorOverrides> Books { get; set; }

		[XmlRoot("Book")]
		public class BookNarratorOverrides
		{
			[XmlAttribute("id")]
			public string Id { get; set; }

			[XmlElement(ElementName = "Override")]
			public List<NarratorOverrideDetail> Overrides { get; set; }
		}

		[XmlRoot("Override")]
		public class NarratorOverrideDetail
		{
			public NarratorOverrideDetail()
			{
				StartVerse = 1;
				StartBlock = 1;
			}

			[XmlAttribute("startChapter")]
			public int StartChapter { get; set; }

			[XmlAttribute("startVerse")]
			[DefaultValue(1)]
			public int StartVerse { get; set; }

			[XmlAttribute("startBlock")]
			[DefaultValue(1)]
			public int StartBlock { get; set; }

			private int? m_endChapter;
			[XmlAttribute("endChapter")]
			public int EndChapter
			{
				get => m_endChapter ?? StartChapter;
				set => m_endChapter = value;
			}

			[XmlAttribute("endVerse")]
			public int EndVerse { get; set; }

			[XmlAttribute("character")]
			public string Character { get; set; }

			private string StartBlockAsSegmentLetter(bool suppressSegmentA = true)
			{
				if (suppressSegmentA && StartBlock == 1)
					return string.Empty;
				return ((char)('a' + StartBlock - 1)).ToString();
			}

			public override string ToString()
			{
				return $"{StartChapter}:{StartVerse}{StartBlockAsSegmentLetter()}-{(EndChapter == StartChapter ? null : EndChapter + ":")}{EndVerse}, {Character}";
			}
		}
	}
}
