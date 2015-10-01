using System;
using System.Collections.Generic;
using System.Linq;

namespace Glyssen.Utilities
{
	/// <summary>
	/// Collection of undoable/redoable IUndoActions with a "pointer" to the current position, so that if a new action
	/// is pushed when the current pointer is not at the top of the stack, any items following that postion are pruned
	/// from the collection.
	/// </summary>
	public class UndoStack
	{
		private List<IUndoAction> m_actions = new List<IUndoAction>();
		private int m_current = 0;

		public bool CanUndo { get { return m_current > 0; } }
		public bool CanRedo { get { return m_current < m_actions.Count; } }
		
		public List<string> UndoDescriptions
		{
			get { return m_actions.Take(m_current).Reverse().Select(a => a.Description).ToList(); }
		}

		public List<string> RedoDescriptions
		{
			get { return m_actions.Skip(m_current).Select(a => a.Description).ToList(); }
		}

		public bool Undo()
		{
			if (!CanUndo)
				throw new InvalidOperationException("No action to undo.");
			if (m_actions[--m_current].Undo())
				return true;
			m_actions.RemoveRange(0, m_current + 1);
			m_current = 0;
			return false;
		}

		public bool Redo()
		{
			if (!CanRedo)
				throw new InvalidOperationException("No action to redo.");
			return m_actions[m_current++].Redo();
		}

		public void Push(IUndoAction action)
		{
			while (CanRedo)
				m_actions.RemoveAt(m_current);
			m_actions.Add(action);
			m_current = m_actions.Count;
		}
	}
}
