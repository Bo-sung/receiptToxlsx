namespace Ragnarok
{
    public enum NpcType
    {
        // <!-- 대화 --!>
        Holoruchi = 1,
        Deviruchi = 2,
        NoviceMale = 3,
        NoviceFemale = 4,
        Pruit = 5,

        // <!-- 길드 로비 --!>
        /// <summary>
        /// 더블류 (길드 상점)
        /// </summary>
        W = 101,
        /// <summary>
        /// 타마미 (테이밍 미궁)
        /// </summary>
        Tamami = 102,
        /// <summary>
        /// 특수요원 (레이드)
        /// </summary>
        SpecialAgent = 103,
        /// <summary>
        /// 비밀요원 (협동전)
        /// </summary>
        SecretAgent = 104,
        /// <summary>
        /// 경비대원 (길드전)
        /// </summary>
        Guard = 105,
        /// <summary>
        /// 길드습격
        /// </summary>
        Emperium = 106,

        // <!-- 타임패트롤 --!>
        /// <summary>
        /// 시계탑 관리자
        /// </summary>
        ClockTower = 201,

        // <!-- 거래소 --!>
        /// <summary>
        /// 테일링
        /// </summary>
        Tailing = 301,
        
        /// <summary>
        /// 소린(카프라 운송)
        /// </summary>
        Sorin = 302,
        
        /// <summary>
        /// 소티
        /// </summary>
        Sortie = 303,

        /// <summary>
        /// 냥쿤
        /// </summary>
        Nyankun = 304,
    }
}