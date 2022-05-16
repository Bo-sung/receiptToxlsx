using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;

namespace Ragnarok
{
    public class ObscuredBoolEqualityComparer : IEqualityComparer<ObscuredBool>
    {
        static volatile ObscuredBoolEqualityComparer defaultComparer;

        public static ObscuredBoolEqualityComparer Default
        {
            get
            {
                ObscuredBoolEqualityComparer comparer = defaultComparer;

                if (comparer == null)
                {
                    comparer = new ObscuredBoolEqualityComparer();
                    defaultComparer = comparer;
                }

                return comparer;
            }
        }

        bool IEqualityComparer<ObscuredBool>.Equals(ObscuredBool x, ObscuredBool y)
        {
            return x.Equals(y);
        }

        int IEqualityComparer<ObscuredBool>.GetHashCode(ObscuredBool obj)
        {
            return obj.GetHashCode();
        }
    }
}