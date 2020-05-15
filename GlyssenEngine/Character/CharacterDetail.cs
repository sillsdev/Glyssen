using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using SIL.Scripture;

namespace GlyssenEngine.Character
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
		//public bool Status { get; set; }
		public string Comment { get; set; }
		public string ReferenceComment { get; set; }
		public string DefaultFCBHCharacter { get; set; }
		public CharacterVerseData.StandardCharacter StandardCharacterType { get; set; } = CharacterVerseData.StandardCharacter.NonStandard;

		private const string kScriptureRegex = "(?<bookId>[1-3A-Z][A-Z][A-Z]) (?<chapter>\\d+):(?<verse>\\d+)";
		static Regex s_regexFirstScriptureRef = new Regex("^" + kScriptureRegex, RegexOptions.Compiled);
		static Regex s_regexLastScriptureRef = new Regex(kScriptureRegex + "$", RegexOptions.Compiled);

		public BCVRef FirstReference
		{
			get
			{
				if (string.IsNullOrEmpty(ReferenceComment))
					return null;
				var m = s_regexFirstScriptureRef.Match(ReferenceComment);
				if (m.Success)
					return new BCVRef(BCVRef.BookToNumber(m.Result("${bookId}")), int.Parse(m.Result("${chapter}")), int.Parse(m.Result("${verse}")));
				throw new DataException($"Invalid ReferenceComment ({ReferenceComment}) in character detail for character {CharacterId}!");
			}
		}

		public BCVRef LastReference
		{
			get
			{
				if (string.IsNullOrEmpty(ReferenceComment))
					return null;
				var m = s_regexLastScriptureRef.Match(ReferenceComment);
				if (m.Success)
					return new BCVRef(BCVRef.BookToNumber(m.Result("${bookId}")), int.Parse(m.Result("${chapter}")), int.Parse(m.Result("${verse}")));
				throw new DataException($"Invalid ReferenceComment ({ReferenceComment}) in character detail for character {CharacterId}!");
			}
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
