using System;
using System.Diagnostics;
#if !MONO
using System.Runtime.InteropServices;
using System.Text;
#endif

namespace GlyssenEngine.Utilities
{
	public static class MessageBoxStrings
	{
#if !MONO
		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		static extern int LoadString(IntPtr hInstance, uint uID, StringBuilder lpBuffer, int nBufferMax);
		[DllImport("kernel32")]
		static extern IntPtr LoadLibrary(string lpFileName);
#endif

		private const uint OK_CAPTION = 800;
		private const uint CANCEL_CAPTION = 801;
		private const uint ABORT_CAPTION = 802;
		private const uint RETRY_CAPTION = 803;
		private const uint IGNORE_CAPTION = 804;
		private const uint YES_CAPTION = 805;
		private const uint NO_CAPTION = 806;
		private const uint CLOSE_CAPTION = 807;
		private const uint HELP_CAPTION = 808;
		private const uint TRYAGAIN_CAPTION = 809;
		private const uint CONTINUE_CAPTION = 810;

		public static string RetryButton => GetMsgBoxString(RETRY_CAPTION);
		public static string IgnoreButton => GetMsgBoxString(IGNORE_CAPTION);
		public static string NoButton => GetMsgBoxString(NO_CAPTION);

		private static string GetMsgBoxString(uint button)
		{
#if !MONO // TODO: If we ever come up with a Mono version of Glyssen, need to figure out the right way to get the MessageBox button labels.
			StringBuilder sb = new StringBuilder(256);

			try
			{
				IntPtr user32 = LoadLibrary(Environment.SystemDirectory + "\\User32.dll");
				LoadString(user32, button, sb, sb.Capacity);
			}
			catch (Exception e)
			{
				Debug.Fail(e.Message);
				// In production, if the call to load the DLL or get the button's label fail,
				// we'll just take the hard-coded (English) versions from the switch below.
			}
			if (sb.Length > 0 && sb[0] == '&')
				sb.Remove(0, 1);
			if (sb.Length > 0)
				return sb.ToString();
#endif
			switch (button)
			{
				case RETRY_CAPTION: return "Retry";
				case IGNORE_CAPTION: return "Ignore";
				case NO_CAPTION: return "No";
				default: throw new NotImplementedException("Unhandled case");
			}
		}
	}
}
