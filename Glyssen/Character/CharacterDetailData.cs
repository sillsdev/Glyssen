using System;
using System.Collections.Generic;
using System.Linq;
using Glyssen.Properties;
using SIL.Scripture;

namespace Glyssen.Character
{
	public class CharacterDetailData
	{
		private static CharacterDetailData s_singleton;
		private static string s_tabDelimitedCharacterDetailData;
		private IList<CharacterDetail> m_data;
		private Dictionary<string, CharacterDetail> m_dictionary;

		internal static string TabDelimitedCharacterDetailData
		{
			get { return s_tabDelimitedCharacterDetailData; }
			set
			{
				s_tabDelimitedCharacterDetailData = value;
				s_singleton = null;
			}
		}

		private CharacterDetailData()
		{
			// Tests can set this before accessing the Singleton.
			if (TabDelimitedCharacterDetailData == null)
				TabDelimitedCharacterDetailData = Resources.CharacterDetail;
			LoadData(TabDelimitedCharacterDetailData);
		}

		public static CharacterDetailData Singleton => s_singleton ?? (s_singleton = new CharacterDetailData());

		public static bool CharacterIsMale(string characterId, bool treatEitherAsMale = true)
		{
			var gender = Singleton.GetDictionary()[characterId].Gender;
			return gender == CharacterGender.Male || gender == CharacterGender.PreferMale ||
				(treatEitherAsMale && gender == CharacterGender.Either);
		}

		public static bool CharacterIsFemale(string characterId)
		{
			var gender = Singleton.GetDictionary()[characterId].Gender;
			return gender == CharacterGender.Female || gender == CharacterGender.PreferFemale;
		}

		public IEnumerable<CharacterDetail> GetAll()
		{
			return m_data;
		}

		public Dictionary<string, CharacterDetail> GetDictionary()
		{
			if (m_dictionary != null)
				return m_dictionary;
			return m_dictionary = m_data.ToDictionary(k => k.CharacterId);
		}

		public IEnumerable<string> GetAllCharacterIdsAsLowerInvariant()
		{
			return m_data.Select(c => c.CharacterId.ToLowerInvariant());
		}

		private void LoadData(string tabDelimitedCharacterDetailData)
		{
			var list = new List<CharacterDetail>();
			int lineNumber = 0;
			foreach (var line in tabDelimitedCharacterDetailData.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
			{
				string[] items = line.Split(new[] { "\t" }, StringSplitOptions.None);
				CharacterDetail detail = ProcessLine(items, lineNumber++);
				if (detail != null)
					list.Add(detail);
			}
			AddStandardCharacters(list);
			AddPsalmistsForPsalm119(list);

			m_data = list;
		}

		private void AddStandardCharacters(List<CharacterDetail> list)
		{
			for (int booknum = 1; booknum <= BCVRef.LastBook; booknum++)
			{
				string bookCode = BCVRef.NumberToBookCode(booknum);
				list.Add(new CharacterDetail
				{
					CharacterId = CharacterVerseData.GetStandardCharacterId(bookCode, CharacterVerseData.StandardCharacter.Narrator),
					Gender = CharacterGender.Neuter,
					StandardCharacterType = CharacterVerseData.StandardCharacter.Narrator,
					//Status = true
				});
				list.Add(new CharacterDetail
				{
					CharacterId = CharacterVerseData.GetStandardCharacterId(bookCode, CharacterVerseData.StandardCharacter.BookOrChapter),
					Gender = CharacterGender.Neuter,
					StandardCharacterType = CharacterVerseData.StandardCharacter.BookOrChapter
				});
				list.Add(new CharacterDetail
				{
					CharacterId = CharacterVerseData.GetStandardCharacterId(bookCode, CharacterVerseData.StandardCharacter.ExtraBiblical),
					Gender = CharacterGender.Neuter,
					StandardCharacterType = CharacterVerseData.StandardCharacter.ExtraBiblical
				});
				list.Add(new CharacterDetail
				{
					CharacterId = CharacterVerseData.GetStandardCharacterId(bookCode, CharacterVerseData.StandardCharacter.Intro),
					Gender = CharacterGender.Neuter,
					StandardCharacterType = CharacterVerseData.StandardCharacter.Intro
				});
			}
		}

		private void AddPsalmistsForPsalm119(List<CharacterDetail> list)
		{
			list.AddRange(HebrewLetters.Select(h => new CharacterDetail
			{
				CharacterId = $"psalmist ({h})",
				Gender = CharacterGender.Neuter
			}));
		}

		public static IEnumerable<string> HebrewLetters => new[]
		{
			"Aleph",
			"Beth",
			"Gimel",
			"Daleth",
			"He",
			"Waw",
			"Zayin",
			"Chet",
			"Tet",
			"Yod",
			"Kaf",
			"Lamed",
			"Mem",
			"Nun",
			"Samekh",
			"Ayin",
			"Pe",
			"Tsadi",
			"Qof",
			"Resh",
			"Sin and Shin",
			"Tav"
		};

		private CharacterDetail ProcessLine(string[] items, int lineNumber)
		{
			if (lineNumber == 0)
				return null;

			CharacterGender gender;
			if (!Enum.TryParse(items[2], false, out gender))
				gender = CharacterGender.Either;
			CharacterAge age;
			if (!Enum.TryParse(items[3], false, out age))
				age = CharacterAge.Adult;
			return new CharacterDetail
			{
				CharacterId = items[0],
				MaxSpeakers = Int32.Parse(items[1]),
				Gender = gender,
				Age = age,
				//Status = items[4].Equals("Y", StringComparison.OrdinalIgnoreCase) || items[4].Equals("True", StringComparison.OrdinalIgnoreCase),
				Comment = items[5],
				ReferenceComment = items.Length >= 7 ? items[6] : null,
				DefaultFCBHCharacter = items.Length >= 8 ? items[7] : null
			};
		}
	}
}
