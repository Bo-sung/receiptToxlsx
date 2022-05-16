using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;

namespace Ragnarok
{
    public class ObscuredIntTupleEqualityComparer : IEqualityComparer<(ObscuredInt key1, ObscuredInt key2)>
    {
        static volatile ObscuredIntTupleEqualityComparer defaultComparer;

        public static ObscuredIntTupleEqualityComparer Default
        {
            get
            {
                ObscuredIntTupleEqualityComparer comparer = defaultComparer;

                if (comparer == null)
                {
                    comparer = new ObscuredIntTupleEqualityComparer();
                    defaultComparer = comparer;
                }

                return comparer;
            }
        }

        public bool Equals((ObscuredInt key1, ObscuredInt key2) x, (ObscuredInt key1, ObscuredInt key2) y)
        {
            return x.Equals(y);
        }

        public int GetHashCode((ObscuredInt key1, ObscuredInt key2) obj)
        {
            return obj.GetHashCode();
        }
    }
}