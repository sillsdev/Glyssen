using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Glyssen.Rules;

namespace Glyssen.Character
{
	public class CharacterGroup
	{
		public const int kNoActorAssigned = -1;
		private Project m_project;
		private bool m_closed;

		//For Serialization
		public CharacterGroup()
		{
			CharacterIds = new CharacterIdHashSet();
			VoiceActorId = kNoActorAssigned;
		}

		public CharacterGroup(Project project, IComparer<string> characterIdPriorityComparer = null) : this()
		{
			// In its current usage, the only reason it makes sense to allow this to be passed in is for efficiency.
			// In some places in the code, we create one comparer and pass it in to multiple constuctors.
			if (characterIdPriorityComparer == null)
				characterIdPriorityComparer = new CharacterByKeyStrokeComparer(project.GetKeyStrokesByCharacterId());

			Initialize(project, characterIdPriorityComparer);
		}

		public void Initialize(Project project, IComparer<string> characterIdPriorityComparer)
		{
			m_project = project;
			CharacterIds.PriorityComparer = characterIdPriorityComparer;
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
		[Browsable(false)]
		public int GroupNumber { get; set; }

		[XmlAttribute("name")]
		[Browsable(false)]
		public string GroupNameInternal { get; set; }

		[Browsable(false)]
		public string Name
		{
			get
			{
				if (string.IsNullOrEmpty(GroupNameInternal))
					return CharacterIds.HighestPriorityCharacter;
				return GroupNameInternal;
			}
			set { GroupNameInternal = value; }
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

		[XmlElement]
		public double EstimatedHours { get; set; }

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

		public bool Matches(CharacterDetail character, CharacterGenderMatchingOptions genderMatchingOptions, CharacterAgeMatchingOptions ageMatchingOptions)
		{
			if (!CharacterIds.Any())
				throw new InvalidOperationException("No characters added to this group yet.");

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
			var characterDetails = m_project.AllCharacterDetailDictionary;
			return CharacterIds.Any(c =>
			{
				if (CharacterVerseData.IsCharacterStandard(c))
					return gender == CharacterGender.Either;

				return characterDetails[c].Gender == gender;
			});
		}

		public bool ContainsCharacterWithAge(CharacterAge age)
		{
			var characterDetails = m_project.AllCharacterDetailDictionary;
			return CharacterIds.Any(c =>
			{
				if (CharacterVerseData.IsCharacterStandard(c))
					return age == CharacterAge.Adult;

				return characterDetails[c].Age == age;
			});
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
			get { return m_hashSet.Count(); }
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
		private readonly Dictionary<string, int> m_keystrokesDictionary;

		public CharacterByKeyStrokeComparer(Dictionary<string, int> keystrokesDictionary)
		{
			m_keystrokesDictionary = keystrokesDictionary;
		}

		public int Compare(string x, string y)
		{
			int xKeyStrokes;
			int yKeyStrokes;
			m_keystrokesDictionary.TryGetValue(x, out xKeyStrokes);
			m_keystrokesDictionary.TryGetValue(y, out yKeyStrokes);
			return -xKeyStrokes.CompareTo(yKeyStrokes);
		}
	}

#endregion
}
