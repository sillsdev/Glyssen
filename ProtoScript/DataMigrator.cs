using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using L10NSharp;
using ProtoScript.Bundle;
using ProtoScript.Properties;
using SIL.Windows.Forms.FileSystem;

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
					foreach (var publicationFolder in Project.AllPublicationFolders)
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
					goto case 1;
				case 1:
					foreach (var recordingProjectFolder in Project.AllRecordingProjectFolders.ToList())
					{
						var versificationPath = Path.Combine(recordingProjectFolder, Project.kVersificationFileName);
						if (!File.Exists(versificationPath))
						{
							var projectFilePath = Directory.GetFiles(recordingProjectFolder, "*" + Project.kProjectFileExtension).FirstOrDefault();
 
							if (projectFilePath != null)
							{
								if (projectFilePath.Equals(Project.SampleProjectFilePath, StringComparison.OrdinalIgnoreCase))
								{
									File.WriteAllText(versificationPath, Resources.EnglishVersification);
								}
								else
								{
									Exception exception;
									var metadata = DblMetadata.Load(projectFilePath, out exception);
									var origBundlePath = metadata.OriginalPathOfDblFile;
									if (string.IsNullOrEmpty(origBundlePath))
									{
										ConfirmRecycleDialog.ConfirmThenRecycle(string.Format("Standard format project \"{0}\"",
											recordingProjectFolder), recordingProjectFolder);
									}
									else
									{
										var bundle = new Bundle.Bundle(origBundlePath);
										bundle.CopyVersificationFile(versificationPath);
									}
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
