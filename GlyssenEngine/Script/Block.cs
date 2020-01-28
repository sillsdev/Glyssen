// Merged
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Serialization;
using Glyssen.Shared;
using GlyssenEngine.Character;
using GlyssenEngine.Quote;
using GlyssenEngine.Utilities;
using SIL.Scripture;
using SIL.Xml;
using static System.Char;
using static System.String;

namespace GlyssenEngine.Script
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

		private static readonly Regex s_regexFollowOnParagraphStyles;

		public static Func<string /* Book ID */, int /*Chapter Number*/, string> FormatChapterAnnouncement;

		private const string kRegexForVerseNumber = @"\{(?<verse>(?<startVerse>[0-9]+)((-|,)(?<endVerse>[0-9]+))?)\}";
		private const string kRegexForWhitespaceFollowingVerseNumber = @"(\u00A0| )*";

		private int m_initialStartVerseNumber;
		private int m_initialEndVerseNumber;
		private int m_chapterNumber;
		private string m_characterIdInScriptOverride;
		private string m_delivery;
		private bool m_matchesReferenceText;
		private static readonly Regex s_verseNumbersOrSounds;
		private static readonly Regex s_emptyVerseText;

		static Block()
		{
			s_verseNumbersOrSounds = new Regex("((" + kRegexForVerseNumber + ")|" + Sound.kRegexForUserLocatedSounds + ")" + kRegexForWhitespaceFollowingVerseNumber,
				RegexOptions.Compiled);
			s_emptyVerseText = new Regex("^ *(?<verseWithWhitespace>" + kRegexForVerseNumber + kRegexForWhitespaceFollowingVerseNumber + @")? *$",
				RegexOptions.Compiled);
			// Rather than a very permissive regex that attempts to include all \q* markers in hopes of catching any future markers that
			// might be added to the USFM standard, this regex matches only the known allowed poetry markers. It specifically prevents matching
			// "qa", which is an acrostic header and should not be treated like other poetry markers. As the standard is changed in the future,
			// any new markers that should be treated as "follow on" paragraphs will need to be added here.
			s_regexFollowOnParagraphStyles = new Regex("^((q((m?\\d?)|[rc])?)|m|mi|(pi\\d?)|(l(f|(i(m?)\\d?))))$", RegexOptions.Compiled);
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

		public enum ReferenceBlockCloningBehavior
		{
			/// <summary>
			/// This results in a no-op, so it is the least expensive option. Although used as the default, it is also
			/// the most dangerous option, since changes to the list (or the items in the list) attached to the clone
			/// will affect the original object as well.
			/// </summary>
			CrossLinkToOriginalReferenceBlockList,
			/// <summary>
			/// This is the safest but most expensive option. The reference block list is deep-cloned recursively.
			/// </summary>
			CloneListAndAllReferenceBlocks,
			/// <summary>
			/// This is also inexpensive but is only useful in cases where the cloned copy has no interest in knowing
			/// how/whether the original block was connected to the reference text.
			/// </summary>
			SetToNewEmptyList,
		}

		/// <summary>
		/// This does a memberwise clone of the normal properties and a deep clone (new list) of the block elements. See explanation
		/// of <paramref name="referenceBlockCloning"/> for details about reference block cloning behavior.
		/// </summary>
		/// <param name="referenceBlockCloning">Determines how/whether reference blocks will be cloned. See
		/// <see cref="ReferenceBlockCloningBehavior"/> for detailed explanation of each option. Note that while the default option
		/// turns out to be the most generally useful one, it is not safe.</param>
		/// <returns></returns>
		public Block Clone(ReferenceBlockCloningBehavior referenceBlockCloning = ReferenceBlockCloningBehavior.CrossLinkToOriginalReferenceBlockList)
		{
			var newBlock = (Block)MemberwiseClone();
			newBlock.BlockElements = new List<BlockElement>(BlockElements.Count);
			foreach (var blockElement in BlockElements)
				newBlock.BlockElements.Add(blockElement.Clone());

			switch (referenceBlockCloning)
			{
				case ReferenceBlockCloningBehavior.CrossLinkToOriginalReferenceBlockList:
					break;
				case ReferenceBlockCloningBehavior.CloneListAndAllReferenceBlocks:
					newBlock.CloneReferenceBlocks();
					break;
				case ReferenceBlockCloningBehavior.SetToNewEmptyList:
					newBlock.m_matchesReferenceText = false;
					newBlock.ReferenceBlocks = new List<Block>();
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(referenceBlockCloning), referenceBlockCloning, null);
			}

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
			set => m_chapterNumber = value;
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
			set => m_initialStartVerseNumber = value;
		}

		[XmlAttribute("initialEndVerse")]
		[DefaultValue(0)]
		public int InitialEndVerseNumber {
			get => m_initialEndVerseNumber;
			set => m_initialEndVerseNumber = m_initialStartVerseNumber == value ? 0 : value;
		}

		public class VerseNumberFromBlockBase : IVerse
		{
			public int StartVerse { get; protected set; }
			public int EndVerse => LastVerseOfBridge == 0 ? StartVerse : LastVerseOfBridge;

			/// <summary>
			/// If the Verse number represents a verse bridge, this will be the ending number in the bridge; otherwise 0.
			/// </summary>
			public int LastVerseOfBridge { get; protected set; }

			public IEnumerable<int> AllVerseNumbers => this.GetAllVerseNumbers();
		}

		public class InitialVerseNumberBridgeFromBlock : VerseNumberFromBlockBase
		{
			public static implicit operator InitialVerseNumberBridgeFromBlock(Block block)
			{
				return new InitialVerseNumberBridgeFromBlock
				{
					StartVerse = block.InitialStartVerseNumber,
					LastVerseOfBridge = block.InitialEndVerseNumber
				};
			}
		}

		public class VerseRangeFromBlock : VerseNumberFromBlockBase
		{
			public static implicit operator VerseRangeFromBlock(Block block)
			{
				return new VerseRangeFromBlock
				{
					StartVerse = block.InitialStartVerseNumber,
					LastVerseOfBridge = block.LastVerseNum
				};
			}
		}

		public int LastVerseNum => LastVerse.EndVerse;
		public IVerse LastVerse => BlockElements.OfType<IVerse>().LastOrDefault() ?? (InitialVerseNumberBridgeFromBlock)this;

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
			get => m_characterIdInScriptOverride;
			set => CharacterIdInScript = value;
		}

		[XmlIgnore]
		public string CharacterIdInScript
		{
			get => m_characterIdInScriptOverride ?? CharacterId;
			set { if (CharacterId != value) m_characterIdInScriptOverride = value; }
		}

		[XmlAttribute("delivery")]
		public string Delivery
		{
			get => m_delivery;
			set => m_delivery = IsNullOrWhiteSpace(value) ? null : value;
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

		/// <summary>
		/// Don't use in production code. It is intended ONLY for use by the XML serializer!
		/// To ensure internal consistency, use SetMatchedReferenceBlock, ClearReferenceText,
		/// SetUnmatchedReferenceBlocks, AppendUnmatchedReferenceBlock, and
		/// InsertUnmatchedReferenceBlocks methods.
		/// </summary>
		[XmlAttribute("matchesReferenceText")]
		[DefaultValue(false)]
		public bool MatchesReferenceText_DoNotUse
		{
			// m_matchesReferenceText should imply exactly one reference block (and I *so* wish I had modeled it
			// that way), but if a previous program bug or deserialization issue should put a block into a bad
			// state, there are lots of places where it could crash, so if there isn't exactly 1, return false.
			get => MatchesReferenceText;
			set => m_matchesReferenceText = value;
		}

		public bool MatchesReferenceText => m_matchesReferenceText && ReferenceBlocks.Count ==  1;

		public void ClearReferenceText()
		{
			m_matchesReferenceText = false;
			if (ReferenceBlocks != null) // This is probably always true, but just to be safe.
				ReferenceBlocks.Clear();
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

		public bool ContainsVerseNumber => BlockElements.OfType<Verse>().Any();

		/// <summary>
		/// Since the name of this property leaves some amount of vaguery, to be precise, it
		/// indicates whether the block represents any verse other than the verse/bridge it starts
		/// with. So it will return false if the block begins with a bridge and has no other
		/// subsequent verses. However, it does not guarantee that it completely covers any full
		/// verse, so it will return true if the block begins with the end of one verse and ends
		/// with the beginning of the following verse.
		/// </summary>
		public bool CoversMoreThanOneVerse => BlockElements.Skip(1).Any(e => e is Verse);

		public int CountOfSoundsWhereUserSpecifiesLocation => BlockElements.OfType<Sound>().Count(s => s.UserSpecifiesLocation);

		public void SetMatchedReferenceBlockFrom(Block sourceBlock)
		{
			ReferenceBlocks = new List<Block> {sourceBlock.ReferenceBlocks.Single().Clone(ReferenceBlockCloningBehavior.CloneListAndAllReferenceBlocks)};
			m_matchesReferenceText = true;
		}

		public void SetMatchedReferenceBlock(Block referenceBlock)
		{
			if (referenceBlock == null)
				throw new ArgumentNullException(nameof(referenceBlock));
			ReferenceBlocks = new List<Block> { referenceBlock };
			m_matchesReferenceText = true;
		}

		public void SetMatchedReferenceBlock(int bookNum, ScrVers versification,
			IReferenceLanguageInfo referenceLanguageInfo, IReadOnlyCollection<Block> referenceBlocksToJoin = null)
		{
			if (referenceBlocksToJoin == null)
			{
				referenceBlocksToJoin = ReferenceBlocks;
				Debug.Assert(referenceBlocksToJoin != null);
			}

			var refBlock = referenceBlocksToJoin.OnlyOrDefault();
			if (refBlock == null)
			{
				var baseBlock = (referenceBlocksToJoin?.FirstOrDefault() ?? this);
				refBlock = new Block(StyleTag, ChapterNumber, baseBlock.InitialStartVerseNumber, baseBlock.InitialEndVerseNumber);
				refBlock.SetCharacterAndDeliveryInfo(this, bookNum, versification);
				if (referenceBlocksToJoin.Any())
					refBlock.AppendJoinedBlockElements(referenceBlocksToJoin, referenceLanguageInfo);
				else
				{
					refBlock.BlockElements.Add(new ScriptText(""));
					if (referenceLanguageInfo.HasSecondaryReferenceText)
						refBlock.SetMatchedReferenceBlock(bookNum, versification, referenceLanguageInfo.BackingReferenceLanguage);
				}
			}
			SetMatchedReferenceBlock(refBlock);
		}

		public Block SetMatchedReferenceBlock(string text, Block prevRefBlock = null)
		{
			var prevVerse = prevRefBlock == null ? (InitialVerseNumberBridgeFromBlock)this : prevRefBlock.LastVerse;
			var refBlock = GetEmptyReferenceBlock(prevVerse);
			refBlock.ParsePlainText(text);
			if (!refBlock.StartsAtVerseStart && prevRefBlock == null && refBlock.InitialEndVerseNumber > 0)
			{
				// If we don't have a preceding ref block that can be used to imply the starting verse number/bridge
				// for this ref block, we at least want to prevent it from looking like it starts at or before the
				// first verse number it actually contains, so we infer that it starts at the preceding verse. This
				// is not a common scenario, and it is really somewhat of a guess as to what is actually happening.
				var firstVerseInRefBlock = refBlock.BlockElements.OfType<Verse>().FirstOrDefault();
				if (firstVerseInRefBlock != null && firstVerseInRefBlock.StartVerse <= refBlock.InitialEndVerseNumber)
					refBlock.InitialEndVerseNumber = firstVerseInRefBlock.StartVerse - 1;
			}
			SetMatchedReferenceBlock(refBlock);

			return refBlock;
		}

		public void SetUnmatchedReferenceBlocks(IEnumerable<Block> referenceBlocks)
		{
			if (referenceBlocks == null)
				throw new ArgumentNullException(nameof(referenceBlocks));
			ReferenceBlocks = referenceBlocks.ToList();
			m_matchesReferenceText = false;
		}

		public void AppendUnmatchedReferenceBlock(Block referenceBlock)
		{
			if (referenceBlock == null)
				throw new ArgumentNullException(nameof(referenceBlock));
			ReferenceBlocks.Add(referenceBlock);
			m_matchesReferenceText = false;
		}

		public void AppendUnmatchedReferenceBlocks(IEnumerable<Block> referenceBlocks)
		{
			if (referenceBlocks == null)
				throw new ArgumentNullException(nameof(referenceBlocks));
			ReferenceBlocks.AddRange(referenceBlocks);
			m_matchesReferenceText = false;
		}

		public void InsertUnmatchedReferenceBlocks(int index, IEnumerable<Block> referenceBlocks)
		{
			if (referenceBlocks == null)
				throw new ArgumentNullException(nameof(referenceBlocks));
			ReferenceBlocks.InsertRange(index, referenceBlocks);
			m_matchesReferenceText = false;
		}

		private Block GetEmptyReferenceBlock(IVerse prevVerse)
		{
			Block refBlock = ReferenceBlocks?.OnlyOrDefault();
			if (refBlock != null)
			{
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
			Block primaryBlock = GetEmptyReferenceBlock((InitialVerseNumberBridgeFromBlock)this);
			primaryBlock.BlockElements.AddRange(referenceTextBlockElements.Select(e => e.Item1));
			var secondaryBlock = primaryBlock.GetEmptyReferenceBlock((InitialVerseNumberBridgeFromBlock) primaryBlock);
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
							prependSpace = "";
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
		/// Technically, any preceding ScriptText element that consists entirely of punctuation will be
		/// considered as being part of the following verse.)
		/// </summary>
		public bool StartsAtVerseStart => !(BlockElements.First() is ScriptText) || (StartsWithScriptTextElementContainingOnlyPunctuation && ContainsVerseNumber);

		/// <summary>
		/// <see cref="StartsAtVerseStart"/> or <see cref="IsChapterAnnouncement"/>
		/// </summary>
		public bool IsVerseBreak => StartsAtVerseStart || IsChapterAnnouncement;

		/// <summary>
		/// Gets the <see cref="Verse"/> objects covered by this block. This may be single
		/// verses or bridges. Note that the initial and final verses may not be entirely contained
		/// within this block.
		/// </summary>
		public IReadOnlyCollection<IVerse> AllVerses
		{
			get
			{
				if (IsScripture)
				{
					var list = new List<IVerse>();
					list.Add(new Verse(InitialVerseNumberOrBridge));
					list.AddRange(BlockElements.Skip(1).OfType<Verse>());
					return list;
				}

				throw new InvalidOperationException("AllVerses property only valid for Scripture blocks.");
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

		public string GetLeadingPunctuation()
		{
			if (BlockElements.FirstOrDefault() is ScriptText initialScriptText &&
			    BlockElements.Skip(1).FirstOrDefault() is Verse initialVerse &&
			    initialVerse.Number == InitialVerseNumberOrBridge)
			{
				return initialScriptText.Content;
			}

			return null;
		}

		public string BuildVerseNumber(string verseNumber, bool rightToLeftScript)
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
			return !includeReference ? ToString() : GetReferenceString(bookId) + " : " + ToString();
		}

		public string GetReferenceString(string bookId = null)
		{
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

			return BCVRef.MakeReferenceString(bookId, startRef, endRef, ":", "-");
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

		public bool CharacterIsUnclear => CharacterVerseData.IsCharacterUnclear(CharacterId);

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

		public void SetCharacterAndDelivery(IQuoteInterruptionFinder interruptionFinder, IReadOnlyCollection<CharacterSpeakingMode> characters)
		{
			var characterList = characters.ToList();
			if (characterList.Count == 1)
			{
				SetCharacterIdAndCharacterIdInScript(characterList[0].Character, () => characterList[0]);
				Delivery = characterList[0].Delivery;
			}
			else if (characterList.Count == 0)
			{
				SetNonDramaticCharacterId(CharacterVerseData.kUnexpectedCharacter);
				UserConfirmed = false;
			}
			else
			{
				// Might all represent the same Character/Delivery. Need to check.
				var uniqueDelivery = characterList.Distinct(new CharacterDeliveryEqualityComparer()).OnlyOrDefault();
				if (uniqueDelivery != null)
				{
					SetCharacterIdAndCharacterIdInScript(characterList[0].Character, () => characterList[0]);
					Delivery = uniqueDelivery.Delivery;
				}
				else
				{
					CharacterSpeakingMode nonInterruption;
					if (characterList.Count(cd => cd.QuoteType == QuoteType.Interruption) == 1 &&
						(nonInterruption = characterList.OnlyOrDefault(cd => cd.QuoteType != QuoteType.Interruption)) != null &&
						ProbablyDoesNotContainInterruption(interruptionFinder))
					{
						// Since this block does not appear to be an interruption, we can safely assign the character from
						// the one and only cv record that is not an "Interruption" type.
						SetCharacterIdAndCharacterIdInScript(nonInterruption.Character, () => nonInterruption);
						Delivery = nonInterruption.Delivery;
					}
					else
					{
						SetNonDramaticCharacterId(CharacterVerseData.kAmbiguousCharacter);
						UserConfirmed = false;
					}
				}
			}
		}

		public bool ProbablyDoesNotContainInterruption(IQuoteInterruptionFinder interruptionFinder)
		{
			var textElement = BlockElements.OnlyOrDefault() as ScriptText;
			if (textElement == null)
				return true;
			var text = textElement.Content;//.Trim();
			return interruptionFinder.ProbablyDoesNotContainInterruption(text);
		}

		public void SetCharacterIdAndCharacterIdInScript(string characterId, int bookNumber, ScrVers scrVers = null)
		{
			SetCharacterIdAndCharacterIdInScript(characterId, () => GetMatchingCharacter(bookNumber, scrVers));
		}

		private void SetCharacterIdAndCharacterIdInScript(string characterId, Func<CharacterSpeakingMode> getMatchingCharacterForVerse)
		{
			if (characterId == CharacterVerseData.kAmbiguousCharacter || characterId == CharacterVerseData.kUnexpectedCharacter ||
				characterId == CharacterVerseData.kNeedsReview)
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

		public void UseDefaultForMultipleChoiceCharacter(Func<ICharacterDeliveryInfo> getMatchingCharacterForVerse)
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

		private CharacterSpeakingMode GetMatchingCharacter(int bookNumber, ScrVers scrVers)
		{
			return GetMatchingCharacter(ControlCharacterVerseData.Singleton, bookNumber, scrVers);
		}

		public CharacterSpeakingMode GetMatchingCharacter(ICharacterVerseInfo cvInfo, int bookNumber, ScrVers scrVers)
		{
			// Note: Do not change this to call the version of GetCharacters that takes a single IVerse because some of
			// the tests use a mocked call and they set up the mock for the other version.
			return cvInfo.GetCharacters(bookNumber, ChapterNumber, new [] { (InitialVerseNumberBridgeFromBlock)this }, scrVers, true)
				.FirstOrDefault(c => c.Character == CharacterId);
		}

		public static Block CombineBlocks(Block blockA, Block blockB)
		{
			var clone = blockA.Clone(ReferenceBlockCloningBehavior.CloneListAndAllReferenceBlocks);
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

					var newScriptText = (characterOffsetToSplit < content.Length) ? new ScriptText(content.Substring(characterOffsetToSplit)) : null;

					int initialStartVerse, initialEndVerse;
					if ((newScriptText == null || newScriptText.ContainsNoWords) && indexOfFirstElementToRemove >= 0)
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
					if (newScriptText != null)
						newBlock.BlockElements.Add(newScriptText);
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
			{
				// Typically, if the reference text has a character ID representing multiple characters, it will not have
				// an override to specify which one to use in the script.
				SetCharacterIdAndCharacterIdInScript(basedOnBlock.CharacterId, bookNumber, scrVers);
				Delivery = basedOnBlock.Delivery;
			}
			else
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
			ReferenceBlocks = new List<Block>(origList.Select(rb => rb.Clone(ReferenceBlockCloningBehavior.CloneListAndAllReferenceBlocks)));
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

		public VerseRef StartRef(BookScript book)
		{
			return StartRef(book.BookNumber, book.Versification);
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
				// PG-1319: We are switching to English (or *hypothetically* some other reference language that is not backed by English).
				// So whatever we have should have English as its second level. If we don't find a second level, we'll play it
				// safe and just keep what we have. Most likely it is English, leftover from a previous change (to something
				// other than English) where the user opted to not blow away the existing reference text.
				var r = existingReferenceText.ReferenceBlocks.OnlyOrDefault();
				if (r != null)
					SetMatchedReferenceBlock(r);
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

		public Tuple<QuoteInterruption, string> GetNextInterruption(IQuoteInterruptionFinder interruptionFinder, int startCharIndex = 1)
		{
			var verse = InitialVerseNumberOrBridge;
			foreach (var element in BlockElements)
			{
				var text = element as ScriptText;
				if (text != null)
				{
					var interruption = interruptionFinder.GetNextInterruption(text.Content, startCharIndex);
					if (interruption != null)
						return new Tuple<QuoteInterruption, string>(interruption, verse);
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
