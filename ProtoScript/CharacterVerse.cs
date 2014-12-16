using System;
using System.Collections.Generic;
using SIL.ScriptureUtils;

namespace ProtoScript
{
	public class CharacterVerse
	{
		public string Character;
		public BCVRef BcvRef;
		public string BookCode { get { return BCVRef.NumberToBookCode(BcvRef.Book); } }
		public int Chapter { get { return BcvRef.Chapter; } }
		public int Verse { get { return BcvRef.Verse; } }
		public string Delivery;
		public string Alias;
		public bool IsDialogue { get; set; }

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

		#region Equality Members
		protected bool Equals(CharacterVerse other)
		{
			return Equals(BcvRef, other.BcvRef) && string.Equals(Character, other.Character) && string.Equals(Delivery, other.Delivery) && string.Equals(Alias, other.Alias) && IsDialogue == other.IsDialogue;
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

	public class CharacterComparer : IComparer<CharacterVerse>
	{
		int IComparer<CharacterVerse>.Compare(CharacterVerse x, CharacterVerse y)
		{
			return String.Compare(x.Character, y.Character, StringComparison.InvariantCultureIgnoreCase);
		}
	}
}
