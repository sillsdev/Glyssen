using System.Collections.Generic;
using NUnit.Framework;
using ProtoScript;

namespace ProtoScriptTests
{
	[TestFixture]
	class QuoteSystemGuesserTests
	{
		[Test]
		public void Guess_NoBooks_ReturnsDefaultQuoteSystem()
		{
			Assert.AreEqual(QuoteSystem.Default, QuoteSystemGuesser.Guess(new List<IScrBook>()));
		}
	}
}
