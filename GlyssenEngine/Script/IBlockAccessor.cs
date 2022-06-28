using System;
using System.Collections.Generic;
using SIL.Scripture;

namespace GlyssenEngine.Script
{
	public interface IBlockAccessor
	{
		BookScript CurrentBook { get; }
		Block CurrentBlock { get; }
		Block CurrentEndBlock { get; }
		BookBlockIndices GetIndices();
		BookBlockIndices GetIndicesOfSpecificBlock(Block block);
		BookBlockIndices GetIndicesOfFirstBlockAtReference(IScrVerseRef verseRef, bool allowMidQuoteBlock = false);
		BookScript GetBookScriptContainingBlock(Block block);
		bool IsLastBook(BookScript book);
		bool IsLastBlock();
		bool IsLastBlockInBook(BookScript book, Block block);
		bool IsFirstBook(BookScript book);
		bool IsFirstBlock(Block block);
		bool IsFirstBlockInBook(BookScript book, Block block);
		Block GetNextBlock();
		IEnumerable<Block> GetNextNBlocksWithinBook(int numberOfBlocks);
		Block GetNthNextBlockWithinBook(int n);
		Block GetNthNextBlockWithinBook(int n, Block baseLineBlock);
		IEnumerable<Block> GetPreviousNBlocksWithinBook(int numberOfBlocks);
		IEnumerable<Block> GetSurroundingBlocksWithinBookWhile(Func<Block, bool> predicate, bool forwardOnly, Block startBlock = null);
		Block GetNthPreviousBlockWithinBook(int n);
		Block GetNthPreviousBlockWithinBook(int n, Block baseLineBlock);
		Block GetPreviousBlock();
	}
}
