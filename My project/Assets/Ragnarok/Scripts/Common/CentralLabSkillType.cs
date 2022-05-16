namespace Ragnarok
{
    public enum CentralLabSkillType
    {
        /// <summary>
        /// 일반 스킬
        /// </summary>
        Normal = 0,

        /// <summary>
        /// 1 스테이지 시작 시 등장하는 스킬
        /// </summary>
        FirstSelect = 1,

        /// <summary>
        /// 보스 처치 후에만 등장하는 스킬
        /// </summary>
        BossKill = 2,

        /// <summary>
        /// 포링의 축복에서만 등장하는 나오는 스킬
        /// </summary>
        PoringBless = 3,
    }
}