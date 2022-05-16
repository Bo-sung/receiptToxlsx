namespace Ragnarok
{
    [System.Flags]
    public enum DungeonFreeRewardType
    {
        /// <summary>
        /// 제니던전 무료보상
        /// </summary>
        FreeRewardZeny = 1 << 0,

        /// <summary>
        /// 경험치던전 무료보상
        /// </summary>
        FreeRewardExp = 1 << 1,

        /// <summary>
        /// 디펜스던전 무료보상
        /// </summary>
        FreeRewardDefence = 1 << 2,
    }
}