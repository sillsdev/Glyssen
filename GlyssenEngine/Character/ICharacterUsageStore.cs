using System.Collections.Generic;
using Glyssen.Shared;
using SIL.Scripture;

namespace GlyssenEngine.Character
{
	/// <summary>
	/// A repository of information about "official" characters and their
	/// corresponding deliveries in the Scripture passages where they are known to speak.
	/// </summary>
	/// <remarks>There is a standard implementation of this interface for Glyssen, but this
	/// interface is defined to allow for alternative implementations.</remarks>
	public interface ICharacterUsageStore
	{
		/// <summary>
		/// Given a "character" which might not be an "official" character ID, returns a standard
		/// character ID that is known to speak in the specified location in Scripture, assuming
		/// it is possibly to reliably infer a single character based on the given one.
		/// </summary>
		/// <param name="character">A string representing a character name, description, etc.
		/// </param>
		/// <param name="bookNum">1-based Scripture book number</param>
		/// <param name="chapter">Chapter number</param>
		/// <param name="verses">One or more verses in which the character speaks</param>
		/// <param name="singleKnownDelivery">If a reliable match is found and the character
		/// has a single known delivery in the given verse(s), then this delivery is returned in
		/// this parameter.</param>
		/// <param name="defaultCharacter">If a reliable match is found and the standard character
		/// ID represents multiple characters, then the standard default character is returned in
		/// this parameter.</param>
		string GetStandardCharacterName(string character, int bookNum, int chapter,
			IReadOnlyCollection<IVerse> verses, out string singleKnownDelivery, out string defaultCharacter);

		/// <summary>
		/// The versification to be used to interpret the chapter and verse numbers passed to
		/// methods of this interface.
		/// </summary>
		ScrVers Versification { get; }
	}
}
