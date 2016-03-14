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
			Console.WriteLine("7) Output verses with single non-narrator character");
			Console.WriteLine();

			string line = Console.ReadLine();

			switch (line)
			{
				case "1": TermTranslator.Processor.Process(); break;
				case "2": VerseBridgeHelper.RemoveAllVerseBridges(); break;
				case "3": FCBH.Processor.Process(); break;
				case "4": CharacterDetailProcessing.GenerateReferences(); break;
				case "5": BiblicalTerms.Processor.Process(); break;
				case "6": CharacterListProcessing.Process(); break;
				case "7": CharacterDetailProcessing.GetAllControlFileEntriesThatCouldBeMarkedAsImplicit(); break;
			}
		}
	}
}
