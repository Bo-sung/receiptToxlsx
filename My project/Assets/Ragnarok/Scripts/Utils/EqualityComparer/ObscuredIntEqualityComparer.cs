using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;

namespace Ragnarok
{
    public class ObscuredIntEqualityComparer : IEqualityComparer<ObscuredInt>
    {
        static volatile ObscuredIntEqualityComparer defaultComparer;

        public static ObscuredIntEqualityComparer Default
        {
            get
            {
                ObscuredIntEqualityComparer comparer = defaultComparer;

                if (comparer == null)
                {
                    comparer = new ObscuredIntEqualityComparer();
                    defaultComparer = comparer;
                }

                return comparer;
            }
        }

        bool IEqualityComparer<ObscuredInt>.Equals(ObscuredInt x, ObscuredInt y)
        {
            return x.Equals(y);
        }

        int IEqualityComparer<ObscuredInt>.GetHashCode(ObscuredInt obj)
        {
            return obj.GetHashCode();
        }
    }
}