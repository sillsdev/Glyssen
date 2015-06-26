using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
