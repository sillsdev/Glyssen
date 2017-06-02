using Glyssen.Shared.Bundle;

namespace Glyssen.Shared
{
	public interface IReferenceTextIdentifier
	{
		GlyssenDblTextMetadata Metadata { get; }
		bool Missing { get; }
		ReferenceTextType Type { get; }
		string ProjectFolder { get; }
		string Name { get; }
	}
}