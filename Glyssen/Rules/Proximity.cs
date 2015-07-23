using System.Collections.Generic;
using System.Linq;

namespace Glyssen.Rules
{
	public class Proximity
	{
		private Project m_project;

		public Proximity(Project project)
		{
			m_project = project;
		}

		/// <summary>
		/// Calculate the minimum number of blocks between two character ids in given collection
		/// </summary>
		public int CalculateMinimumProximity(IEnumerable<string> characterIds)
		{
			bool foundFirst = false;
			int currentBlockCount = 0;
			int minProximity = -1;
			string prevCharacterId = "";
			foreach (var book in m_project.IncludedBooks)
			{
				foreach (var block in book.Blocks)
				{
					if (block.CharacterId == prevCharacterId)
					{
						currentBlockCount = 0;
					}
					else if (characterIds.Contains(block.CharacterId))
					{
						if (foundFirst)
						{
							if (currentBlockCount < minProximity || minProximity < 0)
							{
								minProximity = currentBlockCount;
							}
						}
						foundFirst = true;
						currentBlockCount = 0;
						prevCharacterId = block.CharacterId;
					}
					else
					{
						currentBlockCount++;
					}
				}
			}

			return minProximity;
		}
	}
}
