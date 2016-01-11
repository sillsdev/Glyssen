using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Glyssen;
using Glyssen.Character;
using Glyssen.Rules;
using NUnit.Framework;

namespace GlyssenTests.Rules
{
	[TestFixture]
	class TrialGroupConfigurationTestsSmall
	{
		[Test]
		public void DistributeAuthorsAmongNarratorGroups_FourAuthorsAmongThreeNarrators_TwoAuthorsWithShortestBooksCombined()
		{
			var keyStrokesByBook = new Dictionary<string, int>();
			keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("JER", CharacterVerseData.StandardCharacter.Narrator)] = 52000;
			keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("EZK", CharacterVerseData.StandardCharacter.Narrator)] = 48000;
			keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("HOS", CharacterVerseData.StandardCharacter.Narrator)] = 12000;
			keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.Narrator)] = 1000;
			var authorStats = new List<AuthorStats>();
			var jeremiah = new BiblicalAuthors.Author { Name = "Jeremiah" };
			var ezekiel = new BiblicalAuthors.Author { Name = "Ezekiel" };
			var hosea = new BiblicalAuthors.Author { Name = "Hosea" };
			var jude = new BiblicalAuthors.Author { Name = "Jude" };
			authorStats.Add(new AuthorStats(jeremiah, "JER", keyStrokesByBook));
			authorStats.Add(new AuthorStats(ezekiel, "EZK", keyStrokesByBook));
			authorStats.Add(new AuthorStats(hosea, "HOS", keyStrokesByBook));
			authorStats.Add(new AuthorStats(jude, "JUD", keyStrokesByBook));

			var narratorGroups = new List<CharacterGroup>
			{
				new CharacterGroup {Name = "1"},
				new CharacterGroup {Name = "2"},
				new CharacterGroup {Name = "3"},
			};

			var result = CharacterGroupGenerator.TrialGroupConfiguration.DistributeAuthorsAmongNarratorGroups(authorStats, narratorGroups);
			Assert.AreEqual(4, result.Count);
			Assert.AreNotEqual(result[jeremiah], result[ezekiel]);
			Assert.AreNotEqual(result[jeremiah], result[jude]);
			Assert.AreEqual(result[jude], result[hosea]);
		}

		[Test]
		public void DistributeAuthorsAmongNarratorGroups_SixSimilarAuthorsAmongThreeNarrators_AuthorOfLargestBookCombinesWithShortestEtc()
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
			var moses = new BiblicalAuthors.Author { Name = "Moses" };
			var isaiah = new BiblicalAuthors.Author { Name = "Isaiah" };
			var jeremiah = new BiblicalAuthors.Author { Name = "Jeremiah" };
			var ezekiel = new BiblicalAuthors.Author { Name = "Ezekiel" };
			var luke = new BiblicalAuthors.Author { Name = "Luke" };
			var john = new BiblicalAuthors.Author { Name = "John" };
			authorStats.Add(new AuthorStats(moses, "GEN", keyStrokesByBook));
			authorStats.Add(new AuthorStats(isaiah, "ISA", keyStrokesByBook));
			authorStats.Add(new AuthorStats(jeremiah, "JER", keyStrokesByBook));
			authorStats.Add(new AuthorStats(ezekiel, "EZK", keyStrokesByBook));
			var lukeStats = new AuthorStats(luke, "LUK", keyStrokesByBook);
			lukeStats.AddBook("ACT");
			authorStats.Add(lukeStats);
			var johnStats = new AuthorStats(john, "JHN", keyStrokesByBook);
			johnStats.AddBook("REV");
			authorStats.Add(johnStats);

			var narratorGroups = new List<CharacterGroup>
			{
				new CharacterGroup {Name = "1"},
				new CharacterGroup {Name = "2"},
				new CharacterGroup {Name = "3"},
			};

			var result = CharacterGroupGenerator.TrialGroupConfiguration.DistributeAuthorsAmongNarratorGroups(authorStats, narratorGroups);

			Assert.AreEqual(6, result.Count);
			// Since there are two authors with exactly 52000 keystrokes, we can't know for sure which one will combine with GEN and
			// which will combine with EZK. So we just assert that they are grouped properly.
			Assert.AreNotEqual(result[isaiah], result[jeremiah]);
			Assert.AreNotEqual(result[isaiah], result[ezekiel]);
			Assert.AreNotEqual(result[isaiah], result[moses]);
			Assert.AreNotEqual(result[moses], result[ezekiel]);
			Assert.AreEqual(result[isaiah], result[john]);
			Assert.AreEqual(2, result.Count(g => g.Value.Name == "1"));
			Assert.AreEqual(2, result.Count(g => g.Value.Name == "2"));
			Assert.AreEqual(2, result.Count(g => g.Value.Name == "3"));
		}
	}


	[TestFixture]
	internal class TrialGroupConfigurationTestsWholeBible
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
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("GEN", CharacterVerseData.StandardCharacter.Narrator)] =
				50000; // MOSES
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("EXO", CharacterVerseData.StandardCharacter.Narrator)] =
				40000; // MOSES
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("LEV", CharacterVerseData.StandardCharacter.Narrator)] =
				27000; // MOSES
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("NUM", CharacterVerseData.StandardCharacter.Narrator)] =
				36000; // MOSES
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("DEU", CharacterVerseData.StandardCharacter.Narrator)] =
				34000; // MOSES:     187000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("JOS", CharacterVerseData.StandardCharacter.Narrator)] =
				24000; // JOSHUA:     24000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("JDG", CharacterVerseData.StandardCharacter.Narrator)] =
				21000; // JUDGES:     21000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("RUT", CharacterVerseData.StandardCharacter.Narrator)] =
				4000; // RUTH:        4000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("1SA", CharacterVerseData.StandardCharacter.Narrator)] =
				31000; // SAMUEL
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("2SA", CharacterVerseData.StandardCharacter.Narrator)] =
				24000; // SAMUEL:     55000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("1KI", CharacterVerseData.StandardCharacter.Narrator)] =
				24000; // KINGS
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("2KI", CharacterVerseData.StandardCharacter.Narrator)] =
				25000; // KINGS:      49000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("1CH", CharacterVerseData.StandardCharacter.Narrator)] =
				35000; // CHRONICLES
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("2CH", CharacterVerseData.StandardCharacter.Narrator)] =
				36000; // CHRONICLES: 71000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("EZR", CharacterVerseData.StandardCharacter.Narrator)] =
				10000; // EZRA:       10000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("NEH", CharacterVerseData.StandardCharacter.Narrator)] =
				13000; // NEHEMIAH:   13000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("EST", CharacterVerseData.StandardCharacter.Narrator)] =
				10000; // ESTHER:     10000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("JOB", CharacterVerseData.StandardCharacter.Narrator)] =
				42000; // JOB:        42000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("PSA", CharacterVerseData.StandardCharacter.Narrator)] =
				99999; // PSALMS:     99999
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("PRO", CharacterVerseData.StandardCharacter.Narrator)] =
				31000; // SOLOMON
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("ECC", CharacterVerseData.StandardCharacter.Narrator)] =
				12000; // SOLOMON
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("SNG", CharacterVerseData.StandardCharacter.Narrator)] =
				8000; // SOLOMON:     51000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("ISA", CharacterVerseData.StandardCharacter.Narrator)] =
				66000; // ISAIAH:     66000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("JER", CharacterVerseData.StandardCharacter.Narrator)] =
				52000; // JEREMIAH
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("LAM", CharacterVerseData.StandardCharacter.Narrator)] =
				5000; // JEREMIAH:    57000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("EZK", CharacterVerseData.StandardCharacter.Narrator)] =
				48000; // EZEKIEL:    48000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("DAN", CharacterVerseData.StandardCharacter.Narrator)] =
				12000; // DANIEL:     12000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("HOS", CharacterVerseData.StandardCharacter.Narrator)] =
				14000; // HOSEA:      14000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("JOL", CharacterVerseData.StandardCharacter.Narrator)] =
				3000; // JOEL:         3000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("AMO", CharacterVerseData.StandardCharacter.Narrator)] =
				9000; // AMOS:         9000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("OBA", CharacterVerseData.StandardCharacter.Narrator)] =
				1000; // OBADIAH:      1000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("JON", CharacterVerseData.StandardCharacter.Narrator)] =
				4000; // JONAH:        4000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("MIC", CharacterVerseData.StandardCharacter.Narrator)] =
				7000; // MICAH:        7000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("NAM", CharacterVerseData.StandardCharacter.Narrator)] =
				3000; // NAHUM:        3000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("HAB", CharacterVerseData.StandardCharacter.Narrator)] =
				3000; // HABAKKUK:     3000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("ZEP", CharacterVerseData.StandardCharacter.Narrator)] =
				3000; // ZEPHANIAH:    3000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("HAG", CharacterVerseData.StandardCharacter.Narrator)] =
				2000; // HAGGAI:       2000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("ZEC", CharacterVerseData.StandardCharacter.Narrator)] =
				3000; // ZECHARIAH:    3000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("MAL", CharacterVerseData.StandardCharacter.Narrator)] =
				4000; // MALACHI:      4000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator)] =
				28000; // MATTHEW:    28000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.Narrator)] =
				16000; // MARK:       16000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("LUK", CharacterVerseData.StandardCharacter.Narrator)] =
				24000; // LUKE
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("ACT", CharacterVerseData.StandardCharacter.Narrator)] =
				28000; // LUKE:       52000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("JHN", CharacterVerseData.StandardCharacter.Narrator)] =
				20000; // JOHN
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("JHN", CharacterVerseData.StandardCharacter.Narrator)] =
				20000; // JOHN
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("1JN", CharacterVerseData.StandardCharacter.Narrator)] =
				5000; // JOHN
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("2JN", CharacterVerseData.StandardCharacter.Narrator)] =
				1000; // JOHN
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("3JN", CharacterVerseData.StandardCharacter.Narrator)] =
				1000; // JOHN
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("REV", CharacterVerseData.StandardCharacter.Narrator)] =
				22000; // JOHN:       49000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("ROM", CharacterVerseData.StandardCharacter.Narrator)] =
				16000; // PAUL
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("1CO", CharacterVerseData.StandardCharacter.Narrator)] =
				16000; // PAUL
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("2CO", CharacterVerseData.StandardCharacter.Narrator)] =
				13000; // PAUL
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("GAL", CharacterVerseData.StandardCharacter.Narrator)] =
				6000; // PAUL
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("EPH", CharacterVerseData.StandardCharacter.Narrator)] =
				6000; // PAUL
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("PHP", CharacterVerseData.StandardCharacter.Narrator)] =
				4000; // PAUL
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("COL", CharacterVerseData.StandardCharacter.Narrator)] =
				4000; // PAUL
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("1TH", CharacterVerseData.StandardCharacter.Narrator)] =
				5000; // PAUL
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("2TH", CharacterVerseData.StandardCharacter.Narrator)] =
				3000; // PAUL
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("1TI", CharacterVerseData.StandardCharacter.Narrator)] =
				6000; // PAUL
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("2TI", CharacterVerseData.StandardCharacter.Narrator)] =
				4000; // PAUL
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("TIT", CharacterVerseData.StandardCharacter.Narrator)] =
				3000; // PAUL
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("PHM", CharacterVerseData.StandardCharacter.Narrator)] =
				1000; // PAUL:        87000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("HEB", CharacterVerseData.StandardCharacter.Narrator)] =
				13000; // HEBREWS:    13000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("JAS", CharacterVerseData.StandardCharacter.Narrator)] =
				5000; // JAMES:        5000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("1PE", CharacterVerseData.StandardCharacter.Narrator)] =
				5000; // PETER
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("2PE", CharacterVerseData.StandardCharacter.Narrator)] =
				3000; // PETER:        8000
			m_keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.Narrator)] =
				1000; // JUDE:         1000

			m_authorStats = new List<AuthorStats>();

			m_moses = new BiblicalAuthors.Author {Name = "Moses"};
			m_joshua = new BiblicalAuthors.Author {Name = "Joshua"};
			m_judges = new BiblicalAuthors.Author {Name = "Author of Judges"};
			m_ruth = new BiblicalAuthors.Author {Name = "Ruth"};
			m_samuel = new BiblicalAuthors.Author {Name = "Samuel"};
			m_kings = new BiblicalAuthors.Author {Name = "Author of Kings"};
			m_chronicles = new BiblicalAuthors.Author {Name = "Author of "};
			m_ezra = new BiblicalAuthors.Author {Name = "Ezra"};
			m_nehemiah = new BiblicalAuthors.Author {Name = "Nehemiah"};
			m_esther = new BiblicalAuthors.Author {Name = "Author of Esther"};
			m_job = new BiblicalAuthors.Author {Name = "Job"};
			m_psalms = new BiblicalAuthors.Author {Name = "Author of Psalms"};
			m_solomon = new BiblicalAuthors.Author {Name = "Solomon"};
			m_isaiah = new BiblicalAuthors.Author {Name = "Isaiah"};
			m_jeremiah = new BiblicalAuthors.Author {Name = "Jeremiah"};
			m_ezekiel = new BiblicalAuthors.Author {Name = "Ezekiel"};
			m_daniel = new BiblicalAuthors.Author {Name = "Daniel"};
			m_hosea = new BiblicalAuthors.Author {Name = "Hosea"};
			m_joel = new BiblicalAuthors.Author {Name = "Joel"};
			m_amos = new BiblicalAuthors.Author {Name = "Amos"};
			m_obadiah = new BiblicalAuthors.Author {Name = "Obadiah"};
			m_jonah = new BiblicalAuthors.Author {Name = "Jonah"};
			m_micah = new BiblicalAuthors.Author {Name = "Micah"};
			m_nahum = new BiblicalAuthors.Author {Name = "Nahum"};
			m_habakkuk = new BiblicalAuthors.Author {Name = "Habakkuk"};
			m_zephaniah = new BiblicalAuthors.Author {Name = "Zephaniah"};
			m_haggai = new BiblicalAuthors.Author {Name = "Haggai"};
			m_zechariah = new BiblicalAuthors.Author {Name = "Zechariah"};
			m_malachi = new BiblicalAuthors.Author {Name = "Malachi"};
			m_matthew = new BiblicalAuthors.Author {Name = "Matthew"};
			m_mark = new BiblicalAuthors.Author {Name = "Mark"};
			m_luke = new BiblicalAuthors.Author {Name = "Luke"};
			m_john = new BiblicalAuthors.Author {Name = "John"};
			m_paul = new BiblicalAuthors.Author {Name = "paul"};
			m_hebrews = new BiblicalAuthors.Author {Name = "hebrews"};
			m_james = new BiblicalAuthors.Author {Name = "James"};
			m_peter = new BiblicalAuthors.Author {Name = "Peter"};
			m_jude = new BiblicalAuthors.Author {Name = "Jude"};

			var mosesStats = new AuthorStats(m_moses, "GEN", m_keyStrokesByBook);
			mosesStats.AddBook("EXO");
			mosesStats.AddBook("LEV");
			mosesStats.AddBook("NUM");
			mosesStats.AddBook("DEU");
			m_authorStats.Add(mosesStats);
			m_authorStats.Add(new AuthorStats(m_joshua, "JOS", m_keyStrokesByBook));
			m_authorStats.Add(new AuthorStats(m_judges, "JDG", m_keyStrokesByBook));
			m_authorStats.Add(new AuthorStats(m_ruth, "RUT", m_keyStrokesByBook));
			var samuelStats = new AuthorStats(m_samuel, "1SA", m_keyStrokesByBook);
			samuelStats.AddBook("2SA");
			m_authorStats.Add(samuelStats);
			var kingsStats = new AuthorStats(m_kings, "1KI", m_keyStrokesByBook);
			kingsStats.AddBook("2KI");
			m_authorStats.Add(kingsStats);
			var chroniclesStats = new AuthorStats(m_chronicles, "1CH", m_keyStrokesByBook);
			chroniclesStats.AddBook("2CH");
			m_authorStats.Add(chroniclesStats);
			m_authorStats.Add(new AuthorStats(m_ezra, "EZR", m_keyStrokesByBook));
			m_authorStats.Add(new AuthorStats(m_nehemiah, "NEH", m_keyStrokesByBook));
			m_authorStats.Add(new AuthorStats(m_esther, "EST", m_keyStrokesByBook));
			m_authorStats.Add(new AuthorStats(m_job, "JOB", m_keyStrokesByBook));
			m_authorStats.Add(new AuthorStats(m_psalms, "PSA", m_keyStrokesByBook));
			var solomonStats = new AuthorStats(m_solomon, "PRO", m_keyStrokesByBook);
			solomonStats.AddBook("ECC");
			solomonStats.AddBook("SNG");
			m_authorStats.Add(solomonStats);
			m_authorStats.Add(new AuthorStats(m_isaiah, "ISA", m_keyStrokesByBook));
			var jeremiahStats = new AuthorStats(m_jeremiah, "JER", m_keyStrokesByBook);
			jeremiahStats.AddBook("LAM");
			m_authorStats.Add(jeremiahStats);
			m_authorStats.Add(new AuthorStats(m_ezekiel, "EZK", m_keyStrokesByBook));
			m_authorStats.Add(new AuthorStats(m_daniel, "DAN", m_keyStrokesByBook));
			m_authorStats.Add(new AuthorStats(m_hosea, "HOS", m_keyStrokesByBook));
			m_authorStats.Add(new AuthorStats(m_joel, "JOL", m_keyStrokesByBook));
			m_authorStats.Add(new AuthorStats(m_amos, "AMO", m_keyStrokesByBook));
			m_authorStats.Add(new AuthorStats(m_obadiah, "OBA", m_keyStrokesByBook));
			m_authorStats.Add(new AuthorStats(m_jonah, "JON", m_keyStrokesByBook));
			m_authorStats.Add(new AuthorStats(m_micah, "MIC", m_keyStrokesByBook));
			m_authorStats.Add(new AuthorStats(m_nahum, "NAM", m_keyStrokesByBook));
			m_authorStats.Add(new AuthorStats(m_habakkuk, "HAB", m_keyStrokesByBook));
			m_authorStats.Add(new AuthorStats(m_zephaniah, "ZEP", m_keyStrokesByBook));
			m_authorStats.Add(new AuthorStats(m_haggai, "HAG", m_keyStrokesByBook));
			m_authorStats.Add(new AuthorStats(m_zechariah, "ZEC", m_keyStrokesByBook));
			m_authorStats.Add(new AuthorStats(m_malachi, "MAL", m_keyStrokesByBook));
			m_authorStats.Add(new AuthorStats(m_matthew, "MAT", m_keyStrokesByBook));
			m_authorStats.Add(new AuthorStats(m_mark, "MRK", m_keyStrokesByBook));
			var lukeStats = new AuthorStats(m_luke, "LUK", m_keyStrokesByBook);
			lukeStats.AddBook("ACT");
			m_authorStats.Add(lukeStats);
			var johnStats = new AuthorStats(m_john, "JHN", m_keyStrokesByBook);
			johnStats.AddBook("1JN");
			johnStats.AddBook("2JN");
			johnStats.AddBook("3JN");
			johnStats.AddBook("REV");
			m_authorStats.Add(johnStats);
			var paulStats = new AuthorStats(m_paul, "ROM", m_keyStrokesByBook);
			paulStats.AddBook("1CO");
			paulStats.AddBook("2CO");
			paulStats.AddBook("GAL");
			paulStats.AddBook("EPH");
			paulStats.AddBook("PHP");
			paulStats.AddBook("COL");
			paulStats.AddBook("1TH");
			paulStats.AddBook("2TH");
			paulStats.AddBook("1TI");
			paulStats.AddBook("2TI");
			paulStats.AddBook("TIT");
			paulStats.AddBook("PHM");
			m_authorStats.Add(paulStats);
			m_authorStats.Add(new AuthorStats(m_hebrews, "HEB", m_keyStrokesByBook));
			m_authorStats.Add(new AuthorStats(m_james, "JAS", m_keyStrokesByBook));
			var peterStats = new AuthorStats(m_peter, "1PE", m_keyStrokesByBook);
			peterStats.AddBook("2PE");
			m_authorStats.Add(peterStats);
			m_authorStats.Add(new AuthorStats(m_jude, "JUD", m_keyStrokesByBook));
			Assert.AreEqual(38, m_authorStats.Count);
		}

		private List<CharacterGroup> GetNarratorCharacterGroups(int n)
		{
			var narratorGroups = new List<CharacterGroup>(n);
			for (int i = 0; i < n; i++)
			{
				narratorGroups.Add(new CharacterGroup {Name = (i + 1).ToString()});
			}
			return narratorGroups;
		}

		[Test]
		public void DistributeAuthorsAmongNarratorGroups_ThreeNarrators_AuthorsCombineCorrectly()
		{
			var narratorGroups = GetNarratorCharacterGroups(3);

			var result = CharacterGroupGenerator.TrialGroupConfiguration.DistributeAuthorsAmongNarratorGroups(m_authorStats,
				narratorGroups);

			Assert.AreEqual(38, result.Count);
			Assert.AreNotEqual(result[m_moses], result[m_psalms]);
			Assert.AreNotEqual(result[m_moses], result[m_paul]);
			Assert.AreNotEqual(result[m_psalms], result[m_paul]);
			Assert.AreEqual(result[m_moses], result[m_obadiah]);
			Assert.AreEqual(result[m_moses], result[m_jude]);
			Assert.AreEqual(25, result.Count(g => g.Value.Name == "1"));
			Assert.AreEqual(result[m_moses], result[m_obadiah]);
			// Etc.
			Assert.AreEqual(result[m_moses], result[m_joshua]);
			Assert.AreEqual(7, result.Count(g => g.Value.Name == "2"));
			Assert.AreEqual(result[m_psalms], result[m_matthew]);
			Assert.AreEqual(result[m_psalms], result[m_job]);
			Assert.AreEqual(result[m_psalms], result[m_ezekiel]);
			Assert.AreEqual(result[m_psalms], result[m_kings]);
			Assert.AreEqual(result[m_psalms], result[m_john]);
			Assert.AreEqual(result[m_psalms], result[m_solomon]);
			Assert.AreEqual(6, result.Count(g => g.Value.Name == "3"));
			Assert.AreEqual(result[m_paul], result[m_luke]);
			Assert.AreEqual(result[m_paul], result[m_samuel]);
			Assert.AreEqual(result[m_paul], result[m_jeremiah]);
			Assert.AreEqual(result[m_paul], result[m_isaiah]);
			Assert.AreEqual(result[m_paul], result[m_chronicles]);
		}

		[Test]
		public void DistributeAuthorsAmongNarratorGroups_ThirtySevenNarrators_BottomTwoAuthorsCombine()
		{
			var narratorGroups = GetNarratorCharacterGroups(37);

			var result = CharacterGroupGenerator.TrialGroupConfiguration.DistributeAuthorsAmongNarratorGroups(m_authorStats,
				narratorGroups);

			Assert.AreEqual(38, result.Count);
			for (int i = 1; i <= 36; i++)
				Assert.AreEqual(1, result.Count(g => g.Value.Name == i.ToString()));

			Assert.AreEqual(2, result.Count(g => g.Value.Name == "37"));
			Assert.AreEqual(result[m_jude], result[m_obadiah]);
		}

		[Test]
		public void DistributeAuthorsAmongNarratorGroups_ThirtySixNarrators_BottomFourAuthorsCombineIntoTwoGroups()
		{
			var narratorGroups = GetNarratorCharacterGroups(36);

			var result = CharacterGroupGenerator.TrialGroupConfiguration.DistributeAuthorsAmongNarratorGroups(m_authorStats,
				narratorGroups);

			Assert.AreEqual(38, result.Count);
			for (int i = 1; i <= 34; i++)
				Assert.AreEqual(1, result.Count(g => g.Value.Name == i.ToString()));

			Assert.AreEqual(2, result.Count(g => g.Value.Name == "35"));
			Assert.AreEqual(2, result.Count(g => g.Value.Name == "36"));
			// Obadiah and Jude are tied for the fewest number of keystrokes. Haggai is by itself in second-to-last place.
			// Joel, Nahum, Habakkuk, Zephaniah, and Zechariah are all tied for third-to-last place.
			var authorCombinedWithHaggai = result.Single(g => g.Key != m_haggai && g.Value.Name == "36").Key;
			BiblicalAuthors.Author authorCombinedWithThirdToLastPlaceAuthor;
			if (authorCombinedWithHaggai == m_jude)
				authorCombinedWithThirdToLastPlaceAuthor = m_obadiah;
			else
			{
				Assert.AreEqual(m_obadiah, authorCombinedWithHaggai);
				authorCombinedWithThirdToLastPlaceAuthor = m_jude;
			}
			var thirdToLastPlaceAuthorThatGotCombined =
				result.Single(g => g.Key != authorCombinedWithThirdToLastPlaceAuthor && g.Value.Name == "35").Key;
			Assert.IsTrue(
				thirdToLastPlaceAuthorThatGotCombined == m_joel ||
				thirdToLastPlaceAuthorThatGotCombined == m_nahum ||
				thirdToLastPlaceAuthorThatGotCombined == m_habakkuk ||
				thirdToLastPlaceAuthorThatGotCombined == m_zephaniah ||
				thirdToLastPlaceAuthorThatGotCombined == m_zechariah);
		}

		[Test]
		public void DistributeAuthorsAmongNarratorGroups_ThirtyTwoNarrators_BottomTwelveAuthorsCombineIntoSixGroupsOfTwo()
		{
			var narratorGroups = GetNarratorCharacterGroups(32);

			var result = CharacterGroupGenerator.TrialGroupConfiguration.DistributeAuthorsAmongNarratorGroups(m_authorStats,
				narratorGroups);

			Assert.AreEqual(38, result.Count);
			for (int i = 1; i <= 26; i++)
				Assert.AreEqual(1, result.Count(g => g.Value.Name == i.ToString()));
			for (int i = 27; i <= 32; i++)
				Assert.AreEqual(2, result.Count(g => g.Value.Name == i.ToString()));
		}

		[Test]
		public void DistributeAuthorsAmongNarratorGroups_FifteenNarrators_MostProlificAuthorsDoNotGetCombined()
		{
			var narratorGroups = GetNarratorCharacterGroups(15);

			var result = CharacterGroupGenerator.TrialGroupConfiguration.DistributeAuthorsAmongNarratorGroups(m_authorStats,
				narratorGroups);

			Assert.AreEqual(38, result.Count);
			for (int i = 1; i <= 5; i++)
				Assert.AreEqual(1, result.Count(g => g.Value.Name == i.ToString()));
		}

		[Test]
		[Category("ByHand")]
		public void DistributeAuthorsAmongNarratorGroups_AllCombinations_ManualCheck()
		{
			for (int i = 2; i <= 37; i++)
			{
				var narratorGroups = GetNarratorCharacterGroups(i);

				var result = CharacterGroupGenerator.TrialGroupConfiguration.DistributeAuthorsAmongNarratorGroups(m_authorStats,
					narratorGroups);

				Assert.AreEqual(38, result.Count);
				Trace.WriteLine(i + " Narrator Groups");
				Trace.WriteLine("====================");
				foreach (var narratorGroup in narratorGroups)
				{
					var totalKeyStrokesForNarrator = result.Where(g => g.Value == narratorGroup).Sum(g => m_authorStats.Single(s => s.Author == g.Key).KeyStrokeCount);
					Trace.WriteLine("    " + narratorGroup.Name + ": " + totalKeyStrokesForNarrator);
				}
				Trace.WriteLine("");
			}
		}
	}
}
