using System.Collections.Generic;

namespace ProtoScript
{
	public class QuoteSystemGuesser
	{
		public static QuoteSystem Guess(IEnumerable<IScrBook> bookList)
		{
			return QuoteSystem.Default;
		}
	}
}
