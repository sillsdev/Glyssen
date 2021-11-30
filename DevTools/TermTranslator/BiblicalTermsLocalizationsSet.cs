using System;
using System.IO;
using SIL.IO;
using SIL.Xml;

namespace DevTools.TermTranslator
{
	public class BiblicalTermsLocalizationsSet
	{
		private static readonly string s_paratextTermsListsPath;
		private bool m_prevLoaded;
		private BiblicalTermsLocalizations m_previous;
		public string Locale { get; }
		public BiblicalTermsLocalizations Current { get; }
		private string PathToLocalResource => Path.Combine("..\\..\\DevTools\\Resources", FileName);
		public BiblicalTermsLocalizations Previous
		{
			get
			{
				if (!m_prevLoaded)
				{
					m_previous = DeserializeBiblicalTermsForLanguage(PathToLocalResource);
					m_prevLoaded = true;
				}
				return m_previous;
			}
		}

		private string FileName
		{
			get
			{
				// Paratext identifies its two supported versions of Chinese using "zh-Hant"
				// (traditional) and "zh-Hans" (simplified), but Glyssen (and crowdin) use
				// "zh-TW" (Taiwanese Chinese) and "zh-CN" (mainland Chinese). But just to make
				// it more confusing, for the filename, Glyssen uses simply zh instead of zh-TW.
				switch (Locale)
				{
					case "zh": // Traditional script used in Taiwan
						return "BiblicalTermszh-Hant.xml";
					case "zh-CN": // Simplified script used in mainland China
						return "BiblicalTermszh-Hans.xml";
					default:
						return $"BiblicalTerms{Locale}.xml";
				}
			}
		}

		static BiblicalTermsLocalizationsSet()
		{
			s_paratextTermsListsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
				$"Paratext {Processor.kCurrentParatextVersion}", "Terms", "Lists");
			if (!Directory.Exists(s_paratextTermsListsPath))
			{
				Console.WriteLine($"Could not find valid Paratext installation. Folder does not exist: {s_paratextTermsListsPath}");
				s_paratextTermsListsPath = null;
			}
		}

		public BiblicalTermsLocalizationsSet(string locale)
		{
			Locale = locale;

			if (s_paratextTermsListsPath != null)
				Current = DeserializeBiblicalTermsForLanguage(Path.Combine(s_paratextTermsListsPath, FileName));

			if (Current == null)
			{
				Current = Previous;
				m_previous = null;
			}
		}

		public void ReplacePreviousWithCurrent()
		{
			if (s_paratextTermsListsPath != null)
			{
				var pathToParatextFile = Path.Combine(s_paratextTermsListsPath, FileName);
				if (File.Exists(pathToParatextFile))
					RobustFile.Copy(pathToParatextFile, PathToLocalResource, true);
			}

			m_previous = null;
		}

		private static BiblicalTermsLocalizations DeserializeBiblicalTermsForLanguage(string filePath)
		{
			try
			{
				return XmlSerializationHelper.DeserializeFromFile<BiblicalTermsLocalizations>(
					filePath);
			}
			catch (Exception e)
			{
				Console.WriteLine($"Could not deserialize Paratext terms from {filePath}");
				Console.WriteLine(e.Message);
				return null;
			}
		}
	}
}
