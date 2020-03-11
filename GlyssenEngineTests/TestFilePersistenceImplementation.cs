using System.IO;
using GlyssenFileBasedPersistence;
using SIL.IO;

namespace GlyssenEngineTests
{
	internal class TestFilePersistenceImplementation : PersistenceImplementation
	{
		private readonly string s_customReferenceTextBaseFolder;
		
		protected override string ProprietaryReferenceTextProjectFileLocation =>
			s_customReferenceTextBaseFolder ?? base.ProprietaryReferenceTextProjectFileLocation;

		internal TestFilePersistenceImplementation(string proprietaryReferenceTextProjectFileLocation)
		{
			s_customReferenceTextBaseFolder = proprietaryReferenceTextProjectFileLocation;
		}

		public bool IsProprietaryReferenceTextLocationOveridden =>
			!ProprietaryReferenceTextProjectFileLocation.EndsWith(kLocalReferenceTextDirectoryName);

		public void CleanTempFiles()
		{
			if (IsProprietaryReferenceTextLocationOveridden &&
				Directory.Exists(ProprietaryReferenceTextProjectFileLocation))
			{
				RobustIO.DeleteDirectoryAndContents(ProprietaryReferenceTextProjectFileLocation);
			}
		}
	}
}
