namespace GlyssenEngine.Utilities
{
	public interface IFontInfo
	{
		bool RightToLeftScript { get; }
		string FontFamily { get; }
		int Size { get; }
	}
}
