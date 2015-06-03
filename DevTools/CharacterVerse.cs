using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using SIL.Scripture;

namespace DevTools
{
	class CharacterVerse
	{
		public static List<CharacterVerse> All()
		{
			var all = new List<CharacterVerse>();
			int lineNum = 0;
			foreach (var line in ControlFiles.modified_CharacterNames_BookChapterVerse.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
			{
				lineNum++;
				if (string.IsNullOrEmpty(line))
					continue;
				var cv = new CharacterVerse();
				int endOfRef = IndexOfNth(line, ' ', 2);
				cv.Reference = line.Substring(0, endOfRef);
				ParseAndSetReference(ref cv);
				var theRest = line.Substring(endOfRef + 1);
				if (!theRest.StartsWith("character="))
					Debug.Fail("line number: " + lineNum);
				cv.CharacterAndDelivery = theRest.Substring("character=".Length);
				ParseAndSetCharacterAndDelivery(cv);
				all.Add(cv);
			}
			return all;
		}

		public static string AllTabDelimited(List<CharacterVerse> list)
		{
			var sb = new StringBuilder();
			foreach (CharacterVerse cv in list)
				sb.Append(cv.TabDelimited()).Append(Environment.NewLine);
			return sb.ToString();
		}

		private static int IndexOfNth(string str, char c, int n)
		{
			int s = -1;

			for (int i = 0; i < n; i++)
			{
				s = str.IndexOf(c, s + 1);

				if (s == -1) break;
			}

			return s;
		}

		private static void ParseAndSetReference(ref CharacterVerse cv)
		{
			int i = cv.Reference.IndexOf(" ", StringComparison.InvariantCulture);
			cv.Book = cv.Reference.Substring(0, i);
			var theRest = cv.Reference.Substring(i + 1);
			var split = theRest.Split(new[] { "." }, StringSplitOptions.None);
			cv.Chapter = split[0];
			cv.Verse = split[1];

			if (!cv.Reference.Equals(cv.Book + " " + cv.Chapter + "." + cv.Verse))
				Debug.Fail(cv.Reference + ", " + cv.Book + " " + cv.Chapter + "." + cv.Verse);
		}

		private static void ParseAndSetCharacterAndDelivery(CharacterVerse cv)
		{
			int i = cv.CharacterAndDelivery.LastIndexOf("[", StringComparison.InvariantCulture);
			if (i > -1)
			{
				cv.Character = cv.CharacterAndDelivery.Substring(0, i - 1);
				cv.Delivery = cv.CharacterAndDelivery.Substring(i + 1, cv.CharacterAndDelivery.Length - cv.Character.Length - 3);

				if (!cv.CharacterAndDelivery.Equals(cv.Character + " [" + cv.Delivery + "]"))
					Debug.Fail(cv.CharacterAndDelivery + ", " + cv.Character + " [" + cv.Delivery + "]");
			}
			else
				cv.Character = cv.CharacterAndDelivery;

			i = cv.Character.LastIndexOf(" #", StringComparison.InvariantCulture);
			if (i > -1)
			{
				int num;
				if (!Int32.TryParse(cv.Character.Substring(i+2), out num))
					Debug.Fail("invalid #: " + cv.Character);
				cv.Character = cv.Character.Substring(0, i);
			}
		}

		public static Comparison<CharacterVerse> CharacterComparison = (object1, object2) => String.Compare(object1.CharacterAndDelivery, object2.CharacterAndDelivery, StringComparison.InvariantCulture);
		public static Comparison<CharacterVerse> CharacterIdComparison = (object1, object2) => String.Compare(object1.CharacterId, object2.CharacterId, StringComparison.InvariantCulture);
		public static Comparison<CharacterVerse> ReferenceComparison = (object1, object2) =>
		{
			int result = BCVRef.BookToNumber(object1.Book).CompareTo(BCVRef.BookToNumber(object2.Book));
			if (result != 0)
				return result;
			result = Int32.Parse(object1.Chapter).CompareTo(Int32.Parse(object2.Chapter));
			if (result != 0)
				return result;
			result = ScrReference.VerseToIntStart(object1.Verse).CompareTo(ScrReference.VerseToIntStart(object2.Verse));
			if (result != 0)
				return result;
			result = ScrReference.VerseToIntEnd(object1.Verse).CompareTo(ScrReference.VerseToIntEnd(object2.Verse));
			if (result != 0)
				return result;
			result = String.Compare(object1.Character, object2.Character, StringComparison.InvariantCulture);
			if (result != 0)
				return result;
			result = String.Compare(object1.Delivery, object2.Delivery, StringComparison.InvariantCulture);
			if (result != 0)
				return result;
			result = String.Compare(object1.Alias, object2.Alias, StringComparison.InvariantCulture);
			if (result != 0)
				return result;
			result = String.Compare(object1.Verse, object2.Verse, StringComparison.InvariantCulture);
			if (result != 0)
				return -result;
			return 0;
		};

		public string Reference;
		public string Book;
		public string Chapter;
		public string Verse;
		public string Character;
		public string CharacterAndDelivery;
		public string Delivery;
		public string CharacterId;
		public string Alias;
		public string VoiceTalentId;
		public bool IsDialogue { get; set; }

		public string TabDelimited()
		{
			return Book + "\t" + Chapter + "\t" + Verse + "\t" + Character + "\t" + Delivery + "\t" + Alias + "\t" + IsDialogue;
		}
	}
}
