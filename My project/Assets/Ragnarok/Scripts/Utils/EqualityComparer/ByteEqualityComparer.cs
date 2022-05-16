using System.Collections.Generic;

namespace Ragnarok
{
    public class ByteEqualityComparer : IEqualityComparer<byte>
    {
        static volatile ByteEqualityComparer defaultComparer;

        public static ByteEqualityComparer Default
        {
            get
            {
                ByteEqualityComparer comparer = defaultComparer;

                if (comparer == null)
                {
                    comparer = new ByteEqualityComparer();
                    defaultComparer = comparer;
                }

                return comparer;
            }
        }

        bool IEqualityComparer<byte>.Equals(byte x, byte y)
        {
            return x == y;
        }

        int IEqualityComparer<byte>.GetHashCode(byte obj)
        {
            return obj.GetHashCode();
        }
    }
}