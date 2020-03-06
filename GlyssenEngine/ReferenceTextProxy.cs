using System;
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
using static System.String;

namespace GlyssenEngine
{
	/// <summary>
	/// Lets us work with a reference text even when we don't have an actual reference text object.
	/// This enables the use of local reference texts (e.g., proprietary or customized) which don't
	/// ship with Glyssen and therefore may not be available on some machine where the project is
	/// later opened.
	/// </summary>
	public class ReferenceTextProxy : IReferenceTextProxy, IProject
	{
		public static IProjectPersistenceReader Reader
		{
			get => s_reader;
			set 
			{ 
				if (s_reader == value)
					return;
				ClearCache();
				s_reader = value;
			}
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

		public bool Missing => (m_metadata == null) && Reader.ResourceExists(this, ProjectResource.Metadata);

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
			using (var reader = Reader.Load(this, ProjectResource.Metadata))
			{
				if (reader != null)
					m_metadata = LoadMetadata(Type, reader);
			}
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

		/// <summary>
		/// This is mainly for testing, though some day the need may arise to do this in production code.
		/// </summary>
		internal static void ClearCache()
		{
			s_allAvailable = null;
			s_allAvailableLoaded = false;
		}

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

			foreach (var itm in Enum.GetValues(typeof (ReferenceTextType)).Cast<ReferenceTextType>().Where(t => t.IsStandard() &&
				!s_allAvailable.Any(i => i.Type == t)).ToList())
			{
				var metadata = LoadMetadata(itm, errorReporter);
				if (metadata != null)
					s_allAvailable.Add(new ReferenceTextProxy(itm, metadata));
			}

			if (ErrorReporterForCopyrightedReferenceTexts == null)
				ErrorReporterForCopyrightedReferenceTexts = errorReporter;

			foreach (var resourceReader in Reader.GetAllCustomReferenceTexts(IsCustomReferenceTextIdentifierInListOfAvailable))
			{
				try
				{
					AttemptToAddCustomReferenceTextIdentifier(resourceReader);
				}
				finally
				{
					resourceReader.Dispose();
				}
			}
			
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

		private static bool IsCustomReferenceTextIdentifierInListOfAvailable(string customId)
		{
			return s_allAvailable.Any(i => i.Type == ReferenceTextType.Custom && i.CustomIdentifier == customId);
		}

		private static bool AttemptToAddCustomReferenceTextIdentifier(ResourceReader<string> resourceReader)
		{
			var customId = resourceReader.Id;
			Debug.Assert(customId != null);
			Debug.Assert(!IsCustomReferenceTextIdentifierInListOfAvailable(customId));
			var metadata = LoadMetadata(ReferenceTextType.Custom, resourceReader,
				ErrorReporterForCopyrightedReferenceTexts);
			if (metadata != null)
			{
				s_allAvailable.Add(new ReferenceTextProxy(ReferenceTextType.Custom, customId, metadata));
				return true;
			}
			return false;
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
				return Project.Deserialize<GlyssenDblTextMetadata>(reader);
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

		// REVIEW: Is this needed? Only used in the unit tests that claim to test it.
		public static bool IsCustomReferenceAvailable(string customId)
		{
			return Reader.ResourceExists(new ReferenceTextId(ReferenceTextType.Custom, customId), ProjectResource.Metadata);
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
