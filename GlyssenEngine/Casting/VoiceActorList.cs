using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using SIL.Xml;

namespace GlyssenEngine.Casting
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

		public void Save(TextWriter textWriter)
		{
			Project.Serialize(textWriter, this, out _);
		}

		public static VoiceActorList LoadVoiceActorList(TextReader textReader)
		{
			return XmlSerializationHelper.Deserialize<VoiceActorList>(textReader);
		}

		public VoiceActor GetVoiceActorById(int voiceActorId)
		{
			return AllActors.FirstOrDefault(a => a.Id == voiceActorId);
		}
	}
}
