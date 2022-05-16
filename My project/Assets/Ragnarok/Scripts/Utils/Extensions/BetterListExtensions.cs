using System.Collections.Generic;

namespace Ragnarok
{
    public static class BetterListExtensions
    {
        public static T Find<T>(this BetterList<T> list, T match) where T : class
        {
            foreach (T e in list)
            {
                if (e.Equals(match))
                    return e;
            }
            return null;
        }

        public static T Find<T>(this BetterList<T> list, System.Predicate<T> pred) where T : class
        {
            foreach (T e in list)
            {
                if (pred(e))
                    return e;
            }
            return null;
        }

        public static List<T> FindAll<T>(this BetterList<T> list, System.Predicate<T> pred) where T : class
        {
            List<T> ret = new List<T>();
            foreach (T e in list)
            {
                if (pred(e))
                {
                    ret.Add(e);
                }
            }
            return ret;
        }

        //public static List<T> ToList<T>(this BetterList<T> list) where T : class
        //{
        //    List<T> ret = new List<T>();
        //    foreach (var e in list)
        //        ret.Add(e);
        //    return ret;
        //}

        public static bool Exists<T>(this BetterList<T> list, T match) where T : class
        {
            foreach (var e in list)
            {
                if (e.Equals(match))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool Exists<T>(this BetterList<T> list, System.Predicate<T> pred) where T : class
        {
            foreach (var e in list)
            {
                if (pred(e))
                {
                    return true;
                }
            }
            return false;
        }

        // 이건 직접 함수화 시켜주기 ..
        //public static void ForEach<T>(this BetterList<T> list, System.Action<T> action) where T : class
        //{
        //    foreach (var e in list)
        //        action(e);
        //}
    }
}
