using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;

namespace Ragnarok
{
    public class ObscuredShortEqualityComparer : IEqualityComparer<ObscuredShort>
    {
        static volatile ObscuredShortEqualityComparer defaultComparer;

        public static ObscuredShortEqualityComparer Default
        {
            get
            {
                ObscuredShortEqualityComparer comparer = defaultComparer;

                if (comparer == null)
                {
                    comparer = new ObscuredShortEqualityComparer();
                    defaultComparer = comparer;
                }

                return comparer;
            }
        }

        bool IEqualityComparer<ObscuredShort>.Equals(ObscuredShort x, ObscuredShort y)
        {
            return x.Equals(y);
        }

        int IEqualityComparer<ObscuredShort>.GetHashCode(ObscuredShort obj)
        {
            return obj.GetHashCode();
        }
    }
}