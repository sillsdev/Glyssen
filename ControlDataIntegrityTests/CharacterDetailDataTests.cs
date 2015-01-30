using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ProtoScript.Character;
using ProtoScript.Properties;

namespace ControlDataIntegrityTests
{
	[TestFixture]
	public class CharacterDetailDataTests
	{
		[Test]
		public void DataIntegrity_NoDuplicateLines()
		{
			string[] allLines = Resources.CharacterVerseData.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

			var set = new HashSet<string>();
			foreach (var line in allLines)
			{
				if (line.StartsWith("#"))
					continue;
				Assert.IsTrue(set.Add(line), "Duplicate line: " + line);
			}
		}

		[Test]
		public void DataIntegrity_AllCharacterIdsAndDefaultCharactersHaveCharacterDetail()
		{
			IEnumerable<string> charactersHavingDetail = CharacterDetailData.Singleton.GetAll().Select(d => d.Character);
			foreach (CharacterVerse cv in ControlCharacterVerseData.Singleton.GetAllQuoteInfo())
			{
				Assert.True(charactersHavingDetail.Contains(cv.Character),
					"Character '" + cv.Character + "' is in Character-Verse data but not in Character-Detail");
				Assert.True(string.IsNullOrEmpty(cv.DefaultCharacter) || charactersHavingDetail.Contains(cv.DefaultCharacter),
					"Default character '" + cv.DefaultCharacter + "' is in Character-Verse data but not in Character-Detail");
			}
		}

		[Test]
		public void DataIntegrity_AllCharacterDetailsHaveCharacterIdOrDefaultCharacter()
		{
			IEnumerable<string> charactersIds = ControlCharacterVerseData.Singleton.GetAllQuoteInfo().Select(d => d.Character);
			IEnumerable<string> defaultCharacters = ControlCharacterVerseData.Singleton.GetAllQuoteInfo().Select(d => d.DefaultCharacter);
			foreach (string character in CharacterDetailData.Singleton.GetAll().Select(d => d.Character))
			{
				Assert.True(charactersIds.Contains(character) || defaultCharacters.Contains(character),
					"Character '" + character + "' is in Character-Detail data but not in Character-Verse data");
			}
		}
	}
}
