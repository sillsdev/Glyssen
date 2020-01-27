using System.Collections.Generic;
using GlyssenEngine.Character;
using GlyssenEngine.Utilities;

namespace GlyssenEngine.UndoActions
{
	public interface ICharacterGroupsUndoAction : IUndoAction
	{
		IEnumerable<CharacterGroup> GroupsAffectedByLastOperation { get; }
	}

	public abstract class CharacterGroupsUndoAction : ICharacterGroupsUndoAction
	{
		private readonly List<CharacterGroup> m_groupsAffectedByLastOperation = new List<CharacterGroup>();

		protected CharacterGroupsUndoAction(bool subsequentActionInSequence = false)
		{
			SubsequentActionInSequence = subsequentActionInSequence;
		}

		protected CharacterGroupsUndoAction(CharacterGroup group, bool subsequentActionInSequence = false) : this(subsequentActionInSequence)
		{
			m_groupsAffectedByLastOperation.Add(group);
		}

		protected CharacterGroupsUndoAction(IEnumerable<CharacterGroup> groups, bool subsequentActionInSequence = false) : this(subsequentActionInSequence)
		{
			m_groupsAffectedByLastOperation.AddRange(groups);
		}

		internal bool SubsequentActionInSequence { get; set; }
		public abstract string Description { get; }
		public bool Undo()
		{
			if (!SubsequentActionInSequence)
				m_groupsAffectedByLastOperation.Clear();
			return PerformUndo();
		}

		public bool Redo()
		{
			if (!SubsequentActionInSequence)
				m_groupsAffectedByLastOperation.Clear();
			return PerformRedo();
		}

		protected abstract bool PerformUndo();
		protected abstract bool PerformRedo();

		public IEnumerable<CharacterGroup> GroupsAffectedByLastOperation
		{
			get { return m_groupsAffectedByLastOperation; }
		}

		protected void AddGroupAffected(CharacterGroup group)
		{
			m_groupsAffectedByLastOperation.Add(group);
		}
	}
}
