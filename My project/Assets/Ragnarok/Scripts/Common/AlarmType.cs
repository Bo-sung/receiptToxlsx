namespace Ragnarok
{
    [System.Flags]
    public enum AlarmType : int
    {
        None = 0,
        /// <summary>
        /// 우편함-계정
        /// </summary>
        MailAccount = 1 << 0,  // 1

        /// <summary>
        /// 이벤트
        /// </summary>
        Event = 1 << 1, // 2

        /// <summary>
        /// 길드-가입신청목록
        /// </summary>
        GuildRequest = 1 << 2, // 4

        /// <summary>
        /// 길드-새로운 게시판글(공지포함)
        /// </summary>
        GuildBoard = 1 << 3, // 8

        /// <summary>
        /// 우편함-캐릭터
        /// </summary>
        MailCharacter = 1 << 4, // 16

        /// <summary>
        /// 우편함-상점
        /// </summary>
        MailShop = 1 << 5, // 32

        /// <summary>
        /// 우편함-거래소/개인상점
        /// </summary>
        MailTrade = 1 << 6, // 64

        /// <summary>
        /// 남이 나한테 듀얼 걸었을때
        /// </summary>
        Duel = 1 << 7, // 128

        /// <summary>
        /// 우편함 - 온버프
        /// </summary>
        MailOnBuff = 1 << 10, // 1024
    }

    public static class AlarmTypeExtension
    {
        public static bool IsMailAlarm(this AlarmType type)
        {
            if (type.HasFlag(AlarmType.MailAccount))
                return true;

            if (type.HasFlag(AlarmType.MailCharacter))
                return true;

            if (type.HasFlag(AlarmType.MailShop))
                return true;

            if (type.HasFlag(AlarmType.MailTrade))
                return true;

            if (type.HasFlag(AlarmType.MailOnBuff))
                return true;

            return false;
        }
    }
}


