namespace Ragnarok
{
    public enum ReplaceKey
    {
        VALUE,
        TYPE,
        MAX,
        SIZE,
        TIME,
        NAME,
        LEVEL,
        COUNT,
        ITEM,
        GRADE,
        INDEX,
        RANK,
        PLUS,
        LINK,
        DUNGEON,
        DIFFICULTY,
        YEARS,
        MONTHS,
        DAYS,
        HOURS,
        MINUTES,
        SECONDS,
        WIN,
        LOSE,
        COLOR,
        POSTPOSITION,
        DATE,
        START_DATE,
        END_DATE,
        CHAPTER,
        POINT,
        NUMBER,
        LANG,
    }

    public static class ReplaceKeyExtensions
    {
        public static string Replace(this string text, ReplaceKey key, int value)
        {
            return text.Replace(key, value.ToString());
        }

        public static string Replace(this string text, ReplaceKey key, long value)
        {
            return text.Replace(key, value.ToString());
        }

        public static string Replace(this string text, ReplaceKey key, string value)
        {
            return text.Replace(key.ToSubstituteText(), value);
        }

        public static string Replace(this string text, ReplaceKey key, int index, string value)
        {
            string substitude = key.ToSubstituteText();
            substitude = substitude.Remove(substitude.Length - 1);
            substitude = $"{substitude}{index}}}";
            return text.Replace(substitude, value);
        }

        private static string ToSubstituteText(this ReplaceKey key)
        {
            switch (key)
            {
                case ReplaceKey.VALUE:
                    return "{VALUE}";

                case ReplaceKey.TYPE:
                    return "{TYPE}";

                case ReplaceKey.MAX:
                    return "{MAX}";

                case ReplaceKey.SIZE:
                    return "{SIZE}";

                case ReplaceKey.TIME:
                    return "{TIME}";

                case ReplaceKey.NAME:
                    return "{NAME}";

                case ReplaceKey.LEVEL:
                    return "{LEVEL}";

                case ReplaceKey.COUNT:
                    return "{COUNT}";

                case ReplaceKey.ITEM:
                    return "{ITEM}";

                case ReplaceKey.GRADE:
                    return "{GRADE}";

                case ReplaceKey.INDEX:
                    return "{INDEX}";

                case ReplaceKey.RANK:
                    return "{RANK}";

                case ReplaceKey.PLUS:
                    return "{PLUS}";

                case ReplaceKey.LINK:
                    return "{LINK}";

                case ReplaceKey.DUNGEON:
                    return "{DUNGEON}";

                case ReplaceKey.DIFFICULTY:
                    return "{DIFFICULTY}";

                case ReplaceKey.YEARS:
                    return "{YEARS}";

                case ReplaceKey.MONTHS:
                    return "{MONTHS}";

                case ReplaceKey.DAYS:
                    return "{DAYS}";

                case ReplaceKey.HOURS:
                    return "{HOURS}";

                case ReplaceKey.MINUTES:
                    return "{MINUTES}";

                case ReplaceKey.SECONDS:
                    return "{SECONDS}";

                case ReplaceKey.WIN:
                    return "{WIN}";

                case ReplaceKey.LOSE:
                    return "{LOSE}";

                case ReplaceKey.COLOR:
                    return "{COLOR}";

                case ReplaceKey.POSTPOSITION:
                    return "{POSTPOSITION}";

                case ReplaceKey.DATE:
                    return "{DATE}";

                case ReplaceKey.START_DATE:
                    return "{START_DATE}";

                case ReplaceKey.END_DATE:
                    return "{END_DATE}";

                case ReplaceKey.CHAPTER:
                    return "{CHAPTER}";

                case ReplaceKey.POINT:
                    return "{POINT}";

                case ReplaceKey.NUMBER:
                    return "{NUMBER}";

                case ReplaceKey.LANG:
                    return "{LANG}";
            }

            return string.Empty;
        }
    }
}