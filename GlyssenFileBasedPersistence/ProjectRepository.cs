using System.Collections.Generic;
using System.IO;
using System.Linq;
using Glyssen.Shared;
using SIL.DblBundle;

namespace GlyssenFileBasedPersistence
{
	public static class ProjectRepository
	{
		public static string ProjectsBaseFolder => GlyssenInfo.BaseDataFolder;

		public static IEnumerable<string> AllPublicationFolders => Directory.GetDirectories(ProjectsBaseFolder).SelectMany(Directory.GetDirectories);

		public static IEnumerable<string> AllRecordingProjectFolders => AllPublicationFolders.SelectMany(Directory.GetDirectories);

		public static string GetProjectFolderPath(IProject project) =>
			GetProjectFolderPath(project.LanguageIsoCode, project.MetadataId, project.Name);

		public static string GetProjectFolderPath(string langId, string publicationId, string recordingProjectName) => 
			Path.Combine(ProjectsBaseFolder, langId, publicationId, recordingProjectName);

		public static string GetPublicationFolderPath(IBundle bundle) => 
			Path.Combine(ProjectsBaseFolder, bundle.LanguageIso, bundle.Id);

		public static string GetLanguageFolderPath(string languageIsoCode) =>
			Path.Combine(ProjectsBaseFolder, languageIsoCode);

		public static string GetProjectFilePath(string languageIsoCode, string metadataId, string recordingProjectName) =>
			Path.Combine(GetProjectFolderPath(languageIsoCode, metadataId, recordingProjectName),
				languageIsoCode + PersistenceImplementation.kProjectFileExtension);
	}
}
