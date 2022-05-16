using UnityEngine;

namespace Ragnarok
{
    public enum BattleOptionType
    {
        None = 0,

        /// <summary>
        /// 물리 대미지
        /// </summary>
        Damage = 1,
        /// <summary>
        /// 마법 대미지
        /// </summary>
        MDamage = 2,
        /// <summary>
        /// 체력 회복 (지력% 참조)
        /// </summary>
        RecoveryHp = 3,
        /// <summary>
        /// 체력 흡수 (대미지% 참조)
        /// </summary>
        AbsorbHp = 4,
        /// <summary>
        /// 체력 회복 (체력% 참조)
        /// </summary>
        RecoveryTotalHp = 5,
        /// <summary>
        /// 상태이상
        /// </summary>
        CrowdControl = 6,

        /// <summary>
        /// 모든 스탯 증감
        /// </summary>
        AllStatus = 100,
        /// <summary>
        /// 힘 증감
        /// </summary>
        Str = 101,
        /// <summary>
        /// 민첩 (어질) 증감
        /// </summary>
        Agi = 102,
        /// <summary>
        /// 체력 (바이탈) 증감
        /// </summary>
        Vit = 103,
        /// <summary>
        /// 지력 (인트) 증감
        /// </summary>
        Int = 104,
        /// <summary>
        /// 재주 (덱스) 증감
        /// </summary>
        Dex = 105,
        /// <summary>
        /// 운 (럭) 증감
        /// </summary>
        Luk = 106,
        /// <summary>
        /// 최대 체력 증감
        /// </summary>
        MaxHp = 107,
        /// <summary>
        /// 체력 재생 증감
        /// </summary>
        RegenHpRate = 108,
        /// <summary>
        /// 물리 공격력 증감
        /// </summary>
        Atk = 109,
        /// <summary>
        /// 마법 공격력 증감
        /// </summary>
        MAtk = 110,
        /// <summary>
        /// 물리 방어력 증감
        /// </summary>
        Def = 111,
        /// <summary>
        /// 마법 방어력 증감
        /// </summary>
        MDef = 112,
        /// <summary>
        /// 명중률 증감
        /// </summary>
        Hit = 113,
        /// <summary>
        /// 회피율 증감
        /// </summary>
        Flee = 114,
        /// <summary>
        /// 공격속도 증감
        /// </summary>
        AtkSpd = 115,
        /// <summary>
        /// 치명타 확률 증감
        /// </summary>
        CriRate = 116,
        /// <summary>
        /// 치명타 확률 저항 증감
        /// </summary>
        CriRateResist = 117,
        /// <summary>
        /// 치명타 증폭대미지 비율 증감
        /// </summary>
        CriDmgRate = 118,
        /// <summary>
        /// (모든 공격) 사정 거리 증감
        /// </summary>
        AtkRange = 119,
        /// <summary>
        /// 이동속도 증감
        /// </summary>
        MoveSpd = 120,
        /// <summary>
        /// 쿨타임 감소 증감
        /// </summary>
        CooldownRate = 121,
        /// <summary>
        /// 증폭대미지 비율 증감
        /// </summary>
        DmgRate = 122,
        /// <summary>
        /// 증폭대미지 비율 저항 증감
        /// </summary>
        DmgRateResist = 123,
        /// <summary>
        /// 근거리 증폭대미지 비율 증감
        /// </summary>
        MeleeDmgRate = 124,
        /// <summary>
        /// 근거리 증폭대미지 비율 저항 증감
        /// </summary>
        MeleeDmgRateResist = 125,
        /// <summary>
        /// 원거리 증폭대미지 비율 증감
        /// </summary>
        RangedDmgRate = 126,
        /// <summary>
        /// 원거리 증폭대미지 비율 저항 증감
        /// </summary>
        RangedDmgRateResist = 127,
        /// <summary>
        /// 일반 몬스터 증폭대미지 비율 증감
        /// </summary>
        NormalMonsterDmgRate = 128,
        /// <summary>
        /// 보스 몬스터 증폭대미지 비율 증감
        /// </summary>
        BossMonsterDmgRate = 129,
        /// <summary>
        /// 소형 몬스터 증폭대미지 비율 증감
        /// </summary>
        SmallMonsterDmgRate = 130,
        /// <summary>
        /// 중형 몬스터 증폭대미지 비율 증감
        /// </summary>
        MediumMonsterDmgRate = 131,
        /// <summary>
        /// 대형 몬스터 증폭대미지 비율 증감
        /// </summary>
        LargeMonsterDmgRate = 132,
        /// <summary>
        /// 모든 상태이상 확률 저항 증감
        /// </summary>
        AllCrowdControlRateResist = 133,
        /// <summary>
        /// 특정 상태이상 확률 저항 증감
        /// </summary>
        CrowdControlRateResist = 134,
        /// <summary>
        /// 평타 시 모든 상태이상 확률 증감
        /// </summary>
        BasicActiveSkillAllCrowdControlRate = 135,
        /// <summary>
        /// 평타 시 특정 상태이상 확률 증감
        /// </summary>
        BasicActiveSkillCrowdControlRate = 136,
        /// <summary>
        /// 모든 속성 증폭대미지 비율 증감
        /// </summary>
        AllElementDmgRate = 137,
        /// <summary>
        /// 특정 속성 증폭대미지 비율 증감
        /// </summary>
        ElementDmgRate = 138,
        /// <summary>
        /// 모든 속성 증폭대미지 비율 저항 증감
        /// </summary>
        AllElementDmgRateResist = 139,
        /// <summary>
        /// 특정 속성 증폭대미지 비율 저항 증감
        /// </summary>
        ElementDmgRateResist = 140,
        /// <summary>
        /// 피격 시 공격 무시 확률 증감
        /// </summary>
        AutoGuard = 141,
        /// <summary>
        /// 일반 경험치 드랍률 증감
        /// </summary>
        ExpDropRate = 142,
        /// <summary>
        /// 직업 경험치 드랍률 증감
        /// </summary>
        JobExpDropRate = 143,
        /// <summary>
        /// 제니 드랍률 증감
        /// </summary>
        ZenyDropRate = 144,
        /// <summary>
        /// 아이템 드랍률 증감
        /// </summary>
        ItemDropRate = 145,
        /// <summary>
        /// 몬스터 조각 드랍률 증감
        /// </summary>
        MonsterPieceDropRate = 146,
        /// <summary>
        /// 특정 스킬 id 증폭대미지 비율 증감
        /// </summary>
        SkillIdDmgRate = 147,
        /// <summary>
        /// 화속성 공격 시 특정 스킬 id 비율 증감 (파이어 필라)
        /// </summary>
        FirePillar = 148,
        /// <summary>
        /// 동행
        /// </summary>
        Colleague = 149,
        /// <summary>
        /// 평타 스킬 id 변경
        /// </summary>
        BasicActiveSkillRate = 150,
        /// <summary>
        /// 스킬 참조 후 자동 사용 확률 (서몬 볼)
        /// </summary>
        ActiveSkill = 151,
        /// <summary>
        /// 헌신
        /// </summary>
        Devotion = 152,
        /// <summary>
        /// 룬 마스터리
        /// </summary>
        RuneMastery = 153,
        /// <summary>
        /// 셰어바이스 최대 레벨 증가
        /// </summary>
        ShareBoost = 154,
        /// <summary>
        /// 셰어 정산 보상 증가
        /// </summary>
        ShareReward = 155,
        /// <summary>
        /// 초당 마나 재생률
        /// </summary>
        RegenMpRate = 156,
        /// <summary>
        /// 스킬 초월
        /// </summary>
        SkillOverride = 157,
        /// <summary>
        /// 스킬 연계
        /// </summary>
        SkillChain = 158,

        /// <summary>
        /// 고정 대미지
        /// </summary>
        FixedDamage = 200,
    }

    public static class BattleOptionTypeExtension
    {
        public static string ToText(this BattleOptionType type)
        {
            switch (type)
            {
                case BattleOptionType.Damage:
                    return LocalizeKey._59000.ToText(); // 물리 대미지
                case BattleOptionType.MDamage:
                    return LocalizeKey._59001.ToText(); // 마법 대미지
                case BattleOptionType.RecoveryHp:
                    return LocalizeKey._59002.ToText(); // 체력 회복량
                case BattleOptionType.AbsorbHp:
                    return LocalizeKey._59003.ToText(); // 체력 흡수량
                case BattleOptionType.RecoveryTotalHp:
                    return LocalizeKey._59004.ToText(); // 체력 회복량
                case BattleOptionType.CrowdControl:
                    return LocalizeKey._59005.ToText(); // {LINK} 확률
                case BattleOptionType.AllStatus:
                    return LocalizeKey._59006.ToText(); // 기본 스탯
                case BattleOptionType.Str:
                    return LocalizeKey._59007.ToText(); // STR
                case BattleOptionType.Agi:
                    return LocalizeKey._59008.ToText(); // AGI
                case BattleOptionType.Vit:
                    return LocalizeKey._59009.ToText(); // VIT
                case BattleOptionType.Int:
                    return LocalizeKey._59010.ToText(); // INT
                case BattleOptionType.Dex:
                    return LocalizeKey._59011.ToText(); // DEX
                case BattleOptionType.Luk:
                    return LocalizeKey._59012.ToText(); // LUK
                case BattleOptionType.MaxHp:
                    return LocalizeKey._59013.ToText(); // Max HP
                case BattleOptionType.RegenHpRate:
                    return LocalizeKey._59014.ToText(); // 체력 재생 증가율
                case BattleOptionType.Atk:
                    return LocalizeKey._59015.ToText(); // ATK
                case BattleOptionType.MAtk:
                    return LocalizeKey._59016.ToText(); // MATK
                case BattleOptionType.Def:
                    return LocalizeKey._59017.ToText(); // DEF
                case BattleOptionType.MDef:
                    return LocalizeKey._59018.ToText(); // MDEF
                case BattleOptionType.Hit:
                    return LocalizeKey._59019.ToText(); // Hit
                case BattleOptionType.Flee:
                    return LocalizeKey._59020.ToText(); // Flee
                case BattleOptionType.AtkSpd:
                    return LocalizeKey._59021.ToText(); // 공격 속도
                case BattleOptionType.CriRate:
                    return LocalizeKey._59022.ToText(); // 치명타 확률
                case BattleOptionType.CriRateResist:
                    return LocalizeKey._59023.ToText(); // 치명타 확률 저항
                case BattleOptionType.CriDmgRate:
                    return LocalizeKey._59024.ToText(); // 치명타 대미지 증가율
                case BattleOptionType.AtkRange:
                    return LocalizeKey._59025.ToText(); // 사정 거리
                case BattleOptionType.MoveSpd:
                    return LocalizeKey._59026.ToText(); // 이동 속도
                case BattleOptionType.CooldownRate:
                    return LocalizeKey._59027.ToText(); // 스킬 대기 시간 감소율
                case BattleOptionType.DmgRate:
                    return LocalizeKey._59028.ToText(); // 전체 대미지 증가율
                case BattleOptionType.DmgRateResist:
                    return LocalizeKey._59029.ToText(); // 전체 대미지 저항율
                case BattleOptionType.MeleeDmgRate:
                    return LocalizeKey._59030.ToText(); // 근거리 대미지 증가율
                case BattleOptionType.MeleeDmgRateResist:
                    return LocalizeKey._59031.ToText(); // 근거리 대미지 저항률
                case BattleOptionType.RangedDmgRate:
                    return LocalizeKey._59032.ToText(); // 원거리 대미지 증가율
                case BattleOptionType.RangedDmgRateResist:
                    return LocalizeKey._59033.ToText(); // 원거리 대미지 저항률
                case BattleOptionType.NormalMonsterDmgRate:
                    return LocalizeKey._59034.ToText(); // 일반 몬스터 대미지 증가율
                case BattleOptionType.BossMonsterDmgRate:
                    return LocalizeKey._59035.ToText(); // 보스 몬스터 대미지 증가율
                case BattleOptionType.SmallMonsterDmgRate:
                    return LocalizeKey._59036.ToText(); // 소형 몬스터 대미지 증가율
                case BattleOptionType.MediumMonsterDmgRate:
                    return LocalizeKey._59037.ToText(); // 중형 몬스터 대미지 증가율
                case BattleOptionType.LargeMonsterDmgRate:
                    return LocalizeKey._59038.ToText(); // 대형 몬스터 대미지 증가율
                case BattleOptionType.AllCrowdControlRateResist:
                    return LocalizeKey._59039.ToText(); // 모든 상태 이상 저항률
                case BattleOptionType.CrowdControlRateResist:
                    return LocalizeKey._59040.ToText(); // {LINK} 저항률
                case BattleOptionType.BasicActiveSkillAllCrowdControlRate:
                    return LocalizeKey._59041.ToText(); // 기본 공격 시 모든 상태 이상 확률
                case BattleOptionType.BasicActiveSkillCrowdControlRate:
                    return LocalizeKey._59042.ToText(); // 기본 공격 시 {LINK} 확률
                case BattleOptionType.AllElementDmgRate:
                    return LocalizeKey._59043.ToText(); // 모든 속성 대미지 증가율
                case BattleOptionType.ElementDmgRate:
                    return LocalizeKey._59044.ToText(); // {TYPE} 대미지 증가율
                case BattleOptionType.AllElementDmgRateResist:
                    return LocalizeKey._59045.ToText(); // 모든 속성 대미지 저항률
                case BattleOptionType.ElementDmgRateResist:
                    return LocalizeKey._59046.ToText(); // {TYPE} 대미지 저항률
                case BattleOptionType.AutoGuard:
                    return LocalizeKey._59047.ToText(); // 피해 무시 확률
                case BattleOptionType.ExpDropRate:
                    return LocalizeKey._59048.ToText(); // 일반 경험치 드랍률
                case BattleOptionType.JobExpDropRate:
                    return LocalizeKey._59049.ToText(); // 직업 경험치 드랍률
                case BattleOptionType.ZenyDropRate:
                    return LocalizeKey._59050.ToText(); // 제니 드랍률
                case BattleOptionType.ItemDropRate:
                    return LocalizeKey._59051.ToText(); // 아이템 드랍률
                case BattleOptionType.MonsterPieceDropRate:
                    return LocalizeKey._59052.ToText(); // 몬스터 조각 드랍률
                case BattleOptionType.SkillIdDmgRate:
                    return LocalizeKey._59053.ToText(); // {LINK} 대미지 증가율
                case BattleOptionType.FirePillar:
                    return LocalizeKey._59054.ToText(); // 화속성 공격 시 {LINK} 발동률
                case BattleOptionType.Colleague:
                    return LocalizeKey._59055.ToText(); // {LINK} 동행
                case BattleOptionType.BasicActiveSkillRate:
                    return LocalizeKey._59056.ToText(); // 기본 공격 시 {LINK} 발동률
                case BattleOptionType.ActiveSkill:
                    return LocalizeKey._59057.ToText(); // 공격시 {LINK} 발동률
                case BattleOptionType.Devotion:
                    return LocalizeKey._59058.ToText(); // 아군 피해 흡수 감소율
                case BattleOptionType.RuneMastery:
                    return LocalizeKey._59059.ToText(); // 룬 스톤 제조
                case BattleOptionType.ShareBoost:
                    return LocalizeKey._59060.ToText(); // 셰어바이스 레벨
                case BattleOptionType.ShareReward:
                    return LocalizeKey._59061.ToText(); // 셰어 정산 보상 증가율
                case BattleOptionType.RegenMpRate:
                    return LocalizeKey._59062.ToText(); // 초당 마나 재생율
                case BattleOptionType.SkillOverride:
                    return LocalizeKey._59063.ToText(); // {LINK} 초월
                case BattleOptionType.SkillChain:
                    return LocalizeKey._59064.ToText(); // {LINK} 사용 시 연계

                default:
                    Debug.LogError($"[정의되지 않은 옵션 이름 {nameof(BattleOptionType)}] {nameof(type)} = {type}");
                    return string.Empty;
            }
        }

        public static string ToValueCustomText(this BattleOptionType type, int skillBlowCount = 0)
        {
            switch (type)
            {
                case BattleOptionType.Damage:
                    if (skillBlowCount > 1)
                        return LocalizeKey._59509.ToText(); // {VALUE} x {COUNT}
                    return LocalizeKey._59500.ToText(); // {VALUE}

                case BattleOptionType.MDamage:
                    if (skillBlowCount > 1)
                        return LocalizeKey._59509.ToText(); // {VALUE} x {COUNT}
                    return LocalizeKey._59500.ToText(); // {VALUE}

                case BattleOptionType.RecoveryHp:
                case BattleOptionType.AbsorbHp:
                case BattleOptionType.RecoveryTotalHp:
                    return LocalizeKey._59500.ToText(); // {VALUE}

                case BattleOptionType.CrowdControl:
                    return LocalizeKey._59501.ToText(); // {VALUE}%
            }

            return string.Empty;
        }

        public static string ToPerValueCustomText(this BattleOptionType type, int skillBlowCount = 0)
        {
            switch (type)
            {
                case BattleOptionType.Damage:
                    if (skillBlowCount > 1)
                        return LocalizeKey._59507.ToText(); // ATK {VALUE}% x {COUNT}
                    return LocalizeKey._59502.ToText(); // ATK {VALUE}%

                case BattleOptionType.MDamage:
                    if (skillBlowCount > 1)
                        return LocalizeKey._59508.ToText(); // MATK {VALUE}% x {COUNT}
                    return LocalizeKey._59503.ToText(); // MATK {VALUE}%

                case BattleOptionType.RecoveryHp:
                    return LocalizeKey._59504.ToText(); // INT {VALUE}%

                case BattleOptionType.AbsorbHp:
                    return LocalizeKey._59505.ToText(); // Damage {VALUE}%

                case BattleOptionType.RecoveryTotalHp:
                    return LocalizeKey._59506.ToText(); // MaxHP {VALUE}%
            }

            return string.Empty;
        }

        public static string ToSpriteName(this BattleOptionType type)
        {
            switch (type)
            {
                default:
                case BattleOptionType.None:
                    return string.Empty;

                case BattleOptionType.AllStatus:
                case BattleOptionType.Str:
                case BattleOptionType.Agi:
                case BattleOptionType.Vit:
                case BattleOptionType.Int:
                case BattleOptionType.Dex:
                case BattleOptionType.Luk:
                    return "Ui_Common_Icon_SlotOption_01";

                case BattleOptionType.MaxHp:
                case BattleOptionType.RegenHpRate:
                    return "Ui_Common_Icon_SlotOption_02";

                case BattleOptionType.Hit:
                case BattleOptionType.Flee:
                case BattleOptionType.AtkSpd:
                case BattleOptionType.AtkRange:
                case BattleOptionType.DmgRate:
                    return "Ui_Common_Icon_SlotOption_03";

                case BattleOptionType.CriRate:
                case BattleOptionType.CriDmgRate:
                    return "Ui_Common_Icon_SlotOption_04";

                case BattleOptionType.MoveSpd:
                    return "Ui_Common_Icon_SlotOption_05";

                case BattleOptionType.Def:
                case BattleOptionType.MDef:
                case BattleOptionType.CriRateResist:
                case BattleOptionType.DmgRateResist:
                case BattleOptionType.MeleeDmgRateResist:
                case BattleOptionType.RangedDmgRateResist:
                case BattleOptionType.AllCrowdControlRateResist:
                    return "Ui_Common_Icon_SlotOption_06";

                case BattleOptionType.CooldownRate:
                    return "Ui_Common_Icon_SlotOption_07";

                case BattleOptionType.Atk:
                case BattleOptionType.MeleeDmgRate:
                    return "Ui_Common_Icon_SlotOption_08";

                case BattleOptionType.RangedDmgRate:
                    return "Ui_Common_Icon_SlotOption_09";

                case BattleOptionType.MAtk:
                    return "Ui_Common_Icon_SlotOption_10";

                case BattleOptionType.AllElementDmgRate:
                case BattleOptionType.ElementDmgRate:
                    return "Ui_Common_Icon_SlotOption_11";

                case BattleOptionType.AllElementDmgRateResist:
                case BattleOptionType.ElementDmgRateResist:
                    return "Ui_Common_Icon_SlotOption_12";

                case BattleOptionType.NormalMonsterDmgRate:
                case BattleOptionType.BossMonsterDmgRate:
                    return "Ui_Common_Icon_SlotOption_13";
            }
        }

        public static bool IsActiveOption(this BattleOptionType type)
        {
            switch (type)
            {
                case BattleOptionType.Damage:
                case BattleOptionType.MDamage:
                case BattleOptionType.RecoveryHp:
                case BattleOptionType.AbsorbHp:
                case BattleOptionType.RecoveryTotalHp:
                case BattleOptionType.CrowdControl:
                case BattleOptionType.FixedDamage:
                    return true;
            }

            return false;
        }

        public static bool IsConditionalOption(this BattleOptionType type)
        {
            switch (type)
            {
                case BattleOptionType.CrowdControl:
                case BattleOptionType.CrowdControlRateResist:
                case BattleOptionType.BasicActiveSkillCrowdControlRate:
                case BattleOptionType.ElementDmgRate:
                case BattleOptionType.ElementDmgRateResist:
                case BattleOptionType.SkillIdDmgRate:
                case BattleOptionType.FirePillar:
                case BattleOptionType.Colleague:
                case BattleOptionType.BasicActiveSkillRate:
                case BattleOptionType.ActiveSkill:
                case BattleOptionType.SkillOverride:
                case BattleOptionType.SkillChain:
                    return true;
            }

            return false;
        }

        public static bool IsConditionalSkill(this BattleOptionType type)
        {
            switch (type)
            {
                case BattleOptionType.SkillOverride:
                case BattleOptionType.SkillChain:
                    return true;
            }

            return false;
        }

        public static bool IsRateOption(this BattleOptionType type)
        {
            switch (type)
            {
                case BattleOptionType.CrowdControl:
                case BattleOptionType.RegenHpRate:
                case BattleOptionType.AtkSpd:
                case BattleOptionType.CriRate:
                case BattleOptionType.CriRateResist:
                case BattleOptionType.CriDmgRate:
                case BattleOptionType.AtkRange:
                case BattleOptionType.MoveSpd:
                case BattleOptionType.CooldownRate:
                case BattleOptionType.DmgRate:
                case BattleOptionType.DmgRateResist:
                case BattleOptionType.MeleeDmgRate:
                case BattleOptionType.MeleeDmgRateResist:
                case BattleOptionType.RangedDmgRate:
                case BattleOptionType.RangedDmgRateResist:
                case BattleOptionType.NormalMonsterDmgRate:
                case BattleOptionType.BossMonsterDmgRate:
                case BattleOptionType.SmallMonsterDmgRate:
                case BattleOptionType.MediumMonsterDmgRate:
                case BattleOptionType.LargeMonsterDmgRate:
                case BattleOptionType.AllCrowdControlRateResist:
                case BattleOptionType.CrowdControlRateResist:
                case BattleOptionType.BasicActiveSkillAllCrowdControlRate:
                case BattleOptionType.BasicActiveSkillCrowdControlRate:
                case BattleOptionType.AllElementDmgRate:
                case BattleOptionType.ElementDmgRate:
                case BattleOptionType.AllElementDmgRateResist:
                case BattleOptionType.ElementDmgRateResist:
                case BattleOptionType.AutoGuard:
                case BattleOptionType.ExpDropRate:
                case BattleOptionType.JobExpDropRate:
                case BattleOptionType.ZenyDropRate:
                case BattleOptionType.ItemDropRate:
                case BattleOptionType.MonsterPieceDropRate:
                case BattleOptionType.SkillIdDmgRate:
                case BattleOptionType.FirePillar:
                case BattleOptionType.BasicActiveSkillRate:
                case BattleOptionType.ActiveSkill:
                case BattleOptionType.Devotion:
                case BattleOptionType.ShareReward:
                    return true;
            }

            return false;
        }
    }
}