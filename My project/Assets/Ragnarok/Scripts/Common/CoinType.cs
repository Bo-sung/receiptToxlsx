namespace Ragnarok
{
    public enum CoinType
    {
        [System.Obsolete]
        QuestCoint = 0,

        CatCoin = 1,
        Zeny = 2,
        Cash = 3,
        Ad = 4,
        GuildCoin = 5,
        Free = 6, // 무료 구매 상품
        OnBuffPoint = 7, // 온버프 포인트

        // <!-- 클라 전용 --!>
        RoPoint = 100,
    }
}