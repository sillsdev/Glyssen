// ENHANCE: Move this to SIL.TestUtilities
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Constraints;

namespace GlyssenSharedTests
{
	public static class CustomConstraints
	{
		public static AllItemsMatchConstraint<T> ForEvery<T>(IConstraint itemConstraint) =>
			new AllItemsMatchConstraint<T>(itemConstraint);

		/// <summary>
		/// Note: You cannot use this with negated constraints.
		/// </summary>
		public static AllItemsMatchConstraint<T> ForEvery<T>(Func<T, object> propertySelector,
			IConstraint innerConstraint, string valueName = null)
		{
			return new AllItemsMatchConstraint<T>(new LambdaConstraint<T>(propertySelector,
				innerConstraint, valueName));
		}

		public static AllItemsMatchConstraint<T> No<T>(Func<T, object> propertySelector,
			IConstraint innerConstraintToNegate, string valueName = null)
		{
			return new AllItemsMatchConstraint<T>(new LambdaConstraint<T>(propertySelector,
				innerConstraintToNegate, valueName), true);
		}

		public static Constraint ForEvery<T>(params (Func<T, object> selector, IConstraint constraint, string valueName)[] checks)
		{
			return new AllItemsMatchConstraint<T>(new CompositeConstraint<T>(checks));
		}

		private class LambdaConstraint<T> : Constraint
		{
			private readonly Func<T, object> m_selector;
			private readonly IConstraint m_inner;
			private readonly string m_valueName;

			public LambdaConstraint(Func<T, object> selector, IConstraint inner, string valueName)
			{
				m_selector = selector ?? throw new ArgumentNullException(nameof(selector));
				m_inner = inner ?? throw new ArgumentNullException(nameof(inner));
				m_valueName = valueName;
			}

			public override ConstraintResult ApplyTo<TActual>(TActual actual)
			{
				var value = m_selector((T)(object)actual); // unbox safely
				var result = m_inner.ApplyTo(value);
				return new ConstraintResult(this, value, result.IsSuccess);
			}

			public override string Description => $"{m_valueName ?? "value"} {m_inner.Description}";
		}
	}

	public class AllItemsMatchConstraint<T> : Constraint
	{
		private readonly IConstraint m_itemConstraint;
		private readonly bool m_negate;

		public AllItemsMatchConstraint(IConstraint itemConstraint, bool negate = false)
		{
			m_itemConstraint = itemConstraint ?? throw new ArgumentNullException(nameof(itemConstraint));
			m_negate = negate;
		}

		public override ConstraintResult ApplyTo<TActual>(TActual actual)
		{
			if (!(actual is IEnumerable<T> enumerable))
				throw new ArgumentException($"Actual value must be IEnumerable<{typeof(T).Name}>", nameof(actual));

			var list = actual as IList<T> ?? enumerable.ToList();

			for (var i = 0; i < list.Count; i++)
			{
				var item = list[i];
				var result = m_itemConstraint.ApplyTo(item);
				if (m_negate ? result.IsSuccess : !result.IsSuccess)
					return new ConstraintResult(this, $"[{i}] {item}", false);
			}

			return new ConstraintResult(this, actual, true);
		}

		public override string Description =>
			$"{(m_negate ? "no" : "all")} items to have: {m_itemConstraint.Description}";
	}

	public class CompositeConstraint<T> : Constraint
	{
		private readonly (Func<T, object> selector, IConstraint constraint, string valueName)[] m_checks;

		public CompositeConstraint((Func<T, object>, IConstraint, string)[] checks)
		{
			m_checks = checks;
		}

		public override ConstraintResult ApplyTo<TActual>(TActual actual)
		{
			var value = (T)(object)actual;
			foreach (var (selector, constraint, _) in m_checks)
			{
				var selectedValue = selector(value);
				var result = constraint.ApplyTo(selectedValue);
				if (!result.IsSuccess)
				{
					return new ConstraintResult(this, value, isSuccess: false);
				}
			}
			return new ConstraintResult(this, value, isSuccess: true);
		}

		public override string Description
		{
			get
			{
				var parts = m_checks.Select(c => $"{c.valueName ?? "value"} {c.constraint.Description}");
				return "all of: " + string.Join(", ", parts);
			}
		}
	}
}
