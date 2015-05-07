using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DesktopAnalytics;
using L10NSharp;
using Paratext;
using ProtoScript.Bundle;
using ProtoScript.Properties;
using SIL.DblBundle;
using SIL.DblBundle.Text;

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
			if (info.DataVersion >= Settings.Default.DataFormatVersion)
				return false;

			Analytics.Track("DataVersionUpgrade", new Dictionary<string, string>
			{
				{ "old", info.DataVersion.ToString(CultureInfo.InvariantCulture) },
				{ "new", Settings.Default.DataFormatVersion.ToString(CultureInfo.InvariantCulture) }
			});

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
							var metadata = PgDblTextMetadata.Load<PgDblTextMetadata>(projectFilePath, out exception);
							string recordingProjectName;
							if (exception != null)
							{
								// Just add a directory layer and don't worry about it for now.
								recordingProjectName = Path.GetFileName(publicationFolder);
							}
							else
							{
								if (metadata.Identification != null && !string.IsNullOrEmpty(metadata.Identification.Name))
									recordingProjectName = metadata.Identification.Name;
								else
									recordingProjectName = metadata.Id;
							}
							recordingProjectName = Project.GetDefaultRecordingProjectName(recordingProjectName);
							var recordingProjectFolder = Path.Combine(publicationFolder, recordingProjectName);
							Directory.CreateDirectory(recordingProjectFolder);
							foreach (var file in filesToMove)
								File.Move(file, Path.Combine(recordingProjectFolder, Path.GetFileName(file)));
							if (Settings.Default.CurrentProject == projectFilePath)
							{
								Settings.Default.CurrentProject = Path.Combine(recordingProjectFolder,
									Path.GetFileName(projectFilePath));
							}
						}
					}
					goto case 1;
				case 1:
					foreach (var recordingProjectFolder in Project.AllRecordingProjectFolders.ToList())
					{
						var versificationPath = Path.Combine(recordingProjectFolder, DblBundleFileUtils.kVersificationFileName);
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
									var metadata = PgDblTextMetadata.Load<PgDblTextMetadata>(projectFilePath, out exception);
									var origBundlePath = metadata.OriginalPathOfDblFile;
									if (string.IsNullOrEmpty(origBundlePath))
									{
										try
										{
											Project.DeleteProjectFolderAndEmptyContainingFolders(recordingProjectFolder, true);
										}
										catch (Exception)
										{
											// Oh, well, we tried. Not the end of the world.
										}
									}
									else
									{
										var bundle = new PgBundle(origBundlePath);
										var errorlogPath = Path.Combine(recordingProjectFolder, "errorlog.txt");
										Versification.Table.HandleVersificationLineError = ex =>
										{
											var msg = string.Format(LocalizationManager.GetString("DataMigration.InvalidVersificationFile",
												"Invalid versification file encountered during data migration. Errors must be fixed or subsequent " +
												"attempts to open this project will fail.\r\n" +
												"Project: {0}\r\n" +
												"Text release Bundle: {1}\r\n" +
												"Versification file: {2}\r\n" +
												"Error: {3}"),
												projectFilePath, origBundlePath, versificationPath, ex.Message);
											MessageBox.Show(msg, Program.kProduct, MessageBoxButtons.OK, MessageBoxIcon.Warning);
											File.WriteAllText(errorlogPath, msg);
										};
										bundle.CopyVersificationFile(versificationPath);
										Project.LoadVersification(versificationPath);
									}
								}
							}
						}
					}
					Versification.Table.HandleVersificationLineError = null;
					break;
				default:
					throw new Exception("No migration found from the existing data version!");
			}

			info.DataVersion = Settings.Default.DataFormatVersion;
			return true;
		}
	}
}
