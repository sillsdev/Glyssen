using System;
using System.Text.RegularExpressions;
using Glyssen.RefTextDevUtilities;
using Glyssen.Shared;
using GlyssenEngine;

namespace DevTools
{
	class Program
	{
		static void Main()
		{
			Console.WriteLine("Enter the number of the item you wish to run:");
			Console.WriteLine();
			Console.WriteLine("1) Update XLIFF files from character/alias info and Paratext key terms - FIRST get latest versions from crowdin!");
			Console.WriteLine("2) Remove verse bridges from control file (currently removes comments)");
			Console.WriteLine("3) Generate comparison file against FCBH template");
			Console.WriteLine("4) Update CharacterDetail file (make references correspond to CharacterVerse file)");
			Console.WriteLine("5) BiblicalTerms.Processor.Process()");
			Console.WriteLine("6) CharacterListProcessing.Process()");
			Console.WriteLine("7) Output ranges of consecutive verses with single character");
			Console.WriteLine("8) Diff new version of DG against previous");
			Console.WriteLine("9) Generate reference texts from Excel spreadsheet");
			Console.WriteLine("10) Link reference texts to English");
			Console.WriteLine("11) Generate character mapping FCBH<->Glyssen (output in Resources/temporary)");
			Console.WriteLine("12) Obfuscate proprietary reference texts to make testing resources (output in GlyssenTests/Resources/temporary)");
			Console.WriteLine("13) Generate reference text book title and chapter label summary");
			Console.WriteLine("14) Create new English reference text (see comments in the Mode enum in ReferenceTextUtility). May append OT or NT.");
			Console.WriteLine("15) Exit without doing anything.");

			int command = -1;
			string option = null;

			do
			{
				var selection = Console.ReadLine();
				var match = new Regex("\\d+").Match(selection);
				if (match.Success && Int32.TryParse(match.Value, out command))
				{
					option = selection.Substring(match.Length);
					if (command >= 8 && command <= 14)
						ReferenceTextUtility.OnMessageRaised += (msg, error) => { Console.WriteLine(msg); };
				}

				if (command >= 1 && command <= 15)
					break;
				Console.WriteLine("Invalid option");

			} while (true);

			string outputType = "errors";
			bool waitForUserToSeeOutput = false;

			try
			{
				switch (command)
				{
					case 1: TermTranslator.Processor.Process(); break;
					case 2: VerseBridgeHelper.RemoveAllVerseBridges(); break;
					case 3: FCBH.Processor.Process(); break;
					case 4: CharacterDetailProcessing.GenerateReferences(); break;
					case 5: BiblicalTerms.Processor.Process(); break;
					case 6: CharacterListProcessing.Process(); break;
					case 7: CharacterDetailProcessing.GetAllRangesOfThreeOrMoreConsecutiveVersesWithTheSameSingleCharacterNotMarkedAsImplicit(); break;
					case 8:
						DiffDirectorGuide();
						outputType = "differences";
						waitForUserToSeeOutput = true;
						break;
					case 9:
						ReferenceTextUtility.ProcessReferenceTextDataFromFile(ReferenceTextUtility.Mode.Generate);
						waitForUserToSeeOutput = true;
						break;
					case 10: ReferenceTextUtility.LinkToEnglish();
						waitForUserToSeeOutput = ReferenceTextUtility.ErrorsOccurred;
						break;
					case 11:
						ReferenceTextUtility.ProcessReferenceTextDataFromFile(ReferenceTextUtility.Mode.CreateCharacterMapping, ReferenceTextProxy.GetOrCreate(ReferenceTextType.English));
						waitForUserToSeeOutput = true;
						break;
					case 12: ReferenceTextUtility.ObfuscateProprietaryReferenceTextsToMakeTestingResources();
						break;
					case 13:
						ReferenceTextUtility.ProcessReferenceTextDataFromFile(ReferenceTextUtility.Mode.CreateBookTitleAndChapterLabelSummary);
						waitForUserToSeeOutput = ReferenceTextUtility.ErrorsOccurred;
						break;
					case 14:
						ReferenceTextUtility.Testament testament;
						switch (option)
						{
							case "NT": testament = ReferenceTextUtility.Testament.NT; break;
							case "OT": testament = ReferenceTextUtility.Testament.OT; break;
							default: testament = ReferenceTextUtility.Testament.WholeBible; break;
						}
						ReferenceTextUtility.ProcessReferenceTextDataFromFile(ReferenceTextUtility.Mode.GenerateEnglish, null, testament);
						outputType = "output";
						waitForUserToSeeOutput = true;
						break;
					default:
						return;
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				waitForUserToSeeOutput = true;
			}

			if (waitForUserToSeeOutput)
			{
				Console.WriteLine($"Review {outputType} above, then press Enter to close.");
				Console.ReadLine();
			}
		}

		private static void DiffDirectorGuide()
		{
			Console.WriteLine("What language do you want to diff?");
			Console.WriteLine("");
			Console.WriteLine("1) All");
			Console.WriteLine("2) English");
			Console.WriteLine("3) Russian");
			Console.WriteLine("** For a custom (proprietary) language,  type its name.");
			Console.WriteLine();

			string selection = Console.ReadLine();
			ReferenceTextProxy id = null;
			switch (selection)
			{
				case "1": break; // All
				case "English":
				case "english":
				case "2": id = ReferenceTextProxy.GetOrCreate(ReferenceTextType.English); break;
				case "Russian":
				case "russian":
				case "3": id = ReferenceTextProxy.GetOrCreate(ReferenceTextType.Russian); break;
				default:
					id = ReferenceTextProxy.GetOrCreate(ReferenceTextType.Custom, selection);
					if (id.Missing)
					{
						Console.WriteLine("Requested custom reference text not found!");
						return;
					}
					break;
			}

			Console.WriteLine("Enter desired sensitivity?");
			Console.WriteLine("");
			Console.WriteLine("1) Report all differences");
			Console.WriteLine("2) Ignore only whitespace differences");
			Console.WriteLine("3) Ignore curly vs. straight quote and whitespace differences (default)");
			Console.WriteLine("4) Ignore quotation mark and whitespace differences");
			Console.WriteLine("5) Report only alpha-numeric text differences (ignore differences in whitespace, punctuation, and symbols)");
			Console.WriteLine("*** NOTE *** For more specific control or to see more details, set a breakpoint in CompareIgnoringQuoteMarkDifferences.");

			Console.WriteLine();

			string sensitivity = Console.ReadLine();
			switch (sensitivity)
			{
				case "1":
					ReferenceTextUtility.DifferencesToIgnore = ReferenceTextUtility.Ignore.Nothing;
					break;
				case "2":
					ReferenceTextUtility.DifferencesToIgnore = ReferenceTextUtility.Ignore.WhitespaceDifferences;
					break;
				case "4":
					ReferenceTextUtility.DifferencesToIgnore = ReferenceTextUtility.Ignore.QuotationMarkDifferences |
						ReferenceTextUtility.Ignore.WhitespaceDifferences;
					break;
				case "5":
					ReferenceTextUtility.DifferencesToIgnore = ReferenceTextUtility.Ignore.AllDifferencesExceptAlphaNumericText;
					break;
			}
			ReferenceTextUtility.ProcessReferenceTextDataFromFile(ReferenceTextUtility.Mode.FindDifferencesBetweenCurrentVersionAndNewText, id);
		}
	}
}
