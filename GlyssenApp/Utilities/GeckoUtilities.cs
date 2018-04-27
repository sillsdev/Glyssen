using Gecko;
using Gecko.DOM;

namespace GlyssenApp.Utilities
{
	static class GeckoUtilities
	{
		public static bool ParseDomEventTargetAsGeckoElement(DomEventTarget domEventTarget, out GeckoElement geckoElement)
		{
			geckoElement = null;
			if (domEventTarget == null)
				return false;

			geckoElement = domEventTarget.CastToGeckoElement();
			return geckoElement != null;
		}
	}
}
