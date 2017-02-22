using System;
using Glyssen;

namespace DevTools
{
	class Program
	{
		static void Main()
		{
			Console.WriteLine("Enter the number of the item you wish to run:");
			Console.WriteLine();
			Console.WriteLine("1) Use Paratext key terms to attempt localization of character IDs");
			Console.WriteLine("2) Remove verse bridges from control file (currently removes comments)");
			Console.WriteLine("3) Generate comparison file against FCBH template");
			Console.WriteLine("4) Add references to CharacterDetail file");
			Console.WriteLine("5) BiblicalTerms.Processor.Process()");
			Console.WriteLine("6) CharacterListProcessing.Process()");
			Console.WriteLine("7) Output ranges of consecutive verses with single character");
			Console.WriteLine("8) Diff new version of DG against previous -- set breakpoint in CompareIgnoringQuoteMarkDifferences if desired");
			Console.WriteLine("9) Diff new version of DG against previous -- ignore whitespace and punctuation differences");
			Console.WriteLine("10) Generate reference texts from Excel spreadsheet");
			Console.WriteLine("11) Link reference texts to English");
			Console.WriteLine("12) Generate character mapping FCBH<->Glyssen (output in Resources/temporary)");
			Console.WriteLine("13) Obfuscate proprietary reference texts to make testing resources (output in GlyssenTests/Resources/temporary)");
			Console.WriteLine();

			string selection = Console.ReadLine();
			bool waitForUserToSeeOutput = false;
			string outputType = "errors";

			switch (selection)
			{
				case "1": TermTranslator.Processor.Process(); break;
				case "2": VerseBridgeHelper.RemoveAllVerseBridges(); break;
				case "3": FCBH.Processor.Process(); break;
				case "4": CharacterDetailProcessing.GenerateReferences(); break;
				case "5": BiblicalTerms.Processor.Process(); break;
				case "6": CharacterListProcessing.Process(); break;
				case "7": CharacterDetailProcessing.GetAllRangesOfThreeOrMoreConsecutiveVersesWithTheSameSingleCharacterNotMarkedAsImplicit(); break;
				case "8":
				case "9":
					DiffDirectorGuide(selection == "9");
					outputType = "differences";
					waitForUserToSeeOutput = true;
					break;
				case "10": waitForUserToSeeOutput = !ReferenceTextUtility.GenerateReferenceTexts(false, false); break;
				case "11": waitForUserToSeeOutput = !ReferenceTextUtility.LinkToEnglish(); break;
				case "12": ReferenceTextUtility.GenerateReferenceTexts(false, true); break;
				case "13": ReferenceTextUtility.ObfuscateProprietaryReferenceTextsToMakeTestingResources(); break;
			}

			if (waitForUserToSeeOutput)
			{
				Console.WriteLine($"Review {outputType} above, then press any key to close.");
				Console.ReadLine();
			}
		}

		private static void DiffDirectorGuide(bool ignoreWhitespaceAndPunctuationDifferences = false)
		{
			Console.WriteLine("Enter the number or name of the language you want to diff:");
			Console.WriteLine("");
			Console.WriteLine("1) All");
			Console.WriteLine("2) English");
			Console.WriteLine("3) Russian");
			Console.WriteLine();

			string selection = Console.ReadLine();
			ReferenceTextIdentifier id = null;
			string customId = null;
			switch (selection)
			{
				case "1": break; // All
				case "English":
				case "english":
				case "2": id = ReferenceTextIdentifier.GetOrCreate(ReferenceTextType.English); break;
				case "Russian":
				case "russian":
				case "3": id = ReferenceTextIdentifier.GetOrCreate(ReferenceTextType.Russian); break;
				default:
					id = ReferenceTextIdentifier.GetOrCreate(ReferenceTextType.Custom, selection);
					if (id.Missing)
					{
						Console.WriteLine("Requested custom reference text not found!");
						return;
					}
					break;
			}
			ReferenceTextUtility.GenerateReferenceTexts(true, false, id, ignoreWhitespaceAndPunctuationDifferences);
		}
	}
}
