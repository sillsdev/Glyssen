using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using Glyssen.Dialogs;
using Glyssen.Rules;

namespace Glyssen.Character
{
	public class CharacterGroup
	{
		private bool m_isActorAssigned;
		private VoiceActor.VoiceActor m_actorAssigned;
		private bool m_closed;

		//For Serialization
		public CharacterGroup()
		{
			CharacterIds = new CharacterIdHashSet();
		}

		public CharacterGroup(int groupNumber) : this()
		{
			GroupNumber = groupNumber;
		}

		public void AssignVoiceActor(VoiceActor.VoiceActor actor)
		{
			if (actor == null)
				return;

			m_isActorAssigned = true;
			m_actorAssigned = actor;
		}

		public void RemoveVoiceActor()
		{
			m_isActorAssigned = false;
			m_actorAssigned = null;
		}

		[XmlElement]
		public int GroupNumber { get; set; }

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
				var characterDetails = CharacterDetailData.Singleton.GetDictionary();
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

		[XmlIgnore]
		public string StatusDisplay
		{
			get { return Status ? "Y" : ""; }
		}

		[XmlElement]
		public double EstimatedHours { get; set; }

		[XmlIgnore]
		[Browsable(false)]
		public bool IsVoiceActorAssigned
		{
			get { return m_isActorAssigned; }
		}

		[XmlIgnore]
		[Browsable(false)]
		public VoiceActor.VoiceActor VoiceActorAssigned
		{
			get { return m_actorAssigned; }
		}

		[XmlElement]
		[Browsable(false)]
		public int VoiceActorAssignedId
		{
			get { return m_actorAssigned == null ? -1 : m_actorAssigned.Id; }
			set
			{
				m_actorAssigned = new VoiceActor.VoiceActor();
				m_actorAssigned.Id = value;
				m_isActorAssigned = true;
				if (value < 0)
				{
					m_isActorAssigned = false;
				}
			}
		}

		[XmlIgnore]
		public string VoiceActorAssignedName
		{
			get { return m_isActorAssigned ? m_actorAssigned.Name : ""; }
		}

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

			result &= CharacterIds.All(i =>
			{
				CharacterGender gender = CharacterDetailData.Singleton.GetDictionary()[i].Gender;
				return genderMatchingOptions.Matches(character.Gender, gender);
			});

			result &= CharacterIds.All(i =>
			{
				CharacterAge age = CharacterDetailData.Singleton.GetDictionary()[i].Age;
				return ageMatchingOptions.Matches(character.Age, age);
			});

			return result;
		}

		public bool ContainsCharacterWithGender(CharacterGender gender)
		{
			return CharacterIds.Any(c =>
			{
				if (CharacterVerseData.IsCharacterStandard(c))
					return gender == CharacterGender.Either;

				return CharacterDetailData.Singleton.GetDictionary()[c].Gender == gender;
			});
		}

		public bool ContainsCharacterWithAge(CharacterAge age)
		{
			return CharacterIds.Any(c =>
			{
				if (CharacterVerseData.IsCharacterStandard(c))
					return age == CharacterAge.Adult;

				return CharacterDetailData.Singleton.GetDictionary()[c].Age == age;
			});
		}
	}

	#region CharacterGroupAttribute Definition

	public class CharacterGroupAttributeSet<T> : SortedSet<CharacterGroupAttribute<T>> where T: struct, IConvertible
	{
		private readonly Dictionary<T, CharacterGroupAttribute<T>> m_entryNameToDataEntry;

		private class CharacterAttributeComparer<T> : IComparer<CharacterGroupAttribute<T>> where T : struct, IConvertible
		{
			private readonly IComparer<T> m_comparer;

			public CharacterAttributeComparer(IComparer<T> comparer)
			{
				m_comparer = comparer;
			}

			public int Compare(CharacterGroupAttribute<T> x, CharacterGroupAttribute<T> y)
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
			GetUiStringForValue = a => a.ToString();
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

		public override string ToString()
		{
			return string.Join("; ", ToList());
		}

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

#endregion
}
