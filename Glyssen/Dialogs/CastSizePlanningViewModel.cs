using System;
using System.ComponentModel;
using System.Linq;
using Glyssen.VoiceActor;

namespace Glyssen.Dialogs
{
	public class CastSizePlanningViewModel
	{
		public event EventHandler<CastSizeValueChangedEventArgs> CastSizeRowValuesChanged;

		private const int Unchanged = -999;
		private bool m_somethingChanged;
		private int m_updatedMaleNarrators = Unchanged;
		private int m_updatedFemaleNarrators = Unchanged;
		private NarratorsOption m_updatedNarratorsOption = NarratorsOption.NotSet;
		private CastSizeRow m_updatedCastSizeOption = CastSizeRow.NotSet;

		// values for custom settings
		private int m_updatedCustomMaleActors = Unchanged;
		private int m_updatedCustomFemaleActors = Unchanged;
		private int m_updatedCustomChildActors = Unchanged;

		// values from Voice Actor List
		private int m_actualMaleActorCount = Unchanged;
		private int m_actualFemaleActorCount = Unchanged;
		private int m_actualChildActorCount = Unchanged;

		internal Project Project { get; set; }

		internal int MinimumActorCount
		{
			get { return GetCastSizeRowValues(CastSizeRow.Small).Total; }
		}

		internal int RecommendedActorCount
		{
			get { return GetCastSizeRowValues(CastSizeRow.Recommended).Total; }
		}

		public CastSizePlanningViewModel(Project project)
		{
			Project = project;
		}

		internal CastSizeRowValues GetCastSizeRowValues(CastSizeRow row)
		{
			switch (row)
			{
				case CastSizeRow.Small:
					return new CastSizeRowValues(12, 2, 0);

				case CastSizeRow.Recommended:
					return new CastSizeRowValues(20, 2, 1);

				case CastSizeRow.Large:
					return new CastSizeRowValues(26, 3, 1);

				case CastSizeRow.MatchVoiceActorList:
					if (m_actualMaleActorCount == Unchanged)
					{
						m_actualMaleActorCount = Project.VoiceActorList.ActiveActors.Count(a => a.Gender == ActorGender.Male && a.Age != ActorAge.Child);
						m_actualFemaleActorCount = Project.VoiceActorList.ActiveActors.Count(a => a.Gender == ActorGender.Female && a.Age != ActorAge.Child);
						m_actualChildActorCount = Project.VoiceActorList.ActiveActors.Count(a => a.Age == ActorAge.Child);
					}

					return new CastSizeRowValues(m_actualMaleActorCount, m_actualFemaleActorCount, m_actualChildActorCount);

				case CastSizeRow.Custom:
					var prefs = Project.CharacterGroupGenerationPreferences;

					var male = (m_updatedCustomMaleActors == Unchanged) ? prefs.NumberOfMaleActors : m_updatedCustomMaleActors;
					var female = (m_updatedCustomFemaleActors == Unchanged) ? prefs.NumberOfFemaleActors : m_updatedCustomFemaleActors;
					var child = (m_updatedCustomChildActors == Unchanged) ? prefs.NumberOfChildActors : m_updatedCustomChildActors;

					return new CastSizeRowValues(male, female, child);
			}

			throw new InvalidEnumArgumentException(@"row", (int)row, typeof(CastSizeRow));
		}

		/// <summary></summary>
		/// <param name="option"></param>
		/// <returns>The first value is the number of males, the second is the number of females.</returns>
		internal Tuple<int, int> GetNarratorValues(NarratorsOption option)
		{
			switch (option)
			{
				case NarratorsOption.SingleNarrator:
					return new Tuple<int, int>(1, 0);

				case NarratorsOption.NarrationByAuthor:
					return new Tuple<int, int>(BiblicalAuthors.GetAuthorCount(Project.IncludedBooks.Select(b => b.BookId)), 0);

				default:
					return new Tuple<int, int>(
						Project.CharacterGroupGenerationPreferences.NumberOfMaleNarrators,
						Project.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators
					);
			}
		}

		internal void SetNarratorValues(int male, int female)
		{
			m_updatedMaleNarrators = male;
			m_updatedFemaleNarrators = female;
		}

		internal int MaximumNarratorsValue
		{
			get { return Project.IncludedBooks.Count; }
		}

		internal NarratorsOption NarratorOption
		{
			get
			{
				// did the option change since the dialog opened
				if (m_updatedNarratorsOption != NarratorsOption.NotSet)
					return m_updatedNarratorsOption;

				// if it hasn't changed, return the saved value
				var currentValue = Project.CharacterGroupGenerationPreferences.NarratorsOption;

				// if not yet specified, use "Narration by Author"
				return currentValue == NarratorsOption.NotSet ? NarratorsOption.NarrationByAuthor : currentValue;
			}
			set { m_updatedNarratorsOption = value; }
		}

		internal CastSizeRow CastSizeOption
		{
			get
			{
				// did the option change since the dialog opened
				if (m_updatedCastSizeOption != CastSizeRow.NotSet)
					return m_updatedCastSizeOption;

				// if it hasn't changed, return the saved value
				var currentValue = Project.CharacterGroupGenerationPreferences.CastSizeOption;

				// if not yet specified, use "Recommended"
				return m_updatedCastSizeOption = currentValue == CastSizeRow.NotSet ? CastSizeRow.Recommended : currentValue;
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

		internal void SetCustomVoiceActorValues(int male, int female, int child)
		{
			m_updatedCustomMaleActors = male;
			m_updatedCustomFemaleActors = female;
			m_updatedCustomChildActors = child;

			if (CastSizeRowValuesChanged != null)
				CastSizeRowValuesChanged(this, new CastSizeValueChangedEventArgs(CastSizeRow.Custom, male, female, child));
		}

		internal void SetVoiceActorListValues(int male, int female, int child)
		{
			m_actualMaleActorCount = male;
			m_actualFemaleActorCount = female;
			m_actualChildActorCount = child;

			if (CastSizeRowValuesChanged != null)
				CastSizeRowValuesChanged(this, new CastSizeValueChangedEventArgs(CastSizeRow.MatchVoiceActorList, male, female, child));
		}

		private bool ValueChanged(int newValue, int oldValue)
		{
			if ((newValue == Unchanged) || (newValue == oldValue))
				return false;

			m_somethingChanged = true;
			return true;
		}

		internal void Save()
		{
			m_somethingChanged = false;
			var prefs = Project.CharacterGroupGenerationPreferences;
			prefs.IsSetByUser = true;

			// find values that have changed
			if (ValueChanged(m_updatedMaleNarrators, prefs.NumberOfMaleNarrators))
				prefs.NumberOfMaleNarrators = m_updatedMaleNarrators;

			if (ValueChanged(m_updatedFemaleNarrators, prefs.NumberOfFemaleNarrators))
				prefs.NumberOfFemaleNarrators = m_updatedFemaleNarrators;

			if (ValueChanged((int)m_updatedNarratorsOption, (int)prefs.NarratorsOption))
				prefs.NarratorsOption = m_updatedNarratorsOption;

			if (ValueChanged((int)CastSizeOption, (int)prefs.CastSizeOption))
				prefs.CastSizeOption = CastSizeOption;

			if (ValueChanged(m_updatedCustomMaleActors, prefs.NumberOfMaleActors))
				prefs.NumberOfMaleActors = m_updatedCustomMaleActors;

			if (ValueChanged(m_updatedCustomFemaleActors, prefs.NumberOfFemaleActors))
				prefs.NumberOfFemaleActors = m_updatedCustomFemaleActors;

			if (ValueChanged(m_updatedCustomChildActors, prefs.NumberOfChildActors))
				prefs.NumberOfChildActors = m_updatedCustomChildActors;

			// if something changed, save it now
			if (m_somethingChanged)
			{
				Project.Save();
				m_somethingChanged = false;
			}
		}
	}

	public enum CastSizeRow
	{
		NotSet = 0,
		Small = 1,
		Recommended = 2,
		Large = 3,
		Custom = 4,
		MatchVoiceActorList = 5
	}

	public enum NarratorsOption
	{
		NotSet = 0,
		SingleNarrator = 1,
		NarrationByAuthor = 2,
		Custom = 3
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
	}

	public class CastSizeValueChangedEventArgs : EventArgs
	{
		public CastSizeRow Row;
		public int Male;
		public int Female;
		public int Child;

		public CastSizeRowValues RowValues
		{
			get { return new CastSizeRowValues(Male, Female, Child); }
		}

		public int Count
		{
			get { return Male + Female + Child; }
		}

		public CastSizeValueChangedEventArgs(CastSizeRow row, int male, int female, int child)
		{
			Row = row;
			Male = male;
			Female = female;
			Child = child;
		}
	}

	public class CastSizeOptionChangedEventArgs : EventArgs
	{
		public CastSizeRow Row;

		public CastSizeOptionChangedEventArgs(CastSizeRow row)
		{
			Row = row;
		}
	}
}
