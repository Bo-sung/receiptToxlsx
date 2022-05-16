using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
#if TEST_HIDDEN_CHAPTER
    public struct EditorUnitStatus
    {
        public int str;
        public int agi;
        public int vit;
        public int @int;
        public int dex;
        public int luk;

        public int maxHp;
        public int regenHp;

        public int meleeAtk;
        public int rangedAtk;
        public int matk;
        public int def;
        public int mdef;

        public int hit;
        public int flee;
        public int atkSpd;
        public int criRate;
        public int criRateResist;
        public int criDmgRate;
        public int atkRange;
        public int moveSpd;
        public int cooldownRate;

        public int dmgRate;
        public int dmgRateResist;
        public int meleeDmgRate;
        public int meleeDmgRateResist;
        public int rangedDmgRate;
        public int rangedDmgRateResist;
        public int normalMonsterDmgRate;
        public int bossMonsterDmgRate;
        public int smallMonsterDmgRate;
        public int mediumMonsterDmgRate;
        public int largeMonsterDmgRate;

        public int stunRateResist;
        public int silenceRateResist;
        public int sleepRateResist;
        public int hallucinationRateResist;
        public int bleedingRateResist;
        public int burningRateResist;
        public int poisonRateResist;
        public int curseRateResist;
        public int freezingRateResist;
        public int frozenRateResist;

        public int basicActiveSkillStunRate;
        public int basicActiveSkillSilenceRate;
        public int basicActiveSkillSleepRate;
        public int basicActiveSkillHallucinationRate;
        public int basicActiveSkillBleedingRate;
        public int basicActiveSkillBurningRate;
        public int basicActiveSkillPoisonRate;
        public int basicActiveSkillCurseRate;
        public int basicActiveSkillFreezingRate;
        public int basicActiveSkillFrozenRate;

        public int neutralDmgRate;
        public int fireDmgRate;
        public int waterDmgRate;
        public int windDmgRate;
        public int earthDmgRate;
        public int poisonDmgRate;
        public int holyDmgRate;
        public int shadowDmgRate;
        public int ghostDmgRate;
        public int undeadDmgRate;

        public int neutralDmgRateResist;
        public int fireDmgRateResist;
        public int waterDmgRateResist;
        public int windDmgRateResist;
        public int earthDmgRateResist;
        public int poisonDmgRateResist;
        public int holyDmgRateResist;
        public int shadowDmgRateResist;
        public int ghostDmgRateResist;
        public int undeadDmgRateResist;

        public int autoGuardRate;
        public int firePillarRate;

        public void Draw()
        {
            int tempHp = EditorGUILayout.IntField("체력", maxHp);
            int tempAtk = EditorGUILayout.IntField("공격력", meleeAtk);
            int tempDef = EditorGUILayout.IntField("방어력", def);

            // 체력
            maxHp = tempHp;

            // 공격력
            meleeAtk = tempAtk;
            rangedAtk = tempAtk;
            matk = tempAtk;

            // 방어력
            def = tempDef;
            mdef = tempDef;

            if (NGUIEditorTools.DrawHeader("일반 스텟"))
            {
                NGUIEditorTools.BeginContents();
                {
                    str = EditorGUILayout.IntField(nameof(str), str);
                    agi = EditorGUILayout.IntField(nameof(agi), agi);
                    vit = EditorGUILayout.IntField(nameof(vit), vit);
                    @int = EditorGUILayout.IntField(nameof(@int), @int);
                    dex = EditorGUILayout.IntField(nameof(dex), dex);
                    luk = EditorGUILayout.IntField(nameof(luk), luk);
                }
                NGUIEditorTools.EndContents();
            }

            if (NGUIEditorTools.DrawHeader("체력"))
            {
                NGUIEditorTools.BeginContents();
                {
                    maxHp = EditorGUILayout.IntField(nameof(maxHp), maxHp);
                    regenHp = EditorGUILayout.IntField(nameof(regenHp), regenHp);
                }
                NGUIEditorTools.EndContents();
            }

            if (NGUIEditorTools.DrawHeader("공격력/방어력"))
            {
                NGUIEditorTools.BeginContents();
                {
                    meleeAtk = EditorGUILayout.IntField(nameof(meleeAtk), meleeAtk);
                    rangedAtk = EditorGUILayout.IntField(nameof(rangedAtk), rangedAtk);
                    matk = EditorGUILayout.IntField(nameof(matk), matk);
                    def = EditorGUILayout.IntField(nameof(def), def);
                    mdef = EditorGUILayout.IntField(nameof(mdef), mdef);
                }
                NGUIEditorTools.EndContents();
            }

            if (NGUIEditorTools.DrawHeader("일반 전투옵션"))
            {
                NGUIEditorTools.BeginContents();
                {
                    hit = EditorGUILayout.IntField(nameof(hit), hit);
                    flee = EditorGUILayout.IntField(nameof(flee), flee);
                    atkSpd = EditorGUILayout.IntField(nameof(atkSpd), atkSpd);
                    criRate = EditorGUILayout.IntField(nameof(criRate), criRate);
                    criRateResist = EditorGUILayout.IntField(nameof(criRateResist), criRateResist);
                    criDmgRate = EditorGUILayout.IntField(nameof(criDmgRate), criDmgRate);
                    atkRange = EditorGUILayout.IntField(nameof(atkRange), atkRange);
                    moveSpd = EditorGUILayout.IntField(nameof(moveSpd), moveSpd);
                    cooldownRate = EditorGUILayout.IntField(nameof(cooldownRate), cooldownRate);
                }
                NGUIEditorTools.EndContents();
            }

            if (NGUIEditorTools.DrawHeader("일반 증댐/증댐저항"))
            {
                NGUIEditorTools.BeginContents();
                {
                    dmgRate = EditorGUILayout.IntField(nameof(dmgRate), dmgRate);
                    dmgRateResist = EditorGUILayout.IntField(nameof(dmgRateResist), dmgRateResist);
                    meleeDmgRate = EditorGUILayout.IntField(nameof(meleeDmgRate), meleeDmgRate);
                    meleeDmgRateResist = EditorGUILayout.IntField(nameof(meleeDmgRateResist), meleeDmgRateResist);
                    rangedDmgRate = EditorGUILayout.IntField(nameof(rangedDmgRate), rangedDmgRate);
                    rangedDmgRateResist = EditorGUILayout.IntField(nameof(rangedDmgRateResist), rangedDmgRateResist);
                    normalMonsterDmgRate = EditorGUILayout.IntField(nameof(normalMonsterDmgRate), normalMonsterDmgRate);
                    bossMonsterDmgRate = EditorGUILayout.IntField(nameof(bossMonsterDmgRate), bossMonsterDmgRate);
                    smallMonsterDmgRate = EditorGUILayout.IntField(nameof(smallMonsterDmgRate), smallMonsterDmgRate);
                    mediumMonsterDmgRate = EditorGUILayout.IntField(nameof(mediumMonsterDmgRate), mediumMonsterDmgRate);
                    largeMonsterDmgRate = EditorGUILayout.IntField(nameof(largeMonsterDmgRate), largeMonsterDmgRate);
                }
                NGUIEditorTools.EndContents();
            }

            if (NGUIEditorTools.DrawHeader("상태이상저항"))
            {
                NGUIEditorTools.BeginContents();
                {
                    stunRateResist = EditorGUILayout.IntField(nameof(stunRateResist), stunRateResist);
                    silenceRateResist = EditorGUILayout.IntField(nameof(silenceRateResist), silenceRateResist);
                    sleepRateResist = EditorGUILayout.IntField(nameof(sleepRateResist), sleepRateResist);
                    hallucinationRateResist = EditorGUILayout.IntField(nameof(hallucinationRateResist), hallucinationRateResist);
                    bleedingRateResist = EditorGUILayout.IntField(nameof(bleedingRateResist), bleedingRateResist);
                    burningRateResist = EditorGUILayout.IntField(nameof(burningRateResist), burningRateResist);
                    poisonRateResist = EditorGUILayout.IntField(nameof(poisonRateResist), poisonRateResist);
                    curseRateResist = EditorGUILayout.IntField(nameof(curseRateResist), curseRateResist);
                    freezingRateResist = EditorGUILayout.IntField(nameof(freezingRateResist), freezingRateResist);
                    frozenRateResist = EditorGUILayout.IntField(nameof(frozenRateResist), frozenRateResist);
                }
                NGUIEditorTools.EndContents();
            }

            if (NGUIEditorTools.DrawHeader("평타상태이상"))
            {
                NGUIEditorTools.BeginContents();
                {
                    basicActiveSkillStunRate = EditorGUILayout.IntField(nameof(basicActiveSkillStunRate), basicActiveSkillStunRate);
                    basicActiveSkillSilenceRate = EditorGUILayout.IntField(nameof(basicActiveSkillSilenceRate), basicActiveSkillSilenceRate);
                    basicActiveSkillSleepRate = EditorGUILayout.IntField(nameof(basicActiveSkillSleepRate), basicActiveSkillSleepRate);
                    basicActiveSkillHallucinationRate = EditorGUILayout.IntField(nameof(basicActiveSkillHallucinationRate), basicActiveSkillHallucinationRate);
                    basicActiveSkillBleedingRate = EditorGUILayout.IntField(nameof(basicActiveSkillBleedingRate), basicActiveSkillBleedingRate);
                    basicActiveSkillBurningRate = EditorGUILayout.IntField(nameof(basicActiveSkillBurningRate), basicActiveSkillBurningRate);
                    basicActiveSkillPoisonRate = EditorGUILayout.IntField(nameof(basicActiveSkillPoisonRate), basicActiveSkillPoisonRate);
                    basicActiveSkillCurseRate = EditorGUILayout.IntField(nameof(basicActiveSkillCurseRate), basicActiveSkillCurseRate);
                    basicActiveSkillFreezingRate = EditorGUILayout.IntField(nameof(basicActiveSkillFreezingRate), basicActiveSkillFreezingRate);
                    basicActiveSkillFrozenRate = EditorGUILayout.IntField(nameof(basicActiveSkillFrozenRate), basicActiveSkillFrozenRate);
                }
                NGUIEditorTools.EndContents();
            }

            if (NGUIEditorTools.DrawHeader("속성 증댐"))
            {
                NGUIEditorTools.BeginContents();
                {
                    neutralDmgRate = EditorGUILayout.IntField(nameof(neutralDmgRate), neutralDmgRate);
                    fireDmgRate = EditorGUILayout.IntField(nameof(fireDmgRate), fireDmgRate);
                    waterDmgRate = EditorGUILayout.IntField(nameof(waterDmgRate), waterDmgRate);
                    windDmgRate = EditorGUILayout.IntField(nameof(windDmgRate), windDmgRate);
                    earthDmgRate = EditorGUILayout.IntField(nameof(earthDmgRate), earthDmgRate);
                    poisonDmgRate = EditorGUILayout.IntField(nameof(poisonDmgRate), poisonDmgRate);
                    holyDmgRate = EditorGUILayout.IntField(nameof(holyDmgRate), holyDmgRate);
                    shadowDmgRate = EditorGUILayout.IntField(nameof(shadowDmgRate), shadowDmgRate);
                    ghostDmgRate = EditorGUILayout.IntField(nameof(ghostDmgRate), ghostDmgRate);
                    undeadDmgRate = EditorGUILayout.IntField(nameof(undeadDmgRate), undeadDmgRate);
                }
                NGUIEditorTools.EndContents();
            }

            if (NGUIEditorTools.DrawHeader("속성 증댐저항"))
            {
                NGUIEditorTools.BeginContents();
                {
                    neutralDmgRateResist = EditorGUILayout.IntField(nameof(neutralDmgRateResist), neutralDmgRateResist);
                    fireDmgRateResist = EditorGUILayout.IntField(nameof(fireDmgRateResist), fireDmgRateResist);
                    waterDmgRateResist = EditorGUILayout.IntField(nameof(waterDmgRateResist), waterDmgRateResist);
                    windDmgRateResist = EditorGUILayout.IntField(nameof(windDmgRateResist), windDmgRateResist);
                    earthDmgRateResist = EditorGUILayout.IntField(nameof(earthDmgRateResist), earthDmgRateResist);
                    poisonDmgRateResist = EditorGUILayout.IntField(nameof(poisonDmgRateResist), poisonDmgRateResist);
                    holyDmgRateResist = EditorGUILayout.IntField(nameof(holyDmgRateResist), holyDmgRateResist);
                    shadowDmgRateResist = EditorGUILayout.IntField(nameof(shadowDmgRateResist), shadowDmgRateResist);
                    ghostDmgRateResist = EditorGUILayout.IntField(nameof(ghostDmgRateResist), ghostDmgRateResist);
                    undeadDmgRateResist = EditorGUILayout.IntField(nameof(undeadDmgRateResist), undeadDmgRateResist);
                }
                NGUIEditorTools.EndContents();
            }

            if (NGUIEditorTools.DrawHeader("특수 전투옵션"))
            {
                NGUIEditorTools.BeginContents();
                {
                    autoGuardRate = EditorGUILayout.IntField(nameof(autoGuardRate), autoGuardRate);
                    firePillarRate = EditorGUILayout.IntField(nameof(firePillarRate), firePillarRate);
                }
                NGUIEditorTools.EndContents();
            }
        }

        public override string ToString()
        {
            return JsonUtility.ToJson(this);
        }

        public static implicit operator EditorUnitStatus(BattleStatusInfo info)
        {
            return new EditorUnitStatus()
            {
                str = info.Str,
                agi = info.Agi,
                vit = info.Vit,
                @int = info.Int,
                dex = info.Dex,
                luk = info.Luk,
                maxHp = info.MaxHp,
                regenHp = info.RegenHp,
                meleeAtk = info.MeleeAtk,
                rangedAtk = info.RangedAtk,
                matk = info.MAtk,
                def = info.Def,
                mdef = info.MDef,
                hit = info.Hit,
                flee = info.Flee,
                atkSpd = info.AtkSpd,
                criRate = info.CriRate,
                criRateResist = info.CriRateResist,
                criDmgRate = info.CriDmgRate,
                atkRange = info.AtkRange,
                moveSpd = info.MoveSpd,
                cooldownRate = info.CooldownRate,
                dmgRate = info.DmgRate,
                dmgRateResist = info.DmgRateResist,
                meleeDmgRate = info.MeleeDmgRate,
                meleeDmgRateResist = info.MeleeDmgRateResist,
                rangedDmgRate = info.RangedDmgRate,
                rangedDmgRateResist = info.RangedDmgRateResist,
                normalMonsterDmgRate = info.NormalMonsterDmgRate,
                bossMonsterDmgRate = info.BossMonsterDmgRate,
                smallMonsterDmgRate = info.SmallMonsterDmgRate,
                mediumMonsterDmgRate = info.MediumMonsterDmgRate,
                largeMonsterDmgRate = info.LargeMonsterDmgRate,
                stunRateResist = info.StunRateResist,
                silenceRateResist = info.SilenceRateResist,
                sleepRateResist = info.SleepRateResist,
                hallucinationRateResist = info.HallucinationRateResist,
                bleedingRateResist = info.BleedingRateResist,
                burningRateResist = info.BurningRateResist,
                poisonRateResist = info.PoisonRateResist,
                curseRateResist = info.CurseRateResist,
                freezingRateResist = info.FreezingRateResist,
                frozenRateResist = info.FrozenRateResist,
                basicActiveSkillStunRate = info.BasicActiveSkillStunRate,
                basicActiveSkillSilenceRate = info.BasicActiveSkillSilenceRate,
                basicActiveSkillSleepRate = info.BasicActiveSkillSleepRate,
                basicActiveSkillHallucinationRate = info.BasicActiveSkillHallucinationRate,
                basicActiveSkillBleedingRate = info.BasicActiveSkillBleedingRate,
                basicActiveSkillBurningRate = info.BasicActiveSkillBurningRate,
                basicActiveSkillPoisonRate = info.BasicActiveSkillPoisonRate,
                basicActiveSkillCurseRate = info.BasicActiveSkillCurseRate,
                basicActiveSkillFreezingRate = info.BasicActiveSkillFreezingRate,
                basicActiveSkillFrozenRate = info.BasicActiveSkillFrozenRate,
                neutralDmgRate = info.NeutralDmgRate,
                fireDmgRate = info.FireDmgRate,
                waterDmgRate = info.WaterDmgRate,
                windDmgRate = info.WindDmgRate,
                earthDmgRate = info.EarthDmgRate,
                poisonDmgRate = info.PoisonDmgRate,
                holyDmgRate = info.HolyDmgRate,
                shadowDmgRate = info.ShadowDmgRate,
                ghostDmgRate = info.GhostDmgRate,
                undeadDmgRate = info.UndeadDmgRate,
                neutralDmgRateResist = info.NeutralDmgRateResist,
                fireDmgRateResist = info.FireDmgRateResist,
                waterDmgRateResist = info.WaterDmgRateResist,
                windDmgRateResist = info.WindDmgRateResist,
                earthDmgRateResist = info.EarthDmgRateResist,
                poisonDmgRateResist = info.PoisonDmgRateResist,
                holyDmgRateResist = info.HolyDmgRateResist,
                shadowDmgRateResist = info.ShadowDmgRateResist,
                ghostDmgRateResist = info.GhostDmgRateResist,
                undeadDmgRateResist = info.UndeadDmgRateResist,
                autoGuardRate = info.AutoGuardRate,
                firePillarRate = info.FirePillarRate,
            };
        }

        public static implicit operator EditorUnitStatus(string text)
        {
            return JsonUtility.FromJson<EditorUnitStatus>(text);
        }

        public static implicit operator BattleStatusInfo.Settings(EditorUnitStatus status)
        {
            return new BattleStatusInfo.Settings
            {
                str = status.str,
                agi = status.agi,
                vit = status.vit,
                @int = status.@int,
                dex = status.dex,
                luk = status.luk,
                maxHp = status.maxHp,
                regenHp = status.regenHp,
                meleeAtk = status.meleeAtk,
                rangedAtk = status.rangedAtk,
                matk = status.matk,
                def = status.def,
                mdef = status.mdef,
                hit = status.hit,
                flee = status.flee,
                atkSpd = status.atkSpd,
                criRate = status.criRate,
                criRateResist = status.criRateResist,
                criDmgRate = status.criDmgRate,
                atkRange = status.atkRange,
                moveSpd = status.moveSpd,
                cooldownRate = status.cooldownRate,
                dmgRate = status.dmgRate,
                dmgRateResist = status.dmgRateResist,
                meleeDmgRate = status.meleeDmgRate,
                meleeDmgRateResist = status.meleeDmgRateResist,
                rangedDmgRate = status.rangedDmgRate,
                rangedDmgRateResist = status.rangedDmgRateResist,
                normalMonsterDmgRate = status.normalMonsterDmgRate,
                bossMonsterDmgRate = status.bossMonsterDmgRate,
                smallMonsterDmgRate = status.smallMonsterDmgRate,
                mediumMonsterDmgRate = status.mediumMonsterDmgRate,
                largeMonsterDmgRate = status.largeMonsterDmgRate,
                stunRateResist = status.stunRateResist,
                silenceRateResist = status.silenceRateResist,
                sleepRateResist = status.sleepRateResist,
                hallucinationRateResist = status.hallucinationRateResist,
                bleedingRateResist = status.bleedingRateResist,
                burningRateResist = status.burningRateResist,
                poisonRateResist = status.poisonRateResist,
                curseRateResist = status.curseRateResist,
                freezingRateResist = status.freezingRateResist,
                frozenRateResist = status.frozenRateResist,
                basicActiveSkillStunRate = status.basicActiveSkillStunRate,
                basicActiveSkillSilenceRate = status.basicActiveSkillSilenceRate,
                basicActiveSkillSleepRate = status.basicActiveSkillSleepRate,
                basicActiveSkillHallucinationRate = status.basicActiveSkillHallucinationRate,
                basicActiveSkillBleedingRate = status.basicActiveSkillBleedingRate,
                basicActiveSkillBurningRate = status.basicActiveSkillBurningRate,
                basicActiveSkillPoisonRate = status.basicActiveSkillPoisonRate,
                basicActiveSkillCurseRate = status.basicActiveSkillCurseRate,
                basicActiveSkillFreezingRate = status.basicActiveSkillFreezingRate,
                basicActiveSkillFrozenRate = status.basicActiveSkillFrozenRate,
                neutralDmgRate = status.neutralDmgRate,
                fireDmgRate = status.fireDmgRate,
                waterDmgRate = status.waterDmgRate,
                windDmgRate = status.windDmgRate,
                earthDmgRate = status.earthDmgRate,
                poisonDmgRate = status.poisonDmgRate,
                holyDmgRate = status.holyDmgRate,
                shadowDmgRate = status.shadowDmgRate,
                ghostDmgRate = status.ghostDmgRate,
                undeadDmgRate = status.undeadDmgRate,
                neutralDmgRateResist = status.neutralDmgRateResist,
                fireDmgRateResist = status.fireDmgRateResist,
                waterDmgRateResist = status.waterDmgRateResist,
                windDmgRateResist = status.windDmgRateResist,
                earthDmgRateResist = status.earthDmgRateResist,
                poisonDmgRateResist = status.poisonDmgRateResist,
                holyDmgRateResist = status.holyDmgRateResist,
                shadowDmgRateResist = status.shadowDmgRateResist,
                ghostDmgRateResist = status.ghostDmgRateResist,
                undeadDmgRateResist = status.undeadDmgRateResist,
                autoGuardRate = status.autoGuardRate,
                firePillarRate = status.firePillarRate,
            };
        }
    }
#endif
}