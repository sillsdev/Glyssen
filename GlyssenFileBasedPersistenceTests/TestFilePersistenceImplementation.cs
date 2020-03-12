using System.IO;
using GlyssenFileBasedPersistence;
using SIL.IO;

namespace GlyssenFileBasedPersistenceTests
{
	public class TestFilePersistenceImplementation : PersistenceImplementation
	{
		private static PersistenceImplementation s_currentImpl;

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

		public static PersistenceImplementation DeleteTempCustomReferenceProjectFolder()
		{
			if (s_currentImpl is TestFilePersistenceImplementation testImpl)
				testImpl.CleanTempFiles();

			// REVIEW: Do we want to return null or like this: (Or maybe to an in-memory implementation?)
			s_currentImpl = new PersistenceImplementation();
			return s_currentImpl;
		}

		public static PersistenceImplementation OverrideProprietaryReferenceTextProjectFileLocationToTempLocation()
		{
			if ((s_currentImpl is TestFilePersistenceImplementation testImpl) && testImpl.IsProprietaryReferenceTextLocationOveridden)
				return s_currentImpl;
			var tempFolder = Path.GetTempFileName();
			File.Delete(tempFolder);
			Directory.CreateDirectory(tempFolder);
			s_currentImpl = new TestFilePersistenceImplementation(tempFolder);
			return s_currentImpl;
		}
	}
}
