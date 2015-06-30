using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Glyssen.Character;
using SIL.Xml;
using System.Text.RegularExpressions;

namespace DevTools.TermTranslator
{
	public class Processor
	{
		private static readonly List<string> LanguagesToProcess = new List<string>{"En", "Es", "Fr", "Pt", "zh-Hans", "zh-Hant"}; 

		public static void Process()
		{
			IEnumerable<Glyssen.Character.CharacterVerse> quoteInfo = ControlCharacterVerseData.Singleton.GetAllQuoteInfo();

			SortedSet<string> names = new SortedSet<string>();
			names.UnionWith(quoteInfo.Select(t => t.Alias));
			names.UnionWith(quoteInfo.Select(t => t.Character));
			names.Remove("");
			names.Remove(null);

			BiblicalTermsLocalizations englishTermsList = XmlSerializationHelper.DeserializeFromFile<BiblicalTermsLocalizations>("..\\..\\Resources\\BiblicalTermsEn.xml");
			
			foreach (string langAbbr in LanguagesToProcess)
			{
				BiblicalTermsLocalizations localTermsList = XmlSerializationHelper.DeserializeFromFile<BiblicalTermsLocalizations>("..\\..\\Resources\\BiblicalTerms" + langAbbr + ".xml");

				string modifiedLangAbbr = Char.ToLowerInvariant(langAbbr[0]) + langAbbr.Substring(1);

				string outputFileName = "..\\..\\..\\DistFiles\\localization\\Glyssen." + modifiedLangAbbr + ".tmx";

				TmxFormat newTmx;
				if (File.Exists(outputFileName))
				{
					newTmx = XmlSerializationHelper.DeserializeFromFile<TmxFormat>(outputFileName);

					var tus = newTmx.Body.Tus;

					tus.RemoveAll(t => t.Tuid.StartsWith("CharacterName."));
				}
				else
				{
					newTmx = new TmxFormat();
					newTmx.Header.SrcLang = modifiedLangAbbr;
					newTmx.Header.Props = new Prop[2];
					newTmx.Header.Props[0] = new Prop("x-appversion", "0.1.0.0");
					newTmx.Header.Props[1] = new Prop("x-hardlinebreakreplacement", "\\n");
				}

				Regex notChineseGlossChineseGloss = new Regex("(（|。).+");

				foreach (string name in names)
				{
					Tu tmxTermEntry = new Tu("CharacterName." + name);

					tmxTermEntry.Prop = new Prop("x-dynamic", "true");

					Tuv englishTuv = new Tuv("en", name);

					tmxTermEntry.Tuvs.Add(englishTuv);

					if (modifiedLangAbbr == "en")
					{
						if (!name.Contains("narrator"))
						{
							newTmx.Body.Tus.Add(tmxTermEntry);
						}
					}
					else
					{
						Localization term = englishTermsList.Terms.Locals.Find(t => t.Gloss == name);

						if (term != null)
						{
							string termId = term.Id;

							Localization localTerm = localTermsList.Terms.Locals.Find(t => t.Id == termId);

							if (localTerm != null)
							{
								string localGloss = localTerm.Gloss;

								string testOut = notChineseGlossChineseGloss.Replace(localGloss, "");

								if (localGloss != "")
								{
									tmxTermEntry.Tuvs.Add(new Tuv(modifiedLangAbbr, testOut));

									newTmx.Body.Tus.Add(tmxTermEntry);
								}
							}
						}
					}
				}

				XmlSerializationHelper.SerializeToFile<TmxFormat>(outputFileName, newTmx);
			}
		}
	}
}
