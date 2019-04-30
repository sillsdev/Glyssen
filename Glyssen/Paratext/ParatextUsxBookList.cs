using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SIL.DblBundle.Usx;

namespace Glyssen.Paratext
{
	class ParatextUsxBookList : IEnumerable<UsxDocument>
	{
		private class ParatextUsxBookInfo
		{
			internal UsxDocument UsxDoc { get; set; }
			internal string CheckSum { get; set; }
			internal bool PassesChecks { get; set; }
		}

		private readonly SortedDictionary<int, ParatextUsxBookInfo> m_dictionary = new SortedDictionary<int, ParatextUsxBookInfo>();

		public void Add(int bookNum, UsxDocument usx, string checkSum, bool passesChecks)
		{
			m_dictionary[bookNum] = new ParatextUsxBookInfo { UsxDoc = usx, CheckSum = checkSum, PassesChecks = passesChecks };
		}

		public IEnumerator<UsxDocument> GetEnumerator()
		{
			return m_dictionary.Keys.Select(n => m_dictionary[n]).Select(i => i.UsxDoc).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public string GetCheckum(int bookBookNumber)
		{
			return m_dictionary[bookBookNumber].CheckSum;
		}

		public bool GetPassesChecks(int bookBookNumber)
		{
			return m_dictionary[bookBookNumber].PassesChecks;
		}
	}
}
