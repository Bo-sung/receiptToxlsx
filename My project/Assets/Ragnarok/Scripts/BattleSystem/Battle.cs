using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// 전투 스탯 관리
    /// <para>
    /// <see cref="UnitEntity"/>
    /// <![CDATA[캐릭터]]><seealso cref="CharacterEntity"/>
    /// <![CDATA[몬스터]]><seealso cref="MonsterEntity"/>
    /// <![CDATA[큐펫]]><seealso cref="CupetEntity"/>
    /// </para>
    /// </summary>
    public static class Battle
    {
        public struct StatusInput
        {
            public int basicStr, basicAgi, basicVit, basicInt, basicDex, basicLuk; // 기본 스탯
            public int basicMoveSpd, basicAtkSpd, basicAtkRange; // 기본 스탯 (추가)
            public int hpCoefficient, atkCoefficient, matkCoefficient, defCoefficient, mdefCoefficient; // 스탯 계수
            public int attackSpeedPenalty; // 공속 패널티
        }

        public struct StatusOutput
        {
            public BattleStatusInfo.Settings statusSettings;
        }

        public struct SkillInput
        {
            /******************** 타겟 ********************/
            public BattleUnitInfo.ITargetValue unitTargetValue; // Unit 정보 (타겟)
            public BattleStatusInfo.ITargetValue statusTargetValue; // Status 정보 (타겟)
            public BattleCrowdControlInfo.ITargetValue crowdControlTargetValue; // 상태이상 정보 (타겟)

            /******************** 시전자 ********************/
            public BattleItemInfo.IAttackerValue itemAttackerValue; // Item 정보 (시전자)
            public BattleStatusInfo.IAttackerValue statusAttackerValue; // Status 정보 (시전자)
            public BattleCrowdControlInfo.IAttackerValue crowdControlAttackerValue; // 상태이상 정보 (시전자)

            /******************** 스킬 ********************/
            public SkillInfo.IValue skillValue; // Skill 정보
            public ElementType skillElementType; // 스킬 속성 정보 (평타의 경우에는 무기 속성 따라감)
            public int skillElementLevel; // 스킬 속성 레벨 정보 (평타의 경우에는 무기 속성 따라감)
            public IRandomDamage randomDamage; // 랜덤 정보
            public IElementDamage elementDamage; // 속성댐 정보
            public float dcecreaseDamageRatePer; // 대미지감소율 (보정값)
        }

        public struct SkillOutput
        {
            public bool isAutoGuard; // 타겟의 오토가드 성공여부
            public bool isFlee; // 타겟의 회피 성공여부
            public bool isCritical; // 타겟에게 가해지는 스킬 크리티컬 여부
            public bool hasDamage; // 대미지 존재
            public int totalDamage; // 타겟에게 가해지는 대미지 (추가댐 적용)
            public bool hasRecovery; // 회복 존재
            public int totalRecovery; // 타겟에게 회복되는 체력
            public bool hasAbsorbRecovery; // 흡수 존재
            public int absorbRecovery; // 시전자에게 흡수되는 체력
            public BattleCrowdControlInfo.ApplySettings crowdControlSettings; // 타겟의 상태이상 여부

            /******************** 서버 체크용 ********************/
            public int criDamageRate; // 치명타 증폭대미지 비율
            public int elementFactor; // 속성간 대미지 배율 (기초데이터)
            public int attackerElementDamageRate; // 시전자의 속성 증폭대미지 비율
            public int targetElementDamageRateResist; // 타겟의 속성 증폭대미지 비율 저항
            public int elementDamageRate; // 속성 증폭대미지 비율
            public int damageRate; //전체 증폭대미지 비율
            public int distDamageRate; // 거리 증폭대미지 비율 = (시전자의 거리 증폭대미지 비율 - 타겟의 거리 증폭대미지 비율 저항)
            public int monsterDamageRate; // 몬스터 증폭대미지 비율
            public int attackerWeaponType; // 장착한 무기 타입
            public int unitSizeFactor; // 유닛사이즈 대미지 배율 (기초 데이터)
            public int unitSizeDamageRate; // 유닛사이즈 증폭대미지 비율

            public int rawDamageValue; // 시전자의 물리 공격력에 스킬 물리 공격 적용
            public int damageRandomSeq; // 물댐 랜덤 시퀀스
            public int damageRandomValue; // 랜덤 타격계수
            public int damageValue; // 물리 공격력에 랜덤 타격계수 적용
            public int targetDef; // 타겟 방어력
            public float damageDecreaseRate; // 물리대미지감소 비율은 0에서 1사이
            public int damage; // 물댐

            public int attackerMAtk; // 시전자의 마법공격력
            public int rawMDamageValue; // 시전자의 마법 공격력에 스킬 마법 공격 적용
            public int mdamageRandomSeq; // 마댐 랜덤 시퀀스
            public int mdamageRandomValue; // 랜덤 타격 계수
            public int mdamageValue; // 마법 공격력에 랜덤 타격계수 적용
            public int targetMDef; // 방어력은 0 이상
            public float mdamageDecreaseRate; // 마법대미지감소 비율은 0에서 1사이
            public int mdamage; // 마댐

            public int sumDamage; // 대미지총합 = (물리대미지 + 마법대미지)
            public int damageFactor; // 대미지 배율 계산
            public int normalDamage; // 대미지 배율 적용
            public int sumPlusDamageRate; // 증폭대미지 비율 총합
            public int plusDamage; // 증폭대미지 비율 적용
            public int rawTotalDamage; // 시전자의 전체 대미지 비율 감소율 적용
            public int applyedDecreaseRateDamage; // 보정값이 적용된 대미지
        }

        public static StatusOutput ReloadStatus(StatusInput input, BattleUnitInfo.IValue unitValue, BattleItemInfo.IValue itemValue, BattleOptionList options, UnitEntity entity)
        {
#if UNITY_EDITOR
            entity.SetHeader($"{nameof(ReloadStatus)}");
#endif

            BattleStatus optionAllStatus = options.Get(BattleOptionType.AllStatus);
            BattleStatus optionStr = options.Get(BattleOptionType.Str) + optionAllStatus;
            BattleStatus optionAgi = options.Get(BattleOptionType.Agi) + optionAllStatus;
            BattleStatus optionVit = options.Get(BattleOptionType.Vit) + optionAllStatus;
            BattleStatus optionInt = options.Get(BattleOptionType.Int) + optionAllStatus;
            BattleStatus optionDex = options.Get(BattleOptionType.Dex) + optionAllStatus;
            BattleStatus optionLuk = options.Get(BattleOptionType.Luk) + optionAllStatus;

            int str = optionStr.GetStatusValue(input.basicStr);
            int agi = optionAgi.GetStatusValue(input.basicAgi);
            int vit = optionVit.GetStatusValue(input.basicVit);
            int @int = optionInt.GetStatusValue(input.basicInt);
            int dex = optionDex.GetStatusValue(input.basicDex);
            int luk = optionLuk.GetStatusValue(input.basicLuk);
#if UNITY_EDITOR
            entity.AddLog(nameof(str));
            entity.AddLog($"[{nameof(input.basicStr)}: 스탯] {input.basicStr}");
            entity.AddLog($"[{nameof(optionStr)}: {optionStr}] {optionStr.GetStatusValueText(input.basicStr)} = {str}");

            entity.AddLog(nameof(agi));
            entity.AddLog($"[{nameof(input.basicAgi)}: 스탯] {input.basicAgi}");
            entity.AddLog($"[{nameof(optionAgi)}: {optionAgi}] {optionAgi.GetStatusValueText(input.basicAgi)} = {agi}");

            entity.AddLog(nameof(vit));
            entity.AddLog($"[{nameof(input.basicVit)}: 스탯] {input.basicVit}");
            entity.AddLog($"[{nameof(optionVit)}: {optionVit}] {optionVit.GetStatusValueText(input.basicVit)} = {vit}");

            entity.AddLog(nameof(@int));
            entity.AddLog($"[{nameof(input.basicInt)}: 스탯] {input.basicInt}");
            entity.AddLog($"[{nameof(optionInt)}: {optionInt}] {optionInt.GetStatusValueText(input.basicInt)} = {@int}");

            entity.AddLog(nameof(dex));
            entity.AddLog($"[{nameof(input.basicDex)}: 스탯] {input.basicDex}");
            entity.AddLog($"[{nameof(optionDex)}: {optionDex}] {optionDex.GetStatusValueText(input.basicDex)} = {dex}");

            entity.AddLog(nameof(luk));
            entity.AddLog($"[{nameof(input.basicLuk)}: 스탯] {input.basicLuk}");
            entity.AddLog($"[{nameof(optionLuk)}: {optionLuk}] {optionLuk.GetStatusValueText(input.basicLuk)} = {luk}");
#endif

            // 기본 최대 체력: (100 x (HP계수 / 20)) + ((LEVEL x HP계수) x (1 + (VIT * 0.01)))
            int basicMaxHP = (100 * (input.hpCoefficient / 20)) + (int)MathUtils.ToLong(((unitValue.Level * input.hpCoefficient) * (1 + (vit * 0.01f))));

            // 고정 최대 체력 처리
            if (BattleManager.isFixedMaxHp)
            {
                // [길드 미로] 자신의 고정 체력을 사용 (플레이어는 자신의 최대 체력을 스탯 그대로 적용)
                if (entity.type != UnitEntityType.Player && (entity.State == UnitEntity.UnitState.GVG || entity.State == UnitEntity.UnitState.GVGMultiPlayer))
                {
                    basicMaxHP = 0;
                    if (entity.type == UnitEntityType.Nexus)
                    {
                        NexusEntity nexusEntity = entity as NexusEntity;
                        basicMaxHP = nexusEntity.FixedMaxHp;
                    }
                }
            }

            BattleStatus optionMaxHp = options.Get(BattleOptionType.MaxHp);
            int maxHp = MathUtils.Clamp(optionMaxHp.GetStatusValue(basicMaxHP, isLong: true), 1, int.MaxValue);
#if UNITY_EDITOR
            entity.AddLog(nameof(maxHp));
            entity.AddLog($"[{nameof(basicMaxHP)}: 공식] (100 x (HP계수 / 20)) + ((LEVEL x HP계수) x (1 + (VIT x 0.01)))");
            entity.AddLog($"[{nameof(basicMaxHP)}] ⇒ (100 x ({input.hpCoefficient} / 20)) + (({unitValue.Level} x {input.hpCoefficient}) x (1 + ({vit} x 0.01))) = {basicMaxHP}");
            entity.AddLog($"[{nameof(optionMaxHp)}: {optionMaxHp}] {optionMaxHp.GetStatusValueText(basicMaxHP)} = {maxHp}");
#endif

            // 기본 체력 재생률: 2 + (vit / 8)
            int basicRegenHpRate = 2 + (vit / 8);
            BattleStatus optionRegenHpRate = options.Get(BattleOptionType.RegenHpRate);
            int regenHpRate = optionRegenHpRate.GetStatusValue(basicRegenHpRate);
            int regenHp = MathUtils.Clamp(MathUtils.ToInt(maxHp * MathUtils.ToPermyriadValue(regenHpRate)), 0, int.MaxValue);
#if UNITY_EDITOR
            entity.AddLog(nameof(regenHp));
            entity.AddLog($"[{nameof(basicRegenHpRate)}: 공식] 2 + (VIT / 8)");
            entity.AddLog($"[{nameof(basicRegenHpRate)}] ⇒ 2 + ({vit} / 8) = {basicRegenHpRate}");
            entity.AddLog($"[{nameof(optionRegenHpRate)}: {optionRegenHpRate}] {optionRegenHpRate.GetStatusValueText(basicRegenHpRate)} = {regenHpRate}");
            entity.AddLog($"[{nameof(regenHp)}] {nameof(maxHp)} x {nameof(regenHpRate)} x 0.0001 ⇒ {maxHp} x {regenHpRate} x 0.0001 = {regenHp}");
#endif

            // 기본 마나 재생률: 1
            int basicRegenMpRate = 1;
            BattleStatus optionRegenMpRate = options.Get(BattleOptionType.RegenMpRate);
            int regenMpRate = optionRegenMpRate.GetStatusValue();
            int regenMp = MathUtils.Clamp(basicRegenMpRate + regenMpRate, 0, int.MaxValue);
#if UNITY_EDITOR
            entity.AddLog(nameof(regenMpRate));
            entity.AddLog($"[{nameof(basicRegenMpRate)}: 고정] 1");
            entity.AddLog($"[{nameof(optionRegenMpRate)}: {optionRegenMpRate}] {optionRegenMpRate.GetStatusValueText()} = {regenMpRate}");
            entity.AddLog($"[{nameof(regenMp)}] {nameof(basicRegenMpRate)} + {nameof(regenMpRate)} ⇒ {basicRegenMpRate} + {regenMpRate} = {regenMp}");
#endif

            BattleStatus optionAtk = options.Get(BattleOptionType.Atk); // 물리 공격력 증감

            // 근거리 물리 공격력: (((STR x STR) x ATK계수) / 100) + (STR x 2) + (DEX / 4) + (LUK / 5) + (LEVEL / 2)
            int basicMeleeAtk = (((str * str) * input.atkCoefficient) / 100) + (str * 2) + (dex / 4) + (luk / 5) + (unitValue.Level / 2);
            int meleeAtk = MathUtils.Clamp(optionAtk.GetStatusValue(basicMeleeAtk + itemValue.TotalItemAtk), 0, int.MaxValue);
#if UNITY_EDITOR
            entity.AddLog(nameof(meleeAtk));
            entity.AddLog($"[{nameof(basicMeleeAtk)}: 공식] (((STR x STR) x ATK계수) / 100) + (STR x 2) + (DEX / 4) + (LUK / 5) + (LEVEL / 2)");
            entity.AddLog($"[{nameof(basicMeleeAtk)}] ⇒ ((({str} x {str}) x {input.atkCoefficient}) / 100) + ({str} x 2) + ({dex} / 4) + ({luk} / 5) + ({unitValue.Level} / 2) = {basicMeleeAtk}");
            entity.AddLog($"[{nameof(itemValue.TotalItemAtk)}: 장비스탯] {itemValue.TotalItemAtk}");
            entity.AddLog($"[{nameof(optionAtk)}: {optionAtk}] {optionAtk.GetStatusValueText(basicMeleeAtk + itemValue.TotalItemAtk)} = {meleeAtk}");
#endif

            // 원거리 물리 공격력: (((DEX x DEX) x ATK계수) / 100) + (DEX x 2) + (STR / 4) + (LUK / 5) + (LEVEL / 2)
            int basicRangedAtk = (((dex * dex) * input.atkCoefficient) / 100) + (dex * 2) + (str / 4) + (luk / 5) + (unitValue.Level / 2);
            int rangedAtk = MathUtils.Clamp(optionAtk.GetStatusValue(basicRangedAtk + itemValue.TotalItemAtk), 0, int.MaxValue);
#if UNITY_EDITOR
            entity.AddLog(nameof(rangedAtk));
            entity.AddLog($"[{nameof(basicRangedAtk)}: 공식] (((DEX x DEX) x ATK계수) / 100) + (DEX x 2) + (STR / 4) + (LUK / 5) + (LEVEL / 2)");
            entity.AddLog($"[{nameof(basicRangedAtk)}] ⇒ ((({dex} x {dex}) x {input.atkCoefficient}) / 100) + ({dex} x 2) + ({str} / 4) + ({luk} / 5) + ({unitValue.Level} / 2) = {basicRangedAtk}");
            entity.AddLog($"[{nameof(itemValue.TotalItemAtk)}: 장비스탯] {itemValue.TotalItemAtk}");
            entity.AddLog($"[{nameof(optionAtk)}: {optionAtk}] {optionAtk.GetStatusValueText(basicRangedAtk + itemValue.TotalItemAtk)} = {rangedAtk}");
#endif

            // 기본 Matk: (((INT x INT) x MATK계수) / 100) + (INT x 2) + (LUK / 4) + (DEX / 8) + (LEVEL / 2)
            int basicMatk = (((@int * @int) * input.matkCoefficient) / 100) + (@int * 2) + (luk / 4) + (dex / 8) + (unitValue.Level / 2);
            BattleStatus optionMAtk = options.Get(BattleOptionType.MAtk);
            int matk = MathUtils.Clamp(optionMAtk.GetStatusValue(basicMatk + itemValue.TotalItemMatk), 0, int.MaxValue);
#if UNITY_EDITOR
            entity.AddLog(nameof(matk));
            entity.AddLog($"[{nameof(basicMatk)}: 공식] (((INT x INT) x MATK계수) / 100) + (INT x 2) + (LUK / 4) + (DEX / 8) + (LEVEL / 2)");
            entity.AddLog($"[{nameof(basicMatk)}] ⇒ ((({@int} x {@int}) x {input.matkCoefficient}) / 100) + ({@int} x 2) + ({luk} / 4) + ({dex} / 8) + ({unitValue.Level} / 2) = {basicMatk}");
            entity.AddLog($"[{nameof(itemValue.TotalItemMatk)}: 장비스탯] {itemValue.TotalItemMatk}");
            entity.AddLog($"[{nameof(optionMAtk)}: {optionMAtk}] {optionMAtk.GetStatusValueText(basicMatk + itemValue.TotalItemMatk)} = {matk}");
#endif

            // 기본 Def: (((VIT + LEVEL) x DEF계수) / 2) + (STR / 4) + (AGI / 10)
            int basicDef = (((vit + unitValue.Level) * input.defCoefficient) / 2) + (str / 4) + (agi / 10);
            BattleStatus optionDef = options.Get(BattleOptionType.Def);
            int def = optionDef.GetStatusValue(basicDef + itemValue.TotalItemDef);
#if UNITY_EDITOR
            entity.AddLog(nameof(def));
            entity.AddLog($"[{nameof(basicDef)}: 공식] (((VIT + LEVEL) x DEF계수) / 2) + (STR / 4) + (AGI / 10)");
            entity.AddLog($"[{nameof(basicDef)}] ⇒ ((({vit} + {unitValue.Level}) x {input.defCoefficient}) / 2) + ({str} / 4) + ({agi} / 10) = {basicDef}");
            entity.AddLog($"[{nameof(itemValue.TotalItemDef)}: 장비스탯] {itemValue.TotalItemDef}");
            entity.AddLog($"[{nameof(optionDef)}: {optionDef}] {optionDef.GetStatusValueText(basicDef + itemValue.TotalItemDef)} = {def}");
#endif

            // 기본 Mdef: ((VIT + LEVEL) x MDEF계수 / 4) + INT
            int basicMdef = ((vit + unitValue.Level) * input.mdefCoefficient / 4) + @int;
            BattleStatus optionMDef = options.Get(BattleOptionType.MDef);
            int mdef = optionMDef.GetStatusValue(basicMdef + itemValue.TotalItemMdef);
#if UNITY_EDITOR
            entity.AddLog(nameof(mdef));
            entity.AddLog($"[{nameof(basicMdef)}: 공식] ((VIT + LEVEL) x MDEF계수 / 4) + INT");
            entity.AddLog($"[{nameof(basicMdef)}] ⇒ (({vit} + {unitValue.Level}) x {input.mdefCoefficient} / 4) + {@int} = {basicMdef}");
            entity.AddLog($"[{nameof(itemValue.TotalItemMdef)}: 장비스탯] {itemValue.TotalItemMdef}");
            entity.AddLog($"[{nameof(optionMDef)}: {optionMDef}] {optionMDef.GetStatusValueText(basicMdef + itemValue.TotalItemMdef)} = {mdef}");
#endif

            // 기본 Hit: 100 + LEVEL + (DEX x 2) + ((LUK + AGI) / 2)
            int basicHit = 100 + unitValue.Level + (dex * 2) + ((luk + agi) / 2);
            BattleStatus optionHit = options.Get(BattleOptionType.Hit);
            int hit = optionHit.GetStatusValue(basicHit);
#if UNITY_EDITOR
            entity.AddLog(nameof(hit));
            entity.AddLog($"[{nameof(basicHit)}: 공식] 100 + LEVEL + (DEX x 2) + ((LUK + AGI) / 2)");
            entity.AddLog($"[{nameof(basicHit)}] ⇒ 100 + {unitValue.Level} + ({dex} x 2) + (({luk} + {agi}) / 2) = {basicHit}");
            entity.AddLog($"[{nameof(optionHit)}: {optionHit}] {optionHit.GetStatusValueText(basicHit)} = {hit}");
#endif

            // 기본 Flee: 100 + LEVEL + (AGI x 2) + LUK
            int basicFlee = 100 + unitValue.Level + (agi * 2) + luk;
            BattleStatus optionFlee = options.Get(BattleOptionType.Flee);
            int flee = optionFlee.GetStatusValue(basicFlee);
#if UNITY_EDITOR
            entity.AddLog(nameof(flee));
            entity.AddLog($"[{nameof(basicFlee)}: 공식] 100 + LEVEL + (AGI x 2) + LUK");
            entity.AddLog($"[{nameof(basicFlee)}] ⇒ 100 + {unitValue.Level} + ({agi} x 2) + {luk} = {basicFlee}");
            entity.AddLog($"[{nameof(optionFlee)}: {optionFlee}] {optionFlee.GetStatusValueText(basicFlee)} = {flee}");
#endif

            // 기본 AtkSpd: 10000(캐릭터) or 데이터(몬스터, 큐펫)
            // AtkSpd: (BasicAtkSpd) + (((AGI / 2) + (STR / 8)) x 100)
            int basicAtkSpd = input.basicAtkSpd;
            int atkSpd = basicAtkSpd + (((agi / 2) + (str / 8)) * 100);
            BattleStatus optionAtkSpd = options.Get(BattleOptionType.AtkSpd);
            int rawAtkSpd = optionAtkSpd.GetStatusValue(atkSpd);
            int atkSpdResult = MathUtils.ToInt(rawAtkSpd * (1 - MathUtils.ToPermyriadValue(input.attackSpeedPenalty)));
            int minAtkSpd = BasisType.MIN_ATTACK_SPEED.GetInt();
            int maxAtkSpd = BasisType.MAX_ATTACK_SPEED.GetInt();
            int totalAtkSpd = Mathf.Clamp(atkSpdResult, minAtkSpd, maxAtkSpd);
#if UNITY_EDITOR
            entity.AddLog(nameof(totalAtkSpd));
            entity.AddLog($"[{nameof(basicAtkSpd)}] {basicAtkSpd}");
            entity.AddLog($"[{nameof(atkSpd)}: 공식] (BasicAtkSpd) + (((AGI / 2) + (STR / 8)) x 100)");
            entity.AddLog($"[{nameof(atkSpd)}] ⇒ {basicAtkSpd} + ((({agi} / 2) + ({str} / 8)) x 100) = {atkSpd}");
            entity.AddLog($"[{nameof(optionAtkSpd)}: {optionAtkSpd}] {optionAtkSpd.GetStatusValueText(atkSpd)} = {rawAtkSpd}");
            entity.AddLog($"[{nameof(input.attackSpeedPenalty)}: 공속패널티] {input.attackSpeedPenalty}");
            entity.AddLog($"[{nameof(atkSpdResult)}: 공속패널티 적용] ⇒ {rawAtkSpd} x (1 - {input.attackSpeedPenalty} x 0.0001) = {atkSpdResult}");
            entity.AddLog($"[{nameof(minAtkSpd)}: {minAtkSpd}, {nameof(maxAtkSpd)}: {maxAtkSpd}] = {MathUtils.ToPermyriadText(totalAtkSpd)}");
#endif

            // 기본 CriRate: (LUK / 4) x 100
            int basicCriRate = (luk / 4) * 100;
            BattleStatus optionCriRate = options.Get(BattleOptionType.CriRate);
            int criRate = optionCriRate.GetStatusValue(basicCriRate);
#if UNITY_EDITOR
            entity.AddLog(nameof(criRate));
            entity.AddLog($"[{nameof(basicCriRate)}: 공식] (LUK / 4) x 100");
            entity.AddLog($"[{nameof(basicCriRate)}] ⇒ ({luk} / 4) x 100 = {basicCriRate}");
            entity.AddLog($"[{nameof(optionCriRate)}: {optionCriRate}] {optionCriRate.GetStatusValueText(basicCriRate)} = {MathUtils.ToPermyriadText(criRate)}");
#endif

            // 기본 CriRateResist: (LUK / 8) x 100
            int basicCriRateResist = (luk / 8) * 100;
            BattleStatus optionCriRateResist = options.Get(BattleOptionType.CriRateResist);
            int criRateResist = optionCriRateResist.GetStatusValue(basicCriRateResist);
#if UNITY_EDITOR
            entity.AddLog(nameof(criRateResist));
            entity.AddLog($"[{nameof(basicCriRateResist)}: 공식] (LUK / 8) x 100");
            entity.AddLog($"[{nameof(basicCriRateResist)}] ⇒ ({luk} / 8) x 100 = {basicCriRateResist}");
            entity.AddLog($"[{nameof(optionCriRateResist)}: {optionCriRateResist}] {optionCriRateResist.GetStatusValueText(basicCriRateResist)} = {MathUtils.ToPermyriadText(criRateResist)}");
#endif

            // 기본 CriDmgRate: 기초데이터
            int basicCriDmgRate = BasisType.DEFAULT_CRITICAL_DAMAGE_PER.GetInt();
            BattleStatus optionCriDmgRate = options.Get(BattleOptionType.CriDmgRate);
            int criDmgRate = optionCriDmgRate.GetStatusValue(basicCriDmgRate);
#if UNITY_EDITOR
            entity.AddLog(nameof(criDmgRate));
            entity.AddLog($"[{nameof(basicCriDmgRate)}: 기초데이터] {basicCriDmgRate}");
            entity.AddLog($"[{nameof(optionCriDmgRate)}: {optionCriDmgRate}] {optionCriDmgRate.GetStatusValueText(basicCriDmgRate)} = {MathUtils.ToPermyriadText(criDmgRate)}");
#endif

            // 기본 AtkRange: 10000(캐릭터) or 데이터(몬스터, 큐펫)
            int basicAtkRange = input.basicAtkRange;
            BattleStatus optionAtkRange = options.Get(BattleOptionType.AtkRange);
            int rawAtkRange = optionAtkRange.GetStatusValue(basicAtkRange);
            int maxAtkRange = BasisType.MAX_ATTACK_RANGE.GetInt();
            int atkRange = Mathf.Min(maxAtkRange, rawAtkRange); // max값 이하
#if UNITY_EDITOR
            entity.AddLog(nameof(atkRange));
            entity.AddLog($"[{nameof(basicAtkRange)}: 일반스탯] {basicAtkRange}");
            entity.AddLog($"[{nameof(optionAtkRange)}: {optionAtkRange}] {optionAtkRange.GetStatusValueText(basicAtkRange)} = {rawAtkRange}");
            entity.AddLog($"[{nameof(maxAtkRange)}: {maxAtkRange}] = {MathUtils.ToPermyriadText(atkRange)}");
#endif

            // 기본 MoveSpd: 10000(캐릭터) or 데이터(몬스터, 큐펫)
            int basicMoveSpd = input.basicMoveSpd;
            BattleStatus optionMoveSpd = options.Get(BattleOptionType.MoveSpd);
            int rawMoveSpd = optionMoveSpd.GetStatusValue(basicMoveSpd);
            int maxMoveSpd = BasisType.MAX_MOVE_SPEED.GetInt();
            int moveSpd = Mathf.Min(maxMoveSpd, rawMoveSpd); // max값 이하
#if UNITY_EDITOR
            entity.AddLog(nameof(moveSpd));
            entity.AddLog($"[{nameof(basicMoveSpd)}: 일반스탯] {basicMoveSpd}");
            entity.AddLog($"[{nameof(optionMoveSpd)}: {optionMoveSpd}] {optionMoveSpd.GetStatusValueText(basicMoveSpd)} = {rawMoveSpd}");
            entity.AddLog($"[{nameof(maxMoveSpd)}: {maxMoveSpd}] = {MathUtils.ToPermyriadText(moveSpd)}");
#endif

            BattleStatus optionCooldownRate = options.Get(BattleOptionType.CooldownRate);
            int rawCooldown = optionCooldownRate.GetStatusValue();
            int maxCooldown = BasisType.MAX_PER_REUSE_WAIT_TIME.GetInt();
            int cooldownRate = Mathf.Min(maxCooldown, rawCooldown); // max값 이하
#if UNITY_EDITOR
            entity.AddLog(nameof(cooldownRate));
            entity.AddLog($"[{nameof(optionCooldownRate)}: {optionCooldownRate}] {optionCooldownRate.GetStatusValueText()} = {rawCooldown}");
            entity.AddLog($"[{nameof(maxCooldown)}: {maxCooldown}] = {MathUtils.ToPermyriadText(cooldownRate)}");
#endif

            BattleStatus optionDmgRate = options.Get(BattleOptionType.DmgRate);
            BattleStatus optionDmgRateResist = options.Get(BattleOptionType.DmgRateResist);
            BattleStatus optionMeleeDmgRate = options.Get(BattleOptionType.MeleeDmgRate);
            BattleStatus optionMeleeDmgRateResist = options.Get(BattleOptionType.MeleeDmgRateResist);
            BattleStatus optionRangedDmgRate = options.Get(BattleOptionType.RangedDmgRate);
            BattleStatus optionRangedDmgRateResist = options.Get(BattleOptionType.RangedDmgRateResist);
            BattleStatus optionNormalMonsterDmgRate = options.Get(BattleOptionType.NormalMonsterDmgRate);
            BattleStatus optionBossMonsterDmgRate = options.Get(BattleOptionType.BossMonsterDmgRate);
            BattleStatus optionSmallMonsterDmgRate = options.Get(BattleOptionType.SmallMonsterDmgRate);
            BattleStatus optionMediumMonsterDmgRate = options.Get(BattleOptionType.MediumMonsterDmgRate);
            BattleStatus optionLargeMonsterDmgRate = options.Get(BattleOptionType.LargeMonsterDmgRate);

            int dmgRate = optionDmgRate.GetStatusValue();
            int dmgRateResist = optionDmgRateResist.GetStatusValue();
            int meleeDmgRate = optionMeleeDmgRate.GetStatusValue();
            int meleeDmgRateResist = optionMeleeDmgRateResist.GetStatusValue();
            int rangedDmgRate = optionRangedDmgRate.GetStatusValue();
            int rangedDmgRateResist = optionRangedDmgRateResist.GetStatusValue();
            int normalMonsterDmgRate = optionNormalMonsterDmgRate.GetStatusValue();
            int bossMonsterDmgRate = optionBossMonsterDmgRate.GetStatusValue();
            int smallMonsterDmgRate = optionSmallMonsterDmgRate.GetStatusValue();
            int mediumMonsterDmgRate = optionMediumMonsterDmgRate.GetStatusValue();
            int largeMonsterDmgRate = optionLargeMonsterDmgRate.GetStatusValue();
#if UNITY_EDITOR
            entity.AddLog(nameof(dmgRate));
            entity.AddLog($"[{nameof(optionDmgRate)}: {optionDmgRate}] {optionDmgRate.GetStatusValueText()} = {MathUtils.ToPermyriadText(dmgRate)}");

            entity.AddLog(nameof(dmgRateResist));
            entity.AddLog($"[{nameof(optionDmgRateResist)}: {optionDmgRateResist}] {optionDmgRateResist.GetStatusValueText()} = {MathUtils.ToPermyriadText(dmgRateResist)}");

            entity.AddLog(nameof(meleeDmgRate));
            entity.AddLog($"[{nameof(optionMeleeDmgRate)}: {optionMeleeDmgRate}] {optionMeleeDmgRate.GetStatusValueText()} = {MathUtils.ToPermyriadText(meleeDmgRate)}");

            entity.AddLog(nameof(meleeDmgRateResist));
            entity.AddLog($"[{nameof(optionMeleeDmgRateResist)}: {optionMeleeDmgRateResist}] {optionMeleeDmgRateResist.GetStatusValueText()} = {MathUtils.ToPermyriadText(meleeDmgRateResist)}");

            entity.AddLog(nameof(rangedDmgRate));
            entity.AddLog($"[{nameof(optionRangedDmgRate)}: {optionRangedDmgRate}] {optionRangedDmgRate.GetStatusValueText()} = {MathUtils.ToPermyriadText(rangedDmgRate)}");

            entity.AddLog(nameof(rangedDmgRateResist));
            entity.AddLog($"[{nameof(optionRangedDmgRateResist)}: {optionRangedDmgRateResist}] {optionRangedDmgRateResist.GetStatusValueText()} = {MathUtils.ToPermyriadText(rangedDmgRateResist)}");

            entity.AddLog(nameof(normalMonsterDmgRate));
            entity.AddLog($"[{nameof(optionNormalMonsterDmgRate)}: {optionNormalMonsterDmgRate}] {optionNormalMonsterDmgRate.GetStatusValueText()} = {MathUtils.ToPermyriadText(normalMonsterDmgRate)}");

            entity.AddLog(nameof(bossMonsterDmgRate));
            entity.AddLog($"[{nameof(optionBossMonsterDmgRate)}: {optionBossMonsterDmgRate}] {optionBossMonsterDmgRate.GetStatusValueText()} = {MathUtils.ToPermyriadText(bossMonsterDmgRate)}");

            entity.AddLog(nameof(smallMonsterDmgRate));
            entity.AddLog($"[{nameof(optionSmallMonsterDmgRate)}: {optionSmallMonsterDmgRate}] {optionSmallMonsterDmgRate.GetStatusValueText()} = {MathUtils.ToPermyriadText(smallMonsterDmgRate)}");

            entity.AddLog(nameof(mediumMonsterDmgRate));
            entity.AddLog($"[{nameof(optionMediumMonsterDmgRate)}: {optionMediumMonsterDmgRate}] {optionMediumMonsterDmgRate.GetStatusValueText()} = {MathUtils.ToPermyriadText(mediumMonsterDmgRate)}");

            entity.AddLog(nameof(largeMonsterDmgRate));
            entity.AddLog($"[{nameof(optionLargeMonsterDmgRate)}: {optionLargeMonsterDmgRate}] {optionLargeMonsterDmgRate.GetStatusValueText()} = {MathUtils.ToPermyriadText(largeMonsterDmgRate)}");
#endif

            BattleStatus optionAllCrowdControlRateResist = options.Get(BattleOptionType.AllCrowdControlRateResist);
            BattleStatus optionStunRateResist = options.GetCrowdControlRateResist(CrowdControlType.Stun) + optionAllCrowdControlRateResist;
            BattleStatus optionSilenceRateResist = options.GetCrowdControlRateResist(CrowdControlType.Silence) + optionAllCrowdControlRateResist;
            BattleStatus optionSleepRateResist = options.GetCrowdControlRateResist(CrowdControlType.Sleep) + optionAllCrowdControlRateResist;
            BattleStatus optionHallucinationRateResist = options.GetCrowdControlRateResist(CrowdControlType.Hallucination) + optionAllCrowdControlRateResist;
            BattleStatus optionBleedingRateResist = options.GetCrowdControlRateResist(CrowdControlType.Bleeding) + optionAllCrowdControlRateResist;
            BattleStatus optionBurningRateResist = options.GetCrowdControlRateResist(CrowdControlType.Burning) + optionAllCrowdControlRateResist;
            BattleStatus optionPoisonRateResist = options.GetCrowdControlRateResist(CrowdControlType.Poison) + optionAllCrowdControlRateResist;
            BattleStatus optionCurseRateResist = options.GetCrowdControlRateResist(CrowdControlType.Curse) + optionAllCrowdControlRateResist;
            BattleStatus optionFreezingRateResist = options.GetCrowdControlRateResist(CrowdControlType.Freezing) + optionAllCrowdControlRateResist;
            BattleStatus optionFrozenRateResist = options.GetCrowdControlRateResist(CrowdControlType.Frozen) + optionAllCrowdControlRateResist;
            int stunRateResist = optionStunRateResist.GetStatusValue();
            int silenceRateResist = optionSilenceRateResist.GetStatusValue();
            int sleepRateResist = optionSleepRateResist.GetStatusValue();
            int hallucinationRateResist = optionHallucinationRateResist.GetStatusValue();
            int bleedingRateResist = optionBleedingRateResist.GetStatusValue();
            int burningRateResist = optionBurningRateResist.GetStatusValue();
            int poisonRateResist = optionPoisonRateResist.GetStatusValue();
            int curseRateResist = optionCurseRateResist.GetStatusValue();
            int freezingRateResist = optionFreezingRateResist.GetStatusValue();
            int frozenRateResist = optionFrozenRateResist.GetStatusValue();
#if UNITY_EDITOR
            entity.AddLog(nameof(stunRateResist));
            entity.AddLog($"[{nameof(optionStunRateResist)}: {optionStunRateResist}] {optionStunRateResist.GetStatusValueText()} = {MathUtils.ToPermyriadText(stunRateResist)}");

            entity.AddLog(nameof(silenceRateResist));
            entity.AddLog($"[{nameof(optionSilenceRateResist)}: {optionSilenceRateResist}] {optionSilenceRateResist.GetStatusValueText()} = {MathUtils.ToPermyriadText(silenceRateResist)}");

            entity.AddLog(nameof(sleepRateResist));
            entity.AddLog($"[{nameof(optionSleepRateResist)}: {optionSleepRateResist}] {optionSleepRateResist.GetStatusValueText()} = {MathUtils.ToPermyriadText(sleepRateResist)}");

            entity.AddLog(nameof(hallucinationRateResist));
            entity.AddLog($"[{nameof(optionHallucinationRateResist)}: {optionHallucinationRateResist}] {optionHallucinationRateResist.GetStatusValueText()} = {MathUtils.ToPermyriadText(hallucinationRateResist)}");

            entity.AddLog(nameof(bleedingRateResist));
            entity.AddLog($"[{nameof(optionBleedingRateResist)}: {optionBleedingRateResist}] {optionBleedingRateResist.GetStatusValueText()} = {MathUtils.ToPermyriadText(bleedingRateResist)}");

            entity.AddLog(nameof(burningRateResist));
            entity.AddLog($"[{nameof(optionBurningRateResist)}: {optionBurningRateResist}] {optionBurningRateResist.GetStatusValueText()} = {MathUtils.ToPermyriadText(burningRateResist)}");

            entity.AddLog(nameof(poisonRateResist));
            entity.AddLog($"[{nameof(optionPoisonRateResist)}: {optionPoisonRateResist}] {optionPoisonRateResist.GetStatusValueText()} = {MathUtils.ToPermyriadText(poisonRateResist)}");

            entity.AddLog(nameof(curseRateResist));
            entity.AddLog($"[{nameof(optionCurseRateResist)}: {optionCurseRateResist}] {optionCurseRateResist.GetStatusValueText()} = {MathUtils.ToPermyriadText(curseRateResist)}");

            entity.AddLog(nameof(freezingRateResist));
            entity.AddLog($"[{nameof(optionFreezingRateResist)}: {optionFreezingRateResist}] {optionFreezingRateResist.GetStatusValueText()} = {MathUtils.ToPermyriadText(freezingRateResist)}");

            entity.AddLog(nameof(frozenRateResist));
            entity.AddLog($"[{nameof(optionFrozenRateResist)}: {optionFrozenRateResist}] {optionFrozenRateResist.GetStatusValueText()} = {MathUtils.ToPermyriadText(frozenRateResist)}");
#endif

            BattleStatus optionBasicActiveSkillAllCrowdControlRate = options.Get(BattleOptionType.BasicActiveSkillAllCrowdControlRate);
            BattleStatus optionBasicActiveSkillStunRate = options.GetBasicActiveSkillCrowdControlRate(CrowdControlType.Stun) + optionBasicActiveSkillAllCrowdControlRate;
            BattleStatus optionBasicActiveSkillSilenceRate = options.GetBasicActiveSkillCrowdControlRate(CrowdControlType.Silence) + optionBasicActiveSkillAllCrowdControlRate;
            BattleStatus optionBasicActiveSkillSleepRate = options.GetBasicActiveSkillCrowdControlRate(CrowdControlType.Sleep) + optionBasicActiveSkillAllCrowdControlRate;
            BattleStatus optionBasicActiveSkillHallucinationRate = options.GetBasicActiveSkillCrowdControlRate(CrowdControlType.Hallucination) + optionBasicActiveSkillAllCrowdControlRate;
            BattleStatus optionBasicActiveSkillBleedingRate = options.GetBasicActiveSkillCrowdControlRate(CrowdControlType.Bleeding) + optionBasicActiveSkillAllCrowdControlRate;
            BattleStatus optionBasicActiveSkillBurningRate = options.GetBasicActiveSkillCrowdControlRate(CrowdControlType.Burning) + optionBasicActiveSkillAllCrowdControlRate;
            BattleStatus optionBasicActiveSkillPoisonRate = options.GetBasicActiveSkillCrowdControlRate(CrowdControlType.Poison) + optionBasicActiveSkillAllCrowdControlRate;
            BattleStatus optionBasicActiveSkillCurseRate = options.GetBasicActiveSkillCrowdControlRate(CrowdControlType.Curse) + optionBasicActiveSkillAllCrowdControlRate;
            BattleStatus optionBasicActiveSkillFreezingRate = options.GetBasicActiveSkillCrowdControlRate(CrowdControlType.Freezing) + optionBasicActiveSkillAllCrowdControlRate;
            BattleStatus optionBasicActiveSkillFrozenRate = options.GetBasicActiveSkillCrowdControlRate(CrowdControlType.Frozen) + optionBasicActiveSkillAllCrowdControlRate;
            int basicActiveSkillStunRate = optionBasicActiveSkillStunRate.GetStatusValue();
            int basicActiveSkillSilenceRate = optionBasicActiveSkillSilenceRate.GetStatusValue();
            int basicActiveSkillSleepRate = optionBasicActiveSkillSleepRate.GetStatusValue();
            int basicActiveSkillHallucinationRate = optionBasicActiveSkillHallucinationRate.GetStatusValue();
            int basicActiveSkillBleedingRate = optionBasicActiveSkillBleedingRate.GetStatusValue();
            int basicActiveSkillBurningRate = optionBasicActiveSkillBurningRate.GetStatusValue();
            int basicActiveSkillPoisonRate = optionBasicActiveSkillPoisonRate.GetStatusValue();
            int basicActiveSkillCurseRate = optionBasicActiveSkillCurseRate.GetStatusValue();
            int basicActiveSkillFreezingRate = optionBasicActiveSkillFreezingRate.GetStatusValue();
            int basicActiveSkillFrozenRate = optionBasicActiveSkillFrozenRate.GetStatusValue();
#if UNITY_EDITOR
            entity.AddLog(nameof(basicActiveSkillStunRate));
            entity.AddLog($"[{nameof(optionBasicActiveSkillStunRate)}: {optionBasicActiveSkillStunRate}] {optionBasicActiveSkillStunRate.GetStatusValueText()} = {MathUtils.ToPermyriadText(basicActiveSkillStunRate)}");

            entity.AddLog(nameof(basicActiveSkillSilenceRate));
            entity.AddLog($"[{nameof(optionBasicActiveSkillSilenceRate)}: {optionBasicActiveSkillSilenceRate}] {optionBasicActiveSkillSilenceRate.GetStatusValueText()} = {MathUtils.ToPermyriadText(basicActiveSkillSilenceRate)}");

            entity.AddLog(nameof(basicActiveSkillSleepRate));
            entity.AddLog($"[{nameof(optionBasicActiveSkillSleepRate)}: {optionBasicActiveSkillSleepRate}] {optionBasicActiveSkillSleepRate.GetStatusValueText()} = {MathUtils.ToPermyriadText(basicActiveSkillSleepRate)}");

            entity.AddLog(nameof(basicActiveSkillHallucinationRate));
            entity.AddLog($"[{nameof(optionBasicActiveSkillHallucinationRate)}: {optionBasicActiveSkillHallucinationRate}] {optionBasicActiveSkillHallucinationRate.GetStatusValueText()} = {MathUtils.ToPermyriadText(basicActiveSkillHallucinationRate)}");

            entity.AddLog(nameof(basicActiveSkillBleedingRate));
            entity.AddLog($"[{nameof(optionBasicActiveSkillBleedingRate)}: {optionBasicActiveSkillBleedingRate}] {optionBasicActiveSkillBleedingRate.GetStatusValueText()} = {MathUtils.ToPermyriadText(basicActiveSkillBleedingRate)}");

            entity.AddLog(nameof(basicActiveSkillBurningRate));
            entity.AddLog($"[{nameof(optionBasicActiveSkillBurningRate)}: {optionBasicActiveSkillBurningRate}] {optionBasicActiveSkillBurningRate.GetStatusValueText()} = {MathUtils.ToPermyriadText(basicActiveSkillBurningRate)}");

            entity.AddLog(nameof(basicActiveSkillPoisonRate));
            entity.AddLog($"[{nameof(optionBasicActiveSkillPoisonRate)}: {optionBasicActiveSkillPoisonRate}] {optionBasicActiveSkillPoisonRate.GetStatusValueText()} = {MathUtils.ToPermyriadText(basicActiveSkillPoisonRate)}");

            entity.AddLog(nameof(basicActiveSkillCurseRate));
            entity.AddLog($"[{nameof(optionBasicActiveSkillCurseRate)}: {optionBasicActiveSkillCurseRate}] {optionBasicActiveSkillCurseRate.GetStatusValueText()} = {MathUtils.ToPermyriadText(basicActiveSkillCurseRate)}");

            entity.AddLog(nameof(basicActiveSkillFreezingRate));
            entity.AddLog($"[{nameof(optionBasicActiveSkillFreezingRate)}: {optionBasicActiveSkillFreezingRate}] {optionBasicActiveSkillFreezingRate.GetStatusValueText()} = {MathUtils.ToPermyriadText(basicActiveSkillFreezingRate)}");

            entity.AddLog(nameof(basicActiveSkillFrozenRate));
            entity.AddLog($"[{nameof(optionBasicActiveSkillFrozenRate)}: {optionBasicActiveSkillFrozenRate}] {optionBasicActiveSkillFrozenRate.GetStatusValueText()} = {MathUtils.ToPermyriadText(basicActiveSkillFrozenRate)}");
#endif

            BattleStatus optionAllElementDmgRate = options.Get(BattleOptionType.AllElementDmgRate);
            BattleStatus optionNeutralDmgRate = options.GetElementDmgRate(ElementType.Neutral) + optionAllElementDmgRate;
            BattleStatus optionFireDmgRate = options.GetElementDmgRate(ElementType.Fire) + optionAllElementDmgRate;
            BattleStatus optionWaterDmgRate = options.GetElementDmgRate(ElementType.Water) + optionAllElementDmgRate;
            BattleStatus optionWindDmgRate = options.GetElementDmgRate(ElementType.Wind) + optionAllElementDmgRate;
            BattleStatus optionEarthDmgRate = options.GetElementDmgRate(ElementType.Earth) + optionAllElementDmgRate;
            BattleStatus optionPoisonDmgRate = options.GetElementDmgRate(ElementType.Poison) + optionAllElementDmgRate;
            BattleStatus optionHolyDmgRate = options.GetElementDmgRate(ElementType.Holy) + optionAllElementDmgRate;
            BattleStatus optionShadowDmgRate = options.GetElementDmgRate(ElementType.Shadow) + optionAllElementDmgRate;
            BattleStatus optionGhostDmgRate = options.GetElementDmgRate(ElementType.Ghost) + optionAllElementDmgRate;
            BattleStatus optionUndeadDmgRate = options.GetElementDmgRate(ElementType.Undead) + optionAllElementDmgRate;
            int neutralDmgRate = optionNeutralDmgRate.GetStatusValue();
            int fireDmgRate = optionFireDmgRate.GetStatusValue();
            int waterDmgRate = optionWaterDmgRate.GetStatusValue();
            int windDmgRate = optionWindDmgRate.GetStatusValue();
            int earthDmgRate = optionEarthDmgRate.GetStatusValue();
            int poisonDmgRate = optionPoisonDmgRate.GetStatusValue();
            int holyDmgRate = optionHolyDmgRate.GetStatusValue();
            int shadowDmgRate = optionShadowDmgRate.GetStatusValue();
            int ghostDmgRate = optionGhostDmgRate.GetStatusValue();
            int undeadDmgRate = optionUndeadDmgRate.GetStatusValue();
#if UNITY_EDITOR
            entity.AddLog(nameof(neutralDmgRate));
            entity.AddLog($"[{nameof(optionNeutralDmgRate)}: {optionNeutralDmgRate}] {optionNeutralDmgRate.GetStatusValueText()} = {MathUtils.ToPermyriadText(neutralDmgRate)}");

            entity.AddLog(nameof(fireDmgRate));
            entity.AddLog($"[{nameof(optionFireDmgRate)}: {optionFireDmgRate}] {optionFireDmgRate.GetStatusValueText()} = {MathUtils.ToPermyriadText(fireDmgRate)}");

            entity.AddLog(nameof(waterDmgRate));
            entity.AddLog($"[{nameof(optionWaterDmgRate)}: {optionWaterDmgRate}] {optionWaterDmgRate.GetStatusValueText()} = {MathUtils.ToPermyriadText(waterDmgRate)}");

            entity.AddLog(nameof(windDmgRate));
            entity.AddLog($"[{nameof(optionWindDmgRate)}: {optionWindDmgRate}] {optionWindDmgRate.GetStatusValueText()} = {MathUtils.ToPermyriadText(windDmgRate)}");

            entity.AddLog(nameof(earthDmgRate));
            entity.AddLog($"[{nameof(optionEarthDmgRate)}: {optionEarthDmgRate}] {optionEarthDmgRate.GetStatusValueText()} = {MathUtils.ToPermyriadText(earthDmgRate)}");

            entity.AddLog(nameof(poisonDmgRate));
            entity.AddLog($"[{nameof(optionPoisonDmgRate)}: {optionPoisonDmgRate}] {optionPoisonDmgRate.GetStatusValueText()} = {MathUtils.ToPermyriadText(poisonDmgRate)}");

            entity.AddLog(nameof(holyDmgRate));
            entity.AddLog($"[{nameof(optionHolyDmgRate)}: {optionHolyDmgRate}] {optionHolyDmgRate.GetStatusValueText()} = {MathUtils.ToPermyriadText(holyDmgRate)}");

            entity.AddLog(nameof(shadowDmgRate));
            entity.AddLog($"[{nameof(optionShadowDmgRate)}: {optionShadowDmgRate}] {optionShadowDmgRate.GetStatusValueText()} = {MathUtils.ToPermyriadText(shadowDmgRate)}");

            entity.AddLog(nameof(ghostDmgRate));
            entity.AddLog($"[{nameof(optionGhostDmgRate)}: {optionGhostDmgRate}] {optionGhostDmgRate.GetStatusValueText()} = {MathUtils.ToPermyriadText(ghostDmgRate)}");

            entity.AddLog(nameof(undeadDmgRate));
            entity.AddLog($"[{nameof(optionUndeadDmgRate)}: {optionUndeadDmgRate}] {optionUndeadDmgRate.GetStatusValueText()} = {MathUtils.ToPermyriadText(undeadDmgRate)}");
#endif

            BattleStatus optionAllElementDmgRateResist = options.Get(BattleOptionType.AllElementDmgRateResist);
            BattleStatus optionNeutralDmgRateResist = options.GetElementDmgRateResist(ElementType.Neutral) + optionAllElementDmgRateResist;
            BattleStatus optionFireDmgRateResist = options.GetElementDmgRateResist(ElementType.Fire) + optionAllElementDmgRateResist;
            BattleStatus optionWaterDmgRateResist = options.GetElementDmgRateResist(ElementType.Water) + optionAllElementDmgRateResist;
            BattleStatus optionWindDmgRateResist = options.GetElementDmgRateResist(ElementType.Wind) + optionAllElementDmgRateResist;
            BattleStatus optionEarthDmgRateResist = options.GetElementDmgRateResist(ElementType.Earth) + optionAllElementDmgRateResist;
            BattleStatus optionPoisonDmgRateResist = options.GetElementDmgRateResist(ElementType.Poison) + optionAllElementDmgRateResist;
            BattleStatus optionHolyDmgRateResist = options.GetElementDmgRateResist(ElementType.Holy) + optionAllElementDmgRateResist;
            BattleStatus optionShadowDmgRateResist = options.GetElementDmgRateResist(ElementType.Shadow) + optionAllElementDmgRateResist;
            BattleStatus optionGhostDmgRateResist = options.GetElementDmgRateResist(ElementType.Ghost) + optionAllElementDmgRateResist;
            BattleStatus optionUndeadDmgRateResist = options.GetElementDmgRateResist(ElementType.Undead) + optionAllElementDmgRateResist;
            int neutralDmgRateResist = optionNeutralDmgRateResist.GetStatusValue();
            int fireDmgRateResist = optionFireDmgRateResist.GetStatusValue();
            int waterDmgRateResist = optionWaterDmgRateResist.GetStatusValue();
            int windDmgRateResist = optionWindDmgRateResist.GetStatusValue();
            int earthDmgRateResist = optionEarthDmgRateResist.GetStatusValue();
            int poisonDmgRateResist = optionPoisonDmgRateResist.GetStatusValue();
            int holyDmgRateResist = optionHolyDmgRateResist.GetStatusValue();
            int shadowDmgRateResist = optionShadowDmgRateResist.GetStatusValue();
            int ghostDmgRateResist = optionGhostDmgRateResist.GetStatusValue();
            int undeadDmgRateResist = optionUndeadDmgRateResist.GetStatusValue();
#if UNITY_EDITOR
            entity.AddLog(nameof(neutralDmgRateResist));
            entity.AddLog($"[{nameof(optionNeutralDmgRateResist)}: {optionNeutralDmgRateResist}] {optionNeutralDmgRateResist.GetStatusValueText()} = {MathUtils.ToPermyriadText(neutralDmgRateResist)}");

            entity.AddLog(nameof(fireDmgRateResist));
            entity.AddLog($"[{nameof(optionFireDmgRateResist)}: {optionFireDmgRateResist}] {optionFireDmgRateResist.GetStatusValueText()} = {MathUtils.ToPermyriadText(fireDmgRateResist)}");

            entity.AddLog(nameof(waterDmgRateResist));
            entity.AddLog($"[{nameof(optionWaterDmgRateResist)}: {optionWaterDmgRateResist}] {optionWaterDmgRateResist.GetStatusValueText()} = {MathUtils.ToPermyriadText(waterDmgRateResist)}");

            entity.AddLog(nameof(windDmgRateResist));
            entity.AddLog($"[{nameof(optionWindDmgRateResist)}: {optionWindDmgRateResist}] {optionWindDmgRateResist.GetStatusValueText()} = {MathUtils.ToPermyriadText(windDmgRateResist)}");

            entity.AddLog(nameof(earthDmgRateResist));
            entity.AddLog($"[{nameof(optionEarthDmgRateResist)}: {optionEarthDmgRateResist}] {optionEarthDmgRateResist.GetStatusValueText()} = {MathUtils.ToPermyriadText(earthDmgRateResist)}");

            entity.AddLog(nameof(poisonDmgRateResist));
            entity.AddLog($"[{nameof(optionPoisonDmgRateResist)}: {optionPoisonDmgRateResist}] {optionPoisonDmgRateResist.GetStatusValueText()} = {MathUtils.ToPermyriadText(poisonDmgRateResist)}");

            entity.AddLog(nameof(holyDmgRateResist));
            entity.AddLog($"[{nameof(optionHolyDmgRateResist)}: {optionHolyDmgRateResist}] {optionHolyDmgRateResist.GetStatusValueText()} = {MathUtils.ToPermyriadText(holyDmgRateResist)}");

            entity.AddLog(nameof(shadowDmgRateResist));
            entity.AddLog($"[{nameof(optionShadowDmgRateResist)}: {optionShadowDmgRateResist}] {optionShadowDmgRateResist.GetStatusValueText()} = {MathUtils.ToPermyriadText(shadowDmgRateResist)}");

            entity.AddLog(nameof(ghostDmgRateResist));
            entity.AddLog($"[{nameof(optionGhostDmgRateResist)}: {optionGhostDmgRateResist}] {optionGhostDmgRateResist.GetStatusValueText()} = {MathUtils.ToPermyriadText(ghostDmgRateResist)}");

            entity.AddLog(nameof(undeadDmgRateResist));
            entity.AddLog($"[{nameof(optionUndeadDmgRateResist)}: {optionUndeadDmgRateResist}] {optionUndeadDmgRateResist.GetStatusValueText()} = {MathUtils.ToPermyriadText(undeadDmgRateResist)}");
#endif

            BattleStatus optionAutoGuard = options.Get(BattleOptionType.AutoGuard);
            BattleStatus optionDevote = options.Get(BattleOptionType.Devotion);
            BattleStatus optionExpDropRate = options.Get(BattleOptionType.ExpDropRate);
            BattleStatus optionJobExpDropRate = options.Get(BattleOptionType.JobExpDropRate);
            BattleStatus optionZenyDropRate = options.Get(BattleOptionType.ZenyDropRate);
            BattleStatus optionItemDropRate = options.Get(BattleOptionType.ItemDropRate);

            int autoGuardRate = optionAutoGuard.GetStatusValue();
            int devoteRate = optionDevote.GetStatusValue();
            int expDropRate = optionExpDropRate.GetStatusValue();
            int jobExpDropRate = optionJobExpDropRate.GetStatusValue();
            int zenyDropRate = optionZenyDropRate.GetStatusValue();
            int itemDropRate = optionItemDropRate.GetStatusValue();

#if UNITY_EDITOR
            entity.AddLog(nameof(autoGuardRate));
            entity.AddLog($"[{nameof(optionAutoGuard)}: {optionAutoGuard}] {optionAutoGuard.GetStatusValueText()} = {MathUtils.ToPermyriadText(autoGuardRate)}");

            entity.AddLog(nameof(devoteRate));
            entity.AddLog($"[{nameof(optionDevote)}: {optionDevote}] {optionDevote.GetStatusValueText()} = {MathUtils.ToPermyriadText(devoteRate)}");

            entity.AddLog(nameof(expDropRate));
            entity.AddLog($"[{nameof(optionExpDropRate)}: {optionExpDropRate}] {optionExpDropRate.GetStatusValueText()} = {MathUtils.ToPermyriadText(expDropRate)}");

            entity.AddLog(nameof(jobExpDropRate));
            entity.AddLog($"[{nameof(optionJobExpDropRate)}: {optionJobExpDropRate}] {optionJobExpDropRate.GetStatusValueText()} = {MathUtils.ToPermyriadText(jobExpDropRate)}");

            entity.AddLog(nameof(zenyDropRate));
            entity.AddLog($"[{nameof(optionZenyDropRate)}: {optionZenyDropRate}] {optionZenyDropRate.GetStatusValueText()} = {MathUtils.ToPermyriadText(zenyDropRate)}");

            entity.AddLog(nameof(itemDropRate));
            entity.AddLog($"[{nameof(optionItemDropRate)}: {optionItemDropRate}] {optionItemDropRate.GetStatusValueText()} = {MathUtils.ToPermyriadText(itemDropRate)}");
#endif

#if UNITY_EDITOR
            entity.FinishLog();
#endif

            return new StatusOutput
            {
                statusSettings = new BattleStatusInfo.Settings
                {
                    str = str,
                    agi = agi,
                    vit = vit,
                    @int = @int,
                    dex = dex,
                    luk = luk,

                    maxHp = maxHp,
                    regenHp = regenHp,
                    regenMp = regenMp,
                    meleeAtk = meleeAtk,
                    rangedAtk = rangedAtk,
                    matk = matk,
                    def = def,
                    mdef = mdef,
                    hit = hit,
                    flee = flee,
                    atkSpd = totalAtkSpd,
                    criRate = criRate,
                    criRateResist = criRateResist,
                    criDmgRate = criDmgRate,
                    atkRange = atkRange,
                    moveSpd = moveSpd,

                    cooldownRate = cooldownRate,
                    dmgRate = dmgRate,
                    dmgRateResist = dmgRateResist,
                    meleeDmgRate = meleeDmgRate,
                    meleeDmgRateResist = meleeDmgRateResist,
                    rangedDmgRate = rangedDmgRate,
                    rangedDmgRateResist = rangedDmgRateResist,
                    normalMonsterDmgRate = normalMonsterDmgRate,
                    bossMonsterDmgRate = bossMonsterDmgRate,
                    smallMonsterDmgRate = smallMonsterDmgRate,
                    mediumMonsterDmgRate = mediumMonsterDmgRate,
                    largeMonsterDmgRate = largeMonsterDmgRate,

                    stunRateResist = stunRateResist,
                    silenceRateResist = silenceRateResist,
                    sleepRateResist = sleepRateResist,
                    hallucinationRateResist = hallucinationRateResist,
                    bleedingRateResist = bleedingRateResist,
                    burningRateResist = burningRateResist,
                    poisonRateResist = poisonRateResist,
                    curseRateResist = curseRateResist,
                    freezingRateResist = freezingRateResist,
                    frozenRateResist = frozenRateResist,

                    basicActiveSkillStunRate = basicActiveSkillStunRate,
                    basicActiveSkillSilenceRate = basicActiveSkillSilenceRate,
                    basicActiveSkillSleepRate = basicActiveSkillSleepRate,
                    basicActiveSkillHallucinationRate = basicActiveSkillHallucinationRate,
                    basicActiveSkillBleedingRate = basicActiveSkillBleedingRate,
                    basicActiveSkillBurningRate = basicActiveSkillBurningRate,
                    basicActiveSkillPoisonRate = basicActiveSkillPoisonRate,
                    basicActiveSkillCurseRate = basicActiveSkillCurseRate,
                    basicActiveSkillFreezingRate = basicActiveSkillFreezingRate,
                    basicActiveSkillFrozenRate = basicActiveSkillFrozenRate,

                    neutralDmgRate = neutralDmgRate,
                    fireDmgRate = fireDmgRate,
                    waterDmgRate = waterDmgRate,
                    windDmgRate = windDmgRate,
                    earthDmgRate = earthDmgRate,
                    poisonDmgRate = poisonDmgRate,
                    holyDmgRate = holyDmgRate,
                    shadowDmgRate = shadowDmgRate,
                    ghostDmgRate = ghostDmgRate,
                    undeadDmgRate = undeadDmgRate,

                    neutralDmgRateResist = neutralDmgRateResist,
                    fireDmgRateResist = fireDmgRateResist,
                    waterDmgRateResist = waterDmgRateResist,
                    windDmgRateResist = windDmgRateResist,
                    earthDmgRateResist = earthDmgRateResist,
                    poisonDmgRateResist = poisonDmgRateResist,
                    holyDmgRateResist = holyDmgRateResist,
                    shadowDmgRateResist = shadowDmgRateResist,
                    ghostDmgRateResist = ghostDmgRateResist,
                    undeadDmgRateResist = undeadDmgRateResist,

                    autoGuardRate = autoGuardRate,
                    devoteRate = devoteRate,

                    expDropRate = expDropRate,
                    jobExpDropRate = jobExpDropRate,
                    zenyDropRate = zenyDropRate,
                    itemDropRate = itemDropRate
                },
            };
        }

        public static SkillOutput ApplyActiveSkill(SkillInput input, UnitEntity entity)
        {
#if UNITY_EDITOR
            entity.SetHeader($"{nameof(ApplyActiveSkill)}");
#endif

            bool isAutoGuard; // 오토가드 성공여부
            bool isFlee; // 회피 성공여부
            bool isCritical; // 크리티컬 여부
            bool hasDamage; // 대미지 존재
            int totalDamage; // 대미지
            bool hasRecovery = false; // 회복 존재
            int totalRecovery; // 타겟에게 회복되는 체력
            bool hasAbsorbRecovery; // 흡수 존재
            int absorbRecovery; // 시전자에게 흡수되는 체력
            bool isStun, isSilence, isSleep, isHallucination, isBleeding, isBurning, isPoison, isCurse, isFreezing, isFrozen; // 타겟의 상태이상 여부

            /******************** 서버 체크용 ********************/
            int criDamageRate; // 치명타 증폭대미지 비율
            int elementFactor; // 속성간 대미지 배율 (기초데이터)
            int attackerElementDamageRate; // 시전자의 속성 증폭대미지 비율
            int targetElementDamageRateResist; // 타겟의 속성 증폭대미지 비율 저항
            int elementDamageRate; // 속성 증폭대미지 비율
            int damageRate; //전체 증폭대미지 비율
            int distDamageRate; // 거리 증폭대미지 비율 = (시전자의 거리 증폭대미지 비율 - 타겟의 거리 증폭대미지 비율 저항)
            int monsterDamageRate; // 몬스터 증폭대미지 비율
            int attackerWeaponType; // 장착한 무기 타입
            int unitSizeFactor; // 유닛사이즈 대미지 배율 (기초 데이터)
            int unitSizeDamageRate; // 유닛사이즈 증폭대미지 비율

            int rawDamageValue; // 시전자의 물리 공격력에 스킬 물리 공격 적용
            int damageRandomSeq; // 물댐 랜덤 시퀀스
            int damageRandomValue; // 랜덤 타격계수
            int damageValue; // 물리 공격력에 랜덤 타격계수 적용
            int targetDef; // 타겟 방어력
            float damageDecreaseRate; // 물리대미지감소 비율은 0에서 1사이
            int damage; // 물댐

            int attackerMAtk; // 시전자의 마법공격력
            int rawMDamageValue; // 시전자의 마법 공격력에 스킬 마법 공격 적용
            int mdamageRandomSeq; // 마댐 랜덤 시퀀스
            int mdamageRandomValue; // 랜덤 타격 계수
            int mdamageValue; // 마법 공격력에 랜덤 타격계수 적용
            int targetMDef; // 방어력은 0 이상
            float mdamageDecreaseRate; // 마법대미지감소 비율은 0에서 1사이
            int mdamage; // 마댐

            int sumDamage; // 대미지총합 = (물리대미지 + 마법대미지)
            int damageFactor; // 대미지 배율 계산
            int normalDamage; // 대미지 배율 적용
            int sumPlusDamageRate; // 증폭대미지 비율 총합
            int plusDamage; // 증폭대미지 비율 적용
            int rawTotalDamage; // 시전자의 전체 대미지 비율 감소율 적용
            int applyedDecreaseRateDamage; // 보정값이 적용된 대미지

            ActiveBattleOptionList options = input.skillValue.ActiveOptions;

            int recoveryHp; // 타겟에게 회복되는 체력 (시전자의 지력참조)
            if (!options.HasValue(BattleOptionType.RecoveryHp))
            {
                recoveryHp = 0;
            }
            else
            {
                hasRecovery = true;
                BattleStatus optionRecoveryHp = options.Get(BattleOptionType.RecoveryHp);
                recoveryHp = optionRecoveryHp.value + MathUtils.ToInt(input.statusAttackerValue.Int * MathUtils.ToPermyriadValue(optionRecoveryHp.perValue));

#if UNITY_EDITOR
                entity.AddLog(nameof(recoveryHp));
                entity.AddLog($"[{nameof(input.statusAttackerValue.Int)}: 시전자의 INT] {input.statusAttackerValue.Int}");
                entity.AddLog($"[{nameof(optionRecoveryHp)}: {optionRecoveryHp}] {optionRecoveryHp.value} + ({input.statusAttackerValue.Int} x {optionRecoveryHp.perValue} x 0.0001) = {recoveryHp}");
#endif
            }

            int recoveryTotalHp; // 타겟에게 회복되는 체력 (시전자의 체력참조)
            if (!options.HasValue(BattleOptionType.RecoveryTotalHp))
            {
                recoveryTotalHp = 0;
            }
            else
            {
                hasRecovery = true;
                BattleStatus optionRecoveryTotalHp = options.Get(BattleOptionType.RecoveryTotalHp);
                recoveryTotalHp = optionRecoveryTotalHp.value + MathUtils.ToInt(input.statusAttackerValue.MaxHp * MathUtils.ToPermyriadValue(optionRecoveryTotalHp.perValue));

#if UNITY_EDITOR
                entity.AddLog(nameof(recoveryTotalHp));
                entity.AddLog($"[{nameof(input.statusAttackerValue.MaxHp)}: 시전자의 MAX_HP] {input.statusAttackerValue.MaxHp}");
                entity.AddLog($"[{nameof(optionRecoveryTotalHp)}: {optionRecoveryTotalHp}] {optionRecoveryTotalHp.value} + ({input.statusAttackerValue.MaxHp} x {optionRecoveryTotalHp.perValue} x 0.0001) = {recoveryTotalHp}");
#endif
            }

            totalRecovery = Mathf.Max(1, recoveryHp + recoveryTotalHp); // 회복 수치는 1 이상
#if UNITY_EDITOR
            entity.AddLog(nameof(totalRecovery));
            entity.AddLog($"[{nameof(totalRecovery)}: 회복수치] {totalRecovery}");
#endif

            // 타겟에게 해를 끼치는 요소가 없을 경우 (물리대미지, 마법대미지, 상태이상)
            if (!options.HasDamageValue && !options.HasCrowdControl)
            {
                // 오토가드나 회피는 해를 끼치는 요소일 경우에만 해당
                isAutoGuard = false;
                isFlee = false;
            }
            else
            {
                // 회피 불가일 경우 (스턴, 수면, 동빙)
                if (input.crowdControlTargetValue.CannotFlee)
                {
                    isAutoGuard = false;
                    isFlee = false;
#if UNITY_EDITOR
                    entity.AddLog(nameof(input.crowdControlTargetValue.CannotFlee));
                    entity.AddLog($"[{nameof(input.crowdControlTargetValue.CannotFlee)}: 회피 불가] {input.crowdControlTargetValue.CannotFlee}");
#endif
                }
                else
                {
                    isAutoGuard = MathUtils.IsCheckPermyriad(input.statusTargetValue.AutoGuardRate); // 오토가드 성공여부

#if UNITY_EDITOR
                    entity.AddLog(nameof(isAutoGuard));
                    entity.AddLog($"[{nameof(input.statusTargetValue.AutoGuardRate)}: 오토가드 확률] {MathUtils.ToPermyriadText(input.statusTargetValue.AutoGuardRate)} ⇒ {isAutoGuard}");
#endif

                    if (isAutoGuard)
                    {
                        isFlee = false;
                    }
                    else
                    {
                        if (input.statusTargetValue.Flee == 0)
                        {
                            isFlee = false; // 무조건 타격
                        }
                        else
                        {
                            // 타격확률 = 10000 x ( 최대(0, 시전자의 명중률) / 최대(1, 타겟의 회피율) )
                            int hitRate = MathUtils.ToPermyriad(MathUtils.GetRate(Mathf.Max(0, input.statusAttackerValue.Hit), Mathf.Max(1, input.statusTargetValue.Flee)));
                            int targetRateOverTenPer = BasisType.TARGET_RATE_OVER_TEN_PER.GetInt(); // 특정 타격 확률 이상은 무조건 타격
                            if (hitRate < targetRateOverTenPer)
                            {
                                isFlee = !MathUtils.IsCheckPermyriad(hitRate); // 타격 확률이 기초 데이터보다 낮을 경우에만 회피 체크 가능
                            }
                            else
                            {
                                isFlee = false; // 무조건 타격
                            }

#if UNITY_EDITOR
                            entity.AddLog(nameof(isFlee));
                            entity.AddLog($"[{nameof(input.statusAttackerValue.Hit)}: 시전자의 명중률] {input.statusAttackerValue.Hit}");
                            entity.AddLog($"[{nameof(input.statusTargetValue.Flee)}: 타겟의 회피율] {input.statusTargetValue.Flee}");
                            entity.AddLog($"[{nameof(hitRate)}: 타격 확률] 10000 x ({Mathf.Max(0, input.statusAttackerValue.Hit)} / {Mathf.Max(1, input.statusTargetValue.Flee)}) = {MathUtils.ToPermyriadText(hitRate)}");
                            entity.AddLog($"[{nameof(targetRateOverTenPer)}: 타격 체크 확률 (기초데이터)] {MathUtils.ToPermyriadText(targetRateOverTenPer)}");
                            entity.AddLog($"[회피 체크] {hitRate} < {targetRateOverTenPer} = {(hitRate < targetRateOverTenPer ? "회피체크시도" : "무조건타격")}");
                            entity.AddLog($"[{nameof(isFlee)}: 회피] ⇒ {isFlee}");
#endif
                        }
                    }
                }
            }

            // 오토가드 또는 회피 성공
            if (isAutoGuard || isFlee)
            {
                isCritical = false;
                hasDamage = false;
                totalDamage = 0;
                hasAbsorbRecovery = false;
                absorbRecovery = 0;
                isStun = isSilence = isSleep = isHallucination = isBleeding = isBurning = isPoison = isCurse = isFreezing = isFrozen = false;

                criDamageRate = 0;
                elementFactor = 0;
                attackerElementDamageRate = 0;
                targetElementDamageRateResist = 0;
                elementDamageRate = 0;
                damageRate = 0;
                distDamageRate = 0;
                monsterDamageRate = 0;
                attackerWeaponType = 0;
                unitSizeFactor = 0;
                unitSizeDamageRate = 0;

                rawDamageValue = 0;
                damageRandomSeq = 0;
                damageRandomValue = 0;
                damageValue = 0;
                targetDef = 0;
                damageDecreaseRate = 0f;
                damage = 0;

                attackerMAtk = 0;
                rawMDamageValue = 0;
                mdamageRandomSeq = 0;
                mdamageRandomValue = 0;
                mdamageValue = 0;
                targetMDef = 0;
                mdamageDecreaseRate = 0f;
                mdamage = 0;

                sumDamage = 0;
                damageFactor = 0;
                normalDamage = 0;
                sumPlusDamageRate = 0;
                plusDamage = 0;
                rawTotalDamage = 0;
                applyedDecreaseRateDamage = 0;
            }
            else
            {
                hasDamage = options.HasDamageValue; // 대미지 옵션 존재
                hasAbsorbRecovery = options.HasValue(BattleOptionType.AbsorbHp); // 대미비 흡수 옵션 존재

                // 평타에 의한 추가 상태이상 확률
                int basicActiveSkillStunRate;
                int basicActiveSkillSilenceRate;
                int basicActiveSkillSleepRate;
                int basicActiveSkillHallucinationRate;
                int basicActiveSkillBleedingRate;
                int basicActiveSkillBurningRate;
                int basicActiveSkillPoisonRate;
                int basicActiveSkillCurseRate;
                int basicActiveSkillFreezingRate;
                int basicActiveSkillFrozenRate;

                // 대미지 옵션이 존재하지 않을 경우
                if (!hasDamage)
                {
                    isCritical = false;
                    totalDamage = 0;
                    absorbRecovery = 0;
                    basicActiveSkillStunRate = basicActiveSkillSilenceRate = basicActiveSkillSleepRate = basicActiveSkillHallucinationRate = basicActiveSkillBleedingRate = basicActiveSkillBurningRate = basicActiveSkillPoisonRate = basicActiveSkillCurseRate = basicActiveSkillFreezingRate = basicActiveSkillFrozenRate = 0;

                    criDamageRate = 0;
                    elementFactor = 0;
                    attackerElementDamageRate = 0;
                    targetElementDamageRateResist = 0;
                    elementDamageRate = 0;
                    damageRate = 0;
                    distDamageRate = 0;
                    monsterDamageRate = 0;
                    attackerWeaponType = 0;
                    unitSizeFactor = 0;
                    unitSizeDamageRate = 0;

                    rawDamageValue = 0;
                    damageRandomSeq = 0;
                    damageRandomValue = 0;
                    damageValue = 0;
                    targetDef = 0;
                    damageDecreaseRate = 0f;
                    damage = 0;

                    attackerMAtk = 0;
                    rawMDamageValue = 0;
                    mdamageRandomSeq = 0;
                    mdamageRandomValue = 0;
                    mdamageValue = 0;
                    targetMDef = 0;
                    mdamageDecreaseRate = 0f;
                    mdamage = 0;

                    sumDamage = 0;
                    damageFactor = 0;
                    normalDamage = 0;
                    sumPlusDamageRate = 0;
                    plusDamage = 0;
                    rawTotalDamage = 0;
                    applyedDecreaseRateDamage = 0;
                }
                else
                {
                    ElementType skillElementType = input.skillElementType; // 스킬 속성 타입
                    ElementType unitElementType = input.unitTargetValue.UnitElementType; // 타겟의 속성 타입

                    // null 속성은 존재하지 않는다.
                    if (skillElementType == default)
                        skillElementType = ElementType.Neutral;

                    // null 속성은 존재하지 않는다.
                    if (unitElementType == default)
                        unitElementType = ElementType.Neutral;

                    // 평타가 아닐 경우
                    if (!input.skillValue.IsBasicActiveSkill)
                    {
                        basicActiveSkillStunRate = basicActiveSkillSilenceRate = basicActiveSkillSleepRate = basicActiveSkillHallucinationRate = basicActiveSkillBleedingRate = basicActiveSkillBurningRate = basicActiveSkillPoisonRate = basicActiveSkillCurseRate = basicActiveSkillFreezingRate = basicActiveSkillFrozenRate = 0;
                        isCritical = false;
                        criDamageRate = 0;
                    }
                    else
                    {
                        int attackerCriRate = MathUtils.ToInt(input.statusAttackerValue.CriRate * (1 - MathUtils.ToPermyriadValue(input.crowdControlAttackerValue.CriRateDecreaseRate)));
                        int totalCriRate = attackerCriRate - input.statusTargetValue.CriRateResist; // 크리티컬 비율
                        isCritical = MathUtils.IsCheckPermyriad(totalCriRate); // 크리티컬 여부
                        criDamageRate = isCritical ? input.statusAttackerValue.CriDmgRate : 0; // 치명타 증폭대미지 비율

#if UNITY_EDITOR
                        entity.AddLog(nameof(criDamageRate));
                        entity.AddLog($"[{nameof(input.statusAttackerValue.CriRate)}: 시전자의 기본 치명타 확률] {MathUtils.ToPermyriadText(input.statusAttackerValue.CriRate)}");
                        entity.AddLog($"[{nameof(input.crowdControlAttackerValue.CriRateDecreaseRate)}: 시전자의 치명타 확률 감소율(상태이상)] {MathUtils.ToPermyriadText(input.crowdControlAttackerValue.CriRateDecreaseRate)}");
                        entity.AddLog($"[{nameof(attackerCriRate)}: 시전자의 치명타 확률] {input.statusAttackerValue.CriRate} x (1 - {input.crowdControlAttackerValue.CriRateDecreaseRate} x 0.0001) = {MathUtils.ToPermyriadText(attackerCriRate)}");
                        entity.AddLog($"[{nameof(input.statusTargetValue.CriRateResist)}: 타겟의 치명타 확률 저항] {MathUtils.ToPermyriadText(input.statusTargetValue.CriRateResist)}");
                        entity.AddLog($"[{nameof(totalCriRate)}: 최종 치명타 확률] {MathUtils.ToPermyriadText(totalCriRate)}");
                        entity.AddLog($"[{nameof(isCritical)}: 크리티컬] {isCritical}");
                        entity.AddLog($"[{nameof(isCritical)}: 치명타 증폭대미지 비율] {MathUtils.ToPermyriadText(criDamageRate)}");
#endif

                        // 평타에 의한 추가 상태이상 확률
                        basicActiveSkillStunRate = input.statusAttackerValue.BasicActiveSkillStunRate;
                        basicActiveSkillSilenceRate = input.statusAttackerValue.BasicActiveSkillSilenceRate;
                        basicActiveSkillSleepRate = input.statusAttackerValue.BasicActiveSkillSleepRate;
                        basicActiveSkillHallucinationRate = input.statusAttackerValue.BasicActiveSkillHallucinationRate;
                        basicActiveSkillBleedingRate = input.statusAttackerValue.BasicActiveSkillBleedingRate;
                        basicActiveSkillBurningRate = input.statusAttackerValue.BasicActiveSkillBurningRate;
                        basicActiveSkillPoisonRate = input.statusAttackerValue.BasicActiveSkillPoisonRate;
                        basicActiveSkillCurseRate = input.statusAttackerValue.BasicActiveSkillCurseRate;
                        basicActiveSkillFreezingRate = input.statusAttackerValue.BasicActiveSkillFreezingRate;
                        basicActiveSkillFrozenRate = input.statusAttackerValue.BasicActiveSkillFrozenRate;
                    }

                    int unitElementTypeValue = (int)unitElementType; // 유닛의 속성타입
                    elementFactor = input.elementDamage.Get(skillElementType, input.skillElementLevel, unitElementType, input.unitTargetValue.UnitElementLevel);
                    switch (skillElementType)
                    {
                        default:
                        case ElementType.Neutral: // 공격한 스킬이 무속성 공격
                            attackerElementDamageRate = input.statusAttackerValue.NeutralDmgRate;
                            targetElementDamageRateResist = input.statusTargetValue.NeutralDmgRateResist;
                            break;

                        case ElementType.Fire: // 공격한 스킬이 화속성 공격
                            attackerElementDamageRate = input.statusAttackerValue.FireDmgRate;
                            targetElementDamageRateResist = input.statusTargetValue.FireDmgRateResist;
                            break;

                        case ElementType.Water: // 공격한 스킬이 수속성 공격
                            attackerElementDamageRate = input.statusAttackerValue.WaterDmgRate;
                            targetElementDamageRateResist = input.statusTargetValue.WaterDmgRateResist;
                            break;

                        case ElementType.Wind: // 공격한 스킬이 풍속성 공격
                            attackerElementDamageRate = input.statusAttackerValue.WindDmgRate;
                            targetElementDamageRateResist = input.statusTargetValue.WindDmgRateResist;
                            break;

                        case ElementType.Earth: // 공격한 스킬이 지속성 공격
                            attackerElementDamageRate = input.statusAttackerValue.EarthDmgRate;
                            targetElementDamageRateResist = input.statusTargetValue.EarthDmgRateResist;
                            break;

                        case ElementType.Poison: // 공격한 스킬이 독속성 공격
                            attackerElementDamageRate = input.statusAttackerValue.PoisonDmgRate;
                            targetElementDamageRateResist = input.statusTargetValue.PoisonDmgRateResist;
                            break;

                        case ElementType.Holy: // 공격한 스킬이 성속성 공격
                            attackerElementDamageRate = input.statusAttackerValue.HolyDmgRate;
                            targetElementDamageRateResist = input.statusTargetValue.HolyDmgRateResist;
                            break;

                        case ElementType.Shadow: // 공격한 스킬이 암속성 공격
                            attackerElementDamageRate = input.statusAttackerValue.ShadowDmgRate;
                            targetElementDamageRateResist = input.statusTargetValue.ShadowDmgRateResist;
                            break;

                        case ElementType.Ghost: // 공격한 스킬이 염속성 공격
                            attackerElementDamageRate = input.statusAttackerValue.GhostDmgRate;
                            targetElementDamageRateResist = input.statusTargetValue.GhostDmgRateResist;
                            break;

                        case ElementType.Undead: // 공격한 스킬이 사속성 공격
                            attackerElementDamageRate = input.statusAttackerValue.UndeadDmgRate;
                            targetElementDamageRateResist = input.statusTargetValue.UndeadDmgRateResist;
                            break;
                    }

                    elementDamageRate = attackerElementDamageRate - targetElementDamageRateResist; // 속성 증폭대미지 비율 = (시전자의 속성 증폭대미지 비율 - 타겟의 속성 증폭대미지 비율 저항)

#if UNITY_EDITOR
                    entity.AddLog(nameof(elementFactor));
                    entity.AddLog($"[{nameof(input.unitTargetValue.UnitElementType)}: 타겟 속성] {input.unitTargetValue.UnitElementType}");
                    entity.AddLog($"[{nameof(skillElementType)}: 스킬 속성] {skillElementType}");
                    entity.AddLog($"[{nameof(elementFactor)}: 속성 배율(기초데이터)] {MathUtils.ToPermyriadText(elementFactor)}");

                    entity.AddLog(nameof(elementDamageRate));
                    entity.AddLog($"[{nameof(attackerElementDamageRate)}: 시전자의 속성 증폭대미지 비율] {MathUtils.ToPermyriadText(attackerElementDamageRate)}");
                    entity.AddLog($"[{nameof(targetElementDamageRateResist)}: 타겟의 속성 증폭대미지 비율 저항] {MathUtils.ToPermyriadText(targetElementDamageRateResist)}");
                    entity.AddLog($"[{nameof(elementDamageRate)}: 속성 증폭대미지 비율] {attackerElementDamageRate} - {targetElementDamageRateResist} = {MathUtils.ToPermyriadText(elementDamageRate)}");
#endif

                    damageRate = input.statusAttackerValue.DmgRate - input.statusTargetValue.DmgRateResist; // 전체 증폭대미지 비율
#if UNITY_EDITOR
                    entity.AddLog(nameof(damageRate));
                    entity.AddLog($"[{nameof(input.statusAttackerValue.DmgRate)}: 시전자의 기본 전체 증폭대미지 비율] {MathUtils.ToPermyriadText(input.statusAttackerValue.DmgRate)}");
                    entity.AddLog($"[{nameof(input.statusTargetValue.DmgRateResist)}: 타겟의 전체 증폭대미지 비율 저항] {MathUtils.ToPermyriadText(input.statusTargetValue.DmgRateResist)}");
                    entity.AddLog($"[{nameof(damageRate)}: 전체 증폭대미지 비율] {input.statusAttackerValue.DmgRate} - {input.statusTargetValue.DmgRateResist} = {MathUtils.ToPermyriadText(damageRate)}");
#endif
                    int attackerDistDamageRate; // 시전자의 거리 증폭대미지 비율
                    int targetDistDamageRateResist; // 타겟의 거리 증폭대미지 비율 저항
                    switch (input.skillValue.AttackType)
                    {
                        default:
                        case AttackType.None: // 정의되지 않은 스킬
                            attackerDistDamageRate = 0;
                            targetDistDamageRateResist = 0;
                            break;

                        case AttackType.MeleeAttack: // 근거리스킬
                            attackerDistDamageRate = input.statusAttackerValue.MeleeDmgRate;
                            targetDistDamageRateResist = input.statusTargetValue.MeleeDmgRateResist;
                            break;

                        case AttackType.RangedAttack: // 원거리스킬
                            attackerDistDamageRate = input.statusAttackerValue.RangedDmgRate;
                            targetDistDamageRateResist = input.statusTargetValue.RangedDmgRateResist;
                            break;
                    }

                    distDamageRate = attackerDistDamageRate - targetDistDamageRateResist; // 거리 증폭대미지 비율 = (시전자의 거리 증폭대미지 비율 - 타겟의 거리 증폭대미지 비율 저항)
#if UNITY_EDITOR
                    entity.AddLog(nameof(distDamageRate));
                    entity.AddLog($"[{nameof(input.skillValue.AttackType)}: 스킬 공격 타입] {input.skillValue.AttackType}");
                    entity.AddLog($"[{nameof(attackerDistDamageRate)}: 시전자의 거리 증폭대미지 비율] {MathUtils.ToPermyriadText(attackerDistDamageRate)}");
                    entity.AddLog($"[{nameof(targetDistDamageRateResist)}: 타겟의 거리 증폭대미지 비율 저항] {MathUtils.ToPermyriadText(targetDistDamageRateResist)}");
                    entity.AddLog($"[{nameof(distDamageRate)}: 거리 증폭대미지 비율] {attackerDistDamageRate} - {targetDistDamageRateResist} = {MathUtils.ToPermyriadText(distDamageRate)}");
#endif

                    switch (input.unitTargetValue.UnitMonsterType)
                    {
                        default:
                        case MonsterType.None: // 몬스터 아님
                            monsterDamageRate = 0;
                            break;

                        case MonsterType.Normal: // 일반 몬스터
                            monsterDamageRate = input.statusAttackerValue.NormalMonsterDmgRate;
                            break;

                        case MonsterType.Boss: // 보스 몬스터
                            monsterDamageRate = input.statusAttackerValue.BossMonsterDmgRate;
                            break;
                    }

#if UNITY_EDITOR
                    entity.AddLog(nameof(monsterDamageRate));
                    entity.AddLog($"[{nameof(input.unitTargetValue.UnitMonsterType)}: 타겟의 몬스터 타입] {input.unitTargetValue.UnitMonsterType}");
                    entity.AddLog($"[{nameof(monsterDamageRate)}: 몬스터 증폭대미지 비율] {MathUtils.ToPermyriadText(monsterDamageRate)}");
#endif

                    // 장착한 무기 타입
                    attackerWeaponType = (int)input.itemAttackerValue.WeaponBattleIndex;
                    switch (input.unitTargetValue.UnitSizeType)
                    {
                        default:
                        case UnitSizeType.None: // 타입이 몬스터 타입이 아닐경우
                            unitSizeFactor = 10000; // 공격 대상의 사이즈 타입이 존재하지 않을 때에는 대미지 배율: 10000
                            unitSizeDamageRate = 0; // 공격 대상의 사이즈 타입이 존재하지 않을 때에는 증폭대미지 비율: 0
                            break;

                        case UnitSizeType.Small: // 타겟이 소형 몬스터
                            if (input.itemAttackerValue.WeaponBattleIndex == BattleItemIndex.None)
                            {
                                unitSizeFactor = 10000; // 시전자의 무기가 없을 때에는 대미지 배율: 10000
                            }
                            else
                            {
                                unitSizeFactor = BasisType.SMALL_MONSTER_DAMAGE.GetInt(attackerWeaponType);
                            }
                            unitSizeDamageRate = input.statusAttackerValue.SmallMonsterDmgRate;
                            break;

                        case UnitSizeType.Medium: // 타겟이 중형 몬스터
                            if (input.itemAttackerValue.WeaponBattleIndex == BattleItemIndex.None)
                            {
                                unitSizeFactor = 10000; // 시전자의 무기가 없을 때에는 대미지 배율: 10000
                            }
                            else
                            {
                                unitSizeFactor = BasisType.MEDIUM_MONSTER_DAMAGE.GetInt(attackerWeaponType);
                            }
                            unitSizeDamageRate = input.statusAttackerValue.MediumMonsterDmgRate;
                            break;

                        case UnitSizeType.Large: // 타겟이 대형 몬스터
                            if (input.itemAttackerValue.WeaponBattleIndex == BattleItemIndex.None)
                            {
                                unitSizeFactor = 10000; // 시전자의 무기가 없을 때에는 대미지 배율: 10000
                            }
                            else
                            {
                                unitSizeFactor = BasisType.LARGE_MONSTER_DAMAGE.GetInt(attackerWeaponType);
                            }
                            unitSizeDamageRate = input.statusAttackerValue.LargeMonsterDmgRate;
                            break;
                    }

#if UNITY_EDITOR
                    entity.AddLog(nameof(unitSizeFactor));
                    entity.AddLog($"[{nameof(input.unitTargetValue.UnitSizeType)}: 타겟의 유닛 사이즈 타입] {input.unitTargetValue.UnitSizeType}");
                    entity.AddLog($"[{nameof(input.itemAttackerValue.WeaponBattleIndex)}: 시전자의 무기 타입] {input.itemAttackerValue.WeaponBattleIndex}");
                    entity.AddLog($"[{nameof(unitSizeFactor)}: 유닛사이즈 배율 (기초데이터)] {MathUtils.ToPermyriadText(unitSizeFactor)}");

                    entity.AddLog(nameof(unitSizeDamageRate));
                    entity.AddLog($"[{nameof(unitSizeDamageRate)}: 유닛사이즈 증폭대미지 비율] {MathUtils.ToPermyriadText(unitSizeDamageRate)}");
#endif

                    int skillDamageRate = input.statusAttackerValue.GetSkillDamageRate(input.skillValue.SkillId); // 특정 스킬 증폭대미지 비율
#if UNITY_EDITOR
                    entity.AddLog(nameof(skillDamageRate));
                    entity.AddLog($"[{nameof(input.skillValue.SkillId)}: 스킬 Id] {input.skillValue.SkillId}");
                    entity.AddLog($"[{nameof(skillDamageRate)}: 특정 스킬 증폭대미지 비율] {MathUtils.ToPermyriadText(skillDamageRate)}");
#endif

                    int defenseCoefficient = BasisType.DEFENSE_COEFFICIENT.GetInt(); // 방어력 적용 계수
                    int randomDamageRange = BasisType.RANDOM_DAMAGE_RANGE.GetInt(); // 랜덤 타격 계수
                    if (!options.HasValue(BattleOptionType.Damage))
                    {
                        rawDamageValue = 0;
                        damageRandomSeq = 0;
                        damageRandomValue = 0;
                        damageValue = 0;
                        targetDef = 0;
                        damageDecreaseRate = 0;
                        damage = 0;
                    }
                    else
                    {
                        int attackerAtk; // 시전자의 물리공격력
                        switch (input.skillValue.AttackType)
                        {
                            default:
                            case AttackType.None: // 정의되지 않은 스킬
                                attackerAtk = 0;
                                Debug.LogError($"정의되지 않은 {nameof(AttackType)} 입니다: {input.skillValue.AttackType}");
                                break;

                            case AttackType.MeleeAttack: // 근거리스킬
                                attackerAtk = input.statusAttackerValue.MeleeAtk;
                                break;

                            case AttackType.RangedAttack: // 원거리스킬
                                attackerAtk = input.statusAttackerValue.RangedAtk;
                                break;
                        }

                        BattleStatus optionsDamage = options.Get(BattleOptionType.Damage); // 물리대미지 (주의: 기존의 스탯 계산과는 다른 방식으로 처리! 퍼센테이지 계산을 먼저 한다)
                        rawDamageValue = MathUtils.ToInt(attackerAtk * MathUtils.ToPermyriadValue(optionsDamage.perValue) + optionsDamage.value); // 시전자의 물리 공격력에 스킬 물리 공격 적용
                        damageRandomSeq = input.randomDamage.GetRandomSeq(); // 랜덤 시퀀스
                        damageRandomValue = input.randomDamage.GetRandomRange(damageRandomSeq, -randomDamageRange, randomDamageRange); // 랜덤 타격계수
                        damageValue = MathUtils.ToInt(rawDamageValue * (1 + MathUtils.ToPermyriadValue(damageRandomValue))); // 물리 공격력에 랜덤 타격계수 적용
                        targetDef = Mathf.Max(0, MathUtils.ToInt(input.statusTargetValue.Def * (1 - MathUtils.ToPermyriadValue(input.crowdControlTargetValue.DefDecreaseRate)))); // 방어력은 0 이상
                        damageDecreaseRate = Mathf.Clamp01(MathUtils.GetRate(damageValue + defenseCoefficient, targetDef + defenseCoefficient)); // 물리대미지감소 비율은 0에서 1사이
                        damage = MathUtils.ToInt(damageValue * damageDecreaseRate);

#if UNITY_EDITOR
                        entity.AddLog(nameof(damage));
                        entity.AddLog($"[{nameof(input.skillValue.AttackType)}: 스킬 공격 타입] {input.skillValue.AttackType}");
                        entity.AddLog($"[{nameof(attackerAtk)}: 시전자의 물리공격력] {attackerAtk}");
                        entity.AddLog($"[{nameof(optionsDamage)}: {optionsDamage}] ({attackerAtk} x {optionsDamage.perValue} x 0.0001) + {optionsDamage.value} = {rawDamageValue}");
                        entity.AddLog($"[{nameof(damageRandomSeq)}: 물리공격 랜덤 시퀀스] {damageRandomSeq}");
                        entity.AddLog($"[{nameof(randomDamageRange)}: 기초 데이터] {randomDamageRange}");
                        entity.AddLog($"[{nameof(damageRandomValue)}: 물리공격 랜덤 타격계수] {damageRandomValue}");
                        entity.AddLog($"[{nameof(damageValue)}: 물리 공격력(랜덤 타격계수 적용)] {rawDamageValue} x (1 + {damageRandomValue} x 0.0001) = {damageValue}");
                        entity.AddLog($"[{nameof(input.statusTargetValue.Def)}: 타겟의 기본 물리방어력] {input.statusTargetValue.Def}");
                        entity.AddLog($"[{nameof(input.crowdControlTargetValue.DefDecreaseRate)}: 타겟의 물리방어력 감소율(상태이상)] {MathUtils.ToPermyriadText(input.crowdControlTargetValue.DefDecreaseRate)}");
                        entity.AddLog($"[{nameof(targetDef)}: 타겟의 물리방어력] {input.statusTargetValue.Def} x (1 - {input.crowdControlTargetValue.DefDecreaseRate} x 0.0001) = {targetDef}");
                        entity.AddLog($"[{nameof(defenseCoefficient)}: 기초데이터] {defenseCoefficient}");
                        entity.AddLog($"[{nameof(damageDecreaseRate)}]: 물리대미지감소 비율] ({damageValue} + {defenseCoefficient}) / ({targetDef} + {defenseCoefficient}) = {damageValue + defenseCoefficient} / {targetDef + defenseCoefficient} = {damageDecreaseRate}");
                        entity.AddLog($"[{nameof(damage)}]: 물리대미지] {damageValue} x {damageDecreaseRate} = {damage}");
#endif
                    }

                    if (!options.HasValue(BattleOptionType.MDamage))
                    {
                        attackerMAtk = 0;
                        rawMDamageValue = 0;
                        mdamageRandomSeq = 0;
                        mdamageRandomValue = 0;
                        mdamageValue = 0;
                        targetMDef = 0;
                        mdamageDecreaseRate = 0;
                        mdamage = 0;
                    }
                    else
                    {
                        attackerMAtk = input.statusAttackerValue.MAtk; // 시전자의 마법공격력
                        BattleStatus optionMDamage = options.Get(BattleOptionType.MDamage); // 마법대미지 (주의: 기존의 스탯 계산과는 다른 방식으로 처리! 퍼센테이지 계산을 먼저 한다)
                        rawMDamageValue = MathUtils.ToInt(attackerMAtk * MathUtils.ToPermyriadValue(optionMDamage.perValue) + optionMDamage.value); // 시전자의 마법 공격력에 스킬 마법 공격 적용
                        mdamageRandomSeq = input.randomDamage.GetRandomSeq(); // 랜덤 시퀀스
                        mdamageRandomValue = input.randomDamage.GetRandomRange(mdamageRandomSeq, -randomDamageRange, randomDamageRange); // 랜덤 타격 계수
                        mdamageValue = MathUtils.ToInt(rawMDamageValue * (1 + MathUtils.ToPermyriadValue(mdamageRandomValue))); // 마법 공격력에 랜덤 타격계수 적용
                        targetMDef = Mathf.Max(0, MathUtils.ToInt(input.statusTargetValue.MDef * (1 - MathUtils.ToPermyriadValue(input.crowdControlTargetValue.MdefDecreaseRate)))); // 방어력은 0 이상
                        mdamageDecreaseRate = Mathf.Clamp01(MathUtils.GetRate(mdamageValue + defenseCoefficient, targetMDef + defenseCoefficient)); // 마법대미지감소 비율은 0에서 1사이
                        mdamage = MathUtils.ToInt(mdamageValue * mdamageDecreaseRate);


#if UNITY_EDITOR
                        entity.AddLog(nameof(mdamage));
                        entity.AddLog($"[{nameof(attackerMAtk)}: 시전자의 마법공격력] {attackerMAtk}");
                        entity.AddLog($"[{nameof(optionMDamage)}: {optionMDamage}] ({attackerMAtk} x {optionMDamage.perValue} x 0.0001) + {optionMDamage.value} = {rawMDamageValue}");
                        entity.AddLog($"[{nameof(mdamageRandomSeq)}: 마법공격 랜덤 시퀀스] {mdamageRandomSeq}");
                        entity.AddLog($"[{nameof(randomDamageRange)}: 기초 데이터] {randomDamageRange}");
                        entity.AddLog($"[{nameof(mdamageRandomValue)}: 마법공격 랜덤 타격계수] {mdamageRandomValue}");
                        entity.AddLog($"[{nameof(mdamageValue)}: 마법 공격력(랜덤 타격계수 적용)] {rawMDamageValue} x (1 + {mdamageRandomValue} x 0.0001) = {mdamageValue}");
                        entity.AddLog($"[{nameof(input.statusTargetValue.MDef)}: 타겟의 기본 마법방어력] {input.statusTargetValue.MDef}");
                        entity.AddLog($"[{nameof(input.crowdControlTargetValue.MdefDecreaseRate)}: 타겟의 마법방어력 감소율(상태이상)] {MathUtils.ToPermyriadText(input.crowdControlTargetValue.MdefDecreaseRate)}");
                        entity.AddLog($"[{nameof(targetMDef)}: 타겟의 마법방어력] {input.statusTargetValue.MDef} x (1 - {input.crowdControlTargetValue.MdefDecreaseRate} x 0.0001) = {targetMDef}");
                        entity.AddLog($"[{nameof(defenseCoefficient)}: 기초데이터] {defenseCoefficient}");
                        entity.AddLog($"[{nameof(mdamageDecreaseRate)}]: 마법대미지감소 비율] ({mdamageValue} + {defenseCoefficient}) / ({targetMDef} + {defenseCoefficient}) = {mdamageValue + defenseCoefficient} / {targetMDef + defenseCoefficient} = {mdamageDecreaseRate}");
                        entity.AddLog($"[{nameof(mdamage)}]: 마법대미지] {mdamageValue} x {mdamageDecreaseRate} = {mdamage}");
#endif
                    }

                    sumDamage = damage + mdamage; // 대미지총합 = (물리대미지 + 마법대미지)
                    damageFactor = (unitSizeFactor + elementFactor) / 2; // 대미지 배율 계산
                    normalDamage = MathUtils.ToInt(sumDamage * MathUtils.ToPermyriadValue(damageFactor)); // 대미지 배율 적용
                    sumPlusDamageRate = criDamageRate + damageRate + distDamageRate + monsterDamageRate + elementDamageRate + unitSizeDamageRate + skillDamageRate; // 증폭대미지 비율 총합
                    plusDamage = MathUtils.ToInt(normalDamage * MathUtils.ToPermyriadValue(sumPlusDamageRate)); // 증폭대미지 비율 적용
                    rawTotalDamage = MathUtils.ToInt((normalDamage + plusDamage) * (1 - MathUtils.ToPermyriadValue(input.crowdControlAttackerValue.TotalDmgDecreaseRate))); // 시전자의 전체 대미지 비율 감소율 적용
                    applyedDecreaseRateDamage = MathUtils.ToInt(rawTotalDamage * (1 - input.dcecreaseDamageRatePer)); // 보정에 따른 비율 감소율 적용
                    totalDamage = Mathf.Max(1, applyedDecreaseRateDamage); // 최종 대미지는 1 이상

#if UNITY_EDITOR
                    entity.AddLog(nameof(normalDamage));
                    entity.AddLog($"[{nameof(sumDamage)}: 대미지 (총합)] {damage} + {mdamage} = {sumDamage}");
                    entity.AddLog($"[{nameof(unitSizeFactor)}: 유닛사이즈 배율] {MathUtils.ToPermyriadText(unitSizeFactor)}");
                    entity.AddLog($"[{nameof(elementFactor)}: 속성 배율] = {MathUtils.ToPermyriadText(elementFactor)}");
                    entity.AddLog($"[{nameof(damageFactor)}: 대미지 배율] ⇒ ({unitSizeFactor} + {elementFactor}) / 2 = {MathUtils.ToPermyriadText(damageFactor)}");
                    entity.AddLog($"[{nameof(normalDamage)}: 대미지 배율 적용] ⇒ ({sumDamage} x {damageFactor} x 0.0001) = {normalDamage}");

                    entity.AddLog(nameof(plusDamage));
                    entity.AddLog($"[{nameof(criDamageRate)}: 치명타 증폭대미지 비율] {MathUtils.ToPermyriadText(criDamageRate)}");
                    entity.AddLog($"[{nameof(damageRate)}: 전체 증폭대미지 비율] {MathUtils.ToPermyriadText(damageRate)}");
                    entity.AddLog($"[{nameof(distDamageRate)}: 거리 증폭대미지 비율] {MathUtils.ToPermyriadText(distDamageRate)}");
                    entity.AddLog($"[{nameof(monsterDamageRate)}: 몬스터 증폭대미지 비율] {MathUtils.ToPermyriadText(monsterDamageRate)}");
                    entity.AddLog($"[{nameof(elementDamageRate)}: 속성 증폭대미지 비율] {MathUtils.ToPermyriadText(elementDamageRate)}");
                    entity.AddLog($"[{nameof(unitSizeDamageRate)}: 유닛사이즈 증폭대미지 비율] {MathUtils.ToPermyriadText(unitSizeDamageRate)}");
                    entity.AddLog($"[{nameof(skillDamageRate)}: 특정 스킬 증폭대미지 비율] {MathUtils.ToPermyriadText(skillDamageRate)}");
                    entity.AddLog($"[{nameof(sumPlusDamageRate)}: 증폭대미지 비율 총합] {criDamageRate} + {damageRate} + {distDamageRate} + {monsterDamageRate} + {unitSizeFactor} + {elementDamageRate} + {skillDamageRate} = {MathUtils.ToPermyriadText(sumPlusDamageRate)}");
                    entity.AddLog($"[{nameof(plusDamage)}: 추가 대미지] {normalDamage} x {sumPlusDamageRate} x 0.0001 = {plusDamage}");

                    entity.AddLog(nameof(totalDamage));
                    entity.AddLog($"[{nameof(input.crowdControlAttackerValue.TotalDmgDecreaseRate)}: 시전자의 전체 대미지 비율 감소율(상태이상)] {MathUtils.ToPermyriadText(input.crowdControlAttackerValue.TotalDmgDecreaseRate)}");
                    entity.AddLog($"[{nameof(rawTotalDamage)}: 대미지 비율 감소 적용한 최종대미지] ({normalDamage} + {plusDamage}) x (1 - {input.crowdControlAttackerValue.TotalDmgDecreaseRate} x 0.0001) = {rawTotalDamage}");
                    entity.AddLog($"[{nameof(applyedDecreaseRateDamage)}: 보정값이 적용된 최종대미지] ({rawTotalDamage}) x (1 - {input.dcecreaseDamageRatePer}) = {applyedDecreaseRateDamage}");
                    entity.AddLog($"[{nameof(totalDamage)}: 최종 대미지] {totalDamage}");
#endif

                    // 대미지 흡수 옵션이 존재하지 않을 경우
                    if (!hasAbsorbRecovery)
                    {
                        absorbRecovery = 0;
                    }
                    else
                    {
                        BattleStatus optionAbsorbHp = options.Get(BattleOptionType.AbsorbHp);
                        int rowAbsorbRecovery = optionAbsorbHp.value + MathUtils.ToInt(totalDamage * MathUtils.ToPermyriadValue(optionAbsorbHp.perValue));
                        absorbRecovery = Mathf.Max(1, rowAbsorbRecovery); // 최종 흡수량은 1 이상

#if UNITY_EDITOR
                        entity.AddLog(nameof(absorbRecovery));
                        entity.AddLog($"[{nameof(totalDamage)}: 최종 대미지] {totalDamage}");
                        entity.AddLog($"[{nameof(optionAbsorbHp)}: {optionAbsorbHp}] {optionAbsorbHp.value} + ({totalDamage} x {optionAbsorbHp.perValue} x 0.0001) = {rowAbsorbRecovery}");
                        entity.AddLog($"[{nameof(absorbRecovery)}] {absorbRecovery}");
#endif
                    }
                }

                int stunRate, silenceRate, sleepRate, hallucinationRate, bleedingRate, burningRate, poisonRate, curseRate, freezingRate, frozenRate; // 옵션에 따른 상태이상 확률

                // 상태이상 옵션이 존재하지 않을 경우
                if (!options.HasCrowdControl)
                {
                    stunRate = silenceRate = sleepRate = hallucinationRate = bleedingRate = burningRate = poisonRate = curseRate = freezingRate = frozenRate = 0;
                }
                else
                {
                    BattleStatus optionStun = options.GetCrowdControl(CrowdControlType.Stun);
                    BattleStatus optionSilence = options.GetCrowdControl(CrowdControlType.Silence);
                    BattleStatus optionSleep = options.GetCrowdControl(CrowdControlType.Sleep);
                    BattleStatus optionHallucination = options.GetCrowdControl(CrowdControlType.Hallucination);
                    BattleStatus optionBleeding = options.GetCrowdControl(CrowdControlType.Bleeding);
                    BattleStatus optionBurning = options.GetCrowdControl(CrowdControlType.Burning);
                    BattleStatus optionPoison = options.GetCrowdControl(CrowdControlType.Poison);
                    BattleStatus optionCurse = options.GetCrowdControl(CrowdControlType.Curse);
                    BattleStatus optionFreezing = options.GetCrowdControl(CrowdControlType.Freezing);
                    BattleStatus optionFrozen = options.GetCrowdControl(CrowdControlType.Frozen);

                    stunRate = optionStun.GetStatusValue();
                    silenceRate = optionSilence.GetStatusValue();
                    sleepRate = optionSleep.GetStatusValue();
                    hallucinationRate = optionHallucination.GetStatusValue();
                    bleedingRate = optionBleeding.GetStatusValue();
                    burningRate = optionBurning.GetStatusValue();
                    poisonRate = optionPoison.GetStatusValue();
                    curseRate = optionCurse.GetStatusValue();
                    freezingRate = optionFreezing.GetStatusValue();
                    frozenRate = optionFrozen.GetStatusValue();

#if UNITY_EDITOR
                    entity.AddLog(nameof(CrowdControlType));
                    entity.AddLog($"[{nameof(optionStun)}: {optionStun}] {optionStun.GetStatusValueText()} = {MathUtils.ToPermyriadText(stunRate)}");
                    entity.AddLog($"[{nameof(optionSilence)}: {optionSilence}] {optionSilence.GetStatusValueText()} = {MathUtils.ToPermyriadText(silenceRate)}");
                    entity.AddLog($"[{nameof(optionSleep)}: {optionSleep}] {optionSleep.GetStatusValueText()} = {MathUtils.ToPermyriadText(sleepRate)}");
                    entity.AddLog($"[{nameof(optionHallucination)}: {optionHallucination}] {optionHallucination.GetStatusValueText()} = {MathUtils.ToPermyriadText(hallucinationRate)}");
                    entity.AddLog($"[{nameof(optionBleeding)}: {optionBleeding}] {optionBleeding.GetStatusValueText()} = {MathUtils.ToPermyriadText(bleedingRate)}");
                    entity.AddLog($"[{nameof(optionBurning)}: {optionBurning}] {optionBurning.GetStatusValueText()} = {MathUtils.ToPermyriadText(burningRate)}");
                    entity.AddLog($"[{nameof(optionPoison)}: {optionPoison}] {optionPoison.GetStatusValueText()} = {MathUtils.ToPermyriadText(poisonRate)}");
                    entity.AddLog($"[{nameof(optionCurse)}: {optionCurse}] {optionCurse.GetStatusValueText()} = {MathUtils.ToPermyriadText(curseRate)}");
                    entity.AddLog($"[{nameof(optionFreezing)}: {optionFreezing}] {optionFreezing.GetStatusValueText()} = {MathUtils.ToPermyriadText(freezingRate)}");
                    entity.AddLog($"[{nameof(optionFrozen)}: {optionFrozen}] {optionFrozen.GetStatusValueText()} = {MathUtils.ToPermyriadText(frozenRate)}");
#endif
                }

                int rowTotalStunRate = basicActiveSkillStunRate + stunRate - input.statusTargetValue.StunRateResist;
                int totalStunRate = Mathf.Max(0, rowTotalStunRate);
                isStun = MathUtils.IsCheckPermyriad(totalStunRate);
#if UNITY_EDITOR
                entity.AddLog(nameof(isStun));
                entity.AddLog($"[{nameof(basicActiveSkillStunRate)}: 평타에 의한 추가 스턴 확률] {MathUtils.ToPermyriadText(basicActiveSkillStunRate)}");
                entity.AddLog($"[{nameof(stunRate)}: 스킬 스턴 확률] {MathUtils.ToPermyriadText(stunRate)}");
                entity.AddLog($"[{nameof(input.statusTargetValue.StunRateResist)}: 타겟의 스턴 확률 저항] {MathUtils.ToPermyriadText(input.statusTargetValue.StunRateResist)}");
                entity.AddLog($"[{nameof(totalStunRate)}: 스턴 확률] {basicActiveSkillStunRate} + {stunRate} - {input.statusTargetValue.StunRateResist} = {MathUtils.ToPermyriadText(totalStunRate)} ⇒ {isStun}");
#endif

                int rowTotalSilenceRate = basicActiveSkillSilenceRate + silenceRate - input.statusTargetValue.SilenceRateResist;
                int totalSilenceRate = Mathf.Max(0, rowTotalSilenceRate);
                isSilence = MathUtils.IsCheckPermyriad(totalSilenceRate);
#if UNITY_EDITOR
                entity.AddLog(nameof(isSilence));
                entity.AddLog($"[{nameof(basicActiveSkillSilenceRate)}: 평타에 의한 추가 침묵 확률] {MathUtils.ToPermyriadText(basicActiveSkillSilenceRate)}");
                entity.AddLog($"[{nameof(silenceRate)}: 스킬 침묵 확률] {MathUtils.ToPermyriadText(silenceRate)}");
                entity.AddLog($"[{nameof(input.statusTargetValue.SilenceRateResist)}: 타겟의 침묵 확률 저항] {MathUtils.ToPermyriadText(input.statusTargetValue.SilenceRateResist)}");
                entity.AddLog($"[{nameof(totalSilenceRate)}: 침묵 확률] {basicActiveSkillSilenceRate} + {silenceRate} - {input.statusTargetValue.SilenceRateResist} = {MathUtils.ToPermyriadText(totalSilenceRate)} ⇒ {isSilence}");
#endif

                int rowTotalSleepRate = basicActiveSkillSleepRate + sleepRate - input.statusTargetValue.SleepRateResist;
                int totalSleepRate = Mathf.Max(0, rowTotalSleepRate);
                isSleep = MathUtils.IsCheckPermyriad(totalSleepRate);
#if UNITY_EDITOR
                entity.AddLog(nameof(isSleep));
                entity.AddLog($"[{nameof(basicActiveSkillSleepRate)}: 평타에 의한 추가 수면 확률] {MathUtils.ToPermyriadText(basicActiveSkillSleepRate)}");
                entity.AddLog($"[{nameof(sleepRate)}: 스킬 수면 확률] {MathUtils.ToPermyriadText(sleepRate)}");
                entity.AddLog($"[{nameof(input.statusTargetValue.SleepRateResist)}: 타겟의 수면 확률 저항] {MathUtils.ToPermyriadText(input.statusTargetValue.SleepRateResist)}");
                entity.AddLog($"[{nameof(totalSleepRate)}: 수면 확률] {basicActiveSkillSleepRate} + {sleepRate} - {input.statusTargetValue.SleepRateResist} = {MathUtils.ToPermyriadText(totalSleepRate)} ⇒ {isSleep}");
#endif

                int rowTotalHallucinationRate = basicActiveSkillHallucinationRate + hallucinationRate - input.statusTargetValue.HallucinationRateResist;
                int totalHallucinationRate = Mathf.Max(0, rowTotalHallucinationRate);
                isHallucination = MathUtils.IsCheckPermyriad(totalHallucinationRate);
#if UNITY_EDITOR
                entity.AddLog(nameof(isHallucination));
                entity.AddLog($"[{nameof(basicActiveSkillHallucinationRate)}: 평타에 의한 추가 환각 확률] {MathUtils.ToPermyriadText(basicActiveSkillHallucinationRate)}");
                entity.AddLog($"[{nameof(hallucinationRate)}: 스킬 환각 확률] {MathUtils.ToPermyriadText(hallucinationRate)}");
                entity.AddLog($"[{nameof(input.statusTargetValue.HallucinationRateResist)}: 타겟의 환각 확률 저항] {MathUtils.ToPermyriadText(input.statusTargetValue.HallucinationRateResist)}");
                entity.AddLog($"[{nameof(totalHallucinationRate)}: 환각 확률] {basicActiveSkillHallucinationRate} + {hallucinationRate} - {input.statusTargetValue.HallucinationRateResist} = {MathUtils.ToPermyriadText(totalHallucinationRate)} ⇒ {isHallucination}");
#endif

                int rowTotalBleedingRate = basicActiveSkillBleedingRate + bleedingRate - input.statusTargetValue.BleedingRateResist;
                int totalBleedingRate = Mathf.Max(0, rowTotalBleedingRate);
                isBleeding = MathUtils.IsCheckPermyriad(totalBleedingRate);
#if UNITY_EDITOR
                entity.AddLog(nameof(isBleeding));
                entity.AddLog($"[{nameof(basicActiveSkillBleedingRate)}: 평타에 의한 추가 출혈 확률] {MathUtils.ToPermyriadText(basicActiveSkillBleedingRate)}");
                entity.AddLog($"[{nameof(bleedingRate)}: 스킬 출혈 확률] {MathUtils.ToPermyriadText(bleedingRate)}");
                entity.AddLog($"[{nameof(input.statusTargetValue.BleedingRateResist)}: 타겟의 출혈 확률 저항] {MathUtils.ToPermyriadText(input.statusTargetValue.BleedingRateResist)}");
                entity.AddLog($"[{nameof(totalBleedingRate)}: 출혈 확률] {basicActiveSkillBleedingRate} + {bleedingRate} - {input.statusTargetValue.BleedingRateResist} = {MathUtils.ToPermyriadText(totalBleedingRate)} ⇒ {isBleeding}");
#endif

                int rowTotalBurningRate = basicActiveSkillBurningRate + burningRate - input.statusTargetValue.BurningRateResist;
                int totalBurningRate = Mathf.Max(0, rowTotalBurningRate);
                isBurning = MathUtils.IsCheckPermyriad(totalBurningRate);
#if UNITY_EDITOR
                entity.AddLog(nameof(isBurning));
                entity.AddLog($"[{nameof(basicActiveSkillBurningRate)}: 평타에 의한 추가 화상 확률] {MathUtils.ToPermyriadText(basicActiveSkillBurningRate)}");
                entity.AddLog($"[{nameof(burningRate)}: 스킬 화상 확률] {MathUtils.ToPermyriadText(burningRate)}");
                entity.AddLog($"[{nameof(input.statusTargetValue.BurningRateResist)}: 타겟의 화상 확률 저항] {MathUtils.ToPermyriadText(input.statusTargetValue.BurningRateResist)}");
                entity.AddLog($"[{nameof(totalBurningRate)}: 화상 확률] {basicActiveSkillBurningRate} + {burningRate} - {input.statusTargetValue.BurningRateResist} = {MathUtils.ToPermyriadText(totalBurningRate)} ⇒ {isBurning}");
#endif

                int rowTotalPoisonRate = basicActiveSkillPoisonRate + poisonRate - input.statusTargetValue.PoisonRateResist;
                int totalPoisonRate = Mathf.Max(0, rowTotalPoisonRate);
                isPoison = MathUtils.IsCheckPermyriad(totalPoisonRate);
#if UNITY_EDITOR
                entity.AddLog(nameof(isPoison));
                entity.AddLog($"[{nameof(basicActiveSkillPoisonRate)}: 평타에 의한 추가 중독 확률] {MathUtils.ToPermyriadText(basicActiveSkillPoisonRate)}");
                entity.AddLog($"[{nameof(poisonRate)}: 스킬 중독 확률] {MathUtils.ToPermyriadText(poisonRate)}");
                entity.AddLog($"[{nameof(input.statusTargetValue.PoisonRateResist)}: 타겟의 중독 확률 저항] {MathUtils.ToPermyriadText(input.statusTargetValue.PoisonRateResist)}");
                entity.AddLog($"[{nameof(totalPoisonRate)}: 중독 확률] {basicActiveSkillPoisonRate} + {poisonRate} - {input.statusTargetValue.PoisonRateResist} = {MathUtils.ToPermyriadText(totalPoisonRate)} ⇒ {isPoison}");
#endif

                int rowTotalCurseRate = basicActiveSkillCurseRate + curseRate - input.statusTargetValue.CurseRateResist;
                int totalCurseRate = Mathf.Max(0, rowTotalCurseRate);
                isCurse = MathUtils.IsCheckPermyriad(totalCurseRate);
#if UNITY_EDITOR
                entity.AddLog(nameof(isCurse));
                entity.AddLog($"[{nameof(basicActiveSkillCurseRate)}: 평타에 의한 추가 저주 확률] {MathUtils.ToPermyriadText(basicActiveSkillCurseRate)}");
                entity.AddLog($"[{nameof(curseRate)}: 스킬 저주 확률] {MathUtils.ToPermyriadText(curseRate)}");
                entity.AddLog($"[{nameof(input.statusTargetValue.CurseRateResist)}: 타겟의 저주 확률 저항] {MathUtils.ToPermyriadText(input.statusTargetValue.CurseRateResist)}");
                entity.AddLog($"[{nameof(totalCurseRate)}: 저주 확률] {basicActiveSkillCurseRate} + {curseRate} - {input.statusTargetValue.CurseRateResist} = {MathUtils.ToPermyriadText(totalCurseRate)} ⇒ {isCurse}");
#endif

                int rowTotalFreezingRate = basicActiveSkillFreezingRate + freezingRate - input.statusTargetValue.FreezingRateResist;
                int totalFreezingRate = Mathf.Max(0, rowTotalFreezingRate);
                isFreezing = MathUtils.IsCheckPermyriad(totalFreezingRate);
#if UNITY_EDITOR
                entity.AddLog(nameof(isFreezing));
                entity.AddLog($"[{nameof(basicActiveSkillFreezingRate)}: 평타에 의한 추가 빙결 확률] {MathUtils.ToPermyriadText(basicActiveSkillFreezingRate)}");
                entity.AddLog($"[{nameof(freezingRate)}: 스킬 빙결 확률] {MathUtils.ToPermyriadText(freezingRate)}");
                entity.AddLog($"[{nameof(input.statusTargetValue.FreezingRateResist)}: 타겟의 빙결 확률 저항] {MathUtils.ToPermyriadText(input.statusTargetValue.FreezingRateResist)}");
                entity.AddLog($"[{nameof(totalFreezingRate)}: 빙결 확률] {basicActiveSkillFreezingRate} + {freezingRate} - {input.statusTargetValue.FreezingRateResist} = {MathUtils.ToPermyriadText(totalFreezingRate)} ⇒ {isFreezing}");
#endif

                int rowTotalFrozenRate = basicActiveSkillFrozenRate + frozenRate - input.statusTargetValue.FrozenRateResist;
                int totalFrozenRate = Mathf.Max(0, rowTotalFrozenRate);
                isFrozen = MathUtils.IsCheckPermyriad(totalFrozenRate);
#if UNITY_EDITOR
                entity.AddLog(nameof(isFrozen));
                entity.AddLog($"[{nameof(basicActiveSkillFrozenRate)}: 평타에 의한 추가 동빙 확률] {MathUtils.ToPermyriadText(basicActiveSkillFrozenRate)}");
                entity.AddLog($"[{nameof(frozenRate)}: 스킬 동빙 확률] {MathUtils.ToPermyriadText(frozenRate)}");
                entity.AddLog($"[{nameof(input.statusTargetValue.FrozenRateResist)}: 타겟의 동빙 확률 저항] {MathUtils.ToPermyriadText(input.statusTargetValue.FrozenRateResist)}");
                entity.AddLog($"[{nameof(totalFrozenRate)}: 동빙 확률] {basicActiveSkillFrozenRate} + {frozenRate} - {input.statusTargetValue.FrozenRateResist} = {MathUtils.ToPermyriadText(totalFrozenRate)} ⇒ {isFrozen}");
#endif
            }

#if UNITY_EDITOR
            entity.FinishLog();
#endif

            return new SkillOutput
            {
                isAutoGuard = isAutoGuard,
                isFlee = isFlee,
                isCritical = isCritical,
                hasDamage = hasDamage,
                totalDamage = totalDamage,
                hasRecovery = hasRecovery,
                totalRecovery = totalRecovery,
                hasAbsorbRecovery = hasAbsorbRecovery,
                absorbRecovery = absorbRecovery,

                crowdControlSettings = new BattleCrowdControlInfo.ApplySettings
                {
                    isStun = isStun,
                    isSilence = isSilence,
                    isSleep = isSleep,
                    isHallucination = isHallucination,
                    isBleeding = isBleeding,
                    isBurning = isBurning,
                    isPoison = isPoison,
                    isCurse = isCurse,
                    isFreezing = isFreezing,
                    isFrozen = isFrozen,
                },

                criDamageRate = criDamageRate,
                elementFactor = elementFactor,
                attackerElementDamageRate = attackerElementDamageRate,
                targetElementDamageRateResist = targetElementDamageRateResist,
                elementDamageRate = elementDamageRate,
                damageRate = damageRate,
                distDamageRate = distDamageRate,
                monsterDamageRate = monsterDamageRate,
                attackerWeaponType = attackerWeaponType,
                unitSizeFactor = unitSizeFactor,
                unitSizeDamageRate = unitSizeDamageRate,

                rawDamageValue = rawDamageValue,
                damageRandomSeq = damageRandomSeq,
                damageRandomValue = damageRandomValue,
                damageValue = damageValue,
                targetDef = targetDef,
                damageDecreaseRate = damageDecreaseRate,
                damage = damage,

                attackerMAtk = attackerMAtk,
                rawMDamageValue = rawMDamageValue,
                mdamageRandomSeq = mdamageRandomSeq,
                mdamageRandomValue = mdamageRandomValue,
                mdamageValue = mdamageValue,
                targetMDef = targetMDef,
                mdamageDecreaseRate = mdamageDecreaseRate,
                mdamage = mdamage,

                sumDamage = sumDamage,
                damageFactor = damageFactor,
                normalDamage = normalDamage,
                sumPlusDamageRate = sumPlusDamageRate,
                plusDamage = plusDamage,
                rawTotalDamage = rawTotalDamage,
                applyedDecreaseRateDamage = applyedDecreaseRateDamage,
            };
        }
    }
}