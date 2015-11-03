using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Glyssen.Bundle;
using NUnit.Framework;
using SIL.DblBundle.Tests.Text;
using SIL.IO;
using SIL.WritingSystems;

namespace GlyssenTests.Bundle
{
	class GlyssenBundleTests
	{
		public const string kTestBundleIdPrefix = "test~~ProjectTests";
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

		[Test]
		public void ConvertContinuersToParatextAssumptions_Level1Only_NoChange()
		{
			var bundle = GetNewGlyssenBundleForTest(true);
			bundle.WritingSystemDefinition.QuotationMarks.Clear();
			var quotationMarks = new List<QuotationMark>
			{
				new QuotationMark("<<", ">>", "<<", 1, QuotationMarkingSystemType.Normal)
			};
			bundle.WritingSystemDefinition.QuotationMarks.AddRange(quotationMarks);

			bundle.ConvertContinuersToParatextAssumptions();

			Assert.True(quotationMarks.SequenceEqual(bundle.WritingSystemDefinition.QuotationMarks));
		}

		[Test]
		public void ConvertContinuersToParatextAssumptions_2Levels_NoContinuer_NoChange()
		{
			var bundle = GetNewGlyssenBundleForTest(true);
			bundle.WritingSystemDefinition.QuotationMarks.Clear();
			var quotationMarks = new List<QuotationMark>
			{
				new QuotationMark("<<", ">>", null, 1, QuotationMarkingSystemType.Normal),
				new QuotationMark("<", ">", null, 2, QuotationMarkingSystemType.Normal)
			};
			bundle.WritingSystemDefinition.QuotationMarks.AddRange(quotationMarks);

			bundle.ConvertContinuersToParatextAssumptions();

			Assert.True(quotationMarks.SequenceEqual(bundle.WritingSystemDefinition.QuotationMarks));
		}

		[Test]
		public void ConvertContinuersToParatextAssumptions_2Levels_Continuer_ModifiesLevel2Continuer()
		{
			var bundle = GetNewGlyssenBundleForTest(true);
			bundle.WritingSystemDefinition.QuotationMarks.Clear();
			var quotationMarks = new List<QuotationMark>
			{
				new QuotationMark("<<", ">>", "<<", 1, QuotationMarkingSystemType.Normal),
				new QuotationMark("<", ">", "<", 2, QuotationMarkingSystemType.Normal)
			};
			bundle.WritingSystemDefinition.QuotationMarks.AddRange(quotationMarks);

			bundle.ConvertContinuersToParatextAssumptions();

			var expected = new List<QuotationMark>
			{
				new QuotationMark("<<", ">>", "<<", 1, QuotationMarkingSystemType.Normal),
				new QuotationMark("<", ">", "<< <", 2, QuotationMarkingSystemType.Normal)
			};

			Assert.True(expected.SequenceEqual(bundle.WritingSystemDefinition.QuotationMarks));
		}

		[Test]
		public void ConvertContinuersToParatextAssumptions_3Levels_Continuer_ModifiesLevel2AndLevel3Continuers()
		{
			var bundle = GetNewGlyssenBundleForTest(true);
			bundle.WritingSystemDefinition.QuotationMarks.Clear();
			var quotationMarks = new List<QuotationMark>
			{
				new QuotationMark("<<", ">>", "<<", 1, QuotationMarkingSystemType.Normal),
				new QuotationMark("<", ">", "<", 2, QuotationMarkingSystemType.Normal),
				new QuotationMark("<<", ">>", "<<", 3, QuotationMarkingSystemType.Normal)
			};
			bundle.WritingSystemDefinition.QuotationMarks.AddRange(quotationMarks);

			bundle.ConvertContinuersToParatextAssumptions();

			var expected = new List<QuotationMark>
			{
				new QuotationMark("<<", ">>", "<<", 1, QuotationMarkingSystemType.Normal),
				new QuotationMark("<", ">", "<< <", 2, QuotationMarkingSystemType.Normal),
				new QuotationMark("<<", ">>", "<< < <<", 3, QuotationMarkingSystemType.Normal)
			};

			Assert.True(expected.SequenceEqual(bundle.WritingSystemDefinition.QuotationMarks));
		}

		[Test]
		public void ConvertContinuersToParatextAssumptions_Level1NormalAndNarrative_NoChange()
		{
			var bundle = GetNewGlyssenBundleForTest(true);
			bundle.WritingSystemDefinition.QuotationMarks.Clear();
			var quotationMarks = new List<QuotationMark>
			{
				new QuotationMark("<<", ">>", "<<", 1, QuotationMarkingSystemType.Normal),
				new QuotationMark("--", null, null, 1, QuotationMarkingSystemType.Narrative)
			};
			bundle.WritingSystemDefinition.QuotationMarks.AddRange(quotationMarks);

			bundle.ConvertContinuersToParatextAssumptions();

			Assert.True(quotationMarks.SequenceEqual(bundle.WritingSystemDefinition.QuotationMarks));
		}

		[Test]
		public void ConvertContinuersToParatextAssumptions_3LevelsPlusNarrative_Continuer_ModifiesLevel2AndLevel3Continuers()
		{
			var bundle = GetNewGlyssenBundleForTest(true);
			bundle.WritingSystemDefinition.QuotationMarks.Clear();
			var quotationMarks = new List<QuotationMark>
			{
				new QuotationMark("<<", ">>", "<<", 1, QuotationMarkingSystemType.Normal),
				new QuotationMark("<", ">", "<", 2, QuotationMarkingSystemType.Normal),
				new QuotationMark("<<", ">>", "<<", 3, QuotationMarkingSystemType.Normal),
				new QuotationMark("--", null, null, 1, QuotationMarkingSystemType.Narrative)
			};
			bundle.WritingSystemDefinition.QuotationMarks.AddRange(quotationMarks);

			bundle.ConvertContinuersToParatextAssumptions();

			var expected = new List<QuotationMark>
			{
				new QuotationMark("<<", ">>", "<<", 1, QuotationMarkingSystemType.Normal),
				new QuotationMark("<", ">", "<< <", 2, QuotationMarkingSystemType.Normal),
				new QuotationMark("<<", ">>", "<< < <<", 3, QuotationMarkingSystemType.Normal),
				new QuotationMark("--", null, null, 1, QuotationMarkingSystemType.Narrative)
			};

			Assert.True(expected.SequenceEqual(bundle.WritingSystemDefinition.QuotationMarks));
		}
	}
}
