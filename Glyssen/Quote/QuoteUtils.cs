using System;
using System.Collections.Generic;
using System.Linq;
using L10NSharp;
using SIL.WritingSystems;

namespace Glyssen.Quote
{
	public static class QuoteUtils
	{
		public static readonly string None = LocalizationManager.GetString("Common.None", "None");
		private static readonly object[] DefaultSymbols = { "“", "”", "‘", "’", "«", "»", "‹", "›", "„", "‚", "「", "」", "『", "』", "<<", ">>", "<", ">", None };
		private static readonly Dictionary<MatchedPair, MatchedPair[]> Level2Possibilities = new Dictionary<MatchedPair, MatchedPair[]>
		{
			{new MatchedPair("“", "”", false), new []{new MatchedPair("‘", "’", false)}},
			{new MatchedPair("‘", "’", false), new []{new MatchedPair("“", "”", false)}},
			{new MatchedPair("«", "»", false), new []
			{
				new MatchedPair("“", "”", false),
				new MatchedPair("‹", "›", false),
				new MatchedPair("«", "»", false),
				new MatchedPair("„", "“", false),
				new MatchedPair("’", "’", false),
				new MatchedPair("‘", "’", false)
			}},
			{new MatchedPair("»", "«", false), new []{new MatchedPair("›", "‹", false)}},
			{new MatchedPair("”", "”", false), new []{new MatchedPair("’", "’", false)}},
			{new MatchedPair("\"", "\"", false), new []{new MatchedPair("'", "'", false)}},
			{new MatchedPair("„", "“", false), new []
			{
				new MatchedPair("‚", "‘", false),
				new MatchedPair("‘", "’", false),
				new MatchedPair("’", "’", false),
				new MatchedPair("‚", "’", false),
				new MatchedPair("’", "‚", false),
				new MatchedPair("»", "«", false),
				new MatchedPair("’", "‘", false),
				new MatchedPair("«", "»", false)
			}},
			{new MatchedPair("„", "”", false), new []
			{
				new MatchedPair("»", "«", false),
				new MatchedPair("‚", "’", false),
				new MatchedPair("‘", "’", false),
				new MatchedPair("’", "’", false),
				new MatchedPair("»", "«", false),
				new MatchedPair("«", "»", false)
			}},
			{new MatchedPair("‚", "’", false), new []{new MatchedPair("„", "”", false)}},
			{new MatchedPair("「", "」", false), new []{new MatchedPair("『", "』", false)}},
			{new MatchedPair("”", "“", false), new []{new MatchedPair("’", "‘", false)}},
			{new MatchedPair("<<", ">>", false), new []{new MatchedPair("<", ">", false)}},
		};

		public static object[] AllDefaultSymbols()
		{
			return DefaultSymbols;
		}

		public static Dictionary<MatchedPair, MatchedPair> Level2Defaults
		{
			get { return Level2Possibilities.ToDictionary(p => p.Key, p => p.Value.First()); }
		}

		public static QuotationMark[] GetLevel2Possibilities(QuotationMark level1)
		{
			var level1Mp = new MatchedPair(level1.Open, level1.Close, false);
			MatchedPair[] level2Possibilities;
			Level2Possibilities.TryGetValue(level1Mp, out level2Possibilities);
			if (level2Possibilities == null)
				return null;
			return level2Possibilities.Select(q => new QuotationMark(q.Open, q.Close, level1.Continue + q.Open, 2, level1.Type)).ToArray();
		}

		public static QuotationMark GetLevel2Default(QuotationMark level1)
		{
			var level1Mp = new MatchedPair(level1.Open, level1.Close, false);
			MatchedPair level2;
			Level2Defaults.TryGetValue(level1Mp, out level2);
			if (level2 == null)
				return null;
			return new QuotationMark(level2.Open, level2.Close, level2.Open, 2, level1.Type);
		}

		public static QuotationMark GenerateLevel3(QuoteSystem system, bool concatenateContinuers)
		{
			if (system.NormalLevels.Count < 2)
				throw new ArgumentException("Cannot generate level 3 unless levels 1 and 2 are defined.");
			var level1 = system.FirstLevel;
			string level3Continuer;
			if (concatenateContinuers)
				level3Continuer = system.NormalLevels[1].Continue + level1.Continue;
			else
				level3Continuer = level1.Continue;
			return new QuotationMark(level1.Open, level1.Close, level3Continuer, 3, QuotationMarkingSystemType.Normal);
		}
	}
}
