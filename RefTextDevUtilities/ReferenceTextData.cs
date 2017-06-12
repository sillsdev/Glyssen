using System;
using System.Collections.Generic;
using System.Linq;
using Glyssen.Shared;
using Glyssen.Shared.Bundle;

namespace Glyssen.RefTextDevUtilities
{
	public class ReferenceTextData
	{
		private List<ReferenceTextLanguageInfo> m_languages = new List<ReferenceTextLanguageInfo>();
		private string[] m_refTextFilter;
		public List<ReferenceTextRow> ReferenceTextRows { get; }
		public bool IsValid => !ReferenceTextRows.Any() || !m_languages.Any();
		public IEnumerable<ReferenceTextLanguageInfo> LanguagesToProcess => (m_refTextFilter == null) ? m_languages :
				m_languages.Where(l => m_refTextFilter.Contains(l.Name));

		public ReferenceTextData()
		{
			ReferenceTextRows = new List<ReferenceTextRow>();
		}

		public void FilterBy(params string[] refTextId)
		{
			if (refTextId == null || !refTextId.Any())
				m_refTextFilter = null;
			else
				m_refTextFilter = refTextId;
		}

		public void AddLanguage(ReferenceTextLanguageInfo languageInfo)
		{
			m_languages.Add(languageInfo);
		}

		public void AddLanguages(IEnumerable<string> languageNames)
		{
			foreach (var languageName in languageNames)
				AddLanguage(new ReferenceTextLanguageInfo(languageName));
		}

		public ReferenceTextLanguageInfo GetLanguageInfo(string languageName)
		{
			return m_languages.Single(l => l.Name == languageName);
		}
	}

	public class ReferenceTextLanguageInfo
	{
		public string Name { get;  }
		public string OutputFolder { get; set; }

		public ReferenceTextLanguageInfo(string name, string outputFolder = null)
		{
			Name = name;
			if (outputFolder == null)
			{

			}
			OutputFolder = outputFolder;
		}

		public bool IsEnglish => Name == ReferenceTextType.English.ToString();
	}

	public class ReferenceTextRow
	{
		private readonly Dictionary<string, string> m_text;

		public ReferenceTextRow(string book, string chapter, string verse, string characterId, Dictionary<string, string> text)
		{
			m_text = text;
			Book = book;
			Chapter = chapter;
			Verse = verse;
			CharacterId = characterId;
			m_text = text;
			if (!m_text.ContainsKey("English"))
				throw new ArgumentException("English is required", nameof(text));
		}

		public string Book { get; set; }
		public string Chapter { get; set; }
		public string Verse { get; set; }
		public string CharacterId { get; set; }
		public string English => m_text["English"];

		public string GetText(string language)
		{
			return m_text[language];
		}

		public override string ToString()
		{
			return string.Format("{0} {1} {2} {3} {4}", Book, Chapter, Verse, CharacterId, English);
		}
	}
}
