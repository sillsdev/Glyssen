using System;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Glyssen.Shared.Bundle
{
	[XmlRoot]
	public class BookBlockIndices : IEquatable<BookBlockIndices>, IComparable<BookBlockIndices>
	{
		private uint m_multiBlockCount;

		public BookBlockIndices()
		{
			BookIndex = -1;
			BlockIndex = -1;
		}

		public BookBlockIndices(int bookIndex, int blockIndex, uint multiBlockCount = 0)
		{
			BookIndex = bookIndex;
			BlockIndex = blockIndex;
			MultiBlockCount = multiBlockCount;
		}

		public BookBlockIndices(BookBlockIndices copyFrom)
		{
			BookIndex = copyFrom.BookIndex;
			BlockIndex = copyFrom.BlockIndex;
			MultiBlockCount = copyFrom.MultiBlockCount;
		}

		[XmlElement("bookIndex")]
		public int BookIndex { get; set; }

		[XmlElement("blockIndex")]
		public int BlockIndex { get; set; }

		[XmlElement("multiBlockCount")]
		public uint MultiBlockCount
		{
			get { return m_multiBlockCount; }
			set
			{
				m_multiBlockCount = value;
				Debug.Assert(MultiBlockCount >= 0);
			}
		}

		public int EffectiveFinalBlockIndex => IsMultiBlock ? BlockIndex + (int)MultiBlockCount - 1 : BlockIndex;

		public bool IsUndefined => BookIndex == -1 || BlockIndex == -1;


		/// <summary>
		/// Technically, this just means this object refers to a run of blocks of specified length. It could be 1, though this is
		/// not likely.
		/// </summary>
		public bool IsMultiBlock => MultiBlockCount > 0;

		public bool Contains(BookBlockIndices indices)
		{
			return BookIndex == indices.BookIndex && BlockIndex <= indices.BlockIndex && EffectiveFinalBlockIndex >= indices.EffectiveFinalBlockIndex;
		}

		#region equality members
		public bool Equals(BookBlockIndices other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return BookIndex == other.BookIndex && BlockIndex == other.BlockIndex && MultiBlockCount == other.MultiBlockCount;
		}

		public int CompareTo(BookBlockIndices other)
		{
			int result = BookIndex.CompareTo(other.BookIndex);
			if (result == 0)
				result = BlockIndex.CompareTo(other.BlockIndex);
			if (result == 0)
				result = MultiBlockCount.CompareTo(other.MultiBlockCount);
			return result;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			if (obj.GetType() != this.GetType())
				return false;
			return Equals((BookBlockIndices)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (BookIndex * 397) ^ BlockIndex;
			}
		}

		public static bool operator ==(BookBlockIndices left, BookBlockIndices right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(BookBlockIndices left, BookBlockIndices right)
		{
			return !Equals(left, right);
		}
		#endregion
	}

}
