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

		public IEnumerable<CharacterSpeakingMode> GetCharacters(int bookId, int chapter, int initialStartVerse, int initialEndVerse = 0,
			int finalVerse = 0, ScrVers versification = null, bool includeAlternatesAndRareQuotes = false, bool includeNarratorOverrides = false)
		{
			IEnumerable<CharacterSpeakingMode> project = m_project.ProjectCharacterVerseData.GetCharacters(bookId, chapter, initialStartVerse,
				initialEndVerse, finalVerse, versification);
			IEnumerable<CharacterSpeakingMode> control = ControlCharacterVerseData.Singleton.GetCharacters(bookId, chapter, initialStartVerse,
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

		public IReadOnlySet<ICharacterDeliveryInfo> GetUniqueCharacterDeliveryAliasInfo()
		{
			var result = ControlCharacterVerseData.Singleton.GetUniqueCharacterDeliveryAliasInfo();
			var project = m_project.ProjectCharacterVerseData.GetUniqueCharacterDeliveryAliasInfo();
			if (project.Any()) // Since this is rare, we can save the cost of a union and two set creations
				result = new ReadOnlySet<ICharacterDeliveryInfo>(new HashSet<ICharacterDeliveryInfo>(result.Union(project, new CharacterDeliveryAliasEqualityComparer())));
			return result;
		}

		public ISet<ICharacterDeliveryInfo> GetUniqueCharacterDeliveryInfo(string bookCode)
		{
			var result = ControlCharacterVerseData.Singleton.GetUniqueCharacterDeliveryInfo(bookCode);
			var project = m_project.ProjectCharacterVerseData.GetUniqueCharacterDeliveryInfo(bookCode);
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
