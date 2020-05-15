﻿using GlyssenEngine.Character;
using NUnit.Framework;

namespace GlyssenEngineTests.Character
{
	[TestFixture]
	class CharacterVerseDataTests
	{
		[TestCase("BC-GEN")]
		[TestCase("extra-MAT")]
		[TestCase("intro-REV")]
		[TestCase("intro-wow")] // Note: To be perfectly strict, we would check for a valid book code, but this method tends to be performance critical, so it's not worth it.
		public void IsCharacterExtraBiblical_ExtraBiblicalCharacter_ReturnsTrue(string characterId)
		{
			Assert.IsTrue(CharacterVerseData.IsCharacterExtraBiblical(characterId));
		}

		[TestCase("BCGEN")]
		[TestCase("extra-weird guy")]
		[TestCase("-extra-")]
		[TestCase("introduction of the new king")]
		[TestCase("Joseph")]
		public void IsCharacterExtraBiblical_NormalCharacter_ReturnsFalse(string characterId)
		{
			Assert.IsFalse(CharacterVerseData.IsCharacterExtraBiblical(characterId));
		}

		[TestCase("narrator-MAL")]
		[TestCase("BC-GEN")]
		[TestCase("extra-MAT")]
		[TestCase("intro-REV")]
		public void IsCharacterStandard_StandardCharacter_ReturnsTrue(string characterId)
		{
			Assert.IsTrue(CharacterVerseData.IsCharacterStandard(characterId));
		}

		[TestCase("Jesus")]
		[TestCase("narrator's friend")]
		[TestCase("extraordinary dude in MAT")]
		[TestCase("someone speaking in intro-REV")]
		[TestCase("intro-REVelation")]
		[TestCase("BC-")]
		[TestCase("extra-JOHN")]
		public void IsCharacterStandard_NonStandardCharacter_ReturnsFalse(string characterId)
		{
			Assert.IsFalse(CharacterVerseData.IsCharacterStandard(characterId));
		}


		// If IsCharacterStandard and/or IsCharacterExtraBiblical need to be tweaked, the following test can be used (and adjusted as needed)
		// to provide a good head-to-head comparison to make sure performance is optimized, since these methods are performance-critical.
		//[Test]
		//public void IsCharacterExtraBiblical_SpeedComparison_EnsureFastestVersion()
		//{
		//	var idsToTest = CharacterDetailData.Singleton.GetAll().Select(cd => cd.CharacterId).ToList();
		//	for (int i = 1; i <= BCVRef.LastBook; i++)
		//	{
		//		var bookId = BCVRef.NumberToBookCode(i);
		//		idsToTest.Add(CharacterVerseData.GetStandardCharacterId(bookId, CharacterVerseData.StandardCharacter.BookOrChapter));
		//		idsToTest.Add(CharacterVerseData.GetStandardCharacterId(bookId, CharacterVerseData.StandardCharacter.Narrator));
		//		idsToTest.Add(CharacterVerseData.GetStandardCharacterId(bookId, CharacterVerseData.StandardCharacter.ExtraBiblical));
		//		idsToTest.Add(CharacterVerseData.GetStandardCharacterId(bookId, CharacterVerseData.StandardCharacter.Intro));
		//	}

		//	long baselineTotal = 0;
		//	long proposedNewVersionTotal = 0;

		//	for (int a = 0; a < 12; a++)
		//	{
		//		GC.Collect();
		//		GC.WaitForPendingFinalizers();
		//		GC.Collect();

		//		var stopwatch = new Stopwatch();

		//		if (a % 2 == 0)
		//		{
		//			stopwatch.Start();
		//			for (int r = 0; r < 2000; r++)
		//			{
		//				foreach (var characterId in idsToTest)
		//					CharacterVerseData.IsCharacterExtraBiblical(characterId);
		//			}
		//			GC.Collect();
		//			GC.WaitForPendingFinalizers();
		//			GC.Collect();
		//			stopwatch.Stop();
		//			proposedNewVersionTotal += stopwatch.ElapsedTicks;
		//		}
		//		else
		//		{
		//			stopwatch.Start();
		//			for (int r = 0; r < 2000; r++)
		//			{
		//				foreach (var characterId in idsToTest)
		//					CharacterVerseData.IsCharacterExtraBiblicalBaselineVersion(characterId);
		//			}
		//			GC.Collect();
		//			GC.WaitForPendingFinalizers();
		//			GC.Collect();
		//			stopwatch.Stop();
		//			baselineTotal += stopwatch.ElapsedTicks;
		//		}
		//	}

		//	Assert.IsTrue(proposedNewVersionTotal < baselineTotal);
		//	Console.WriteLine($"Elapsed new proposed version ticks: {proposedNewVersionTotal}; Elapsed baseline version ticks: {baselineTotal}");
		//}
	}
}
