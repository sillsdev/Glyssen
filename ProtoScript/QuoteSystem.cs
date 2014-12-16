using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Palaso.Xml;
using ProtoScript.Properties;

namespace ProtoScript
{
	[XmlRoot]
	public class QuoteSystem
	{
		public static string AnyPunctuation
		{
			get { return Palaso.Extensions.StringExtensions.kObjReplacementChar.ToString(CultureInfo.InvariantCulture); }
		}
		
		private static IDictionary<string, QuoteSystem> s_systems;

		static QuoteSystem()
		{
			LoadDefaultSystems();
		}

		public static QuoteSystem Default
		{
			get
			{
				QuoteSystem defaultQuoteSystem;
				TryGetQuoteSystem("Guillemets", out defaultQuoteSystem);
				return defaultQuoteSystem;
			}
		}

		public static IEnumerable<QuoteSystem> AllSystems
		{
			get { return s_systems.Values; }
		}

		public static IEnumerable<QuoteSystem> AllUniqueFirstLevelSystems
		{
			get { return s_systems.Values.Where(q => q.QuotationDashMarker == null).Distinct(new FirstLevelQuoteSystemComparer()); }
		}

		public static bool TryGetQuoteSystem(string name, out QuoteSystem quoteSystem)
		{
			return s_systems.TryGetValue(name, out quoteSystem);
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

			var match = AllSystems.SingleOrDefault(qs => qs.Equals(newQuoteSystem));
			return match ?? newQuoteSystem;
		}

		private static void LoadDefaultSystems()
		{
			if (s_systems != null)
				return;

			s_systems = new Dictionary<string, QuoteSystem>();

			var doc = new XmlDocument();
			doc.LoadXml(Resources.QuoteSystemData);
			foreach(XmlNode node in doc.SafeSelectNodes("//QuoteSystem"))
			{
				var qs = XmlSerializationHelper.DeserializeFromString<QuoteSystem>(node.OuterXml);
				s_systems.Add(qs.Name, qs);
			}
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
