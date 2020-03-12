using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

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

		/// <summary>
		/// Gets a voice actor list representing the data (or a new list if null).
		/// Note: This method will take care of disposing the TextReader object.
		/// </summary>
		public static VoiceActorList LoadVoiceActorList(TextReader data)
		{
			return Project.Deserialize<VoiceActorList>(data) ?? new VoiceActorList();
		}

		public VoiceActor GetVoiceActorById(int voiceActorId)
		{
			return AllActors.FirstOrDefault(a => a.Id == voiceActorId);
		}
	}
}
