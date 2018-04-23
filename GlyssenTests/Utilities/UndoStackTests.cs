using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Waxuquerque.Utilities;

namespace GlyssenTests.Utilities
{
	[TestFixture]
	class UndoStackTests
	{
		private List<string> m_actionsTaken;

		[SetUp]
		public void Setup()
		{
			m_actionsTaken = new List<string>();
		}

		[Test]
		public void Push_EmptyStack_CanUndoButNotRedo()
		{
			var stack = new UndoStack<IUndoAction>();
			stack.Push(NewAction());
			Assert.IsTrue(stack.CanUndo);
			Assert.IsFalse(stack.CanRedo);
		}

		[Test]
		public void GetUndoDescriptions_EmptyStack_ReturnsEmptyList()
		{
			var stack = new UndoStack<IUndoAction>();
			Assert.AreEqual(0, stack.UndoDescriptions.Count);
		}

		[Test]
		public void GetUndoDescriptions_AllItemsAreUndoable_ReturnsDescriptionsForAllItems()
		{
			var stack = new UndoStack<IUndoAction>();
			stack.Push(NewAction("Assigning voice actor: Fred Flintstone"));
			stack.Push(NewAction("Removing voice actor assignment"));
			stack.Push(NewAction("Regenerating character groups"));
			var descriptions = stack.UndoDescriptions;
			Assert.AreEqual(3, descriptions.Count);
			int i = 0;
			Assert.AreEqual("Regenerating character groups", descriptions[i++]);
			Assert.AreEqual("Removing voice actor assignment", descriptions[i++]);
			Assert.AreEqual("Assigning voice actor: Fred Flintstone", descriptions[i++]);
		}

		[Test]
		public void GetUndoDescriptions_NoItemsAreUndoable_ReturnsEmptyList()
		{
			var stack = new UndoStack<IUndoAction>();
			stack.Push(NewAction("Assigning voice actor: Fred Flintstone"));
			stack.Push(NewAction("Removing voice actor assignment"));
			stack.Push(NewAction("Regenerating character groups"));
			stack.Undo();
			stack.Undo();
			stack.Undo();
			Assert.AreEqual(0, stack.UndoDescriptions.Count);
		}

		[Test]
		public void GetUndoDescriptions_StackContainsUndoableAndRedoableItams_ReturnsOnlyUndoableItems()
		{
			var stack = new UndoStack<IUndoAction>();
			stack.Push(NewAction("Assigning voice actor: Fred Flintstone"));
			stack.Push(NewAction("Removing voice actor assignment"));
			stack.Push(NewAction("Regenerating character groups"));
			stack.Undo();
			var descriptions = stack.UndoDescriptions;
			Assert.AreEqual(2, descriptions.Count);
			int i = 0;
			Assert.AreEqual("Removing voice actor assignment", descriptions[i++]);
			Assert.AreEqual("Assigning voice actor: Fred Flintstone", descriptions[i++]);
		}

		[Test]
		public void GetRedoDescriptions_EmptyStack_ReturnsEmptyList()
		{
			var stack = new UndoStack<IUndoAction>();
			Assert.AreEqual(0, stack.RedoDescriptions.Count);
		}

		[Test]
		public void GetRedoDescriptions_AllItemsAreRedoable_ReturnsDescriptionsForAllItems()
		{
			var stack = new UndoStack<IUndoAction>();
			stack.Push(NewAction("Assigning voice actor: Fred Flintstone"));
			stack.Push(NewAction("Removing voice actor assignment"));
			stack.Push(NewAction("Regenerating character groups"));
			stack.Undo();
			stack.Undo();
			stack.Undo();
			var descriptions = stack.RedoDescriptions;
			Assert.AreEqual(3, descriptions.Count);
			int i = 0;
			Assert.AreEqual("Assigning voice actor: Fred Flintstone", descriptions[i++]);
			Assert.AreEqual("Removing voice actor assignment", descriptions[i++]);
			Assert.AreEqual("Regenerating character groups", descriptions[i++]);
		}

		[Test]
		public void GetRedoDescriptions_NoItemsAreRedoable_ReturnsEmptyList()
		{
			var stack = new UndoStack<IUndoAction>();
			stack.Push(NewAction("Assigning voice actor: Fred Flintstone"));
			stack.Push(NewAction("Removing voice actor assignment"));
			stack.Push(NewAction("Regenerating character groups"));
			Assert.AreEqual(0, stack.RedoDescriptions.Count);
		}

		[Test]
		public void GetRedoDescriptions_StackContainsUndoableAndRedoableItams_ReturnsOnlyRedoableItems()
		{
			var stack = new UndoStack<IUndoAction>();
			stack.Push(NewAction("Assigning voice actor: Fred Flintstone"));
			stack.Push(NewAction("Removing voice actor assignment"));
			stack.Push(NewAction("Regenerating character groups"));
			stack.Undo();
			stack.Undo();
			var descriptions = stack.RedoDescriptions;
			Assert.AreEqual(2, descriptions.Count);
			int i = 0;
			Assert.AreEqual("Removing voice actor assignment", descriptions[i++]);
			Assert.AreEqual("Regenerating character groups", descriptions[i++]);
		}

		[Test]
		public void Push_StackHasUndoneActions_ActionsAfterCurrentPositionAreRemoved()
		{
			var stack = new UndoStack<IUndoAction>();
			stack.Push(NewAction("a"));
			stack.Push(NewAction("b"));
			stack.Push(NewAction("c"));
			stack.Undo();
			stack.Undo();
			stack.Push(NewAction("d"));
			stack.Undo();
			stack.Undo();
			Assert.IsFalse(stack.CanUndo);
			stack.Redo();
			stack.Redo();
			Assert.IsFalse(stack.CanRedo);
			Assert.AreEqual(6, m_actionsTaken.Count);
			int i = 0;
			Assert.AreEqual("Undo c", m_actionsTaken[i++]);
			Assert.AreEqual("Undo b", m_actionsTaken[i++]);
			Assert.AreEqual("Undo d", m_actionsTaken[i++]);
			Assert.AreEqual("Undo a", m_actionsTaken[i++]);
			Assert.AreEqual("Redo a", m_actionsTaken[i++]);
			Assert.AreEqual("Redo d", m_actionsTaken[i++]);
		}

		[Test]
		public void CanUndo_EmptyStack_ReturnsFalse()
		{
			var stack = new UndoStack<IUndoAction>();
			Assert.IsFalse(stack.CanUndo);
		}

		[Test]
		public void CanUndo_AfterUndoOfOnlyItemOnStack_ReturnsFalse()
		{
			var stack = new UndoStack<IUndoAction>();
			stack.Push(NewAction());
			stack.Undo();
			Assert.IsFalse(stack.CanUndo);
		}

		[Test]
		public void CanUndo_AfterPushingItemOnStack_ReturnsTrue()
		{
			var stack = new UndoStack<IUndoAction>();
			stack.Push(NewAction());
			Assert.IsTrue(stack.CanUndo);
		}

		[Test]
		public void Undo_EmptyStack_ThrowsInvalidOperationException()
		{
			var stack = new UndoStack<IUndoAction>();
			Assert.Throws(typeof(InvalidOperationException), () => { stack.Undo(); });
		}

		[Test]
		public void Redo_EmptyStack_ThrowsInvalidOperationException()
		{
			var stack = new UndoStack<IUndoAction>();
			Assert.Throws(typeof(InvalidOperationException), () => { stack.Redo(); });
		}

		[Test]
		public void Redo_OneUndoneItemOnStack_ActionRedone()
		{
			var stack = new UndoStack<IUndoAction>();
			stack.Push(NewAction("a"));
			stack.Undo();
			Assert.IsTrue(stack.Redo());
			Assert.AreEqual(2, m_actionsTaken.Count);
			Assert.AreEqual("Redo a", m_actionsTaken[1]);
		}

		[Test]
		public void Undo_OneCurrentItemOnStack_ActionUndone()
		{
			var stack = new UndoStack<IUndoAction>();
			stack.Push(NewAction("a"));
			Assert.IsTrue(stack.Undo());
			Assert.AreEqual(1, m_actionsTaken.Count);
			Assert.AreEqual("Undo a", m_actionsTaken[0]);
		}

		[Test]
		public void Undo_UndoActionReturnsFalse_ReturnsFalseAndPreventsFurtherUndo()
		{
			var stack = new UndoStack<IUndoAction>();
			stack.Push(NewAction("a"));
			stack.Push(NewAction("b", false));
			stack.Push(NewAction("c"));
			Assert.IsTrue(stack.Undo());
			Assert.IsFalse(stack.Undo());
			Assert.IsTrue(stack.CanRedo);
			Assert.IsFalse(stack.CanUndo);
			Assert.IsTrue(stack.Redo());
			Assert.AreEqual(3, m_actionsTaken.Count);
			int i = 0;
			Assert.AreEqual("Undo c", m_actionsTaken[i++]);
			Assert.AreEqual("Failed to undo b", m_actionsTaken[i++]);
			Assert.AreEqual("Redo c", m_actionsTaken[i++]);
		}

		[Test]
		public void Undo_Multiple_ActionsUndoneInReverseOrderOfPush()
		{
			var stack = new UndoStack<IUndoAction>();
			stack.Push(NewAction("a"));
			stack.Push(NewAction("b"));
			stack.Push(NewAction("c"));
			Assert.IsTrue(stack.Undo());
			Assert.IsTrue(stack.Undo());
			Assert.IsTrue(stack.Undo());
			Assert.AreEqual(3, m_actionsTaken.Count);
			int i = 0;
			Assert.AreEqual("Undo c", m_actionsTaken[i++]);
			Assert.AreEqual("Undo b", m_actionsTaken[i++]);
			Assert.AreEqual("Undo a", m_actionsTaken[i++]);
		}

		[Test]
		public void CanRedo_EmptyStack_ReturnsFalse()
		{
			var stack = new UndoStack<IUndoAction>();
			Assert.IsFalse(stack.CanRedo);
		}

		[Test]
		public void CanRedo_AfterUndo_ReturnsTrue()
		{
			var stack = new UndoStack<IUndoAction>();
			stack.Push(NewAction());
			stack.Undo();
			Assert.IsTrue(stack.CanRedo);
		}

		private IUndoAction NewAction(string description = "", bool expectedUndoResult = true, bool expectedRedoResult = true)
		{
			var action = MockRepository.GenerateMock<IUndoAction>();
			action.Stub(a => a.Description).Return(description);
			action.Stub(a => a.Undo()).Do(new Func<bool>(() =>
			{
				m_actionsTaken.Add((!expectedUndoResult ? "Failed to undo " : "Undo ") + description);
				return expectedUndoResult;
			}));
			action.Stub(a => a.Redo()).Do(new Func<bool>(() =>
			{
				m_actionsTaken.Add((!expectedRedoResult ? "Failed to redo " : "Redo ") + description);
				return expectedRedoResult;
			}));
			return action;
		}

		[Test]
		public void Peek_EmptyStack_ReturnsNull()
		{
			var stack = new UndoStack<IUndoAction>();
			Assert.IsNull(stack.Peek());
		}

		[Test]
		public void Peek_AfterUndoOfOnlyItemOnStack_ReturnsNull()
		{
			var stack = new UndoStack<IUndoAction>();
			stack.Push(NewAction());
			stack.Undo();
			Assert.IsNull(stack.Peek());
		}

		[Test]
		public void Peek_AfterPushingItemOnStack_ReturnsItem()
		{
			var stack = new UndoStack<IUndoAction>();
			var action = NewAction();
			stack.Push(action);
			Assert.AreEqual(action, stack.Peek());
		}

		[Test]
		public void Peek_AfterUndoWithAdditionalItemsOnStack_ReturnsNextUndoableItem()
		{
			var stack = new UndoStack<IUndoAction>();
			var action = NewAction();
			stack.Push(action);
			stack.Push(NewAction());
			stack.Undo();
			Assert.AreEqual(action, stack.Peek());
		}
	}
}
