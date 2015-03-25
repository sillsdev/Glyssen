using System.Collections.Generic;
using System.Linq;
using SIL.ScriptureUtils;

namespace ProtoScript.Character
{
	class CombinedCharacterVerseData : ICharacterVerseInfo
	{
		private readonly Project m_project;

		public CombinedCharacterVerseData(Project project)
		{
			m_project = project;
		}

		public IEnumerable<CharacterVerse> GetCharacters(string bookId, int chapter, int initialStartVerse, int initialEndVerse = 0, int finalVerse = 0)
		{
			return GetCharacters(BCVRef.BookToNumber(bookId), chapter, initialStartVerse, initialEndVerse, finalVerse);
		}

		public IEnumerable<CharacterVerse> GetCharacters(int bookId, int chapter, int initialStartVerse, int initialEndVerse = 0, int finalVerse = 0)
		{
			IEnumerable<CharacterVerse> project = m_project.ProjectCharacterVerseData.GetCharacters(bookId, chapter, initialStartVerse, initialEndVerse, finalVerse);
			IEnumerable<CharacterVerse> control = ControlCharacterVerseData.Singleton.GetCharacters(bookId, chapter, initialStartVerse, initialEndVerse, finalVerse);
			return project.Union(control);
		}

		public IEnumerable<CharacterVerse> GetAllQuoteInfo()
		{
			IEnumerable<CharacterVerse> project = m_project.ProjectCharacterVerseData.GetAllQuoteInfo();
			IEnumerable<CharacterVerse> control = ControlCharacterVerseData.Singleton.GetAllQuoteInfo();
			return project.Union(control);
		}

		public IEnumerable<CharacterVerse> GetAllQuoteInfo(string bookId)
		{
			IEnumerable<CharacterVerse> project = m_project.ProjectCharacterVerseData.GetAllQuoteInfo(bookId);
			IEnumerable<CharacterVerse> control = ControlCharacterVerseData.Singleton.GetAllQuoteInfo(bookId);
			return project.Union(control);
		}

		public IEnumerable<CharacterVerse> GetUniqueCharacterAndDeliveries()
		{
			IEnumerable<CharacterVerse> project = m_project.ProjectCharacterVerseData.GetUniqueCharacterAndDeliveries();
			IEnumerable<CharacterVerse> control = ControlCharacterVerseData.Singleton.GetUniqueCharacterAndDeliveries();
			return project.Union(control);
		}

		public IEnumerable<CharacterVerse> GetUniqueCharacterAndDeliveries(string bookCode)
		{
			IEnumerable<CharacterVerse> project = m_project.ProjectCharacterVerseData.GetUniqueCharacterAndDeliveries(bookCode);
			IEnumerable<CharacterVerse> control = ControlCharacterVerseData.Singleton.GetUniqueCharacterAndDeliveries(bookCode);
			return project.Union(control);
		}

		public IEnumerable<string> GetUniqueDeliveries()
		{
			IEnumerable<string> project = m_project.ProjectCharacterVerseData.GetUniqueDeliveries();
			IEnumerable<string> control = ControlCharacterVerseData.Singleton.GetUniqueDeliveries();
			return project.Union(control);
		}
	}
}
