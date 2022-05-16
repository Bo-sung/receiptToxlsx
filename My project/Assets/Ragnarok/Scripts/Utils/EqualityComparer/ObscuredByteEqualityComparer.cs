using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;

namespace Ragnarok
{
    public class ObscuredByteEqualityComparer : IEqualityComparer<ObscuredByte>
    {
        static volatile ObscuredByteEqualityComparer defaultComparer;

        public static ObscuredByteEqualityComparer Default
        {
            get
            {
                ObscuredByteEqualityComparer comparer = defaultComparer;

                if (comparer == null)
                {
                    comparer = new ObscuredByteEqualityComparer();
                    defaultComparer = comparer;
                }

                return comparer;
            }
        }

        bool IEqualityComparer<ObscuredByte>.Equals(ObscuredByte x, ObscuredByte y)
        {
            return x.Equals(y);
        }

        int IEqualityComparer<ObscuredByte>.GetHashCode(ObscuredByte obj)
        {
            return obj.GetHashCode();
        }
    }
}