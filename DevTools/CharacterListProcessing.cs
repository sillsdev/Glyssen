using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DevTools
{
	class CharacterListProcessing
	{
		private const string kBaseDirForOutput = "..\\..\\Resources";

		public static void Process()
		{
			var allCv = ProcessJimFiles();
			FindAliases(allCv);
		}

		static void FindAliases(List<CharacterVerse> characterVerses)
		{
			characterVerses.Sort(CharacterVerse.CharacterComparison);
			File.WriteAllText(Path.Combine(kBaseDirForOutput, "gen_CharacterVerse_SortCharacter.txt"), CharacterVerse.AllTabDelimited(characterVerses));

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
			File.WriteAllText(Path.Combine(kBaseDirForOutput, "gen_MultipleCharacter.txt"), TabDelimited(multiCharacters));

			characterVerses.Sort(CharacterVerse.CharacterIdComparison);
			File.WriteAllText(Path.Combine(kBaseDirForOutput, "gen_CharacterVerse_SortCharacterId.txt"), CharacterVerse.AllTabDelimited(characterVerses));

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
			File.WriteAllText(Path.Combine(kBaseDirForOutput, "gen_MultipleCharacterId.txt"), TabDelimited(multiCharacterIds));

			ProcessUniqueIds(uniqueCharacterIds);
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
						cciFound.Add(cci);
						found = true;
						break;
					}
				}
				if (!found)
					cvNotFound.Add(cv);

				SetAlias(cv);
			}

			File.WriteAllText(Path.Combine(kBaseDirForOutput, "gen_CharacterCharacterId.txt"), CharacterCharacterId.AllTabDilimited(allCci));
			allCci.RemoveAll(cciFound.Contains);

			File.WriteAllText(Path.Combine(kBaseDirForOutput, "gen_CharacterVerse.txt"), CharacterVerse.AllTabDelimited(allCv));
			File.WriteAllText(Path.Combine(kBaseDirForOutput, "gen_cvNotFound.txt"), CharacterVerse.AllTabDelimited(cvNotFound));
			File.WriteAllText(Path.Combine(kBaseDirForOutput, "gen_cciNotFound.txt"), CharacterCharacterId.AllTabDilimited(allCci));

			return allCv;
		}

		private static void ProcessUniqueIds(HashSet<string> uniqueCharacterIds)
		{
			File.WriteAllText(Path.Combine(kBaseDirForOutput, "gen_UniqueCharacterIds.txt"), TabDelimited(uniqueCharacterIds));

			var missingIds = new List<int>();

			int i = 0;
			foreach (string charId in uniqueCharacterIds)
			{
				int id;
				if (Int32.TryParse(charId, out id))
					while (id > i++)
						missingIds.Add(i - 1);
			}

			File.WriteAllText(Path.Combine(kBaseDirForOutput, "gen_MissingCharacterIds.txt"), TabDelimited(missingIds));
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

		public static string TabDelimited(HashSet<string> set)
		{
			var sb = new StringBuilder();
			foreach (var entry in set)
			{
				sb.Append(entry).Append(Environment.NewLine);
			}
			return sb.ToString();
		}

		public static string TabDelimited(List<int> list)
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
	}
}
