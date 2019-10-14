using System.Collections.Generic;
using System.Linq;
using SIL.ObjectModel;
using SIL.Scripture;

namespace Glyssen.Character
{
	class CombinedCharacterVerseData : ICharacterVerseInfo
	{
		private readonly Project m_project;
		private readonly IEqualityComparer<ICharacterDeliveryInfo> m_characterDeliveryEqualityComparer = new CharacterDeliveryEqualityComparer();

		public CombinedCharacterVerseData(Project project)
		{
			m_project = project;
		}

		public IEnumerable<CharacterVerse> GetCharacters(int bookId, int chapter, int initialStartVerse, int initialEndVerse = 0,
			int finalVerse = 0, ScrVers versification = null, bool includeAlternatesAndRareQuotes = false, bool includeNarratorOverrides = false)
		{
			IEnumerable<CharacterVerse> project = m_project.ProjectCharacterVerseData.GetCharacters(bookId, chapter, initialStartVerse,
				initialEndVerse, finalVerse, versification);
			IEnumerable<CharacterVerse> control = ControlCharacterVerseData.Singleton.GetCharacters(bookId, chapter, initialStartVerse,
				initialEndVerse, finalVerse, versification, includeAlternatesAndRareQuotes, includeNarratorOverrides);
			return project.Union(control);
		}

		public ICharacterDeliveryInfo GetImplicitCharacter(int bookId, int chapter, int startVerse, int endVerse = 0, ScrVers versification = null)
		{
			return ControlCharacterVerseData.Singleton.GetImplicitCharacter(bookId, chapter, startVerse, endVerse, versification);
		}

		public IEnumerable<CharacterVerse> GetAllQuoteInfo()
		{
			IEnumerable<CharacterVerse> project = m_project.ProjectCharacterVerseData.GetAllQuoteInfo();
			IEnumerable<CharacterVerse> control = ControlCharacterVerseData.Singleton.GetAllQuoteInfo();
			return project.Union(control);
		}

		public IEnumerable<CharacterVerse> GetAllQuoteInfo(int bookNum)
		{
			IEnumerable<CharacterVerse> project = m_project.ProjectCharacterVerseData.GetAllQuoteInfo(bookNum);
			IEnumerable<CharacterVerse> control = ControlCharacterVerseData.Singleton.GetAllQuoteInfo(bookNum);
			return project.Union(control);
		}

		public IReadOnlySet<ICharacterDeliveryInfo> GetUniqueCharacterAndDeliveries()
		{
			var result = ControlCharacterVerseData.Singleton.GetUniqueCharacterAndDeliveries();
			var project = m_project.ProjectCharacterVerseData.GetUniqueCharacterAndDeliveries();
			if (project.Any()) // Since this is rare, we can save the cost of a union and two set creations
				result = new ReadOnlySet<ICharacterDeliveryInfo>(new HashSet<ICharacterDeliveryInfo>(result.Union(project, m_characterDeliveryEqualityComparer)));
			return result;
		}

		public ISet<ICharacterDeliveryInfo> GetUniqueCharacterAndDeliveries(string bookCode)
		{
			var result = ControlCharacterVerseData.Singleton.GetUniqueCharacterAndDeliveries(bookCode);
			var project = m_project.ProjectCharacterVerseData.GetUniqueCharacterAndDeliveries(bookCode);
			result.UnionWith(project);
			return result;
		}

		public ISet<ICharacterDeliveryInfo> GetUniqueCharacterAndDeliveries(string bookCode, int chapter)
		{
			var result = ControlCharacterVerseData.Singleton.GetUniqueCharacterAndDeliveries(bookCode, chapter);
			var project = m_project.ProjectCharacterVerseData.GetUniqueCharacterAndDeliveries(bookCode, chapter);
			result.UnionWith(project);
			return result;
		}

		public ISet<string> GetUniqueDeliveries()
		{
			var result = ControlCharacterVerseData.Singleton.GetUniqueDeliveries();
			var project = m_project.ProjectCharacterVerseData.GetUniqueDeliveries();
			result.UnionWith(project);
			return result;
		}
	}
}
