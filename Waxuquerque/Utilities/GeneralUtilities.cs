using System.Reflection;

namespace Waxuquerque.Utilities
{
	public class GeneralUtilities
	{
		/// <summary>
		/// The first method which should be called by a client using the Waxuquerque library
		/// </summary>
		public static void InitializeLibrary()
		{
			Icu.Wrapper.Init();
		}

		/// <summary>
		/// The last method which should be called by a client using the Waxuquerque library
		/// </summary>
		public static void CleanupLibrary()
		{
			Icu.Wrapper.Cleanup();
		}

		public static bool RunningUnitTests => Assembly.GetEntryAssembly() == null;
	}
}
