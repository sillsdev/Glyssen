namespace Glyssen
{
	public interface IUndoAction
	{
		/// <summary>
		/// Reverses (or "undoes") an action.
		///</summary>
		bool Undo();

		/// <summary> Reapplies (or "redoes") an action. </summary>
		bool Redo();
	}
}
