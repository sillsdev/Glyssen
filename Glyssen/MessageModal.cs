using System;

namespace Glyssen
{
	public interface IMessageModal
	{
		void Show(string text);
	}

	public static class MessageModal
	{
		private static IMessageModal s_instance;

		public static IMessageModal Default
		{
			get
			{
				if (s_instance == null)
					throw new InvalidOperationException("Not Initialized. Set MessageModal.Default first.");
				return s_instance;
			}
			set => s_instance = value;
		}

		public static void Show(string text)
		{
			Default.Show(text);
		}
	}
}
