using System;
using System.Collections.Generic;
using GlyssenEngine.UndoActions;
using NUnit.Framework;
using Rhino.Mocks;

namespace GlyssenEngineTests.UndoActionsTests
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
			Assert.That(stack.CanUndo, Is.True);
			Assert.That(stack.CanRedo, Is.False);
		}

		[Test]
		public void GetUndoDescriptions_EmptyStack_ReturnsEmptyList()
		{
			var stack = new UndoStack<IUndoAction>();
			Assert.That(stack.UndoDescriptions.Count, Is.EqualTo(0));
		}

		[Test]
		public void GetUndoDescriptions_AllItemsAreUndoable_ReturnsDescriptionsForAllItems()
		{
			var stack = new UndoStack<IUndoAction>();
			stack.Push(NewAction("Assigning voice actor: Fred Flintstone"));
			stack.Push(NewAction("Removing voice actor assignment"));
			stack.Push(NewAction("Regenerating character groups"));
			var descriptions = stack.UndoDescriptions;
			Assert.That(descriptions.Count, Is.EqualTo(3));
			int i = 0;
			Assert.That(descriptions[i++], Is.EqualTo("Regenerating character groups"));
			Assert.That(descriptions[i++], Is.EqualTo("Removing voice actor assignment"));
			Assert.That(descriptions[i++], Is.EqualTo("Assigning voice actor: Fred Flintstone"));
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
			Assert.That(stack.UndoDescriptions.Count, Is.EqualTo(0));
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
			Assert.That(descriptions.Count, Is.EqualTo(2));
			int i = 0;
			Assert.That(descriptions[i++], Is.EqualTo("Removing voice actor assignment"));
			Assert.That(descriptions[i++], Is.EqualTo("Assigning voice actor: Fred Flintstone"));
		}

		[Test]
		public void GetRedoDescriptions_EmptyStack_ReturnsEmptyList()
		{
			var stack = new UndoStack<IUndoAction>();
			Assert.That(stack.RedoDescriptions.Count, Is.EqualTo(0));
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
			Assert.That(descriptions.Count, Is.EqualTo(3));
			int i = 0;
			Assert.That(descriptions[i++], Is.EqualTo("Assigning voice actor: Fred Flintstone"));
			Assert.That(descriptions[i++], Is.EqualTo("Removing voice actor assignment"));
			Assert.That(descriptions[i++], Is.EqualTo("Regenerating character groups"));
		}

		[Test]
		public void GetRedoDescriptions_NoItemsAreRedoable_ReturnsEmptyList()
		{
			var stack = new UndoStack<IUndoAction>();
			stack.Push(NewAction("Assigning voice actor: Fred Flintstone"));
			stack.Push(NewAction("Removing voice actor assignment"));
			stack.Push(NewAction("Regenerating character groups"));
			Assert.That(stack.RedoDescriptions.Count, Is.EqualTo(0));
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
			Assert.That(descriptions.Count, Is.EqualTo(2));
			int i = 0;
			Assert.That(descriptions[i++], Is.EqualTo("Removing voice actor assignment"));
			Assert.That(descriptions[i++], Is.EqualTo("Regenerating character groups"));
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
			Assert.That(stack.CanUndo, Is.False);
			stack.Redo();
			stack.Redo();
			Assert.That(stack.CanRedo, Is.False);
			Assert.That(m_actionsTaken.Count, Is.EqualTo(6));
			int i = 0;
			Assert.That(m_actionsTaken[i++], Is.EqualTo("Undo c"));
			Assert.That(m_actionsTaken[i++], Is.EqualTo("Undo b"));
			Assert.That(m_actionsTaken[i++], Is.EqualTo("Undo d"));
			Assert.That(m_actionsTaken[i++], Is.EqualTo("Undo a"));
			Assert.That(m_actionsTaken[i++], Is.EqualTo("Redo a"));
			Assert.That(m_actionsTaken[i++], Is.EqualTo("Redo d"));
		}

		[Test]
		public void CanUndo_EmptyStack_ReturnsFalse()
		{
			var stack = new UndoStack<IUndoAction>();
			Assert.That(stack.CanUndo, Is.False);
		}

		[Test]
		public void CanUndo_AfterUndoOfOnlyItemOnStack_ReturnsFalse()
		{
			var stack = new UndoStack<IUndoAction>();
			stack.Push(NewAction());
			stack.Undo();
			Assert.That(stack.CanUndo, Is.False);
		}

		[Test]
		public void CanUndo_AfterPushingItemOnStack_ReturnsTrue()
		{
			var stack = new UndoStack<IUndoAction>();
			stack.Push(NewAction());
			Assert.That(stack.CanUndo, Is.True);
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
			Assert.That(stack.Redo(), Is.True);
			Assert.That(m_actionsTaken.Count, Is.EqualTo(2));
			Assert.That(m_actionsTaken[1], Is.EqualTo("Redo a"));
		}

		[Test]
		public void Undo_OneCurrentItemOnStack_ActionUndone()
		{
			var stack = new UndoStack<IUndoAction>();
			stack.Push(NewAction("a"));
			Assert.That(stack.Undo(), Is.True);
			Assert.That(m_actionsTaken.Count, Is.EqualTo(1));
			Assert.That(m_actionsTaken[0], Is.EqualTo("Undo a"));
		}

		[Test]
		public void Undo_UndoActionReturnsFalse_ReturnsFalseAndPreventsFurtherUndo()
		{
			var stack = new UndoStack<IUndoAction>();
			stack.Push(NewAction("a"));
			stack.Push(NewAction("b", false));
			stack.Push(NewAction("c"));
			Assert.That(stack.Undo(), Is.True);
			Assert.That(stack.Undo(), Is.False);
			Assert.That(stack.CanRedo, Is.True);
			Assert.That(stack.CanUndo, Is.False);
			Assert.That(stack.Redo(), Is.True);
			Assert.That(m_actionsTaken.Count, Is.EqualTo(3));
			int i = 0;
			Assert.That(m_actionsTaken[i++], Is.EqualTo("Undo c"));
			Assert.That(m_actionsTaken[i++], Is.EqualTo("Failed to undo b"));
			Assert.That(m_actionsTaken[i++], Is.EqualTo("Redo c"));
		}

		[Test]
		public void Undo_Multiple_ActionsUndoneInReverseOrderOfPush()
		{
			var stack = new UndoStack<IUndoAction>();
			stack.Push(NewAction("a"));
			stack.Push(NewAction("b"));
			stack.Push(NewAction("c"));
			Assert.That(stack.Undo(), Is.True);
			Assert.That(stack.Undo(), Is.True);
			Assert.That(stack.Undo(), Is.True);
			Assert.That(m_actionsTaken.Count, Is.EqualTo(3));
			int i = 0;
			Assert.That(m_actionsTaken[i++], Is.EqualTo("Undo c"));
			Assert.That(m_actionsTaken[i++], Is.EqualTo("Undo b"));
			Assert.That(m_actionsTaken[i++], Is.EqualTo("Undo a"));
		}

		[Test]
		public void CanRedo_EmptyStack_ReturnsFalse()
		{
			var stack = new UndoStack<IUndoAction>();
			Assert.That(stack.CanRedo, Is.False);
		}

		[Test]
		public void CanRedo_AfterUndo_ReturnsTrue()
		{
			var stack = new UndoStack<IUndoAction>();
			stack.Push(NewAction());
			stack.Undo();
			Assert.That(stack.CanRedo, Is.True);
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
			Assert.That(stack.Peek(), Is.Null);
		}

		[Test]
		public void Peek_AfterUndoOfOnlyItemOnStack_ReturnsNull()
		{
			var stack = new UndoStack<IUndoAction>();
			stack.Push(NewAction());
			stack.Undo();
			Assert.That(stack.Peek(), Is.Null);
		}

		[Test]
		public void Peek_AfterPushingItemOnStack_ReturnsItem()
		{
			var stack = new UndoStack<IUndoAction>();
			var action = NewAction();
			stack.Push(action);
			Assert.That(action, Is.EqualTo(stack.Peek()));
		}

		[Test]
		public void Peek_AfterUndoWithAdditionalItemsOnStack_ReturnsNextUndoableItem()
		{
			var stack = new UndoStack<IUndoAction>();
			var action = NewAction();
			stack.Push(action);
			stack.Push(NewAction());
			stack.Undo();
			Assert.That(action, Is.EqualTo(stack.Peek()));
		}
	}
}
