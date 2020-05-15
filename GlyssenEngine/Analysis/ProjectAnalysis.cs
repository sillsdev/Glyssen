﻿using System;
using System.Diagnostics.CodeAnalysis;
using GlyssenEngine.Character;
using GlyssenEngine.Script;
using GlyssenEngine.Utilities;

namespace GlyssenEngine.Analysis
{
	public class ProjectAnalysis
	{
		private readonly Project m_projectToAnalyze;
		private double m_alignmentPercent = -1;

		public ProjectAnalysis(Project projectToAnalyze)
		{
			m_projectToAnalyze = projectToAnalyze;
		}

		public int TotalBlocks { get; private set; }
		public int NarratorBlocks { get; private set; }
		public int UnknownBlocks { get; private set; }
		public int AmbiguousBlocks { get; private set; }
		public int NeedsReviewBlocks { get; private set; }
		public double TotalPercentAssigned { get; private set; }
		public int UserAssignedBlocks { get; private set; }
		public int NeedsAssignment { get; private set; }
		public double UserPercentAssigned { get; private set; }
		public double AlignmentPercent
		{
			get
			{
				if (m_alignmentPercent < 0)
					CalculateAlignmentPercentage();
				return m_alignmentPercent;
			}
		}
		public double PercentUnknown { get; private set; }

		public void AnalyzeQuoteParse()
		{
			TotalBlocks = 0;
			NarratorBlocks = 0;
			UnknownBlocks = 0;
			AmbiguousBlocks = 0;
			NeedsReviewBlocks = 0;
			UserAssignedBlocks = 0;
			NeedsAssignment = 0;
			foreach (BookScript book in m_projectToAnalyze.IncludedBooks)
			{
				foreach (Block block in book.GetScriptBlocks())
				{
					if (block.IsContinuationOfPreviousBlockQuote)
						continue;
					TotalBlocks++;

					if (book.SingleVoice)
					{
						NarratorBlocks++;
						continue;
					}

					if (block.CharacterIs(book.BookId, CharacterVerseData.StandardCharacter.Narrator))
						NarratorBlocks++;
					else if (block.CharacterId == CharacterVerseData.kUnexpectedCharacter)
						UnknownBlocks++;
					else if (block.CharacterId == CharacterVerseData.kAmbiguousCharacter)
						AmbiguousBlocks++;
					else if (block.CharacterId == CharacterVerseData.kNeedsReview)
						NeedsReviewBlocks++;
					if (block.UserConfirmed)
						UserAssignedBlocks++;
					if (block.UserConfirmed || block.CharacterIsUnclear)
						NeedsAssignment++;
				}
			}
			m_alignmentPercent = -1;

			TotalPercentAssigned = MathUtilities.PercentAsDouble(TotalBlocks - (UnknownBlocks + AmbiguousBlocks), TotalBlocks);
			UserPercentAssigned = MathUtilities.PercentAsDouble(UserAssignedBlocks, NeedsAssignment);
			PercentUnknown = MathUtilities.PercentAsDouble(UnknownBlocks, TotalBlocks);
#if DEBUG
			ReportInConsole();
#endif
		}

		private void CalculateAlignmentPercentage()
		{
			int totalBlocksForExport = 0;
			int blocksNotAlignedToReferenceText = 0;
			var refText = m_projectToAnalyze.ReferenceText;
			if (refText == null)
			{
				m_alignmentPercent = 0;
				return;
			}
			// Note: Since we want this to be as efficient as possible and all we care about here is statistics,
			// we don't need to bother with applying narrator overrides.
			foreach (var book in refText.GetBooksWithBlocksConnectedToReferenceText(m_projectToAnalyze, false))
			{
				var blocks = book.GetScriptBlocks();
				if (!refText.CanDisplayReferenceTextForBook(book) || book.SingleVoice)
					totalBlocksForExport += blocks.Count;
				else
				{
					foreach (Block block in blocks)
					{
						totalBlocksForExport++;
						if (!CharacterVerseData.IsCharacterExtraBiblical(block.CharacterId) && !block.MatchesReferenceText)
							blocksNotAlignedToReferenceText++;
					}
				}
			}
			m_alignmentPercent = MathUtilities.PercentAsDouble(totalBlocksForExport - blocksNotAlignedToReferenceText, totalBlocksForExport);
		}

		[SuppressMessage("ReSharper", "LocalizableElement")]
		private void ReportInConsole()
		{
			Console.WriteLine("*************************************************************");
			Console.WriteLine("Language iso code: " + m_projectToAnalyze.LanguageIsoCode);
			Console.WriteLine("Blocks assigned automatically: " + MathUtilities.FormattedPercent(TotalPercentAssigned, 2, 5));
			double narrator = MathUtilities.PercentAsDouble(NarratorBlocks, TotalBlocks);
			Console.WriteLine("Narrator: " + MathUtilities.FormattedPercent(narrator, 2, 5));
			Console.WriteLine("Unknown: " + MathUtilities.FormattedPercent(PercentUnknown, 2, 5));
			Console.WriteLine("*************************************************************");
		}
	}
}
