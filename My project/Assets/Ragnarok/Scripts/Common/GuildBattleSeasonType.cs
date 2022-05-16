namespace Ragnarok
{
    public enum GuildBattleSeasonType : byte
    {
        /// <summary>
        /// 길드전 참가신청 가능 기간
        /// </summary>
        Ready = 0,

        /// <summary>
        /// 길드전 진행 중
        /// </summary>
        InProgress = 1,

        /// <summary>
        /// 랭킹 정산 중
        /// </summary>
        Calculating = 2,
    }
}
