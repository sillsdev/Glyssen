using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Glyssen;
using Glyssen.Bundle;
using Glyssen.Character;
using Glyssen.Rules;
using Glyssen.Shared.Bundle;
using Glyssen.VoiceActor;
using GlyssenTests.Properties;
using NUnit.Framework;
using SIL.Extensions;

namespace GlyssenTests.Rules
{
	[TestFixture]
	class TrialGroupConfigurationTestsSmall : CharacterGroupGeneratorAndAdjusterTestBase
	{
		internal static List<CharacterGroup> GetNarratorCharacterGroups(int n)
		{
			var narratorGroups = new List<CharacterGroup>(n);
			for (int i = 0; i < n; i++)
			{
				narratorGroups.Add(new CharacterGroup { GroupIdLabel = CharacterGroup.Label.Narrator, GroupIdNumber = i + 1 });
			}
			return narratorGroups;
		}

		[Test]
		public void DistributeBooksAmongNarratorGroups_FourAuthorsOfFourBooksAmongThreeNarrators_TwoAuthorsWithShortestBooksCombined()
		{
			var keyStrokesByBook = new Dictionary<string, int>();
			keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("JER", CharacterVerseData.StandardCharacter.Narrator)] = 52000;
			keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("EZK", CharacterVerseData.StandardCharacter.Narrator)] = 48000;
			keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("HOS", CharacterVerseData.StandardCharacter.Narrator)] = 12000;
			keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.Narrator)] = 1000;
			var authorStats = new List<AuthorStats>();
			authorStats.Add(new AuthorStats(BiblicalAuthors.GetAuthorOfBook("JER"), keyStrokesByBook, "JER"));
			authorStats.Add(new AuthorStats(BiblicalAuthors.GetAuthorOfBook("EZK"), keyStrokesByBook, "EZK"));
			authorStats.Add(new AuthorStats(BiblicalAuthors.GetAuthorOfBook("HOS"), keyStrokesByBook, "HOS"));
			authorStats.Add(new AuthorStats(BiblicalAuthors.GetAuthorOfBook("JUD"), keyStrokesByBook, "JUD"));

			var narratorGroups = GetNarratorCharacterGroups(3);

			CharacterGroupGenerator.TrialGroupConfiguration.DistributeBooksAmongNarratorGroups(authorStats, narratorGroups);
			var groupForJeremiah = GetNarratorGroupForBook(narratorGroups, "JER");
			var groupForJude = GetNarratorGroupForBook(narratorGroups, "JUD");
			Assert.AreNotEqual(groupForJeremiah, GetNarratorGroupForBook(narratorGroups, "EZK"));
			Assert.AreNotEqual(groupForJeremiah, groupForJude);
			Assert.AreEqual(groupForJude, GetNarratorGroupForBook(narratorGroups, "HOS"));
		}

		[Test]
		public void DistributeBooksAmongNarratorGroups_SixSimilarAuthorsOfEightBooksAmongThreeNarrators_AuthorOfLargestBookCombinesWithShortestEtc()
		{
			var keyStrokesByBook = new Dictionary<string, int>();
			keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("GEN", CharacterVerseData.StandardCharacter.Narrator)] = 50000;
			keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("ISA", CharacterVerseData.StandardCharacter.Narrator)] = 66000;
			keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("JER", CharacterVerseData.StandardCharacter.Narrator)] = 52000;
			keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("EZK", CharacterVerseData.StandardCharacter.Narrator)] = 48000;
			keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("LUK", CharacterVerseData.StandardCharacter.Narrator)] = 24000; // 52000 combined
			keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("ACT", CharacterVerseData.StandardCharacter.Narrator)] = 28000;
			keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("JHN", CharacterVerseData.StandardCharacter.Narrator)] = 20000; // 42000 combined
			keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("REV", CharacterVerseData.StandardCharacter.Narrator)] = 22000;
			var authorStats = new List<AuthorStats>();

			authorStats.Add(new AuthorStats(BiblicalAuthors.GetAuthorOfBook("GEN"), keyStrokesByBook, "GEN"));
			authorStats.Add(new AuthorStats(BiblicalAuthors.GetAuthorOfBook("ISA"), keyStrokesByBook, "ISA"));
			authorStats.Add(new AuthorStats(BiblicalAuthors.GetAuthorOfBook("JER"), keyStrokesByBook, "JER"));
			authorStats.Add(new AuthorStats(BiblicalAuthors.GetAuthorOfBook("EZK"), keyStrokesByBook, "EZK"));
			authorStats.Add(new AuthorStats(BiblicalAuthors.GetAuthorOfBook("LUK"), keyStrokesByBook, "LUK", "ACT"));
			authorStats.Add(new AuthorStats(BiblicalAuthors.GetAuthorOfBook("JHN"), keyStrokesByBook, "JHN", "REV"));

			var narratorGroups = GetNarratorCharacterGroups(3);

			CharacterGroupGenerator.TrialGroupConfiguration.DistributeBooksAmongNarratorGroups(authorStats, narratorGroups);

			// Since there are two authors with exactly 52000 keystrokes, we can't know for sure which one will combine with GEN and
			// which will combine with EZK. So we just assert that they are grouped properly.
			Assert.AreNotEqual(GetNarratorGroupForBook(narratorGroups, "ISA"), GetNarratorGroupForBook(narratorGroups, "JER"));
			Assert.AreNotEqual(GetNarratorGroupForBook(narratorGroups, "ISA"), GetNarratorGroupForBook(narratorGroups, "EZK"));
			Assert.AreNotEqual(GetNarratorGroupForBook(narratorGroups, "ISA"), GetNarratorGroupForBook(narratorGroups, "GEN"));
			Assert.AreNotEqual(GetNarratorGroupForBook(narratorGroups, "GEN"), GetNarratorGroupForBook(narratorGroups, "EZK"));
			Assert.AreEqual(GetNarratorGroupForBook(narratorGroups, "ISA"), GetNarratorGroupForBook(narratorGroups, "JHN"));
			Assert.AreEqual(GetNarratorGroupForBook(narratorGroups, "LUK"), GetNarratorGroupForBook(narratorGroups, "ACT"));
			Assert.AreEqual(GetNarratorGroupForBook(narratorGroups, "JHN"), GetNarratorGroupForBook(narratorGroups, "REV"));
			Assert.AreEqual(3, narratorGroups.Single(g => g.GroupId == "Narrator1").CharacterIds.Count);
			Assert.AreEqual(3, narratorGroups.Single(g => g.GroupId == "Narrator2").CharacterIds.Count);
			Assert.AreEqual(2, narratorGroups.Single(g => g.GroupId == "Narrator3").CharacterIds.Count);
		}

		[Test]
		public void DistributeBooksAmongNarratorGroups_FourAuthorsOfSixBooksAmongFiveNarrators_AuthorWithLongestBooksSplit()
		{
			var keyStrokesByBook = new Dictionary<string, int>();
			keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("GEN", CharacterVerseData.StandardCharacter.Narrator)] = 50000;
			keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("EXO", CharacterVerseData.StandardCharacter.Narrator)] = 28000;
			keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("JER", CharacterVerseData.StandardCharacter.Narrator)] = 52000;
			keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("LAM", CharacterVerseData.StandardCharacter.Narrator)] = 6000;
			keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("HOS", CharacterVerseData.StandardCharacter.Narrator)] = 12000;
			keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.Narrator)] = 1000;

			var narratorGroups = GetNarratorCharacterGroups(5);

			CharacterGroupGenerator.TrialGroupConfiguration.DistributeBooksAmongNarratorGroups(narratorGroups, 4,
				keyStrokesByBook.Keys.Select(CharacterVerseData.GetBookCodeFromStandardCharacterId), keyStrokesByBook);
			var narratorGroupForJeremiah = GetNarratorGroupForBook(narratorGroups, "JER");
			Assert.AreEqual(narratorGroupForJeremiah, GetNarratorGroupForBook(narratorGroups, "LAM"));
			var listOfBooksFoundSoFar = new HashSet<string>();
			foreach (var group in narratorGroups)
			{
				var booksAssignedToNarrator = group.CharacterIds.Select(CharacterVerseData.GetBookCodeFromStandardCharacterId).ToList();
				if (group != narratorGroupForJeremiah)
					Assert.AreEqual(1, booksAssignedToNarrator.Count);
				Assert.IsFalse(listOfBooksFoundSoFar.Overlaps(booksAssignedToNarrator));
				listOfBooksFoundSoFar.AddRange(booksAssignedToNarrator);
			}
		}

		[Test]
		public void DistributeBooksAmongNarratorGroups_TwoAuthorsOfSixBooksAmongFiveNarrators_AuthorWithLongestBooksSplit()
		{
			var keyStrokesByBook = new Dictionary<string, int>();
			keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("GEN", CharacterVerseData.StandardCharacter.Narrator)] = 50000;
			keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("EXO", CharacterVerseData.StandardCharacter.Narrator)] = 28000;
			keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("LEV", CharacterVerseData.StandardCharacter.Narrator)] = 28000;
			keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("DEU", CharacterVerseData.StandardCharacter.Narrator)] = 28000;
			keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("JHN", CharacterVerseData.StandardCharacter.Narrator)] = 20000;
			keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("REV", CharacterVerseData.StandardCharacter.Narrator)] = 22000;

			var narratorGroups = GetNarratorCharacterGroups(5);

			CharacterGroupGenerator.TrialGroupConfiguration.DistributeBooksAmongNarratorGroups(narratorGroups, 2,
				keyStrokesByBook.Keys.Select(CharacterVerseData.GetBookCodeFromStandardCharacterId), keyStrokesByBook);
			var narratorGroupForJohn = GetNarratorGroupForBook(narratorGroups, "JHN");
			Assert.AreEqual(narratorGroupForJohn, GetNarratorGroupForBook(narratorGroups, "REV"));
			var listOfBooksFoundSoFar = new HashSet<string>();
			foreach (var group in narratorGroups)
			{
				var booksAssignedToNarrator = group.CharacterIds.Select(CharacterVerseData.GetBookCodeFromStandardCharacterId).ToList();
				if (group != narratorGroupForJohn)
					Assert.AreEqual(1, booksAssignedToNarrator.Count);
				Assert.IsFalse(listOfBooksFoundSoFar.Overlaps(booksAssignedToNarrator));
				listOfBooksFoundSoFar.AddRange(booksAssignedToNarrator);
			}
		}
	}

	[TestFixture]
	internal class TrialGroupConfigurationTestsWholeBible : CharacterGroupGeneratorAndAdjusterTestBase
	{
		private Dictionary<string, int> m_keyStrokesByBook;
		private List<AuthorStats> m_authorStats;
		private BiblicalAuthors.Author m_moses;
		private BiblicalAuthors.Author m_joshua;
		private BiblicalAuthors.Author m_judges;
		private BiblicalAuthors.Author m_ruth;
		private BiblicalAuthors.Author m_samuel;
		private BiblicalAuthors.Author m_kings;
		private BiblicalAuthors.Author m_chronicles;
		private BiblicalAuthors.Author m_ezra;
		private BiblicalAuthors.Author m_nehemiah;
		private BiblicalAuthors.Author m_esther;
		private BiblicalAuthors.Author m_job;
		private BiblicalAuthors.Author m_psalms;
		private BiblicalAuthors.Author m_solomon;
		private BiblicalAuthors.Author m_isaiah;
		private BiblicalAuthors.Author m_jeremiah;
		private BiblicalAuthors.Author m_ezekiel;
		private BiblicalAuthors.Author m_daniel;
		private BiblicalAuthors.Author m_hosea;
		private BiblicalAuthors.Author m_joel;
		private BiblicalAuthors.Author m_amos;
		private BiblicalAuthors.Author m_obadiah;
		private BiblicalAuthors.Author m_jonah;
		private BiblicalAuthors.Author m_micah;
		private BiblicalAuthors.Author m_nahum;
		private BiblicalAuthors.Author m_habakkuk;
		private BiblicalAuthors.Author m_zephaniah;
		private BiblicalAuthors.Author m_haggai;
		private BiblicalAuthors.Author m_zechariah;
		private BiblicalAuthors.Author m_malachi;
		private BiblicalAuthors.Author m_matthew;
		private BiblicalAuthors.Author m_mark;
		private BiblicalAuthors.Author m_luke;
		private BiblicalAuthors.Author m_john;
		private BiblicalAuthors.Author m_paul;
		private BiblicalAuthors.Author m_hebrews;
		private BiblicalAuthors.Author m_james;
		private BiblicalAuthors.Author m_peter;
		private BiblicalAuthors.Author m_jude;


		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			m_keyStrokesByBook = new Dictionary<string, int>();
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("GEN", CharacterVerseData.StandardCharacter.Narrator)] = 50000; // MOSES
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("EXO", CharacterVerseData.StandardCharacter.Narrator)] = 40000; // MOSES
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("LEV", CharacterVerseData.StandardCharacter.Narrator)] = 27000; // MOSES
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("NUM", CharacterVerseData.StandardCharacter.Narrator)] = 36000; // MOSES
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("DEU", CharacterVerseData.StandardCharacter.Narrator)] = 34000; // MOSES:     187000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("JOS", CharacterVerseData.StandardCharacter.Narrator)] = 24000; // JOSHUA:     24000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("JDG", CharacterVerseData.StandardCharacter.Narrator)] = 21000; // JUDGES:     21000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("RUT", CharacterVerseData.StandardCharacter.Narrator)] = 4000; // RUTH:        4000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("1SA", CharacterVerseData.StandardCharacter.Narrator)] = 31000; // SAMUEL
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("2SA", CharacterVerseData.StandardCharacter.Narrator)] = 24000; // SAMUEL:     55000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("1KI", CharacterVerseData.StandardCharacter.Narrator)] = 24000; // KINGS
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("2KI", CharacterVerseData.StandardCharacter.Narrator)] = 25000; // KINGS:      49000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("1CH", CharacterVerseData.StandardCharacter.Narrator)] = 35000; // CHRONICLES
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("2CH", CharacterVerseData.StandardCharacter.Narrator)] = 36000; // CHRONICLES: 71000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("EZR", CharacterVerseData.StandardCharacter.Narrator)] = 10000; // EZRA:       10000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("NEH", CharacterVerseData.StandardCharacter.Narrator)] = 13000; // NEHEMIAH:   13000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("EST", CharacterVerseData.StandardCharacter.Narrator)] = 10000; // ESTHER:     10000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("JOB", CharacterVerseData.StandardCharacter.Narrator)] = 42000; // JOB:        42000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("PSA", CharacterVerseData.StandardCharacter.Narrator)] = 99999; // PSALMS:     99999
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("PRO", CharacterVerseData.StandardCharacter.Narrator)] = 31000; // SOLOMON
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("ECC", CharacterVerseData.StandardCharacter.Narrator)] = 12000; // SOLOMON
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("SNG", CharacterVerseData.StandardCharacter.Narrator)] = 8000; // SOLOMON:     51000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("ISA", CharacterVerseData.StandardCharacter.Narrator)] = 66000; // ISAIAH:     66000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("JER", CharacterVerseData.StandardCharacter.Narrator)] = 52000; // JEREMIAH
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("LAM", CharacterVerseData.StandardCharacter.Narrator)] = 5000; // JEREMIAH:    57000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("EZK", CharacterVerseData.StandardCharacter.Narrator)] = 48000; // EZEKIEL:    48000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("DAN", CharacterVerseData.StandardCharacter.Narrator)] = 12000; // DANIEL:     12000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("HOS", CharacterVerseData.StandardCharacter.Narrator)] = 14000; // HOSEA:      14000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("JOL", CharacterVerseData.StandardCharacter.Narrator)] = 3000; // JOEL:         3000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("AMO", CharacterVerseData.StandardCharacter.Narrator)] = 9000; // AMOS:         9000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("OBA", CharacterVerseData.StandardCharacter.Narrator)] = 1000; // OBADIAH:      1000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("JON", CharacterVerseData.StandardCharacter.Narrator)] = 4000; // JONAH:        4000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("MIC", CharacterVerseData.StandardCharacter.Narrator)] = 7000; // MICAH:        7000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("NAM", CharacterVerseData.StandardCharacter.Narrator)] = 3000; // NAHUM:        3000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("HAB", CharacterVerseData.StandardCharacter.Narrator)] = 3000; // HABAKKUK:     3000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("ZEP", CharacterVerseData.StandardCharacter.Narrator)] = 3000; // ZEPHANIAH:    3000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("HAG", CharacterVerseData.StandardCharacter.Narrator)] = 2000; // HAGGAI:       2000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("ZEC", CharacterVerseData.StandardCharacter.Narrator)] = 3000; // ZECHARIAH:    3000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("MAL", CharacterVerseData.StandardCharacter.Narrator)] = 4000; // MALACHI:      4000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator)] = 28000; // MATTHEW:    28000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.Narrator)] = 16000; // MARK:       16000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("LUK", CharacterVerseData.StandardCharacter.Narrator)] = 24000; // LUKE
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("ACT", CharacterVerseData.StandardCharacter.Narrator)] = 28000; // LUKE:       52000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("JHN", CharacterVerseData.StandardCharacter.Narrator)] = 20000; // JOHN
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("1JN", CharacterVerseData.StandardCharacter.Narrator)] = 5000; // JOHN
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("2JN", CharacterVerseData.StandardCharacter.Narrator)] = 1000; // JOHN
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("3JN", CharacterVerseData.StandardCharacter.Narrator)] = 1000; // JOHN
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("REV", CharacterVerseData.StandardCharacter.Narrator)] = 22000; // JOHN:       49000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("ROM", CharacterVerseData.StandardCharacter.Narrator)] = 16000; // PAUL
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("1CO", CharacterVerseData.StandardCharacter.Narrator)] = 16000; // PAUL
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("2CO", CharacterVerseData.StandardCharacter.Narrator)] = 13000; // PAUL
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("GAL", CharacterVerseData.StandardCharacter.Narrator)] = 6000; // PAUL
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("EPH", CharacterVerseData.StandardCharacter.Narrator)] = 6000; // PAUL
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("PHP", CharacterVerseData.StandardCharacter.Narrator)] = 4000; // PAUL
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("COL", CharacterVerseData.StandardCharacter.Narrator)] = 4000; // PAUL
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("1TH", CharacterVerseData.StandardCharacter.Narrator)] = 5000; // PAUL
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("2TH", CharacterVerseData.StandardCharacter.Narrator)] = 3000; // PAUL
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("1TI", CharacterVerseData.StandardCharacter.Narrator)] = 6000; // PAUL
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("2TI", CharacterVerseData.StandardCharacter.Narrator)] = 4000; // PAUL
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("TIT", CharacterVerseData.StandardCharacter.Narrator)] = 3000; // PAUL
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("PHM", CharacterVerseData.StandardCharacter.Narrator)] = 1000; // PAUL:        87000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("HEB", CharacterVerseData.StandardCharacter.Narrator)] = 13000; // HEBREWS:    13000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("JAS", CharacterVerseData.StandardCharacter.Narrator)] = 5000; // JAMES:        5000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("1PE", CharacterVerseData.StandardCharacter.Narrator)] = 5000; // PETER
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("2PE", CharacterVerseData.StandardCharacter.Narrator)] = 3000; // PETER:        8000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.Narrator)] = 1000; // JUDE:         1000

			m_authorStats = new List<AuthorStats>();

			m_moses = BiblicalAuthors.GetAuthorOfBook("GEN"); // Moses
			m_joshua = BiblicalAuthors.GetAuthorOfBook("JOS"); //Joshua
			m_judges = BiblicalAuthors.GetAuthorOfBook("JDG"); //Author of Judges
			m_ruth = BiblicalAuthors.GetAuthorOfBook("RUT"); //Author of Ruth
			m_samuel = BiblicalAuthors.GetAuthorOfBook("1SA"); //Samuel
			m_kings = BiblicalAuthors.GetAuthorOfBook("1KI"); //Author of Kings
			m_chronicles = BiblicalAuthors.GetAuthorOfBook("1CH"); //Author of Chronicles
			m_ezra = BiblicalAuthors.GetAuthorOfBook("EZR"); //Ezra
			m_nehemiah = BiblicalAuthors.GetAuthorOfBook("NEH"); //Nehemiah
			m_esther = BiblicalAuthors.GetAuthorOfBook("EST"); //Author of Esther
			m_job = BiblicalAuthors.GetAuthorOfBook("JOB"); //Job
			m_psalms = BiblicalAuthors.GetAuthorOfBook("PSA"); //Author of Psalms
			m_solomon = BiblicalAuthors.GetAuthorOfBook("PRO"); //Solomon
			m_isaiah = BiblicalAuthors.GetAuthorOfBook("ISA"); //Isaiah
			m_jeremiah = BiblicalAuthors.GetAuthorOfBook("JER"); //Jeremiah
			m_ezekiel = BiblicalAuthors.GetAuthorOfBook("EZK"); //Ezekiel
			m_daniel = BiblicalAuthors.GetAuthorOfBook("DAN"); //Daniel
			m_hosea = BiblicalAuthors.GetAuthorOfBook("HOS"); //Hosea
			m_joel = BiblicalAuthors.GetAuthorOfBook("JOL"); //Joel
			m_amos = BiblicalAuthors.GetAuthorOfBook("AMO"); //Amos
			m_obadiah = BiblicalAuthors.GetAuthorOfBook("OBA"); //Obadiah
			m_jonah = BiblicalAuthors.GetAuthorOfBook("JON"); //Jonah
			m_micah = BiblicalAuthors.GetAuthorOfBook("MIC"); //Micah
			m_nahum = BiblicalAuthors.GetAuthorOfBook("NAM"); //Nahum
			m_habakkuk = BiblicalAuthors.GetAuthorOfBook("HAB"); //Habakkuk
			m_zephaniah = BiblicalAuthors.GetAuthorOfBook("ZEP"); //Zephaniah
			m_haggai = BiblicalAuthors.GetAuthorOfBook("HAG"); //Haggai
			m_zechariah = BiblicalAuthors.GetAuthorOfBook("ZEC"); //Zechariah
			m_malachi = BiblicalAuthors.GetAuthorOfBook("MAL"); //Malachi
			m_matthew = BiblicalAuthors.GetAuthorOfBook("MAT"); //Matthew
			m_mark = BiblicalAuthors.GetAuthorOfBook("MRK"); //Mark
			m_luke = BiblicalAuthors.GetAuthorOfBook("LUK"); //Luke
			m_john = BiblicalAuthors.GetAuthorOfBook("JHN"); //John
			m_paul = BiblicalAuthors.GetAuthorOfBook("ROM"); //Paul
			m_hebrews = BiblicalAuthors.GetAuthorOfBook("HEB"); //Author of Hebrews
			m_james = BiblicalAuthors.GetAuthorOfBook("JAS"); //James
			m_peter = BiblicalAuthors.GetAuthorOfBook("1PE"); //Peter
			m_jude = BiblicalAuthors.GetAuthorOfBook("JUD"); //Jude

			m_authorStats.Add(new AuthorStats(m_moses, m_keyStrokesByBook, "GEN", "EXO", "LEV", "NUM", "DEU"));
			m_authorStats.Add(new AuthorStats(m_joshua, m_keyStrokesByBook, "JOS"));
			m_authorStats.Add(new AuthorStats(m_judges, m_keyStrokesByBook, "JDG"));
			m_authorStats.Add(new AuthorStats(m_ruth, m_keyStrokesByBook, "RUT"));
			m_authorStats.Add(new AuthorStats(m_samuel, m_keyStrokesByBook, "1SA", "2SA"));
			m_authorStats.Add(new AuthorStats(m_kings, m_keyStrokesByBook, "1KI", "2KI"));
			m_authorStats.Add(new AuthorStats(m_chronicles, m_keyStrokesByBook, "1CH", "2CH"));
			m_authorStats.Add(new AuthorStats(m_ezra, m_keyStrokesByBook, "EZR"));
			m_authorStats.Add(new AuthorStats(m_nehemiah, m_keyStrokesByBook, "NEH"));
			m_authorStats.Add(new AuthorStats(m_esther, m_keyStrokesByBook, "EST"));
			m_authorStats.Add(new AuthorStats(m_job, m_keyStrokesByBook, "JOB"));
			m_authorStats.Add(new AuthorStats(m_psalms, m_keyStrokesByBook, "PSA"));
			m_authorStats.Add(new AuthorStats(m_solomon, m_keyStrokesByBook, "PRO", "ECC", "SNG"));
			m_authorStats.Add(new AuthorStats(m_isaiah, m_keyStrokesByBook, "ISA"));
			m_authorStats.Add(new AuthorStats(m_jeremiah, m_keyStrokesByBook, "JER", "LAM"));
			m_authorStats.Add(new AuthorStats(m_ezekiel, m_keyStrokesByBook, "EZK"));
			m_authorStats.Add(new AuthorStats(m_daniel, m_keyStrokesByBook, "DAN"));
			m_authorStats.Add(new AuthorStats(m_hosea, m_keyStrokesByBook, "HOS"));
			m_authorStats.Add(new AuthorStats(m_joel, m_keyStrokesByBook, "JOL"));
			m_authorStats.Add(new AuthorStats(m_amos, m_keyStrokesByBook, "AMO"));
			m_authorStats.Add(new AuthorStats(m_obadiah, m_keyStrokesByBook, "OBA"));
			m_authorStats.Add(new AuthorStats(m_jonah, m_keyStrokesByBook, "JON"));
			m_authorStats.Add(new AuthorStats(m_micah, m_keyStrokesByBook, "MIC"));
			m_authorStats.Add(new AuthorStats(m_nahum, m_keyStrokesByBook, "NAM"));
			m_authorStats.Add(new AuthorStats(m_habakkuk, m_keyStrokesByBook, "HAB"));
			m_authorStats.Add(new AuthorStats(m_zephaniah, m_keyStrokesByBook, "ZEP"));
			m_authorStats.Add(new AuthorStats(m_haggai, m_keyStrokesByBook, "HAG"));
			m_authorStats.Add(new AuthorStats(m_zechariah, m_keyStrokesByBook, "ZEC"));
			m_authorStats.Add(new AuthorStats(m_malachi, m_keyStrokesByBook, "MAL"));
			m_authorStats.Add(new AuthorStats(m_matthew, m_keyStrokesByBook, "MAT"));
			m_authorStats.Add(new AuthorStats(m_mark, m_keyStrokesByBook, "MRK"));
			m_authorStats.Add(new AuthorStats(m_luke, m_keyStrokesByBook, "LUK", "ACT"));
			m_authorStats.Add(new AuthorStats(m_john, m_keyStrokesByBook, "JHN", "1JN", "2JN", "3JN", "REV"));
			m_authorStats.Add(new AuthorStats(m_paul, m_keyStrokesByBook, "ROM", "1CO", "2CO", "GAL", "EPH", "PHP", "COL", "1TH", "2TH", "1TI", "2TI", "TIT", "PHM"));
			m_authorStats.Add(new AuthorStats(m_hebrews, m_keyStrokesByBook, "HEB"));
			m_authorStats.Add(new AuthorStats(m_james, m_keyStrokesByBook, "JAS"));
			m_authorStats.Add(new AuthorStats(m_peter, m_keyStrokesByBook, "1PE", "2PE"));
			m_authorStats.Add(new AuthorStats(m_jude, m_keyStrokesByBook, "JUD"));
			Assert.AreEqual(38, m_authorStats.Count);
		}

		private static List<CharacterGroup> GetNarratorCharacterGroups(int n)
		{
			return TrialGroupConfigurationTestsSmall.GetNarratorCharacterGroups(n);
		}

		private void VerifyBasic(List<CharacterGroup> narratorGroups, int numberOfNarratorsExpectedToBeAssignedToASingleAuthor)
		{
			CharacterGroupGenerator.TrialGroupConfiguration.DistributeBooksAmongNarratorGroups(m_authorStats, narratorGroups);

			for (int i = 0; i < narratorGroups.Count; i++)
			{
				var group = narratorGroups[i];
				var booksAssignedToNarrator = group.CharacterIds.Select(CharacterVerseData.GetBookCodeFromStandardCharacterId).ToList();
				if (i < numberOfNarratorsExpectedToBeAssignedToASingleAuthor)
				{
					var author = BiblicalAuthors.GetAuthorOfBook(booksAssignedToNarrator[0]).Name;
					Assert.IsTrue(booksAssignedToNarrator.SetEquals(m_authorStats.Single(a => a.Author.Name == author).BookIds));
				}
				else
				{
					// This is a narrator group that is expected to be assigned to multiple authors. For each author,
					// the set of books for this narrator MUST contain ALL the books for that author, plus at least one
					// other book.
					var set = new HashSet<string>(booksAssignedToNarrator);
					foreach (var bookId in booksAssignedToNarrator)
					{
						var author = BiblicalAuthors.GetAuthorOfBook(bookId).Name;
						Assert.IsTrue(set.IsProperSupersetOf(m_authorStats.Single(a => a.Author.Name == author).BookIds));
					}
				}
			}
		}

		[Test]
		public void DistributeBooksAmongNarratorGroups_ThreeNarrators_AuthorsCombineCorrectly()
		{
			var narratorGroups = GetNarratorCharacterGroups(3);

			CharacterGroupGenerator.TrialGroupConfiguration.DistributeBooksAmongNarratorGroups(m_authorStats, narratorGroups);

			Assert.AreNotEqual(GetNarratorGroupForBook(narratorGroups, "GEN"), GetNarratorGroupForBook(narratorGroups, "PSA"));
			Assert.AreNotEqual(GetNarratorGroupForBook(narratorGroups, "GEN"), GetNarratorGroupForBook(narratorGroups, "ROM"));
			Assert.AreNotEqual(GetNarratorGroupForBook(narratorGroups, "PSA"), GetNarratorGroupForBook(narratorGroups, "ROM"));
			Assert.AreEqual(GetNarratorGroupForBook(narratorGroups, "GEN"), GetNarratorGroupForBook(narratorGroups, "OBA"));
			Assert.AreEqual(GetNarratorGroupForBook(narratorGroups, "GEN"), GetNarratorGroupForBook(narratorGroups, "JUD"));
			Assert.AreEqual(30, narratorGroups[0].CharacterIds.Count);
			Assert.AreEqual(GetNarratorGroupForBook(narratorGroups, "GEN"), GetNarratorGroupForBook(narratorGroups, "OBA"));
			// Etc.
			Assert.AreEqual(GetNarratorGroupForBook(narratorGroups, "GEN"), GetNarratorGroupForBook(narratorGroups, "JOS"));
			Assert.AreEqual(14, narratorGroups[1].CharacterIds.Count);
			Assert.AreEqual(GetNarratorGroupForBook(narratorGroups, "PSA"), GetNarratorGroupForBook(narratorGroups, "MAT"));
			Assert.AreEqual(GetNarratorGroupForBook(narratorGroups, "PSA"), GetNarratorGroupForBook(narratorGroups, "JOB"));
			Assert.AreEqual(GetNarratorGroupForBook(narratorGroups, "PSA"), GetNarratorGroupForBook(narratorGroups, "EZK"));
			Assert.AreEqual(GetNarratorGroupForBook(narratorGroups, "PSA"), GetNarratorGroupForBook(narratorGroups, "1KI"));
			Assert.AreEqual(GetNarratorGroupForBook(narratorGroups, "PSA"), GetNarratorGroupForBook(narratorGroups, "JHN"));
			Assert.AreEqual(GetNarratorGroupForBook(narratorGroups, "PSA"), GetNarratorGroupForBook(narratorGroups, "PRO"));
			Assert.AreEqual(22, narratorGroups[2].CharacterIds.Count);
			Assert.AreEqual(GetNarratorGroupForBook(narratorGroups, "ROM"), GetNarratorGroupForBook(narratorGroups, "LUK"));
			Assert.AreEqual(GetNarratorGroupForBook(narratorGroups, "ROM"), GetNarratorGroupForBook(narratorGroups, "1SA"));
			Assert.AreEqual(GetNarratorGroupForBook(narratorGroups, "ROM"), GetNarratorGroupForBook(narratorGroups, "JER"));
			Assert.AreEqual(GetNarratorGroupForBook(narratorGroups, "ROM"), GetNarratorGroupForBook(narratorGroups, "ISA"));
			Assert.AreEqual(GetNarratorGroupForBook(narratorGroups, "ROM"), GetNarratorGroupForBook(narratorGroups, "1CH"));
		}

		[Test]
		public void DistributeBooksAmongNarratorGroups_ThirtySevenNarrators_BottomTwoAuthorsCombine()
		{
			var narratorGroups = GetNarratorCharacterGroups(37);

			VerifyBasic(narratorGroups, 36);
			Assert.AreEqual(2, narratorGroups[36].CharacterIds.Count);
			Assert.AreEqual(GetNarratorGroupForBook(narratorGroups, "JUD"), GetNarratorGroupForBook(narratorGroups, "OBA"));
		}

		[Test]
		public void DistributeBooksAmongNarratorGroups_ThirtySixNarrators_BottomFourAuthorsCombineIntoTwoGroups()
		{
			var narratorGroups = GetNarratorCharacterGroups(36);

			VerifyBasic(narratorGroups, 34);

			Assert.AreEqual(2, narratorGroups[34].CharacterIds.Count);
			Assert.AreEqual(2, narratorGroups[35].CharacterIds.Count);
			// Obadiah and Jude are tied for the fewest number of keystrokes. Haggai is by itself in second-to-last place.
			// Joel, Nahum, Habakkuk, Zephaniah, and Zechariah are all tied for third-to-last place.
			var bookCombinedWithHaggai = CharacterVerseData.GetBookCodeFromStandardCharacterId(narratorGroups[35].CharacterIds.Single(
				c => c != CharacterVerseData.GetStandardCharacterId("HAG", CharacterVerseData.StandardCharacter.Narrator)));
			string bookWhoseAuthorCombinedWithThirdToLastPlaceAuthor;
			if (bookCombinedWithHaggai == "JUD")
				bookWhoseAuthorCombinedWithThirdToLastPlaceAuthor = "OBA";
			else
			{
				Assert.AreEqual("OBA", bookCombinedWithHaggai);
				bookWhoseAuthorCombinedWithThirdToLastPlaceAuthor = "JUD";
			}
			var thirdToLastPlaceBookThatGotCombined = CharacterVerseData.GetBookCodeFromStandardCharacterId(
				narratorGroups[34].CharacterIds.Single(c => c != CharacterVerseData.GetStandardCharacterId(bookWhoseAuthorCombinedWithThirdToLastPlaceAuthor, CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(
				thirdToLastPlaceBookThatGotCombined == "JOL" ||
				thirdToLastPlaceBookThatGotCombined == "NAM" ||
				thirdToLastPlaceBookThatGotCombined == "HAB" ||
				thirdToLastPlaceBookThatGotCombined == "ZEP" ||
				thirdToLastPlaceBookThatGotCombined == "ZEC");
		}

		[Test]
		public void DistributeBooksAmongNarratorGroups_ThirtyTwoNarrators_BottomTwelveAuthorsCombineIntoSixGroupsOfTwo()
		{
			var narratorGroups = GetNarratorCharacterGroups(32);

			VerifyBasic(narratorGroups, 26);

			for (int i = 27; i <= 32; i++)
				Assert.AreEqual(2, narratorGroups[i - 1].CharacterIds.Count);
		}

		[Test]
		public void DistributeBooksAmongNarratorGroups_FifteenNarrators_MostProlificAuthorsDoNotGetCombined()
		{
			var narratorGroups = GetNarratorCharacterGroups(15);
			VerifyBasic(narratorGroups, 5);
		}

		[Test]
		[Category("ByHand")]
		public void DistributeBooksAmongNarratorGroups_AllCombinationsOfFewerNarratorsThanAuthors_ManualCheck()
		{
			for (int i = 2; i <= 37; i++)
			{
				var narratorGroups = GetNarratorCharacterGroups(i);

				CharacterGroupGenerator.TrialGroupConfiguration.DistributeBooksAmongNarratorGroups(m_authorStats,
					narratorGroups);

				Trace.WriteLine(i + " Narrator Groups");
				Trace.WriteLine("====================");
				foreach (var narratorGroup in narratorGroups)
				{
					var totalKeyStrokesForNarrator = narratorGroup.CharacterIds.Sum(c => m_keyStrokesByBook[c]);
					Trace.WriteLine("    " + narratorGroup.GroupIdForUiDisplay + ": " + totalKeyStrokesForNarrator);
				}
				Trace.WriteLine("");
			}
		}

		[Test]
		public void DistributeBooksAmongNarratorGroups_FiftyNarrators_TwoNarratorsEachForPeterAndJohnThreeForPaul()
		{
			var narratorGroups = GetNarratorCharacterGroups(50);

			CharacterGroupGenerator.TrialGroupConfiguration.DistributeBooksAmongNarratorGroups(narratorGroups, 38,
				m_keyStrokesByBook.Keys.Select(CharacterVerseData.GetBookCodeFromStandardCharacterId), m_keyStrokesByBook);

			var narratorsWithMultipleBooks = new List<CharacterGroup>();
			// John
			var narratorGroupForGospelOfJohn = GetNarratorGroupForBook(narratorGroups, "JHN");
			var narratorGroupForRevelation = GetNarratorGroupForBook(narratorGroups, "REV");
			narratorsWithMultipleBooks.Add(narratorGroupForGospelOfJohn);
			narratorsWithMultipleBooks.Add(narratorGroupForRevelation);
			Assert.AreNotEqual(narratorGroupForGospelOfJohn, narratorGroupForRevelation);
			Assert.AreEqual(narratorGroupForGospelOfJohn, GetNarratorGroupForBook(narratorGroups, "1JN"));
			Assert.AreEqual(narratorGroupForRevelation, GetNarratorGroupForBook(narratorGroups, "2JN"));
			Assert.AreEqual(narratorGroupForRevelation, GetNarratorGroupForBook(narratorGroups, "3JN"));

			// Peter
			var narratorGroupForFirstPeter = GetNarratorGroupForBook(narratorGroups, "2PE");
			narratorsWithMultipleBooks.Add(narratorGroupForFirstPeter);
			Assert.AreEqual(narratorGroupForFirstPeter, GetNarratorGroupForBook(narratorGroups, "1PE"));

			// Paul
			var narratorGroupForRomans = GetNarratorGroupForBook(narratorGroups, "ROM");
			var narratorGroupForFirstCorinthians = GetNarratorGroupForBook(narratorGroups, "1CO");
			var narratorGroupForSecondCorinthians = GetNarratorGroupForBook(narratorGroups, "2CO");
			narratorsWithMultipleBooks.Add(narratorGroupForRomans);
			narratorsWithMultipleBooks.Add(narratorGroupForFirstCorinthians);
			narratorsWithMultipleBooks.Add(narratorGroupForSecondCorinthians);
			Assert.AreNotEqual(narratorGroupForRomans, narratorGroupForFirstCorinthians);
			Assert.AreNotEqual(narratorGroupForFirstCorinthians, narratorGroupForSecondCorinthians);
			Assert.AreNotEqual(narratorGroupForRomans, narratorGroupForSecondCorinthians);

			// Jeremiah
			var narratorGroupForJeremiah = GetNarratorGroupForBook(narratorGroups, "JER");
			narratorsWithMultipleBooks.Add(narratorGroupForJeremiah);
			Assert.AreEqual(narratorGroupForJeremiah, GetNarratorGroupForBook(narratorGroups, "LAM"));

			// Solomon
			var narratorGroupForEcclesiastes = GetNarratorGroupForBook(narratorGroups, "ECC");
			narratorsWithMultipleBooks.Add(narratorGroupForEcclesiastes);
			Assert.AreNotEqual(narratorGroupForEcclesiastes, GetNarratorGroupForBook(narratorGroups, "PRO"));
			Assert.AreEqual(narratorGroupForEcclesiastes, GetNarratorGroupForBook(narratorGroups, "SNG"));

			var listOfBooksFoundSoFar = new HashSet<string>();

			foreach (var group in narratorGroups)
			{
				var booksAssignedToNarrator = group.CharacterIds.Select(CharacterVerseData.GetBookCodeFromStandardCharacterId).ToList();
				Assert.IsTrue(booksAssignedToNarrator.Any());
				if (booksAssignedToNarrator.Count > 1)
				{
					var author = BiblicalAuthors.GetAuthorOfBook(booksAssignedToNarrator[0]);
					Assert.IsFalse(booksAssignedToNarrator.Any(b => author != BiblicalAuthors.GetAuthorOfBook(b)));
					Assert.IsTrue(narratorsWithMultipleBooks.Contains(group), author.Name);
				}
				Assert.IsFalse(listOfBooksFoundSoFar.Overlaps(booksAssignedToNarrator));
				listOfBooksFoundSoFar.AddRange(booksAssignedToNarrator);
			}
		}
	}

	[TestFixture]
	class TrialGroupConfigurationConstructorTests : CharacterGroupGeneratorAndAdjusterTestBase
	{
		[TestFixtureSetUp]
		public void TextFixtureSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerse;
			CharacterDetailData.TabDelimitedCharacterDetailData = Resources.TestCharacterDetail;
			RelatedCharactersData.Source = Resources.TestRelatedCharacters;
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.LUK, TestProject.TestBook.JUD);
		}

		[SetUp]
		public void SetUp()
		{
			m_testProject.VoiceActorList.AllActors.Clear();
			m_testProject.CharacterGroupList.CharacterGroups.Clear();
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.CastSizeOption = CastSizeOption.NotSet;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = false;
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			TestProject.DeleteTestProjectFolder();
		}

		[TestCase(true)]
		[TestCase(false)]
		public void GeneratePossibilities_OneMaleActorOneFemaleActorOneChildActor_DoesNotThrow(bool fallbackPass)
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 1;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;

			SetVoiceActors(1, 1, 1);
			var gen = new CharacterGroupGenerator(m_testProject);
			var groups = gen.GenerateCharacterGroups();
			var maxMaleNarrators = 2;  // one per book
			var maxFemaleNarrators = 0;

			var characterDetails = m_testProject.AllCharacterDetailDictionary;
			var includedCharacterDetails = characterDetails.Values.Where(c => m_testProject.AllCharacterIds.Contains(c.CharacterId)).ToList();

			// Adult groups are already assigned to actors because they are exclusive matches for their respective characterIds.
			// GeneratePossibilities assumes each group has an actor assigned, so we make the final assignment here.
			groups.Single(g => g.GroupIdLabel == CharacterGroup.Label.Child).AssignVoiceActor(m_testProject.VoiceActorList.AllActors.Single(a => a.Age == ActorAge.Child).Id);

			Assert.DoesNotThrow(() => CharacterGroupGenerator.TrialGroupConfiguration.GeneratePossibilities(
				fallbackPass,
				groups,
				maxMaleNarrators,
				maxFemaleNarrators,
				true,
				includedCharacterDetails,
				m_testProject.KeyStrokesByCharacterId,
				m_testProject,
				characterDetails,
				m_testProject.DramatizationPreferences
				)
			);
		}
	}
}
