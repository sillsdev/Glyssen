using System.IO;
using SIL.DblBundle.Text;

namespace Glyssen.Shared
{
	public interface IProjectPersistenceWriter
	{
		void SetUpProjectPersistence(IProject project);
		void SetUpProjectPersistence<TM, TL>(IProject project, TextBundle<TM, TL> bundle)
			where TM : DblTextMetadata<TL>
			where TL : DblMetadataLanguage, new();
		void DeleteProject(IProject project);
		void CreateBackup(IProject project, string description, bool hidden);
		void ChangeProjectName(IProject project, string newName);

		TextWriter GetTextWriter(IProject project, ProjectResource resource);
		TextWriter GetTextWriter(IProject project, IScrBook book);
		void ArchiveBookNoLongerAvailable(IProject project, string bookCode);

		int GetMaxProjectNameLength(IProject project);
		int MaxBaseRecordingNameLength { get; }
	}
}
