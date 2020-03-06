using Glyssen.Shared.Bundle;

namespace Glyssen.Shared
{
	/// <summary>
	/// Unfortunately, this interface was created out of refactoring necessity rather than an actual
	/// semantic notion. It simply wraps the bits of ReferenceTextProxy which are needed outside of the Glyssen assembly.
	/// </summary>
	public interface IReferenceTextProxy
	{
		GlyssenDblTextMetadataBase Metadata { get; }
		bool Missing { get; }
		ReferenceTextType Type { get; }
		string Name { get; }
	}
}