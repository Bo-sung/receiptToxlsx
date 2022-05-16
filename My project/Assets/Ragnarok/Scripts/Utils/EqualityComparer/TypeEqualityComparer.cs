using System;
using System.Collections.Generic;

namespace Ragnarok
{
	public class TypeEqualityComparer : IEqualityComparer<Type>
    {
        static volatile TypeEqualityComparer defaultComparer;

        public static TypeEqualityComparer Default
        {
            get
            {
                TypeEqualityComparer comparer = defaultComparer;

                if (comparer == null)
                {
                    comparer = new TypeEqualityComparer();
                    defaultComparer = comparer;
                }

                return comparer;
            }
        }

        bool IEqualityComparer<Type>.Equals(Type x, Type y)
        {
            return x.Equals(y);
        }

        int IEqualityComparer<Type>.GetHashCode(Type obj)
        {
            return obj.GetHashCode();
        }
    }
}