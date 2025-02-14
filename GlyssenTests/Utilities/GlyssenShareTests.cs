using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

		[Test] public void Constructor_ValidGlyssenshareFile_Sets()
		{
			using (var file = TempFile.FromResource(Properties.Resources.qaa_Elbonian, kShareFileExtension))
			{
				Assert.That(file.Path, Does.Not.Exist, "SETUP - Sanity check");
				var glyssenShare = new GlyssenShare(file.Path);
				Assert.That(glyssenShare.ProjectFilePath, Does.EndWith(@"\qaa\1af90518e2ccf03b\Elbonian"));
			}
		}
	}
}
