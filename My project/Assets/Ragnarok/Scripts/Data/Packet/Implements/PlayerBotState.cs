namespace Ragnarok
{
    public enum PlayerBotState
    {
        /// <summary>
        /// 일반
        /// </summary>
        General = 1,

        /// <summary>
        /// 보스 전투 중
        /// </summary>
        BattleBoss = 5,

        /// <summary>
        /// 무방비 상태
        /// </summary>
        Defenseless = 7,

        /// <summary>
        /// 죽어있음
        /// </summary>
        Dead = 9,
    }
}