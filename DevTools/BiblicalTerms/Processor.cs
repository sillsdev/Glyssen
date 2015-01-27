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
				.Where(t => t.CategoryIds.Contains("PN")) //PN = proper name
				.Where(t => t.Domain.Contains("person") || t.Domain.Contains("group") || t.Domain.Contains("title") || t.Domain.Contains("supernatural beings and powers"))
				.Where(t => !t.Id.EndsWith("(DC)")) //don't want deuterocanon
				.ToList();

			// See what domains we are working with
			//List<string> domains = filteredList.Distinct(Term.DomainComparer).Select(t => t.Domain).ToList();
			//domains.Sort();
			//File.WriteAllLines("..\\..\\Resources\\temporary\\UniqueDomains.txt", domains);

			// I used this to ensure there were no categories we didn't expect to sneak in
			//List<string> categories = filteredList.Where(t => t.CategoryIds.Count > 1).Select(t => t.CategoryIds[1]).ToList();
			//categories.Sort();
			//File.WriteAllLines("..\\..\\Resources\\temporary\\categories.txt", categories);

			filteredList.Sort(new Term.GlossDomainComparer());
			//File.WriteAllLines("..\\..\\Resources\\temporary\\ProperNames.txt", filteredList.Distinct(Term.GlossComparer).Select(t => t.ToTabDelimited()));

//			var controlData = CharacterDetailData.Singleton.GetAll();
			var controlData = ControlCharacterVerseData.Singleton.GetAllQuoteInfo();
			File.WriteAllLines("..\\..\\Resources\\temporary\\duplicatedInControl.txt", filteredList.Select(t => t.Gloss).Intersect(controlData.Select(c => c.Character)));

			var notInControlList = filteredList.Where(t => !controlData.Select(c => c.Character).Contains(t.Gloss));
			//File.WriteAllLines("..\\..\\Resources\\temporary\\notInControl.txt", notInControlList.Select(t => t.ToTabDelimited()));

			var combinedWithoutDuplicates = new BiblicalTermsList { Terms = notInControlList.ToList() }.CombinedReferencesWithoutDuplicates();
			File.WriteAllLines("..\\..\\Resources\\temporary\\NamesWithReferencesOnSameLine.txt", combinedWithoutDuplicates.Select(t => t.ToTabDelimited()));
			File.WriteAllLines("..\\..\\Resources\\temporary\\NamesWithReferencesOnSeparateLines.txt", combinedWithoutDuplicates.Select(t => t.ToTabDelimitedOneReferencePerLine()));
		}
	}
}
