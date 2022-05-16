namespace Ragnarok
{
    /// <summary>
    /// 카프라 운송 완료 상태
    /// </summary>
    public enum KafraCompleteType
    {
        /// <summary>
        /// 없음
        /// </summary>
        None = 0,
        /// <summary>
        /// 진행중
        /// </summary>
        InProgress = 1,
        /// <summary>
        /// 보상 대기중
        /// </summary>
        StandByReward = 2,
    }
}