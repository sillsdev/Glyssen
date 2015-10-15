using System.Collections.Generic;
using System.Xml.Serialization;

namespace DevTools.BiblicalTerms
{
	[XmlRoot("BiblicalTermsList")]
	public sealed class BiblicalTermsList
	{
		public BiblicalTermsList()
		{
			Terms = new List<Term>();
		}

		[XmlElement("Term")]
		public List<Term> Terms { get; set; }

		public List<Term> CombinedReferencesWithoutDuplicates()
		{
			var sortedOriginal = new List<Term>(Terms);
			sortedOriginal.Sort(new Term.GlossDomainComparer());

			var noDuplicateList = new List<Term>();
			Term priorTerm = null;
			foreach (Term term in sortedOriginal)
			{
				if (priorTerm == null || term.Gloss != priorTerm.Gloss)
				{
					if (priorTerm != null)
					noDuplicateList.Add(priorTerm);
					priorTerm = new Term
					{
						CategoryIds = term.CategoryIds,
						Domain = term.Domain,
						Gloss = term.Gloss,
						Id = term.Id,
						References = term.References
					};
				}
				else
				{
					priorTerm.References.AddRange(term.References);
				}
			}
			noDuplicateList.Add(priorTerm);
			return noDuplicateList;
		}
	}
}
