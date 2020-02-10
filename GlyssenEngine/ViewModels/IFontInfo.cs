namespace GlyssenEngine.ViewModels
{
	public interface IFontInfo<out TFont>
	{
		bool RightToLeftScript { get; }
		string FontFamily { get; }
		int Size { get; }
		TFont Font { get; }
	}

	public interface IAdjustableFontInfo<out TFont> : IFontInfo<TFont>
	{
		int FontSizeUiAdjustment { get; set; }
	}
}
