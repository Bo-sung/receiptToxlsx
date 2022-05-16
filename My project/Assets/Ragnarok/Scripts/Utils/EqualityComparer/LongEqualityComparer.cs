using System.Collections.Generic;

namespace Ragnarok
{
    public class LongEqualityComparer : IEqualityComparer<long>
    {
        static volatile LongEqualityComparer defaultComparer;

        public static LongEqualityComparer Default
        {
            get
            {
                LongEqualityComparer comparer = defaultComparer;

                if (comparer == null)
                {
                    comparer = new LongEqualityComparer();
                    defaultComparer = comparer;
                }

                return comparer;
            }
        }

        bool IEqualityComparer<long>.Equals(long x, long y)
        {
            return x == y;
        }

        int IEqualityComparer<long>.GetHashCode(long obj)
        {
            return obj.GetHashCode();
        }
    }
}