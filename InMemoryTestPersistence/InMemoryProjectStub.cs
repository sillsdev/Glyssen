using Glyssen.Shared;

namespace InMemoryTestPersistence
{
	internal class InMemoryProjectStub : IUserProject
	{
		public string Name { get; }
		public string LanguageIsoCode { get; }
		public string ValidLanguageIsoCode => LanguageIsoCode;
		public string MetadataId { get; }

		internal InMemoryProjectStub(string languageIsoCode, string metadataId, string name)
		{
			LanguageIsoCode = languageIsoCode;
			MetadataId = metadataId;
			Name = name;
		}
	}
}
