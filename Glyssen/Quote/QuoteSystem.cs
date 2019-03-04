using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Glyssen.Properties;
using SIL.ObjectModel;
using SIL.WritingSystems;
using SIL.Xml;

namespace Glyssen.Quote
{
	[XmlRoot]
	public class QuoteSystem
	{
		public static IComparer<QuotationMark> QuotationMarkTypeAndLevelComparer = new TypeAndLevelComparer();

		/// <summary>
		/// This is deprecated and should only be used for upgrading from old data
		/// </summary>
		private static string AnyPunctuation_Deprecated
		{
			get { return SIL.Extensions.StringExtensions.kObjReplacementChar.ToString(CultureInfo.InvariantCulture); }
		}

		private static List<QuoteSystem> s_systems;

		static QuoteSystem()
		{
			s_systems = new List<QuoteSystem>();

			var doc = new XmlDocument();
			doc.LoadXml(Resources.QuoteSystemData);
			foreach (XmlNode node in doc.SafeSelectNodes("//QuoteSystem"))
				s_systems.Add(XmlSerializationHelper.DeserializeFromString<QuoteSystem>(node.OuterXml));

			var systemsWithAllLevels = new List<QuoteSystem>();
			foreach (var quoteSystem in s_systems)
			{
				foreach (var level2 in QuoteUtils.GetLevel2Possibilities(quoteSystem.FirstLevel))
				{
					var qs = new QuoteSystem(quoteSystem);
					if (!string.IsNullOrWhiteSpace(quoteSystem.Name))
						qs.Name = String.Format("{0} with levels 2 ({1}/{2}) and 3.", quoteSystem.Name, level2.Open, level2.Close);
					qs.AllLevels.Add(level2);
					qs.AllLevels.Add(QuoteUtils.GenerateLevel3(qs, true));
					systemsWithAllLevels.Add(qs);
				}
			}
			s_systems.AddRange(systemsWithAllLevels);
		}

		private BulkObservableList<QuotationMark> m_allLevels;
		private string m_majorLanguage;

		public QuoteSystem()
		{
			AllLevels = new BulkObservableList<QuotationMark>();
		}

		public QuoteSystem(QuotationMark firstLevel, string quotationDashMarker = null, string quotationDashEndMarker = null) : this()
		{
			AllLevels.Add(firstLevel);
			if (quotationDashMarker != null)
				AllLevels.Add(new QuotationMark(quotationDashMarker, quotationDashEndMarker, null, 1, QuotationMarkingSystemType.Narrative));
		}

		public QuoteSystem(BulkObservableList<QuotationMark> allLevels) : this()
		{
			AllLevels = allLevels;
		}

		public QuoteSystem(QuoteSystem quoteSystem) : this()
		{
			AllLevels.AddRange(quoteSystem.AllLevels.Select(l => new QuotationMark(l.Open, l.Close, l.Continue, l.Level, l.Type)));
		}

		public static QuoteSystem TryCreateFromWritingSystem(WritingSystemDefinition ws)
		{
			var quoteMarks = ws?.QuotationMarks;
			return quoteMarks != null && quoteMarks.Any() ? new QuoteSystem(quoteMarks) : null;
		}

		public static QuoteSystem Default
		{
			get { return s_systems.SingleOrDefault(s => s.Name == "Guillemets"); }
		}

		public static IEnumerable<QuoteSystem> UniquelyGuessableSystems
		{
			get { return s_systems; }
		}

		public static QuoteSystem GetOrCreateQuoteSystem(QuotationMark firstLevel, string quotationDashMarker, string quotationDashEndMarker)
		{
			var newQuoteSystem = new QuoteSystem(firstLevel, quotationDashMarker, quotationDashEndMarker);

			var match = s_systems.SingleOrDefault(qs => qs.Equals(newQuoteSystem));
			return match ?? newQuoteSystem;
		}

		public string Name { get; set; }

		public string MajorLanguage
		{
			get { return m_majorLanguage; }
			set
			{
				m_majorLanguage = value;
				if (value == "French" && AllLevels.Any())
				{
					AllLevels[0] = new QuotationMark(AllLevels[0].Open, AllLevels[0].Close, AllLevels[0].Close, 1,
						QuotationMarkingSystemType.Normal);
				}
			}
		}

		[XmlIgnore]
		public BulkObservableList<QuotationMark> AllLevels
		{
			get { return m_allLevels; }
			set { m_allLevels = value; }
		}

		[XmlIgnore]
		public System.Collections.ObjectModel.ReadOnlyCollection<QuotationMark> NormalLevels
		{
			get
			{
				return m_allLevels.Where(l => l.Type == QuotationMarkingSystemType.Normal).ToList().AsReadOnly();
			}
		}

		[XmlIgnore]
		public QuotationMark FirstLevel { get { return AllLevels[0]; } }

		[XmlElement("StartQuoteMarker")]
		public string StartQuoteMarker_DeprecatedXml
		{
			get { return null; }
			set
			{
				if (AllLevels.Count == 0)
				{
					var cont = MajorLanguage == "French" ? null : value;
					AllLevels.Add(new QuotationMark(value, null, cont, 1, QuotationMarkingSystemType.Normal));
				}
				else
				{
					var cont = MajorLanguage == "French" ? AllLevels[0].Close : value;
					AllLevels[0] = new QuotationMark(value, AllLevels[0].Close, cont, 1, QuotationMarkingSystemType.Normal);
				}
			}
		}

		[XmlElement("EndQuoteMarker")]
		public string EndQuoteMarker_DeprecatedXml
		{
			get { return null; }
			set
			{
				if (AllLevels.Count == 0)
				{
					var cont = MajorLanguage == "French" ? value : null;
					AllLevels.Add(new QuotationMark(null, value, cont, 1, QuotationMarkingSystemType.Normal));
				}
				else
				{
					var cont = MajorLanguage == "French" ? value : AllLevels[0].Continue;
					AllLevels[0] = new QuotationMark(AllLevels[0].Open, value, cont, 1,
						QuotationMarkingSystemType.Normal);
				}
			}
		}

		[XmlElement("QuotationDashMarker")]
		public string QuotationDashMarker_DeprecatedXml
		{
			get { return null; }
			set
			{
				QuotationMark dialog = AllLevels.FirstOrDefault(l => l.Level == 1 && l.Type == QuotationMarkingSystemType.Narrative);
				if (dialog == null)
				{
					AllLevels.Add(new QuotationMark(value, null, null, 1, QuotationMarkingSystemType.Narrative));
				}
				else
				{
					AllLevels.Remove(dialog);
					AllLevels.Add(new QuotationMark(value, dialog.Close, dialog.Continue, 1, QuotationMarkingSystemType.Narrative));
				}
			}
		}

		[XmlElement("QuotationDashEndMarker")]
		public string QuotationDashEndMarker_DeprecatedXml
		{
			get { return null; }
			set
			{
				QuotationMark dialog = AllLevels.FirstOrDefault(l => l.Level == 1 && l.Type == QuotationMarkingSystemType.Narrative);
				if (value == AnyPunctuation_Deprecated)
					value = null;
				if (dialog == null)
				{
					AllLevels.Add(new QuotationMark(null, value, null, 1, QuotationMarkingSystemType.Narrative));
				}
				else
				{
					AllLevels.Remove(dialog);
					AllLevels.Add(new QuotationMark(dialog.Open, value, dialog.Continue, 1, QuotationMarkingSystemType.Narrative));
				}
			}
		}

		[XmlIgnore]
		public string QuotationDashMarker {
			get
			{
				QuotationMark dialog = AllLevels.FirstOrDefault(l => l.Level == 1 && l.Type == QuotationMarkingSystemType.Narrative);
				if (dialog == null)
					return null;
				return dialog.Open;
			}
		}

		[XmlIgnore]
		public string QuotationDashEndMarker
		{
			get
			{
				QuotationMark dialog = AllLevels.FirstOrDefault(l => l.Level == 1 && l.Type == QuotationMarkingSystemType.Narrative);
				if (dialog == null)
					return null;
				return dialog.Close;
			}
		}

		public string ShortSummary
		{
			get
			{
				var firstLevel = (FirstLevel.Open + " " + FirstLevel.Close).Trim();

				StringBuilder sb = new StringBuilder();
				sb.Append(firstLevel);
				if (!string.IsNullOrEmpty(QuotationDashMarker))
				{
					var dashLevel = (QuotationDashMarker + " " + QuotationDashEndMarker ?? "").Trim();
					if (dashLevel != firstLevel)
						sb.Append(" ").Append(dashLevel);
				}
				return sb.ToString();
			}
		}

		public string FullSummary
		{
			get { return ToString(); }
		}

		//public QuoteSystem GetCorrespondingFirstLevelQuoteSystem()
		//{
		//	return AllUniqueFirstLevelSystems.FirstOrDefault(f => f.FirstLevel.Open == FirstLevel.Open && f.FirstLevel.Close == FirstLevel.Close);
		//}

		public override string ToString()
		{
			var sb = new StringBuilder();
			foreach (var level in NormalLevels)
				sb.Append(level.Open).Append(" ").Append(level.Continue).Append(" ").Append(level.Close).Append(" / ");
			if (sb.Length >= 3)
				sb.Length -= 3;
			if (!string.IsNullOrEmpty(QuotationDashMarker))
				sb.Append(" / ").Append(QuotationDashMarker);
			if (!string.IsNullOrEmpty(QuotationDashEndMarker))
				sb.Append(" ").Append(QuotationDashEndMarker);
			return sb.ToString();
		}

		#region Equals methods overrides

		protected bool Equals(QuoteSystem other)
		{
			if (other == null)
				return false;
			if (AllLevels == null)
				return other.AllLevels == null;
			if (other.AllLevels == null)
				return false;
			return AllLevels.SequenceEqual(other.AllLevels);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			if (obj.GetType() != GetType())
				return false;
			return Equals((QuoteSystem)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (AllLevels != null ? AllLevels.GetHashCode() : 0);
			}
		}

		public static bool operator ==(QuoteSystem left, QuoteSystem right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(QuoteSystem left, QuoteSystem right)
		{
			return !Equals(left, right);
		}
		#endregion

		private class TypeAndLevelComparer : IComparer<QuotationMark>
		{
			public int Compare(QuotationMark x, QuotationMark y)
			{
				int result = x.Type.CompareTo(y.Type);
				if (result != 0)
					return result;
				return x.Level.CompareTo(y.Level);
			}
		}
	}
}
