using System;
using ProtoScript.Character;
using ProtoScript.Utilities;

namespace ProtoScript.Analysis
{
	public class ProjectAnalysis
	{
		private readonly Project m_projectToAnalyze;

		public ProjectAnalysis(Project projectToAnalyze)
		{
			m_projectToAnalyze = projectToAnalyze;
		}

		public int TotalBlocks { get; private set; }
		public int NarratorBlocks { get; private set; }
		public int UnknownBlocks { get; private set; }
		public int AmbiguousBlocks { get; private set; }
		public double PercentAssigned { get; private set; }

		public void AnalyzeQuoteParse()
		{
			TotalBlocks = 0;
			NarratorBlocks = 0;
			UnknownBlocks = 0;
			AmbiguousBlocks = 0;
			foreach (BookScript book in m_projectToAnalyze.IncludedBooks)
			{
				foreach (Block block in book.GetScriptBlocks(false))
				{
					TotalBlocks++;
					if (block.CharacterIs(book.BookId, CharacterVerseData.StandardCharacter.Narrator))
						NarratorBlocks++;
					else if (block.CharacterId == CharacterVerseData.UnknownCharacter)
						UnknownBlocks++;
					else if (block.CharacterId == CharacterVerseData.AmbiguousCharacter)
						AmbiguousBlocks++;
				}
			}
			PercentAssigned = MathUtilities.PercentAsDouble(TotalBlocks - (UnknownBlocks + AmbiguousBlocks), TotalBlocks);
#if DEBUG
			ReportInConsole();
#endif
		}

		private void ReportInConsole()
		{
			Console.WriteLine("*************************************************************");
			Console.WriteLine();
			Console.WriteLine(m_projectToAnalyze.LanguageIsoCode);
			Console.WriteLine("Blocks assigned automatically: {0:N2}%", PercentAssigned);
			double narrator = MathUtilities.PercentAsDouble(NarratorBlocks, TotalBlocks);
			Console.WriteLine("Narrator: {0:N2}%", narrator);
			double unknown = MathUtilities.PercentAsDouble(UnknownBlocks, TotalBlocks);
			Console.WriteLine("Unknown: {0:N2}%", unknown);
			Console.WriteLine();
			Console.WriteLine("*************************************************************");
		}
	}
}
