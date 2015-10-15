using System.Collections.Generic;
using System.Xml.Serialization;

namespace DevTools.TermTranslator
{
	[XmlRoot("tmx")]
	public sealed class TmxFormat
	{
		public TmxFormat()
		{
			Version = "1.4";
			Header = new TmxHeader();
			Body = new TmxBody();
		}

		public TmxFormat(TmxFormat copyFrom)
		{
			Version = copyFrom.Version;
			Header = copyFrom.Header.Clone();
			Body = copyFrom.Body.Clone();
		}

		[XmlAttribute("version")]
		public string Version { get; set; }

		[XmlElement("header")]
		public TmxHeader Header { get; set; }

		[XmlElement("body")]
		public TmxBody Body { get; set; }
	}

	public class TmxHeader
	{
		public TmxHeader()
		{
			SrcLang = "en";
			AdminLang = "en";
			CreationTool = "Palaso Localization Manager";
			CTVersion = "2.0.35.0";
			SegType = "block";
			DataType = "unknown";
			OTMF = "PalasoTMXUtils";
		}

		public TmxHeader Clone()
		{
			var clone = (TmxHeader)MemberwiseClone();
			clone.Props = new Prop[Props.Length];
			for (int index = 0; index < Props.Length; index++)
				clone.Props[index] = Props[index].Clone();
			return clone;
		}

		[XmlAttribute("srclang")]
		public string SrcLang { get; set; }

		[XmlAttribute("adminlang")]
		public string AdminLang { get; set; }

		[XmlAttribute("creationtool")]
		public string CreationTool { get; set; }

		[XmlAttribute("creationtoolversion")]
		public string CTVersion { get; set; }

		[XmlAttribute("segtype")]
		public string SegType { get; set; }

		[XmlAttribute("datatype")]
		public string DataType { get; set; }

		[XmlAttribute("o-tmf")]
		public string OTMF { get; set; }

		[XmlElement("prop")]
		public Prop[] Props { get; set; }
	}

	public class Prop
	{
		public Prop()
		{
		}

		public Prop(string propType, string inner)
		{
			PropType = propType;
			InnerText = inner;
		}

		public Prop Clone()
		{
			return new Prop(PropType, InnerText);
		}

		[XmlAttribute("type")]
		public string PropType { get; set; }

		[XmlText]
		public string InnerText { get; set; }
	}

	public class TmxBody
	{
		public TmxBody()
		{
			Tus = new List<Tu>();
		}

		public TmxBody Clone()
		{
			var clone = new TmxBody();
			foreach (var tu in Tus)
				clone.Tus.Add(tu.Clone());
			return clone;
		}

		[XmlElement("tu")]
		public List<Tu> Tus { get; set; }
	}

	public class Tu
	{
		public Tu()
		{
			Tuvs = new List<Tuv>();
		}

		public Tu(string id)
		{
			Tuvs = new List<Tuv>();
			Tuid = id;
		}

		public Tu Clone()
		{
			var clone = new Tu(Tuid);
			clone.Note = Note;
			if (Prop != null)
				clone.Prop = Prop.Clone();
			foreach (var tuv in Tuvs)
				clone.Tuvs.Add(new Tuv(tuv.Lang, tuv.LocalizedTerm));
			return clone;
		}

		[XmlElement("note")]
		public string Note { get; set; }

		[XmlElement("prop")]
		public Prop Prop { get; set; }

		[XmlAttribute("tuid")]
		public string Tuid { get; set; }

		[XmlElement("tuv")]
		public List<Tuv> Tuvs { get; set; }

		public void AddTuv(string lang, string localTerm)
		{
			Tuvs.Add(new Tuv(lang, localTerm));
		}
	}

	public class Tuv
	{
		public Tuv()
		{

		}

		public Tuv(string lang, string localTerm)
		{
			Lang = lang;
			LocalizedTerm = localTerm;
		}

		[XmlAttribute("xml:lang")]
		public string Lang { get; set; }

		[XmlElement("seg")]
		public string LocalizedTerm { get; set; }
	}
}
