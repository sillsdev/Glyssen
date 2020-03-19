using System.IO;
using GlyssenFileBasedPersistence;
using SIL.IO;

namespace GlyssenFileBasedPersistenceTests
{
	public class TestFilePersistenceImplementation : PersistenceImplementation
	{
		private static PersistenceImplementation s_currentImpl;
		private static PersistenceImplementation s_restoreImpl;

		private readonly string s_customReferenceTextBaseFolder;
		
		protected override string ProprietaryReferenceTextProjectFileLocation =>
			s_customReferenceTextBaseFolder ?? base.ProprietaryReferenceTextProjectFileLocation;

		internal TestFilePersistenceImplementation(string proprietaryReferenceTextProjectFileLocation)
		{
			s_customReferenceTextBaseFolder = proprietaryReferenceTextProjectFileLocation;
		}

		public bool IsProprietaryReferenceTextLocationOveridden =>
			!ProprietaryReferenceTextProjectFileLocation.EndsWith(kLocalReferenceTextDirectoryName);

		private void CleanTempFiles()
		{
			if (IsProprietaryReferenceTextLocationOveridden &&
				Directory.Exists(ProprietaryReferenceTextProjectFileLocation))
			{
				RobustIO.DeleteDirectoryAndContents(ProprietaryReferenceTextProjectFileLocation);
			}
		}

		public static void CleanupUpTempImplementationAndRestorePreviousImplementation()
		{
			if (s_currentImpl is TestFilePersistenceImplementation testImpl)
				testImpl.CleanTempFiles();

			s_currentImpl = s_restoreImpl;
		}

		public static PersistenceImplementation CurrentImplementation => s_currentImpl;

		public static PersistenceImplementation OverrideProprietaryReferenceTextProjectFileLocationToTempLocation()
		{
			if ((s_currentImpl is TestFilePersistenceImplementation testImpl) && testImpl.IsProprietaryReferenceTextLocationOveridden)
				return s_currentImpl;
			var tempFolder = Path.GetTempFileName();
			File.Delete(tempFolder);
			Directory.CreateDirectory(tempFolder);
			s_restoreImpl = s_currentImpl;
			s_currentImpl = new TestFilePersistenceImplementation(tempFolder);
			return s_currentImpl;
		}
	}
}
