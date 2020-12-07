namespace GlyssenEngine.Quote
{
	public interface IQuoteInterruptionFinder
	{
		QuoteInterruption GetNextInterruption(string text, int startCharIndex);
		bool ProbablyIsNotAnInterruption(string text);
	}
}
