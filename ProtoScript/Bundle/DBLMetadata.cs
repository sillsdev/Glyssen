using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Paratext;
using ProtoScript.Dialogs;
using ProtoScript.Properties;
using ProtoScript.Quote;
using SIL.Xml;

namespace ProtoScript.Bundle
{
	[XmlRoot("DBLMetadata")]
	public class DblMetadataBase
	{
		[XmlAttribute] public string id;
		[XmlAttribute] public string type;
		[XmlAttribute] public string typeVersion;

		public bool IsTextReleaseBundle { get { return type == "text"; } }
	}

	[XmlRoot("DBLMetadata")]
	public class DblMetadata : DblMetadataBase
	{
		///// <summary>This is only needed as a temporary place to store the recording project name when the user metadata
		///// is created for a project that does not have a bundle - this can go away if we stop supporting those.</summary>
		//[XmlIgnore]
		//public string PgRecordingProjectName;

		/// <summary>This is not part of the original DBL metadata. We add this when we parse the USX to create
		/// a script. If significant changes to the parser are made and the parser version in the program does
		/// not match the stored parser version, then we know to re-parse the original USX data.</summary>
		[XmlAttribute("usxparserversion")]
		public string PgUsxParserVersion;

		/// <summary>This is not part of the original DBL metadata. We add this when we parse the USX to create
		/// a script. This tells us the original (local) path of the DBL file used to create this project.</summary>
		[XmlAttribute("origdblpath")]
		public string OriginalPathOfDblFile;

		/// <summary>This is not part of the original DBL metadata. We add this when we parse an SFM file to create
		/// a script. This tells us the original (local) path of the SFM file used to create this project.</summary>
		[XmlAttribute("origsfmfile")]
		public string OriginalPathOfSfmFile;

		/// <summary>This is not part of the original DBL metadata. We add this when we parse a directory of SFM files to create
		/// a script. This tells us the original (local) path of the directory used to create this project.</summary>
		[XmlAttribute("origsfmdir")]
		public string OriginalPathOfSfmDirectory;

		/// <summary>
		/// This is not part of the original DBL metadata. 
		/// We use this to know if character assignments should be reprocessed.
		/// </summary>
		[XmlAttribute("controlfileversion")]
		public int ControlFileVersion;

		/// <summary>
		/// This is not part of the original DBL metadata.
		/// If true, the project is hidden from the user by default (usually hidden by the user)
		/// </summary>
		[XmlAttribute("hiddenbydefault")]
		[DefaultValue(false)]
		public bool HiddenByDefault;

		private QuoteSystem m_quoteSystem;
		/// <summary>
		/// This is not part of the original DBL metadata. This data is now stored in LDML.
		/// </summary>
		[XmlElement("QuoteSystem")]
		public QuoteSystem QuoteSystem_DeprecatedXml
		{
			get { return null; }
			set { m_quoteSystem = value; }
		}

		[XmlIgnore]
		public QuoteSystem QuoteSystem
		{
			get { return m_quoteSystem; }
			set { m_quoteSystem = value; }
		}

		/// <summary>
		/// This is not part of the original DBL metadata.
		/// </summary>
		[XmlElement("projectStatus")]
		public ProjectStatus ProjectStatus = new ProjectStatus();

		[XmlElement("isQuoteSystemUserConfirmed")]
		[DefaultValue(false)]
		public bool IsQuoteSystemUserConfirmed;

		/// <summary>
		/// This is not part of the original DBL metadata. This data is now stored as part of the "language" data. 
		/// </summary>
		[XmlElement("fontFamily")]
		public string FontFamily_DeprecatedXml
		{
			get { return null; }
			set { FontFamily = value; }
		}

		/// <summary>
		/// The font family for the language associated with this project.
		/// </summary>
		[XmlIgnore]
		public string FontFamily
		{
			get { return language == null ? m_fontFamilyTemp : language.FontFamily; }
			set
			{
				if (language == null)
					m_fontFamilyTemp = value;
				else
					language.FontFamily = value;
			}
		}

		/// <summary>
		/// This is not part of the original DBL metadata. This data is now stored as part of the "langauge" data. 
		/// </summary>
		[XmlElement("fontSizeInPoints")]
		[DefaultValue(default(int))]
		public int FontSizeInPoints_DeprecatedXml
		{
			get { return default(int); }
			set { FontSizeInPoints = value; }
		}

		/// <summary>
		/// This is not part of the original DBL metadata but rather is pulled in from the stylesheet
		/// or set via the ProjectMetadataDlg.
		/// </summary>
		[XmlIgnore]
		public int FontSizeInPoints
		{
			get  { return language == null ? m_fontSizeInPointsTemp : language.FontSizeInPoints; }
			set
			{
				if (language == null)
					m_fontSizeInPointsTemp = value;
				else
					language.FontSizeInPoints = value;
			}
		}

		[XmlIgnore]
		public int FontSizeUiAdjustment
		{
			get { return language == null ? 0 : language.FontSizeUiAdjustment; }
			set { language.FontSizeUiAdjustment = value; }
		}

		/// <summary>This is not part of the original DBL metadata. If a project does not come with
		/// a versification file, this is the name of the standard versification to be used.</summary>
		[XmlAttribute("versification")]
		[DefaultValue("English")]
		public string Versification;

		private int m_fontSizeInPointsTemp;
		private string m_fontFamilyTemp;
		private DblMetadataLanguage m_language;

		public DblMetadataIdentification identification;
		public DblMetadataLanguage language
		{
			get { return m_language; }
			set
			{
				m_language = value;
				if (m_fontFamilyTemp != default(string))
				{
					m_language.FontFamily = m_fontFamilyTemp;
					m_fontFamilyTemp = default(string);
				}
				if (m_fontSizeInPointsTemp != default(int))
				{
					m_language.FontSizeInPoints = m_fontSizeInPointsTemp;
					m_fontSizeInPointsTemp = default(int);
				}
			}
		}
		public DblMetadataPromotion promotion;
		public DblMetadataArchiveStatus archiveStatus;
		[XmlArray("bookNames")]
		[XmlArrayItem("book")]
		public List<Book> AvailableBooks { get; set; }

		public string GetAsXml()
		{
			return XmlSerializationHelper.SerializeToString(this);
		}

		public static DblMetadata Load(string projectFilePath, out Exception exception)
		{
			var metadata = XmlSerializationHelper.DeserializeFromFile<DblMetadata>(projectFilePath, out exception);
			if (metadata.language == null && (metadata.m_fontFamilyTemp != default(string) || metadata.m_fontSizeInPointsTemp != default(int)))
				metadata.language = new DblMetadataLanguage();
			return metadata;
		}

		public override string ToString()
		{
			if (language.iso == "sample")
				return id;
			string languagePart;
			if (string.IsNullOrEmpty(language.name))
				languagePart = language.iso;
			else
				languagePart = string.Format("{0} ({1})", language.name, language.iso);

			string identificationPart;
			if (identification == null)
				identificationPart = id;
			else
			{
				if (identification.nameLocal == identification.name)
					identificationPart = identification.nameLocal;
				else
					identificationPart = String.Format("{0} ({1})", identification.nameLocal, identification.name);
			}

			return String.Format("{0} - {1}", languagePart, identificationPart);
		}
	}

	public class ProjectStatus
	{
		[XmlElement("assignCharacterBlock")]
		public BookBlockIndices AssignCharacterBlock;
		[XmlElement("assignCharacterMode")]
		public BlocksToDisplay AssignCharacterMode;
	}

	public class DblMetadataIdentification
	{
		public string name;
		public string nameLocal;

		[XmlElement("systemId")]
		public HashSet<DblMetadataSystemId> systemIds;
	}

	public class DblMetadataLanguage
	{
		private int m_fontSizeInPoints;

		public string iso;
		public string name;
		public string ldml;
		public string rod;
		public string script;
		[DefaultValue("LTR")]
		public string scriptDirection;
		public string numerals;

		/// <summary>
		/// This is not part of the original language metadata but rather is pulled in from the stylesheet
		/// or set via the ProjectMetadataDlg.
		/// </summary>
		[XmlElement("fontFamily")]
		public string FontFamily { get; set; }

		/// <summary>
		/// This is not part of the original language metadata but rather is pulled in from the stylesheet
		/// or set via the ProjectMetadataDlg.
		/// </summary>
		[XmlElement("fontSizeInPoints")]
		public int FontSizeInPoints
		{
			get { return m_fontSizeInPoints == 0 ? Settings.Default.DefaultFontSize : m_fontSizeInPoints; }
			set { m_fontSizeInPoints = value; }
		}

		/// <summary>
		/// This is not part of the original language metadata.
		/// </summary>
		[XmlElement("fontSizeUiAdjustment")]
		[DefaultValue(0)]
		public int FontSizeUiAdjustment { get; set; }

		public override string ToString()
		{
			return iso;
		}
	}

	public class DblMetadataPromotion
	{
		[XmlElement("promoVersionInfo")]
		public DblMetadataXhtmlContentNode promoVersionInfo;

		[XmlElement("promoEmail")]
		public DblMetadataXhtmlContentNode promoEmail;
	}

	public class DblMetadataXhtmlContentNode
	{
		private string m_value;

		public DblMetadataXhtmlContentNode()
		{
			contentType = "xhtml";
		}

		[XmlAttribute]
		public string contentType;

		[XmlAnyElement]
		public XmlElement[] InternalNodes { get; set; }

		[XmlIgnore]
		public string value
		{
			get
			{
				if (m_value == null)
				{
					var sb = new StringBuilder();
					foreach (var node in InternalNodes)
						sb.Append(node.OuterXml);
					m_value = sb.ToString();
				}
				return m_value;
			}
			set
			{
				m_value = value;
				var doc = new XmlDocument();
				doc.LoadXml(value);
				InternalNodes = new[] { doc.DocumentElement };
			}
		}
	}

	public class DblMetadataSystemId
	{
		[XmlAttribute]
		public string type;

		[XmlText]
		public string value;
	}

	public class DblMetadataArchiveStatus
	{
		public string dateArchived;
		public string dateUpdated;
	}

	public class Book
	{
		public Book()
		{
			IncludeInScript = true;
		}

		[XmlAttribute("code")]
		public string Code { get; set; }

		[XmlAttribute("include")]
		[DefaultValue(true)]
		public bool IncludeInScript { get; set; }

		[XmlElement("long")]
		public string LongName { get; set; }

		[XmlElement("short")]
		public string ShortName { get; set; }

		[XmlElement("abbr")]
		public string Abbreviation { get; set; }
	}
}
