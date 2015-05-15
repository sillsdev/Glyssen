using System;
using System.Windows.Forms;

namespace Glyssen.Controls
{
	static class ControlExtensions
	{
		public static void SafeInvoke(this Control uiElement, Action updater, bool ignoreIfDisposed = false, bool forceSynchronous = false)
		{
			if (uiElement == null)
			{
				throw new ArgumentNullException("uiElement");
			}

			if (uiElement.InvokeRequired)
			{
				if (forceSynchronous)
					uiElement.Invoke(updater);
				else
					uiElement.BeginInvoke(updater);
			}
			else
			{
				if (uiElement.IsDisposed)
				{
					if (ignoreIfDisposed)
						return;
					throw new ObjectDisposedException("Control is already disposed.");
				}

				updater();
			}
		}
	}
}
