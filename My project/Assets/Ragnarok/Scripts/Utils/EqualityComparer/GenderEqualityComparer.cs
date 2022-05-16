using System.Collections.Generic;

namespace Ragnarok
{
    public class GenderEqualityComparer : IEqualityComparer<Gender>
    {
        static volatile GenderEqualityComparer defaultComparer;

        public static GenderEqualityComparer Default
        {
            get
            {
                GenderEqualityComparer comparer = defaultComparer;

                if (comparer == null)
                {
                    comparer = new GenderEqualityComparer();
                    defaultComparer = comparer;
                }

                return comparer;
            }
        }

        bool IEqualityComparer<Gender>.Equals(Gender x, Gender y)
        {
            return x.Equals(y);
        }

        int IEqualityComparer<Gender>.GetHashCode(Gender obj)
        {
            return obj.GetHashCode();
        }
    }
}