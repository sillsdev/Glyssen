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

			ControlCharacterVerseData.ReadHypotheticalAsNarrator = false;
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			ControlCharacterVerseData.ReadHypotheticalAsNarrator = true;
		}

		[Test]
		public void DataIntegrity_RequiredFieldsHaveValidFormatAndThereAreNoDuplicateLines()
		{
			Regex regex = new Regex("^[^\t/]+\t\\-?\\d+\t(" + typeof(CharacterGender).GetRegexEnumValuesString() + ")?\t(" +
				typeof(CharacterAge).GetRegexEnumValuesString() + ")?\tY?\t[^\t]*\t[^\t]*\t[^\t]*$", RegexOptions.Compiled);
			Regex extraSpacesRegex = new Regex("^ |\t | \t| $", RegexOptions.Compiled);
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

				var extraSpacesMatch = extraSpacesRegex.Match(line);
				Assert.IsFalse(extraSpacesMatch.Success, "Line with extra space(s): " + line);
			}
		}

		[Test]
		public void DataIntegrity_AllNonNarratorCharacterDetailsHaveCharacterIdOrDefaultCharacter()
		{
			var characterIds = new List<string>(ControlCharacterVerseData.Singleton.GetAllQuoteInfo().Select(d => d.Character)
				.SelectMany(characters => characters.Split('/')));

			var defaultCharacters = ControlCharacterVerseData.Singleton.GetAllQuoteInfo().Select(d => d.DefaultCharacter).ToList();
			ISet<string> missingCharacters = new SortedSet<string>();
			foreach (string character in CharacterDetailData.Singleton.GetAll().Select(d => d.CharacterId))
			{
				if (!CharacterVerseData.IsCharacterStandard(character) &&
					(!(characterIds.Contains(character) || defaultCharacters.Contains(character))))
				{
					missingCharacters.Add(character);
				}
			}
			Assert.False(missingCharacters.Any(c => !NarratorOverrides.Singleton.Books.SelectMany(b => b.Overrides.Select(o => o.Character)).Contains(c)),
				"Characters in Character-Detail data but not in Character-Verse data or NarratorOverrides:" +
				Environment.NewLine +
				missingCharacters.OnePerLineWithIndent());
		}

		[Test]
		public void DataIntegrity_NoDuplicateCharacterIds()
		{
			var duplicateCharacterIds = CharacterDetailData.Singleton.GetAll().Select(d => d.CharacterId).FindDuplicates();
			Assert.IsFalse(duplicateCharacterIds.Any(),
				"Duplicate character IDs in Character-Detail data:" +
				Environment.NewLine +
				duplicateCharacterIds.OnePerLineWithIndent());
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

		internal static IEnumerable<T> FindDuplicates<T>(this IEnumerable<T> enumerable)
		{
			var hashset = new HashSet<T>();
			// Without ToList, re-enumerating causes more to be added because it is still based on the original enumerable
			return enumerable.Where(cur => !hashset.Add(cur)).ToList();
		}
	}

	internal static class TestEnumTypeExtensions
	{
		internal static string GetRegexEnumValuesString(this Type type)
		{
			StringBuilder bldr = new StringBuilder();
			foreach (var enumVal in Enum.GetValues(type))
			{
				bldr.Append("(");
				bldr.Append(enumVal);
				bldr.Append(")|");
			}
			bldr.Length--;

			return bldr.ToString();
		}
	}
}
