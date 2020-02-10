using System.Collections.Generic;
using System.Diagnostics;
using Glyssen.Shared;

namespace GlyssenEngineTests.Utilities
{
	internal class VerseBridge : IVerse
	{
		public int StartVerse { get; }
		public int EndVerse { get; }

		public VerseBridge(int start, int end)
		{
			Debug.Assert(start > 0);
			Debug.Assert(end > start);
			StartVerse = start;
			EndVerse = end;
		}

		public int LastVerseOfBridge => EndVerse;
		public IEnumerable<int> AllVerseNumbers => this.GetAllVerseNumbers();
	}
}
