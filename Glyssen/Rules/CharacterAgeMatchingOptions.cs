using System.Collections.Generic;
using Glyssen.Character;

namespace Glyssen.Rules
{
	public class CharacterAgeMatchingOptions
	{
		private readonly Dictionary<CharacterAge, HashSet<CharacterAge>> m_matchingRules = new Dictionary<CharacterAge, HashSet<CharacterAge>>();
		private static CharacterAgeMatchingOptions s_strict;
		private static CharacterAgeMatchingOptions s_looseExceptChild;

		public static CharacterAgeMatchingOptions Strict
		{
			get
			{
				if (s_strict == null)
				{
					s_strict = new CharacterAgeMatchingOptions();
					s_strict.AddRule(CharacterAge.Child, new[] { CharacterAge.Child, });
					s_strict.AddRule(CharacterAge.YoungAdult, new[] { CharacterAge.YoungAdult, });
					s_strict.AddRule(CharacterAge.Adult, new[] { CharacterAge.Adult, });
					s_strict.AddRule(CharacterAge.Elder, new[] { CharacterAge.Elder, });
				}
				return s_strict;
			}
		}

		public static CharacterAgeMatchingOptions LooseExceptChild
		{
			get
			{
				if (s_looseExceptChild == null)
				{
					s_looseExceptChild = new CharacterAgeMatchingOptions();
					s_looseExceptChild.AddRule(CharacterAge.Child, new[] { CharacterAge.Child, });
					s_looseExceptChild.AddRule(CharacterAge.YoungAdult, new[] { CharacterAge.YoungAdult, CharacterAge.Adult, CharacterAge.Elder, });
					s_looseExceptChild.AddRule(CharacterAge.Adult, new[] { CharacterAge.YoungAdult, CharacterAge.Adult, CharacterAge.Elder, });
					s_looseExceptChild.AddRule(CharacterAge.Elder, new[] { CharacterAge.YoungAdult, CharacterAge.Adult, CharacterAge.Elder, });
				}
				return s_looseExceptChild;
			}
		}

		public void AddRule(CharacterAge age, IEnumerable<CharacterAge> matchingAges)
		{
			if (matchingAges == null)
				m_matchingRules.Add(age, null);
			else
				m_matchingRules.Add(age, new HashSet<CharacterAge>(matchingAges));
		}

		public bool Matches(CharacterAge g1, CharacterAge g2)
		{
			var hashSet = m_matchingRules[g1];
			if (hashSet == null)
				return true;
			return hashSet.Contains(g2);
		}
	}
}
