using System;
using System.Diagnostics;
using System.Drawing;
using GlyssenEngine;
using GlyssenEngine.ViewModels;

namespace Glyssen.Utilities
{
	public class FontProxy : IFontInfo<Font>, IDisposable
	{
		protected Font m_font;
		private readonly FontFamily m_fontFamily;
		private readonly FontStyle m_style;
		private readonly string m_fontFamilyName;
		private readonly float m_fontSizeInPoints;
		protected readonly bool m_clientResponsibleForDisposing;

		public FontProxy(string fontFamilyName, int fontSizeInPoints, bool rightToLeftScript)
		{
			m_fontFamilyName = fontFamilyName;
			m_fontSizeInPoints = fontSizeInPoints;
			RightToLeftScript = rightToLeftScript;
			m_clientResponsibleForDisposing = false;
		}

		public FontProxy(Font originalFont, bool rightToLeftScript)
		{
			m_fontFamily = originalFont.FontFamily;
			m_style = originalFont.Style;
			m_fontSizeInPoints = originalFont.SizeInPoints;
			RightToLeftScript = rightToLeftScript;
			m_clientResponsibleForDisposing = true;
		}

		public Font Font
		{
			get
			{
				if (m_font == null)
					m_font = m_fontFamily != null ? new Font(m_fontFamily, Size, m_style) : new Font(m_fontFamilyName, Size);
				return m_font;
			}
		}

		public bool RightToLeftScript { get; }

		public string FontFamily => m_fontFamilyName ?? m_fontFamily.Name;

		public virtual int Size => (int)m_fontSizeInPoints;

		public void Dispose()
		{
			if (m_font != null)
			{
				Debug.Assert(!m_clientResponsibleForDisposing);
				m_font.Dispose();
				m_font = null;
			}
		}

		public static implicit operator Font(FontProxy fontProxy)
		{
			return fontProxy.Font;
		}
	}

	public class AdjustableFontProxy : FontProxy, IAdjustableFontInfo<Font>
	{
		private const int kMinFontSize = 3;

		private int m_fontSizeUiAdjustment;

		public AdjustableFontProxy(string fontFamilyName, int baseFontSizeInPoints, bool rightToLeftScript) :
			base(fontFamilyName, baseFontSizeInPoints, rightToLeftScript)
		{
		}

		public AdjustableFontProxy(Font originalFont, bool rightToLeftScript) : base(originalFont, rightToLeftScript)
		{
		}

		public override int Size => (int)AdjustedFontSize;

		private float AdjustedFontSize => Math.Max(base.Size + m_fontSizeUiAdjustment, kMinFontSize);

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

		public static IAdjustableFontInfo<Font> GetFontProxyForReferenceText(ReferenceText referenceText) =>
			new AdjustableFontProxy(referenceText.FontFamily, referenceText.FontSizeInPoints, referenceText.RightToLeftScript);
	}
}
