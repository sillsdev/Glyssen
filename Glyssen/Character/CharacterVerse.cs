using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Glyssen.Shared;
using L10NSharp;
using SIL.Scripture;
using static System.String;

namespace Glyssen.Character
{
	// REVIEW: It would be really nice to be able to make this Flags, but some of the useful "flag" values
	// (e.g., Expected) would not be valid values to set the property of a CharacterSpeakingMode to, so
	// then we'd need some more sanity checking (at least in data integrity tests, but for maximum safety,
	// in the production code).
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
		/// Like <seealso cref="Implicit"/>, but when a verse also has the possibility of a self-quote.
		/// Knowing this makes it possible for us to ignore quoted text within the larger discourse and
		/// not incorrectly assume that explicit quotes are being used (along with a "he said" reporting
		/// clause) in the verse. (As noted in the quote parser, there is a slight chance a stray
		/// "he said" could mess us up here, but that's unlikely.)
		/// </summary>
		ImplicitWithPotentialSelfQuote,

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
		/// c) likely not to be marked as speech at all, but in these cases <seealso cref="Rare"/> is
		/// probably a better choice.
		/// d) A self-quote by the narrator (especially where the narrator refers to himself in the
		/// first person). * ENHANCE: We might want to consider breaking this case out into a
		/// distinct type.
		/// For now, Potential quotes will be treated just like <seealso cref="Indirect"/> quotes --
		/// they will not be considered as "expected" quotes.
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
		/// quoted or commanded to speak actually speak the words (especially if it is a command to say
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
		/// automatically assign the quoted text to the preferred character and never to the alternate, but the
		/// alternate will be listed as an option in Identify Speaking Parts, in case the scripter wants to
		/// choose that character instead. This makes it possible to avoid ambiguity for the vast number of
		/// passages where there is a single well-defined quote and the ambiguity is merely a dramatization
		/// decision. (Basically, the effect is the same as using <seealso cref="Hypothetical"/>, except without
		/// the performance hit to look in the reference text to see whether the alternate character is used.
		/// This type can also be used when there is a potential quote that spills over into a subsequent verse
		/// that has an actual expected quote. Some translations (e.g., ISV) wrap virtually the entire text in
		/// quotes to indicate that the prophet is speaking all the text. In these cases, the actual quote may
		/// be second level. For translations that do not do this, we don't want the prophet to accidentally be
		/// considered as a candidate for the quoted text (which would result in an ambiguity). By using the
		/// Alternate quote type, we ensure that it will only be considered for an on-going quote that was
		/// opened in a previous verse.
		/// </summary>
		Alternate,

		/// <summary>
		/// Used to indicate a potential speaker which so very rarely occurs in any project that it cannot
		/// be safely assigned automatically. Typically, this kind of quote represents an attitude, thought,
		/// intention, desire, or belief. In the target language, this may be expressed as actual speech, in
		/// which case it should definitely be dramatized. But it might just be an unexpressed thought,
		/// spelled out literally for the sake of the audience, in which case, the target community will
		/// need to decide whether to dramatize it or have it read by the narrator. If a verse has only Rare quotes and the parser finds a
		/// quote in that verse, it will be regarded as unexpected, but in Identify Speaking Parts, the rare
		/// speaker(s) will appear (along with the narrator) in the list of choices. So at least for now,
		/// the actual handling will be identical to <seealso cref="Alternate"/>, though it is a semantically
		/// distinct case. See PG-1233 for a full discussion, along with an alternate proposal for how to
		/// handle Rare when it occurs along with other quote types in a verse.
		/// </summary>
		Rare,
	}

	/// <summary>
	/// Class that vaguely represents a "local" instance of speech, but is not tied to a specific verse. In most cases
	/// an object of type <see cref="CharacterSpeakingMode"/> will actually be a <seealso cref="CharacterVerse"/>, which
	/// is representative of the entire speech, even though it may cover multiple verses. Where the Delivery, Alias and
	/// QuoteType are the same for the entire speech (which is the most typical case), they will accurately reflect that
	/// homogeneity. In cases where a speech overruns the natural limits (e.g. crosses over into a different delivery or
	/// type of quote), then an object of this class might be a synthesis reflecting that, which may or may not correspond
	/// to any existing <seealso cref="CharacterVerse"/>. For example, if there is no paragraph break, a block may have
	/// Jesus first scolding and then instructing. In such a case, the delivery would need to be unspecified.
	/// </summary>
	public class CharacterSpeakingMode : ICharacterDeliveryInfo
	{
		public const string kScriptureCharacter = "scripture";
		internal const string kMultiCharacterIdSeparator = "/";
		private string m_localizedCharacter;
		private string m_localizedAlias;

		private bool m_localized;

		public string Character { get; }
		public string Delivery { get; }
		public string Alias { get; }
		public QuoteType QuoteType { get; protected set; }
		public bool IsDialogue => QuoteType == QuoteType.Dialogue;
		public bool IsExpected => QuoteType == QuoteType.Dialogue || QuoteType == QuoteType.Normal || QuoteType == QuoteType.Implicit || QuoteType == QuoteType.ImplicitWithPotentialSelfQuote || IsScriptureQuotation;
		public bool IsScriptureQuotation => QuoteType == QuoteType.Quotation && Character == kScriptureCharacter;
		public bool IsUnusual => QuoteType == QuoteType.Alternate || QuoteType == QuoteType.Rare;
		public bool IsImplicit => QuoteType == QuoteType.Implicit || QuoteType == QuoteType.ImplicitWithPotentialSelfQuote;

		/// <summary>
		/// A single character ID which could/should be used in the script.
		/// </summary>
		/// <remarks>
		/// In many cases this is set in the control file, but is not actually used. (It is informational,
		/// historical, or possibly for some future feature.) <seealso cref="ResolvedDefaultCharacter"/>
		/// Examples of other uses:
		/// <list type="number">
		/// <item><description>character is "Good Priest" (FCBH concept)</description></item>
		/// <item><description>to translate from generic groups (i.e., "parents" or "disciples", "people in high
		/// priest's courtyard") to specific individuals ("father", "mother", "Thomas", etc.)</description></item>
		/// <item><description>to indicate a guess as to who the real "voice from heaven" is</description></item>
		/// <item><description>to translate possible OT theophanies to Jesus</description></item>
		/// <item><description>entries in JHN 21:7 and JHN 21:17 that sit alongside the "correct" entry that
		/// basically tell the user to have the narrator read it</description></item>
		/// <item><description>In GEN 28:6, there's an entry to attempt to say that a quotation by Isaac
		/// should be read by the narrator (even though FCBH expects Isaac to say it)</description></item>
		/// <item><description>Some places where we have a narrator Quotation (e.g., PRO 30:8), we note that it
		/// should be read by Agur, but now this is actually handled by narrator overrides</description></item>
		/// <item><description>dream => angel</description></item>
		/// </list>
		/// Probably most of theses should be handled differently...
		/// </remarks>
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
		
		/// <summary>
		/// This property returns the Default character to use if this object's Character property represents
		/// multiple character IDs. Otherwise, it just returns the Character (the <see cref="DefaultCharacter"/>
		/// is ignored in this case).
		/// </summary>
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

		public CharacterSpeakingMode(string character, string delivery, string alias, bool projectSpecific,
			QuoteType quoteType = QuoteType.Normal, string defaultCharacter = null, string parallelPassageReferences = null)
		{
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

		internal static string GetLocalizedIndividualCharacterString(string character)
		{
			return LocalizationManager.GetDynamicString(GlyssenInfo.kApplicationId, "CharacterName." + character, character);
		}

		#region Equality Members
		// Note QuoteType *cannot* be included in equality determination (nor do we want it to be) because it is not readonly.
		protected bool Equals(CharacterSpeakingMode other)
		{
			return Equals(Character, other.Character) &&
				Equals(Delivery, other.Delivery) &&
				Equals(Alias, other.Alias);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			if (obj.GetType() != GetType())
				return false;
			return Equals((CharacterSpeakingMode)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = (Character != null ? Character.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Delivery != null ? Delivery.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Alias != null ? Alias.GetHashCode() : 0);
				return hashCode;
			}
		}

		public static bool operator ==(CharacterSpeakingMode left, CharacterSpeakingMode right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(CharacterSpeakingMode left, CharacterSpeakingMode right)
		{
			return !Equals(left, right);
		}
		#endregion
	}

	/// <summary>
	/// Represents all the known details of an actual or potential speech by a character in a particular
	/// verse.
	/// </summary>
	public class CharacterVerse : CharacterSpeakingMode
	{
		public BCVRef BcvRef { get; }
		public string BookCode => BCVRef.NumberToBookCode(BcvRef.Book);
		public int Book => BcvRef.Book;
		public int Chapter => BcvRef.Chapter;
		public int Verse => BcvRef.Verse;
		// There are a few places where we need to allow two entries in the CV control file that differ only by Quote Type,
		// so we need to include the quote type when evaluating "equality" (otherwise, they can't both be included in the set).
		// However, since we occasionally "adjust" the quotation type (e.g., change some Quotation entries to Alternate), it
		// is not readonly and therefore can't be used in determining equality. So here we store the original value.
		private readonly QuoteType m_origQuoteType;

		public CharacterVerse(BCVRef bcvRef, string character, string delivery, string alias, bool projectSpecific,
			QuoteType quoteType = QuoteType.Normal, string defaultCharacter = null, string parallelPassageReferences = null) :
			base(character, delivery, alias, projectSpecific, quoteType, defaultCharacter, parallelPassageReferences)
		{
			BcvRef = new BCVRef(bcvRef);
			m_origQuoteType = quoteType;
		}

		public void ChangeToAlternate()
		{
			Debug.Assert(QuoteType == QuoteType.Quotation &&
				DefaultCharacter == CharacterVerseData.GetStandardCharacterId(BookCode, CharacterVerseData.StandardCharacter.Narrator),
				"At least for now, we only allow this for Quotations that default to the narrator.");
			QuoteType = QuoteType.Alternate;
		}

		#region Equality Members
		protected bool Equals(CharacterVerse other)
		{
			return Equals(BcvRef, other.BcvRef) &&
				base.Equals(other) &&
				m_origQuoteType == other.m_origQuoteType;
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
				hashCode = (hashCode * 397) ^ base.GetHashCode();
				hashCode = (hashCode * 397) ^ m_origQuoteType.GetHashCode();
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

	public class NarratorOverrideCharacter : ICharacterDeliveryInfo
	{
		public string Character { get; }
		public string LocalizedCharacter { get; }
		public string Delivery => null;
		public string DefaultCharacter => null;
		public string Alias => null;
		public string LocalizedAlias => null;
		public bool ProjectSpecific => false;

		public NarratorOverrideCharacter(string character)
		{
			Character = character;
			LocalizedCharacter = CharacterVerse.GetLocalizedIndividualCharacterString(character);
		}
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

	public class CharacterDeliveryAliasEqualityComparer : CharacterDeliveryEqualityComparer
	{
		public override bool Equals(ICharacterDeliveryInfo x, ICharacterDeliveryInfo y)
		{
			return base.Equals(x, y) && x?.Alias == y?.Alias;
		}

		public override int GetHashCode(ICharacterDeliveryInfo obj)
		{
			unchecked
			{
				int hashCode = base.GetHashCode(obj);
				hashCode = (hashCode * 397) ^ (obj.Alias != null ? obj.Alias.GetHashCode() : 0);
				return hashCode;
			}
		}
	}

	public class CharacterDeliveryEqualityComparer : IEqualityComparer<ICharacterDeliveryInfo>
	{
		public virtual bool Equals(ICharacterDeliveryInfo x, ICharacterDeliveryInfo y)
		{
			if (x == null && y == null)
				return true;
			if (x == null || y == null)
				return false;

			return x.Character.Equals(y.Character) && x.Delivery == y.Delivery;
		}

		public virtual int GetHashCode(ICharacterDeliveryInfo obj)
		{
			unchecked
			{
				int hashCode = (obj.Character != null ? obj.Character.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (obj.Delivery != null ? obj.Delivery.GetHashCode() : 0);
				return hashCode;
			}
		}
	}

	public class CharacterEqualityComparer : IEqualityComparer<CharacterSpeakingMode>
	{
		public bool Equals(CharacterSpeakingMode x, CharacterSpeakingMode y)
		{
			return String.Equals(x.Character, y.Character);
		}

		public int GetHashCode(CharacterSpeakingMode obj)
		{
			return obj.Character.GetHashCode();
		}
	}

	public class CharacterDeliveryComparer : IComparer<ICharacterDeliveryInfo>
	{
		int IComparer<ICharacterDeliveryInfo>.Compare(ICharacterDeliveryInfo x, ICharacterDeliveryInfo y)
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
