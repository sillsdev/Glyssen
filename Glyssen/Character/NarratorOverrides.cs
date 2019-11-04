using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using Glyssen.Properties;
using SIL.Scripture;
using SIL.Xml;
using static System.Int32;

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

		public static IEnumerable<NarratorOverrideDetail> GetNarratorOverridesForBook(string bookId, ScrVers targetVersification = null)
		{
			if (!Singleton.m_dictionary.TryGetValue(bookId, out List<NarratorOverrideDetail> details))
				return new NarratorOverrideDetail[0];
			if (targetVersification == null || targetVersification == ScrVers.English)
				return details;

			var bookNum = BCVRef.BookToNumber(bookId);
			return details.Select(t => t.WithVersification(bookNum, targetVersification));
		}

		/// <summary>
		/// Gets the character to use in the script for a narrator block in the reference range of the given block. Note
		/// that this code does not bother to check whether the given block is actually a narrator block. Typically, there
		/// will only be one override character in the list, but if this is a verse that has an a/b split, then there can be
		/// two.
		/// </summary>
		public static IEnumerable<string> GetCharacterOverrideForBlock(int bookNum, Block block, ScrVers versification)
		{
			return GetCharacterOverrideDetailsForRefRange(block.StartRef(bookNum, versification), block.LastVerseNum)
				?.Select(d => d.Character);
		}

		public static IEnumerable<NarratorOverrideDetail> GetCharacterOverrideDetailsForRefRange(VerseRef startRef, int endVerse)
		{
			if (!ChangeToEnglishVersification(ref startRef, ref endVerse, out var endChapter))
				return new NarratorOverrideDetail[0];

			return GetNarratorOverridesForBook(startRef.Book).Where(o =>
				(o.StartChapter < startRef.ChapterNum || (o.StartChapter == startRef.ChapterNum && o.StartVerse <= startRef.VerseNum)) &&
				(o.EndChapter > endChapter || (o.EndChapter == endChapter && o.EndVerse >= endVerse)));
		}

		public static bool ChangeToEnglishVersification(ref VerseRef startRef, ref int endVerse, out int endChapter)
		{
			if (endVerse < startRef.VerseNum)
				throw new ArgumentOutOfRangeException(nameof(endVerse), "Range must be in a single chapter and end verse must be greater than start verse. " +
					$"Details: {nameof(startRef)} = {startRef}; {nameof(endVerse)} = {endVerse}");

			bool endAndStartAreSame = endVerse == startRef.VerseNum;
			VerseRef endRef = endAndStartAreSame ? startRef : new VerseRef(startRef) { VerseNum = endVerse };

			startRef.ChangeVersification(ScrVers.English);
			if (startRef.VerseNum == 0) // Currently, we don't support overriding verse 0 (Hebrew subtitle in Psalms) -- this allows us to define overrides with chapter ranges.
			{
				endChapter = -1;
				return false;
			}

			if (endAndStartAreSame) // Calling change versification is kind of expensive, so this is a helpful optimization
				endRef = startRef;
			else
				endRef.ChangeVersification(ScrVers.English);
			endVerse = endRef.VerseNum;
			endChapter = endRef.ChapterNum;
			return true;
		}

		public static IReadOnlyDictionary<string, List<NarratorOverrideDetail>> NarratorOverridesByBookId => Singleton.m_dictionary;

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
				StartBlock = 0;
				EndBlock = 0;
			}

			[XmlAttribute("startChapter")]
			public int StartChapter { get; set; }

			[XmlAttribute("startVerse")]
			[DefaultValue(1)]
			public int StartVerse { get; set; }

			[XmlAttribute("startBlock")]
			[DefaultValue(0)]
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

			[XmlAttribute("endBlock")]
			[DefaultValue(0)]
			public int EndBlock { get; set; }

			[XmlAttribute("character")]
			public string Character { get; set; }

			public bool StartsAndEndsInSameVerse => StartChapter == EndChapter && StartVerse == EndVerse;

			/// <summary>
			/// For this override's <see cref="EndVerse"/>, the maximum number of blocks (starting with the
			/// first one for the verse) to override. If no <see cref="EndBlock"/> is specified, then this
			/// property returns <see cref="MaxValue"/> since any narrator blocks that cover the verse should
			/// be overridden. If <see cref="EndBlock"/> is specified, then this property typically returns
			/// <see cref="EndBlock"/>. However, there is a special case when this is an override for a limited
			/// range of blocks within a single verse. For example, if the override starts at block 2 (the third
			/// block) of verse 6:9 and ends at block 4 (the fifth block), then it includes 3 blocks (the 3rd, 4th
			/// and 5th blocks). Of course, if the target language only has four blocks for verse 6:9, then the
			/// actual number of blocks that will be overridden is 2, assuming they are both narrator blocks.
			/// </summary>
			public int NumberOfBlocksIncludedInEndVerse => EndBlock == 0 ? MaxValue :
				EndBlock - (StartsAndEndsInSameVerse && StartBlock > 0 ? StartBlock - 1 : 0);

			private string StartBlockAsSegmentLetter(bool suppressSegmentA = true)
			{
				if (StartBlock == 0 || (suppressSegmentA && StartBlock == 1))
					return string.Empty;
				return ((char)('a' + StartBlock - 1)).ToString();
			}

			public override string ToString()
			{
				return $"{StartChapter}:{StartVerse}{StartBlockAsSegmentLetter()}-{(EndChapter == StartChapter ? null : EndChapter + ":")}{EndVerse}, {Character}";
			}

			public NarratorOverrideDetail WithVersification(int bookNum, ScrVers targetVersification)
			{
				var clone = (NarratorOverrideDetail)MemberwiseClone();
				var startRef = new VerseRef(bookNum, StartChapter, StartVerse, ScrVers.English);
				startRef.ChangeVersification(targetVersification);
				var endRef = new VerseRef(bookNum, EndChapter, EndVerse, ScrVers.English);
				endRef.ChangeVersification(targetVersification);
				clone.StartChapter = startRef.ChapterNum;
				clone.StartVerse = startRef.VerseNum;
				clone.EndChapter = endRef.ChapterNum;
				clone.EndVerse = endRef.VerseNum;
				return clone;
			}
		}
	}
}
