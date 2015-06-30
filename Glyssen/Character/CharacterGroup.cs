using System.Collections.Generic;

namespace Glyssen.Character
{
	public class CharacterGroup
	{
		public HashSet<string> CharacterIds { get; private set; }
		public int GroupNumber { get; private set; }

		public CharacterGroup(int groupNumber)
		{
			CharacterIds = new HashSet<string>();
			GroupNumber = groupNumber;
		}
	}
}
