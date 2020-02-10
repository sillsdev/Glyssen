﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Glyssen.Shared;
using GlyssenEngine.Bundle;
using GlyssenEngine.Script;
using GlyssenEngine.Utilities;
using SIL;
using SIL.DblBundle;
using SIL.IO;
using SIL.Reporting;
using SIL.Scripture;
using SIL.Xml;

namespace GlyssenEngine
{
	public static class DataMigrator
	{
		private static bool s_alreadyCalled = false;
		private const string kOldProjectExtension = ".pgproj";
		public static Tuple<int, int> UpgradeToCurrentDataFormatVersion(Func<Project, bool> handleMissingBundleNeededForUpgrade,
			Action<string, string> handleProjectPathChanged, Func<IReadOnlyList<Tuple<string, string>>, bool> confirmSafeAudioAudioReplacements)
		{
			if (s_alreadyCalled)
				return null;
			var settings = ApplicationMetadata.Load(out var error);
			if (error != null)
				throw error;
			s_alreadyCalled = true;
			int from = settings.DataVersion;
			if (UpgradeToCurrentDataFormatVersion(settings, handleMissingBundleNeededForUpgrade, handleProjectPathChanged, confirmSafeAudioAudioReplacements))
			{
				settings.Save();
				return new Tuple<int, int>(from, settings.DataVersion);
			}
			return null;
		}

		private static bool UpgradeToCurrentDataFormatVersion(ApplicationMetadata info, Func<Project, bool> handleMissingBundleNeededForUpgrade,
			Action<string, string> handleProjectPathChanged, Func<IReadOnlyList<Tuple<string, string>>, bool> confirmSafeAudioAudioReplacements)
		{
			if (info.DataVersion >= ApplicationMetadata.kDataFormatVersion)
				return false;

			bool retVal = true;

			switch (info.DataVersion)
			{
				case 0:
					foreach (var publicationFolder in Project.AllPublicationFolders)
					{
						var filesToMove = Directory.GetFiles(publicationFolder);
						if (!filesToMove.Any())
							continue;

						var projectFilePath = Directory.GetFiles(publicationFolder, "*" + kOldProjectExtension).FirstOrDefault();
						if (projectFilePath != null)
						{
							Exception exception;
							var metadata = GlyssenDblTextMetadata.Load<GlyssenDblTextMetadata>(projectFilePath, out exception);
							string recordingProjectName;
							if (exception != null)
							{
								// Just add a directory layer and don't worry about it for now.
								recordingProjectName = Path.GetFileName(publicationFolder);
							}
							else
							{
								if (metadata.Identification != null && !string.IsNullOrEmpty(metadata.Identification.Name))
									recordingProjectName = metadata.Identification.Name;
								else
									recordingProjectName = metadata.Id;
							}
							recordingProjectName = Project.GetDefaultRecordingProjectName(recordingProjectName);
							var recordingProjectFolder = Path.Combine(publicationFolder, recordingProjectName);
							Directory.CreateDirectory(recordingProjectFolder);
							foreach (var file in filesToMove)
								File.Move(file, Path.Combine(recordingProjectFolder, Path.GetFileName(file)));
							handleProjectPathChanged(projectFilePath, Path.Combine(recordingProjectFolder, Path.GetFileName(projectFilePath)));
						}
					}
					goto case 1;
				case 1:
					foreach (var recordingProjectFolder in Project.AllRecordingProjectFolders.ToList())
					{
						var versificationPath = Path.Combine(recordingProjectFolder, DblBundleFileUtils.kVersificationFileName);
						if (!File.Exists(versificationPath))
						{
							var projectFilePath = Directory.GetFiles(recordingProjectFolder, "*" + kOldProjectExtension).FirstOrDefault();

							if (projectFilePath != null)
							{
								if (projectFilePath.Equals(SampleProject.SampleProjectFilePath, StringComparison.OrdinalIgnoreCase))
								{
									File.WriteAllText(versificationPath, Resources.EnglishVersification);
								}
								else
								{
									Exception exception;
									var metadata = GlyssenDblTextMetadata.Load<GlyssenDblTextMetadata>(projectFilePath, out exception);
									var origBundlePath = metadata.OriginalReleaseBundlePath;

									var bundle = new GlyssenBundle(origBundlePath);
									var errorlogPath = Path.Combine(recordingProjectFolder, "errorlog.txt");
									bundle.CopyVersificationFile(versificationPath);
									try
									{
										ProjectBase.LoadVersification(versificationPath);
									}
									catch (InvalidVersificationLineException ex)
									{
										var msg = string.Format(Localizer.GetString("DataMigration.InvalidVersificationFile",
												"Invalid versification file encountered during data migration. Errors must be fixed or subsequent " +
												"attempts to open this project will fail.\r\n" +
												"Project: {0}\r\n" +
												"Text release Bundle: {1}\r\n" +
												"Versification file: {2}\r\n" +
												"Error: {3}"),
											projectFilePath, origBundlePath, versificationPath, ex.Message);
										MessageModal.Show(msg, GlyssenInfo.kProduct, Buttons.OK, Icon.Warning);
										File.WriteAllText(errorlogPath, msg);
									}
								}
							}
						}
					}
					goto case 2;
				case 2:
					foreach (var pgProjFile in Project.AllRecordingProjectFolders.SelectMany(d => Directory.GetFiles(d, "*" + kOldProjectExtension)))
					{
						var newName = Path.ChangeExtension(pgProjFile, Constants.kProjectFileExtension);
						File.Move(pgProjFile, newName);
						handleProjectPathChanged(pgProjFile, newName);
					}
					break; // No need to go to case 3, since the problem it fixes would only have been caused by a version of Glyssen with data version 3 
				case 3:
					try
					{
						RobustIO.DeleteDirectory(Path.GetDirectoryName(SampleProject.SampleProjectFilePath) + " Audio", true);
					}
					catch (IOException e)
					{
						Logger.WriteError("Unable to clean up superfluous sample Audio Audio folder.", e);
					}
					var safeReplacements = new List<Tuple<string, string>>();
					var unsafeReplacements = new List<Tuple<string, string>>();
					foreach (var folder in Project.AllRecordingProjectFolders.Where(d => d.EndsWith(" Audio Audio")))
					{
						// Because of the way this bug (PG-1192) worked, the most likely thing is that the "correct"
						// version of the project will have been initially created but then all the actual work will
						// have gotten saved into the "incorrect" version. If this looks to be the case, we can
						// probably safely delete the correct one and then rename the incorrect one to have the correct
						// name.
						var baseFolder = Path.GetDirectoryName(folder);
						Debug.Assert(baseFolder != null);
						var languageFolder = Path.GetDirectoryName(baseFolder);
						Debug.Assert(languageFolder != null);
						var langCode = Path.GetFileName(languageFolder);
						Debug.Assert(langCode != null);
						var incorrectProjName = Path.GetFileName(folder);
						Debug.Assert(incorrectProjName != null);
						var correctProjectName = incorrectProjName.Substring(0, incorrectProjName.Length - " Audio".Length);
						var correctProjectFolder = Path.Combine(baseFolder, correctProjectName);
						if (Directory.Exists(correctProjectFolder))
						{
							var glyssenProjFilename = langCode + Constants.kProjectFileExtension;
							var incorrectProjectFilePath = Path.Combine(folder, glyssenProjFilename);
							var correctProjectFilePath = Path.Combine(correctProjectFolder, glyssenProjFilename);
							var finfoIncorrectProject = new FileInfo(incorrectProjectFilePath);
							var finfoCorrectProject = new FileInfo(correctProjectFilePath);
							if (finfoCorrectProject.Exists && finfoIncorrectProject.Exists)
							{
								if (finfoCorrectProject.LastWriteTimeUtc < finfoIncorrectProject.LastWriteTimeUtc)
								{
									var books = Directory.GetFiles(correctProjectFolder, "???.xml").Where(b => Canon.IsBookIdValid(Path.GetFileNameWithoutExtension(b)))
										.Select(XmlSerializationHelper.DeserializeFromFile<BookScript>);
									foreach (var book in books)
									{
										// If book == null, there was a problem loading it. It may be locked or be in some incompatible format.
										// In any case, we shouldn't risk assuming we can safely replace it.
										if (book == null || book.GetScriptBlocks().Any(b => b.UserConfirmed || b.MatchesReferenceText))
										{
											unsafeReplacements.Add(new Tuple<string, string>(folder, correctProjectFolder));
											break;
										}
									}
									if (unsafeReplacements.LastOrDefault()?.Item1 == folder)
										continue;
									try
									{
										// This bug/fix predates "live" Paratext projects, so there is definitely no need for a Paratext loading assistant.
										var projToBackUp = Project.Load(correctProjectFilePath, handleMissingBundleNeededForUpgrade, null);
										projToBackUp.CreateBackup("Overwritten by migration 3-4");
									}
									catch (Exception e)
									{
										Logger.WriteError("Unable to load project and create backup", e);
										safeReplacements.Add(new Tuple<string, string>(folder, correctProjectFolder));
										continue;
									}
									try
									{
										RobustIO.DeleteDirectory(correctProjectFolder, true);
										RobustIO.MoveDirectory(folder, correctProjectFolder);
									}
									catch (IOException e)
									{
										Logger.WriteError("Unable to replace project after making backup", e);
										Console.WriteLine(e);
										unsafeReplacements.Add(new Tuple<string, string>(folder, correctProjectFolder));
									}
								}
								else
								{
									unsafeReplacements.Add(new Tuple<string, string>(folder, correctProjectFolder));
								}
							}
						}
					}

					if (safeReplacements.Any())
					{
						if (confirmSafeAudioAudioReplacements != null && confirmSafeAudioAudioReplacements(safeReplacements))
						{
							foreach (var replacement in safeReplacements)
							{
								RobustIO.DeleteDirectory(replacement.Item2, true);
								RobustIO.MoveDirectory(replacement.Item1, replacement.Item2);
							}
							safeReplacements.Clear();
						}
					}
					if (unsafeReplacements.Any())
					{
						string fmt;
						if (unsafeReplacements.Count == 1)
						{
							fmt = Localizer.GetString("DataMigration.NoticeToManuallyFixAudioAudioProject",
								"However, doing this would replace the existing project by the same name. " +
								"Since {0} was unable to determine whether this was safe or otherwise failed to do the replacement, it is recommended " +
								"that you clean up the problem project yourself. You are encouraged to contact a local support person if needed or " +
								"seek help on {1}. You will be reminded about this each time you start the program as long as this problem remains unresolved.",
								"Param 0: \"Glyssen\" (product name); " +
								"Param 1: \"https://community.scripture.software.sil.org/\"" +
								"This follows the \"AudioAudioProblemPreambleSingle\".");
						}
						else
						{

							fmt = Localizer.GetString("DataMigration.NoticeToManuallyFixAudioAudioProjects",
								"However, doing this would replace the existing projects by the same name. " +
								"Since {0} was unable to determine whether this was safe or otherwise failed to do the replacements, it is recommended " +
								"that you clean up the problem projects yourself. You are encouraged to contact a local support person if needed or " +
								"seek help on {1}. You will be reminded about this each time you start the program as long as these problems remains unresolved.",
								"Param 0: \"Glyssen\" (product name); " +
								"Param 1: \"https://community.scripture.software.sil.org/\"" +
								"This follows the \"AudioAudioProblemPreambleMultiple\".");
						}
						var msg = GetAudioAudioProblemPreamble(unsafeReplacements.Count) +
							String.Join(Environment.NewLine, unsafeReplacements.Select(r => r.Item1)) + Environment.NewLine + Environment.NewLine +
							String.Format(fmt, GlyssenInfo.kProduct, Constants.kSupportSite);
						MessageModal.Show(msg, GlyssenInfo.kProduct, Buttons.OK, Icon.Warning);
					}
					if (unsafeReplacements.Any() || safeReplacements.Any())
						retVal = false;
					break;
				default:
					throw new Exception("No migration found from the existing data version!");
			}

			if (retVal)
				info.DataVersion = ApplicationMetadata.kDataFormatVersion;
			return retVal;
		}

		public static string GetAudioAudioProblemPreamble(int count)
		{
			string fmt;
			if (count == 1)
			{
				fmt = Localizer.GetString("DataMigration.AudioAudioProblemPreambleSingle",
					"To correct a problem in the way default recording project names were assigned in a previous version of the program, {0} has identified " +
					"the following project that should be renamed to end with only a single \"{1}\":",
					"Param 0: \"Glyssen\" (product name); " +
					"Param 1: the default recording project suffix (see \"Project.RecordingProjectDefaultSuffix\"); " +
					"This version of the preamble is for when there is a single project (see \"DataMigration.AudioAudioProblemPreambleMultiple\"");
			}
			else
			{
				fmt = Localizer.GetString("DataMigration.AudioAudioProblemPreambleMultiple",
					"To correct a problem in the way default recording project names were assigned in a previous version of the program, {0} has identified " +
					"the following projects that should be renamed to end with only a single \"{1}\":",
					"Param 0: \"Glyssen\" (product name); " +
					"Param 1: the default recording project suffix (see \"Project.RecordingProjectDefaultSuffix\"); " +
					"This version of the preamble is for when there are multiple projects (see \"DataMigration.AudioAudioProblemPreambleSingle\"");
			}
			return String.Format(fmt, GlyssenInfo.kProduct, Project.DefaultRecordingProjectNameSuffix.TrimStart()) + Environment.NewLine;
		}
	}
}
