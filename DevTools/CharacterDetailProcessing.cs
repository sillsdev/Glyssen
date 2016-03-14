using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Glyssen;
using Glyssen.Bundle;
using Glyssen.Character;
using SIL.IO;
using SIL.Reporting;
using SIL.Scripture;
using SIL.WritingSystems;

namespace DevTools
{
	class CharacterDetailProcessing
	{
		private const char Tab = '\t';
		private static readonly Regex FindTabsRegex = new Regex("\t", RegexOptions.Compiled);

		public static void GenerateReferences()
		{
			var data = ReadFile();
//			data.Sort((x, y) => String.Compare(x.CharacterId, y.CharacterId, StringComparison.Ordinal));
			PopulateReferences(data);
			WriteFile(Stringify(data));
		}

		private static void PopulateReferences(List<CharacterDetailLine> lines)
		{
			Dictionary<string, List<BCVRef>> dictionary =
				ControlCharacterVerseData.Singleton.GetAllQuoteInfo()
					.GroupBy(c => c.Character)
					.ToDictionary(c => c.Key, cv => cv.Select(c => c.BcvRef).ToList());
			foreach (var line in lines)
			{
				List<BCVRef> bcvRefs;
				if (dictionary.TryGetValue(line.CharacterId, out bcvRefs))
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

		private static string Stringify(List<CharacterDetailLine> lines)
		{
			StringBuilder sb = new StringBuilder();
			foreach (var line in lines)
				sb.Append(GetNewLine(line)).Append(Environment.NewLine);
			return sb.ToString();
		}

		private static string GetNewLine(CharacterDetailLine line)
		{
			if (FindTabsRegex.Matches(line.CurrentLine).Count == 5)
				return line.CurrentLine + Tab + line.ReferenceComment;

			int finalTabIndex = line.CurrentLine.LastIndexOf("\t", StringComparison.Ordinal);
			return line.CurrentLine.Substring(0, finalTabIndex + 1) + line.ReferenceComment;
		}

		private static void WriteFile(string fileText)
		{
			fileText = "#Character ID\tMax Speakers\tGender\tAge\tStatus\tComment\tReference Comment" + Environment.NewLine +
						fileText;
			File.WriteAllText("..\\..\\..\\Glyssen\\Resources\\CharacterDetail.txt", fileText);
		}

		private static List<CharacterDetailLine> ReadFile()
		{
			return
				File.ReadAllLines("..\\..\\..\\Glyssen\\Resources\\CharacterDetail.txt")
					.Select(ReadLine)
					.Where(l => l != null)
					.ToList();
		}

		private static CharacterDetailLine ReadLine(string line)
		{
			if (line.StartsWith("#"))
				return null;

			string characterId = line.Substring(0, line.IndexOf(Tab));
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

		public static void GetAllControlFileEntriesThatCouldBeMarkedAsImplicit()
		{
			Sldr.Initialize();
			try
			{
				var projectsToUse = new Dictionary<string, Tuple<DateTime, Project>>();
				foreach (var projFile in Project.AllRecordingProjectFolders.SelectMany(d => Directory.GetFiles(d, "*" + Project.kProjectFileExtension)))
				{
					var lastWriteTime = File.GetLastWriteTime(projFile);
					try
					{
						Project proj = Project.Load(projFile);
						if (proj.ProjectState == ProjectState.FullyInitialized)
						{
							var drammatizedBookCount = proj.IncludedBooks.Count(b => !b.SingleVoice);
							if (drammatizedBookCount > 15)
							{
								Tuple<DateTime, Project> existingProjectInfo;
								if (projectsToUse.TryGetValue(proj.Id, out existingProjectInfo))
								{
									var existingProject = existingProjectInfo.Item2;
									var existingProjDrammatizedBookCount = existingProject.IncludedBooks.Count(b => !b.SingleVoice);
									if (drammatizedBookCount > existingProjDrammatizedBookCount ||
										(drammatizedBookCount == existingProjDrammatizedBookCount && lastWriteTime > existingProjectInfo.Item1))
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
						var allKnownSpeakers = ControlCharacterVerseData.Singleton.GetAllQuoteInfo(bookCode).ToList();
						var prevCharacterVerse = allKnownSpeakers.FirstOrDefault();
						if (prevCharacterVerse == null)
							continue;
						var booksInAllProjects = projectsToUse.Values.Select(t => t.Item2).SelectMany(p => p.IncludedBooks).Where(book => book.BookId == bookCode && !book.SingleVoice).ToList();
						if (!booksInAllProjects.Any())
						{
							Console.WriteLine("No drammatized books included in any projects for " + bookCode);
							continue;
						}
						Console.WriteLine("Analyzing " + booksInAllProjects.Count + " drammatized books for " + bookCode);

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

		private static void WriteImplicitCvLineIfNoNarratorBlocks(StreamWriter writer, Glyssen.Character.CharacterVerse cv, IEnumerable<BookScript> books)
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
			string path = BuildProjectPath(Project.GetLanguageFolderPath(""), ref languageToUse, "Enter langauge of project to use:");
			if (path == null)
				return null;
			path = BuildProjectPath(path, ref bundleIdToUse, "Enter bundle ID of project to use:");
			if (path == null)
				return null;
			path = BuildProjectPath(path, ref recordingProjectToUse, "Enter name of recording project to use:");
			if (path == null)
				return null;
			path = Project.GetProjectFilePath(languageToUse, bundleIdToUse, recordingProjectToUse);
			return File.Exists(path) ? Project.Load(path) : null;
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
