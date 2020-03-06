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
		/// Usually the same as the <see cref="LanguageIsoCode"/>. But if that is not 
		/// </summary>
		string ValidLanguageIsoCode { get; }
		string MetadataId { get; }
	}

	public interface IReferenceTextProject : IProject
	{
		ReferenceTextType Type { get; }
	}
}
