using System;
using System.Collections.Generic;
using System.Linq;
using Glyssen.Shared;
using L10NSharp;
using SIL.Scripture;
using static System.String;

namespace Glyssen.Character
{
	public enum QuoteType
	{
		/// <summary>
		/// Normal speech expected to be marked up using quotation marks in the text (this
		/// can include both dialogue as well as other forms of spoken discourse, but quotes that
		/// are commonly marked up as dialog (in writing systems that distinguish between dialogue
		/// and other forms of spoken discourse) will typically be identified as Dialogue.
		/// </summary>
		Normal,

		/// <summary>
		/// Used for speech in passages that are known to be spoken by a particular character
		/// and can be assigned as such even if no punctuation is present to indicate the spoken
		/// discourse. (In some translations, quotation marks may be omitted in text marked as
		/// poetry or in long speeches, especially where the speeches contain other nested quotations
		/// that might make explicit use of first-level quotation marks more unwieldy.
		/// </summary>
		Implicit,

		/// <summary>
		/// Conversation between two or more characters, generally consisting of relatively short
		/// exchanges. (Some writing systems use punctuation to distinguish between dialogue
		/// and other forms of spoken discourse.)
		/// </summary>
		Dialogue,

		/// <summary>
		/// Speech that is commonly rendered in an indirect way rather than as a direct quote.
		/// Since quotation marks were not in use when the Bible was written, this distinction is
		/// not based on the original languages. The decision about whether to render a particular
		/// piece of spoken discourse as direct or indirect speech will tend to vary from language
		/// to language, and some languages simply do not allow for indirect speech at all. Quotes
		/// marked as Indirect will not be considered as "expected" quotes.
		/// </summary>
		Indirect,

		/// <summary>
		/// Potential direct speech that is
		/// a) in verses that are not found in some manuscripts and may be omitted from translations;
		/// b) likely to be marked up using poetry but without quotes;
		/// c) likely not to be marked as speech at all.
		/// d) A self-quote by the narrator (especially where the narrator refers to himself in the
		/// first person). * ENHANCE: We might want to consider breaking this case out into a
		/// distinct type.
		/// For now, Potential quotes will be treated just like Indirect quotes -- they will not be
		/// considered as "expected" quotes.
		/// </summary>
		Potential,

		/// <summary>
		/// Speech not attributed to a real, historical figure. This includes things that someone
		/// might say, predicted future speech*, hypothetical words expressing an attitude held
		/// by a group, words attributed to personified objects, etc. *Note: future speech attributed
		/// to a character in the context of a narrative-style vision (that can be presented
		/// dramatically) need not be regarded as hypothetical.
		/// </summary>
		Hypothetical,

		/// <summary>
		/// Quotations of actual past speech or written words, proverbs, etc. OR something a person or
		/// group is commanded to speak. Typically, these can be read by the narrator, though in some
		/// cases it may be useful to use another voice and/or special sound effects. When spoken by
		/// the narrator, a "Quotation" can also be a place where quotation marks are likely to be used
		/// for something other than speech (e.g., a translation, a foreign phrase, a title, or a literal
		/// name). For dramatic effect, it might sometimes be appropriate to have the person being
		/// quoted or commanded to speak actually speak the words (especially it is a command to say
		/// something immediately (e.g., when God tells Moses or a prophet to say something). This quote
		/// type is used any place where the reference text dramatizes this kind of second-level speech.
		/// See also, <seealso cref="Alternate"/>
		/// </summary>
		Quotation,

		/// <summary>
		/// Technically not a "quote type" per se - rather, this is a special case of where a quote can be
		/// interrupted (i.e., by the narrator) using a parenthetical remark. For example, in MAT 24:15 or
		/// MRK 13:14, where it says: (let the reader understand). Technically, it is probably better for
		/// the quote to be explicitly ended and re-opened, but it is not uncommon for translators to leave
		/// these kinds of interruptions inside the surrounding direct speech. Because these are not easy
		/// to identify unambiguously and there are different ideas about how best to dramatize them, they
		/// will always be marked as ambiguous so the user has a chance to evaluate them and decide what to do.
		/// </summary>
		Interruption,

		/// <summary>
		/// Used to indicate a possible alternate speaker for dramatization purposes. This is normally used in
		/// prophetic works or other places where speech is attributed to both the original speaker and the
		/// prophet, spokesperson, or announcer. As opposed to <seealso cref="Quotation"/>, which normally
		/// indicates past speech, this quote type is used to distinguish between the character presumed to be
		/// the preferred speaker and an alternate speaker who could legitimately speak the same lines but is
		/// not expected to (based on the decision reflected by the reference text). Hence, Glyssen will
		/// automatically assign the quoted text to the preferred chatacter and never to the alternate, but the
		/// alternate will be listed as an option in Identify Speaking Parts, in case the scripter wants to
		/// choose that character instead. This makes it possible to avoid ambiguity for the vast number of
		/// passages where there is a single well-defined quote and the ambiguity is mearly a dramatization
		/// decision. (Basically, the effect is the same as using <seealso cref="Hypothetical"/>, except without
		/// the performance hit to look in the reference text to see whether the alternate character is used.
		/// </summary>
		Alternate,
	}

	public class CharacterVerse : ICharacterDeliveryInfo
	{
		public const string kScriptureCharacter = "scripture";
		internal const string kMultiCharacterIdSeparator = "/";
		private string m_localizedCharacter;
		private string m_localizedAlias;

		private bool m_localized;

		public BCVRef BcvRef { get; }
		public string BookCode => BCVRef.NumberToBookCode(BcvRef.Book);
		public int Book => BcvRef.Book;
		public int Chapter => BcvRef.Chapter;
		public int Verse => BcvRef.Verse;
		public string Character { get; }
		public string Delivery { get; }
		public string Alias { get; }
		public QuoteType QuoteType { get; }
		public bool IsDialogue => QuoteType == QuoteType.Dialogue;
		public bool IsExpected => QuoteType == QuoteType.Dialogue || QuoteType == QuoteType.Normal || QuoteType == QuoteType.Implicit || IsScriptureQuotation;
		public bool IsScriptureQuotation => QuoteType == QuoteType.Quotation && Character == kScriptureCharacter;
		public string DefaultCharacter { get; }
		public string LocalizedCharacter
		{
			get
			{
				if (!m_localized)
					Localize();
				return m_localizedCharacter;
			}
		}
		public string LocalizedAlias
		{
			get
			{
				if (!m_localized)
					Localize();
				return m_localizedAlias;
			}
		}
		// This property returns the Default character to use if this object's Character property represents
		// multiple character IDs. Otherwise, it just returns the Character (the DefaultCharacter is ignored
		// in this case).
		public string ResolvedDefaultCharacter
		{
			get
			{
				var ids = Character.SplitCharacterId(2);
				return ids.Length == 1 ? Character : (!IsNullOrEmpty(DefaultCharacter) ? DefaultCharacter : ids[0]);
			}
		}
		public string ParallelPassageReferences { get; }
		public bool ProjectSpecific { get; }

		public CharacterVerse(BCVRef bcvRef, string character, string delivery, string alias, bool projectSpecific,
			QuoteType quoteType = QuoteType.Normal, string defaultCharacter = null, string parallelPassageReferences = null)
		{
			BcvRef = new BCVRef(bcvRef);
			Character = character;
			Delivery = delivery;
			Alias = alias;
			DefaultCharacter = defaultCharacter;
			ParallelPassageReferences = parallelPassageReferences;
			ProjectSpecific = projectSpecific;
			QuoteType = quoteType;
		}

		public override string ToString()
		{
			return Character;
		}

		public string ToStringWithDelivery()
		{
			if (IsNullOrEmpty(Delivery))
				return Character;
			return $"{Character} [{Delivery}]";
		}

		public void ResetLocalization()
		{
			m_localized = false;
		}

		private void Localize()
		{
			m_localizedCharacter = GetLocalizedCharacterString(Character);
			m_localizedAlias = IsNullOrWhiteSpace(Alias) ? null : GetLocalizedCharacterString(Alias);
			m_localized = true;
		}

		// If an ID or Alias consists of multiple individual characters (or groups), separated by slashes,
		// each individual is localized separately.
		private string GetLocalizedCharacterString(string character)
		{
			return Join(kMultiCharacterIdSeparator, character.SplitCharacterId().Select(GetLocalizedIndividualCharacterString));
		}

		private string GetLocalizedIndividualCharacterString(string character)
		{
			return LocalizationManager.GetDynamicString(GlyssenInfo.kApplicationId, "CharacterName." + character, character);
		}

		#region Equality Members
		protected bool Equals(CharacterVerse other)
		{
			return Equals(BcvRef, other.BcvRef) &&
				String.Equals(Character, other.Character) &&
				String.Equals(Delivery, other.Delivery) &&
				String.Equals(Alias, other.Alias) &&
				QuoteType == other.QuoteType;
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

	public static class CharacterStringExtensions
	{
		private static readonly char[] s_multiCharacterIdSeparators;

		static CharacterStringExtensions()
		{
			s_multiCharacterIdSeparators = CharacterVerse.kMultiCharacterIdSeparator.ToCharArray();
		}

		public static string[] SplitCharacterId(this string characterId, int max = Int32.MaxValue)
		{
			return characterId.Split(s_multiCharacterIdSeparators, max);
		}
	}

	public class BcvCharacterDeliveryEqualityComparer : IEqualityComparer<CharacterVerse>
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

	public class CharacterDeliveryEqualityComparer : IEqualityComparer<ICharacterDeliveryInfo>
	{
		public bool Equals(ICharacterDeliveryInfo x, ICharacterDeliveryInfo y)
		{
			if (x == null && y == null)
				return true;
			if (x == null || y == null)
				return false;

			return x.Character.Equals(y.Character) && x.Delivery.Equals(y.Delivery);
		}

		public int GetHashCode(ICharacterDeliveryInfo obj)
		{
			unchecked
			{
				int hashCode = (obj.Character != null ? obj.Character.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (obj.Delivery != null ? obj.Delivery.GetHashCode() : 0);
				return hashCode;
			}
		}
	}

	public class CharacterEqualityComparer : IEqualityComparer<CharacterVerse>
	{
		public bool Equals(CharacterVerse x, CharacterVerse y)
		{
			return String.Equals(x.Character, y.Character);
		}

		public int GetHashCode(CharacterVerse obj)
		{
			return obj.Character.GetHashCode();
		}
	}

	public class CharacterDeliveryComparer : IComparer<CharacterVerse>
	{
		int IComparer<CharacterVerse>.Compare(CharacterVerse x, CharacterVerse y)
		{
			int result = Compare(x.Character, y.Character, StringComparison.InvariantCultureIgnoreCase);
			if (result != 0)
				return result;
			result = Compare(x.Delivery, y.Delivery, StringComparison.InvariantCultureIgnoreCase);
			if (result != 0)
				return result;
			return 0;
		}
	}
}
