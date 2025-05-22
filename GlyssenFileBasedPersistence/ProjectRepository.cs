using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Glyssen.Shared;
using GlyssenEngine;
using GlyssenEngine.Bundle;
using GlyssenEngine.Utilities;
using SIL.DblBundle;
using SIL.IO;
using static System.IO.Path;

namespace GlyssenFileBasedPersistence
{
	public static class ProjectRepository
	{
		public const string kBookScriptFileExtension = ".xml";
		public const string kProjectFileExtension = ".glyssen";
		public const string kShareFolderName = "share";
		private const string kDistFilesReferenceTextDirectoryName = "reference_texts";

		public static IEnumerable<string> AllPublicationFolders => Directory.GetDirectories(ProjectsBaseFolder).SelectMany(Directory.GetDirectories);

		public static IEnumerable<string> AllRecordingProjectFolders => AllPublicationFolders.SelectMany(Directory.GetDirectories);

		public static string GetProjectFolderPath(IUserProject project) =>
			GetProjectFolderPath(project.LanguageIsoCode, project.MetadataId, project.Name);
		
		public static string GetProjectFilePath(IUserProject project) =>
			project == null ? null : Combine(GetProjectFolderPath(project), project.LanguageIsoCode + kProjectFileExtension);

		public static string GetProjectFolderPath(string langId, string publicationId, string recordingProjectName) => 
			Combine(ProjectsBaseFolder, langId, publicationId, recordingProjectName);

		public static string GetPublicationFolderPath(IBundle bundle) => 
			Combine(ProjectsBaseFolder, bundle.LanguageIso, bundle.Id);

		public static string GetLanguageFolderPath(string languageIsoCode) =>
			Combine(ProjectsBaseFolder, languageIsoCode);

		public static string GetProjectFilePath(string languageIsoCode, string metadataId, string recordingProjectName) =>
			Combine(GetProjectFolderPath(languageIsoCode, metadataId, recordingProjectName),
				languageIsoCode + kProjectFileExtension);

		public static string ProjectsBaseFolder => Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
			GlyssenInfo.Company, GlyssenInfo.Product);

		public static string GetRecordingProjectNameFromProjectFilePath(string path) => path.GetContainingFolderName();

		public static string DefaultShareFolder => Combine(ProjectsBaseFolder, kShareFolderName);

		internal static string GetProjectFolderForStandardReferenceText(ReferenceTextType referenceTextType)
		{
			if (!referenceTextType.IsStandard())
				throw new InvalidOperationException("Attempt to get standard reference project folder for a non-standard type.");

			return GetDirectoryName(GetStandardReferenceTextProjectFileLocation(referenceTextType));
		}

		private static string GetStandardReferenceTextProjectFileLocation(ReferenceTextType referenceTextType)
		{
			Debug.Assert(referenceTextType.IsStandard());
			string projectFileName = referenceTextType.ToString().ToLowerInvariant() + kProjectFileExtension;
			return FileLocationUtilities.GetFileDistributedWithApplication(kDistFilesReferenceTextDirectoryName, referenceTextType.ToString(), projectFileName);
		}

		public static string GetProjectName(string projectFilePath)
		{
			Debug.Assert(GetExtension(projectFilePath) == kProjectFileExtension);
			var folder = GetDirectoryName(projectFilePath);
			Debug.Assert(folder != null);
			var val = GetFileName(folder);
			Debug.Assert(val != null);
			return val;
		}

		public static Project LoadProject(string projectFilePath)
		{
			bool fileLocked = FileHelper.IsLocked(projectFilePath);

			var metadata = GlyssenDblTextMetadata.Load<GlyssenDblTextMetadata>(projectFilePath, out var exception);
			if (exception != null)
				throw exception;

			var project = new Project(metadata, GetRecordingProjectNameFromProjectFilePath(projectFilePath), true);
			if (fileLocked)
				project.ProjectIsWritable = false;
			return project;
		}

		public static void ForEachBookFileInProject(string projectDir, Action<string, string> action)
		{
			string[] files = Directory.GetFiles(projectDir, "???" + kBookScriptFileExtension);
			foreach (var bookCode in StandardCanon.AllBookCodes)
			{
				string possibleFileName = Combine(projectDir, bookCode + kBookScriptFileExtension);
				if (files.Contains(possibleFileName))
					action(bookCode, possibleFileName);
			}
		}
	}
}
