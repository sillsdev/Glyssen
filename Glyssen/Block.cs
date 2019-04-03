using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Serialization;
using Glyssen.Character;
using Glyssen.Dialogs;
using Glyssen.Shared;
using Glyssen.Utilities;
using SIL.Scripture;
using SIL.Xml;
using static System.Char;
using static System.String;

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

		public const string kLeadingPunctuationHtmlStart = "<span class=\"leading-punctuation\">";
		public const string kLeadingPunctuationHtmlEnd = "</span>";

		private static readonly Regex s_regexFollowOnParagraphStyles;
		internal static Regex s_regexInterruption;

		public static Func<string /* Book ID */, int /*Chapter Number*/, string> FormatChapterAnnouncement;

		public const string kSplitElementIdPrefix = "split";
		private const string kSplitLineFrame = "<div id=\"" + kSplitElementIdPrefix + "{0}\" class=\"split-line\"><div class=\"split-line-top\"></div></div>";
		private const string kRegexForVerseNumber = @"\{(?<verse>(?<startVerse>[0-9]+)((-|,)(?<endVerse>[0-9]+))?)\}";
		private const string kRegexForWhitespaceFollowingVerseNumber = @"(\u00A0| )*";

		/// <summary>Random string which will (hopefully) never appear in real text</summary>
		private const string kAwooga = "^~^";

		private int m_initialStartVerseNumber;
		private int m_initialEndVerseNumber;
		private int m_chapterNumber;
		private string m_characterIdInScriptOverride;
		private string m_delivery;
		private bool m_matchesReferenceText;
		private static string s_characterSelect;
		private static readonly Regex s_verseNumbersOrSounds;
		private static readonly Regex s_emptyVerseText;

		static Block()
		{
			s_verseNumbersOrSounds = new Regex("((" + kRegexForVerseNumber + ")|" + Sound.kRegexForUserLocatedSounds + ")" + kRegexForWhitespaceFollowingVerseNumber,
				RegexOptions.Compiled);
			s_emptyVerseText = new Regex("^ *(?<verseWithWhitespace>" + kRegexForVerseNumber + kRegexForWhitespaceFollowingVerseNumber + @")? *$",
				RegexOptions.Compiled);
			s_regexFollowOnParagraphStyles = new Regex("^((q.{0,2})|m|mi|(pi.?))$", RegexOptions.Compiled);
			InitializeInterruptionRegEx(false);
		}

		internal static void InitializeInterruptionRegEx(bool excludeLongDashes)
		{
			//                                   interruption in parentheses
			//                                                  |||        OR interruption in square brackets
			//                                                  |||                |||
			//                                                  |||                |||         OR interruption set off by single or double
			//                                                  vvv                vvv            (non word-medial) dashes
			StringBuilder pattern = new StringBuilder(@"((\(\w+[^)(\[\]]*\))|(\[\w+[^)(\[\]]*\])|((^|\B)-{1,2}[^-]*[-\w]+[^-]*-{1,2}(\Z|\B))");
			if (!excludeLongDashes)
			{
				// Long dashes should never be word-forming, so even if there is no surrounding whitespace,
				// they can safely be treated as punctuation dashes that could indicate an interruption.
				// Hence, the simpler regex (compared to the above regex for normal dashes).
				const String longDashStyleInterruptionFmt = @"|({0}[^{0}]*\w+[^{0}]*{0})";
				pattern.AppendFormat(longDashStyleInterruptionFmt, "\u2014");
				pattern.AppendFormat(longDashStyleInterruptionFmt, "\u2015");
			}
			pattern.Append(@")[^\w]*");
			s_regexInterruption = new Regex(pattern.ToString(), RegexOptions.Compiled);
		}

		internal Block()
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

		public Block Clone(bool includeReferenceBlocks = false)
		{
			var newBlock = (Block)MemberwiseClone();
			newBlock.BlockElements = new List<BlockElement>(BlockElements.Count);
			foreach (var blockElement in BlockElements)
				newBlock.BlockElements.Add(blockElement.Clone());

			if (includeReferenceBlocks)
				newBlock.CloneReferenceBlocks();

			return newBlock;

			// When cloning, we intentionally do not clone reference text info.
			// If caller (or anything downstream) needs to modify the reference text, it should either replace existing blocks or
			// clone them before modifying them.
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

		[XmlAttribute("initialEndVerse")]
		[DefaultValue(0)]
		public int InitialEndVerseNumber {
			get { return m_initialEndVerseNumber; }
			set { m_initialEndVerseNumber = m_initialStartVerseNumber == value ? 0 : value; }
		}

		private class VerseNumberFromBlock : IVerse
		{
			public static implicit operator VerseNumberFromBlock(Block block)
			{
				return new VerseNumberFromBlock
				{
					StartVerse = block.InitialStartVerseNumber,
					LastVerseOfBridge = block.InitialEndVerseNumber
				 };
			}

			public int StartVerse { get; private set; }
			public int EndVerse => LastVerseOfBridge == 0 ? StartVerse : LastVerseOfBridge;

			/// <summary>
			/// If the Verse number represents a verse bridge, this will be the ending number in the bridge; otherwise 0.
			/// </summary>
			public int LastVerseOfBridge { get; private set; }
		}

		public int LastVerseNum => LastVerse.EndVerse;
		public IVerse LastVerse => BlockElements.OfType<IVerse>().LastOrDefault() ?? (VerseNumberFromBlock)this;

		/// <summary>
		/// This is the character ID assigned by Glyssen or selected by the user during Phase 1 (protoscript).
		/// Do not use this in Phase 2 (actor assignment); Instead, use CharacterIdInScript.
		/// This setter does not update the CharacterIdInScript value. Therefore, it can be used for
		/// deserialization, cloning (e.g., when applying user decisions), and setting character IDs that
		/// are guaranteed not to represent multiple characters (e.g., Standard characters) - but if setting
		/// the id BACK to one of these, care must be taken to ensure that m_characterIdInScript is set back to
		/// null. In other contexts, use the SetCharacterIdAndCharacterIdInScript method.
		/// </summary>
		[XmlAttribute("characterId")]
		public string CharacterId { get; set; }

		[XmlAttribute("characterIdOverrideForScript")]
		public string CharacterIdOverrideForScript
		{
			get { return m_characterIdInScriptOverride; }
			set { CharacterIdInScript = value; }
		}

		[XmlIgnore]
		public string CharacterIdInScript
		{
			get { return m_characterIdInScriptOverride ?? CharacterId; }
			set { if (CharacterId != value) m_characterIdInScriptOverride = value; }
		}

		public void ApplyNarratorOverrides(ScrVers versification)
		{
			if (CharacterIdOverrideForScript == null &&
				CharacterVerseData.TryGetBookIdFromNarratorCharacterId(CharacterId, out string bookId))
			{
				m_characterIdInScriptOverride = NarratorOverrides.GetCharacterOverrideForBlock(BCVRef.BookToNumber(bookId), this, versification);
			}
		}

		public string GetCharacterIdInScript(ScrVers versification)
		{
			return CharacterIdOverrideForScript ??
				(CharacterVerseData.TryGetBookIdFromNarratorCharacterId(CharacterId, out string bookId) ?
				(NarratorOverrides.GetCharacterOverrideForBlock(BCVRef.BookToNumber(bookId), this, versification) ?? CharacterId) :
				CharacterId);
		}

		[XmlAttribute("delivery")]
		public string Delivery
		{
			get => m_delivery;
			set
			{
				if (IsNullOrWhiteSpace(value))
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

		public bool IsContinuationOfPreviousBlockQuote => MultiBlockQuote == MultiBlockQuote.Continuation;

		[XmlAttribute("splitId")]
		[DefaultValue(-1)]
		public int SplitId { get; set; }

		[XmlAttribute("matchesReferenceText")]
		[DefaultValue(false)]
		public bool MatchesReferenceText
		{
			// m_matchesReferenceText should imply exactly one reference block (and I *so* wish I had modeled it
			// that way), but if a previous program bug or deserialization issue should put a block into a bad
			// state, there are lots of places where it could crash, so if there isn't exactly 1, return false.
			get { return m_matchesReferenceText && ReferenceBlocks.Count ==  1; }
			set
			{
				m_matchesReferenceText = value;
				if (!m_matchesReferenceText)
					ReferenceBlocks.Clear();
			}
		}

		public int Length => IsChapterAnnouncement ? GetText(false).Length : BlockElements.OfType<ScriptText>().Sum(t => t.Content.Length);

		public int ScriptTextCount => BlockElements.Count(e => e is ScriptText);

		public string GetPrimaryReferenceText(bool textOnly = false)
		{
			return MatchesReferenceText ? ReferenceBlocks[0].GetTextFromBlockElements(!textOnly, !textOnly) : null;
		}

		/// <summary>
		/// Similar to PrimaryReferenceText, this method returns <code>null</code> if this block corresponds to more
		/// than one reference block (or corresponds to exactly one block that is considered a mismatch). However,
		/// it returns an empty string (which should be understood to be different from <code>null</code>) if no
		/// reference blocks are found at the requested (or any lower) level.
		/// </summary>
		/// <param name="depth">Current constraints elsewhere in the code make it such that this should never be
		/// greater than 1, but this method is implemented to support a (future) scenerio allowing more deeply
		/// nested reference texts.</param>
		public string GetReferenceTextAtDepth(int depth)
		{
			if (depth < 0)
				throw new ArgumentOutOfRangeException("depth", "Depth must not be negative.");
			// This might seem a little weird (and unlikely), but if the caller is asking for a particular level,
			// it is assumed that it is a valid level, so the absence of any reference blocks should be treated
			// the same as having a single empty block (which is the normal scenario in production).
			if (!ReferenceBlocks.Any())
				return "";
			if (depth == 0)
				return GetPrimaryReferenceText();
			if (ReferenceBlocks.Count == 1)
				return ReferenceBlocks[0].GetReferenceTextAtDepth(depth - 1);
			return null;
		}

		public string GetEmptyVerseReferenceTextAtDepth(int depth)
		{
			var refText = GetReferenceTextAtDepth(depth);
			var m = s_emptyVerseText.Match(refText);
			if (m.Success)
				return m.Result("${verseWithWhitespace}");
			return null;
		}

		public static bool IsEmptyVerseReferenceText(string text)
		{
			return text == null || s_emptyVerseText.IsMatch(text);
		}

		[XmlElement]
		public List<Block> ReferenceBlocks { get; set; }

		[XmlElement(Type = typeof (ScriptText), ElementName = "text")]
		[XmlElement(Type = typeof(Verse), ElementName = "verse")]
		[XmlElement(Type = typeof(Sound), ElementName = "sound")]
		[XmlElement(Type = typeof(Pause), ElementName = "pause")]
		public List<BlockElement> BlockElements { get; set; }

		public bool CharacterIsStandard { get { return CharacterVerseData.IsCharacterStandard(CharacterId); } }

		public bool IsChapterAnnouncement { get { return StyleTag == "c" || StyleTag == "cl"; } }

		public bool ContainsVerseNumber { get { return BlockElements.OfType<Verse>().Any(); } }

		public void SetMatchedReferenceBlock(Block referenceBlock)
		{
			if (referenceBlock == null)
				throw new ArgumentNullException("referenceBlock");
			ReferenceBlocks = new List<Block> { referenceBlock };
			MatchesReferenceText = true;
		}

		public void SetMatchedReferenceBlock(int bookNum, ScrVers versification,
			IReferenceLanguageInfo referenceLanguageInfo, IEnumerable<Block> referenceBlocksToJoin = null)
		{
			if (referenceBlocksToJoin == null)
				referenceBlocksToJoin = ReferenceBlocks;
			var refBlock = new Block(StyleTag, ChapterNumber, InitialStartVerseNumber, InitialEndVerseNumber);
			refBlock.SetCharacterAndDeliveryInfo(this, bookNum, versification);
			if (referenceBlocksToJoin.Any())
				refBlock.AppendJoinedBlockElements(referenceBlocksToJoin, referenceLanguageInfo);
			else
			{
				refBlock.BlockElements.Add(new ScriptText(""));
				if (referenceLanguageInfo.HasSecondaryReferenceText)
					refBlock.SetMatchedReferenceBlock(bookNum, versification, referenceLanguageInfo.BackingReferenceLanguage);
			}
			SetMatchedReferenceBlock(refBlock);
		}

		public Block SetMatchedReferenceBlock(string text, Block prevRefBlock = null)
		{
			var prevVerse = prevRefBlock == null ? (VerseNumberFromBlock)this : prevRefBlock.LastVerse;
			var refBlock = GetEmptyReferenceBlock(prevVerse);
			refBlock.ParsePlainText(text);
			if (!refBlock.StartsAtVerseStart)
			{
				var firstVerseInRefBlock = refBlock.BlockElements.OfType<Verse>().FirstOrDefault();
				if (firstVerseInRefBlock != null && firstVerseInRefBlock.EndVerse <= refBlock.InitialEndVerseNumber)
					refBlock.InitialEndVerseNumber = firstVerseInRefBlock.EndVerse - 1;
			}
			SetMatchedReferenceBlock(refBlock);

			return refBlock;
		}

		private Block GetEmptyReferenceBlock(IVerse prevVerse)
		{
			Block refBlock;
			if (ReferenceBlocks != null && ReferenceBlocks.Count == 1)
			{
				refBlock = ReferenceBlocks[0];
				refBlock.StyleTag = StyleTag;
				refBlock.ChapterNumber = ChapterNumber;
				refBlock.InitialStartVerseNumber = prevVerse.StartVerse;
				refBlock.InitialEndVerseNumber = prevVerse.LastVerseOfBridge;
				refBlock.BlockElements.Clear();
			}
			else
				refBlock = new Block(StyleTag, ChapterNumber, prevVerse.StartVerse, prevVerse.LastVerseOfBridge);
			refBlock.SetCharacterAndDeliveryInfo(this);
			return refBlock;
		}

		private void SetMatchedReferenceBlock(List<Tuple<BlockElement, BlockElement>> referenceTextBlockElements)
		{
			Block primaryBlock = GetEmptyReferenceBlock((VerseNumberFromBlock)this);
			primaryBlock.BlockElements.AddRange(referenceTextBlockElements.Select(e => e.Item1));
			var secondaryBlock = primaryBlock.GetEmptyReferenceBlock((VerseNumberFromBlock) primaryBlock);
			secondaryBlock.BlockElements.AddRange(referenceTextBlockElements.Select(e => e.Item2));
			primaryBlock.SetMatchedReferenceBlock(secondaryBlock);
		}

		private void ParsePlainText(string text)
		{
			var pos = 0;
			text = text.TrimStart();
			var prependSpace = "";
			while (pos < text.Length)
			{
				var match = s_verseNumbersOrSounds.Match(text, pos);
				if (match.Success)
				{
					if (match.Index == pos)
					{
						// We don't allow two verses in a row with no text between, so unless this is a verse at the very
						// beginning, remove the preceding (empty) verse.
						if (match.Index > 0 && BlockElements.Last() is Verse)
							BlockElements.RemoveAt(BlockElements.Count - 1);

						if (match.Groups["verse"].Success)
						{
							InitialStartVerseNumber = Int32.Parse(match.Result("${startVerse}"));
							int endVerse;
							if (!Int32.TryParse(match.Result("${endVerse}"), out endVerse))
								endVerse = 0;
							InitialEndVerseNumber = endVerse;
						}
					}
					else
					{
						BlockElements.Add(new ScriptText(prependSpace + text.Substring(pos, match.Index - pos)));
					}
					if (match.Groups["verse"].Success)
						BlockElements.Add(new Verse(match.Result("${verse}").Replace(',', '-')));
					else
					{
						var prevText = BlockElements.LastOrDefault() as ScriptText;
						if (prevText != null && prevText.Content.Last() != ' ')
							prevText.Content += " ";
						BlockElements.Add(Sound.CreateFromMatchedRegex(match));
						prependSpace = " ";
					}
					pos = match.Index + match.Length;
				}
				else
				{
					BlockElements.Add(new ScriptText(prependSpace + text.Substring(pos)));
					break;
				}
			}
			if (!BlockElements.Any())
				BlockElements.Add(new ScriptText(""));
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
							bldr.Append(annotation.ToDisplay());
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

		/// <summary>
		/// This handles the common case where the first Block Element is a Verse and the more
		/// unusual case where there is a preceding Script Text consisting only of an opening square
		/// bracket (which indicates a verse that is often omitted because of weak manuscript evidence).
		/// Technically, any preceding SciptText element that consists entirely of punctuation will be
		/// considered as being part of the following verse.)
		/// </summary>
		public bool StartsAtVerseStart
		{
			get
			{
				return BlockElements.First() is Verse || (StartsWithScriptTextElementContainingOnlyPunctuation && ContainsVerseNumber);
			}
		}

		public bool StartsWithEllipsis => BlockElements.OfType<ScriptText>().FirstOrDefault()?.StartsWithEllipsis ?? false;

		public bool StartsWithScriptTextElementContainingOnlyPunctuation
		{
			get
			{
				return (BlockElements.FirstOrDefault() as ScriptText)?.Content.All(c => IsPunctuation(c) || IsWhiteSpace(c)) ?? false;
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

				var content = Format("<div id=\"{0}\" class=\"scripttext\">{1}</div>", currVerse, HttpUtility.HtmlEncode(text.Content));
				bldr.Append(content);
			}

			return bldr.ToString();
		}

		private string GetLeadingPunctuation()
		{
			if (BlockElements.FirstOrDefault() is ScriptText initialScriptText &&
			    BlockElements.Skip(1).FirstOrDefault() is Verse initialVerse &&
			    initialVerse.Number == InitialVerseNumberOrBridge)
			{
				return initialScriptText.Content;
			}

			return null;
		}

		public string GetSplitTextAsHtml(int blockId, bool rightToLeftScript, IEnumerable<BlockSplitData> blockSplits, bool showCharacters)
		{
			var bldr = new StringBuilder();
			var currVerse = InitialVerseNumberOrBridge;
			var verseNumberHtml = Empty;
			string leadingPunctuationHtml = null;
			const string splitTextTemplate = "<div class=\"splittext\" data-blockid=\"{2}\" data-verse=\"{3}\">{4}{0}{1}</div>";
			const string leadingPunctuationTemplate = kLeadingPunctuationHtmlStart + "{0}" + kLeadingPunctuationHtmlEnd;

			// Look for special case where verse has leading punctuation before the verse number such as
			// ({1} This verse is surrounded by parentheses)
			// This can only happen at the beginning of a block.
			// If we have it, we basically want to do the split as if it wasn't there at all. i.e. Split the main part of the verse only, and
			// do not include the leading punctuation as part of the offset.
			var leadingPunctuation = GetLeadingPunctuation();
			if (leadingPunctuation != null)
				leadingPunctuationHtml = Format(leadingPunctuationTemplate, leadingPunctuation);

			foreach (var blockElement in BlockElements.Skip(leadingPunctuationHtml != null ? 1 : 0))
			{
				// add verse marker
				if (blockElement is Verse verse)
				{
					verseNumberHtml = BuildVerseNumber(verse.Number, rightToLeftScript);
					currVerse = verse.Number;
					continue;
				}

				// add verse text
				var text = blockElement as ScriptText;
				if (text == null) continue;

				var encodedContent = Format(splitTextTemplate, verseNumberHtml, HttpUtility.HtmlEncode(text.Content), blockId, currVerse, leadingPunctuationHtml);

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
								if (offsetToInsertExtra == PortionScript.kSplitAtEndOfVerse)
									offsetToInsertExtra = preEncodedContent.Length;

								if (offsetToInsertExtra < 0 || offsetToInsertExtra > preEncodedContent.Length)
								{
									throw new IndexOutOfRangeException("Value of offsetToInsertExtra must be greater than or equal to 0 and less " +
									                                   $"than or equal to the length ({preEncodedContent.Length}) of the content of verse {currVerse}");
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
							newSegments.Add(Format(splitTextTemplate, verseNumberHtml, segment, blockId, currVerse, leadingPunctuationHtml));
							verseNumberHtml = Empty;
							leadingPunctuationHtml = null;
						}

						encodedContent = Join(kAwooga, newSegments);

						foreach (var contentToInsert in allContentToInsert)
							encodedContent = encodedContent.ReplaceFirst(kAwooga, contentToInsert);
					}
				}

				bldr.Append(encodedContent);

				// reset verse number element
				verseNumberHtml = Empty;

				leadingPunctuationHtml = null;
			}

			return bldr.ToString();
		}

		private string BuildVerseNumber(string verseNumber, bool rightToLeftScript)
		{
			const string template = "<sup>{1}{0}&#160;{1}</sup>";
			var rtl = rightToLeftScript ? "&rlm;" : "";

			return Format(template, verseNumber, rtl);
		}

		public override string ToString()
		{
			return IsNullOrEmpty(CharacterId) ? GetText(true) : Format("{0}: ({1}) {2}", CharacterId, MultiBlockQuote.ShortName(), GetText(true));
		}

		public string ToString(bool includeReference, string bookId = null)
		{
			if (!includeReference)
				return ToString();

			if (bookId == null && !IsNullOrEmpty(BookCode))
				bookId = BookCode;
			int bookNum;
			if (bookId == null)
			{
				bookId = Empty;
				bookNum = 1;
			}
			else
				bookNum = BCVRef.BookToNumber(bookId);

			var startRef = new BCVRef(bookNum, ChapterNumber, InitialStartVerseNumber);
			var endRef = new BCVRef(bookNum, ChapterNumber, LastVerseNum);

			return BCVRef.MakeReferenceString(bookId, startRef, endRef, ":", "-") + " : " + ToString();
		}

		/// <summary>
		/// Gets whether this block is a quote. It's not 100% reliable since there's the possibility that the user
		/// could assign the character for a block (or that Glyssen could think it was a quote) and then assign it
		/// back to Narrator. This would result in UserConfirmed being set to true even though it was a "non-quote"
		/// (unmarked) narrator block. Depending on how this property gets used in the future, we might need to
		/// actually store an additional piece of information about the block to distinguish this case and prevent
		/// a false positive. (For the current planned usage, an occasional false positive will not be a big deal.)
		/// </summary>
		public bool IsQuote
		{
			get { return !CharacterVerseData.IsCharacterStandard(CharacterId) || UserConfirmed; }
		}

		public bool IsQuoteStart => IsQuote && !IsContinuationOfPreviousBlockQuote;

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
			return CharacterVerseData.IsCharacterUnclear(CharacterId);
		}

		public void SetStandardCharacter(string bookId, CharacterVerseData.StandardCharacter standardCharacterType)
		{
			SetNonDramaticCharacterId(CharacterVerseData.GetStandardCharacterId(bookId, standardCharacterType));
		}

		// Convenience method for setting the character ID to a standard character or non-character. This ensures that
		// fields are cleared that would be inappropriate for this kind of character.
		public void SetNonDramaticCharacterId(string characterID)
		{
			CharacterId = characterID;
			m_characterIdInScriptOverride = null;
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
				SetCharacterIdAndCharacterIdInScript(characterList[0].Character, () => characterList[0]);
				Delivery = characterList[0].Delivery;
			}
			else if (characterList.Count == 0)
			{
				SetNonDramaticCharacterId(CharacterVerseData.kUnknownCharacter);
				UserConfirmed = false;
			}
			else
			{
				// Might all represent the same Character/Delivery. Need to check.
				var set = new SortedSet<CharacterVerse>(characterList, new CharacterDeliveryComparer());
				if (set.Count == 1)
				{
					SetCharacterIdAndCharacterIdInScript(characterList[0].Character, () => characterList[0]);
					Delivery = set.First().Delivery;
				}
				else
				{
					if (characterList.Count(cd => cd.QuoteType == QuoteType.Interruption) == 1 &&
						characterList.Count(cd => cd.QuoteType != QuoteType.Interruption) == 1 &&
						ProbablyDoesNotContainInterruption)
					{
						// Since this block does not appear to be an interruption, we can safely assign the character from
						// the one and only cv record that is not an "Interruption" type.
						var cv = characterList.First(cd => cd.QuoteType != QuoteType.Interruption);
						SetCharacterIdAndCharacterIdInScript(cv.Character, () => cv);
						Delivery = cv.Delivery;
					}
					else
					{
						SetNonDramaticCharacterId(CharacterVerseData.kAmbiguousCharacter);
						UserConfirmed = false;
					}
				}
			}
		}

		public bool ProbablyDoesNotContainInterruption
		{
			get
			{
				if (BlockElements.Count != 1)
					return true;
				var textElement = BlockElements[0] as ScriptText;
				if (textElement == null)
					return true;
				var text = textElement.Content;//.Trim();
				var match = s_regexInterruption.Match(text);
				return !match.Success;
			}
		}

		public void SetCharacterIdAndCharacterIdInScript(string characterId, int bookNumber, ScrVers scrVers = null)
		{
			SetCharacterIdAndCharacterIdInScript(characterId, () => GetMatchingCharacter(bookNumber, scrVers));
		}

		private void SetCharacterIdAndCharacterIdInScript(string characterId, Func<CharacterVerse> getMatchingCharacterForVerse)
		{
			if (characterId == CharacterVerseData.kAmbiguousCharacter || characterId == CharacterVerseData.kUnknownCharacter)
			{
				SetNonDramaticCharacterId(characterId);
				return;
			}
			if (CharacterId == characterId && CharacterIdOverrideForScript != null)
			{
				if (CharacterIsStandard)
					m_characterIdInScriptOverride = null;
				return;
			}
			CharacterId = characterId;
			UseDefaultForMultipleChoiceCharacter(getMatchingCharacterForVerse);
		}

		public void UseDefaultForMultipleChoiceCharacter(int bookNumber, ScrVers scrVers = null)
		{
			UseDefaultForMultipleChoiceCharacter(() => GetMatchingCharacter(bookNumber, scrVers));
		}

		public void UseDefaultForMultipleChoiceCharacter(Func<CharacterVerse> getMatchingCharacterForVerse)
		{
			var ids = CharacterId.SplitCharacterId(2);
			if (ids.Length > 1)
			{
				var cv = getMatchingCharacterForVerse();
				CharacterIdInScript = (cv != null && !IsNullOrEmpty(cv.DefaultCharacter) ? cv.DefaultCharacter : ids[0]);
			}
			else if (CharacterIsStandard)
				m_characterIdInScriptOverride = null;
		}

		private CharacterVerse GetMatchingCharacter(int bookNumber, ScrVers scrVers)
		{
			return GetMatchingCharacter(ControlCharacterVerseData.Singleton, bookNumber, scrVers);
		}

		public CharacterVerse GetMatchingCharacter(ICharacterVerseInfo cvInfo, int bookNumber, ScrVers scrVers)
		{
			return cvInfo.GetCharacters(bookNumber, ChapterNumber, InitialStartVerseNumber,
				InitialEndVerseNumber, versification: scrVers).FirstOrDefault(c => c.Character == CharacterId);
		}

		public static string BuildSplitLineHtml(int id)
		{
			return Format(kSplitLineFrame, id);
		}

		public string CharacterSelect(int splitId, IEnumerable<AssignCharacterViewModel.Character> characters = null)
		{
			if ((characters == null) && !IsNullOrEmpty(s_characterSelect))
				return Format(s_characterSelect, splitId);

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

			return Format(s_characterSelect, splitId);
		}

		public static Block CombineBlocks(Block blockA, Block blockB)
		{
			var clone = blockA.Clone(true);
			return clone.CombineWith(blockB);
		}

		public Block CombineWith(Block otherBlock)
		{
			var skip = 0;
			var lastElementOfThisBlockAsScriptText = BlockElements.Last() as ScriptText;
			if (lastElementOfThisBlockAsScriptText != null)
			{
				if (lastElementOfThisBlockAsScriptText.Content.Any())
				{
					var firstElementOfOtherBlockAsScriptText = otherBlock.BlockElements.First() as ScriptText;
					if (firstElementOfOtherBlockAsScriptText != null)
					{
						var followingContent = firstElementOfOtherBlockAsScriptText.ContentWithoutLeadingEllipsis;
						var space = IsWhiteSpace(lastElementOfThisBlockAsScriptText.Content.Last()) ||
							!followingContent.Any() || IsWhiteSpace(followingContent[0]) ? Empty : " ";
						lastElementOfThisBlockAsScriptText.Content += space + followingContent;
						skip = 1;
						if (!IsWhiteSpace(lastElementOfThisBlockAsScriptText.Content.Last()) && otherBlock.BlockElements.Skip(skip).Any())
							lastElementOfThisBlockAsScriptText.Content += " ";
					}
					else if (!IsWhiteSpace(lastElementOfThisBlockAsScriptText.Content.Last()))
					{
						lastElementOfThisBlockAsScriptText.Content += " ";
					}
				}
				else if (otherBlock.BlockElements.Any())
				{
					BlockElements.RemoveAt(BlockElements.Count - 1);
				}
			}
			foreach (var blockElement in otherBlock.BlockElements.Skip(skip))
				BlockElements.Add(blockElement.Clone());

			UserConfirmed &= otherBlock.UserConfirmed;
			if (MatchesReferenceText)
			{
				if (otherBlock.MatchesReferenceText)
					ReferenceBlocks.Single().CombineWith(otherBlock.ReferenceBlocks.Single());
				else
					throw new InvalidOperationException("No known need for combining blocks where only one of them is aligned to reference text.");
			}
			else if (otherBlock.MatchesReferenceText)
				throw new InvalidOperationException("No known need for combining blocks where only one of them is aligned to reference text.");
			return this;
		}

		public bool IsFollowOnParagraphStyle => s_regexFollowOnParagraphStyles.IsMatch(StyleTag);

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

				if (blockElement is Verse verse)
					currVerse = verse.Number;
				else if (verseToSplit == currVerse)
				{
					ScriptText text = blockElement as ScriptText;

					string content;
					if (text == null)
					{
						if (BlockElements.Count > i + 1 && BlockElements[i + 1] is Verse)
						{
							content = Empty;
							characterOffsetToSplit = 0;
							indexOfFirstElementToRemove = i + 1;
						}
						else
							continue;
					}
					else
					{
						content = text.Content;

						if (content.All(c => !IsLetter(c)))
							continue; // Probably a leading square bracket.

						if (BlockElements.Count > i + 1)
						{
							if (!(BlockElements[i + 1] is Verse) &&
								(characterOffsetToSplit == PortionScript.kSplitAtEndOfVerse || characterOffsetToSplit > content.Length))
							{
								// Some kind of annotation. We can skip this. If we're splitting at
								continue;
							}
							indexOfFirstElementToRemove = i + 1;
						}

						if (characterOffsetToSplit == PortionScript.kSplitAtEndOfVerse)
							characterOffsetToSplit = content.Length;

						if (characterOffsetToSplit <= 0 || characterOffsetToSplit > content.Length)
						{
							throw new ArgumentOutOfRangeException(nameof(characterOffsetToSplit), characterOffsetToSplit,
								$@"Value must be greater than 0 and less than or equal to the length ({content.Length}) of the text of verse {currVerse}.");
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
						initialStartVerse = BCVRef.VerseToIntStart(verseToSplit);
						initialEndVerse = BCVRef.VerseToIntEnd(verseToSplit);
					}
					newBlock = new Block(StyleTag, ChapterNumber, initialStartVerse, initialEndVerse)
					{
						CharacterId = CharacterId,
						m_characterIdInScriptOverride = m_characterIdInScriptOverride,
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
				throw new ArgumentException($@"Verse {verseToSplit} not found in given block: {GetText(true)}", nameof(verseToSplit));

			if (indexOfFirstElementToRemove >= 0)
			{
				while (indexOfFirstElementToRemove < BlockElements.Count)
					BlockElements.RemoveAt(indexOfFirstElementToRemove);
			}

			return newBlock;
		}

		public void SetCharacterAndDeliveryInfo(Block basedOnBlock, int bookNumber, ScrVers scrVers)
		{
			if (basedOnBlock.CharacterIdOverrideForScript == null)
				SetCharacterIdAndCharacterIdInScript(basedOnBlock.CharacterId, bookNumber, scrVers);
			SetCharacterAndDeliveryInfo(basedOnBlock);
		}

		public void SetCharacterInfo(Block basedOnBlock)
		{
			CharacterId = basedOnBlock.CharacterId;
			m_characterIdInScriptOverride = basedOnBlock.m_characterIdInScriptOverride;
		}

		private void SetCharacterAndDeliveryInfo(Block basedOnBlock)
		{
			SetCharacterInfo(basedOnBlock);
			Delivery = basedOnBlock.Delivery;
		}

		public void AppendJoinedBlockElements(IEnumerable<Block> referenceBlocks, IReferenceLanguageInfo languageInfo)
		{
			var nestedRefBlocks = new List<Block>();

			foreach (Block r in referenceBlocks)
			{
				if (r.MatchesReferenceText)
					nestedRefBlocks.Add(r.ReferenceBlocks.Single());
				foreach (BlockElement element in r.BlockElements)
				{
					var prevScriptText = BlockElements.LastOrDefault() as ScriptText;
					if (prevScriptText != null)
						prevScriptText.Content = prevScriptText.Content.TrimEnd() + languageInfo.WordSeparator;

					var scriptText = element as ScriptText;
					if (scriptText != null)
					{
						if (prevScriptText != null)
						{
							prevScriptText.Content += scriptText.Content;
							continue;
						}
					}
					BlockElements.Add(element.Clone());
				}
			}
			if (nestedRefBlocks.Any())
			{
				var backingRefBlock = new Block(StyleTag, ChapterNumber, InitialStartVerseNumber,
					InitialEndVerseNumber);
				backingRefBlock.SetCharacterAndDeliveryInfo(this);
				backingRefBlock.AppendJoinedBlockElements(nestedRefBlocks, languageInfo.BackingReferenceLanguage);
				SetMatchedReferenceBlock(backingRefBlock);
			}
		}

		public void CloneReferenceBlocks()
		{
			var origList = ReferenceBlocks;
			ReferenceBlocks = new List<Block>(origList.Select(rb => rb.Clone(true)));
		}

		public static void GetSwappedReferenceText(string rowA, string rowB, out string newRowAValue, out string newRowBValue)
		{
			newRowBValue = rowA;
			if (rowA == null || rowB == null)
			{
				newRowAValue = rowB;
				return;
			}

			var leadingVerse = Empty;
			var verseNumbers = new Regex("^" + kRegexForVerseNumber + kRegexForWhitespaceFollowingVerseNumber);
			var match = verseNumbers.Match(newRowBValue);
			if (match.Success && !verseNumbers.IsMatch(rowB))
			{
				leadingVerse = match.Value;
				newRowBValue = newRowBValue.Substring(match.Length);
			}
			newRowAValue = leadingVerse + rowB;
		}

		public VerseRef StartRef(int bookNum, ScrVers versification)
		{
			return new VerseRef(bookNum, ChapterNumber, InitialStartVerseNumber, versification);
		}

		public VerseRef EndRef(int bookNum, ScrVers versification)
		{
			return new VerseRef(bookNum, ChapterNumber, LastVerseNum, versification);
		}

		public bool ChangeReferenceText(string bookId, ReferenceText referenceText, ScrVers vernVersification)
		{
			if (!MatchesReferenceText)
				throw new InvalidOperationException("ChangeReferenceText should not be called for a block that is not aligned to a reference text block.");
			var refBook = referenceText.GetBook(bookId);

			if (refBook == null)
				return true;

			var existingReferenceText = ReferenceBlocks.Single();

			if (!referenceText.HasSecondaryReferenceText)
			{
				SetMatchedReferenceBlock(existingReferenceText.ReferenceBlocks.Single());
				return true;
			}

			var englishRefText = ReferenceText.GetStandardReferenceText(ReferenceTextType.English);
			var bookNumber = BCVRef.BookToNumber(bookId);
			var startVerse = StartRef(bookNumber, vernVersification);
			var endVerse = EndRef(bookNumber, vernVersification);
			startVerse.ChangeVersification(englishRefText.Versification);
			endVerse.ChangeVersification(englishRefText.Versification);

			List<Block> refBlocksForPassage;
			if (startVerse.ChapterNum == endVerse.ChapterNum)
				refBlocksForPassage = refBook.GetBlocksForVerse(startVerse.ChapterNum, startVerse.VerseNum, endVerse.VerseNum).ToList();
			else
			{
				int lastVerseInStartChapter = englishRefText.Versification.GetLastVerse(bookNumber, startVerse.ChapterNum);
				refBlocksForPassage = refBook.GetBlocksForVerse(startVerse.ChapterNum, startVerse.VerseNum, lastVerseInStartChapter).ToList();
				refBlocksForPassage.AddRange(refBook.GetBlocksForVerse(endVerse.ChapterNum, 1, endVerse.VerseNum));
			}

			var matchingRefBlocks = refBlocksForPassage.Where(refBlock => refBlock.GetPrimaryReferenceText() == GetPrimaryReferenceText()).ToList();
			if (matchingRefBlocks.Count == 1)
			{
				SetMatchedReferenceBlock(BCVRef.BookToNumber(bookId), vernVersification, referenceText, matchingRefBlocks);
				return true;
			}

			var englishToPrimaryDictionary = new Dictionary<string, string>();
			foreach (var refBlock in refBlocksForPassage)
			{
				var scriptBlocks = refBlock.BlockElements.OfType<ScriptText>().ToList();
				if (scriptBlocks.Count != refBlock.ReferenceBlocks.Single().BlockElements.OfType<ScriptText>().Count())
				{
					return false;
				}
				for (int i = 0; i < scriptBlocks.Count; i++)
				{
					var primaryElement = scriptBlocks[i];
					var secondaryElement = refBlock.ReferenceBlocks.Single().BlockElements.OfType<ScriptText>().ElementAt(i);
					englishToPrimaryDictionary[secondaryElement.Content] = primaryElement.Content;
				}
			}
			var englishHeSaidText = englishRefText.HeSaidText;
			if (!englishToPrimaryDictionary.ContainsKey(englishHeSaidText))
				englishToPrimaryDictionary.Add(englishHeSaidText, referenceText.HeSaidText);

			var referenceTextBlockElements = new List<Tuple<BlockElement, BlockElement>>(); // Item1 = Primary; Item 2 = English

			var blockElements = existingReferenceText.MatchesReferenceText
				? existingReferenceText.ReferenceBlocks.Single().BlockElements
				: existingReferenceText.BlockElements;
			foreach (var origRefTextBlockElement in blockElements)
			{
				var origScriptText = origRefTextBlockElement as ScriptText;
				if (origScriptText != null)
				{
					string primaryRefText;
					if (!englishToPrimaryDictionary.TryGetValue(origScriptText.Content, out primaryRefText))
					{
						primaryRefText = Empty;
						var origText = origScriptText.Content;
						while (origText.Any(c => !IsWhiteSpace(c)))
						{
							var key = englishToPrimaryDictionary.Keys.FirstOrDefault(s => origText.StartsWith(s));
							if (key == null)
							{
								return false;
							}
							//if (primaryRefText.Any() && !IsWhiteSpace(primaryRefText[0]))
							//	primaryRefText += " ";
							primaryRefText += englishToPrimaryDictionary[key] + " ";
							origText = origText.Remove(0, key.Length).TrimStart();
						}
					}
					referenceTextBlockElements.Add(new Tuple<BlockElement, BlockElement>(
						new ScriptText(primaryRefText), origScriptText));
				}
				else // verse or annotation
				{
					referenceTextBlockElements.Add(new Tuple<BlockElement, BlockElement>(origRefTextBlockElement, origRefTextBlockElement));
				}
			}

			var last = referenceTextBlockElements.Last();
			if (last.Item1 is ScriptText)
			{
				((ScriptText) last.Item1).Content = ((ScriptText) last.Item1).Content.TrimEnd();
				((ScriptText) last.Item2).Content = ((ScriptText) last.Item2).Content.TrimEnd();
			}

			SetMatchedReferenceBlock(referenceTextBlockElements);
			return true;
		}

		public Tuple<Match, string> GetNextInterruption(int startCharIndex = 1)
		{
			var verse = InitialVerseNumberOrBridge;
			foreach (var element in BlockElements)
			{
				var text = element as ScriptText;
				if (text != null)
				{
					var match = s_regexInterruption.Match(text.Content, startCharIndex);
					if (match.Success)
						return new Tuple<Match, string>(match, verse);
					startCharIndex = 1;
				}
				else
				{
					verse = ((Verse)element).Number;
				}
			}
			return null;
		}
	}

	public class BlockComparer : IEqualityComparer<Block>
	{
		readonly BlockReferenceComparer m_referenceComparer = new BlockReferenceComparer();

		public bool Equals(Block x, Block y)
		{
			if (x == null && y == null)
				return true;
			if (x == null || y == null)
				return false;

			return x.StyleTag == y.StyleTag &&
				x.IsParagraphStart == y.IsParagraphStart &&
				m_referenceComparer.Compare(x, y) == 0 &&
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
		readonly BlockReferenceComparer m_referenceComparer = new BlockReferenceComparer();

		public bool Equals(Block x, Block y)
		{
			if (x == null && y == null)
				return true;
			if (x == null || y == null)
				return false;

			return CompareReferences(x, y) == 0 &&
				x.BlockElements.SequenceEqual(y.BlockElements, new BlockElementContentsComparer());
		}

		public int CompareReferences(Block x, Block y)
		{
			return m_referenceComparer.Compare(x, y);
		}

		public int GetHashCode(Block obj)
		{
			return obj.GetHashCode();
		}
	}

	public class BlockReferenceComparer : IComparer<Block>
	{
		/// <summary>Compares two blocks (assumed to be in the same book) and returns a value indicating
		/// whether the reference of the one is less than, equal to, or greater than the other.</summary>
		/// <returns>A signed integer that indicates the relative values of x and y:
		/// Less than zero => x is less than y;
		/// Zero => x == y;
		/// Greater than zero x is greater than y
		/// </returns>  
		public int Compare(Block x, Block y)
		{
			return (x.ChapterNumber - y.ChapterNumber) * 1000000 +
				(x.InitialStartVerseNumber - y.InitialStartVerseNumber) * 1000 +
				(x.InitialEndVerseNumber - y.InitialEndVerseNumber);
		}
	}
}
