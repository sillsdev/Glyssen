using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using SIL.Xml;

namespace Glyssen.VoiceActor
{
	[XmlRoot("VoiceActors")]
	public class VoiceActorList
	{
		public VoiceActorList()
		{
			AllActors = new List<VoiceActor>();
		}

		[XmlElement("VoiceActor")]
		public List<VoiceActor> AllActors { get; set; }

		public IEnumerable<VoiceActor> ActiveActors
		{
			get { return AllActors.Where(a => !a.IsInactive); }
		}

		public void SaveToFile(string filename)
		{
			XmlSerializationHelper.SerializeToFile(filename, this);
		}

		public static VoiceActorList LoadVoiceActorListFromFile(string filename)
		{
			return XmlSerializationHelper.DeserializeFromFile<VoiceActorList>(filename);
		}

		public VoiceActor GetVoiceActorById(int voiceActorId)
		{
			return AllActors.FirstOrDefault(a => a.Id == voiceActorId);
		}
	}
}
