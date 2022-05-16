namespace Ragnarok
{
    public enum EnableType 
    {
        /// <summary>
        /// 0 : 빈칸으로 쓰는 용도. (터치X, 제작X)
        /// </summary>
        DeActive = 0,
        /// <summary>
        /// 표기용으로 쓰는 용도. (터치O, 제작X)
        /// [터치 시 아이템 정보 팝업 등장]
        /// </summary>
        Disable = 1,
        /// <summary>
        /// 제작용으로 쓰는 용도. (터치O, 제작O)
        /// </summary>
        Enable = 2,

    } 
}
