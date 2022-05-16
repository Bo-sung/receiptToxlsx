using System.Collections.Generic;

namespace Ragnarok
{
    public class IntEqualityComparer : IEqualityComparer<int>
    {
        static volatile IntEqualityComparer defaultComparer;

        public static IntEqualityComparer Default
        {
            get
            {
                IntEqualityComparer comparer = defaultComparer;

                if (comparer == null)
                {
                    comparer = new IntEqualityComparer();
                    defaultComparer = comparer;
                }

                return comparer;
            }
        }

        bool IEqualityComparer<int>.Equals(int x, int y)
        {
            return x == y;
        }

        int IEqualityComparer<int>.GetHashCode(int obj)
        {
            return obj.GetHashCode();
        }
    }
}