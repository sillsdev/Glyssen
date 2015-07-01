using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using SIL.Xml;

namespace Glyssen.VoiceActor
{
	[XmlRoot("VoiceActors")]
	public class VoiceActorList
	{
		public VoiceActorList()
		{
			Actors = new List<VoiceActorEntity>();
		}

		[XmlElement("VoiceActor")]
		public List<VoiceActorEntity> Actors { get; set; }

		public void SaveToFile(string filename)
		{
			XmlSerializationHelper.SerializeToFile<VoiceActorList>(filename, this);
		}

		public static VoiceActorList LoadVoiceActorListFromFile(string filename)
		{
			return XmlSerializationHelper.DeserializeFromFile<VoiceActorList>(filename);
		}
	}
}
