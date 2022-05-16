namespace Ragnarok
{
    public enum BasisGuildWarInfo
    {
        /// <summary>
        /// 길드전 일일 전투 횟수 (5)
        /// </summary>
        DailyEntryCount = 1,

        /// <summary>
        /// 길드전 전투 지속시간 (밀리초) (180000)
        /// </summary>
        PlayTime = 2,

        /// <summary>
        /// 방어타워 몬스터 아이디 (547)
        /// </summary>
        DefenceTowerId = 3,

        /// <summary>
        /// 공격타워 몬스터 아이디 (546)
        /// </summary>
        AttackTowerId = 4,

        /// <summary>
        /// 타워 레벨 (1)
        /// </summary>
        TowerLevel = 5,

        /// <summary>
        /// 엠펠리움 몬스터 아이디 (545)
        /// </summary>
        EmperiumId = 6,

        /// <summary>
        /// 엠펠리움 몬스터 레벨 (1)
        /// </summary>
        EmperiumLevel = 7,

        /// <summary>
        /// 최대 방어큐펫 소환 수 (16)
        /// </summary>
        MaxDefenceCupetCount = 8,

        /// <summary>
        /// 최대 공격큐펫 소환 수 (16)
        /// </summary>
        MaxAttackCupetCount = 9,

        /// <summary>
        /// 큐펫 소환 딜레이 (밀리초) (1800)
        /// </summary>
        CupetSpawnDelay = 10,

        /// <summary>
        /// 길드 리스트 목록 쿨타임 (밀리초) (5000)
        /// </summary>
        GuildListRequestDelay = 11,

        /// <summary>
        /// 길드전 버프 레벨업 필요 경험치 (100)
        /// </summary>
        BuffNeedLevelUpExp = 12,

        /// <summary>
        /// 버프스킬 1 아이디 (큐펫 버프[힘]) (40001)
        /// </summary>
        BuffSkill1 = 13,

        /// <summary>
        /// 버프스킬 2 아이디 (큐펫 버프[마법]) (40002)
        /// </summary>
        BuffSkill2 = 14,

        /// <summary>
        /// 버프스킬 3 아이디 (큐펫 버프[체력]) (40003)
        /// </summary>
        BuffSkill3 = 15,

        /// <summary>
        /// 버프스킬 4 아이디 (큐펫 버프[방어]) (40004)
        /// </summary>
        BuffSkill4 = 16,

        /// <summary>
        /// 버프스킬 5 아이디 (포링 포탑[체력]) (40005)
        /// </summary>
        BuffSkill5 = 17,

        /// <summary>
        /// 길드전 버프 최대 스킬 (20)
        /// </summary>
        BuffSkillMaxLevel = 18,

        /// <summary>
        /// 엠펠리움 체력
        /// </summary>
        EmperiumMaxHp = 19,
    }
}