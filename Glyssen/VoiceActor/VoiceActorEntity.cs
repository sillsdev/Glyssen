using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Glyssen.VoiceActor
{
	public class VoiceActorEntity
	{
		public VoiceActorEntity()
		{

		}

		[XmlText]
		public string Name { get; set; }

		[XmlAttribute("Id")]
		public int Id { get; set; }

		[XmlAttribute("Gender")]
		public string Gender { get; set; }

		[XmlAttribute("Age")]
		public string Age { get; set; }

		public bool isEmpty()
		{
			return Name == null && Gender == null && Age == null;
		}
	}
}
