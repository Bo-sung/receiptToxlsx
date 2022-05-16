namespace Ragnarok
{
    public enum BasisForestMazeInfo
    {
        /// <summary>
        /// 미궁숲 전체 플레이 타임 (밀리초) (600000)
        /// </summary>
        PlayTime = 1,

        /// <summary>
        /// 바포메트 도전 조건 엠펠리움 조각 수량 (9)
        /// </summary>
        NeedEmperiumCount = 2,

        /// <summary>
        /// 엠펠리움 조각 획득 최대치 (17)
        /// </summary>
        MaxEmperiumCount = 3,

        /// <summary>
        /// 엠펠리움 조각 초과 획득 시 차감 바포메트 레벨 (5)
        /// </summary>
        LevelDecreaseValue = 4,

        /// <summary>
        /// 포션 획득 시 체력 회복 퍼센트 (백분율) (10)
        /// </summary>
        HpRegenRate = 5,

        /// <summary>
        /// 초승달 반지, 대미지 저항 퍼센트(백분율) (50)
        /// </summary>
        DmgRateResist = 6,

        /// <summary>
        /// 마제스틱 고우트, 대미지 증가 퍼센트(백분율) (100)
        /// </summary>
        DmgRate = 7,

        /// <summary>
        /// 이그드라실 씨앗, 사용할 수 있는 스킬(스킬 id) (60001)
        /// </summary>
        UseSkillId = 8,

        /// <summary>
        /// 악마의 뿔, 교체되는 보스 몬스터(몬스터 id) (54001)
        /// </summary>
        ChangeBossMonsterId = 9,

        /// <summary>
        /// 최종 보스 몬스터 전투 시간(밀리초) (180000)
        /// </summary>
        BossPlayTime = 10,
    }
}