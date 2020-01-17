using System;
using System.Collections.Generic;
using SIL.Scripture;

namespace GlyssenEngine
{
	public class BlockSplitData
	{
		public static BlockSplitDataOffsetComparer BlockSplitDataOffsetComparer = new BlockSplitDataOffsetComparer();
		public static BlockSplitDataVerseAndOffsetComparer BlockSplitDataVerseAndOffsetComparer = new BlockSplitDataVerseAndOffsetComparer();
		public BlockSplitData(int id)
		{
			Id = id;
		}

		public BlockSplitData(int id, Block blockToSplit, string verseToSplit, int characterOffsetToSplit)
		{
			if (id < 1)
				throw new ArgumentException(@"The value for id must be greater than zero", "id");

			Id = id;
			BlockToSplit = blockToSplit;
			VerseToSplit = verseToSplit;
			CharacterOffsetToSplit = characterOffsetToSplit;
		}

		public int Id { get; private set; }
		public Block BlockToSplit { get; set; }
		public string VerseToSplit { get; set; }
		public int CharacterOffsetToSplit { get; set; }
	}

	public class BlockSplitDataVerseAndOffsetComparer : IComparer<BlockSplitData>
	{
		public int Compare(BlockSplitData x, BlockSplitData y)
		{
			if (x.VerseToSplit == y.VerseToSplit)
			{
				if (x.CharacterOffsetToSplit == y.CharacterOffsetToSplit)
					return 0;
				if (x.CharacterOffsetToSplit == BookScript.kSplitAtEndOfVerse)
					return 1;
				if (y.CharacterOffsetToSplit == BookScript.kSplitAtEndOfVerse)
					return -1;
				return x.CharacterOffsetToSplit < y.CharacterOffsetToSplit ? -1 : 1;
			}

			// PG-671: VerseToSplit can be null
			if (x.VerseToSplit == null)
				return -1;
			if (y.VerseToSplit == null)
				return 1;

			BCVRef xStart = BCVRef.Empty;
			BCVRef xEnd = BCVRef.Empty;
			BCVRef yStart = BCVRef.Empty;
			BCVRef yEnd = BCVRef.Empty;
			BCVRef.VerseToScrRef(x.VerseToSplit, out var xLiteralVerse, out var xRemainingText, ref xStart, ref xEnd);
			BCVRef.VerseToScrRef(y.VerseToSplit, out var yLiteralVerse, out var yRemainingText, ref yStart, ref yEnd);

			if (xStart == yStart)
			{
				if (xEnd == yEnd)
				{
					// Sort verse segments correctly
					if (string.IsNullOrEmpty(xRemainingText))
						return -1;
					if (string.IsNullOrEmpty(yRemainingText))
						return 1;
					return String.Compare(xRemainingText, yRemainingText, StringComparison.InvariantCulture);
				}

				return xEnd < yEnd ? -1 : 1;
			}

			return xStart < yStart ? -1 : 1;
		}
	}

	public class BlockSplitDataOffsetComparer : IComparer<BlockSplitData>
	{
		public int Compare(BlockSplitData x, BlockSplitData y)
		{
			if (x.CharacterOffsetToSplit == y.CharacterOffsetToSplit)
				return 0;
			if (x.CharacterOffsetToSplit == BookScript.kSplitAtEndOfVerse)
				return 1;
			if (y.CharacterOffsetToSplit == BookScript.kSplitAtEndOfVerse)
				return -1;
			return x.CharacterOffsetToSplit < y.CharacterOffsetToSplit ? -1 : 1;
		}
	}
}
