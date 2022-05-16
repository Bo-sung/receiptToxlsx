using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;

namespace Ragnarok
{
    public class ObscuredStringEqualityComparer : IEqualityComparer<ObscuredString>
    {
        static volatile ObscuredStringEqualityComparer defaultComparer;

        public static ObscuredStringEqualityComparer Default
        {
            get
            {
                ObscuredStringEqualityComparer comparer = defaultComparer;

                if (comparer == null)
                {
                    comparer = new ObscuredStringEqualityComparer();
                    defaultComparer = comparer;
                }

                return comparer;
            }
        }

        bool IEqualityComparer<ObscuredString>.Equals(ObscuredString x, ObscuredString y)
        {
            return x == y;
        }

        int IEqualityComparer<ObscuredString>.GetHashCode(ObscuredString obj)
        {
            return obj.GetHashCode();
        }
    }
}