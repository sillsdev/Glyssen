using System;
using System.Collections.Generic;
using System.Text;

namespace Glyssen.Shared
{
	public interface IProject
	{
		string LanguageIsoCode { get; }
		/// <summary>
		/// Usually the same as the <see cref="LanguageIsoCode"/>. But if that is not 
		/// </summary>
		string ValidLanguageIsoCode { get; }
		string MetadataId { get; }
		string Name { get; }
	}
}
