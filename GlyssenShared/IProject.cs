using System;
using System.Collections.Generic;
using System.Text;

namespace Glyssen.Shared
{
	public interface IProject
	{
		string LanguageIsoCode { get; }
		string MetadataId { get; }
		string Name { get; }
	}
}
