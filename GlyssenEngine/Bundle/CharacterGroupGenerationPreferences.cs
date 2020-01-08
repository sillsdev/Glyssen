using System;
using System.ComponentModel;
using System.Xml.Serialization;
using GlyssenEngine.Character;

namespace GlyssenEngine.Bundle
{
	public class CharacterGroupGenerationPreferences
	{
		[XmlElement("isSetByUser")]
		[DefaultValue(false)]
		public bool IsSetByUser { get; set; }

		/// <summary>
		/// Number of male narrators set by user
		/// </summary>
		[XmlElement("numberOfMaleNarrators")]
		[DefaultValue(0)]
		public int NumberOfMaleNarrators { get; set; }

		/// <summary>
		/// Number of female narrators set by user
		/// </summary>
		[XmlElement("numberOfFemaleNarrators")]
		[DefaultValue(0)]
		public int NumberOfFemaleNarrators { get; set; }

		/// <summary>
		/// How the number of narrators was set:
		/// </summary>
		[XmlElement("narratorsOption")]
		[DefaultValue(NarratorsOption.NotSet)]
		public NarratorsOption NarratorsOption { get; set; }

		/// <summary>
		/// How the number of voice actors was set:
		/// </summary>
		[XmlElement("castSizeOption")]
		[DefaultValue(CastSizeOption.NotSet)]
		public CastSizeOption CastSizeOption { get; set; }

		/// <summary>
		/// Number of male actors set by user
		/// </summary>
		[XmlElement("numberOfMaleActors")]
		[DefaultValue(0)]
		public int NumberOfMaleActors { get; set; }

		/// <summary>
		/// Number of female actors set by user
		/// </summary>
		[XmlElement("numberOfFemaleActors")]
		[DefaultValue(0)]
		public int NumberOfFemaleActors { get; set; }

		/// <summary>
		/// Number of child actors set by user
		/// </summary>
		[XmlElement("numberOfChildActors")]
		[DefaultValue(0)]
		public int NumberOfChildActors { get; set; }
	}

	public class ProjectDramatizationPreferences
	{
		public ProjectDramatizationPreferences()
		{
			BookTitleAndChapterDramatization = ExtraBiblicalMaterialSpeakerOption.Narrator;
			SectionHeadDramatization = ExtraBiblicalMaterialSpeakerOption.Omitted;
			BookIntroductionsDramatization = ExtraBiblicalMaterialSpeakerOption.Omitted;
		}

		/// <summary>
		/// Gets or sets a value indicating how to dramatize "scripture" quotations
		/// </summary>
		[XmlElement("ScriptureQuotationsShouldBeSpokenBy")]
		[DefaultValue(DramatizationOption.DedicatedCharacter)]
		public DramatizationOption ScriptureQuotationsShouldBeSpokenBy { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether narrators should be allowed to speak the lines of biblical characters who have roles in other books
		/// </summary>
		[XmlElement("HaveNarratorsPerformCharacterRolesInOtherBooks")]
		[DefaultValue(PreferenceLevel.NoOpinion)]
		public PreferenceLevel HaveNarratorsPerformCharacterRolesInOtherBooks { get; set; }

		[XmlElement("BookTitleAndChapterDramatization")]
		[DefaultValue(ExtraBiblicalMaterialSpeakerOption.Narrator)]
		public ExtraBiblicalMaterialSpeakerOption BookTitleAndChapterDramatization { get; set; }

		[XmlElement("SectionHeadDramatization")]
		[DefaultValue(ExtraBiblicalMaterialSpeakerOption.Omitted)]
		public ExtraBiblicalMaterialSpeakerOption SectionHeadDramatization { get; set; }

		[XmlElement("BookIntroductionsDramatization")]
		[DefaultValue(ExtraBiblicalMaterialSpeakerOption.Omitted)]
		public ExtraBiblicalMaterialSpeakerOption BookIntroductionsDramatization { get; set; }

		public bool IncludeCharacter(string characterId)
		{
			switch (CharacterVerseData.GetStandardCharacterType(characterId))
			{
				case CharacterVerseData.StandardCharacter.BookOrChapter:
					if (BookTitleAndChapterDramatization == ExtraBiblicalMaterialSpeakerOption.Omitted)
						return false;
					break;
				case CharacterVerseData.StandardCharacter.Intro:
					if (BookIntroductionsDramatization == ExtraBiblicalMaterialSpeakerOption.Omitted)
						return false;
					break;
				case CharacterVerseData.StandardCharacter.ExtraBiblical:
					if (SectionHeadDramatization == ExtraBiblicalMaterialSpeakerOption.Omitted)
						return false;
					break;
			}
			return true;
		}
	}

	public enum NarratorsOption
    {
        NotSet = 0,
        SingleNarrator = 1,
        NarrationByAuthor = 2,
        Custom = 3
    }

    public enum CastSizeOption
    {
        NotSet = 0,
        Small = 1,
        Recommended = 2,
        Large = 3,
        Custom = 4,
        MatchVoiceActorList = 5
    }

    public enum DramatizationOption
    {
        DedicatedCharacter,
        DefaultCharacter,
        Narrator,
    }

    public enum PreferenceLevel
    {
        /// <summary>
        /// This is the default, indicating a behavior that is "Permissible" (but the user has not indicated a specific preference)
        /// </summary>
        NoOpinion,
        Prevent,
        Undesirable,
        Preferable,
        Required,
    }

    [Flags]
    public enum ExtraBiblicalMaterialSpeakerOption
    {
        Narrator = 1 << 0,
        ActorOfEitherGender = 1 << 1,
        MaleActor = 1 << 2,
        FemaleActor = 1 << 3,
        Omitted = 1 << 4,
        NotNarratorOrOmitted = MaleActor | FemaleActor | ActorOfEitherGender
    }

    [Flags]
    public enum QuoteSystemStatus
    {
        Unknown = 1,
        Guessed = 2,
        Reviewed = 4,
        Obtained = 8,
        UserSet = 16,
        NotParseReady = Unknown | Guessed,
        ParseReady = Reviewed | Obtained | UserSet
    }

    public enum BookSelectionStatus
    {
        UnReviewed,
        Reviewed
    }

    public enum ProjectSettingsStatus
    {
        UnReviewed,
        Reviewed
    }

    public enum ChapterAnnouncement
    {
        // These "unused" items are used dynamically. The number of items before ChapterLabel must match the
        // number (and order) of items in ProjectSettingsDlg.m_cboBookMarker.Items.
        PageHeader,
        MainTitle1,
        ShortNameFromMetadata,
        LongNameFromMetadata,
        ChapterLabel, // Keep this one last for easier logic in ProjectSettingsDlg
    }
}
