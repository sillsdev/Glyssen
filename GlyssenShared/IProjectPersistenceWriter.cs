using System;
using System.IO;
using SIL.DblBundle.Text;

namespace Glyssen.Shared
{
	public interface IProjectPersistenceWriter
	{
		// These two methods could specify an IUserProject, but I'm keeping them more general
		// to allow for a situation where an implementation might want to allow a custom
		// reference text to be created using these methods.
		void SetUpProjectPersistence(IProject project);
		TextWriter GetTextWriter(IProject project, ProjectResource resource);
		TextWriter GetTextWriter(IProject project, IScrBook book);
		
		void SetUpProjectPersistence<TM, TL>(IUserProject project, TextBundle<TM, TL> bundle)
			where TM : DblTextMetadata<TL>
			where TL : DblMetadataLanguage, new();

		void DeleteProject(IUserProject project);
		void CreateBackup(IUserProject project, string description, bool hidden);
		void ChangeProjectName(IUserProject project, string newName);

		void ArchiveBookNoLongerAvailable(IUserProject project, string bookCode);

		int GetMaxProjectNameLength(IUserProject project);
		int MaxBaseRecordingNameLength { get; }
	}
}
