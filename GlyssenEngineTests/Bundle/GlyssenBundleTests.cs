using System;
using System.IO;
using Glyssen.Shared;
using GlyssenEngine;
using GlyssenEngine.Bundle;
using SIL.DblBundle.Tests.Text;
using SIL.IO;

namespace GlyssenEngineTests.Bundle
{
	internal static class GlyssenBundleTests
	{
		public const string kTestBundleIdPrefix = "test~~ProjectTests";

		static GlyssenBundleTests()
		{
			GlyssenInfo.Product = "GlyssenTests";
			if (Project.FontRepository == null)
				Project.FontRepository = new TestProject.TestFontRepository();
		}

		private static string GetUniqueBundleId()
		{
			return kTestBundleIdPrefix + Path.GetFileNameWithoutExtension(Path.GetTempFileName());
		}

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
			bundle.Metadata.Language.Iso = bundle.Metadata.Id = GetUniqueBundleId();
			return new Tuple<GlyssenBundle, TempFile>(bundle, bundleFile);
		}
	}
}
