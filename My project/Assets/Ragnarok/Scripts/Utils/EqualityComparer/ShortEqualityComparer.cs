using System.Collections.Generic;

namespace Ragnarok
{
    public class ShortEqualityComparer : IEqualityComparer<short>
    {
        static volatile ShortEqualityComparer defaultComparer;

        public static ShortEqualityComparer Default
        {
            get
            {
                ShortEqualityComparer comparer = defaultComparer;

                if (comparer == null)
                {
                    comparer = new ShortEqualityComparer();
                    defaultComparer = comparer;
                }

                return comparer;
            }
        }

        bool IEqualityComparer<short>.Equals(short x, short y)
        {
            return x == y;
        }

        int IEqualityComparer<short>.GetHashCode(short obj)
        {
            return obj.GetHashCode();
        }
    }
}