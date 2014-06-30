using System;
using System.Collections.Generic;
using System.Linq;

namespace DanTup.DartVS
{
	static class EnumerableExtensions
	{
		public static IEnumerable<T> Flatten<T>(this IEnumerable<T> e, Func<T, IEnumerable<T>> childSelector)
		{
			if (e == null)
				return null;

			return e.SelectMany(root => Flatten(root, childSelector));
		}

		static IEnumerable<T> Flatten<T>(T e, Func<T, IEnumerable<T>> childSelector)
		{
			var parent = new[] { e };
			var children = childSelector(e);

			if (children == null)
				return parent;

			return parent.Concat(children);
		}
	}
}
