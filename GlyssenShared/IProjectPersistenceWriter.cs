using System;

namespace Glyssen.Shared
{
	public interface IProjectPersistenceWriter
	{
		void CreateBackup(IProject project, string description, bool hidden);
		void ChangeProjectName(IProject project, string newName);

		void Save(IProject project, ProjectResource resource, string data);
		void SaveBook(IProject project, string bookId, string data);

		int GetMaxProjectNameLength(IProject project);
		int MaxBaseRecordingNameLength { get; }
	}
}
