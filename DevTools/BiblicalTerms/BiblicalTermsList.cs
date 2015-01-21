using System.Collections.Generic;
using System.Xml.Serialization;

namespace DevTools.BiblicalTerms
{
	/// <summary>
	/// Serializable object containing the Biblical terms for <see cref="BiblicalTerms.BiblicalTermsData"/> implementations
	/// </summary>
	[XmlRoot("BiblicalTermsList")]
	public sealed class BiblicalTermsList
	{
		private List<Term> terms = new List<Term>();

		[XmlElement("Term")]
		public List<Term> Terms
		{
			get { return terms; }
			set { terms = value; }
		}
	}
}
