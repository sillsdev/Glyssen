using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using GlyssenEngine.Character;

namespace Glyssen.Character
{
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

	public class CharacterGroupAttribute<T> where T : struct, IConvertible
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
