using System;
using System.Diagnostics;
using System.Drawing;
using GlyssenEngine.ViewModels;

namespace Glyssen.Utilities
{
	public class FontProxy : IAdjustableFontInfo<Font>, IDisposable
	{
		private const int kMinFontSize = 3;

		private Font m_font;
		private readonly FontFamily m_fontFamily;
		private readonly FontStyle m_style;
		private readonly string m_fontFamilyName;
		private readonly float m_baseFontSizeInPoints;
		private readonly bool m_clientResponsibleForDisposing;

		private int m_fontSizeUiAdjustment;

		public FontProxy(string fontFamilyName, int baseFontSizeInPoints, bool rightToLeftScript)
		{
			m_fontFamilyName = fontFamilyName;
			m_baseFontSizeInPoints = baseFontSizeInPoints;
			RightToLeftScript = rightToLeftScript;
			m_clientResponsibleForDisposing = false;
		}

		public FontProxy(Font originalFont)
		{
			m_fontFamily = originalFont.FontFamily;
			m_style = originalFont.Style;
			m_baseFontSizeInPoints = originalFont.SizeInPoints;
			m_baseFontSizeInPoints = originalFont.SizeInPoints;
			RightToLeftScript = false;
			m_clientResponsibleForDisposing = true;
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

		public bool RightToLeftScript { get; }

		public string FontFamily => m_fontFamilyName ?? m_fontFamily.Name;

		public int Size => (int)AdjustedFontSize;

		private float AdjustedFontSize => Math.Max(m_baseFontSizeInPoints + m_fontSizeUiAdjustment, kMinFontSize);

		public int FontSizeUiAdjustment
		{
			get => m_fontSizeUiAdjustment;
			set 
			{
				if (m_clientResponsibleForDisposing)
					m_font = null;
				else
					Dispose();
				m_fontSizeUiAdjustment = value;
			}
		}

		public void Dispose()
		{
			if (m_font != null)
			{
				Debug.Assert(m_clientResponsibleForDisposing);
				m_font.Dispose();
				m_font = null;
			}
		}

		public static implicit operator Font(FontProxy fontProxy)
		{
			return fontProxy.Font;
		}
	}
}
