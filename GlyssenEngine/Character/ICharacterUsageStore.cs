using System.Collections.Generic;
using Glyssen.Shared;
using SIL.Scripture;

namespace GlyssenEngine.Character
{
	public interface ICharacterUsageStore
	{
		string GetStandardCharacterName(string character, int bookNum, int chapter,
			IReadOnlyCollection<IVerse> verses, out string singleKnownDelivery, out string defaultCharacter);

		ScrVers Versification { get; }
	}
}
