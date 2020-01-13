using System;
using System.Collections.Generic;
using Glyssen.Shared;
using SIL;

namespace GlyssenEngine.Character
{
	#region Character class
	public class Character
	{
		private static Character s_narrator;
		private static string s_bookChapterCharacter;
		private static string s_introCharacter;
		private static string s_extraCharacter;

		private readonly string m_characterId;
		private readonly string m_localizedCharacterId;
		private readonly string m_localizedAlias;
		private readonly string m_alias;
		private readonly bool m_projectSpecific;
		private static Func<string> s_funcToGetBookId;
		private static Func<string, string> s_funcToGetRelevantAlias;

		public static Character Narrator { get { return s_narrator; } }

		public string CharacterId { get { return m_characterId; } }
		public string LocalizedCharacterId { get { return m_localizedCharacterId; } }
		public string Alias { get { return m_alias; } }
		public string LocalizedAlias { get { return m_localizedAlias; } }
		public bool ProjectSpecific { get { return m_projectSpecific; } }
		public bool IsNarrator { get { return Equals(s_narrator); } }
		public bool IsStandard => new List<String>
		{
			s_narrator.CharacterId,
			s_bookChapterCharacter,
			s_introCharacter,
			s_extraCharacter
		}.Contains(CharacterId);

		public string LocalizedDisplay { get { return ToLocalizedString(); } }

		public static void SetUiStrings(string narrator, string bookChapterCharacter, string introCharacter,
			string extraCharacter, Func<string> funcToGetBookId, Func<string, string> funcToGetRelevantAlias)
		{
			s_funcToGetBookId = funcToGetBookId;
			s_funcToGetRelevantAlias = funcToGetRelevantAlias;
			s_narrator = new Character(narrator, null, null, null, false);
			s_bookChapterCharacter = bookChapterCharacter;
			s_introCharacter = introCharacter;
			s_extraCharacter = extraCharacter;
		}

		internal Character(string characterId, string localizedCharacterId = null, string alias = null, string localizedAlias = null, bool projectSpecific = true)
		{
			m_characterId = CharacterVerseData.IsCharacterOfType(characterId, CharacterVerseData.StandardCharacter.Narrator) ?
				s_narrator.CharacterId : characterId;
			m_localizedCharacterId = localizedCharacterId ?? characterId;
			m_alias = String.IsNullOrWhiteSpace(alias) ? null : alias;
			m_localizedAlias = String.IsNullOrWhiteSpace(localizedAlias) ? null : localizedAlias;
			m_projectSpecific = projectSpecific;
		}

		public override string ToString()
		{
			if (IsNarrator)
				return String.Format(CharacterId, s_funcToGetBookId());
			return LocalizedAlias ?? CharacterId;
		}

		public string ToLocalizedString()
		{
			if (IsNarrator)
				return ToString();
			return LocalizedAlias ?? LocalizedCharacterId;
		}

		public static string GetCharacterIdForUi(string characterId)
		{
			switch (CharacterVerseData.GetStandardCharacterType(characterId))
			{
				case CharacterVerseData.StandardCharacter.Narrator: return s_narrator.ToString();
				case CharacterVerseData.StandardCharacter.Intro: return String.Format(s_introCharacter, s_funcToGetBookId());
				case CharacterVerseData.StandardCharacter.ExtraBiblical: return String.Format(s_extraCharacter, s_funcToGetBookId());
				case CharacterVerseData.StandardCharacter.BookOrChapter: return String.Format(s_bookChapterCharacter, s_funcToGetBookId());
				default:
					if (characterId == CharacterVerseData.kAmbiguousCharacter || characterId == CharacterVerseData.kUnexpectedCharacter)
						return "";
					string relevantAlias = s_funcToGetRelevantAlias(characterId);
					characterId = Localizer.GetDynamicString(GlyssenInfo.kApplicationId, "CharacterName." + characterId, characterId);
					if (relevantAlias != null)
						return characterId + " [" + relevantAlias + "]";
					return characterId;
			}
		}

		#region Equality members
		protected bool Equals(Character other)
		{
			return string.Equals(CharacterId, other.CharacterId);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			if (obj.GetType() != GetType())
				return false;
			return Equals((Character)obj);
		}

		public override int GetHashCode()
		{
			return (m_characterId != null ? m_characterId.GetHashCode() : 0);
		}

		public static bool operator ==(Character left, Character right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(Character left, Character right)
		{
			return !Equals(left, right);
		}
		#endregion
	}
	#endregion

	#region CharacterIdComparer class

	public abstract class CharacterComparer
	{
		protected int CompareSpecialCases(Character x, Character y, string xTextToCompare, string yTextToCompare)
		{
			// if the CharacterIds are not the same, check for a special case
			if ((x.CharacterId) != (y.CharacterId))
			{
				// narrator should be first item
				if (x.IsNarrator) return -1;
				if (y.IsNarrator) return 1;

				// Jesus should be second item
				if (x.CharacterId == "Jesus") return -1;
				if (y.CharacterId == "Jesus") return 1;
			}

			// this is not a special case
			return string.Compare(xTextToCompare, yTextToCompare, StringComparison.InvariantCultureIgnoreCase);
		}
	}

	public class CharacterIdComparer : CharacterComparer, IComparer<Character>
	{
		int IComparer<Character>.Compare(Character x, Character y)
		{
			return CompareSpecialCases(x, y, x.CharacterId, y.CharacterId);
		}
	}
	#endregion

	#region AliasComparer class
	public class AliasComparer : CharacterComparer, IComparer<Character>
	{
		int IComparer<Character>.Compare(Character x, Character y)
		{
			var xTextToCompare = string.IsNullOrEmpty(x.Alias) ? x.CharacterId : x.Alias;
			var yTextToCompare = string.IsNullOrEmpty(y.Alias) ? y.CharacterId : y.Alias;

			var result = CompareSpecialCases(x, y, xTextToCompare, yTextToCompare);
			return result != 0 ? result : string.Compare(x.CharacterId, y.CharacterId, StringComparison.InvariantCultureIgnoreCase);
		}
	}
	#endregion

	#region Delivery class
	public class Delivery
	{
		private static Delivery s_normalDelivery;

		private readonly string m_text;
		private readonly bool m_projectSpecific;

		public string Text { get { return m_text; } }
		public string LocalizedDisplay { get { return ToLocalizedString(); } }

		private string ToLocalizedString()
		{
			// TODO: Enable localization of deliveries
			return Text;
		}
		public bool ProjectSpecific { get { return m_projectSpecific; } }
		public static Delivery Normal { get { return s_normalDelivery; } }
		public bool IsNormal { get { return Equals(s_normalDelivery); } }

		public static void SetNormalDelivery(string normalDelivery)
		{
			s_normalDelivery = new Delivery(normalDelivery, false);
		}

		internal Delivery(string text, bool projectSpecific = true)
		{
			m_text = text;
			m_projectSpecific = projectSpecific;
		}

		public override string ToString()
		{
			return Text;
		}

		#region Equality members
		protected bool Equals(Delivery other)
		{
			return String.Equals(Text, other.Text);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			if (obj.GetType() != GetType())
				return false;
			return Equals((Delivery)obj);
		}

		public override int GetHashCode()
		{
			return (Text != null ? Text.GetHashCode() : 0);
		}

		public static bool operator ==(Delivery left, Delivery right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(Delivery left, Delivery right)
		{
			return !Equals(left, right);
		}
		#endregion
	}
	#endregion

	#region DeliveryComparer class
	public class DeliveryComparer : IComparer<Delivery>
	{
		int IComparer<Delivery>.Compare(Delivery x, Delivery y)
		{
			return String.Compare(x.Text, y.Text, StringComparison.InvariantCultureIgnoreCase);
		}
	}
	#endregion
}
