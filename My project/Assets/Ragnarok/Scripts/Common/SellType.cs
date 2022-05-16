namespace Ragnarok
{
    public enum SellType
    {
        /// <summary>
        /// 상시 판매
        /// </summary>
        SellAlways = 1,

        /// <summary>
        /// 기간제 상품
        /// </summary>
        SellPeriodProduct = 2,

        /// <summary>
        /// 단위 기간 상품
        /// </summary>
        ResetPeriodProduct = 3,

        /// <summary>
        /// 복귀 유저용 판매
        /// </summary>
        [System.Obsolete]
        ComebackUser = 4,

        /// <summary>
        /// 신규 유저용 판매
        /// </summary>
        [System.Obsolete]
        NewUser = 5,
    }
}
