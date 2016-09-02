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
		public const string kDistFilesReferenceTextDirectoryName = "reference_texts";

		#region static internals to support testing
		internal static string ProprietaryReferenceTextProjectFileLocation { get; set; }
		internal static Action<Exception, string, string> ErrorReporterForCopyrightedReferenceTexts { get; set; }
		#endregion

		private static Dictionary<string, ReferenceTextIdentifier> s_allAvailable;
		private static bool s_allAvailableLoaded = false;

		private readonly ReferenceTextType m_referenceTextType;
		private readonly GlyssenDblTextMetadata m_metadata;
		private readonly string m_name;
		public ReferenceTextType Type
		{
			get { return m_referenceTextType; }
		}
		public string UiLanguageName
		{
			get
			{
				var retVal = m_metadata.Language.Name;
				if (!IsStandardReferenceText(m_referenceTextType))
				{
					Debug.Assert(s_allAvailableLoaded);
					if (s_allAvailable.Values.Any(r => r != this && r.m_metadata.Language.Name == this.m_metadata.Language.Name))
					{
						// More than one reference text with the same language name, so need to qualify the name in the UI using the specific project name
						retVal += string.Format(" ({0})", m_name);
					}
				}
				return retVal;
			}
		}

		public string CustomIdentifier
		{
			get
			{
				return IsStandardReferenceText(m_referenceTextType) ? null : m_metadata.Language.Name + ":" + m_name;
			}
		}

		private ReferenceTextIdentifier(ReferenceTextType type, GlyssenDblTextMetadata metadata = null)
		{
			m_referenceTextType = type;
			m_metadata = metadata;
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
				var metadata = LoadMetadata(itm, errorReporter);
				if (metadata != null)
					s_allAvailable.Add(metadata.Language.Name, new ReferenceTextIdentifier(itm, metadata));
			}

			//if (ErrorReporterForCopyrightedReferenceTexts == null)
			//	ErrorReporterForCopyrightedReferenceTexts = errorReporter;
			//if (ProprietaryReferenceTextProjectFileLocation == null)
			//	ProprietaryReferenceTextProjectFileLocation = Path.Combine(ProjectsBaseFolder, "Local Reference Texts");

			//foreach (var itm in Enum.GetValues(typeof(ReferenceTextType)).Cast<ReferenceTextType>())
			//{
			//	if (itm == ReferenceTextType.Custom || itm == ReferenceTextType.Unknown) continue;

			//	var metadata = LoadMetadata(itm, (exception, token, path) =>
			//	{
			//		Analytics.ReportException(exception);
			//		if (firstLoadError == null)
			//			firstLoadError = new Tuple<Exception, string, string>(exception, token, path);
			//		else
			//			additionalErrors.Add(token);
			//	});
			//	if (metadata != null)
			//		items.Add(metadata.Language.Name, itm);
			//}

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
		}

		internal static GlyssenDblTextMetadata LoadMetadata(ReferenceTextType referenceTextType,
			Action<Exception, string, string> reportError = null)
		{
			Debug.Assert(IsStandardReferenceText(referenceTextType));
			var referenceProjectFilePath = GetReferenceTextProjectFileLocation(referenceTextType);
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
			return Path.Combine(GetProjectFolderForStandardReferenceText(referenceTextType), projectFileName);
		}

		internal static string GetProjectFolderForStandardReferenceText(ReferenceTextType referenceTextType)
		{
			if (!IsStandardReferenceText(referenceTextType))
				throw new InvalidOperationException("Attempt to get standard reference project folder for a non-standard type.");

			return FileLocator.GetFileDistributedWithApplication(kDistFilesReferenceTextDirectoryName,
				referenceTextType.ToString());
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
