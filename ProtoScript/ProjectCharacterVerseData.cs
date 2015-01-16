using System.IO;

namespace ProtoScript
{
	public class ProjectCharacterVerseData : CharacterVerseData
	{
		public ProjectCharacterVerseData(string fullPath)
		{
			if (File.Exists(fullPath))
				LoadData(File.ReadAllText(fullPath));
		}

		public void WriteToFile(string fullPath)
		{
			RemoveDataAlsoInControlFile();
			File.WriteAllText(fullPath, ToTabDelimited());
		}

		private void RemoveDataAlsoInControlFile()
		{
			RemoveAll(ControlCharacterVerseData.Singleton.GetAllQuoteInfo(), new BcvCharacterDeliveryComparer());
		}
	}
}
