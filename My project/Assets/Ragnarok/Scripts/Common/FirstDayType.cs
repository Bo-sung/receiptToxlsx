namespace Ragnarok
{
    /// <summary>
    /// [상점] 첫 구매시 2배 기간 타입
    /// </summary>
    public enum FirstDayType
    {
        None = 0,
        /// <summary>
        /// 일간
        /// </summary>
        Daily = 1,
        /// <summary>
        /// 주간
        /// </summary>
        Weekly = 2,
        /// <summary>
        /// 월간
        /// </summary>
        Monthly = 3,
        /// <summary>
        /// 영구
        /// </summary>
        OneTime = 4,
    }
}
