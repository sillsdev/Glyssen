using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Glyssen.Shared;
using GlyssenEngine;
using SIL.DblBundle.Text;

namespace InMemoryTestPersistence
{
	public class PersistenceImplementation : IProjectPersistenceReader, IProjectPersistenceWriter
	{
		public const string kBookPrefix = "book:";
		public const string kBackupExtSuffix = "bak";

		private readonly IProjectPersistenceReader m_readerForStandardReferenceTexts = new GlyssenFileBasedPersistence.PersistenceImplementation();

		private static readonly IEqualityComparer<IProject> s_comparer = new ProjectKeyComparer();
		private readonly Dictionary<IProject, Dictionary<string, string>> m_memoryCache = new Dictionary<IProject, Dictionary<string, string>>(s_comparer);
		private readonly Dictionary<IProject, List<Tuple<string, bool>>> m_projectBackups = new Dictionary<IProject, List<Tuple<string, bool>>>();

		public event ProjectDeletedHandler OnProjectDeleted;

		public IEnumerable<ResourceReader>  GetCustomReferenceTextsNotAlreadyLoaded()
		{
			return m_memoryCache.Keys.OfType<IReferenceTextProject>()
				.Where(r => r.Type == ReferenceTextType.Custom && !ReferenceTextProxy.IsCustomReferenceTextIdentifierInListOfAvailable(r.Name))
				.Select(key => new ResourceReader(key.Name, new StringReader(m_memoryCache[key][ProjectResource.Metadata.ToString()])));
		}

		public bool ProjectExistsHaving(string languageIsoCode, string metadataId, string name)
		{
			return m_memoryCache.ContainsKey(new InMemoryProjectStub(languageIsoCode, metadataId, name));
		}

		public bool ResourceExists(IProject project, ProjectResource resource)
		{
			if (project is IReferenceTextProject refText && refText.Type.IsStandard())
				return m_readerForStandardReferenceTexts.ResourceExists(project, resource);
			return m_memoryCache.TryGetValue(project, out var resources) && resources.ContainsKey(resource.ToString());
		}

		public bool BackupResourceExists(IProject project, ProjectResource resource)
		{
			return m_memoryCache.TryGetValue(project, out var resources) && resources.ContainsKey(resource + kBackupExtSuffix);
		}

		public bool BookResourceExists(IProject project, string bookId)
		{
			if (project is IReferenceTextProject refText && refText.Type.IsStandard())
				return m_readerForStandardReferenceTexts.BookResourceExists(project, bookId);
			return m_memoryCache.TryGetValue(project, out var resources) && resources.ContainsKey(kBookPrefix + bookId);
		}

		public TextReader Load(IProject project, ProjectResource resource)
		{
			if (project is IReferenceTextProject refText && refText.Type.IsStandard())
				return m_readerForStandardReferenceTexts.Load(project, resource);
			return Load(project, resource.ToString());
		}

		public TextReader LoadBook(IProject project, string bookId)
		{
			if (project is IReferenceTextProject refText && refText.Type.IsStandard())
				return m_readerForStandardReferenceTexts.LoadBook(project, bookId);
			return Load(project, kBookPrefix + bookId);
		}

		private TextReader Load(IProject project, string name)
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

		public bool TryInstallFonts(IUserProject project, IFontRepository fontRepository)
		{
			Debug.Fail("If a test actually needs this, some kind of simulated implementation should be written.");
			return false;
		}

		public void SetUpProjectPersistence(IProject project)
		{
			if (!m_memoryCache.ContainsKey(project))
				m_memoryCache[project] = new Dictionary<string, string>();
		}

		public TextWriter GetTextWriter(IProject project, ProjectResource resource)
		{
			if (m_memoryCache.TryGetValue(project, out var resources))
			{
				return new InMemoryStringWriter(writer => resources[resource.ToString()] = writer.ToString());
			}
			throw new InvalidOperationException("SetUpProjectPersistence should be called before trying to write a resource!");
		}

		public TextWriter GetTextWriter(IProject project, IScrBook book)
		{
			if (m_memoryCache.TryGetValue(project, out var resources))
			{
				return new InMemoryStringWriter(writer => resources[kBookPrefix + book.BookId] = writer.ToString());
			}
			throw new InvalidOperationException("SetUpProjectPersistence should be called before trying to write a book!");
		}

		public void SetUpProjectPersistence<TM, TL>(IUserProject project, TextBundle<TM, TL> bundle) where TM : DblTextMetadata<TL> where TL : DblMetadataLanguage, new()
		{
			SetUpProjectPersistence(project);
			using (var reader = bundle.GetVersification())
			{
				using (var writer = GetTextWriter(project, ProjectResource.Versification))
					writer.Write(reader.ReadToEnd());
			}
		}

		public void DeleteProject(IUserProject project)
		{
			DeleteProjectInternal(project);
		}

		private void DeleteProjectInternal(IProject project)
		{
			if (m_memoryCache.Remove(project))
				OnProjectDeleted?.Invoke(this, project);
		}

		public void CreateBackup(IUserProject project, string description, bool inactive)
		{
			if (!m_projectBackups.TryGetValue(project, out var backups))
			{
				m_projectBackups[project] = backups = new List<Tuple<string, bool>>();
			}
			backups.Add(new Tuple<string, bool>(description, inactive));
		}

		public bool WasExpectedBackupCreated(IUserProject project, string description, bool inactive)
		{
			return m_projectBackups.TryGetValue(project, out var backups) &&
				backups.Any(b => b.Item1 == description && b.Item2 == inactive);
		}

		public void ChangeProjectName(IUserProject project, string newName)
		{
			throw new NotImplementedException();
		}

		public void ChangePublicationId(IUserProject project, Action setInternalId, Action<TextWriter> saveMetadata)
		{
			throw new NotImplementedException();
		}

		public void ArchiveBookThatIsNoLongerAvailable(IUserProject project, string bookCode)
		{
			throw new NotImplementedException();
		}

		public void RestoreResourceFromBackup(IUserProject project, ProjectResource resource)
		{
			throw new NotImplementedException();
		}

		public bool SaveBackupResource(IUserProject project, ProjectResource resource)
		{
			var projectStore = m_memoryCache[project];
			if (projectStore.TryGetValue(resource.ToString(), out var resourceValue))
			{
				projectStore[resource + kBackupExtSuffix] = resourceValue;
				return true;
			}

			return false;
		}

		public int GetMaxProjectNameLength(IUserProject project)
		{
			throw new NotImplementedException();
		}

		public int GetMaxProjectNameLength(string languageIsoCode) => 100;

		public void ForgetCustomReferenceTexts()
		{
			foreach (var project in m_memoryCache.Keys
				.Where(k => k is IReferenceTextProject refText &&
				refText.Type == ReferenceTextType.Custom).ToList())
			{
				DeleteProjectInternal(project);
			}
		}

		private class InMemoryStringWriter : StringWriter
		{
			private Action<StringWriter> SaveAction { get; }

			public InMemoryStringWriter(Action<StringWriter> saveAction)
			{
				SaveAction = saveAction;
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					//Flush();
					SaveAction(this);
				}

				base.Dispose(disposing);
			}
		}

		public void ClearAllUserProjects()
		{
			if (OnProjectDeleted != null)
			{
				foreach (var project in m_memoryCache.Keys)
					OnProjectDeleted(this, project);
			}

			m_memoryCache.Clear();
			m_projectBackups.Clear();
		}
	}
}
