using System;
using System.Xml.Serialization;

namespace Glyssen.VoiceActor
{
	public class VoiceActor : IEquatable<VoiceActor>
	{
		public VoiceActor()
		{
			Name = "";
		}

		[XmlText]
		public string Name { get; set; }

		[XmlAttribute("Id")]
		public int Id { get; set; }

		[XmlAttribute("Gender")]
		public string Gender { get; set; }

		[XmlAttribute("Age")]
		public string Age { get; set; }

		public bool HasMeaningfulData()
		{
			return Name != null || Gender != null || Age != null;
		}

		#region IEquatable<T> members
		public bool Equals(VoiceActor other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return Id == other.Id;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			var other = obj as VoiceActor;
			return other != null && Equals(other);
		}

		public override int GetHashCode()
		{
			return Id;
		}

		public static bool operator ==(VoiceActor left, VoiceActor right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(VoiceActor left, VoiceActor right)
		{
			return !Equals(left, right);
		}
		#endregion
	}
}
