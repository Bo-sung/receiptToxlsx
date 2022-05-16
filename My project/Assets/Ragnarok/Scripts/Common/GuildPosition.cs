namespace Ragnarok
{
    public enum GuildPosition : byte
    {
        None = 0,       // 길드원 아님
        Member = 1,     // 일반 길드원
        PartMaster = 2, // 부길드마스터
        Master = 3,     // 길드마스터    
    } 

    public static class GuildPositionExtension
    {
        public static string ToText(this GuildPosition type)
        {
            switch (type)
            {                
                case GuildPosition.Member:
                    return LocalizeKey._62000.ToText(); // 일반길드원
                case GuildPosition.PartMaster:
                    return LocalizeKey._62001.ToText(); // 부길드장
                case GuildPosition.Master:
                    return LocalizeKey._62002.ToText(); // 길드장
            }
            return default;
        }
    }
}
