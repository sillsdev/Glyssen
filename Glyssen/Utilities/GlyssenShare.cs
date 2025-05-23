using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DesktopAnalytics;
using Glyssen.Shared;
using GlyssenFileBasedPersistence;
using ICSharpCode.SharpZipLib.Zip;
using SIL.IO;
using SIL.Reporting;
using static System.StringSplitOptions;
using static GlyssenFileBasedPersistence.ProjectRepository;

namespace Glyssen.Utilities
{
	internal class GlyssenShare
	{
		private readonly string m_glyssenShareFilePath;
		public const string kShareFileExtension = ".glyssenshare";

		private readonly IUserProject m_project;

		public static string WildcardFileMatch => "*" + kShareFileExtension;

		private static int ProjectsBasePathLength { get; }

		public string ProjectFilePath { get; }

		static GlyssenShare()
		{
			ProjectsBasePathLength = ProjectsBaseFolder.Length;
		}

		public GlyssenShare(IUserProject project)
		{
			m_project = project ?? throw new ArgumentNullException(nameof(project));
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
			string[] levels = null;
			var foundProjectFileEntry = false;

			try
			{
				using (var zipFile = new ZipFile(glyssenShareFilePath))
				{

					// Find the first (full) directory entry and also check for the project file.
					foreach (ZipEntry entry in zipFile)
					{
						if (entry.IsDirectory)
						{
							if (path != null)
								continue;

							// For a truly well-formed glyssenshare file, there should be only one
							// Directory entry, and it should have exactly 3 levels, but for the sake
							// of robustness, I'm allowing for a manually created glyssenshare that
							// might have entries for each successive level.
							path = entry.Name;
							levels = path.Split(new[] { '\\', '/' }, RemoveEmptyEntries);
							switch (levels.Length)
							{
								case 1:
								case 2:
									path = null;
									levels = null;
									break;
								case 3:
									path = Path.Combine(levels);
									break;
								default:
									// Technically, when creating the glyssenshare, we do include
									// subfolders. I'm 99.9% sure that Glyssen never creates or
									// expects any subfolders, so maybe this is unnecessary or even
									// potentially a bad idea. But rather than throwing an exception,
									// I think we'll just consider it a feature, but intentionally
									// not covering with tests, since it's dubious.
									Analytics.Track("Glyssenshare with subfolders", new Dictionary<string, string>
									{
										{ "dir", path }
									});
									Logger.WriteEvent("Glyssenshare with subfolders: " +
										glyssenShareFilePath);
									break;
							}
						}
						else if (!foundProjectFileEntry)
						{
							// Usually, I think we should have found the directory entry first, so we
							// can do the more strict check.
							if (levels != null)
							{
								foundProjectFileEntry = Path.GetFileName(entry.Name) == levels[0] + kProjectFileExtension;
								if (foundProjectFileEntry)
									break;
							}
							else
							{
								// Allow for a manually created glyssenshare that might omit the
								// directory entry altogether.
								foundProjectFileEntry = entry.Name.EndsWith(kProjectFileExtension);
								path = Path.GetDirectoryName(entry.Name);
								if (path != null)
								{
									levels = path.Split(new[] { '\\', '/' }, RemoveEmptyEntries);
									if (levels.Length != 3)
									{
										path = null;
										levels = null;
									}
								}
							}
						}
					}
				}
			}
			catch (ZipException ex)
			{
				throw new ArgumentException("Must be a glyssenshare file.",
					nameof(glyssenShareFilePath), ex);
			}

			if (path == null)
				throw new ArgumentException("Invalid glyssenshare file - no valid directory entry.");

			if (!foundProjectFileEntry)
				throw new ArgumentException("Invalid glyssenshare file - no project entry.");

			var targetDir = Path.Combine(ProjectsBaseFolder, path);

			var projectFileName = levels[0] + kProjectFileExtension;
			ProjectFilePath = Path.Combine(targetDir, projectFileName);
		}

		public string Create()
		{
			if (m_project == null)
			{
				throw new InvalidOperationException("To create a GlyssenShare file, use the " +
					"version of the constructor that takes a source folder and a project.");
			}

			var shareFolder = DefaultShareFolder;
			Directory.CreateDirectory(shareFolder);

			var saveAsName = Path.Combine(shareFolder, m_project.LanguageIsoCode + "_" + m_project.Name) + kShareFileExtension;

			RobustFile.Delete(saveAsName);

			CreateZipFile(saveAsName);

			return saveAsName;
		}

		private void CreateZipFile(string saveAsName)
		{
			using (var zipStream = new ZipOutputStream(File.Create(saveAsName)))
			{
				zipStream.SetLevel(9); // Set compression level (0-9)

				var projectFolder = GetProjectFolderPath(m_project);
				Debug.Assert(projectFolder.StartsWith(ProjectsBaseFolder));

				AddDirectoryToZip(zipStream, projectFolder);
			}
		}

		/// <summary>
		/// Ensure forward slashes for ZIP format
		/// </summary>
		private static ZipEntry MakeZipEntry(string path) => new ZipEntry(path.Replace("\\", "/"));

		private void AddDirectoryToZip(ZipOutputStream zipStream, string directory)
		{
			foreach (var filePath in Directory.GetFiles(directory))
			{
				string relativePath = filePath.Substring(ProjectsBasePathLength);
				var entry = MakeZipEntry(relativePath);
				entry.DateTime = File.GetLastWriteTime(filePath);

				zipStream.PutNextEntry(entry);

				using (var fileStream = File.OpenRead(filePath))
					fileStream.CopyTo(zipStream);

				zipStream.CloseEntry();
			}

			foreach (var subDir in Directory.GetDirectories(directory))
				AddDirectoryToZip(zipStream, subDir);
		}

		public static bool IsGlyssenShare(string file) =>
			file.ToLowerInvariant().EndsWith(kShareFileExtension);

		public bool Extract()
		{
			if (m_glyssenShareFilePath == null)
				throw new InvalidOperationException("No GlyssenShare file was specified.");

			var fastZip = new FastZip();
			fastZip.ExtractZip(m_glyssenShareFilePath, ProjectsBaseFolder, null);

			return RobustFile.Exists(ProjectFilePath);
		}
	}
}
