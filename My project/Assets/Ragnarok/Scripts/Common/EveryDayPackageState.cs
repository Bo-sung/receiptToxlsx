namespace Ragnarok
{
    public enum EveryDayPackageState
    {
        /// <summary>
        /// 구매 완료(메일함에 있음)
        /// </summary>
        CompletePurchase,

        /// <summary>
        /// 구매 가능 (첫구매, 재구매)
        /// </summary>
        AvailablePurchase,

        /// <summary>
        /// 보상 대기 중
        /// </summary>
        StandByReward,

        /// <summary>
        /// 보상 완료
        /// </summary>
        ReceivedReward,
    }
}