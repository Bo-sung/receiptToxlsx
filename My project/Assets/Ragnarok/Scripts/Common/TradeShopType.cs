namespace Ragnarok
{
    /// <summary>
    /// 개인상점 개설 상태
    /// </summary>
    public enum PrivateStoreSellingState
    {
        NOT_SELLING = 0,
        SELLING = 1,
    }


    /// <summary>
    /// 거래소 거래기록 데이터의 판매/구매 타입
    /// </summary>
    public enum TradeShopTradeSellType : byte
    {
        PRIVATE_SELL = 0, // 개인상점 판매
        PRIVATE_BUY = 1, // 개인상점 구매
        TRADE_SELL = 2, // 거래소 판매
        TRADE_BUY = 3, // 거래소 구매
    }
}