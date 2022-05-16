using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public class AbilityPopupView : UIView
    {
        [SerializeField] UITabHelper mainTab;
        [SerializeField] GameObject prefab;
        [SerializeField] SuperScrollListWrapper scroll;
        [SerializeField] UIButtonHelper okButton;
        [SerializeField] UIButtonHelper closeButton;
        [SerializeField] UILabelHelper labelTitle;

        private CharacterEntity player;
        private AbilityPopupSlot.Input[] abilities = new AbilityPopupSlot.Input[31];
        private int curShowingToggle = 0;
        private int maxAtkSpd;
        private int maxAtkRange;
        private int maxMoveSpd;
        private int maxCooldownRate;

        protected override void Awake()
        {
            base.Awake();

            scroll.SpawnNewList(prefab, 0, 0);
            scroll.SetRefreshCallback(OnScrollRefresh);

            EventDelegate.Add(mainTab[0].OnChange, OnToggleChange);
            EventDelegate.Add(mainTab[1].OnChange, OnToggleChange);
            EventDelegate.Add(okButton.OnClick, Hide);
            EventDelegate.Add(closeButton.OnClick, Hide);

            maxAtkSpd = BasisType.MAX_ATTACK_SPEED.GetInt();
            maxAtkRange = BasisType.MAX_ATTACK_RANGE.GetInt();
            maxMoveSpd = BasisType.MAX_MOVE_SPEED.GetInt();
            maxCooldownRate = BasisType.MAX_PER_REUSE_WAIT_TIME.GetInt();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (player != null)
                player.OnReloadStatus -= UpdateView;

            EventDelegate.Remove(mainTab[0].OnChange, OnToggleChange);
            EventDelegate.Remove(mainTab[1].OnChange, OnToggleChange);
            EventDelegate.Remove(okButton.OnClick, Hide);
            EventDelegate.Remove(closeButton.OnClick, Hide);
        }

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._4048; // 상세 능력치
            mainTab[0].LocalKey = LocalizeKey._4049; // 공격 능력
            mainTab[1].LocalKey = LocalizeKey._4050; // 방어 능력
            okButton.LocalKey = LocalizeKey._4051; // 확인
        }

        public override void Show()
        {
        }

        public void Show(CharacterEntity charaEntity)
        {
            this.player = charaEntity;

            player.OnReloadStatus -= UpdateView;
            player.OnReloadStatus += UpdateView;

            base.Show();
            UpdateView();
        }

        private void OnToggleChange()
        {
            if (!UIToggle.current.value)
                return;

            curShowingToggle = UIToggle.current.transform.GetSiblingIndex();
            UpdateView();
        }

        private void UpdateView()
        {
            BattleStatusInfo bs = player?.battleStatusInfo;
            BattleStatusInfo pbs = player?.onlyBaseBattleStatusInfo;
            if (bs is null || pbs is null)
                return;

            int count = 0;

            if (curShowingToggle == 0)
            {
                bool isMeleeAttack = player.battleSkillInfo.GetBasicActiveAttackType() == AttackType.MeleeAttack;
                // 공격
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._4011.ToText(), GetStatString(isMeleeAttack ? pbs.MeleeAtk : pbs.RangedAtk, isMeleeAttack ? bs.MeleeAtk : bs.RangedAtk));//
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._48004.ToText(), GetStatString(pbs.MAtk, bs.MAtk));//
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._48006.ToText(), GetStatString(pbs.Hit, bs.Hit));//
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59022.ToText(), GetPercentStatString(pbs.CriRate, bs.CriRate));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59024.ToText(), GetPercentStatString(pbs.CriDmgRate, bs.CriDmgRate));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59021.ToText(), GetMaxPercentStat(maxAtkSpd == bs.AtkSpd, GetPercentStatString(pbs.AtkSpd, bs.AtkSpd)));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59025.ToText(), GetMaxPercentStat(maxAtkRange == bs.AtkRange, GetPercentStatString(pbs.AtkRange, bs.AtkRange)));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59026.ToText(), GetMaxPercentStat(maxMoveSpd == bs.MoveSpd, GetPercentStatString(pbs.MoveSpd, bs.MoveSpd)));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59028.ToText(), GetPercentStatString(pbs.DmgRate, bs.DmgRate));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59030.ToText(), GetPercentStatString(pbs.MeleeDmgRate, bs.MeleeDmgRate));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59032.ToText(), GetPercentStatString(pbs.RangedDmgRate, bs.RangedDmgRate));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59044.ToText().Replace(ReplaceKey.TYPE, LocalizeKey._50000.ToText()), GetPercentStatString(pbs.NeutralDmgRate, bs.NeutralDmgRate));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59044.ToText().Replace(ReplaceKey.TYPE, LocalizeKey._50001.ToText()), GetPercentStatString(pbs.FireDmgRate, bs.FireDmgRate));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59044.ToText().Replace(ReplaceKey.TYPE, LocalizeKey._50002.ToText()), GetPercentStatString(pbs.WaterDmgRate, bs.WaterDmgRate));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59044.ToText().Replace(ReplaceKey.TYPE, LocalizeKey._50003.ToText()), GetPercentStatString(pbs.WindDmgRate, bs.WindDmgRate));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59044.ToText().Replace(ReplaceKey.TYPE, LocalizeKey._50004.ToText()), GetPercentStatString(pbs.EarthDmgRate, bs.EarthDmgRate));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59044.ToText().Replace(ReplaceKey.TYPE, LocalizeKey._50005.ToText()), GetPercentStatString(pbs.PoisonDmgRate, bs.PoisonDmgRate));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59044.ToText().Replace(ReplaceKey.TYPE, LocalizeKey._50006.ToText()), GetPercentStatString(pbs.HolyDmgRate, bs.HolyDmgRate));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59044.ToText().Replace(ReplaceKey.TYPE, LocalizeKey._50007.ToText()), GetPercentStatString(pbs.ShadowDmgRate, bs.ShadowDmgRate));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59044.ToText().Replace(ReplaceKey.TYPE, LocalizeKey._50009.ToText()), GetPercentStatString(pbs.GhostDmgRate, bs.GhostDmgRate));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59044.ToText().Replace(ReplaceKey.TYPE, LocalizeKey._50008.ToText()), GetPercentStatString(pbs.UndeadDmgRate, bs.UndeadDmgRate));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59034.ToText(), GetPercentStatString(pbs.NormalMonsterDmgRate, bs.NormalMonsterDmgRate));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59035.ToText(), GetPercentStatString(pbs.BossMonsterDmgRate, bs.BossMonsterDmgRate));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59036.ToText(), GetPercentStatString(pbs.SmallMonsterDmgRate, bs.SmallMonsterDmgRate));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59037.ToText(), GetPercentStatString(pbs.MediumMonsterDmgRate, bs.MediumMonsterDmgRate));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59038.ToText(), GetPercentStatString(pbs.LargeMonsterDmgRate, bs.LargeMonsterDmgRate));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59027.ToText(), GetMaxPercentStat(maxCooldownRate == bs.CooldownRate, GetPercentStatString(pbs.CooldownRate, bs.CooldownRate)));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59048.ToText(), GetPercentStatString(pbs.ExpDropRate, bs.ExpDropRate));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59049.ToText(), GetPercentStatString(pbs.JobExpDropRate, bs.JobExpDropRate));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59050.ToText(), GetPercentStatString(pbs.ZenyDropRate, bs.ZenyDropRate));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59051.ToText(), GetPercentStatString(pbs.ItemDropRate, bs.ItemDropRate));
            }
            else if (curShowingToggle == 1)
            {
                // 방어
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59013.ToText(), GetStatString(pbs.MaxHp, bs.MaxHp));//
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59014.ToText(), GetStatString(pbs.RegenHp, bs.RegenHp));//
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59062.ToText(), GetStatString(pbs.RegenMp, bs.RegenMp));// 초당 마나 재생율
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59017.ToText(), GetStatString(pbs.Def, bs.Def));//
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59018.ToText(), GetStatString(pbs.MDef, bs.MDef));//
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59020.ToText(), GetStatString(pbs.Flee, bs.Flee));//
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59023.ToText(), GetPercentStatString(pbs.CriRateResist, bs.CriRateResist));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59047.ToText(), GetPercentStatString(pbs.AutoGuardRate, bs.AutoGuardRate));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59029.ToText(), GetPercentStatString(pbs.DmgRateResist, bs.DmgRateResist));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59031.ToText(), GetPercentStatString(pbs.MeleeDmgRateResist, bs.MeleeDmgRateResist));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59033.ToText(), GetPercentStatString(pbs.RangedDmgRateResist, bs.RangedDmgRateResist));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59046.ToText().Replace(ReplaceKey.TYPE, LocalizeKey._50000.ToText()), GetPercentStatString(pbs.NeutralDmgRateResist, bs.NeutralDmgRateResist));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59046.ToText().Replace(ReplaceKey.TYPE, LocalizeKey._50001.ToText()), GetPercentStatString(pbs.FireDmgRateResist, bs.FireDmgRateResist));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59046.ToText().Replace(ReplaceKey.TYPE, LocalizeKey._50002.ToText()), GetPercentStatString(pbs.WaterDmgRateResist, bs.WaterDmgRateResist));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59046.ToText().Replace(ReplaceKey.TYPE, LocalizeKey._50003.ToText()), GetPercentStatString(pbs.WindDmgRateResist, bs.WindDmgRateResist));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59046.ToText().Replace(ReplaceKey.TYPE, LocalizeKey._50004.ToText()), GetPercentStatString(pbs.EarthDmgRateResist, bs.EarthDmgRateResist));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59046.ToText().Replace(ReplaceKey.TYPE, LocalizeKey._50005.ToText()), GetPercentStatString(pbs.PoisonDmgRateResist, bs.PoisonDmgRateResist));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59046.ToText().Replace(ReplaceKey.TYPE, LocalizeKey._50006.ToText()), GetPercentStatString(pbs.HolyDmgRateResist, bs.HolyDmgRateResist));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59046.ToText().Replace(ReplaceKey.TYPE, LocalizeKey._50007.ToText()), GetPercentStatString(pbs.ShadowDmgRateResist, bs.ShadowDmgRateResist));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59046.ToText().Replace(ReplaceKey.TYPE, LocalizeKey._50009.ToText()), GetPercentStatString(pbs.GhostDmgRateResist, bs.GhostDmgRateResist));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59046.ToText().Replace(ReplaceKey.TYPE, LocalizeKey._50008.ToText()), GetPercentStatString(pbs.UndeadDmgRateResist, bs.UndeadDmgRateResist));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59040.ToText().Replace(ReplaceKey.LINK, LocalizeKey._50100.ToText()), GetPercentStatString(pbs.StunRateResist, bs.StunRateResist));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59040.ToText().Replace(ReplaceKey.LINK, LocalizeKey._50101.ToText()), GetPercentStatString(pbs.SilenceRateResist, bs.SilenceRateResist));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59040.ToText().Replace(ReplaceKey.LINK, LocalizeKey._50102.ToText()), GetPercentStatString(pbs.SleepRateResist, bs.SleepRateResist));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59040.ToText().Replace(ReplaceKey.LINK, LocalizeKey._50103.ToText()), GetPercentStatString(pbs.HallucinationRateResist, bs.HallucinationRateResist));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59040.ToText().Replace(ReplaceKey.LINK, LocalizeKey._50104.ToText()), GetPercentStatString(pbs.BleedingRateResist, bs.BleedingRateResist));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59040.ToText().Replace(ReplaceKey.LINK, LocalizeKey._50105.ToText()), GetPercentStatString(pbs.BurningRateResist, bs.BurningRateResist));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59040.ToText().Replace(ReplaceKey.LINK, LocalizeKey._50106.ToText()), GetPercentStatString(pbs.PoisonRateResist, bs.PoisonRateResist));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59040.ToText().Replace(ReplaceKey.LINK, LocalizeKey._50107.ToText()), GetPercentStatString(pbs.CurseRateResist, bs.CurseRateResist));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59040.ToText().Replace(ReplaceKey.LINK, LocalizeKey._50108.ToText()), GetPercentStatString(pbs.FreezingRateResist, bs.FreezingRateResist));
                abilities[count++] = new AbilityPopupSlot.Input(LocalizeKey._59040.ToText().Replace(ReplaceKey.LINK, LocalizeKey._50109.ToText()), GetPercentStatString(pbs.FrozenRateResist, bs.FrozenRateResist));
            }

            scroll.Resize(count);
            scroll.SetProgress(0);
        }

        private string GetStatString(int noBuff, int buff)
        {
            int ds = buff - noBuff;

            if (ds != 0)
                return string.Format("[c][4C4A4D]{0} ({1}{2}[908F90]{3}[-])[-][/c]", buff, noBuff, ds >= 0 ? '+' : '-', Mathf.Abs(ds));
            else
                return string.Format("[c][4C4A4D]{0}[-][/c]", buff);
        }

        private string GetPercentStatString(int noBuff, int buff)
        {
            int ds = buff - noBuff;

            if (ds != 0)
                return string.Format("[c][4C4A4D]{0:0.00} ({1:0.00}{2}[908F90]{3:0.00}[-])[-][/c]", MathUtils.ToPercentValue(buff), MathUtils.ToPercentValue(noBuff), ds >= 0 ? '+' : '-', MathUtils.ToPercentValue(Mathf.Abs(ds)));
            else
                return string.Format("[c][4C4A4D]{0:0.00}[-][/c]", MathUtils.ToPercentValue(buff));
        }

        private string GetMaxPercentStat(bool isMax, string text)
        {
            if (isMax)
            {
                return LocalizeKey._4063.ToText().Replace(ReplaceKey.VALUE, text); // MAX {VALUE}
            }
            return text;
        }

        private void OnScrollRefresh(GameObject obj, int index)
        {
            obj.GetComponent<AbilityPopupSlot>().SetData(abilities[index], index);
            obj.SetActive(true);
        }
    }
}