using System.IO;

namespace Glyssen.Shared
{
	public interface IProjectPersistenceReader
	{
		bool ProjectExists(string languageIsoCode, string metadataId, string name);
		bool ResourceExists(ProjectResource resource, IProject project);
		/// <summary>
		/// Client is responsible for disposing the TextReader when done with it.
		/// </summary>
		TextReader Load(ProjectResource resource, IProject project);
		TextReader LoadBook(IProject project, string bookId);
	}
}
