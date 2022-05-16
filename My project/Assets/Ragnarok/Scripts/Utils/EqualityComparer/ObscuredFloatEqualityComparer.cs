using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;

namespace Ragnarok
{
    public class ObscuredFloatEqualityComparer : IEqualityComparer<ObscuredFloat>
    {
        static volatile ObscuredFloatEqualityComparer defaultComparer;

        public static ObscuredFloatEqualityComparer Default
        {
            get
            {
                ObscuredFloatEqualityComparer comparer = defaultComparer;

                if (comparer == null)
                {
                    comparer = new ObscuredFloatEqualityComparer();
                    defaultComparer = comparer;
                }

                return comparer;
            }
        }

        bool IEqualityComparer<ObscuredFloat>.Equals(ObscuredFloat x, ObscuredFloat y)
        {
            return x.Equals(y);
        }

        int IEqualityComparer<ObscuredFloat>.GetHashCode(ObscuredFloat obj)
        {
            return obj.GetHashCode();
        }
    }
}