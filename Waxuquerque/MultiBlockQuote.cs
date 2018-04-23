namespace Waxuquerque
{
	public enum MultiBlockQuote
	{
		None,
		Start,
		Continuation,
		/// <summary>
		/// Deprecated: Use Continuation.
		/// </summary>
		ChangeOfDelivery = Continuation
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
				default:
					return "?";
			}
		}
	}
}
