using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Glyssen.Dialogs;
using GlyssenEngine;
using GlyssenEngine.Bundle;
using GlyssenEngine.Character;
using GlyssenEngine.Rules;
using GlyssenEngine.ViewModels;
using GlyssenEngineTests;
using GlyssenFileBasedPersistence;
using L10NSharp;
using NUnit.Framework;
using SIL.Scripture;
using SIL.TestUtilities;
using SIL.WritingSystems;

namespace GlyssenTests.Dialogs
{
	/// <summary>
	/// This class and the one below are designed to help the developer calculate and set the minimum
	/// cast sizes needed for each book in the Bible. For the test to run properly, you will need
	/// projects set up at
	/// C:\ProgramData\FCBH-SIL\Glyssen\ach\3b9fdc679b9319c3\Acholi New Test 1985 Audio\ach.glyssen
	/// and
	/// C:\ProgramData\FCBH-SIL\Glyssen\cuk\5a6b88fafe1c8f2b\The Bible in Kuna, San Blas Audio (1)\cuk.glyssen
	///
	/// There are instructions pumped out to the unit test results console telling you what to do with the results.
	///
	/// AFAIK, test failures should not be considered problems with the code but rather problems with the data.
	/// </summary>
	[Category("SkipOnTeamCity")] // These tests rely on local files that the developer has to put in place.
	[TestFixture]
	[OfflineSldr]
	class CalculateMinimumCastSizesForNewTestamentBasedOnAcholi
	{
		private Project m_project;
		private readonly ConcurrentDictionary<string, CastSizeRowValues> m_results = new ConcurrentDictionary<string, CastSizeRowValues>();

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			// Use the real version of the file because we want the results to be based on the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = null;
			CharacterDetailData.TabDelimitedCharacterDetailData = null;

			m_project =
				Project.Load(ProjectRepository.LoadProject(@"C:\ProgramData\FCBH-SIL\Glyssen\ach\3b9fdc679b9319c3\Acholi New Test 1985 Audio\ach.glyssen"), null, null);
			TestProject.SimulateDisambiguationForAllBooks(m_project);
			m_project.CharacterGroupGenerationPreferences.NarratorsOption = NarratorsOption.SingleNarrator;
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			var ntBooks = SilBooks.Codes_3Letter.Skip(39).ToArray();

			if (m_results.Count == 27 || m_results.Count == 1)
			{
				foreach (var bookCode in ntBooks)
				{
					CastSizeRowValues validCast;
					if (m_results.TryGetValue(bookCode, out validCast))
						Debug.WriteLine("[TestCase(\"" + bookCode + "\", " + (validCast.Male - 1) + ")]");
				}
				Debug.WriteLine("****************");
			}
			else
			{
				Debug.WriteLine("WARNING: not all NT books are included in these results!!!!!!!!!!!");
			}

			Debug.WriteLine("Copy and paste the following into the CastSizePlanningViewModel constructor:");
			Debug.WriteLine("");

			foreach (var bookCode in ntBooks)
			{
				CastSizeRowValues validCast;
				if (m_results.TryGetValue(bookCode, out validCast))
				{
					Debug.WriteLine("case \"" + bookCode + "\":");
					if (bookCode == "HEB")
					{
						Debug.WriteLine("switch (m_project.DramatizationPreferences.ScriptureQuotationsShouldBeSpokenBy)");
						Debug.WriteLine("{");
						Debug.WriteLine("\tcase DramatizationOption.DedicatedCharacter:");
						Debug.WriteLine("\t\tsmallCast.Male = Math.Max(smallCast.Male, 1);");
						Debug.WriteLine("\t\tbreak;");
						Debug.WriteLine("\tcase DramatizationOption.DefaultCharacter:");
						Debug.WriteLine("\t\tsmallCast.Male = Math.Max(smallCast.Male, 4);");
						Debug.WriteLine("\t\tbreak;");
						Debug.WriteLine("\tcase DramatizationOption.Narrator:");
						Debug.WriteLine("\t\tsmallCast.Male = Math.Max(smallCast.Male, 0);");
						Debug.WriteLine("\t\tbreak;");
						Debug.WriteLine("}");
					}
					else
						Debug.WriteLine("smallCast.Male = Math.Max(smallCast.Male, " + (validCast.Male - 2) + ");");
					if (validCast.Female != 2)
						Debug.WriteLine("smallCast.Female = " + validCast.Female + ";");
					Debug.WriteLine("break;");
				}
			}
		}

		[Explicit] // This is a utility disguised as a unit test. Only run it by hand.
		[TestCase("MAT", 6)]
		[TestCase("MRK", 6)]
		[TestCase("LUK", 7)]
		[TestCase("JHN", 9)]
		[TestCase("ACT", 8)]
		[TestCase("ROM", 3)]
		[TestCase("1CO", 3)]
		[TestCase("2CO", 2)]
		[TestCase("GAL", 2)]
		[TestCase("EPH", 1)]
		[TestCase("PHP", 1)]
		[TestCase("COL", 1)]
		[TestCase("1TH", 1)]
		[TestCase("2TH", 1)]
		[TestCase("1TI", 1)]
		[TestCase("2TI", 1)]
		[TestCase("TIT", 1)]
		[TestCase("PHM", 1)]
		[TestCase("HEB", 1)]
		[TestCase("JAS", 3)]
		[TestCase("1PE", 1)]
		[TestCase("2PE", 2)]
		[TestCase("1JN", 1)]
		[TestCase("2JN", 1)]
		[TestCase("3JN", 1)]
		[TestCase("JUD", 3)]
		[TestCase("REV", 8)]
		public void UtilityToCalculateMinimumCastSizesForAcholi_ThisIsNotARealUnitTest(string bookCode, int initialGuess)
		{
			foreach (var book in m_project.AvailableBooks)
			{
				book.IncludeInScript = book.Code == bookCode;
			}
			m_project.ClearCharacterStatistics();

			CastSizeRowValues validCast = new CastSizeRowValues(initialGuess + 2, 2, 1);
			var currentCast = new CastSizeRowValues(initialGuess + 1, 2, 1);
			List<CharacterGroup> groups;
			do
			{
				var gen = new CharacterGroupGenerator(m_project, currentCast);

				groups = gen.GenerateCharacterGroups(true);
				if (groups != null)
					validCast.Male = currentCast.Male;
				currentCast.Male--;

			} while (groups != null && currentCast.Male >= 2);
			Assert.IsTrue(validCast.Male <= initialGuess + 1);
			m_results[bookCode] = validCast;
		}
	}

	[Category("SkipOnTeamCity")] // These tests rely on local files that the developer has to put in place.
	[TestFixture]
	[OfflineSldr]
	class CalculateMinimumCastSizesForOldTestamentBasedOnKunaSanBlas
	{
		private Project m_project;
		private readonly ConcurrentDictionary<string, CastSizeRowValues> m_results = new ConcurrentDictionary<string, CastSizeRowValues>();

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			// Use the real version of the file because we want the results to be based on the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = null;
			CharacterDetailData.TabDelimitedCharacterDetailData = null;

			m_project = Project.Load(ProjectRepository.LoadProject(
					@"C:\ProgramData\FCBH-SIL\Glyssen\cuk\5a6b88fafe1c8f2b\The Bible in Kuna, San Blas Audio\cuk.glyssen"),
				HandleMissingBundleNeededForProjectUpgrade, null);
			TestProject.SimulateDisambiguationForAllBooks(m_project);
			m_project.CharacterGroupGenerationPreferences.NarratorsOption = NarratorsOption.SingleNarrator;
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			var otBooks = SilBooks.Codes_3Letter.Take(39).ToArray();

			if (m_results.Count == 39 || m_results.Count == 1)
			{
				foreach (var bookCode in otBooks)
				{
					CastSizeRowValues validCast;
					if (m_results.TryGetValue(bookCode, out validCast))
						Debug.WriteLine("[TestCase(\"" + bookCode + "\", " + (validCast.Male - 1) + ")]");
				}
				Debug.WriteLine("****************");
			}
			else
			{
				Debug.WriteLine("WARNING: not all OT books are included in these results!!!!!!!!!!!");
			}

			Debug.WriteLine("Copy and paste the following into the CastSizePlanningViewModel constructor:");
			Debug.WriteLine("");

			foreach (var bookCode in otBooks)
			{
				CastSizeRowValues validCast;
				if (m_results.TryGetValue(bookCode, out validCast))
				{
					Debug.WriteLine("case \"" + bookCode + "\":");
					Debug.WriteLine("smallCast.Male = Math.Max(smallCast.Male, " + (validCast.Male - 2) + ");");
					if (validCast.Female != 2)
						Debug.WriteLine("smallCast.Female = " + validCast.Female + ";");
					Debug.WriteLine("break;");
				}
			}
		}

		private bool HandleMissingBundleNeededForProjectUpgrade(Project existingProject)
		{
			string msg = "The parser has been upgraded " +
					$"To make use of the new engine, the original text release bundle must be available, but it is not in the original location ({existingProject.OriginalBundlePath})." +
					Environment.NewLine +
					"Would you like to locate the text release bundle yourself?";
			string caption = LocalizationManager.GetString("Project.UnableToLocateTextBundle", "Unable to Locate Text Bundle");
			if (DialogResult.Yes == MessageBox.Show(msg, caption, MessageBoxButtons.YesNo))
				return SelectBundleForProjectDlg.GiveUserChanceToFindOriginalBundle(existingProject);
			return false;
		}

		[Explicit] // This is a utility disguised as a unit test. Only run it by hand.
		[TestCase("GEN", 7)]
		[TestCase("EXO", 4)]
		[TestCase("LEV", 3)]
		[TestCase("NUM", 5)]
		[TestCase("DEU", 2)]
		[TestCase("JOS", 6)]
		[TestCase("JDG", 6)]
		[TestCase("RUT", 2)]
		[TestCase("1SA", 7)]
		[TestCase("2SA", 7)]
		[TestCase("1KI", 7)]
		[TestCase("2KI", 7)]
		[TestCase("1CH", 4)]
		[TestCase("2CH", 7)]
		[TestCase("EZR", 3)]
		[TestCase("NEH", 5)]
		[TestCase("EST", 3)]
		[TestCase("JOB", 7)]
		[TestCase("PSA", 4)]
		[TestCase("PRO", 2)]
		[TestCase("ECC", 1)]
		[TestCase("SNG", 3)]
		[TestCase("ISA", 5)]
		[TestCase("JER", 5)]
		[TestCase("LAM", 2)]
		[TestCase("EZK", 3)]
		[TestCase("DAN", 6)]
		[TestCase("HOS", 2)]
		[TestCase("JOL", 2)]
		[TestCase("AMO", 4)]
		[TestCase("OBA", 1)]
		[TestCase("JON", 4)]
		[TestCase("MIC", 3)]
		[TestCase("NAM", 1)]
		[TestCase("HAB", 1)]
		[TestCase("ZEP", 1)]
		[TestCase("HAG", 2)]
		[TestCase("ZEC", 5)]
		[TestCase("MAL", 2)]
		public void UtilityToCalculateMinimumCastSizesForKuna_ThisIsNotARealUnitTest(string bookCode, int initialGuess)
		{
			if (bookCode == "SNG")
			{
				// Song of Solomon is special because all the speech is "Implicit" (which we don't handle properly yet)
				m_results[bookCode] = new CastSizeRowValues(4, 2, 0);
				return;
			}

			foreach (var book in m_project.AvailableBooks)
				book.IncludeInScript = book.Code == bookCode;
			m_project.IncludedBooks.Single().SingleVoice = false;

			m_project.ClearCharacterStatistics();

			var women = (bookCode == "RUT") ? 4 : 2;
			if (bookCode == "GEN" || bookCode == "EXO")
				women = 3;
			CastSizeRowValues validCast = new CastSizeRowValues(initialGuess + 2, women, 1);
			var currentCast = new CastSizeRowValues(initialGuess + 1, women, 1);
			List<CharacterGroup> groups = null;
			do
			{
				var gen = new CharacterGroupGenerator(m_project, currentCast);

				groups = gen.GenerateCharacterGroups(true);
				if (groups != null)
					validCast.Male = currentCast.Male;
				currentCast.Male--;

			} while (groups != null && currentCast.Male >= 2);
			Assert.IsTrue(validCast.Male <= initialGuess + 1);
			m_results[bookCode] = validCast;
		}
	}
}
