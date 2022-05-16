namespace Ragnarok
{
    /// <summary>
    /// [상점] 횟수 제한 초기화 타입
    /// </summary>
    public enum LimitDayType
    {
        /// <summary>
        /// 횟수 초기화 없음
        /// </summary>
        NoLimit = 0,
        /// <summary>
        /// 하루 단위 초기화
        /// </summary>
        DailyLimit = 1,
        /// <summary>
        /// 주 단위 초기화
        /// </summary>
        WeeklyLimit = 2,
        /// <summary>
        /// 월 단위 초기화
        /// </summary>
        MonthlyLimit = 3,

        /// <summary>
        /// 12시간 단위 초기화
        /// </summary>
        Lmit_12H = 4,
        /// <summary>
        /// 24시간 단위 초기화
        /// </summary>
        Lmit_24H = 5,
        /// <summary>
        /// 36시간 단위 초기화
        /// </summary>
        Lmit_36H = 6,
        /// <summary>
        /// 48시간 단위 초기화
        /// </summary>
        Lmit_48H = 7,
        /// <summary>
        /// 60시간 단위 초기화
        /// </summary>
        Lmit_60H = 8,

        /// <summary>
        /// 6시간 단위 초기화
        /// </summary>
        Lmit_6H = 9,
    }
}