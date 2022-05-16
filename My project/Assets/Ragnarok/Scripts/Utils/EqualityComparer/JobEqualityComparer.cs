using System.Collections.Generic;

namespace Ragnarok
{
    public class JobEqualityComparer : IEqualityComparer<Job>
    {
        static volatile JobEqualityComparer defaultComparer;

        public static JobEqualityComparer Default
        {
            get
            {
                JobEqualityComparer comparer = defaultComparer;

                if (comparer == null)
                {
                    comparer = new JobEqualityComparer();
                    defaultComparer = comparer;
                }

                return comparer;
            }
        }

        bool IEqualityComparer<Job>.Equals(Job x, Job y)
        {
            return x.Equals(y);
        }

        int IEqualityComparer<Job>.GetHashCode(Job obj)
        {
            return obj.GetHashCode();
        }
    }
}