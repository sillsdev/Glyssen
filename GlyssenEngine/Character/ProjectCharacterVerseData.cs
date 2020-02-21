using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Glyssen.Shared;
using GlyssenEngine.Script;
using SIL.Extensions;
using SIL.Scripture;
using static System.String;

namespace GlyssenEngine.Character
{
	public class ProjectCharacterVerseData : CharacterVerseData
	{
		private readonly ScrVers m_versification;

		public ProjectCharacterVerseData(TextReader reader, ScrVers versification)
		{
			Debug.Assert(versification != null);
			m_versification = versification;
			LoadData(reader != null ? reader.ReadToEnd() : "");
		}

		public virtual bool AddEntriesFor(int bookNumber, Block block)
		{
			bool added = false;
			foreach (var verse in block.AllVerses)
			{
				if (!ControlCharacterVerseData.Singleton.GetCharacters(bookNumber, block.ChapterNumber, verse, m_versification, true, true)
					.Any(c => c.Character == block.CharacterId && c.Delivery == (block.Delivery ?? Empty)))
				{
					foreach (var v in verse.AllVerseNumbers)
					{
						var verseRef = new VerseRef(bookNumber, block.ChapterNumber, v, m_versification);
						verseRef.ChangeVersification(ScrVers.English);
						added |= base.AddCharacterVerse(new CharacterVerse(verseRef.BBBCCCVVV, block.CharacterId, block.Delivery ?? Empty, Empty, true));
					}
				}
			}

			return added;
		}

		/// <summary>
		/// Gets all project-specific character/delivery pairs for the given verse or bridge.
		/// </summary>
		public override HashSet<CharacterSpeakingMode> GetCharacters(int bookId, int chapter, IVerse verseOrBridge,
			ScrVers versification = null, bool includeAlternatesAndRareQuotes = false, bool includeNarratorOverrides = false)
		{
			Debug.Assert(!includeAlternatesAndRareQuotes);
			Debug.Assert(!includeNarratorOverrides);
			Debug.Assert(versification == null || versification == m_versification);
			var result = new HashSet<CharacterSpeakingMode>(m_characterDeliveryEqualityComparer);
			foreach (var v in verseOrBridge.AllVerseNumbers)
			{
				var verseRef = new VerseRef(bookId, chapter, v, versification ?? m_versification);
				verseRef.ChangeVersification(ScrVers.English);
				result.AddRange(GetSpeakingModesForRef(verseRef));
			}
			return result;
		}

		/// <summary>
		/// Gets all characters completely covered (in this project!) by the given range of verses. If there are multiple verses, only
		/// characters known to speak in ALL the verses will be included in the returned set, with the exception of Interruptions,
		/// which will be included if they occur in any verse. Returned items will include the accompanying deliveries if the
		/// deliveries are consistent across all verses.
		/// This is a simpler implementation than the one used for the control file because project entries are always of type Normal,
		/// and there is no support for project-specific narrator overrides.
		/// It should be noted that for most purposes in production code, this version of the method should not be called, as it will
		/// not produce the desired results if a character is included in the master control file for some/most of the verses in the
		/// range. Instead the single-IVerse version should normally be used (typically via the implementation in
		/// <see cref="CombinedCharacterVerseData"/>).
		/// </summary>
		/// <param name="bookId">1-based index of book, based on prevailing Protestant canonical order</param>
		/// <param name="chapter"></param>
		/// <param name="verses"></param>
		/// <param name="versification">If specified, this must be the project's versification. (Note that the persisted references are
		/// always stored using the English versification. Although for the sake of this project this is a needless hassle and a potential
		/// pitfall, it does to make it possible to copy them over to the control CV file without converting them to English.)</param>
		/// <param name="includeAlternatesAndRareQuotes">Project-specific entries are always <seealso cref="QuoteType.Normal"/>, so this
		/// is assumed <c>false</c></param>
		/// <param name="includeNarratorOverrides">Project-specific narrator overrides are not supported, so this is assumed <c>false</c></param>
		/// <returns></returns>
		public override HashSet<CharacterSpeakingMode> GetCharacters(int bookId, int chapter, IReadOnlyCollection<IVerse> verses,
			ScrVers versification = null, bool includeAlternatesAndRareQuotes = false,
			bool includeNarratorOverrides = false)
		{
			Debug.Assert(!includeAlternatesAndRareQuotes);
			Debug.Assert(!includeNarratorOverrides);
			Debug.Assert(versification == null || versification == m_versification);
			HashSet<CharacterSpeakingMode> result = null;

			foreach (var verse in verses)
			{
				var entriesForCurrentVerseBridge = GetCharacters(bookId, chapter, verse, m_versification);

				if (!entriesForCurrentVerseBridge.Any())
					return entriesForCurrentVerseBridge; // No point going on

				if (result == null)
					result = entriesForCurrentVerseBridge;
				else
					result.IntersectWith(entriesForCurrentVerseBridge);
			}

			return result;
		}

		public override ICharacterDeliveryInfo GetImplicitCharacter(int bookId, int chapter, int startVerse, int endVerse = 0, ScrVers versification = null)
		{
			// There's no way to add a project-specific implicit character
			return null;
		}

		public void WriteToFile(string fullPath)
		{
			RemoveDataAlsoInControlFile();
			File.WriteAllText(fullPath, ToTabDelimited());
		}

		private string ToTabDelimited()
		{
			var sb = new StringBuilder();
			foreach (CharacterVerse cv in GetAllQuoteInfo())
				sb.Append(GetTabDelimitedFields(cv)).Append(Environment.NewLine);
			return sb.ToString();
		}

		private string GetTabDelimitedFields(CharacterVerse cv)
		{
			return cv.BookCode + "\t" + cv.Chapter + "\t" + cv.Verse + "\t" + cv.Character + "\t" + cv.Delivery + "\t" + cv.Alias;
		}

		protected override CharacterVerse CreateCharacterVerse(BCVRef bcvRef, string[] items)
		{
			return new CharacterVerse(bcvRef, items[3], items[4], items[5], true);
		}

		private void RemoveDataAlsoInControlFile()
		{
			RemoveAll(ControlCharacterVerseData.Singleton.GetAllQuoteInfo(), new BcvCharacterDeliveryEqualityComparer());
		}

		public void RemoveAllEntriesForBlock(int bookNum, Block block)
		{
			foreach (var verseNumber in block.AllVerses.SelectMany(v => v.AllVerseNumbers))
				RemoveEntryForBlock(bookNum, block, verseNumber);
		}

		public bool RemoveEntryForBlock(int bookNum, Block block, int verseNumber)
		{
			return Remove(bookNum, block.ChapterNumber, verseNumber, block.CharacterId, block.Delivery ?? "");
		}
	}
}
