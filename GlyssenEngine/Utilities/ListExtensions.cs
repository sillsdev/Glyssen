using System;
using System.Collections.Generic;
using System.Linq;

namespace GlyssenEngine.Utilities
{
	public static class ListExtensions
	{
		/// <summary>
		/// Merges two sorted lists containing potentially different types of objects, resulting in a single
		/// sorted list of objects of type T with no duplicates.
		/// </summary>
		public static void MergeOrderedList<TMe, TOther>(this List<TMe> me, IReadOnlyList<TOther> other, Func<TMe, TOther, int> compare = null, Func<TOther, TMe> selectT = null)
		{
			if (other == null)
				throw new ArgumentNullException(nameof(other));
			if (compare == null)
			{
				if (typeof(TMe).GetInterfaces().Any(i => i == typeof(IComparable<TOther>)))
				{
					compare = (a, b) => ((IComparable<TOther>)a).CompareTo(b);
				}
				else
				{
					throw new ArgumentNullException(nameof(compare),
						"A comparison method must be supplied if no default comparison exists.");
				}
			}

			if (selectT == null)
				if (typeof(TMe).IsAssignableFrom(typeof(TOther)))
				{
					selectT = o => (TMe)(o as object);
				}
				else
				{
					throw new ArgumentNullException(nameof(selectT),
						$"A selection method must be supplied if the items in the other list cannot be assigned to the type of the items in \"{nameof(me)}\"");
				}

			if (me.Count == 0)
			{
				me.AddRange(other.Select(selectT));
				return;
			}

			for (int o = 0, m = 0; o < other.Count; o++)
			{
				var currentOther = other[o];
				while (compare(me[m], currentOther) < 0 && ++m < me.Count) {}

				if (m == me.Count)
				{
					me.AddRange(other.Skip(o).Select(selectT));
					break;
				}

				if (compare(me[m], currentOther) != 0)
					me.Insert(m, selectT(currentOther));
			}
		}
	}
}
