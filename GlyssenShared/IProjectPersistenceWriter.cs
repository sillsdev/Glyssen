using System;
using System.IO;
using SIL.DblBundle.Text;

namespace Glyssen.Shared
{
	public interface IProjectPersistenceWriter
	{
		void SetUpProjectPersistence(IProject project);
		TextWriter GetTextWriter(IProject project, ProjectResource resource);
		TextWriter GetTextWriter(IProject project, IScrBook book);
		
		void SetUpProjectPersistence<TM, TL>(IUserProject project, TextBundle<TM, TL> bundle)
			where TM : DblTextMetadata<TL>
			where TL : DblMetadataLanguage, new();

		void DeleteProject(IUserProject project);
		void CreateBackup(IUserProject project, string description, bool hidden);
		void ChangeProjectName(IUserProject project, string newName);
		void ChangePublicationId(IUserProject project, string newId, Action setInternalId, Action<TextWriter> saveMetadata);

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
		bool SaveBackupResource(IUserProject project, ProjectResource resource);

		int GetMaxProjectNameLength(IUserProject project);
		int MaxBaseRecordingNameLength { get; }
	}
}
