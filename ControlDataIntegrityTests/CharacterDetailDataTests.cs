using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Glyssen.Character;
using Glyssen.Properties;
using NUnit.Framework;

namespace ControlDataIntegrityTests
{
	[TestFixture]
	public class CharacterDetailDataTests
	{
		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			// Fixes issue where other test project was interfering with the running of this one (by setting the data to test data).
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = null;
		}

		[Test]
		public void DataIntegrity_RequiredFieldsHaveValidFormatAndThereAreNoDuplicateLines()
		{
			Regex regex = new Regex("^[^\t]+\t((TRUE)|(FALSE))\t((Male)|(Female)|(Both)|(Unknown)|(Pref: Male)|(Pref: Female))?\t[^\t]*\t[^\t]*", RegexOptions.Compiled);
			string[] allLines = Resources.CharacterDetail.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

			var set = new HashSet<string>();
			foreach (var line in allLines.Skip(1))
			{
				if (line.StartsWith("#"))
					continue;

				var match = regex.Match(line);
				Assert.IsTrue(match.Success, "Failed to match line: " + line);

				var matchResult = match.Result("$&");
				Assert.IsTrue(set.Add(matchResult), "Duplicate line: " + matchResult);
			}
		}

		[Test]
		public void DataIntegrity_AllNonNarratorCharacterDetailsHaveCharacterIdOrDefaultCharacter()
		{
			var charactersIds = ControlCharacterVerseData.Singleton.GetAllQuoteInfo().Select(d => d.Character).ToList();
			var defaultCharacters = ControlCharacterVerseData.Singleton.GetAllQuoteInfo().Select(d => d.DefaultCharacter).ToList();
			ISet<string> missingCharacters = new HashSet<string>();
			foreach (string character in CharacterDetailData.Singleton.GetAll().Select(d => d.Character))
			{
				if (!CharacterVerseData.IsCharacterOfType(character, CharacterVerseData.StandardCharacter.Narrator) &&
					(!(charactersIds.Contains(character) || defaultCharacters.Contains(character))))
					missingCharacters.Add(character);
			}
			Assert.False(missingCharacters.Any(),
				"Characters in Character-Detail data but not in Character-Verse data:" +
				Environment.NewLine +
				missingCharacters.OnePerLineWithIndent());
		}
	}

	internal static class TestOutputExtensions
	{
		internal static string OnePerLineWithIndent(this IEnumerable<string> enumerable)
		{
			var sb = new StringBuilder();
			foreach (string item in enumerable)
				sb.Append("\t").Append(item).Append(Environment.NewLine);
			return sb.ToString();
		}
	}
}
