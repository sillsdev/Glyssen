using System.Collections.Generic;
using System.Linq;
using SIL.Scripture;

namespace Glyssen.Character
{
	class CombinedCharacterVerseData : ICharacterVerseInfo
	{
		private readonly Project m_project;

		public CombinedCharacterVerseData(Project project)
		{
			m_project = project;
		}

		public IEnumerable<CharacterVerse> GetCharacters(int bookId, int chapter, int initialStartVerse, int initialEndVerse = 0, int finalVerse = 0, ScrVers versification = null)
		{
			IEnumerable<CharacterVerse> project = m_project.ProjectCharacterVerseData.GetCharacters(bookId, chapter, initialStartVerse, initialEndVerse, finalVerse, versification);
			IEnumerable<CharacterVerse> control = ControlCharacterVerseData.Singleton.GetCharacters(bookId, chapter, initialStartVerse, initialEndVerse, finalVerse, versification);
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

		public IEnumerable<CharacterVerse> GetUniqueCharacterAndDeliveries(string bookCode, int chapter)
		{
			IEnumerable<CharacterVerse> project = m_project.ProjectCharacterVerseData.GetUniqueCharacterAndDeliveries(bookCode, chapter);
			IEnumerable<CharacterVerse> control = ControlCharacterVerseData.Singleton.GetUniqueCharacterAndDeliveries(bookCode, chapter);
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
