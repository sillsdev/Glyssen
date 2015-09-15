using System.Collections.Generic;

namespace Glyssen.Character
{
	public enum CharacterAge
	{
		Adult,
		Child,
		YoungAdult,
		Elder,
	}

	public enum CharacterGender
	{
		Either,
		Female,
		PreferFemale,
		Male,
		PreferMale,
		Neuter,
	}

	public class CharacterDetail
	{
		public string CharacterId { get; set; }
		public int MaxSpeakers { get; set; }
		public CharacterGender Gender { get; set; }
		public CharacterAge Age { get; set; }
		public bool Status { get; set; }
		public string Comment { get; set; }
		public string ReferenceComment { get; set; }
	}

	public class CharacterGenderComparer : IComparer<CharacterGender>
	{
		private static CharacterGenderComparer s_singleton;

		public int Compare(CharacterGender x, CharacterGender y)
		{
			return ((int)x).CompareTo((int)y);
		}

		public static CharacterGenderComparer Singleton
		{
			get
			{
				if (s_singleton == null)
					s_singleton = new CharacterGenderComparer();
				return s_singleton;
			}
		}
	}

	public class CharacterAgeComparer : IComparer<CharacterAge>
	{
		private static CharacterAgeComparer s_singleton;

		public int Compare(CharacterAge x, CharacterAge y)
		{
			return ((int)x).CompareTo((int)y);
		}

		public static CharacterAgeComparer Singleton
		{
			get
			{
				if (s_singleton == null)
					s_singleton = new CharacterAgeComparer();
				return s_singleton;
			}
		}
	}
}
