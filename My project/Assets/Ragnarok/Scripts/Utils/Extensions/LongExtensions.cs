using System;
using UnityEngine;

namespace Ragnarok
{
    public static class LongExtensions
    {
        private static string[] sizeNames = { "B", "KB", "MB", "GB" };

        /// <summary>
        /// 용량 string
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string FormatBytes(this long bytes)
        {
            float pow = Mathf.Floor((bytes > 0 ? Mathf.Log(bytes) : 0) / Mathf.Log(1024));
            pow = Mathf.Min(pow, sizeNames.Length - 1);
            float value = bytes / Mathf.Pow(1024, pow);
            return $"{value.ToString(pow == 0 ? "F0" : "F2")}{sizeNames[(int)pow]}";
        }

        public static DateTime ToDateTime(this long date)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(date).ToLocalTime();
        }

        //public static DateTime ToDateTime(long epochTime)
        //{
        //    return DateTimeOffset.FromUnixTimeMilliseconds(epochTime).DateTime;
        //}

        //public static long GetNowEpochTime()
        //{
        //    return DateTimeOffset.Now.ToUnixTimeMilliseconds();
        //}
    }
}
