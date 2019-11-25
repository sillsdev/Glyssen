using System;
using System.Collections.Generic;
using System.Linq;
using SIL.Extensions;
using SIL.ObjectModel;

namespace GlyssenEngine.Utilities
{
	public class UndoActionSequence<T> where T : IUndoAction
	{
		private readonly T[] m_actions;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="actions">Individual undo actions that make up the sequence, in the original order (they will be undone in reverse order).
		/// Note that by default the description of the last action will be used as the description for this sequence.</param>
		public UndoActionSequence(params T[] actions)
		{
			m_actions = actions.ToArray();
			if (!m_actions.Any())
				throw new ArgumentException("At least one undoable action must be provided");
			Description = m_actions.Last().Description;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="actions">Individual undo actions that make up the sequence, in the original order (they will be undone in reverse order).
		/// Note that by default the description of the last action will be used as the description for this sequence.</param>
		public UndoActionSequence(IEnumerable<T> actions) : this (actions.ToArray())
		{
		}

		protected ReadOnlyList<T> Actions
		{
			get { return m_actions.ToReadOnlyList(); }
		}

		public virtual string Description { get; set; }

		public virtual bool Undo()
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

		public virtual bool Redo()
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
