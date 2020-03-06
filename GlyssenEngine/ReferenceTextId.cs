using System.Diagnostics;
using Glyssen.Shared;

namespace GlyssenEngine
{
	class ReferenceTextId : IReferenceTextProject
	{
		public ReferenceTextId(ReferenceTextType type, string name = null)
		{
			Type = type;
			if (type.IsStandard())
			{
				Debug.Assert(name == null);
				Name = type.ToString();
			}
			else
			{
				Debug.Assert(name != null);
				Name = name;
			}
		}

		public ReferenceTextType Type { get; }
		public string Name { get; }
	}
}
