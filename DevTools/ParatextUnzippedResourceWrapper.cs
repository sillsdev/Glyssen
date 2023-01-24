using System.Collections.Generic;
using System.Linq;
using Glyssen.Shared;
using GlyssenEngine;
using GlyssenEngine.Paratext;
using GlyssenEngine.Utilities;
using Paratext.Data;
using SIL.Scripture;
using SIL.WritingSystems;

namespace DevTools
{
	/// <summary>
	/// Allows "hard-coded" overrides of some properties that are not exposed by the
	/// Paratext or Glyssen UI that affect the way quote parsing is done.
	/// </summary>
	internal class ParatextUnzippedResourceWrapper : ParatextScrTextWrapper
	{
		private static List<string> s_allCanonicalBookIds;

		static ParatextUnzippedResourceWrapper()
		{
			ParatextData.Initialize();
			ScrTextCollection.Initialize();

			GlyssenVersificationTable.Initialize();
			// The following pre-loads the reference text and prevents thread-contention.
			ReferenceTextProxy.GetOrCreate(ReferenceTextType.English);

			s_allCanonicalBookIds = new List<string>(66);
			for (int bookNum = 1; bookNum <= BCVRef.LastBook; bookNum++)
				s_allCanonicalBookIds.Add(BCVRef.NumberToBookCode(bookNum));
		}

		public ParatextUnzippedResourceWrapper(string resourceName) : base(ScrTextCollection.Get(resourceName))
		{
			IncludeBooks(s_allCanonicalBookIds);
		}

		// Note: In my first attempt to parse NVI-S, I did quite a bit of manual cleanup
		// on the quotation marks because of some "non-standard" ways of marking things up that
		// Paratext and Glyssen do not support well. This almost certainly means that these
		// programs have room for improvement in their quotation settings and parsing/checking.
		// I have introduced this class to handle these things by a) tweaking the existing closer
		// for the alternate (dialogue) first-level quotes and b) adding a new property that is
		// not currently exposed in the UI so that the characters used to set off reporting clauses
		// can be specified. Mike Lothers (Paratext UX) is looking into creating an issue to add
		// new settings to Paratext in support of this.
		private bool IsNVISpanish => UnderlyingScrText.Name == "NVI-S";

		public override IEnumerable<QuotationMark> QuotationMarks
		{
			get
			{
				if (!IsNVISpanish)
					return base.QuotationMarks;

				var marks = base.QuotationMarks.ToList();
				var iNormal = marks.FindIndex(m => m.Type == QuotationMarkingSystemType.Normal);
				var iDialogue = marks.FindIndex(m => m.Type == QuotationMarkingSystemType.Narrative);
				marks[iDialogue] = new QuotationMark(marks[iDialogue].Open, marks[iNormal].Close,
					marks[iDialogue].Continue, marks[iDialogue].Level,
					QuotationMarkingSystemType.Narrative);
				return marks;
			}
		}

		public override string ReportingClauseStartDelimiter =>
			IsNVISpanish ? "—" : base.ReportingClauseStartDelimiter;
	}
}
