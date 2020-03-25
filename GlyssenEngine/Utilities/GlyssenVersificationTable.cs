using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Paratext.Data;
using SIL.Scripture;
using SIL;

namespace GlyssenEngine.Utilities
{
	public class GlyssenVersificationTable : ParatextVersificationTable
	{
		public enum InvalidVersificationLineExceptionHandling
		{
			ReportIndividually,
			BatchErrors,
			Throw,
		}

		private InvalidVersificationLineExceptionHandling m_errorHandling = InvalidVersificationLineExceptionHandling.ReportIndividually;

		private Dictionary<string, Dictionary<VersificationLoadErrorType, List<string>>> m_errors;

		public InvalidVersificationLineExceptionHandling VersificationLineExceptionHandling
		{
			get => m_errorHandling;
			set
			{
				m_errorHandling = value;
				if (m_errorHandling == InvalidVersificationLineExceptionHandling.BatchErrors)
				{
					m_errors = new Dictionary<string, Dictionary<VersificationLoadErrorType, List<string>>>();
				}
			}
		}

		protected override bool HandleVersificationLineError(InvalidVersificationLineException ex)
		{
			if (VersificationLineExceptionHandling == InvalidVersificationLineExceptionHandling.BatchErrors)
			{
				if (!m_errors.TryGetValue(ex.FileName, out var errorList))
					m_errors[ex.FileName] = errorList = new Dictionary<VersificationLoadErrorType, List<string>>();
				if (!errorList.TryGetValue(ex.Type, out var errorsOfCurrentType))
				{
					errorList[ex.Type] = errorsOfCurrentType = new List<string>();
				}
				errorsOfCurrentType.Add(ex.LineText);
				// Returning true means that we have handled the error, so it will not be thrown.
				return true;
			}

			return VersificationLineExceptionHandling == InvalidVersificationLineExceptionHandling.ReportIndividually &&
				base.HandleVersificationLineError(ex);
		}

		private  GlyssenVersificationTable()
		{
		}

		public static void Initialize()
		{
			if (!(Implementation is GlyssenVersificationTable))
				Implementation = new GlyssenVersificationTable();
		}

		public string GetBatchedErrorString(string versificationKey)
		{
			Debug.Assert(VersificationLineExceptionHandling == InvalidVersificationLineExceptionHandling.BatchErrors);
			if (m_errors.TryGetValue(versificationKey, out var errors))
			{
				var sb = new StringBuilder();

				foreach (var type in errors.Keys)
				{
					switch (type)
					{
						case VersificationLoadErrorType.InvalidSyntax:
							sb.Append(Localizer.GetString("VersificationLoadErrorType.InvalidSyntax", "Invalid syntax:"));
							break;
						case VersificationLoadErrorType.MissingName:
							sb.Append(Localizer.GetString("VersificationLoadErrorType.MissingName", "Versification file must contain a name line"));
							break;
						case VersificationLoadErrorType.NoSegmentsDefined:
							sb.Append(Localizer.GetString("VersificationLoadErrorType.NoSegmentsDefined", "No segments defined:"));
							break;
						case VersificationLoadErrorType.DuplicateExcludedVerse:
							sb.Append(Localizer.GetString("VersificationLoadErrorType.DuplicateExcludedVerse", "Duplicate excluded verse in line:"));
							break;
						case VersificationLoadErrorType.DuplicateSegment:
							sb.Append(Localizer.GetString("VersificationLoadErrorType.DuplicateSegment", "Duplicate verse segment in line:"));
							break;
						case VersificationLoadErrorType.InvalidManyToOneMap:
							sb.Append(Localizer.GetString("VersificationLoadErrorType.InvalidManyToOneMap", "Must map to or from a single verse in line:"));
							break;
						case VersificationLoadErrorType.UnspecifiedSegmentLocation:
							sb.Append(Localizer.GetString("VersificationLoadErrorType.UnspecifiedSegmentLocation", "Special '-' segment must be first segment in line:"));
							break;
						default:
							sb.Append(Localizer.GetString("VersificationLoadErrorType.UnexpectedErrorType", "Unexpected error type:"));
							break;
					}

					var singleError = errors[type].OnlyOrDefault();
					if (singleError != null)
					{
						sb.Append(" ");
						sb.Append(singleError);
					}
					else
					{
						foreach (var line in errors[type])
						{
							sb.Append(Environment.NewLine);
							sb.Append(" -- ");
							sb.Append(line.Truncate(50));
						}
					}

					sb.Append(Environment.NewLine);
				}

				return sb.ToString();
			}

			return null;
		}
	}
}
