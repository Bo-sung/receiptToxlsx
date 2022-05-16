using UnityEngine;
using System.Collections.Generic;

namespace Ragnarok
{
    public class ColorEqualityComparer : IEqualityComparer<Color>
    {
        static volatile ColorEqualityComparer defaultComparer;

        public static ColorEqualityComparer Default
        {
            get
            {
                ColorEqualityComparer comparer = defaultComparer;

                if (comparer == null)
                {
                    comparer = new ColorEqualityComparer();
                    defaultComparer = comparer;
                }

                return comparer;
            }
        }

        bool IEqualityComparer<Color>.Equals(Color x, Color y)
        {
            return x == y;
        }

        int IEqualityComparer<Color>.GetHashCode(Color obj)
        {
            return obj.GetHashCode();
        }
    }
}