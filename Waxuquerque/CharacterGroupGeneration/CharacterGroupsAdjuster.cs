using System.Collections.Generic;
using System.Linq;
using SIL.Extensions;
using Waxuquerque.Character;

namespace Waxuquerque.CharacterGroupGeneration
{
	public class CharacterGroupsAdjuster
	{
		private readonly Project m_project;
		private readonly HashSet<string> m_charactersNotCoveredByAnyGroup;
		private readonly HashSet<string> m_charactersNoLongerInUse;

		public CharacterGroupsAdjuster(Project project)
		{
			m_project = project;
			var characterGroups = m_project.CharacterGroupList.CharacterGroups;
			var charactersInProject = m_project.AllCharacterIds;
			m_charactersNotCoveredByAnyGroup = new HashSet<string>(charactersInProject.Where(c => characterGroups.All(g => !g.CharacterIds.Contains(c))));
			m_charactersNoLongerInUse = new HashSet<string>(characterGroups.SelectMany(g => g.CharacterIds).Where(c => !charactersInProject.Contains(c)));
		}

		public IEnumerable<string> CharactersNotCoveredByAnyGroup { get { return m_charactersNotCoveredByAnyGroup; } }
		public IEnumerable<string> CharactersNoLongerInUse { get { return m_charactersNoLongerInUse; } }
		public IEnumerable<CharacterGroup> CharacterGroupsToRemove
		{
			get
			{
				return m_project.CharacterGroupList.CharacterGroups
					.Where(g => !g.AssignedToCameoActor && g.CharacterIds.Any() && g.CharacterIds.All(c => m_charactersNoLongerInUse.Contains(c)));
			}
		}

		public bool NewBooksHaveBeenIncluded
		{
			get
			{
				return m_project.IncludedBooks.Select(b => CharacterVerseData.GetStandardCharacterId(b.BookId, CharacterVerseData.StandardCharacter.Narrator))
					.Any(narrator => !m_project.CharacterGroupList.CharacterGroups.SelectMany(g => g.CharacterIds).Contains(narrator));
			}
		}

		public bool BooksHaveBeenExcluded
		{
			get
			{
				return m_project.CharacterGroupList.CharacterGroups.SelectMany(g => g.CharacterIds).Any(c => CharacterVerseData.IsCharacterStandard(c) &&
					!m_project.IncludedBooks.Select(b => b.BookId).Contains(CharacterVerseData.GetBookCodeFromStandardCharacterId(c)));
			}
		}

		/// <summary>
		/// For now, this is only used in tests. If we decide that we need to distinguish between different cases to decide how strongly
		/// to recommned doing a full regeneration, we'll need to look at the specifics here to see if they meet the need.
		/// </summary>
		public bool FullRegenerateRecommended
		{
			get
			{
				return NewBooksHaveBeenIncluded || BooksHaveBeenExcluded || CharacterGroupsToRemove.Any() ||
					CharactersNotCoveredByAnyGroup.Count() + CharactersNoLongerInUse.Count() > 4;
			}
		}

		public bool GroupsAreNotInSynchWithData
		{
			get
			{
				return NewBooksHaveBeenIncluded || BooksHaveBeenExcluded || CharacterGroupsToRemove.Any() ||
					CharactersNotCoveredByAnyGroup.Any() || CharactersNoLongerInUse.Any();
			}
		}

		public void MakeMinimalAdjustments()
		{
			if (m_charactersNotCoveredByAnyGroup.Any())
			{
				var characterGroups = m_project.CharacterGroupList.CharacterGroups;
				var newGroup = new CharacterGroup(m_project);
				characterGroups.Add(newGroup);
				newGroup.CharacterIds.AddRange(m_charactersNotCoveredByAnyGroup);
				newGroup.SetGroupIdLabel();
				m_project.CharacterGroupList.UpdateGroupIdNumbers();
				m_charactersNotCoveredByAnyGroup.Clear();
			}
			foreach (var character in m_charactersNoLongerInUse)
			{
				var group = m_project.CharacterGroupList.GroupContainingCharacterId(character);
				group.CharacterIds.Remove(character);
				if (!group.CharacterIds.Any() && !group.AssignedToCameoActor)
					m_project.CharacterGroupList.CharacterGroups.Remove(group);
			}
			m_charactersNoLongerInUse.Clear();
		}

		public void FinalizeRegenerationOfGroups()
		{
			m_charactersNotCoveredByAnyGroup.Clear();
			m_charactersNoLongerInUse.Clear();
		}
	}
}
