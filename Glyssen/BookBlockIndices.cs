using System;
using System.Diagnostics;
using System.Xml.Serialization;
using Microsoft.DotNet.PlatformAbstractions;

namespace Glyssen
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

		/// <summary>
		/// Treat this setter as private (needs to be public for deserialization). Use one of the "Extend..." methods if you want to change this.
		/// </summary>
		[XmlElement("multiBlockCount")]
		public uint MultiBlockCount
		{
			get => m_multiBlockCount;
			set => m_multiBlockCount = value == 1 ? 0 : value;
		}

		public void ExtendToIncludeMoreBlocks(uint insertions)
		{
			MultiBlockCount = BlockCount + insertions;
		}

		public bool ExtendForMatchup(BlockMatchup matchup)
		{
			if (IsMultiBlock)
				throw new InvalidOperationException($"Invalid attempt to extend a multi-block {GetType()} based on a {matchup.GetType()}.");
			if (matchup.IndexOfStartBlockInBook != BlockIndex)
				throw new InvalidOperationException($"This  {GetType()} does not correspond to the given {matchup.GetType()}.");
			if (matchup.OriginalBlockCount == 1)
				return false;
			ExtendToIncludeMoreBlocks((uint)matchup.OriginalBlockCount - 1); // -1 because the first one is already implicitly included
			return true;
		}

		/// <summary>
		/// Advances to the next block index beyond the EffectiveFinalBlockIndex. If this object was
		/// a multiblock location, the MultiBlockCount is re-set such that the state of this object
		/// represents only a single block.
		/// </summary>
		public void AdvanceToNextBlock()
		{
			BlockIndex += (int)BlockCount;
			MultiBlockCount = 0;
		}

		/// <summary>
		/// This will always return a value which is >= 1. Use when the actual
		/// number of blocks is needed, because unfortunately MultiBlockCount never returns 1
		/// (when it is set to either 1 or 0, it represents a single block and returns 0).
		/// </summary>
		public uint BlockCount => MultiBlockCount > 1 ? MultiBlockCount : 1;

		public int EffectiveFinalBlockIndex => IsMultiBlock ? BlockIndex + (int)MultiBlockCount - 1 : BlockIndex;

		public bool IsUndefined => BookIndex == -1 || BlockIndex == -1;

		/// <summary>
		/// This object refers to a run (typically a "matchup") of two or more blocks.
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

		public override string ToString()
		{
			return $"BookIndex: {BookIndex}, BlockIndex: {BlockIndex}, MultiBlockCount: {MultiBlockCount}, EffectiveFinalBlockIndex: {EffectiveFinalBlockIndex}";
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
