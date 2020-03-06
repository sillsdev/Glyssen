using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Glyssen.Shared;
using SIL.DblBundle;
using SIL.IO;
using static System.IO.Path;

namespace GlyssenFileBasedPersistence
{
	public static class ProjectRepository
	{
		public const string kProjectFileExtension = ".glyssen";
		private const string kDistFilesReferenceTextDirectoryName = "reference_texts";

		public static IEnumerable<string> AllPublicationFolders => Directory.GetDirectories(ProjectsBaseFolder).SelectMany(Directory.GetDirectories);

		public static IEnumerable<string> AllRecordingProjectFolders => AllPublicationFolders.SelectMany(Directory.GetDirectories);

		public static string GetProjectFolderPath(IProject project) =>
			GetProjectFolderPath(project.LanguageIsoCode, project.MetadataId, project.Name);

		public static string GetProjectFolderPath(string langId, string publicationId, string recordingProjectName) => 
			Combine(ProjectsBaseFolder, langId, publicationId, recordingProjectName);

		public static string GetPublicationFolderPath(IBundle bundle) => 
			Combine(ProjectsBaseFolder, bundle.LanguageIso, bundle.Id);

		public static string GetLanguageFolderPath(string languageIsoCode) =>
			Combine(ProjectsBaseFolder, languageIsoCode);

		public static string GetProjectFilePath(string languageIsoCode, string metadataId, string recordingProjectName) =>
			Combine(GetProjectFolderPath(languageIsoCode, metadataId, recordingProjectName),
				languageIsoCode + kProjectFileExtension);

		public static string ProjectsBaseFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
			GlyssenInfo.Company, GlyssenInfo.Product);

		internal static string GetProjectFolderForStandardReferenceText(ReferenceTextType referenceTextType)
		{
			if (!referenceTextType.IsStandard())
				throw new InvalidOperationException("Attempt to get standard reference project folder for a non-standard type.");

			return Path.GetDirectoryName(GetReferenceTextProjectFileLocation(referenceTextType));
		}

		private static string GetReferenceTextProjectFileLocation(ReferenceTextType referenceTextType)
		{
			Debug.Assert(referenceTextType.IsStandard());
			string projectFileName = referenceTextType.ToString().ToLowerInvariant() + kProjectFileExtension;
			return FileLocationUtilities.GetFileDistributedWithApplication(kDistFilesReferenceTextDirectoryName, referenceTextType.ToString(), projectFileName);
		}
	}
}
