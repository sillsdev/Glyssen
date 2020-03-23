using System;
using System.IO;
using SIL.DblBundle.Text;

namespace Glyssen.Shared
{
	public interface IProjectPersistenceWriter
	{
		/// <summary>
		/// Method to alert the persistence implementation that project data is about
		/// to be persisted so that any required initialization can be performed.
		/// </summary>
		void SetUpProjectPersistence(IProject project);
		
		/// <summary>
		/// Method to alert the persistence implementation that a project is being
		/// created from a Text Release Bundle.
		/// Implementation note: In addition to any required initialization to prepare for
		/// persistence, any required work related to extracting the contents of the bundle
		/// and preparing those resources for use should also be performed. If the application(s)
		/// dependent on a persistence implementation do not need to handle creation of projects
		/// from bundles, the implementation can just throw a NotImplementedException, which will
		/// result in a fatal (unhandled) error if called.
		/// </summary>
		void SetUpProjectPersistence<TM, TL>(IUserProject project, TextBundle<TM, TL> bundle)
			where TM : DblTextMetadata<TL>
			where TL : DblMetadataLanguage, new();
		
		/// <summary>
		/// Gets a TextWriter for the requested resource belonging to the specified project.
		/// Client is responsible for disposing the TextWriter when done with it.
		/// </summary>
		TextWriter GetTextWriter(IProject project, ProjectResource resource);

		/// <summary>
		/// Gets a TextWriter for the requested book belonging to the specified project.
		/// Client is responsible for disposing the TextWriter when done with it.
		/// </summary>
		TextWriter GetTextWriter(IProject project, IScrBook book);
		
		/// <summary>
		/// Permanently removes all persisted resources related to the specified project,
		/// included any required cleanup.
		/// </summary>
		/// <param name="project"></param>
		void DeleteProject(IUserProject project);

		/// <summary>
		/// Persists a new backup copy of the project using the provided description as the
		/// project name. if so requested, the backup will have the Inactive flag set in its
		/// metadata (this will not affect the metadata of the original project).
		/// </summary>
		void CreateBackup(IUserProject project, string description, bool hidden);

		/// <summary>
		/// Changes the name of the project. After this returns, any future access to project
		/// resources (for reading or writing) must be done using the new name.
		/// Implementation notes: Implementation need not be thread safe. Caller will ensure that
		/// no other threads attempt to read or persist project data while this method is being
		/// executed.
		/// Caller must guarantee that the length of the new name is not greater than the value
		/// returned by <see cref="GetMaxProjectNameLength"/>. Implementation may throw a fatal
		/// exception if the caller violates this requirement.
		/// </summary>
		void ChangeProjectName(IUserProject project, string newName);

		/// <summary>
		/// Changes the metadata ID (also called the publication ID because it represents
		/// the ID under which the project data is published).
		/// Implementation note: It is fairly unusual for a publication ID to be changed.
		/// Currently, it can only happen when restoring from a glyssenshare file on a
		/// machine where the Paratext project was restored from a backup (resulting in a
		/// different publication ID). If the application(s) dependent on a persistence
		/// implementation do not need to handle this case, the implementation can just
		/// throw a NotImplementedException. GlyssenEngine guarantees to handle it in a
		/// non-fatal way (unless the implementation of ReportApplicationError in the
		/// IParatextProjectLoadingAssistant treats it as a fatal error).
		/// </summary>
		/// <param name="project">The project (having the old MetadataId)</param>
		/// <param name="setInternalId">Action to be called when the implementation is ready for
		/// the internal metadata id of the project to be changed (after which getting the
		/// project's MetadataId will return the new value.</param>
		/// <param name="saveMetadata">Action to be called to persist the project metadata
		/// using the given TextWriter. Callee is responsible for disposing the writer.
		/// If calling this action throws an exception, callee is responsible for reverting
		/// the internal metadata ID to the previous value and the persistence implementation
		/// is responsible for rolling back any other changes so that the project is still
		/// accessible using the old metadata ID.</param>
		void ChangePublicationId(IUserProject project, Action setInternalId, Action<TextWriter> saveMetadata);

		void ArchiveBookThatIsNoLongerAvailable(IUserProject project, string bookCode);

		/// <summary>
		/// Replaces the current version of the project resource with the backup.
		/// Caller is responsible for calling BackupResourceExists first to ensure
		/// this is a valid operation. If the backup does not exist, the behavior of
		/// this method is undefined (i.e., it can throw an exception).
		/// Implementation note: currently, the only resource for which we have a need to keep
		/// backups is LDML files, but the implementation should probably be generic in case'
		/// that changes in the future.
		/// </summary>
		void RestoreResourceFromBackup(IUserProject project, ProjectResource resource);

		/// <summary>
		/// Persists a backup copy of the specified project resource.
		/// </summary>
		bool SaveBackupResource(IUserProject project, ProjectResource resource);

		/// <summary>
		/// For a given project (i.e., with a known LanguageIsoCode length), this method gets the maximum
		/// allowable project name length so as to ensure that this persistence implementation would be
		/// able to store any and all (including possible future) project resources for the project. The
		/// implementation of <see cref="ChangeProjectName"/> can assume that the caller has ensured that
		/// any new name passed in to it will be no longer than the value returned by this method.
		/// </summary>
		int GetMaxProjectNameLength(IUserProject project);

		/// <summary>
		/// For a given LanguageIsoCode, this method gets the maximum allowable project name length so as
		/// to ensure that this persistence implementation would be able to store any and all (including
		/// possible future) project resources for the project. Implementations of this interface can
		/// assume that any IProject object passed to any method in this interface, will have a Name that
		/// is no longer than the value returned by this method.
		/// </summary>
		int GetMaxProjectNameLength(string languageIsoCode);
	}
}
