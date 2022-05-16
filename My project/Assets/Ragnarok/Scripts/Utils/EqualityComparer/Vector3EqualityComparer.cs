using UnityEngine;
using System.Collections.Generic;

namespace Ragnarok
{
    public class Vector3EqualityComparer : IEqualityComparer<Vector3>
    {
        static volatile Vector3EqualityComparer defaultComparer;

        public static Vector3EqualityComparer Default
        {
            get
            {
                Vector3EqualityComparer comparer = defaultComparer;

                if (comparer == null)
                {
                    comparer = new Vector3EqualityComparer();
                    defaultComparer = comparer;
                }

                return comparer;
            }
        }

        bool IEqualityComparer<Vector3>.Equals(Vector3 x, Vector3 y)
        {
            return x.Equals(y);
        }

        int IEqualityComparer<Vector3>.GetHashCode(Vector3 obj)
        {
            return obj.GetHashCode();
        }
    }
}