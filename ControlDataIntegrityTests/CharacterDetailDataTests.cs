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
			Regex regex = new Regex("^[^\t/]+\t\\-?\\d+\t(" + typeof(CharacterGender).GetRegexEnumValuesString() + ")?\t(" +
				typeof(CharacterAge).GetRegexEnumValuesString() + ")?\tY?\t[^\t]*\t[^\t]*\t?[^\t]*$", RegexOptions.Compiled);
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

		// Technically, there's no harm in having them be identical, but it's not necessary.
		[Test]
		public void DataIntegrity_FCBHCharacterIdNotEqualtoGlyssenCharacterId()
		{
			var unnecessary = CharacterDetailData.Singleton.GetAll().Where(d => d.CharacterId == d.DefaultFCBHCharacter).ToList();
			Assert.IsFalse(unnecessary.Any(),
				"No need to specify FCBH character ID in Character-Detail data if it matches Glyssen character ID:" +
				Environment.NewLine +
				unnecessary.Select(d => d.CharacterId).OnePerLineWithIndent());
		}

		[Test]
		public void DataIntegrity_AdultFemaleCharacterFCBHCharacterIdInOldTestamentHasFemaleSuffix()
		{
			var missingFemaleSuffix = CharacterDetailData.Singleton.GetAll()
				.Where(d => (d.Gender == CharacterGender.Female || d.Gender == CharacterGender.PreferFemale) && 
					d.LastReference?.Book < 40 &&
					(d.Age == CharacterAge.Adult || d.Age == CharacterAge.Elder) &&
					d.DefaultFCBHCharacter != null &&
					!d.DefaultFCBHCharacter.EndsWith(" (female)")).ToList();
			Assert.IsFalse(missingFemaleSuffix.Any(),
				"Missing \" (female)\" suffix on FCBH character ID in Character-Detail data:" +
				Environment.NewLine +
				missingFemaleSuffix.Select(d => $"{d.CharacterId} => {d.DefaultFCBHCharacter}").OnePerLineWithIndent());
		}

		[Test]
		public void DataIntegrity_YoungFemaleCharacterFCBHCharacterIdHasGirlSuffix()
		{
			var missingGirlSuffix = CharacterDetailData.Singleton.GetAll()
				.Where(d => (d.Gender == CharacterGender.Female || d.Gender == CharacterGender.PreferFemale) &&
					d.Age == CharacterAge.YoungAdult &&
					d.DefaultFCBHCharacter != null &&
					(!d.DefaultFCBHCharacter.EndsWith(" (girl)") &&
						!d.DefaultFCBHCharacter.EndsWith(" (female)"))).ToList();
			Assert.IsFalse(missingGirlSuffix.Any(),
				"Missing \" (girl)\" suffix on FCBH character ID in Character-Detail data:" +
				Environment.NewLine +
				missingGirlSuffix.Select(d => $"{d.CharacterId} => {d.DefaultFCBHCharacter}").OnePerLineWithIndent());
		}

		[Test]
		public void DataIntegrity_ChildFemaleCharacterFCBHCharacterIdHasYoungGirlSuffix()
		{
			var missingGirlSuffix = CharacterDetailData.Singleton.GetAll()
				.Where(d => (d.Gender == CharacterGender.Female || d.Gender == CharacterGender.PreferFemale) &&
					d.Age == CharacterAge.Child &&
					d.DefaultFCBHCharacter != null &&
					(!d.DefaultFCBHCharacter.EndsWith(" (young girl)") &&
					!d.DefaultFCBHCharacter.EndsWith(" Girl (female)"))).ToList();
			Assert.IsFalse(missingGirlSuffix.Any(),
				"Missing \" (young girl)\" suffix on FCBH character ID in Character-Detail data:" +
				Environment.NewLine +
				missingGirlSuffix.Select(d => $"{d.CharacterId} => {d.DefaultFCBHCharacter}").OnePerLineWithIndent());
		}

		[Test]
		public void DataIntegrity_ReferenceCommentFieldIsValid()
		{
			var bogusReferenceComment = CharacterDetailData.Singleton.GetAll()
				.Where(d => d.FirstReference?.BBCCCVVV > d.LastReference?.BBCCCVVV).ToList();
			Assert.IsFalse(bogusReferenceComment.Any(),
				"ReferenceComment in Character-Detail data has references out of canonical order:" +
				Environment.NewLine +
				bogusReferenceComment.Select(d => $"{d.CharacterId}: {d.ReferenceComment}").OnePerLineWithIndent());
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
