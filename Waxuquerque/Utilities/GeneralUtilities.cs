using System.Reflection;

namespace Waxuquerque.Utilities
{
	class GeneralUtilities
	{
		public static bool RunningUnitTests => Assembly.GetEntryAssembly() == null;
	}
}
