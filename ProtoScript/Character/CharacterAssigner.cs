using System.Collections.Generic;
using Paratext;

namespace ProtoScript.Character
{
	public class CharacterAssigner
	{
		private readonly ICharacterVerseInfo m_cvInfo;

		public CharacterAssigner(ICharacterVerseInfo cvInfo)
		{
			m_cvInfo = cvInfo;
		}

		public void AssignAll(ICollection<BookScript> bookScripts, ScrVers versification, bool overwriteUserConfirmed = false)
		{
			foreach (BookScript bookScript in bookScripts)
				Assign(bookScript, versification, overwriteUserConfirmed);
		}

		public void Assign(BookScript bookScript, ScrVers versification, bool overwriteUserConfirmed = false)
		{
			foreach (Block block in bookScript.GetScriptBlocks())
			{
				if (block.CharacterIsStandard || (block.UserConfirmed && !overwriteUserConfirmed))
					continue;
				block.SetCharacterAndDelivery(m_cvInfo.GetCharacters(bookScript.BookId, block.ChapterNumber, block.InitialStartVerseNumber, block.InitialEndVerseNumber, block.LastVerse, versification));
			}
		}
	}
}
