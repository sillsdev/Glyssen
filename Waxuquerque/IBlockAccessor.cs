using System;
using System.Collections.Generic;
using SIL.Scripture;

namespace Glyssen
{
	public interface IBlockAccessor
	{
		BookScript CurrentBook { get; }
		Block CurrentBlock { get; }
		Block CurrentEndBlock { get; }
		BookBlockIndices GetIndices();
		BookBlockIndices GetIndicesOfSpecificBlock(Block block);
		BookBlockIndices GetIndicesOfFirstBlockAtReference(VerseRef verseRef, bool allowMidQuoteBlock = false);
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
		IEnumerable<Block> GetPreviousBlocksWithinBookWhile(Func<Block, bool> predicate);
		IEnumerable<Block> GetNextBlocksWithinBookWhile(Func<Block, bool> predicate);
		Block GetNthPreviousBlockWithinBook(int n);
		Block GetNthPreviousBlockWithinBook(int n, Block baseLineBlock);
		Block GetPreviousBlock();
	}
}