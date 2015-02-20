using SIL.Windows.Forms.WritingSystems;

namespace ProtoScript
{
	public class ProjectMetadataViewModel
	{
		public ProjectMetadataViewModel(WritingSystemSetupModel wsModel)
		{
			WsModel = wsModel;
		}

		public WritingSystemSetupModel WsModel { get; private set; }
		public string LanguageName { get; set; }
		public string IsoCode { get; set; }
		public string ProjectName { get; set; }
		public string ProjectId { get; set; }
	}
}
