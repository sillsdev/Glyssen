using System;
using System.Collections.Generic;
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
	public class PersistenceImplementation : IProjectPersistenceReader, IProjectPersistenceWriter
	{
		public const string kLocalReferenceTextDirectoryName = "Local Reference Texts";

		private static string s_customReferenceTextBaseFolder;
		
		#region static internals to support testing
		protected string ProprietaryReferenceTextProjectFileLocation
		{
			get => s_customReferenceTextBaseFolder ?? (s_customReferenceTextBaseFolder = Combine(ProjectRepository.ProjectsBaseFolder, kLocalReferenceTextDirectoryName));
			set => s_customReferenceTextBaseFolder = value;
		}
		#endregion

		private const int kMaxPath = 260;
		public const string kGlyssenScriptFileExtension = ".glyssenscript";
		public const string kGlyssenPackFileExtension = ".glyssenpack";
		public const string kBookScriptFileExtension = ".xml";
		private const string kBookNoLongerAvailableExtension = ".nolongeravailable";
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
					// Could use ReadToEnd here because these files are relatively small, but just in case.
					const int kBufferSize = 4096;
					var buffer = new char[kBufferSize];
					var index = 0;
					do
					{
						var read = reader.Read(buffer, index, kBufferSize);
						if (read > 0)
						{
							writer.Write(buffer);
							index += read;
						}
						else
							break;
					} while (true);
				}
			}
		}

		public void DeleteProject(IProject project)
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
					var newFilePath = Directory.GetFiles(newDirectoryPath, "*" + ProjectRepository.kProjectFileExtension).FirstOrDefault();
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

		public TextWriter GetTextWriter(IProject project, IScrBook book)
		{
			return new StreamWriter(GetBookDataFilePath(project, book.BookId));
		}

		public void ArchiveBookNoLongerAvailable(IProject project, string bookCode)
		{
			var origPath = GetBookDataFilePath(project, bookCode);
			RobustFile.Move(origPath, origPath + kBookNoLongerAvailableExtension);
		}

		// Total path limit is 260 (MAX_PATH) characters. Need to allow for:
		// C:\ProgramData\FCBH-SIL\Glyssen\<ISO>\xxxxxxxxxxxxxxxx\<Recording Project Name>\ProjectCharacterDetail.txt
		// C:\ProgramData\FCBH-SIL\Glyssen\<ISO>\xxxxxxxxxxxxxxxx\<Recording Project Name>\<ISO>.glyssen
		public int GetMaxProjectNameLength(IProject project) =>
			kMaxPath - Combine(ProjectRepository.ProjectsBaseFolder, project.LanguageIsoCode, project.MetadataId).Length -
			Math.Max(GetProjectFilename(project).Length, kProjectCharacterDetailFileName.Length) - 3; // the magic 3 allows for three Path.DirectorySeparatorChar's

		public int MaxBaseRecordingNameLength => kMaxDefaultProjectNameLength - Project.DefaultRecordingProjectNameSuffix.Length;

		private string GetProjectFilename(IProject project) => project.LanguageIsoCode + ProjectRepository.kProjectFileExtension;

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
					return ChangeExtension(GetLdmlFilePath(project), DblBundleFileUtils.kUnzippedLdmlFileExtension + "bak");
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

		public IEnumerable<BookReader> GetExistingBooks(IProject project)
		{
			var list = new List<BookReader>();
			ForEachBookFileInProject(GetProjectFolderPath(project),
				(bookId, fileName) => { list.Add(new BookReader(bookId, new StreamReader(fileName))); });
			return list;
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

		private string GetProjectFolderPath(IProject project)
		{
			if (project is IReferenceTextProxy proxy)
			{
				if (proxy.Type == ReferenceTextType.Custom)
				{
					var lowercase = proxy.Name.ToLowerInvariant();
					// TODO: This try (needs a catch) probably doesn't belong here.
					try
					{
						m_metadata = LoadMetadata(Type, Combine(ProjectFolder, lowercase + ProjectRepository.kProjectFileExtension));

						//if (IsStandardReferenceText(Type))
						//	return GetProjectFolderForStandardReferenceText(Type);

						return Combine(ProprietaryReferenceTextProjectFileLocation, proxy.Name);
					}
				}
				return ProjectRepository.GetProjectFolderForStandardReferenceText(proxy.Type);
			}
			else
				return ProjectRepository.GetProjectFolderPath(project);
		}

		private string GetProjectResourceFilePath(IProject project, string filename) =>
			Combine(GetProjectFolderPath(project), filename);

		public static string GetDefaultProjectFilePath(IProjectSourceMetadata bundle) => 
			ProjectRepository.GetProjectFilePath(bundle.LanguageIso, bundle.Id,
				Project.GetDefaultRecordingProjectName(bundle.Name));

		protected string GetVersificationFilePath(IProject project) =>
			GetProjectResourceFilePath(project, DblBundleFileUtils.kVersificationFileName);

		protected string GetLdmlFilePath(IProject project) =>
			GetProjectResourceFilePath(project, project.ValidLanguageIsoCode + DblBundleFileUtils.kUnzippedLdmlFileExtension);

		protected string GetFallbackVersificationFilePath(IProject project) => 
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
