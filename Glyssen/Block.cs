﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.Serialization;
using Glyssen.Character;
using Glyssen.Dialogs;
using Glyssen.Utilities;
using SIL.Scripture;
using SIL.Xml;

namespace Glyssen
{
	[XmlRoot("block")]
	public class Block
	{
		/// <summary>Blocks which has not yet been parsed to identify contents/character</summary>
		public const string kNotSet = null;

		public const int kNotSplit = -1;

		public const string kCssFrame = "body{{font-family:{0};font-size:{1}pt}}" +
						".right-to-left{{direction:rtl}}" +
						".scripttext {{display:inline}}";

		public static Func<string /* Book ID */, int /*Chapter Number*/, string> FormatChapterAnnouncement;

		public const string kSplitElementIdPrefix = "split";
		private const string kSplitLineFrame = "<div id=\"" + kSplitElementIdPrefix + "{0}\" class=\"split-line\"><div class=\"split-line-top\"></div></div>";

		/// <summary>Random string which will (hopefully) never appear in real text</summary>
		private const string kAwooga = "^~^";

		private int m_initialStartVerseNumber;
		private int m_initialEndVerseNumber;
		private int m_chapterNumber;
		private string m_characterIdInScript;
		private string m_delivery;
		private static string s_characterSelect;

		public Block()
		{
			// Needed for deserialization
			SplitId = kNotSplit;
			// We would really prefer to have this be null, but in deserializing,
			// it always creates an empty list, so it's simpler to be able to assume that it's always non-null.
			ReferenceBlocks = new List<Block>();
		}

		public Block(string styleTag, int chapterNum = 0, int initialStartVerseNum = 0, int initialEndVerseNum = 0) : this()
		{
			StyleTag = styleTag;
			BlockElements = new List<BlockElement>();
			ChapterNumber = chapterNum;
			InitialStartVerseNumber = initialStartVerseNum;
			InitialEndVerseNumber = initialEndVerseNum;
		}

		public Block Clone()
		{
			var newBlock = (Block)MemberwiseClone();
			newBlock.BlockElements = new List<BlockElement>(BlockElements.Count);
			foreach (var blockElement in BlockElements)
				newBlock.BlockElements.Add(blockElement.Clone());
			return newBlock;
		}

		[XmlAttribute("style")]
		public string StyleTag { get; set; }

		[XmlAttribute("paragraphStart")]
		[DefaultValue(false)]
		public bool IsParagraphStart { get; set; }

		[XmlAttribute("chapter")]
		public int ChapterNumber
		{
			get
			{
				if (m_chapterNumber == 0)
				{
					if (InitialStartVerseNumber > 0 || BlockElements.Any(b => b is Verse))
						m_chapterNumber = 1;
				}
				return m_chapterNumber;
			}
			set { m_chapterNumber = value; }
		}

		/// <summary>
		/// This is only set for chapter announcement blocks
		/// </summary>
		[XmlAttribute("book")]
		public string BookCode { get; set; }

		[XmlAttribute("initialStartVerse")]
		public int InitialStartVerseNumber
		{
			get
			{
				if (m_initialStartVerseNumber == 0)
				{
					var leadingVerseElement = BlockElements.FirstOrDefault() as Verse;
					if (leadingVerseElement != null)
					{
						m_initialStartVerseNumber = leadingVerseElement.StartVerse;
					}
					else if (BlockElements.Any(b => b is Verse))
					{
						m_initialStartVerseNumber = 1;
					}
				}
				return m_initialStartVerseNumber;
			}
			set { m_initialStartVerseNumber = value; }
		}

		public int LastVerse
		{
			get
			{
				var lastVerse = BlockElements.OfType<Verse>().LastOrDefault();
				if (lastVerse == null)
					return m_initialEndVerseNumber > 0 ? m_initialEndVerseNumber : m_initialStartVerseNumber;
				return lastVerse.EndVerse;
			}
		}

		[XmlAttribute("initialEndVerse")]
		[DefaultValue(0)]
		public int InitialEndVerseNumber {
			get { return m_initialEndVerseNumber; }
			set { m_initialEndVerseNumber = m_initialStartVerseNumber == value ? 0 : value; }
		}

		/// <summary>
		/// This is the character ID assigned by Glyssen or selected by the user during Phase 1 (protoscript).
		/// Do not use this in Phase 2 (actor assignment); Instead, use CharacterIdInScript.
		/// This setter does not update the CharacterIdInScript value. Therefore, it can be used for
		/// deserialization, cloning (e.g., when applying user decisions), and setting character IDs that
		/// are guaranteed not to represent multiple characters (e.g., Standard characters). In other contexts,
		/// use the SetCharacterIdAndCharacterIdInScript method.
		/// </summary>
		[XmlAttribute("characterId")]
		public string CharacterId { get; set; }

		[XmlAttribute("characterIdOverrideForScript")]
		public string CharacterIdOverrideForScript
		{
			get { return m_characterIdInScript; }
			set { CharacterIdInScript = value; }
		}

		[XmlIgnore]
		public string CharacterIdInScript
		{
			get { return m_characterIdInScript ?? CharacterId; }
			set { if (CharacterId != value) m_characterIdInScript = value; }
		}

		[XmlAttribute("delivery")]
		public string Delivery
		{
			get { return m_delivery; }
			set
			{
				if (string.IsNullOrWhiteSpace(value))
					value = null;
				m_delivery = value;
			}
		}

		[XmlAttribute("userConfirmed")]
		[DefaultValue(false)]
		public bool UserConfirmed { get; set; }

		[XmlAttribute("multiBlockQuote")]
		[DefaultValue(MultiBlockQuote.None)]
		public MultiBlockQuote MultiBlockQuote { get; set; }

		[XmlAttribute("splitId")]
		[DefaultValue(-1)]
		public int SplitId { get; set; }

		[XmlAttribute("matchesReferenceText")]
		[DefaultValue(false)]
		public bool MatchesReferenceText { get; set; }

		public string PrimaryReferenceText
		{
			get { return MatchesReferenceText ? ReferenceBlocks[0].GetTextFromBlockElements(true, true) : null; }
		}

		[XmlElement]
		public List<Block> ReferenceBlocks { get; set; }

		[XmlElement(Type = typeof (ScriptText), ElementName = "text")]
		[XmlElement(Type = typeof(Verse), ElementName = "verse")]
		[XmlElement(Type = typeof(Sound), ElementName = "sound")]
		[XmlElement(Type = typeof(Pause), ElementName = "pause")]
		public List<BlockElement> BlockElements { get; set; }

		public bool CharacterIsStandard
		{
			get { return CharacterVerseData.IsCharacterStandard(CharacterId); }
		}

		public bool IsChapterAnnouncement
		{
			get { return StyleTag == "c" || StyleTag == "cl"; }
		}

		public void SetMatchedReferenceBlock(Block referenceBlock)
		{
			ReferenceBlocks = new List<Block> { referenceBlock };
			MatchesReferenceText = true;
		}

		public string GetText(bool includeVerseNumbers, bool includeAnnotations = false)
		{
			if (IsChapterAnnouncement && BookCode != null && FormatChapterAnnouncement != null)
				return FormatChapterAnnouncement(BookCode, ChapterNumber) ?? ((ScriptText)BlockElements.First()).Content;

			return GetTextFromBlockElements(includeVerseNumbers, includeAnnotations);
		}

		private string GetTextFromBlockElements(bool includeVerseNumbers, bool includeAnnotations = false)
		{
			StringBuilder bldr = new StringBuilder();

			foreach (var blockElement in BlockElements)
			{
				Verse verse = blockElement as Verse;
				if (verse != null)
				{
					if (includeVerseNumbers)
					{
						bldr.Append("{");
						bldr.Append(verse.Number);
						bldr.Append("}\u00A0");
					}
				}
				else
				{
					ScriptText text = blockElement as ScriptText;
					if (text != null)
						bldr.Append(text.Content);
					else if (includeAnnotations)
					{
						ScriptAnnotation annotation = blockElement as ScriptAnnotation;
						if (annotation != null)
							bldr.Append(annotation.ToDisplay(" "));
					}
				}
			}

			return bldr.ToString();
		}

		public string InitialVerseNumberOrBridge
		{
			get
			{
				return InitialEndVerseNumber == 0 ? InitialStartVerseNumber.ToString(CultureInfo.InvariantCulture) :
					InitialStartVerseNumber + "-" + InitialEndVerseNumber;
			}
		}

		public string GetTextAsHtml(bool showVerseNumbers, bool rightToLeftScript)
		{
			var bldr = new StringBuilder();

			var currVerse = InitialVerseNumberOrBridge;

			foreach (var blockElement in BlockElements)
			{
				var verse = blockElement as Verse;
				if (verse != null)
				{
					if (showVerseNumbers)
						bldr.Append(BuildVerseNumber(verse.Number, rightToLeftScript));
					currVerse = verse.Number;
					continue;
				}

				var text = blockElement as ScriptText;
				if (text == null) continue;

				var content = string.Format("<div id=\"{0}\" class=\"scripttext\">{1}</div>", currVerse, HttpUtility.HtmlEncode(text.Content));
				bldr.Append(content);
			}

			return bldr.ToString();
		}

		public string GetSplitTextAsHtml(int blockId, bool rightToLeftScript, IEnumerable<BlockSplitData> blockSplits, bool showCharacters)
		{
			var bldr = new StringBuilder();
			var currVerse = InitialVerseNumberOrBridge;
			var verseNumberHtml = string.Empty;
			const string splitTextTemplate = "<div class=\"splittext\" data-blockid=\"{2}\" data-verse=\"{3}\">{0}{1}</div>";

			foreach (var blockElement in BlockElements)
			{
				// add verse marker
				var verse = blockElement as Verse;
				if (verse != null)
				{
					verseNumberHtml = BuildVerseNumber(verse.Number, rightToLeftScript);
					currVerse = verse.Number;
					continue;
				}

				// add verse text
				var text = blockElement as ScriptText;
				if (text == null) continue;

				var encodedContent = string.Format(splitTextTemplate, verseNumberHtml, HttpUtility.HtmlEncode(text.Content), blockId, currVerse);
				
				if ((blockSplits != null) && blockSplits.Any())
				{
					var preEncodedContent = text.Content;

					var allContentToInsert = new List<string>();
					foreach (var groupOfSplits in blockSplits.GroupBy(s => new { s.BlockToSplit, s.VerseToSplit }))
					{
						
						var sortedGroupOfSplits = groupOfSplits.OrderByDescending(s => s, BlockSplitData.BlockSplitDataOffsetComparer);
						foreach (var blockSplit in sortedGroupOfSplits)
						{
							var offsetToInsertExtra = blockSplit.CharacterOffsetToSplit;
							if (blockSplit.VerseToSplit == currVerse)
							{
								if (offsetToInsertExtra == BookScript.kSplitAtEndOfVerse)
									offsetToInsertExtra = preEncodedContent.Length;
								if (offsetToInsertExtra < 0 || offsetToInsertExtra > preEncodedContent.Length)
								{
									// ReSharper disable once NotResolvedInText
									throw new ArgumentOutOfRangeException("offsetToInsertExtra", offsetToInsertExtra,
										@"Value must be greater than or equal to 0 and less than or equal to the length (" + preEncodedContent.Length +
										@") of the encoded content of verse " + currVerse);
								}
								allContentToInsert.Insert(0, BuildSplitLineHtml(blockSplit.Id) + (showCharacters ? CharacterSelect(blockSplit.Id) : ""));
								preEncodedContent = preEncodedContent.Insert(offsetToInsertExtra, kAwooga);
							}
						}
					}
					if (preEncodedContent != text.Content)
					{
						encodedContent = HttpUtility.HtmlEncode(preEncodedContent);

						// wrap each text segment in a splittext div
						var segments = encodedContent.Split(new[] { kAwooga }, StringSplitOptions.None);
						var newSegments = new List<string>();
						foreach (var segment in segments)
						{
							newSegments.Add(string.Format(splitTextTemplate, verseNumberHtml, segment, blockId, currVerse));
							verseNumberHtml = string.Empty;
						}

						encodedContent = string.Join(kAwooga, newSegments);

						foreach (var contentToInsert in allContentToInsert)
							encodedContent = encodedContent.ReplaceFirst(kAwooga, contentToInsert);
					}
				}

				bldr.Append(encodedContent);

				// reset verse number element
				verseNumberHtml = string.Empty;
			}

			return bldr.ToString();
		}

		private string BuildVerseNumber(string verseNumber, bool rightToLeftScript)
		{
			const string template = "<sup>{1}{0}&#160;{1}</sup>";
			var rtl = rightToLeftScript ? "&rlm;" : "";

			return string.Format(template, verseNumber, rtl);
		}

		public override string ToString()
		{
			return string.IsNullOrEmpty(CharacterId) ? GetText(true) : string.Format("{0}: ({1}) {2}", CharacterId, MultiBlockQuote.ShortName(), GetText(true));
		}

		public string ToString(bool includeReference, string bookId = null)
		{
			if (!includeReference)
				return ToString();

			if (bookId == null && !string.IsNullOrEmpty(BookCode))
				bookId = BookCode;
			int bookNum;
			if (bookId == null)
			{
				bookId = string.Empty;
				bookNum = 1;
			}
			else
				bookNum = BCVRef.BookToNumber(bookId);

			var startRef = new BCVRef(bookNum, ChapterNumber, InitialStartVerseNumber);
			var endRef = new BCVRef(bookNum, ChapterNumber, LastVerse);
			
			return BCVRef.MakeReferenceString(bookId, startRef, endRef, ":", "-") + " : " + ToString();
		}

		/// <summary>
		/// Gets whether this block is a quote. It's not 100% reliable since there's the (slight) possibility that the user
		/// could assign the character for a block and then assign it back to Narrator. This would result in UserConfirmed
		/// being set to true even though it was a "non-quote" (unmarked) narrator block. Depending on how this
		/// property gets used in the future, we might need to actually store an additional piece of information about
		/// the block to distinguish this case and prevent a false positive. (For the current planned usage, an occasional
		/// false positive will not be a big deal.)
		/// </summary>
		public bool IsQuote
		{
			get { return !CharacterVerseData.IsCharacterStandard(CharacterId) || UserConfirmed; }
		}

		/// <summary>
		/// Gets whether the specified block represents Scripture text. (Only Scripture blocks can have their
		/// character/delivery changed. Book titles, chapters, and section heads have characters assigned
		/// programmatically and cannot be changed.)
		/// </summary>
		public bool IsScripture
		{
			get { return !CharacterVerseData.IsCharacterExtraBiblical(CharacterId); }
		}

		public bool CharacterIs(string bookId, CharacterVerseData.StandardCharacter standardCharacterType)
		{
			return CharacterId == CharacterVerseData.GetStandardCharacterId(bookId, standardCharacterType);
		}

		public bool CharacterIsUnclear()
		{
			return CharacterId == CharacterVerseData.kUnknownCharacter || CharacterId == CharacterVerseData.kAmbiguousCharacter;
		}

		public void SetStandardCharacter(string bookId, CharacterVerseData.StandardCharacter standardCharacterType)
		{
			CharacterId = CharacterVerseData.GetStandardCharacterId(bookId, standardCharacterType);
			Delivery = null;
		}

		public string GetAsXml(bool includeXmlDeclaration = true)
		{
			return XmlSerializationHelper.SerializeToString(this, !includeXmlDeclaration);
		}

		public void SetCharacterAndDelivery(IEnumerable<CharacterVerse> characters)
		{
			var characterList = characters.ToList();
			if (characterList.Count == 1)
			{
				SetCharacterAndCharacterIdInScript(characterList[0].Character, () => characterList[0]);
				Delivery = characterList[0].Delivery;
			}
			else if (characterList.Count == 0)
			{
				CharacterId = CharacterVerseData.kUnknownCharacter;
				CharacterIdOverrideForScript = null;
				Delivery = null;
				UserConfirmed = false;
			}
			else
			{
				// Might all represent the same Character/Delivery. Need to check.
				var set = new SortedSet<CharacterVerse>(characterList, new CharacterDeliveryComparer());
				if (set.Count == 1)
				{
					SetCharacterAndCharacterIdInScript(characterList[0].Character, () => characterList[0]);
					Delivery = set.First().Delivery;
				}
				else
				{
					CharacterId = CharacterVerseData.kAmbiguousCharacter;
					CharacterIdOverrideForScript = null;
					Delivery = null;
					UserConfirmed = false;
				}
			}
		}

		public void SetCharacterAndCharacterIdInScript(string characterId, int bookNumber, Paratext.ScrVers scrVers = null)
		{
			SetCharacterAndCharacterIdInScript(characterId, () => GetMatchingCharacter(bookNumber, scrVers));
		}

		private void SetCharacterAndCharacterIdInScript(string characterId, Func<CharacterVerse> getMatchingCharacterForVerse)
		{
			if (characterId == CharacterVerseData.kAmbiguousCharacter || characterId == CharacterVerseData.kUnknownCharacter)
			{
				CharacterId = characterId;
				CharacterIdInScript = null;
				return;
			}
			if (CharacterId == characterId && CharacterIdOverrideForScript != null)
				return;
			CharacterId = characterId;
			UseDefaultForMultipleChoiceCharacter(getMatchingCharacterForVerse);
		}

		public void UseDefaultForMultipleChoiceCharacter(int bookNumber, Paratext.ScrVers scrVers = null)
		{
			UseDefaultForMultipleChoiceCharacter(() => GetMatchingCharacter(bookNumber, scrVers));
		}

		public void UseDefaultForMultipleChoiceCharacter(Func<CharacterVerse> getMatchingCharacterForVerse)
		{
			var ids = CharacterId.SplitCharacterId(2);
			if (ids.Length > 1)
			{
				var cv = getMatchingCharacterForVerse();
				CharacterIdInScript = (cv != null && !String.IsNullOrEmpty(cv.DefaultCharacter) ? cv.DefaultCharacter : ids[0]);
			}
		}

		private CharacterVerse GetMatchingCharacter(int bookNumber, Paratext.ScrVers scrVers)
		{
			return GetMatchingCharacter(ControlCharacterVerseData.Singleton, bookNumber, scrVers);
		}

		public CharacterVerse GetMatchingCharacter(ICharacterVerseInfo cvInfo, int bookNumber, Paratext.ScrVers scrVers)
		{
			return cvInfo.GetCharacters(bookNumber, ChapterNumber, InitialStartVerseNumber,
				InitialEndVerseNumber, versification: scrVers).FirstOrDefault(c => c.Character == CharacterId);
		}

		public static string BuildSplitLineHtml(int id)
		{
			return string.Format(kSplitLineFrame, id);
		}

		public string CharacterSelect(int splitId, IEnumerable<AssignCharacterViewModel.Character> characters = null)
		{
			if ((characters == null) && !string.IsNullOrEmpty(s_characterSelect)) 
				return string.Format(s_characterSelect, splitId);

			const string optionTemplate = "<option value=\"{0}\">{1}</option>";
			var sb = new StringBuilder("<select class=\"select-character\" data-splitid=\"{0}\"><option value=\"\"></option>");

			if (characters != null)
			{
				foreach (var character in characters)
				{
					if (CharacterVerseData.IsCharacterStandard(character.CharacterId))
					{
							
					}
					if (character.IsNarrator)
					{
						sb.AppendFormat(optionTemplate,
							CharacterVerseData.GetStandardCharacterId(BookCode, CharacterVerseData.StandardCharacter.Narrator),
							character.LocalizedDisplay);
					}
					else
					{
						var stdCharacterType = CharacterVerseData.GetStandardCharacterType(character.CharacterId);
						if (stdCharacterType == CharacterVerseData.StandardCharacter.NonStandard)
						{
							sb.AppendFormat(optionTemplate, character.CharacterId, character.LocalizedDisplay);								
						}
						else
						{
							sb.AppendFormat(optionTemplate,
								CharacterVerseData.GetStandardCharacterId(BookCode, stdCharacterType),
								character.LocalizedDisplay);
						}
					}
				}
			}

			sb.Append("</select>");
			s_characterSelect = sb.ToString();

			return string.Format(s_characterSelect, splitId);
		}

		internal Block SplitBlock(string verseToSplit, int characterOffsetToSplit)
		{
			var currVerse = InitialVerseNumberOrBridge;

			Block newBlock = null;
			int indexOfFirstElementToRemove = -1;

			for (int i = 0; i < BlockElements.Count; i++)
			{
				var blockElement = BlockElements[i];

				if (newBlock != null)
				{
					newBlock.BlockElements.Add(blockElement);
					continue;
				}

				Verse verse = blockElement as Verse;
				if (verse != null)
					currVerse = verse.Number;
				else if (verseToSplit == currVerse)
				{
					ScriptText text = blockElement as ScriptText;

					string content;
					if (text == null)
					{
						if (BlockElements.Count > i + 1 && BlockElements[i + 1] is Verse)
						{
							content = string.Empty;
							characterOffsetToSplit = 0;
							indexOfFirstElementToRemove = i + 1;
						}
						else
							continue;
					}
					else
					{
						content = text.Content;

						if (BlockElements.Count > i + 1)
						{
							if (!(BlockElements[i + 1] is Verse) &&
								(characterOffsetToSplit == BookScript.kSplitAtEndOfVerse || characterOffsetToSplit > content.Length))
							{
								// Some kind of annotation. We can skip this. If we're splitting at
								continue;
							}
							indexOfFirstElementToRemove = i + 1;
						}

						if (characterOffsetToSplit == BookScript.kSplitAtEndOfVerse)
							characterOffsetToSplit = content.Length;

						if (characterOffsetToSplit <= 0 || characterOffsetToSplit > content.Length)
						{
							throw new ArgumentOutOfRangeException("characterOffsetToSplit", characterOffsetToSplit,
								@"Value must be greater than 0 and less than or equal to the length (" + content.Length +
								@") of the text of verse " + currVerse + @".");
						}
						if (characterOffsetToSplit == content.Length && indexOfFirstElementToRemove < 0)
							return null;
					}

					int initialStartVerse, initialEndVerse;
					if (characterOffsetToSplit == content.Length)
					{
						var firstVerseAfterSplit = (Verse)BlockElements[indexOfFirstElementToRemove];
						initialStartVerse = firstVerseAfterSplit.StartVerse;
						initialEndVerse = firstVerseAfterSplit.EndVerse;
					}
					else
					{
						var verseNumParts = verseToSplit.Split(new[] {'-'}, 2, StringSplitOptions.None);
						initialStartVerse = int.Parse(verseNumParts[0]);
						initialEndVerse = verseNumParts.Length == 2 ? int.Parse(verseNumParts[1]) : 0;
					}
					newBlock = new Block(StyleTag, ChapterNumber,
						initialStartVerse, initialEndVerse)
					{
						CharacterId = CharacterId,
						CharacterIdOverrideForScript = CharacterIdOverrideForScript,
						Delivery = Delivery,
						UserConfirmed = UserConfirmed
					};
					if (characterOffsetToSplit < content.Length)
						newBlock.BlockElements.Add(new ScriptText(content.Substring(characterOffsetToSplit)));
					if (text != null)
						text.Content = content.Substring(0, characterOffsetToSplit);
				}
			}

			if (newBlock == null)
				throw new ArgumentException(String.Format("Verse {0} not found in given block: {1}", verseToSplit, GetText(true)), "verseToSplit");

			if (indexOfFirstElementToRemove >= 0)
			{
				while (indexOfFirstElementToRemove < BlockElements.Count)
					BlockElements.RemoveAt(indexOfFirstElementToRemove);
			}
			return newBlock;
		}
	}

	public class BlockComparer : IEqualityComparer<Block>
	{
		public bool Equals(Block x, Block y)
		{
			if (x == null && y == null)
				return true;
			if (x == null || y == null)
				return false;

			return x.StyleTag == y.StyleTag &&
				x.IsParagraphStart == y.IsParagraphStart &&
				x.ChapterNumber == y.ChapterNumber &&
				x.InitialStartVerseNumber == y.InitialStartVerseNumber &&
				x.InitialEndVerseNumber == y.InitialEndVerseNumber &&
				x.CharacterId == y.CharacterId &&
				x.CharacterIdOverrideForScript == y.CharacterIdOverrideForScript &&
				x.Delivery == y.Delivery &&
				x.UserConfirmed == y.UserConfirmed &&
				x.MultiBlockQuote == y.MultiBlockQuote &&
				x.SplitId == y.SplitId &&
				x.BlockElements.SequenceEqual(y.BlockElements, new BlockElementContentsComparer());
		}

		public int GetHashCode(Block obj)
		{
			return obj.GetHashCode();
		}
	}

	public class SplitBlockComparer : IEqualityComparer<Block>
	{
		public bool Equals(Block x, Block y)
		{
			if (x == null && y == null)
				return true;
			if (x == null || y == null)
				return false;

			return x.ChapterNumber == y.ChapterNumber &&
				x.InitialStartVerseNumber == y.InitialStartVerseNumber &&
				x.InitialEndVerseNumber == y.InitialEndVerseNumber &&
				x.BlockElements.SequenceEqual(y.BlockElements, new BlockElementContentsComparer());
		}

		public int GetHashCode(Block obj)
		{
			return obj.GetHashCode();
		}
	}
}
