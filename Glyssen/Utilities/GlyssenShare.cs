using System;
using System.IO;
using System.Linq;
using GlyssenEngine;
using GlyssenFileBasedPersistence;
using ICSharpCode.SharpZipLib.Zip;
using SIL.IO;
using static System.StringSplitOptions;

namespace Glyssen.Utilities
{
	internal class GlyssenShare
	{
		private readonly string m_glyssenShareFilePath;
		public const string kShareFileExtension = ".glyssenshare";

		private readonly string m_sourceDir;
		private readonly Project m_project;

		public static string WildcardFileMatch => "*" + kShareFileExtension;

		public string ProjectFilePath { get; }

		public GlyssenShare(string sourceDir, Project project)
		{
			m_sourceDir = sourceDir;
			m_project = project;
		}

		public GlyssenShare(string glyssenShareFilePath)
		{
			if (!IsGlyssenShare(glyssenShareFilePath))
			{
				throw new ArgumentException("Must be a glyssenshare file.",
					nameof(glyssenShareFilePath));
			}

			m_glyssenShareFilePath = glyssenShareFilePath;

			string path = null;

			using (var zipFile = new ZipFile(glyssenShareFilePath))
			{
				// Find the first (full) directory entry
				foreach (ZipEntry entry in zipFile)
				{
					if (entry.IsDirectory)
					{
						// For a truly well-formed glyssenshare file, there should be only one
						// Directory entry, and it should have exactly 3 levels, but for the sake
						// of robustness, I'm allowing for a manually created glyssenshare that
						// might have entries for each successive level.
						path = entry.Name;
						var levels = path.Split(new[] { '\\', '/' }, RemoveEmptyEntries).Length;
						switch (levels)
						{
							case 1:
							case 2:
								path = null;
								break;
							case 3:
								break;
							default:
								throw new ArgumentException("Not a valid glyssenshare file.",
									nameof(glyssenShareFilePath));
						}
						if (path != null)
							break;
					}
				}
			}

			if (path == null)
				return;

			var targetDir = Path.Combine(ProjectRepository.ProjectsBaseFolder, path);

			var projectFileName = path.Split('/').First() + ProjectRepository.kProjectFileExtension;
			ProjectFilePath = Path.Combine(targetDir, projectFileName);
		}

		public string Create()
		{
			if (m_sourceDir == null || m_project == null)
			{
				throw new InvalidOperationException("To create a GlyssenShare file, use the " +
					"version of the constructor that takes a source folder and a project.");
			}

			var shareFolder = ProjectRepository.DefaultShareFolder;
			Directory.CreateDirectory(shareFolder);

			var saveAsName = Path.Combine(shareFolder, m_project.LanguageIsoCode + "_" + m_project.Name) + kShareFileExtension;

			CreateZipWithCustomPath(m_sourceDir, ProjectRepository.ProjectsBaseFolder, saveAsName);

			return saveAsName;
		}

		private void CreateZipWithCustomPath(string sourceDir, string baseFolder, string saveAsName)
		{
			var nameInZip = sourceDir.Substring(baseFolder.Length);

			using (var zipStream = new ZipOutputStream(File.Create(saveAsName)))
			{
				zipStream.SetLevel(9); // Set compression level (0-9)

				AddDirectoryToZip(zipStream, sourceDir, nameInZip);
			}
		}

		private void AddDirectoryToZip(ZipOutputStream zipStream, string directory, string entryPath)
		{
			foreach (var filePath in Directory.GetFiles(directory))
			{
				string entryName = Path.Combine(entryPath, Path.GetFileName(filePath));
				var entry = new ZipEntry(entryName.Replace("\\", "/")) // Ensure forward slashes for ZIP format
				{
					DateTime = File.GetLastWriteTime(filePath),
					Size = new FileInfo(filePath).Length
				};

				zipStream.PutNextEntry(entry);

				using (var fileStream = File.OpenRead(filePath))
				{
					fileStream.CopyTo(zipStream);
				}

				zipStream.CloseEntry();
			}

			foreach (var subDir in Directory.GetDirectories(directory))
			{
				AddDirectoryToZip(zipStream, subDir, Path.Combine(entryPath, Path.GetFileName(subDir)));
			}
		}

		public static bool IsGlyssenShare(string file) =>
			file.ToLowerInvariant().EndsWith(kShareFileExtension);

		public bool Extract()
		{
			if (m_glyssenShareFilePath == null)
				throw new InvalidOperationException("No GlyssenShare file was specified.");

			var fastZip = new FastZip();
			fastZip.ExtractZip(m_glyssenShareFilePath, ProjectRepository.ProjectsBaseFolder, null);

			return RobustFile.Exists(ProjectFilePath);
		}
	}
}
