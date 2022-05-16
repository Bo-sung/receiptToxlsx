using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;

namespace Ragnarok
{
    public class ObscuredLongEqualityComparer : IEqualityComparer<ObscuredLong>
    {
        static volatile ObscuredLongEqualityComparer defaultComparer;

        public static ObscuredLongEqualityComparer Default
        {
            get
            {
                ObscuredLongEqualityComparer comparer = defaultComparer;

                if (comparer == null)
                {
                    comparer = new ObscuredLongEqualityComparer();
                    defaultComparer = comparer;
                }

                return comparer;
            }
        }

        bool IEqualityComparer<ObscuredLong>.Equals(ObscuredLong x, ObscuredLong y)
        {
            return x.Equals(y);
        }

        int IEqualityComparer<ObscuredLong>.GetHashCode(ObscuredLong obj)
        {
            return obj.GetHashCode();
        }
    }
}