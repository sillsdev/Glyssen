using System.Collections.Generic;
using System.Linq;
using GlyssenEngine.Character;
using GlyssenEngine.Quote;
using SIL.Scripture;

namespace Glyssen.Character
{
	public class CharacterAssigner
	{
		private readonly ICharacterVerseInfo m_cvInfo;
		private readonly IQuoteInterruptionFinder m_interruptionFinder;

		public CharacterAssigner(ICharacterVerseInfo cvInfo, IQuoteInterruptionFinder interruptionFinder)
		{
			m_cvInfo = cvInfo;
			m_interruptionFinder = interruptionFinder;
		}

		public void AssignAll(ICollection<BookScript> bookScripts, bool setDefaultForMultipleChoiceCharacters, bool overwriteUserConfirmed = false)
		{
			foreach (BookScript bookScript in bookScripts)
				Assign(bookScript, setDefaultForMultipleChoiceCharacters, overwriteUserConfirmed);
		}

		private void Assign(BookScript bookScript, bool setDefaultForMultipleChoiceCharacters, bool overwriteUserConfirmed)
		{
			var bookNum = BCVRef.BookToNumber(bookScript.BookId);
			foreach (Block block in bookScript.GetScriptBlocks().Where(b => !b.CharacterIsStandard))
			{
				if (!block.UserConfirmed || overwriteUserConfirmed)
				{
					block.SetCharacterAndDelivery(m_interruptionFinder, m_cvInfo.GetCharacters(bookNum, block.ChapterNumber, block.AllVerses, bookScript.Versification));
				}
				else if (setDefaultForMultipleChoiceCharacters)
				{
					block.UseDefaultForMultipleChoiceCharacter(() => block.GetMatchingCharacter(m_cvInfo, bookNum, bookScript.Versification));
				}
			}

			bookScript.CleanUpMultiBlockQuotes();
		}
	}
}
