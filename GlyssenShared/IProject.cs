namespace Glyssen.Shared
{
	public interface IProject
	{
		string Name { get; }
	}

	public interface IUserProject : IProject
	{
		/// <summary>
		/// A language tag (typically conforming to BCP-47) used to help identify the target
		/// (i.e., vernacular) language of the recording project. This can be a more fully
		/// specified tag consisting of a sequence of "subtags", but most often just a three-letter
		/// language subtag.
		/// </summary>
		string LanguageIsoCode { get; }
		/// <summary>
		/// Usually the same as the <see cref="LanguageIsoCode"/>. But if that is not a valid
		/// IETF BCP-47 language tag (i.e., that can be looked up in the WS repository), this will return
		/// a standard subtag to indicate that it is an unlisted language.
		/// </summary>
		string ValidLanguageIsoCode { get; }
		/// <summary>
		/// The "publication ID" (typically a 16-digit hexadecimal number) associated with the
		/// publication that serves as the basis for the recording project.
		/// </summary>
		string MetadataId { get; }
		/// <summary>
		/// Name of the font family used for vernacular project data
		/// </summary>
		string FontFamily { get; }
	}

	public interface IReferenceTextProject : IProject
	{
		ReferenceTextType Type { get; }
	}
}
