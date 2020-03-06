using GlyssenFileBasedPersistence;

namespace GlyssenEngineTests
{
	internal class TestFilePersistenceImplementation : PersistenceImplementation
	{
		internal TestFilePersistenceImplementation(string proprietaryReferenceTextProjectFileLocation)
		{
			base.ProprietaryReferenceTextProjectFileLocation = proprietaryReferenceTextProjectFileLocation;
		}
	}
}
