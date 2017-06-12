using Glyssen.Shared.Bundle;

namespace Glyssen.Shared
{
	public interface IReferenceTextIdentifier
	{
		GlyssenDblTextMetadataBase Metadata { get; }
		bool Missing { get; }
		ReferenceTextType Type { get; }
		string ProjectFolder { get; }
	}
}