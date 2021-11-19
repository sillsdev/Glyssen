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

		private string FileName => $"BiblicalTerms{Locale}.xml";

		static BiblicalTermsLocalizationsSet()
		{
			s_paratextTermsListsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
				$"Paratext {Processor.s_currentParatextVersion}", "Terms", "Lists");
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
