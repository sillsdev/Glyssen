using System.Collections.Generic;
using SIL.ObjectModel;
using SIL.Scripture;

namespace Glyssen.Character
{
	public interface ICharacterDeliveryInfo
	{
		string Character { get; }
		string LocalizedCharacter { get; }
		string Delivery { get; }
		string DefaultCharacter { get; }
		string Alias { get; }
		string LocalizedAlias { get; }
		bool ProjectSpecific { get; }
	}

	public interface ICharacterVerseRepository
	{
		/// <summary>
		/// Gets all characters completely covered by the given range of verses. If there are multiple verses, only
		/// characters known to speak in ALL the verses will be included in the returned set.
		/// </summary>
		IEnumerable<CharacterSpeakingMode> GetCharacters(int bookId, int chapter, int initialStartVerse, int initialEndVerse = 0,
			int finalVerse = 0, ScrVers versification = null, bool includeAlternatesAndRareQuotes = false, bool includeNarratorOverrides = false);

		/// <summary>
		/// Gets a single character/delivery object that represents the one known character expected to be the
		/// exclusive (implicit) speaker over the entire reference range represented by the given parameters.
		/// If there are conflicting implicit characters or an implicit character covers only part of the range,
		/// the returned object will be a "Needs Review" character.
		/// </summary>
		ICharacterDeliveryInfo GetImplicitCharacter(int bookId, int chapter, int startVerse, int endVerse = 0, ScrVers versification = null);
	}

	public interface ICharacterVerseInfo : ICharacterVerseRepository
	{
		IEnumerable<CharacterVerse> GetAllQuoteInfo();

		IEnumerable<CharacterVerse> GetAllQuoteInfo(int bookNum);

		IReadOnlySet<ICharacterDeliveryInfo> GetUniqueCharacterAndDeliveries();

		ISet<ICharacterDeliveryInfo> GetUniqueCharacterAndDeliveries(string bookCode);

		ISet<string> GetUniqueDeliveries();
	}
}
