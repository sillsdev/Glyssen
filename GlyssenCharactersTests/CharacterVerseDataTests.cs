using GlyssenCharacters;
using NUnit.Framework;

namespace GlyssenCharactersTests
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
			Assert.That(CharacterVerseData.IsCharacterExtraBiblical(characterId), Is.True);
		}

		[TestCase("BCGEN")]
		[TestCase("extra-weird guy")]
		[TestCase("-extra-")]
		[TestCase("introduction of the new king")]
		[TestCase("Joseph")]
		public void IsCharacterExtraBiblical_NormalCharacter_ReturnsFalse(string characterId)
		{
			Assert.That(CharacterVerseData.IsCharacterExtraBiblical(characterId), Is.False);
		}

		[TestCase("narrator-MAL")]
		[TestCase("BC-GEN")]
		[TestCase("extra-MAT")]
		[TestCase("intro-REV")]
		public void IsCharacterStandard_StandardCharacter_ReturnsTrue(string characterId)
		{
			Assert.That(CharacterVerseData.IsCharacterStandard(characterId), Is.True);
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
			Assert.That(CharacterVerseData.IsCharacterStandard(characterId), Is.False);
		}

		[TestCase("intro-REV")]
		[TestCase("BC-MAT")]
		[TestCase("extra-JHN")]
		[TestCase("interruption-JHN")]
		public void IsUserAssignable_ExtraBiblicalOrInterruption_ReturnsFalse(string characterId)
		{
			Assert.That(CharacterVerseData.IsUserAssignable(characterId), Is.False);
		}

		[TestCase("intro-REVELATION")]
		[TestCase("BC-matthew")]
		[TestCase("extraJHN")]
		[TestCase("interruption")]
		[TestCase("interruption-")]
		[TestCase("interruption-Whatever")]
		[TestCase("frog man")]
		[TestCase("narrator-JHN")]
		public void IsUserAssignable_NotExtraBiblicalOrInterruption_ReturnsTrue(string characterId)
		{
			Assert.That(CharacterVerseData.IsUserAssignable(characterId), Is.True);
		}

		[TestCase("interruption-MAT")]
		[TestCase("interruption-JHN")]
		[TestCase("interruption-FRG")]
		public void IsInterruption_Interruption_ReturnsTrue(string characterId)
		{
			Assert.That(CharacterVerseData.IsInterruption(characterId), Is.True);
		}

		[TestCase("intro-REV")]
		[TestCase("BC-MAT")]
		[TestCase("extra-JHN")]
		[TestCase("interruption")]
		[TestCase("interruption-")]
		[TestCase("interruption-Whatever")]
		[TestCase("frog man")]
		[TestCase("narrator-JHN")]
		public void IsInterruption_NotInterruption_ReturnsFalse(string characterId)
		{
			Assert.That(CharacterVerseData.IsInterruption(characterId), Is.False);
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

		//	Assert.That(proposedNewVersionTotal < baselineTotal, Is.True);
		//	Console.WriteLine($"Elapsed new proposed version ticks: {proposedNewVersionTotal}; Elapsed baseline version ticks: {baselineTotal}");
		//}
	}
}
