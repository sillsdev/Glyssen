using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Glyssen.Character;

namespace DevTools
{
	class CharacterListProcessing
	{
		public const string kBaseDirForRealOutput = "..\\..\\Glyssen\\Resources";
		private const string kBaseDirForHelperOutput = "..\\..\\Resources\\temporary";

		public static void Process()
		{
//			var allCv = ProcessJimFiles();
//			allCv = FindAliases(allCv);
//			FindDeliveries(ControlCharacterVerseData.Singleton.GetAllQuoteInfo());
			GetAllNtCharacters();
		}

		private static void GetAllNtCharacters()
		{
			var ntCharacters = new SortedSet<string>();
			foreach (var cv in ControlCharacterVerseData.Singleton.GetAllQuoteInfo())
				if (cv.Book > 39 && !cv.Character.Contains('/'))
					ntCharacters.Add(cv.Character);
			Debug.WriteLine(TabDelimited(ntCharacters));
		}

		static List<CharacterVerse> FindAliases(List<CharacterVerse> characterVerses)
		{
			characterVerses.Sort(CharacterVerse.CharacterComparison);
			File.WriteAllText(Path.Combine(kBaseDirForHelperOutput, "CharacterVerse_SortCharacter.txt"), CharacterVerse.AllTabDelimited(characterVerses));

			var characters = new Dictionary<string, HashSet<string>>();
			foreach (var cv in characterVerses)
			{
				HashSet<string> idSet;
				if (characters.TryGetValue(cv.Character, out idSet))
					idSet.Add(cv.CharacterId);
				else
					characters.Add(cv.Character, new HashSet<string> { cv.CharacterId });
			}
			var multiCharacters = new Dictionary<string, HashSet<string>>();
			foreach (var ch in characters)
			{
				if (ch.Value.Count > 1)
					multiCharacters.Add(ch.Key, ch.Value);
			}
			File.WriteAllText(Path.Combine(kBaseDirForHelperOutput, "MultipleCharacter.txt"), TabDelimited(multiCharacters));
			if (multiCharacters.Count > 0)
				MessageBox.Show("Two or more numerical character IDs have the same character name.\n" +
							"See MultipleCharacter.txt for the character IDs in question.");

			characterVerses.Sort(CharacterVerse.CharacterIdComparison);
			File.WriteAllText(Path.Combine(kBaseDirForHelperOutput, "CharacterVerse_SortCharacterId.txt"), CharacterVerse.AllTabDelimited(characterVerses));

			var characterIds = new Dictionary<string, HashSet<string>>();
			var uniqueCharacterIds = new HashSet<string>();
			foreach (var cv in characterVerses)
			{
				uniqueCharacterIds.Add(cv.CharacterId);
				HashSet<string> idSet;
				if (characterIds.TryGetValue(cv.CharacterId, out idSet))
					idSet.Add(cv.Character);
				else
					characterIds.Add(cv.CharacterId, new HashSet<string> { cv.Character });
			}
			var multiCharacterIds = new Dictionary<string, HashSet<string>>();
			foreach (var ch in characterIds)
			{
				if (ch.Value.Count > 1)
					multiCharacterIds.Add(ch.Key, ch.Value);
			}
			File.WriteAllText(Path.Combine(kBaseDirForHelperOutput, "MultipleCharacterId.txt"), TabDelimited(multiCharacterIds));
			if (multiCharacterIds.Count > 0)
				MessageBox.Show("Two or more characters have the same numerical character ID.\n" +
							"One way to resolve this is by making them aliases in AliasUtil.cs.\n" +
							"See MultipleCharacterId.txt or MultipleCharacterId_Extra.txt for the characters in question.");

			ProcessUniqueIds(uniqueCharacterIds);

			ProcessHelpfulVersionOfMultipleCharacterId(multiCharacterIds, characterVerses);

			return characterVerses;
		}

		private static void ProcessHelpfulVersionOfMultipleCharacterId(Dictionary<string, HashSet<string>> multiCharacterIds, List<CharacterVerse> characterVerses)
		{
			var sb = new StringBuilder();
			var lines = new List<string>();
			foreach (string characterId in multiCharacterIds.Keys)
			{
				HashSet<string> characters;
				multiCharacterIds.TryGetValue(characterId, out characters);
				foreach (string character in characters)
				{
					IEnumerable<CharacterVerse> cvs = characterVerses.Where(c => c.Character == character);
					if (!cvs.Any())
						Debug.Fail("Character not found: " + character);
					foreach (CharacterVerse cv in cvs)
					{
						if (cv.CharacterId != characterId)
							Debug.Fail("CharacterIds do not match: " + cv.CharacterId + ", " + characterId);
						sb.Append(characterId).Append("\t").Append(cv.VoiceTalentId).Append("\t").Append(character).Append("\t").Append(cv.Reference);
						lines.Add(sb.ToString());
						sb.Clear();
					}
				}
			}
			File.WriteAllText(Path.Combine(kBaseDirForHelperOutput, "MultipleCharacterId_Extra.txt"), TabDelimited(lines));
		}

		static List<CharacterVerse> ProcessJimFiles()
		{
			var allCv = CharacterVerse.All();
			var allCci = CharacterCharacterId.All();

			var cvNotFound = new List<CharacterVerse>();
			var cciFound = new List<CharacterCharacterId>();

			foreach (CharacterVerse cv in allCv)
			{
				bool found = false;
				foreach (CharacterCharacterId cci in allCci)
				{
					if (cv.CharacterAndDelivery.Equals(cci.Character))
					{
						cv.CharacterId = cci.CharacterId;
						cv.VoiceTalentId = cci.VoiceTalentId;
						cciFound.Add(cci);
						found = true;
						break;
					}
				}
				if (!found)
					cvNotFound.Add(cv);

				SetAlias(cv);

				if (DialogueQuotesReferences.Contains(cv.Book, cv.Chapter, cv.Verse))
					cv.IsDialogue = true;
			}

			Directory.CreateDirectory(kBaseDirForHelperOutput);
			File.WriteAllText(Path.Combine(kBaseDirForHelperOutput, "CharacterCharacterId_notFullyProcessed.txt"), CharacterCharacterId.AllTabDilimited(allCci));
			allCci.RemoveAll(cciFound.Contains);

			GenerateControlFile(allCv);
			GenerateCharacterIdMap(allCv);

			File.WriteAllText(Path.Combine(kBaseDirForHelperOutput, "cvNotFound.txt"), CharacterVerse.AllTabDelimited(cvNotFound));
			File.WriteAllText(Path.Combine(kBaseDirForHelperOutput, "cciNotFound.txt"), CharacterCharacterId.AllTabDilimited(allCci));

			return allCv;
		}

		private static void GenerateControlFile(List<CharacterVerse> allCv)
		{
			int versionNumber = ControlCharacterVerseData.Singleton.ControlFileVersion + 1;

			allCv.Sort(CharacterVerse.ReferenceComparison);
			var sb = new StringBuilder();
			sb.Append("Control File Version\t").Append(versionNumber).Append("\tGenerated\t").Append(DateTime.Now.ToString("R")).Append(Environment.NewLine);
			sb.Append(CharacterVerse.AllTabDelimited(allCv));
			File.WriteAllText(Path.Combine(kBaseDirForRealOutput, "CharacterVerse.txt"), sb.ToString());
		}

		private static void GenerateCharacterIdMap(IEnumerable<CharacterVerse> allCv)
		{
			var set = new SortedSet<string>();
			foreach (var cv in allCv)
				set.Add(cv.Character + "\t" + cv.CharacterId);
			File.WriteAllText(Path.Combine(kBaseDirForRealOutput, "CharacterIdMap.txt"), TabDelimited(set));
		}

		private static void ProcessUniqueIds(HashSet<string> uniqueCharacterIds)
		{
			File.WriteAllText(Path.Combine(kBaseDirForHelperOutput, "UniqueCharacterIds.txt"), TabDelimited(uniqueCharacterIds));

			var missingIds = new List<int>();

			int i = 0;
			foreach (string charId in uniqueCharacterIds)
			{
				int id;
				if (Int32.TryParse(charId, out id))
					while (id > i++)
						missingIds.Add(i - 1);
			}

			File.WriteAllText(Path.Combine(kBaseDirForHelperOutput, "MissingCharacterIds.txt"), TabDelimited(missingIds));
		}

		private static string TabDelimited(Dictionary<string, HashSet<string>> dictionary)
		{
			var sb = new StringBuilder();
			foreach (var entry in dictionary)
			{
				sb.Append(entry.Key).Append("\t");
				foreach (var value in entry.Value)
					sb.Append(value).Append("\t");
				sb.Append(Environment.NewLine);
			}
			return sb.ToString();
		}

		public static string TabDelimited(ICollection<string> set)
		{
			var sb = new StringBuilder();
			foreach (var entry in set)
			{
				sb.Append(entry).Append(Environment.NewLine);
			}
			return sb.ToString();
		}

		public static string TabDelimited(ICollection<int> list)
		{
			var sb = new StringBuilder();
			foreach (var entry in list)
			{
				sb.Append(entry).Append(Environment.NewLine);
			}
			return sb.ToString();
		}

		private static void SetAlias(CharacterVerse cv)
		{

			int charId = Int32.Parse(cv.CharacterId);
			string character;
			if (AliasUtil.Aliases.TryGetValue(charId, out character) && cv.Character != character)
			{
				cv.Alias = cv.Character;
				cv.Character = character;
			}
		}

		private static void FindDeliveries(List<CharacterVerse> allCv)
		{
			var deliveries = new SortedSet<string>();
			foreach (CharacterVerse cv in allCv)
				deliveries.Add(cv.Delivery);
			File.WriteAllText(Path.Combine(kBaseDirForHelperOutput, "UniqueDeliveries.txt"), TabDelimited(deliveries));
		}

		private static void FindDeliveries(IEnumerable<Glyssen.Character.CharacterVerse> allCv)
		{
			var deliveries = new SortedSet<string>();
			foreach (Glyssen.Character.CharacterVerse cv in allCv)
				deliveries.Add(cv.Delivery);
			File.WriteAllText(Path.Combine(kBaseDirForHelperOutput, "UniqueDeliveries.txt"), TabDelimited(deliveries));
		}
	}
}
