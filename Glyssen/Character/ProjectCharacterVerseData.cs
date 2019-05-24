using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DesktopAnalytics;
using SIL.Extensions;
using SIL.Scripture;

namespace Glyssen.Character
{
	public class ProjectCharacterVerseData : CharacterVerseData
	{
		public ProjectCharacterVerseData(string fullPath)
		{
			if (File.Exists(fullPath))
				LoadData(File.ReadAllText(fullPath));
			else
				LoadData("");
		}

		public virtual void Add(CharacterVerse cv)
		{
			AddCharacterVerse(cv);

			Analytics.Track("AddCharacter", new Dictionary<string, string>
			{
				{"verseReference", cv.BcvRef.AsString},
				{"characterId", cv.Character},
				{"delivery", cv.Delivery}
			});
		}

		public override ICharacterDeliveryInfo GetImplicitCharacter(int bookId, int chapter, int startVerse, int endVerse = 0, ScrVers versification = null)
		{
			// There's no way to add a project-specific implicit character
			return null;
		}

		public void WriteToFile(string fullPath)
		{
			RemoveDataAlsoInControlFile();
			File.WriteAllText(fullPath, ToTabDelimited());
		}

		private string ToTabDelimited()
		{
			var sb = new StringBuilder();
			foreach (CharacterVerse cv in GetAllQuoteInfo())
				sb.Append(GetTabDelimitedFields(cv)).Append(Environment.NewLine);
			return sb.ToString();
		}

		private string GetTabDelimitedFields(CharacterVerse cv)
		{
			return cv.BookCode + "\t" + cv.Chapter + "\t" + cv.Verse + "\t" + cv.Character + "\t" + cv.Delivery + "\t" + cv.Alias;
		}

		protected override CharacterVerse CreateCharacterVerse(BCVRef bcvRef, string[] items)
		{
			return new CharacterVerse(bcvRef, items[3], items[4], items[5], true);
		}

		private void RemoveDataAlsoInControlFile()
		{
			RemoveAll(ControlCharacterVerseData.Singleton.GetAllQuoteInfo(), new BcvCharacterDeliveryEqualityComparer());
		}

		public void Remove(int bookNum, int chapterNumber, int initialStartVerseNumber, int initialEndVerseNumber, string characterId, string delivery)
		{
			RemoveAll(GetCharacters(bookNum, chapterNumber, initialStartVerseNumber, initialEndVerseNumber)
				.Where(c => c.Character == characterId && c.Delivery == delivery), new BcvCharacterDeliveryEqualityComparer());
		}

		public void UnionWith(ProjectCharacterVerseData otherProjectData)
		{
			foreach (var cv in otherProjectData.GetAllQuoteInfo())
				AddCharacterVerse(cv);
		}
	}
}
