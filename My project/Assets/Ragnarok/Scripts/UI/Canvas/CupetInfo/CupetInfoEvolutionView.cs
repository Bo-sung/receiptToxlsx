using UnityEngine;

namespace Ragnarok
{
    public class CupetInfoEvolutionView : UISubCanvas<CupetInfoPresenter>
    {
        // 체력, 사정거리, 이동속도, 공격속도, 물리공격
        // 물리방어, 마법공격, 마법방어, STR, VIT
        // AGI, DEX, INT, LUK
        [SerializeField] UILabelPreviewValue[] labelValueStatus;

        [SerializeField] UILabelHelper labelNotice;
        [SerializeField] UICupetProfile cupetProfile; // 진화탭 왼쪽 아래 작은 썸네일
        [SerializeField] UILabelHelper LabelNeedPieceDesc;
        [SerializeField] UILabelHelper labelNeedPiece;
        [SerializeField] UICostButtonHelper btnEvolution;

        CupetEntity Cupet => presenter.Cupet;

        protected override void OnInit()
        {

            EventDelegate.Add(btnEvolution.OnClick, OnClickedBtnEvolution);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnEvolution.OnClick, OnClickedBtnEvolution);
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
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

            labelNotice.LocalKey = LocalizeKey._19029; // 진화 시 추가 능력치, 스킬 레벨이 올라갑니다.
            LabelNeedPieceDesc.LocalKey = LocalizeKey._19030; // 진화 시 필요 조각
            btnEvolution.LocalKey = LocalizeKey._19031; // 진화
        }

        protected override void OnShow()
        {
            Refresh();
        }

        public void Refresh()
        {
            if (presenter == null || presenter.IsInvalid())
                return;

            // 스탯 뷰
            BattleStatusInfo status;
            if (presenter.IsInPossesion()) // 미보유일 시, 레벨1의 큐펫을 생성해서 보여준다.
            {
                status = presenter.GetCurrentCupetStatus();
            }
            else
            {
                status = presenter.GetDefaultCupetStatus();
            }

            bool showPreview = (presenter.IsInPossesion() && !presenter.IsMaxRank());
            if (showPreview)
            {
                BattleStatusInfo preview = presenter.GetNextRankCupetStatus();

                labelValueStatus[0].Show(status.MaxHp.ToString(), preview.MaxHp.ToString()); // 체력
                int atkRangeBefore = (int)MathUtils.ToPermyriadValue(presenter.GetBasicAttackRange() * status.AtkRange);
                int atkRangeAfter = (int)MathUtils.ToPermyriadValue(presenter.GetBasicAttackRange() * preview.AtkRange);
                labelValueStatus[1].Show(atkRangeBefore.ToString(), atkRangeAfter.ToString()); // 사정거리
                labelValueStatus[2].Show(MathUtils.ToPermyriadText(status.MoveSpd), MathUtils.ToPermyriadText(preview.MoveSpd)); // 이동속도
                labelValueStatus[3].Show(MathUtils.ToPermyriadText(status.AtkSpd), MathUtils.ToPermyriadText(preview.AtkSpd)); // 공격속도
                labelValueStatus[4].Show(Mathf.Max(status.MeleeAtk, status.RangedAtk).ToString(), Mathf.Max(preview.MeleeAtk, preview.RangedAtk).ToString()); // 물리공격
                labelValueStatus[5].Show(status.Def.ToString(), preview.Def.ToString()); // 물리방어
                labelValueStatus[6].Show(status.MAtk.ToString(), preview.MAtk.ToString()); // 마법공격
                labelValueStatus[7].Show(status.MDef.ToString(), preview.MDef.ToString()); // 마법방어

                labelValueStatus[8].Show(status.Str.ToString(), preview.Str.ToString()); // STR
                labelValueStatus[9].Show(status.Vit.ToString(), preview.Vit.ToString()); // VIT
                labelValueStatus[10].Show(status.Agi.ToString(), preview.Agi.ToString()); // AGI
                labelValueStatus[11].Show(status.Dex.ToString(), preview.Dex.ToString()); // DEX
                labelValueStatus[12].Show(status.Int.ToString(), preview.Int.ToString()); // INT
                labelValueStatus[13].Show(status.Luk.ToString(), preview.Luk.ToString()); // LUK
            }
            else
            {
                labelValueStatus[0].Show(status.MaxHp.ToString()); // 체력
                int atkRange = (int)MathUtils.ToPermyriadValue(presenter.GetBasicAttackRange() * status.AtkRange);
                labelValueStatus[1].Show(atkRange.ToString()); // 사정거리
                labelValueStatus[2].Show(MathUtils.ToPermyriadText(status.MoveSpd)); // 이동속도
                labelValueStatus[3].Show(MathUtils.ToPermyriadText(status.AtkSpd)); // 공격속도
                labelValueStatus[4].Show(Mathf.Max(status.MeleeAtk, status.RangedAtk).ToString()); // 물리공격
                labelValueStatus[5].Show(status.Def.ToString()); // 물리방어
                labelValueStatus[6].Show(status.MAtk.ToString()); // 마법공격
                labelValueStatus[7].Show(status.MDef.ToString()); // 마법방어

                labelValueStatus[8].Show(status.Str.ToString()); // STR
                labelValueStatus[9].Show(status.Vit.ToString()); // VIT
                labelValueStatus[10].Show(status.Agi.ToString()); // AGI
                labelValueStatus[11].Show(status.Dex.ToString()); // DEX
                labelValueStatus[12].Show(status.Int.ToString()); // INT
                labelValueStatus[13].Show(status.Luk.ToString()); // LUK
            }

            // 진화 뷰
            cupetProfile.SetData(presenter.CPModel);

            if (presenter.IsMaxRank())
            {
                labelNeedPiece.LocalKey = LocalizeKey._19011; // MAX
                btnEvolution.SetCostCount(0);
                btnEvolution.IsEnabled = false;
            }
            else
            {
                int haveMonsterPiece = presenter.GetCurrentMonsterPieceCount();
                int needMonsterPiece = presenter.GetNeedMonsterPieceCount();
                labelNeedPiece.Text = $"{haveMonsterPiece}/{needMonsterPiece}";

                int evolutionPrice = presenter.GetNeedEvolutionPrice();
                btnEvolution.SetCostCount(evolutionPrice);
                btnEvolution.IsEnabled = presenter.IsEvolution();
            }
        }

        #region 이벤트

        void OnClickedBtnEvolution()
        {
            presenter.RequestCupetEvolution();
        }

        #endregion
    }
}
