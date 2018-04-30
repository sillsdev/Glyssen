using System;
using System.IO;
using NUnit.Framework;
using SIL.IO;
using Waxuquerque.Utilities;

namespace WaxuquerqueTests.Utilities
{
	class FileSystemUtilsTests
	{
		[Test]
		public void GetShortName()
		{
			const string extension = ".txt";
			const string contents = "ABC 123";
			using (var tempFile = TempFile.WithFilename("This is a long file name" + extension))
			{
				File.WriteAllText(tempFile.Path, contents);

				string shortNameFullPath = FileSystemUtils.GetShortName(tempFile.Path);

				string shortNameFileName = Path.GetFileName(shortNameFullPath);
				Assert.AreEqual(12, shortNameFileName.Length);

				string shortNameExtension = Path.GetExtension(shortNameFullPath);
				Assert.IsTrue(extension.Equals(shortNameExtension, StringComparison.OrdinalIgnoreCase));

				Assert.AreEqual(contents, File.ReadAllText(shortNameFullPath));
			}
		}
	}
}
