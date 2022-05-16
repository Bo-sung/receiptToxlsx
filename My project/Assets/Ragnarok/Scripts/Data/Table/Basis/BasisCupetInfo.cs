namespace Ragnarok
{
    public enum BasisCupetInfo
    {
        /// <summary>
        /// 큐펫 레벨업에 따른 스탯 포인트 증가량
        /// </summary>
        StatPointbyLevel = 1,

        /// <summary>
        /// 큐펫 Lv.1 기초 경험치량
        /// </summary>
        NeedLevelUpExp = 2,

        /// <summary>
        /// 큐펫 레벨업에 따른 경험치 증가량[가중치]
        /// </summary>
        IncreaseLevelUpExp = 3,

        /// <summary>
        /// 큐펫 레벨 max
        /// </summary>
        [System.Obsolete]
        MaxLevel = 4,
    }
}