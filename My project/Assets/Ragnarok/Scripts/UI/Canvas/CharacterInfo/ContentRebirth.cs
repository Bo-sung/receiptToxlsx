using UnityEngine;

namespace Ragnarok
{
    public sealed class ContentRebirth : UISubCanvas, ContentRebirthPresenter.IView, IAutoInspectorFinder
    {
        [SerializeField] UILabelValue baseLevel;// 베이스 레벨
        [SerializeField] UILabelValue stackSP;  // 누적 추가 AP
        [SerializeField] UILabelValue remainSP; // 획득가능 한계 AP
        [SerializeField] UILabelValue addSP;    // 전승 시 추가 AP
        [SerializeField] UILabelHelper labNotice;
        [SerializeField] UIButtonHelper btnRebirth;
        [SerializeField] GameObject firstRebirthNotiRoot;
        [SerializeField] UILabelHelper[] firstRebirthNoti;

        ContentRebirthPresenter presenter;
        ContentRebirthPresenter.IView view;

        protected override void OnInit()
        {
            presenter = new ContentRebirthPresenter(this);
            view = this;
            presenter.AddEvent();
            EventDelegate.Add(btnRebirth.OnClick, OnClickedBtnRebirth);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();
            EventDelegate.Remove(btnRebirth.OnClick, OnClickedBtnRebirth);
        }

        protected override void OnShow()
        {
            presenter.OnShow();
        }

        protected override void OnLocalize()
        {
            baseLevel.TitleKey = LocalizeKey._4037; // BASE
            stackSP.TitleKey = LocalizeKey._4038; // 전승 누적 SP
            remainSP.TitleKey = LocalizeKey._4039; // 획득 가능 한계 SP : 
            addSP.TitleKey = LocalizeKey._4040; // 전승 시 추가 SP
            view.ShowNotice(Entity.player.Character.PossibleRebirthLv);
            btnRebirth.LocalKey = LocalizeKey._4042; // 전승

            firstRebirthNoti[0].Text = LocalizeKey._4053.ToText();
            firstRebirthNoti[1].Text = LocalizeKey._4054.ToText();
            firstRebirthNoti[2].Text = $"+{BasisType.FIRST_REBIRTH_BONUS.GetInt()}";
        }

        protected override void OnHide() { }

        // 전승 버튼
        void OnClickedBtnRebirth()
        {
            presenter.OnClickedBtnCharacterRebirth();
        }

        void ContentRebirthPresenter.IView.ShowBaseLevel(int level)
        {
            baseLevel.Value = level.ToString();
        }

        void ContentRebirthPresenter.IView.ShowNotice(int level)
        {
            labNotice.Text = LocalizeKey._4041.ToText().Replace("{Level}", level.ToString());
        }

        void ContentRebirthPresenter.IView.ShowAccrueSP(int SP)
        {
            stackSP.Value = SP.ToString("N0");
        }

        void ContentRebirthPresenter.IView.ShowAddSP(int SP, bool firstBonus)
        {
            if (firstBonus)
            {
                int firstBonusStat = BasisType.FIRST_REBIRTH_BONUS.GetInt();
                addSP.Value = $"+{(SP + firstBonusStat).ToString("n0")} ({SP.ToString("n0")}+{firstBonusStat.ToString("n0")})";
            }
            else
            {
                addSP.Value = $"+{SP.ToString("n0")}";
            }
        }

        void ContentRebirthPresenter.IView.ShowRemainSP(int SP)
        {
            remainSP.Value = SP.ToString("N0");
        }

        public void SetActiveFirstRebirthNoti(bool value)
        {
            firstRebirthNotiRoot.SetActive(value);
        }
    }
}
