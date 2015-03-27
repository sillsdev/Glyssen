using System.Linq;
using SIL.Windows.Forms.WritingSystems;

namespace ProtoScript
{
	public class ProjectMetadataViewModel
	{
		public ProjectMetadataViewModel(WritingSystemSetupModel wsModel)
		{
			WsModel = wsModel;
		}

		public ProjectMetadataViewModel(Project project)
		{
			WsModel = new WritingSystemSetupModel(project.WritingSystem)
			{
				CurrentDefaultFontName = project.FontFamily,
				CurrentDefaultFontSize = project.FontSizeInPoints,
				CurrentRightToLeftScript = project.RightToLeftScript
			};

			LanguageName = project.LanguageName;
			IsoCode = project.LanguageIsoCode;
			PublicationId = project.Id;
			PublicationName = project.PublicationName;
			VersificationFilePath = project.VersificationFilePath;
			VersificationName = project.VersificationName;

			var block = project.IncludedBooks.SelectMany(book => book.GetScriptBlocks().Where(b => b.BlockElements.OfType<Verse>().Any()))
					.FirstOrDefault();
			if (block != null)
				SampleText = block.GetText(false);
		}

		public WritingSystemSetupModel WsModel { get; private set; }
		public string LanguageName { get; set; }
		public string IsoCode { get; set; }
		public string PublicationName { get; set; }
		public string RecordingProjectName { get; set; }
		public string PublicationId { get; set; }
		public string SampleText { get; set; }
		public string VersificationFilePath { get; set; }
		public string VersificationName { get; set; }
	}
}
