using System.Collections.Generic;
using System.IO;

namespace Glyssen.Shared
{
	public interface IProjectPersistenceReader
	{
		/// <summary>
		/// Enumerates resource readers (where the Id is the name of the custom reference text)
		/// for all available custom reference texts, except those already loaded, as determined
		/// by a call to the static method IsCustomReferenceTextIdentifierInListOfAvailable.
		/// Note that if an implementation returns a reference texts that is already in the list,
		/// it will be disregarded by the caller, but the reader will be properly disposed.
		/// </summary>
		IEnumerable<ResourceReader<string>> GetCustomReferenceTextsNotAlreadyLoaded();

		/// <summary>
		/// Gets whether creating a new project (or renaming an existing project) could result
		/// in a collision with an existing project identified by the given language code, id,
		/// and name without.
		/// Implementation note: Typically, this would return the same value as ResourceExists
		/// for ProjectResource.Metadata, given an IUserProject object having these same three
		/// values. If your implementation ensures that the resources are completely under program
		/// control, then an implementation based on that approach should suffice.
		/// </summary>
		bool ProjectExistsHaving(string languageIsoCode, string metadataId, string name);

		/// <summary>
		/// Gets whether the specified resource is present for the project.
		/// </summary>
		bool ResourceExists(IProject project, ProjectResource resource);
		
		/// <summary>
		/// Gets whether a backup of the specified resource is present for the project.
		/// Implementation note: Currently, the only resource for which we have a need
		/// to keep backups is LDML files, but the implementation should probably be
		/// generic in case that changes in the future.
		/// </summary>
		bool BackupResourceExists(IProject project, ProjectResource resource);
		
		/// <summary>
		/// Gets whether the specified book is present for the project.
		/// </summary>
		bool BookResourceExists(IProject project, string bookId);
		
		/// <summary>
		/// Gets a TextReader for the requested resource belonging to the specified project or
		/// null if the resource is not present in the project.
		/// Client is responsible for disposing the TextReader when done with it.
		/// </summary>
		TextReader Load(IProject project, ProjectResource resource);

		/// <summary>
		/// Gets a TextReader for the requested book belonging to the specified project or
		/// null if the book is not present in the project.
		/// Client is responsible for disposing the TextReader when done with it.
		/// </summary>
		TextReader LoadBook(IProject project, string bookId);

		/// <summary>
		/// Attempts to install missing fonts so that they will be available to display
		/// vernacular project data in the host application.
		/// </summary>
		/// <param name="project">Project for which font(s) are missing</param>
		/// <param name="fontRepository"></param>
		/// <returns><c>true</c> if installation is performed (or offered)</returns>
		bool TryInstallFonts(IUserProject project, IFontRepository fontRepository);
	}
}
