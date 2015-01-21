using System.Collections.Generic;
using System.IO;
using System.Linq;
using Palaso.Xml;
using ProtoScript.Character;

namespace DevTools.BiblicalTerms
{
	public class Processor
	{
		public static void Process()
		{
			var list = XmlSerializationHelper.DeserializeFromFile<BiblicalTermsList>("..\\..\\Resources\\BiblicalTerms.xml");
			List<Term> filteredList = list.Terms
				.Where(t => t.CategoryIds.Contains("PN"))
				.Where(t => t.Domain.Contains("person") || t.Domain.Contains("group") || t.Domain.Contains("title") || t.Domain.Contains("supernatural beings and powers"))
				.Where(t => !t.Id.EndsWith("(DC)"))
				.ToList();

			List<string> domains = filteredList.Distinct(new Term.DomainEqualityComparer()).Select(t => t.Domain).ToList();
			domains.Sort();
			File.WriteAllLines("..\\..\\Resources\\temporary\\UniqueDomains.txt", domains);

			List<string> categories = filteredList.Where(t => t.CategoryIds.Count > 1).Select(t => t.CategoryIds[1]).ToList();
			categories.Sort();
			File.WriteAllLines("..\\..\\Resources\\temporary\\categories.txt", categories);

			filteredList.Sort(new TermComparer());
			File.WriteAllLines("..\\..\\Resources\\temporary\\ProperNames.txt", filteredList.Select(t => t.ToTabDelimited()));

			var controlData = ControlCharacterVerseData.Singleton.GetAllQuoteInfo();
			File.WriteAllLines("..\\..\\Resources\\temporary\\duplicatesWithControl.txt", filteredList.Select(t => t.Gloss).Intersect(controlData.Select(c => c.Character)));
		}
	}
}
