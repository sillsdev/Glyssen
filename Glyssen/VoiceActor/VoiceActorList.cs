using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using SIL.Xml;

namespace Glyssen.VoiceActor
{
	[XmlRoot("VoiceActors")]
	public class VoiceActorList
	{
		public VoiceActorList()
		{
			Actors = new List<VoiceActor>();
		}

		[XmlElement("VoiceActor")]
		public List<VoiceActor> Actors { get; set; }

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
