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

	public abstract class CharacterDetail
	{
		public abstract string CharacterId { get; }
		public abstract int MaxSpeakers { get; }
		public abstract CharacterGender Gender { get; }
		public abstract CharacterAge Age { get; }
		public abstract CharacterVerseData.CharacterType CharacterType { get; }
	}

	public class BiblicalCharacterDetail : CharacterDetail
	{
		private CharacterGender m_gender;
		private int m_maxSpeakers;
		private CharacterAge m_age;

		public BiblicalCharacterDetail(string characterId, int maxSpeakers, CharacterGender gender, CharacterAge age)
		{
			CharacterId = characterId;
			m_maxSpeakers = maxSpeakers;
			m_gender = gender;
			m_age = age;
		}

		public override string CharacterId { get; }

		public override int MaxSpeakers => m_maxSpeakers;
		public override CharacterGender Gender => m_gender;
		public override CharacterAge Age => m_age;

		public bool Status { get; set; }
		public string Comment { get; set; }
		public string ReferenceComment { get; set; }
		public override CharacterVerseData.CharacterType CharacterType => CharacterVerseData.CharacterType.NonStandard;

		public void Redefine(BiblicalCharacterDetail basedOn)
		{
			m_maxSpeakers = basedOn.MaxSpeakers;
			m_gender = basedOn.Gender;
			m_age = basedOn.Age;
		}
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
