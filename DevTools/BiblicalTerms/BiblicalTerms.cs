using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using SIL.ScriptureUtils;

namespace DevTools.BiblicalTerms
{

	#region Term class

	/// <summary>
	/// A Biblical term. Contains no localizable data (see TermLocalization)
	/// </summary>
	public class Term
	{
		string id;
		List<string> categoryIds;
		List<Verse> references;
		string localGloss;

		/// <summary>
		/// Unique identifier for this term.
		/// </summary>
		[XmlAttribute("Id")]
		public string Id
		{
			get { return id; }
			set { id = value; }
		}

		/// <summary>
		/// Comma separated list of categories from CategoryIds. Usually a term only has one category
		/// but occasionally it may have multiple categories.
		/// </summary>
		[XmlElement("Category")]
		public List<string> CategoryIds
		{
			get { return categoryIds; }
			set { categoryIds = value ?? new List<string>(); }
		}

		[XmlElement("Domain")]
		public string Domain { get; set; }

		[XmlElement("Gloss")]
		public string Gloss
		{
			get { return localGloss; }
			set { localGloss = value; }
		}

		/// <summary>
		/// All references to this term.
		/// </summary>
		public List<Verse> References
		{
			get { return references; }
			set { references = value; }
		}

		public override string ToString()
		{
			return id;
		}

		public string ToTabDelimited()
		{
			return Gloss + "\t" + Domain;
		}

		public sealed class DomainEqualityComparer : IEqualityComparer<Term>
		{
			public bool Equals(Term x, Term y)
			{
				if (ReferenceEquals(x, y))
					return true;
				if (ReferenceEquals(x, null))
					return false;
				if (ReferenceEquals(y, null))
					return false;
				if (x.GetType() != y.GetType())
					return false;
				return string.Equals(x.Domain, y.Domain);
			}

			public int GetHashCode(Term obj)
			{
				return (obj.Domain != null ? obj.Domain.GetHashCode() : 0);
			}
		}

		private static readonly IEqualityComparer<Term> DomainComparerInstance = new DomainEqualityComparer();

		public static IEqualityComparer<Term> DomainComparer
		{
			get { return DomainComparerInstance; }
		}
	}

	#endregion

	public class TermComparer : IComparer<Term>
	{
		public int Compare(Term x, Term y)
		{
			int result = String.Compare(x.Domain, y.Domain, StringComparison.InvariantCultureIgnoreCase);
			if (result != 0)
				return result;
			result = String.Compare(x.Gloss, y.Gloss, StringComparison.InvariantCultureIgnoreCase);
			if (result != 0)
				return result;
			return 0;
		}
	}

	#region Verse class

	/// <summary>
	/// A reference to a biblical term in a specific verse.
	/// </summary>
	public class Verse
	{
		string verseText;

		/// <summary>
		/// Reference for biblical term. Formats allowed:
		/// 1) 040001010 = MAT 1:10.
		/// 2) GEN 3:11    (Gen 3:11 "orignal" versification)
		/// 3) GEN 3:11/4  (Gen 3:11 "English" versification)
		/// This is not used directly at his used in directly by the XML serialization code.
		/// </summary>
		[XmlText]
		public string VerseText
		{
			get { return verseText; }
			set { verseText = value; }
		}
	}

	#endregion

}
