using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using Palaso.Xml;
using ProtoScript.Properties;

namespace ProtoScript
{
	[XmlRoot]
	public class QuoteSystem
	{
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

		public static ICollection<QuoteSystem> AllSystems
		{
			get { return s_systems.Values; }
		}

		public static bool TryGetQuoteSystem(string name, out QuoteSystem quoteSystem)
		{
			return s_systems.TryGetValue(name, out quoteSystem);
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

		public string Name;

		public string MajorLanguage;

		public string StartQuoteMarker;

		public string EndQuoteMarker;

		public override string ToString()
		{
			return StartQuoteMarker + "  " + EndQuoteMarker;
		}

		#region Equals methods overrides

		protected bool Equals(QuoteSystem other)
		{
			return string.Equals(StartQuoteMarker, other.StartQuoteMarker) && string.Equals(EndQuoteMarker, other.EndQuoteMarker);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			if (obj.GetType() != this.GetType())
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
}
