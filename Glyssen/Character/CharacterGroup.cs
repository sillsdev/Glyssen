using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using Glyssen.Rules;
using Glyssen.Shared;
using GlyssenEngine.Character;
using GlyssenEngine.Rules;
using GlyssenEngine.VoiceActor;
using L10NSharp;
using SIL;
using SIL.Reporting;

namespace Glyssen.Character
{
	public class CharacterGroup
	{
		public enum Label
		{
			None,
			Male,
			Female,
			Child,
			Narrator,
			Other
		}

		public const int kNoActorAssigned = -1;
		private Project m_project;
		private bool m_closed;
		private double m_estimatedHours = -1;

		//For Deserialization
		internal CharacterGroup()
		{
			CharacterIds = new CharacterIdHashSet();
			VoiceActorId = kNoActorAssigned;
		}

		public CharacterGroup(Project project) : this()
		{
			Initialize(project);
		}

		public CharacterGroup Copy()
		{
			var copy = (CharacterGroup)MemberwiseClone();
			copy.CharacterIds = new CharacterIdHashSet(CharacterIds);
			return copy;
		}

		public void Initialize(Project project)
		{
			m_project = project;
			// This shouldn't be necessary, but if a prior crash (or someone mucking with the project files) has left a
			// character assigned to a non-existent voice actor, we don't want the project permanently hamstrung, so we'll
			// just report it non-fatally and clear the actor info.
			if (VoiceActorId >= 0 && VoiceActor == null)
			{
				ErrorReport.NotifyUserOfProblem(Localizer.GetString("CharacterGroup.InvalidActorId",
					"Character group {0} is assigned to a voice actor who is no longer part of this project. " +
					"This might have been caused by a previous failure. {1} will clear this assignment for you now."),
					GroupIdForUiDisplay, GlyssenInfo.kProduct);
				RemoveVoiceActor();
			}
			CharacterIds.PriorityComparer = new CharacterByKeyStrokeComparer(characterId => m_project.KeyStrokesByCharacterId[characterId]);
		}

		public void AssignVoiceActor(int actorId)
		{
			VoiceActorId = actorId;
		}

		public void RemoveVoiceActor()
		{
			VoiceActorId = kNoActorAssigned;
		}

		[XmlElement]
		public Label GroupIdLabel { get; set; }

		[XmlElement]
		[DefaultValue(0)]
		public int GroupIdNumber { get; set; }

		[XmlElement]
		[DefaultValue(null)]
		public string GroupIdOtherText { get; set; }

		[XmlIgnore]
		public string GroupIdForUiDisplay
		{
			get
			{
				switch (GroupIdLabel)
				{
					case Label.Male:
						return string.Format(Localizer.GetString("CharacterGroup.MaleGroupId", "Man {0}"), GroupIdNumber);
					case Label.Female:
						return string.Format(Localizer.GetString("CharacterGroup.FemaleGroupId", "Woman {0}"), GroupIdNumber);
					case Label.Child:
						return string.Format(Localizer.GetString("CharacterGroup.ChildGroupId", "Child {0}"), GroupIdNumber);
					case Label.Narrator:
						return string.Format(Localizer.GetString("CharacterGroup.NarratorGroupId", "Narrator {0}"), GroupIdNumber);
					case Label.Other:
						return GroupIdOtherText;
					default:
						return string.Empty;
				}
			}
		}

		[XmlIgnore]
		public string GroupId
		{
			get
			{
				if (GroupIdLabel == Label.Other)
					return GroupIdOtherText;
				return GroupIdLabel.ToString() + GroupIdNumber;
			}
		}

		[XmlArray("CharacterIds")]
		[XmlArrayItem("CharacterId")]
		public CharacterIdHashSet CharacterIds { get; set; }

		[XmlIgnore]
		public string AttributesDisplay
		{
			get
			{
				var genderAttributes = new CharacterGroupAttributeSet<CharacterGender>(CharacterGenderComparer.Singleton);
				var ageAttributes = new CharacterGroupAttributeSet<CharacterAge>(CharacterAgeComparer.Singleton);
				var characterDetails = m_project.AllCharacterDetailDictionary;
				foreach (var characterId in CharacterIds)
				{
					genderAttributes.Add(characterDetails[characterId].Gender);
					ageAttributes.Add(characterDetails[characterId].Age);
				}

				var genderString = string.Join("; ", genderAttributes.Select(a => a.ToString()).Where(a => !string.IsNullOrEmpty(a)));
				var ageString = string.Join("; ", ageAttributes.Select(a => a.ToString()).Where(a => !string.IsNullOrEmpty(a)));
				if (genderString.Length == 0)
					return ageString;
				if (ageString.Length == 0)
					return genderString;
				return genderString + "; " + ageString;
			}
		}

		[XmlElement]
		[Browsable(false)]
		public bool Status { get; set; }

		public string StatusDisplay
		{
			get { return Status ? "Y" : ""; }
		}

		public double EstimatedHours
		{
			get
			{
				if (m_estimatedHours < 0)
				{
					int keyStrokes = 0;
					foreach (var characterId in CharacterIds)
					{
						int keystrokesForCharacter;
						if (m_project.KeyStrokesByCharacterId.TryGetValue(characterId, out keystrokesForCharacter))
							keyStrokes += keystrokesForCharacter;
						else
						{
							throw new InvalidOperationException("Character " + characterId + " is not in use the project.");
						}
					}
					m_estimatedHours = keyStrokes / Project.kKeyStrokesPerHour;
				}
				return m_estimatedHours;
			}
		}

		[Browsable(false)]
		public bool IsVoiceActorAssigned
		{
			get { return VoiceActorId >= 0; }
		}

		[Browsable(false)]
		public bool AssignedToCameoActor
		{
			get { return IsVoiceActorAssigned && m_project.VoiceActorList.GetVoiceActorById(VoiceActorId).IsCameo; }
		}

		[XmlElement("VoiceActorAssignedId")]
		[DefaultValue(-1)]
		public int VoiceActorId { get; set; }

		[Browsable(false)]
		public bool Closed
		{
			get { return m_closed; }
			set
			{
				m_closed = value;
				CharacterIds.IsReadOnly = value;
			}
		}

		public VoiceActor VoiceActor
		{
			get
			{
				return IsVoiceActorAssigned ? m_project.VoiceActorList.GetVoiceActorById(VoiceActorId) : null;
			}
		}

		/// <summary>
		/// We used to use this in character group generation. We no longer use it, but since there are a lot of tests for it and it might
		/// be useful in the future, we'll keep it around for now.
		/// </summary>
		public bool Matches(CharacterDetail character, CharacterGenderMatchingOptions genderMatchingOptions, CharacterAgeMatchingOptions ageMatchingOptions)
		{
			if (!CharacterIds.Any())
				return false; // Probably a group set aside for a special purpose in the group generator (e.g., narrator)

			if (CharacterVerseData.IsCharacterStandard(character.CharacterId))
			{
				switch (CharacterVerseData.GetStandardCharacterType(character.CharacterId))
				{
					case CharacterVerseData.StandardCharacter.Narrator:
						return CharacterIds.All(i => CharacterVerseData.GetStandardCharacterType(i) == CharacterVerseData.StandardCharacter.Narrator);
					default:
						return CharacterIds.All(i =>
						{
							var type = CharacterVerseData.GetStandardCharacterType(i);
							return type != CharacterVerseData.StandardCharacter.Narrator && type != CharacterVerseData.StandardCharacter.NonStandard;
						});
				}
			}
			if (CharacterIds.Any(i => CharacterVerseData.IsCharacterStandard(i)))
				return false;

			bool result = true;

			var characterDetails = m_project.AllCharacterDetailDictionary;
			result &= CharacterIds.All(i =>
			{
				CharacterGender gender = characterDetails[i].Gender;
				return genderMatchingOptions.Matches(character.Gender, gender);
			});

			result &= CharacterIds.All(i =>
			{
				CharacterAge age = characterDetails[i].Age;
				return ageMatchingOptions.Matches(character.Age, age);
			});

			return result;
		}

		public bool ContainsCharacterWithGender(CharacterGender gender)
		{
			return CharactersWithGender(gender).Any();
		}

		public IEnumerable<CharacterDetail> CharactersWithGender(params CharacterGender[] genders)
		{
			var characterDetails = m_project.AllCharacterDetailDictionary;
			foreach (var c in CharacterIds)
			{
				CharacterDetail characterDetail;
				if (characterDetails.TryGetValue(c, out characterDetail) && genders.Contains(characterDetail.Gender))
					yield return characterDetail;
			}
		}

		public bool ContainsCharacterWithAge(CharacterAge age)
		{
			var characterDetails = m_project.AllCharacterDetailDictionary;
			return CharacterIds.Any(c =>
			{
				if (CharacterVerseData.IsCharacterStandard(c))
					return age == CharacterAge.Adult;

				CharacterDetail characterDetail;
				if (!characterDetails.TryGetValue(c, out characterDetail))
					return false;
				return characterDetail.Age == age;
			});
		}

		public bool ContainsOnlyCharactersWithAge(CharacterAge age)
		{
			var characterDetails = m_project.AllCharacterDetailDictionary;
			return CharacterIds.All(c =>
			{
				if (CharacterVerseData.IsCharacterStandard(c))
					return age == CharacterAge.Adult;

				CharacterDetail characterDetail;
				if (!characterDetails.TryGetValue(c, out characterDetail))
					return false;
				return characterDetail.Age == age;
			});
		}

		public void SetGroupIdLabel()
		{
			if (GroupIdLabel != Label.None)
				return;

			if (AssignedToCameoActor)
			{
				GroupIdLabel = Label.Other;
				GroupIdOtherText = VoiceActor.Name;
				return;
			}

			if (CharacterIds.All(c => CharacterVerseData.IsCharacterOfType(c, CharacterVerseData.StandardCharacter.Narrator)))
				GroupIdLabel = Label.Narrator;
			else if (IsVoiceActorAssigned)
			{
				VoiceActor actor = VoiceActor;
				if (actor.Age == ActorAge.Child)
					GroupIdLabel = Label.Child;
				else if (actor.Gender == ActorGender.Male)
					GroupIdLabel = Label.Male;
				else
					GroupIdLabel = Label.Female;
			}
			else
			{
				if (ContainsOnlyCharactersWithAge(CharacterAge.Child))
					GroupIdLabel = Label.Child;
				else if (ContainsCharacterWithGender(CharacterGender.Male))
					GroupIdLabel = Label.Male;
				else if (ContainsCharacterWithGender(CharacterGender.Female))
					GroupIdLabel = Label.Female;
				else if (ContainsCharacterWithGender(CharacterGender.PreferMale))
					GroupIdLabel = Label.Male;
				else if (ContainsCharacterWithGender(CharacterGender.PreferFemale))
					GroupIdLabel = Label.Female;
				else
					GroupIdLabel = Label.Male;
			}
		}

		public void ClearCacheOfEstimatedHours()
		{
			m_estimatedHours = -1;
		}

		public override string ToString()
		{
			return GroupIdForUiDisplay;
		}
	}
}
