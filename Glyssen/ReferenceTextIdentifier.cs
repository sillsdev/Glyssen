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
		private const string kCustomIdPrefix = "Custom: ";

		#region static internals to support testing
		internal static string ProprietaryReferenceTextProjectFileLocation
		{
			get
			{
				if (s_proprietaryReferenceTextProjectFileLocation == null)
					s_proprietaryReferenceTextProjectFileLocation = Path.Combine(Program.BaseDataFolder, kLocalReferenceTextDirectoryName);

				return s_proprietaryReferenceTextProjectFileLocation;
			}
			set { s_proprietaryReferenceTextProjectFileLocation = value; }
		}
		internal static Action<Exception, string, string> ErrorReporterForCopyrightedReferenceTexts { get; set; }
		#endregion

		private static Dictionary<string, ReferenceTextIdentifier> s_allAvailable;
		private static bool s_allAvailableLoaded = false;
		private static string s_proprietaryReferenceTextProjectFileLocation;

		private readonly ReferenceTextType m_referenceTextType;
		private readonly GlyssenDblTextMetadata m_metadata;
		private readonly string m_customId;

		public ReferenceTextType Type { get { return m_referenceTextType; } }
		public GlyssenDblTextMetadata Metadata { get { return m_metadata; } }
		public string CustomIdentifier { get { return m_customId; } }

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
			var lowercase = customId.ToLowerInvariant();
			m_metadata = metadata ?? LoadMetadata(type, Path.Combine(ProjectFolder, lowercase + ProjectBase.kProjectFileExtension));
		}

		// ENHANCE: Change the key from ReferenceTextType to some kind of token that can represent either a standard
		// reference text or a specific custom one.
		public static Dictionary<string, ReferenceTextIdentifier> AllAvailable
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
			var key = standard ? referenceTextType.ToString() : kCustomIdPrefix + proprietaryReferenceTextIdentifier;
			if (s_allAvailable == null)
			{
				s_allAvailable = new Dictionary<string, ReferenceTextIdentifier>();
				identifier = null;
			}
			else
			{
				s_allAvailable.TryGetValue(key, out identifier);
			}
			if (identifier == null)
			{
				if (standard)
					identifier = new ReferenceTextIdentifier(referenceTextType);
				else
				{
					identifier = new ReferenceTextIdentifier(referenceTextType, proprietaryReferenceTextIdentifier, null); 
				}
			}
			s_allAvailable[key] = identifier;
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
				s_allAvailable = new Dictionary<string, ReferenceTextIdentifier>();
			Tuple<Exception, string, string> firstLoadError = null;
			var additionalErrors = new List<string>();
			Action<Exception, string, string> errorReporter = (exception, token, path) =>
			{
				if (firstLoadError == null)
					firstLoadError = new Tuple<Exception, string, string>(exception, token, path);
				else
					additionalErrors.Add(token);
			};

			foreach (var itm in Enum.GetValues(typeof (ReferenceTextType)).Cast<ReferenceTextType>().Where(IsStandardReferenceText))
			{
				if (s_allAvailable.ContainsKey(itm.ToString()))
					continue;
				var metadata = LoadMetadata(itm, errorReporter);
				if (metadata != null)
					s_allAvailable.Add(itm.ToString(), new ReferenceTextIdentifier(itm, metadata));
			}

			if (ErrorReporterForCopyrightedReferenceTexts == null)
				ErrorReporterForCopyrightedReferenceTexts = errorReporter;

			if (Directory.Exists(ProprietaryReferenceTextProjectFileLocation))
			{
				foreach (var dir in Directory.GetDirectories(ProprietaryReferenceTextProjectFileLocation))
				{
					var customId = Path.GetFileName(dir);
					Debug.Assert(customId != null);
					if (s_allAvailable.ContainsKey(customId))
						continue;
					string projectFileName = customId.ToLowerInvariant() + ProjectBase.kProjectFileExtension;
					var refTextProjectFilePath = Path.Combine(dir, projectFileName);
					if (!File.Exists(refTextProjectFilePath))
						continue;
					var metadata = LoadMetadata(ReferenceTextType.Custom, refTextProjectFilePath,
						ErrorReporterForCopyrightedReferenceTexts);
					if (metadata != null)
						s_allAvailable.Add(customId, new ReferenceTextIdentifier(ReferenceTextType.Custom, customId, metadata));
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
