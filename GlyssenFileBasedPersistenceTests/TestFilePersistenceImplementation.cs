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
		
		protected override string CustomReferenceTextProjectFileLocation =>
			s_customReferenceTextBaseFolder ?? base.CustomReferenceTextProjectFileLocation;

		internal TestFilePersistenceImplementation(string proprietaryReferenceTextProjectFileLocation)
		{
			s_customReferenceTextBaseFolder = proprietaryReferenceTextProjectFileLocation;
		}

		public bool IsProprietaryReferenceTextLocationOveridden =>
			!CustomReferenceTextProjectFileLocation.EndsWith(kLocalReferenceTextDirectoryName);

		private void CleanTempFiles()
		{
			if (IsProprietaryReferenceTextLocationOveridden &&
				Directory.Exists(CustomReferenceTextProjectFileLocation))
			{
				RobustIO.DeleteDirectoryAndContents(CustomReferenceTextProjectFileLocation);
			}
		}

		public static void CleanupUpTempImplementationAndRestorePreviousImplementation()
		{
			if (s_currentImpl is TestFilePersistenceImplementation testImpl)
				testImpl.CleanTempFiles();

			s_currentImpl = s_restoreImpl;
		}

		public static PersistenceImplementation CurrentImplementation => s_currentImpl;

		public static PersistenceImplementation OverrideProprietaryReferenceTextProjectFileLocationToTempLocation(bool createFolder = true)
		{
			if ((s_currentImpl is TestFilePersistenceImplementation testImpl) && testImpl.IsProprietaryReferenceTextLocationOveridden)
				return s_currentImpl;
			var tempFolder = Path.GetTempFileName();
			File.Delete(tempFolder);
			if (createFolder)
				Directory.CreateDirectory(tempFolder);
			s_restoreImpl = s_currentImpl;
			s_currentImpl = new TestFilePersistenceImplementation(tempFolder);
			return s_currentImpl;
		}
	}
}
