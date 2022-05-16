using System;

namespace Ragnarok
{
    public static class FloatExtensions
    {

        /// <summary>
        /// 남은 시간 toString
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string ToStringTime(this float time, string format = @"hh\:mm\:ss")
        {
            TimeSpan span = TimeSpan.FromMilliseconds(time);

            return span.ToString(format);
        }

        public static double ToTotalSeconds(this float time)
        {
            return TimeSpan.FromMilliseconds(time).TotalSeconds;
        }

        public static string ToTotalMinute(this float time)
        {
            return $"{(int)TimeSpan.FromMilliseconds(time).TotalMinutes}";
        }

        public static string ToStringTime(this int time, string format = @"hh\:mm\:ss")
        {
            TimeSpan span = TimeSpan.FromSeconds(time);
            return span.ToString(format);
        }

        public static TimeSpan ToTimeSpan(this float time)
        {
            return TimeSpan.FromMilliseconds(time);
        }

        public static string ToStringOnlineTime(this TimeSpan timeSpan)
        {
            int temp = (int)timeSpan.TotalDays;
            if (temp > 0)
                return LocalizeKey._65002.ToText().Replace(ReplaceKey.DAYS, temp); // {DAYS}일전

            temp = (int)timeSpan.TotalHours;
            if (temp > 0)
                return LocalizeKey._65003.ToText().Replace(ReplaceKey.HOURS, temp); // {HOURS}시간전

            temp = (int)timeSpan.TotalMinutes;
            if (temp > 0)
                return LocalizeKey._65004.ToText().Replace(ReplaceKey.MINUTES, temp); // {MINUTES}분전

            temp = (int)timeSpan.TotalSeconds;
            return LocalizeKey._65005.ToText().Replace(ReplaceKey.SECONDS, temp); // {SECONDS}초전
        }

        public static string ToStringTimeConatinsDay(this float time, string format = @"hh\:mm\:ss")
        {
            TimeSpan span = TimeSpan.FromMilliseconds(time);

            int totalDays = (int)span.TotalDays;
            if (totalDays > 0)
                return LocalizeKey._65002.ToText().Replace(ReplaceKey.DAYS, totalDays);// {DAYS}일전

            return span.ToString(format);
        }

        public static string ToStringTimeConatinsDayLocal(this float time, string format = @"hh\:mm\:ss")
        {
            TimeSpan span = TimeSpan.FromMilliseconds(time);

            int totalDays = (int)span.TotalDays;
            if (totalDays > 0)
                return LocalizeKey._17.ToText().Replace(ReplaceKey.DAYS, totalDays);// {DAYS}일전

            return span.ToString(format);
        }

        public static string ToTatalHourTime(this int time)
        {
            TimeSpan span = TimeSpan.FromSeconds(time);

            return $"{((int)span.TotalHours).ToString("00")}{span.ToString(@"\:mm\:ss")}";
        }
    }
}
