using UnityEngine;

namespace Ragnarok.View
{
    public class UIApplyStatBase : UIView
    {
        [SerializeField] UILabelValue[] labelDetailStatus;

        protected override void OnLocalize()
        {
            labelDetailStatus[0].TitleKey = LocalizeKey._4010;
            labelDetailStatus[1].TitleKey = LocalizeKey._4011;
            labelDetailStatus[2].TitleKey = LocalizeKey._4012;
            labelDetailStatus[3].TitleKey = LocalizeKey._4013;
            labelDetailStatus[4].TitleKey = LocalizeKey._4014;
            labelDetailStatus[5].TitleKey = LocalizeKey._4015;
            labelDetailStatus[6].TitleKey = LocalizeKey._4016;
            labelDetailStatus[7].TitleKey = LocalizeKey._4017;
        }

        public void SetStatView(bool isMeleeAttack, BattleStatusInfo previewStatus, BattleStatusInfo noBuffStatus, BattleStatusInfo previewWithNoBuffStatus)
        {
            labelDetailStatus[0].SetStatValue(previewStatus.MaxHp, noBuffStatus.MaxHp, previewWithNoBuffStatus.MaxHp - noBuffStatus.MaxHp);

            if (isMeleeAttack)
                labelDetailStatus[1].SetStatValue(previewStatus.MeleeAtk, noBuffStatus.MeleeAtk, previewWithNoBuffStatus.MeleeAtk - noBuffStatus.MeleeAtk);
            else
                labelDetailStatus[1].SetStatValue(previewStatus.RangedAtk, noBuffStatus.RangedAtk, previewWithNoBuffStatus.RangedAtk - noBuffStatus.RangedAtk);

            labelDetailStatus[2].SetStatValue(previewStatus.Def, noBuffStatus.Def, previewWithNoBuffStatus.Def - noBuffStatus.Def);

            labelDetailStatus[3].SetStatValue(previewStatus.MAtk, noBuffStatus.MAtk, previewWithNoBuffStatus.MAtk - noBuffStatus.MAtk);

            labelDetailStatus[4].SetStatValue(previewStatus.MDef, noBuffStatus.MDef, previewWithNoBuffStatus.MDef - noBuffStatus.MDef);

            labelDetailStatus[5].SetStatValue(previewStatus.Hit, noBuffStatus.Hit, previewWithNoBuffStatus.Hit - noBuffStatus.Hit);

            labelDetailStatus[6].SetStatValue(previewStatus.Flee, noBuffStatus.Flee, previewWithNoBuffStatus.Flee - noBuffStatus.Flee);

            labelDetailStatus[7].SetPercentStatValue(previewStatus.CriRate, noBuffStatus.CriRate, previewWithNoBuffStatus.CriRate - noBuffStatus.CriRate);
        }
    }
}