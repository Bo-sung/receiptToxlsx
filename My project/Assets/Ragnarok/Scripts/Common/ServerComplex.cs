namespace Ragnarok
{
    // [1] 원활 [2] 혼잡 [3] 매우혼잡
    public enum ServerComplex : byte
    {
        /// <summary>원활</summary>
        SMOOTH = 1,
        /// <summary>혼잡</summary>
        CROWDED,
        /// <summary>매우혼잡</summary>
        VERY_CROWDED,
    }
}