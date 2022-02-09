using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glyssen
{
	/// <summary>
	/// Interface for windows/controls that need to do something special when on-the-fly
	/// localization occurs.
	/// </summary>
	public interface ILocalizable
	{
		/// <summary>
		/// Implement this to save localized format strings, reformat formatted strings displayed
		/// in the UI, and/or repopulate UI elements with dynamic localized strings.
		/// </summary>
		void HandleStringsLocalized();
	}
}
