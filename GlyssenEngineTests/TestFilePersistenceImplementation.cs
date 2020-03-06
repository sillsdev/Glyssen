using System.IO;
using GlyssenFileBasedPersistence;
using SIL.IO;

namespace GlyssenEngineTests
{
	internal class TestFilePersistenceImplementation : PersistenceImplementation
	{
		internal TestFilePersistenceImplementation(string proprietaryReferenceTextProjectFileLocation)
		{
			ProprietaryReferenceTextProjectFileLocation = proprietaryReferenceTextProjectFileLocation;
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
