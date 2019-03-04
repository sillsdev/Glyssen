using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Glyssen.Character;

namespace Glyssen.VoiceActor
{
	public enum ActorAge
	{
		Adult = CharacterAge.Adult,
		Child = CharacterAge.Child,
		YoungAdult = CharacterAge.YoungAdult,
		Elder = CharacterAge.Elder,
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

	public enum MatchLevel
	{
		Perfect,
		Acceptable,
		Poor,
		Mismatch,
	}

	public class MatchQuality
	{
		public MatchLevel AgeMatchQuality { get; set; }
		public MatchLevel GenderMatchQuality { get; set; }

		public MatchQuality(MatchLevel genderMatchQuality, MatchLevel ageMatchQuality)
		{
			GenderMatchQuality = genderMatchQuality;
			AgeMatchQuality = ageMatchQuality;
		}

		public override bool Equals(object obj)
		{
			var matchQuality = obj as MatchQuality;
			return matchQuality != null ? Equals(matchQuality) : base.Equals(obj);
		}

		protected bool Equals(MatchQuality other)
		{
			return AgeMatchQuality == other.AgeMatchQuality && GenderMatchQuality == other.GenderMatchQuality;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((int)AgeMatchQuality * 397) ^ (int)GenderMatchQuality;
			}
		}

		public static bool operator ==(MatchQuality left, MatchQuality right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(MatchQuality left, MatchQuality right)
		{
			return !Equals(left, right);
		}
	}

	public class VoiceActor : IEquatable<VoiceActor>
	{
		public VoiceActor()
		{
			Name = "";
		}

		public VoiceActor MakeCopy()
		{
			return (VoiceActor)MemberwiseClone();
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
		[DefaultValue("Normal")]
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
		[DefaultValue(false)]
		public bool Status { get; set; }

		[XmlAttribute("IsCameo")]
		[DefaultValue(false)]
		public bool IsCameo { get; set; }

		[XmlAttribute("IsInactive")]
		[DefaultValue(false)]
		public bool IsInactive { get; set; }

		public bool IsValid()
		{
			return !string.IsNullOrEmpty(Name);
		}

		public bool IsInterchangeableWith(VoiceActor otherActor)
		{
			return Gender == otherActor.Gender &&
				Age == otherActor.Age &&
				VoiceQuality == otherActor.VoiceQuality &&
				Status == otherActor.Status &&
				IsCameo == otherActor.IsCameo &&
				IsInactive == otherActor.IsInactive;
		}

		public MatchLevel GetAgeMatchQuality(CharacterDetail character)
		{
			switch (character.Age)
			{
				case CharacterAge.Child:
					switch (Age)
					{
						case ActorAge.Child: return MatchLevel.Perfect;
						case ActorAge.YoungAdult: return Gender == ActorGender.Female ? MatchLevel.Acceptable : MatchLevel.Mismatch;
						case ActorAge.Adult: return Gender == ActorGender.Female ? MatchLevel.Poor : MatchLevel.Mismatch;
						default: return MatchLevel.Mismatch;
					}
				default:
					switch (Math.Abs((int)Age - (int)character.Age))
					{
						case 0: return MatchLevel.Perfect;
						case 1: return MatchLevel.Acceptable;
						case 2: return MatchLevel.Poor;
						default: return MatchLevel.Mismatch;
					}
			}
		}


		public MatchLevel GetGenderMatchQuality(CharacterDetail characterDetail)
		{
			switch (Gender)
			{
				case ActorGender.Male:
					switch (characterDetail.Gender)
					{
						case CharacterGender.Male:
						case CharacterGender.PreferMale:
						case CharacterGender.Either:
						case CharacterGender.Neuter:
							return MatchLevel.Perfect;
						case CharacterGender.PreferFemale:
							// This might seem odd at first glance, but the only "prefer female" characters in the data
							// could easily be performed by a male actor.
							return MatchLevel.Acceptable;
						default:
							return MatchLevel.Mismatch;
					}

				default:
					switch (characterDetail.Gender)
					{
						case CharacterGender.Female:
						case CharacterGender.PreferFemale:
						case CharacterGender.Neuter:
							return MatchLevel.Perfect;
//PG-1055						case CharacterGender.Either:
//									return GenderMatchQuality.Acceptable;
						default:
							if (characterDetail.Age == CharacterAge.Child)
							{
								switch (Age)
								{
									case ActorAge.Adult: return MatchLevel.Poor;
									case ActorAge.Child:
									case ActorAge.YoungAdult:
										return MatchLevel.Acceptable;
									default: return MatchLevel.Mismatch;
								}
							}
							return MatchLevel.Mismatch;
					}
			}
		}

		public bool Matches(CharacterDetail character, bool strictAgeMatching = false)
		{
			if (GetGenderMatchQuality(character) != MatchLevel.Perfect)
				return false;

			var ageMatchQuality = GetAgeMatchQuality(character);

			if (ageMatchQuality == MatchLevel.Mismatch)
				return false;

			return !strictAgeMatching || ageMatchQuality == MatchLevel.Perfect;
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
