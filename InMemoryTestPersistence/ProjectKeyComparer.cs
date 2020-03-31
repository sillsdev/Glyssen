using System;
using System.Collections.Generic;
using Glyssen.Shared;

namespace InMemoryTestPersistence
{
	/// <summary>
	/// Allows projects to be compared for "key" equality (as opposed to actual object equality)
	/// taking into consideration both the type of project (reference text vs. user project) and
	/// the properties that would determine the project folder path (in a file-based persistence
	/// world).
	/// </summary>
	class ProjectKeyComparer : IEqualityComparer<IProject>
	{
		public bool Equals(IProject x, IProject y)
		{
			if (x == y)
				return true;
			if (x == null || y == null)
				return false;
			switch (x)
			{
				case IReferenceTextProject refTextX:
					if (y is IReferenceTextProject refTextY)
						return refTextX.Name.Equals(refTextY.Name) && refTextY.Type.Equals(refTextY.Type);
					else
						return false;
				case IUserProject userProjectX:
					if (y is IUserProject userProjectY)
					{
						return userProjectX.LanguageIsoCode.Equals(userProjectY.LanguageIsoCode) &&
							userProjectX.MetadataId.Equals(userProjectY.MetadataId) &&
							userProjectX.Name.Equals(userProjectY.Name);
					}
					else
						return false;
				default:
					throw new ArgumentException("Unexpected project type", nameof(x));
			}
		}

		public int GetHashCode(IProject obj)
		{
			unchecked
			{
				int hashCode = obj.Name.GetHashCode();
				switch (obj)
				{
					case IReferenceTextProject refText:
						hashCode = (hashCode * 397) ^ refText.Type.GetHashCode();
						break;
					case IUserProject userProject:
						hashCode = (hashCode * 397) ^ userProject.LanguageIsoCode.GetHashCode();
						hashCode = (hashCode * 397) ^ userProject.MetadataId.GetHashCode();
						break;
				}

				return hashCode;
			}
		}
	}
}
