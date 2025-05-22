using System;
using System.IO;
using Glyssen.Shared;
using Glyssen.Utilities;
using NUnit.Framework;
using SIL.IO;
using static System.IO.Path;
using static Glyssen.Utilities.GlyssenShare;

namespace GlyssenTests.Utilities
{
	[TestFixture]
	public class GlyssenShareTests
	{
		[Test]
		public void Constructor_NullProject_ThrowsArgumentNullException()
		{
			Assert.That(() => new GlyssenShare((IUserProject)null),
				Throws.InstanceOf<ArgumentNullException>().With.Property("ParamName").EqualTo("project"));
		}

		[Test]
		public void Constructor_GlyssenshareFileInNonexistentFolder_ThrowsFileNotFoundException()
		{
			Assert.That(() => new GlyssenShare(@"\some\nonexistent\folder\file" + kShareFileExtension),
				Throws.InstanceOf<DirectoryNotFoundException>());
		}

		[Test]
		public void Constructor_NonexistentGlyssenshareFile_ThrowsFileNotFoundException()
		{
			var filepath = Combine(GetTempPath(), "my_missing_file_9856738" + kShareFileExtension);
			Assert.That(filepath, Does.Not.Exist, "SETUP - Sanity check");
			Assert.That(() => new GlyssenShare(filepath),
				Throws.InstanceOf<FileNotFoundException>());
		}

		[TestCase(true)]
		[TestCase(false)]
		public void Constructor_NotAGlyssenshareFile_ThrowsArgumentException(bool nameAsGlyssenShare)
		{
			var filepath = Combine(GetTempFileName());
			if (nameAsGlyssenShare)
			{
				var newPath = ChangeExtension(filepath, kShareFileExtension);
				File.Move(filepath, newPath);
				filepath = newPath;
			}
			Assert.That(filepath, Does.Exist, "SETUP - Sanity check");
			Assert.That(() => new GlyssenShare(filepath), Throws.InstanceOf<ArgumentException>());
		}

		[Test] public void Constructor_InvalidGlyssenshareFileNoDirAndWrongNumberOfFolderLevels_ThrowsArgumentException()
		{
			using (var file = TempFile.FromResource(Properties.Resources.novaliddir, kShareFileExtension))
			{
				Assert.That(file.Path, Does.Exist, "SETUP - Sanity check");
				Assert.That(() => new GlyssenShare(file.Path), Throws.InstanceOf<ArgumentException>());
			}
		}

		[Test] public void Constructor_InvalidGlyssenshareFileNoProject_ThrowsArgumentException()
		{
			using (var file = TempFile.FromResource(Properties.Resources.qaa_Bogus, kShareFileExtension))
			{
				Assert.That(file.Path, Does.Exist, "SETUP - Sanity check");
				Assert.That(() => new GlyssenShare(file.Path), Throws.InstanceOf<ArgumentException>());
			}
		}

		[Test] public void Constructor_ValidGlyssenshareFile_SetsProjectFilePathCorrectly()
		{
			using (var file = TempFile.FromResource(Properties.Resources.qaa_Elbonian, kShareFileExtension))
			{
				Assert.That(file.Path, Does.Exist, "SETUP - Sanity check");
				var glyssenShare = new GlyssenShare(file.Path);
				Assert.That(glyssenShare.ProjectFilePath,
					Does.EndWith(Combine("qaa", "1af90518e2ccf03b", "Elbonian", "qaa.glyssen")));
			}
		}

		[Test] public void Constructor_ManualGlyssenshareFileWithMultipleDirEntries_SetsProjectFilePathCorrectly()
		{
			using (var file = TempFile.FromResource(Properties.Resources.manual, kShareFileExtension))
			{
				Assert.That(file.Path, Does.Exist, "SETUP - Sanity check");
				var glyssenShare = new GlyssenShare(file.Path);
				Assert.That(glyssenShare.ProjectFilePath,
					Does.EndWith(Combine("qaa", "1af90518e2ccf03b", "Elbonian", "qaa.glyssen")));
			}
		}

		[Test] public void Constructor_ManualGlyssenshareFileWithNoDirEntries_SetsProjectFilePathCorrectly()
		{
			using (var file = TempFile.FromResource(Properties.Resources.nodirentries, kShareFileExtension))
			{
				Assert.That(file.Path, Does.Exist, "SETUP - Sanity check");
				var glyssenShare = new GlyssenShare(file.Path);
				Assert.That(glyssenShare.ProjectFilePath,
					Does.EndWith(Combine("qaa", "1af90518e2ccf03b", "Elbonian", "qaa.glyssen")));
			}
		}

		internal class ProjectStub : IUserProject
		{
			public string Name { get; }
			public string LanguageIsoCode { get; }
			public string ValidLanguageIsoCode => LanguageIsoCode;
			public string MetadataId { get; }
			public string FontFamily => "Charis SIL";

			internal ProjectStub()
			{
				LanguageIsoCode = "qaa";
				MetadataId = "not used";
				Name = "Elbonian";
			}
		}
	}
}
