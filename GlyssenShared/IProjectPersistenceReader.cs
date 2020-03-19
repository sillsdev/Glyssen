using System;
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

		// REVIEW: Maybe a better name is possible
		/// <summary>
		/// Gets whether there is (or might be) an existing project identified by the given
		/// language code, id, and name. Typically, this would return the same value as
		/// ResourceExists for ProjectResource.Metadata, given an IUserProject object having
		/// these same three value. However, the purpose of this method is to determine whether
		/// it would be valid to create a new project (or rename an existing project) to have
		/// these values without clobbering something else. So, for example, even if there
		/// were no existing metadata resource, if there were other things identified by
		/// these attributes, this should still return true.
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
		/// Gets a TextReader for the requested resource belonging to the specified project.
		/// Client is responsible for disposing the TextReader when done with it.
		/// </summary>
		TextReader Load(IProject project, ProjectResource resource);

		/// <summary>
		/// Gets a TextReader for the requested book belonging to the specified project.
		/// Client is responsible for disposing the TextReader when done with it.
		/// </summary>
		TextReader LoadBook(IProject project, string bookId);

		/// <summary>
		/// Enumerates the existing books in for the specified project, getting a
		/// wrapper object that holds the TextReader and book ID.
		/// Client is responsible for disposing each ResourceReader when done with it.
		/// </summary>
		IEnumerable<ResourceReader<string>> GetExistingBooks(IProject project);

		bool TryInstallFonts(IUserProject project, string fontFamily, IFontRepository fontRepository);
	}
}
