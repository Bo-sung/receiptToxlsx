namespace Ragnarok
{
    public static class StringUtils
    {
        public enum SplitType
        {
            Comma,
            Clone,
        }

        private static readonly char[] SEPARATOR_COMMA = { ',' };
        private static readonly char[] SEPARATOR_COLON = { ':' };

        public static string[] Split(string text, SplitType type)
        {
            if (string.IsNullOrEmpty(text))
                return System.Array.Empty<string>();

            switch (type)
            {
                case SplitType.Comma: return text.Split(SEPARATOR_COMMA, System.StringSplitOptions.RemoveEmptyEntries);
                case SplitType.Clone: return text.Split(SEPARATOR_COLON, System.StringSplitOptions.RemoveEmptyEntries);
            }

            return System.Array.Empty<string>();
        }

        public static bool IsDigit(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            foreach (char ch in text)
            {
                if (!char.IsDigit(ch))
                    return false;
            }

            return true;
        }

        public static string TimeToText(System.TimeSpan timeSpan)
        {
            if (timeSpan.Days > 0)
            {
                if (timeSpan.Hours == 0 && timeSpan.Minutes == 0 && timeSpan.Seconds == 0)
                {
                    return LocalizeKey._65002.ToText() // {DAYS}일
                        .Replace(ReplaceKey.DAYS, timeSpan.Days.ToString());
                }

                return LocalizeKey._47844.ToText() // {DAYS}일 {HOURS}:{MINUTES}:{SECONDS}
                    .Replace(ReplaceKey.DAYS, timeSpan.Days.ToString())
                    .Replace(ReplaceKey.HOURS, timeSpan.Hours.ToString("00"))
                    .Replace(ReplaceKey.MINUTES, timeSpan.Minutes.ToString("00"))
                    .Replace(ReplaceKey.SECONDS, timeSpan.Seconds.ToString("00"));
            }

            return timeSpan.ToString(@"hh\:mm\:ss");
        }
    }
}