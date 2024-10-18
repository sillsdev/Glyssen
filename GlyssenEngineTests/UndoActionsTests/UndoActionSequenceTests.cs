using System;
using System.Collections.Generic;
using GlyssenEngine.UndoActions;
using NUnit.Framework;
using Rhino.Mocks;

namespace GlyssenEngineTests.UndoActionsTests
{
	[TestFixture]
	class UndoActionSequenceTests
	{
		private List<string> m_actionsTaken;

		[SetUp]
		public void Setup()
		{
			m_actionsTaken = new List<string>();
		}

		[Test]
		public void Constructor_NoUndoActions_ThrowsArgumentException()
		{
			Assert.Throws<ArgumentException>(() => new UndoActionSequence<IUndoAction>());
		}

		[Test]
		public void Description_NoUndoActions_ThrowsArgumentException()
		{
			Assert.That(new UndoActionSequence<IUndoAction>(NewAction("a"), NewAction("Blah")).Description, Is.EqualTo("Blah"));
		}

		[Test]
		public void Undo_MultipleActions_UndoesActionsInReverseOrder()
		{
			var action = new UndoActionSequence<IUndoAction>(NewAction("a"), NewAction("b"), NewAction("c"));
			Assert.That(action.Undo(), Is.True);
			Assert.That(m_actionsTaken.Count, Is.EqualTo(3));
			int i = 0;
			Assert.That(m_actionsTaken[i++], Is.EqualTo("Undo c"));
			Assert.That(m_actionsTaken[i++], Is.EqualTo("Undo b"));
			Assert.That(m_actionsTaken[i++], Is.EqualTo("Undo a"));
		}

		[Test]
		public void Undo_OneActionCannotBeUndone_RedoesActionsThatWereUndoneToGetBackToStateBeforeCallingUndo()
		{
			var action = new UndoActionSequence<IUndoAction>(NewAction("a", false), NewAction("b"), NewAction("c"));
			Assert.That(action.Undo(), Is.False);
			Assert.That(m_actionsTaken.Count, Is.EqualTo(5));
			int i = 0;
			Assert.That(m_actionsTaken[i++], Is.EqualTo("Undo c"));
			Assert.That(m_actionsTaken[i++], Is.EqualTo("Undo b"));
			Assert.That(m_actionsTaken[i++], Is.EqualTo("Failed to undo a"));
			Assert.That(m_actionsTaken[i++], Is.EqualTo("Redo b"));
			Assert.That(m_actionsTaken[i++], Is.EqualTo("Redo c"));
		}

		[Test]
		public void Redo_MultipleActions_RedoesActionsInOriginalOrder()
		{
			var action = new UndoActionSequence<IUndoAction>(NewAction("a"), NewAction("b"), NewAction("c"));
			Assert.That(action.Redo(), Is.True);
			Assert.That(m_actionsTaken.Count, Is.EqualTo(3));
			int i = 0;
			Assert.That(m_actionsTaken[i++], Is.EqualTo("Redo a"));
			Assert.That(m_actionsTaken[i++], Is.EqualTo("Redo b"));
			Assert.That(m_actionsTaken[i++], Is.EqualTo("Redo c"));
		}

		[Test]
		public void Redo_OneActionCannotBeRedone_UndoesActionsThatWereRedoneToGetBackToStateBeforeCallingRedo()
		{
			var action = new UndoActionSequence<IUndoAction>(NewAction("a"), NewAction("b"), NewAction("c", true, false));
			Assert.That(action.Redo(), Is.False);
			Assert.That(m_actionsTaken.Count, Is.EqualTo(5));
			int i = 0;
			Assert.That(m_actionsTaken[i++], Is.EqualTo("Redo a"));
			Assert.That(m_actionsTaken[i++], Is.EqualTo("Redo b"));
			Assert.That(m_actionsTaken[i++], Is.EqualTo("Failed to redo c"));
			Assert.That(m_actionsTaken[i++], Is.EqualTo("Undo b"));
			Assert.That(m_actionsTaken[i++], Is.EqualTo("Undo a"));
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
	}
}
