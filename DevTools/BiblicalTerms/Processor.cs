using System.Collections.Generic;
using System.IO;
using System.Linq;
using GlyssenEngine.Character;
using SIL.Xml;

namespace DevTools.BiblicalTerms
{
	public class Processor
	{
		public static void Process()
		{
			var list = XmlSerializationHelper.DeserializeFromFile<BiblicalTermsList>("..\\..\\DevTools\\Resources\\BiblicalTerms.xml");
			List<Term> filteredList = list.Terms
				.Where(t => t.CategoryIds.Contains("PN")) //PN = proper name
				.Where(t => t.Domain.Contains("person") || t.Domain.Contains("group") || t.Domain.Contains("title") || t.Domain.Contains("supernatural beings and powers"))
				.Where(t => !t.Id.EndsWith("(DC)")) //don't want deuterocanon
				.ToList();

			// See what domains we are working with
			//List<string> domains = filteredList.Distinct(Term.DomainComparer).Select(t => t.Domain).ToList();
			//domains.Sort();
			//File.WriteAllLines("..\\..\\DevTools\\Resources\\temporary\\UniqueDomains.txt", domains);

			// I used this to ensure there were no categories we didn't expect to sneak in
			//List<string> categories = filteredList.Where(t => t.CategoryIds.Count > 1).Select(t => t.CategoryIds[1]).ToList();
			//categories.Sort();
			//File.WriteAllLines("..\\..\\DevTools\\Resources\\temporary\\categories.txt", categories);

			filteredList.Sort(new Term.GlossDomainComparer());
			//File.WriteAllLines("..\\..\\DevTools\\Resources\\temporary\\ProperNames.txt", filteredList.Distinct(Term.GlossComparer).Select(t => t.ToTabDelimited()));

			Directory.CreateDirectory("..\\..\\DevTools\\Resources\\temporary");
			var controlData = CharacterDetailData.Singleton.GetAll();
			File.WriteAllLines("..\\..\\DevTools\\Resources\\temporary\\duplicatedInControl.txt", filteredList.Select(t => t.Gloss).Intersect(controlData.Select(c => c.CharacterId)));

			var notInControlList = filteredList.Where(t => !controlData.Select(c => c.CharacterId).Contains(t.Gloss));
			//File.WriteAllLines("..\\..\\DevTools\\Resources\\temporary\\notInControl.txt", notInControlList.Select(t => t.ToTabDelimited()));

			var combinedWithoutDuplicates = new BiblicalTermsList { Terms = notInControlList.ToList() }.CombinedReferencesWithoutDuplicates();
			File.WriteAllLines("..\\..\\DevTools\\Resources\\temporary\\NamesWithReferencesOnSameLine.txt", combinedWithoutDuplicates.Select(t => t.ToTabDelimited()));
			File.WriteAllLines("..\\..\\DevTools\\Resources\\temporary\\NamesWithReferencesOnSeparateLines.txt", combinedWithoutDuplicates.Select(t => t.ToTabDelimitedOneReferencePerLine()));
		}
	}
}
