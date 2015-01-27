using System;
using System.Collections.Generic;
using SIL.ScriptureUtils;

namespace ProtoScript.Character
{
	public class CharacterVerse
	{
		private readonly string m_character;
		private readonly BCVRef m_bcvRef;

		private readonly string m_delivery;
		private readonly string m_alias;
		private readonly bool m_isDialogue;
		private readonly string m_defaultCharacter;
		private readonly string m_parallelPassageReferences;
		private readonly bool m_userCreated;

		public BCVRef BcvRef { get { return m_bcvRef; } }
		public string BookCode { get { return BCVRef.NumberToBookCode(m_bcvRef.Book); } }
		public int Chapter { get { return m_bcvRef.Chapter; } }
		public int Verse { get { return m_bcvRef.Verse; } }
		public string Character { get { return m_character; } }
		public string Delivery { get { return m_delivery; } }
		public string Alias { get { return m_alias; } }
		public bool IsDialogue { get { return m_isDialogue; } }
		public string DefaultCharacter { get { return m_defaultCharacter; } }
		public string ParallelPassageReferences { get { return m_parallelPassageReferences; } }
		public bool UserCreated { get { return m_userCreated; } }

		public CharacterVerse(BCVRef bcvRef, string character, string delivery, string alias, bool userCreated,
			bool isDialogue = false, string defaultCharacter = null, string parallelPassageReferences = null)
		{
			m_bcvRef = bcvRef;
			m_character = character;
			m_delivery = delivery;
			m_alias = alias;
			m_isDialogue = isDialogue;
			m_defaultCharacter = defaultCharacter;
			m_parallelPassageReferences = parallelPassageReferences;
			m_userCreated = userCreated;
		}

		public override string ToString()
		{
			return Character;
		}

		public string ToStringWithDelivery()
		{
			if (string.IsNullOrEmpty(Delivery))
				return Character;
			return string.Format("{0} [{1}]", Character, Delivery);
		}

		public string ToTabDelimited()
		{
			return BookCode + "\t" + Chapter + "\t" + Verse + "\t" + Character + "\t" + Delivery + "\t" + Alias + "\t" +
				IsDialogue + "\t" + DefaultCharacter + "\t" + ParallelPassageReferences + "\t" + UserCreated;
		}

		#region Equality Members
		protected bool Equals(CharacterVerse other)
		{
			return Equals(m_bcvRef, other.m_bcvRef) && string.Equals(Character, other.Character) && string.Equals(Delivery, other.Delivery) && string.Equals(Alias, other.Alias) && IsDialogue == other.IsDialogue;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			if (obj.GetType() != GetType())
				return false;
			return Equals((CharacterVerse)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = (m_bcvRef != null ? m_bcvRef.GetHashCode() : 0);
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

	public class BcvCharacterDeliveryComparer : IEqualityComparer<CharacterVerse>
	{
		public bool Equals(CharacterVerse x, CharacterVerse y)
		{
			return x.BcvRef.Equals(y.BcvRef) && x.Character.Equals(y.Character) && x.Delivery.Equals(y.Delivery);
		}

		public int GetHashCode(CharacterVerse obj)
		{
			unchecked
			{
				int hashCode = (obj.BcvRef != null ? obj.BcvRef.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (obj.Character != null ? obj.Character.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (obj.Delivery != null ? obj.Delivery.GetHashCode() : 0);
				return hashCode;
			}
		}
	}

	public class CharacterDeliveryComparer : IComparer<CharacterVerse>
	{
		int IComparer<CharacterVerse>.Compare(CharacterVerse x, CharacterVerse y)
		{
			int result = String.Compare(x.Character, y.Character, StringComparison.InvariantCultureIgnoreCase);
			if (result != 0)
				return result;
			result = String.Compare(x.Delivery, y.Delivery, StringComparison.InvariantCultureIgnoreCase);
			if (result != 0)
				return result;
			return 0;
		}
	}
}
