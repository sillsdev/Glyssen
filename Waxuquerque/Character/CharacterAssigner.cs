using System.Collections.Generic;
using System.Linq;
using SIL.Scripture;

namespace Waxuquerque.Character
{
	public class CharacterAssigner
	{
		private readonly ICharacterVerseInfo m_cvInfo;

		public CharacterAssigner(ICharacterVerseInfo cvInfo)
		{
			m_cvInfo = cvInfo;
		}

		public void AssignAll(ICollection<BookScript> bookScripts, ScrVers versification, bool setDefaultForMultipleChoiceCharacters, bool overwriteUserConfirmed = false)
		{
			foreach (BookScript bookScript in bookScripts)
				Assign(bookScript, versification, setDefaultForMultipleChoiceCharacters, overwriteUserConfirmed);
		}

		private void Assign(BookScript bookScript, ScrVers versification, bool setDefaultForMultipleChoiceCharacters, bool overwriteUserConfirmed)
		{
			var bookNum = BCVRef.BookToNumber(bookScript.BookId);
			foreach (Block block in bookScript.GetScriptBlocks().Where(b => !b.CharacterIsStandard))
			{
				if (!block.UserConfirmed || overwriteUserConfirmed)
				{
					block.SetCharacterAndDelivery(m_cvInfo.GetCharacters(bookScript.BookId, block.ChapterNumber, block.InitialStartVerseNumber, block.InitialEndVerseNumber, block.LastVerseNum, versification));
				}
				else if (setDefaultForMultipleChoiceCharacters)
				{
					block.UseDefaultForMultipleChoiceCharacter(() => block.GetMatchingCharacter(m_cvInfo, bookNum, versification));
				}
			}

			bookScript.CleanUpMultiBlockQuotes(versification);
		}
	}
}
