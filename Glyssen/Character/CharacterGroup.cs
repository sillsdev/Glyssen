using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Glyssen.Rules;
using Glyssen.VoiceActor;
using L10NSharp;

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
			// just clear the actor info.
			if (VoiceActorId >= 0 && VoiceActor == null)
			{
				// At one time, we reported this; but there is really no reason to.
				// The user can't do anything about it. And if there was a crash or
				// they mucked with project files, it isn't totally unexpected to have
				// the assignment cleared.
				//ErrorReport.NotifyUserOfProblem(LocalizationManager.GetString("CharacterGroup.InvalidActorId",
				//	"Character group {0} is assigned to a voice actor who is no longer part of this project. " +
				//	"This might have been caused by a previous failure. {1} will clear this assignment for you now."),
				//	GroupIdForUiDisplay, GlyssenInfo.kProduct);
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
						return string.Format(LocalizationManager.GetString("CharacterGroup.MaleGroupId", "Man {0}"), GroupIdNumber);
					case Label.Female:
						return string.Format(LocalizationManager.GetString("CharacterGroup.FemaleGroupId", "Woman {0}"), GroupIdNumber);
					case Label.Child:
						return string.Format(LocalizationManager.GetString("CharacterGroup.ChildGroupId", "Child {0}"), GroupIdNumber);
					case Label.Narrator:
						return string.Format(LocalizationManager.GetString("CharacterGroup.NarratorGroupId", "Narrator {0}"), GroupIdNumber);
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

		public VoiceActor.VoiceActor VoiceActor
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
				VoiceActor.VoiceActor actor = VoiceActor;
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

	#region CharacterGroupAttribute Definition

	public class CharacterGroupAttributeSet<T> : SortedSet<CharacterGroupAttribute<T>> where T: struct, IConvertible
	{
		private readonly Dictionary<T, CharacterGroupAttribute<T>> m_entryNameToDataEntry;

		private class CharacterAttributeComparer<T2> : IComparer<CharacterGroupAttribute<T2>> where T2 : struct, IConvertible
		{
			private readonly IComparer<T2> m_comparer;

			public CharacterAttributeComparer(IComparer<T2> comparer)
			{
				m_comparer = comparer;
			}

			public int Compare(CharacterGroupAttribute<T2> x, CharacterGroupAttribute<T2> y)
			{
				return m_comparer.Compare(x.AttributeValue, y.AttributeValue);
			}
		}

		public CharacterGroupAttributeSet(IComparer<T> comparer) : base(new CharacterAttributeComparer<T>(comparer))
		{
			m_entryNameToDataEntry = new Dictionary<T, CharacterGroupAttribute<T>>();
		}

		public void Add(T attributeValue)
		{
			if (!m_entryNameToDataEntry.ContainsKey(attributeValue))
			{
				var newEntry = new CharacterGroupAttribute<T>(attributeValue);
				Add(newEntry);
				m_entryNameToDataEntry.Add(attributeValue, newEntry);
			}

			m_entryNameToDataEntry[attributeValue].Count++;
		}

		public new void Clear()
		{
			base.Clear();
			m_entryNameToDataEntry.Clear();
		}
	}

	public class CharacterGroupAttribute<T> where T: struct, IConvertible
	{
		public static Func<T, string> GetUiStringForValue { get; set; }

		static CharacterGroupAttribute()
		{
			GetUiStringForValue = a => a.ToString(CultureInfo.InvariantCulture);
		}

		public CharacterGroupAttribute(T attributeValue, int count = 0)
		{
			AttributeValue = attributeValue;
			Count = count;
		}

		public T AttributeValue { get; set; }

		public int Count { get; set; }

		public override string ToString()
		{
			var uiString = GetUiStringForValue(AttributeValue);
			return Count == 0 || string.IsNullOrEmpty(uiString) ? String.Empty : uiString + " [" + Count + "]";
		}
	}

	#endregion

	#region CharacterIdHashSet Definition
	public class CharacterIdHashSet : ISerializable, IDeserializationCallback, ISet<string>
	{
		private readonly HashSet<string> m_hashSet;

		public CharacterIdHashSet()
		{
			IsReadOnly = false;
			m_hashSet = new HashSet<string>();
		}

		public CharacterIdHashSet(IEnumerable<string> sourceEnumerable)
		{
			IsReadOnly = false;
			m_hashSet = new HashSet<string>(sourceEnumerable);
			var sourceHashset = sourceEnumerable as CharacterIdHashSet;
			if (sourceHashset != null)
				PriorityComparer = sourceHashset.PriorityComparer;
		}

		public IComparer<string> PriorityComparer { private get; set; }

		public override string ToString()
		{
			return string.Join("; ", PrioritySortedList.Select(CharacterVerseData.GetCharacterNameForUi));
		}

		public string HighestPriorityCharacter
		{
			get { return PrioritySortedList.FirstOrDefault(); }
		}

		private List<string> PrioritySortedList
		{
			get
			{
				var characterList = m_hashSet.ToList();
				characterList.Sort(PriorityComparer);
				return characterList;
			}
		}

		/// <summary>
		/// Gets an alphabetically sorted list
		/// </summary>
		public List<string> ToList()
		{
			return m_hashSet.Select(CharacterVerseData.GetCharacterNameForUi).OrderBy(c => c).ToList();
		}

		#region Serialization
		public void OnDeserialization(object sender)
		{
			m_hashSet.OnDeserialization(sender);
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			m_hashSet.GetObjectData(info, context);
		}
		#endregion

		public void Add(string item)
		{
			((ISet<string>)this).Add(item);
		}

		public bool Remove(string item)
		{
			if (IsReadOnly)
				throw new InvalidOperationException("Set is closed.");

			return m_hashSet.Remove(item);
		}

		public IEnumerator<string> GetEnumerator()
		{
			return m_hashSet.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return m_hashSet.GetEnumerator();
		}

		bool ISet<string>.Add(string item)
		{
			if (IsReadOnly)
				throw new InvalidOperationException("Set is closed.");

			return m_hashSet.Add(item);
		}

		public void Clear()
		{
			if (IsReadOnly)
				throw new InvalidOperationException("Set is closed.");

			m_hashSet.Clear();
		}

		public bool Contains(string item)
		{
			return m_hashSet.Contains(item);
		}

		public int Count
		{
			get { return m_hashSet.Count; }
		}

		public void ExceptWith(IEnumerable<string> other)
		{
			m_hashSet.ExceptWith(other);
		}

		public void IntersectWith(IEnumerable<string> other)
		{
			m_hashSet.IntersectWith(other);
		}

		public bool IsProperSubsetOf(IEnumerable<string> other)
		{
			return m_hashSet.IsProperSubsetOf(other);
		}

		public bool IsProperSupersetOf(IEnumerable<string> other)
		{
			return m_hashSet.IsProperSupersetOf(other);
		}

		public bool IsSubsetOf(IEnumerable<string> other)
		{
			return m_hashSet.IsSubsetOf(other);
		}

		public bool IsSupersetOf(IEnumerable<string> other)
		{
			return m_hashSet.IsSupersetOf(other);
		}

		public bool Overlaps(IEnumerable<string> other)
		{
			return m_hashSet.Overlaps(other);
		}

		public bool SetEquals(IEnumerable<string> other)
		{
			return m_hashSet.SetEquals(other);
		}

		public void SymmetricExceptWith(IEnumerable<string> other)
		{
			m_hashSet.SymmetricExceptWith(other);
		}

		public void UnionWith(IEnumerable<string> other)
		{
			m_hashSet.UnionWith(other);
		}

		public void CopyTo(string[] array, int arrayIndex)
		{
			m_hashSet.CopyTo(array, arrayIndex);
		}

		public bool IsReadOnly { get; set; }
	}

	public class CharacterByKeyStrokeComparer : IComparer<String>
	{
		private Func<string, int> GetKeystrokesByCharacterId { get; }

		public CharacterByKeyStrokeComparer(Func<string, int> getKeystrokesByCharacterId)
		{
			GetKeystrokesByCharacterId = getKeystrokesByCharacterId;
		}

		public int Compare(string x, string y)
		{
			var xKeyStrokes = GetKeystrokesByCharacterId(x);
			var yKeyStrokes = GetKeystrokesByCharacterId(y);
			return -xKeyStrokes.CompareTo(yKeyStrokes);
		}
	}

#endregion
}
