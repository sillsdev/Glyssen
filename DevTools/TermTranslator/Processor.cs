using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SIL.Xml;
using System.Text.RegularExpressions;
using GlyssenEngine.Character;
using L10NSharp.XLiffUtils;
using SIL.Extensions;

namespace DevTools.TermTranslator
{
	/// <summary>
	/// "DevTool" class to update the English XLIFF file from character/alias info and attempt to
	/// compute (sometimes partially) localized strings for any languages for which Paratext has
	/// localized biblical terms. This process also does a preliminary cleanup step to remove any
	/// entries from the localized files that are marked as "needs-translation" since L10nSharp
	/// already nicely handles missing localizations and we do not expect anyone to directly
	/// localize these files outside of Crowdin. As noted in the menu that is displayed in the
	/// DevTool program, it is recommended that before running this update, the maintainer should
	/// first get latest versions from crowdin.
	/// </summary>
	public static class Processor
	{
		// #############################
		// ######### IMPORTANT #########
		// #############################
		// If Paratext is not installed or is installed in a non-standard location, the (possibly
		// outdated) files in DevTools\Resources will be used. If a new version of Paratext is
		// installed, update s_currentParatextVersion.
		// To add a new localization, simply create a file in DistFiles\localization named
		// Glyssen.LOCALE.xlf (where LOCALE is the ICU locale). Model it off one of the other ones and
		// set the target-language attribute to be the locale as it comes from Crowdin (usually, but not always,
		// the same as the locale used in the file name). The body should be left empty; it is not necessary to
		// include any tarns-units in order to run this process.
		internal static readonly string s_currentParatextVersion = "9";
		private static string s_glyssenXlfFilePrefix = "Glyssen.";
		private static string s_XliffExt = ".xlf";

		private static readonly Regex s_partOfChineseOrFrenchGlossThatIsNotTheGloss = new Regex("((（|。).+)|(\\[1\\] )", RegexOptions.Compiled);
		private static readonly SortedSet<string> s_names = new SortedSet<string>();
		private static BiblicalTermsLocalizations s_englishTermsList;
		private static List<XLiffTransUnit> s_englishTranslationUnits;
		// For each locale, this is list of newly computed localizations (i.e., different from the
		// previously calculated version) that conflict with the current translation. In the case
		// of these conflicts, the existing (presumably human-edited) translation is preserved.
		private static readonly Dictionary<string, List<Tuple<XLiffTransUnit, string>>> s_conflictingLocalizations =
			new Dictionary<string, List<Tuple<XLiffTransUnit, string>>>();

		private const string kLocalizationFolder = @"..\..\DistFiles\localization";

		public static void Process()
		{
			foreach (var cv in ControlCharacterVerseData.Singleton.GetAllQuoteInfo())
			{
				AddNames(cv.Character);
				if (!string.IsNullOrEmpty(cv.Alias))
					AddNames(cv.Alias);
				if (!string.IsNullOrEmpty(cv.DefaultCharacter))
					AddNames(cv.DefaultCharacter);
			}

			s_englishTermsList = new BiblicalTermsLocalizationsSet("En").Current;

			s_englishTranslationUnits = ProcessLanguage("en", (doc, unit, s) => doc.AddTransUnit(unit));

			foreach (string locale in LanguagesToProcess)
			{
				BiblicalTermsLocalizationsSet localTermsList = new BiblicalTermsLocalizationsSet(locale);
				if (localTermsList.Current == null)
					continue;
				ProcessLanguage(locale, (xlf, tu, name) => { AddLocalizedTerm(xlf, localTermsList, tu, name); });
				localTermsList.ReplacePreviousWithCurrent();
			}

			foreach (var conflictingLocalization in s_conflictingLocalizations)
			{
				var path = Path.Combine(kLocalizationFolder, $"LocalizationsFromParatextBiblicalTerms.{conflictingLocalization.Key}.txt");
				using (var writer = new StreamWriter(path, false))
				{
					foreach (var tuple in conflictingLocalization.Value)
						writer.WriteLine($"{tuple.Item1.Id}\t{tuple.Item1.Source.Value}\t{tuple.Item2}");
				}
			}
		}

		private static IEnumerable<string> LanguagesToProcess =>
			Directory.GetFiles(kLocalizationFolder, s_glyssenXlfFilePrefix + "*" + s_XliffExt)
				.Select(f => Path.GetFileNameWithoutExtension(f).Substring(s_glyssenXlfFilePrefix.Length))
				.Where(l => !l.Equals("en", StringComparison.OrdinalIgnoreCase));

		private static void AddNames(string character)
		{
			if (!character.StartsWith("narrator-"))
				s_names.AddRange(character.Split('/'));
		}

		private static List<XLiffTransUnit> ProcessLanguage(string locale, Action<XLiffDocument, XLiffTransUnit, string> addTerm)
		{
			string xlfFileName = Path.Combine(kLocalizationFolder, s_glyssenXlfFilePrefix + locale + s_XliffExt);

			XLiffDocument newXlf = XLiffDocument.Read(xlfFileName);

			bool english = locale.Equals("En", StringComparison.OrdinalIgnoreCase);
			IComparer<XLiffTransUnit> comparer= new TransUnitComparer(english);
			if (!english)
				RemoveUntranslatedAndDeletedEntries(newXlf);

			foreach (string name in s_names)
			{
				var id = "CharacterName." + name;
				XLiffTransUnit xlfTermEntry = new XLiffTransUnit
				{
					Id = id,
					Dynamic = true,
					Source = new XLiffTransUnitVariant { Lang = "en" },
					Notes = new List<XLiffNote>(new [] { new XLiffNote { Text =$"ID: {id}" }})
				};

				addTerm(newXlf, xlfTermEntry, name);
			}

			newXlf.File.Body.TransUnits.Sort(comparer);

			newXlf.Save(xlfFileName);
			if (!english)
			{
				// Unfortunately the serializer puts the xml:lang and state attributes
				// in the opposite order from what crowdin does and also swaps the order
				// of the note and target elements, which makes it hard to see the real
				// differences.
				var regexSwapper = new Regex("<target (?<state>state=\"[^\"]*\") " +
					$"(?<lang>xml:lang=\"{newXlf.File.TargetLang}\")>(?<localization>[^<]+)<\\/target>" +
					@"\s*(?<linebreak>(\r|\n)+\s*)(?<note><note>[^\r\n]+)", RegexOptions.Compiled);
				var swapped = regexSwapper.Replace(File.ReadAllText(xlfFileName),
					"${note}${linebreak}<target ${lang} ${state}>${localization}</target>");
				using (var writer = new StreamWriter(xlfFileName))
					writer.Write(swapped);
			}

			return newXlf.File.Body.TransUnits;
		}

		private static void RemoveUntranslatedAndDeletedEntries(XLiffDocument newXlf)
		{
			newXlf.File.Body.TransUnits.RemoveAll(tu =>
				tu.Target.TargetState == XLiffTransUnitVariant.TranslationState.NeedsTranslation ||
				tu.Dynamic && !s_englishTranslationUnits.Any(ent => ent.Id == tu.Id));
		}

		private static void AddLocalizedTerm(XLiffDocument newXlf, BiblicalTermsLocalizationsSet localTerms,
			XLiffTransUnit xlfTermEntry, string name)
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
				BiblicalTermsLocalizations termLocalizations = localTerms.Current;

				var localization = ComputeLocalization(termLocalizations, term, endingPunct, parts);

				if (localization != null)
				{
					var existingTranslationUnit = newXlf.GetTransUnitForId(xlfTermEntry.Id);
					var existingTarget = existingTranslationUnit?.Target;
					if (existingTarget == null)
						AddEntryWithLocalizedGloss(newXlf, xlfTermEntry, localization);
					else if (existingTarget.Value != localization)
					{
						var prevComputedLocalization = ComputeLocalization(localTerms.Previous, term, endingPunct, parts);
						if (localization == prevComputedLocalization)
						{
							var locale = newXlf.File.TargetLang;
							// Conflicts are most likely to come from human localization work. But it's also possible that
							// we're processing updates to the Paratext Biblical Terms.
							if (!s_conflictingLocalizations.TryGetValue(locale, out var conflictList))
								s_conflictingLocalizations[locale] = conflictList = new List<Tuple<XLiffTransUnit, string>>();
								
							conflictList.Add(new Tuple<XLiffTransUnit, string>(existingTranslationUnit, localization));
						}

						existingTarget.Value = localization;
					}
				}
			}
		}

		private static string ComputeLocalization(BiblicalTermsLocalizations termLocalizations,
			Localization term, string endingPunct, IReadOnlyList<string> nameParts)
		{
			string englishGloss;
			Localization localTerm = termLocalizations.Terms.Locals.Find(t => t.Id == term.Id);

			string localization = null;

			if (localTerm != null)
			{
				string localGloss = s_partOfChineseOrFrenchGlossThatIsNotTheGloss.Replace(localTerm.Gloss, "");

				if (localGloss != "")
				{
					var parts = nameParts.ToArray();
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
								localTerm = termLocalizations.Terms.Locals.Find(t => t.Id == term.Id);

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

					localization = String.Join(" ", parts);
				}
			}

			return localization;
		}

		private static void AddEntryWithLocalizedGloss(XLiffDocument newXlf, XLiffTransUnit xlfTermEntry, string newLocalization)
		{
			xlfTermEntry.Target = new XLiffTransUnitVariant
			{
				Lang = newXlf.File.TargetLang,
				TargetState = XLiffTransUnitVariant.TranslationState.Translated,
				Value = newLocalization
			};
			newXlf.AddTransUnit(xlfTermEntry);
		}

		private class TransUnitComparer : IComparer<XLiffTransUnit>
		{
			private readonly bool m_english;

			public TransUnitComparer(bool english)
			{
				m_english = english;
			}

			public int Compare(XLiffTransUnit x, XLiffTransUnit y)
			{
				if (ReferenceEquals(x, y))
					return 0;
				if (ReferenceEquals(null, y))
					return 1;
				if (ReferenceEquals(null, x))
					return -1;
				if (m_english)
				{
					// This section just preserves the existing order to make comparisons easier
					int prefixValX = x.Id.StartsWith("CharacterName.Standard") ? 0 :
						(x.Id.StartsWith("Common.BookName") ? 1 : Int32.MaxValue);
					int prefixValY = y.Id.StartsWith("CharacterName.Standard") ? 0 :
						(y.Id.StartsWith("Common.BookName") ? 1 : Int32.MaxValue);
					if (prefixValX != prefixValY)
						return prefixValX.CompareTo(prefixValY);
				}

				return string.Compare(x.Id, y.Id, StringComparison.InvariantCultureIgnoreCase);
			}
		}
	}
}
