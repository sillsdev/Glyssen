using System;

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
			Console.WriteLine("8) Generate standard reference texts from Excel spreadsheet");
			Console.WriteLine("9) Link standard reference texts to English");
			Console.WriteLine();

			string line = Console.ReadLine();
			bool errorOccurred = false;

			switch (line)
			{
				case "1": TermTranslator.Processor.Process(); break;
				case "2": VerseBridgeHelper.RemoveAllVerseBridges(); break;
				case "3": FCBH.Processor.Process(); break;
				case "4": CharacterDetailProcessing.GenerateReferences(); break;
				case "5": BiblicalTerms.Processor.Process(); break;
				case "6": CharacterListProcessing.Process(); break;
				case "7": CharacterDetailProcessing.GetAllRangesOfThreeOrMoreConsecutiveVersesWithTheSameSingleCharacterNotMarkedAsImplicit(); break;
				case "8": errorOccurred = !ReferenceTextUtility.GenerateReferenceTexts(); break;
				case "9": errorOccurred = !ReferenceTextUtility.LinkToEnglish(); break;
			}

			if (errorOccurred)
			{
				Console.WriteLine("Review errors above, then press any key to close.");
				Console.ReadLine();
			}
		}
	}
}
