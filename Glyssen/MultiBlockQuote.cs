namespace Glyssen
{
	public enum MultiBlockQuote
	{
		None,
		Start,
		Continuation,
		ChangeOfDelivery
	}

	internal static class MultiBlockQuoteExtensions{
		internal static string ShortName(this MultiBlockQuote multiBlockQuote)
		{
			switch (multiBlockQuote)
			{
				case MultiBlockQuote.None:
					return "None";
				case MultiBlockQuote.Start:
					return "Start";
				case MultiBlockQuote.Continuation:
					return "Cont";
				case MultiBlockQuote.ChangeOfDelivery:
					return "CoD";
				default:
					return "?";
			}
		}
	}
}
