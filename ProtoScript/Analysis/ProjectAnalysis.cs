using System;
using ProtoScript.Character;

namespace ProtoScript.Analysis
{
	class ProjectAnalysis
	{
		private readonly Project m_projectToAnalyze;

		public ProjectAnalysis(Project projectToAnalyze)
		{
			m_projectToAnalyze = projectToAnalyze;
		}

		public void AnalyzeQuoteParse()
		{
			int totalBlocks = 0;
			int narratorBlocks = 0;
			int unknownBlocks = 0;
			int ambiguousBlocks = 0;
			foreach (BookScript book in m_projectToAnalyze.IncludedBooks)
			{
				foreach (Block block in book.GetScriptBlocks(false))
				{
					totalBlocks++;
					if (block.CharacterIs(book.BookId, CharacterVerseData.StandardCharacter.Narrator))
						narratorBlocks++;
					else if (block.CharacterId == CharacterVerseData.UnknownCharacter)
						unknownBlocks++;
					else if (block.CharacterId == CharacterVerseData.AmbiguousCharacter)
						ambiguousBlocks++;
				}
			}
			Console.WriteLine("*************************************************************");
			Console.WriteLine();
			Console.WriteLine(m_projectToAnalyze.LanguageIsoCode);
			double assignedAutomatically = (totalBlocks - (unknownBlocks + ambiguousBlocks)) / (double)totalBlocks;
			Console.WriteLine("Percentage of blocks assigned automatically: " + assignedAutomatically * 100);
			double narrator = narratorBlocks / (double)totalBlocks;
			Console.WriteLine("Percentage of blocks narrator: " + narrator * 100);
			double unknown = unknownBlocks / (double)totalBlocks;
			Console.WriteLine("Percentage of blocks unknown: " + unknown * 100);
			Console.WriteLine();
			Console.WriteLine("*************************************************************");
		}
	}
}
