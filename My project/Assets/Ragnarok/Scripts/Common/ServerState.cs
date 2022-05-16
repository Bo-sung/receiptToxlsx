namespace Ragnarok
{
    // [0] 점검중 [1] 내부오픈 [2] 외부오픈
    public enum ServerState : byte
    {
        /// <summary>점검중</summary>
        MAINTENANCE = 0,
        /// <summary>내부오픈</summary>
        INSIDE_OPEN,
        /// <summary>외부오픈</summary>
        EXTERNAL_OPEN,
    }
}