using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using ProtoScript.Properties;
using SIL.ObjectModel;
using SIL.WritingSystems;
using SIL.Xml;

namespace ProtoScript.Quote
{
	[XmlRoot]
	public class QuoteSystem
	{
		public static string AnyPunctuation
		{
			get { return SIL.Extensions.StringExtensions.kObjReplacementChar.ToString(CultureInfo.InvariantCulture); }
		}

		private static List<QuoteSystem> s_systems;
		private static List<QuoteSystem> s_uniquelyGuessableSystems;

		static QuoteSystem()
		{
			s_systems = new List<QuoteSystem>();

			var doc = new XmlDocument();
			doc.LoadXml(Resources.QuoteSystemData);
			foreach (XmlNode node in doc.SafeSelectNodes("//QuoteSystem"))
				s_systems.Add(XmlSerializationHelper.DeserializeFromString<QuoteSystem>(node.OuterXml));
		}

		public QuoteSystem()
		{
			Levels = new BulkObservableList<QuotationMark>();
		}

		public QuoteSystem(QuotationMark firstLevel,
			string quotationDashMarker = null, string quotationDashEndMarker = null, bool quotationDashesIndicateChangeOfSpeakerInFirstLevelQuotes = false) : this()
		{
			Levels.Add(firstLevel);
			QuotationDashMarker = quotationDashMarker;
			QuotationDashEndMarker = quotationDashEndMarker;
			QuotationDashesIndicateChangeOfSpeakerInFirstLevelQuotes = quotationDashesIndicateChangeOfSpeakerInFirstLevelQuotes;
		}

		public static QuoteSystem Default
		{
			get { return s_systems.SingleOrDefault(s => s.Name == "Guillemets"); }
		}

		public static IEnumerable<QuoteSystem> UniquelyGuessableSystems
		{
			get
			{
				if (s_uniquelyGuessableSystems == null)
				{
					s_uniquelyGuessableSystems = AllUniqueFirstLevelSystems.ToList();
					var comparer = new FirstLevelQuoteSystemComparer();
					foreach (var qs in s_systems.Where(q => q.QuotationDashMarker != null))
					{
						if (!s_uniquelyGuessableSystems.Any(s => comparer.Equals(s, qs) && s.QuotationDashMarker == qs.QuotationDashMarker))
						{
							var minimallySpecifiedSystem = GetOrCreateQuoteSystem(qs.FirstLevel, qs.QuotationDashMarker, null, false);
							if (string.IsNullOrEmpty(minimallySpecifiedSystem.Name))
								minimallySpecifiedSystem = qs;
							s_uniquelyGuessableSystems.Add(minimallySpecifiedSystem);
						}
					}
				}
				return s_uniquelyGuessableSystems;
			}
		}

		public static IEnumerable<QuoteSystem> AllUniqueFirstLevelSystems
		{
			get { return s_systems.Where(q => q.QuotationDashMarker == null).Distinct(new FirstLevelQuoteSystemComparer()); }
		}

		public static QuoteSystem GetOrCreateQuoteSystem(QuotationMark firstLevel,
			string quotationDashMarker, string quotationDashEndMarker, bool quotationDashesIndicateChangeOfSpeakerInFirstLevelQuotes)
		{
			var newQuoteSystem = new QuoteSystem(firstLevel, quotationDashMarker, quotationDashEndMarker, quotationDashesIndicateChangeOfSpeakerInFirstLevelQuotes);

			var match = s_systems.SingleOrDefault(qs => qs.Equals(newQuoteSystem));
			return match ?? newQuoteSystem;
		}

		public string Name { get; set; }

		public string MajorLanguage { get; set; }

		[XmlIgnore]
		public BulkObservableList<QuotationMark> Levels;

		[XmlIgnore]
		public QuotationMark FirstLevel { get { return Levels[0]; } }

		[XmlElement("StartQuoteMarker")]
		public string StartQuoteMarker_DeprecatedXml
		{
			get { return FirstLevel.Open; }
			set
			{
				if (Levels.Count == 0)
					Levels.Add(new QuotationMark(value, null, value, 1, QuotationMarkingSystemType.Normal));
				else
				{
					string close = Levels[0].Close;
					Levels[0] = new QuotationMark(value, close, value, 1, QuotationMarkingSystemType.Normal);
				}
			}
		}

		[XmlElement("EndQuoteMarker")]
		public string EndQuoteMarker_DeprecatedXml
		{
			get { return FirstLevel.Close; }
			set
			{
				if (Levels.Count == 0)
					Levels.Add(new QuotationMark(value, null, value, 1, QuotationMarkingSystemType.Normal));
				else
				{
					string open = Levels[0].Open;
					string cont = Levels[0].Continue;
					Levels[0] = new QuotationMark(open, value, cont, 1, QuotationMarkingSystemType.Normal);
				}
			}
		}

		public string QuotationDashMarker { get; set; }

		public string QuotationDashEndMarker { get; set; }

		[DefaultValue(false)]
		public bool QuotationDashesIndicateChangeOfSpeakerInFirstLevelQuotes { get; set; }

		public QuoteSystem GetCorrespondingFirstLevelQuoteSystem()
		{
			return AllUniqueFirstLevelSystems.FirstOrDefault(f => f.FirstLevel.Open == FirstLevel.Open && f.FirstLevel.Close == FirstLevel.Close);
		}

		public override string ToString()
		{
			return FirstLevel.Open + "  " + FirstLevel.Close;
		}

		#region Equals methods overrides
		      
		protected bool Equals(QuoteSystem other)
		{
			if (other == null)
				return false;
			if (Levels == null)
				return other.Levels == null;
			if (other.Levels == null)
				return false;
			return Levels.SequenceEqual(other.Levels) &&
				string.Equals(QuotationDashMarker, other.QuotationDashMarker) && 
				string.Equals(QuotationDashEndMarker, other.QuotationDashEndMarker) && 
				QuotationDashesIndicateChangeOfSpeakerInFirstLevelQuotes.Equals(other.QuotationDashesIndicateChangeOfSpeakerInFirstLevelQuotes);
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
				int hashCode = (Levels != null ? Levels.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (QuotationDashMarker != null ? QuotationDashMarker.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (QuotationDashEndMarker != null ? QuotationDashEndMarker.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ QuotationDashesIndicateChangeOfSpeakerInFirstLevelQuotes.GetHashCode();
				return hashCode;
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
	}

	public class FirstLevelQuoteSystemComparer : IEqualityComparer<QuoteSystem>
	{
		public bool Equals(QuoteSystem x, QuoteSystem y)
		{
			return Equals(x.FirstLevel, y.FirstLevel);
		}

		public int GetHashCode(QuoteSystem obj)
		{
			return obj.GetHashCode();
		}
	}
}
