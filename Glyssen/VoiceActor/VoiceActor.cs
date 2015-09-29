using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Glyssen.Character;

namespace Glyssen.VoiceActor
{
	public enum ActorAge
	{
		Adult,
		Child,
		YoungAdult,
		Elder,
	}

	public enum ActorGender
	{
		Male,
		Female,
	}

	public enum VoiceQuality
	{
		Normal,
		Dramatic,
		Authoritative,
		Weak,
		Suspicious,
		Clear
	}

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
		[Browsable(false)]
		public string GenderDeprecatedString
		{
			get { return Gender.ToString(); }
			set
			{
				ActorGender val;
				if (!Enum.TryParse(value, false, out val))
					val = MigrateFromDeprecatedVersionOfGenderData(value);
				Gender = val;
			}
		}

		[XmlIgnore]
		public ActorGender Gender { get; set; }

		[XmlAttribute("Age")]
		[Browsable(false)]
		public string AgeDeprecatedString
		{
			get { return Age.ToString(); }
			set
			{
				ActorAge val;
				if (!Enum.TryParse(value, false, out val))
					val = MigrateFromDeprecatedVersionOfAgeData(value);
				Age = val;
			}
		}

		[XmlIgnore]
		public ActorAge Age { get; set; }

		[XmlAttribute("VoiceQuality")]
		[Browsable(false)]
		public string VoiceQualityDeprecatedString
		{
			get { return VoiceQuality.ToString(); }
			set
			{
				VoiceQuality val;
				if (!Enum.TryParse(value, false, out val))
					val = MigrateFromDeprecatedVersionOfVoiceQualityData(value);
				VoiceQuality = val;
			}
		}

		[XmlIgnore]
		public VoiceQuality VoiceQuality { get; set; }

		[XmlAttribute("Status")]
		public bool Status { get; set; }

		[XmlAttribute("IsCameo")]
		public bool IsCameo { get; set; }

		public bool IsValid()
		{
			return !string.IsNullOrEmpty(Name);
		}

		public bool Matches(CharacterDetail character, bool strictAgeMatching = false)
		{
			bool result = true;
			switch (character.Gender)
			{
				case CharacterGender.Female:
				case CharacterGender.PreferFemale: // TODO: Handle preferences based on user settings
					result &= Gender == ActorGender.Female;
					break;
				case CharacterGender.Male:
				case CharacterGender.PreferMale: // TODO: Handle preferences based on user settings
					result &= Gender == ActorGender.Male;
					break;
			}

			if (strictAgeMatching)
			{
				switch (character.Age)
				{
					case CharacterAge.Child:
						result &= Age == ActorAge.Child;
						break;
					case CharacterAge.YoungAdult:
						result &= Age == ActorAge.YoungAdult;
						break;
					case CharacterAge.Elder:
						result &= Age == ActorAge.Elder;
						break;
					default:
						result &= Age == ActorAge.Adult;
						break;
				}
			}
			else if (character.Age == CharacterAge.Child && Age != ActorAge.Child)
				result = false;
			else if (Age == ActorAge.Child && character.Age != CharacterAge.Child)
				result = false;

			return result;
		}

		private ActorGender MigrateFromDeprecatedVersionOfGenderData(string deprecatedData)
		{
			// Do not localize these strings
			switch (deprecatedData)
			{
				case "F - Female": return ActorGender.Female;
				default: return ActorGender.Male;
			}
		}

		private ActorAge MigrateFromDeprecatedVersionOfAgeData(string deprecatedData)
		{
			// Do not localize these strings
			switch (deprecatedData)
			{
				case "O - Old":
				case "E - Elder":
					return ActorAge.Elder;
				case "Y - Young":
				case "Y - Young Adult":
					return ActorAge.YoungAdult;
				case "C - Child":
					return ActorAge.Child;
				default:
					return ActorAge.Adult;
			}
		}

		private VoiceQuality MigrateFromDeprecatedVersionOfVoiceQualityData(string deprecatedData)
		{
			// Do not localize these strings
			switch (deprecatedData)
			{
				case "D - Dramatic":
					return VoiceQuality.Dramatic;
				case "A - Authoritative/Firm":
					return VoiceQuality.Authoritative;
				case "W - Weak":
					return VoiceQuality.Weak;
				case "D - Deceptive":
					return VoiceQuality.Suspicious;
				case "C - Clear":
					return VoiceQuality.Clear;
				default: // "N - Normal"
					return VoiceQuality.Normal;
			}
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
