using System;
using System.IO;
using System.Linq;
using L10NSharp;
using ProtoScript.Bundle;

namespace ProtoScript
{
	internal static class DataMigrator
	{
		public static void UpgradeToCurrentDataFormatVersion()
		{
			Exception error;
			var settings = ApplicationMetadata.Load(out error);
			if (error != null)
				throw error;
			if (UpgradeToCurrentDataFormatVersion(settings))
				settings.Save();
		}

		private static bool UpgradeToCurrentDataFormatVersion(ApplicationMetadata info)
		{
			if (info.DataVersion >= Properties.Settings.Default.DataFormatVersion)
				return false;

			switch (info.DataVersion)
			{
				case 0:
					foreach (var languageFolder in Directory.GetDirectories(Project.ProjectsBaseFolder))
					{
						foreach (var publicationFolder in Directory.GetDirectories(languageFolder))
						{
							var filesToMove = Directory.GetFiles(publicationFolder);
							if (!filesToMove.Any())
								continue;

							var projectFilePath = Directory.GetFiles(publicationFolder, "*" + Project.kProjectFileExtension).FirstOrDefault();
							if (projectFilePath != null)
							{
								Exception exception;
								var metadata = DblMetadata.Load(projectFilePath, out exception);
								string recordingProjectName;
								if (exception != null)
								{
									// Just add a directory layer and don't worry about it for now.
									recordingProjectName = Path.GetFileName(publicationFolder);
								}
								else
								{
									if (metadata.identification != null && !string.IsNullOrEmpty(metadata.identification.name))
										recordingProjectName = metadata.identification.name;
									else
										recordingProjectName = metadata.id;
								}
								recordingProjectName = Project.GetDefaultRecordingProjectName(recordingProjectName);
								var recordingProjectFolder = Path.Combine(publicationFolder, recordingProjectName);
								Directory.CreateDirectory(recordingProjectFolder);
								foreach (var file in filesToMove)
									File.Move(file, Path.Combine(recordingProjectFolder, Path.GetFileName(file)));
								if (Properties.Settings.Default.CurrentProject == projectFilePath)
								{
									Properties.Settings.Default.CurrentProject = Path.Combine(recordingProjectFolder,
										Path.GetFileName(projectFilePath));
								}
							}
						}
					}
					break;
				default:
					throw new Exception("No migration found from the existing data version!");
			}

			info.DataVersion = Properties.Settings.Default.DataFormatVersion;
			return true;
		}
	}
}
