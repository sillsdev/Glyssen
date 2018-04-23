using System.Collections.Generic;
using System.Linq;
using Waxuquerque.Character;
using Waxuquerque.Utilities;

namespace Waxuquerque.ViewModel.Undo
{
	public class CharacterGroupUndoActionSequence : UndoActionSequence<CharacterGroupsUndoAction>, ICharacterGroupsUndoAction
	{
		private bool m_lastOperationSucceeded = true;

		public CharacterGroupUndoActionSequence(params CharacterGroupsUndoAction[] actions) : base(actions)
		{
			actions[0].SubsequentActionInSequence = true;
			foreach (var action in actions.Skip(1))
				action.SubsequentActionInSequence = true;
		}

		public IEnumerable<CharacterGroup> GroupsAffectedByLastOperation
		{
			get
			{
				var affectedGroups = new HashSet<CharacterGroup>();
				if (m_lastOperationSucceeded)
				{
					foreach (var group in Actions.SelectMany(action => action.GroupsAffectedByLastOperation))
						affectedGroups.Add(group);
				}
				return affectedGroups;
			}
		}

		public override bool Undo()
		{
			// Prepare for Undo (the last shall be first)
			// The order here is important, because there might only be one action, in which case it should
			// be marked as NOT the last in the sequence.
			Actions.First().SubsequentActionInSequence = true;
			Actions.Last().SubsequentActionInSequence = false;

			m_lastOperationSucceeded = base.Undo();

			// Prepare for possible future Redo (the first is first again)
			// Again, order matters.
			Actions.Last().SubsequentActionInSequence = true;
			Actions.First().SubsequentActionInSequence = false;

			return m_lastOperationSucceeded;
		}

		public override bool Redo()
		{
			m_lastOperationSucceeded = base.Redo();
			return m_lastOperationSucceeded;
		}
	}
}
