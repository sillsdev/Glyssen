using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Palaso.Xml;
using ProtoScript.Properties;

namespace ProtoScript.Quote
{
	[XmlRoot]
	public class QuoteSystem
	{
		public static string AnyPunctuation
		{
			get { return Palaso.Extensions.StringExtensions.kObjReplacementChar.ToString(CultureInfo.InvariantCulture); }
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
							var minimallySpecifiedSystem = GetOrCreateQuoteSystem(qs.StartQuoteMarker,
								qs.EndQuoteMarker, qs.QuotationDashMarker, null, false);
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

		public static QuoteSystem GetOrCreateQuoteSystem(string startQuoteMarker, string endQuoteMarker,
			string quotationDashMarker, string quotationDashEndMarker, bool quotationDashesIndicateChangeOfSpeakerInFirstLevelQuotes)
		{
			var newQuoteSystem = new QuoteSystem
			{
				StartQuoteMarker = startQuoteMarker,
				EndQuoteMarker = endQuoteMarker,
				QuotationDashMarker = quotationDashMarker,
				QuotationDashEndMarker = quotationDashEndMarker,
				QuotationDashesIndicateChangeOfSpeakerInFirstLevelQuotes = quotationDashesIndicateChangeOfSpeakerInFirstLevelQuotes
			};

			var match = s_systems.SingleOrDefault(qs => qs.Equals(newQuoteSystem));
			return match ?? newQuoteSystem;
		}

		public string Name { get; set; }

		public string MajorLanguage { get; set; }

		public string StartQuoteMarker { get; set; }

		public string EndQuoteMarker { get; set; }

		public string QuotationDashMarker { get; set; }

		public string QuotationDashEndMarker { get; set; }

		[DefaultValue(false)]
		public bool QuotationDashesIndicateChangeOfSpeakerInFirstLevelQuotes { get; set; }

		public override string ToString()
		{
			return StartQuoteMarker + "  " + EndQuoteMarker;
		}

		#region Equals methods overrides

		protected bool Equals(QuoteSystem other)
		{
			return string.Equals(StartQuoteMarker, other.StartQuoteMarker) && 
				string.Equals(EndQuoteMarker, other.EndQuoteMarker) &&
				string.Equals(QuotationDashMarker, other.QuotationDashMarker) &&
				string.Equals(QuotationDashEndMarker, other.QuotationDashEndMarker) &&
				QuotationDashesIndicateChangeOfSpeakerInFirstLevelQuotes == other.QuotationDashesIndicateChangeOfSpeakerInFirstLevelQuotes;
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
				return ((StartQuoteMarker != null ? StartQuoteMarker.GetHashCode() : 0) * 397) ^ (EndQuoteMarker != null ? EndQuoteMarker.GetHashCode() : 0);
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
			return string.Equals(x.StartQuoteMarker, y.StartQuoteMarker) &&
				string.Equals(x.EndQuoteMarker, y.EndQuoteMarker);
		}

		public int GetHashCode(QuoteSystem obj)
		{
			return obj.GetHashCode();
		}
	}
}
