using System.Collections.Generic;

namespace Glyssen
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

		public bool CanUndo { get { return true; } }
		public bool CanRedo { get { return true; } }

		public bool Undo()
		{
			return true;
		}

		public bool Redo()
		{
			return true;
		}

		public void Push()
		{

		}
	}
}
