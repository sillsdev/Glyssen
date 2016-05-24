using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Glyssen.Character;
using SIL.Xml;
using System.Text.RegularExpressions;

namespace DevTools.TermTranslator
{
	public static class Processor
	{
		// #############################
		// ######### IMPORTANT #########
		// #############################
		// The following three variables need to be updated as appropriate whenever:
		// A) New/modified localizations of the Biblical Terms files (from Paratext) become available.
		// B) New HUMAN localizations of TMX files are done for Glyssen.
		private static readonly List<string> LanguagesToProcess = new List<string> { "Es", "Fr", "Pt", "zh-Hans", "zh-Hant" };
		private static readonly List<string> LanguagesWithCustomizedTranslations = new List<string> {"es", "fr", "pt"};
		private static readonly bool ProcessingUpdatedBiblicalTermsFiles = false;

		private static readonly Regex s_partOfChineseOrFrenchGlossThatIsNotTheGloss = new Regex("((（|。).+)|(\\[1\\] )", RegexOptions.Compiled);
		private static readonly SortedSet<string> s_names = new SortedSet<string>();
		private static BiblicalTermsLocalizations s_englishTermsList;
		private static List<Tu> s_englishTranslationUnits;
		private static readonly Dictionary<string, TmxFormat> s_conflictingLocalizations = new Dictionary<string, TmxFormat>();

		private const string kLocalizationFolder = @"..\..\DistFiles\localization";

		public static void Process()
		{
			foreach (var cv in ControlCharacterVerseData.Singleton.GetAllQuoteInfo())
			{
				var s = cv.LocalizedAlias;
				AddNames(cv.Character);
				if (!string.IsNullOrEmpty(cv.Alias))
					AddNames(cv.Alias);
				if (!string.IsNullOrEmpty(cv.DefaultCharacter))
					AddNames(cv.DefaultCharacter);
			}

			s_englishTermsList = DeserializeBiblicalTermsForLanguage("En");

			s_englishTranslationUnits = ProcessLanguage("en", AddEnglishTerm);

			foreach (string langAbbr in LanguagesToProcess)
			{
				BiblicalTermsLocalizations localTermsList = DeserializeBiblicalTermsForLanguage(langAbbr);
				var modifiedLangAbbr = Char.ToLowerInvariant(langAbbr[0]) + langAbbr.Substring(1);
				Action<TmxFormat, Tu, Tuv> processLocalizedGloss = LanguagesWithCustomizedTranslations.Contains(modifiedLangAbbr)
					? (Action<TmxFormat, Tu, Tuv>)UpdateEntryWithLocalizedGloss : AddEntryWithLocalizedGloss;
				ProcessLanguage(modifiedLangAbbr,
					(tmx, tu, name) => { AddLocalizedTerm(tmx, modifiedLangAbbr, localTermsList, tu, name, processLocalizedGloss); });
			}

			foreach (var conflictingLocalization in s_conflictingLocalizations)
			{
				var path = Path.Combine(kLocalizationFolder, "LocalizationsFromParatextBiblicalTerms." + conflictingLocalization.Key + ".tmx");
				XmlSerializationHelper.SerializeToFile(path, conflictingLocalization.Value);
			}
		}

		private static void AddNames(string character)
		{
			foreach (string individual in character.Split('/'))
				s_names.Add(individual);
		}

		private static List<Tu> ProcessLanguage(string modifiedLangAbbr, Action<TmxFormat, Tu, string> AddTerm)
		{
			string outputFileName = Path.Combine(kLocalizationFolder, "Glyssen." + modifiedLangAbbr + ".tmx");

			TmxFormat newTmx;
			if (File.Exists(outputFileName))
			{
				newTmx = XmlSerializationHelper.DeserializeFromFile<TmxFormat>(outputFileName);

				var tus = newTmx.Body.Tus;

				// If this is not a language that has been worked on by a localizer, we can safely blow
				// away everything and start from scratch. Otherwise, we only want to remove translation units
				// which no longer exist in English.
				if (LanguagesWithCustomizedTranslations.Contains(modifiedLangAbbr))
					tus.RemoveAll(lt => lt.Tuid.StartsWith("CharacterName.") && !s_englishTranslationUnits.Any(ent => ent.Tuid == lt.Tuid));
				else
					tus.RemoveAll(t => t.Tuid.StartsWith("CharacterName."));
			}
			else
			{
				newTmx = new TmxFormat();
				newTmx.Header.SrcLang = modifiedLangAbbr;
				newTmx.Header.Props = new Prop[2];
				newTmx.Header.Props[0] = new Prop("x-appversion", "0.11.0.0");
				newTmx.Header.Props[1] = new Prop("x-hardlinebreakreplacement", "\\n");
			}

			foreach (string name in s_names)
			{
				Tu tmxTermEntry = new Tu("CharacterName." + name) { Prop = new Prop("x-dynamic", "true") };
				tmxTermEntry.Tuvs.Add(new Tuv("en", name));

				AddTerm(newTmx, tmxTermEntry, name);
			}

			XmlSerializationHelper.SerializeToFile(outputFileName, newTmx);

			return newTmx.Body.Tus;
		}

		private static BiblicalTermsLocalizations DeserializeBiblicalTermsForLanguage(string langAbbr)
		{
			return XmlSerializationHelper.DeserializeFromFile<BiblicalTermsLocalizations>(
				"..\\..\\DevTools\\Resources\\BiblicalTerms" + langAbbr + ".xml");
		}

		private static void AddEnglishTerm(TmxFormat newTmx, Tu tmxTermEntry, string name)
		{
			if (!name.Contains("narrator"))
				newTmx.Body.Tus.Add(tmxTermEntry);
		}

		private static void AddLocalizedTerm(TmxFormat newTmx, string modifiedLangAbbr, BiblicalTermsLocalizations localTermsList,
			Tu tmxTermEntry, string name, Action<TmxFormat, Tu, Tuv> ProcessLocalizedGloss)
		{
			// We only want to break a character ID into separate words for individual localization if it begins with a
			// proper name.
			int maxParts = Char.IsUpper(name[0]) ? Int32.MaxValue : 1;

			string[] parts = name.Split(new[] { ' ' }, maxParts, StringSplitOptions.RemoveEmptyEntries);

			string englishGloss = parts[0];
			string endingPunct;
			if (Char.IsPunctuation(englishGloss.Last()))
			{
				endingPunct = englishGloss.Last().ToString();
				englishGloss = englishGloss.Remove(englishGloss.Length - 1);
			}
			else
				endingPunct = null;

			Localization term = s_englishTermsList.Terms.Locals.Find(t => t.Gloss == englishGloss);
			if (term != null)
			{
				Localization localTerm = localTermsList.Terms.Locals.Find(t => t.Id == term.Id);

				if (localTerm != null)
				{
					string localGloss = s_partOfChineseOrFrenchGlossThatIsNotTheGloss.Replace(localTerm.Gloss, "");

					if (localGloss != "")
					{
						if (endingPunct != null)
							localGloss += endingPunct;
						parts[0] = localGloss;

						if (parts.Length > 1)
						{
							for (int i = 1; i < parts.Length; i++)
							{
								englishGloss = parts[i];
								bool openingParenthesis = false;
								if (englishGloss.StartsWith("("))
								{
									englishGloss = englishGloss.Substring(1);
									openingParenthesis = true;
								}
								if (Char.IsPunctuation(englishGloss.Last()))
								{
									endingPunct = englishGloss.Last().ToString();
									englishGloss = englishGloss.Remove(englishGloss.Length - 1);
								}
								else
									endingPunct = null;
								term = s_englishTermsList.Terms.Locals.Find(t => t.Gloss == englishGloss);
								if (term != null)
								{
									localTerm = localTermsList.Terms.Locals.Find(t => t.Id == term.Id);

									if (localTerm != null)
									{
										localGloss = s_partOfChineseOrFrenchGlossThatIsNotTheGloss.Replace(localTerm.Gloss, "");
										if (localGloss != "")
										{
											if (openingParenthesis)
												localGloss = "(" + localGloss;
											if (endingPunct != null)
												localGloss += endingPunct;
											parts[i] = localGloss;
											continue;
										}
									}
								}
								parts[i] = "***" + parts[i] + "***";
							}
						}

						var newTuv = new Tuv(modifiedLangAbbr, String.Join(" ", parts));
						ProcessLocalizedGloss(newTmx, tmxTermEntry, newTuv);
					}
				}
			}
		}

		private static void AddEntryWithLocalizedGloss(TmxFormat newTmx, Tu tmxTermEntry, Tuv newTuv)
		{
			tmxTermEntry.Tuvs.Add(newTuv);
			newTmx.Body.Tus.Add(tmxTermEntry);
		}

		private static void UpdateEntryWithLocalizedGloss(TmxFormat newTmx, Tu tmxTermEntry, Tuv newTuv)
		{
			var existingTranslationUnit = newTmx.Body.Tus.FirstOrDefault(t => t.Tuid == tmxTermEntry.Tuid);
			Tuv existingtuv = null;
			if (existingTranslationUnit != null)
			{
				existingtuv = existingTranslationUnit.Tuvs.FirstOrDefault(t => t.Lang == newTuv.Lang);
			}
			if (existingtuv != null)
			{
				if (ProcessingUpdatedBiblicalTermsFiles && existingtuv.LocalizedTerm != newTuv.LocalizedTerm)
				{
					// Unless we're processing new updates to the Paratext Biblical Terms, any conflicts must come
					// from previous human localization work, so we'll just leave them as they are and not even bother
					// reporting them.
					TmxFormat paratextTmxFormat;
					if (!s_conflictingLocalizations.TryGetValue(newTuv.Lang, out paratextTmxFormat))
						s_conflictingLocalizations[newTuv.Lang] = paratextTmxFormat = new TmxFormat(newTmx);
					paratextTmxFormat.Body.Tus.First((t => t.Tuid == tmxTermEntry.Tuid))
						.Tuvs.First(t => t.Lang == newTuv.Lang)
						.LocalizedTerm = newTuv.LocalizedTerm;
				}
			}
			else
			{
				AddEntryWithLocalizedGloss(newTmx, tmxTermEntry, newTuv);

				TmxFormat conflictingTmx;
				if (ProcessingUpdatedBiblicalTermsFiles && s_conflictingLocalizations.TryGetValue(newTuv.Lang, out conflictingTmx))
					conflictingTmx.Body.Tus.Add(tmxTermEntry.Clone());
			}
		}
	}
}
