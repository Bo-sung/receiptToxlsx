using System.Collections.Generic;

namespace Ragnarok
{
    public class FloatEqualityComparer : IEqualityComparer<float>
    {
        static volatile FloatEqualityComparer defaultComparer;

        public static FloatEqualityComparer Default
        {
            get
            {
                FloatEqualityComparer comparer = defaultComparer;

                if (comparer == null)
                {
                    comparer = new FloatEqualityComparer();
                    defaultComparer = comparer;
                }

                return comparer;
            }
        }

        bool IEqualityComparer<float>.Equals(float x, float y)
        {
            return x == y;
        }

        int IEqualityComparer<float>.GetHashCode(float obj)
        {
            return obj.GetHashCode();
        }
    }
}