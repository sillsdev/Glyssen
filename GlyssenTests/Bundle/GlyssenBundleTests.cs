using System;
using Glyssen.Bundle;
using SIL.DblBundle.Tests.Text;
using SIL.IO;

namespace GlyssenTests.Bundle
{
	class GlyssenBundleTests
	{
		public static Tuple<GlyssenBundle, TempFile> GetNewGlyssenBundleAndFile()
		{
			return GetNewGlyssenBundleFromFile();
		}

		public static GlyssenBundle GetNewGlyssenBundleForTest(bool includeLdml = true)
		{
			var bundleAndFile = GetNewGlyssenBundleFromFile(includeLdml);
			bundleAndFile.Item2.Dispose();
			return bundleAndFile.Item1;
		}

		private static Tuple<GlyssenBundle, TempFile> GetNewGlyssenBundleFromFile(bool includeLdml = true)
		{
			var bundleFile = TextBundleTests.CreateZippedTextBundleFromResources(includeLdml);
			var bundle = new GlyssenBundle(bundleFile.Path);
			bundle.Metadata.Language.Iso = ProjectTests.kTest;
			bundle.Metadata.Id = ProjectTests.kTest;
			return new Tuple<GlyssenBundle, TempFile>(bundle, bundleFile);
		}
	}
}
