namespace Waxuquerque.Utilities
{
	public interface IUndoAction
	{
		/// <summary>Gets a description suitable for presenting in the UI to indicate the action to be undone or redone<summary>
		string Description { get; }

		/// <summary>
		/// Reverses (or "undoes") an action.
		///</summary>
		bool Undo();

		/// <summary> Reapplies (or "redoes") an action. </summary>
		bool Redo();
	}
}
