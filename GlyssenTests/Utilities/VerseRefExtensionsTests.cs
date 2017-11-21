using System;
using System.Threading;
using System.Windows.Forms;
using GlyssenApp.UI.Utilities;
using NUnit.Framework;
using SIL.Scripture;
using SIL.Windows.Forms.Scripture;

namespace GlyssenTests.Utilities
{
	class VerseRefExtensionsTests
	{
		[Test, Ignore("By Hand Only")]
		public void SendScrReference()
		{
			DummyForm.Start();

			// MAT 28:18
			VerseRef vr = new VerseRef(040028018);
			vr.SendScrReference();

			Thread.Sleep(1000);

			Assert.AreEqual(vr, DummyForm.MessageReceived);

			DummyForm.Stop();
		}

		private class DummyForm : Form
		{
			public static VerseRef MessageReceived;
			private static DummyForm s_instance;

			public static void Start()
			{
				Thread t = new Thread(RunForm);
				t.SetApartmentState(ApartmentState.STA);
				t.IsBackground = true;
				t.Start();
			}
			public static void Stop()
			{
				if (s_instance == null)
					throw new InvalidOperationException("Stop without Start");
				s_instance.Invoke(new MethodInvoker(s_instance.EndForm));
			}
			private static void RunForm()
			{
				Application.Run(new DummyForm());
			}

			private void EndForm()
			{
				Close();
			}

			protected override void SetVisibleCore(bool value)
			{
				// Prevent window getting visible
				if (s_instance == null)
					CreateHandle();
				s_instance = this;
				base.SetVisibleCore(false);
			}

			protected override void WndProc(ref Message msg)
			{
				if (msg.Msg == SantaFeFocusMessageHandler.FocusMsg)
				{
					var verseRef = new VerseRef(SantaFeFocusMessageHandler.ReceiveFocusMessage(msg));
					MessageReceived = verseRef;
				}

				base.WndProc(ref msg);
			}
		}
	}
}
