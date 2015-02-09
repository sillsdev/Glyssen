using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using ProtoScript.Character;

namespace ProtoScript.Quote
{
	public class ProjectQuoteParser
	{
		private int m_allProjectBlocks;
		private int m_completedProjectBlocks;
		readonly Dictionary<string, int> m_completedBlocksPerBook = new Dictionary<string, int>();

		public void ParseProject(Project project, BackgroundWorker projectWorker)
		{
			var cvInfo = new CombinedCharacterVerseData(project);

			foreach (var bookScript in project.Books)
				m_allProjectBlocks += bookScript.GetScriptBlocks().Count;

			Parallel.ForEach(project.Books, b =>
			{
				var bookProgress = new Progress<int>();
				bookProgress.ProgressChanged += (sender, completedBookBlocks) =>
				{
					int prior;
					if (m_completedBlocksPerBook.TryGetValue(b.BookId, out prior))
						m_completedBlocksPerBook[b.BookId] = completedBookBlocks;
					else
						m_completedBlocksPerBook.Add(b.BookId, completedBookBlocks);
					m_completedProjectBlocks += completedBookBlocks - prior;
					int totalPercentComplete = Math.Min(99, (int)((((double)m_completedProjectBlocks) / ((double)m_allProjectBlocks)) * 100));
					projectWorker.ReportProgress(totalPercentComplete);
				};
				b.Blocks = new QuoteParser(cvInfo, b.BookId, b.GetScriptBlocks(), project.ConfirmedQuoteSystem).Parse(bookProgress).ToList();
			});

			projectWorker.ReportProgress(100);
		}
	}
}
