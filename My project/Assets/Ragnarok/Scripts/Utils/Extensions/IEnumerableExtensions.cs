using System.Collections.Generic;
using System.Linq;

namespace Ragnarok
{
    public static class IEnumerableExtensions
    {
        static System.Random random = new System.Random();

        public static IEnumerable<T> OrEmptyIfNull<T>(this IEnumerable<T> source)
        {
            return source ?? Enumerable.Empty<T>();
        }

        public static T GetRandomPick<T>(this IEnumerable<T> source)
        {
            int rnd = random.Next(source.Count());
            return source.ElementAt(rnd);
        }

        public static T GetRandomPick<T>(this IEnumerable<T> source, IEnumerable<T> excepts)
        {
            var except = source.Except(excepts);
            int rnd = random.Next(except.Count());
            return except.ElementAt(rnd);
        }

        public static T[] GetRandomPick<T>(this IEnumerable<T> source, int count)
        {
            count = UnityEngine.Mathf.Min(source.Count(), count);

            var clone = source.ToArray();
            clone.Shuffle();
            return clone.Take(count).ToArray();
        }
    }
}
