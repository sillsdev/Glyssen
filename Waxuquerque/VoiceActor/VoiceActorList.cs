using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using SIL.Xml;

namespace Waxuquerque.VoiceActor
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

		public int ActiveMaleAdultActorCount { get { return ActiveActors.Count(a => a.Gender == ActorGender.Male && a.Age != ActorAge.Child); } }
		public int ActiveFemaleAdultActorCount { get { return ActiveActors.Count(a => a.Gender == ActorGender.Female && a.Age != ActorAge.Child); } }
		public int ActiveChildActorCount { get { return ActiveActors.Count(a => a.Age == ActorAge.Child); } }

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
