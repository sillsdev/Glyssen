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
			Console.WriteLine("8) Diff new version of DG against previous -- need to set breakpoint in CompareIgnoringQuoteMarkDifferences method");
			Console.WriteLine("9) Generate standard reference texts from Excel spreadsheet");
			Console.WriteLine("10) Link standard reference texts to English");
			Console.WriteLine("11) Generate character mapping FCBH<->Glyssen (output in Resources/temporary)");
			Console.WriteLine();

			string selection = Console.ReadLine();
			bool errorOccurred = false;

			switch (selection)
			{
				case "1": TermTranslator.Processor.Process(); break;
				case "2": VerseBridgeHelper.RemoveAllVerseBridges(); break;
				case "3": FCBH.Processor.Process(); break;
				case "4": CharacterDetailProcessing.GenerateReferences(); break;
				case "5": BiblicalTerms.Processor.Process(); break;
				case "6": CharacterListProcessing.Process(); break;
				case "7": CharacterDetailProcessing.GetAllRangesOfThreeOrMoreConsecutiveVersesWithTheSameSingleCharacterNotMarkedAsImplicit(); break;
				case "8": DiffDirectorGuide(); break;
				case "9": errorOccurred = !ReferenceTextUtility.GenerateReferenceTexts(false, false); break;
				case "10": errorOccurred = !ReferenceTextUtility.LinkToEnglish(); break;
				case "11": ReferenceTextUtility.GenerateReferenceTexts(false, true); break;
			}

			if (errorOccurred)
			{
				Console.WriteLine("Review errors above, then press any key to close.");
				Console.ReadLine();
			}
		}

		private static void DiffDirectorGuide()
		{
			Console.WriteLine("Enter the number corresponding to the language you want to diff:");
			Console.WriteLine("");
			Console.WriteLine("1) All");
			Console.WriteLine("2) English");
			//Console.WriteLine("3) Azeri");
			//Console.WriteLine("4) French");
			//Console.WriteLine("5) Indonesian");
			//Console.WriteLine("6) Portuguese");
			Console.WriteLine("7) Russian");
			//Console.WriteLine("8) Spanish");
			//Console.WriteLine("9) Tok Pisin");
			Console.WriteLine();

			string selection = Console.ReadLine();
			ReferenceTextType type = ReferenceTextType.Unknown;
			switch (selection)
			{
				case "2": type = ReferenceTextType.English; break;
				//case "3": type = ReferenceTextType.Azeri; break;
				//case "4": type = ReferenceTextType.French; break;
				//case "5": type = ReferenceTextType.Indonesian; break;
				//case "6": type = ReferenceTextType.Portuguese; break;
				case "7": type = ReferenceTextType.Russian; break;
				//case "8": type = ReferenceTextType.Spanish; break;
				//case "9": type = ReferenceTextType.TokPisin; break;
			}
			ReferenceTextUtility.GenerateReferenceTexts(true, false, type);
		}
	}
}
