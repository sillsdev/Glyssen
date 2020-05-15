using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Glyssen.RefTextDevUtilities;
using Glyssen.Shared;
using Glyssen.Utilities;
using GlyssenEngine;
using GlyssenEngine.Character;
using GlyssenEngine.Script;
using GlyssenFileBasedPersistence;
using SIL.Scripture;
using SIL.WritingSystems;
using static System.String;

namespace DevTools
{
	class CharacterDetailProcessing
	{
		private const char kTab = '\t';
		private static readonly Regex s_regexContentBeforeAndAfterReference = new Regex(@"(?<precedingPart>([^\t]*\t){6})[^\t]*(?<followingPart>.*)", RegexOptions.Compiled);

		public static void GenerateReferences()
		{
			var data = ReadFile();
//			data.Sort((x, y) => String.Compare(x.CharacterId, y.CharacterId, StringComparison.Ordinal));
			PopulateReferences(data);
			WriteFile(Stringify(data));
		}

		private static void PopulateReferences(List<CharacterDetailLine> lines)
		{
			// ENHANCE: Get info from NarratorOverrides as well. The information there
			// would not currently allow us to indicate exactly how many occurrences
			// there are because it is done using ranges and we do not necessarily
			// expect that every verse in the range could/would be assigned to that
			// character. Perhaps we could tack on something like:
			// ; Also used as narrator override in PSA, JER, EZK.
			// But how useful would that information really be?

			Dictionary<string, List<BCVRef>> dictionaryOfCharactersToReferences =
				ControlCharacterVerseData.Singleton.GetAllQuoteInfo().SelectMany(cv => cv.Character.Split('/')
				.Select(c => new Tuple<string, GlyssenEngine.Character.CharacterVerse>(c, cv)))
				.GroupBy(t => t.Item1, t => t.Item2).ToDictionary(c => c.Key, cv => cv.Select(c => c.BcvRef).ToList());

			foreach (var line in lines)
			{
				if (dictionaryOfCharactersToReferences.TryGetValue(line.CharacterId, out var bcvRefs))
				{
					switch (bcvRefs.Count)
					{
						case 0:
							continue;
						case 1:
							line.ReferenceComment = bcvRefs.Min().ToString();
							break;
						case 2:
							line.ReferenceComment = bcvRefs.Min() + " & " + bcvRefs.Max();
							break;
						default:
							line.ReferenceComment = bcvRefs.Min() + " <-(" + (bcvRefs.Count - 2) + " more)-> " + bcvRefs.Max();
							break;
					}
				}
			}
		}

		private static string Stringify(List<CharacterDetailLine> lines)
		{
			return Join(Environment.NewLine, lines.Select(GetNewLine));
		}

		private static string GetNewLine(CharacterDetailLine line)
		{
			var match = s_regexContentBeforeAndAfterReference.Match(line.CurrentLine);
			if (!match.Success)
				throw new ArgumentException($"Invalid input: {line.CurrentLine}", nameof(line));

			var lineWithReferenceComment = match.Result("${precedingPart}") + line.ReferenceComment + match.Result("${followingPart}");
			return lineWithReferenceComment;
		}

		private static string RelativePathToCharacterDetailFile => $"..\\..\\GlyssenEngine\\Resources\\{ReferenceTextUtility.kCharacterDetailTxtFilename}";

		private static void WriteFile(string fileText)
		{
			// Note: Keeping the "Status" column around in case we want to bring it back, but it is currently always empty and
			// always ignored in Glyssen.
			fileText = "#Character ID\tMax Speakers\tGender\tAge\tStatus\tComment\tReference\tFCBH Character" + Environment.NewLine +
				fileText;
			File.WriteAllText(RelativePathToCharacterDetailFile, fileText);
		}

		private static List<CharacterDetailLine> ReadFile()
		{
			return File.ReadAllLines(RelativePathToCharacterDetailFile)
				.Select(ReadLine)
				.Where(l => l != null)
				.ToList();
		}

		private static CharacterDetailLine ReadLine(string line)
		{
			if (line.StartsWith("#"))
				return null;

			string characterId = line.Substring(0, line.IndexOf(kTab));
			return new CharacterDetailLine
			{
				CharacterId = characterId,
				CurrentLine = line
			};
		}

		private class CharacterDetailLine
		{
			public string CharacterId { get; set; }
			public string CurrentLine { get; set; }
			public string ReferenceComment { get; set; }
		}

		private class NonUiFontRepository : IFontRepository
		{
			public bool IsFontInstalled(string fontFamilyIdentifier)
			{
				return true;
			}

			public bool DoesTrueTypeFontFileContainFontFamily(string ttfFile, string fontFamilyIdentifier)
			{
				throw new NotImplementedException();
			}

			public void TryToInstall(string fontFamilyIdentifier, IReadOnlyCollection<string> ttfFile)
			{
				throw new NotImplementedException();
			}

			public void ReportMissingFontFamily(string fontFamilyIdentifier)
			{
				throw new NotImplementedException();
			}
		}

		public static void GetAllControlFileEntriesThatCouldBeMarkedAsImplicit()
		{
			Sldr.Initialize();
			ProjectBase.Reader = ReferenceTextProxy.Reader = new PersistenceImplementation();
			Project.FontRepository = new NonUiFontRepository();
			try
			{
				var projectsToUse = new Dictionary<string, Tuple<DateTime, Project>>();
				foreach (var projFile in ProjectRepository.AllRecordingProjectFolders.SelectMany(d => Directory.GetFiles(d, "*" + ProjectRepository.kProjectFileExtension)))
				{
					var lastWriteTime = File.GetLastWriteTime(projFile);
					try
					{
						Project proj = ProjectRepository.LoadProject(projFile);
						if (proj.ProjectState == ProjectState.FullyInitialized)
						{
							var dramatizedBookCount = proj.IncludedBooks.Count(b => !b.SingleVoice);
							if (dramatizedBookCount > 15)
							{
								Tuple<DateTime, Project> existingProjectInfo;
								if (projectsToUse.TryGetValue(proj.Id, out existingProjectInfo))
								{
									var existingProject = existingProjectInfo.Item2;
									var existingProjDramatizedBookCount = existingProject.IncludedBooks.Count(b => !b.SingleVoice);
									if (dramatizedBookCount > existingProjDramatizedBookCount ||
										(dramatizedBookCount == existingProjDramatizedBookCount && lastWriteTime > existingProjectInfo.Item1))
										projectsToUse[proj.Id] = new Tuple<DateTime, Project>(lastWriteTime, proj);
								}
								else
									projectsToUse.Add(proj.Id, new Tuple<DateTime, Project>(lastWriteTime, proj));
							}
						}
					}
					catch (Exception)
					{
						// Ignore
					}
				}

				//var project = GetProjectToUse("eng", "9879dbb7cfe39e4d", "World English Bible Audio");
				//if (project == null)
				//	return;

				if (!projectsToUse.Any())
				{
					Console.WriteLine("No suitable projects found on this computer.");
					return;
				}

				string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SoftDev", "Glyssen",
					"AllControlFileEntriesThatCouldBeMarkedAsImplicit.txt");

				using (StreamWriter writer = new StreamWriter(filePath))
				{
					//foreach (var book in project.IncludedBooks)
					for (int b = 1; b <= 66; b++)
					{
						var bookCode = BCVRef.NumberToBookCode(b);
						bool multipleCharactersInVerse = false;
						var allKnownSpeakers = ControlCharacterVerseData.Singleton.GetAllQuoteInfo(b).ToList();
						var prevCharacterVerse = allKnownSpeakers.FirstOrDefault();
						if (prevCharacterVerse == null)
							continue;
						var booksInAllProjects = projectsToUse.Values.Select(t => t.Item2).SelectMany(p => p.IncludedBooks).Where(book => book.BookNumber == b && !book.SingleVoice).ToList();
						if (!booksInAllProjects.Any())
						{
							Console.WriteLine("No dramatized books included in any projects for " + bookCode);
							continue;
						}
						Console.WriteLine("Analyzing " + booksInAllProjects.Count + " dramatized books for " + bookCode);

						foreach (var cv in allKnownSpeakers.Skip(1))
						{
							if (cv.Chapter == prevCharacterVerse.Chapter && cv.Verse == prevCharacterVerse.Verse)
							{
								multipleCharactersInVerse = true;
							}
							else
							{
								if (!multipleCharactersInVerse && !prevCharacterVerse.Character.StartsWith("narrator-"))
									WriteImplicitCvLineIfNoNarratorBlocks(writer, prevCharacterVerse, booksInAllProjects);
								multipleCharactersInVerse = false;
							}
							prevCharacterVerse = cv;
						}
						if (!multipleCharactersInVerse && !prevCharacterVerse.Character.StartsWith("narrator-"))
							WriteImplicitCvLineIfNoNarratorBlocks(writer, prevCharacterVerse, booksInAllProjects);
					}
				}
			}
			finally
			{
				Sldr.Cleanup();
			}
		}

		private static void WriteImplicitCvLineIfNoNarratorBlocks(StreamWriter writer, GlyssenEngine.Character.CharacterVerse cv, IEnumerable<BookScript> books)
		{
			if (cv.QuoteType == QuoteType.Normal || cv.QuoteType == QuoteType.Dialogue)
			{
				if (books.Any(book => book.GetBlocksForVerse(cv.Chapter, cv.Verse).Any(b => b.CharacterIs(book.BookId, CharacterVerseData.StandardCharacter.Narrator))))
					return;

				writer.Write(cv.BookCode);
				writer.Write("\t");
				writer.Write(cv.Chapter);
				writer.Write("\t");
				writer.Write(cv.Verse);
				writer.Write("\t");
				writer.Write(cv.Character);
				writer.Write("\t");
				writer.Write(cv.Delivery);
				writer.Write("\t");
				writer.Write(cv.Alias);
				writer.Write("\tImplicit\t");
				writer.Write(cv.DefaultCharacter);
				writer.Write("\t");
				writer.WriteLine(cv.ParallelPassageReferences);
			}
		}

		private static Project GetProjectToUse(string languageToUse = "", string bundleIdToUse = "", string recordingProjectToUse = "")
		{
			string path = BuildProjectPath(ProjectRepository.ProjectsBaseFolder, ref languageToUse, "Enter language of project to use:");
			if (path == null)
				return null;
			path = BuildProjectPath(path, ref bundleIdToUse, "Enter bundle ID of project to use:");
			if (path == null)
				return null;
			path = BuildProjectPath(path, ref recordingProjectToUse, "Enter name of recording project to use:");
			if (path == null)
				return null;
			path = ProjectRepository.GetProjectFilePath(languageToUse, bundleIdToUse, recordingProjectToUse);
			return File.Exists(path) ? ProjectRepository.LoadProject(path) : null;
		}

		private static string BuildProjectPath(string pathBase, ref string partToAdd, string prompt)
		{
			string path = Path.Combine(pathBase, partToAdd);
			while (!Directory.Exists(path) || partToAdd == string.Empty)
			{
				Console.WriteLine(prompt);
				Console.WriteLine();
				partToAdd = Console.ReadLine();
				if (partToAdd == null || partToAdd.Length < 3)
					return null;
				path = Path.Combine(pathBase, partToAdd);
			}
			return path;
		}

		//public static void GetAllRangesOfThreeOrMoreConsecutiveVersesWithTheSameSingleCharacterNotMarkedAsImplicit()
		//{
		//	string prevBook = "";
		//	int prevChapter = 0;
		//	int prevVerse = -1;
		//	int firstVerseOfRange = -1;
		//	string prevCharacter = "";
		//	string prevDelivery = "";
		//	string prevDefaultCharacter = "";
		//	string prevAlias = "";
		//	string prevParallelPassageReferences = "";

		//	string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SoftDev", "Glyssen",
		//		"RangesOfThreeOrMoreConsecutiveVersesWithTheSameSingleCharacter.txt");

		//	using (StreamWriter writer = new StreamWriter(filePath))
		//	{
		//		foreach (var quote in ControlCharacterVerseData.Singleton.GetAllQuoteInfo().Where(q => q.QuoteType == QuoteType.Normal || q.QuoteType == QuoteType.Dialogue))
		//		{
		//			bool sameBookAndChapter = prevBook == quote.BookCode && prevChapter == quote.Chapter;
		//			if (sameBookAndChapter && prevVerse + 1 == quote.Verse &&
		//				prevCharacter == quote.Character && prevDelivery == quote.Delivery && prevDefaultCharacter == quote.DefaultCharacter)
		//			{
		//				prevVerse = quote.Verse;
		//			}
		//			else
		//			{
		//				bool thisVerseHasMultipleSpeakersOrDeliveries = sameBookAndChapter && prevVerse == quote.Verse;
		//				if (thisVerseHasMultipleSpeakersOrDeliveries)
		//					prevVerse--;

		//				if (prevVerse - firstVerseOfRange >= 2)
		//				{
		//					for (int i = firstVerseOfRange; i <= prevVerse; i++)
		//					{
		//						writer.Write(prevBook);
		//						writer.Write("\t");
		//						writer.Write(prevChapter);
		//						writer.Write("\t");
		//						writer.Write(i);
		//						writer.Write("\t");
		//						writer.Write(prevCharacter);
		//						writer.Write("\t");
		//						writer.Write(prevDelivery);
		//						writer.Write("\t");
		//						writer.Write(prevAlias);
		//						writer.Write("\tImplicit\t");
		//						writer.Write(prevDefaultCharacter);
		//						writer.Write("\t");
		//						writer.WriteLine(prevParallelPassageReferences);
		//					}
		//				}
		//				if (thisVerseHasMultipleSpeakersOrDeliveries)
		//				{
		//					firstVerseOfRange = prevVerse = quote.Verse + 1;
		//					prevCharacter = "";
		//					prevDelivery = "";
		//					prevDefaultCharacter = "";
		//					prevAlias = "";
		//					prevParallelPassageReferences = "";
		//				}
		//				else if (!sameBookAndChapter || quote.Verse >= prevVerse)
		//				{
		//					if (!sameBookAndChapter)
		//					{
		//						prevBook = quote.BookCode;
		//						prevChapter = quote.Chapter;
		//					}
		//					firstVerseOfRange = prevVerse = quote.Verse;
		//					prevCharacter = quote.Character;
		//					prevDelivery = quote.Delivery;
		//					prevDefaultCharacter = quote.DefaultCharacter;
		//					prevAlias = quote.Alias;
		//					prevParallelPassageReferences = quote.ParallelPassageReferences;
		//				}
		//			}
		//		}
		//	}
		//}
	}
}
