using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Glyssen.Bundle;
using Glyssen.Character;
using Glyssen.VoiceActor;

namespace Glyssen.Dialogs
{
	public class CastSizePlanningViewModel
	{
		public event EventHandler<CastSizeValueChangedEventArgs> CastSizeRowValuesChanged;

		public event EventHandler<int> MaleNarratorsValueChanged;
		public event EventHandler<int> FemaleNarratorsValueChanged;

		private NarratorsOption m_narratorsOption;
		private CastSizeOption m_updatedCastSizeOption = CastSizeOption.NotSet;

		// values for custom settings
		private CastSizeRowValues m_updatedCustomActorCounts;

		// values from Voice Actor List
		private CastSizeRowValues m_actualActorCounts = null;
		private int m_customMaleNarratorCount;
		private int m_customFemaleNarratorCount;
		private readonly Dictionary<CastSizeOption, CastSizeRowValues> m_baseRowValues = new Dictionary<CastSizeOption, CastSizeRowValues>();
		private Project m_project;

		internal Project Project
		{
			get { return m_project; }
		}

		public int MaleNarrators
		{
			get
			{
				switch (NarratorOption)
				{
					case NarratorsOption.SingleNarrator: return 1;
					case NarratorsOption.NarrationByAuthor:
					case NarratorsOption.Custom: return m_customMaleNarratorCount;
					default: throw new InvalidOperationException("Attempt to get number of Male Narrators with NarratorOption not set to a valid value.");
				}
			}
			set
			{
				if (MaleNarrators == value)
					return;
				if (value < 0)
					throw new ArgumentOutOfRangeException("value", "MaleNarrators cannot be negative.");
				if (value > MaximumNarratorsValue)
					throw new ArgumentOutOfRangeException("value", "MaleNarrators cannot be set to a value greater than MaximumNarratorsValue.");
				if (NarratorOption == NarratorsOption.SingleNarrator)
					throw new InvalidOperationException("Number of male narrators cannot be set for Single narrator option.");
				m_customMaleNarratorCount = value;
				if (m_customMaleNarratorCount + m_customFemaleNarratorCount > MaximumNarratorsValue)
					FemaleNarrators = MaximumNarratorsValue - MaleNarrators;
				MaleNarratorsValueChanged?.Invoke(this, value);
				NotifyOfRowValueChangesBasedOnNarratorChanges();
			}
		}

		public int FemaleNarrators
		{
			get { return (NarratorOption == NarratorsOption.Custom) ? m_customFemaleNarratorCount : 0; }
			set
			{
				if (FemaleNarrators == value)
					return;
				if (value < 0)
					throw new ArgumentOutOfRangeException("value", "FemaleNarrators cannot be negative.");
				if (value > MaximumNarratorsValue)
					throw new ArgumentOutOfRangeException("value", "FemaleNarrators cannot be set to a value greater than MaximumNarratorsValue.");
				if (NarratorOption != NarratorsOption.Custom)
					throw new InvalidOperationException("Number of female narrators can only be set for the custom option.");
				m_customFemaleNarratorCount = value;
				if (m_customMaleNarratorCount + m_customFemaleNarratorCount > MaximumNarratorsValue)
					MaleNarrators = MaximumNarratorsValue - FemaleNarrators;
				FemaleNarratorsValueChanged?.Invoke(this, value);
				NotifyOfRowValueChangesBasedOnNarratorChanges();
			}
		}

		private void NotifyOfRowValueChangesBasedOnNarratorChanges()
		{
			if (CastSizeRowValuesChanged != null)
			{
				CastSizeRowValuesChanged(this, new CastSizeValueChangedEventArgs(CastSizeOption.Small, GetCastSizeRowValues(CastSizeOption.Small), true));
				CastSizeRowValuesChanged(this, new CastSizeValueChangedEventArgs(CastSizeOption.Recommended, GetCastSizeRowValues(CastSizeOption.Recommended), true));
				CastSizeRowValuesChanged(this, new CastSizeValueChangedEventArgs(CastSizeOption.Large, GetCastSizeRowValues(CastSizeOption.Large), true));
			}
		}

		internal int MinimumActorCount
		{
			get { return GetCastSizeRowValues(CastSizeOption.Small).Total; }
		}

		internal int RecommendedActorCount
		{
			get { return GetCastSizeRowValues(CastSizeOption.Recommended).Total; }
		}

		public CastSizePlanningViewModel(Project project)
		{
			m_project = project;
			var prefs = m_project.CharacterGroupGenerationPreferences;

			m_narratorsOption = prefs.NarratorsOption;
			// if not yet specified, use "Narration by Author"
			if (m_narratorsOption == NarratorsOption.NotSet)
				m_narratorsOption = NarratorsOption.NarrationByAuthor;

			if (m_narratorsOption == NarratorsOption.SingleNarrator)
			{
				m_customMaleNarratorCount = 0;
				m_customFemaleNarratorCount = 0;
			}
			else
			{
				m_customMaleNarratorCount = prefs.NumberOfMaleNarrators;
				m_customFemaleNarratorCount = m_narratorsOption == NarratorsOption.NarrationByAuthor ? 0 : prefs.NumberOfFemaleNarrators;
				m_project.EnsureNarratorPreferencesAreValid(m_narratorsOption,
					(v) => m_customMaleNarratorCount = v, (v) => m_customFemaleNarratorCount = v);
			}

			var smallCast = new CastSizeRowValues(2, 2, 0);
			int extraCharacterCount = 0;

			foreach (var bookId in m_project.IncludedBooks.Where(b => !b.SingleVoice).Select(b => b.BookId))
			{
				extraCharacterCount = 1;
				// OT numbers are based on actual generation from Kuna San Blas project
				// NT numbers are based on actual generation from Acholi project
				switch (bookId)
				{
					case "GEN":
						smallCast.Male = Math.Max(smallCast.Male, 9);
						smallCast.Female = 3;
						break;
					case "EXO":
						smallCast.Male = Math.Max(smallCast.Male, 6);
						smallCast.Female = 3;
						break;
					case "LEV":
						smallCast.Male = Math.Max(smallCast.Male, 3);
						break;
					case "NUM":
						smallCast.Male = Math.Max(smallCast.Male, 8);
						break;
					case "DEU":
						smallCast.Male = Math.Max(smallCast.Male, 2);
						break;
					case "JOS":
						smallCast.Male = Math.Max(smallCast.Male, 7);
						break;
					case "JDG":
						smallCast.Male = Math.Max(smallCast.Male, 8);
						break;
					case "RUT":
						smallCast.Male = Math.Max(smallCast.Male, 3);
						smallCast.Female = 4;
						break;
					case "1SA":
						smallCast.Male = Math.Max(smallCast.Male, 11);
						break;
					case "2SA":
						smallCast.Male = Math.Max(smallCast.Male, 12);
						break;
					case "1KI":
						smallCast.Male = Math.Max(smallCast.Male, 10);
						break;
					case "2KI":
						smallCast.Male = Math.Max(smallCast.Male, 9);
						break;
					case "1CH":
						smallCast.Male = Math.Max(smallCast.Male, 5);
						break;
					case "2CH":
						smallCast.Male = Math.Max(smallCast.Male, 9);
						break;
					case "EZR":
						smallCast.Male = Math.Max(smallCast.Male, 5);
						break;
					case "NEH":
						smallCast.Male = Math.Max(smallCast.Male, 7);
						break;
					case "EST":
						smallCast.Male = Math.Max(smallCast.Male, 6);
						break;
					case "JOB":
						smallCast.Male = Math.Max(smallCast.Male, 7);
						break;
					case "PSA":
						smallCast.Male = Math.Max(smallCast.Male, 3);
						break;
					case "PRO":
						smallCast.Male = Math.Max(smallCast.Male, 0);
						break;
					case "ECC":
						smallCast.Male = Math.Max(smallCast.Male, 1);
						break;
					case "SNG":
						smallCast.Male = Math.Max(smallCast.Male, 2);
						break;
					case "ISA":
						smallCast.Male = Math.Max(smallCast.Male, 6);
						break;
					case "JER":
						smallCast.Male = Math.Max(smallCast.Male, 9);
						break;
					case "LAM":
						smallCast.Male = Math.Max(smallCast.Male, 1);
						break;
					case "EZK":
						smallCast.Male = Math.Max(smallCast.Male, 4);
						break;
					case "DAN":
						smallCast.Male = Math.Max(smallCast.Male, 5);
						break;
					case "HOS":
						smallCast.Male = Math.Max(smallCast.Male, 2);
						break;
					case "JOL":
						smallCast.Male = Math.Max(smallCast.Male, 2);
						break;
					case "AMO":
						smallCast.Male = Math.Max(smallCast.Male, 4);
						break;
					case "OBA":
						smallCast.Male = Math.Max(smallCast.Male, 1);
						break;
					case "JON":
						smallCast.Male = Math.Max(smallCast.Male, 4);
						break;
					case "MIC":
						smallCast.Male = Math.Max(smallCast.Male, 3);
						break;
					case "NAM":
						smallCast.Male = Math.Max(smallCast.Male, 1);
						break;
					case "HAB":
						smallCast.Male = Math.Max(smallCast.Male, 1);
						break;
					case "ZEP":
						smallCast.Male = Math.Max(smallCast.Male, 1);
						break;
					case "HAG":
						smallCast.Male = Math.Max(smallCast.Male, 2);
						break;
					case "ZEC":
						smallCast.Male = Math.Max(smallCast.Male, 5);
						break;
					case "MAL":
						smallCast.Male = Math.Max(smallCast.Male, 1);
						break;

					//// New Testament
						
					case "MAT":
						smallCast.Male = Math.Max(smallCast.Male, 10);
						break;
					case "MRK":
						smallCast.Male = Math.Max(smallCast.Male, 12);
						break;
					case "LUK":
						smallCast.Male = Math.Max(smallCast.Male, 13);
						break;
					case "JHN":
						smallCast.Male = Math.Max(smallCast.Male, 10);
						break;
					case "ACT":
						smallCast.Male = Math.Max(smallCast.Male, 13);
						break;
					case "ROM":
						smallCast.Male = Math.Max(smallCast.Male, 2);
						break;
					case "1CO":
						smallCast.Male = Math.Max(smallCast.Male, 5);
						break;
					case "2CO":
						smallCast.Male = Math.Max(smallCast.Male, 3);
						break;
					case "GAL":
						smallCast.Male = Math.Max(smallCast.Male, 3);
						break;
					case "EPH":
						smallCast.Male = Math.Max(smallCast.Male, 0);
						break;
					case "PHP":
						smallCast.Male = Math.Max(smallCast.Male, 0);
						break;
					case "COL":
						smallCast.Male = Math.Max(smallCast.Male, 1);
						break;
					case "1TH":
						smallCast.Male = Math.Max(smallCast.Male, 0);
						break;
					case "2TH":
						smallCast.Male = Math.Max(smallCast.Male, 0);
						break;
					case "1TI":
						smallCast.Male = Math.Max(smallCast.Male, 0);
						break;
					case "2TI":
						smallCast.Male = Math.Max(smallCast.Male, 0);
						break;
					case "TIT":
						smallCast.Male = Math.Max(smallCast.Male, 1);
						break;
					case "PHM":
						smallCast.Male = Math.Max(smallCast.Male, 0);
						break;
					case "HEB":
						switch (m_project.DramatizationPreferences.ScriptureQuotationsShouldBeSpokenBy)
						{
							case DramatizationOption.DedicatedCharacter:
								smallCast.Male = Math.Max(smallCast.Male, 2);
								break;
							case DramatizationOption.DefaultCharacter:
								smallCast.Male = Math.Max(smallCast.Male, 4); // Not sure if 4 is correct
								break;
							case DramatizationOption.Narrator:
								smallCast.Male = Math.Max(smallCast.Male, 1);
								break;
						}
						break;
					case "JAS":
						smallCast.Male = Math.Max(smallCast.Male, 2);
						break;
					case "1PE":
						smallCast.Male = Math.Max(smallCast.Male, 0);
						break;
					case "2PE":
						smallCast.Male = Math.Max(smallCast.Male, 2);
						break;
					case "1JN":
						smallCast.Male = Math.Max(smallCast.Male, 0);
						break;
					case "2JN":
						smallCast.Male = Math.Max(smallCast.Male, 0);
						break;
					case "3JN":
						smallCast.Male = Math.Max(smallCast.Male, 0);
						break;
					case "JUD":
						smallCast.Male = Math.Max(smallCast.Male, 3);
						break;
					case "REV":
						smallCast.Male = Math.Max(smallCast.Male, 15);
						break;
				}
			}

			IReadOnlyDictionary<string, CharacterDetail> characterDetails = Project.AllCharacterDetailDictionary;
			int maleCharacterCount = 0;
			int femaleCharacterCount = 0;
			int childCharacterCount = 0;
			foreach (var character in Project.AllCharacterIds.Where(c => !CharacterVerseData.IsCharacterStandard(c)))
			{
				CharacterDetail characterInfo;
				try
				{
					characterInfo = characterDetails[character];
				}
				catch (KeyNotFoundException e)
				{
					throw new KeyNotFoundException(String.Format("Unable to find details for character {0}", character), e);
				}
				if (characterInfo.Age == CharacterAge.Child)
					childCharacterCount++;
				else
				{
					if (characterInfo.Gender == CharacterGender.Female || characterInfo.Gender == CharacterGender.PreferFemale)
						femaleCharacterCount++;
					else
						maleCharacterCount++;
				}
			}

			smallCast.Male = Math.Min(maleCharacterCount, smallCast.Male);
			smallCast.Male += extraCharacterCount;
			smallCast.Female = Math.Min(femaleCharacterCount, smallCast.Female);
			smallCast.Child = Math.Min(1, childCharacterCount);

			if (smallCast.Female == 0)
			{
				if ((m_project.DramatizationPreferences.BookIntroductionsDramatization == ExtraBiblicalMaterialSpeakerOption.FemaleActor) ||
				    (m_project.DramatizationPreferences.SectionHeadDramatization == ExtraBiblicalMaterialSpeakerOption.FemaleActor) ||
				    (m_project.DramatizationPreferences.BookTitleAndChapterDramatization == ExtraBiblicalMaterialSpeakerOption.FemaleActor))
				{
					smallCast.Female = 1;
				}
			}

			var largeCast = new CastSizeRowValues(smallCast.Male * 2, Math.Min(8, femaleCharacterCount), Math.Min(4, childCharacterCount));
			if (largeCast.Male > 10)
				largeCast.Male = Math.Min(39, largeCast.Male);
			largeCast.Male = Math.Min(maleCharacterCount + extraCharacterCount, largeCast.Male);

			m_baseRowValues[CastSizeOption.Small] = smallCast;
			m_baseRowValues[CastSizeOption.Recommended] = new CastSizeRowValues(
				(int)Math.Ceiling((double)(smallCast.Male + largeCast.Male) / 2),
				(int)Math.Ceiling((double)(smallCast.Female + largeCast.Female) / 2),
				(int)Math.Ceiling((double)(smallCast.Child + largeCast.Child) / 2));
			m_baseRowValues[CastSizeOption.Large] = largeCast;

			if (prefs.NumberOfMaleActors == 0 && prefs.NumberOfFemaleActors == 0 && prefs.NumberOfChildActors == 0)
				m_updatedCustomActorCounts = GetCastSizeRowValues(CastSizeOption.Recommended);
			m_updatedCustomActorCounts = new CastSizeRowValues(prefs.NumberOfMaleActors, prefs.NumberOfFemaleActors, prefs.NumberOfChildActors);
		}

		public CastSizeRowValues SelectedCastSize
		{
			get { return GetCastSizeRowValues(CastSizeOption); }
		}

		internal CastSizeRowValues GetCastSizeRowValues(CastSizeOption row)
		{
			//if (Project.CalculatedCastSizeRowValues == null)
			//	Project.CalculateCastSizeRowValues();

			switch (row)
			{
				case CastSizeOption.Small:
				case CastSizeOption.Recommended:
				case CastSizeOption.Large:
					var baseRowValues = m_baseRowValues[row];
					return new CastSizeRowValues(Math.Max(MaleNarrators + 1, baseRowValues.Male + Math.Min(MaleNarrators, 1)), baseRowValues.Female + FemaleNarrators, baseRowValues.Child);

				case CastSizeOption.MatchVoiceActorList:
					if (m_actualActorCounts == null)
						m_actualActorCounts = new CastSizeRowValues(Project.VoiceActorList);

					return m_actualActorCounts;

				case CastSizeOption.Custom:
					return m_updatedCustomActorCounts;
			}

			throw new InvalidEnumArgumentException(@"row", (int)row, typeof(CastSizeOption));
		}

		internal int MaximumNarratorsValue => m_narratorsOption == NarratorsOption.Custom ? Project.IncludedBooks.Count : Project.AuthorCount;

		internal NarratorsOption NarratorOption
		{
			get { return m_narratorsOption; }
			set
			{
				if (m_narratorsOption == value)
					return;

				int prevMaleNarratorCount = MaleNarrators;
				int prevFemaleNarratorCount = FemaleNarrators;

				m_narratorsOption = value;
				if (m_narratorsOption == NarratorsOption.NarrationByAuthor)
					MaleNarrators = Project.DefaultNarratorCountForNarrationByAuthor;

				if (prevMaleNarratorCount != MaleNarrators && MaleNarratorsValueChanged != null)
					MaleNarratorsValueChanged(this, MaleNarrators);
				if (prevFemaleNarratorCount != FemaleNarrators && FemaleNarratorsValueChanged != null)
					FemaleNarratorsValueChanged(this, FemaleNarrators);
				NotifyOfRowValueChangesBasedOnNarratorChanges();
			}
		}

		internal CastSizeOption CastSizeOption
		{
			get
			{
				// did the option change since the dialog opened
				if (m_updatedCastSizeOption != CastSizeOption.NotSet)
					return m_updatedCastSizeOption;

				// if it hasn't changed, return the saved value
				var currentValue = Project.CharacterGroupGenerationPreferences.CastSizeOption;

				// if not yet specified, use "Recommended"
				return m_updatedCastSizeOption = currentValue == CastSizeOption.NotSet ? CastSizeOption.Recommended : currentValue;
			}
			set { m_updatedCastSizeOption = value; }
		}

		internal bool HasVoiceActors
		{
			get { return Project.VoiceActorList.AllActors.Any(); }
		}

		internal int VoiceActorCount
		{
			get { return Project.VoiceActorList.AllActors.Count; }
		}

		internal void SetCustomVoiceActorValues(CastSizeRowValues values)
		{
			m_updatedCustomActorCounts = new CastSizeRowValues(values);

			if (CastSizeRowValuesChanged != null)
				CastSizeRowValuesChanged(this, new CastSizeValueChangedEventArgs(CastSizeOption.Custom, values, false));
		}

		internal void SetVoiceActorListValues(CastSizeRowValues values, bool keepSelection)
		{
			m_actualActorCounts = new CastSizeRowValues(values);

			if (CastSizeRowValuesChanged != null)
				CastSizeRowValuesChanged(this, new CastSizeValueChangedEventArgs(CastSizeOption.MatchVoiceActorList, values, keepSelection));
		}

		internal void Save()
		{
			var prefs = Project.CharacterGroupGenerationPreferences;
			prefs.IsSetByUser = true;

			prefs.NumberOfMaleNarrators = MaleNarrators;
			prefs.NumberOfFemaleNarrators = FemaleNarrators;
			prefs.NarratorsOption = NarratorOption;
			prefs.CastSizeOption = CastSizeOption;
			prefs.NumberOfMaleActors = m_updatedCustomActorCounts.Male;
			prefs.NumberOfFemaleActors = m_updatedCustomActorCounts.Female;
			prefs.NumberOfChildActors = m_updatedCustomActorCounts.Child;

			Project.Save();
		}
	}
	
	public class CastSizeRowValues
	{
		public int Male { get; set; }
		public int Female { get; set; }
		public int Child { get; set; }

		public int Total
		{
			get { return Male + Female + Child; }
		}

		public CastSizeRowValues(int male, int female, int child)
		{
			Male = male;
			Female = female;
			Child = child;
		}

		public CastSizeRowValues(CastSizeRowValues copyFrom)
		{
			Male = copyFrom.Male;
			Female = copyFrom.Female;
			Child = copyFrom.Child;
		}

		public CastSizeRowValues(VoiceActorList actualActorList)
		{
			Male = actualActorList.ActiveMaleAdultActorCount;
			Female = actualActorList.ActiveFemaleAdultActorCount;
			Child = actualActorList.ActiveChildActorCount;
		}
	}

	public class CastSizeValueChangedEventArgs : EventArgs
	{
		public CastSizeOption Row { get; set; }
		public CastSizeRowValues RowValues { get; private set; }
		public bool KeepSelection { get; private set; }

		public CastSizeValueChangedEventArgs(CastSizeOption row, CastSizeRowValues values, bool keepSelection)
		{
			Row = row;
			RowValues = new CastSizeRowValues(values);
			KeepSelection = keepSelection;
		}
	}

	public class CastSizeOptionChangedEventArgs : EventArgs
	{
		public CastSizeOption Row { get; set; }

		public CastSizeOptionChangedEventArgs(CastSizeOption row)
		{
			Row = row;
		}
	}
}
