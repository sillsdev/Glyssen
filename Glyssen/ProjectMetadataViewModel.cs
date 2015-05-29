using System.Linq;
using Paratext;
using SIL.Windows.Forms.WritingSystems;

namespace Glyssen
{
	public class ProjectMetadataViewModel
	{
		public ProjectMetadataViewModel(WritingSystemSetupModel wsModel)
		{
			WsModel = wsModel;
		}

		public ProjectMetadataViewModel(Project project)
		{
			Project = project;

			WsModel = new WritingSystemSetupModel(project.WritingSystem)
			{
				CurrentDefaultFontName = project.FontFamily,
				CurrentDefaultFontSize = project.FontSizeInPoints,
				CurrentRightToLeftScript = project.RightToLeftScript
			};

			RecordingProjectName = project.Name;
			LanguageName = project.LanguageName;
			IsoCode = project.LanguageIsoCode;
			PublicationId = project.Id;
			PublicationName = project.PublicationName;
			Versification = project.Versification;

			var block = project.IncludedBooks.SelectMany(book => book.GetScriptBlocks().Where(b => b.BlockElements.OfType<Verse>().Any()))
					.FirstOrDefault();
			if (block != null)
				SampleText = block.GetText(false);
		}

		public Project Project { get; private set; }
		public WritingSystemSetupModel WsModel { get; private set; }
		public string LanguageName { get; set; }
		public string IsoCode { get; set; }
		public string PublicationName { get; set; }
		public string RecordingProjectName { get; set; }
		public string PublicationId { get; set; }
		public string SampleText { get; set; }
		public ScrVers Versification { get; set; }
	}
}
