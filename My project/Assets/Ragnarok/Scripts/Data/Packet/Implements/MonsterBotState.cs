namespace Ragnarok
{
    public enum MonsterBotState
    {
        /// <summary>
        /// 아무 상태 아님
        /// </summary>
        None = 0,

        /// <summary>
        /// 일반
        /// </summary>
        General = 1,

        /// <summary>
        /// 순찰
        /// </summary>
        Patrol = 3,

        /// <summary>
        /// 전투중 (보스 몹 전투 중)
        /// </summary>
        MazeBattle = 5,

        /// <summary>
        /// 멈춤
        /// </summary>
        Freeze = 7,

        /// <summary>
        /// 죽고 난 후 리스폰까지의 대기 상태
        /// </summary>
        Die = 9,
    }
}