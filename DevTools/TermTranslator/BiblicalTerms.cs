using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using SIL.Scripture;

namespace DevTools.TermTranslator
{
    public class Terms
    {
        public Terms()
        {
            Locals = new List<Localization>();
        }

        [XmlElement("Localization")]
        public List<Localization> Locals { get; set; }

    }

    public class Localization
    {
        [XmlAttribute("Id")]
        public string Id { get; set; }

        [XmlAttribute("Gloss")]
        public string Gloss { get; set; }

        [XmlText]
        public string Text { get; set; }
    }
}
