using System.Collections.Generic;
using System.Xml;
using Glyssen.Shared;
using Glyssen.Shared.Bundle;
using GlyssenEngine.Bundle;
using GlyssenEngine.Quote;
using SIL.DblBundle.Text;
using SIL.DblBundle.Usx;
using SIL.WritingSystems;

namespace GlyssenEngine
{
	public static class SampleProject
	{
		public const string kSample = "sample";
		private const string kSampleProjectName = "Sample Project";

		private class SampleProjectStub : IUserProject
		{
			public string Name => Project.GetDefaultRecordingProjectName(kSampleProjectName);
			public string LanguageIsoCode => kSample;
			public string ValidLanguageIsoCode => "qaa";
			public string MetadataId => kSample;
		}

		public static IUserProject Stub => new SampleProjectStub();

		public static void CreateSampleProjectIfNeeded()
		{
			if (ProjectBase.Reader.ResourceExists(Stub, ProjectResource.Metadata))
				return;
			var sampleMetadata = new GlyssenDblTextMetadata();

			sampleMetadata.AvailableBooks = new List<Book>();
			var bookOfMark = new Book();
			bookOfMark.Code = "MRK";
			bookOfMark.IncludeInScript = true;
			bookOfMark.LongName = "De Good Nyews Bout Jedus Christ Wa Mark Write";
			bookOfMark.ShortName = "Mark";
			bookOfMark.Abbreviation = "Mr";
			sampleMetadata.AvailableBooks.Add(bookOfMark);
			sampleMetadata.FontFamily = "Times New Roman";
			sampleMetadata.FontSizeInPoints = 12;
			sampleMetadata.Id = kSample;
			sampleMetadata.Copyright = Copyright;
			sampleMetadata.Promotion = Promotion;
			sampleMetadata.Language = new GlyssenDblMetadataLanguage { Iso = kSample };
			sampleMetadata.Identification = new DblMetadataIdentification { Name = kSampleProjectName, NameLocal = kSampleProjectName };
			sampleMetadata.ProjectStatus.ProjectSettingsStatus = ProjectSettingsStatus.Reviewed;
			sampleMetadata.ProjectStatus.QuoteSystemStatus = QuoteSystemStatus.Obtained;
			sampleMetadata.ProjectStatus.BookSelectionStatus = BookSelectionStatus.Reviewed;

			var sampleWs = new WritingSystemDefinition();
			sampleWs.QuotationMarks.AddRange(GetSampleQuoteSystem().AllLevels);

			XmlDocument sampleMark = new XmlDocument();
			sampleMark.LoadXml(Resources.SampleMRK);
			UsxDocument mark = new UsxDocument(sampleMark);

			new Project(sampleMetadata, new[] { mark }, SfmLoader.GetUsfmStylesheet(), sampleWs);
		}

		private static QuoteSystem GetSampleQuoteSystem()
		{
			QuoteSystem sampleQuoteSystem = new QuoteSystem();
			sampleQuoteSystem.AllLevels.Add(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal));
			sampleQuoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "“‘", 2, QuotationMarkingSystemType.Normal));
			sampleQuoteSystem.AllLevels.Add(new QuotationMark("“", "”", "“‘“", 3, QuotationMarkingSystemType.Normal));
			return sampleQuoteSystem;
		}

		private static DblMetadataCopyright Copyright
		{
			get
			{
				return new DblMetadataCopyright
				{
					Statement = new DblMetadataXhtmlContentNode
					{
						Xhtml = "© 2005, Wycliffe Bible Translators, Inc. All rights reserved."
					}
				};
			}
		}

		private static DblMetadataPromotion Promotion
		{
			get
			{
				DblMetadataPromotion promotion = new DblMetadataPromotion
				{
					PromoVersionInfo = new DblMetadataXhtmlContentNode
					{
						Xhtml =
@"<p>Sea Island Creole English [gul], USA</p>
<h2>Copyright Information</h2>
<p>© 2005, Wycliffe Bible Translators, Inc. All rights reserved.</p>
<p>This translation text is made available to you under the
terms of the Creative Commons License: Attribution-Noncommercial-No Derivative Works.
(<a href=""http://creativecommons.org/licenses/by-nc-nd/3.0/"">http://creativecommons.org/licenses/by-nc-nd/3.0/</a>)
In addition, you have permission to port the text to different file formats, as long as you
do not change any of the text or punctuation of the Bible.</p>
<p>You may share, copy, distribute, transmit, and extract portions
or quotations from this work, provided that you include the above copyright
information:</p>
<ul>
	<li>You must give Attribution to the work.</li>
	<li>You do not sell this work for a profit.</li>
	<li>You do not make any derivative works that change any of the actual words or punctuation of the Scriptures.</li>
</ul>
<p>Permissions beyond the scope of this license may be
available if you <a href=""mailto:ScriptureCopyrightPermissions_Intl@Wycliffe.org"">contact
us</a> with your request.</p>
<p><b>The New Testament</b><br />
in Sea Island Creole English</p>"
					}
				};
				return promotion;
			}
		}
	}
}
