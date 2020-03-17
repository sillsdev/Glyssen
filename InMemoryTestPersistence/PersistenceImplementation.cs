using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Glyssen.Shared;
using SIL.DblBundle.Text;

namespace InMemoryTestPersistence
{
	public class PersistenceImplementation : IProjectPersistenceReader, IProjectPersistenceWriter
	{
		public const string kBookPrefix = "book:";
		public const string kFallbackVersificationPrefix = "fallback_";
		public const string kBackupExtSuffix = "bak";

		private static readonly IEqualityComparer<IProject> s_comparer = new ProjectKeyComparer();
		private readonly Dictionary<IProject, Dictionary<string, string>> m_memoryCache = new Dictionary<IProject, Dictionary<string, string>>(s_comparer);

		public IEnumerable<ResourceReader<string>> GetAllCustomReferenceTexts(Func<string, bool> exclude)
		{
			foreach (var key in m_memoryCache.Keys.OfType<IReferenceTextProject>().Where(r => r.Type == ReferenceTextType.Custom))
			{
				yield return new ResourceReader<string>(key.Name, new StringReader(m_memoryCache[key][ProjectResource.Metadata.ToString()]));
			}
		}

		public bool ProjectExistsHaving(string languageIsoCode, string metadataId, string name)
		{
			return m_memoryCache.ContainsKey(new InMemoryProjectStub(languageIsoCode, metadataId, name));
		}

		public bool ResourceExists(IProject project, ProjectResource resource)
		{
			return m_memoryCache.TryGetValue(project, out var resources) && resources.ContainsKey(resource.ToString());
		}

		public bool BackupResourceExists(IProject project, ProjectResource resource)
		{
			return m_memoryCache.TryGetValue(project, out var resources) && resources.ContainsKey(resource.ToString() + kBackupExtSuffix);
		}

		public bool BookResourceExists(IProject project, string bookId)
		{
			return m_memoryCache.TryGetValue(project, out var resources) && resources.ContainsKey(kBookPrefix + bookId);
		}

		public TextReader Load(IProject project, ProjectResource resource)
		{
			return Load(project, resource.ToString());
		}

		public TextReader LoadBook(IProject project, string bookId)
		{
			return Load(project, kBookPrefix + bookId);
		}

		public TextReader Load(IProject project, string name)
		{
			if (m_memoryCache.TryGetValue(project, out var resources))
			{
				if (resources.TryGetValue(name, out var stringValue))
				{
					return new StringReader(stringValue);
				}
			}
			return null;
		}

		public IEnumerable<ResourceReader<string>> GetExistingBooks(IProject project)
		{
			throw new NotImplementedException();
		}

		public bool TryInstallFonts(IUserProject project, string fontFamily, IFontRepository fontRepository)
		{
			throw new NotImplementedException();
		}

		public void SetUpProjectPersistence(IProject project)
		{
			throw new NotImplementedException();
		}

		public TextWriter GetTextWriter(IProject project, ProjectResource resource)
		{
			throw new NotImplementedException();
		}

		public TextWriter GetTextWriter(IProject project, IScrBook book)
		{
			throw new NotImplementedException();
		}

		public void SetUpProjectPersistence<TM, TL>(IUserProject project, TextBundle<TM, TL> bundle) where TM : DblTextMetadata<TL> where TL : DblMetadataLanguage, new()
		{
			throw new NotImplementedException();
		}

		public void DeleteProject(IUserProject project)
		{
			throw new NotImplementedException();
		}

		public void CreateBackup(IUserProject project, string description, bool hidden)
		{
			throw new NotImplementedException();
		}

		public void ChangeProjectName(IUserProject project, string newName)
		{
			throw new NotImplementedException();
		}

		public void ChangePublicationId(IUserProject project, string newId, Action setInternalId, Action<TextWriter> saveMetadata)
		{
			throw new NotImplementedException();
		}

		public void ArchiveBookNoLongerAvailable(IUserProject project, string bookCode)
		{
			throw new NotImplementedException();
		}

		public void UseBackupResource(IUserProject project, ProjectResource resource)
		{
			throw new NotImplementedException();
		}

		public bool SaveBackupResource(IUserProject project, ProjectResource resource)
		{
			throw new NotImplementedException();
		}

		public int GetMaxProjectNameLength(IUserProject project)
		{
			throw new NotImplementedException();
		}

		public int MaxBaseRecordingNameLength { get; }
	}
}
