using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Glyssen.Shared;
using SIL;
using SIL.IO;
using SIL.Reporting;
using Glyssen.Shared.Bundle;
using GlyssenEngine.Bundle;
using GlyssenEngine.ErrorHandling;
using static System.String;

namespace GlyssenEngine
{
	/// <summary>
	/// Lets us work with a reference text even when we don't have an actual reference text object.
	/// Originally (maybe still?) this enabled the use of "proprietary" reference texts
	/// which can't ship with Glyssen for IP reasons but which we know FCBH is using.
	/// </summary>
	public class ReferenceTextProxy : IReferenceTextProxy
	{
		private const string kDistFilesReferenceTextDirectoryName = "reference_texts";

		#region static internals to support testing
		internal static string ProprietaryReferenceTextProjectFileLocation
		{
			get
			{
				if (s_proprietaryReferenceTextProjectFileLocation == null)
					s_proprietaryReferenceTextProjectFileLocation = Path.Combine(GlyssenInfo.BaseDataFolder, Constants.kLocalReferenceTextDirectoryName);

				return s_proprietaryReferenceTextProjectFileLocation;
			}
			set
			{
				if (s_proprietaryReferenceTextProjectFileLocation == value)
					return;
				ClearCache();
				s_proprietaryReferenceTextProjectFileLocation = value;
			}
		}
		internal static Action<Exception, string, string> ErrorReporterForCopyrightedReferenceTexts { get; set; }
		#endregion

		private static List<ReferenceTextProxy> s_allAvailable;
		private static bool s_allAvailableLoaded = false;
		private static string s_proprietaryReferenceTextProjectFileLocation;

		private readonly ReferenceTextType m_referenceTextType;
		private GlyssenDblTextMetadata m_metadata;
		private readonly string m_customId;

		public ReferenceTextType Type => m_referenceTextType;
		public GlyssenDblTextMetadataBase Metadata => m_metadata;
		public string CustomIdentifier => m_customId;
		public string Name => m_customId ?? Type.ToString();

		public bool Missing
		{
			get
			{
				if (m_metadata == null)
					AttemptToLoadMetadataForCustomRefText();
				return m_metadata == null;
			}
		}

		private ReferenceTextProxy(ReferenceTextType type, GlyssenDblTextMetadata metadata = null)
		{
			Debug.Assert(IsStandardReferenceText(type));
			m_referenceTextType = type;
			m_customId = null;
			m_metadata = metadata ?? LoadMetadata(type);
		}

		private ReferenceTextProxy(ReferenceTextType type, string customId, GlyssenDblTextMetadata metadata)
		{
			Debug.Assert(!IsStandardReferenceText(type));
			Debug.Assert(customId != null);
			m_referenceTextType = type;
			m_customId = customId;
			if (metadata != null)
				m_metadata = metadata;
			else
				AttemptToLoadMetadataForCustomRefText();
		}

		private void AttemptToLoadMetadataForCustomRefText()
		{
			Debug.Assert(Type == ReferenceTextType.Custom);
			var lowercase = m_customId.ToLowerInvariant();
			try
			{
				m_metadata = LoadMetadata(Type, Path.Combine(ProjectFolder, lowercase + Constants.kProjectFileExtension));
			}
			catch (Exception)
			{
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
			bool standard = IsStandardReferenceText(referenceTextType);
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

		//public override bool Equals(object obj)
		//{
		//	return base.Equals(obj);
		//}

		//protected bool Equals(ReferenceTextProxy other)
		//{
		//	return m_referenceTextType == other.m_referenceTextType && Equals(m_metadata, other.m_metadata);
		//}

		//public override int GetHashCode()
		//{
		//	unchecked
		//	{
		//		return ((int) m_referenceTextType * 397) ^ (m_metadata != null ? m_metadata.GetHashCode() : 0);
		//	}
		//}

		//public static bool operator ==(ReferenceTextProxy left, ReferenceTextProxy right)
		//{
		//	return Equals(left, right);
		//}

		//public static bool operator !=(ReferenceTextProxy left, ReferenceTextProxy right)
		//{
		//	return !Equals(left, right);
		//}

		private static bool IsStandardReferenceText(ReferenceTextType type)
		{
			return (type != ReferenceTextType.Custom && type != ReferenceTextType.Unknown);
		}

		private static void LoadAllAvailable()
		{
			if (s_allAvailable == null)
				s_allAvailable = new List<ReferenceTextProxy>();
			Tuple<Exception, string, string> firstLoadError = null;
			var additionalErrors = new List<string>();
			Action<Exception, string, string> errorReporter = (exception, token, path) =>
			{
				if (firstLoadError == null)
					firstLoadError = new Tuple<Exception, string, string>(exception, token, path);
				else
					additionalErrors.Add(token);
			};

			foreach (var itm in Enum.GetValues(typeof (ReferenceTextType)).Cast<ReferenceTextType>().Where(t => IsStandardReferenceText(t) &&
				!s_allAvailable.Any(i => i.Type == t)).ToList())
			{
				var metadata = LoadMetadata(itm, errorReporter);
				if (metadata != null)
					s_allAvailable.Add(new ReferenceTextProxy(itm, metadata));
			}

			if (ErrorReporterForCopyrightedReferenceTexts == null)
				ErrorReporterForCopyrightedReferenceTexts = errorReporter;

			if (Directory.Exists(ProprietaryReferenceTextProjectFileLocation))
			{
				foreach (var dir in Directory.GetDirectories(ProprietaryReferenceTextProjectFileLocation))
				{
					var customId = Path.GetFileName(dir);
					AttemptToAddCustomReferenceTextIdentifier(customId, dir);
				}
			}

			if (firstLoadError != null)
			{
				if (!s_allAvailable.Any())
				{
					throw new Exception(
						Format(Localizer.GetString("ReferenceText.NoReferenceTextsLoaded",
							"No reference texts could be loaded. There might be a problem with your {0} installation. See InnerException " +
							"for more details."), GlyssenInfo.kProduct),
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
					ReportNonFatalLoadError(firstLoadError.Item1, firstLoadError.Item2, firstLoadError.Item3);
				}
			}
			s_allAvailableLoaded = true;
		}

		private static bool IsCustomReferenceTextIdentifierInListOfAvailable(string customId)
		{
			return s_allAvailable.Any(i => i.Type == ReferenceTextType.Custom && i.CustomIdentifier == customId);
		}

		private static bool AttemptToAddCustomReferenceTextIdentifier(string customId, string dir)
		{
			Debug.Assert(customId != null);
			if (IsCustomReferenceTextIdentifierInListOfAvailable(customId))
				return false;
			string projectFileName = customId.ToLowerInvariant() + Constants.kProjectFileExtension;
			var refTextProjectFilePath = Path.Combine(dir, projectFileName);
			if (!File.Exists(refTextProjectFilePath))
				return false;
			var metadata = LoadMetadata(ReferenceTextType.Custom, refTextProjectFilePath,
				ErrorReporterForCopyrightedReferenceTexts);
			if (metadata != null)
			{
				s_allAvailable.Add(new ReferenceTextProxy(ReferenceTextType.Custom, customId, metadata));
				return true;
			}
			return false;
		}

		private static GlyssenDblTextMetadata LoadMetadata(ReferenceTextType referenceTextType,
			Action<Exception, string, string> reportError = null)
		{
			Debug.Assert(IsStandardReferenceText(referenceTextType));
			var referenceProjectFilePath = GetReferenceTextProjectFileLocation(referenceTextType);
			return LoadMetadata(referenceTextType, referenceProjectFilePath, reportError);
		}

		private static GlyssenDblTextMetadata LoadMetadata(ReferenceTextType referenceTextType,
			string referenceProjectFilePath, Action<Exception, string, string> reportError = null)
		{
			Exception exception;
			var metadata = GlyssenDblTextMetadata.Load<GlyssenDblTextMetadata>(referenceProjectFilePath, out exception);
			if (exception != null)
			{
				NonFatalErrorHandler.HandleException(exception);
				string token = referenceTextType.ToString();
				if (reportError == null)
					throw new ReferenceTextMetadataLoadException(GetLoadErrorMessage(token, referenceProjectFilePath), exception);
				reportError(exception, token, referenceProjectFilePath);
				return null;
			}
			return metadata;
		}

		public static bool IsCustomReferenceAvailable(string customId)
		{
			var dir = Path.Combine(ProprietaryReferenceTextProjectFileLocation, customId);
			if (!Directory.Exists(dir))
				return false;

			if (s_allAvailable == null)
				s_allAvailable = new List<ReferenceTextProxy>();
			else if (IsCustomReferenceTextIdentifierInListOfAvailable(customId))
				return true;

			ErrorReporterForCopyrightedReferenceTexts = (exception, s, arg3) => { };

			return AttemptToAddCustomReferenceTextIdentifier(customId, dir);
		}

		private static string GetReferenceTextProjectFileLocation(ReferenceTextType referenceTextType)
		{
			Debug.Assert(IsStandardReferenceText(referenceTextType));
			string projectFileName = referenceTextType.ToString().ToLowerInvariant() + Constants.kProjectFileExtension;
			return FileLocationUtilities.GetFileDistributedWithApplication(kDistFilesReferenceTextDirectoryName, referenceTextType.ToString(), projectFileName);
		}

		internal static string GetProjectFolderForStandardReferenceText(ReferenceTextType referenceTextType)
		{
			if (!IsStandardReferenceText(referenceTextType))
				throw new InvalidOperationException("Attempt to get standard reference project folder for a non-standard type.");

			return Path.GetDirectoryName(GetReferenceTextProjectFileLocation(referenceTextType));
		}

		public string ProjectFolder
		{
			get
			{
				if (IsStandardReferenceText(Type))
					return GetProjectFolderForStandardReferenceText(Type);

				return Path.Combine(ProprietaryReferenceTextProjectFileLocation, m_customId);
			}
		}

		private static void ReportNonFatalLoadError(Exception exception, string token, string path)
		{
			ErrorReport.ReportNonFatalExceptionWithMessage(exception, GetLoadErrorMessage(token, path));
		}

		private static string GetLoadErrorMessage(string token, string path)
		{
			return Format(Localizer.GetString("ReferenceText.CouldNotLoad", "The {0} reference text could not be loaded from: {1}"),
				token, path);
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
