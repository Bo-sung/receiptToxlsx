using System.Linq;
using System.Reflection;
using UnityEditor;

namespace Ragnarok
{
    /// <summary>
    /// 출처<see cref="https://baba-s.hatenablog.com/entry/2017/11/13/144824"/>
    /// </summary>
    public static class AsyncProgressBar
    {
        private static MethodInfo m_display = null;
        private static MethodInfo m_clear = null;

        private static bool isRun;

        static AsyncProgressBar()
        {
            System.Type type = typeof(Editor).Assembly
                .GetTypes()
                .Where(c => c.Name == "AsyncProgressBar")
                .FirstOrDefault();

            m_display = type.GetMethod("Display");
            m_clear = type.GetMethod("Clear");
        }

        public static void Display(string progressInfo, float progress)
        {
            var parameters = new object[] { progressInfo, progress };
            m_display.Invoke(null, parameters);
            isRun = true;
        }

        public static bool Clear()
        {
            if (isRun)
            {
                m_clear.Invoke(null, null);
                isRun = false;
                return true;
            }

            return false;
        }
    }
}