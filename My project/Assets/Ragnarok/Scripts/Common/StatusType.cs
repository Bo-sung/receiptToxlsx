namespace Ragnarok
{
    public enum StatusType
    {
        /// <summary>
        /// 힘
        /// </summary>
        Str,
        /// <summary>
        /// 민첩 (어질)
        /// </summary>
        Agi,
        /// <summary>
        /// 체력 (바이탈)
        /// </summary>
        Vit,
        /// <summary>
        /// 지력 (인트)
        /// </summary>
        Int,
        /// <summary>
        /// 재주 (덱스)
        /// </summary>
        Dex,
        /// <summary>
        /// 운 (럭)
        /// </summary>
        Luk,

        /// <summary>
        /// 현재 채력
        /// </summary>
        CurrentHP,
        /// <summary>
        /// 체력
        /// </summary>
        MaxHP,
        /// <summary>
        /// 체력 재생
        /// </summary>
        RegenHP,

        /// <summary>
        /// 기본 근거리 물리 공격력
        /// </summary>
        BasicMeleeAtk,
        /// <summary>
        /// 기본 원거리 물리 공격력
        /// </summary>
        BasicRangedAtk,
        /// <summary>
        /// 무기 물리 공격력
        /// </summary>
        WeaponAtk,
        /// <summary>
        /// 장착 장비 물리 공격력
        /// </summary>
        TotalItemAtk,
        /// <summary>
        /// 추가 물리 공격력
        /// </summary>
        AddAtk,
        /// <summary>
        /// 추가 물리 공격력 (퍼센트)
        /// </summary>
        AddAtkPer,
        /// <summary>
        /// 보여주기 용 근거리 물리 공격력 (전투에 관여하지 않는 수치)
        /// </summary>
        DisplayMeleeAtk,
        /// <summary>
        /// 보여주기 용 원거리 물리 공격력 (전투에 관여하지 않는 수치)
        /// </summary>
        DisplayRangedAtk,
        /// <summary>
        /// 마법 공격력
        /// </summary>
        MAtk,
        /// <summary>
        /// 물리 방어력
        /// </summary>
        Def,
        /// <summary>
        /// 마법 방어력
        /// </summary>
        MDef,
        /// <summary>
        /// 명중률
        /// </summary>
        Hit,
        /// <summary>
        /// 회피율
        /// </summary>
        Flee,
        /// <summary>
        /// 공격속도
        /// </summary>
        AtkSpd,
        /// <summary>
        /// 치명타 확률
        /// </summary>
        CriRate,
        /// <summary>
        /// 치명타 확률 저항
        /// </summary>
        CriRateResist,
        /// <summary>
        /// 치명타 증폭대미지 비율
        /// </summary>
        CriDmgRate,
        /// <summary>
        /// 사정 거리 (모든 공격)
        /// </summary>
        AtkRange,
        /// <summary>
        /// 이동속도
        /// </summary>
        MoveSpd,
        /// <summary>
        /// 쿨타임 감소
        /// </summary>
        CooldownRate,

        /// <summary>
        /// 증폭대미지 비율
        /// </summary>
        DmgRate,
        /// <summary>
        /// 증폭대미지 비율 저항
        /// </summary>
        DmgRateResist,
        /// <summary>
        /// 근거리 증폭대미지 비율
        /// </summary>
        MeleeDmgRate,
        /// <summary>
        /// 근거리 증폭대미지 비율 저항
        /// </summary>
        MeleeDmgRateResist,
        /// <summary>
        /// 원거리 증폭대미지 비율
        /// </summary>
        RangedDmgRate,
        /// <summary>
        /// 원거리 증폭대미지 비율 저항
        /// </summary>
        RangedDmgRateResist,
        /// <summary>
        /// 일반 몬스터 증폭대미지 비율
        /// </summary>
        NormalMonsterDmgRate,
        /// <summary>
        /// 보스 몬스터 증폭대미지 비율
        /// </summary>
        BossMonsterDmgRate,
        /// <summary>
        /// 소형 몬스터 증폭대미지 비율
        /// </summary>
        SmallMonsterDmgRate,
        /// <summary>
        /// 중형 몬스터 증폭대미지 비율
        /// </summary>
        MediumMonsterDmgRate,
        /// <summary>
        /// 대형 몬스터 증폭대미지 비율
        /// </summary>
        LargeMonsterDmgRate,

        /// <summary>
        /// 스턴 확률 저항
        /// </summary>
        StunRateResist,
        /// <summary>
        /// 침묵 확률 저항
        /// </summary>
        SilenceRateResist,
        /// <summary>
        /// 수면 확률 저항
        /// </summary>
        SleepRateResist,
        /// <summary>
        /// 환각 확률 저항
        /// </summary>
        HallucinationRateResist,
        /// <summary>
        /// 출혈 확률 저항
        /// </summary>
        BleedingRateResist,
        /// <summary>
        /// 화상 확률 저항
        /// </summary>
        BurningRateResist,
        /// <summary>
        /// 독 확률 저항
        /// </summary>
        PoisonRateResist,
        /// <summary>
        /// 저주 확률 저항
        /// </summary>
        CurseRateResist,
        /// <summary>
        /// 빙결 확률 저항
        /// </summary>
        FreezingRateResist,
        /// <summary>
        /// 동빙 확률 저항
        /// </summary>
        FrozenRateResist,

        /// <summary>
        /// 평타 시 스턴 확률
        /// </summary>
        BasicActiveSkillStunRate,
        /// <summary>
        /// 평타 시 침묵 확률
        /// </summary>
        BasicActiveSkillSilenceRate,
        /// <summary>
        /// 평타 시 수면 확률
        /// </summary>
        BasicActiveSkillSleepRate,
        /// <summary>
        /// 평타 시 환각 확률
        /// </summary>
        BasicActiveSkillHallucinationRate,
        /// <summary>
        /// 평타 시 출혈 확률
        /// </summary>
        BasicActiveSkillBleedingRate,
        /// <summary>
        /// 평타 시 화상 확률
        /// </summary>
        BasicActiveSkillBurningRate,
        /// <summary>
        /// 평타 시 독 확률
        /// </summary>
        BasicActiveSkillPoisonRate,
        /// <summary>
        /// 평타 시 저주 확률
        /// </summary>
        BasicActiveSkillCurseRate,
        /// <summary>
        /// 평타 시 빙결 확률
        /// </summary>
        BasicActiveSkillFreezingRate,
        /// <summary>
        /// 평타 시 동빙 확률
        /// </summary>
        BasicActiveSkillFrozenRate,

        /// <summary>
        /// 무속성 증폭대미지 비율
        /// </summary>
        NeutralDmgRate,
        /// <summary>
        /// 무속성 증폭대미지 비율 저항
        /// </summary>
        NeutralDmgRateResist,
        /// <summary>
        /// 화속성 증폭대미지 비율
        /// </summary>
        FireDmgRate,
        /// <summary>
        /// 화속성 증폭대미지 비율 저항
        /// </summary>
        FireDmgRateResist,
        /// <summary>
        /// 수속성 증폭대미지 비율
        /// </summary>
        WaterDmgRate,
        /// <summary>
        /// 수속성 증폭대미지 비율 저항
        /// </summary>
        WaterDmgRateResist,
        /// <summary>
        /// 풍속성 증폭대미지 비율
        /// </summary>
        WindDmgRate,
        /// <summary>
        /// 풍속성 증폭대미지 비율 저항
        /// </summary>
        WindDmgRateResist,
        /// <summary>
        /// 지속성 증폭대미지 비율
        /// </summary>
        EarthDmgRate,
        /// <summary>
        /// 지속성 증폭대미지 비율 저항
        /// </summary>
        EarthDmgRateResist,
        /// <summary>
        /// 독속성 증폭대미지 비율
        /// </summary>
        PoisonDmgRate,
        /// <summary>
        /// 독속성 증폭대미지 비율 저항
        /// </summary>
        PoisonDmgRateResist,
        /// <summary>
        /// 성속성 증폭대미지 비율
        /// </summary>
        HolyDmgRate,
        /// <summary>
        /// 성속성 증폭대미지 비율 저항
        /// </summary>
        HolyDmgRateResist,
        /// <summary>
        /// 암속성 증폭대미지 비율
        /// </summary>
        ShadowDmgRate,
        /// <summary>
        /// 암속성 증폭대미지 비율 저항
        /// </summary>
        ShadowDmgRateResist,
        /// <summary>
        /// 염속성 증폭대미지 비율
        /// </summary>
        GhostDmgRate,
        /// <summary>
        /// 염속성 증폭대미지 비율 저항
        /// </summary>
        GhostDmgRateResist,
        /// <summary>
        /// 사속성 증폭대미지 비율
        /// </summary>
        UndeadDmgRate,
        /// <summary>
        /// 사속성 증폭대미지 비율 저항
        /// </summary>
        UndeadDmgRateResist,
        /// <summary>
        /// 피격 시 공격 무시 확률
        /// </summary>
        AutoGuard,
        /// <summary>
        /// 일반 경험치 드랍률
        /// </summary>
        ExpDropRate,
        /// <summary>
        /// 직업 경험치 드랍률
        /// </summary>
        JobExpDropRate,
        /// <summary>
        /// 제니 드랍률
        /// </summary>
        ZenyDropRate,
        /// <summary>
        /// 아이템 드랍률
        /// </summary>
        ItemDropRate,
        /// <summary>
        /// 몬스터 조각 드랍률
        /// </summary>
        MonsterPieceDropRate,

        /// <summary>
        /// 특정 스킬 id 증폭대미지 비율 증감
        /// </summary>
        SkillIdDmgRate,

        /// <summary>
        /// 화속성 공격 시 불기둥 소환률 (파이어 필라)
        /// </summary>
        FirePillar,

        /// <summary>
        /// 헌신
        /// </summary>
        Devotion,
        /// <summary>
        /// 소환
        /// </summary>
        Summon,
        /// <summary>
        /// 평타 스킬
        /// </summary>
        BasicActiveSkill,
        /// <summary>
        /// 평타 스킬 참조 후 자동 사용 (서몬 볼)
        /// </summary>
        UseSummonBall,
        
        /// <summary>
        /// 룬 마스터리
        /// </summary>
        RuneMastery,
    }
}