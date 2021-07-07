using System;
using System.Collections.Generic;
using System.Linq;
using Glyssen.Shared;
using SIL.Scripture;

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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="book">FCBH standard three-letter book code</param>
		/// <param name="chapter">Chapter number as string (note: caller has cast as a double, so it is guaranteed to be numeric)</param>
		/// <param name="verse">Verse number as string (for a chapter row, this is "<<")</param>
		/// <param name="characterId">Contents of the Char column</param>
		/// <param name="text">Dictionary of language names to corresponding cell text</param>
		public ReferenceTextRow(string book, string chapter, string verse, string characterId, Dictionary<string, string> text)
		{
			m_text = text;
			Book = ConvertFcbhBookCodeToSilBookCode(book, out var bookNum);
			BookNum = bookNum;
			Chapter = chapter;
			Verse = verse;
			CharacterId = characterId;
			m_text = text;
			if (!m_text.ContainsKey("English"))
				throw new ArgumentException("English is required", nameof(text));

			IsSectionHead = CharacterId.StartsWith("Section Head_");
			if (IsSectionHead)
			{
				if (Book == "PSA" && Chapter == "119")
				{
					// Note: this is a bit fragile. Currently, FCBH uses something like this in their Char column:
					// Section Head_19_Psalms_119 (ALEPH)
					// It would be possible to write a regular expression to try to match that, but if they ever
					// changed it, it might begin to fail to match and would still be fragile.
					IsAcrosticHead = CharacterId.EndsWith(")");
					IsSectionHead = !IsAcrosticHead;
				}
			}
		}

		/// <summary>
		/// SIL standard three-letter book code (already converted from FCBH code, if needed)
		/// </summary>
		public string Book { get; }
		public int BookNum { get; }
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

		private static string ConvertFcbhBookCodeToSilBookCode(string fcbhBookCode, out int bookNum)
		{
			{
				bookNum = BCVRef.BookToNumber(fcbhBookCode);
				if (bookNum > 0)
					return fcbhBookCode;

				string silBookCode;
				switch (fcbhBookCode)
				{
					case "1SM":
						silBookCode = "1SA";
						break;
					case "2SM":
						silBookCode = "2SA";
						break;
					case "PSM":
						silBookCode = "PSA";
						break;
					case "PRV":
						silBookCode = "PRO";
						break;
					case "SOS":
						silBookCode = "SNG";
						break;
					case "EZE":
						silBookCode = "EZK";
						break;
					case "JOE":
						silBookCode = "JOL";
						break;
					case "NAH":
						silBookCode = "NAM";
						break;
					case "TTS":
						silBookCode =  "TIT";
						break;
					case "JMS":
						silBookCode =  "JAS";
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(fcbhBookCode), fcbhBookCode, "Unexpected Book code");
				}

				bookNum = BCVRef.BookToNumber(silBookCode);
				return silBookCode;
			}
		}
	}
}
