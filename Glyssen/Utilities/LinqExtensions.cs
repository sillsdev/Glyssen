using System;
using System.Collections.Generic;
using System.Linq;

namespace Glyssen.Utilities
{
	public static class LinqExtensions
	{
		public static T OnlyOrDefault<T>(this IEnumerable<T> source, Func<T, bool> predicate) => OnlyOrDefault(source.Where(predicate));

		public static T OnlyOrDefault<T>(this IEnumerable<T> source)
		{
			bool any = false;
			T first = default;
			foreach (var t in source)
			{
				if (any)
					return default(T);
				first = t;
				any = true;
			}

			return first;
		}
	}
}
