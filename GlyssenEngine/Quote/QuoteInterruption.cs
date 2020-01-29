namespace GlyssenEngine.Quote
{
	public class QuoteInterruption
	{
		public int Index { get; }
		public int Length { get; }
		public string Value { get; }

		public QuoteInterruption(int index, int length, string value)
		{
			Index = index;
			Length = length;
			Value = value;
		}
	}
}
