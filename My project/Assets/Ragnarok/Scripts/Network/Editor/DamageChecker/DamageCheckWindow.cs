using System.Collections.Generic;
using UnityEditor;

namespace Ragnarok
{
    public class DamageCheckWindow : EditorWindow
    {
        WindowSplitter splitter;
        DamagePacketTreeView damagePacketTreeView;
        MultiViewer multiViwer;

        void OnEnable()
        {
            Initialize();

            DamageCheck.OnUpdate += Reload;
        }

        void OnDisable()
        {
            DamageCheck.OnUpdate += Reload;
        }

        void OnGUI()
        {
            splitter.OnGUI(position);
            damagePacketTreeView.OnGUI(splitter[0]);
            multiViwer.OnGUI(splitter[1]);
        }

        void Reload(List<DebugDamageTuple> damagePacketList)
        {
            damagePacketTreeView.SetData(damagePacketList);
        }

        private void Initialize()
        {
            if (splitter == null)
            {
                splitter = new HorizontalSplitter(Repaint);
            }

            if (damagePacketTreeView == null)
            {
                damagePacketTreeView = new DamagePacketTreeView { onSingleClicked = Select };
                Reload(DamageCheck.list);
            }

            if (multiViwer == null)
            {
                multiViwer = new MultiViewer(MultiViewer.DivisionType.Horizontal, multiCount: 2);
                multiViwer[0].SetTitle("클라이언트");
                multiViwer[1].SetTitle("서버");
            }
        }

        /// <summary>
        /// 패킷 선택
        /// </summary>
        private void Select(DebugDamageTuple tuple)
        {
            multiViwer.Clear();
            multiViwer[0].SetText(GetText(tuple.client, tuple.client));
            multiViwer[1].SetText(GetText(tuple.server, tuple.client));
        }

        private string GetText(IDamageTuple damageTuple, IDamageTuple comparer)
        {
            return StringBuilderPool.Get()
                .Append("========== 타겟 (스탯) ==========").AppendLine()
                .Append(GetText(nameof(damageTuple.Def), damageTuple.Def, comparer.Def)).AppendLine()
                .Append(GetText(nameof(damageTuple.Mdef), damageTuple.Mdef, comparer.Mdef)).AppendLine()
                .Append(GetText(nameof(damageTuple.DmgRateResist), damageTuple.DmgRateResist, comparer.DmgRateResist)).AppendLine()
                .Append(GetText(nameof(damageTuple.MeleeDmgRateResist), damageTuple.MeleeDmgRateResist, comparer.MeleeDmgRateResist)).AppendLine()
                .Append(GetText(nameof(damageTuple.RangedDmgRateResist), damageTuple.RangedDmgRateResist, comparer.RangedDmgRateResist)).AppendLine()
                .Append(GetText(nameof(damageTuple.NeutralDmgRateResist), damageTuple.NeutralDmgRateResist, comparer.NeutralDmgRateResist)).AppendLine()
                .Append(GetText(nameof(damageTuple.FireDmgRateResist), damageTuple.FireDmgRateResist, comparer.FireDmgRateResist)).AppendLine()
                .Append(GetText(nameof(damageTuple.WaterDmgRateResist), damageTuple.WaterDmgRateResist, comparer.WaterDmgRateResist)).AppendLine()
                .Append(GetText(nameof(damageTuple.WindDmgRateResist), damageTuple.WindDmgRateResist, comparer.WindDmgRateResist)).AppendLine()
                .Append(GetText(nameof(damageTuple.EarthDmgRateResist), damageTuple.EarthDmgRateResist, comparer.EarthDmgRateResist)).AppendLine()
                .Append(GetText(nameof(damageTuple.PoisonDmgRateResist), damageTuple.PoisonDmgRateResist, comparer.PoisonDmgRateResist)).AppendLine()
                .Append(GetText(nameof(damageTuple.HolyDmgRateResist), damageTuple.HolyDmgRateResist, comparer.HolyDmgRateResist)).AppendLine()
                .Append(GetText(nameof(damageTuple.ShadowDmgRateResist), damageTuple.ShadowDmgRateResist, comparer.ShadowDmgRateResist)).AppendLine()
                .Append(GetText(nameof(damageTuple.GhostDmgRateResist), damageTuple.GhostDmgRateResist, comparer.GhostDmgRateResist)).AppendLine()
                .Append(GetText(nameof(damageTuple.UndeadDmgRateResist), damageTuple.UndeadDmgRateResist, comparer.UndeadDmgRateResist)).AppendLine()
                .AppendLine()
                .Append("========== 타겟 (상태이상) ==========").AppendLine()
                .Append(GetText(nameof(damageTuple.DefDecreaseRate), damageTuple.DefDecreaseRate, comparer.DefDecreaseRate)).AppendLine()
                .Append(GetText(nameof(damageTuple.MdefDecreaseRate), damageTuple.MdefDecreaseRate, comparer.MdefDecreaseRate)).AppendLine()
                .AppendLine()
                .Append("========== 시전자 (스탯) ==========").AppendLine()
                .Append(GetText(nameof(damageTuple.Str), damageTuple.Str, comparer.Str)).AppendLine()
                .Append(GetText(nameof(damageTuple.Agi), damageTuple.Agi, comparer.Agi)).AppendLine()
                .Append(GetText(nameof(damageTuple.Vit), damageTuple.Vit, comparer.Vit)).AppendLine()
                .Append(GetText(nameof(damageTuple.Inte), damageTuple.Inte, comparer.Inte)).AppendLine()
                .Append(GetText(nameof(damageTuple.Dex), damageTuple.Dex, comparer.Dex)).AppendLine()
                .Append(GetText(nameof(damageTuple.Luk), damageTuple.Luk, comparer.Luk)).AppendLine()
                .Append(GetText(nameof(damageTuple.MeleeAtk), damageTuple.MeleeAtk, comparer.MeleeAtk)).AppendLine()
                .Append(GetText(nameof(damageTuple.RangedAtk), damageTuple.RangedAtk, comparer.RangedAtk)).AppendLine()
                .Append(GetText(nameof(damageTuple.Matk), damageTuple.Matk, comparer.Matk)).AppendLine()
                .Append(GetText(nameof(damageTuple.CriDmgRate), damageTuple.CriDmgRate, comparer.CriDmgRate)).AppendLine()
                .Append(GetText(nameof(damageTuple.DmgRate), damageTuple.DmgRate, comparer.DmgRate)).AppendLine()
                .Append(GetText(nameof(damageTuple.MeleeDmgRate), damageTuple.MeleeDmgRate, comparer.MeleeDmgRate)).AppendLine()
                .Append(GetText(nameof(damageTuple.RangedDmgRate), damageTuple.RangedDmgRate, comparer.RangedDmgRate)).AppendLine()
                .Append(GetText(nameof(damageTuple.NormalMonsterDmgRate), damageTuple.NormalMonsterDmgRate, comparer.NormalMonsterDmgRate)).AppendLine()
                .Append(GetText(nameof(damageTuple.BossMonsterDmgRate), damageTuple.BossMonsterDmgRate, comparer.BossMonsterDmgRate)).AppendLine()
                .Append(GetText(nameof(damageTuple.SmallMonsterDmgRate), damageTuple.SmallMonsterDmgRate, comparer.SmallMonsterDmgRate)).AppendLine()
                .Append(GetText(nameof(damageTuple.MediumMonsterDmgRate), damageTuple.MediumMonsterDmgRate, comparer.MediumMonsterDmgRate)).AppendLine()
                .Append(GetText(nameof(damageTuple.LargeMonsterDmgRate), damageTuple.LargeMonsterDmgRate, comparer.LargeMonsterDmgRate)).AppendLine()
                .Append(GetText(nameof(damageTuple.NeutralDmgRate), damageTuple.NeutralDmgRate, comparer.NeutralDmgRate)).AppendLine()
                .Append(GetText(nameof(damageTuple.FireDmgRate), damageTuple.FireDmgRate, comparer.FireDmgRate)).AppendLine()
                .Append(GetText(nameof(damageTuple.WaterDmgRate), damageTuple.WaterDmgRate, comparer.WaterDmgRate)).AppendLine()
                .Append(GetText(nameof(damageTuple.WindDmgRate), damageTuple.WindDmgRate, comparer.WindDmgRate)).AppendLine()
                .Append(GetText(nameof(damageTuple.EarthDmgRate), damageTuple.EarthDmgRate, comparer.EarthDmgRate)).AppendLine()
                .Append(GetText(nameof(damageTuple.PoisonDmgRate), damageTuple.PoisonDmgRate, comparer.PoisonDmgRate)).AppendLine()
                .Append(GetText(nameof(damageTuple.HolyDmgRate), damageTuple.HolyDmgRate, comparer.HolyDmgRate)).AppendLine()
                .Append(GetText(nameof(damageTuple.ShadowDmgRate), damageTuple.ShadowDmgRate, comparer.ShadowDmgRate)).AppendLine()
                .Append(GetText(nameof(damageTuple.GhostDmgRate), damageTuple.GhostDmgRate, comparer.GhostDmgRate)).AppendLine()
                .Append(GetText(nameof(damageTuple.UndeadDmgRate), damageTuple.UndeadDmgRate, comparer.UndeadDmgRate)).AppendLine()
                .Append(GetText(nameof(damageTuple.SkillDamageRate), damageTuple.SkillDamageRate, comparer.SkillDamageRate)).AppendLine()
                .AppendLine()
                .Append("========== 시전자 (상태이상) ==========").AppendLine()
                .Append(GetText(nameof(damageTuple.TotalDmgRateDecreaseRate), damageTuple.TotalDmgRateDecreaseRate, comparer.TotalDmgRateDecreaseRate)).AppendLine()
                .AppendLine()
                .Append("========== 추가 상세 정보 ==========").AppendLine()
                .Append(GetText(nameof(damageTuple.AttackerElementType), damageTuple.AttackerElementType, comparer.AttackerElementType)).AppendLine()
                .Append(GetText(nameof(damageTuple.AttackerElementLevel), damageTuple.AttackerElementLevel, comparer.AttackerElementLevel)).AppendLine()
                .Append(GetText(nameof(damageTuple.TargetElementType), damageTuple.TargetElementType, comparer.TargetElementType)).AppendLine()
                .Append(GetText(nameof(damageTuple.TargetElementLevel), damageTuple.TargetElementLevel, comparer.TargetElementLevel)).AppendLine()
                .Append(GetText(nameof(damageTuple.SkillId), damageTuple.SkillId, comparer.SkillId)).AppendLine()
                .Append(GetText(nameof(damageTuple.SkillLevel), damageTuple.SkillLevel, comparer.SkillLevel)).AppendLine()
                .AppendLine()
                .Append("========== 계산 과정 (대미지 상세) ==========").AppendLine()
                .Append(GetText(nameof(damageTuple.CriDamageRate), damageTuple.CriDamageRate, comparer.CriDamageRate)).AppendLine()
                .Append(GetText(nameof(damageTuple.ElementFactor), damageTuple.ElementFactor, comparer.ElementFactor)).AppendLine()
                .Append(GetText(nameof(damageTuple.AttackerElementDamageRate), damageTuple.AttackerElementDamageRate, comparer.AttackerElementDamageRate)).AppendLine()
                .Append(GetText(nameof(damageTuple.TargetElementDamageRateResist), damageTuple.TargetElementDamageRateResist, comparer.TargetElementDamageRateResist)).AppendLine()
                .Append(GetText(nameof(damageTuple.ElementDamageRate), damageTuple.ElementDamageRate, comparer.ElementDamageRate)).AppendLine()
                .Append(GetText(nameof(damageTuple.DamageRate), damageTuple.DamageRate, comparer.DamageRate)).AppendLine()
                .Append(GetText(nameof(damageTuple.DistDamageRate), damageTuple.DistDamageRate, comparer.DistDamageRate)).AppendLine()
                .Append(GetText(nameof(damageTuple.MonsterDamageRate), damageTuple.MonsterDamageRate, comparer.MonsterDamageRate)).AppendLine()
                .Append(GetText(nameof(damageTuple.AttackerWeaponType), damageTuple.AttackerWeaponType, comparer.AttackerWeaponType)).AppendLine()
                .Append(GetText(nameof(damageTuple.UnitSizeFactor), damageTuple.UnitSizeFactor, comparer.UnitSizeFactor)).AppendLine()
                .Append(GetText(nameof(damageTuple.UnitSizeDamageRate), damageTuple.UnitSizeDamageRate, comparer.UnitSizeDamageRate)).AppendLine()
                .AppendLine()
                .Append(GetText(nameof(damageTuple.RawDamageValue), damageTuple.RawDamageValue, comparer.RawDamageValue)).AppendLine()
                .Append(GetText(nameof(damageTuple.DamageRandomValue), damageTuple.DamageRandomValue, comparer.DamageRandomValue)).AppendLine()
                .Append(GetText(nameof(damageTuple.DamageValue), damageTuple.DamageValue, comparer.DamageValue)).AppendLine()
                .Append(GetText(nameof(damageTuple.TargetDef), damageTuple.TargetDef, comparer.TargetDef)).AppendLine()
                .Append(GetText(nameof(damageTuple.DamageDecreaseRate), damageTuple.DamageDecreaseRate, comparer.DamageDecreaseRate)).AppendLine()
                .Append(GetText(nameof(damageTuple.Damage), damageTuple.Damage, comparer.Damage)).AppendLine()
                .AppendLine()
                .Append(GetText(nameof(damageTuple.AttackerMAtk), damageTuple.AttackerMAtk, comparer.AttackerMAtk)).AppendLine()
                .Append(GetText(nameof(damageTuple.RawMDamageValue), damageTuple.RawMDamageValue, comparer.RawMDamageValue)).AppendLine()
                .Append(GetText(nameof(damageTuple.MdamageRandomValue), damageTuple.MdamageRandomValue, comparer.MdamageRandomValue)).AppendLine()
                .Append(GetText(nameof(damageTuple.MdamageValue), damageTuple.MdamageValue, comparer.MdamageValue)).AppendLine()
                .Append(GetText(nameof(damageTuple.TargetMDef), damageTuple.TargetMDef, comparer.TargetMDef)).AppendLine()
                .Append(GetText(nameof(damageTuple.MdamageDecreaseRate), damageTuple.MdamageDecreaseRate, comparer.MdamageDecreaseRate)).AppendLine()
                .Append(GetText(nameof(damageTuple.Mdamage), damageTuple.Mdamage, comparer.Mdamage)).AppendLine()
                .AppendLine()
                .Append(GetText(nameof(damageTuple.SumDamage), damageTuple.SumDamage, comparer.SumDamage)).AppendLine()
                .Append(GetText(nameof(damageTuple.DamageFactor), damageTuple.DamageFactor, comparer.DamageFactor)).AppendLine()
                .Append(GetText(nameof(damageTuple.NormalDamage), damageTuple.NormalDamage, comparer.NormalDamage)).AppendLine()
                .Append(GetText(nameof(damageTuple.SumPlusDamageRate), damageTuple.SumPlusDamageRate, comparer.SumPlusDamageRate)).AppendLine()
                .Append(GetText(nameof(damageTuple.PlusDamage), damageTuple.PlusDamage, comparer.PlusDamage)).AppendLine()
                .Append(GetText(nameof(damageTuple.RawTotalDamage), damageTuple.RawTotalDamage, comparer.RawTotalDamage)).AppendLine()
                .AppendLine()
                .Append(GetText(nameof(damageTuple.TotalDamage), damageTuple.TotalDamage, comparer.TotalDamage)).AppendLine()
                .Append(GetText(nameof(damageTuple.FinalDamage), damageTuple.FinalDamage, comparer.FinalDamage)).AppendLine()
                .Release();
        }

        private string GetText(string header, int value1, int value2)
        {
            if (value1 == value2)
                return string.Concat("[", header, "] ", value1);

            return string.Concat("?? [", header, "] ", value1);
        }
    }
}