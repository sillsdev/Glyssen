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
					if (block.ChapterNumber == 11 && block.InitialStartVerseNumber > 20)
					{
						
					}
					var characters = block.CharacterId.Split('/');
					bool foundCharacter = false;
					foreach (var characterId in characters)
					{
						if (characterId == prevCharacterId)
						{
							foundCharacter = true;
						}
						else if (characterIds.Contains(characterId))
						{
							if (foundFirst)
							{
								if (currentBlockCount < minProximity || minProximity < 0)
								{
									minProximity = currentBlockCount;
								}
							}
							foundFirst = true;
							foundCharacter = true;
							prevCharacterId = characterId;
							break;
						}
					}
					if (foundCharacter)
					{
						currentBlockCount = 0;
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
