using System.Text;

namespace Ragnarok
{
    public static class StringBuilderPool
    {
        public static readonly ObjectPool<StringBuilder> sbPool = new ObjectPool<StringBuilder>(null, OnRelease);

        public static StringBuilder Get()
        {
            return sbPool.Get();
        }

        private static void OnRelease(StringBuilder stringBuilder)
        {
            stringBuilder.Length = 0;
        }

        public static string Release(this StringBuilder toRelease)
        {
            string output = toRelease.ToString();
            sbPool.Release(toRelease);
            return output;
        }
    }
}