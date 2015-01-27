using System;
using System.Collections.Generic;
using ProtoScript.Properties;
using SIL.ScriptureUtils;

namespace ProtoScript.Character
{
	public class ControlCharacterVerseData : CharacterVerseData
	{
		private static ControlCharacterVerseData s_singleton;

		internal static string TabDelimitedCharacterVerseData { get; set; }

		private ControlCharacterVerseData()
		{
			// Tests can set this before accessing the Singleton.
			if (TabDelimitedCharacterVerseData == null)
				TabDelimitedCharacterVerseData = Resources.CharacterVerseData;
			LoadAll();
		}

		public static ControlCharacterVerseData Singleton
		{
			get { return s_singleton ?? (s_singleton = new ControlCharacterVerseData()); }
		}

		public int ControlFileVersion { get; private set; }

		private void LoadAll()
		{
			if (Any())
				return;

			LoadData(TabDelimitedCharacterVerseData);

			if (!Any())
				throw new ApplicationException("No character verse data available!");
		}

		public override void AddCharacterVerse(CharacterVerse cv)
		{
			throw new ApplicationException("A new character cannot be added to the control file directly");
		}

		protected override ISet<CharacterVerse> ProcessLine(string[] items, int lineNumber)
		{
			if (lineNumber == 0)
			{
				ProcessFirstLine(items);
				return null;
			}
			return base.ProcessLine(items, lineNumber);
		}

		private void ProcessFirstLine(string[] items)
		{
			int cfv;
			if (Int32.TryParse(items[1], out cfv) && items[0].StartsWith("Control File"))
				ControlFileVersion = cfv;
			else
				throw new ApplicationException("Bad format in CharacterVerseDataBase metadata: " + items);
		}

		protected override CharacterVerse CreateCharacterVerse(BCVRef bcvRef, string[] items)
		{
			return new CharacterVerse(bcvRef, items[3], items[4], items[5], false,
				(items.Length > kiIsDialogue && items[kiIsDialogue].Equals("True", StringComparison.OrdinalIgnoreCase)),
				(items.Length > kiDefaultCharacter) ? items[kiDefaultCharacter] : null,
				(items.Length > kiParallelPassageInfo) ? items[kiParallelPassageInfo] : null);
		}
	}
}
