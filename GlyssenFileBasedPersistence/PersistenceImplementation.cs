using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Glyssen.Shared;
using GlyssenEngine;
using GlyssenEngine.Bundle;
using SIL;
using SIL.DblBundle;
using SIL.DblBundle.Text;
using SIL.IO;
using SIL.Reporting;
using SIL.Scripture;
using static System.IO.Path;
using static System.String;
// IBundle is probably an unfortunate name. IProjectSourceMetadata would have been a better name.
using IProjectSourceMetadata = SIL.DblBundle.IBundle;

namespace GlyssenFileBasedPersistence
{
	/// <summary>
	/// This class implements both the reader and writer interfaces for persisting Glyssen project-related files.
	/// This implementation is the "standard" file-based persistence used by the Glyssen program and can be used
	/// as a reference implementation for other implementations of these interfaces. Note that although these
	/// interfaces are intentionally separate (since some types of projects never need to be written), a
	/// typical implementation will cover both interfaces (or at least share a common helper class that handles
	/// the details of locating resources).
	/// </summary>
	public class PersistenceImplementation : IProjectPersistenceReader, IProjectPersistenceWriter
	{
		public const string kLocalReferenceTextDirectoryName = "Local Reference Texts";

		// Virtual to allow test implementation to override
		protected virtual string CustomReferenceTextProjectFileLocation =>
			Combine(ProjectRepository.ProjectsBaseFolder, kLocalReferenceTextDirectoryName);

		private const int kMaxPath = 259; // 260- 1 (for the NUL terminator)
		public const string kBookScriptFileExtension = ".xml";
		private const string kBookNoLongerAvailableExtension = ".nolongeravailable";
		public const string kProjectCharacterVerseFileName = "ProjectCharacterVerse.txt";
		public const string kProjectCharacterDetailFileName = "ProjectCharacterDetail.txt";
		private const string kVoiceActorInformationFileName = "VoiceActorInformation.xml";
		private const string kCharacterGroupFileName = "CharacterGroups.xml";
		public const string kFallbackVersificationPrefix = "fallback_";
		public const string kBackupExtSuffix = "bak";

		public void SetUpProjectPersistence(IProject project)
		{
			Directory.CreateDirectory(GetProjectFolderPath(project));
		}

		public void SetUpProjectPersistence<TM, TL>(IUserProject project, TextBundle<TM, TL> bundle)
			where TM : DblTextMetadata<TL>
			where TL : DblMetadataLanguage, new()
		{
			SetUpProjectPersistence(project);

			var languageFolder = GetLanguageFolder(project.LanguageIsoCode);

			int cFontFileCopyFailures = 0;
			foreach (var font in bundle.GetFonts())
			{
				string fontFileName = font.Item1;
				string targetFontPath = Combine(languageFolder, fontFileName);
				try
				{
					if (!File.Exists(targetFontPath))
					{
						using (var targetStream = new FileStream(targetFontPath, FileMode.Create))
						{
							font.Item2.CopyTo(targetStream);
						}
					}
				}
				catch (Exception ex)
				{
					var message = Format(Localizer.GetString("DblBundle.FontFileCopyFailed", "An attempt to copy font file {0} from the bundle to {1} failed."),
						fontFileName, languageFolder);
					if (cFontFileCopyFailures == 0)
					{
						ErrorReport.ReportNonFatalExceptionWithMessage(ex, message);
					}
					else
					{
						// For any subsequent failures, just log them.
						Logger.WriteError(message, ex);
					}
				}
			}

			using (var reader = bundle.GetVersification())
			{
				using (var writer = File.CreateText(GetVersificationFilePath(project)))
				{
					// Could use writer.Write(reader.ReadToEnd()) because these files are relatively small, but just in case.
					CopyContents(reader, writer);
				}
			}
		}

		private static void CopyContents(TextReader from, StreamWriter to)
		{
			const int kBufferSize = 4096;
			var buffer = new char[kBufferSize];
			var index = 0;
			int read;
			do
			{
				read = from.Read(buffer, 0, kBufferSize);
				if (read > 0)
				{
					to.Write(buffer, 0, read);
				}
			} while (read == kBufferSize);
		}

		public void DeleteProject(IUserProject project)
		{
			var projectFolder = ProjectRepository.GetProjectFolderPath(project);

			if (Directory.Exists(projectFolder))
				Directory.Delete(projectFolder, true);
			// Now also clear out the higher level folders if they are empty.
			var parent = GetDirectoryName(projectFolder);
			if (Directory.Exists(parent) && !Directory.GetFileSystemEntries(parent).Any())
				Directory.Delete(parent);
			parent = GetDirectoryName(parent);
			if (Directory.Exists(parent) && !Directory.GetFileSystemEntries(parent).Any())
				Directory.Delete(parent);
		}

		public void CreateBackup(IUserProject project, string description, bool hidden)
		{
			var newName = project.Name + " - " + description;
			string newDirectoryPath = ProjectRepository.GetProjectFolderPath(
				project.LanguageIsoCode, project.MetadataId, newName);
			if (Directory.Exists(newDirectoryPath))
			{
				string fmt = newDirectoryPath + " ({0})";
				int n = 1;
				do
				{
					newDirectoryPath = Format(fmt, n++);
				} while (Directory.Exists(newDirectoryPath));
			}

			try
			{
				DirectoryHelper.Copy(ProjectRepository.GetProjectFolderPath(project), newDirectoryPath);

				if (hidden)
				{
					var newFilePath = Directory.GetFiles(newDirectoryPath, "*" + ProjectRepository.kProjectFileExtension).FirstOrDefault();
					if (newFilePath != null)
					{
						using (var reader = new StreamReader(new FileStream(newFilePath, FileMode.Open)))
						{
							Project.SetHiddenFlag(GlyssenDblTextMetadata.Load(reader, newFilePath), newName, true);
						}
					}
				}
			}
			catch (Exception exceptionWhenCreatingBackup)
			{
				Logger.WriteError(exceptionWhenCreatingBackup);
				try
				{
					if (!Directory.Exists(newDirectoryPath))
						return;

					// Clean up by removing the partially copied directory.
					Directory.Delete(newDirectoryPath, true);
				}
				catch (Exception exceptionWhenCleaningUpFailedBackup)
				{
					Logger.WriteError(exceptionWhenCleaningUpFailedBackup);
				}
			}

		}

		public void ChangeProjectName(IUserProject project, string newName)
		{
			if (project.Name == newName)
				return; // Or should we throw an exception?
			var existingProjectFolder = ProjectRepository.GetProjectFolderPath(project);
			var newPath = ProjectRepository.GetProjectFolderPath(project.LanguageIsoCode, project.MetadataId, newName);
			Directory.Move(existingProjectFolder, newPath);
		}

		/// <summary>
		/// Changes the metadata ID (also called the publication ID because it represents
		/// the ID under which the project data is published).
		/// </summary>
		/// <param name="project">The project (having the old MetadataId)</param>
		/// <param name="setInternalId">Action to be called when the implementation is ready for
		/// the internal metadata id of the project to be changed (after which getting the
		/// project's MetadataId will return the new value.</param>
		/// <param name="saveMetadata">Action to be called to persist the project metadata
		/// using the given TextWriter. Callee is responsible for disposing the writer.
		/// If calling this action throws an exception, callee is responsible for reverting
		/// the internal metadata ID to the previous value.</param>
		public void ChangePublicationId(IUserProject project, Action setInternalId, Action<TextWriter> saveMetadata)
		{
			string origProjectFolder = ProjectRepository.GetProjectFolderPath(project);

			setInternalId();
			var newProjectFolder = ProjectRepository.GetProjectFolderPath(project);

			Directory.CreateDirectory(Path.GetDirectoryName(newProjectFolder));
			RobustIO.MoveDirectory(origProjectFolder, newProjectFolder);

			try
			{
				saveMetadata(GetTextWriter(project, ProjectResource.Metadata));
			}
			catch (Exception error)
			{
				try
				{
					// Try to move it back before throwing the error.
					RobustIO.MoveDirectory(newProjectFolder, origProjectFolder);
					throw;
				}
				catch (Exception moveFolderBackError)
				{
					// Uh-oh. We've gotten this project into a corrupted state.
					throw new Exception(Environment.NewLine + error + Environment.NewLine + Environment.NewLine +
						Localizer.GetString("Project.ChangeMetadataIdCatastrophicFailure",
							"During the attempt to recover and revert to the original ID, a catastrophic error occurred that has probably left " +
							"this project in a corrupted state. Please contact support."), moveFolderBackError);
				}
			}

			try
			{
				// REVIEW: Wouldn't the above call to RobustIO.MoveDirectory delete the original folder (or does it only move the contents)?
				RobustIO.DeleteDirectory(Path.GetDirectoryName(origProjectFolder));
			}
			catch (Exception e)
			{
				Logger.WriteError(e);
			}
		}

		public TextWriter GetTextWriter(IProject project, ProjectResource resource)
		{
			return new StreamWriter(GetPath(resource, project));
		}

		public TextWriter GetTextWriter(IProject project, IScrBook book)
		{
			return new StreamWriter(GetBookDataFilePath(project, book.BookId));
		}

		public void ArchiveBookThatIsNoLongerAvailable(IUserProject project, string bookCode)
		{
			var origPath = GetBookDataFilePath(project, bookCode);
			RobustFile.Move(origPath, origPath + kBookNoLongerAvailableExtension);
		}

		public void RestoreResourceFromBackup(IUserProject project, ProjectResource resource)
		{
			var resourceFilePath = GetPath(resource, project);
			if (RobustFile.Exists(resourceFilePath))
			{
				var corruptedLdmlFilePath = resourceFilePath + "corrupted";
				RobustFile.Delete(corruptedLdmlFilePath);
				RobustFile.Move(resourceFilePath, corruptedLdmlFilePath);
			}
			RobustFile.Move(resourceFilePath + kBackupExtSuffix, resourceFilePath);
		}

		public bool SaveBackupResource(IUserProject project, ProjectResource resource)
		{
			var resourceFilePath = GetPath(resource, project);
			if (!RobustFile.Exists(resourceFilePath))
				return false;

			var backupPath = resourceFilePath + kBackupExtSuffix;
			if (RobustFile.Exists(backupPath))
				RobustFile.Delete(backupPath);
			RobustFile.Move(resourceFilePath, backupPath);
			return true;
		}

		// Total path limit is 260 characters. Need to allow for:
		// C:\ProgramData\FCBH-SIL\Glyssen\Language IEFT tag (<=35 chars)\xxxxxxxxxxxxxxxx\<Recording Project Name>\ProjectCharacterDetail.txt
		// C:\ProgramData\FCBH-SIL\Glyssen\Language IEFT tag (<=35 chars)\xxxxxxxxxxxxxxxx\<Recording Project Name>\fallback_versification.vrs (same length as above)
		// C:\ProgramData\FCBH-SIL\Glyssen\Language IEFT tag (<=35 chars)\xxxxxxxxxxxxxxxx\<Recording Project Name>\Language IEFT tag (<=35 chars).glyssen
		
		// In the first implementation, we use the actual project metadata length, but this is effectively a constant
		// because all valid metadata IDs are exactly 16 characters long (except in tests and in the sample project).
		private const int kMetadataIdLength = 16;

		/// <summary>
		/// For a given project (i.e., with a known LanguageIsoCode length), this method gets the maximum
		/// allowable project name length so as to ensure that this persistence implementation would be
		/// able to store any and all (including possible future) project files for the project using
		/// the name without violating operating system limits on path lengths.
		/// </summary>
		/// <remarks>
		/// The implementation of <see cref="ChangeProjectName"/> assumes that the caller has ensured
		/// that any new name passed in to it will be no longer than the value returned by this method.
		/// </remarks>
		public int GetMaxProjectNameLength(IUserProject project) =>
			kMaxPath - Combine(ProjectRepository.ProjectsBaseFolder, project.LanguageIsoCode, project.MetadataId).Length -
			Math.Max(GetProjectFilename(project).Length, kProjectCharacterDetailFileName.Length) - 2; // the magic 2 allows for two Path.DirectorySeparatorChar's

		public int GetMaxProjectNameLength(string languageIsoCode) =>
			kMaxPath - Combine(ProjectRepository.ProjectsBaseFolder, languageIsoCode).Length - kMetadataIdLength -
			Math.Max(GetProjectFilename(languageIsoCode).Length, kProjectCharacterDetailFileName.Length) - 3; // the magic 3 allows for three Path.DirectorySeparatorChar's

		private string GetProjectFilename(IProject project)
		{
			switch (project)
			{
				case IReferenceTextProject refText:
					var name = refText.Name;
					if (refText.Type.IsStandard())
						name = name.ToLower(CultureInfo.InvariantCulture); // Shouldn't matter on Windows, but just in case.
					return GetProjectFilename(name);
				case IUserProject userProject:
					return GetProjectFilename(userProject.LanguageIsoCode);
				default:
					throw new ArgumentException("Unexpected project type", nameof(project));
			}
		}

		private string GetProjectFilename(string baseName) =>
			baseName + ProjectRepository.kProjectFileExtension;

		public IEnumerable<ResourceReader<string>> GetCustomReferenceTextsNotAlreadyLoaded()
		{
			if (Directory.Exists(CustomReferenceTextProjectFileLocation))
			{
				foreach (var dir in Directory.GetDirectories(CustomReferenceTextProjectFileLocation))
				{
					var customId = GetFileName(dir);
					var metadataFilePath = Combine(dir, customId + ProjectRepository.kProjectFileExtension);
					if (RobustFile.Exists(metadataFilePath) && !ReferenceTextProxy.IsCustomReferenceTextIdentifierInListOfAvailable(customId))
					{
						yield return new ResourceReader<string>(customId,
							GetReader(metadataFilePath));
					}
				}
			}
		}
		
		/// <summary>
		/// Gets whether there is (or might be) an existing project identified by the given
		/// language code, id, and name. Typically, this would return the same value as
		/// ResourceExists for ProjectResource.Metadata, given an IUserProject object having
		/// these same three value. However, the purpose of this method is to determine whether
		/// it would be valid to create a new project (or rename an existing project) to have
		/// these values without clobbering something else. So, for example, even if there
		/// were no existing metadata resource, if there were other things identified by
		/// these attributes, this should still return true.
		/// </summary>
		public bool ProjectExistsHaving(string languageIsoCode, string metadataId, string name)
		{
			var projectFolder = ProjectRepository.GetProjectFolderPath(languageIsoCode, metadataId, name);
			return Directory.Exists(projectFolder) && Directory.EnumerateFileSystemEntries(projectFolder).Any();
		}

		public bool ResourceExists(IProject project, ProjectResource resource) =>
			RobustFile.Exists(GetPath(resource, project));

		public bool BackupResourceExists(IProject project, ProjectResource resource) =>
			RobustFile.Exists(GetPath(resource, project) + kBackupExtSuffix);

		public TextReader Load(IProject project, ProjectResource resource) =>
			GetReader(GetPath(resource, project));

		private string GetPath(ProjectResource resource, IProject project)
		{
			switch (resource)
			{
				case ProjectResource.Metadata:
					return GetProjectFilePath(project);
				case ProjectResource.Ldml:
					return GetLdmlFilePath(project);
				case ProjectResource.Versification:
					return GetVersificationFilePath(project);
				case ProjectResource.FallbackVersification:
					if (project is IUserProject userProject)
						return GetFallbackVersificationFilePath(userProject);
					else
					{
						Debug.Fail("Requested fallback versification from non user project");
						return null;
					}
				case ProjectResource.CharacterVerseData:
					return GetProjectResourceFilePath(project, kProjectCharacterVerseFileName);
				case ProjectResource.CharacterDetailData:
					return GetProjectResourceFilePath(project, kProjectCharacterDetailFileName);
				case ProjectResource.CharacterGroups:
					return GetProjectResourceFilePath(project, kCharacterGroupFileName);
				case ProjectResource.VoiceActorInformation:
					return GetProjectResourceFilePath(project, kVoiceActorInformationFileName);
				default:
					throw new ArgumentOutOfRangeException(nameof(resource), resource, null);
			}
		}

		public TextReader LoadBook(IProject project, string bookId) =>
			GetReader(GetBookDataFilePath(project, bookId));

		public IEnumerable<ResourceReader<string>> GetExistingBooks(IProject project)
		{
			var list = new List<ResourceReader<string>>();
			ForEachBookFileInProject(GetProjectFolderPath(project),
				(bookId, fileName) => { list.Add(new ResourceReader<string>(bookId, new StreamReader(fileName))); });
			return list;
		}

		public bool TryInstallFonts(IUserProject project, string fontFamily, IFontRepository fontRepository)
		{
			string languageFolder = GetLanguageFolder(project.LanguageIsoCode);

			// There could be more than one if different styles (Regular, Italics, etc.) are in different files
			var ttfFilesToInstall = Directory.GetFiles(languageFolder, "*.ttf")
				.Where(ttf => fontRepository.DoesTrueTypeFontFileContainFontFamily(ttf, fontFamily)).ToList();

			if (ttfFilesToInstall.Count > 0)
			{
				fontRepository.TryToInstall(fontFamily, ttfFilesToInstall);
				return true;
			}

			return false;
		}

		private TextReader GetReader(string fullPath) =>
			RobustFile.Exists(fullPath) ? new StreamReader(new FileStream(fullPath, FileMode.Open)) : null;

		private string GetBookDataFilePath(IProject project, string bookId)
		{
			return ChangeExtension(Combine(GetProjectFolderPath(project), bookId), "xml");
		}

		public bool BookResourceExists(IProject project, string bookId)
		{
			return RobustFile.Exists(GetBookDataFilePath(project, bookId));
		}

		public string GetProjectFolderPath(IProject project)
		{
			switch (project)
			{
				case IReferenceTextProject refText when refText.Type == ReferenceTextType.Custom:
					return Combine(CustomReferenceTextProjectFileLocation, refText.Name);
				case IReferenceTextProject refText:
					return ProjectRepository.GetProjectFolderForStandardReferenceText(refText.Type);
				case IUserProject userProject:
					return ProjectRepository.GetProjectFolderPath(userProject);
				default:
					throw new ArgumentException("Unexpected project type", nameof(project));
			}
		}

		private string GetProjectResourceFilePath(IProject project, string filename) =>
			Combine(GetProjectFolderPath(project), filename);

		public string GetProjectFilePath(IProject project) => Combine(GetProjectFolderPath(project), GetProjectFilename(project));

		public static string GetDefaultProjectFilePath(IProjectSourceMetadata bundle)
		{
			var publicationName = bundle.Name;
			if (IsNullOrEmpty(publicationName))
				publicationName = (bundle as GlyssenBundle)?.Metadata?.Language?.Name ?? Empty;
			return ProjectRepository.GetProjectFilePath(bundle.LanguageIso, bundle.Id,
				Project.GetDefaultRecordingProjectName(publicationName, bundle.LanguageIso));
		}

		private string GetVersificationFilePath(IProject project) =>
			GetProjectResourceFilePath(project, DblBundleFileUtils.kVersificationFileName);

		public string GetLdmlFilePath(IProject project)
		{
			// We actually currently do not include or use LDML files with reference texts, but if we
			// ever did, this is possibly what we'd want. (It's safe, because the caller handles the
			// case of a non-existent file.)
			var filename = (project is IUserProject userProject) ? userProject.ValidLanguageIsoCode : project.Name;
			return GetProjectResourceFilePath(project, filename + DblBundleFileUtils.kUnzippedLdmlFileExtension);
		}

		public string GetFallbackVersificationFilePath(IUserProject project) => 
			Combine(ProjectRepository.GetProjectFolderPath(project), kFallbackVersificationPrefix + DblBundleFileUtils.kVersificationFileName);

		private string GetLanguageFolder(string languageIsoCode) => ProjectRepository.GetLanguageFolderPath(languageIsoCode);

		public static void ForEachBookFileInProject(string projectDir, Action<string, string> action)
		{
			string[] files = Directory.GetFiles(projectDir, "???" + kBookScriptFileExtension);
			for (int i = 1; i <= BCVRef.LastBook; i++)
			{
				string bookCode = BCVRef.NumberToBookCode(i);
				string possibleFileName = Combine(projectDir, bookCode + kBookScriptFileExtension);
				if (files.Contains(possibleFileName))
					action(bookCode, possibleFileName);
			}
		}
	}
}
