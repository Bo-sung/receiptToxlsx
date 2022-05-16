using UnityEngine;
using System.Collections.Generic;

namespace Ragnarok
{
    public class TransformEqualityComparer : IEqualityComparer<Transform>
    {
        static volatile TransformEqualityComparer defaultComparer;

        public static TransformEqualityComparer Default
        {
            get
            {
                TransformEqualityComparer comparer = defaultComparer;

                if (comparer == null)
                {
                    comparer = new TransformEqualityComparer();
                    defaultComparer = comparer;
                }

                return comparer;
            }
        }

        bool IEqualityComparer<Transform>.Equals(Transform x, Transform y)
        {
            return x.Equals(y);
        }

        int IEqualityComparer<Transform>.GetHashCode(Transform obj)
        {
            return obj.GetHashCode();
        }
    }
}