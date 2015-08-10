using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Glyssen.Rules
{
	public class Proximity
	{
		public const int kDefaultMinimumProximity = 30;
		private readonly Project m_project;

		public Proximity(Project project)
		{
			m_project = project;
		}

		/// <summary>
		/// Calculate the minimum number of blocks between two character ids in given collection
		/// </summary>
		public MinimumProximity CalculateMinimumProximity(IEnumerable<string> characterIds)
		{
			bool foundFirst = false;
			int currentBlockCount = 0;
			int minProximity = -1;
			string prevCharacterId = "";
			BookScript firstBook = null;
			Block firstBlock = null;
			BookScript secondBook = null;
			Block secondBlock = null;
			BookScript prevBook = null;
			Block prevBlock = null;
			foreach (var book in m_project.IncludedBooks)
			{
				foreach (var block in book.Blocks)
				{
					if (block.CharacterId == prevCharacterId)
					{
						currentBlockCount = 0;
						prevBook = book;
						prevBlock = block;
					}
					else if (characterIds.Contains(block.CharacterId))
					{
						if (foundFirst)
						{
							if (currentBlockCount < minProximity || minProximity < 0)
							{
								minProximity = currentBlockCount;
								firstBook = prevBook;
								firstBlock = prevBlock;
								secondBook = book;
								secondBlock = block;
							}
						}
						else
						{
							firstBook = book;
							firstBlock = block;
							secondBook = book;
							secondBlock = block;
						}
						foundFirst = true;
						currentBlockCount = 0;
						prevCharacterId = block.CharacterId;
						prevBook = book;
						prevBlock = block;
					}
					else
					{
						currentBlockCount++;
					}
				}
			}

			return new MinimumProximity
			{
				NumberOfBlocks = minProximity,
				FirstBook = firstBook,
				SecondBook = secondBook,
				FirstBlock = firstBlock,
				SecondBlock = secondBlock
			};
		}
	}

	public class MinimumProximity
	{
		public int NumberOfBlocks { get; set; }
		public BookScript FirstBook { get; set; }
		public BookScript SecondBook { get; set; }
		public Block FirstBlock { get; set; }
		public Block SecondBlock { get; set; }

		public override string ToString()
		{
			if (FirstBlock == null || SecondBlock == null)
				return "[no characters in group]";

			var sb = new StringBuilder();
			sb.Append(NumberOfBlocks).Append("  |  ")
				.Append(FirstBook.BookId).Append(" ").Append(FirstBlock.ChapterNumber).Append(":").Append(FirstBlock.InitialStartVerseNumber)
				.Append(" (").Append(FirstBlock.CharacterId).Append(")")
				.Append(" - ")
				.Append(SecondBook.BookId).Append(" ").Append(SecondBlock.ChapterNumber).Append(":").Append(SecondBlock.InitialStartVerseNumber)
				.Append(" (").Append(SecondBlock.CharacterId).Append(")");
			return sb.ToString();
		}
	}
}
