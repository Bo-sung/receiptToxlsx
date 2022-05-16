namespace Ragnarok
{
    public interface IDamageTuple
    {
        /******************** 타겟 (스탯) ********************/
        int Def { get; } // 물방
        int Mdef { get; } // 마방
        int DmgRateResist { get; } // 증댐저항
        int MeleeDmgRateResist { get; } // 근거리물증댐저항
        int RangedDmgRateResist { get; } // 원거리물증댐저항
        int NeutralDmgRateResist { get; } // 무속성증댐저항
        int FireDmgRateResist { get; } // 화속성증댐저항
        int WaterDmgRateResist { get; } // 수속성증댐저항
        int WindDmgRateResist { get; } // 풍속성증댐저항
        int EarthDmgRateResist { get; } // 지속성증댐저항
        int PoisonDmgRateResist { get; } // 독속성증댐저항
        int HolyDmgRateResist { get; } // 성속성증댐저항
        int ShadowDmgRateResist { get; } // 암속성증댐저항
        int GhostDmgRateResist { get; } // 염속성증댐저항
        int UndeadDmgRateResist { get; } // 사속성증댐저항

        /******************** 타겟 (상태이상) ********************/
        int DefDecreaseRate { get; } // 타겟 (상태이상)
        int MdefDecreaseRate { get; } // 타겟 (상태이상)

        /******************** 시전자 (스탯) ********************/
        int Str { get; } // 힘
        int Agi { get; } // 민첩_어질
        int Vit { get; } // 체력_바이탈
        int Inte { get; } // 지력_인트
        int Dex { get; } // 재주_덱스
        int Luk { get; } // 운_럭
        int MeleeAtk { get; } // 근거리물공
        int RangedAtk { get; } // 원거리물공
        int Matk { get; } // 마공
        int CriDmgRate { get; } // 크리증댐
        int DmgRate { get; } // 증댐
        int MeleeDmgRate { get; } // 근거리물증댐
        int RangedDmgRate { get; } // 원거리물증댐
        int NormalMonsterDmgRate { get; } // 일반몹증댐
        int BossMonsterDmgRate { get; } // 보스몹증댐
        int SmallMonsterDmgRate { get; } // 소형몹증댐
        int MediumMonsterDmgRate { get; } // 중형몹증댐
        int LargeMonsterDmgRate { get; } // 대형몹증댐
        int NeutralDmgRate { get; } // 무속성증댐
        int FireDmgRate { get; } // 화속성증댐
        int WaterDmgRate { get; } // 수속성증댐
        int WindDmgRate { get; } // 풍속성증댐
        int EarthDmgRate { get; } // 지속성증댐
        int PoisonDmgRate { get; } // 독속성증댐
        int HolyDmgRate { get; } // 성속성증댐
        int ShadowDmgRate { get; } // 암속성증댐
        int GhostDmgRate { get; } // 염속성증댐
        int UndeadDmgRate { get; } // 사속성증댐
        int SkillDamageRate { get; } // 특정스킬증댐

        /******************** 시전자 (상태이상) ********************/
        int TotalDmgRateDecreaseRate { get; } // 시전자 (상태이상 - 전체 대미지 감소율)

        /******************** 계산 과정 (대미지 상세) ********************/
        int CriDamageRate { get; } // 치명타 증폭대미지 비율
        int ElementFactor { get; } // 속성간 대미지 배율 (기초데이터)
        int AttackerElementDamageRate { get; } // 시전자의 속성 증폭대미지 비율
        int TargetElementDamageRateResist { get; } // 타겟의 속성 증폭대미지 비율 저항
        int ElementDamageRate { get; } // 속성 증폭대미지 비율
        int DamageRate { get; } // 전체 증폭대미지 비율
        int DistDamageRate { get; } // 거리 증폭대미지 비율 = (시전자의 거리 증폭대미지 비율 - 타겟의 거리 증폭대미지 비율 저항)
        int MonsterDamageRate { get; } // 몬스터 증폭대미지 비율
        int AttackerWeaponType { get; } // 장착한 무기 타입
        int UnitSizeFactor { get; } // 유닛사이즈 대미지 배율 (기초 데이터)
        int UnitSizeDamageRate { get; } // 유닛사이즈 증폭대미지 비율

        int RawDamageValue { get; } // 시전자의 물리 공격력에 스킬 물리 공격 적용
        int DamageRandomValue { get; } // 랜덤 타격계수
        int DamageValue { get; } // 물리 공격력에 랜덤 타격계수 적용
        int TargetDef { get; } // 타겟 방어력
        int DamageDecreaseRate { get; } // 물리대미지감소 비율은 0에서 1사이
        int Damage { get; } // 물댐

        int AttackerMAtk { get; } // 시전자의 마법공격력
        int RawMDamageValue { get; } // 시전자의 마법 공격력에 스킬 마법 공격 적용
        int MdamageRandomValue { get; } // 랜덤 타격 계수
        int MdamageValue { get; } // 마법 공격력에 랜덤 타격계수 적용
        int TargetMDef { get; } // 방어력은 0 이상
        int MdamageDecreaseRate { get; } // 마법대미지감소 비율은 0에서 1사이
        int Mdamage { get; } // 마댐

        int SumDamage { get; } // 대미지총합 = (물리대미지 + 마법대미지)
        int DamageFactor { get; } // 대미지 배율 계산
        int NormalDamage { get; } // 대미지 배율 적용
        int SumPlusDamageRate { get; } // 증폭대미지 비율 총합
        int PlusDamage { get; } // 증폭대미지 비율 적용
        int RawTotalDamage { get; } // 시전자의 전체 대미지 비율 감소율 적용

        int TotalDamage { get; } // 최종댐 (추가댐 적용)
        int FinalDamage { get; } // 타겟에게 가해지는 대미지 (타격횟수 적용)

        /******************** 추가 상세 정보 ********************/
        int AttackerElementType { get; } // 시전자의 속성 타입
        int AttackerElementLevel { get; } // 시전자의 속성 레벨
    	int TargetElementType { get; } // 피격자의 속성 타입
        int TargetElementLevel { get; } // 피격자의 속성 레벨
        int SkillId { get; } // 스킬 id
        int SkillLevel { get; } // 스킬 Level
    }
}