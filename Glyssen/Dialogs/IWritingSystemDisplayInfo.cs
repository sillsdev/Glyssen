﻿namespace Glyssen.Dialogs
{
	public interface IWritingSystemDisplayInfo
	{
		bool RightToLeft { get; }
		string FontFamily { get; }
		int FontSize { get; }
	}
}
