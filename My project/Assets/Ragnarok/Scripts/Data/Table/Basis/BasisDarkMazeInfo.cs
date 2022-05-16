namespace Ragnarok
{
    public enum BasisDarkMazeInfo
    {
        /// <summary>
        /// 매일 최초 보상 아이템 아이디 (오크용 기저귀) (94235)
        /// </summary>
        DailyRewardItemId = 1,

        /// <summary>
        /// 클리어 제니 보상 (30000)
        /// </summary>
        ClearRewardZeny = 2,

        /// <summary>
        /// 매칭 플레이어 수 (3)
        /// </summary>
        [System.Obsolete("", true)]
        MatchPlayerCount = 3,

        /// <summary>
        /// 플레이 시간 (밀리초) (180000)
        /// </summary>
        PlayTime = 4,

        /// <summary>
        /// 최대 몬스터(루드) 수 (5)
        /// </summary>
        [System.Obsolete("", true)]
        MonsterCount = 5,

        /// <summary>
        /// 이벤트 몬스터(루드) 아이디 (50160)
        /// </summary>
        MonsterId = 6,

        /// <summary>
        /// 이벤트 보스몬스터(오크베이비) 아이디 (50161)
        /// </summary>
        BossMonsterId = 7,
    }
}