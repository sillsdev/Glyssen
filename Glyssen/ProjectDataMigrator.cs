﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Glyssen.Character;
using SIL.Extensions;
using SIL.Reporting;
using SIL.Scripture;

namespace Glyssen
{
	internal class ProjectDataMigrator
	{
		private static Project s_lastProjectMigrated;
		private static Migrations s_migrationsRun;

		[Flags]
		private enum Migrations
		{
			SetBookIdForChapterBlocks = 1,
		}

		public static void MigrateProjectData(Project project, int fromControlFileVersion)
		{
			Logger.WriteEvent("Migrating project " + project.ProjectFilePath);

			if (s_lastProjectMigrated != project)
				s_migrationsRun = 0;

			if (fromControlFileVersion < 90 && (project != s_lastProjectMigrated || (s_migrationsRun & Migrations.SetBookIdForChapterBlocks) == 0))
			{
				SetBookIdForChapterBlocks(project.Books);
				s_migrationsRun |= Migrations.SetBookIdForChapterBlocks;
			}

			// We don't need to track runs of Migrations which only occur for FullyInitialized projects
			// because we shouldn't call MigrateProjectData again.
			if (project.ProjectState == ProjectState.FullyInitialized)
			{
				if (fromControlFileVersion < 96)
				{
					MigrateInvalidMultiBlockQuoteData(project.Books);
					CleanUpOrphanedMultiBlockQuoteStati(project.Books);
				}
				if (fromControlFileVersion < 102)
					MigrateInvalidCharacterIdForScriptData(project.Books);
				if (fromControlFileVersion == 104)
					MigrateInvalidCharacterIdsWithoutCharacterIdInScriptOverrides(project);

				MigrateDeprecatedCharacterIds(project);
			}

			s_lastProjectMigrated = project;
		}

		// internal for testing
		internal static void MigrateInvalidMultiBlockQuoteData(IReadOnlyList<BookScript> books)
		{
			foreach (var book in books)
			{
				ISet<int> processedSplitIds = new HashSet<int>();
				var blocks = book.GetScriptBlocks();
				foreach (var firstBlockOfSplit in blocks.Where(b => b.SplitId != -1))
				{
					int splitId = firstBlockOfSplit.SplitId;
					if (processedSplitIds.Contains(splitId))
						continue;
					if (firstBlockOfSplit.MultiBlockQuote == MultiBlockQuote.None)
					{
						processedSplitIds.Add(splitId);
						continue;
					}

					List<Block> subsequentBlocksInSplit = new List<Block>(5);
					Block firstBlockAfterSplit = null;
					int iBlock = blocks.IndexOf(firstBlockOfSplit);
					while (++iBlock < blocks.Count)
					{
						if (blocks[iBlock].SplitId == splitId)
							subsequentBlocksInSplit.Add(blocks[iBlock]);
						else
						{
							firstBlockAfterSplit = blocks[iBlock];
							break;
						}
					}

					if (!subsequentBlocksInSplit.Any())
					{
						// This should never happen
						Debug.Fail("Splits should always have more than one block and those blocks should be sequential.");
						processedSplitIds.Add(splitId);
						continue;
					}

					if (subsequentBlocksInSplit.All(b => b.MultiBlockQuote == MultiBlockQuote.None) &&
						(firstBlockAfterSplit == null ||
						firstBlockAfterSplit.MultiBlockQuote == MultiBlockQuote.Continuation ||
						firstBlockAfterSplit.MultiBlockQuote == MultiBlockQuote.ChangeOfDelivery))
					{
						if (firstBlockOfSplit.MultiBlockQuote == MultiBlockQuote.Start)
							firstBlockOfSplit.MultiBlockQuote = MultiBlockQuote.None;
						subsequentBlocksInSplit.Last().MultiBlockQuote = MultiBlockQuote.Start;
					}

					processedSplitIds.Add(splitId);
				}
			}
		}

		//internal for testing
		internal static void CleanUpOrphanedMultiBlockQuoteStati(IReadOnlyList<BookScript> books)
		{
			foreach (var book in books)
			{
				var blocks = book.GetScriptBlocks();
				if (!blocks.Any())
					continue;

				Block previousBlock = blocks[0];
				if (blocks[0].MultiBlockQuote != MultiBlockQuote.None && blocks[0].MultiBlockQuote != MultiBlockQuote.Start)
					previousBlock.MultiBlockQuote = MultiBlockQuote.None;
				MultiBlockQuote previousBlockMultiBlockStatus = previousBlock.MultiBlockQuote;
				foreach (var block in blocks.Skip(1))
				{
					if (block.MultiBlockQuote == MultiBlockQuote.None)
					{
						if (previousBlockMultiBlockStatus == MultiBlockQuote.Start)
							previousBlock.MultiBlockQuote = MultiBlockQuote.None;
					}
					else if (block.MultiBlockQuote == MultiBlockQuote.Start)
					{
						if (previousBlockMultiBlockStatus == MultiBlockQuote.Start)
							previousBlock.MultiBlockQuote = MultiBlockQuote.None;
					}
					else
					{
						if (previousBlockMultiBlockStatus == MultiBlockQuote.None)
							block.MultiBlockQuote = MultiBlockQuote.None;
					}

					previousBlock = block;
					previousBlockMultiBlockStatus = block.MultiBlockQuote;
				}
			}
		}

		public static void MigrateInvalidCharacterIdForScriptData(IReadOnlyList<BookScript> books)
		{
			foreach (var block in books.SelectMany(book => book.GetScriptBlocks().Where(block =>
				(block.CharacterId == CharacterVerseData.kAmbiguousCharacter || block.CharacterId == CharacterVerseData.kUnknownCharacter) &&
				block.CharacterIdOverrideForScript != null)))
			{
				block.CharacterIdInScript = null;
				block.UserConfirmed = false;
			}
		}

		public static int MigrateInvalidCharacterIdsWithoutCharacterIdInScriptOverrides(Project project)
		{
			int numberOfChangesMade = 0; // For testing

			foreach (BookScript book in project.Books)
			{
				int bookNum = BCVRef.BookToNumber(book.BookId);

				foreach (Block block in book.GetScriptBlocks().Where(block => block.CharacterId != null &&
					block.CharacterId.Contains("/") &&
					block.CharacterIdOverrideForScript == null))
				{
					block.UseDefaultForMultipleChoiceCharacter(bookNum, project.Versification);
					numberOfChangesMade++;
				}
			}
			return numberOfChangesMade;
		}

		public static int MigrateDeprecatedCharacterIds(Project project)
		{
			var cvInfo = new CombinedCharacterVerseData(project);
			var characterDetailDictionary = project.AllCharacterDetailDictionary;
			int numberOfChangesMade = 0; // For testing

			foreach (BookScript book in project.Books)
			{
				int bookNum = BCVRef.BookToNumber(book.BookId);

				foreach (Block block in book.GetScriptBlocks().Where(block => block.CharacterId != null &&
					block.CharacterId != CharacterVerseData.kUnknownCharacter &&
					!CharacterVerseData.IsCharacterStandard(block.CharacterId)))
				{
					if (block.CharacterId == CharacterVerseData.kAmbiguousCharacter)
					{
						if (block.UserConfirmed)
						{
							block.UserConfirmed = false;
							numberOfChangesMade++;
						}
					}
					else
					{
						var unknownCharacter = !characterDetailDictionary.ContainsKey(block.CharacterIdInScript);
						if (unknownCharacter && project.ProjectCharacterVerseData.GetCharacters(bookNum, block.ChapterNumber, block.InitialStartVerseNumber,
								block.InitialEndVerseNumber).
								Any(c => c.Character == block.CharacterId && c.Delivery == (block.Delivery ?? "")))
						{
							// PG-471: This is a known character who spoke in an unexpected location and was therefore added to the project CV file,
							// but was subsequently removed or renamed from the master character detail list.
							project.ProjectCharacterVerseData.Remove(bookNum, block.ChapterNumber, block.InitialStartVerseNumber,
								block.InitialEndVerseNumber, block.CharacterId, block.Delivery ?? "");
							block.CharacterId = CharacterVerseData.kUnknownCharacter;
							block.CharacterIdInScript = null;
							numberOfChangesMade++;
						}
						else
						{
							var characters = cvInfo.GetCharacters(bookNum, block.ChapterNumber, block.InitialStartVerseNumber, block.InitialEndVerseNumber).ToList();
							if (unknownCharacter || !characters.Any(c => c.Character == block.CharacterId && c.Delivery == (block.Delivery ?? "")))
							{
								if (characters.Count(c => c.Character == block.CharacterId) == 1)
									block.Delivery = characters.First(c => c.Character == block.CharacterId).Delivery;
								else
									block.SetCharacterAndDelivery(characters);
								numberOfChangesMade++;
							}
						}
					}
				}
			}
			return numberOfChangesMade;
		}

		internal static void SetBookIdForChapterBlocks(IReadOnlyList<BookScript> books)
		{
			foreach (var book in books)
			{
				foreach (var block in book.GetScriptBlocks().Where(block => block.IsChapterAnnouncement && block.BookCode == null))
					block.BookCode = book.BookId;
			}
		}
	}
}
