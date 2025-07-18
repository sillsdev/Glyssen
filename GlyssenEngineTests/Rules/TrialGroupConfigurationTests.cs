﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GlyssenCharacters;
using GlyssenEngine;
using GlyssenEngine.Bundle;
using GlyssenEngine.Casting;
using GlyssenEngine.Character;
using GlyssenEngine.Rules;
using NUnit.Framework;
using SIL.Extensions;
using static GlyssenEngine.Rules.CharacterGroupGenerator.TrialGroupConfiguration;
using Resources = GlyssenCharactersTests.Properties.Resources;

namespace GlyssenEngineTests.Rules
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
			var keyStrokesByBook = new Dictionary<string, int>
			{
				[GetNarratorId("JER")] = 52000,
				[GetNarratorId("EZK")] = 48000,
				[GetNarratorId("HOS")] = 12000,
				[GetNarratorId("JUD")] = 1000
			};
			var authorStats = new List<AuthorStats>
			{
				new AuthorStats(BiblicalAuthors.GetAuthorOfBook("JER"), keyStrokesByBook, "JER"),
				new AuthorStats(BiblicalAuthors.GetAuthorOfBook("EZK"), keyStrokesByBook, "EZK"),
				new AuthorStats(BiblicalAuthors.GetAuthorOfBook("HOS"), keyStrokesByBook, "HOS"),
				new AuthorStats(BiblicalAuthors.GetAuthorOfBook("JUD"), keyStrokesByBook, "JUD")
			};

			var narratorGroups = GetNarratorCharacterGroups(3);

			DistributeBooksAmongNarratorGroups(authorStats, narratorGroups);
			var groupForJeremiah = GetNarratorGroupForBook(narratorGroups, "JER");
			var groupForJude = GetNarratorGroupForBook(narratorGroups, "JUD");
			Assert.That(groupForJeremiah, Is.Not.EqualTo(GetNarratorGroupForBook(narratorGroups, "EZK")));
			Assert.That(groupForJeremiah, Is.Not.EqualTo(groupForJude));
			Assert.That(groupForJude, Is.EqualTo(GetNarratorGroupForBook(narratorGroups, "HOS")));
		}

		[Test]
		public void DistributeBooksAmongNarratorGroups_SixSimilarAuthorsOfEightBooksAmongThreeNarrators_AuthorOfLargestBookCombinesWithShortestEtc()
		{
			var keyStrokesByBook = new Dictionary<string, int>
			{
				[GetNarratorId("GEN")] = 50000,
				[GetNarratorId("ISA")] = 66000,
				[GetNarratorId("JER")] = 52000,
				[GetNarratorId("EZK")] = 48000,
				[GetNarratorId("LUK")] = 24000, // 52000 combined
				[GetNarratorId("ACT")] = 28000,
				[GetNarratorId("JHN")] = 20000, // 42000 combined
				[GetNarratorId("REV")] = 22000
			};
			var authorStats = new List<AuthorStats>
			{
				new AuthorStats(BiblicalAuthors.GetAuthorOfBook("GEN"), keyStrokesByBook, "GEN"),
				new AuthorStats(BiblicalAuthors.GetAuthorOfBook("ISA"), keyStrokesByBook, "ISA"),
				new AuthorStats(BiblicalAuthors.GetAuthorOfBook("JER"), keyStrokesByBook, "JER"),
				new AuthorStats(BiblicalAuthors.GetAuthorOfBook("EZK"), keyStrokesByBook, "EZK"),
				new AuthorStats(BiblicalAuthors.GetAuthorOfBook("LUK"), keyStrokesByBook, "LUK", "ACT"),
				new AuthorStats(BiblicalAuthors.GetAuthorOfBook("JHN"), keyStrokesByBook, "JHN", "REV")
			};

			var narratorGroups = GetNarratorCharacterGroups(3);

			DistributeBooksAmongNarratorGroups(authorStats, narratorGroups);

			// Since there are two authors with exactly 52000 keystrokes, we can't know for sure which one will combine with GEN and
			// which will combine with EZK. So we just assert that they are grouped properly.
			Assert.That(GetNarratorGroupForBook(narratorGroups, "ISA"), Is.Not.EqualTo(GetNarratorGroupForBook(narratorGroups, "JER")));
			Assert.That(GetNarratorGroupForBook(narratorGroups, "ISA"), Is.Not.EqualTo(GetNarratorGroupForBook(narratorGroups, "EZK")));
			Assert.That(GetNarratorGroupForBook(narratorGroups, "ISA"), Is.Not.EqualTo(GetNarratorGroupForBook(narratorGroups, "GEN")));
			Assert.That(GetNarratorGroupForBook(narratorGroups, "GEN"), Is.Not.EqualTo(GetNarratorGroupForBook(narratorGroups, "EZK")));
			Assert.That(GetNarratorGroupForBook(narratorGroups, "ISA"), Is.EqualTo(GetNarratorGroupForBook(narratorGroups, "JHN")));
			Assert.That(GetNarratorGroupForBook(narratorGroups, "LUK"), Is.EqualTo(GetNarratorGroupForBook(narratorGroups, "ACT")));
			Assert.That(GetNarratorGroupForBook(narratorGroups, "JHN"), Is.EqualTo(GetNarratorGroupForBook(narratorGroups, "REV")));
			Assert.That(narratorGroups.Single(g => g.GroupId == "Narrator1").CharacterIds.Count, Is.EqualTo(3));
			Assert.That(narratorGroups.Single(g => g.GroupId == "Narrator2").CharacterIds.Count, Is.EqualTo(3));
			Assert.That(narratorGroups.Single(g => g.GroupId == "Narrator3").CharacterIds.Count, Is.EqualTo(2));
		}

		[Test]
		public void DistributeBooksAmongNarratorGroups_FourAuthorsOfSixBooksAmongFiveNarrators_AuthorWithLongestBooksSplit()
		{
			var keyStrokesByBook = new Dictionary<string, int>
			{
				[GetNarratorId("GEN")] = 50000,
				[GetNarratorId("EXO")] = 28000,
				[GetNarratorId("JER")] = 52000,
				[GetNarratorId("LAM")] = 6000,
				[GetNarratorId("HOS")] = 12000,
				[GetNarratorId("JUD")] = 1000
			};

			var narratorGroups = GetNarratorCharacterGroups(5);

			DistributeBooksAmongNarratorGroups(narratorGroups, 4,
				keyStrokesByBook.Keys.Select(CharacterVerseData.GetBookCodeFromStandardCharacterId), keyStrokesByBook);
			var narratorGroupForJeremiah = GetNarratorGroupForBook(narratorGroups, "JER");
			Assert.That(narratorGroupForJeremiah, Is.EqualTo(GetNarratorGroupForBook(narratorGroups, "LAM")));
			var listOfBooksFoundSoFar = new HashSet<string>();
			foreach (var group in narratorGroups)
			{
				var booksAssignedToNarrator = group.CharacterIds.Select(CharacterVerseData.GetBookCodeFromStandardCharacterId).ToList();
				if (group != narratorGroupForJeremiah)
					Assert.That(booksAssignedToNarrator.Count, Is.EqualTo(1));
				Assert.That(listOfBooksFoundSoFar.Overlaps(booksAssignedToNarrator), Is.False);
				listOfBooksFoundSoFar.AddRange(booksAssignedToNarrator);
			}
		}

		[Test]
		public void DistributeBooksAmongNarratorGroups_TwoAuthorsOfSixBooksAmongFiveNarrators_AuthorWithLongestBooksSplit()
		{
			var keyStrokesByBook = new Dictionary<string, int>
			{
				[GetNarratorId("GEN")] = 50000,
				[GetNarratorId("EXO")] = 28000,
				[GetNarratorId("LEV")] = 28000,
				[GetNarratorId("DEU")] = 28000,
				[GetNarratorId("JHN")] = 20000,
				[GetNarratorId("REV")] = 22000
			};

			var narratorGroups = GetNarratorCharacterGroups(5);

			DistributeBooksAmongNarratorGroups(narratorGroups, 2,
				keyStrokesByBook.Keys.Select(CharacterVerseData.GetBookCodeFromStandardCharacterId), keyStrokesByBook);
			var narratorGroupForJohn = GetNarratorGroupForBook(narratorGroups, "JHN");
			Assert.That(narratorGroupForJohn, Is.EqualTo(GetNarratorGroupForBook(narratorGroups, "REV")));
			var listOfBooksFoundSoFar = new HashSet<string>();
			foreach (var group in narratorGroups)
			{
				var booksAssignedToNarrator = group.CharacterIds.Select(CharacterVerseData.GetBookCodeFromStandardCharacterId).ToList();
				if (group != narratorGroupForJohn)
					Assert.That(booksAssignedToNarrator.Count, Is.EqualTo(1));
				Assert.That(listOfBooksFoundSoFar.Overlaps(booksAssignedToNarrator), Is.False);
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

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			m_keyStrokesByBook = new Dictionary<string, int>();
			m_keyStrokesByBook[GetNarratorId("GEN")] = 50000; // MOSES
			m_keyStrokesByBook[GetNarratorId("EXO")] = 40000; // MOSES
			m_keyStrokesByBook[GetNarratorId("LEV")] = 27000; // MOSES
			m_keyStrokesByBook[GetNarratorId("NUM")] = 36000; // MOSES
			m_keyStrokesByBook[GetNarratorId("DEU")] = 34000; // MOSES:     187000
			m_keyStrokesByBook[GetNarratorId("JOS")] = 24000; // JOSHUA:     24000
			m_keyStrokesByBook[GetNarratorId("JDG")] = 21000; // JUDGES:     21000
			m_keyStrokesByBook[GetNarratorId("RUT")] = 4000; // RUTH:        4000
			m_keyStrokesByBook[GetNarratorId("1SA")] = 31000; // SAMUEL
			m_keyStrokesByBook[GetNarratorId("2SA")] = 24000; // SAMUEL:     55000
			m_keyStrokesByBook[GetNarratorId("1KI")] = 24000; // KINGS
			m_keyStrokesByBook[GetNarratorId("2KI")] = 25000; // KINGS:      49000
			m_keyStrokesByBook[GetNarratorId("1CH")] = 35000; // CHRONICLES
			m_keyStrokesByBook[GetNarratorId("2CH")] = 36000; // CHRONICLES: 71000
			m_keyStrokesByBook[GetNarratorId("EZR")] = 10000; // EZRA:       10000
			m_keyStrokesByBook[GetNarratorId("NEH")] = 13000; // NEHEMIAH:   13000
			m_keyStrokesByBook[GetNarratorId("EST")] = 10000; // ESTHER:     10000
			m_keyStrokesByBook[GetNarratorId("JOB")] = 42000; // JOB:        42000
			m_keyStrokesByBook[GetNarratorId("PSA")] = 99999; // PSALMS:     99999
			m_keyStrokesByBook[GetNarratorId("PRO")] = 31000; // SOLOMON
			m_keyStrokesByBook[GetNarratorId("ECC")] = 12000; // SOLOMON
			m_keyStrokesByBook[GetNarratorId("SNG")] = 8000; // SOLOMON:     51000
			m_keyStrokesByBook[GetNarratorId("ISA")] = 66000; // ISAIAH:     66000
			m_keyStrokesByBook[GetNarratorId("JER")] = 52000; // JEREMIAH
			m_keyStrokesByBook[GetNarratorId("LAM")] = 5000; // JEREMIAH:    57000
			m_keyStrokesByBook[GetNarratorId("EZK")] = 48000; // EZEKIEL:    48000
			m_keyStrokesByBook[GetNarratorId("DAN")] = 12000; // DANIEL:     12000
			m_keyStrokesByBook[GetNarratorId("HOS")] = 14000; // HOSEA:      14000
			m_keyStrokesByBook[GetNarratorId("JOL")] = 3000; // JOEL:         3000
			m_keyStrokesByBook[GetNarratorId("AMO")] = 9000; // AMOS:         9000
			m_keyStrokesByBook[GetNarratorId("OBA")] = 1000; // OBADIAH:      1000
			m_keyStrokesByBook[GetNarratorId("JON")] = 4000; // JONAH:        4000
			m_keyStrokesByBook[GetNarratorId("MIC")] = 7000; // MICAH:        7000
			m_keyStrokesByBook[GetNarratorId("NAM")] = 3000; // NAHUM:        3000
			m_keyStrokesByBook[GetNarratorId("HAB")] = 3000; // HABAKKUK:     3000
			m_keyStrokesByBook[GetNarratorId("ZEP")] = 3000; // ZEPHANIAH:    3000
			m_keyStrokesByBook[GetNarratorId("HAG")] = 2000; // HAGGAI:       2000
			m_keyStrokesByBook[GetNarratorId("ZEC")] = 3000; // ZECHARIAH:    3000
			m_keyStrokesByBook[GetNarratorId("MAL")] = 4000; // MALACHI:      4000
			m_keyStrokesByBook[GetNarratorId("MAT")] = 28000; // MATTHEW:    28000
			m_keyStrokesByBook[GetNarratorId("MRK")] = 16000; // MARK:       16000
			m_keyStrokesByBook[GetNarratorId("LUK")] = 24000; // LUKE
			m_keyStrokesByBook[GetNarratorId("ACT")] = 28000; // LUKE:       52000
			m_keyStrokesByBook[GetNarratorId("JHN")] = 20000; // JOHN
			m_keyStrokesByBook[GetNarratorId("1JN")] = 5000; // JOHN
			m_keyStrokesByBook[GetNarratorId("2JN")] = 1000; // JOHN
			m_keyStrokesByBook[GetNarratorId("3JN")] = 1000; // JOHN
			m_keyStrokesByBook[GetNarratorId("REV")] = 22000; // JOHN:       49000
			m_keyStrokesByBook[GetNarratorId("ROM")] = 16000; // PAUL
			m_keyStrokesByBook[GetNarratorId("1CO")] = 16000; // PAUL
			m_keyStrokesByBook[GetNarratorId("2CO")] = 13000; // PAUL
			m_keyStrokesByBook[GetNarratorId("GAL")] = 6000; // PAUL
			m_keyStrokesByBook[GetNarratorId("EPH")] = 6000; // PAUL
			m_keyStrokesByBook[GetNarratorId("PHP")] = 4000; // PAUL
			m_keyStrokesByBook[GetNarratorId("COL")] = 4000; // PAUL
			m_keyStrokesByBook[GetNarratorId("1TH")] = 5000; // PAUL
			m_keyStrokesByBook[GetNarratorId("2TH")] = 3000; // PAUL
			m_keyStrokesByBook[GetNarratorId("1TI")] = 6000; // PAUL
			m_keyStrokesByBook[GetNarratorId("2TI")] = 4000; // PAUL
			m_keyStrokesByBook[GetNarratorId("TIT")] = 3000; // PAUL
			m_keyStrokesByBook[GetNarratorId("PHM")] = 1000; // PAUL:        87000
			m_keyStrokesByBook[GetNarratorId("HEB")] = 13000; // HEBREWS:    13000
			m_keyStrokesByBook[GetNarratorId("JAS")] = 5000; // JAMES:        5000
			m_keyStrokesByBook[GetNarratorId("1PE")] = 5000; // PETER
			m_keyStrokesByBook[GetNarratorId("2PE")] = 3000; // PETER:        8000
			m_keyStrokesByBook[GetNarratorId("JUD")] = 1000; // JUDE:         1000

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
			Assert.That(m_authorStats.Count, Is.EqualTo(38));
		}

		private static List<CharacterGroup> GetNarratorCharacterGroups(int n)
		{
			return TrialGroupConfigurationTestsSmall.GetNarratorCharacterGroups(n);
		}

		private void VerifyBasic(List<CharacterGroup> narratorGroups, int numberOfNarratorsExpectedToBeAssignedToASingleAuthor)
		{
			DistributeBooksAmongNarratorGroups(m_authorStats, narratorGroups);

			for (int i = 0; i < narratorGroups.Count; i++)
			{
				var group = narratorGroups[i];
				var booksAssignedToNarrator = group.CharacterIds.Select(CharacterVerseData.GetBookCodeFromStandardCharacterId).ToList();
				if (i < numberOfNarratorsExpectedToBeAssignedToASingleAuthor)
				{
					var author = BiblicalAuthors.GetAuthorOfBook(booksAssignedToNarrator[0]).Name;
					Assert.That(booksAssignedToNarrator, Is.EquivalentTo(
						m_authorStats.Single(a => a.Author.Name == author).BookIds));
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
						Assert.That(set.IsProperSupersetOf(m_authorStats.Single(a => a.Author.Name == author).BookIds), Is.True);
					}
				}
			}
		}

		[Test]
		public void DistributeBooksAmongNarratorGroups_ThreeNarrators_AuthorsCombineCorrectly()
		{
			var narratorGroups = GetNarratorCharacterGroups(3);

			DistributeBooksAmongNarratorGroups(m_authorStats, narratorGroups);

			Assert.That(GetNarratorGroupForBook(narratorGroups, "GEN"), Is.Not.EqualTo(GetNarratorGroupForBook(narratorGroups, "PSA")));
			Assert.That(GetNarratorGroupForBook(narratorGroups, "GEN"), Is.Not.EqualTo(GetNarratorGroupForBook(narratorGroups, "ROM")));
			Assert.That(GetNarratorGroupForBook(narratorGroups, "PSA"), Is.Not.EqualTo(GetNarratorGroupForBook(narratorGroups, "ROM")));
			Assert.That(GetNarratorGroupForBook(narratorGroups, "GEN"), Is.EqualTo(GetNarratorGroupForBook(narratorGroups, "OBA")));
			Assert.That(GetNarratorGroupForBook(narratorGroups, "GEN"), Is.EqualTo(GetNarratorGroupForBook(narratorGroups, "JUD")));
			Assert.That(narratorGroups[0].CharacterIds.Count, Is.EqualTo(30));
			Assert.That(GetNarratorGroupForBook(narratorGroups, "GEN"), Is.EqualTo(GetNarratorGroupForBook(narratorGroups, "OBA")));
			// Etc.
			Assert.That(GetNarratorGroupForBook(narratorGroups, "GEN"), Is.EqualTo(GetNarratorGroupForBook(narratorGroups, "JOS")));
			Assert.That(narratorGroups[1].CharacterIds.Count, Is.EqualTo(14));
			Assert.That(GetNarratorGroupForBook(narratorGroups, "PSA"), Is.EqualTo(GetNarratorGroupForBook(narratorGroups, "MAT")));
			Assert.That(GetNarratorGroupForBook(narratorGroups, "PSA"), Is.EqualTo(GetNarratorGroupForBook(narratorGroups, "JOB")));
			Assert.That(GetNarratorGroupForBook(narratorGroups, "PSA"), Is.EqualTo(GetNarratorGroupForBook(narratorGroups, "EZK")));
			Assert.That(GetNarratorGroupForBook(narratorGroups, "PSA"), Is.EqualTo(GetNarratorGroupForBook(narratorGroups, "1KI")));
			Assert.That(GetNarratorGroupForBook(narratorGroups, "PSA"), Is.EqualTo(GetNarratorGroupForBook(narratorGroups, "JHN")));
			Assert.That(GetNarratorGroupForBook(narratorGroups, "PSA"), Is.EqualTo(GetNarratorGroupForBook(narratorGroups, "PRO")));
			Assert.That(narratorGroups[2].CharacterIds.Count, Is.EqualTo(22));
			Assert.That(GetNarratorGroupForBook(narratorGroups, "ROM"), Is.EqualTo(GetNarratorGroupForBook(narratorGroups, "LUK")));
			Assert.That(GetNarratorGroupForBook(narratorGroups, "ROM"), Is.EqualTo(GetNarratorGroupForBook(narratorGroups, "1SA")));
			Assert.That(GetNarratorGroupForBook(narratorGroups, "ROM"), Is.EqualTo(GetNarratorGroupForBook(narratorGroups, "JER")));
			Assert.That(GetNarratorGroupForBook(narratorGroups, "ROM"), Is.EqualTo(GetNarratorGroupForBook(narratorGroups, "ISA")));
			Assert.That(GetNarratorGroupForBook(narratorGroups, "ROM"), Is.EqualTo(GetNarratorGroupForBook(narratorGroups, "1CH")));
		}

		[Test]
		public void DistributeBooksAmongNarratorGroups_ThirtySevenNarrators_BottomTwoAuthorsCombine()
		{
			var narratorGroups = GetNarratorCharacterGroups(37);

			VerifyBasic(narratorGroups, 36);
			Assert.That(narratorGroups[36].CharacterIds.Count, Is.EqualTo(2));
			Assert.That(GetNarratorGroupForBook(narratorGroups, "JUD"), Is.EqualTo(GetNarratorGroupForBook(narratorGroups, "OBA")));
		}

		[Test]
		public void DistributeBooksAmongNarratorGroups_ThirtySixNarrators_BottomFourAuthorsCombineIntoTwoGroups()
		{
			var narratorGroups = GetNarratorCharacterGroups(36);

			VerifyBasic(narratorGroups, 34);

			Assert.That(narratorGroups[34].CharacterIds.Count, Is.EqualTo(2));
			Assert.That(narratorGroups[35].CharacterIds.Count, Is.EqualTo(2));
			// Obadiah and Jude are tied for the fewest number of keystrokes. Haggai is by itself in second-to-last place.
			// Joel, Nahum, Habakkuk, Zephaniah, and Zechariah are all tied for third-to-last place.
			var bookCombinedWithHaggai = CharacterVerseData.GetBookCodeFromStandardCharacterId(narratorGroups[35].CharacterIds.Single(
				c => c != GetNarratorId("HAG")));
			string bookWhoseAuthorCombinedWithThirdToLastPlaceAuthor;
			if (bookCombinedWithHaggai == "JUD")
				bookWhoseAuthorCombinedWithThirdToLastPlaceAuthor = "OBA";
			else
			{
				Assert.That(bookCombinedWithHaggai, Is.EqualTo("OBA"));
				bookWhoseAuthorCombinedWithThirdToLastPlaceAuthor = "JUD";
			}
			var thirdToLastPlaceBookThatGotCombined = CharacterVerseData.GetBookCodeFromStandardCharacterId(
				narratorGroups[34].CharacterIds.Single(c => c != CharacterVerseData.GetStandardCharacterId(bookWhoseAuthorCombinedWithThirdToLastPlaceAuthor, CharacterVerseData.StandardCharacter.Narrator)));
			Assert.That(thirdToLastPlaceBookThatGotCombined,
				Is.AnyOf("JOL", "NAM", "HAB", "ZEP", "ZEC"));
		}

		[Test]
		public void DistributeBooksAmongNarratorGroups_ThirtyTwoNarrators_BottomTwelveAuthorsCombineIntoSixGroupsOfTwo()
		{
			var narratorGroups = GetNarratorCharacterGroups(32);

			VerifyBasic(narratorGroups, 26);

			for (int i = 27; i <= 32; i++)
				Assert.That(narratorGroups[i - 1].CharacterIds.Count, Is.EqualTo(2));
		}

		[Test]
		public void DistributeBooksAmongNarratorGroups_FifteenNarrators_MostProlificAuthorsDoNotGetCombined()
		{
			var narratorGroups = GetNarratorCharacterGroups(15);
			VerifyBasic(narratorGroups, 5);
		}

		[Test, Explicit] // by hand only
		public void DistributeBooksAmongNarratorGroups_AllCombinationsOfFewerNarratorsThanAuthors_ManualCheck()
		{
			for (int i = 2; i <= 37; i++)
			{
				var narratorGroups = GetNarratorCharacterGroups(i);

				DistributeBooksAmongNarratorGroups(m_authorStats,
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

			DistributeBooksAmongNarratorGroups(narratorGroups, 38,
				m_keyStrokesByBook.Keys.Select(CharacterVerseData.GetBookCodeFromStandardCharacterId), m_keyStrokesByBook);

			var narratorsWithMultipleBooks = new List<CharacterGroup>();
			// John
			var narratorGroupForGospelOfJohn = GetNarratorGroupForBook(narratorGroups, "JHN");
			var narratorGroupForRevelation = GetNarratorGroupForBook(narratorGroups, "REV");
			narratorsWithMultipleBooks.Add(narratorGroupForGospelOfJohn);
			narratorsWithMultipleBooks.Add(narratorGroupForRevelation);
			Assert.That(narratorGroupForGospelOfJohn, Is.Not.EqualTo(narratorGroupForRevelation));
			Assert.That(narratorGroupForGospelOfJohn, Is.EqualTo(GetNarratorGroupForBook(narratorGroups, "1JN")));
			Assert.That(narratorGroupForRevelation, Is.EqualTo(GetNarratorGroupForBook(narratorGroups, "2JN")));
			Assert.That(narratorGroupForRevelation, Is.EqualTo(GetNarratorGroupForBook(narratorGroups, "3JN")));

			// Peter
			var narratorGroupForFirstPeter = GetNarratorGroupForBook(narratorGroups, "2PE");
			narratorsWithMultipleBooks.Add(narratorGroupForFirstPeter);
			Assert.That(narratorGroupForFirstPeter, Is.EqualTo(GetNarratorGroupForBook(narratorGroups, "1PE")));

			// Paul
			var narratorGroupForRomans = GetNarratorGroupForBook(narratorGroups, "ROM");
			var narratorGroupForFirstCorinthians = GetNarratorGroupForBook(narratorGroups, "1CO");
			var narratorGroupForSecondCorinthians = GetNarratorGroupForBook(narratorGroups, "2CO");
			narratorsWithMultipleBooks.Add(narratorGroupForRomans);
			narratorsWithMultipleBooks.Add(narratorGroupForFirstCorinthians);
			narratorsWithMultipleBooks.Add(narratorGroupForSecondCorinthians);
			Assert.That(narratorGroupForRomans, Is.Not.EqualTo(narratorGroupForFirstCorinthians));
			Assert.That(narratorGroupForFirstCorinthians, Is.Not.EqualTo(narratorGroupForSecondCorinthians));
			Assert.That(narratorGroupForRomans, Is.Not.EqualTo(narratorGroupForSecondCorinthians));

			// Jeremiah
			var narratorGroupForJeremiah = GetNarratorGroupForBook(narratorGroups, "JER");
			narratorsWithMultipleBooks.Add(narratorGroupForJeremiah);
			Assert.That(narratorGroupForJeremiah, Is.EqualTo(GetNarratorGroupForBook(narratorGroups, "LAM")));

			// Solomon
			var narratorGroupForEcclesiastes = GetNarratorGroupForBook(narratorGroups, "ECC");
			narratorsWithMultipleBooks.Add(narratorGroupForEcclesiastes);
			Assert.That(narratorGroupForEcclesiastes, Is.Not.EqualTo(GetNarratorGroupForBook(narratorGroups, "PRO")));
			Assert.That(narratorGroupForEcclesiastes, Is.EqualTo(GetNarratorGroupForBook(narratorGroups, "SNG")));

			var listOfBooksFoundSoFar = new HashSet<string>();

			foreach (var group in narratorGroups)
			{
				var booksAssignedToNarrator = group.CharacterIds.Select(CharacterVerseData.GetBookCodeFromStandardCharacterId).ToList();
				Assert.That(booksAssignedToNarrator.Any(), Is.True);
				if (booksAssignedToNarrator.Count > 1)
				{
					var author = BiblicalAuthors.GetAuthorOfBook(booksAssignedToNarrator[0]);
					Assert.That(booksAssignedToNarrator.Any(b => author != BiblicalAuthors.GetAuthorOfBook(b)), Is.False);
					Assert.That(narratorsWithMultipleBooks, Does.Contain(group), author.Name);
				}
				Assert.That(listOfBooksFoundSoFar.Overlaps(booksAssignedToNarrator), Is.False);
				listOfBooksFoundSoFar.AddRange(booksAssignedToNarrator);
			}
		}
	}

	[TestFixture]
	class TrialGroupConfigurationConstructorTests : CharacterGroupGeneratorAndAdjusterTestBase
	{
		[OneTimeSetUp]
		public void OneTimeSetUp()
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

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			TestProject.DeleteTestProjects();
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

			Assert.DoesNotThrow(() => GeneratePossibilities(
				fallbackPass,
				groups,
				maxMaleNarrators,
				maxFemaleNarrators,
				true,
				includedCharacterDetails,
				m_testProject.KeyStrokesByCharacterId,
				m_testProject,
				characterDetails
				)
			);
		}
	}

	[TestFixture]
	class TrialGroupConfigurationTestsNarrationByAuthorWithPaulPeterMisc : CharacterGroupGeneratorAndAdjusterTestBase
	{
		private readonly string idPaul = BiblicalAuthors.GetAuthorOfBook("GAL").Name;
		private readonly string idPeter = BiblicalAuthors.GetAuthorOfBook("2PE").Name;
		private readonly string narratorGalForUi = CharacterVerseData.GetCharacterNameForUi(CharacterVerseData.GetStandardCharacterId("GAL", CharacterVerseData.StandardCharacter.Narrator));
		private readonly string narrator2PeForUi = CharacterVerseData.GetCharacterNameForUi(CharacterVerseData.GetStandardCharacterId("2PE", CharacterVerseData.StandardCharacter.Narrator));
		private readonly string narratorMrkForUi = CharacterVerseData.GetCharacterNameForUi(CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.Narrator));
		private readonly string narratorActForUi = CharacterVerseData.GetCharacterNameForUi(CharacterVerseData.GetStandardCharacterId("ACT", CharacterVerseData.StandardCharacter.Narrator));
		private Dictionary<string, int> m_keyStrokesByCharId;
		private List<CharacterDetail> m_includedCharacterDetails;
		private Dictionary<string, CharacterDetail> m_characterDetails;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerse;
			CharacterDetailData.TabDelimitedCharacterDetailData = Resources.TestCharacterDetail;
			RelatedCharactersData.Source = Resources.TestRelatedCharacters;
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.ACT, TestProject.TestBook.MRK,
				TestProject.TestBook.GAL, TestProject.TestBook.IIPE_NoData);
			m_testProject.CharacterGroupGenerationPreferences.CastSizeOption = CastSizeOption.NotSet;
			m_testProject.CharacterGroupGenerationPreferences.NarratorsOption = NarratorsOption.NarrationByAuthor;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;
		}

		[SetUp]
		public void SetUp()
		{
			m_testProject.VoiceActorList.AllActors.Clear();
			m_testProject.CharacterGroupList.CharacterGroups.Clear();
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = Int32.MaxValue; // Tests have to set this to a valid value!
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			TestProject.DeleteTestProjects();
		}

		private List<CharacterGroup> PopulateGroups()
		{
			SetVoiceActors(m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators);

			var groups = new List<CharacterGroup>(m_testProject.VoiceActorList.AllActors.Count);
			foreach (VoiceActor actor in m_testProject.VoiceActorList.AllActors)
			{
				var group = new CharacterGroup(m_testProject);
				group.AssignVoiceActor(actor.Id);
				groups.Add(group);
			}
			return groups;
		}

		private void PopulateKeyStrokesByCharId(int charactersSpokenByNarratorOfGalatians,
			int charactersSpokenByNarratorOf2Peter, int charactersSpokenByPaul, int charactersSpokenByPeter)
		{
			m_keyStrokesByCharId = new Dictionary<string, int>
			{
				[GetNarratorId("ACT")] = 50000,
				[GetNarratorId("MRK")] = 25000,
				[GetNarratorId("GAL")] = charactersSpokenByNarratorOfGalatians,
				[GetNarratorId("2PE")] = charactersSpokenByNarratorOf2Peter,
				[idPaul] = charactersSpokenByPaul,
				[idPeter] = charactersSpokenByPeter
			};
		}

		private void PopulateCharacterDetails()
		{
			m_includedCharacterDetails = new List<CharacterDetail>();
			m_characterDetails = new Dictionary<string, CharacterDetail>();
			foreach (var charId in m_keyStrokesByCharId.Keys)
			{
				var detail = new CharacterDetail
				{
					CharacterId = charId,
					StandardCharacterType = CharacterVerseData.GetStandardCharacterType(charId)
				};
				m_includedCharacterDetails.Add(detail);
				m_characterDetails[charId] = detail;
			}
		}

		[TestCase(30000, 30000, 1000, 999, "GAL")]
		[TestCase(30000, 30000, 999, 1000, "2PE")]
		[TestCase(30000, 29000, 1000, 1000, "GAL")]
		[TestCase(29000, 30000, 1000, 1000, "2PE")]
		[TestCase(30000, 29000, 900, 1000, "GAL")]
		[TestCase(29000, 30000, 1000, 900, "2PE")]
		public void GeneratePossibilities_TwoNarrators_NarratorAndAuthorWithMostKeystrokesInSeparateNarratorGroupAndOtherAuthorCharacterNotInNarratorGroup(
			int charactersSpokenByNarratorOfGalatians, int charactersSpokenByNarratorOf2Peter,
			int charactersSpokenByPaul, int charactersSpokenByPeter, string expectedBookToGroupWithAuthor)
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 2;

			var groups = PopulateGroups();
			PopulateKeyStrokesByCharId(charactersSpokenByNarratorOfGalatians, charactersSpokenByNarratorOf2Peter,
				charactersSpokenByPaul, charactersSpokenByPeter);
			PopulateCharacterDetails();

			var result = GeneratePossibilities(false, groups,
				m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators, 0, false,
				m_includedCharacterDetails, m_keyStrokesByCharId, m_testProject, m_characterDetails).Single();

			var narratorGroups = result.NarratorGroups;
			Assert.That(groups.Count, Is.EqualTo(narratorGroups.Count));

			var authorsGroup = GetNarratorGroupForBook(narratorGroups, expectedBookToGroupWithAuthor);
			var narratorGroupForActs = GetNarratorGroupForBook(narratorGroups, "ACT");
			Assert.That(authorsGroup.CharacterIds.Count, Is.EqualTo(2));
			Assert.That(narratorGroupForActs.CharacterIds.Count, Is.EqualTo(3));
			string expectedNarrToGroupWithActsAndMark, idAuthorCharacter;
			if (expectedBookToGroupWithAuthor == "GAL")
			{
				expectedNarrToGroupWithActsAndMark = narrator2PeForUi;
				idAuthorCharacter = idPaul;
			}
			else
			{
				expectedNarrToGroupWithActsAndMark = narratorGalForUi;
				idAuthorCharacter = idPeter;
			}

			Assert.That(authorsGroup.CharacterIds.ToList(), Does.Contain(idAuthorCharacter));
			Assert.That(narratorGroupForActs.CharacterIds.ToList(), Is.EquivalentTo(new[] {
				narratorMrkForUi, narratorActForUi, expectedNarrToGroupWithActsAndMark }));
		}

		[TestCase(30000, 2900, 1000, 999)]
		[TestCase(3000, 3000, 999, 1000)]
		[TestCase(2900, 3000, 1000, 999)]
		[TestCase(2900, 3000, 999, 1000)]
		public void GeneratePossibilities_ThreeNarrators_SplitGroupsForPaulAndPeterAndEverybodyElse(
			int charactersSpokenByNarratorOfGalatians, int charactersSpokenByNarratorOf2Peter,
			int charactersSpokenByPaul, int charactersSpokenByPeter)
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 3;

			var groups = PopulateGroups();
			PopulateKeyStrokesByCharId(charactersSpokenByNarratorOfGalatians, charactersSpokenByNarratorOf2Peter,
				charactersSpokenByPaul, charactersSpokenByPeter);
			PopulateCharacterDetails();

			var result = GeneratePossibilities(false, groups,
				m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators, 0, false,
				m_includedCharacterDetails, m_keyStrokesByCharId, m_testProject, m_characterDetails).Single();

			var narratorGroups = result.NarratorGroups;
			Assert.That(groups.Count, Is.EqualTo(narratorGroups.Count));

			var paulGroup = GetNarratorGroupForBook(narratorGroups, "GAL");
			var peterGroup = GetNarratorGroupForBook(narratorGroups, "2PE");
			var narratorGroupForActs = GetNarratorGroupForBook(narratorGroups, "ACT");

			Assert.That(paulGroup.CharacterIds.ToList(),
				Is.EquivalentTo(new[] { narratorGalForUi, idPaul }));
			Assert.That(peterGroup.CharacterIds.ToList(),
				Is.EquivalentTo(new[] { narrator2PeForUi, idPeter }));
			Assert.That(narratorGroupForActs.CharacterIds.ToList(),
				Is.EquivalentTo(new[] { narratorMrkForUi, narratorActForUi }));
		}

		[TestCase(60000, 60000, 1000, 999)]
		[TestCase(30000, 30000, 999, 1000)]
		[TestCase(52000, 2900, 1000, 999)]
		[TestCase(3000, 3000, 999, 1000)]
		[TestCase(2900, 3000, 1000, 999)]
		[TestCase(2900, 3000, 999, 1000)]
		public void GeneratePossibilities_FourNarrators_SplitGroupsForPaulAndPeterAndLukeAndActs(
			int charactersSpokenByNarratorOfGalatians, int charactersSpokenByNarratorOf2Peter,
			int charactersSpokenByPaul, int charactersSpokenByPeter)
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 4;

			var groups = PopulateGroups();
			PopulateKeyStrokesByCharId(charactersSpokenByNarratorOfGalatians, charactersSpokenByNarratorOf2Peter,
				charactersSpokenByPaul, charactersSpokenByPeter);
			PopulateCharacterDetails();

			var result = GeneratePossibilities(false, groups,
				m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators, 0, false,
				m_includedCharacterDetails, m_keyStrokesByCharId, m_testProject, m_characterDetails).Single();

			var narratorGroups = result.NarratorGroups;
			Assert.That(groups.Count, Is.EqualTo(narratorGroups.Count));

			var paulGroup = GetNarratorGroupForBook(narratorGroups, "GAL");
			var peterGroup = GetNarratorGroupForBook(narratorGroups, "2PE");
			var narratorGroupForActs = GetNarratorGroupForBook(narratorGroups, "ACT");
			var narratorGroupForMark = GetNarratorGroupForBook(narratorGroups, "MRK");

			Assert.That(paulGroup.CharacterIds.ToList(),
				Is.EquivalentTo(new[] { narratorGalForUi, idPaul }));
			Assert.That(peterGroup.CharacterIds.ToList(),
				Is.EquivalentTo(new[] { narrator2PeForUi, idPeter }));
			Assert.That(narratorGroupForActs.CharacterIds.Count, Is.EqualTo(1));
			Assert.That(narratorGroupForMark.CharacterIds.Count, Is.EqualTo(1));
		}
	}

	[TestFixture]
	class TrialGroupConfigurationTestsNarationByAuthorWithWholeNT : CharacterGroupGeneratorAndAdjusterTestBase
	{
		private readonly string idPaul = BiblicalAuthors.GetAuthorOfBook("GAL").Name;
		private readonly string idPeter = BiblicalAuthors.GetAuthorOfBook("2PE").Name;
		private readonly string idJames = BiblicalAuthors.GetAuthorOfBook("JAS").Name;
		private Dictionary<string, int> m_keyStrokesByCharId;
		private List<CharacterDetail> m_includedCharacterDetails;
		private Dictionary<string, CharacterDetail> m_characterDetails;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerse;
			CharacterDetailData.TabDelimitedCharacterDetailData = Resources.TestCharacterDetail;
			RelatedCharactersData.Source = Resources.TestRelatedCharacters;
			m_testProject = TestProject.CreateTestProject(
				TestProject.TestBook.MAT,
				TestProject.TestBook.MRK,
				TestProject.TestBook.LUK,
				TestProject.TestBook.JHN,
				TestProject.TestBook.ACT,
				TestProject.TestBook.ROM_NoData,
				TestProject.TestBook.ICO,
				TestProject.TestBook.IICO_NoData,
				TestProject.TestBook.GAL,
				TestProject.TestBook.EPH,
				TestProject.TestBook.PHP_NoData,
				TestProject.TestBook.COL_NoData,
				TestProject.TestBook.ITH_NoData,
				TestProject.TestBook.IITH_NoData,
				TestProject.TestBook.ITI_NoData,
				TestProject.TestBook.IITI_NoData,
				TestProject.TestBook.TIT_NoData,
				TestProject.TestBook.PHM,
				TestProject.TestBook.HEB,
				TestProject.TestBook.JAS_NoData,
				TestProject.TestBook.IPE_NoData,
				TestProject.TestBook.IIPE_NoData,
				TestProject.TestBook.IJN,
				TestProject.TestBook.IIJN,
				TestProject.TestBook.IIIJN,
				TestProject.TestBook.JUD,
				TestProject.TestBook.REV);
			m_testProject.CharacterGroupGenerationPreferences.CastSizeOption = CastSizeOption.NotSet;
			m_testProject.CharacterGroupGenerationPreferences.NarratorsOption = NarratorsOption.NarrationByAuthor;
			m_testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 0;
			m_testProject.CharacterGroupGenerationPreferences.IsSetByUser = true;
		}

		[SetUp]
		public void SetUp()
		{
			m_testProject.VoiceActorList.AllActors.Clear();
			m_testProject.CharacterGroupList.CharacterGroups.Clear();
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = Int32.MaxValue; // Tests have to set this to a valid value!
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			TestProject.DeleteTestProjects();
		}

		private List<CharacterGroup> PopulateGroups()
		{
			SetVoiceActors(m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators);

			var groups = new List<CharacterGroup>(m_testProject.VoiceActorList.AllActors.Count);
			foreach (VoiceActor actor in m_testProject.VoiceActorList.AllActors)
			{
				var group = new CharacterGroup(m_testProject);
				group.AssignVoiceActor(actor.Id);
				groups.Add(group);
			}
			return groups;
		}

		private void PopulateKeyStrokesByCharId(int charactersSpokenByNarratorOfGalatians,
			int charactersSpokenByNarratorOf2Peter, int charactersSpokenByPaul, int charactersSpokenByPeter,
			int defaultCharCount = 9000)
		{
			m_keyStrokesByCharId = new Dictionary<string, int>
			{
				[GetNarratorId("ACT")] = 50000,
				[GetNarratorId("MRK")] = 25000,
				[GetNarratorId("JHN")] = 35000,
				[GetNarratorId("REV")] = 25000,
				[GetNarratorId("GAL")] = charactersSpokenByNarratorOfGalatians,
				[GetNarratorId("2PE")] = charactersSpokenByNarratorOf2Peter,
				[BiblicalAuthors.GetAuthorOfBook("JAS").Name] = 1000,
				[idPaul] = charactersSpokenByPaul,
				[idPeter] = charactersSpokenByPeter
			};
			foreach (var bookNarrator in m_testProject.IncludedBookIds.Select(GetNarratorId))
			{
				if (!m_keyStrokesByCharId.ContainsKey(bookNarrator))
					m_keyStrokesByCharId[bookNarrator] = defaultCharCount;
			}
		}

		private void PopulateCharacterDetails()
		{
			m_includedCharacterDetails = new List<CharacterDetail>();
			m_characterDetails = new Dictionary<string, CharacterDetail>();
			foreach (var charId in m_keyStrokesByCharId.Keys)
			{
				var detail = new CharacterDetail
				{
					CharacterId = charId,
					StandardCharacterType = CharacterVerseData.GetStandardCharacterType(charId)
				};
				m_includedCharacterDetails.Add(detail);
				m_characterDetails[charId] = detail;
			}
		}

		[TestCase(30000, 30000, 1000, 999)]
		[TestCase(30000, 29000, 1000, 1000)]
		[TestCase(30000, 29000, 900, 1000)]
		public void GeneratePossibilities_TwoNarrators_OnlyPaulGroupedWithCharacter(
			int charactersSpokenByNarratorOfGalatians, int charactersSpokenByNarratorOf2Peter,
			int charactersSpokenByPaul, int charactersSpokenByPeter)
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 2;

			var groups = PopulateGroups();
			PopulateKeyStrokesByCharId(charactersSpokenByNarratorOfGalatians, charactersSpokenByNarratorOf2Peter,
				charactersSpokenByPaul, charactersSpokenByPeter);
			PopulateCharacterDetails();

			var result = GeneratePossibilities(false, groups,
				m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators, 0, false,
				m_includedCharacterDetails, m_keyStrokesByCharId, m_testProject, m_characterDetails).Single();

			var narratorGroups = result.NarratorGroups;
			Assert.That(groups.Count, Is.EqualTo(narratorGroups.Count));

			var narratorGroupForActs = GetNarratorGroupForBook(narratorGroups, "ACT");
			var authorsGroup = GetNarratorGroupForBook(narratorGroups, "ROM");
			Assert.That(narratorGroupForActs, Is.Not.EqualTo(authorsGroup));

			Assert.That(authorsGroup.CharacterIds, Does.Contain(idPaul));
			var narratorsForAuthor = authorsGroup.CharacterIds.Where(c => c != idPaul).ToList();
			Assert.That(narratorsForAuthor.All(n => BiblicalAuthors.GetAuthorOfBook(CharacterVerseData.GetBookCodeFromStandardCharacterId(n))
				.Name == idPaul), Is.True);
			Assert.That(narratorGroupForActs.CharacterIds.All(c =>
				CharacterVerseData.GetStandardCharacterType(c) == CharacterVerseData.StandardCharacter.Narrator), Is.True);
			Assert.That(authorsGroup.CharacterIds.ToList().Intersect(narratorGroupForActs.CharacterIds.ToList()), Is.Empty);
		}

		[TestCase(30000, 2900, 1000, 999)]
		[TestCase(3000, 3000, 999, 1000)]
		[TestCase(2900, 3000, 1000, 999)]
		[TestCase(2900, 3000, 999, 1000)]
		public void GeneratePossibilities_ThreeNarrators_SplitGroupsForPaulAndPeterAndEverybodyElse(
			int charactersSpokenByNarratorOfGalatians, int charactersSpokenByNarratorOf2Peter,
			int charactersSpokenByPaul, int charactersSpokenByPeter)
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 3;

			var groups = PopulateGroups();
			PopulateKeyStrokesByCharId(charactersSpokenByNarratorOfGalatians, charactersSpokenByNarratorOf2Peter,
				charactersSpokenByPaul, charactersSpokenByPeter);
			PopulateCharacterDetails();

			var result = GeneratePossibilities(false, groups,
				m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators, 0, false,
				m_includedCharacterDetails, m_keyStrokesByCharId, m_testProject, m_characterDetails).Single();

			var narratorGroups = result.NarratorGroups;
			Assert.That(groups.Count, Is.EqualTo(narratorGroups.Count));

			var paulGroup = GetNarratorGroupForBook(narratorGroups, "GAL");
			var peterGroup = GetNarratorGroupForBook(narratorGroups, "2PE");
			var narratorGroupForActs = GetNarratorGroupForBook(narratorGroups, "ACT");

			Assert.That(paulGroup.CharacterIds.ToList(), Is.EquivalentTo(new[] { GetNarrUi("ROM"),
				GetNarrUi("1CO"), GetNarrUi("2CO"), GetNarrUi("GAL"), GetNarrUi("EPH"),
				GetNarrUi("PHP"), GetNarrUi("COL"), GetNarrUi("1TH"), GetNarrUi("2TH"),
				GetNarrUi("1TI"), GetNarrUi("2TI"), GetNarrUi("TIT"), GetNarrUi("PHM"), idPaul }));
			Assert.That(peterGroup.CharacterIds.ToList(),
				Is.EquivalentTo(new[] { GetNarrUi("1PE"), GetNarrUi("2PE"), idPeter }));
			Assert.That(narratorGroupForActs.CharacterIds.ToList(), Is.EquivalentTo(new[] {
				GetNarrUi("MAT"), GetNarrUi("MRK"), GetNarrUi("LUK"), GetNarrUi("JHN"),
				GetNarrUi("ACT"), GetNarrUi("HEB"), GetNarrUi("JAS"), GetNarrUi("1JN"),
				GetNarrUi("2JN"), GetNarrUi("3JN"), GetNarrUi("JUD"), GetNarrUi("REV") }));
		}

		[TestCase(60000, 60000, 1000, 999)]
		[TestCase(30000, 30000, 999, 1000)]
		[TestCase(52000, 2900, 1000, 999)]
		[TestCase(3000, 3000, 999, 1000)]
		[TestCase(2900, 3000, 1000, 999)]
		[TestCase(2900, 3000, 999, 1000)]
		public void GeneratePossibilities_FourNarrators_SplitGroupsForPaulAndPeterAndJamesAndEverybodyElse(
			int charactersSpokenByNarratorOfGalatians, int charactersSpokenByNarratorOf2Peter,
			int charactersSpokenByPaul, int charactersSpokenByPeter)
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 4;

			var groups = PopulateGroups();
			PopulateKeyStrokesByCharId(charactersSpokenByNarratorOfGalatians, charactersSpokenByNarratorOf2Peter,
				charactersSpokenByPaul, charactersSpokenByPeter);
			PopulateCharacterDetails();

			var result = GeneratePossibilities(false, groups,
				m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators, 0, false,
				m_includedCharacterDetails, m_keyStrokesByCharId, m_testProject, m_characterDetails).Single();

			var narratorGroups = result.NarratorGroups;
			Assert.That(groups.Count, Is.EqualTo(narratorGroups.Count));

			var paulGroup = GetNarratorGroupForBook(narratorGroups, "GAL");
			var peterGroup = GetNarratorGroupForBook(narratorGroups, "2PE");
			var jamesGroup = GetNarratorGroupForBook(narratorGroups, "JAS");
			var narratorGroupForActs = GetNarratorGroupForBook(narratorGroups, "ACT");

			Assert.That(paulGroup.CharacterIds.ToList(), Is.EquivalentTo(new[] { GetNarrUi("ROM"),
				GetNarrUi("1CO"), GetNarrUi("2CO"), GetNarrUi("GAL"), GetNarrUi("EPH"),
				GetNarrUi("PHP"), GetNarrUi("COL"), GetNarrUi("1TH"), GetNarrUi("2TH"),
				GetNarrUi("1TI"), GetNarrUi("2TI"), GetNarrUi("TIT"), GetNarrUi("PHM"), idPaul }));
			Assert.That(peterGroup.CharacterIds.ToList(),
				Is.EquivalentTo(new[] { GetNarrUi("1PE"), GetNarrUi("2PE"), idPeter }));
			Assert.That(jamesGroup.CharacterIds.ToList(),
				Is.EquivalentTo(new[] { GetNarrUi("JAS"), idJames }));
			Assert.That(narratorGroupForActs.CharacterIds.ToList(), Is.EquivalentTo(new[] {
				GetNarrUi("MAT"), GetNarrUi("MRK"), GetNarrUi("LUK"), GetNarrUi("JHN"),
				GetNarrUi("ACT"), GetNarrUi("HEB"), GetNarrUi("1JN"), GetNarrUi("2JN"),
				GetNarrUi("3JN"), GetNarrUi("JUD"), GetNarrUi("REV") }));
		}

		[TestCase(60000, 60000, 1000, 999)]
		[TestCase(30000, 30000, 999, 1000)]
		[TestCase(52000, 2900, 1000, 999)]
		[TestCase(3000, 3000, 999, 1000)]
		[TestCase(2900, 3000, 1000, 999)]
		[TestCase(2900, 3000, 999, 1000)]
		public void GeneratePossibilities_FiveNarrators_SplitGroupsForPaulAndPeterAndJamesAndTwoOtherNarratorGroups(
			int charactersSpokenByNarratorOfGalatians, int charactersSpokenByNarratorOf2Peter,
			int charactersSpokenByPaul, int charactersSpokenByPeter)
		{
			m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 5;

			var groups = PopulateGroups();
			PopulateKeyStrokesByCharId(charactersSpokenByNarratorOfGalatians, charactersSpokenByNarratorOf2Peter,
				charactersSpokenByPaul, charactersSpokenByPeter);
			PopulateCharacterDetails();

			var result = GeneratePossibilities(false, groups,
				m_testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators, 0, false,
				m_includedCharacterDetails, m_keyStrokesByCharId, m_testProject, m_characterDetails).Single();

			var narratorGroups = result.NarratorGroups;
			Assert.That(groups.Count, Is.EqualTo(narratorGroups.Count));
			Assert.That(narratorGroups.Sum(g => g.CharacterIds.Count(
				c => CharacterVerseData.IsCharacterOfType(c, CharacterVerseData.StandardCharacter.Narrator))), Is.EqualTo(27));

			var paulGroup = GetNarratorGroupForBook(narratorGroups, "GAL");
			var peterGroup = GetNarratorGroupForBook(narratorGroups, "2PE");
			var jamesGroup = GetNarratorGroupForBook(narratorGroups, "JAS");
			var johnGroup = GetNarratorGroupForBook(narratorGroups, "JHN");
			var otherNarratorGroup = GetNarratorGroupForBook(narratorGroups, "ACT");
			Assert.That(johnGroup, Is.Not.EqualTo(otherNarratorGroup));

			Assert.That(paulGroup.CharacterIds.ToList(), Is.EquivalentTo(
				new[] { GetNarrUi("ROM"), GetNarrUi("1CO"), GetNarrUi("2CO"), GetNarrUi("GAL"),
					GetNarrUi("EPH"), GetNarrUi("PHP"), GetNarrUi("COL"), GetNarrUi("1TH"),
					GetNarrUi("2TH"), GetNarrUi("1TI"), GetNarrUi("2TI"), GetNarrUi("TIT"),
					GetNarrUi("PHM"), idPaul }));
			Assert.That(peterGroup.CharacterIds.ToList(),
				Is.EquivalentTo(new[] { GetNarrUi("1PE"), GetNarrUi("2PE"), idPeter }));
			Assert.That(jamesGroup.CharacterIds.ToList(),
				Is.EquivalentTo(new[] { GetNarrUi("JAS"), idJames }));
			// Algorithm will group another small book with John's
			Assert.That(johnGroup.CharacterIds.IsProperSupersetOf(new[] { "narrator-JHN",
				"narrator-1JN", "narrator-2JN", "narrator-3JN", "narrator-REV" }));
			Assert.That(otherNarratorGroup.CharacterIds.IsProperSupersetOf(
				new[] { "narrator-ACT", "narrator-MRK" }));
		}

		private string GetNarrUi(string bookId)
		{
			return CharacterVerseData.GetCharacterNameForUi(CharacterVerseData.GetStandardCharacterId(
				bookId, CharacterVerseData.StandardCharacter.Narrator));
		}
	}
}
