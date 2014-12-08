using System.Collections.Generic;

namespace ProtoScript
{
	public class CharacterAssigner
	{
		private readonly ICharacterVerseInfo m_cvInfo;

		public CharacterAssigner(ICharacterVerseInfo cvInfo)
		{
			m_cvInfo = cvInfo;
		}

		public void AssignAll(ICollection<BookScript> bookScripts, bool overwriteUserConfirmed = false)
		{
			foreach (BookScript bookScript in bookScripts)
				Assign(bookScript, overwriteUserConfirmed);
		}

		public void Assign(BookScript bookScript, bool overwriteUserConfirmed = false)
		{
			foreach (Block block in bookScript.ScriptBlocks)
			{
				if (block.CharacterIsStandard || (block.UserConfirmed && !overwriteUserConfirmed))
					continue;
				block.SetCharacterAndDelivery(m_cvInfo.GetCharacters(bookScript.BookId, block.ChapterNumber, block.InitialVerseNumber));
			}
		}
	}
}
