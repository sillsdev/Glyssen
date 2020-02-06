namespace GlyssenEngine
{
	public interface IReferenceLanguageInfo
	{
		bool HasSecondaryReferenceText { get; }
		IReferenceLanguageInfo BackingReferenceLanguage { get; }
		string HeSaidText { get; }
		string WordSeparator { get; }
	}
}
