using System;
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
using static System.IO.Path;
using static System.String;
// IBundle is probably an unfortunate name. IProjectSourceMetadata would have been a better name.
using IProjectSourceMetadata = SIL.DblBundle.IBundle;

namespace GlyssenFileBasedPersistence
{
	/// <summary>
	/// This class implements both the reader (via inheritance) and writer interfaces for persisting Glyssen
	/// project-related files. This implementation is the "standard" file-based persistence used by the
	/// Glyssen program and can be used as a reference implementation for other implementations of these
	/// interfaces. Note that although these interfaces are intentionally separate (since some types of
	/// projects never need to be written), a typical implementation will cover both interfaces (or at
	/// least share a common helper class that handles the details of locating resources).
	/// </summary>
	public class PersistenceImplementation : ProjectFileReader, IProjectPersistenceWriter
	{
		#region IProjectPersistenceWriter implementation
		public event ProjectDeletedHandler OnProjectDeleted;

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

			var cFontFileCopyFailures = 0;
			try
			{
				foreach (var font in bundle.GetFonts())
				{
					var fontFileName = font.Item1;
					var targetFontPath = Combine(languageFolder, fontFileName);
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
					catch (Exception fontSpecificException)
					{
						var message = Format(Localizer.GetString("DblBundle.FontFileCopyFailed", 
							"An attempt to copy font file {0} from the bundle to {1} failed."),
							fontFileName, languageFolder);
						if (cFontFileCopyFailures == 0)
						{
							ErrorReport.ReportNonFatalExceptionWithMessage(fontSpecificException, message);
						}
						else
						{
							// For any subsequent failures, just log them.
							Logger.WriteError(message, fontSpecificException);
						}
					}
				}
			}
			catch (Exception bundleFontsAccessException)
			{
				var message = Format(Localizer.GetString("DblBundle.BundleFontAccessFailure",
					"Failed to retrieve font files from the bundle."));
				Debug.Assert(cFontFileCopyFailures == 0);
				ErrorReport.ReportNonFatalExceptionWithMessage(bundleFontsAccessException, message);
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

		public TextWriter GetTextWriter(IProject project, ProjectResource resource)
		{
			return new StreamWriter(GetPath(resource, project));
		}

		public TextWriter GetTextWriter(IProject project, IScrBook book)
		{
			return new StreamWriter(GetBookDataFilePath(project, book.BookId));
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

			OnProjectDeleted?.Invoke(this, project);
		}

		public void CreateBackup(IUserProject project, string description, bool inactive)
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

				if (inactive)
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
				// Above call to RobustIO.MoveDirectory moves the contents but does not delete the original folder itself.
				RobustIO.DeleteDirectory(Path.GetDirectoryName(origProjectFolder));
			}
			catch (Exception e)
			{
				Logger.WriteError(e);
			}
		}

		public void ArchiveBookThatIsNoLongerAvailable(IUserProject project, string bookCode)
		{
			const string kBookNoLongerAvailableExtension = ".nolongeravailable";
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
		private const int kMaxPath = 259; // 260- 1 (for the NUL terminator)

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

		#endregion

		#region Additional public methods not part of the interface (not used in GlyssenEngine)

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

		/// <summary>
		/// Gets the default project file path. If the normal default project folder already exists
		/// and has files in it, adds a number to ensure a unique, available location is returned in
		/// which a new project can be created.
		/// </summary>
		/// <param name="bundle">The bundle from which project information is obtained</param>
		/// <param name="revisionToUseIfProjectExists">Revision number (of the bundle)</param>
		public static string GetAvailableDefaultProjectFilePath(IProjectSourceMetadata bundle, int revisionToUseIfProjectExists = -1)
		{
			var recordingProjectName = GetDefaultProjectName(bundle);
			if (NonEmptyFolderExists(bundle.LanguageIso, bundle.Id, recordingProjectName))
			{
				string numericSuffix;
				if (revisionToUseIfProjectExists >= 0 &&
					File.Exists(ProjectRepository.GetProjectFilePath(bundle.LanguageIso, bundle.Id, recordingProjectName)))
				{
					// If we get here, there is already a project with the default path but for a
					// different revision). So we need to generate a unique revision-specific
					// project path.
					// TODO (PG-222): Before blindly creating a new project, we probably need to
					// prompt the user to see if they want to upgrade an existing project instead.
					// If there are multiple candidate projects, we'll need to present a list.
					// (This would require some refactoring because we don't want UI here.)
					recordingProjectName = $"{recordingProjectName} (Rev {revisionToUseIfProjectExists})";
					if (!NonEmptyFolderExists(bundle.LanguageIso, bundle.Id, recordingProjectName))
						return ProjectRepository.GetProjectFilePath(bundle.LanguageIso, bundle.Id, recordingProjectName);
					numericSuffix = ".{0}"; // Really unlikely that we would ever get here.
				}
				else
					numericSuffix = " ({0})";

				var fmt = recordingProjectName + numericSuffix;
				var n = 1;
				do
				{
					recordingProjectName = Format(fmt, n++);
				} while (NonEmptyFolderExists(bundle.LanguageIso, bundle.Id, recordingProjectName));
			}
			return ProjectRepository.GetProjectFilePath(bundle.LanguageIso, bundle.Id, recordingProjectName);
		}

		public static string GetDefaultProjectFilePath(IProjectSourceMetadata bundle)
		{
			return ProjectRepository.GetProjectFilePath(bundle.LanguageIso, bundle.Id,
				GetDefaultProjectName(bundle));
		}

		public static string GetDefaultProjectName(IProjectSourceMetadata bundle)
		{			
			var publicationName = bundle.Name;
			if (IsNullOrEmpty(publicationName))
				publicationName = (bundle as GlyssenBundle)?.Metadata?.Language?.Name ?? Empty;
			return Project.GetDefaultRecordingProjectName(publicationName, bundle.LanguageIso);
		}

		private const string kFallbackVersificationPrefix = "fallback_";

		public string GetFallbackVersificationFilePath(IUserProject project) => 
			Combine(ProjectRepository.GetProjectFolderPath(project), kFallbackVersificationPrefix + DblBundleFileUtils.kVersificationFileName);
		
		#endregion

		#region General-purpose private helper methods (used for reader, writer, etc.)

		private string GetVersificationFilePath(IProject project) =>
			GetProjectResourceFilePath(project, DblBundleFileUtils.kVersificationFileName);
		#endregion
	}
}
