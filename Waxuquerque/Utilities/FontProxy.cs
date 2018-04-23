using System;
using System.Drawing;

namespace Waxuquerque.Utilities
{
	public class FontProxy : IDisposable
	{
		private const int kMinFontSize = 3;

		private Font m_font;
		private readonly FontFamily m_fontFamily;
		private readonly FontStyle m_style;
		private readonly string m_fontFamilyName;
		private readonly float m_baseFontSizeInPoints;
		private readonly bool m_rightToLeftScript;

		private int m_fontSizeUiAdjustment;

		public FontProxy(string fontFamilyName, int baseFontSizeInPoints, bool rightToLeftScript)
		{
			m_fontFamilyName = fontFamilyName;
			m_baseFontSizeInPoints = baseFontSizeInPoints;
			m_rightToLeftScript = rightToLeftScript;
		}

		public FontProxy(Font originalFont)
		{
			m_fontFamily = originalFont.FontFamily;
			m_style = originalFont.Style;
			m_baseFontSizeInPoints = originalFont.SizeInPoints;
			m_baseFontSizeInPoints = originalFont.SizeInPoints;
			m_rightToLeftScript = false;
		}

		public Font Font
		{
			get
			{
				if (m_font == null)
					m_font = m_fontFamily != null ? new Font(m_fontFamily, AdjustedFontSize, m_style) : new Font(m_fontFamilyName, AdjustedFontSize);
				return m_font;
			}
		}

		public bool RightToLeftScript
		{
			get { return m_rightToLeftScript; }
		}

		public string FontFamily
		{
			get { return m_fontFamilyName ?? m_fontFamily.Name; }
		}

		public int Size
		{
			get { return (int)AdjustedFontSize; }
		}

		private float AdjustedFontSize
		{
			get { return Math.Max(m_baseFontSizeInPoints + m_fontSizeUiAdjustment, kMinFontSize); }
		}

		public int FontSizeUiAdjustment
		{
			get { return m_fontSizeUiAdjustment; }
		}

		public void Dispose()
		{
			if (m_font != null)
			{
				m_font.Dispose();
				m_font = null;
			}
		}

		public Font AdjustFontSize(int newAdjustedFontSize, bool disposeOldFont = false)
		{
			if (disposeOldFont)
				Dispose();
			else
				m_font = null;
			m_fontSizeUiAdjustment = newAdjustedFontSize;
			return Font;
		}

		public static implicit operator Font(FontProxy fontProxy)
		{
			return fontProxy.Font;
		}
	}
}
