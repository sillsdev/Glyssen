using System;

namespace Glyssen.Shared.Bundle
{
	[Flags]
	public enum BlocksToDisplay
	{
		Unexpected = 1,
		Ambiguous = 2,
		MissingExpectedQuote = 4,
		MoreQuotesThanExpectedSpeakers = 8,
		KnownTroubleSpots = 16,
		AllScripture = 32, // If this bit is set, ignore everything else (except Exclude user-confirmed)- show all editable (i.e., Scripture) blocks
		AllExpectedQuotes = 64,
		ExcludeUserConfirmed = 128,
		AllQuotes = 256,
		NotAlignedToReferenceText = 512,
		NotAssignedAutomatically = Unexpected | Ambiguous,
		/// <summary>
		/// This name is ambiguous, but we'll keep it around for backwards compatibility.
		/// </summary>
		NeedAssignments = NotAssignedAutomatically,
		NotYetAssigned = NotAssignedAutomatically | ExcludeUserConfirmed,
		HotSpots = MissingExpectedQuote | MoreQuotesThanExpectedSpeakers | KnownTroubleSpots,
	}
}
