﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Glyssen.Shared;
using SIL;
using SIL.Reporting;
using Glyssen.Shared.Bundle;
using GlyssenEngine.Bundle;
using GlyssenEngine.ErrorHandling;
using SIL.Xml;
using static System.String;

namespace GlyssenEngine
{
	/// <summary>
	/// Lets us work with a reference text even when we don't have an actual reference text object.
	/// This enables the use of local reference texts (e.g., proprietary or customized) which don't
	/// ship with Glyssen and therefore may not be available on some machine where the project is
	/// later opened.
	/// </summary>
	public class ReferenceTextProxy : IReferenceTextProxy, IReferenceTextProject
	{
		public static IProjectPersistenceReader Reader
		{
			get => s_reader;
			set 
			{ 
				// For production code, this should only ever get set once and we could just use a
				// simple auto-property, but for tests, it could possibly get changed (though at
				// the time of this writing, it never does), and since tests create and discard
				// custom reference texts, we need to forget about any that are deleted.
				if (s_reader == value)
					return;
				if (s_reader != null)
				{
					if (s_reader is IProjectPersistenceWriter oldWriter)
						oldWriter.OnProjectDeleted -= OnProjectDeleted;
					else
						ClearCache();
				}

				s_reader = value;

				if (s_reader is IProjectPersistenceWriter newWriter)
					newWriter.OnProjectDeleted += OnProjectDeleted;
			}
		}

		private static void OnProjectDeleted(object sender, IProject project)
		{
			if (project is IReferenceTextProject refText && refText.Type == ReferenceTextType.Custom)
				s_allAvailable.RemoveAll(r => r.Type == ReferenceTextType.Custom && r.Name == project.Name);
		}

		private static Action<Exception, string> ErrorReporterForCopyrightedReferenceTexts { get; set; }
		private static List<ReferenceTextProxy> s_allAvailable;
		private static bool s_allAvailableLoaded = false;
		private static IProjectPersistenceReader s_reader;
		private GlyssenDblTextMetadata m_metadata;

		public ReferenceTextType Type { get; }
		public GlyssenDblTextMetadataBase Metadata => m_metadata;
		public string CustomIdentifier { get; }
		public string Name => CustomIdentifier ?? Type.ToString();

		public bool Missing => (m_metadata == null) && !Reader.ResourceExists(this, ProjectResource.Metadata);

		private ReferenceTextProxy(ReferenceTextType type, GlyssenDblTextMetadata metadata = null)
		{
			Debug.Assert(type.IsStandard());
			Type = type;
			CustomIdentifier = null;
			m_metadata = metadata ?? LoadMetadata(type);
		}

		private ReferenceTextProxy(ReferenceTextType type, string customId, GlyssenDblTextMetadata metadata)
		{
			Debug.Assert(!type.IsStandard());
			Debug.Assert(customId != null);
			Type = type;
			CustomIdentifier = customId;
			if (metadata != null)
				m_metadata = metadata;
			else
				AttemptToLoadMetadataForCustomRefText();
		}

		private void AttemptToLoadMetadataForCustomRefText()
		{
			Debug.Assert(Type == ReferenceTextType.Custom);
			m_metadata = LoadMetadata(Type, Reader.Load(this, ProjectResource.Metadata));
		}

		public static IEnumerable<ReferenceTextProxy> AllAvailable
		{
			get
			{
				if (!s_allAvailableLoaded)
					LoadAllAvailable();
				return s_allAvailable;
			}
		}

		#region Internal methods to support unit testing (some day the need may arise to do this in production code)
		internal static void ClearCache()
		{
			s_allAvailable = null;
			s_allAvailableLoaded = false;
		}

		internal static void ForgetMissingCustomReferenceTexts()
		{
			s_allAvailable.RemoveAll(r => r.Type == ReferenceTextType.Custom && r.Missing);
		}
		#endregion

		/// <summary>
		/// Note that this is NOT thread-safe!
		/// </summary>
		public static ReferenceTextProxy GetOrCreate(ReferenceTextType referenceTextType, string proprietaryReferenceTextIdentifier = null)
		{
			ReferenceTextProxy proxy;
			bool standard = referenceTextType.IsStandard();
			if (s_allAvailable == null)
			{
				s_allAvailable = new List<ReferenceTextProxy>();
				proxy = null;
			}
			else
			{
				proxy = standard ? s_allAvailable.SingleOrDefault(i => i.Type == referenceTextType) :
					s_allAvailable.SingleOrDefault(i => i.CustomIdentifier == proprietaryReferenceTextIdentifier);
			}
			if (proxy == null)
			{
				proxy = standard ? new ReferenceTextProxy(referenceTextType) :
					new ReferenceTextProxy(referenceTextType, proprietaryReferenceTextIdentifier, null);
				s_allAvailable.Add(proxy);
			}
			return proxy;
		}

		private static void LoadAllAvailable()
		{
			if (s_allAvailable == null)
				s_allAvailable = new List<ReferenceTextProxy>();
			Tuple<Exception, string> firstLoadError = null;
			var additionalErrors = new List<string>();
			Action<Exception, string> errorReporter = (exception, token) =>
			{
				if (firstLoadError == null)
					firstLoadError = new Tuple<Exception, string>(exception, token);
				else
					additionalErrors.Add(token);
			};

			foreach (var stdReferenceTextType in Enum.GetValues(typeof (ReferenceTextType)).Cast<ReferenceTextType>().Where(t => t.IsStandard() &&
				!s_allAvailable.Any(i => i.Type == t)).ToList())
			{
				var metadata = LoadMetadata(stdReferenceTextType, errorReporter);
				if (metadata != null)
					s_allAvailable.Add(new ReferenceTextProxy(stdReferenceTextType, metadata));
			}

			if (ErrorReporterForCopyrightedReferenceTexts == null)
				ErrorReporterForCopyrightedReferenceTexts = errorReporter;

			foreach (var resourceReader in Reader.GetCustomReferenceTextsNotAlreadyLoaded())
				AttemptToAddCustomReferenceText(resourceReader);
			
			if (firstLoadError != null)
			{
				if (!s_allAvailable.Any())
				{
					throw new Exception(
						Format(Localizer.GetString("ReferenceText.NoReferenceTextsLoaded",
							"No reference texts could be loaded. There might be a problem with your {0} installation. See InnerException " +
							"for more details."), GlyssenInfo.Product),
						firstLoadError.Item1);
				}
				if (additionalErrors.Any())
				{
					var comma = Localizer.GetString("Common.SimpleListSeparator", ", ");
					ErrorReport.ReportNonFatalExceptionWithMessage(firstLoadError.Item1,
						Format(Localizer.GetString("ReferenceText.MultipleLoadErrors",
							"The following reference texts could not be loaded: {0}"),
							Format($"{firstLoadError.Item2}{comma}{Join(comma, additionalErrors)}")));
				}
				else
				{
					ReportNonFatalLoadError(firstLoadError.Item1, firstLoadError.Item2);
				}
			}
			s_allAvailableLoaded = true;
		}

		public static bool IsCustomReferenceTextIdentifierInListOfAvailable(string customId)
		{
			return s_allAvailable?
				.Any(i => i.Type == ReferenceTextType.Custom && i.CustomIdentifier == customId) ??
				false;
		}

		/// <summary>
		/// Attempts to add a custom reference text corresponding to the data in the given ResourceReader.
		/// Note: This method will take care of disposing the ResourceReader object.
		/// </summary>
		private static void AttemptToAddCustomReferenceText(ResourceReader resourceReader)
		{
			var customId = resourceReader.Id;
			Debug.Assert(customId != null);
			if (IsCustomReferenceTextIdentifierInListOfAvailable(customId))
			{
				resourceReader.Dispose();
				return;
			}
			var metadata = LoadMetadata(ReferenceTextType.Custom, resourceReader,
				ErrorReporterForCopyrightedReferenceTexts);
			if (metadata != null)
				s_allAvailable.Add(new ReferenceTextProxy(ReferenceTextType.Custom, customId, metadata));
		}

		private static GlyssenDblTextMetadata LoadMetadata(ReferenceTextType referenceTextType,
			Action<Exception, string> reportError = null)
		{
			Debug.Assert(referenceTextType.IsStandard());
			return LoadMetadata(referenceTextType, Reader.Load(new ReferenceTextId(referenceTextType), ProjectResource.Metadata), reportError);
		}

		private static GlyssenDblTextMetadata LoadMetadata(ReferenceTextType referenceTextType,
			TextReader reader, Action<Exception, string> reportError = null)
		{
			try
			{
				return XmlSerializationHelper.Deserialize<GlyssenDblTextMetadata>(reader);
			}
			catch (Exception exception)
			{
				NonFatalErrorHandler.HandleException(exception);
				string token = referenceTextType.ToString();
				if (reportError == null)
					throw new ReferenceTextMetadataLoadException(GetLoadErrorMessage(token), exception);
				reportError(exception, token);

				return null;
			}
		}

		private static void ReportNonFatalLoadError(Exception exception, string token)
		{
			ErrorReport.ReportNonFatalExceptionWithMessage(exception, GetLoadErrorMessage(token));
		}

		private static string GetLoadErrorMessage(string token)
		{
			return Format(Localizer.GetString("ReferenceText.CouldNotLoad", "The {0} reference text could not be loaded."),
				token);
		}

		/// <summary>
		/// This class just makes it easier to ignore this kind of exception when debugging.
		/// </summary>
		private class ReferenceTextMetadataLoadException : Exception
		{
			public ReferenceTextMetadataLoadException(string message, Exception innerException) : base(message, innerException)
			{
			}
		}
	}
}
