using CodeStage.AntiCheat.ObscuredTypes;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class BattleStatusInfo : BattleStatusInfo.ITargetValue, BattleStatusInfo.IAttackerValue
    {
        
        public struct Settings
        {
            public readonly static Settings DEFAULT = new Settings()
            {
                atkSpd = 10000,
                moveSpd = 10000,
                atkRange = 10000,
                regenMp = 1,
            };

            public int str, agi, vit, @int, dex, luk;
            public int maxHp, regenHp;
            public int regenMp;
            public int meleeAtk, rangedAtk, matk, def, mdef;
            public int hit, flee, atkSpd, criRate, criRateResist, criDmgRate, atkRange, moveSpd, cooldownRate;
            public int dmgRate, dmgRateResist, meleeDmgRate, meleeDmgRateResist, rangedDmgRate, rangedDmgRateResist, normalMonsterDmgRate, bossMonsterDmgRate, smallMonsterDmgRate, mediumMonsterDmgRate, largeMonsterDmgRate;
            public int stunRateResist, silenceRateResist, sleepRateResist, hallucinationRateResist, bleedingRateResist, burningRateResist, poisonRateResist, curseRateResist, freezingRateResist, frozenRateResist;
            public int basicActiveSkillStunRate, basicActiveSkillSilenceRate, basicActiveSkillSleepRate, basicActiveSkillHallucinationRate, basicActiveSkillBleedingRate, basicActiveSkillBurningRate, basicActiveSkillPoisonRate, basicActiveSkillCurseRate, basicActiveSkillFreezingRate, basicActiveSkillFrozenRate;
            public int neutralDmgRate, fireDmgRate, waterDmgRate, windDmgRate, earthDmgRate, poisonDmgRate, holyDmgRate, shadowDmgRate, ghostDmgRate, undeadDmgRate;
            public int neutralDmgRateResist, fireDmgRateResist, waterDmgRateResist, windDmgRateResist, earthDmgRateResist, poisonDmgRateResist, holyDmgRateResist, shadowDmgRateResist, ghostDmgRateResist, undeadDmgRateResist;
            public int autoGuardRate, devoteRate;
            public int expDropRate, jobExpDropRate, zenyDropRate, itemDropRate;            
        }

        public interface ITargetValue
        {
            int Def { get; }
            int MDef { get; }

            int Flee { get; }

            int CriRateResist { get; }

            int DmgRateResist { get; }
            int MeleeDmgRateResist { get; }
            int RangedDmgRateResist { get; }

            int StunRateResist { get; }
            int SilenceRateResist { get; }
            int SleepRateResist { get; }
            int HallucinationRateResist { get; }
            int BleedingRateResist { get; }
            int BurningRateResist { get; }
            int PoisonRateResist { get; }
            int CurseRateResist { get; }
            int FreezingRateResist { get; }
            int FrozenRateResist { get; }

            int NeutralDmgRateResist { get; }
            int FireDmgRateResist { get; }
            int WaterDmgRateResist { get; }
            int WindDmgRateResist { get; }
            int EarthDmgRateResist { get; }
            int PoisonDmgRateResist { get; }
            int HolyDmgRateResist { get; }
            int ShadowDmgRateResist { get; }
            int GhostDmgRateResist { get; }
            int UndeadDmgRateResist { get; }

            int AutoGuardRate { get; }
        }

        public interface IAttackerValue
        {
            int Str { get; }
            int Agi { get; }
            int Vit { get; }
            int Int { get; }
            int Dex { get; }
            int Luk { get; }

            int MaxHp { get; }

            int MeleeAtk { get; }
            int RangedAtk { get; }
            int MAtk { get; }

            int Hit { get; }

            int CriRate { get; }

            int CriDmgRate { get; }

            int DmgRate { get; }
            int MeleeDmgRate { get; }
            int RangedDmgRate { get; }
            int NormalMonsterDmgRate { get; }
            int BossMonsterDmgRate { get; }
            int SmallMonsterDmgRate { get; }
            int MediumMonsterDmgRate { get; }
            int LargeMonsterDmgRate { get; }

            int BasicActiveSkillStunRate { get; }
            int BasicActiveSkillSilenceRate { get; }
            int BasicActiveSkillSleepRate { get; }
            int BasicActiveSkillHallucinationRate { get; }
            int BasicActiveSkillBleedingRate { get; }
            int BasicActiveSkillBurningRate { get; }
            int BasicActiveSkillPoisonRate { get; }
            int BasicActiveSkillCurseRate { get; }
            int BasicActiveSkillFreezingRate { get; }
            int BasicActiveSkillFrozenRate { get; }

            int NeutralDmgRate { get; }
            int FireDmgRate { get; }
            int WaterDmgRate { get; }
            int WindDmgRate { get; }
            int EarthDmgRate { get; }
            int PoisonDmgRate { get; }
            int HolyDmgRate { get; }
            int ShadowDmgRate { get; }
            int GhostDmgRate { get; }
            int UndeadDmgRate { get; }

            int GetSkillDamageRate(int skillId);
        }

        /// <summary>
        /// 스킬 id 대미지 증감률
        /// key: 스킬id, value: 스킬증댐률
        /// </summary>
        private readonly Dictionary<ObscuredInt, ObscuredInt> skillIdDmgRateDic;

        private ObscuredInt str, agi, vit, @int, dex, luk;
        private ObscuredInt maxHp, regenHP;
        private ObscuredInt regenMp;
        private ObscuredInt meleeAtk, rangedAtk, matk, def, mdef;
        private ObscuredInt hit, flee, atkSpd, criRate, criRateResist, criDmgRate, atkRange, moveSpd, cooldownRate;
        private ObscuredInt dmgRate, dmgRateResist, meleeDmgRate, meleeDmgRateResist, rangedDmgRate, rangedDmgRateResist, normalMonsterDmgRate, bossMonsterDmgRate, smallMonsterDmgRate, mediumMonsterDmgRate, largeMonsterDmgRate;
        private ObscuredInt stunRateResist, silenceRateResist, sleepRateResist, hallucinationRateResist, bleedingRateResist, burningRateResist, poisonRateResist, curseRateResist, freezingRateResist, frozenRateResist;
        private ObscuredInt basicActiveSkillStunRate, basicActiveSkillSilenceRate, basicActiveSkillSleepRate, basicActiveSkillHallucinationRate, basicActiveSkillBleedingRate, basicActiveSkillBurningRate, basicActiveSkillPoisonRate, basicActiveSkillCurseRate, basicActiveSkillFreezingRate, basicActiveSkillFrozenRate;
        private ObscuredInt neutralDmgRate, fireDmgRate, waterDmgRate, windDmgRate, earthDmgRate, poisonDmgRate, holyDmgRate, shadowDmgRate, ghostDmgRate, undeadDmgRate;
        private ObscuredInt neutralDmgRateResist, fireDmgRateResist, waterDmgRateResist, windDmgRateResist, earthDmgRateResist, poisonDmgRateResist, holyDmgRateResist, shadowDmgRateResist, ghostDmgRateResist, undeadDmgRateResist;
        private ObscuredInt autoGuardRate, devoteRate, expDropRate, jobExpDropRate, zenyDropRate, itemDropRate;

        /// <summary>
        /// 힘 (근력)
        /// </summary>
        public int Str => str;
        /// <summary>
        /// 민첩 (어질)
        /// </summary>
        public int Agi => agi;
        /// <summary>
        /// 체력 (바이탈)
        /// </summary>
        public int Vit => vit;
        /// <summary>
        /// 지력 (인트)
        /// </summary>
        public int Int => @int;
        /// <summary>
        /// 재주 (덱스)
        /// </summary>
        public int Dex => dex;
        /// <summary>
        /// 운 (럭)
        /// </summary>
        public int Luk => luk;

        /// <summary>
        /// 최대 체력
        /// </summary>
        public int MaxHp => maxHp;
        /// <summary>
        /// 체력 재생
        /// </summary>
        public int RegenHp => regenHP;

        /// <summary>
        /// 마나 재생
        /// </summary>
        public int RegenMp => regenMp;

        /// <summary>
        /// 근거리 물리 공격력
        /// </summary>
        public int MeleeAtk => meleeAtk;
        /// <summary>
        /// 원거리 물리 공격력
        /// </summary>
        public int RangedAtk => rangedAtk;
        /// <summary>
        /// 마법 공격력
        /// </summary>
        public int MAtk => matk;
        /// <summary>
        /// 물리 방어력
        /// </summary>
        public int Def => def;
        /// <summary>
        /// 마법 방어력
        /// </summary>
        public int MDef => mdef;
        /// <summary>
        /// 명중률
        /// </summary>
        public int Hit => hit;
        /// <summary>
        /// 회피율
        /// </summary>
        public int Flee => flee;
        /// <summary>
        /// 공격속도
        /// </summary>
        public int AtkSpd => atkSpd;
        /// <summary>
        /// 치명타 확률
        /// </summary>
        public int CriRate => criRate;
        /// <summary>
        /// 치명타 확률 저항
        /// </summary>
        public int CriRateResist => criRateResist;
        /// <summary>
        /// 치명타 증폭대미지 비율
        /// </summary>
        public int CriDmgRate => criDmgRate;
        /// <summary>
        /// 사정 거리 (모든 공격)
        /// </summary>
        public int AtkRange => atkRange;
        /// <summary>
        /// 이동속도
        /// </summary>
        public int MoveSpd => moveSpd;
        /// <summary>
        /// 쿨타임 감소
        /// </summary>
        public int CooldownRate => cooldownRate;

        /// <summary>
        /// 증폭대미지 비율
        /// </summary>
        public int DmgRate => dmgRate;
        /// <summary>
        /// 증폭대미지 비율 저항
        /// </summary>
        public int DmgRateResist => dmgRateResist;
        /// <summary>
        /// 근거리 증폭대미지 비율
        /// </summary>
        public int MeleeDmgRate => meleeDmgRate;
        /// <summary>
        /// 근거리 증폭대미지 비율 저항
        /// </summary>
        public int MeleeDmgRateResist => meleeDmgRateResist;
        /// <summary>
        /// 원거리 증폭대미지 비율
        /// </summary>
        public int RangedDmgRate => rangedDmgRate;
        /// <summary>
        /// 원거리 증폭대미지 비율 저항
        /// </summary>
        public int RangedDmgRateResist => rangedDmgRateResist;
        /// <summary>
        /// 일반 몬스터 증폭대미지 비율
        /// </summary>
        public int NormalMonsterDmgRate => normalMonsterDmgRate;
        /// <summary>
        /// 보스 몬스터 증폭대미지 비율
        /// </summary>
        public int BossMonsterDmgRate => bossMonsterDmgRate;
        /// <summary>
        /// 소형 몬스터 증폭대미지 비율
        /// </summary>
        public int SmallMonsterDmgRate => smallMonsterDmgRate;
        /// <summary>
        /// 중형 몬스터 증폭대미지 비율
        /// </summary>
        public int MediumMonsterDmgRate => mediumMonsterDmgRate;
        /// <summary>
        /// 대형 몬스터 증폭대미지 비율
        /// </summary>
        public int LargeMonsterDmgRate => largeMonsterDmgRate;

        /// <summary>
        /// 스턴 확률 저항
        /// </summary>
        public int StunRateResist => stunRateResist;
        /// <summary>
        /// 침묵 확률 저항
        /// </summary>
        public int SilenceRateResist => silenceRateResist;
        /// <summary>
        /// 수면 확률 저항
        /// </summary>
        public int SleepRateResist => sleepRateResist;
        /// <summary>
        /// 환각 확률 저항
        /// </summary>
        public int HallucinationRateResist => hallucinationRateResist;
        /// <summary>
        /// 출혈 확률 저항
        /// </summary>
        public int BleedingRateResist => bleedingRateResist;
        /// <summary>
        /// 화상 확률 저항
        /// </summary>
        public int BurningRateResist => burningRateResist;
        /// <summary>
        /// 독 확률 저항
        /// </summary>
        public int PoisonRateResist => poisonRateResist;
        /// <summary>
        /// 저주 확률 저항
        /// </summary>
        public int CurseRateResist => curseRateResist;
        /// <summary>
        /// 빙결 확률 저항
        /// </summary>
        public int FreezingRateResist => freezingRateResist;
        /// <summary>
        /// 동빙 확률 저항
        /// </summary>
        public int FrozenRateResist => frozenRateResist;

        /// <summary>
        /// 평타 시 스턴 확률
        /// </summary>
        public int BasicActiveSkillStunRate => basicActiveSkillStunRate;
        /// <summary>
        /// 평타 시 침묵 확률
        /// </summary>
        public int BasicActiveSkillSilenceRate => basicActiveSkillSilenceRate;
        /// <summary>
        /// 평타 시 수면 확률
        /// </summary>
        public int BasicActiveSkillSleepRate => basicActiveSkillSleepRate;
        /// <summary>
        /// 평타 시 환각 확률
        /// </summary>
        public int BasicActiveSkillHallucinationRate => basicActiveSkillHallucinationRate;
        /// <summary>
        /// 평타 시 출혈 확률
        /// </summary>
        public int BasicActiveSkillBleedingRate => basicActiveSkillBleedingRate;
        /// <summary>
        /// 평타 시 화상 확률
        /// </summary>
        public int BasicActiveSkillBurningRate => basicActiveSkillBurningRate;
        /// <summary>
        /// 평타 시 독 확률
        /// </summary>
        public int BasicActiveSkillPoisonRate => basicActiveSkillPoisonRate;
        /// <summary>
        /// 평타 시 저주 확률
        /// </summary>
        public int BasicActiveSkillCurseRate => basicActiveSkillCurseRate;
        /// <summary>
        /// 평타 시 빙결 확률
        /// </summary>
        public int BasicActiveSkillFreezingRate => basicActiveSkillFreezingRate;
        /// <summary>
        /// 평타 시 동빙 확률
        /// </summary>
        public int BasicActiveSkillFrozenRate => basicActiveSkillFrozenRate;

        /// <summary>
        /// 무속성 증폭대미지 비율
        /// </summary>
        public int NeutralDmgRate => neutralDmgRate;
        /// <summary>
        /// 화속성 증폭대미지 비율
        /// </summary>
        public int FireDmgRate => fireDmgRate;
        /// <summary>
        /// 수속성 증폭대미지 비율
        /// </summary>
        public int WaterDmgRate => waterDmgRate;
        /// <summary>
        /// 풍속성 증폭대미지 비율
        /// </summary>
        public int WindDmgRate => windDmgRate;
        /// <summary>
        /// 지속성 증폭대미지 비율
        /// </summary>
        public int EarthDmgRate => earthDmgRate;
        /// <summary>
        /// 독속성 증폭대미지 비율
        /// </summary>
        public int PoisonDmgRate => poisonDmgRate;
        /// <summary>
        /// 성속성 증폭대미지 비율
        /// </summary>
        public int HolyDmgRate => holyDmgRate;
        /// <summary>
        /// 암속성 증폭대미지 비율
        /// </summary>
        public int ShadowDmgRate => shadowDmgRate;
        /// <summary>
        /// 염속성 증폭대미지 비율
        /// </summary>
        public int GhostDmgRate => ghostDmgRate;
        /// <summary>
        /// 사속성 증폭대미지 비율
        /// </summary>
        public int UndeadDmgRate => undeadDmgRate;

        /// <summary>
        /// 무속성 증폭대미지 비율 저항
        /// </summary>
        public int NeutralDmgRateResist => neutralDmgRateResist;
        /// <summary>
        /// 화속성 증폭대미지 비율 저항
        /// </summary>
        public int FireDmgRateResist => fireDmgRateResist;
        /// <summary>
        /// 수속성 증폭대미지 비율 저항
        /// </summary>
        public int WaterDmgRateResist => waterDmgRateResist;
        /// <summary>
        /// 풍속성 증폭대미지 비율 저항
        /// </summary>
        public int WindDmgRateResist => windDmgRateResist;
        /// <summary>
        /// 지속성 증폭대미지 비율 저항
        /// </summary>
        public int EarthDmgRateResist => earthDmgRateResist;
        /// <summary>
        /// 독속성 증폭대미지 비율 저항
        /// </summary>
        public int PoisonDmgRateResist => poisonDmgRateResist;
        /// <summary>
        /// 성속성 증폭대미지 비율 저항
        /// </summary>
        public int HolyDmgRateResist => holyDmgRateResist;
        /// <summary>
        /// 암속성 증폭대미지 비율 저항
        /// </summary>
        public int ShadowDmgRateResist => shadowDmgRateResist;
        /// <summary>
        /// 염속성 증폭대미지 비율 저항
        /// </summary>
        public int GhostDmgRateResist => ghostDmgRateResist;
        /// <summary>
        /// 사속성 증폭대미지 비율 저항
        /// </summary>
        public int UndeadDmgRateResist => undeadDmgRateResist;

        /// <summary>
        /// 피격 시 공격 무시 확률
        /// </summary>
        public int AutoGuardRate => autoGuardRate;
        /// <summary>
        /// 헌신 대미지 흡수량
        /// </summary>
        public int DevoteRate => devoteRate;

        /// <summary>
        /// 일반 경험치 드랍률
        /// </summary>
        public int ExpDropRate => expDropRate;

        /// <summary>
        /// 직업 경험치 드랍률
        /// </summary>
        public int JobExpDropRate => jobExpDropRate;

        /// <summary>
        /// 제니 드랍률
        /// </summary>
        public int ZenyDropRate => zenyDropRate;

        /// <summary>
        /// 아이템 드랍률
        /// </summary>
        public int ItemDropRate => itemDropRate;

        /// <summary>
        /// 최대 체력 변경 시 호출
        /// </summary>
        public event System.Action OnChangeMaxHp;

        /// <summary>
        /// 이동 속도 변경 시 호출
        /// </summary>
        public event System.Action OnChangeMoveSpd;

        /// <summary>
        /// 공격 속도 변경 시 호출
        /// </summary>
        public event System.Action OnChangeAtkSpd;

        public BattleStatusInfo()
        {
            skillIdDmgRateDic = new Dictionary<ObscuredInt, ObscuredInt>(ObscuredIntEqualityComparer.Default);
        }

        /// <summary>
        /// 스탯 세팅
        /// </summary>
        public void Initialize(Settings settings)
        {
            str = settings.str;
            agi = settings.agi;
            vit = settings.vit;
            @int = settings.@int;
            dex = settings.dex;
            luk = settings.luk;

            if (maxHp.Replace(settings.maxHp))
                OnChangeMaxHp?.Invoke();

            regenHP = settings.regenHp;
            regenMp = settings.regenMp;
            meleeAtk = settings.meleeAtk;
            rangedAtk = settings.rangedAtk;
            matk = settings.matk;
            def = settings.def;
            mdef = settings.mdef;
            hit = settings.hit;
            flee = settings.flee;

            int atkSpdValue = Mathf.Min(BasisType.MAX_ATTACK_SPEED.GetInt(), settings.atkSpd);
            if (atkSpd.Replace(atkSpdValue))
                OnChangeAtkSpd?.Invoke();

            criRate = settings.criRate;
            criRateResist = settings.criRateResist;
            criDmgRate = settings.criDmgRate;
            atkRange = Mathf.Min(BasisType.MAX_ATTACK_RANGE.GetInt(), settings.atkRange);

            int moveSpdValue = Mathf.Min(BasisType.MAX_MOVE_SPEED.GetInt(), settings.moveSpd);
            if (moveSpd.Replace(moveSpdValue))
                OnChangeMoveSpd?.Invoke();

            cooldownRate = Mathf.Min(BasisType.MAX_PER_REUSE_WAIT_TIME.GetInt(), settings.cooldownRate);
            dmgRate = settings.dmgRate;
            dmgRateResist = settings.dmgRateResist;
            meleeDmgRate = settings.meleeDmgRate;
            meleeDmgRateResist = settings.meleeDmgRateResist;
            rangedDmgRate = settings.rangedDmgRate;
            rangedDmgRateResist = settings.rangedDmgRateResist;
            normalMonsterDmgRate = settings.normalMonsterDmgRate;
            bossMonsterDmgRate = settings.bossMonsterDmgRate;
            smallMonsterDmgRate = settings.smallMonsterDmgRate;
            mediumMonsterDmgRate = settings.mediumMonsterDmgRate;
            largeMonsterDmgRate = settings.largeMonsterDmgRate;

            stunRateResist = settings.stunRateResist;
            silenceRateResist = settings.silenceRateResist;
            sleepRateResist = settings.sleepRateResist;
            hallucinationRateResist = settings.hallucinationRateResist;
            bleedingRateResist = settings.bleedingRateResist;
            burningRateResist = settings.burningRateResist;
            poisonRateResist = settings.poisonRateResist;
            curseRateResist = settings.curseRateResist;
            freezingRateResist = settings.freezingRateResist;
            frozenRateResist = settings.frozenRateResist;

            basicActiveSkillStunRate = settings.basicActiveSkillStunRate;
            basicActiveSkillSilenceRate = settings.basicActiveSkillSilenceRate;
            basicActiveSkillSleepRate = settings.basicActiveSkillSleepRate;
            basicActiveSkillHallucinationRate = settings.basicActiveSkillHallucinationRate;
            basicActiveSkillBleedingRate = settings.basicActiveSkillBleedingRate;
            basicActiveSkillBurningRate = settings.basicActiveSkillBurningRate;
            basicActiveSkillPoisonRate = settings.basicActiveSkillPoisonRate;
            basicActiveSkillCurseRate = settings.basicActiveSkillCurseRate;
            basicActiveSkillFreezingRate = settings.basicActiveSkillFreezingRate;
            basicActiveSkillFrozenRate = settings.basicActiveSkillFrozenRate;

            neutralDmgRate = settings.neutralDmgRate;
            fireDmgRate = settings.fireDmgRate;
            waterDmgRate = settings.waterDmgRate;
            windDmgRate = settings.windDmgRate;
            earthDmgRate = settings.earthDmgRate;
            poisonDmgRate = settings.poisonDmgRate;
            holyDmgRate = settings.holyDmgRate;
            shadowDmgRate = settings.shadowDmgRate;
            ghostDmgRate = settings.ghostDmgRate;
            undeadDmgRate = settings.undeadDmgRate;

            neutralDmgRateResist = settings.neutralDmgRateResist;
            fireDmgRateResist = settings.fireDmgRateResist;
            waterDmgRateResist = settings.waterDmgRateResist;
            windDmgRateResist = settings.windDmgRateResist;
            earthDmgRateResist = settings.earthDmgRateResist;
            poisonDmgRateResist = settings.poisonDmgRateResist;
            holyDmgRateResist = settings.holyDmgRateResist;
            shadowDmgRateResist = settings.shadowDmgRateResist;
            ghostDmgRateResist = settings.ghostDmgRateResist;
            undeadDmgRateResist = settings.undeadDmgRateResist;

            autoGuardRate = settings.autoGuardRate;
            devoteRate = settings.devoteRate;
            expDropRate = settings.expDropRate;
            jobExpDropRate = settings.jobExpDropRate;
            zenyDropRate = settings.zenyDropRate;
            itemDropRate = settings.itemDropRate;
        }

        /// <summary>
        /// 특정 스킬 증폭대미지 비율 세팅
        /// </summary>
        public void SetSkillIdDmgRate(Dictionary<int, int> dic)
        {
            skillIdDmgRateDic.Clear();
            foreach (var item in dic)
            {
                skillIdDmgRateDic.Add(item.Key, item.Value);
            }
        }

        /// <summary>
        /// 특정 스킬 증폭대미지 비율
        /// </summary>
        public int GetSkillDamageRate(int skillId)
        {
            if (skillIdDmgRateDic.ContainsKey(skillId))
                return skillIdDmgRateDic[skillId];

            return 0;
        }

#if UNITY_EDITOR
        public void SetTestAtk(int value)
        {
            meleeAtk = value;
            rangedAtk = value;
            matk = value;
            def = value;
            mdef = value;
        }

        public void SetTestCrowdContorlRate(CrowdControlType type, int value)
        {
            switch (type)
            {
                case CrowdControlType.Stun:
                    basicActiveSkillStunRate = value;
                    break;

                case CrowdControlType.Silence:
                    basicActiveSkillSilenceRate = value;
                    break;

                case CrowdControlType.Sleep:
                    basicActiveSkillSleepRate = value;
                    break;

                case CrowdControlType.Hallucination:
                    basicActiveSkillHallucinationRate = value;
                    break;

                case CrowdControlType.Bleeding:
                    basicActiveSkillBleedingRate = value;
                    break;

                case CrowdControlType.Burning:
                    basicActiveSkillBurningRate = value;
                    break;

                case CrowdControlType.Poison:
                    basicActiveSkillPoisonRate = value;
                    break;

                case CrowdControlType.Curse:
                    basicActiveSkillCurseRate = value;
                    break;

                case CrowdControlType.Freezing:
                    basicActiveSkillFreezingRate = value;
                    break;

                case CrowdControlType.Frozen:
                    basicActiveSkillFrozenRate = value;
                    break;
            }
        }

        public void SetTestAutoGuard(int value)
        {
            autoGuardRate = value;
        }

        public void SetCriticalRate(int criRate, int criDmgRate)
        {
            this.criRate = criRate;
            this.criDmgRate = criDmgRate;
        }

        public void SetFlee(int value)
        {
            flee = value;
        }

        public void SetSpeed(int atkSpd, int moveSpd)
        {
            if (this.atkSpd.Replace(atkSpd))
                OnChangeAtkSpd?.Invoke();

            if (this.moveSpd.Replace(moveSpd))
                OnChangeMoveSpd?.Invoke();
        }

        public void SetMaxHp(int maxHp)
        {
            if (this.maxHp.Replace(maxHp))
                OnChangeMaxHp?.Invoke();
        }

        public void SetRegenHp(int regenHP)
        {
            this.regenHP = regenHP;
        }       
#endif
    }
}