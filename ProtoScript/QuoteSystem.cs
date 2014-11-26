using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using Palaso.Xml;

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
			doc.LoadXml(QuoteSystemData.quoteSystemsXml);
			foreach(XmlNode node in doc.SafeSelectNodes("//QuoteSystem"))
			{
				var qs = XmlSerializationHelper.DeserializeFromString<QuoteSystem>(node.OuterXml);
				s_systems.Add(qs.Name, qs);
			}
			
//			foreach (var line in QuoteSystemData.quoteSystems.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
//			{
//				var items = line.Split('\t');
//				s_systems.Add(items[3], new QuoteSystem { StartQuoteMarker = items[0], EndQuoteMarker = items[1], MajorLanguage = items[2], Name = items[3] });
//			}
		}

		public string Name;

		public string MajorLanguage;

		public string StartQuoteMarker;

		public string EndQuoteMarker;

		public override string ToString()
		{
			return StartQuoteMarker + "  " + EndQuoteMarker;
		}
	}
}
