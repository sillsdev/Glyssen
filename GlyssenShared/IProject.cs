namespace Glyssen.Shared
{
	public interface IProject
	{
		string Name { get; }
	}

	public interface IUserProject : IProject
	{
		string LanguageIsoCode { get; }
		/// <summary>
		/// Usually the same as the <see cref="LanguageIsoCode"/>. But if that is not a valid
		/// IETF language tag (i.e., that can be looked up in the WS repository), this will return
		/// a standard subtag to indicate that it is an unlisted language.
		/// </summary>
		string ValidLanguageIsoCode { get; }
		string MetadataId { get; }
	}

	public interface IReferenceTextProject : IProject
	{
		ReferenceTextType Type { get; }
	}
}
