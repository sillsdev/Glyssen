using System;
using System.Collections.Generic;
using System.IO;

namespace Glyssen.Shared
{
	public interface IProjectPersistenceReader
	{
		IEnumerable<IProject> AllProjects { get; }
		IEnumerable<ResourceReader<string>> GetAllCustomReferenceTexts(Func<string, bool> exclude);
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
		bool ResourceExists(IProject project, ProjectResource resource);
		// FWIW, currently, the only resource for which we have a need to keep backups is LDML files.
		bool BackupResourceExists(IProject project, ProjectResource resource);
		bool BookResourceExists(IProject project, string bookId);
		/// <summary>
		/// Client is responsible for disposing the TextReader when done with it.
		/// </summary>
		TextReader Load(IProject project, ProjectResource resource);
		TextReader LoadBook(IProject project, string bookId);
		IEnumerable<ResourceReader<string>> GetExistingBooks(IProject project);
		bool TryInstallFonts(IUserProject project, string fontFamily, IFontRepository fontRepository);
	}
}
