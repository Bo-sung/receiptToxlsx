namespace Ragnarok
{
    public enum ShopConditionType
    {
        /// <summary>
        /// 지급타입 없음
        /// </summary>
        None = 0,
        /// <summary>
        /// 28일간 매일보상(중복가능)
        /// </summary>
        EveryDay = 1,
        /// <summary>
        /// 직업레벨 패키지(1회성) 중복불가
        /// </summary>
        JobLevel = 2,
        /// <summary>
        /// 챕터 해금 패키지(1회성) 중복불가
        /// </summary>
        Scenario = 3,
        /// <summary>
        /// 셰어 공유 UP 패키지
        /// </summary>
        ShareUp = 4,
        /// <summary>
        /// 첫결제 보상
        /// </summary>
        FirstPurchaseReward = 5,
        /// <summary>
        /// 금빛 영양제 패키지
        /// </summary>
        TreeReward = 6,
        /// <summary>
        /// 시즌 패스
        /// </summary>
        SeasonPass = 7,
        /// <summary>
        /// 온버프 패스
        /// </summary>
        OnBuffPass = 8,
    }
}