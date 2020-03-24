using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using PtxUtils;
using PtxUtils.Progress;
using SIL.Reporting;

namespace Glyssen.Utilities
{
    /// <summary>
    /// Utilities for running actions in the background or with progress bar
    /// </summary>
    public class ProgressUtilsImpl : ProgressUtils
    {
        /// <summary>
        /// Event that fires before a synchronous (InvokeOnUIThread not InvokeLaterOnUIThread) call to the UI thread occurs.
        /// </summary>
        public event EventHandler BeforeUIThreadSynchronization = delegate { };

        /// <summary>InvokeLaterOnUIThread reentry protection</summary>
        private bool m_runningInvokeLaterOnUIThreadAction;
        private volatile ManualResetEvent m_executeOnSameThreadComplete = new ManualResetEvent(true);

        /// <summary>
        /// If Reentry occurs while processing a InvokeLaterOnUIThread action, then the second action gets moved
        /// into this List. When the original action is finished then the saved actions are re-posted to the
        /// synchronization context, and this list is cleared.
        /// </summary>
        private readonly List<ThreadStart> m_postponedInvokeLaterOnUIThreadActions = new List<ThreadStart>();

        private readonly SynchronizationContext m_uiSynchronizationContext;
        private readonly Thread m_uiSynchronizationThread;

        /// <summary>
        /// Stores key to EventHandler map for InvokeLaterOnIdle
        /// </summary>
        private readonly Dictionary<string, EventHandler> m_actionToEventHandlers = new Dictionary<string, EventHandler>();

        /// <summary>
        /// Creates a new instance. This must be created on the UI thread of the application as
        /// soon as possible (like in the Main method before anything else is done).
        /// </summary>
        public ProgressUtilsImpl()
        {
            // Create the smallest UI item possible that will initialize the SynchronizationContext.Current.
            // ReSharper disable once ObjectCreationAsStatement
            new Control();

            m_uiSynchronizationContext = SynchronizationContext.Current;
            m_uiSynchronizationThread = Thread.CurrentThread;
        }

        #region Implementation of ProgressUtils
        protected override bool OnMainUiThreadInternal
        {
            get
            {
                // True if no UI context
                if (m_uiSynchronizationContext == null)
                    return true;

                // It's the same thread as our UI thread. (Fixes some tests that seem to change the current SynchronizationContext).
                if (m_uiSynchronizationThread != null && m_uiSynchronizationThread.Equals(Thread.CurrentThread))
                    return true;

                // Note: Assumes that synchronization context of UI thread is only on UI thread
                return m_uiSynchronizationContext.Equals(SynchronizationContext.Current);
            }
        }

        protected override void InvokeOnUIThreadInternal(ThreadStart action, bool exclusively)
        {
            if (action == null)
                return;

            // Run directly if no UI context or if already on UI thread
            if (OnMainUiThread)
            {
                action();
                return;
            }

            Exception caughtException = null;
            bool finished = false;

            // Looping may be necessary to run exclusively
            while (!finished)
            {
                BeforeUIThreadSynchronization(Thread.CurrentThread, EventArgs.Empty);
                // Wait for the UI Thread
                if (Progress.ExhaustiveDebugging)
                    ErrorUtils.LogStackTrace("Invoking the UI thread");
                m_uiSynchronizationContext.Send(delegate
                {
	                try
                    {
                        action();
                    }
                    catch (Exception exception)
                    {
                        ErrorUtils.SaveStackTrace(exception);
                        caughtException = exception;
                    }
                    finally
                    {
                        finished = true;
                    }
                }, null);

                // We can only get here if we are running exclusively and if ExecuteOnSameThread was running.
                // Wait for ExecuteOnSameThread to finish
                if (!finished)
                    m_executeOnSameThreadComplete.WaitOne();
            }

            if (caughtException == null)
                return;

            Trace.TraceError("Original exception before being re-thrown: {0}", caughtException);

            throw caughtException; // would like to wrap this in TargetInvocationException, but some code depends on getting original exception type.
        }

        protected override void InvokeLaterOnUIThreadInternal(ThreadStart action)
        {
            if (action == null)
                return;

            // Run directly if no UI context
            if (m_uiSynchronizationContext == null)
            {
                action();
                return;
            }

            SendOrPostCallback cb = state =>
            {
                if (Progress.ExhaustiveDebugging)
                    ErrorUtils.LogStackTrace("Invoking the UI thread Later. Post-invoke.");

                if (m_runningInvokeLaterOnUIThreadAction)
                {
                    m_postponedInvokeLaterOnUIThreadActions.Add((ThreadStart)state);
                    return;
                }

                // If Application Message pump has quit, Ignore Later actions.
                // Mono: mono is setting Application.MessageLoop to false when showing
                // a modal dialog and not setting it to true when closing the dialog.
                if (Platform.IsDotNet && !Application.MessageLoop)
                    return;

                Debug.Assert(Thread.CurrentThread == m_uiSynchronizationThread, "Invoked on wrong thread");

                m_runningInvokeLaterOnUIThreadAction = true;
                try
                {
                    ((ThreadStart)state)();
                }
                finally
                {
                    m_runningInvokeLaterOnUIThreadAction = false;
                }

                foreach (var postponedAction in m_postponedInvokeLaterOnUIThreadActions)
                    InvokeLaterOnUIThread(postponedAction);

                m_postponedInvokeLaterOnUIThreadActions.Clear();
            };

            if (Progress.ExhaustiveDebugging)
                ErrorUtils.LogStackTrace("Invoking the UI thread Later. Pre-invoke.");

            m_uiSynchronizationContext.Post(cb, action);
        }

        protected override void InvokeLaterOnUIThreadAllowingReEntryInternal(ThreadStart action)
        {
            // Run directly if no UI context
            if (m_uiSynchronizationContext == null)
            {
                action();
                return;
            }

            if (Progress.ExhaustiveDebugging)
                ErrorUtils.LogStackTrace("Invoking the UI thread Later. Pre-invoke.");

            m_uiSynchronizationContext.Post(state =>
            {
                Debug.Assert(Thread.CurrentThread == m_uiSynchronizationThread, "Invoked on wrong thread");
                if (Progress.ExhaustiveDebugging)
                    ErrorUtils.LogStackTrace("Invoking the UI thread Later. Post-invoke.");
                action();
            }, null);
        }

        protected override EventHandler InvokeLaterOnIdleInternal(ThreadStart action, EventHandler cancelIdleEvent)
        {
            return InvokeLaterOnIdleInternal(action, cancelIdleEvent, 0);
        }

        protected override EventHandler InvokeLaterOnIdleInternal(ThreadStart action, EventHandler cancelIdleEvent, int msDelay)
        {
            AssertOnMainUiThread();

            if (cancelIdleEvent != null)
                Application.Idle -= cancelIdleEvent;

            EventHandler idleAction = null;
            var waitUntilTime = DateTime.Now + TimeSpan.FromMilliseconds(msDelay);
            idleAction = (s, e) =>
            {
                if (DateTime.Now < waitUntilTime)
                    return;

                Application.Idle -= idleAction;
                action?.Invoke();
            };

            Application.Idle += idleAction;
            return idleAction;
        }

        protected override void CancelInvokeLaterOnIdleInternal(EventHandler idleEvent)
        {
            if (idleEvent != null)
                Application.Idle -= idleEvent;
        }

        protected override void InvokeLaterOnIdleInternal(ThreadStart action)
        {
            string key = action.Target.GetHashCode() + action.Method.ToString();

            void NewIdleAction(object s, EventArgs e)
            {
	            lock (m_actionToEventHandlers)
	            {
		            m_actionToEventHandlers.Remove(key);
		            Application.Idle -= NewIdleAction;
	            }

	            action();
            }

            lock (m_actionToEventHandlers)
            {
	            if (m_actionToEventHandlers.TryGetValue(key, out var prevIdleAction))
                    Application.Idle -= prevIdleAction;
                m_actionToEventHandlers[key] = NewIdleAction;
                Application.Idle += NewIdleAction;
            }
        }

        protected override void ExecuteInternal(string title, CancelModes cancelMode, ThreadStart action, bool topMost)
        {
	        Logger.WriteEvent(title);
	        Debug.Fail("Did not expect: " + title);
        }

        /// <summary>
        /// Execute the specified action on the UI thread (must be called from the UI thread).
        /// </summary>
        protected override void ExecuteOnSameThreadInternal(string title, CancelModes cancelMode, ThreadStart action)
        {
	        Logger.WriteEvent(title);
	        Debug.Fail("Did not expect: " + title);
        }

        protected override BackgroundExecution CurrentExecutingBackgroundSingleAsyncInternal(BackgroundExecutionAction action)
        {
	        Debug.Fail("Did not expect CurrentExecutingBackgroundSingleAsyncInternal to be called");
	        return null;
        }

        protected override void ExecuteInBackgroundSingleAsyncInternal(IComponent parentControl, string title,
            CancelModes cancelMode, BackgroundExecutionAction action,
            ThreadStart cancelAction, ExceptionHandlerDelegate exceptionHandler)
        {
	        Logger.WriteEvent(title);
	        Debug.Fail("Did not expect: " + title);
        }

        protected override BackgroundExecution ExecuteInBackgroundInternal(IComponent parent, string title,
            CancelModes cancelMode, Action<IProgressControl> action,
            ThreadStart cancelAction, ExceptionHandlerDelegate exceptionHandler, ProgressBarOptions options = null)
        {
	        Logger.WriteEvent(title);
	        Debug.Fail("Did not expect: " + title);
	        return null;
        }
        #endregion
    }
}

