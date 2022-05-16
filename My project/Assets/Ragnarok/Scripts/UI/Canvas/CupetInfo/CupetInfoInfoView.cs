using UnityEngine;

namespace Ragnarok
{
    public class CupetInfoInfoView : UISubCanvas<CupetInfoPresenter>, IAutoInspectorFinder
    {
        // 체력, 사정거리, 이동속도, 공격속도, 물리공격
        // 물리방어, 마법공격, 마법방어, STR, VIT
        // AGI, DEX, INT, LUK
        [SerializeField] UILabelValue[] labelValueStatus;
        [SerializeField] UILabelHelper cupetSkillDesc;
        [SerializeField] CupetSkillSimpleView cupetSkillSimpleView;

        protected override void OnInit()
        {
            cupetSkillSimpleView.OnClickEvent = OnClickedBtnSkillIcon;
        }

        protected override void OnHide()
        {

        }

        protected override void OnClose()
        {

        }

        protected override void OnLocalize()
        {
            cupetSkillDesc.LocalKey = LocalizeKey._19036; // 보유 스킬

            labelValueStatus[0].TitleKey = LocalizeKey._19012; // 체력
            labelValueStatus[1].TitleKey = LocalizeKey._19013; // 사정거리
            labelValueStatus[2].TitleKey = LocalizeKey._19014; // 이동속도
            labelValueStatus[3].TitleKey = LocalizeKey._19015; // 공격속도
            labelValueStatus[4].TitleKey = LocalizeKey._19016; // 물리공격
            labelValueStatus[5].TitleKey = LocalizeKey._19017; // 물리방어
            labelValueStatus[6].TitleKey = LocalizeKey._19018; // 마법공격
            labelValueStatus[7].TitleKey = LocalizeKey._19019; // 마법방어
            labelValueStatus[8].TitleKey = LocalizeKey._19020; // STR
            labelValueStatus[9].TitleKey = LocalizeKey._19021; // VIT
            labelValueStatus[10].TitleKey = LocalizeKey._19022; // AGI
            labelValueStatus[11].TitleKey = LocalizeKey._19023; // DEX
            labelValueStatus[12].TitleKey = LocalizeKey._19024; // INT
            labelValueStatus[13].TitleKey = LocalizeKey._19025; // LUK
        }

        protected override void OnShow()
        {
            Refresh();
        }

        public void Refresh()
        {
            cupetSkillSimpleView.SetData(presenter, presenter.Cupet);

            BattleStatusInfo status;
            if (presenter.IsInPossesion()) // 미보유일 시, 레벨1의 큐펫을 생성해서 보여준다.
            {
                status = presenter.GetCurrentCupetStatus();
            }
            else
            {
                status = presenter.GetDefaultCupetStatus();
            }


            labelValueStatus[0].Value = status.MaxHp.ToString(); // 체력
            int atkRange = (int)MathUtils.ToPermyriadValue(presenter.GetBasicAttackRange() * status.AtkRange);
            labelValueStatus[1].Value = atkRange.ToString(); // 사정거리
            labelValueStatus[2].Value = MathUtils.ToPermyriadText(status.MoveSpd); // 이동속도
            labelValueStatus[3].Value = MathUtils.ToPermyriadText(status.AtkSpd); // 공격속도
            labelValueStatus[4].Value = Mathf.Max(status.MeleeAtk, status.RangedAtk).ToString(); // 물리공격
            labelValueStatus[5].Value = status.Def.ToString(); // 물리방어
            labelValueStatus[6].Value = status.MAtk.ToString(); // 마법공격
            labelValueStatus[7].Value = status.MDef.ToString(); // 마법방어

            labelValueStatus[8].Value = status.Str.ToString(); // STR
            labelValueStatus[9].Value = status.Vit.ToString(); // VIT
            labelValueStatus[10].Value = status.Agi.ToString(); // AGI
            labelValueStatus[11].Value = status.Dex.ToString(); // DEX
            labelValueStatus[12].Value = status.Int.ToString(); // INT
            labelValueStatus[13].Value = status.Luk.ToString(); // LUK
        }

        /// <summary>
        /// 스킬 클릭 이벤트
        /// </summary>
        void OnClickedBtnSkillIcon(int index)
        {
            presenter.InfoViewSelectSkill(index);
        }
    }
}