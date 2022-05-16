namespace Ragnarok
{
    public enum BasisOnBuffInfo
    {
        /// <summary>
        /// 무료 MVP 처치 일일 한도 (100)
        /// </summary>
        DailyMvpKillCount = 1,

        /// <summary>
        /// 유료 MVP 처치 일일 한도 (500)
        /// </summary>
        [System.Obsolete]
        PremiumDailyUniqueMvpKillCount = 2,

        /// <summary>
        /// 노출 여부
        /// </summary>
        Display = 3,

        /// <summary>
        /// 무료 MVP 처치 보상 - OnBuff포인트 (100)
        /// </summary>
        FreeOnBuffRewardPoints = 4,

        /// <summary>
        /// 유료 MVP 처치 보상 - OnBuff포인트 (400)
        /// </summary>
        PremiumOnBuffRewardPoints = 5,
    }
}