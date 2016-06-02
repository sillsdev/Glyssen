using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using SIL.Scripture;

namespace DevTools.BiblicalTerms
{

	#region Term class

	/// <summary>
	/// A Biblical term. Contains no localizable data (see TermLocalization)
	/// </summary>
	public class Term
	{
		List<string> m_categoryIds;
		private IEnumerable<string> m_uniqueOtNtBooks;

		/// <summary>
		/// Unique identifier for this term.
		/// </summary>
		[XmlAttribute("Id")]
		public string Id { get; set; }

		/// <summary>
		/// Comma separated list of categories from CategoryIds. Usually a term only has one category
		/// but occasionally it may have multiple categories.
		/// </summary>
		[XmlElement("Category")]
		public List<string> CategoryIds
		{
			get { return m_categoryIds; }
			set { m_categoryIds = value ?? new List<string>(); }
		}

		[XmlElement("Domain")]
		public string Domain { get; set; }

		public bool IsMultiCharacter
		{
			get { return Domain != null && Domain.Contains("group"); }
		}

		[XmlElement("Gloss")]
		public string Gloss { get; set; }

		public List<Verse> References { get; set; }

		public IEnumerable<string> UniqueOtNtBooks
		{
			get
			{
				if (m_uniqueOtNtBooks != null)
					return m_uniqueOtNtBooks;
				return m_uniqueOtNtBooks = GetUniqueOtNtBooks();
			}
		}

		public override string ToString()
		{
			return Id;
		}

		public string ToTabDelimited()
		{
			return Gloss + "\t" + ListUniqueOtNtBooks() + "\t" + IsMultiCharacter + "\t" + Domain;
		}

		public string ToTabDelimitedOneReferencePerLine()
		{
			var sb = new StringBuilder();
			foreach (string book in UniqueOtNtBooks)
				sb.Append(book + "\t" + Gloss + "\t" + IsMultiCharacter + "\t" + Domain).Append(Environment.NewLine);
			return sb.ToString().Substring(0, sb.ToString().Length - Environment.NewLine.Length);
		}

		private string ListReferences()
		{
			var sb = new StringBuilder();
			foreach (Verse reference in References)
				sb.Append(reference.VerseText).Append(", ");
			return sb.ToString();
		}

		private string ListUniqueOtNtBooks()
		{
			var sb = new StringBuilder();
			foreach (string book in UniqueOtNtBooks)
				sb.Append(book).Append(", ");
			return sb.ToString().Substring(0, sb.ToString().Length-2);
		}

		private IEnumerable<string> GetUniqueOtNtBooks()
		{
			var books = new SortedSet<string>(Comparer<string>.Create((a, b) => BCVRef.BookToNumber(a).CompareTo(BCVRef.BookToNumber(b))));
			foreach (Verse reference in References)
			{
				var bcvRef = new BCVRef(reference.VerseText);
				if (bcvRef.BookIsValid)
					books.Add(BCVRef.NumberToBookCode(bcvRef.Book));
				else if (Char.IsLetter(reference.VerseText.Last()))
				{
					bcvRef = new BCVRef(reference.VerseText.Substring(0, reference.VerseText.Length - 1));
					if (bcvRef.BookIsValid)
						books.Add(BCVRef.NumberToBookCode(bcvRef.Book));
					else
						Debug.Fail(reference.VerseText);
				}
				else
					Debug.Fail(reference.VerseText);
			}
			return books;
		}

		#region comparers
		#region DomainComparer
		private sealed class DomainEqualityComparer : IEqualityComparer<Term>
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

		private static readonly IEqualityComparer<Term> s_domainComparerInstance = new DomainEqualityComparer();

		public static IEqualityComparer<Term> DomainComparer
		{
			get { return s_domainComparerInstance; }
		}
		#endregion

		#region GlossComparer
		private sealed class GlossEqualityComparer : IEqualityComparer<Term>
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
				return string.Equals(x.Gloss, y.Gloss);
			}

			public int GetHashCode(Term obj)
			{
				return (obj.Gloss != null ? obj.Gloss.GetHashCode() : 0);
			}
		}

		private static readonly IEqualityComparer<Term> s_glossComparerInstance = new GlossEqualityComparer();

		public static IEqualityComparer<Term> GlossComparer
		{
			get { return s_glossComparerInstance; }
		}
		#endregion

		public class DomainGlossComparer : IComparer<Term>
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

		public class GlossDomainComparer : IComparer<Term>
		{
			public int Compare(Term x, Term y)
			{
				int result = String.Compare(x.Gloss, y.Gloss, StringComparison.InvariantCultureIgnoreCase);
				if (result != 0)
					return result;
				result = String.Compare(x.Domain, y.Domain, StringComparison.InvariantCultureIgnoreCase);
				if (result != 0)
					return result;
				return 0;
			}
		}
		#endregion
	}

	#endregion

	#region Verse class

	/// <summary>
	/// A reference to a biblical term in a specific verse.
	/// </summary>
	public class Verse
	{
		/// <summary>
		/// Reference for biblical term. Formats allowed:
		/// 1) 040001010 = MAT 1:10.
		/// 2) GEN 3:11    (Gen 3:11 "orignal" versification)
		/// 3) GEN 3:11/4  (Gen 3:11 "English" versification)
		/// This is not used directly at his used in directly by the XML serialization code.
		/// </summary>
		[XmlText]
		public string VerseText { get; set; }
	}

	#endregion

}
