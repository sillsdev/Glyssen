using System.Collections.Generic;
using System.Linq;
using Glyssen.Quote;
using SIL.DblBundle.Text;
using SIL.WritingSystems;

namespace Glyssen.Bundle
{
	public class GlyssenBundle : TextBundle<GlyssenDblTextMetadata, GlyssenDblMetadataLanguage>
	{
		static GlyssenBundle()
		{
			DefaultLanguageIsoCode = WellKnownSubtags.UnlistedLanguage;
		}

		public GlyssenBundle(string pathToZippedBundle) : base(pathToZippedBundle)
		{
			Metadata.OriginalPathBundlePath = BundlePath;
			Metadata.FontFamily = Stylesheet.FontFamily;
			Metadata.FontSizeInPoints = Stylesheet.FontSizeInPoints;
			if (string.IsNullOrEmpty(Metadata.Language.Iso))
				Metadata.Language.Iso = LanguageIso;
		}

		public string LanguageAsString { get { return Metadata.Language.ToString(); } }

		public void ConvertContinuersToParatextAssumptions()
		{
			if (WritingSystemDefinition == null || WritingSystemDefinition.QuotationMarks == null)
				return;

			List<QuotationMark> replacementQuotationMarks = new List<QuotationMark>();
			foreach (var level in WritingSystemDefinition.QuotationMarks.OrderBy(q => q, QuoteSystem.QuotationMarkTypeAndLevelComparer))
			{
				if (level.Type == QuotationMarkingSystemType.Normal && level.Level > 1 && !string.IsNullOrWhiteSpace(level.Continue))
				{
					var oneLevelUp = replacementQuotationMarks.SingleOrDefault(q => q.Level == level.Level - 1 && q.Type == QuotationMarkingSystemType.Normal);
					if (oneLevelUp == null)
						continue;
					string oneLevelUpContinuer = oneLevelUp.Continue;
					if (string.IsNullOrWhiteSpace(oneLevelUpContinuer))
						continue;
					string newContinuer = oneLevelUpContinuer + " " + level.Continue;
					replacementQuotationMarks.Add(new QuotationMark(level.Open, level.Close, newContinuer, level.Level, level.Type));
					continue;
				}
				replacementQuotationMarks.Add(level);
			}

			WritingSystemDefinition.QuotationMarks.Clear();
			WritingSystemDefinition.QuotationMarks.AddRange(replacementQuotationMarks);
		}
	}
}
