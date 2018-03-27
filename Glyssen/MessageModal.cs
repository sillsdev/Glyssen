using System;

namespace Glyssen
{
	public interface IMessageModal
	{
		void Show(string text);
		//void Show(string text, string caption);
	}

	public static class MessageModal
	{
		private static IMessageModal s_instance;

		public static void SetInstance(IMessageModal instance)
		{
			s_instance = instance;
		}

		public static void Show(string text)
		{
			if (s_instance == null)
				throw new InvalidOperationException("You must call SetInstance() first.");

			s_instance.Show(text);
		}
	}
}
