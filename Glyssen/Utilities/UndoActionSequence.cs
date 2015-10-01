using System;
using System.Linq;

namespace Glyssen.Utilities
{
	public class UndoActionSequence : IUndoAction
	{
		private readonly IUndoAction[] m_actions;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="actions">Individual undo actions that make up the sequence, in the original order (they will be undone in reverse order).
		/// Note that by default the description of the last action will be used as the description for this sequence.</param>
		public UndoActionSequence(params IUndoAction[] actions)
		{
			if (!actions.Any())
				throw new ArgumentException("At least one undoable action must be provided");
			m_actions = actions;
			Description = m_actions.Last().Description;
		}

		public string Description { get; set; }

		public bool Undo()
		{
			int i;
			bool result = true;
			for (i = m_actions.Length - 1; i >= 0; i--)
			{
				result = m_actions[i].Undo();
				if (!result)
					break;
			}
			if (!result)
				while (++i < m_actions.Length && m_actions[i].Redo());
			return result;
		}

		public bool Redo()
		{
			int i;
			bool result = true;
			for (i = 0; i < m_actions.Length; i++)
			{
				result = m_actions[i].Redo();
				if (!result)
					break;
			}
			if (!result)
				while (--i >= 0 && m_actions[i].Undo());
			return result;
		}
	}
}
