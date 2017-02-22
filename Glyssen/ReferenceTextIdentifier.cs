using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DesktopAnalytics;
using Glyssen.Bundle;
using L10NSharp;
using SIL.IO;
using SIL.Reporting;

namespace Glyssen
{
	public enum ReferenceTextType
	{
		Unknown,
		English,
		//Azeri,
		//French,
		//Indonesian,
		//Portuguese,
		Russian,
		//Spanish,
		//TokPisin,
		Custom
	}

	public class ReferenceTextIdentifier
	{
		private const string kDistFilesReferenceTextDirectoryName = "reference_texts";
		public const string kLocalReferenceTextDirectoryName = "Local Reference Texts";

		#region static internals to support testing
		internal static string ProprietaryReferenceTextProjectFileLocation
		{
			get
			{
				if (s_proprietaryReferenceTextProjectFileLocation == null)
					s_proprietaryReferenceTextProjectFileLocation = Path.Combine(Program.BaseDataFolder, kLocalReferenceTextDirectoryName);

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

		private static List<ReferenceTextIdentifier> s_allAvailable;
		private static bool s_allAvailableLoaded = false;
		private static string s_proprietaryReferenceTextProjectFileLocation;

		private readonly ReferenceTextType m_referenceTextType;
		private GlyssenDblTextMetadata m_metadata;
		private readonly string m_customId;

		public ReferenceTextType Type => m_referenceTextType;
		public GlyssenDblTextMetadata Metadata => m_metadata;
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

		private ReferenceTextIdentifier(ReferenceTextType type, GlyssenDblTextMetadata metadata = null)
		{
			Debug.Assert(IsStandardReferenceText(type));
			m_referenceTextType = type;
			m_customId = null;
			m_metadata = metadata ?? LoadMetadata(type);
		}

		private ReferenceTextIdentifier(ReferenceTextType type, string customId, GlyssenDblTextMetadata metadata)
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
				m_metadata = LoadMetadata(Type, Path.Combine(ProjectFolder, lowercase + ProjectBase.kProjectFileExtension));
			}
			catch (Exception)
			{
			}
		}

		public static IEnumerable<ReferenceTextIdentifier> AllAvailable
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

		public static ReferenceTextIdentifier GetOrCreate(ReferenceTextType referenceTextType, string proprietaryReferenceTextIdentifier = null)
		{
			ReferenceTextIdentifier identifier;
			bool standard = IsStandardReferenceText(referenceTextType);
			if (s_allAvailable == null)
			{
				s_allAvailable = new List<ReferenceTextIdentifier>();
				identifier = null;
			}
			else
			{
				identifier = standard ? s_allAvailable.SingleOrDefault(i => i.Type == referenceTextType) :
					s_allAvailable.SingleOrDefault(i => i.CustomIdentifier == proprietaryReferenceTextIdentifier);
			}
			if (identifier == null)
			{
				identifier = standard ? new ReferenceTextIdentifier(referenceTextType) :
					new ReferenceTextIdentifier(referenceTextType, proprietaryReferenceTextIdentifier, null);
				s_allAvailable.Add(identifier);
			}
			return identifier;
		}

		//public override bool Equals(object obj)
		//{
		//	return base.Equals(obj);
		//}

		//protected bool Equals(ReferenceTextIdentifier other)
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

		//public static bool operator ==(ReferenceTextIdentifier left, ReferenceTextIdentifier right)
		//{
		//	return Equals(left, right);
		//}

		//public static bool operator !=(ReferenceTextIdentifier left, ReferenceTextIdentifier right)
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
				s_allAvailable = new List<ReferenceTextIdentifier>();
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
					s_allAvailable.Add(new ReferenceTextIdentifier(itm, metadata));
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
						String.Format(LocalizationManager.GetString("ReferenceText.NoReferenceTextsLoaded",
							"No reference texts could be loaded. There might be a problem with your {0} installation. See InnerException " +
							"for more details."), Program.kProduct),
						firstLoadError.Item1);
				}
				if (additionalErrors.Any())
				{
					ErrorReport.ReportNonFatalExceptionWithMessage(firstLoadError.Item1,
						String.Format(LocalizationManager.GetString("ReferenceText.MultipleLoadErrors",
							"The following reference texts could not be loaded: {0}, {1}"), firstLoadError.Item2,
							String.Join(", ", additionalErrors)));
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
			string projectFileName = customId.ToLowerInvariant() + ProjectBase.kProjectFileExtension;
			var refTextProjectFilePath = Path.Combine(dir, projectFileName);
			if (!File.Exists(refTextProjectFilePath))
				return false;
			var metadata = LoadMetadata(ReferenceTextType.Custom, refTextProjectFilePath,
				ErrorReporterForCopyrightedReferenceTexts);
			if (metadata != null)
			{
				s_allAvailable.Add(new ReferenceTextIdentifier(ReferenceTextType.Custom, customId, metadata));
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
			string referenceProjectFilePath,
			Action<Exception, string, string> reportError = null)
		{
			Exception exception;
			var metadata = GlyssenDblTextMetadata.Load<GlyssenDblTextMetadata>(referenceProjectFilePath, out exception);
			if (exception != null)
			{
				Analytics.ReportException(exception);
				string token = referenceTextType.ToString();
				if (reportError == null)
					throw new Exception(GetLoadErrorMessage(token, referenceProjectFilePath));
				else
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
				s_allAvailable = new List<ReferenceTextIdentifier>();
			else if (IsCustomReferenceTextIdentifierInListOfAvailable(customId))
				return true;

			ErrorReporterForCopyrightedReferenceTexts = (exception, s, arg3) => { };

			return AttemptToAddCustomReferenceTextIdentifier(customId, dir);
		}

		private static string GetReferenceTextProjectFileLocation(ReferenceTextType referenceTextType)
		{
			Debug.Assert(IsStandardReferenceText(referenceTextType));
			string projectFileName = referenceTextType.ToString().ToLowerInvariant() + ProjectBase.kProjectFileExtension;
			return FileLocator.GetFileDistributedWithApplication(kDistFilesReferenceTextDirectoryName, referenceTextType.ToString(), projectFileName);
		}

		internal static string GetProjectFolderForStandardReferenceText(ReferenceTextType referenceTextType)
		{
			if (!IsStandardReferenceText(referenceTextType))
				throw new InvalidOperationException("Attempt to get standard reference project folder for a non-standard type.");

			return Path.GetDirectoryName(GetReferenceTextProjectFileLocation(referenceTextType));
		}

		internal string ProjectFolder
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
			return String.Format(LocalizationManager.GetString("ReferenceText.CouldNotLoad", "The {0} reference text could not be loaded from: {1}"),
				token, path);
		}
	}
}
