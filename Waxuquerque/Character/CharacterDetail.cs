using System.Collections.Generic;

namespace Glyssen.Character
{
	public enum CharacterAge
	{
		Adult = 0,
		Child = 6,
		YoungAdult = -1,
		Elder = 1,
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
		public CharacterVerseData.StandardCharacter StandardCharacterType { get; set; } = CharacterVerseData.StandardCharacter.NonStandard;
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
			if (x == CharacterAge.Child && y != CharacterAge.Child)
				return -1;
			if (y == CharacterAge.Child && x != CharacterAge.Child)
				return 1;
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
