// This class is nearly identical to the one in HearThis, though with a different namespace.
// If improvements are made here, they should also be made there if applicable.

using Paratext.Data;
using SIL.DblBundle;
using SIL.DblBundle.Text;

namespace GlyssenEngine.Paratext
{
	internal class ParatextProjectProxy : IProjectInfo
	{
		public ParatextProjectProxy(ScrText scrText)
		{
			ScrText = scrText;
			Language = new DblMetadataLanguage();
			Language.Iso = scrText.Settings.LanguageID.Id;
			Language.Name = scrText.DisplayLanguageName;
			Language.Script = scrText.Language.FontName;
			Language.ScriptDirection = scrText.RightToLeft ? "RTL" : "LTR";
		}

		private ScrText ScrText { get; }

		public string Name => ScrText.Name;

		public string Id => ScrText.Settings.DBLId ?? ScrText.Name;

		public DblMetadataLanguage Language { get; }
	}
}
