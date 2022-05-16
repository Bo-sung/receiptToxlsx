using System.Collections.Generic;
using System.Linq;

namespace Ragnarok
{
    public static class TreeViewExtensions
    {
        public static IOrderedEnumerable<T> Order<T, TKey>(this IEnumerable<T> source, System.Func<T, TKey> selector, bool ascending)
        {
            if (ascending)
                return source.OrderBy(selector);

            return source.OrderByDescending(selector);
        }

        public static IOrderedEnumerable<T> ThenBy<T, TKey>(this IOrderedEnumerable<T> source, System.Func<T, TKey> selector, bool ascending)
        {
            if (ascending)
                return source.ThenBy(selector);

            return source.ThenByDescending(selector);
        }
    }
}