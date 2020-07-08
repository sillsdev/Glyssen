using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Glyssen.Shared;
using GlyssenEngine;
using SIL.DblBundle;
using SIL.IO;
using static System.IO.Path;
using static System.String;

namespace GlyssenFileBasedPersistence
{
	public class ProjectFileReader : IProjectPersistenceReader
	{
		// Virtual to allow test implementation to override
		protected virtual string CustomReferenceTextProjectFileLocation =>
			Combine(ProjectRepository.ProjectsBaseFolder, kLocalReferenceTextDirectoryName);

		public const string kLocalReferenceTextDirectoryName = "Local Reference Texts";

		protected const string kBackupExtSuffix = "bak";

		protected const string kProjectCharacterVerseFileName = "ProjectCharacterVerse.txt";
		protected const string kProjectCharacterDetailFileName = "ProjectCharacterDetail.txt";

		public IEnumerable<ResourceReader> GetCustomReferenceTextsNotAlreadyLoaded()
		{
			if (Directory.Exists(CustomReferenceTextProjectFileLocation))
			{
				foreach (var dir in Directory.GetDirectories(CustomReferenceTextProjectFileLocation))
				{
					var customId = GetFileName(dir);
					var metadataFilePath = Combine(dir, customId + ProjectRepository.kProjectFileExtension);
					if (RobustFile.Exists(metadataFilePath) && !ReferenceTextProxy.IsCustomReferenceTextIdentifierInListOfAvailable(customId))
					{
						yield return new ResourceReader(customId, GetReader(metadataFilePath));
					}
				}
			}
		}

		/// <summary>
		/// Gets whether it would be valid to create a new project (or rename an existing project)
		/// in the project folder identified by the given language code, id, and name. Typically,
		/// this would return the same value as ResourceExists for ProjectResource.Metadata, given
		/// an IUserProject object having these same three values (since the metadata file is
		/// always required to exist for a project). However, this implementation returns true if
		/// there are any existing files in that folder since it is possible that other files
		/// could be in the folder (left over from some project or copied there by the user for
		/// some reason) and the purpose of this method is to determine whether we risk clobbering
		/// something else.
		/// </summary>
		public bool ProjectExistsHaving(string languageIsoCode, string metadataId, string name) =>
			NonEmptyFolderExists(languageIsoCode, metadataId, name);

		public bool ResourceExists(IProject project, ProjectResource resource) =>
			RobustFile.Exists(GetPath(resource, project));

		public bool BackupResourceExists(IProject project, ProjectResource resource) =>
			RobustFile.Exists(GetPath(resource, project) + kBackupExtSuffix);

		public bool BookResourceExists(IProject project, string bookId) =>
			RobustFile.Exists(GetBookDataFilePath(project, bookId));

		public TextReader Load(IProject project, ProjectResource resource) =>
			GetReader(GetPath(resource, project));

		public TextReader LoadBook(IProject project, string bookId) =>
			GetReader(GetBookDataFilePath(project, bookId));

		public bool TryInstallFonts(IUserProject project, IFontRepository fontRepository)
		{
			var fontFamily = project.FontFamily;
			var languageFolder = GetLanguageFolder(project.LanguageIsoCode);

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

		public string GetProjectFilePath(IProject project) =>
			Combine(GetProjectFolderPath(project), GetProjectFilename(project));

		public string GetLdmlFilePath(IProject project)
		{
			// We actually currently do not include or use LDML files with reference texts, but if we
			// ever did, this is possibly what we'd want. (It's safe, because the caller handles the
			// case of a non-existent file.)
			var filename = (project is IUserProject userProject) ? userProject.ValidLanguageIsoCode : project.Name;
			return GetProjectResourceFilePath(project, filename + DblBundleFileUtils.kUnzippedLdmlFileExtension);
		}
		#endregion

		#region Private/protected Helper methods
		private TextReader GetReader(string fullPath) =>
			RobustFile.Exists(fullPath) ? new StreamReader(new FileStream(fullPath, FileMode.Open, FileAccess.Read)) : null;

		protected string GetProjectFilename(string baseName) =>
			baseName + ProjectRepository.kProjectFileExtension;
		protected string GetProjectResourceFilePath(IProject project, string filename) =>
			Combine(GetProjectFolderPath(project), filename);

		protected string GetProjectFilename(IProject project)
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

		protected string GetPath(ProjectResource resource, IProject project)
		{
			const string kVoiceActorInformationFileName = "VoiceActorInformation.xml";
			const string kCharacterGroupFileName = "CharacterGroups.xml";

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

		protected string GetBookDataFilePath(IProject project, string bookId) =>
			ChangeExtension(Combine(GetProjectFolderPath(project), bookId),
				ProjectRepository.kBookScriptFileExtension);

		protected string GetLanguageFolder(string languageIsoCode) =>
			ProjectRepository.GetLanguageFolderPath(languageIsoCode);

		protected static bool NonEmptyFolderExists(string languageIsoCode, string metadataId, string name)
		{
			var projectFolder = ProjectRepository.GetProjectFolderPath(languageIsoCode, metadataId, name);
			return Directory.Exists(projectFolder) && Directory.EnumerateFileSystemEntries(projectFolder).Any();
		}
		#endregion
	}
}
