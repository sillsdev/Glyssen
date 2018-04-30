using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Waxuquerque.ViewModel.Undo;

namespace WaxuquerqueTests.ViewModel.Undo
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
			Assert.AreEqual("Blah", new UndoActionSequence<IUndoAction>(NewAction("a"), NewAction("Blah")).Description);
		}

		[Test]
		public void Undo_MultipleActions_UndoesActionsInReverseOrder()
		{
			var action = new UndoActionSequence<IUndoAction>(NewAction("a"), NewAction("b"), NewAction("c"));
			Assert.IsTrue(action.Undo());
			Assert.AreEqual(3, m_actionsTaken.Count);
			int i = 0;
			Assert.AreEqual("Undo c", m_actionsTaken[i++]);
			Assert.AreEqual("Undo b", m_actionsTaken[i++]);
			Assert.AreEqual("Undo a", m_actionsTaken[i++]);
		}

		[Test]
		public void Undo_OneActionCannotBeUndone_RedoesActionsThatWereUndoneToGetBackToStateBeforeCallingUndo()
		{
			var action = new UndoActionSequence<IUndoAction>(NewAction("a", false), NewAction("b"), NewAction("c"));
			Assert.IsFalse(action.Undo());
			Assert.AreEqual(5, m_actionsTaken.Count);
			int i = 0;
			Assert.AreEqual("Undo c", m_actionsTaken[i++]);
			Assert.AreEqual("Undo b", m_actionsTaken[i++]);
			Assert.AreEqual("Failed to undo a", m_actionsTaken[i++]);
			Assert.AreEqual("Redo b", m_actionsTaken[i++]);
			Assert.AreEqual("Redo c", m_actionsTaken[i++]);
		}

		[Test]
		public void Redo_MultipleActions_RedoesActionsInOriginalOrder()
		{
			var action = new UndoActionSequence<IUndoAction>(NewAction("a"), NewAction("b"), NewAction("c"));
			Assert.IsTrue(action.Redo());
			Assert.AreEqual(3, m_actionsTaken.Count);
			int i = 0;
			Assert.AreEqual("Redo a", m_actionsTaken[i++]);
			Assert.AreEqual("Redo b", m_actionsTaken[i++]);
			Assert.AreEqual("Redo c", m_actionsTaken[i++]);
		}

		[Test]
		public void Redo_OneActionCannotBeRedone_UndoesActionsThatWereRedoneToGetBackToStateBeforeCallingRedo()
		{
			var action = new UndoActionSequence<IUndoAction>(NewAction("a"), NewAction("b"), NewAction("c", true, false));
			Assert.IsFalse(action.Redo());
			Assert.AreEqual(5, m_actionsTaken.Count);
			int i = 0;
			Assert.AreEqual("Redo a", m_actionsTaken[i++]);
			Assert.AreEqual("Redo b", m_actionsTaken[i++]);
			Assert.AreEqual("Failed to redo c", m_actionsTaken[i++]);
			Assert.AreEqual("Undo b", m_actionsTaken[i++]);
			Assert.AreEqual("Undo a", m_actionsTaken[i++]);
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
