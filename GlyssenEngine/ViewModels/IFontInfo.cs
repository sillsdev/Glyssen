namespace GlyssenEngine.ViewModels
{
	public interface IFontInfo<TFont>
	{
		bool RightToLeftScript { get; }
		string FontFamily { get; }
		int Size { get; }
		TFont Font { get; }
	}

	public interface IAdjustableFontInfo<TFont> : IFontInfo<TFont>
	{
		int FontSizeUiAdjustment { get; set; }
	}
}
