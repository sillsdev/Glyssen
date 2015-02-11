using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using ProtoScript.Bundle;
using ProtoScript.Utilities;

namespace ProtoScript
{
	public class ProjectUsxParser
	{
		public IEnumerable<BookScript> ParseProject(IEnumerable<UsxDocument> books, IStylesheet stylesheet, BackgroundWorker projectWorker)
		{
			var numBlocksPerBook = new ConcurrentDictionary<string, int>();
			var blocksInBook = new ConcurrentDictionary<string, XmlNodeList>();
			Parallel.ForEach(books, bookScript =>
			{
				var nodeList = bookScript.GetChaptersAndParas();
				blocksInBook.AddOrUpdate(bookScript.BookId, nodeList, (s, list) => nodeList);
				numBlocksPerBook.AddOrUpdate(bookScript.BookId, nodeList.Count, (s, i) => nodeList.Count);
			});
			int allProjectBlocks = numBlocksPerBook.Values.Sum();
			
			int completedProjectBlocks = 0;
			var result = new List<BookScript>();
			Parallel.ForEach(blocksInBook.Keys, bookId =>
			{
				result.Add(new BookScript(bookId, new UsxParser(bookId, stylesheet, blocksInBook[bookId]).Parse()));
				completedProjectBlocks += numBlocksPerBook[bookId];
				projectWorker.ReportProgress(MathUtilities.Percent(completedProjectBlocks, allProjectBlocks, 99));
			});

			projectWorker.ReportProgress(100);
			return result;
		}
	}
}
