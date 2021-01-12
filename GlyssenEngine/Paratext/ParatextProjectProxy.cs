// This class is nearly identical to the one in HearThis, though with a different namespace.
// If improvements are made here, they should also be made there if applicable.

using System.IO;
using Paratext.Data;
using SIL.DblBundle;
using SIL.DblBundle.Text;

namespace GlyssenEngine.Paratext
{
	public class ParatextProjectProxy : IProjectInfo
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

		/// <summary>
		/// Gets whether Paratext can find this project using Find (vs. FindById). This
		/// is almost always true, but if two local projects have the same (short) Name,
		/// then this will be false for one of them.
		/// </summary>
		/// <remarks>There are two possible ways to implement this: 1) Try ScrTextCollection.Find
		/// and if it returns null, then return false. (Of course, null can also be returned if the
		/// project can't be found at all, but since we have a ScrText object, that's presumably
		/// impossible.) 2) As follows. This relies on an implementation detail, but it's probably
		/// faster and (maybe?) more intuitive. A ScrText that must be loaded by ID will have a
		/// directory named: name.id</remarks>
		public bool CanBeFoundUsingShortName => Path.GetFileName(ScrText.Directory) == ScrText.Name;

		public string Name => CanBeFoundUsingShortName ? ScrText.Name :
			$"{ScrText.Name} ({ScrText.FullName})";

		public string Id => ScrText.Settings.DBLId ?? (CanBeFoundUsingShortName ? ScrText.Name : ScrText.Guid);

		public DblMetadataLanguage Language { get; }
	}
}
