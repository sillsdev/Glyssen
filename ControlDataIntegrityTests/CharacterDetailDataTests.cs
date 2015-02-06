using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
			ISet<string> missingCharacters = new HashSet<string>();
			ISet<string> missingDefaultCharacters = new HashSet<string>();
			foreach (CharacterVerse cv in ControlCharacterVerseData.Singleton.GetAllQuoteInfo())
			{
				if (!charactersHavingDetail.Contains(cv.Character))
					missingCharacters.Add(cv.Character);
				if (!(string.IsNullOrEmpty(cv.DefaultCharacter) || charactersHavingDetail.Contains(cv.DefaultCharacter)))
					missingDefaultCharacters.Add(cv.DefaultCharacter);
			}
			Assert.False(missingCharacters.Any() || missingDefaultCharacters.Any(),
				"Characters in Character-Verse data but not in Character-Detail:" +
				Environment.NewLine +
				OnePerLineWithIndent(missingCharacters) +
				Environment.NewLine +
				"Default characters in Character-Verse data but not in Character-Detail:" +
				Environment.NewLine +
				OnePerLineWithIndent(missingDefaultCharacters));
		}

		[Test]
		public void DataIntegrity_AllCharacterDetailsHaveCharacterIdOrDefaultCharacter()
		{
			IEnumerable<string> charactersIds = ControlCharacterVerseData.Singleton.GetAllQuoteInfo().Select(d => d.Character);
			IEnumerable<string> defaultCharacters = ControlCharacterVerseData.Singleton.GetAllQuoteInfo().Select(d => d.DefaultCharacter);
			ISet<string> missingCharacters = new HashSet<string>();
			foreach (string character in CharacterDetailData.Singleton.GetAll().Select(d => d.Character))
				if (!(charactersIds.Contains(character) || defaultCharacters.Contains(character)))
					missingCharacters.Add(character);
			Assert.False(missingCharacters.Any(),
				"Characters in Character-Detail data but not in Character-Verse data:" +
				Environment.NewLine +
				OnePerLineWithIndent(missingCharacters));
		}

		private string OnePerLineWithIndent(IEnumerable<string> enumerable)
		{
			var sb = new StringBuilder();
			foreach (string item in enumerable)
				sb.Append("\t").Append(item).Append(Environment.NewLine);
			return sb.ToString();
		}
	}
}
