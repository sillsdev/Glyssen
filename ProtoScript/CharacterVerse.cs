using System;
using System.Collections.Generic;
using System.Linq;
using ProtoScript.Properties;
using SIL.ScriptureUtils;

namespace ProtoScript
{
	public class CharacterVerse
	{
		public const string kNotAQuote = "Not A Quote";

		private static IEnumerable<CharacterVerse> s_data;

		static CharacterVerse()
		{
			LoadAll();
		}

		public static int ControlFileVersion { get; private set; }

		public static string GetCharacter(string bookId, int chapter, int verse)
		{
			IList<CharacterVerse> matches = s_data.Where(cv => cv.BookCode == bookId && cv.Chapter == chapter && cv.Verse == verse).ToList();
			if (matches.Count == 1)
				return matches.First().Character;
			if (matches.Count > 1)
			{
				string character = null;
				foreach (CharacterVerse cv in matches)
				{
					if (character == null)
					{
						character = cv.Character;
						continue;
					}
					if (character != cv.Character)
						return Block.AmbiguousCharacter;
				}
				return character;
			}
			return Block.UnknownCharacter;
		}

		public static IEnumerable<CharacterVerse> GetCharacters(string bookId, int chapter, int verse)
		{
			return s_data.Where(cv => cv.BookCode == bookId && cv.Chapter == chapter && cv.Verse == verse);
		}

		public static IEnumerable<CharacterVerse> GetAllQuoteInfo(string bookId)
		{
			return s_data.Where(cv => cv.BookCode == bookId);
		}

		private static void LoadAll()
		{
			if (s_data != null)
				return;

			bool firstLine = true;
			var list = new HashSet<CharacterVerse>();
			foreach (var line in Resources.CharacterVerseData.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
			{
				string[] items = line.Split(new[] { "\t" }, StringSplitOptions.None);
				if (firstLine)
				{
					int cfv;
					if (Int32.TryParse(items[1], out cfv) && items[0].StartsWith("Control File"))
						ControlFileVersion = cfv;
					else
						throw new ApplicationException("Bad format in CharacterVerseData metadata: " + line);
					firstLine = false;
					continue;
				}
				if (items.Length != 6)
					throw new ApplicationException("Bad format in CharacterVerseData! Line #: " + list.Count + "; Line contents: " + line);

				int chapter = Int32.Parse(items[1]);
				for (int verse = ScrReference.VerseToIntStart(items[2]); verse <= ScrReference.VerseToIntEnd(items[2]); verse++)
					list.Add(new CharacterVerse
					{
						BcvRef = new BCVRef(BCVRef.BookToNumber(items[0]), chapter, verse),
						Character = items[3], 
						Delivery = items[4],
						Alias = items[5]
					});
			}
			if (!list.Any())
				throw new ApplicationException("No character verse data available!");
			s_data = list;
		}

		public string Character;
		public BCVRef BcvRef;
		public string BookCode { get { return BCVRef.NumberToBookCode(BcvRef.Book); } }
		public int Chapter { get { return BcvRef.Chapter; } }
		public int Verse { get { return BcvRef.Verse; } }
		public string Delivery;
		public string Alias;

		public override string ToString()
		{
			if (string.IsNullOrEmpty(Delivery))
				return Character;
			return string.Format("{0} [{1}]", Character, Delivery);
		}

		#region Equality Members
		protected bool Equals(CharacterVerse other)
		{
			return Equals(BcvRef, other.BcvRef) && string.Equals(Character, other.Character) && string.Equals(Delivery, other.Delivery) && string.Equals(Alias, other.Alias);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			if (obj.GetType() != this.GetType())
				return false;
			return Equals((CharacterVerse)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = (BcvRef != null ? BcvRef.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Character != null ? Character.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Delivery != null ? Delivery.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Alias != null ? Alias.GetHashCode() : 0);
				return hashCode;
			}
		}

		public static bool operator ==(CharacterVerse left, CharacterVerse right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(CharacterVerse left, CharacterVerse right)
		{
			return !Equals(left, right);
		}
		#endregion
	}
}
