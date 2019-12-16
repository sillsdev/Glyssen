using System.Collections.Generic;
using GlyssenEngine.Character;

namespace GlyssenEngine.Rules
{
	public class CharacterGenderMatchingOptions
	{
		private readonly Dictionary<CharacterGender, HashSet<CharacterGender>> m_matchingRules = new Dictionary<CharacterGender, HashSet<CharacterGender>>();
		private static CharacterGenderMatchingOptions s_moderate;
		private static CharacterGenderMatchingOptions s_loose;

		public static CharacterGenderMatchingOptions Moderate
		{
			get
			{
				if (s_moderate == null)
				{
					s_moderate = new CharacterGenderMatchingOptions();
					s_moderate.AddRule(CharacterGender.Male, new[] { CharacterGender.Male, CharacterGender.PreferMale, });
					s_moderate.AddRule(CharacterGender.Female, new[] { CharacterGender.Female, CharacterGender.PreferFemale, });
					s_moderate.AddRule(CharacterGender.PreferMale, new[] { CharacterGender.Male, CharacterGender.PreferMale, });
					s_moderate.AddRule(CharacterGender.PreferFemale, new[] { CharacterGender.Female, CharacterGender.PreferFemale, });
					s_moderate.AddRule(CharacterGender.Either, new[] { CharacterGender.Either, CharacterGender.Neuter, });
					s_moderate.AddRule(CharacterGender.Neuter, new[] { CharacterGender.Either, CharacterGender.Neuter, });
				}
				return s_moderate;
			}
		}

		public static CharacterGenderMatchingOptions Loose
		{
			get
			{
				if (s_loose == null)
				{
					s_loose = new CharacterGenderMatchingOptions();
					s_loose.AddRule(CharacterGender.Male, new[] { CharacterGender.Male, CharacterGender.PreferMale, CharacterGender.Either, CharacterGender.Neuter, });
					s_loose.AddRule(CharacterGender.Female, new[] { CharacterGender.Female, CharacterGender.PreferFemale, CharacterGender.Either, CharacterGender.Neuter, });
					s_loose.AddRule(CharacterGender.PreferMale, new[] { CharacterGender.Male, CharacterGender.PreferMale, CharacterGender.Either, CharacterGender.Neuter, });
					s_loose.AddRule(CharacterGender.PreferFemale, new[] { CharacterGender.Female, CharacterGender.PreferFemale, CharacterGender.Either, CharacterGender.Neuter, });
					s_loose.AddRule(CharacterGender.Either, null);
					s_loose.AddRule(CharacterGender.Neuter, null);
				}
				return s_loose;
			}
		}

		public void AddRule(CharacterGender gender, IEnumerable<CharacterGender> matchingGenders)
		{
			if (matchingGenders == null)
				m_matchingRules.Add(gender, null);
			else
				m_matchingRules.Add(gender, new HashSet<CharacterGender>(matchingGenders));
		}

		public bool Matches(CharacterGender g1, CharacterGender g2)
		{
			var hashSet = m_matchingRules[g1];
			if (hashSet == null)
				return true;
			return hashSet.Contains(g2);
		}
	}
}
