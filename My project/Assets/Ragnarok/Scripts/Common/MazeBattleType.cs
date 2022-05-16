namespace Ragnarok
{
    public enum MazeBattleType
    {
        /// <summary>
        /// 미로 전투 중이 아님
        /// </summary>
        None = 0,
        /// <summary>
        /// UI 데이터 전투
        /// </summary>
        Simple = 1,
        /// <summary>
        /// 클리커 전투
        /// </summary>
        Clicker = 2,
        /// <summary>
        /// 클리어 전투 (무조건 이김)
        /// </summary>
        ClearAction = 3,
        /// <summary>
        /// 필드 전투
        /// </summary>
        Action = 4,
        /// <summary>
        /// 진입 전투
        /// </summary>
        IntoBattle = 5,
        /// <summary>
        /// 자동 진입 전투
        /// </summary>
        AutoIntoBattle = 6,
    }
}