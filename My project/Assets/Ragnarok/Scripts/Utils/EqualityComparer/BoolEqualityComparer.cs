using System.Collections.Generic;

namespace Ragnarok
{
    public class BoolEqualityComparer : IEqualityComparer<bool>
    {
        static volatile BoolEqualityComparer defaultComparer;

        public static BoolEqualityComparer Default
        {
            get
            {
                BoolEqualityComparer comparer = defaultComparer;

                if (comparer == null)
                {
                    comparer = new BoolEqualityComparer();
                    defaultComparer = comparer;
                }

                return comparer;
            }
        }

        bool IEqualityComparer<bool>.Equals(bool x, bool y)
        {
            return x == y;
        }

        int IEqualityComparer<bool>.GetHashCode(bool obj)
        {
            return obj.GetHashCode();
        }
    }
}