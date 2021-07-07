using System;
using System.Collections.Generic;
using System.Linq;
using Glyssen.Shared;

namespace Glyssen.RefTextDevUtilities
{
	public class ReferenceTextData
	{
		private List<ReferenceTextLanguageInfo> m_languages = new List<ReferenceTextLanguageInfo>();
		private string[] m_refTextFilter;
		public List<ReferenceTextRow> ReferenceTextRows { get; }
		public bool IsValid => ReferenceTextRows.Any() && m_languages.Any();
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

		public ReferenceTextLanguageInfo(string name)
		{
			Name = name;
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

			IsSectionHead = CharacterId.StartsWith("Section Head_");
			if (IsSectionHead)
			{
				IsAcrosticHead = CharacterId.EndsWith(")");
				IsSectionHead = !IsAcrosticHead;
			}
		}

		public string Book { get; }
		public string Chapter { get; }
		public string Verse { get; }
		public string CharacterId { get; }
		public string English => m_text["English"];
		public bool IsSectionHead { get; }
		public bool IsAcrosticHead { get; }

		public string GetText(string language)
		{
			return m_text[language];
		}

		public override string ToString()
		{
			return $"{Book} {Chapter} {Verse} {CharacterId} {English}";
		}
	}
}
