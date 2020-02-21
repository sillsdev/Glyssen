using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Glyssen.Shared;
using GlyssenEngine;
using GlyssenEngine.Bundle;
using GlyssenEngine.Paratext;
using SIL.DblBundle;
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

		public void Save(IProject project, ProjectResource resource, string data)
		{
			throw new NotImplementedException();
		}

		public void SaveBook(IProject project, string bookId, string data)
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

		public bool ResourceExists(ProjectResource resource, IProject project) =>
			File.Exists(GetPath(resource, project));

		public TextReader Load(ProjectResource resource, IProject project)
		{
			string fullPath = GetPath(resource, project);
			return File.Exists(fullPath) ?
				new StreamReader(new FileStream(fullPath, FileMode.Open)) :
				null;
		}

		private string GetPath(ProjectResource resource, IProject project)
		{
			switch (resource)
			{
				case ProjectResource.Metadata:
					return GetProjectFilename(project);
				case ProjectResource.Ldml:
					return
				case ProjectResource.Versification:
					break;
				case ProjectResource.FallbackVersification:
					return GetFallbackVersificationFilePath(project);
					break;
				case ProjectResource.ProjectCharacterVerseData:
					return GetProjectCharacterVerseDataPath(project);
					break;
				case ProjectResource.ProjectCharacterDetailData:
					return GetProjectCharacterDetailDataPath(project);
					break;
				case ProjectResource.CharacterGroups:
					break;
				case ProjectResource.VoiceActorInformation:
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(resource), resource, null);
			}
		}

		public TextReader LoadBook(IProject project, string bookId)
		{
			throw new NotImplementedException();
		}


		//public string GetProjectFilePath(IProject project) => ProjectRepository.GetProjectFilePath(LanguageIsoCode, m_metadata.Id, m_recordingProjectName);

		private string GetProjectResourceFilePath(IProject project, string filename) =>
			Path.Combine(ProjectRepository.GetProjectFolderPath(project), filename);

		private string GetProjectCharacterVerseDataPath(IProject project) =>
			GetProjectResourceFilePath(project, kProjectCharacterVerseFileName);

		private string GetProjectCharacterDetailDataPath(IProject project) =>
			GetProjectResourceFilePath(project, kProjectCharacterDetailFileName);

		public static string GetDefaultProjectFilePath(IProjectSourceMetadata bundle) => 
			ProjectRepository.GetProjectFilePath(bundle.LanguageIso, bundle.Id,
				Project.GetDefaultRecordingProjectName(bundle.Name));


		protected string GetVersificationFilePath(IProject project) =>
			Path.Combine(ProjectRepository.GetProjectFolderPath(project), DblBundleFileUtils.kVersificationFileName);

		protected string GetFallbackVersificationFilePath(IProject project) => 
			Path.Combine(ProjectRepository.GetProjectFolderPath(project), kFallbackVersificationPrefix + DblBundleFileUtils.kVersificationFileName);


		//private string LanguageFolder => GetLanguageFolderPath(m_metadata.Language.Iso);
	}
}
