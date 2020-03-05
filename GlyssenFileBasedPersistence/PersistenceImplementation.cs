using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Glyssen.Shared;
using GlyssenEngine;
using GlyssenEngine.Bundle;
using GlyssenEngine.Paratext;
using SIL;
using SIL.DblBundle;
using SIL.DblBundle.Text;
using SIL.IO;
using SIL.Reporting;
using static System.String;
// IBundle is probably an unfortunate name. IProjectSourceMetadata would have been a better name.
using IProjectSourceMetadata = SIL.DblBundle.IBundle;

namespace GlyssenFileBasedPersistence
{
	public class PersistenceImplementation : IProjectPersistenceReader, IProjectPersistenceWriter
	{
		private const int kMaxPath = 260;
		public const string kProjectFileExtension = ".glyssen";
		public const string kGlyssenScriptFileExtension = ".glyssenscript";
		public const string kGlyssenPackFileExtension = ".glyssenpack";
		public const string kLocalReferenceTextDirectoryName = "Local Reference Texts";
		public const string kBookScriptFileExtension = ".xml";
		public const string kProjectCharacterVerseFileName = "ProjectCharacterVerse.txt";
		public const string kProjectCharacterDetailFileName = "ProjectCharacterDetail.txt";
		private const string kVoiceActorInformationFileName = "VoiceActorInformation.xml";
		private const string kCharacterGroupFileName = "CharacterGroups.xml";
		public const string kFallbackVersificationPrefix = "fallback_";

		// Total path limit is 260 characters. Need to allow for:
		// C:\ProgramData\FCBH-SIL\Glyssen\xxx\xxxxxxxxxxxxxxxx\<Recording Project Name>\ProjectCharacterDetail.txt
		// C:\ProgramData\FCBH-SIL\Glyssen\xxx\xxxxxxxxxxxxxxxx\<Recording Project Name>\xxx*.glyssen
		private const int kMaxDefaultProjectNameLength = 150;

		public void SetUpProjectPersistence(IProject project)
		{
			Directory.CreateDirectory(ProjectRepository.GetProjectFolderPath(project));
		}

		public void SetUpProjectPersistence<TM, TL>(IProject project, TextBundle<TM, TL> bundle)
			where TM : DblTextMetadata<TL>
			where TL : DblMetadataLanguage, new()
		{
			SetUpProjectPersistence(project);

			var languageFolder = GetLanguageFolder(project.LanguageIsoCode);


			int cFontFileCopyFailures = 0;
			foreach (var font in bundle.GetFonts())
			{
				string fontFileName = font.Item1;
				string targetFontPath = Path.Combine(languageFolder, fontFileName);
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
				File.WriteAllText(GetVersificationFilePath(project), reader.ReadToEnd());
			}
		}

		public void DeleteProject(IProject project)
		{
			var projectFolder = ProjectRepository.GetProjectFolderPath(project);

			if (Directory.Exists(projectFolder))
				Directory.Delete(projectFolder, true);
			// Now also clear out the higher level folders if they are empty.
			var parent = Path.GetDirectoryName(projectFolder);
			if (Directory.Exists(parent) && !Directory.GetFileSystemEntries(parent).Any())
				Directory.Delete(parent);
			parent = Path.GetDirectoryName(parent);
			if (Directory.Exists(parent) && !Directory.GetFileSystemEntries(parent).Any())
				Directory.Delete(parent);
		}

		public void CreateBackup(IProject project, string description, bool hidden)
		{
			string newDirectoryPath = ProjectRepository.GetProjectFolderPath(
				project.LanguageIsoCode, project.MetadataId, project.Name + " - " + description);
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
					var newFilePath = Directory.GetFiles(newDirectoryPath, "*" + kProjectFileExtension).FirstOrDefault();
					if (newFilePath != null)
					{
						using (var reader = new StreamReader(new FileStream(newFilePath, FileMode.Open)))
						{
							GlyssenDblTextMetadata.SetHiddenFlag(reader, newFilePath, hidden);
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

		public void ChangeProjectName(IProject project, string newName)
		{
			if (project.Name == newName)
				return; // Or should we throw an exception?
			var existingProjectFolder = ProjectRepository.GetProjectFolderPath(project);
			var newPath = ProjectRepository.GetProjectFolderPath(project.LanguageIsoCode, project.MetadataId, newName);
			Directory.Move(existingProjectFolder, newPath);
		}

		public TextWriter GetTextWriter(IProject project, ProjectResource resource)
		{
			return new StreamWriter(GetPath(resource, project));
		}

		public void SaveBook(IProject project, string bookId, TextReader data)
		{
			throw new NotImplementedException();
		}

		// Total path limit is 260 (MAX_PATH) characters. Need to allow for:
		// C:\ProgramData\FCBH-SIL\Glyssen\<ISO>\xxxxxxxxxxxxxxxx\<Recording Project Name>\ProjectCharacterDetail.txt
		// C:\ProgramData\FCBH-SIL\Glyssen\<ISO>\xxxxxxxxxxxxxxxx\<Recording Project Name>\<ISO>.glyssen
		public int GetMaxProjectNameLength(IProject project) =>
			kMaxPath - Path.Combine(ProjectRepository.ProjectsBaseFolder, project.LanguageIsoCode, project.MetadataId).Length -
			Math.Max(GetProjectFilename(project).Length, kProjectCharacterDetailFileName.Length) - 3; // the magic 3 allows for three Path.DirectorySeparatorChar's

		public int MaxBaseRecordingNameLength => kMaxDefaultProjectNameLength - Project.DefaultRecordingProjectNameSuffix.Length;

		private string GetProjectFilename(IProject project) => project.LanguageIsoCode + kProjectFileExtension;

		public bool ProjectExists(string languageIsoCode, string metadataId, string name) => 
			Directory.Exists(ProjectRepository.GetProjectFolderPath(languageIsoCode, metadataId, name));

		public bool ResourceExists(IProject project, ProjectResource resource) =>
			RobustFile.Exists(GetPath(resource, project));

		public TextReader Load(IProject project, ProjectResource resource) =>
			GetReader(GetPath(resource, project));

		private string GetPath(ProjectResource resource, IProject project)
		{
			switch (resource)
			{
				case ProjectResource.Metadata:
					return GetProjectFilename(project);
				case ProjectResource.Ldml:
					return GetLdmlFilePath(project);
				case ProjectResource.LdmlBackupFile:
					return Path.ChangeExtension(GetLdmlFilePath(project), DblBundleFileUtils.kUnzippedLdmlFileExtension + "bak");
				case ProjectResource.Versification:
					return GetVersificationFilePath(project);
				case ProjectResource.FallbackVersification:
					return GetFallbackVersificationFilePath(project);
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

		private TextReader GetReader(string fullPath) =>
			RobustFile.Exists(fullPath) ? new StreamReader(new FileStream(fullPath, FileMode.Open)) : null;

		private string GetBookDataFilePath(IProject project, string bookId)
		{
			return Path.ChangeExtension(Path.Combine(ProjectRepository.GetProjectFolderPath(project), bookId), "xml");
		}

		public bool BookResourceExists(IProject project, string bookId)
		{
			return RobustFile.Exists(GetBookDataFilePath(project, bookId));
		}

		private string GetProjectResourceFilePath(IProject project, string filename) =>
			Path.Combine(ProjectRepository.GetProjectFolderPath(project), filename);

		public static string GetDefaultProjectFilePath(IProjectSourceMetadata bundle) => 
			ProjectRepository.GetProjectFilePath(bundle.LanguageIso, bundle.Id,
				Project.GetDefaultRecordingProjectName(bundle.Name));

		protected string GetVersificationFilePath(IProject project) =>
			GetProjectResourceFilePath(project, DblBundleFileUtils.kVersificationFileName);

		protected string GetLdmlFilePath(IProject project) =>
			GetProjectResourceFilePath(project, project.ValidLanguageIsoCode + DblBundleFileUtils.kUnzippedLdmlFileExtension);

		protected string GetFallbackVersificationFilePath(IProject project) => 
			Path.Combine(ProjectRepository.GetProjectFolderPath(project), kFallbackVersificationPrefix + DblBundleFileUtils.kVersificationFileName);

		private string GetLanguageFolder(string languageIsoCode) => ProjectRepository.GetLanguageFolderPath(languageIsoCode);
	}
}
